using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class AttackState : ActionState {
	//	Editted On TransferToAttackState
	public bool skipToCounter = false;
	public SkillInfo skillInfo;

	public void EditStateInfo (SkillInfo skillInfo)
	{
		if (null != skillInfo)
		{
			this.skillInfo = skillInfo;
		}
		this.stateInfo.animIndex = this.skillInfo.animIndex;
		this.stateInfo.animName = this.skillInfo.animName;
		targetActor.SetUsedSkill (this.skillInfo.skillName);
	}

	//	HELPER Function Block
	protected bool CheckConsumeable (Actor actor, SkillInfo tmpSkill)
	{
		if (tmpSkill.consumeType == ConsumeResourceType.HP)
		{
			if (actor.actorInfo.GetLife() < tmpSkill.consumeAmount)
			{
				return false;
			}
		}
		else if (tmpSkill.consumeType == ConsumeResourceType.MP)
		{
			if (actor.actorInfo.GetLife() < tmpSkill.consumeAmount)
			{
				return false;
			}
		}
		return true;
	}
	protected bool CheckAttackCancling(ActionState fromNode, SkillInfo tmpSkillInfo)
	{
		//	카운터 성공시 모든 스킬을 1회 준비동작없이 사용
		if (fromNode == fsm.GetState<CounterState>()) {
			return true;
		}
		if (tmpSkillInfo.skillType > skillInfo.skillType)
		{
			return true;
		}
		if (tmpSkillInfo.skillType - skillInfo.skillType == -3)
		{
			return true;
		}
		return false;
	}
	//	End HELPER Block
	protected SkillInfo tmpSkillInfo;

	#region implemented abstract members of ActionStateNode
	public bool CommonCheckEnter (ActionState fromNode, object infoParam)
	{
		tmpSkillInfo = infoParam as SkillInfo;
		if (tmpSkillInfo.IsEmpty ())
			return false;
		if (fromNode == fsm.GetState<StunState>())
			return false;
		if (fromNode == fsm.GetState<DamagedState>())
			return false;
		if (!CheckConsumeable (targetActor, tmpSkillInfo))
			return false;
		if (fromNode == fsm.GetState<JumpState>()) {
			if (tmpSkillInfo.entityType == SkillEntityType.GROUND)
				return false;
		}
		if (CheckAnimationEnd ())
			return false;
		if (CheckIsLoopNode (fromNode))
			return false;
		if (!targetActor.skillCoolTimer.IsUseable (tmpSkillInfo.skillName))
			return false;
		return true;
	}
	public bool CheckEnterByPlayer (ActionState fromNode, object infoParam)
	{
		tmpSkillInfo = infoParam as SkillInfo;
		if (tmpSkillInfo.IsEmpty ())
			return false;
		if (fromNode == fsm.GetState<DamagedState>())
			return false;
		if (fromNode == fsm.GetState<RollingState>())
			return false;
		if (fromNode == fsm.GetState<CounterState>())
			return false;
		if (fromNode == fsm.GetState<DeadState>())
			return false;
		if (fromNode == fsm.GetState<DownState>())
			return false;
		if (!CheckConsumeable (targetActor, tmpSkillInfo))
			return false;
		if (fromNode == fsm.GetState<JumpState>()) {
			if (tmpSkillInfo.entityType == SkillEntityType.GROUND)
				return false;
		}
		if (tmpSkillInfo.skillName == "Air_Attack") {
			if (!targetActor.actorInfo.canDoAirBehaviour)
				return false;
		}
		if (!targetActor.skillCoolTimer.IsUseable (tmpSkillInfo.skillName))
			return false;
		var counterState = fsm.GetState<CounterState> ();
		if (fromNode == counterState) {
			if (tmpSkillInfo.skillCounterType != SkillCounterType.NONE) {
				skipToCounter = true;
				return true;
			}
		}
		if (CheckAnimationEnd ())
			return true;
		if (CheckIsLoopNode (fromNode)) {
			/*if (CheckAttackCancling (fromNode, tmpSkillInfo)) {
				//	공격 Cancling 후 특정행동이 있는가?
				return true;
			}*/
			return false;
		}
		return true;
	}
	public bool CheckEnterByEnemy (ActionState fromNode, object infoParam)
	{
		tmpSkillInfo = infoParam as SkillInfo;
		if (null == infoParam)
		{
			Debug.Log("Call From No Param Translation");
			//	SkillInfo가 무조건 등록되어있어야함
			tmpSkillInfo = skillInfo;
			targetActor.skillCoolTimer.MakeFullCharge(skillInfo.skillName);
		}
		else if (tmpSkillInfo.IsEmpty ())
		{
			Debug.LogError("tmpSkillInfo is NULL in AttackState");
			return false;
		}
		if (fromNode == fsm.GetState<StunState>())
		{
			Debug.Log("ATTACK FAIL : FROM STUN");
			return false;
		}
		if (fromNode == fsm.GetState<DamagedState>())
		{
			Debug.Log("ATTACK FAIL : FROM DAMAGED");
			return false;
		}
		if (!CheckConsumeable (targetActor, tmpSkillInfo))
		{
			Debug.Log("ATTACK FAIL : FROM CONSUME TEST");
			return false;
		}
		if (fromNode == fsm.GetState<JumpState>()) {
			if (tmpSkillInfo.entityType == SkillEntityType.GROUND)
				return false;
		}

		var enemy = targetActor as EnemySpineBase;
		if (!enemy.actorInfo.isStunByParry)
		{
			if (CheckAnimationEnd ())
				return false;
			if (CheckIsLoopNode (fromNode))
				return false;
		}
		if (!targetActor.skillCoolTimer.IsUseable (tmpSkillInfo.skillName))
		{
			Debug.Log("ATTACK FAIL : FROM SKILL COOL TEST");
			return false;
		}
		return true;
	}
	public bool CheckExitByPlayer ()
	{
		if (skillInfo.entityType == SkillEntityType.AIR && targetActor.actorInfo.isGrounded)
			return true;
		if (CheckAnimationEnd () && fsm.serviceInstance.CheckSkillBufferEmpty ())
			return true;
		return false;
	}
	public bool CommonCheckExit ()
	{
		if (CheckAnimationEnd ()) {
			OnAnimationEnd ();
			return true;
		}
		return false;
	}

	public override void CommonEnter ()
	{
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, skillInfo.animationSpeed - 1);
		if (skillInfo.fxName != "")
			PlayEffectAnimation (skillInfo.fxName);
		targetActor.SetMoveable (false);
		if (skillInfo.entityType == SkillEntityType.GROUND) {
			var prevVelocity = targetActor.rigid.velocity;
			if (!skillInfo.useInertia)
				prevVelocity.x = 0f;
			targetActor.rigid.velocity = prevVelocity;
			targetActor.rigid.AddForce (-Mathf.Sign (targetActor.transform.localScale.x) * skillInfo.forwardStepAmount * Vector3.right, ForceMode.Impulse);
		}
		for (int i = 0; i < skillInfo.onAttackEnterEventList.Count; i++) 
		{
			SkillContentPool.ExecuteOnAttackEnterContent (skillInfo.onAttackEnterEventList [i], targetActor, skillInfo);
		}
	}
	public void EnterByPlayer ()
	{
		if (skillInfo.entityType == SkillEntityType.AIR) {
			if (!targetActor.actorInfo.canDoAirBehaviour)
				return;
			targetActor.actorInfo.canDoAirBehaviour = false;
			fsm.serviceInstance.ClearSkillBuffer ();
		}
		isAnimationEnd = false;
		if (!skipToCounter)
			PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, skillInfo.animationSpeed - 1);
		else
			PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, skillInfo.animationSpeed - 1, true);
		if (skillInfo.fxName != "")
			PlayEffectAnimation (skillInfo.fxName);
		targetActor.SetMoveable (false);
		skipToCounter = false;
		if (skillInfo.entityType == SkillEntityType.GROUND) {
			var prevVelocity = targetActor.rigid.velocity;
			prevVelocity.x = 0f;
			targetActor.rigid.velocity = prevVelocity;
			targetActor.rigid.AddForce (-Mathf.Sign (targetActor.transform.localScale.x) * skillInfo.forwardStepAmount * Vector3.right, ForceMode.Impulse);
		}
		for (int i = 0; i < skillInfo.onAttackEnterEventList.Count; i++) 
		{
			SkillContentPool.ExecuteOnAttackEnterContent (skillInfo.onAttackEnterEventList [i], targetActor, skillInfo);
		}
	}
	public void EnterByEnemy ()
	{
		isAnimationEnd = false;
		var enemy = targetActor as EnemySpineBase;
		if (!enemy.actorInfo.isStunByParry)
			PlayAnimation (stateInfo.animIndex, stateInfo.animName, skillInfo.animLoop, skillInfo.animationSpeed - 1);
		else
			PlayAnimation (stateInfo.animIndex, stateInfo.animName, skillInfo.animLoop, skillInfo.animationSpeed - 1, true);
		if (skillInfo.fxName != "")
			PlayEffectAnimation (skillInfo.fxName);
		targetActor.SetMoveable (false);
		if (skillInfo.entityType == SkillEntityType.GROUND) {
			var prevVelocity = targetActor.rigid.velocity;
			if (!skillInfo.useInertia)
				prevVelocity.x = 0f;
			targetActor.rigid.velocity = prevVelocity;
			targetActor.rigid.AddForce (-Mathf.Sign (targetActor.transform.localScale.x) * skillInfo.forwardStepAmount * Vector3.right, ForceMode.Impulse);
		}
		for (int i = 0; i < skillInfo.onAttackEnterEventList.Count; i++) 
		{
			SkillContentPool.ExecuteOnAttackEnterContent (skillInfo.onAttackEnterEventList [i], targetActor, skillInfo);
		}
	}
	public override void CommonUpdate()
	{
		for (int i = 0; i < skillInfo.onAttackUpdateEventList.Count; i++) 
		{
			SkillContentPool.ExecuteOnAttackUpdateContent (skillInfo.onAttackUpdateEventList [i], targetActor, skillInfo);
		}
	}
	public override void CommonExit ()
	{
		base.CommonExit ();
		targetActor.SetMoveable (true);
		targetActor.skillCoolTimer.ResetSkill(skillInfo.skillName);
		for (int i = 0; i < skillInfo.onAttackExitEventList.Count; i++) 
		{
			SkillContentPool.ExecuteOnAttackExitContent (skillInfo.onAttackExitEventList [i], targetActor, skillInfo);
		}
	}
	public void ExitByPlayer ()
	{
		CommonExit ();
		fsm.serviceInstance.ClearSkillBuffer ();
	}

	#endregion
}
