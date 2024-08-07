using UnityEngine;
using UnityEngine.InputSystem;

namespace GinjaGaming.FinalCharacterController.Core.PlayerController.Input
{
    [DefaultExecutionOrder(-2)]
    public class ThirdPersonInput : MonoBehaviour, PlayerControls.IThirdPersonMapActions
    {
        #region Class Variables
        public Vector2 ScrollInput { get; private set; }
        public InputDevice ActiveDevice { get; private set; }

        private bool _isScrollHeld;

        #endregion

        #region Startup
        private void OnEnable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Player controls is not initialized - cannot enable");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Enable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Player controls is not initialized - cannot disable");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Disable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.RemoveCallbacks(this);
        }
        #endregion

        #region Update
        private void LateUpdate()
        {
            if (ActiveDevice is Gamepad && _isScrollHeld)
            {
                return;
            }
            ScrollInput = Vector2.zero;
        }
        #endregion

        #region Input Callbacks
        public void OnScrollCamera(InputAction.CallbackContext context)
        {
            ActiveDevice = context.control.device;

            if (ActiveDevice is Gamepad)
            {
                if (context.performed)
                {
                    _isScrollHeld = true;
                    Vector2 gamepadScrollInput = context.ReadValue<Vector2>();
                    ScrollInput = -1f * gamepadScrollInput.normalized;
                }
                else if (context.canceled)
                {
                    _isScrollHeld = false;
                }
            }
            else
            {
                if (!context.performed)
                    return;
            }

            Vector2 scrollInput = context.ReadValue<Vector2>();
            ScrollInput = -1f * scrollInput.normalized;
        }
        #endregion
    }
}