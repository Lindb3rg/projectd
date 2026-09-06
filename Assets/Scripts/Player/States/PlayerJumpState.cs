using UnityEngine;

using EPlayerState = PlayerStateMachine.EPlayerState;

public class PlayerJumpState : PlayerAirborneState
{
    public PlayerJumpState(EPlayerState key, PlayerContext ctx) : base(key, ctx) { }

    public override void EnterState()
    {
        // Re-entering Jump while already airborne means this is the second jump.
        bool cameFromAir = _ctx.PreviousStateKey == EPlayerState.Jump
                || _ctx.PreviousStateKey == EPlayerState.Fall;

        bool isDoubleJump = cameFromAir && !_ctx.CanCoyoteJump;



        float force;

        if (isDoubleJump)
        {
            force = _ctx.Config.DoubleJumpForce;
            _ctx.DidDoubleJump = true;
        }
        else
        {
            force = _ctx.IsSprinting
                ? _ctx.Config.JumpForce * _ctx.Config.SprintJumpMultiplier
                : _ctx.Config.JumpForce;
        }

        // Jump() zeroes coyote time, arms JumpLockTimer, and replaces the
        // anti-gravity component outright so height is consistent regardless
        // of what the character was doing beforehand.
        _ctx.Jump(force);

        _ctx.Anim.CrossFadeInFixedTime(isDoubleJump ? "doubleJump" : "jump", 0.1f);
    }

    // FixedUpdateState is inherited: air control plus arc weighting.
    // The old short-hop damping and the manual velocity nudge both did by hand
    // what ApplyJumpArcModifiers and ApplyAirMovement now do.

    public override EPlayerState GetNextState()
    {
        var shared = GetSharedAirborneState();
        if (!shared.Equals(StateKey)) return shared;

        // Past the apex, or stopped dead against a ceiling.
        if (_ctx.GetAntiGravVelocity() <= 0f)
            return EPlayerState.Fall;

        return StateKey;
    }
    public override void OnTriggerEnter2D(Collider2D other) { }
    public override void OnTriggerStay2D(Collider2D other) { }
    public override void OnTriggerExit2D(Collider2D other) { }


}