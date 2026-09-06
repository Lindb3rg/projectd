using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    protected Dictionary<EState, BaseState<EState>> States = new();
    protected BaseState<EState> CurrentState;

    public BaseState<EState> PreviousState { get; private set; }
    protected bool IsTransitioningState;

    protected virtual void Awake() { }

    protected virtual void Start()
    {
        if (CurrentState == null)
        {
            Debug.LogError($"{name}: no initial state was set in Awake.", this);
            enabled = false;
            return;
        }

        CurrentState.EnterState();
    }

    protected virtual void Update()
    {
        if (CurrentState == null) return;

        // Update first, then decide. Deciding on pre-update data means a state
        // can transition out before its own logic has run for the frame.
        CurrentState.UpdateState();

        EState nextStateKey = CurrentState.GetNextState();

        if (!nextStateKey.Equals(CurrentState.StateKey))
            TransitionToState(nextStateKey);
    }

    protected virtual void FixedUpdate() => CurrentState?.FixedUpdateState();
    protected virtual void LateUpdate()  => CurrentState?.LateUpdateState();

    public void TransitionToState(EState stateKey)
    {
        if (!States.TryGetValue(stateKey, out BaseState<EState> nextState))
        {
            Debug.LogError($"{name}: state '{stateKey}' was requested but never registered.", this);
            return;
        }

        EState previousKey = CurrentState.StateKey;

        IsTransitioningState = true;

        CurrentState.ExitState();
        PreviousState = CurrentState;
        CurrentState  = nextState;

        // Hook fires before EnterState, so the incoming state can read
        // where it came from.
        OnStateChanged(previousKey, stateKey);

        CurrentState.EnterState();

        IsTransitioningState = false;
    }

    // Overridden by concrete machines to publish the transition to their context.
    protected virtual void OnStateChanged(EState previousKey, EState newKey) { }

    protected virtual void OnTriggerEnter2D(Collider2D other) => CurrentState?.OnTriggerEnter2D(other);
    protected virtual void OnTriggerStay2D(Collider2D other)  => CurrentState?.OnTriggerStay2D(other);
    protected virtual void OnTriggerExit2D(Collider2D other)  => CurrentState?.OnTriggerExit2D(other);
}