using System.Linq;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.CharacterController
{
    public class CharacterAnimation : MonoBehaviour
    {
        #region Class Variables
        [Header("Animation Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private float forwardAnimationSmoothing = 4f;
        [SerializeField] private float turnAnimationSmoothing = 10f;
        [SerializeField] private float verticalAnimationSmoothing = 4f;

        private float _currentForwardSpeed;
        private float _currentLateralSpeed;
        private float _currentVerticalSpeed;
        private float _currentRotationSpeed;

        private CharacterState _characterState;
        protected BaseCharacterController BaseCharacterController { get; private set; }
        protected Animator Animator => animator;

        // Locomotion
        private static readonly int LateralSpeedHash = Animator.StringToHash("LateralSpeed");
        private static readonly int ForwardSpeedHash = Animator.StringToHash("ForwardSpeed");
        private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
        private static readonly int IsIdlingHash = Animator.StringToHash("IsIdling");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsFallingHash = Animator.StringToHash("IsFalling");
        private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
        private static readonly int IsRollingHash = Animator.StringToHash("IsRolling");
        private static readonly int IsCrouchedHash = Animator.StringToHash("IsCrouched");

        // Actions
        private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
        private static readonly int IsGatheringHash = Animator.StringToHash("IsGathering");
        private static readonly int IsPlayingActionHash = Animator.StringToHash("IsPlayingAction");
        private int[] _actionHashes;

        // Health State
        private static readonly int IsInjured = Animator.StringToHash("IsInjured");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        #endregion

        #region Startup
        protected virtual void Awake()
        {
            _characterState = GetComponent<CharacterState>();
            BaseCharacterController = GetComponent<BaseCharacterController>();
            _actionHashes = new[] { IsGatheringHash };
        }
        #endregion

        #region Update
        private void Update()
        {
            UpdateAnimationState();
        }

        protected virtual void UpdateAnimationState()
        {
            bool isIdling = _characterState.CurrentCharacterMovementState == CharacterMovementState.Idling;
            bool isJumping = _characterState.CurrentCharacterMovementState == CharacterMovementState.Jumping;
            bool isFalling = _characterState.CurrentCharacterMovementState == CharacterMovementState.Falling;
            bool isCrouched = _characterState.CurrentCharacterMovementState == CharacterMovementState.Crouching;
            bool isRolling = _characterState.CurrentCharacterMovementState == CharacterMovementState.Rolling;

            bool isAttacking = _characterState.CurrentCharacterActionState == CharacterActionState.Attacking;
            bool isGathering = _characterState.CurrentCharacterActionState == CharacterActionState.Gathering;

            bool isGrounded = _characterState.InGroundedState();
            bool isPlayingAction = _actionHashes.Any(hash => animator.GetBool(hash));

            bool isInjured = _characterState.CurrentCharacterHealthState == CharacterHealthState.Injured;
            bool isDead = _characterState.CurrentCharacterHealthState == CharacterHealthState.Dead;

            animator.SetBool(IsInjured, isInjured);
            animator.SetBool(IsDead, isDead);

            animator.SetBool(IsGroundedHash, isGrounded);
            animator.SetBool(IsIdlingHash, isIdling);
            animator.SetBool(IsFallingHash, isFalling);
            animator.SetBool(IsJumpingHash, isJumping);

            animator.SetBool(IsAttackingHash, isAttacking);
            animator.SetBool(IsGatheringHash, isGathering);

            animator.SetBool(IsRollingHash, isRolling);
            animator.SetBool(IsCrouchedHash, isCrouched);
            animator.SetBool(IsPlayingActionHash, isPlayingAction);

            _currentForwardSpeed = Mathf.Lerp(_currentForwardSpeed, BaseCharacterController.ForwardSpeed,
                forwardAnimationSmoothing * Time.deltaTime);
            animator.SetFloat(ForwardSpeedHash, _currentForwardSpeed);

            _currentLateralSpeed = Mathf.Lerp(_currentLateralSpeed, BaseCharacterController.LateralSpeed,
                turnAnimationSmoothing * Time.deltaTime);
            animator.SetFloat(LateralSpeedHash, _currentLateralSpeed);

            _currentVerticalSpeed = Mathf.Lerp(_currentVerticalSpeed, BaseCharacterController.VerticalSpeed,
                verticalAnimationSmoothing * Time.deltaTime);
            animator.SetFloat(VerticalSpeedHash, _currentVerticalSpeed);

        }
        #endregion

        #region Editor Helper methods
        #if UNITY_EDITOR
        public void SetAnimator(Animator newAnimator)
        {
            animator = newAnimator;
        }
        #endif
        #endregion
    }
}