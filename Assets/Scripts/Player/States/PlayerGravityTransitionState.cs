using UnityEngine;

public class PlayerGravityTransitionState : BaseState<PlayerStateMachine.EPlayerState>
{
    private readonly PlayerContext _ctx;

    // Phase tracking
    private enum TransitionPhase { FloatUp, Hover, Rotate, Done }
    private TransitionPhase _phase;
    private float _phaseTimer;

    // Float phase
    private Vector2 _floatStartPosition;
    private Vector2 _floatPeakPosition;

    // Rotate phase
    private float _startAngle;
    private float _targetAngle;
    private float _rotateTimer;
    private Vector2 _pivotWorld;        // fixed world point to rotate around
    private Vector2 _rootOffsetFromPivot;

    // Preserved depth — Vector2 assignment would flatten this to 0
    private float _z;

    // Safety timeout
    private float _timeoutTimer;
    private const float MaxTransitionTime = 5f;

    public PlayerGravityTransitionState(PlayerStateMachine.EPlayerState key, PlayerContext ctx) : base(key)
    {
        _ctx = ctx;
    }

    public override void EnterState()
    {
        _phase        = TransitionPhase.FloatUp;
        _phaseTimer   = 0f;
        _timeoutTimer = 0f;

        GravityContext gravCtx = GravityStateMachine.Instance.Context;
        GravityStateMachine gm = GravityStateMachine.Instance;

        _z = _ctx.Transform.position.z;

        // Zero velocity first, then go kinematic, so input momentum
        // does not carry into the transition.
        _ctx.Rb.linearVelocity      = Vector2.zero;
        _ctx.Rb.angularVelocity     = 0f;
        _ctx.Rb.bodyType            = RigidbodyType2D.Kinematic;

        // Float away from the surface we are leaving, along the OLD gravity axis.
        // antiGravDir is axis-aligned, so this only moves one axis by construction.
        Vector2 antiGravDir  = -GravityDirectionToVector(gravCtx.PreviousDirection);
        _floatStartPosition  = _ctx.Transform.position;
        _floatPeakPosition   = _floatStartPosition + antiGravDir * gm.FloatPeakHeight;

        _ctx.Anim.SetTrigger("gravityTransition");
    }

    public override void UpdateState()
    {
        _timeoutTimer += Time.deltaTime;
    }

    // All movement runs in FixedUpdate. The body is kinematic, so it must be
    // driven through MovePosition/MoveRotation rather than Transform writes —
    // otherwise the collider teleports and interpolation jitters.
    public override void FixedUpdateState()
    {
        _phaseTimer += Time.fixedDeltaTime;

        GravityStateMachine gm = GravityStateMachine.Instance;

        switch (_phase)
        {
            case TransitionPhase.FloatUp: UpdateFloatUp(gm); break;
            case TransitionPhase.Hover:   UpdateHover(gm);   break;
            case TransitionPhase.Rotate:  UpdateRotate(gm);  break;
        }
    }

    private void UpdateFloatUp(GravityStateMachine gm)
    {
        float t        = Mathf.Clamp01(_phaseTimer / gm.FloatRiseDuration);
        float smoothT  = Mathf.SmoothStep(0f, 1f, t);
        Vector2 newPos = Vector2.Lerp(_floatStartPosition, _floatPeakPosition, smoothT);

        _ctx.Rb.MovePosition(newPos);

        if (_phaseTimer >= gm.FloatRiseDuration)
        {
            _ctx.Rb.MovePosition(_floatPeakPosition);
            _phase      = TransitionPhase.Hover;
            _phaseTimer = 0f;
        }
    }

