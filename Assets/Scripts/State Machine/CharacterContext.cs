using System.Collections.Generic;
using UnityEngine;

// @CodeScene(disable:"Constructor Over-Injection")

public abstract class CharacterContext
{
    // --- Components ---
    public Rigidbody2D Rb { get; }
    public Animator Anim { get; }
    public Transform Transform { get; }
    public SpriteRenderer Sprite { get; }
    public Transform GroundCheck { get; }
    public Transform PivotCheck { get; }
    public Transform FrontCheck { get; }

    // --- Tuning ---
    public CharacterMovementConfig Config { get; }

    // --- Intent ---
    // Written by input for the player, by AI for everyone else.
    // Nothing in this class ever writes them.
    public Vector2 MoveInput { get; set; }
    public bool JumpPressed { get; set; }
    public bool JumpHeld { get; set; }
    public bool AttackPrimary { get; set; }
    public bool AttackSecondary { get; set; }

    // --- Sensing (written by CharacterStateMachine each frame) ---
    public bool IsGrounded { get; set; }
    public bool TouchesWallPositive { get; set; }
    public bool TouchesWallNegative { get; set; }
    public bool EdgeDetected { get; set; }
    public Vector2 EdgePosition { get; set; }

    // Derived, so it can never disagree with the two bools above.
    public bool IsPushingIntoWall =>
        (MoveInput.x > 0f && TouchesWallPositive) ||
        (MoveInput.x < 0f && TouchesWallNegative);

    // --- Runtime state ---
    public int FacingDirection { get; private set; } = 1;
    public float TurnTimer { get; set; }
    public float CoyoteTimeCounter { get; set; }
    public float JumpLockTimer { get; set; }
    public bool DidDoubleJump { get; set; }
    public float LastAirborneSpeed { get; set; }

    public bool IsWalking { get; set; }
    public bool IsRunning { get; set; }
    public bool IsSprinting { get; set; }
    public bool IsCrouching { get; set; }

    // --- Gravity ---
    public GravityDirection CurrentGravityDirection { get; set; } = GravityDirection.Down;
    public Vector2 GravityDown { get; private set; } = Vector2.down;
    public Vector2 GravityUp { get; private set; } = Vector2.up;
    public Vector2 MoveAxis { get; private set; } = Vector2.right;

    protected CharacterContext(
        Rigidbody2D rb,
        Animator anim,
        Transform transform,
        SpriteRenderer sprite,
        Transform groundCheck,
        Transform pivotCheck,
        Transform frontCheck,
        CharacterMovementConfig config)
    {
        Rb = rb;
        Anim = anim;
        Transform = transform;
        Sprite = sprite;
        GroundCheck = groundCheck;
        PivotCheck = pivotCheck;
        FrontCheck = frontCheck;
        Config = config;

        SyncGravityAxes();
    }

    // ------------------------------------------------------------------
    // Gravity basis
    // ------------------------------------------------------------------

    // Call whenever CurrentGravityDirection changes.
    // MoveAxis is DERIVED from GravityUp rather than looked up, so the four
    // orientations can't drift out of sync with each other.
    public void SyncGravityAxes()
    {
        GravityDown = CurrentGravityDirection switch
        {
            GravityDirection.Up    => Vector2.up,
            GravityDirection.Left  => Vector2.left,
            GravityDirection.Right => Vector2.right,
            _                      => Vector2.down,
        };

        GravityUp = -GravityDown;

        // GravityUp rotated -90 degrees: the character's local "right".
        MoveAxis = new Vector2(GravityUp.y, -GravityUp.x);
    }

    // ------------------------------------------------------------------
    // Velocity decomposition
    // ------------------------------------------------------------------

    public float GetGravityVelocity()   => Vector2.Dot(Rb.linearVelocity, GravityDown);
    public float GetAntiGravVelocity()  => Vector2.Dot(Rb.linearVelocity, GravityUp);
    public float GetMoveAxisVelocity()  => Vector2.Dot(Rb.linearVelocity, MoveAxis);

    public Vector2 BuildVelocity(float moveVelocity, float antiGravVelocity)
        => MoveAxis * moveVelocity + GravityUp * antiGravVelocity;

