using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class StunState : ActionState {
	public float knockoutTime = 0.2f;
	public float knockoutTimer = 0f;
	public DamageInfo damageInfo;

	public void EditStateInfo (DamageInfo damageInfo)
	{
		this.damageInfo = damageInfo;
		//this.stateInfo.animName = targetActor.actorInfo.actor_name + "_Damaged";
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
		var damagePos = targetActor.transform.position + Vector3.up * targetActor.GetComponent<Collider> ().bounds.max.y * 0.8f;
		var damageDir = targetActor.transform.position - damageInfo.attacker.transform.position;
		damageDir.Normalize ();
		if (damProb <= tmpBasicStat.quickness) {
			//	TODO : EVASION
			fsm.serviceInstance.PopEvasionText (damagePos, damageDir);
			//	stateInfo.anim_name = targetActor.actorInfo.actor_name + "_Evasion";
			//	TODO : Call 'MISS' UI
		} 
		else {
			if (damProb >= tmpBasicStat.combatExperience) {
				//	TODO : CRITIDCAL
				var damage = 0f;
				if (damageInfo.skillInfo.skillType == SkillType.NORMAL)
					damage = (damageInfo.skillInfo.damage * 15f - tmpBasicStat.physicalDefense);
				else
					if (damageInfo.skillInfo.skillType == SkillType.SPECIAL)
						damage = (damageInfo.skillInfo.damage * 15f - tmpBasicStat.specialDefense);

				damage = Mathf.Max (0f, damage);
				targetActor.actorInfo.SubLife(15);
				fsm.serviceInstance.PopCriticalText (damagePos, 15, damageDir);
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
				targetActor.actorInfo.SubLife (15);
				fsm.serviceInstance.PopDamageText (damagePos, 15, damageDir);
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

	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		var tmpDamageInfo = infoParam as DamageInfo;
		if (null == tmpDamageInfo)
			return false;
		return true;
	}
	public override bool CommonCheckExit ()
	{
		if (knockoutTimer <= knockoutTime)
			return false;
		return true;
	}
	public override void CommonEnter ()
	{

		//	TODO : CommonEnter Enemy로 분류 필요
		//targetActor.skillCoolTimer.ResetSkill();
		/*------Parring Damage 적용하려면 주석을 해제------
		var tmpBasicStat = targetActor.actorInfo.GetTotalBasicInfo ();
		var damProb = 0f;
		var conProb = 0f;
		//	화상, 빙결과 같은 상태이상을 검사하고 크리티컬을 터트리게 확률을 지정.
		//ApplyElementCondition (ref damProb);
		//	회피 및 크리티컬 데미지 적용
		//ApplyDamage (tmpBasicStat, damProb);
		//	상태 이상 적용
		//AddSkillConditions (tmpBasicStat, ref conProb);
		---------------------------------------------------*/
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
		targetActor.SetMoveable (false);
		targetActor.ResetRecycleTimer ();

		//knockoutTime = damageInfo.victim.GetUsedSkill().knockoutTime;
		knockoutTimer = 0f;
		damageInfo.attacker.OnActorAttackSuccess (damageInfo);
		targetActor.OnActorStun (damageInfo);
	}
	public override void CommonUpdate ()
	{
		if (targetActor.actorInfo.isGrounded)
			knockoutTimer += Time.deltaTime;
	}
	public override void CommonExit ()
	{
		base.CommonExit ();
		targetActor.SetMoveable (true);

		targetActor.actorInfo.isStunByParry = false;
	}
}
