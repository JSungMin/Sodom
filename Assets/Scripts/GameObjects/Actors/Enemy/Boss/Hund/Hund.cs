using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;
using Spine.Unity;
using Spine.Unity.Modules;
using BossSpace.CommonSubInfo;
using BossSpace.HundSpace.SubInfo;


[RequireComponent(typeof(HundActionFSM))]
public class Hund : EnemySpineBase {
	[HideInInspector]
	public HundActionFSM hundFsm;

	private float defaultMoveSpeed;
	public AnimationCurve walkSpeedCurve;
	public Color directWarnColor = new Color(0.404f, 0f, 0f, 0f);
	public Color comboWarnColor = new Color(0.514f, 0.05f, 0.714f, 0f);
	public Color noneParryWarnColor = new Color(0f, 0.635f, 0.69f, 0f);
	public Material mat;

	public SkeletonGhost ghosting;
	public List<ProjectileLauncher> projectile;
    [HideInInspector]
    public HundHideManager hideManager;

	//	상황 조건 변수
	public List<Transform> movePointList = new List<Transform> ();
	public SpeedInfo speedInfo;

	public int phaseIndex = 0; 
	public int disState = 0;
	private int prevDisState = 0;
	//	Need Attack Dirty Mask일 때 해당 버퍼에서 꺼내 씀
	public string delayAttackBuffer = "";

	//	Hund Flag Block
	private int m_biteAttackCount = 0;
	private int m_tailAttackCount = 0;
	private int m_damagedCount = 0;
	private int m_tackleNum = 0;
	public int m_shorkwaveNum = 0;
	private bool isPlayerGround;
	[HideInInspector]
	public bool isTurning = false;
	//	End of Hund Flag
	[HideInInspector]
	public float desireLookDir;

	//	Specific FSM State Cashing
	[HideInInspector]
	public IdleState idleState;
	[HideInInspector]
	public WalkState walkState;

	public Color deadColor;

