using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;

public class Actor : MonoBehaviour {
    protected static GameSystemService serviceInstance;

	public ActorInfo actorInfo;
	public AnimationType animatorType;
    public int activeACIndex = -1;
    public List<AttackCollider> attackColliderList = new List<AttackCollider> ();
	[HideInInspector]
	//public ActorActionFSM fsm;
	public ActorActionFSM fsm;
	[HideInInspector]
	public Rigidbody rigid;
	public Collider bodyCollider;
	public LayerMask collisionMask;
	//	Outter Flag Property 하이어라키에서 수정 가능한 FLAG
	public bool useRecycle = true;
	//	Inner Flag Property 내부 함수를 통해서만 수정 가능한 FLAG
	protected bool canMoveControl = true;
	protected bool canRecycle = true;

	protected float recycleTimer = 0f;

	public float lookDirection;
	[SerializeField]
	protected string recentlyUsedSkillName;

	public float animPlaybackTime = 0f;

	public SkillCoolTimer skillCoolTimer;

	public event EventHandler<ActorCollisionEventArg>	RaiseActorCollisionEnter;
	public event EventHandler<ActorCollisionEventArg>	RaiseActorCollisionStay;
	public event EventHandler<ActorCollisionEventArg>	RaiseActorCollisionExit;

	public event EventHandler<ActorWalkEventArg> 		RaiseActorWalk;
	public event EventHandler<ActorJumpEventArg> 		RaiseActorJump;
	public event EventHandler<ActorRollingEventArg>		RaiseActorRolling;
	public event EventHandler<ActorAirEvnetArg> 		RaiseActorAir;
	public event EventHandler<ActorLandEventArg> 		RaiseActorLand;
	public event EventHandler<ActorSitEventArg> 		RaiseActorSit;

	public event EventHandler<ActorAttackEventArg> 		RaiseActorAttackTry;
	public event EventHandler<ActorDamagedEventArg> 	RaiseActorAttackSuccess;
	public event EventHandler<ActorDamagedEventArg> 	RaiseActorKill;
	public event EventHandler<ActorDamagedEventArg> 	RaiseActorDamaged;
	public event EventHandler<ActorDamagedEventArg> 	RaiseActorStun;
	public event EventHandler<ActorDamagedEventArg> 	RaiseActorCounter;
	public event EventHandler<ActorDamagedEventArg> 	RaiseActorDead;
	public event EventHandler<ActorEquipEventArg>		RaiseActorEquip;
	public event EventHandler<ActorRootEventArg>		RaiseActorRoot;
	public event EventHandler<ActorRootEventArg> 		RaiseActorDrop;
	public event EventHandler<ActorInteractEventArg>	RaiseActorInteract;

	public DelegatePool.VoidOneParam StateChainBroken;

	public virtual void ActorInit ()
	{
        serviceInstance = GameObject.FindObjectOfType<GameSystemService>();
        if (null == serviceInstance)
            serviceInstance = GameSystemService.Instance;
		fsm = GetComponent<ActorActionFSM> ();
		rigid = GetComponent<Rigidbody> ();
		if (null == bodyCollider)
			bodyCollider = GetComponent<Collider> ();
		//	ActorInfo Init
		//		Pooling Condition		:	Condition Applier를 Pooling 해 놓음
		//		Apply Stat 				:	Inspector에서 정의된 Basic Stat을 따라 Detail Stat을 적용함
		//		SyncSkillMapWithList	:	Actor의 learnSkillMap에 미리 정의된 learnSkillList의 값들을 정의함
        actorInfo.Init(this);
		//	FSM을 Initialize함
		//		BuildStates				:	사용되는 State를 생성함
		//		BuildStatePool			:	생성한 State를 List에 채워넣음
		//		BuildTransitionGraph	:	각 State 간의 Transition Graph를 구성
		//fsm.InitFSMStates ();
		fsm.InitFSMStates ();
		//attackColliderList = new List<AttackCollider>(gameObject.GetComponentsInChildren<AttackCollider> (true));

		if (useRecycle)
			StartCoroutine ("IRecycleEnergy");
		skillCoolTimer.Initialize (this);
	}
	public void Start()
	{
		ActorInit ();
	}

