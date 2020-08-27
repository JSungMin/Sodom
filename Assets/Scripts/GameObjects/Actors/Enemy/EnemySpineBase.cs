using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;
using BossSpace.CommonSubInfo;

public class EnemySpineBase : SpineBaseActor {
	[HideInInspector]
	public Player player;
	public EnemyInfo enemyInfo;
	public DisInfo disInfo;

	//	공격 패턴
	//	Increase Case of PatternIndex
	//	--AttackState Exit
	//	--Combo Attack이 Parry되서 강제로 다음 공격으로 넘어갈때
	public int patternIndex = 0;
	public int parriedCount = 0;

	//	Parrying Timing Alarm Effect
	protected Vector3 ptEffectPos;
	public ParticleSystem ptEffect;

	public EnemyPatternInfoData patternData;
	[System.NonSerialized]
	public EnemyAttackPatternInfo nowPattern = null;
	public List<EnemyAttackPatternInfo> patternBuffer = new List<EnemyAttackPatternInfo> ();

	protected new void Start () {
		serviceInstance = GameSystemService.Instance;
		base.Start ();
		RaiseActorCollisionEnter += HandleGroundEnter;
		RaiseActorCollisionStay += HandleGroundStay;
		RaiseActorCollisionExit += HandleGroundExit;
		RaiseActorLand += HandleEnemyLand;
		RaiseActorAir += HandleEnemyAir;
		ptEffect = Resources.Load<ParticleSystem> ("Prefabs/Objects/Alarm_Effect/Alarm_" + actorInfo.actor_name);
	}
	public void HandleGroundEnter (object sender, ActorCollisionEventArg arg)
	{
		if (arg.col.collider.CompareTag ("Ground"))
		{
			if (!actorInfo.isGrounded) 
			{
				OnActorLand (this, arg.col.collider);
				actorInfo.isGrounded = true;
			} 
			else
			{
				actorInfo.isGrounded = true;
			}
		}
	}

	public void HandleGroundStay (object sender, ActorCollisionEventArg arg)
	{
		if (arg.col.collider.CompareTag ("Ground"))
		{
			if (arg.col.collider.bounds.max.y > bodyCollider.bounds.min.y + 0.05f)
				return;
			if (!actorInfo.isGrounded) 
			{
				OnActorLand (this, arg.col.collider);
				actorInfo.isGrounded = true;
			} 
			else
			{
				actorInfo.isGrounded = true;
			}
		}
	}

	public void HandleGroundExit (object sender, ActorCollisionEventArg arg)
	{
		if (arg.col.collider.CompareTag ("Ground"))
		{
			if (actorInfo.isGrounded) 
			{
				actorInfo.isGrounded = false;
				OnActorAir (this);
			}
			else
			{
				actorInfo.isGrounded = false;
			}
		}
	}

	void HandleEnemyLand (object sender, ActorLandEventArg e)
	{
		if (fsm.nowState != fsm.GetState<AttackState>())
			SetMoveable (true);
	}

	public void HandleEnemyAir (object sender, ActorAirEvnetArg e)
	{
		SetMoveable (false);
	}
	// public virtual void HandleAttackEvent (int colliderIndex)
	// {
	// 	//List<Actor> victimList = new List<Actor> ();
	// 	attackColliderList [colliderIndex].Initialize (this, GetUsedSkill());
	// }
	public virtual void OnParriedInDirect (object sender, DamageInfo d){	}
	public virtual void OnParriedInCombo (object sender, DamageInfo d)
	{		
		parriedCount++;
	}
	public virtual void OnParriedInCombo2 (object sender, DamageInfo d){	}
	public override SkillInfo GetUsedSkill ()
	{
		return skillCoolTimer.GetSkill (recentlyUsedSkillName);
	}
	public virtual void Update()
	{
		if (animator != null)
			animPlaybackTime = animator.state.GetCurrent(trackIndex).AnimationTime / animator.state.GetCurrent(trackIndex).Animation.Duration;
	}
}
