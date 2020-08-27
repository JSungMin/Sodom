using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class HundEMPState : ActionState {
	public Hund hund;
	public Vector3 empPos;
	private Vector3 startPos;
	private float disToEmpPos;	//	Set When State Entered	
	public float speed;			//	empPos까지 걸어가는데 필요한 등속도
	public Timer stateTimer;	//	등속도 운동으로 걸어가는데 필요한 Timer

		
	//	End HELPER Block
	#region implemented abstract members of ActionStateNode
	public bool CommonCheckEnter (ActionState fromNode, object infoParam)
	{
		if (fromNode == fsm.GetState<StunState>())
			return false;
		if (fromNode == fsm.GetState<DamagedState>())
			return false;
		if (fromNode == fsm.GetState<HundChaseState> ())
			return false;
		if (CheckAnimationEnd ())
			return false;
		if (CheckIsLoopNode (fromNode))
			return false;
		return true;
	}
	public bool CommonCheckExit ()
	{
		if (stateTimer.CheckTimer()) {
			OnAnimationEnd ();
			return true;
		}
		return false;
	}

	public override void CommonEnter ()
	{
        EnemyAIHelper.HundAIHelper.IsLightOFF = true;
        hund = (hund == null) ? targetActor as Hund : hund;
		isAnimationEnd = false;
		empPos.y = hund.transform.position.y;
		empPos.z = hund.transform.position.z;
		startPos = hund.transform.position;
		disToEmpPos = Mathf.Abs(empPos.x - hund.transform.position.x);
		stateTimer.timer = 0;
		stateTimer.duration = disToEmpPos / speed;
	}
	public override void CommonUpdate()
	{
		if (hund.isTurning)
			return;
		if (stateTimer.CheckTimer ())
		{
			isAnimationEnd = false;
		}
		else
		{
			stateTimer.IncTimer (Time.deltaTime);
			hund.SetDesireDirection (Mathf.Sign(empPos.x - startPos.x));
			if (hund.desireLookDir == hund.lookDirection) {
				PlayAnimation (0, "Walk2", true, 0f);
				hund.transform.position = Vector3.Lerp (startPos, empPos, stateTimer.GetRatio ());
			}
			else {
				hund.SetLookDirection ();
			}
		}
	}
	public override void CommonExit ()
	{
		base.CommonExit ();
        EnemyAIHelper.ClearPatternBuffer (hund);
		hund.nowPattern.ReadyPattern (0);
	}

	#endregion
}
