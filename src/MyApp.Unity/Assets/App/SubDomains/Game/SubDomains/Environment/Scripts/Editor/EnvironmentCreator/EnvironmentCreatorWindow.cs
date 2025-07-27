using UnityEditor;
using UnityEngine.UIElements;

namespace App.Scripts.Editor.EnvironmentCreator
{
    public class EnvironmentCreatorWindow : EditorWindow
    {
        private EnvironmentCreatorContext _context;
        private Label _label;

        [MenuItem("Tools/Environment Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<EnvironmentCreatorWindow>("Environment Creator");
            window.Show();
        }

        private void OnEnable()
        {
            _context = new EnvironmentCreatorContext(rootVisualElement);
        }

        private void OnDisable()
        {
            _context?.Dispose();
            _context = null;
        }

        public void CreateGUI()
        {
        }
    }

    // Example service to demonstrate DI
    public class SampleService
    {
        public string GetMessage() => "Message from DI Service!";
    }
}