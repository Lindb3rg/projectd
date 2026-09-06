using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : CharacterStateMachine<PlayerStateMachine.EPlayerState>
{
    public enum EPlayerState
    {
        Idle,
        Walk,
        Run,
        Sprint,
        Crouch,
        Jump,
        Fall,
        Land,
        EdgeClimb,
        GravityTransition,
    }

    [Header("Player Config")]
    public PlayerAbilityConfig AbilityConfig;

    [Header("Player References")]
    public Transform HeadCheck;    // ceiling clearance for standing up from crouch
    public Transform ChestTarget;  // aim target, may be null in a pure sprite setup

    [Header("Audio")]
    public AudioClip[] WalkFootstepClips;
    public AudioClip[] RunFootstepClips;

    private AudioSource _audioSource;
    private InputSystem_Actions _input;

    private PlayerContext _context;
    public PlayerContext Context => _context;

    protected override CharacterContext CharacterCtx => _context;

    public PlayerFallState Fall => (PlayerFallState)States[EPlayerState.Fall];

    protected override void Awake()
    {
        base.Awake();

        _input = new InputSystem_Actions();

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (AbilityConfig == null)
            Debug.LogError($"{name}: AbilityConfig is not assigned.", this);

        _context = new PlayerContext(
            Rb, Anim, transform, Sprite,
            GroundCheck, PivotCheck, FrontCheck, HeadCheck, ChestTarget,
            MovementConfig, AbilityConfig
        );

        BuildStates();

        CurrentState = States[EPlayerState.Idle];
    }

    private void BuildStates()
    {
        States[EPlayerState.Idle] = new PlayerIdleState(EPlayerState.Idle, _context);
        States[EPlayerState.Walk] = new PlayerWalkState(EPlayerState.Walk, _context);
        States[EPlayerState.Run] = new PlayerRunState(EPlayerState.Run, _context);
        States[EPlayerState.Sprint] = new PlayerSprintState(EPlayerState.Sprint, _context);
        States[EPlayerState.Jump] = new PlayerJumpState(EPlayerState.Jump, _context);
        States[EPlayerState.Fall] = new PlayerFallState(EPlayerState.Fall, _context);
        States[EPlayerState.Land] = new PlayerLandState(EPlayerState.Land, _context);
        // States[EPlayerState.EdgeClimb]     = new PlayerEdgeClimbState(EPlayerState.EdgeClimb, _context);
        States[EPlayerState.GravityTransition] = new PlayerGravityTransitionState(EPlayerState.GravityTransition, _context);
    }

    private void OnEnable() => _input.Player.Enable();
    private void OnDisable() => _input.Player.Disable();

    // ------------------------------------------------------------------
    // Intent — the only place in the whole system that knows a gamepad exists
    // ------------------------------------------------------------------

    protected override void ReadIntent()
    {
        _context.MoveInput = _input.Player.Move.ReadValue<Vector2>();
        _context.AimInput = _input.Player.Look.ReadValue<Vector2>();

        _context.JumpPressed = _input.Player.Jump.WasPressedThisFrame();
        _context.JumpHeld = _input.Player.Jump.IsPressed();
        _context.SprintHeld = _input.Player.Sprint.IsPressed();
        _context.CrouchHeld = _input.Player.Crouch.IsPressed();
        _context.AimHeld = _input.Player.Aim.IsPressed();

        _context.EquipPressed = _input.Player.Equip.WasPressedThisFrame();
        _context.InteractPressed = _input.Player.Interact.WasPressedThisFrame();
        _context.AttackPrimary = _input.Player.AttackPrimary.WasPressedThisFrame();

        _context.GravityFlipPressed = _input.Player.GravityFlip.WasPressedThisFrame();
        _context.GravityFlipDirectionInput = _input.Player.GravityFlipDirection.ReadValue<Vector2>();
    }

    protected override void OnStateChanged(EPlayerState previousKey, EPlayerState newKey)
    {
        _context.PreviousStateKey = previousKey;
    }

    // ------------------------------------------------------------------
    // Per-frame player behaviour, after sensing and syncing
    // ------------------------------------------------------------------

    protected override void HandleCharacterUpdate()
    {
        _context.HandleTurning(Time.deltaTime);
        _context.HandleCrouchToggle();
        _context.HandleSprintToggle();
        _context.HandleAim(Time.deltaTime);

        HandleInteractInput();
        HandleGravityFlipInput();
    }

    private void HandleInteractInput()
    {
        if (!_context.InteractPressed) return;

        Log("Interact pressed");
        // Weapon pickup goes here once the inventory is ported.
    }

    // ------------------------------------------------------------------
    // Gravity
    // ------------------------------------------------------------------

    private void HandleGravityFlipInput()
    {
        if (!_context.GravityFlipPressed) return;
        if (GravityStateMachine.Instance == null) return;

        GravityDirection target = GetGravityDirectionFromInput(_context.GravityFlipDirectionInput);
        if (target == GravityDirection.None) return;

        GravityStateMachine.Instance.RequestGravityFlip(target);
    }

    private GravityDirection GetGravityDirectionFromInput(Vector2 input)
    {
        if (Mathf.Abs(input.x) < 0.1f && Mathf.Abs(input.y) < 0.1f)
            return GravityDirection.None;

        if (Mathf.Abs(input.y) >= Mathf.Abs(input.x))
            return input.y > 0f ? GravityDirection.Up : GravityDirection.Down;

        return input.x > 0f ? GravityDirection.Right : GravityDirection.Left;
    }

    public void OnGravityFlipStarted(GravityDirection newDirection)
    {
        _context.CurrentGravityDirection = newDirection;
        _context.SyncGravityAxes();
        TransitionToState(EPlayerState.GravityTransition);
    }

    public void OnGravityFlipCompleted(GravityDirection newDirection)
    {
        _context.CurrentGravityDirection = newDirection;
        _context.SyncGravityAxes();
    }

    // ------------------------------------------------------------------
    // Animation events
    // ------------------------------------------------------------------

    public void FootstepSound()
    {
        AudioClip[] clips = _context.IsRunning ? RunFootstepClips : WalkFootstepClips;
        if (clips == null || clips.Length == 0) return;

        _audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}