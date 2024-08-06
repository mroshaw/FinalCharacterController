using GinjaGaming.FinalCharacterController.Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GinjaGaming.FinalCharacterController.Core.Player
{
    [DefaultExecutionOrder(-1)]
    public class PlayerController : CharacterControllerBase
    {
        #region Class Variables
        [Header("Animation")]
        public float playerModelRotationSpeed = 10f;
        public float rotateToTargetTime = 0.67f;

        [Header("Camera Settings")]
        [SerializeField] private Camera playerCamera;
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;

        [Header("Gamepad Settings")]
        public float gamepadLookXMultiplier = 5.0f;
        public float gamepadLookYMultiplier = 5.0f;

        // Used by Player Animation to determine if player is rotating
        public float RotationMismatch { get; private set; }
        public bool IsRotatingToTarget { get; private set; }

        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerActionsInput _playerActionInput;

        private Vector2 _cameraRotation = Vector2.zero;
        private float _rotatingToTargetTimer;
        private bool _isRotatingClockwise;

        #endregion

        #region Startup
        public override void Awake()
        {
            base.Awake();
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerActionInput = GetComponent<PlayerActionsInput>();
        }
        #endregion

        #region Update Logic
        public override void Update()
        {
            HandleVerticalInput();
            HandleLateralInput();
            UpdateActionState();
            base.Update();
        }

        /// <summary>
        /// This is the 'Player Action' state machine. This controls what actions the player can and will perform,
        /// based on current action states, and from and to permitted action states.
        /// </summary>
        private void UpdateActionState()
        {
            if (_playerActionInput.AttackPressed)
            {
                _playerActionInput.SetGatherPressedFalse();
                // Tell the CharacterController we want to attack
                CharacterState.SetCharacterActionState(CharacterActionState.Attacking);
            }


            if (_playerActionInput.GatherPressed && CharacterState.CurrentCharacterActionState == CharacterActionState.None)
            {
                CharacterState.SetCharacterActionState(CharacterActionState.Gathering);
            }

            if (!_playerActionInput.GatherPressed && !_playerActionInput.AttackPressed)
            {
                CharacterState.SetCharacterActionState(CharacterActionState.None);
            }
        }

        private void HandleLateralInput()
        {
            Vector3 cameraForwardXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(playerCamera.transform.right.x, 0f, playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

            // Let the CharacterController know we want to move
            LateralVelocityDirection = movementDirection;

            if (_playerLocomotionInput.RollPressed)
            {
                RequestMovementStateChange = CharacterMovementStateChange.Rolling;
            }
            else if (_playerLocomotionInput.CrouchToggledOn)
            {
                RequestMovementStateChange = CharacterMovementStateChange.Crouching;
            }
        }

        private void HandleVerticalInput()
        {
            // Handle Jump pressed
            if (_playerLocomotionInput.JumpPressed)
            {
                // Let the CharacterController know that we want to jump
                RequestMovementStateChange = CharacterMovementStateChange.Jump;
            }
        }
        #endregion

        #region Late Update Logic
        private void LateUpdate()
        {
            UpdateCameraRotation();
        }

        private void UpdateIdleRotation(float rotationTolerance)
        {
            // Initiate new rotation direction
            if (Mathf.Abs(RotationMismatch) > rotationTolerance)
            {
                _rotatingToTargetTimer = rotateToTargetTime;
                _isRotatingClockwise = RotationMismatch > rotationTolerance;
            }

            _rotatingToTargetTimer -= Time.deltaTime;

            // Rotate character
            if (_isRotatingClockwise && RotationMismatch > 0f ||
                !_isRotatingClockwise && RotationMismatch < 0f)
            {
                RotateToTarget();
            }
        }

        private void RotateToTarget()
        {
            Quaternion targetRotationX = Quaternion.Euler(0f, TargetRotation.x, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX,
                playerModelRotationSpeed * Time.deltaTime);
        }

        private void UpdateCameraRotation()
        {
            float lookInputX = _playerLocomotionInput.LookInput.x;
            float lookInputY = _playerLocomotionInput.LookInput.y;

            if (_playerLocomotionInput.ActiveDevice is Gamepad)
            {
                lookInputX *= gamepadLookXMultiplier;
                lookInputY *= gamepadLookYMultiplier;
            }

            _cameraRotation.x += lookSenseH * lookInputX;
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * lookInputY, -lookLimitV, lookLimitV);
            TargetRotation.x += transform.eulerAngles.x + lookSenseH * lookInputX;

            float rotationTolerance = 90f;
            bool isIdling = CharacterState.CurrentCharacterMovementState == CharacterMovementState.Idling;
            IsRotatingToTarget = _rotatingToTargetTimer > 0;

            if (!CharacterState.InDeadState())
            {
                // Rotate if we're not idling
                if (!isIdling)
                {
                    RotateToTarget();
                }
                // If rotation mismatch not within tolerance, or rotate to target is active, ROTATE
                else if (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget)
                {
                    UpdateIdleRotation(rotationTolerance);
                }
            }
            playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

            // Get angle between camera and player
            Vector3 camForwardProjectedXZ =
                new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
            RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);
        }

        /// <summary>
        /// Can be used to change the camera, for example if instantiating new character after death
        /// </summary>
        /// <param name="newCamera"></param>
        public void SetCamera(Camera newCamera)
        {
            playerCamera = newCamera;
        }
        #endregion
    }
}