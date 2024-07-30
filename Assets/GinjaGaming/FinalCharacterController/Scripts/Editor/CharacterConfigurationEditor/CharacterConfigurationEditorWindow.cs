using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{
    public class CharacterConfigurationEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset tree;

        private Transform _characterController;
        private CharacterAnimationSettings _animationSettings;

        private Button _applyButton;
        private TextField _logText;

        [MenuItem("Final Character Controller/Character Configuration Window")]
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
                UpdateApplyButtonState();
            });

            rootVisualElement.Q<ObjectField>("AnimSettings").RegisterValueChangedCallback(evt =>
            {
                _animationSettings = evt.newValue as CharacterAnimationSettings;
                UpdateApplyButtonState();
            });

            _applyButton = rootVisualElement.Q<Button>("ApplyButton");
            UpdateApplyButtonState();
            _applyButton.RegisterCallback<ClickEvent>(evt =>
            {
                ApplySettings();
            });

            _logText = rootVisualElement.Q<TextField>("LogText");
            _logText.value = "";

        }

        private void AddToLog(string logText)
        {
            _logText.value += $"{logText}\n";
            Debug.Log("logText");
        }

        private void UpdateApplyButtonState()
        {
            if (_characterController != null && _animationSettings != null &&
                _animationSettings.mappingSettings.referenceController != null)
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
            if (!ValidateUserInput())
            {
                AddToLog("Validation failed. Cannot apply settings!");
                return;
            }

            AddToLog("Configuring Animator...");
            AnimatorController controller = ConfigureAnimator();

            AddToLog("Applying animations...");
            ConfigureAnims(controller);

            AddToLog("Done!");
        }

        private bool ValidateUserInput()
        {
            if (!_characterController)
            {
                AddToLog("Please select your character controller!");
                return false;
            }

            if (!_animationSettings)
            {
                AddToLog("Please select your animation settings!");
                return false;
            }

            if (!_animationSettings.mappingSettings.referenceController)
            {
                AddToLog("Please select a reference controller in your animation settings!");
                return false;
            }

            return true;
        }

        private AnimatorController ConfigureAnimator()
        {
            Animator animator = _characterController.GetComponent<Animator>();
            if (!animator)
            {
                animator = _characterController.gameObject.AddComponent<Animator>();
            }

            AnimatorController controller =
                _animationSettings.mappingSettings.DuplicateController(_characterController.gameObject.name);

            animator.runtimeAnimatorController = controller;

            return controller;
        }

        private void ConfigureAnims(AnimatorController animatorController)
        {
            _animationSettings.UpdateAllAnims(animatorController);
        }
    }
}