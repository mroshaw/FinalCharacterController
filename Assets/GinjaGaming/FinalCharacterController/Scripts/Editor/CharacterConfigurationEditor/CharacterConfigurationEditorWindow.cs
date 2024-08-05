using System.Collections.Generic;
using System.Linq;
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

        private void AddToLog(List<string> logTexts)
        {
            foreach (string logText in logTexts)
            {
                AddToLog(logText);
            }
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

            AddToLog("Adding presets...");
            _characterEditorPreset.ApplyPreset(_characterController.gameObject, out List<string> applyPresetResults);
            AddToLog(applyPresetResults);
            AddToLog("Done!");
        }

        private bool ValidateUserInput(bool showLog)
        {
            bool validationResult = true;
            List <string> resultLogs = new();

            if (!_characterEditorPreset)
            {
                validationResult = false;
                resultLogs.Add("Please select a character preset!");
            }

            // Validate the Character presets
            if(_characterEditorPreset && !_characterEditorPreset.Validate(out List<string> validationResultLogs))
            {
                validationResult = false;
                resultLogs.AddRange(validationResultLogs);
            }

            if (!_characterController)
            {
                validationResult = false;
                resultLogs.Add("Please select your character model game object!");
            }

            if (_characterController && !_characterController.GetComponent<Animator>())
            {
                validationResult = false;
                resultLogs.Add("Please add an animator to your character model and ensure the Avatar is set!");
            }

            if (showLog)
            {
                AddToLog(resultLogs);
            }
            return validationResult;
        }
    }
}