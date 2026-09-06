using UnityEngine;

public class PlayerLandState : BaseState<PlayerStateMachine.EPlayerState>
{
    private readonly PlayerContext _ctx;
    private bool _didDoubleJump;

    public PlayerLandState(PlayerStateMachine.EPlayerState key, PlayerContext ctx) : base(key)
    {
        _ctx = ctx;
    }

    public override void EnterState()
    {
        if (_ctx.DidDoubleJump)
            Debug.Log("Double Jumped!");;
        _ctx.Anim.CrossFadeInFixedTime("land",1f);
        Debug.Log("We entered Land state");
    }

    public override void ExitState() { }
    public override void UpdateState() { }
    public override void FixedUpdateState() { }
    public override void LateUpdateState() { }

    public override PlayerStateMachine.EPlayerState GetNextState()
    {
        if (!_didDoubleJump)
            return PlayerStateMachine.EPlayerState.Idle;

        if (_ctx.Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            return _ctx.MoveInput.x != 0
                ? PlayerStateMachine.EPlayerState.Walk
                : PlayerStateMachine.EPlayerState.Idle;

        return StateKey;
    }

    public override void OnTriggerEnter2D(Collider2D other) { }
    public override void OnTriggerStay2D(Collider2D other)  { }
    public override void OnTriggerExit2D(Collider2D other)  { }
}