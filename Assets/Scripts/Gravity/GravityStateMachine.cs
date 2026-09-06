using UnityEngine;

public class GravityStateMachine : StateManager<GravityState>
{
    public static GravityStateMachine Instance { get; private set; }

    [Header("Gravity Settings")]
    [SerializeField] private float gravityStrength = 9.81f;
    [SerializeField] GravityDirection initialDirection = GravityDirection.Down;


    [Header("Timing")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float flipCooldown = 1f;
    [SerializeField][Range(0f, 1f)] private float transitionReleasePoint = 0.5f;

    [Header("Transition Phases")]
    public float FloatPeakHeight = 2f;
    public float FloatRiseDuration = 0.4f;
    public float HoverDuration = 0.3f;
    public float RotateDuration = 0.5f;
    public AnimationCurve RotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Zero Gravity Window")]
    public bool EnableZeroGravityWindow = true;
    public float ZeroGravityDuration = 0.4f;

    [Header("Debug")]
    [SerializeField] private GravityDirection debugDirection;
    GravityDirection _lastDebugDirection;

    public GravityContext Context { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Context = new GravityContext(
            gravityStrength,
            transitionDuration,
            flipCooldown,
            initialDirection,
            transitionReleasePoint
        );

        Context.PreviousDirection = initialDirection;
        Context.CurrentDirection = initialDirection;
        Context.TargetDirection = initialDirection;

        InitialiseStates();
    }

    protected override void Start()
    {
        debugDirection = initialDirection;
        _lastDebugDirection = initialDirection;

        // Apply initial gravity before any state runs
        Physics2D.gravity = Context.GravityVector = initialDirection switch
        {
            GravityDirection.Up => Vector2.up * gravityStrength,
            GravityDirection.Left => Vector2.left * gravityStrength,
            GravityDirection.Right => Vector2.right * gravityStrength,
            GravityDirection.None => Vector2.zero,
            _ => Vector2.down * gravityStrength,
        };

        base.Start();
    }

    protected override void Update()
    {
        // Debug direction switching from inspector at runtime
        if (debugDirection != _lastDebugDirection)
        {
            _lastDebugDirection = debugDirection;
            RequestGravityFlip(debugDirection);
        }

        base.Update();
    }

    private void InitialiseStates()
    {
        var settledState = new GravitySettledState(GravityState.Settled, Context);
        var transitioningState = new GravityTransitioningState(GravityState.Transitioning, Context);
        var blockedState = new GravityBlockedState(GravityState.Blocked, Context);

        States.Add(GravityState.Settled, settledState);
        States.Add(GravityState.Transitioning, transitioningState);
        States.Add(GravityState.Blocked, blockedState);

        Context.TargetDirection = initialDirection;
        CurrentState = States[GravityState.Settled];
    }


    // --- Public API ---

    public Vector2 GetMoveAxis()
    {
        return GetCurrentDirection() switch
        {
            GravityDirection.Right => Vector2.up,
            GravityDirection.Left => Vector2.down,
            _ => Vector2.right,
        };
    }

    public void RequestGravityFlip(GravityDirection targetDirection)
    {
        if (Context.IsBlocked)
        {
            Context.OnGravityBlocked.Invoke();
            return;
        }

        if (Context.CooldownTimer > 0f) return;

        if (targetDirection == Context.CurrentDirection) return;

        Context.PreviousDirection = Context.CurrentDirection;
        Context.TargetDirection = targetDirection;
        TransitionToState(GravityState.Transitioning);
    }

    // Convenience overload — flips to the opposite of current direction
    public void RequestGravityFlip()
    {
        GravityDirection opposite = Context.CurrentDirection switch
        {
            GravityDirection.Down => GravityDirection.Up,
            GravityDirection.Up => GravityDirection.Down,
            GravityDirection.Left => GravityDirection.Right,
            GravityDirection.Right => GravityDirection.Left,
            _ => GravityDirection.Down
        };

        RequestGravityFlip(opposite);
    }

    public void RequestGravityBlock()
    {
        Context.IsBlocked = true;
        TransitionToState(GravityState.Blocked);
    }

    public void ReleaseGravityBlock()
    {
        Context.IsBlocked = false;
        TransitionToState(GravityState.Settled);
    }

    // --- Accessors for listeners ---

    public GravityDirection GetCurrentDirection() => Context.CurrentDirection;


    public Vector2 GetGravityVector() => Context.GravityVector;
    public bool IsTransitioning() => CurrentState.StateKey == GravityState.Transitioning;
    public bool IsBlocked() => Context.IsBlocked;
}