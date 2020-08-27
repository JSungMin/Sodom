using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class DamagedState : ActionState {
	public float knockoutTime = 0.2f;
	public float knockoutTimer = 0f;

	public DamageInfo damageInfo;
	public AnimationCurve vibrateCurve;
	public float vibrateAmount;

	private SkillDamageType damageType;

	public void EditStateInfo (DamageInfo damageInfo)
	{
		this.damageInfo = damageInfo;
		//this.stateInfo.animName = targetActor.actorInfo.actor_name + "_Damaged";
	}


	public SkillDamageType GetDamageType ()
	{
		if (null != damageInfo.skillInfo)
			return damageInfo.skillInfo.skillDamageType;
		return 0;
	}

	//	HELPER Function Block
	public virtual void ApplyElementCondition (ref float damProb)
	{
		if (targetActor.HasCondition (ActorConditionType.BURN)) {
			if (damageInfo.HasCondition(ActorConditionType.BURN))
				damProb = 1f;
		}
		else if (targetActor.HasCondition (ActorConditionType.ICED)) {
			if (damageInfo.HasCondition(ActorConditionType.ICED))
				damProb = 1f;
		}
		else {
			damProb = UnityEngine.Random.Range (0f, 1f);
		}
	}

	public virtual void ApplyDamage (BasicStatInfo tmpBasicStat, float damProb)
	{
		var damagePos = targetActor.bodyCollider.bounds.center;
		var damageDir = Vector3.zero;
		damageDir.x = targetActor.transform.position.x - damageInfo.attacker.transform.position.x;
		damageDir.y = 0.5f;
		damageDir.Normalize ();
		if (targetActor.actorInfo.isStunByParry) {
			targetActor.actorInfo.SubLife(10f);
			//fsm.serviceInstance.PopCriticalText (damagePos, 10f, damageDir);
			return;
		}
		if (damProb <= tmpBasicStat.quickness) {
			//	TODO : EVASION
			//fsm.serviceInstance.PopEvasionText (damagePos, damageDir);
			//	stateInfo.anim_name = targetActor.actorInfo.actor_name + "_Evasion";
			//	TODO : Call 'MISS' UI
		} 
		else {
			if (damProb >= tmpBasicStat.combatExperience) {
				//	TODO : CRITIDCAL
				var damage = 0f;
				if (damageInfo.skillInfo.skillType == SkillType.NORMAL)
					damage = (damageInfo.skillInfo.damage * 1.5f - tmpBasicStat.physicalDefense);
				else
					if (damageInfo.skillInfo.skillType == SkillType.SPECIAL)
						damage = (damageInfo.skillInfo.damage * 1.5f - tmpBasicStat.specialDefense);

				damage = Mathf.Max (0f, damage);
				targetActor.actorInfo.SubLife(damage);
				fsm.serviceInstance.PopCriticalText (damagePos, damage, damageDir);
				//InGameUtility.KnockbackToActor (targetActor, damageDir, 2.5f);
			}
			else {
				//	TODO : NORMAL 
				var damage = 0f;
				if (damageInfo.skillInfo.skillType == SkillType.NORMAL)
					damage = (damageInfo.skillInfo.damage - tmpBasicStat.physicalDefense);
				else
					if (damageInfo.skillInfo.skillType == SkillType.SPECIAL)
						damage = (damageInfo.skillInfo.damage - tmpBasicStat.specialDefense);
				damage = Mathf.Max (0f, damage);
				targetActor.actorInfo.SubLife (damage);
				fsm.serviceInstance.PopDamageText (damagePos, damage, damageDir);
				//InGameUtility.KnockbackToActor (targetActor, damageDir, 2.5f);
			}
		}
	}

	public virtual void AddSkillConditions (BasicStatInfo tmpDetailStat, ref float conProb)
	{
		for (int i = 0; i < damageInfo.skillInfo.conditionEffects.Count; ++i) {
			conProb = UnityEngine.Random.Range (0f, 1f);
			if (tmpDetailStat.immunity < conProb) {
				fsm.serviceInstance.AddActorCondition (targetActor, damageInfo.skillInfo.conditionEffects [i], damageInfo.skillInfo.durations [i], damageInfo.skillInfo.effectiveness [i]);
			}
		}
	}

	protected virtual bool CheckAttackCounter(ActionState fromNode, DamageInfo damageInfo)
	{
		if (fromNode != fsm.GetState<AttackState>())
			return false;
		var victimType = fsm.GetState<AttackState>().skillInfo.skillCounterType;
		var attackerType = damageInfo.skillInfo.skillCounterType;
		if (!fsm.serviceInstance.CheckSkillCounter(victimType,	attackerType))
		{
			return false;
		}
		return true;
	}

	public bool isCounter = false;

	#region implemented abstract members of ActionState

	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		var tmpDamageInfo = infoParam as DamageInfo;
		if (null == tmpDamageInfo)
			return false;
		if (targetActor.GetUnbeatable ())
			return false;
		if (CheckAttackCounter (fromState, infoParam as DamageInfo)) 
		{
			//fsm.TryTransferAction<StunState> ("StunState", tmpDamageInfo);
			if (targetActor.actorInfo.GetLife () <= 0f)
			{
				fsm.TryTransferAction<DeadState> (damageInfo);
			}
			return false;
		}
		if (tmpDamageInfo.skillInfo.skillCounterType != SkillCounterType.NONE)
			return false;
		if (targetActor.actorInfo.isSuperarmor) 
		{
			var tmpBasicStat = targetActor.actorInfo.GetTotalBasicInfo ();
			var damProb = 0f;
			var conProb = 0f;
			this.damageInfo = tmpDamageInfo;
			//	화상, 빙결과 같은 상태이상을 검사하고 크리티컬을 터트리게 확률을 지정.
			ApplyElementCondition (ref damProb);
			//	회피 및 크리티컬 데미지 적용
			ApplyDamage (tmpBasicStat, damProb);
			//	상태 이상 적용
			AddSkillConditions (tmpBasicStat, ref conProb);
			damageInfo.attacker.OnActorAttackSuccess (tmpDamageInfo);
			for (int i = 0; i < damageInfo.skillInfo.onDamagedEnterEventList.Count; i++) 
			{
				SkillContentPool.ExecuteOnDamagedEnterContent (damageInfo.skillInfo.onDamagedEnterEventList [i], damageInfo);
			}
			if (targetActor.actorInfo.GetLife () <= 0f)
			{
				fsm.TryTransferAction<DeadState> (damageInfo);
			}
			return false;
		}
		return true;
	}
	public override bool CommonCheckExit ()
	{
		if (Mathf.Abs(targetActor.rigid.velocity.x) >= 0.5f)
			return false;
		if (knockoutTimer <= knockoutTime)
			return false;
		return true;
	}
	public override void CommonEnter ()
	{
		base.CommonEnter ();
		var tmpBasicStat = targetActor.actorInfo.GetTotalBasicInfo ();
		var damProb = 0f;
		var conProb = 0f;

		//	TODO : CommonEnter Enemy로 분류 필요
		//if (targetActor.tag == "Enemy")
		//	targetActor.skillCoolTimer.ResetSkill();
		//	화상, 빙결과 같은 상태이상을 검사하고 크리티컬을 터트리게 확률을 지정.
		ApplyElementCondition (ref damProb);
		//	회피 및 크리티컬 데미지 적용
		ApplyDamage (tmpBasicStat, damProb);
		//	상태 이상 적용
		AddSkillConditions (tmpBasicStat, ref conProb);
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
		targetActor.SetMoveable (false);
		targetActor.ResetRecycleTimer ();

		for (int i = 0; i < damageInfo.skillInfo.onDamagedEnterEventList.Count; i++) 
		{
			SkillContentPool.ExecuteOnDamagedEnterContent (damageInfo.skillInfo.onDamagedEnterEventList [i], damageInfo);
		}

		if (targetActor.actorInfo.GetLife () <= 0f)
		{
			fsm.TryTransferAction<DeadState> (damageInfo);
		}
		knockoutTime = damageInfo.skillInfo.knockoutTime;
		knockoutTimer = 0f;
	}
	public bool CheckEnterByPlayer (ActionState fromState, object infoParam)
	{
		var tmpDamageInfo = infoParam as DamageInfo;
		if (null == tmpDamageInfo)
			return false;
		if (targetActor.GetUnbeatable ())
			return false;
		if (fromState == fsm.GetState <DeadState>())
			return false;
		if (tmpDamageInfo.attacker.actorInfo.isStunByParry)
			return false;
		if (CheckAttackCounter (fromState, tmpDamageInfo)) 
		{
			//	연속 공격은 반격 못 하게 함
			if (fsm.TryTransferAction <CounterState> (tmpDamageInfo))
				return false;
			if (fsm.TryTransferAction<DeadState> (damageInfo))
				return false;
			return true;
		}
		if (fsm.TryTransferAction<DeadState> (damageInfo))
			return false;
		if (targetActor.actorInfo.isSuperarmor) 
		{
			var tmpBasicStat = targetActor.actorInfo.GetTotalBasicInfo ();
			var damProb = 0f;
			var conProb = 0f;
			this.damageInfo = tmpDamageInfo;
			//	화상, 빙결과 같은 상태이상을 검사하고 크리티컬을 터트리게 확률을 지정.
			ApplyElementCondition (ref damProb);
			//	회피 및 크리티컬 데미지 적용
			ApplyDamage (tmpBasicStat, damProb);
			//	상태 이상 적용
			AddSkillConditions (tmpBasicStat, ref conProb);
			return false;
		}
		return true;
	}
	public bool CheckExitByPlayer ()
	{
		if (knockoutTimer <= knockoutTime)
			return false;
		return true;
	}
	public void EnterByPlayer()
	{
		base.CommonEnter ();
		var tmpBasicStat = targetActor.actorInfo.GetTotalBasicInfo ();
		var damProb = 0f;
		var conProb = 0f;

		//	TODO : CommonEnter Enemy로 분류 필요
		//if (targetActor.tag == "Enemy")
		//	targetActor.skillCoolTimer.ResetSkill();
		//	화상, 빙결과 같은 상태이상을 검사하고 크리티컬을 터트리게 확률을 지정.
		ApplyElementCondition (ref damProb);
		//	회피 및 크리티컬 데미지 적용
		ApplyDamage (tmpBasicStat, damProb);
		//	상태 이상 적용
		AddSkillConditions (tmpBasicStat, ref conProb);
		isAnimationEnd = false;

		knockoutTime = damageInfo.skillInfo.knockoutTime;
		knockoutTimer = 0f;

		//	플레이어가 날아가는 모션으로 Damage를 받을지, 일반모션을 실행할지 정함
		damageType = GetDamageType();
		switch (damageType) 
		{
		case SkillDamageType.NORMAL:
			PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
			break;
		case SkillDamageType.THROW:
			PlayAnimation (stateInfo.animIndex, stateInfo.animName + "_Air", false, 0f);
			break;
		case SkillDamageType.BLOW:
			targetActor.SetUnbeatable (true, 1f);
			PlayAnimation (stateInfo.animIndex, stateInfo.animName + "_Down", false, 0f);
			knockoutTime = 1f;
			break;
		case SkillDamageType.AIRBORN:
			PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
			break;
		}
		targetActor.SetMoveable (false);
		targetActor.ResetRecycleTimer ();

		for (int i = 0; i < damageInfo.skillInfo.onDamagedEnterEventList.Count; i++) 
		{
			SkillContentPool.ExecuteOnDamagedEnterContent (damageInfo.skillInfo.onDamagedEnterEventList [i], damageInfo);
		}

		if (targetActor.actorInfo.GetLife () <= 0f)
		{
			fsm.TryTransferAction<DeadState> (damageInfo);
		}

		PlayEffectAnimation ("None");
		fsm.serviceInstance.ClearSkillBuffer ();
	}
	public void UpdateByPlayer ()
	{
		CommonUpdate ();
	}
	public override void CommonUpdate ()
	{
		if (targetActor.actorInfo.isGrounded)
			knockoutTimer += Time.deltaTime;
		for (int i = 0; i < damageInfo.skillInfo.onDamagedUpdateEventList.Count; i++) 
		{
			SkillContentPool.ExecuteOnDamagedUpdateContent (damageInfo.skillInfo.onDamagedUpdateEventList [i], damageInfo);
		}
	}
	public override void CommonExit ()
	{
		for (int i = 0; i < damageInfo.skillInfo.onDamagedExitEventList.Count; i++) 
		{
			SkillContentPool.ExecuteOnDamagedExitContent (damageInfo.skillInfo.onDamagedExitEventList [i], damageInfo);
		}
		targetActor.SetMoveable (true);
	}

	#endregion
}
