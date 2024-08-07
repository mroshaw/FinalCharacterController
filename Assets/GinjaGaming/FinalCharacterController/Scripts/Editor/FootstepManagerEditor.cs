using GinjaGaming.FinalCharacterController.Core.CharacterController.Footsteps;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{
    [CustomEditor(typeof(FootstepManager))]
    public class FootstepManagerEditor : UnityEditor.Editor
    {
        private FootstepManager _target;

        private void OnEnable()
        {
            _target = target as FootstepManager;
        }

        public override VisualElement CreateInspectorGUI()
        {

            VisualElement visualElement = new VisualElement();
            InspectorElement.FillDefaultInspector(visualElement, serializedObject, this);
            Button configureButton = new Button
            {
                text = "Configure Triggers"
            };

            configureButton.RegisterCallback<ClickEvent>(evt =>
            {
                _target.ConfigureFootstepTriggers();
            });

            visualElement.Add(configureButton);

            return visualElement;
        }
    }
}