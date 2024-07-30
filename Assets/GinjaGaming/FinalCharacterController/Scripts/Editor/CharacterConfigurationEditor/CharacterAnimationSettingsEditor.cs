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
            if (!rootVisualElement)
            {
                return null;
            }
            _inspectorTree = new VisualElement();
            rootVisualElement.CloneTree(_inspectorTree);

            /*

            // Create a container for the list
            VisualElement listContainer = new VisualElement();
            listContainer.style.flexDirection = FlexDirection.Row;

            // Create columns
            VisualElement column1 = new VisualElement();
            column1.style.flexGrow = 1;
            column1.style.marginRight = 5;

            VisualElement column2 = new VisualElement();
            column2.style.flexGrow = 1;
            column2.style.marginRight = 5;

            VisualElement column3 = new VisualElement();
            column2.style.flexGrow = 1;
            column2.style.marginRight = 5;

            VisualElement column4 = new VisualElement();
            column3.style.flexGrow = 1;

            listContainer.Add(column1);
            listContainer.Add(column2);
            listContainer.Add(column3);
            listContainer.Add(column4);

            _inspectorTree.Add(listContainer);

            SerializedProperty listProperty = serializedObject.FindProperty("animMappings");

            // Display the headers

            column1.Add(new Label("Anim"));
            column2.Add(new Label("Clip"));
            column3.Add(new Label("Mirror"));
            column4.Add(new Label("Speed"));

            // Display the list elements
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty element = listProperty.GetArrayElementAtIndex(i);

                SerializedProperty field1 = element.FindPropertyRelative("animMappingLabel");
                SerializedProperty field2 = element.FindPropertyRelative("animClip");
                SerializedProperty field3 = element.FindPropertyRelative("reverseAnimation");
                SerializedProperty field4 = element.FindPropertyRelative("animSpeed");

                column1.Add(new PropertyField(field1));
                column2.Add(new PropertyField(field2));
                column3.Add(new PropertyField(field3));
                column4.Add(new PropertyField(field4));
            }
            */
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
            SerializedProperty mirrorProperty = property.FindPropertyRelative("mirrorAnimation");
            SerializedProperty animSpeedProperty = property.FindPropertyRelative("animSpeed");

            string entryLabelText = labelProperty.stringValue;
            container.Add(new PropertyField(property.FindPropertyRelative("animClip"), entryLabelText));
            container.Add(new PropertyField(mirrorProperty));
            container.Add(new PropertyField(animSpeedProperty));
            // Return the finished UI.
            return container;
        }
    }

}