	//	Damage Flicker : 데미지 받았을 때 유닛을 깜빡거리게 함
	public ColorFlicker damFlicker;
    // Use this for initialization
    public void OnDisable()
    {
        mat.SetColor("_Color", Color.white);
        mat.SetColor("_OverlayColor", new Color(1f, 1f, 1f, 0f));
        mat.SetFloat("_BlendAmount", 0f);
    }
    public new void Start () {
		base.Start ();
		base.SetLookDirection (lookDirection);
		mat.SetColor ("_Color", Color.white);
		mat.SetColor ("_OverlayColor", new Color(1f,1f,1f,0f));
        mat.SetFloat ("_BlendAmount", 0f);
		defaultMoveSpeed = GetMoveSpeed ();
		damFlicker.Initialize ();
		player = GameObject.FindObjectOfType<Player> ();
		hideManager = GameObject.FindObjectOfType<HundHideManager>();
		EnemyAIHelper.HundAIHelper.dirtyFlag = 0;
		EnemyAIHelper.HundAIHelper.prevDirtyFlag = 0;

		//	Specific State Cashing
		hundFsm = fsm.GetSpecifiedFSM<HundActionFSM>();
		idleState = hundFsm.idleState;
		walkState = hundFsm.walkState;

		//	Boss Pattern Build;
		patternData.BuildPattern (this);
		//	Actor Event Listener Initialize
		BuildActorEventListener ();
		//	FSM Event Listener Initialize
		BuildFSMEventListener ();
		//	Spine Event Listener Initialize
		BuildSpineEventListener ();

		ptEffectPos = Vector3.up;
		hideManager.Init();
	}
	public void FixedUpdate ()
	{
		//base.Update ();
		if (actorInfo.GetLifePercent () <= 0f)
			return;
		//	각종 Boss의 행동 선택에 필요한 Information 객체와 Variable들을 Update한다.
		OnStatusValueUpdate();
		//	Boss의 각종 Skill의 WarmupTimer, CoolingTimer를 조건에 맞춰 Update한다.
		OnSkillTimerUpdate();
		//	현 상황에 맞춰 Attack 이외의 행동을 선택한다. EX) Idle, Run
		OnSelectDefaultState();
		/*	스킬을 사용할지 말지 결정지으며, 단계가 ConsumeFiltering, CheckSkillUsable, Consume으로 나뉜다.
		 *	--ConsumeFiltering	: CheckSkillUsable, Consume 단계 이전으로, 이 단계에서 아래 활동을 모두 스킵 할 수 있다.
		 *	--CheckPatternUsable: Consume 이전에 해당 스킬을 사용하는 환경을 구체적으로 따지며, 만약 여기서 걸러지면 Skill을 Buffer에서 제거한다. 
		 *	--Consume			: Buffer에서 Skill을 소거하며 nowPattern에 현 Pattern을 대입한다.
		*/	
		OnSkillSelectProcess();
	}
	private void OnStatusValueUpdate()
	{
		//	Initialize 조건변수
		EnemyAIHelper.UpdateDistanceInfo(this);
		isPlayerGround = player.actorInfo.isGrounded;
		prevDisState = disState;
		if (EnemyAIHelper.DisInExShort(this))
			disState = 0;
		else if (EnemyAIHelper.DisInShort(this))
			disState = 1;
		else if (EnemyAIHelper.DisInMiddle(this))
			disState = 2;
		else if (EnemyAIHelper.DisInFar(this))
			disState = 3;
	}
	private void OnSkillTimerUpdate()
	{
		switch (phaseIndex) 
		{
			case 0:
			if (EnemyAIHelper.HundAIHelper.IsNormal||
				EnemyAIHelper.HundAIHelper.IsTurning
			) 
			{
				skillCoolTimer.CoolingAll ();
				if (disState == 0) 
				{
					WarmupPattern (HundPatternType.중단뜯기01);
					WarmupPattern (HundPatternType.하단뜯기02);
					WarmupPattern (HundPatternType.백스텝);
				}
				else if (disState == 1)
				{
					WarmupPattern (HundPatternType.꼬리치기_중단);
					WarmupPattern (HundPatternType.꼬리치기_상단);
					WarmupPattern (HundPatternType.중단뜯기01);
					WarmupPattern (HundPatternType.하단뜯기02);
					WarmupPattern (HundPatternType.추적_중단뜯기02);
					WarmupPattern (HundPatternType.추적_하단뜯기01);
				}
				else if (disState == 2)
				{
					WarmupPattern (HundPatternType.돌진);
					WarmupPattern (HundPatternType.꼬리치기_중단);
					WarmupPattern (HundPatternType.꼬리치기_상단);
					WarmupPattern (HundPatternType.추적_중단뜯기02);
					WarmupPattern (HundPatternType.추적_하단뜯기01);
				}
				else if (disState == 3)
				{
					WarmupPattern (HundPatternType.돌진);
					WarmupPattern (HundPatternType.추적_중단뜯기02);
					WarmupPattern (HundPatternType.추적_하단뜯기01);
				}
			}
			break;
			case 2:
			if (EnemyAIHelper.HundAIHelper.IsNormal||
				EnemyAIHelper.HundAIHelper.IsTurning
			) 
			{
				skillCoolTimer.CoolingAll ();
				if (disState == 0) 
				{
					WarmupPattern (HundPatternType.중단뜯기01);
					WarmupPattern (HundPatternType.하단뜯기02);
					WarmupPattern (HundPatternType.백스텝);
				}
				else if (disState == 1)
				{
					WarmupPattern (HundPatternType.꼬리치기_중단);
					WarmupPattern (HundPatternType.꼬리치기_상단);
					WarmupPattern (HundPatternType.중단뜯기01);
					WarmupPattern (HundPatternType.하단뜯기02);
					WarmupPattern (HundPatternType.추적_중단뜯기02);
					WarmupPattern (HundPatternType.추적_하단뜯기01);
				}
				else if (disState == 2)
				{
					WarmupPattern (HundPatternType.돌진);
					WarmupPattern (HundPatternType.꼬리치기_중단);
					WarmupPattern (HundPatternType.꼬리치기_상단);
					WarmupPattern (HundPatternType.추적_중단뜯기02);
					WarmupPattern (HundPatternType.추적_하단뜯기01);
				}
				else if (disState == 3)
				{
					WarmupPattern (HundPatternType.돌진);
					WarmupPattern (HundPatternType.추적_중단뜯기02);
					WarmupPattern (HundPatternType.추적_하단뜯기01);
				}
			}
			break;
			case 3:
			if ((EnemyAIHelper.HundAIHelper.IsNormal||
				EnemyAIHelper.HundAIHelper.IsTurning) &&
				hideManager.hidePhase == 0
			) 
			{
				skillCoolTimer.CoolingAll ();
				if (disState == 0) 
				{
					WarmupPattern (HundPatternType.중단뜯기01);
					WarmupPattern (HundPatternType.하단뜯기02);
					WarmupPattern (HundPatternType.백스텝);
				}
				else if (disState == 1)
				{
					WarmupPattern (HundPatternType.꼬리치기_중단);
					WarmupPattern (HundPatternType.꼬리치기_상단);
					WarmupPattern (HundPatternType.중단뜯기01);
					WarmupPattern (HundPatternType.하단뜯기02);
					WarmupPattern (HundPatternType.추적_중단뜯기02);
					WarmupPattern (HundPatternType.추적_하단뜯기01);
				}
				else if (disState == 2)
				{
					WarmupPattern (HundPatternType.돌진);
					WarmupPattern (HundPatternType.꼬리치기_중단);
					WarmupPattern (HundPatternType.꼬리치기_상단);
					WarmupPattern (HundPatternType.추적_중단뜯기02);
					WarmupPattern (HundPatternType.추적_하단뜯기01);
				}
				else if (disState == 3)
				{
					WarmupPattern (HundPatternType.돌진);
					WarmupPattern (HundPatternType.추적_중단뜯기02);
					WarmupPattern (HundPatternType.추적_하단뜯기01);
				}
			}
			break;
		}
	}
	private void OnStateChainBroken(object chainName)
	{
        patternBuffer.Clear();
		var tmpName = (string)chainName;
        if (tmpName == "EMP")
        {

        }
        else if (tmpName == "Chase01")
		{
			 //	Chase Attack이 끝나면 Chase Pipeline 종료
            ResetPattern(HundPatternType.중단뜯기02);
			EnemyAIHelper.HundAIHelper.ResetDirty();
		}
		else if (tmpName == "Chase02")
		{
            ResetPattern(HundPatternType.하단뜯기01);
			EnemyAIHelper.HundAIHelper.ResetDirty();
		}
		else if (tmpName == "Tackle")
		{
            tackleNum--;
            if (tackleNum > 0)
            {
                skillCoolTimer.MakeFullCharge("돌진");
                patternBuffer.Clear();
                AddPatternToBuffer(HundPatternType.돌진);
            }
			else
			{
				ResetPattern(HundPatternType.돌진);
				EnemyAIHelper.HundAIHelper.ResetDirty();
			}
		}
		else if (tmpName == "Backstep")
		{
			//  BackStep and Attack End
			if (disState < 3)
			{
				//	disState >= 3 이면 ChaseChain이 실행됨
				//	이때 DirtyFlag를 Reset하면 SkillSelect가 진행됨으로
				//	예외 처리를 해준다.
				EnemyAIHelper.HundAIHelper.ResetDirty();
			}
			ResetPattern(HundPatternType.백스텝);
		}
	}
	private void OnSelectDefaultState()
	{
		SetDesireDirection (disInfo.dirToPlayer);
		//	Check Hund is Idle
		if (EnemyAIHelper.HundAIHelper.IsNormal && !fsm.IsChainActivated())
		{
			SetLookDirection();
			if (EnemyAIHelper.HundAIHelper.IsTurning)
				return;
			if (prevDisState <= 1)
			{
				fsm.TryTransferAction<IdleState>();
			}
			else
			{
				SetMoveSpeed(speedInfo.walkSpeed);
				fsm.TryTransferAction<WalkState>();
			}
		}
	}
	private void OnSkillSelectProcess()
	{
		//	ConsumeFiltering
		if (EnemyAIHelper.IsPlayerDamaged (player))
			return;
		if (fsm.IsChainActivated())
			return;
		if (EnemyAIHelper.HundAIHelper.IsNormal)
		{
			//	CheckPatternUsable 및 Consume
			EnemyAIHelper.ConsumePatternBuffer(this, CheckPatternUsable);
		}
	}
	private void OnIncBiteCount()
	{	
		if (biteAttackCount >= 3 && disInfo.dirToPlayer == lookDirection && disState < 1)
		{
			biteAttackCount = 0;
			damagedCount = 0;
			AddPatternToBuffer(HundPatternType.백스텝);
		}
		else if (biteAttackCount >= 2 && phaseIndex == 2 && disState >= 1)
		{
			biteAttackCount = 0;
			AddPatternToBuffer(HundPatternType.돌진);
		}
	}
	private void OnIncTailCount()
	{

	}
	private void OnIncDamagedCount()
	{
		if (damagedCount >= 4 && disInfo.dirToPlayer == lookDirection && disState < 1)
		{
			damagedCount = 0;
			biteAttackCount = 0;
			AddPatternToBuffer(HundPatternType.백스텝);
		}
	}
	/*	Pattern을 Buffer에서 Consume하기 전 사용하려는 스킬에 따른 예외처리를 함
	 *	--만약 Return false일시 현재 사용하려던 스킬은 Buffer에서 삭제됨
	 * 	--때문에 이 스크립트 안에선 RemoveFromBuffer이런건 사용하지 말 것
	 *	--대신 ResetPattern을 통해 해당 Pattern이 지워지게 설정
	 */
	private bool CheckPatternUsable (object obj)
	{
		var pattern = obj as EnemyAttackPatternInfo;
		if (pattern.patternName == HundPatternType.중단뜯기01.ToString() ||
            pattern.patternName == HundPatternType.중단뜯기02.ToString() ||
            pattern.patternName == HundPatternType.중단뜯기03.ToString() ||
            pattern.patternName == HundPatternType.하단뜯기01.ToString() ||
            pattern.patternName == HundPatternType.하단뜯기02.ToString())
        {
            if (disState >= 2)
            {
                ResetPattern(HundPatternType.중단뜯기01);
                ResetPattern(HundPatternType.중단뜯기02);
                ResetPattern(HundPatternType.중단뜯기03);
                ResetPattern(HundPatternType.하단뜯기01);
                ResetPattern(HundPatternType.하단뜯기02);
                RemovePatternFromBuffer(HundPatternType.중단뜯기01);
                RemovePatternFromBuffer(HundPatternType.중단뜯기02);
                RemovePatternFromBuffer(HundPatternType.중단뜯기03);
                RemovePatternFromBuffer(HundPatternType.하단뜯기01);
                RemovePatternFromBuffer(HundPatternType.하단뜯기02);
                WarmupPattern(HundPatternType.꼬리치기_중단);
                WarmupPattern(HundPatternType.꼬리치기_상단);
                return false;
            }
        }
        else if(pattern.patternName == HundPatternType.꼬리치기_중단.ToString())
		{	
			if (disState > 2)
			{
				ResetPattern (HundPatternType.꼬리치기_중단);
				RemovePatternFromBuffer (HundPatternType.꼬리치기_중단);
				WarmupPattern (HundPatternType.추적_하단뜯기01);
				return false;
			}
		}
		else if(pattern.patternName == HundPatternType.꼬리치기_상단.ToString())
		{	
			if (disState > 2)
			{
				ResetPattern (HundPatternType.꼬리치기_상단);
				RemovePatternFromBuffer (HundPatternType.꼬리치기_상단);
				WarmupPattern (HundPatternType.추적_하단뜯기01);
				return false;
			}
		}
		else if (pattern.patternName == HundPatternType.쇼크웨이브.ToString())
		{
            nowPattern = pattern;
            skillCoolTimer.MakeFullCharge("쇼크웨이브");
            hundFsm.attackState.EditStateInfo(EnemyAIHelper.GetSkillInfo("쇼크웨이브",this));
            hundFsm.StartStateChain("EMP", new List<ActionState>(){
                hundFsm.empState,
                hundFsm.attackState,
                hundFsm.empEndState
            });
            return false;
		}
		else if (pattern.patternName == HundPatternType.추적_중단뜯기02.ToString())
		{
			nowPattern = pattern;
			skillCoolTimer.MakeFullCharge("중단뜯기02");
			hundFsm.attackState.EditStateInfo (EnemyAIHelper.GetSkillInfo("중단뜯기02", this));
			hundFsm.StartStateChain("Chase01",new List<ActionState>(){
				hundFsm.chaseState,
				hundFsm.attackState
			});
			return false;
		}
		else if (pattern.patternName == HundPatternType.추적_하단뜯기01.ToString())
		{
			//delayAttackBuffer = HundPatternType.추적_하단뜯기01.ToString();
			nowPattern = pattern;
			hundFsm.attackState.EditStateInfo (EnemyAIHelper.GetSkillInfo("하단뜯기01", this));
			hundFsm.StartStateChain("Chase02",new List<ActionState>(){
				hundFsm.chaseState,
				hundFsm.attackState
			});
			//hundFsm.TryTransferAction<HundChaseState>();
			return false;
		}
		else if (pattern.patternName == HundPatternType.돌진.ToString())
		{
            if (tackleNum == 0)
			{
				if (phaseIndex == 0)
                	tackleNum = Random.Range(2,3);
				if (phaseIndex == 2)
                	tackleNum = Random.Range(3,4);
			}
			nowPattern = pattern;
			hundFsm.attackState.EditStateInfo (EnemyAIHelper.GetSkillInfo("돌진", this));
			hundFsm.StartStateChain("Tackle",new List<ActionState>(){
				hundFsm.tackleState,
				hundFsm.tackleSubState,
				hundFsm.attackState,
				hundFsm.stopState
			});
			return false;
		}
		else if (pattern.patternName == HundPatternType.백스텝.ToString())
		{
            //  delayAttackBuffer가 BackStepState에서 거리에 따라 지정됨
			//	AttackState의 SkillInfo는 BackStepState에서 결정됨
			nowPattern = pattern;
			hundFsm.StartStateChain("Backstep",new List<ActionState>(){
				hundFsm.backStepState,
				hundFsm.attackState
			});
			//hundFsm.TryTransferAction<HundBackStepState>();
			return false;
		}
		return pattern.CheckUseable(patternIndex);
	}
	public new void OnCollisionEnter (Collision col)
	{
        //rigid.velocity = Vector3.zero;
		//rigid.AddForce (col.relativeVelocity * 0.8f,ForceMode.Impulse);
	}
	public void OnStunEnter (object sender, ActorDamagedEventArg e)
	{
        hundFsm.BreakStateChain();
		serviceInstance.VibrateCameraByAttack (1f, 8f, 0.1f);
		HandleAttackEndEvent(activeACIndex);
		OnSkillEffectEnd ();
		rigid.velocity = Vector3.zero;
		Debug.Log ("On Enter Stun");
		nowPattern.ResetPattern ();
		nowPattern = null;
		patternIndex = 0;
		parriedCount = 0;
		if (actorInfo.GetLife () <= 0f)
		{
			//StopCoroutine (MadnessBehaviour.starter);
			//StartCoroutine (IChangeColor (deadColor, 0.35f));
		}
	}

