using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class HundTackleState : ActionState {
	private Hund hund;
	private Player player;
	private Vector3 startPos;
	private Vector3 playerPos;	
	public float minX = 1f;
	public float maxX = 14.83f;
	public float safetyDis = -1.5f;
	private float runDir;
	private float speed;			//	empPos까지 걸어가는데 필요한 등속도
	public Timer stateTimer;	//	등속도 운동으로 걸어가는데 필요한 Timer
	protected SkillInfo tmpSkillInfo;

	public void EditStateInfo (SkillInfo skillInfo)
	{
		
	}

	//	End HELPER Block

	#region implemented abstract members of ActionStateNode
	public bool CommonCheckEnter (ActionState fromNode, object infoParam)
	{
		if (fromNode == this)
			return false;
		else if (fromNode == fsm.GetState<HundTackleSubState>())
			return false;
		else if (fromNode == fsm.GetState<HundStopState>())
			return false;
		else if (fromNode == fsm.GetState<AttackState>())
			return false;
		if (fromNode == fsm.GetState<StunState>())
			return false;
		if (fromNode == fsm.GetState<DamagedState>())
			return false;
		if (fromNode == fsm.GetState<HundEMPState> ())
			return false;
		if (CheckIsLoopNode (fromNode))
			return false;
		return true;
	}
	public bool CommonCheckExit ()
	{
		if (CheckAnimationEnd() &&
			stateTimer.CheckTimer()) 
		{
			OnAnimationEnd ();
			return true;
		}
		return false;
	}

	public override void CommonEnter ()
	{
		EnemyAIHelper.HundAIHelper.ResetDirty();
		EnemyAIHelper.HundAIHelper.IsTacklePipeline = true;
		hund = (hund == null) ? targetActor as Hund : hund;
		player = (player == null) ? hund.player : player;
		startPos = hund.transform.position;
		playerPos = player.transform.position;
		playerPos.y = startPos.y;
		playerPos.z = startPos.z;

		var deltaToMinX = Mathf.Abs(startPos.x - minX);
		var deltaToMaxX = Mathf.Abs(startPos.x - maxX);

		runDir = Mathf.Sign(playerPos.x - startPos.x);
		hund.rigid.velocity = Vector3.zero;
		isAnimationEnd = false;
		stateTimer.timer = 0;
		stateTimer.duration = 0f;
		//stateTimer.duration = Mathf.Abs(playerPos.x - startPos.x) / (maxPosX - minPosX);
		if (runDir != hund.lookDirection)
		{
			if (deltaToMinX >= deltaToMaxX)
        	{
				if (deltaToMaxX <= 1f && hund.lookDirection != -1f)
				{
					hund.SetDesireDirection(-1f);
					hund.SetLookDirection();
					return;
				}
			}
			else
			{
				if (deltaToMinX <= 1f && hund.lookDirection != 1f)
				{
					hund.SetDesireDirection(1f);
					hund.SetLookDirection();
					return;
				}
			}
			hund.SetDesireDirection (runDir);
			hund.SetLookDirection();
		}
		else
		{
			if (deltaToMinX >= deltaToMaxX)
        	{
				if (deltaToMaxX <= 1f && hund.lookDirection != -1f)
				{
					hund.SetDesireDirection(-1f);
					hund.SetLookDirection();
					return;
				}
			}
			else
			{
				if (deltaToMinX <= 1f && hund.lookDirection != 1f)
				{
					hund.SetDesireDirection(1f);
					hund.SetLookDirection();
					return;
				}
			}
			isAnimationEnd = true;
		}
	}
	public override void CommonUpdate()
	{
		//if (EnemyAIHelper.HundAIHelper.IsTurning)
		//	return;
		//playerPos.x = player.transform.position.x;
		//var destPos = playerPos + Vector3.right * runDir * safetyDis;
		//destPos.x = Mathf.Clamp (destPos.x, minPosX, maxPosX);
		
		//hund.transform.position = Vector3.Lerp (startPos, destPos, stateTimer.GetRatio ());
		//stateTimer.IncTimer (Time.deltaTime);
		//PlayAnimation (0, "Run", true, 0f);
	}
	public override void CommonExit ()
	{
		base.CommonExit ();
		stateTimer.Reset();
	}
	#endregion
}
