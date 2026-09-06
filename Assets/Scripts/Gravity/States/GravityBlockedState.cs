using UnityEngine;

public class GravityBlockedState : BaseGravityState
{
    public GravityBlockedState(GravityState key, GravityContext context) : base(key, context) { }

    public override void EnterState() { }

    public override void UpdateState() { }

    public override void ExitState() { }

    public override GravityState GetNextState() => StateKey;

    public override void OnTriggerEnter2D(Collider2D other)
    {
        throw new System.NotImplementedException();
    }

    public override void OnTriggerStay2D(Collider2D other)
    {
        throw new System.NotImplementedException();
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        throw new System.NotImplementedException();
    }
}