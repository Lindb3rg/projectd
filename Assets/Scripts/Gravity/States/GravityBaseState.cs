using UnityEngine;

public abstract class BaseGravityState : BaseState<GravityState>
{
    protected GravityContext Context;

    public BaseGravityState(GravityState key, GravityContext context) : base(key)
    {
        Context = context;
    }

    // --- Shared helpers available to all gravity states ---

    protected bool IsCooldownComplete()
    {
        return Context.CooldownTimer <= 0f;
    }

    protected bool IsTransitionComplete()
    {
        return Context.TransitionTimer <= 0f;
    }

    protected Vector2 DirectionToVector(GravityDirection direction)
    {
        return direction switch
        {
            GravityDirection.Up    => Vector2.up    * Context.GravityStrength,
            GravityDirection.Left  => Vector2.left  * Context.GravityStrength,
            GravityDirection.Right => Vector2.right * Context.GravityStrength,
            GravityDirection.None  => Vector2.zero,
            _                      => Vector2.down  * Context.GravityStrength,
        };
    }

    protected void ApplyGravity(GravityDirection direction)
    {
        Context.GravityVector = DirectionToVector(direction);
        Context.CurrentDirection = direction;
        Physics.gravity = Context.GravityVector;
    }

    protected void TickCooldown()
    {
        if (Context.CooldownTimer > 0f)
            Context.CooldownTimer -= Time.deltaTime;
    }

    protected void TickTransition()
    {
        if (Context.TransitionTimer > 0f)
            Context.TransitionTimer -= Time.deltaTime;
    }

    // --- Default empty implementations for unused callbacks ---

    public override void FixedUpdateState() { }
    public override void LateUpdateState() { }
    
}