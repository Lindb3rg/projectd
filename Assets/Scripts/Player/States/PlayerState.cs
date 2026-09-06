using UnityEngine;

// Root of every player state. Holds the context so Fall and Jump don't have to
// declare it separately, and absorbs the hooks most states never use.
public abstract class PlayerState : BaseState<PlayerStateMachine.EPlayerState>
{
    protected readonly PlayerContext _ctx;

    protected PlayerState(PlayerStateMachine.EPlayerState key, PlayerContext ctx) : base(key)
    {
        _ctx = ctx;
    }

    public override void UpdateState() { }
    public override void FixedUpdateState() { }
    public override void LateUpdateState() { }
    public override void EnterState() { }
    public override void ExitState() { }
}