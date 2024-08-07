
using GinjaGaming.FinalCharacterController.Core.PlayerController.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.PlayerController
{
    [DefaultExecutionOrder(-1)]
    public class ThirdPersonCameraController : MonoBehaviour
    {
        #region Class Variables
        [Header("Components")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [Header("Camera Settings")]
        [SerializeField] private float cameraZoomSpeed = 0.1f;
        [SerializeField] private float cameraMinZoom = 1f;
        [SerializeField] private float cameraMaxZoom = 5f;

        private ThirdPersonInput _thirdPersonInput;
        private CinemachineThirdPersonFollow _thirdPersonFollow;
        #endregion

        #region Startup
        private void Awake()
        {
            _thirdPersonInput = GetComponent<ThirdPersonInput>();
            _thirdPersonFollow = virtualCamera.GetComponent<CinemachineThirdPersonFollow>();
        }
        #endregion

        #region Update Logic
        private void Update()
        {
            UpdateCameraZoom();
        }

        private void UpdateCameraZoom()
        {
            Vector2 scrollInput = _thirdPersonInput.ScrollInput * cameraZoomSpeed;
            _thirdPersonFollow.CameraDistance = Mathf.Clamp(_thirdPersonFollow.CameraDistance + scrollInput.y, cameraMinZoom, cameraMaxZoom);
        }
        #endregion
    }
}