using System.Linq;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core
{
    public class CharacterAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private float locomotionBlendSpeed = 4f;

        private CharacterState _characterState;
        private CharacterControllerBase _characterController;

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

        // Camera/Rotation
        private static readonly int IsRotatingToTargetHash = Animator.StringToHash("IsRotatingToTarget");
        private static readonly int RotationMismatchHash = Animator.StringToHash("RotationMismatch");

        private Vector3 _currentBlendInput = Vector3.zero;

        private void Awake()
        {
            _characterState = GetComponent<CharacterState>();
            _characterController = GetComponent<CharacterControllerBase>();
            _actionHashes = new int[] { IsGatheringHash };
        }

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
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

            Vector3 inputTarget = new Vector3(_characterController.LateralSpeed, _characterController.VerticalSpeed, _characterController.ForwardSpeed);
            _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

            animator.SetBool(IsInjured, isInjured);
            animator.SetBool(IsDead, isDead);

            animator.SetBool(IsGroundedHash, isGrounded);
            animator.SetBool(IsIdlingHash, isIdling);
            animator.SetBool(IsFallingHash, isFalling);
            animator.SetBool(IsJumpingHash, isJumping);
            animator.SetBool(IsRotatingToTargetHash, _characterController.IsRotatingToTarget);

            animator.SetBool(IsAttackingHash, isAttacking);
            animator.SetBool(IsGatheringHash, isGathering);

            animator.SetBool(IsRollingHash, isRolling);
            animator.SetBool(IsCrouchedHash, isCrouched);
            animator.SetBool(IsPlayingActionHash, isPlayingAction);

            animator.SetFloat(LateralSpeedHash, _currentBlendInput.x);
            animator.SetFloat(ForwardSpeedHash, _currentBlendInput.z);
            animator.SetFloat(VerticalSpeedHash, _currentBlendInput.y);
            animator.SetFloat(RotationMismatchHash, _characterController.RotationMismatch);
        }
    }
}