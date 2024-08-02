using GinjaGaming.Core.Extensions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{
    public enum CharacterEditorType {ThirdPerson, FirstPerson, AI }
    public class CharacterConfigurationEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset tree;

        private Transform _characterController;
        private AnimationPresets _animationPresets;
        private CharacterEditorPreset _characterEditorPreset;

        private Button _applyButton;
        private TextField _logText;

        [MenuItem("Window/Final Character Controller/Character Configuration Window")]
        public static void ShowWindow()
        {
            CharacterConfigurationEditorWindow charEditorWindow = GetWindow<CharacterConfigurationEditorWindow>();
            charEditorWindow.titleContent = new GUIContent("Final Character Controller");
        }

        public void CreateGUI()
        {
            tree.CloneTree(rootVisualElement);

            rootVisualElement.Q<ObjectField>("CharGameObject").RegisterValueChangedCallback(evt =>
            {
                _characterController = evt.newValue as Transform;
                UpdateApplyButtonState(ValidateUserInput(false));
            });

            rootVisualElement.Q<ObjectField>("AnimPresets").RegisterValueChangedCallback(evt =>
            {
                _animationPresets = evt.newValue as AnimationPresets;
                UpdateApplyButtonState(ValidateUserInput(false));
            });

            rootVisualElement.Q<ObjectField>("CharacterPreset").RegisterValueChangedCallback(evt =>
            {
                _characterEditorPreset = evt.newValue as CharacterEditorPreset;
                UpdateApplyButtonState(ValidateUserInput(false));
            });

            _applyButton = rootVisualElement.Q<Button>("ApplyButton");
            UpdateApplyButtonState(ValidateUserInput(false));
            _applyButton.RegisterCallback<ClickEvent>(evt =>
            {
                ClearLog();
                ApplySettings();
            });

            _logText = rootVisualElement.Q<TextField>("LogTextField");
            _logText.value = "";
            UpdateApplyButtonState(ValidateUserInput(false));
        }

        private void AddToLog(string logText)
        {
            _logText.value += $"{logText}\n";
            Debug.Log(logText);
        }

        private void ClearLog()
        {
            _logText.value = "";
        }

        private void UpdateApplyButtonState(bool validationState)
        {
            if(validationState)
            {
                // Valid settings
                _applyButton.style.backgroundColor = Color.green;
                _applyButton.style.color = Color.black;
            }
            else
            {
                // Invalid settings
                _applyButton.style.backgroundColor = Color.red;
                _applyButton.style.color = Color.white;
            }
        }

        private void ApplySettings()
        {
            if (!ValidateUserInput(true))
            {
                AddToLog("Validation failed. Cannot apply settings!");
                return;
            }

            AddToLog("Configuring Animator...");
            AnimatorController controller = ConfigureAnimator();

            AddToLog("Applying animations...");
            ConfigureAnims(controller);

            AddToLog("Adding custom components...");
            _characterEditorPreset.ApplyPreset(_characterController.gameObject);
            AddToLog("Done!");
        }

        private bool ValidateUserInput(bool showLog)
        {
            bool validationResult = true;
            string resultLog = "";

            if (!_characterController)
            {
                validationResult = false;
                resultLog += "Please select your character model game object!\n";
            }

            if (_characterController && !_characterController.GetComponent<Animator>())
            {
                validationResult = false;
                resultLog += "Please add an animator to your character model and ensure the Avatar is set!\n";
            }

            if (!_animationPresets)
            {
                validationResult = false;
                resultLog += "Please select your animation presets!\n";
            }

            if (_animationPresets && (!_animationPresets.animMappings || !_animationPresets.animMappings.referenceController))
            {
                validationResult = false;
                resultLog += "Animation presets is not configured! Please check that mappings are assigned and the mapping has a reference controller set!\n";
            }

            if (!_characterEditorPreset)
            {
                validationResult = false;
                resultLog += "Please select a character preset!\n";
            }

            if (showLog)
            {
                AddToLog(resultLog.TrimEnd( '\r', '\n' ));
            }
            return validationResult;
        }

        private AnimatorController ConfigureAnimator()
        {
            Animator animator = _characterController.EnsureComponent<Animator>();

            AnimatorController controller =
                _animationPresets.animMappings.DuplicateController(_characterController.gameObject.name);
            animator.runtimeAnimatorController = controller;

            return controller;
        }

        private void ConfigureAnims(AnimatorController animatorController)
        {
            _animationPresets.UpdateAllAnims(animatorController);
        }
    }
}