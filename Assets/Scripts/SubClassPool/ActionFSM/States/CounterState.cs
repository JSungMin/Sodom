using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

//	피해도 50프로 적용은 ApplyDamage Func에 flag_damage_counted를 이용해 구현됨
//	TODO : Actor 무적 상태
//	TODO : 카운터 스킬 범위 내 모든 적들에게 넉백 효과를 줍니다.
//	TODO : 카운터에 성공하면 모든 스킬을 1회에 한해 준비동작 없이 사용 할 수 있다.
[System.Serializable]
public class CounterState : ActionState {
	//	Editted On TransferToCounterState
	public DamageInfo damageInfo;
	public EnemyAttackPatternType enemyPatternType;
	public bool isCountered = false;

	public string[] counterSkillList = new string[3];

	public void EditStateInfo (DamageInfo damageInfo)
	{
		this.damageInfo = damageInfo;
		//this.stateInfo.animName = damageInfo.skillInfo.animName;
	}

	#region implemented abstract members of ActionState

	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		Debug.Log ("Try Transfer To Counter");
		var tmpDamageInfo = infoParam as DamageInfo;
	
		var enemy = tmpDamageInfo.attacker as EnemySpineBase;
		var enemyPattern = enemy.nowPattern;
		enemyPatternType = enemy.nowPattern.patternType;

		if (Mathf.Sign(fsm.transform.localScale.x) != Mathf.Sign(enemy.transform.localScale.x)) {
			Debug.Log ("방향이 안 맞음");
			return false;
		}
		else if (!targetActor.attackColliderList [0].isInitialize) {
			Debug.Log ("Player Attack Collider Is Not Initialzied");
			return false;
		}
		isCountered = false;
		if (enemyPatternType == EnemyAttackPatternType.DIRECT)
		{
			enemy.OnParriedInDirect (damageInfo.attacker, tmpDamageInfo);
			Debug.Log ("Counter : DIRECT");
			isCountered = true;
		} 
		else if (enemyPatternType == EnemyAttackPatternType.COMBO) 
		{
			enemy.OnParriedInCombo (damageInfo.attacker, tmpDamageInfo);
			if (enemyPattern.skillBuffer.Count == enemy.parriedCount)
			{
				Debug.Log ("Counter : COMBO");
				isCountered = true;
			}
		} 
		else if (enemyPatternType == EnemyAttackPatternType.COMBO2) 
		{
			enemy.OnParriedInCombo2 (damageInfo.attacker, tmpDamageInfo);
			if (enemy.patternIndex == enemyPattern.skillBuffer.Count) 
			{
				Debug.Log ("Counter : COMBO2");
				isCountered = true;
			}
		}		
		return true;
	}
	public override bool CommonCheckExit ()
	{
		if (isAnimationEnd)
			return true;
		return false;
	}

	public override void CommonEnter ()
	{
		targetActor.OnActorCounter (damageInfo);
		isAnimationEnd = false;
		stateInfo.animName = targetActor.GetNowAnimationName ();
		//	SkillCounterType : NONE = 0, UPPER = 1, MIDDLE = 2, LOWER = 3
		//	따라서 UPPER, MIDDLE, LOWER Animation 이름이 저장된 CounterAnimList를 불러올 때 -1 필요
		fsm.serviceInstance.ClearSkillBuffer ();
		targetActor.SetMoveable (false);

		if (isCountered) 
		{
			damageInfo.attacker.fsm.TryTransferAction<StunState> (damageInfo);
			damageInfo.attacker.actorInfo.isStunByParry = true;
			//fsm.serviceInstance.AddActorCondition (targetActor, ActorConditionType.UNBEATABLE, 0.5f, 0f);
			//fsm.serviceInstance.SlowMotionCamera (0.25f, 0.2f);
			//fsm.serviceInstance.StopFrame (0.15f);
			fsm.serviceInstance.ZoomMotionCamera (0f, 0.3f);
		}
	}

	public override void CommonUpdate ()
	{
		
	}
	public override void CommonExit ()
	{
		targetActor.SetMoveable (true);
		isAnimationEnd = false;
		fsm.serviceInstance.ClearSkillBuffer ();
		var skillInfo = damageInfo.skillInfo;
		var counterSkill =  counterSkillList [(int)skillInfo.skillCounterType - 1];
		targetActor.skillCoolTimer.ResetSkill(counterSkill);
		//fsm.serviceInstance.AddBufferSkill (targetActor.GetLearndSkill (counterSkill));
		//fsm.serviceInstance.StopSlowMotionCamera ();
		if (isCountered) 
		{
			isCountered = false;
		}
	}

	#endregion
}
