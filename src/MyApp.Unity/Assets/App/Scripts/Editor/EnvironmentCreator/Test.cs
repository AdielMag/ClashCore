using UnityEngine.UIElements;

namespace App.Scripts.Editor.EnvironmentCreator
{
    public class Test : VisualElement
    {
        public Test(VisualElement root)
        {
            var label = new Label("Hello, World!");
            Add(label);
            root.Add(this);
        }
    }
}