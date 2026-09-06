using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public abstract class CharacterStateMachine<EState> : StateManager<EState> where EState : Enum
{
    protected static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");

    [Header("Config")]
    public CharacterMovementConfig MovementConfig;

    [Header("Ground Check")]
    public Transform GroundCheck;
    public LayerMask GroundLayer;
    public float GroundCheckRadius = 0.2f;
    public float GroundCheckDistance = 0.5f;

    [Header("Pivot Check")]
    public Transform PivotCheck;

    [Header("Wall Check")]
    public Vector2 WallCheckSize = new(0.6f, 1.6f);
    public float WallCheckSideOffset = 0.35f;
    public Vector2 WallCheckOffset = Vector2.zero;

    [Header("Front Check")]
    public Transform FrontCheck;
    public Vector2 FrontCheckSize = new(0.1f, 1.5f);
    public LayerMask EdgeLayer;

    [Header("Debug")]
    public bool UseLogs = true;

    // --- Sensing results ---
    public bool IsGrounded { get; private set; }
    public bool TouchesWallPositive { get; private set; }
    public bool TouchesWallNegative { get; private set; }
    public bool EdgeDetected { get; private set; }
    public Vector2 EdgePosition { get; private set; }

    // --- Components ---
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public SpriteRenderer Sprite { get; private set; }

    // Derived classes expose their own context here so this base can sync it
    // without knowing whether it's a player, an enemy, or an NPC.
    protected abstract CharacterContext CharacterCtx { get; }

    protected override void Awake()
    {
        base.Awake();

        Rb     = GetComponent<Rigidbody2D>();
        Anim   = GetComponent<Animator>();
        Sprite = GetComponentInChildren<SpriteRenderer>();

        if (MovementConfig == null)
            Debug.LogError($"{name}: MovementConfig is not assigned.", this);
    }

    // Fixed order, and the order matters:
    //   intent first, then sense the world, then publish both to the context,
    //   then let the states decide. Sensing before syncing is what stops the
    //   states from running on last frame's ground check.
    protected override void Update()
    {
        ReadIntent();
        UpdateChecks();
        SyncCharacterContext();
        HandleCharacterUpdate();

        base.Update();
    }

    // Overridden by the player to read the gamepad, by enemies to run their AI.
    protected virtual void ReadIntent() { }

    // Per-frame work that isn't intent and isn't sensing (turning, toggles, aim).
    protected virtual void HandleCharacterUpdate() { }

    // ------------------------------------------------------------------
    // Sensing
    // ------------------------------------------------------------------

    protected void UpdateChecks()
    {
        UpdateGroundCheck();
        UpdateFrontCheckForEdge();
        UpdateWallCheck();
    }

    // Read orientation from the context so there is one source of truth.
    // Falls back to world-down before the context exists (edit mode gizmos).
    private Vector2 GravityDown => CharacterCtx?.GravityDown ?? Vector2.down;
    private Vector2 MoveAxis    => CharacterCtx?.MoveAxis    ?? Vector2.right;

    private void UpdateGroundCheck()
    {
        if (GroundCheck == null)
        {
            IsGrounded = false;
            return;
        }

        RaycastHit2D hit = Physics2D.CircleCast(
            GroundCheck.position,
            GroundCheckRadius,
            GravityDown,
            GroundCheckDistance,
            GroundLayer
        );

        IsGrounded = hit.collider != null;
    }

    private void UpdateFrontCheckForEdge()
    {
        EdgeDetected = false;

        if (FrontCheck == null) return;

        // Physics2D.OverlapBoxAll takes the FULL size, unlike the 3D
        // Physics.OverlapBox which takes half-extents. Do not halve this.
        Collider2D[] frontHits = Physics2D.OverlapBoxAll(
            FrontCheck.position,
            FrontCheckSize,
            FrontCheck.eulerAngles.z,
            EdgeLayer
        );

        foreach (Collider2D col in frontHits)
        {
            EdgeDetected = true;
            EdgePosition = new Vector2(transform.position.x, col.bounds.max.y);
        }
    }

    private void UpdateWallCheck()
    {
        Vector2 baseCenter = (Vector2)transform.position
            + (Vector2)transform.TransformDirection(WallCheckOffset);

        Vector2 posCenter = baseCenter + MoveAxis * WallCheckSideOffset;
        Vector2 negCenter = baseCenter - MoveAxis * WallCheckSideOffset;

        float angle = transform.eulerAngles.z;

        TouchesWallPositive = Physics2D.OverlapBox(posCenter, WallCheckSize, angle, GroundLayer) != null;
        TouchesWallNegative = Physics2D.OverlapBox(negCenter, WallCheckSize, angle, GroundLayer) != null;
    }

    // ------------------------------------------------------------------
    // Context sync
    // ------------------------------------------------------------------

    protected virtual void SyncCharacterContext()
    {
        CharacterContext ctx = CharacterCtx;
        if (ctx == null) return;

        ctx.SyncGravityAxes();

        ctx.IsGrounded          = IsGrounded;
        ctx.TouchesWallPositive = TouchesWallPositive;
        ctx.TouchesWallNegative = TouchesWallNegative;
        ctx.EdgeDetected        = EdgeDetected;
        ctx.EdgePosition        = EdgePosition;

        ctx.TickTimers(Time.deltaTime);

        Anim.SetBool(IsGroundedHash, IsGrounded);
    }

    // ------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        if (GroundCheck != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(GroundCheck.position, GroundCheckRadius);
            Gizmos.DrawRay(GroundCheck.position, GravityDown * GroundCheckDistance);
        }

        if (PivotCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(PivotCheck.position, 0.1f);
        }

        if (FrontCheck != null)
        {
            Gizmos.color = EdgeDetected ? Color.green : Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(
                FrontCheck.position,
                Quaternion.Euler(0f, 0f, FrontCheck.eulerAngles.z),
                Vector3.one
            );
            Gizmos.DrawWireCube(Vector3.zero, FrontCheckSize);
            Gizmos.matrix = Matrix4x4.identity;
        }

        Vector2 baseCenter = (Vector2)transform.position
            + (Vector2)transform.TransformDirection(WallCheckOffset);

        Quaternion gizmoRotation = Quaternion.Euler(0f, 0f, transform.eulerAngles.z);

        Gizmos.color  = TouchesWallPositive ? Color.red : Color.green;
        Gizmos.matrix = Matrix4x4.TRS(baseCenter + MoveAxis * WallCheckSideOffset, gizmoRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, WallCheckSize);

        Gizmos.color  = TouchesWallNegative ? Color.red : Color.green;
        Gizmos.matrix = Matrix4x4.TRS(baseCenter - MoveAxis * WallCheckSideOffset, gizmoRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, WallCheckSize);

        Gizmos.matrix = Matrix4x4.identity;
    }

    public void Log(string message)
    {
        if (UseLogs) Debug.Log(message, this);
    }
}