	protected IEnumerator IRecycleEnergy()
	{
		canRecycle = true;
		recycleTimer = 0f;
		while (actorInfo.GetLife() > 0f)
		{
			recycleTimer += Time.deltaTime;
			if (recycleTimer >= 5f)
			{
				actorInfo.AddLife (actorInfo.GetTotalBasicInfo ().recyclablityLife);
				actorInfo.AddEnergy (actorInfo.GetTotalBasicInfo ().recyclablityEnergy);
				recycleTimer = 0f;
			}
			yield return null;
		}
		canRecycle = false;
	}

	public void OnCollisionEnter (Collision col)
	{
		EventHandler<ActorCollisionEventArg> handler = RaiseActorCollisionEnter;
		if (null != handler) {
			handler (this, new ActorCollisionEventArg (col));
		}
	}
	public void OnCollisionStay (Collision col)
	{
		EventHandler<ActorCollisionEventArg> handler = RaiseActorCollisionStay;
		if (null != handler) {
			handler (this, new ActorCollisionEventArg (col));
		}
	}
	public void OnCollisionExit (Collision col)
	{
		EventHandler<ActorCollisionEventArg> handler = RaiseActorCollisionExit;
		if (null != handler) {
			handler (this, new ActorCollisionEventArg (col));
		}
	}
	public void OnActorLand (Actor actor, Collider plane)
	{
		EventHandler<ActorLandEventArg> handler = RaiseActorLand;
		if (null != handler) {
			handler (this, new ActorLandEventArg(actor, plane));
		}
	}
	public void OnActorAir (Actor actor)
	{
		EventHandler<ActorAirEvnetArg> handler = RaiseActorAir;
		if (null != handler) {
			handler (this, new ActorAirEvnetArg(actor));
		}
	}
	public bool OnActorAttackTry(SkillInfo usedSkill)
	{
		//bool result = fsm.TryTransferActionToAttack(usedSkill);
		bool result = fsm.TryTransferAction<AttackState> (usedSkill);
		EventHandler<ActorAttackEventArg> handler = RaiseActorAttackTry;
		if (null != handler) {
			//	TODO : 쿨 타임 시작 
			handler (this, new ActorAttackEventArg(this, usedSkill));
		}
		return result;
	}
	public bool OnActorAttackSuccess(DamageInfo damageInfo)
	{
		EventHandler<ActorDamagedEventArg> handler = RaiseActorAttackSuccess;
		if (null != handler) {
			handler (this, new ActorDamagedEventArg (damageInfo));
		}
		return true;
	}
	public bool OnActorStun (DamageInfo damageInfo)
	{
		EventHandler<ActorDamagedEventArg> handler = RaiseActorStun;
		if (null != handler) {
			handler (this, new ActorDamagedEventArg (damageInfo));
		}
		return true;
	}
	public bool OnActorCounter (DamageInfo damageInfo)
	{
		EventHandler<ActorDamagedEventArg> handler = RaiseActorCounter;
		if (null != handler) {
			handler (this, new ActorDamagedEventArg (damageInfo));
		}
		return true;
	}
	public bool OnActorKill(DamageInfo damageInfo)
	{
		EventHandler<ActorDamagedEventArg> handler = RaiseActorKill;
		if (null != handler) {
			handler (this, new ActorDamagedEventArg (damageInfo));
		}
		return true;
	}
	public bool OnActorDamaged (DamageInfo damageInfo)
	{
		bool result = fsm.TryTransferAction<DamagedState> (damageInfo);
		EventHandler<ActorDamagedEventArg> handler = damageInfo.victim.RaiseActorDamaged;
		if (null != handler) {
			//	TODO : Damage Label 표시
			handler (this, new ActorDamagedEventArg(damageInfo));
		}
		return result;
	}
	public bool OnActorDead (DamageInfo damageInfo)
	{
		bool result = fsm.TryTransferAction<DeadState> (damageInfo);
		EventHandler<ActorDamagedEventArg> handler = damageInfo.victim.RaiseActorDead;
		if (null != handler) {
			//	TODO : Damage Label 표시
			handler (this, new ActorDamagedEventArg(damageInfo));
		}
		return result;
	}
	public bool OnActorWalk ()
	{
		bool result = fsm.TryTransferAction<WalkState> ();
		EventHandler<ActorWalkEventArg> handler = RaiseActorWalk;
		if (null != handler)
			handler (this, new ActorWalkEventArg (this, lookDirection));
		return result;
	}
	public bool OnActorJump ()
	{
		bool result = fsm.TryTransferAction<JumpState> ();
		EventHandler<ActorJumpEventArg> handler = RaiseActorJump;
		if (null != handler)
			handler (this, new ActorJumpEventArg (this));
		return result;
	}
	public bool OnActorRolling ()
	{
		bool result = fsm.TryTransferAction<RollingState> ();
		if (result)
		{
			EventHandler<ActorRollingEventArg> handler = RaiseActorRolling;
			if (null != handler)
				handler (this, new ActorRollingEventArg (this));
		}
		return result;
	}
	public bool OnActorSit ()
	{
		bool result = fsm.TryTransferAction<SitState> ();
		EventHandler<ActorSitEventArg> handler = RaiseActorSit;
		if (null != handler)
			handler (this, new ActorSitEventArg (this));
		return result;
	}
	public bool OnActorEquip(ItemInfo equipItem)
	{
		EventHandler<ActorEquipEventArg> handler = RaiseActorEquip;
		if (null != handler)
			handler (this, new ActorEquipEventArg(this, equipItem));
		return true;
	}
	public bool OnActorRoot(ItemInfo rootItem)
	{
		EventHandler<ActorRootEventArg> handler = RaiseActorRoot;
		if (null != handler)
			handler (this, new ActorRootEventArg (this, rootItem));
		return true;
	}
	public bool OnActorDrop(ItemInfo rootItem)
	{
		EventHandler<ActorRootEventArg> handler = RaiseActorDrop;
		if (null != handler)
			handler (this, new ActorRootEventArg (this, rootItem));
		return true;
	}
	public bool OnActorInteract(Actor interactWith)
	{
		EventHandler<ActorInteractEventArg> handler = RaiseActorInteract;
		if (null != handler)
			handler (this, new ActorInteractEventArg (this, interactWith));
		return true;
	}
	public void PlaySound (AudioType audioType, bool overlap = false, float delay = 0f)
	{
		serviceInstance.PlaySound(audioType, overlap, delay);
	}
	public void StopSound (AudioType audioType)
	{
		serviceInstance.StopSound(audioType);
	}
	public string GetNowAnimationName ()
	{
		if (animatorType == AnimationType.FRAME) {
			return ((FrameBaseActor)(this)).curAnimName;
		} 
		return ((SpineBaseActor)(this)).curAnimName[0];
	}
	public string GetNowAnimationName (int index)
	{
		if (animatorType == AnimationType.FRAME) {
			return ((FrameBaseActor)(this)).curAnimName;
		} 
		return ((SpineBaseActor)(this)).curAnimName[index];
	}

