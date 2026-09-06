using UnityEngine;

using EPlayerState = PlayerStateMachine.EPlayerState;

// Base for every state where the character is off the ground.
// Air control and jump-arc weighting are identical whether rising or falling,
// so they live here rather than being duplicated in Jump and Fall.
public abstract class PlayerAirborneState : PlayerState
{
    protected PlayerAirborneState(EPlayerState key, PlayerContext ctx) : base(key, ctx) { }

    public override void FixedUpdateState()
    {
        _ctx.LastAirborneSpeed = Mathf.Max(0f, -_ctx.GetAntiGravVelocity());

        _ctx.ApplyAirMovement(_ctx.Config.JumpHorizontalSpeed);
        _ctx.ApplyJumpArcModifiers();
    }

    // True once the character is moving with gravity rather than against it.
    protected bool IsDescending => _ctx.GetAntiGravVelocity() < 0f;

    // JumpLockTimer stops the ground check from cancelling a jump on the frame
    // it leaves the floor, while the feet are still inside the cast radius.
    protected bool CanLand => _ctx.IsGrounded && _ctx.JumpLockTimer <= 0f;

    protected bool CanDoubleJump => _ctx.JumpPressed && !_ctx.DidDoubleJump && !_ctx.CanCoyoteJump;

    // Shared airborne exits. Jump and Fall check this first, then add their own.
    // Landing wins over double jumping: if the feet are already down, the press
    // should become a fresh ground jump next frame, not a wasted air jump.
    protected EPlayerState GetSharedAirborneState()
    {
        if (CanLand)
            return EPlayerState.Land;

        if (CanDoubleJump)
            return EPlayerState.Jump;

        return StateKey;
    }
}