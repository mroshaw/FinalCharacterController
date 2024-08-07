using GinjaGaming.FinalCharacterController.Core.CharacterController;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.PlayerController
{
    public class PlayerAnimation : CharacterAnimation
    {
        #region Class Variables
        // Camera/Rotation
        private static readonly int IsRotatingToTargetHash = Animator.StringToHash("IsRotatingToTarget");
        private static readonly int RotationMismatchHash = Animator.StringToHash("RotationMismatch");

        private PlayerController _playerController;
        #endregion

        #region Startup
        protected override void Awake()
        {
            base.Awake();
            _playerController = BaseCharacterController as PlayerController;
        }
        #endregion

        #region Update
        protected override void UpdateAnimationState()
        {
            base.UpdateAnimationState();
            Animator.SetBool(IsRotatingToTargetHash, _playerController.IsRotatingToTarget);
            Animator.SetFloat(RotationMismatchHash, _playerController.RotationMismatch);
        }
        #endregion
    }
}