    private void UpdateHover(GravityStateMachine gm)
    {
        if (_phaseTimer < gm.HoverDuration)
            return;

        GravityContext gravCtx = GravityStateMachine.Instance.Context;

        _startAngle = _ctx.Rb.rotation;
        _targetAngle = _startAngle + ShortestSignedDelta(
            _startAngle,
            GetSnappedAngle(gravCtx.TargetDirection),
            gravCtx.PreviousDirection,
            gravCtx.TargetDirection
        );

        // Capture the pivot as a fixed world point. The pivot is a child, so
        // reading it live while moving the root would chase its own tail.
        _pivotWorld          = _ctx.PivotCheck != null ? (Vector2)_ctx.PivotCheck.position : (Vector2)_ctx.Transform.position;
        _rootOffsetFromPivot = (Vector2)_ctx.Transform.position - _pivotWorld;

        _phase       = TransitionPhase.Rotate;
        _phaseTimer  = 0f;
        _rotateTimer = 0f;
    }

    private void UpdateRotate(GravityStateMachine gm)
    {
        _rotateTimer += Time.fixedDeltaTime;
        float t       = Mathf.Clamp01(_rotateTimer / gm.RotateDuration);
        float curvedT = gm.RotationCurve.Evaluate(t);

        float angle = Mathf.Lerp(_startAngle, _targetAngle, curvedT);

        // Rotate the root around the fixed pivot point.
        Quaternion delta = Quaternion.Euler(0f, 0f, angle - _startAngle);
        Vector2 newPos   = _pivotWorld + (Vector2)(delta * _rootOffsetFromPivot);

        _ctx.Rb.MovePosition(newPos);
        _ctx.Rb.MoveRotation(angle);

        if (_rotateTimer >= gm.RotateDuration)
        {
            _ctx.Rb.MoveRotation(_targetAngle);
            _phase = TransitionPhase.Done;
        }
    }

    public override void LateUpdateState() { }

    public override void ExitState()
    {
        // Hand control back to the physics engine, or the player hangs in the air.
        _ctx.Rb.bodyType        = RigidbodyType2D.Dynamic;
        _ctx.Rb.linearVelocity  = Vector2.zero;
        _ctx.Rb.angularVelocity = 0f;

        // Snap to the exact target angle so accumulated lerp error does not persist.
        _ctx.Rb.MoveRotation(GetSnappedAngle(GravityStateMachine.Instance.Context.TargetDirection));

        // Restore depth, in case anything flattened Z during the transition.
        Vector3 p = _ctx.Transform.position;
        _ctx.Transform.position = new Vector3(p.x, p.y, _z);
    }

    public override PlayerStateMachine.EPlayerState GetNextState()
    {
        if (_timeoutTimer >= MaxTransitionTime)
            return PlayerStateMachine.EPlayerState.Fall;

        if (_phase == TransitionPhase.Done)
            return PlayerStateMachine.EPlayerState.Fall;

        return StateKey;
    }

    // --- Helpers ---

    private Vector2 GravityDirectionToVector(GravityDirection dir) => dir switch
    {
        GravityDirection.Up    => Vector2.up,
        GravityDirection.Left  => Vector2.left,
        GravityDirection.Right => Vector2.right,
        _                      => Vector2.down,
    };

    // Z-only rotation. The character's local up must end up opposite gravity.
    private float GetSnappedAngle(GravityDirection dir) => dir switch
    {
        GravityDirection.Up    => 180f,   // on the ceiling
        GravityDirection.Left  => -90f,   // gravity pulls left, so up-vector points right
        GravityDirection.Right => 90f,    // gravity pulls right, so up-vector points left
        _                      => 0f,     // Down — normal standing
    };

    // Shortest path, except for the 180-degree floor/ceiling flip where the
    // shortest path is ambiguous. There, spin in the direction the player faces
    // so the flip reads as a deliberate roll rather than an arbitrary one.
    private float ShortestSignedDelta(float from, float to, GravityDirection prev, GravityDirection next)
    {
        bool isFlip =
            (prev == GravityDirection.Down && next == GravityDirection.Up) ||
            (prev == GravityDirection.Up   && next == GravityDirection.Down);

        if (isFlip)
            return _ctx.FacingDirection == 1 ? -180f : 180f;

        return Mathf.DeltaAngle(from, to);
    }


    public override void OnTriggerEnter2D(Collider2D other)
    {
        
    }

    public override void OnTriggerStay2D(Collider2D other)
    {
        
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        
    }
}