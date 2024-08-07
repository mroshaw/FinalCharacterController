using System;
using System.Collections.Generic;
using GinjaGaming.FinalCharacterController.Core.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{
    public class CharacterConfigurationEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset tree;
        [SerializeField] private Transform characterModel;
        [SerializeField] private CharacterEditorPreset characterEditorPreset;
        private TextField _logText;

        private Button _applyButton;

        [MenuItem("Window/Final Character Controller/Character Configuration Window")]
        public static void ShowWindow()
        {
            CharacterConfigurationEditorWindow charEditorWindow = GetWindow<CharacterConfigurationEditorWindow>();
            charEditorWindow.titleContent = new GUIContent("Final Character Controller");
        }

        private void OnEnable()
        {
            Reset();
        }

        public void CreateGUI()
        {
            tree.CloneTree(rootVisualElement);

            rootVisualElement.Q<ObjectField>("CharGameObject").RegisterValueChangedCallback(evt =>
            {
                characterModel = evt.newValue as Transform;
                UpdateApplyButtonState(ValidateUserInput(false));
            });

            rootVisualElement.Q<ObjectField>("CharacterPreset").RegisterValueChangedCallback(evt =>
            {
                characterEditorPreset = evt.newValue as CharacterEditorPreset;
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
            this._logText.value += $"{logText}\n";
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
            characterEditorPreset.ApplyPreset(characterModel.gameObject, out List<string> applyPresetResults);
            AddToLog(applyPresetResults);
            AddToLog("Done!");
            UpdateApplyButtonState(ValidateUserInput(false));
        }

        private bool ValidateUserInput(bool showLog)
        {
            bool validationResult = true;
            List <string> resultLogs = new();

            if (!characterEditorPreset)
            {
                validationResult = false;
                resultLogs.Add("Please select a character preset!");
            }

            // Validate the Character presets
            if(characterEditorPreset && !characterEditorPreset.Validate(out List<string> validationResultLogs))
            {
                validationResult = false;
                resultLogs.AddRange(validationResultLogs);
            }

            if (!characterModel)
            {
                validationResult = false;
                resultLogs.Add("Please select your character model game object!");
            }

            if (characterModel && characterModel.transform.root.GetComponentInChildren<CharacterController>(true))
            {
                validationResult = false;
                resultLogs.Add("Selected GameObject already has a Character Controller set up!");
            }

            if (characterModel && !characterModel.GetComponent<Animator>())
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

        private void Reset()
        {
            characterModel = null;
            characterEditorPreset = null;
        }


    }
}