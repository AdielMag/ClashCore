using Cysharp.Threading.Tasks;

namespace App.SubDomains.Game.SubDomains.Environment.Scripts.Interface
{
    public interface IEnvironmentManager
    {
        UniTask LoadEnvironment(string levelName);
    }
}