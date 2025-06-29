using System;
using System.Collections.Generic;

using App.InternalDomains.DebugService;
using App.SubDomains.Game.Scripts.Interface;
using App.SubDomains.Game.SubDomains.ProximityService.Jobs;

using Cysharp.Threading.Tasks;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace App.SubDomains.Game.SubDomains.ProximityService
{
    /// <summary>
    /// Zero‑GC proximity look‑up service that is also <em>job‑reentrant</em> – you can call it multiple
    /// times per frame without running into <c>AtomicSafetyHandle</c> errors.
    /// </summary>
    public sealed class ProximityService : IProximityService, IDisposable
    {
        private readonly List<Transform> _transforms = new(capacity: 100);
        private readonly IDebugService   _debugService;

        // Native, grow‑only buffers (Allocator.Persistent)
        private NativeArray<float3> _positions;
        private NativeArray<float>  _distances;
        private NativeArray<int>    _indices;

        // Single‑target scratch
        private NativeArray<int>   _singleResult;
        private NativeArray<float> _singleMinDist;

        // Managed scratch – reused each call
        private readonly List<(int index, float distSq)> _results = new(capacity: 32);
        private readonly List<Transform>                  _found   = new(capacity: 16);

        // Keeps track of the last scheduled job that reads our native buffers
        private JobHandle _activeJob;

        public ProximityService(IDebugService debugService)
        {
            _debugService     = debugService;
            _singleResult     = new NativeArray<int>(1,   Allocator.Persistent);
            _singleMinDist    = new NativeArray<float>(1, Allocator.Persistent);
            _activeJob        = default; // no job yet
        }

        #region Registration

        public void RegisterTransform(Transform transform)
        {
            if (!transform)
            {
                _debugService.LogError("Transform cannot be null");
                return;
            }

            if (!_transforms.Contains(transform))
                _transforms.Add(transform);
            else
                _debugService.LogWarning($"Transform {transform.name} already registered.");
        }

        public void UnregisterTransform(Transform transform)
        {
            if (!transform)
            {
                _debugService.LogError("Transform cannot be null");
                return;
            }

            if (!_transforms.Remove(transform))
                _debugService.LogWarning($"Transform {transform.name} not found in registry.");
        }

        #endregion

        #region Public API

        public Transform GetNearbyTarget(Transform self, Vector3 position, float range)
        {
            int count = _transforms.Count;
            if (count == 0) return null;

            int selfIndex = _transforms.IndexOf(self);
            if (selfIndex == -1) return null;

            _activeJob.Complete();
            EnsureCapacity(count);

            for (int i = 0; i < count; i++)
                _positions[i] = _transforms[i].position;

            _singleResult[0] = -1;
            _singleMinDist[0] = float.MaxValue;

            var job = new FindClosestExcludingJob
            {
                source     = position,
                positions  = _positions,
                rangeSq    = range * range,
                selfIndex  = selfIndex,
                results    = _singleResult,
                minDistSq  = _singleMinDist
            };

            var handle = job.Schedule(count, 32);
            handle.Complete(); // ← blocks, no GC

            int idx = _singleResult[0];
            return idx >= 0 ? _transforms[idx] : null;
        }
        
        public async UniTask<Transform> GetNearbyTargetAsync(Transform self, Vector3 position, float range)
        {
            var count = _transforms.Count;
            if (count == 0)
            {
                return null;
            }

            var selfIndex = _transforms.IndexOf(self);
            if (selfIndex == -1)
            {
                _debugService.LogWarning("Caller Transform is not registered in ProximityService.");
                return null;
            }

            _activeJob.Complete();
            EnsureCapacity(count);

            for (var i = 0; i < count; i++)
            {
                _positions[i] = _transforms[i].position;
            }

            _singleMinDist[0] = float.MaxValue;
            _singleResult[0]  = -1;

            var job = new FindClosestExcludingJob
            {
                source     = position,
                positions  = _positions,
                rangeSq    = range * range,
                selfIndex  = selfIndex,
                results    = _singleResult,
                minDistSq  = _singleMinDist
            };

            _activeJob = job.Schedule(count, 32);
            await _activeJob.ToUniTask(PlayerLoopTiming.Update);
            _activeJob.Complete();

            var idx = _singleResult[0];
            return idx >= 0 ? _transforms[idx] : null;
        }

        public async UniTask<Transform[]> GetNearbyTargetsAsync(Transform self, Vector3 position, float range, int maxCount = 10)
        {
            var count = _transforms.Count;
            if (count == 0)
            {
                return Array.Empty<Transform>();
            }

            int selfIndex = _transforms.IndexOf(self);
            if (selfIndex == -1)
            {
                _debugService.LogWarning("Caller Transform is not registered in ProximityService.");
                return Array.Empty<Transform>();
            }

            _activeJob.Complete();
            EnsureCapacity(count);

            for (int i = 0; i < count; i++)
            {
                _positions[i] = _transforms[i].position;
                _indices[i]   = i;
            }

            var job = new FindAllInRangeExcludingJob
            {
                source    = position,
                positions = _positions,
                rangeSq   = range * range,
                selfIndex = selfIndex,
                distances = _distances
            };

            _activeJob = job.Schedule(count, 32);
            await _activeJob.ToUniTask(PlayerLoopTiming.Update);
            _activeJob.Complete();

            _results.Clear();
            for (int i = 0; i < count; i++)
            {
                float d = _distances[i];
                if (d >= 0f) _results.Add((_indices[i], d));
            }

            // sort and pick
            for (int i = 1; i < _results.Count; i++)
            {
                var k = _results[i];
                int j = i - 1;
                while (j >= 0 && _results[j].distSq > k.distSq)
                {
                    _results[j + 1] = _results[j];
                    j--;
                }
                _results[j + 1] = k;
            }

            int take = math.min(maxCount, _results.Count);
            _found.Clear();
            for (int i = 0; i < take; i++)
                _found.Add(_transforms[_results[i].index]);

            return _found.ToArray();
        }

        #endregion

        #region Native memory helpers

        private void EnsureCapacity(int required)
        {
            if (!_positions.IsCreated || _positions.Length < required)
            {
                Resize(ref _positions,  required);
                Resize(ref _distances, required);
                Resize(ref _indices,   required);
            }
        }

        private static void Resize<T>(ref NativeArray<T> array, int length) where T : struct
        {
            if (array.IsCreated) array.Dispose();
            array = new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _activeJob.Complete();
            if (_positions.IsCreated)   _positions.Dispose();
            if (_distances.IsCreated)   _distances.Dispose();
            if (_indices.IsCreated)     _indices.Dispose();
            if (_singleResult.IsCreated)    _singleResult.Dispose();
            if (_singleMinDist.IsCreated)   _singleMinDist.Dispose();
        }

        #endregion
    }
}
