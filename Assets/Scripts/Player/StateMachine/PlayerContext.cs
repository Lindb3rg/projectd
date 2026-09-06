using System.Collections.Generic;
using UnityEngine;

// Everything shared with enemies and NPCs lives in CharacterContext.
// Only things unique to the controllable player belong here.
public class PlayerContext : CharacterContext
{
    // --- Player-only components ---
    public Transform HeadCheck { get; }      // ceiling clearance for standing up from crouch
    public Transform ChestTarget { get; }    // aim target

    // --- Player-only tuning ---
    public PlayerAbilityConfig Abilities { get; }

    // --- State machine feedback ---
    // Plain data rather than a reference back to PlayerStateMachine, so the
    // context doesn't depend on the MonoBehaviour that owns it.
    public PlayerStateMachine.EPlayerState PreviousStateKey { get; set; }

    // --- Player-only intent ---
    // These are button semantics. An AI decides to crouch; it doesn't hold a key.
    public Vector2 AimInput { get; set; }
    public bool AimHeld { get; set; }
    public bool SprintHeld { get; set; }
    public bool CrouchHeld { get; set; }
    public bool EquipPressed { get; set; }
    public bool InteractPressed { get; set; }
    public bool GravityFlipPressed { get; set; }
    public Vector2 GravityFlipDirectionInput { get; set; }

    // --- Player-only runtime state ---
    public bool IsAiming { get; set; }
    public bool IsArmed { get; set; }
    public float DefaultLocalY { get; set; }

    private float _aimResetTimer;

    public InputCooldownTracker InputCooldown { get; } = new InputCooldownTracker();

    public PlayerContext(
        Rigidbody2D rb,
        Animator anim,
        Transform transform,
        SpriteRenderer sprite,
        Transform groundCheck,
        Transform pivotCheck,
        Transform frontCheck,
        Transform headCheck,
        Transform chestTarget,
        CharacterMovementConfig config,
        PlayerAbilityConfig abilities)
        : base(rb, anim, transform, sprite, groundCheck, pivotCheck, frontCheck, config)
    {
        HeadCheck   = headCheck;
        ChestTarget = chestTarget;
        Abilities   = abilities;

        if (ChestTarget != null)
            DefaultLocalY = ChestTarget.localPosition.y;
    }

    // ------------------------------------------------------------------
    // Toggles
    // ------------------------------------------------------------------

    public void HandleCrouchToggle()
    {
        if (CrouchHeld && InputCooldown.TryTrigger("Crouch", Abilities.CrouchToggleCooldown))
            IsCrouching = !IsCrouching;
    }

    public void HandleSprintToggle()
    {
        if (SprintHeld && InputCooldown.TryTrigger("Sprint", Abilities.SprintToggleCooldown))
            IsSprinting = !IsSprinting;
    }

    // ------------------------------------------------------------------
    // Turning
    // ------------------------------------------------------------------

    // Facing lags input by TurnDelay. Movement does not wait for this —
    // ApplyGroundMovement reads MoveInput directly, so the character moves
    // immediately and the visual turn catches up.
    public void HandleTurning(float deltaTime)
    {
        bool wantsToTurn = (MoveInput.x > 0f && FacingDirection == -1)
                        || (MoveInput.x < 0f && FacingDirection == 1);

        if (!wantsToTurn)
        {
            TurnTimer = 0f;
            return;
        }

        TurnTimer += deltaTime;
        if (TurnTimer < Config.TurnDelay) return;

        SetFacing(MoveInput.x > 0f ? 1 : -1);
        TurnTimer = 0f;
    }

    // ------------------------------------------------------------------
    // Aiming
    // ------------------------------------------------------------------

    public void HandleAim(float deltaTime)
    {
        if (ChestTarget == null) return;

        if (Mathf.Abs(AimInput.y) > 0.1f)
        {
            Vector3 localPos = ChestTarget.localPosition;
            localPos.y += AimInput.y * Abilities.AimSpeed * deltaTime;
            localPos.y  = Mathf.Clamp(localPos.y, -1f, 2f);
            ChestTarget.localPosition = localPos;

            _aimResetTimer = Abilities.AimResetDelay;
            return;
        }

        _aimResetTimer -= deltaTime;
        if (_aimResetTimer > 0f) return;

        Vector3 pos = ChestTarget.localPosition;
        pos.y = Mathf.MoveTowards(pos.y, DefaultLocalY, Abilities.AimResetSpeed * deltaTime);
        ChestTarget.localPosition = pos;
    }

    // ------------------------------------------------------------------

    public class InputCooldownTracker
    {
        private readonly Dictionary<string, float> _lastTriggerTimes = new();

        public bool TryTrigger(string key, float cooldown)
        {
            if (_lastTriggerTimes.TryGetValue(key, out float lastTime) &&
                Time.time - lastTime < cooldown)
            {
                return false;
            }

            _lastTriggerTimes[key] = Time.time;
            return true;
        }
    }
}