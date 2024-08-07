using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{
    [CustomEditor(typeof(AnimationMappings))]
    public class AnimationMappingsEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset rootVisualElement;
        private VisualElement _inspectorTree;
        private AnimationMappings _target;

        private void OnEnable()
        {
            _target = target as AnimationMappings;
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (!rootVisualElement)
            {
                return null;
            }
            _inspectorTree = new VisualElement();
            rootVisualElement.CloneTree(_inspectorTree);

            Button validateButton = _inspectorTree.Q<Button>("ValidateButton");
            validateButton.RegisterCallback<ClickEvent>(evt =>
            {
                Validate();
            });

            return _inspectorTree;
        }

        private void Validate()
        {
            if(!_target.Validate(out List<string> validationErrors))
            {
                Debug.LogError("Errors found!");
                foreach (string validationError in validationErrors)
                {
                    Debug.LogError(validationError);
                }
            }
            else
            {
                Debug.Log("No errors found!");
            }
        }
    }

    [CustomPropertyDrawer(typeof(AnimationMappings.AnimMapping))]
    public class AnimMappingDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI.
            VisualElement parentContainer = new VisualElement();

            SerializedProperty animHeaderProperty = property.FindPropertyRelative("animationHeader");
            SerializedProperty animNameProperty = property.FindPropertyRelative("animationName");

            string animHeader = animHeaderProperty.stringValue;
            string animName = animNameProperty.stringValue;
            string labelText = $"{animHeader}/{animName}";
            
            Foldout entryFoldout = new Foldout
            {
                text = labelText,
                value = false,
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                }
            };
            VisualElement entryContainer = new VisualElement();
            entryFoldout.Add(entryContainer);
            parentContainer.Add(entryFoldout);

            SerializedProperty stateNameProperty = property.FindPropertyRelative("stateName");
            SerializedProperty stateMachineNameProperty = property.FindPropertyRelative("stateMachineName");
            SerializedProperty layerNameProperty = property.FindPropertyRelative("layerName");

            SerializedProperty blendTreeNameProperty = property.FindPropertyRelative("blendTreeName");
            SerializedProperty blendTreeIndexProperty = property.FindPropertyRelative("blendTreeIndex");


            entryContainer.Add(new PropertyField(animHeaderProperty));
            entryContainer.Add(new PropertyField(animNameProperty));
            entryContainer.Add(new PropertyField(stateNameProperty));
            entryContainer.Add(new PropertyField(stateMachineNameProperty));
            entryContainer.Add(new PropertyField(layerNameProperty));
            entryContainer.Add(new PropertyField(blendTreeNameProperty));
            entryContainer.Add(new PropertyField(blendTreeIndexProperty));

            // Return the finished UI.
            return parentContainer;
        }
    }
}