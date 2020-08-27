using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BossSpace.MetatronSpace.SubInfo;
namespace BossSpace.MetatronSpace.Behaviour
{
	public class MetatronBehaviour : BossBehaviour 
	{
		protected static Metatron actor;
		protected static EnemyActionFSM fsm;
	}
	public class TwoPointMove : MetatronBehaviour
	{
		public static void InitFactory (Vector3 dest, string animName = "", bool autoDir = true, float duration = 0.5f)
		{
			actor = GetBoss<Metatron>();
			fsm = actor.fsm as EnemyActionFSM;
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
				actor.lookDirection = Mathf.Sign (actor.tpMoveInfo.Destination.x - actor.tpMoveInfo.Origin.x);
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
			//actor.OnTwoPointMoveEnd ();
			actor.tpMoveInfo.moveTimer.duration = 0.5f;
			yield return null;
		}
	}
}
