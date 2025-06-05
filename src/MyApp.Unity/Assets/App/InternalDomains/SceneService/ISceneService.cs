using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace App.InternalDomains.SceneService
{
    public interface ISceneService
    {
        UniTask LoadSceneAsync(string sceneName,
                               LoadSceneMode mode = LoadSceneMode.Additive,
                               IProgress<float> progress = null,
                               CancellationToken cancellationToken = default);

        UniTask UnloadSceneAsync(string sceneName,
                                 CancellationToken cancellationToken = default);

        bool IsSceneLoaded(string sceneName);

        void SetActiveScene(string sceneName);

        string GetActiveSceneName();

        List<string> GetLoadedSceneNames();
    }
}