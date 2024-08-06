using System.Runtime.CompilerServices;
using Codice.CM.Client.Differences;
using UnityEngine;
using UnityEngine.AI;

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

        [Header("Environment Details")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private bool isGroundedDebug;

        [Header("Debug Speed")]
        [SerializeField] private float forwardSpeedDebug;
        [SerializeField] private float verticalSpeedDebug;
        [SerializeField] private float lateralSpeedDebug;
        [Header("Debug Targets")]
        [SerializeField] private float verticalVelocityDebug;
        [SerializeField] private Vector3 lateralVelocityDirectionDebug;
        [SerializeField] private Vector3 lateralVelocityDebug;
        [SerializeField] private Vector3 charControllerVelocityDebug;

        // Use this in parent classes to request a change in state - e.g. to crouch, roll or jump
        protected CharacterMovementStateChange RequestMovementStateChange;

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
        protected CharacterMovementState LastMovementState { get; set; } = CharacterMovementState.Idling;
        public CharacterController CharacterController
        {
            get => characterController;
            set => characterController = value;
        }

        // Set by parent to control target lateral and vertical movement
        protected Vector3 LateralVelocityDirection = Vector3.zero;
        protected Vector2 TargetRotation  = Vector2.zero;

        private bool _jumpedLastFrame;
        private float _antiBump;
        private float _stepOffset;
        private float _verticalSpeed;
        private Vector3 _lateralVelocity = Vector3.zero;
        #endregion

        #region Startup
        public virtual void Awake()
        {
            CharacterState = GetComponent<CharacterState>();
            _antiBump = sprintSpeed;
            _stepOffset = characterController.stepOffset;
        }
        #endregion

        #region Update Logic
        public virtual void Update()
        {
            #if UNITY_EDITOR
            UpdateDebugProperties();
            #endif
            CalculateCharacterSpeeds();
            UpdateMovementState();
            HandleVerticalMovement();
            HandleLateralMovement();
            Move();
            RequestMovementStateChange = CharacterMovementStateChange.None;
        }

        /// <summary>
        /// Derive velocities in each plane, so these can be used by the Animator to drive BlendTree
        /// animations.
        /// </summary>
        private void CalculateCharacterSpeeds()
        {
            // ForwardSpeed = transform.InverseTransformDirection(characterController.velocity).z;
            ForwardSpeed = (transform.forward * Vector3.Dot(transform.forward, characterController.velocity)).magnitude;
            // LateralSpeed = transform.InverseTransformDirection(characterController.velocity).x;
            LateralSpeed = (transform.right * Vector3.Dot(transform.right, characterController.velocity)).magnitude;
            // VerticalSpeed = transform.InverseTransformDirection(characterController.velocity).y;
            VerticalSpeed = (transform.up * Vector3.Dot(transform.up, characterController.velocity)).magnitude;
        }

        /// <summary>
        /// Determines the MovementState for the character. Can only be in one such state at a time. Uses
        /// the characters velocity to determine certain states, or can be influenced by public boolean setters to
        /// request a state change - for example, rolling or crouched states.
        /// </summary>
        private void UpdateMovementState()
        {
            LastMovementState = CharacterState.CurrentCharacterMovementState;

            CharacterMovementStateChange requestMovementChange = RequestMovementStateChange;

            bool isGrounded = IsGrounded();

            // If rising or falling, player must be in a vertical movement state
            // i.e. Jumping or Falling
            if ((!isGrounded || _jumpedLastFrame) && _verticalSpeed > 0.0f)
            {
                CharacterState.SetCharacterMovementState(CharacterMovementState.Jumping);
                _jumpedLastFrame = false;
                CharacterController.stepOffset = 0f;
                return;
            }

            if ((!isGrounded || _jumpedLastFrame) && _verticalSpeed < 0f)
            {
                CharacterState.SetCharacterMovementState(CharacterMovementState.Falling);
                _jumpedLastFrame = false;
                CharacterController.stepOffset = 0f;
                return;
            }

            // Handle a request for a state change

            // Check if character can move to the rolling state
            if (requestMovementChange == CharacterMovementStateChange.Rolling && CanRoll())
            {
                CharacterState.SetCharacterMovementState(CharacterMovementState.Rolling);
                CharacterController.stepOffset = _stepOffset;
                return;
            }

            if (requestMovementChange == CharacterMovementStateChange.Crouching && CanCrouch())
            {
                CharacterState.SetCharacterMovementState(CharacterMovementState.Crouching);
                CharacterController.stepOffset = _stepOffset;
                return;
            }

            // Player is grounded, not rolling or crouching, so must be in a ground based movement state
            if (ForwardSpeed > 0 && ForwardSpeed <= walkSpeed)
            {
                CharacterState.SetCharacterMovementState(CharacterMovementState.Walking);
            }
            else if (ForwardSpeed > walkSpeed && ForwardSpeed <= runSpeed)
            {
                CharacterState.SetCharacterMovementState(CharacterMovementState.Running);
            }
            else if (ForwardSpeed > runSpeed && ForwardSpeed <= sprintSpeed)
            {
                CharacterState.SetCharacterMovementState(CharacterMovementState.Sprinting);
            }
            else
            {
                CharacterState.SetCharacterMovementState(CharacterMovementState.Idling);
            }
            CharacterController.stepOffset = _stepOffset;

        }

        private void HandleVerticalMovement()
        {
            bool isGrounded = IsGrounded();

            // Falling
            _verticalSpeed -= gravity * Time.deltaTime;

            if (isGrounded && _verticalSpeed < 0)
            {
                _verticalSpeed = -_antiBump;
            }

            // Jump, if requested
            if (RequestMovementStateChange == CharacterMovementStateChange.Jump && CanJump())
            {
                _verticalSpeed+= _verticalSpeed + Mathf.Sqrt(jumpSpeed * 3 * gravity);
                _jumpedLastFrame = true;
            }

            // Give the character a bit of a bump, if moving from grounded to not grounded state
            if (CharacterState.IsStateGroundedState(LastMovementState) && !isGrounded)
            {
                _verticalSpeed += _antiBump;
            }

            // Clamp at terminal velocity
            if (Mathf.Abs(_verticalSpeed) > Mathf.Abs(terminalVelocity))
            {
                _verticalSpeed = -1f * Mathf.Abs(terminalVelocity);
            }
        }

        private void HandleLateralMovement()
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

            // Take the lateral velocity direction from the parent
            Vector3 movementDelta = LateralVelocityDirection * (lateralAcceleration * Time.deltaTime);
            Vector3 newVelocity = characterController.velocity + movementDelta;

            // Add drag to character
            float dragMagnitude = CharacterState.InGroundedState() ? drag : inAirDrag;
            Vector3 currentDrag = newVelocity.normalized * (dragMagnitude * Time.deltaTime);
            newVelocity = (newVelocity.magnitude > dragMagnitude * Time.deltaTime)
                ? newVelocity - currentDrag
                : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampLateralMagnitude);

            // Set target velocity
            _lateralVelocity = !CharacterState.InGroundedState() ? HandleSteepWalls(newVelocity) : newVelocity;
        }

        private void Move()
        {
            // Move character (Unity suggests only calling this once per tick)
            if (!CharacterState.InDeadState())
            {
                Vector3 newVelocity = _lateralVelocity;
                newVelocity.y += _verticalSpeed;
                characterController.Move(newVelocity * Time.deltaTime);
            }
        }

        private Vector3 HandleSteepWalls(Vector3 velocity)
        {
            Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(characterController, groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle <= characterController.slopeLimit;

            if (!validAngle && _verticalSpeed < 0f)
                velocity = Vector3.ProjectOnPlane(velocity, normal);

            return velocity;
        }

        #endregion

        #region State Checks
        private bool IsGrounded()
        {
            bool grounded = CharacterState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
            return grounded;
        }

        private bool IsGroundedWhileGrounded()
        {
            Vector3 spherePosition = new Vector3(transform.position.x,
                transform.position.y - characterController.radius, transform.position.z);

            bool grounded = Physics.CheckSphere(spherePosition, characterController.radius, groundLayers,
                QueryTriggerInteraction.Ignore);

            return grounded;
        }

        private bool IsGroundedWhileAirborne()
        {
            Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(characterController, groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle <= characterController.slopeLimit;

            return characterController.isGrounded && validAngle;
        }

        protected bool CanRun()
        {
            // Character is moving at less than a 45 degree angle, and so can run.
            return ForwardSpeed >= Mathf.Abs(LateralSpeed);
        }

        private bool CanRoll()
        {
            return CharacterState.CurrentCharacterMovementState != CharacterMovementState.Jumping &&
                   CharacterState.CurrentCharacterMovementState != CharacterMovementState.Falling;
        }

        private bool CanCrouch()
        {
            return CharacterState.CurrentCharacterMovementState != CharacterMovementState.Jumping &&
                   CharacterState.CurrentCharacterMovementState != CharacterMovementState.Falling &&
                   CharacterState.CurrentCharacterMovementState != CharacterMovementState.Rolling;
        }

        private bool CanJump()
        {
            return IsGrounded() &&
                   CharacterState.CurrentCharacterMovementState != CharacterMovementState.Rolling;
        }
        #endregion

        #region Editor methods
        #if UNITY_EDITOR
        public void SetGroundLayer(LayerMask layerMask)
        {
            groundLayers = layerMask;
        }

        private void UpdateDebugProperties()
        {
            isGroundedDebug = IsGrounded();
            forwardSpeedDebug = ForwardSpeed;
            verticalSpeedDebug = VerticalSpeed;
            lateralSpeedDebug = LateralSpeed;
            verticalVelocityDebug = _verticalSpeed;
            lateralVelocityDebug = _lateralVelocity;
            lateralVelocityDirectionDebug = LateralVelocityDirection;
            charControllerVelocityDebug = characterController.velocity;
        }
        #endif
        #endregion
    }
}