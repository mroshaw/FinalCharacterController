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
        [SerializeField] private Camera _playerCamera;
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
        private Vector2 _playerTargetRotation = Vector2.zero;
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

        protected override void UpdateCharacterVelocities()
        {
            base.UpdateCharacterVelocities();
        }

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

            bool isGrounded = IsGrounded();

            CharacterMovementState lateralState = isRolling ? CharacterMovementState.Rolling :
                isCrouching ? CharacterMovementState.Crouching :
                isWalking ? CharacterMovementState.Walking :
                isSprinting ? CharacterMovementState.Sprinting :
                isMovingLaterally || isMovementInput ? CharacterMovementState.Running : CharacterMovementState.Idling;

            CharacterState.SetCharacterMovementState(lateralState);

            // Control Airborne State
            if ((!isGrounded || JumpedLastFrame) && CharacterController.velocity.y > 0.0f)
            {
                CharacterState.SetCharacterMovementState(CharacterMovementState.Jumping);
                JumpedLastFrame = false;
                CharacterController.stepOffset = 0f;
            }
            else if ((!isGrounded || JumpedLastFrame) && CharacterController.velocity.y <= 0f)
            {
                CharacterState.SetCharacterMovementState(CharacterMovementState.Falling);
                JumpedLastFrame = false;
                CharacterController.stepOffset = 0f;
            }
            else
            {
                CharacterController.stepOffset = StepOffset;
            }
            base.UpdateMovementState();
        }

        protected override void HandleVerticalMovement()
        {
            VerticalVelocity -= gravity * Time.deltaTime;

            if (CharacterState.InGroundedState() && VerticalVelocity < 0)
                VerticalVelocity = -AntiBump;

            if (_playerLocomotionInput.JumpPressed && CharacterState.InGroundedState() && !CharacterState.InRollingState())
            {
                // Cancel crouch
                _playerLocomotionInput.SetCrouchToggleToFalse();
                VerticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
                JumpedLastFrame = true;
            }

            if (CharacterState.IsStateGroundedState(LastMovementState) && !CharacterState.InGroundedState())
            {
                VerticalVelocity += AntiBump;
            }

            // Clamp at terminal velocity
            if (Mathf.Abs(VerticalVelocity) > Mathf.Abs(terminalVelocity))
            {
                VerticalVelocity = -1f * Mathf.Abs(terminalVelocity);
            }
            base.HandleVerticalMovement();
        }

        protected override void HandleLateralMovement()
        {
            base.HandleLateralMovement();
        }

        protected override Vector3 GetMovementDirection()
        {
            Vector3 cameraForwardXZ =
                new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z)
                .normalized;

            // If rolling, then ignore the input and move forward
            Vector3 movementDirection = !CharacterState.InRollingState() ? (cameraRightXZ *  _playerLocomotionInput.MovementInput.x +
                                                                            cameraForwardXZ * _playerLocomotionInput.MovementInput.y) : transform.forward;

            return movementDirection;
        }

        protected override Vector3 HandleSteepWalls(Vector3 velocity)
        {
            return base.HandleSteepWalls(velocity);
        }
        #endregion

        #region Late Update Logic

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        protected override void UpdateCameraRotation()
        {
            base.UpdateCameraRotation();
            float lookInputX = _playerLocomotionInput.LookInput.x;
            float lookInputY = _playerLocomotionInput.LookInput.y;

            if (_playerLocomotionInput.ActiveDevice is Gamepad)
            {
                lookInputX *= gamepadLookXMultiplier;
                lookInputY *= gamepadLookYMultiplier;
            }

            _cameraRotation.x += lookSenseH * lookInputX;
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * lookInputY, -lookLimitV, lookLimitV);
            _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * lookInputX;

            float rotationTolerance = 90f;
            bool isIdling = CharacterState.CurrentCharacterMovementState == CharacterMovementState.Idling;
            IsRotatingToTarget = RotatingToTargetTimer > 0;

            if (!CharacterState.InDeadState())
            {
                // ROTATE if we're not idling
                if (!isIdling)
                {
                    RotatePlayerToTarget();
                }
                // If rotation mismatch not within tolerance, or rotate to target is active, ROTATE
                else if (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget)
                {
                    UpdateIdleRotation(rotationTolerance);
                }
            }
            _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

            // Get angle between camera and player
            Vector3 camForwardProjectedXZ =
                new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
            RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);
        }

        protected override void UpdateIdleRotation(float rotationTolerance)
        {
            base.UpdateIdleRotation(rotationTolerance);
        }

        protected override void RotatePlayerToTarget()
        {
            Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX,
                playerModelRotationSpeed * Time.deltaTime);
            base.RotatePlayerToTarget();
        }

        #endregion

        #region State Checks

        protected override bool IsMovingLaterally()
        {
            return base.IsMovingLaterally();
        }

        protected override bool IsGrounded()
        {
            return base.IsGrounded();
        }

        protected override bool IsGroundedWhileGrounded()
        {
            return base.IsGroundedWhileGrounded();
        }

        protected override bool IsGroundedWhileAirborne()
        {
            return base.IsGroundedWhileAirborne();
        }

        protected override bool CanRun()
        {
            // This means player is moving diagonally at 45 degrees or forward, if so, we can run
            return _playerLocomotionInput.MovementInput.y >= Mathf.Abs(_playerLocomotionInput.MovementInput.x);
        }

        protected override bool CanRoll()
        {
            return base.CanRoll();
        }

        #endregion
    }
}