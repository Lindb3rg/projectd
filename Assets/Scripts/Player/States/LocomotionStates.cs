using UnityEngine;

using EPlayerState = PlayerStateMachine.EPlayerState;

// Walk, Run and Sprint differ only in animation clip, speed, and which flag
// they set. Everything else comes from PlayerGroundedState.
// Split these into separate files if you prefer; they're grouped here to make
// the shape obvious.

public class PlayerWalkState : PlayerGroundedState
{
    public PlayerWalkState(EPlayerState key, PlayerContext ctx) : base(key, ctx) { }

    public override void EnterState()
    {
        _ctx.IsWalking = true;

        // Longer blend when standing up out of a crouch so the transition reads.
        float blend = _ctx.PreviousStateKey == EPlayerState.Crouch ? 0.5f : 0.01f;
        _ctx.Anim.CrossFadeInFixedTime("walk", blend);
    }

    public override void ExitState() => _ctx.IsWalking = false;

    public override void FixedUpdateState() => _ctx.ApplyGroundMovement(_ctx.Config.WalkSpeed);
    public override void OnTriggerEnter2D(Collider2D other) { }
    public override void OnTriggerStay2D(Collider2D other) { }
    public override void OnTriggerExit2D(Collider2D other) { }
}



public class PlayerRunState : PlayerGroundedState
{
    public PlayerRunState(EPlayerState key, PlayerContext ctx) : base(key, ctx) { }

    public override void EnterState()
    {
        _ctx.IsRunning = true;
        _ctx.Anim.CrossFadeInFixedTime("run", 0.2f);
    }

    public override void ExitState() => _ctx.IsRunning = false;

    public override void FixedUpdateState() => _ctx.ApplyGroundMovement(_ctx.Config.RunSpeed);

    public override void OnTriggerEnter2D(Collider2D other) { }
    public override void OnTriggerStay2D(Collider2D other) { }
    public override void OnTriggerExit2D(Collider2D other) { }
}



public class PlayerSprintState : PlayerGroundedState
{
    public PlayerSprintState(EPlayerState key, PlayerContext ctx) : base(key, ctx) { }

    public override void EnterState()
    {
        _ctx.Anim.CrossFadeInFixedTime("sprint", 0.2f);
    }

    public override void FixedUpdateState() => _ctx.ApplyGroundMovement(_ctx.Config.SprintSpeed);

    public override void OnTriggerEnter2D(Collider2D other) { }
    public override void OnTriggerStay2D(Collider2D other) { }
    public override void OnTriggerExit2D(Collider2D other) { }
}



public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(EPlayerState key, PlayerContext ctx) : base(key, ctx) { }

    public override void EnterState()
    {
        _ctx.Anim.CrossFadeInFixedTime("idle", 0.15f);
    }

    // Still call this rather than leaving velocity alone — it zeroes movement
    // along the move axis while preserving fall velocity.
    public override void FixedUpdateState() => _ctx.ApplyGroundMovement(0f);

    public override void OnTriggerEnter2D(Collider2D other) { }
    public override void OnTriggerStay2D(Collider2D other) { }
    public override void OnTriggerExit2D(Collider2D other) { }
}