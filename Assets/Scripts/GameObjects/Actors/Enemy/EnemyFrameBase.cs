using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;
using BossSpace.CommonSubInfo;

[RequireComponent(typeof(ActorActionFSM))]
public class EnemyFrameBase : FrameBaseActor {
	[HideInInspector]
	public Player player;
	public EnemyInfo enemyInfo;
	public DisInfo disInfo;

	//	공격 패턴
	public int patternIndex = 0;
	public EnemyPatternInfoData patternData;
	[System.NonSerialized]
	public EnemyAttackPatternInfo nowPattern = null;
	public List<EnemyAttackPatternInfo> patternBuffer = new List<EnemyAttackPatternInfo> ();

	// Use this for initialization
	protected new void Start () {
		base.Start ();
		RaiseActorCollisionEnter += HandleGroundEnter;
		RaiseActorCollisionStay += HandleGroundStay;
		RaiseActorCollisionExit += HandleGroundExit;
		RaiseActorLand += HandleEnemyLand;
		RaiseActorAir += HandleEnemyAir;
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
}