	public override void OnParriedInDirect (object sender, DamageInfo d)
	{
        //serviceInstance.SlowMotionCamera(0.1f, 0.1f);
    }
	public override void OnParriedInCombo (object sender, DamageInfo d)
	{
		base.OnParriedInCombo (sender, d);
		OnSkillEffectEnd ();
		HandleAttackEndEvent (activeACIndex);
		Debug.Log ("Combo Attack Skill Used");
		serviceInstance.SlowMotionCamera (0.1f, 0.1f);
		if (!nowPattern.UseNextSkill (ref patternIndex)) 
		{
			patternIndex++;
		}
	}
	public override void OnParriedInCombo2 (object sender, DamageInfo d)
	{

	}

	public void OnPatternCompleted ()
	{
		Debug.Log ("Pattern Completed : " + nowPattern.patternName);
		delayAttackBuffer = "";
		patternIndex = 0;
		parriedCount = 0;
		nowPattern.ResetPattern();
		patternBuffer.Remove (nowPattern);
		nowPattern = null;
	}
	public void OnPhaseChanged(int to)
	{
		phaseIndex = to;
		if (to == 3)
		{
			hideManager.OnLightOff();
			hundFsm.attackState.OnEnter += delegate()
			{
				if (!hideManager.hundDisappearParticle.isPlaying)
					hideManager.hundDisappearParticle.Play();
				hideManager.hidePhase = 3;
				PlaySound (AudioType.HUND_APPEAR);
				GetComponent<MeshRenderer>().enabled = true;
				SetUnbeatable(false);
			};
			hundFsm.attackState.OnExit += delegate()
			{
				hideManager.hidePhase = 0;
				PlaySound (AudioType.HUND_HIDE);
				hideManager.hundDisappearParticle.Play();
				hideManager.hundDarkdustParticle.Stop();
				GetComponent<MeshRenderer>().enabled = false;
				SetUnbeatable(true);
			};
		}
	}
	//	Build Event Listener Block
	private void BuildActorEventListener ()
	{
		StateChainBroken += OnStateChainBroken;
		RaiseActorAttackSuccess += (object sender, ActorDamagedEventArg e) =>  {
			foreach (var ac in attackColliderList) 
			{
				ac.Release (player);
			}
		};
		RaiseActorDamaged += (object sender, ActorDamagedEventArg e) =>  {
			Debug.Log ("Hund Damaged : " + e.damageInfo.skillInfo.skillName);
			damagedCount++;
			if (actorInfo.isStunByParry &&
				e.damageInfo.skillInfo.skillCounterType == SkillCounterType.NONE) 
			{
				damFlicker.Initialize ();
				StartCoroutine (damFlicker.OnDamageFlick);
			}
			if (actorInfo.GetLife () <= 0f)
				StartCoroutine (IChangeColor (deadColor, 0.35f));
			else if (actorInfo.GetLifePercent() <= 0.75f && phaseIndex == 0)
			{
				skillCoolTimer.ResetSkill();
				patternBuffer.Clear();
				EnemyAIHelper.AddPatternToBuffer("쇼크웨이브", this);
			}
			else if (actorInfo.GetLifePercent() <= 0.25f && phaseIndex == 2)
			{
				OnPhaseChanged(3);
			}
		};
		RaiseActorStun += OnStunEnter;
		RaiseActorLand += delegate(object sender, ActorLandEventArg e) {
			rigid.velocity = Vector3.zero;
            Debug.Log("HUND LAND");
		};
		RaiseActorDead += (object sender, ActorDamagedEventArg e) => {
			damFlicker.damFlickTimer = damFlicker.damFlickDuration;
		};
	}
	private void BuildFSMEventListener ()
	{
		var idleState = hundFsm.idleState;
		idleState.OnEnter += delegate()
		{
			EnemyAIHelper.HundAIHelper.ResetDirty();
		};
		var walkState = hundFsm.walkState;
		walkState.OnEnter += delegate()
		{
			EnemyAIHelper.HundAIHelper.SetDirty(1);
		};
		walkState.OnExit += delegate()
		{
			EnemyAIHelper.HundAIHelper.ResetDirty(1);
		};
		walkState.OnUpdate = delegate () {
			var adjustVelocity = rigid.velocity;
			if (System.Single.IsNaN (animPlaybackTime))
				adjustVelocity.x = Mathf.Sign (lookDirection) * GetMoveSpeed () * walkSpeedCurve.Evaluate (0f);
			else
				adjustVelocity.x = Mathf.Sign (lookDirection) * GetMoveSpeed () * walkSpeedCurve.Evaluate (animPlaybackTime);
			rigid.velocity = adjustVelocity;
		};
		var tackleEnd = hundFsm.stopState;
		tackleEnd.OnEnter += delegate()
		{
			PlaySound(AudioType.HUND_TACKLE);
			for (int i = 0; i < attackColliderList.Count; i++)
			{
				attackColliderList[i].Release(player);
			}
			HandleAttackEndEvent(activeACIndex);
		};
		var attackState = hundFsm.attackState;
		//	Assign Enemy Attack Warning Color Func
		attackState.OnEnter += delegate () {
			EnemyAIHelper.HundAIHelper.IsAttacking = true;
			ghosting.ghostingEnabled = true;
			if (nowPattern == null)
			{
				var firstPattern = patternBuffer[0];
				if (firstPattern.patternType == EnemyAttackPatternType.DIRECT)
				{
					if (firstPattern.GetCounterType(patternIndex) == SkillCounterType.NONE)
						ghosting.color = noneParryWarnColor;
					else
						ghosting.color = directWarnColor;
				}
				else if (firstPattern.patternType == EnemyAttackPatternType.COMBO)
				{
					if (firstPattern.GetCounterType(patternIndex) == SkillCounterType.NONE)
						ghosting.color = noneParryWarnColor;
					else
						ghosting.color = comboWarnColor;
				}
			}
			else
			{
				if (nowPattern.patternType == EnemyAttackPatternType.DIRECT)
				{
					if (nowPattern.GetCounterType(patternIndex) == SkillCounterType.NONE)
						ghosting.color = noneParryWarnColor;
					else
						ghosting.color = directWarnColor;
				}
				else if (nowPattern.patternType == EnemyAttackPatternType.COMBO)
				{
					if (nowPattern.GetCounterType(patternIndex) == SkillCounterType.NONE)
						ghosting.color = noneParryWarnColor;
					else
						ghosting.color = comboWarnColor;
				}
			}
		};
		attackState.OnExit += delegate() 
		{
			EnemyAIHelper.HundAIHelper.IsAttacking = false;
			if (!actorInfo.isStunByParry)
				patternIndex++;
			ghosting.ghostingEnabled = false;
			if (GetUsedSkill().skillName == HundPatternType.쇼크웨이브.ToString())
			{
				
			}
			else
			{
				if (null == nowPattern)
				{
					if (patternIndex == patternBuffer[0].skillBuffer.Count)
					{
						OnPatternCompleted();
					}
					return;
				}
				if (patternIndex == nowPattern.skillBuffer.Count)
				{
					OnPatternCompleted();
				}
			}
			HandleAttackEndEvent(activeACIndex);
		};
		var stunState = fsm.GetState<StunState> ();
		stunState.OnEnter += delegate() {
			EnemyAIHelper.HundAIHelper.IsStunning = true;
			var bc = bodyCollider as BoxCollider;
			var prevCenter = bc.center;
			prevCenter.x = -1.2f;
			bc.center = prevCenter;
		};
		stunState.OnUpdate += delegate() 
		{
			if (stunState.knockoutTimer >= 2.3f)
			{
				PlayAnimation (0, "Wake", false, 1f);
			}
		};
		stunState.OnExit += delegate() {
			EnemyAIHelper.HundAIHelper.ResetDirty();
			var bc = bodyCollider as BoxCollider;
			var prevCenter = bc.center;
			prevCenter.x = 0.4144f;
			bc.center = prevCenter;
		};
		float timer = 0f;
		fsm.GetState<DeadState> ().OnUpdate += delegate() {
			
		};
	}
	private void BuildSpineEventListener ()
	{
		animator.state.Start += (Spine.TrackEntry trackEntry) => {
			if (trackEntry.Animation.Name == "Stop")
			{
				EnemyAIHelper.HundAIHelper.IsStopping = true;
			}
		};
		animator.state.End += (Spine.TrackEntry trackEntry) => {
			if(trackEntry.Animation.Name == "Stop")
			{
				EnemyAIHelper.HundAIHelper.IsStopping = false;
			}
		};
		animator.state.Complete += (Spine.TrackEntry trackEntry) => {
			if (trackEntry.Animation.Name == "Stop_Turn")
			{
				base.SetLookDirection (desireLookDir);
				EnemyAIHelper.HundAIHelper.IsTurning = false;
				isTurning = false;
			}
			else if(trackEntry.Animation.Name == "Stop")
			{
				EnemyAIHelper.HundAIHelper.IsStopping = false;
			}
		};
		animator.state.Event += (Spine.TrackEntry trackEntry, Spine.Event e) =>  {
			if (e.Data.Name == "Effect_Start") 
			{
				OnSkillEffectStart ();
			}
			else if (e.Data.Name == "Effect_End") 
			{
				OnSkillEffectEnd ();
			}
			else if (e.Data.Name == "Hit_Start") 
			{
				OnSkillHitStart ();
			}
			else if (e.Data.Name == "Hit_End") 
			{
				OnSkillHitEnd ();
			}
			else if (e.Data.Name == "Turn")
			{
                isTurning = false;
				SetMoveable(true);
			}
			else if (e.Data.Name == "Alarm_Start")
			{
				var newAlarm = GameObject.Instantiate (ptEffect, transform.position + ptEffectPos , Quaternion.identity);
				newAlarm.transform.parent = transform;
			}
		};
	}
	//	End of EventListener Block

