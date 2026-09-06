using UnityEngine;

// Player-only tuning. Enemies use CharacterMovementConfig alone.
[CreateAssetMenu(fileName = "PlayerAbilityConfig", menuName = "Divers/Player Ability Config")]
public class PlayerAbilityConfig : ScriptableObject
{
    [Header("Edge Climbing")]
    public float ClimbRepositionThreshold = 0.05f;
    public float ClimbLedgeOffset = 0.1f;
    public float ClimbRepositionSpeed = 10f;

    [Header("Aiming")]
    public float AimDistance = 10f;
    public float AimSpeed = 30f;
    public float AimResetDelay = 1f;
    public float AimResetSpeed = 50f;

    [Header("Input Cooldowns")]
    public float CrouchToggleCooldown = 0.8f;
    public float SprintToggleCooldown = 1f;
}