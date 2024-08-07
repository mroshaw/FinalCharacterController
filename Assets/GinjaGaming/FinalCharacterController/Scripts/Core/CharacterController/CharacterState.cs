using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.CharacterController
{
    /// <summary>
    /// These are the various Movement, Action, and Health states. The character can be in only one of these states,
    /// within each type, at any one time.
    /// </summary>
    public enum CharacterMovementState
    {
        None,
        Idling,
        Walking,
        Running,
        Sprinting,
        Jumping,
        Falling,
        Strafing,
        Rolling,
        Crouching,
    }

    public enum CharacterMovementStateChange
    {
        None,
        Rolling,
        Crouching,
        Jump
    }

    public enum CharacterActionState
    {
        None,
        Attacking,
        Gathering,
    }

    public enum CharacterHealthState
    {
        Fine,
        Injured,
        Dead,
    }

    public class CharacterState : MonoBehaviour
    {
        [field: SerializeField] public CharacterMovementState CurrentCharacterMovementState { get; private set; } = CharacterMovementState.Idling;
        [field: SerializeField] public CharacterActionState CurrentCharacterActionState { get; private set; } = CharacterActionState.None;
        [field: SerializeField] public CharacterHealthState CurrentCharacterHealthState { get; private set; } = CharacterHealthState.Fine;

        #region Class methods
        public void ResetStates()
        {
            CurrentCharacterMovementState = CharacterMovementState.Idling;
            CurrentCharacterActionState = CharacterActionState.None;
            CurrentCharacterHealthState = CharacterHealthState.Fine;
        }

        public void SetCharacterMovementState(CharacterMovementState characterMovementState)
        {
            CurrentCharacterMovementState = characterMovementState;
        }

        public void SetCharacterActionState(CharacterActionState characterActionState)
        {
            CurrentCharacterActionState = characterActionState;
        }

        public void SetCharacterHealthState(CharacterHealthState characterHealthState)
        {
            CurrentCharacterHealthState = characterHealthState;
        }

        public void SetCharacterInjuredState()
        {
            CurrentCharacterHealthState = CharacterHealthState.Injured;
        }

        public void SetCharacterDeadState()
        {
            CurrentCharacterMovementState = CharacterMovementState.Idling;
            CurrentCharacterActionState = CharacterActionState.None;
            CurrentCharacterHealthState = CharacterHealthState.Dead;
        }

        public void SetCharacterFineState()
        {
            CurrentCharacterHealthState = CharacterHealthState.Fine;
        }

        public bool InDeadState()
        {
            return CurrentCharacterHealthState == CharacterHealthState.Dead;
        }

        public bool InGroundedState()
        {
            return IsStateGroundedState(CurrentCharacterMovementState);
        }

        public bool InRollingState()
        {
            return CurrentCharacterMovementState == CharacterMovementState.Rolling;
        }

        public bool InWalkingState()
        {
            return CurrentCharacterMovementState == CharacterMovementState.Walking;
        }

        public bool InRunningState()
        {
            return CurrentCharacterMovementState == CharacterMovementState.Running;
        }

        public bool InSprintingState()
        {
            return CurrentCharacterMovementState == CharacterMovementState.Sprinting;
        }

        public bool InCrouchingState()
        {
            return CurrentCharacterMovementState == CharacterMovementState.Crouching;
        }

        public bool IsStateGroundedState(CharacterMovementState movementState)
        {
            return movementState is CharacterMovementState.Idling or CharacterMovementState.Walking or
                CharacterMovementState.Running or CharacterMovementState.Sprinting or CharacterMovementState.Rolling
                or CharacterMovementState.Crouching;
        }
        #endregion
    }
}