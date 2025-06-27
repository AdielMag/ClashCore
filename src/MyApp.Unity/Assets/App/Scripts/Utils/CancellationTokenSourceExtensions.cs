using System.Threading;

namespace App.Scripts.Utils
{
    public static class CancellationTokenSourceExtensions
    {
        public static CancellationTokenSource Refresh(this CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource ??= new CancellationTokenSource();

            cancellationTokenSource.Dispose();
            return new CancellationTokenSource();
        }
    }
}