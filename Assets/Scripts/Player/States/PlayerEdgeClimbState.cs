// using UnityEngine;

// public class PlayerEdgeClimbState : BaseState<PlayerStateMachine.EPlayerState>
// {
//     private readonly PlayerContext _ctx;
//     private float _climbSpeed;
//     private const float MaxClimbSpeed = 2f;

//     public PlayerEdgeClimbState(PlayerStateMachine.EPlayerState key, PlayerContext ctx) : base(key)
//     {
//         _ctx = ctx;
//     }

//     public override void EnterState()
//     {
//         _climbSpeed                  = 1.2f;
//         _ctx.Rb.useGravity           = false;
//         _ctx.Rb.linearVelocity       = Vector3.zero;
//         _ctx.Anim.applyRootMotion    = true;

//         _ctx.Transform.position = new Vector3(
//             _ctx.Transform.position.x,
//             _ctx.EdgePosition.y - _ctx.ClimbLedgeOffset,
//             _ctx.Transform.position.z
//         );

//         _ctx.Anim.SetBool("isRunning", false);
//         _ctx.Anim.SetTrigger("edgeClimbTrigger");
//         _ctx.Anim.SetFloat("climbSpeed", _climbSpeed);
//     }

//     public override void ExitState()
//     {
//         _ctx.Anim.applyRootMotion = false;
//         _ctx.Anim.SetFloat("climbSpeed", _climbSpeed);

//         _ctx.Transform.position = new Vector3(
//             _ctx.Transform.position.x,
//             _ctx.EdgePosition.y + _ctx.ClimbLedgeOffset,
//             _ctx.Transform.position.z
//         );

//         _ctx.Rb.linearVelocity = Vector3.zero;
//         _ctx.Rb.useGravity     = true;

//         if (_ctx.MeshChild != null)
//         {
//             _ctx.MeshChild.localPosition = Vector3.zero;
//             _ctx.MeshChild.localRotation = Quaternion.identity;
//         }
//     }

//     public override void UpdateState()
//     {
//         if (_ctx.JumpPressed)
//         {
//             _climbSpeed = Mathf.Clamp(_climbSpeed + 0.2f, 1f, MaxClimbSpeed);
//             _ctx.Anim.SetFloat("climbSpeed", _climbSpeed);
//         }
//     }

//     public override void FixedUpdateState() { }
//     public override void LateUpdateState()  { }

//     public override PlayerStateMachine.EPlayerState GetNextState()
//     {
//         AnimatorStateInfo info = _ctx.Anim.GetCurrentAnimatorStateInfo(0);
//         if (info.IsName("edge_climb") && info.normalizedTime >= 0.8f)
//             return PlayerStateMachine.EPlayerState.Idle;

//         return StateKey;
//     }

//     public override void OnTriggerEnter(Collider other) { }
//     public override void OnTriggerStay(Collider other)  { }
//     public override void OnTriggerExit(Collider other)  { }
// }