using App.Scripts.Editor.VContainerExtensionsEditor;

using UnityEngine.UIElements;

using VContainer;

namespace App.Scripts.Editor.EnvironmentCreator
{
    public class EnvironmentCreatorContext : EditorDiContext
    {
        public EnvironmentCreatorContext(VisualElement root) : base(root)
        {
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<EnvironmentCreatorController>(Lifetime.Singleton).AsImplementedInterfaces();
            
            /*builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<Test>();
            })*/;
        }
    }
}