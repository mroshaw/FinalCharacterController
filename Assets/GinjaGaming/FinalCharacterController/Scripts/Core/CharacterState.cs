using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core
{
    public class CharacterState : MonoBehaviour
    {
        [field: SerializeField] public CharacterMovementState CurrentCharacterMovementState { get; private set; } = CharacterMovementState.Idling;
        [field: SerializeField] public CharacterActionState CurrentCharacterActionState { get; private set; } = CharacterActionState.Idle;
        [field: SerializeField] public CharacterHealthState CurrentCharacterHealthState { get; private set; } = CharacterHealthState.Fine;

        public void ResetStates()
        {
            CurrentCharacterMovementState = CharacterMovementState.Idling;
            CurrentCharacterActionState = CharacterActionState.Idle;
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
            CurrentCharacterActionState = CharacterActionState.Idle;
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
    }
    public enum CharacterMovementState
    {
        Idling = 0,
        Walking = 1,
        Running = 2,
        Sprinting = 3,
        Jumping = 4,
        Falling = 5,
        Strafing = 6,
        Rolling = 7,
        Crouching = 8,
    }

    public enum CharacterActionState
    {
        Idle = 0,
        Attacking = 1,
        Gathering = 2,
    }

    public enum CharacterHealthState
    {
        Fine = 0,
        Injured = 1,
        Dead = 2,
    }
}