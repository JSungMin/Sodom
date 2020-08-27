using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BossSpace.StilettoSpace.SubInfo;
namespace BossSpace.StilettoSpace.Behaviour
{
	public class StilettoBehaviour : BossBehaviour 
	{
		protected static Stiletto actor;
		protected static StilettoActionFSM fsm;
	}
	public class TwoPointMove : StilettoBehaviour
	{
		public static void InitFactory (Vector3 dest, string animName = "", bool autoDir = true, float duration = 0.5f)
		{
			actor = GetBoss<Stiletto>();
			fsm = actor.fsm as StilettoActionFSM;
			init = IInit (autoDir);
			update = IUpdate ();
			reset = IReset ();
			starter = IStartBehaviour();
			if (animName != "")
				actor.PlayAnimation (0, animName, true, 1f);
			actor.tpMoveInfo.moveTimer.duration = duration;
			actor.tpMoveInfo.Destination = dest;
		}
		public static IEnumerator IInit (bool autoDir = true)
		{ 
			actor.tpMoveInfo.Initialize ();
			actor.tpMoveInfo.points [0].transform.position = actor.transform.position;
			if (autoDir)
				actor.desireLookDir = Mathf.Sign (actor.tpMoveInfo.Destination.x - actor.tpMoveInfo.Origin.x);
			//actor.SetLookDirection (actor.desireLookDir);
			yield return null;
		}
		public static IEnumerator IUpdate ()
		{

			while (!actor.tpMoveInfo.CheckMoveTimer ()) {
				actor.tpMoveInfo.IncMoveTimer (Time.deltaTime);
				var lerpValue = actor.tpMoveInfo.EvaluateCurve ();
				var curPos = actor.tpMoveInfo.GetPosition (lerpValue);
				actor.transform.position = curPos;
				yield return null;
			}
			actor.transform.position = actor.tpMoveInfo.Destination;
			yield return null;
		}
		public static IEnumerator IReset ()
		{
			actor.tpMoveInfo.Reset ();
			actor.OnTwoPointMoveEnd ();
			actor.tpMoveInfo.moveTimer.duration = 0.5f;
			yield return null;
		}
	}
	public class BaldoBehaviour : StilettoBehaviour
	{
		public static void InitFactory (Vector3 dest)
		{
			actor = GetBoss<Stiletto>();
			fsm = actor.fsm as StilettoActionFSM;
			init = IInit ();
			update = TwoPointMove.IUpdate ();
			reset = IReset ();
			starter = IStartBehaviour();
			actor.tpMoveInfo.moveTimer.duration = 0.1f;
			actor.tpMoveInfo.Destination = dest;
		}
		public static IEnumerator IInit ()
		{
			actor.baldoInfo.Initialize ();
			actor.tpMoveInfo.Initialize ();
			actor.tpMoveInfo.points [0].transform.position = actor.transform.position;
			yield return null;
		}
		public static IEnumerator IReset (string anim = "")
		{
			actor.PlayAnimation (0, "Battojutsu_End", false, 1f);
			actor.tpMoveInfo.moveGhost.ghostingEnabled = false;
			actor.ghosting.ghostingEnabled = false;
			while (!actor.baldoInfo.attackTimer.CheckTimer ())
			{
				actor.baldoInfo.attackTimer.IncTimer (Time.deltaTime);
				yield return null;
			}
			var projPos = actor.projectile [1].transform.position;
			projPos.x = (actor.tpMoveInfo.Destination.x + actor.tpMoveInfo.Origin.x) * 0.5f;
			actor.projectile [1].transform.position = projPos;
			actor.projectile [1].OnLaunch ("발도_완료", true);
			float timer = 0f;
			while (timer <= 0.3f)
			{
				timer += Time.deltaTime;
				yield return null;
			}
			actor.PlayAnimation (0, fsm.idleState.stateInfo.animName, false, 1f);
			actor.baldoInfo.Reset ();
			actor.tpMoveInfo.Reset ();
			yield return null;
		}
	}
}
