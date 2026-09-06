using UnityEngine;
using UnityEngine.Events;

public class GravityContext
{
    // --- Config (set from inspector via GravityStateManager) ---
    public readonly float GravityStrength;
    public readonly float TransitionDuration;
    public readonly float FlipCooldown;
    public readonly GravityDirection InitialDirection;
    public readonly float TransitionReleasePoint;

    // --- Runtime state ---
    public GravityDirection CurrentDirection { get; set; }
    public GravityDirection TargetDirection { get; set; }
    public GravityDirection PreviousDirection { get; set; }
    public Vector2 GravityVector { get; set; }
    public bool IsBlocked { get; set; }
    public float CooldownTimer { get; set; }
    public float TransitionTimer { get; set; }

    // --- Events ---
    public UnityEvent<GravityDirection> OnGravityFlipStarted { get; } = new();
    public UnityEvent<GravityDirection> OnGravityFlipCompleted { get; } = new();
    public UnityEvent OnGravityBlocked { get; } = new();

    public GravityContext(
        float gravityStrength,
        float transitionDuration,
        float flipCooldown,
        GravityDirection initialDirection,
        float transitionReleasePoint)
    {
        GravityStrength = gravityStrength;
        TransitionDuration = transitionDuration;
        FlipCooldown = flipCooldown;
        InitialDirection = initialDirection;
        TransitionReleasePoint = transitionReleasePoint;
    }
}