	public int GetConditionOverlap(ActorConditionType t)
	{
		return actorInfo.conditionInfo.overlapInfoList [(int)t];
	}
	public bool HasCondition (ActorConditionType t)
	{
		if (GetConditionOverlap(t) >= 1)
		{
			return true;
		}
		return false;
	}

	protected void ResetAllConditions ()
	{
		foreach (var applierList in actorInfo.conditionInfo.applierPool) {
			foreach (var applier in applierList) {
				if (!applier.IsBusy)
					continue;
				//	활성화 되있는 Applier들만 제거
				actorInfo.conditionInfo.RemoveCondition (applier.conditionType, applier);
			}
		}
	}
	public void LearnSkill(string skillName)
	{
		actorInfo.LearnSkill (skillName);
	}
	public SkillInfo GetLearndSkill (string skillName)
	{
		return actorInfo.GetLearnedSkill (skillName);
	}
	public bool IsActorLearnedSkill(string skillName)
	{
		return actorInfo.IsActorLearnedSkill (skillName);
	}

	public void SetMoveable(bool val)
	{
		canMoveControl = val;
	}
	public bool GetMoveable ()
	{
		return canMoveControl;
	}
	public void SetUnbeatable(bool val)
	{
		if (val)
			actorInfo.conditionInfo.AddCondition (ActorConditionType.UNBEATABLE, 1f, 100f);
		else {
			if (actorInfo.conditionInfo.CheckOverlapValidation(ActorConditionType.UNBEATABLE))
				actorInfo.conditionInfo.RemoveCondition (ActorConditionType.UNBEATABLE);
		}
	}
	public void SetUnbeatable(bool val, float duration)
	{
		if (val)
			actorInfo.conditionInfo.AddCondition (ActorConditionType.UNBEATABLE, duration, 100f);
		else
			actorInfo.conditionInfo.RemoveCondition (ActorConditionType.UNBEATABLE);
	}
	public bool GetUnbeatable()
	{
		return HasCondition (ActorConditionType.UNBEATABLE);
	}
	public void SetRecycleable(bool val)
	{
		if (canRecycle && !val) {	
			StopCoroutine ("IRecycleEnergy");
		}
		else if (!canRecycle && val)
		{
			StartCoroutine ("IRecycleEnergy");
		}
		canRecycle = val;
	}
	public void ResetRecycleTimer ()
	{
		recycleTimer = 0f;	
	}
	public void SetMoveSpeed (float speed)
	{
		actorInfo.GetTotalBasicInfo().moveSpeed = speed;
	}
	public float GetMoveSpeed()
	{
		return actorInfo.GetTotalBasicInfo().moveSpeed;
	}
	public float GetJumpAmount()
	{
		return actorInfo.GetTotalBasicInfo ().jumpSpeed;
	}
	public virtual void SetLookDirection (float dir)
	{
		lookDirection = dir;
		var prevScale = transform.localScale;
		prevScale.x = Mathf.Abs(transform.localScale.x) * lookDirection;
		transform.localScale = prevScale;
	}
	public float GetLookDirection ()
	{
		return lookDirection;
		//return Mathf.Sign (-transform.localScale.x);
	}
	public virtual void SetUsedSkill (string skillName)
	{
		recentlyUsedSkillName = skillName;
	}
	public virtual SkillInfo GetUsedSkill()
	{
		return serviceInstance.GetPlayerSkillInfoByName (recentlyUsedSkillName);
	}
	public virtual void HandleAttackEvent (int colliderIndex)
	{
        activeACIndex = colliderIndex;
		//List<Actor> victimList = new List<Actor> ();
		var usedSkill = GetUsedSkill();
		PlaySound(usedSkill.useSfxType);
		attackColliderList [colliderIndex].Initialize (this, usedSkill);
	}
	public virtual void HandleAttackEndEvent (int colliderIndex)
	{
		if (-1 == colliderIndex)
			return;
        var actors = serviceInstance.inGameActorList;
        foreach (var a in actors)
        {
            if (a.CompareTag(this.gameObject.tag))
            {
                continue;
            }
            attackColliderList[colliderIndex].Release(a);
        }
        activeACIndex = -1;
        //attackColliderList [colliderIndex].Release (this);
    }
	public void UseAttackOnEnterSkillContent (string contentName)
	{
		SkillContentPool.ExecuteOnAttackEnterContent (contentName, this, GetUsedSkill());
	}
	public void UseAttackOnUpdateSkillContent (string contentName)
	{
		SkillContentPool.ExecuteOnAttackUpdateContent (contentName, this, GetUsedSkill());
	}
	public void UseAttackOnExitSkillContent (string contentName)
	{
		SkillContentPool.ExecuteOnAttackExitContent (contentName, this, GetUsedSkill());
	}
}
