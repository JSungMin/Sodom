using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;

public class ActorInteractManager {
	private GameSystemService serviceInstance;
	//	TODO : UI 

	public void Start()
	{
		serviceInstance = GameSystemService.Instance;
	}

	//	현재는 Actor to Actor만 구현되지만 이후, 
	//	Actor to Interactable Object,
	//	Interactable Object to Actor 도 구현 할 가능성 있음
	public bool OnActorAttackTry (Actor attacker, SkillInfo usedSkill)
	{
		//	TODO : Actor의 FSM State 변경시도 필요
		return attacker.OnActorAttackTry(usedSkill);
	}
	public bool OnActorAttackSuccess (DamageInfo damageInfo)
	{
		return damageInfo.attacker.OnActorAttackSuccess (damageInfo);
	}
	public bool OnActorDamaged (DamageInfo damageInfo)
	{
		return damageInfo.victim.OnActorDamaged (damageInfo);
	}
	public bool OnActorJump (Actor actor)
	{
		return actor.OnActorJump ();
	}
	public bool OnActorRolling (Actor actor)
	{
		return actor.OnActorRolling ();	
	}
	public bool OnActorWalk (Actor actor)
	{
		return actor.OnActorWalk ();
	}
	public bool OnActorSit (Actor actor)
	{
		return actor.OnActorSit ();
	}
}