    // ------------------------------------------------------------------
    // Movement
    // ------------------------------------------------------------------

    // Grounded movement. Direction comes from intent, never from the transform,
    // so this works identically for input-driven and AI-driven characters.
    public void ApplyGroundMovement(float speed)
    {
        if (IsPushingIntoWall)
        {
            ZeroMoveVelocity();
            return;
        }

        float dir = Mathf.Abs(MoveInput.x) < 0.1f ? 0f : Mathf.Sign(MoveInput.x);
        Rb.linearVelocity = BuildVelocity(dir * speed, GetAntiGravVelocity());
    }

    // Airborne movement. Accelerates toward the target rather than snapping,
    // and preserves momentum when there is no input.
    public void ApplyAirMovement(float targetSpeed)
    {
        if (IsPushingIntoWall)
        {
            ZeroMoveVelocity();
            return;
        }

        float current = GetMoveAxisVelocity();

        if (Mathf.Abs(MoveInput.x) < 0.1f)
            return; // coast

        float target = Mathf.Sign(MoveInput.x) * targetSpeed;
        float accel  = Config.AirAcceleration * Config.AirAccelerationMultiplier;
        float newSpeed = Mathf.MoveTowards(current, target, accel * Time.fixedDeltaTime);

        Rb.linearVelocity = BuildVelocity(newSpeed, GetAntiGravVelocity());
    }

    // Zeroes movement along the move axis while leaving fall/rise untouched.
    public void ZeroMoveVelocity()
    {
        Rb.linearVelocity = BuildVelocity(0f, GetAntiGravVelocity());
    }

    // Replaces the anti-gravity component outright so jump height is
    // consistent regardless of what the character was doing before.
    public void Jump(float force)
    {
        Rb.linearVelocity = BuildVelocity(GetMoveAxisVelocity(), force);
        CoyoteTimeCounter = 0f;
        JumpLockTimer     = Config.JumpLockDuration;
    }

    // Snappier arcs: heavier on the way down, and cut the rise short when the
    // jump button is released early.
    public void ApplyJumpArcModifiers()
    {
        float vertical = GetAntiGravVelocity();
        float extra    = 0f;

        if (vertical < 0f)
            extra = Config.FallMultiplier - 1f;
        else if (vertical > 0f && !JumpHeld)
            extra = Config.LowJumpMultiplier - 1f;

        if (extra <= 0f) return;

        float g = Physics2D.gravity.magnitude;
        Rb.linearVelocity += GravityDown * (extra * g * Time.fixedDeltaTime);
    }

    // ------------------------------------------------------------------
    // Facing
    // ------------------------------------------------------------------

    // Facing is a sprite flip only. Transform rotation belongs to gravity.
    public void SetFacing(int direction)
    {
        if (direction == 0 || direction == FacingDirection) return;

        FacingDirection = direction;

        if (Sprite != null)
            Sprite.flipX = FacingDirection == -1;

        if (FrontCheck != null)
        {
            Vector3 p = FrontCheck.localPosition;
            p.x = Mathf.Abs(p.x) * FacingDirection;
            FrontCheck.localPosition = p;
        }
    }

    // ------------------------------------------------------------------
    // Timers
    // ------------------------------------------------------------------

    // Call once per frame from the state machine, after sensing.
    public void TickTimers(float deltaTime)
    {
        CoyoteTimeCounter = IsGrounded
            ? Config.CoyoteTime
            : CoyoteTimeCounter - deltaTime;

        if (JumpLockTimer > 0f)
            JumpLockTimer -= deltaTime;

        if (IsGrounded)
            DidDoubleJump = false;
    }

    // ------------------------------------------------------------------
    // Queries
    // ------------------------------------------------------------------

    public bool IsMoving => Mathf.Abs(MoveInput.x) > 0.1f;

    public bool CanCoyoteJump => CoyoteTimeCounter > 0f;

    public float GetMovementSpeed(float moveInputX)
    {
        float absX = Mathf.Abs(moveInputX);

        if (absX < 0.1f) return 0f;
        if (absX < 0.6f) return 0.3f;
        return 0.8f;
    }
}