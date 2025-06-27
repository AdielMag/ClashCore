using MagicOnion.Client;
using MessagePack;
using MessagePack.Resolvers;
using Shared.Services;
using Shared.Helpersl;

namespace App.Scripts
{
    [MagicOnionClientGeneration(typeof(ISampleService))]
    internal partial class MagicOnionGeneratedClientInitializer
    {
#if UNITY_2019_4_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
#elif NET5_0_OR_GREATER
        [System.Runtime.CompilerServices.ModuleInitializer]
#endif
        private static void RegisterResolvers()
        {
            StaticCompositeResolver.Instance.Register(MessagePackResolverConfig.Resolver);

            MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
               .WithResolver(StaticCompositeResolver.Instance);
        }
        
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            RegisterResolvers();
        }
#endif
    }
}