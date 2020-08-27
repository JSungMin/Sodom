using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class HundChaseState : ActionState {
	private Hund hund;
	private Player player;
	private Vector3 startPos;
	private Vector3 playerPos;
	public float minPosX = 1f;
	public float maxPosX = 14.83f;

	public float safetyDis = 1.5f;
	private float runDir;
	private float speed;			//	empPos까지 걸어가는데 필요한 등속도
	public Timer stateTimer;	//	등속도 운동으로 걸어가는데 필요한 Timer
	public bool useTurn = false;
	public void EditStateInfo (SkillInfo skillInfo)
	{
		//tmpSkillInfo = skillInfo;
		//targetActor.SetUsedSkill (tmpSkillInfo.skillName);
	}

	//	End HELPER Block

	#region implemented abstract members of ActionStateNode
	public override bool CommonCheckEnter (ActionState fromNode, object infoParam)
	{
		if (fromNode == fsm.GetState<StunState>())
			return false;
		if (fromNode == fsm.GetState<DamagedState>())
			return false;
		if (fromNode == fsm.GetState<HundEMPState> ())
			return false;
		if (CheckAnimationEnd ())
			return false;
		if (CheckIsLoopNode (fromNode))
			return false;
		return true;
	}
	public override bool CommonCheckExit ()
	{
		if (CheckAnimationEnd() &&
			stateTimer.CheckTimer())
		{
			if (useTurn)
			{
				if (hund.GetNowAnimationName() == "Stop_Turn")
				{
					OnAnimationEnd ();
					return true;
				}
			}
			else
			{
				OnAnimationEnd ();
					return true;
			}
		}
		return false;
	}

	public override void CommonEnter ()
	{
		EnemyAIHelper.HundAIHelper.ResetDirty();
		EnemyAIHelper.HundAIHelper.IsChasePipeline = true;
		hund = (hund == null) ? targetActor as Hund : hund;
		player = (player == null) ? hund.player : player;
		hund.rigid.velocity = Vector3.zero;
		startPos = hund.transform.position;
		playerPos = player.transform.position;
		playerPos.y = startPos.y;
		playerPos.z = startPos.z;
		runDir = Mathf.Sign(playerPos.x - startPos.x);
		isAnimationEnd = false;
		stateTimer.timer = 0;
		stateTimer.duration = 0.5f * Mathf.Abs(playerPos.x - startPos.x) / (maxPosX - minPosX);
		hund.SetDesireDirection (runDir);
		hund.SetLookDirection();
		useTurn = false;
	}
	public override void CommonUpdate()
	{
		if (EnemyAIHelper.HundAIHelper.IsTurning)
			return;
		playerPos.x = player.transform.position.x;
		var destPos = playerPos + Vector3.right * runDir * safetyDis;
		destPos.x = Mathf.Clamp (destPos.x, minPosX, maxPosX);
		if (stateTimer.CheckTimer ())
		{
			EnemyAIHelper.UpdateDistanceInfo(hund);
			hund.SetDesireDirection (hund.disInfo.dirToPlayer);
			if (hund.desireLookDir != hund.lookDirection)
			{
				useTurn = true;
			}
			hund.SetLookDirection();
			isAnimationEnd = false;
		}
		else
		{
			hund.transform.position = Vector3.Lerp (startPos, destPos, stateTimer.GetRatio ());
			stateTimer.IncTimer (Time.deltaTime);
			if (Mathf.Abs(destPos.x - startPos.x) < 1f)
			{
				stateTimer.duration = 0f;
				isAnimationEnd = true;
			}
			else
			{
				PlayAnimation (0,"Run",true, 0f);
			}
		}
	}

	public override void CommonExit ()
	{
		base.CommonExit ();
        hund.patternIndex++;
        //	Dirty Result is 0x09 
	}
	#endregion
}
