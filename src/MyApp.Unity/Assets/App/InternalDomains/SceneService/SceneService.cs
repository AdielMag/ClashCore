using System;
using System.Collections.Generic;
using System.Threading;
using App.InternalDomains.DebugService;

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace App.InternalDomains.SceneService
{
    public class SceneService : ISceneService
    {
        [Inject] private readonly IDebugService _debugService;
        
        private readonly Dictionary<string, UniTask<AsyncOperation>> _loadOperations = new();

        public async UniTask LoadSceneAsync(string sceneName,
                                            LoadSceneMode mode = LoadSceneMode.Additive,
                                            IProgress<float> progress = null,
                                            CancellationToken cancellationToken = default)
        {
            if (_loadOperations.TryGetValue(sceneName, out var existingOperation))
            {
                await existingOperation;
                return;
            }

            var loadOperation = SceneManager.LoadSceneAsync(sceneName, mode);
            var tcs = new UniTaskCompletionSource<AsyncOperation>();
            _loadOperations[sceneName] = tcs.Task;

            loadOperation.completed += operation =>
            {
                _loadOperations.Remove(sceneName);
                tcs.TrySetResult(operation);
            };

            await TrackLoadProgressAsync(loadOperation, progress, cancellationToken);
        }

        private async UniTask TrackLoadProgressAsync(AsyncOperation asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
        {
            while (!asyncOperation.isDone)
            {
                progress?.Report(asyncOperation.progress);
                await UniTask.Yield(cancellationToken);
            }
            progress?.Report(1f);
        }

        public async UniTask UnloadSceneAsync(string sceneName, CancellationToken cancellationToken = default)
        {
            var asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
            if (asyncUnload == null)
            {
                return;
            }

            await asyncUnload.ToUniTask(cancellationToken: cancellationToken);
        }

        public bool IsSceneLoaded(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).isLoaded;
        }

        public void SetActiveScene(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (scene.isLoaded)
            {
                SceneManager.SetActiveScene(scene);
            }
            else
            {
                _debugService.LogWarning($"Scene '{sceneName}' is not loaded and cannot be set as active.");
            }
        }

        public string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        public List<string> GetLoadedSceneNames()
        {
            var loadedScenes = new List<string>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);

                if (scene.isLoaded)
                {
                    loadedScenes.Add(scene.name);
                }
            }

            return loadedScenes;
        }
    }
}