using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core
{
    /// <summary>
    /// This is an abstract class from which 'CharacterControllers' inherit core functionality to move a
    /// character. For example, this class is inherited by the 'PlayerController', that adds both end-user
    /// input and camera controls to create a player controlled character. This class is also inherited by
    /// an 'AIController' that effectively takes input from a 'NavMeshAgent', rather than input from the player.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public abstract class CharacterControllerBase : MonoBehaviour
    {
        #region Class Variables
        [Header("Components")]
        [SerializeField] private CharacterController characterController;

        public float RotationMismatch { get; set; }
        public bool IsRotatingToTarget { get; set; }

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

        /// <summary>
        /// These properties are derived from the controller velocity, and can be used by the Animator
        /// to drive animations based on the properties of the controller, rather than by properties of the input.
        /// This abstracts the Animator from Input, allowing it to be used for other purposes, such as AI characters.
        /// </summary>
        public float ForwardSpeed { get; private set; }
        public float LateralSpeed { get; private set; }
        public float VerticalSpeed { get; private set; }

        /// <summary>
        /// These protected properties are used and set by super-class implementations, for example the PlayerController
        /// and AIController.
        /// </summary>
        protected CharacterState CharacterState { get; private set; }
        protected CharacterMovementState LastMovementState { get; set; } = CharacterMovementState.Falling;
        public CharacterController CharacterController
        {
            get => characterController;
            set => characterController = value;
        }

        protected bool JumpedLastFrame { get; set; }
        protected float VerticalVelocity { get; set; }
        private float AntiBump { get; set; }
        private float StepOffset { get; set; }
        protected float RotatingToTargetTimer { get; private set; }

        protected Vector2 TargetRotation  = Vector2.zero;

        private bool _isRotatingClockwise;
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

        /// <summary>
        /// Derive velocities in each plane, so these can be used by the Animator to drive BlendTree
        /// animations.
        /// </summary>
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
            bool isGrounded = IsGrounded();

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
        }

        protected virtual void HandleVerticalMovement()
        {
            if (CharacterState.IsStateGroundedState(LastMovementState) && !CharacterState.InGroundedState())
            {
                VerticalVelocity += AntiBump;
            }

            // Clamp at terminal velocity
            if (Mathf.Abs(VerticalVelocity) > Mathf.Abs(terminalVelocity))
            {
                VerticalVelocity = -1f * Mathf.Abs(terminalVelocity);
            }
        }

        protected virtual void UpdateVerticalVelocity()
        {
            VerticalVelocity -= gravity * Time.deltaTime;

            if (CharacterState.InGroundedState() && VerticalVelocity < 0)
            {
                VerticalVelocity = -AntiBump;
            }
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

            // Add drag to character
            float dragMagnitude = CharacterState.InGroundedState() ? drag : inAirDrag;
            Vector3 currentDrag = newVelocity.normalized * (dragMagnitude * Time.deltaTime);
            newVelocity = (newVelocity.magnitude > dragMagnitude * Time.deltaTime)
                ? newVelocity - currentDrag
                : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampLateralMagnitude);
            newVelocity.y += VerticalVelocity;
            newVelocity = !CharacterState.InGroundedState() ? HandleSteepWalls(newVelocity) : newVelocity;

            // Move character (Unity suggests only calling this once per tick)
            if (!CharacterState.InDeadState())
            {
                characterController.Move(newVelocity * Time.deltaTime);
            }
        }

        /// <summary>
        /// Implement this in the parent class to return the direction vector for the character. This can, for example,
        /// use end-user input for a PlayerController, or results from a NavMeshAgent for an AIController.
        /// </summary>
        /// <returns></returns>
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
        protected virtual void UpdateIdleRotation(float rotationTolerance)
        {
            // Initiate new rotation direction
            if (Mathf.Abs(RotationMismatch) > rotationTolerance)
            {
                RotatingToTargetTimer = rotateToTargetTime;
                _isRotatingClockwise = RotationMismatch > rotationTolerance;
            }

            RotatingToTargetTimer -= Time.deltaTime;

            // Rotate character
            if (_isRotatingClockwise && RotationMismatch > 0f ||
                !_isRotatingClockwise && RotationMismatch < 0f)
            {
                RotateToTarget();
            }
        }

        protected virtual void RotateToTarget()
        {
            Quaternion targetRotationX = Quaternion.Euler(0f, TargetRotation.x, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX,
                playerModelRotationSpeed * Time.deltaTime);
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
            // Character is moving at less than a 45 degree angle, and so can run.
            return ForwardSpeed >= Mathf.Abs(LateralSpeed);
        }

        protected virtual bool CanRoll()
        {
            return CharacterState.CurrentCharacterMovementState != CharacterMovementState.Jumping &&
                   CharacterState.CurrentCharacterMovementState != CharacterMovementState.Falling;
        }
        #endregion

        #region Editor Helper methods
        #if UNITY_EDITOR
        public void SetGroundLayer(LayerMask layerMask)
        {
            groundLayers = layerMask;
        }
        #endif
        #endregion
    }
}