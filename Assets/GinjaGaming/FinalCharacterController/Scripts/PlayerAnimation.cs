using System.Linq;
using GinjaGaming.FinalCharacterController.Input;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController
{
    public class PlayerAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private float locomotionBlendSpeed = 4f;

        public bool RollingAnimIsPlaying { get; set; }

        private PlayerState _playerState;
        private PlayerController _playerController;
        private PlayerActionsInput _playerActionsInput;

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

        // Camera/Rotation
        private static readonly int IsRotatingToTargetHash = Animator.StringToHash("IsRotatingToTarget");
        private static readonly int RotationMismatchHash = Animator.StringToHash("RotationMismatch");

        private Vector3 _currentBlendInput = Vector3.zero;

        private void Awake()
        {
            _playerState = GetComponent<PlayerState>();
            _playerController = GetComponent<PlayerController>();
            _playerActionsInput = GetComponent<PlayerActionsInput>();
            _actionHashes = new int[] { IsGatheringHash };
        }

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
            bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
            bool isCrouched = _playerState.CurrentPlayerMovementState == PlayerMovementState.Crouching;
            bool isRolling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Rolling;

            bool isGrounded = _playerState.InGroundedState();
            bool isPlayingAction = _actionHashes.Any(hash => animator.GetBool(hash));

            Vector3 inputTarget = new Vector3(_playerController.LateralSpeed, _playerController.VerticalSpeed, _playerController.ForwardSpeed);
            _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

            animator.SetBool(IsGroundedHash, isGrounded);
            animator.SetBool(IsIdlingHash, isIdling);
            animator.SetBool(IsFallingHash, isFalling);
            animator.SetBool(IsJumpingHash, isJumping);
            animator.SetBool(IsRotatingToTargetHash, _playerController.IsRotatingToTarget);
            animator.SetBool(IsAttackingHash, _playerActionsInput.AttackPressed);
            animator.SetBool(IsGatheringHash, _playerActionsInput.GatherPressed);
            animator.SetBool(IsRollingHash, isRolling);
            animator.SetBool(IsCrouchedHash, isCrouched);
            animator.SetBool(IsPlayingActionHash, isPlayingAction);

            animator.SetFloat(LateralSpeedHash, _currentBlendInput.x);
            animator.SetFloat(ForwardSpeedHash, _currentBlendInput.z);
            animator.SetFloat(VerticalSpeedHash, _currentBlendInput.y);
            animator.SetFloat(RotationMismatchHash, _playerController.RotationMismatch);
        }
    }
}