	/*	Spine Event Handler Block
	 *	Spine의 Track에 발생하는 Event를 Handle하는 Function들이다.
	 *	--OnSkillEffectStart	:  
	 * 	--OnSkillEffectEnd
	 * 	--OnSkillHitStart
	 * 	--OnSkillHitEnd
	 */
	private void OnSkillEffectStart ()
	{

	}
	private void OnSkillEffectEnd ()
	{
		if (CheckUsedSkill(HundPatternType.하단뜯기01))
		{

		}
		else if (CheckUsedSkill(HundPatternType.상단뜯기))
		{
			serviceInstance.VibrateCameraByAttack (0.4f, 8f, 0.1f);
		}
		ghosting.ghostingEnabled = false;
	}
	private void OnSkillHitStart ()
	{
		ghosting.ghostingEnabled = false;
		if (CheckUsedSkill (HundPatternType.상단뜯기)) 
		{
			biteAttackCount++;
			rigid.velocity = Vector3.zero;
			SkillContentPool.ForwardDash (this, GetUsedSkill ());
			activeACIndex = 0;
		}
		else if (CheckUsedSkill (HundPatternType.투사체_준비))
		{

		}
		else if (CheckUsedSkill (HundPatternType.투사체_상단)) 
		{
			activeACIndex = -1;
			projectile [0].OnLaunch (GetUsedSkill ().skillName, true);
		} 
		else if (CheckUsedSkill (HundPatternType.투사체_하단)) 
		{
			activeACIndex = -1;
			projectile [0].OnLaunch (GetUsedSkill ().skillName, true);
		} 
		else if (CheckUsedSkill (HundPatternType.돌진)) 
		{
			activeACIndex = 2;
			SkillContentPool.ForwardDash (this, GetUsedSkill ());
		} 
		else if (CheckUsedSkill (HundPatternType.급습)) 
		{
			activeACIndex = 0;
		} 
		else if (CheckUsedSkill (HundPatternType.중단뜯기01)) 
		{
			biteAttackCount++;
			rigid.velocity = Vector3.zero;
			SkillContentPool.ForwardDash (this, GetUsedSkill ());
			activeACIndex = 0;
		} 
		else if (CheckUsedSkill (HundPatternType.중단뜯기02)) 
		{
			biteAttackCount++;
			rigid.velocity = Vector3.zero;
			SkillContentPool.ForwardDash (this, GetUsedSkill ());
			activeACIndex = 0;
		} 
		else if (CheckUsedSkill (HundPatternType.중단뜯기03)) 
		{
			biteAttackCount++;
			rigid.velocity = Vector3.zero;
			SkillContentPool.ForwardDash (this, GetUsedSkill ());
			activeACIndex = 0;
		} 
		else if (CheckUsedSkill (HundPatternType.하단뜯기01)) 
		{
			biteAttackCount++;
			rigid.velocity = Vector3.zero;
			SkillContentPool.ForwardDash (this, GetUsedSkill ());
			activeACIndex = 0;
		}
		else if (CheckUsedSkill (HundPatternType.하단뜯기02))
		{
			biteAttackCount++;
			rigid.velocity = Vector3.zero;
			SkillContentPool.ForwardDash (this, GetUsedSkill ());
			activeACIndex = 0;
		}
		else if (CheckUsedSkill (HundPatternType.꼬리치기_중단)) 
		{
			activeACIndex = 1;
		}
		else if (CheckUsedSkill (HundPatternType.꼬리치기_상단))
		{
			activeACIndex = 1;
		}
		else if (CheckUsedSkill (HundPatternType.쇼크웨이브)) 
		{
            hideManager.OnLightOff();
            activeACIndex = -1;
		}
		else if (CheckUsedSkill (HundPatternType.중단_하단_중단뜯기))
		{
			activeACIndex = 0;
		}
		if (activeACIndex != -1)
			HandleAttackEvent (activeACIndex);
	}
	private void OnSkillHitEnd ()
	{
		if (CheckUsedSkill(HundPatternType.급습))
		{
			for (int i = 0; i < projectile.Count; i++)
			{
				projectile[i].OnLaunch("미사일발사", true);
			}
		}
		if (activeACIndex != -1)
			HandleAttackEndEvent (activeACIndex);
	}
	//	End of Spine Event Handler Block

