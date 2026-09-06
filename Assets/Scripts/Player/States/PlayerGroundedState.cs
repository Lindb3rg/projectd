using UnityEngine;

using EPlayerState = PlayerStateMachine.EPlayerState;

// Base for every state where the character is standing on a surface.
// Owns the shared exits (falling, jumping) and the locomotion tier decision,
// so Idle/Walk/Run/Sprint don't each reimplement the same threshold logic.
public abstract class PlayerGroundedState : PlayerState
{
    protected PlayerGroundedState(EPlayerState key, PlayerContext ctx) : base(key, ctx) { }

    // Default transition logic for grounded states. Land and Crouch override
    // this because they have their own exit conditions.
    public override EPlayerState GetNextState()
    {
        if (!_ctx.IsGrounded)
            return EPlayerState.Fall;

        if (_ctx.JumpPressed && _ctx.CanCoyoteJump)
            return EPlayerState.Jump;

        return GetLocomotionState();
    }

    // Maps current intent to the matching locomotion state.
    // Deliberately does NOT branch on wall contact: ApplyGroundMovement already
    // zeroes velocity against a wall, and returning Idle here made the machine
    // oscillate Walk <-> Idle every frame while the stick was held into it.
    protected EPlayerState GetLocomotionState()
    {
        if (_ctx.IsCrouching)
            return EPlayerState.Crouch;

        float input = Mathf.Abs(_ctx.MoveInput.x);

        if (input < _ctx.Config.MoveDeadzone)
            return EPlayerState.Idle;

        if (input >= _ctx.Config.RunThreshold)
            return _ctx.IsSprinting ? EPlayerState.Sprint : EPlayerState.Run;

        return EPlayerState.Walk;
    }
}