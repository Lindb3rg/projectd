using UnityEngine;
using System;
public abstract class BaseState<EState> where EState:Enum
{
    public BaseState(EState key)
    {
        StateKey = key;
    }
    public EState StateKey { get; private set; }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void LateUpdateState();
    public abstract void ExitState();
    public abstract EState GetNextState();
    public abstract void OnTriggerEnter2D(Collider2D other);
    public abstract void OnTriggerStay2D(Collider2D other);
    public abstract void OnTriggerExit2D(Collider2D other);

}