	/*	Enemy AI Helper Wrapping Block
	 *	--AddPatternToBuffer
	 *	--RemovePatternFromBuffer
	 * 	--CheckUsedSkill
	 * 	--WarmupPattern
	 * 	--ResetPattern
	 */
	private void AddPatternToBuffer(HundPatternType skillName)
	{
		EnemyAIHelper.HundAIHelper.AddPatternToBuffer (skillName, this);
	}
	private void RemovePatternFromBuffer(HundPatternType skillName)
	{
		EnemyAIHelper.HundAIHelper.RemovePatternFromBuffer (skillName, this);
	}
	private bool CheckUsedSkill(HundPatternType skillName)
	{
		return EnemyAIHelper.HundAIHelper.CheckUsedSkill (skillName, this);
	}
	private void WarmupPattern(HundPatternType skillName)
	{
		EnemyAIHelper.HundAIHelper.WarmupPattern (skillName, this);
	}
	private void ResetPattern(HundPatternType skillName)
	{
		EnemyAIHelper.HundAIHelper.ResetPattern (skillName, this);
	}

	//	조건변수 Help Func Block
	public void SetLookDirection ()
	{
		if (desireLookDir != lookDirection) 
		{
			PlayAnimation (0, "Stop_Turn", false, 1.25f, true);
			isTurning = true;
            EnemyAIHelper.HundAIHelper.IsTurning = true;
        } 
	}
	public void SetDesireDirection (float dir)
	{
		if (isTurning)
			return;
		if (EnemyAIHelper.IsAttackState(fsm))
			return;
		desireLookDir = dir;
	}
	private IEnumerator IChangeColor (Color color, float duration)
	{
		float timer = 0f;
		while (timer <= duration)
		{
			mat.SetColor ("_Color", Color.Lerp(mat.GetColor("_Color"), color , timer/duration));
			timer += Time.deltaTime;
			yield return null;
		}
	}
	//	End of Help Func Block
	//	
	public int biteAttackCount 
	{
		get{
			return m_biteAttackCount;
		}
		set{
			m_biteAttackCount = value;
			OnIncBiteCount();
		}
	}
	public int damagedCount
	{
		get{
			return m_damagedCount;
		}
		set{
			m_damagedCount = value;
			OnIncDamagedCount();
		}
	}
	public int tackleNum
	{
		get{
			return m_tackleNum;
		}
		set{
			m_tackleNum = value;
		}
	}
}