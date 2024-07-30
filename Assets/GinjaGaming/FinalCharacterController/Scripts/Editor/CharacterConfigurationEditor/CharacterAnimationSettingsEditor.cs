using UnityEditor;
using UnityEngine;

using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{
    [CustomEditor(typeof(CharacterAnimationSettings))]
    public class CharacterAnimationSettingsEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset rootVisualElement;

        private VisualElement _inspectorTree;

        private CharacterAnimationSettings _target;

        private void OnEnable()
        {
            _target = target as CharacterAnimationSettings;
        }

        public override VisualElement CreateInspectorGUI()
        {
            _inspectorTree = new VisualElement();
            rootVisualElement.CloneTree(_inspectorTree);

            Button initTargetButton = _inspectorTree.Q<Button>("InitSettingsButton");
            initTargetButton.RegisterCallback<ClickEvent>(evt =>
            {
                InitSettings();
            });

            Button updateSettingsButton = _inspectorTree.Q<Button>("UpdateSettingsButton");
            updateSettingsButton.RegisterCallback<ClickEvent>(evt =>
            {
                UpdateSettings();
            });

            return _inspectorTree;
        }

        private void InitSettings()
        {
            Debug.Log("InitSettings clicked...");
            _target.InitSettings();
        }

        private void UpdateSettings()
        {
            Debug.Log("UpdateSettings clicked...");
            _target.UpdateMappings();
        }
    }
    [CustomPropertyDrawer(typeof(CharacterAnimationSettings.AnimMapping))]
    public class AnimMapping_PropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI.
            var container = new VisualElement();

            SerializedProperty labelProperty = property.FindPropertyRelative("animMappingLabel");
            string entryLabelText = labelProperty.stringValue;
            container.Add(new PropertyField(property.FindPropertyRelative("animClip"), entryLabelText));

            // Return the finished UI.
            return container;
        }
    }

}