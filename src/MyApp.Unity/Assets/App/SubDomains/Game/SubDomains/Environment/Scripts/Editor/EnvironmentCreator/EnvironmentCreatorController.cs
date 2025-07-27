using System;

using UnityEngine.UIElements;

using VContainer.Unity;

namespace App.Scripts.Editor.EnvironmentCreator
{
    public class EnvironmentCreatorController : IInitializable,
                                                IDisposable
    {
        private Button _createButton;

        private readonly VisualElement _root;
        private readonly OrganicHexEnvironmentCreator _environmentCreationHandler;

        // New parameters for hex environment
        private int _gridSize = 4;
        private float _edgeWidth = 0.02f;
        private float _perturbBoundaryMagnitude = 0.2f;
        private float _perturbBoundarySmoothness = 2f;
        private float _perturbBoundaryInnerMagnitude = 0.15f;

        private IntegerField _gridSizeField;
        private FloatField _edgeWidthField;
        private FloatField _perturbBoundaryMagnitudeField;
        private FloatField _perturbBoundarySmoothnessField;
        private FloatField _perturbBoundaryInnerMagnitudeField;


        public EnvironmentCreatorController(VisualElement root)
        {
            _root = root;
            _environmentCreationHandler = new OrganicHexEnvironmentCreator();
        }

        public void Initialize()
        {
            // New parameter fields
            _gridSizeField = new IntegerField("Grid Size") { value = _gridSize };
            _edgeWidthField = new FloatField("Edge Width") { value = _edgeWidth };
            _perturbBoundaryMagnitudeField = new FloatField("Perturb Boundary Magnitude") { value = _perturbBoundaryMagnitude };
            _perturbBoundarySmoothnessField = new FloatField("Perturb Boundary Smoothness") { value = _perturbBoundarySmoothness };
            _perturbBoundaryInnerMagnitudeField = new FloatField("Perturb Boundary Inner Magnitude") { value = _perturbBoundaryInnerMagnitude };

            var paramContainer = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Column,
                    marginTop = 10,
                    marginBottom = 10,
                    marginLeft = 10,
                    marginRight = 10,
                    paddingTop = 8,
                    paddingBottom = 8,
                    paddingLeft = 16,
                    paddingRight = 16
                }
            };
            paramContainer.Add(_gridSizeField);
            paramContainer.Add(_edgeWidthField);
            paramContainer.Add(_perturbBoundaryMagnitudeField);
            paramContainer.Add(_perturbBoundarySmoothnessField);
            paramContainer.Add(_perturbBoundaryInnerMagnitudeField);
            _root.Add(paramContainer);

            _createButton = new Button
            {
                text = "Create Environment",
                style = {
                    marginTop = 10,
                    marginBottom = 10,
                    marginLeft = 10,
                    marginRight = 10,
                    paddingTop = 8,
                    paddingBottom = 8,
                    paddingLeft = 16,
                    paddingRight = 16
                }
            };
            _createButton.clicked += OnCreateButtonClicked;
            _root.Add(_createButton);
        }

        private async void OnCreateButtonClicked()
        {
            _gridSize = _gridSizeField.value;
            _edgeWidth = _edgeWidthField.value;
            _perturbBoundaryMagnitude = _perturbBoundaryMagnitudeField.value;
            _perturbBoundarySmoothness = _perturbBoundarySmoothnessField.value;
            _perturbBoundaryInnerMagnitude = _perturbBoundaryInnerMagnitudeField.value;

            _createButton.SetEnabled(false);
            var originalText = _createButton.text;
            _createButton.text = "Creating...";

            await _environmentCreationHandler.CreateGrid(_edgeWidth, _gridSize, _perturbBoundaryMagnitude, _perturbBoundarySmoothness, _perturbBoundaryInnerMagnitude);
            
            _createButton.text = originalText;
            _createButton.SetEnabled(true);
        }

        public void Dispose()
        {
            _environmentCreationHandler?.Dispose();

            _createButton.clicked -= OnCreateButtonClicked;
            _createButton = null;
        }
    }
}