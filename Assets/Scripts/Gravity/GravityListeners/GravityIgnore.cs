using UnityEngine;

/// <summary>
/// Attach to any GameObject that should be unaffected by gravity flips.
/// The GravityStateMachine and listeners can check for this component
/// before applying gravity changes to an object.
/// </summary>
public class GravityIgnore : MonoBehaviour { }