using System.Globalization;
using System.Threading;

using App.Scripts.Utils;

using Cysharp.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace App.Scripts.View
{
    public class LoadingScreenBar : MonoBehaviour
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;

        private CancellationTokenSource _cancellationTokenSource = new ();
        
        public void UpdateProgress(float progress)
        {
            UpdateValueAndText(progress);
        }

        public UniTask UpdateProgressAsync(float progress)
        {
            if (Mathf.Approximately(progressBar.value, progress))
            {
                return UniTask.CompletedTask;
            }

            _cancellationTokenSource = _cancellationTokenSource.Refresh();
            return UpdateProgressInternal(progress);
        }
        
        private async UniTask UpdateProgressInternal(float progress)
        {
            var token = _cancellationTokenSource.Token;
            var currentProgress = progressBar.value;
            while (Mathf.Abs(currentProgress - progress) > 0.1f && currentProgress < progress)
            {
                currentProgress = Mathf.MoveTowards(currentProgress, progress, Time.deltaTime);
                
                UpdateValueAndText(currentProgress);

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate,token);
                
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }

            if (token.IsCancellationRequested)
            {
                return;
            }
            progressBar.value = progress;
        }
        
        private void UpdateValueAndText(float value)
        {
            progressBar.value = value;
            progressText.text = (value * 100).ToString("0.0") + "%";
        }
    }
}