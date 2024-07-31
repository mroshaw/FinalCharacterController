using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core
{
    [DefaultExecutionOrder(-1)]
    public abstract class CharacterControllerBase : MonoBehaviour
    {
        #region Class Variables

        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        public float RotationMismatch { get; set; } = 0f;
        public bool IsRotatingToTarget { get; set; } = false;

        [Header("Base Movement")]
        public float walkAcceleration = 25f;
        public float walkSpeed = 2f;
        public float runAcceleration = 35f;
        public float runSpeed = 4f;
        public float sprintAcceleration = 50f;
        public float sprintSpeed = 7f;
        public float crouchAcceleration = 15f;
        public float crouchSpeed = 1.2f;
        public float rollingAcceleration = 25f;
        public float rollingSpeed = 2.5f;
        public float inAirAcceleration = 25f;
        public float drag = 20f;
        public float inAirDrag = 5f;
        public float gravity = 25f;
        public float terminalVelocity = 50f;
        public float jumpSpeed = 0.8f;
        public float movingThreshold = 0.01f;

        [Header("Animation")]
        public float playerModelRotationSpeed = 10f;
        public float rotateToTargetTime = 0.67f;

        [Header("Environment Details")]
        [SerializeField] private LayerMask groundLayers;

        public float ForwardSpeed { get; private set; }
        public float LateralSpeed { get; private set; }
        public float VerticalSpeed { get; private set; }

        protected CharacterState CharacterState { get; private set; }
        protected CharacterMovementState LastMovementState { get; set; } = CharacterMovementState.Falling;
        protected CharacterController CharacterController => characterController;
        protected bool JumpedLastFrame { get; set; } = false;
        protected float VerticalVelocity { get; set; } = 0f;
        protected float AntiBump { get; private set; }
        protected float StepOffset { get; private set; }
        protected float RotatingToTargetTimer { get; private set; } = 0f;

        private bool _isRotatingClockwise = false;
        #endregion

        #region Startup
        public virtual void Awake()
        {
            CharacterState = GetComponent<CharacterState>();
            AntiBump = sprintSpeed;
            StepOffset = characterController.stepOffset;
        }

        #endregion

        #region Update Logic
        public virtual void Update()
        {
            UpdateMovementState();
            HandleVerticalMovement();
            HandleLateralMovement();
            UpdateCharacterVelocities();
        }

        protected virtual void UpdateCharacterVelocities()
        {
            ForwardSpeed = transform.InverseTransformDirection(characterController.velocity).z;
            LateralSpeed = transform.InverseTransformDirection(characterController.velocity).x;
            VerticalSpeed = transform.InverseTransformDirection(characterController.velocity).y;
        }

        protected virtual void UpdateActionState()
        {

        }

        protected virtual void UpdateMovementState()
        {

        }

        protected virtual void HandleVerticalMovement()
        {

        }

        protected virtual void HandleLateralMovement()
        {
            // State dependent acceleration and speed
            float lateralAcceleration = !CharacterState.InGroundedState() ? inAirAcceleration :
                CharacterState.InRollingState() ? rollingAcceleration :
                CharacterState.InCrouchingState() ? crouchAcceleration :
                CharacterState.InWalkingState()? walkAcceleration :
                CharacterState.InSprintingState() ? sprintAcceleration : runAcceleration;

            float clampLateralMagnitude = !CharacterState.InGroundedState() ? sprintSpeed :
                CharacterState.InRollingState() ? rollingSpeed :
                CharacterState.InCrouchingState() ? crouchSpeed :
                CharacterState.InWalkingState() ? walkSpeed :
                CharacterState.InSprintingState() ? sprintSpeed : runSpeed;

            Vector3 movementDirection = GetMovementDirection();
            Vector3 movementDelta = movementDirection * (lateralAcceleration * Time.deltaTime);
            Vector3 newVelocity = characterController.velocity + movementDelta;

            // Add drag to player
            float dragMagnitude = CharacterState.InGroundedState() ? drag : inAirDrag;
            Vector3 currentDrag = newVelocity.normalized * (dragMagnitude * Time.deltaTime);
            newVelocity = (newVelocity.magnitude > dragMagnitude * Time.deltaTime)
                ? newVelocity - currentDrag
                : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampLateralMagnitude);
            newVelocity.y += VerticalVelocity;
            newVelocity = !CharacterState.InGroundedState() ? HandleSteepWalls(newVelocity) : newVelocity;

            // Move character (Unity suggests only calling this once per tick)
            characterController.Move(newVelocity * Time.deltaTime);
        }

        protected abstract Vector3 GetMovementDirection();

        protected virtual Vector3 HandleSteepWalls(Vector3 velocity)
        {
            Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(characterController, groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle <= characterController.slopeLimit;

            if (!validAngle && VerticalVelocity < 0f)
                velocity = Vector3.ProjectOnPlane(velocity, normal);

            return velocity;
        }

        #endregion

        #region Late Update Logic

        public virtual void LateUpdate()
        {
            UpdateCameraRotation();
        }

        protected virtual void UpdateCameraRotation()
        {

        }

        protected virtual void UpdateIdleRotation(float rotationTolerance)
        {
            // Initiate new rotation direction
            if (Mathf.Abs(RotationMismatch) > rotationTolerance)
            {
                RotatingToTargetTimer = rotateToTargetTime;
                _isRotatingClockwise = RotationMismatch > rotationTolerance;
            }

            RotatingToTargetTimer -= Time.deltaTime;

            // Rotate player
            if (_isRotatingClockwise && RotationMismatch > 0f ||
                !_isRotatingClockwise && RotationMismatch < 0f)
            {
                RotatePlayerToTarget();
            }
        }

        protected virtual void RotatePlayerToTarget()
        {

        }

        #endregion
        #region State Checks

        protected virtual bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);

            return lateralVelocity.magnitude > movingThreshold;
        }

        protected virtual bool IsGrounded()
        {
            bool grounded = CharacterState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();

            return grounded;
        }

        protected virtual bool IsGroundedWhileGrounded()
        {
            Vector3 spherePosition = new Vector3(transform.position.x,
                transform.position.y - characterController.radius, transform.position.z);

            bool grounded = Physics.CheckSphere(spherePosition, characterController.radius, groundLayers,
                QueryTriggerInteraction.Ignore);

            return grounded;
        }

        protected virtual bool IsGroundedWhileAirborne()
        {
            Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(characterController, groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle <= characterController.slopeLimit;

            return characterController.isGrounded && validAngle;
        }

        protected virtual bool CanRun()
        {
            return false;
        }

        protected virtual bool CanRoll()
        {
            return CharacterState.CurrentCharacterMovementState != CharacterMovementState.Jumping &&
                   CharacterState.CurrentCharacterMovementState != CharacterMovementState.Falling;
        }
        #endregion
    }
}