using GinjaGaming.FinalCharacterController.Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GinjaGaming.FinalCharacterController.Core.Player
{
    [DefaultExecutionOrder(-1)]
    public class PlayerController : CharacterControllerBase
    {
        #region Class Variables
        [Header("Camera Settings")]
        [SerializeField] private Camera playerCamera;
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;

        [Header("Gamepad Settings")]
        public float gamepadLookXMultiplier = 5.0f;
        public float gamepadLookYMultiplier = 5.0f;
        public float gamepadMoveRunThreshold = 0.5f;

        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerActionsInput _playerActionInput;
        private Vector2 _cameraRotation = Vector2.zero;

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
            UpdateActionState();
            base.Update();
        }

        /// <summary>
        /// This is the 'Player Action' state machine. This controls what actions the player can and will perform,
        /// based on current action states, and from and to permitted action states.
        /// </summary>
        protected override void UpdateActionState()
        {
            if (_playerActionInput.AttackPressed)
            {
                _playerActionInput.SetGatherPressedFalse();
                CharacterState.SetCharacterActionState(CharacterActionState.Attacking);
            }

            if (_playerActionInput.GatherPressed && CharacterState.CurrentCharacterActionState == CharacterActionState.Idle)
            {
                CharacterState.SetCharacterActionState(CharacterActionState.Gathering);
            }

            if (!_playerActionInput.GatherPressed && !_playerActionInput.AttackPressed)
            {
                CharacterState.SetCharacterActionState(CharacterActionState.Idle);
            }
        }

        /// <summary>
        /// This is the 'Player Movement' state machine. This method therefore controls what state the player is in, and from
        /// and to which states the player transitions, typically based on input received.
        /// </summary>
        protected override void UpdateMovementState()
        {
            // Process player input
            bool canRun = CanRun();
            bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero; //order
            bool isMovingLaterally = IsMovingLaterally(); //matters
            bool isSprinting = isMovingLaterally && (!canRun || _playerLocomotionInput.SprintToggledOn);
            bool isCrouching = _playerLocomotionInput.CrouchToggledOn;
            bool isRolling = _playerLocomotionInput.RollPressed;
            bool isWalking;
            if (_playerLocomotionInput.ActiveDevice is Gamepad)
            {
                isWalking = _playerLocomotionInput.MovementInput.y < gamepadMoveRunThreshold;
            }
            else
            {
                isWalking = isMovingLaterally && (!canRun || _playerLocomotionInput.WalkToggledOn);
            }

            // Cancel roll if not able to do so
            if (_playerLocomotionInput.RollPressed && !CanRoll())
            {
                _playerLocomotionInput.SetRollPressedFalse();
            }

            LastMovementState = CharacterState.CurrentCharacterMovementState;

            CharacterMovementState lateralState = isRolling ? CharacterMovementState.Rolling :
                isCrouching ? CharacterMovementState.Crouching :
                isWalking ? CharacterMovementState.Walking :
                isSprinting ? CharacterMovementState.Sprinting :
                isMovingLaterally || isMovementInput ? CharacterMovementState.Running : CharacterMovementState.Idling;

            CharacterState.SetCharacterMovementState(lateralState);

            base.UpdateMovementState();
        }

        protected override void HandleVerticalMovement()
        {
            UpdateVerticalVelocity();

            // Handle Jump pressed
            if (_playerLocomotionInput.JumpPressed && CharacterState.InGroundedState() && !CharacterState.InRollingState())
            {
                // Cancel crouch
                _playerLocomotionInput.SetCrouchToggleToFalse();
                VerticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
                JumpedLastFrame = true;
            }
            base.HandleVerticalMovement();
        }

        protected override Vector3 GetMovementDirection()
        {
            // Derive movement direction from the player camera
            Vector3 cameraForwardXZ =
                new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(playerCamera.transform.right.x, 0f, playerCamera.transform.right.z)
                .normalized;

            // If rolling, then ignore the input and move forward
            Vector3 movementDirection = !CharacterState.InRollingState() ? (cameraRightXZ *  _playerLocomotionInput.MovementInput.x +
                                                                            cameraForwardXZ * _playerLocomotionInput.MovementInput.y) : transform.forward;

            return movementDirection;
        }
        #endregion

        #region Late Update Logic
        private void LateUpdate()
        {
            UpdateCameraRotation();
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
            IsRotatingToTarget = RotatingToTargetTimer > 0;

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
        #endregion

        #region State Checks
        protected override bool CanRun()
        {
            // This means player is moving diagonally at 45 degrees or forward, if so, we can run
            return _playerLocomotionInput.MovementInput.y >= Mathf.Abs(_playerLocomotionInput.MovementInput.x);
        }
        #endregion
    }
}