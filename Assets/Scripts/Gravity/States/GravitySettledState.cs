using UnityEngine;

public class GravitySettledState : BaseGravityState
{
    public GravitySettledState(GravityState key, GravityContext context) : base(key, context) { }

    public override void EnterState()
    {
        // Gravity already applied by GravityTransitioningState
        // Just fire the completed event
        Context.OnGravityFlipCompleted.Invoke(Context.CurrentDirection);
    }

    public override void UpdateState()
    {
        TickCooldown();
    }

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