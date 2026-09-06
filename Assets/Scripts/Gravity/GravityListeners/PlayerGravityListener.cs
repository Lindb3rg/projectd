using UnityEngine;

[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerGravityListener : MonoBehaviour, IGravityListener
{
    private PlayerStateMachine _playerStateMachine;

    private void Awake()
    {
        _playerStateMachine = GetComponent<PlayerStateMachine>();
    }

    private void Start()
    {
        if (GravityStateMachine.Instance == null) return;
        GravityStateMachine.Instance.Context.OnGravityFlipStarted.AddListener(OnGravityFlipStarted);
        GravityStateMachine.Instance.Context.OnGravityFlipCompleted.AddListener(OnGravityFlipCompleted);
    }

    private void OnDisable()
    {
        if (GravityStateMachine.Instance == null) return;
        GravityStateMachine.Instance.Context.OnGravityFlipStarted.RemoveListener(OnGravityFlipStarted);
        GravityStateMachine.Instance.Context.OnGravityFlipCompleted.RemoveListener(OnGravityFlipCompleted);
    }

    public void OnGravityFlipStarted(GravityDirection newDirection)
    {
        // Tell the player state machine to enter its gravity transition state
        // locking input and beginning the visual flip
        Debug.Log("PlayerGravityListener.OnGravityFlipStarted called");
        _playerStateMachine.OnGravityFlipStarted(newDirection);
        

    }

    public void OnGravityFlipCompleted(GravityDirection newDirection)
    {
        // Notify the player state machine that the world has settled
        // so it can restore input and update grounding logic
        _playerStateMachine.OnGravityFlipCompleted(newDirection);
    }
}


