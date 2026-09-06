using UnityEngine;

using EPlayerState = PlayerStateMachine.EPlayerState;

public class PlayerFallState : PlayerAirborneState
{
    public PlayerFallState(EPlayerState key, PlayerContext ctx) : base(key, ctx) { }

    public override void EnterState()
    {
        // Coming off the apex of a jump already looks like falling, so blend
        // longer. Walking off a ledge should snap.
        float blend = _ctx.PreviousStateKey == EPlayerState.Jump ? 0.3f : 0.1f;

        _ctx.Anim.CrossFadeInFixedTime("fall", blend);
    }

    // FixedUpdateState is inherited from PlayerAirborneState:
    // ApplyAirMovement handles the horizontal nudge and the decay toward zero,
    // ApplyJumpArcModifiers handles the extra fall weight.

    public override EPlayerState GetNextState()
    {
        // Re-enable once PlayerEdgeClimbState is registered in BuildStates.
        // if (_ctx.EdgeDetected && (_ctx.TouchesWallPositive || _ctx.TouchesWallNegative))
        //     return EPlayerState.EdgeClimb;

        return GetSharedAirborneState();
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {

    }

    public override void OnTriggerExit2D(Collider2D other)
    {

    }

    public override void OnTriggerStay2D(Collider2D other)
    {

    }
}