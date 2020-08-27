using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class IdleState : ActionState {
	public void EditStateInfo (string animName)
	{
		stateInfo.animName = animName;
	}

	public bool CheckEnterByPlayer (ActionState fromState, object infoParam)
	{
		//	TODO : 이후 Actor의 지상, 공중 상태 체크필요
		if (!targetActor.GetMoveable ())
			return false;
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (fromState == fsm.GetState<DamagedState>()) {
			if (!fromState.checkExit())
				return false;
		}
		return true;
	}
	public bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (fromState == fsm.GetState<DamagedState>()){
			if (!fromState.checkExit())
				return false;
		}
		if (fromState == fsm.GetState<StunState>()) {
			if (!fromState.checkExit ())
				return false;
		}
		return true;
	}
	public bool CheckEnterByHund (ActionState fromState, object infoParam)
	{
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (fromState == fsm.GetState<HundTackleState>())
			return false;
		if (fromState == fsm.GetState<HundTackleSubState>())
		if (fromState == fsm.GetState<DamagedState>()){
			if (!fromState.checkExit())
				return false;
		}
		if (fromState == fsm.GetState<StunState>()) {
			if (!fromState.checkExit ())
				return false;
		}
		return true;
	}
	//	Replace To AlwaysReturnTrue();
	/* public bool CheckExitByPlayer ()
	{
		return true;
	}*/
	//	Replace To CommonEnter, CommonExit
	/* public void OnEnterByPlayer ()
	{
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, true, 0f);
	}
	public void OnExitByPlayer ()
	{
		isAnimationEnd = false;
	}*/
	//	Replace To AlwaysReturnTrue();
	/* public bool CheckExitByEnemy ()
	{
		return true;
	}*/



}
