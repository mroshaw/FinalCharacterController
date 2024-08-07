using System;
using System.Collections.Generic;
using GinjaGaming.Editor;
using GinjaGaming.FinalCharacterController.Core.CharacterController;
using GinjaGaming.FinalCharacterController.Core.CharacterController.Footsteps;
using GinjaGaming.FinalCharacterController.Core.Extensions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using CharacterController = UnityEngine.CharacterController;
using Vector3 = UnityEngine.Vector3;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{

    [CreateAssetMenu(fileName = "CharacterEditorPreset", menuName = "Final Character Controller/Character Editor Preset", order = 1)]
    [Serializable]
    public class CharacterEditorPreset : ScriptableObject
    {
        #region Class Variables
        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject footstepPoolPrefab;
        [Header("Animation Settings")]
        [SerializeField] private AnimationPresets animPresets;
        [Header("Controller Settings")]
        [SerializeField] private string playerLayer = "Player";
        [SerializeField] private LayerMask groundLayerMask;
        #endregion

        #region Class Methods
        public bool Validate(out List<string> validationErrors)
        {
            bool validationResult = true;

            validationErrors = new();

            if (LayerMask.NameToLayer(playerLayer) == -1)
            {
                validationResult = false;
                validationErrors.Add($"The Player Layer '{playerLayer}' doesn't exist. Please  add a '{playerLayer}' layer via 'Layers > Edit Layers");
            }

            if (!animPresets)
            {
                validationResult = false;
                validationErrors.Add("Please select your animation presets!");
            }

            if (animPresets && (!animPresets.animMappings || !animPresets.animMappings.referenceController))
            {
                validationResult = false;
                validationErrors.Add("Animation presets is not configured! Please check that mappings are assigned and the mapping has a reference controller set!");
            }

            return validationResult;
        }

        public bool ApplyPreset(GameObject playerModelGameObject, out List<string> applyResults)
        {
            bool applyResult = true;
            applyResults = new List<string>();
            GameObject playerPrefabInstance = ClonePlayerPrefab(playerModelGameObject);
            playerPrefabInstance.layer = LayerMask.NameToLayer(playerLayer);
            UpdateCharacterController(playerPrefabInstance);
            ConfigureAnimator(playerPrefabInstance);
            ConfigureFootstepManager(playerPrefabInstance);
            return applyResult;
        }

        private GameObject ClonePlayerPrefab(GameObject playerModelGameObject)
        {
            if (!playerPrefab)
            {
                return null;
            }

            GameObject prefabInstance = Instantiate(playerPrefab);
            prefabInstance.transform.position = playerModelGameObject.transform.position;
            prefabInstance.transform.rotation = playerModelGameObject.transform.rotation;

            playerModelGameObject.transform.SetParent(prefabInstance.transform, false);
            playerModelGameObject.transform.localPosition = Vector3.zero;
            playerModelGameObject.transform.localRotation = Quaternion.identity;

            string sourceGameObjectName = playerModelGameObject.name;
            if (sourceGameObjectName.Contains("Model", StringComparison.OrdinalIgnoreCase))
            {
                sourceGameObjectName = sourceGameObjectName.Replace("Model", "", StringComparison.OrdinalIgnoreCase).Trim();
            }
            prefabInstance.name = $"{sourceGameObjectName} Controller";
            playerModelGameObject.name = $"{sourceGameObjectName} Model";
            return prefabInstance;
        }

        private void ConfigureAnimator(GameObject prefabInstance)
        {
            // Copy the animator
            Animator sourceAnimator = prefabInstance.GetComponentInChildren<Animator>();

            if (sourceAnimator)
            {
                Animator targetAnimator = prefabInstance.EnsureComponent<Animator>();
                EditorUtils.MoveComponentToTop(prefabInstance, targetAnimator);
                EditorUtility.CopySerialized(sourceAnimator, targetAnimator);
                DestroyImmediate(sourceAnimator);
                AnimatorController controller =
                    animPresets.animMappings.DuplicateController(prefabInstance.name);
                targetAnimator.runtimeAnimatorController = controller;
                animPresets.UpdateAllAnims(controller);

                prefabInstance.GetComponent<CharacterAnimation>().SetAnimator(targetAnimator);
            }
        }

        private void ConfigureFootstepManager(GameObject prefabInstance)
        {
            FootstepManager footstepManager = prefabInstance.GetComponent<FootstepManager>();
            if (!footstepManager)
            {
                return;
            }

            FootstepPoolManager poolManager = FindObjectOfType<FootstepPoolManager>();
            if (!poolManager)
            {
                GameObject poolManagerPrefabInstance = Instantiate(footstepPoolPrefab);
                poolManagerPrefabInstance.name = "Footstep Pool Manager";
                poolManager = poolManagerPrefabInstance.GetComponent<FootstepPoolManager>();
            }

            prefabInstance.GetComponent<FootstepManager>().SetPoolManager(poolManager);

            footstepManager.ConfigureFootstepTriggers();
        }

        private bool UpdateCharacterController(GameObject targetGameObject)
        {
            CharacterController characterController = targetGameObject.GetComponent<CharacterController>();
            if (!characterController)
            {
                return false;
            }

            Vector3 modelSize = GetMeshSize(targetGameObject);
            characterController.center = new Vector3(0, modelSize.y / 2, 0);
            characterController.height = modelSize.y;
            characterController.radius = 0.25f;
            return true;
        }

        /*
        private bool ApplyGenericPresets(GameObject targetGameObject, out List<string> applyResults)
        {
            applyResults = new();
            applyResults.Add("Configure CharacterController...");

            // Calculate the size of the character mesh
            Vector3 charSize = GetMeshBounds(targetGameObject);

            CharacterController characterController = targetGameObject.EnsureComponent<CharacterController>();
            characterController.center = new Vector3(0, charSize.y/2, 0);
            characterController.skinWidth = 0.01f;
            characterController.minMoveDistance = 1e-05f;
            characterController.radius = 0.225f;
            characterController.height = charSize.y;
            return true;
        }


        private bool ApplyThirdPersonPresets(GameObject targetGameObject, out List<string> applyResults)
        {
            applyResults = new();
            // Instantiate the Camera prefab
            Camera tpsCamera =
                targetGameObject.GetComponentInChildren<Camera>();
            if (!tpsCamera)
            {
                applyResults.Add("Adding Camera prefab...");
                GameObject cameraPrefabInstance = PrefabUtility.InstantiatePrefab(cameraPrefab) as GameObject;
                if (cameraPrefabInstance != null)
                {
                    cameraPrefabInstance.transform.SetParent(targetGameObject.transform, false);
                    tpsCamera =
                        cameraPrefabInstance.GetComponentInChildren<Camera>();
                }
            }

            applyResults.Add("Configure PlayerController...");
            PlayerController playerController = targetGameObject.EnsureComponent<PlayerController>();
            CharacterController characterController = targetGameObject.GetComponent<CharacterController>();
            playerController.CharacterController = characterController;
            playerController.SetCamera(tpsCamera);
            playerController.SetGroundLayer(groundLayerMask);
            targetGameObject.EnsureComponent<PlayerLocomotionInput>();
            targetGameObject.EnsureComponent<CharacterState>();

            applyResults.Add("Configure CharacterAnimation...");
            CharacterAnimation characterAnimation = targetGameObject.EnsureComponent<CharacterAnimation>();
            Animator animator = targetGameObject.GetComponent<Animator>();
            characterAnimation.SetAnimator(animator);

            applyResults.Add("Configure PlayerActionsInput...");
            targetGameObject.EnsureComponent<PlayerActionsInput>();

            applyResults.Add("Configure FootstepManager...");
            targetGameObject.EnsureComponent<FootstepManager>();

            applyResults.Add("Configure AudioSource...");
            targetGameObject.EnsureComponent<AudioSource>();

            applyResults.Add("Configure CharacterAudio...");
            targetGameObject.EnsureComponent<CharacterAudio>();

            applyResults.Add("Configure CharacterHealth...");
            targetGameObject.EnsureComponent<CharacterHealth>();

            return true;
        }

        private bool ApplyFirstPersonPresets(GameObject targetGameObject, out List<string> applyResults)
        {
            applyResults = new();
            int layer = LayerMask.NameToLayer(playerLayer);
            if (layer == -1)
            {
                applyResults.Add($"Layer {playerLayer} not found!");
                return false;
            }

            applyResults.Add($"Setting player GameObject layer to: {playerLayer}");
            targetGameObject.layer = layer;
            return true;
        }

        private bool ApplyAiPresets(GameObject targetGameObject, out List<string> applyResults)
        {
            applyResults = new();
            return true;
        }
        */
        private Vector3 GetMeshSize(GameObject targetGameObject)
        {
            Bounds bounds = new Bounds(targetGameObject.transform.position, Vector3.zero);
            foreach (Renderer currRenderer in targetGameObject.GetComponentsInChildren<Renderer>(true))
            {
                bounds.Encapsulate(currRenderer.bounds);
            }

            return bounds.size;
        }

        #endregion

    }
}