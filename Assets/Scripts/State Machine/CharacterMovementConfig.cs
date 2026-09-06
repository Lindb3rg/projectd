using UnityEngine;
[System.Serializable]
public class CharacterMovementConfig
{
    [Header("Input Thresholds")]
    public float MoveDeadzone = 0.1f;
    public float RunThreshold = 0.6f;

    [Header("Speeds")]
    public float WalkSpeed = 3f;
    public float RunSpeed = 10f;
    public float SprintSpeed = 15f;
    public float CrouchSpeed = 2f;
    public float TurnDelay = 0.15f;

    [Header("Jumping")]
    public float JumpForce = 5f;
    public float JumpHorizontalSpeed = 8f;
    public float DoubleJumpForce = 6f;
    public float FallMultiplier = 2.5f;
    public float LowJumpMultiplier = 2f;
    public float AirAcceleration = 5f;
    public float AirAccelerationMultiplier = 1f;
    public float CoyoteTime = 0.1f;
    public float JumpLockDuration = 0.15f;
    public float SprintJumpMultiplier = 1.2f;

    [Header("Landing")]
    public float LandDuration = 0.2f;
    public float SoftLandThreshold = 6f;
    public float LandControlFactor = 0.5f;

    [Header("Gravity")]
    public float GravityStrength = 1f;

}