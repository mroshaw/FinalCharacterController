using GinjaGaming.FinalCharacterController.Core.Player;
using GinjaGaming.Core.Extensions;
using GinjaGaming.FinalCharacterController.Core;
using GinjaGaming.FinalCharacterController.Core.Footsteps;
using GinjaGaming.FinalCharacterController.Core.Input;
using UnityEditor;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{

    [CreateAssetMenu(fileName = "CharacterEditorPreset", menuName = "Final Character Controller/Character Editor Preset", order = 1)]
    public class CharacterEditorPreset : ScriptableObject
    {
        #region Class Variables
        [Header("Settings")] [SerializeField] private CharacterEditorType characterType;
        [Header("Prefabs")] [SerializeField] private GameObject cameraPrefab;
        [Header("Controller Settings")] [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private FootstepSurface defaultFootstepSurface;
        [SerializeField] private FootstepSurface[] footstepSurfaces;

        #endregion

        #region Class Methods

        public void ApplyPreset(GameObject targetGameObject)
        {
            switch (characterType)
            {
                case CharacterEditorType.FirstPerson:
                    ApplyFirstPersonPresets(targetGameObject);
                    break;
                case CharacterEditorType.ThirdPerson:
                    ApplyThirdPersonPresets(targetGameObject);
                    break;
                case CharacterEditorType.AI:
                    ApplyAiPresets(targetGameObject);
                    break;
            }
        }

        private void ApplyThirdPersonPresets(GameObject targetGameObject)
        {
            // Instantiate the Camera prefab
            Camera tpsCamera =
                targetGameObject.GetComponentInChildren<Camera>();
            if (!tpsCamera)
            {
                GameObject cameraPrefabInstance = PrefabUtility.InstantiatePrefab(cameraPrefab) as GameObject;
                if (cameraPrefabInstance != null)
                {
                    cameraPrefabInstance.transform.SetParent(targetGameObject.transform, false);
                    tpsCamera =
                        cameraPrefabInstance.GetComponentInChildren<Camera>();
                }
            }
            CharacterController characterController = targetGameObject.EnsureComponent<CharacterController>();
            characterController.center = new Vector3(0, 0.9f, 0);
            PlayerController playerController = targetGameObject.EnsureComponent<PlayerController>();
            playerController.CharacterController = characterController;
            playerController.SetCamera(tpsCamera);
            playerController.SetGroundLayer(groundLayerMask);
            targetGameObject.EnsureComponent<PlayerLocomotionInput>();
            targetGameObject.EnsureComponent<CharacterState>();

            CharacterAnimation characterAnimation = targetGameObject.EnsureComponent<CharacterAnimation>();
            Animator animator = targetGameObject.GetComponent<Animator>();
            characterAnimation.SetAnimator(animator);

            targetGameObject.EnsureComponent<PlayerActionsInput>();
            targetGameObject.EnsureComponent<FootstepManager>();
            targetGameObject.EnsureComponent<AudioSource>();
            targetGameObject.EnsureComponent<CharacterAudio>();
            targetGameObject.EnsureComponent<CharacterHealth>();
        }

        private void ApplyFirstPersonPresets(GameObject targetGameObject)
        {

        }

        private void ApplyAiPresets(GameObject targetGameObject)
        {

        }

        #endregion

    }
}