using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;
using Spine.Unity;
using Spine.Unity.Modules;
using BossSpace.StilettoSpace;
using BossSpace.StilettoSpace.SubInfo;
using BossSpace.StilettoSpace.Behaviour;

[RequireComponent(typeof(HundActionFSM))]
public class Stiletto : EnemySpineBase {

	private float defaultMoveSpeed;
	public AnimationCurve walkSpeedCurve;
	public Color directWarnColor = new Color(0.404f, 0f, 0f, 0f);
	public Color comboWarnColor = new Color(0.514f, 0.05f, 0.714f, 0f);
	public Color noneParryWarnColor = new Color(0f, 0.635f, 0.69f, 0f);
	public Material mat;

	public SkeletonGhost ghosting;
	public List<ProjectileLauncher> projectile;

	//	상황 조건 변수
	public AngerGauge angerInfo;
	public SpeedInfo speedInfo;
	public TwoPointMoveInfo tpMoveInfo;
	public BaldoInfo baldoInfo;
	public ShotGunBurstInfo shotBurstInfo;
	public BaldoBurstInfo baldoBurstInfo;

	public float mapMinX = 0.5f, mapMaxX = 19f;
	public bool comboAttackSuccessFlag = false;

	public int projectileCount = 0;

	private bool skipAttackAnim = false;
	public int phaseIndex = 0; 
	private bool isPlayerGround;
	[HideInInspector]
	public float desireLookDir;
	//	idx = 0 None
	//	idx = 1 Angry Motion - ing
	//	idx = 2 Baldo - ing
	public bool skipBehaviour = false;

	//	Specific FSM State Cashing
	private IdleState idleState;
	private WalkState walkState;

	public Color deadColor;

	//	Damage Flicker : 데미지 받았을 때 유닛을 깜빡거리게 함
	public ColorFlicker damFlicker;
	// Use this for initialization
	public new void Start () {
		base.Start ();
		base.SetLookDirection (lookDirection);
		mat.SetColor ("_Color", Color.white);
		mat.SetColor ("_OverlayColor", new Color(1f,1f,1f,0f));
		defaultMoveSpeed = GetMoveSpeed ();
		damFlicker.Initialize ();
		player = GameObject.FindObjectOfType<Player> ();

		//	Specific State Cashing
		idleState = fsm.GetState<IdleState>();
		walkState = fsm.GetState<WalkState>();

		//	Boss Pattern Build;
		patternData.BuildPattern (this);
		//	Actor Event Listener Initialize
		BuildActorEventListener ();
		//	FSM Event Listener Initialize
		BuildFSMEventListener ();
		//	Spine Event Listener Initialize
		BuildSpineEventListener ();

		angerInfo.OnEnterAngry += delegate() 
		{
			patternBuffer.Clear();
			actorInfo.basicStat.physicalDefense += 100f;
			fsm.GetState<IdleState>().EditStateInfo("Idle_Rage");
			AddPatternToBuffer(StilettoPatternType.분노);
			skipBehaviour = true;
		};
		angerInfo.OnExitAngry += delegate()
		{
			StartCoroutine ("IResetAngerGauge", 1f);
			fsm.GetState<IdleState>().EditStateInfo("Idle_Battle");
		};
		ptEffectPos = Vector3.up * 0.5f;
	}
	public override void Update ()
	{
		base.Update ();
		if (actorInfo.GetLifePercent () <= 0f)
			return;
		skillCoolTimer.CoolingAll ();

		//	Initialize 조건변수
		disInfo.dirToPlayer = Mathf.Sign (player.transform.position.x - transform.position.x);
		disInfo.disToPlayer = Vector3.Distance (transform.position, player.transform.position);

		isPlayerGround = player.actorInfo.isGrounded;
		var prevPhase = phaseIndex;
		//phaseIndex = (actorInfo.GetLifePercent() > 0.6f) ? 0 : 1;
		//	세부조건
		switch (phaseIndex) 
		{
		case 0:
			if (StilettoFlag.IsNormal) {
				OnNormalBehaviour01 ();
				if (!StilettoFlag.IsMoving) 
				{
					desireLookDir = disInfo.dirToPlayer;
				}
				if (GetMoveable () && patternIndex == 0) 
				{
					SetLookDirection (desireLookDir);
				}
			} else if (StilettoFlag.IsOnlyAngry) {
				if (!StilettoFlag.IsMoving) 
				{
					desireLookDir = disInfo.dirToPlayer;
				}
				OnAngryBehaviour ();
				if (GetMoveable ()) 
				{
					SetLookDirection (desireLookDir);
				}
			}
			break;
		}

		if (actorInfo.isStunByParry)
			return;
		if (StilettoFlag.IsMoving)
			return;
		//	Skill Buffer 해소
		if (nowPattern == null) 
		{
			if (patternBuffer.Count == 0)
				return;
			if (!CheckSkillUsable ())
				return;
			if (patternBuffer [0].UseSkill (ref patternIndex))
			{
				nowPattern = patternBuffer [0];
				Debug.Log ("Pattern Start : " + nowPattern.patternName);
			}
			else 
			{
				Debug.Log ("Pattern Deleted : " + patternBuffer[0].patternName);
				patternBuffer.RemoveAt (0);
			}
		} 
		else 
		{
			if (patternIndex != 0)
			{
				nowPattern.ReadyPattern (patternIndex);
				if (!CheckSkillUsable ())
					return;
			}
			if (!nowPattern.UseSkill (ref patternIndex)) {
				if (patternIndex == 0) {
					patternBuffer.Sort (delegate(EnemyAttackPatternInfo x, EnemyAttackPatternInfo y) {
						return (x.patternPriority > y.patternPriority) ? -1 : 1;	
					});
					patternBuffer.Remove (nowPattern);
				}
			}
		}
	}
	private void OnNormalBehaviour01 ()
	{
		var tmpDisState = -1;
		if (StilettoFlag.DisInExShort) 
		{
			tmpDisState = 0;
			WarmupPattern (StilettoPatternType.발도);
		}
		else if (StilettoFlag.DisInShort)
		{
			tmpDisState = 1;
			WarmupPattern (StilettoPatternType.발도);
			WarmupPattern (StilettoPatternType.샷건);
		}
		else if (StilettoFlag.DisInMiddle)
		{
			tmpDisState = 2;
			WarmupPattern (StilettoPatternType.샷건);
		}
		else if (StilettoFlag.DisInFar)
		{
			tmpDisState = 3;
			WarmupPattern (StilettoPatternType.샷건);
		}
		if (tmpDisState <= 1) 
		{
			tpMoveInfo.IncCoolTimer (Time.deltaTime * ((tmpDisState == 0) ? 2f : 1f));
			if (tpMoveInfo.CheckCoolTimer ()) 
			{
				var newPos = GetFarestWallPosition ();
				desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
				TwoPointMove.InitFactory (newPos, "Dash_Front");
				StartCoroutine (TwoPointMove.starter);
				return;
			}
		}
		if (patternBuffer.Count != 0)
			return;
		//	Select FSM State 
		if (disInfo.disToPlayer >= 1f) 
		{
			fsm.TryTransferAction<IdleState> ("IdleState");
		}
	}
	private void OnAngryBehaviour ()
	{
		var tmpDisState = -1;
		if (StilettoFlag.DisInExShort) 
		{
			tmpDisState = 0;
			WarmupPattern (StilettoPatternType.하단찌르기_중단찌르기);
			WarmupPattern (StilettoPatternType.중단찌르기_즉시사격);
		}
		else if (StilettoFlag.DisInShort)
		{
			tmpDisState = 1;
			WarmupPattern (StilettoPatternType.상단킥_중단찌르기_중단베기);
			WarmupPattern (StilettoPatternType.하단찌르기_중단찌르기);
			WarmupPattern (StilettoPatternType.중단찌르기_즉시사격);
			WarmupPattern (StilettoPatternType.슬라이딩_즉시사격);
		}
		else if (StilettoFlag.DisInMiddle)
		{
			WarmupPattern (StilettoPatternType.중단찌르기_즉시사격);
			WarmupPattern (StilettoPatternType.상단킥_중단찌르기_중단베기);
			WarmupPattern (StilettoPatternType.슬라이딩_즉시사격);
			tmpDisState = 2;
		}
		else if (StilettoFlag.DisInFar)
		{
			WarmupPattern (StilettoPatternType.상단킥_중단찌르기_중단베기);
			WarmupPattern (StilettoPatternType.슬라이딩_즉시사격);
			tmpDisState = 3;
		}
		if (patternBuffer.Count != 0)
			return;
		//	Select FSM State 
		if (disInfo.disToPlayer >= 1f) 
		{
			fsm.TryTransferAction<IdleState> ();
		}
	}
	public void OnTwoPointMoveEnd ()
	{
		if (StilettoFlag.IsNormal) 
		{
			AddPatternToBuffer (StilettoPatternType.샷건);
		}
		tpMoveInfo.moveTimer.duration = 0.5f;
	}
	private bool CheckSkillUsable ()
	{
		var pattern = (nowPattern == null) ? patternBuffer[0] : nowPattern;

		if (pattern.patternName == StilettoPatternType.분노.ToString())
		{
			if (!StilettoFlag.IsMoving && !StilettoFlag.IsAttackState) 
			{
				var newPos = GetCenterPosition ();
				var disToCenter = Mathf.Abs (transform.position.x - newPos.x);
				if (disToCenter <= 3f)
					return true;
				desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
				if (lookDirection != desireLookDir) {
					TwoPointMove.InitFactory (newPos, "Dash_Back", false, 0.5f);
				} else {
					TwoPointMove.InitFactory (newPos, "Dash_Front",true, 0.5f);
				}

				StartCoroutine (TwoPointMove.starter);
				tpMoveInfo.moveGhost.ghostingEnabled = false;
				return false;
			}
			serviceInstance.VibrateCameraByAttack (0.4f, 8f, 1.5f);
		}
		else if (pattern.patternName == StilettoPatternType.발도.ToString ()) 
		{
			if (patternIndex != 0)
				return true;
			Vector3 newPos = GetPlayerBackPosition ();
			float corDis = Mathf.Abs (newPos.x - transform.position.x);
			if (corDis < 2f || disInfo.disToPlayer > 3f && !StilettoFlag.IsAngry)
			{
				tpMoveInfo.IncCoolTimer (Time.deltaTime);
				ResetPattern (StilettoPatternType.발도);
				WarmupPattern (StilettoPatternType.샷건);
				return false;
			}
			baldoInfo.destPos = newPos;
		} 
		else if (pattern.patternName == StilettoPatternType.상단킥_중단찌르기_중단베기.ToString ()) 
		{
			if (!pattern.CheckUseable(patternIndex))
				return false;
			if (disInfo.disToPlayer >= 0.6f && !StilettoFlag.IsMoving && !StilettoFlag.IsAttackState) 
			{
				var newPos = GetCloseMovingPosition ();
				desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
				TwoPointMove.InitFactory (newPos, "", false, 0.15f);
				StartCoroutine (TwoPointMove.starter);
				return true;
			}
		}
		else if (pattern.patternName == StilettoPatternType.중단찌르기_즉시사격.ToString())
		{
			if (!pattern.CheckUseable (patternIndex))
				return false;
			if (!StilettoFlag.IsMoving && !StilettoFlag.IsAttackState) 
			{
				if (patternIndex == 0) {
					var newPos = GetCloseMovingPosition ();
					desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
					TwoPointMove.InitFactory (newPos, "", false, 0.15f);
				} 
				else {
					var newPos = GetPlayerBackPosition (2.5f);
					desireLookDir = disInfo.dirToPlayer;
					SetLookDirection (-desireLookDir);
					TwoPointMove.InitFactory (newPos, "", false, 0.15f);
				}
				StartCoroutine (TwoPointMove.starter);
				return true;
			}
		}

		return true;
	}

	public bool CheckUsedSkill (string skillName)
	{
		if (GetUsedSkill ().skillName == skillName)
			return true;
		return false;
	}
	public bool CheckUsedSkill (StilettoPatternType patternName)
	{
		if (GetUsedSkill ().skillName == patternName.ToString ())
			return true;
		return false;
	}
	public void AddPatternToBuffer (StilettoPatternType patternName)
	{
		var tmpPattern = patternData.patternDic [patternName.ToString ()];
		tmpPattern.ReadyPattern (0);
		if (!patternBuffer.Contains (tmpPattern))
		{
			patternBuffer.Add (tmpPattern);
			patternBuffer.Sort (delegate(EnemyAttackPatternInfo x, EnemyAttackPatternInfo y) {
				return (x.patternPriority > y.patternPriority) ? -1 : 1;	
			});
		}
	}
	public new void OnCollisionEnter (Collision col)
	{
		rigid.AddForce (col.relativeVelocity * 0.8f,ForceMode.Impulse);
	}
	public void OnStunEnter (object sender, ActorDamagedEventArg e)
	{
		serviceInstance.VibrateCameraByAttack (0.4f, 8f, 0.1f);
		if (StilettoFlag.IsMoving) 
		{
			StopCoroutine (TwoPointMove.update);
			StartCoroutine (TwoPointMove.reset);
		}
		OnSkillEffectEnd ();
		patternIndex = 0;
		parriedCount = 0;
		nowPattern.ResetPattern();
		patternBuffer.Clear ();
		nowPattern = null;
		rigid.velocity = Vector3.zero;
		Debug.Log ("On Enter Stun");
		skipAttackAnim = false;
		if (patternBuffer.Count != 0)
			patternBuffer.RemoveAt (0);
		if (actorInfo.GetLife () <= 0f)
		{
			StartCoroutine (IChangeColor (deadColor, 0.35f));
		}
	}

	public override void OnParriedInDirect (object sender, DamageInfo d)
	{

	}
	public override void OnParriedInCombo (object sender, DamageInfo d)
	{
		//	대신 Anger Count 감소
		//base.OnParriedInCombo (sender, d);
		angerInfo.DecAngryCount();

		OnSkillEffectEnd ();
		HandleAttackEndEvent (activeACIndex);
		rigid.velocity = Vector3.zero;
		serviceInstance.SlowMotionCamera (0.1f, 0.1f);
		if (nowPattern.patternName == StilettoPatternType.중단찌르기_즉시사격.ToString())
		{
			var newPos = GetPlayerBackPosition (3f);
			desireLookDir = disInfo.dirToPlayer;
			SetLookDirection (-desireLookDir);
			TwoPointMove.InitFactory (newPos, "", false, 0.15f);
			StartCoroutine (TwoPointMove.starter);
		}
		UseNextSkill ();
		if (angerInfo.remainAngryCount <= 0) 
		{
			parriedCount = nowPattern.skillBuffer.Count;
			angerInfo.OnExitAngry ();
		}
	}
	public override void OnParriedInCombo2 (object sender, DamageInfo d)
	{
		OnSkillEffectEnd ();
		HandleAttackEndEvent (activeACIndex);

		serviceInstance.SlowMotionCamera (0.1f, 0.1f);
		UseNextSkill ();
	}

	public void OnPatternCompleted ()
	{
		var patternName = nowPattern.patternName;
		Debug.Log ("Pattern Completed : " + patternName);
		patternIndex = 0;
		parriedCount = 0;
		nowPattern.ResetPattern();
		patternBuffer.Remove (nowPattern);
		nowPattern = null;
		if (patternName == StilettoPatternType.중단찌르기_즉시사격.ToString ()) 
		{
			//skillCoolTimer.MakeFullCharge ("중단찌르기");
			skillCoolTimer.MakeFullCharge ("즉시사격");
		}
		else if (patternName == StilettoPatternType.상단킥_중단찌르기_중단베기.ToString())
		{
			if (StilettoFlag.IsMoving)
				return;
			var newPos = GetAvoidMovingPosition ();
			desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
			if (lookDirection != desireLookDir) {
				TwoPointMove.InitFactory (newPos, "Avoid_Back", false, 0.25f);
			} else {
				TwoPointMove.InitFactory (newPos, "Avoid_Back", true, 0.25f);
			}
			StartCoroutine (TwoPointMove.starter);
		}
	}

	//	Build Event Listener Block
	private void BuildActorEventListener ()
	{
		RaiseActorAttackSuccess += (object sender, ActorDamagedEventArg e) =>  {
			foreach (var ac in attackColliderList) 
			{
				ac.Release (this);
			}
		};
		RaiseActorDamaged += (object sender, ActorDamagedEventArg e) =>  {
			Debug.Log ("Stiletto Damaged : " + e.damageInfo.skillInfo.skillName);
			if (actorInfo.isStunByParry &&
				e.damageInfo.skillInfo.skillCounterType == SkillCounterType.NONE) 
			{
				damFlicker.Initialize ();
				StartCoroutine (damFlicker.OnDamageFlick);
			}
			else if (!actorInfo.isStunByParry &&
				!StilettoFlag.IsAngry &&
				!StilettoFlag.IsMoving &&
				e.damageInfo.skillInfo.skillCounterType == SkillCounterType.NONE)
			{
				angerInfo.IncAngerGauge (e.damageInfo.skillInfo.damage);
			}
			if (actorInfo.GetLife () <= 0f)
				StartCoroutine (IChangeColor (deadColor, 0.35f));
		};
		RaiseActorStun += OnStunEnter;
		RaiseActorLand += delegate(object sender, ActorLandEventArg e) {
			rigid.velocity = Vector3.zero;
		};
		RaiseActorDead += (object sender, ActorDamagedEventArg e) => {
			damFlicker.damFlickTimer = damFlicker.damFlickDuration;
		};
	}
	private void BuildFSMEventListener ()
	{
		var walkState = fsm.GetState<WalkState> ();
		var attackState = fsm.GetState<AttackState> ();
		walkState.OnUpdate = delegate () {
			var adjustVelocity = rigid.velocity;
			if (System.Single.IsNaN (animPlaybackTime))
				adjustVelocity.x = Mathf.Sign (lookDirection) * GetMoveSpeed () * walkSpeedCurve.Evaluate (0f);
			else
				adjustVelocity.x = Mathf.Sign (lookDirection) * GetMoveSpeed () * walkSpeedCurve.Evaluate (animPlaybackTime);
			rigid.velocity = adjustVelocity;
		};
		//	Charge 공격에서 isAttackEnd가 false가 되고, Charge가 끝나면 true가 됨
		attackState.checkExit = delegate()
		{
			if (attackState.skillInfo.skillName == StilettoPatternType.발도_준비.ToString())
			{
				if (StilettoFlag.IsBaldoAttack)
					return false;
			}
			else if (attackState.skillInfo.skillName == StilettoPatternType.발도_완료.ToString())
			{
				if (StilettoFlag.IsBaldoAttack)
					return false;
			}
			return attackState.CommonCheckExit();
		};
		//	Assign Enemy Attack Warning Color Func
		attackState.OnEnter += delegate () {
			ghosting.ghostingEnabled = true;
			SetWarningGhostingColor ();
		};
		attackState.OnExit += delegate() 
		{
			ghosting.ghostingEnabled = false;
			if (null == nowPattern)
				nowPattern = patternBuffer[0];
			if (nowPattern.patternName == StilettoPatternType.상단킥_중단찌르기_중단베기
				.ToString())
			{
				if (StilettoFlag.DisInExShort || StilettoFlag.DisInShort)
					comboAttackSuccessFlag = true;
				bool cancle = (Random.Range (0, 2) == 0) ? false : true;
				if (cancle && patternIndex >= 1 && !StilettoFlag.IsMoving) {
					var newPos = GetAvoidMovingPosition ();
					desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
					if (lookDirection != desireLookDir) {
						TwoPointMove.InitFactory (newPos, "Dash_Back", false, 0.25f);
					} else {
						TwoPointMove.InitFactory (newPos, "Dash_Front",true, 0.25f);
					}
					StartCoroutine (TwoPointMove.starter);
					comboAttackSuccessFlag = false;
					OnPatternCompleted();
					if (Random.Range (0, 2) == 0)
						AddPatternToBuffer (StilettoPatternType.즉시사격);
					else
						AddPatternToBuffer (StilettoPatternType.발도);
					return;
				}
				if (comboAttackSuccessFlag)
					patternIndex++;
				if (patternIndex == nowPattern.skillBuffer.Count || !comboAttackSuccessFlag)
				{
					OnPatternCompleted();
				}
				comboAttackSuccessFlag = false;
				return;
			}
			else if (StilettoFlag.IsShotGunBurst)
			{
				if (disInfo.disToPlayer <= 4f)
					StilettoFlag.IsShotGunBurst = false;
				return;
			}
			else if (StilettoFlag.IsBaldoBurst)
			{
				if (baldoBurstInfo.burstNum > 0)
				{
					baldoBurstInfo.burstNum--;
					//float newDis = 4.5f;
					var newPos = GetPlayerBackPosition();
					baldoInfo.destPos = newPos;
					SetLookDirection (disInfo.dirToPlayer);
					BaldoBehaviour.InitFactory (baldoInfo.destPos);
					StartCoroutine (BaldoBehaviour.starter);
					return;
				}
				else
					StilettoFlag.IsBaldoBurst = false;
			}
			patternIndex++;
			if (patternIndex == nowPattern.skillBuffer.Count)
			{
				OnPatternCompleted();
			}
		};
		var stunState = fsm.GetState<StunState> ();
		stunState.OnUpdate += delegate() 
		{
			if (stunState.knockoutTimer >= 0.7f)
			{
				PlayAnimation (0, "Stun_loop", true, 1f);
			}
		};
	}
	private void BuildSpineEventListener ()
	{
		animator.state.Start += (Spine.TrackEntry trackEntry) => {
			;
			if (trackEntry.Animation.Name == "HighAttack_Kick1") 
			{
				//serviceInstance.SlowMotionCamera (0.3f, 0.2f);
			}
		};
		animator.state.Complete += (Spine.TrackEntry trackEntry) => {
			if (trackEntry.Animation.Name == "Battojutsu_Ready") {
				if (!StilettoFlag.IsBaldoBurst && StilettoFlag.IsAngry)
				{
					StilettoFlag.IsBaldoBurst = true;
					patternIndex++;
				}
				BaldoBehaviour.InitFactory (baldoInfo.destPos);
				StartCoroutine (BaldoBehaviour.starter);
			}
			else if (trackEntry.Animation.Name == "ShotGun_Ready") {
				StilettoFlag.IsShotGunBurst = true;
				patternIndex++;
			}
			else if (trackEntry.Animation.Name == "Rage_Start")
			{
				angerInfo.angryParticle.Play();
				angerInfo.SetAngerValue();
				skipBehaviour = false;
			}
			else if (trackEntry.Animation.Name == "FastDraw")
			{
				if (nowPattern.patternName == StilettoPatternType.슬라이딩_즉시사격.ToString())
				{
					HandleAttackEndEvent (0);
				}
			}
			else if (trackEntry.Animation.Name == "LowAttack_Slide_Start" )
			{
				SetLookDirection (disInfo.dirToPlayer);
			}
		};
		animator.state.End += (Spine.TrackEntry trackEntry) => {
			/*if (isStunByParry)
			{
				Debug.Log ("Animation Interrupted");
				SpineAnimationEndHandler(trackEntry);
				isStunByParry = false;
			}*/
			if (skipAttackAnim)
			{
				Debug.Log ("Animation Interrupted");
				SpineAnimationEndHandler(trackEntry);
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
			else if (e.Data.Name == "Alarm_Start")
			{
				var newAlarm = GameObject.Instantiate (ptEffect, transform.position + ptEffectPos , Quaternion.identity);
				newAlarm.transform.parent = transform;
			}
		};
	}
	//	End of EventListener Block

	//	Spine Event Handler Block
	private void OnSkillEffectStart ()
	{
		if (CheckUsedSkill("하단베기"))
		{

		}
		else if (CheckUsedSkill("상단베기"))
		{

		}
	}
	private void OnSkillEffectEnd ()
	{
		if (CheckUsedSkill("하단베기"))
		{

		}
		else if (CheckUsedSkill("상단베기"))
		{
			serviceInstance.VibrateCameraByAttack (0.4f, 8f, 0.1f);
		}
		ghosting.ghostingEnabled = false;
	}
	private void OnSkillHitStart ()
	{
		ghosting.ghostingEnabled = false;
		if (CheckUsedSkill(StilettoPatternType.상단찌르기))
		{
			activeACIndex = 1;
			if (disInfo.disToPlayer >= 1f && !StilettoFlag.IsMoving)
			{
				var newPos = GetCloseMovingPosition ();
				desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
				TwoPointMove.InitFactory (newPos, "", false, 0.15f);
				StartCoroutine (TwoPointMove.starter);
			}
		}
		else if (CheckUsedSkill (StilettoPatternType.상단킥)) 
		{
			activeACIndex = 1;
			SetLookDirection (disInfo.dirToPlayer);
			if (disInfo.disToPlayer >= 1f && !StilettoFlag.IsMoving) 
			{
				var newPos = GetCloseMovingPosition ();
				desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
				TwoPointMove.InitFactory (newPos, "", false, 0.3f);
				StartCoroutine (TwoPointMove.starter);
			}
		} 
		else if (CheckUsedSkill (StilettoPatternType.중단베기))
		{
			activeACIndex = 1;
			if (disInfo.disToPlayer >= 1f && !StilettoFlag.IsMoving)
			{
				var newPos = GetCloseMovingPosition ();
				desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
				TwoPointMove.InitFactory (newPos, "", false, 0.05f);
				StartCoroutine (TwoPointMove.starter);
			}
			SetLookDirection (disInfo.dirToPlayer);
		}
		else if (CheckUsedSkill (StilettoPatternType.중단찌르기))
		{
			activeACIndex = 1;
			if (disInfo.disToPlayer >= 1f && !StilettoFlag.IsMoving)
			{
				var newPos = GetCloseMovingPosition ();
				desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
				TwoPointMove.InitFactory (newPos, "", false, 0.05f);
				StartCoroutine (TwoPointMove.starter);
			}
			SetLookDirection (disInfo.dirToPlayer);
		} 
		else if (CheckUsedSkill (StilettoPatternType.하단찌르기))
		{
			activeACIndex = 1;
		} 
		else if (CheckUsedSkill (StilettoPatternType.하단찌르기_중단찌르기))
		{
			activeACIndex = 1;
		} 
		else if (CheckUsedSkill (StilettoPatternType.상단킥_중단찌르기_중단베기)) 
		{
			activeACIndex = 1;
		} 
		else if (CheckUsedSkill (StilettoPatternType.샷건_샷)) 
		{
			activeACIndex = -1;
			projectile [0].OnLaunch ("샷건_샷", true);
		}
		else if (CheckUsedSkill (StilettoPatternType.샷건_완료)) 
		{
			activeACIndex = -1;
			ghosting.ghostingEnabled = false;
		}
		else if (CheckUsedSkill (StilettoPatternType.즉시사격))
		{
			activeACIndex = -1;
			projectile [0].OnLaunch ("즉시사격", true);
		}
		else if (CheckUsedSkill (StilettoPatternType.슬라이딩_준비))
		{
			activeACIndex = 0;
			var newPos = GetPlayerBackPosition (3.5f);
			desireLookDir = Mathf.Sign (newPos.x - transform.position.x);
			TwoPointMove.InitFactory (newPos, "", true, 0.4f);
			StartCoroutine (TwoPointMove.starter);
		} 
		if (activeACIndex != -1)
			HandleAttackEvent (activeACIndex);
	}
	private void OnSkillHitEnd ()
	{
		HandleAttackEndEvent (activeACIndex);
	}
	//	End of Spine Event Handler Block

	//	조건변수 Help Func Block
	private IEnumerator IResetAngerGauge(float duration)
	{
		float timer = 0f;
		while (timer <= duration || StilettoFlag.IsMoving || StilettoFlag.IsBaldoBurst) 
		{
			timer += Time.deltaTime;
			angerInfo.curGauge = (int)Mathf.Lerp (100f, 0f, timer / duration); 
			yield return null;
		}
		actorInfo.basicStat.physicalDefense = 0f;
		angerInfo.ResetGauge ();
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
	private IEnumerator IChangeOverlayColor (Color color, float duration)
	{
		float timer = 0f;
		while (timer <= duration) 
		{
			mat.SetColor ("_OverlayColor", Color.Lerp(mat.GetColor("_OverlayColor"), color , timer/duration));
			timer += Time.deltaTime;
			yield return null;
		}
	}
	private EnemyAttackPatternInfo topSkill
	{
		get{
			if (patternBuffer.Count == 0)
				return null;
			return patternBuffer [0];
		}
	}
	private Vector3 GetFarestWallPosition ()
	{
		float disToMax = Mathf.Abs (transform.position.x - mapMaxX);
		float disToMin = Mathf.Abs (transform.position.x - mapMinX);
		Vector3 newPos = transform.position;
		if (disToMax > disToMin) {
			newPos.x = mapMaxX;
		}
		else {
			newPos.x = mapMinX;
		}
		return newPos;
	}
	private Vector3 GetAvoidMovingPosition ()
	{
		float newDir = (Random.Range(0, 2) == 0) ? disInfo.dirToPlayer : Mathf.Sign(transform.position.x - player.transform.position.x);
		float newDis = disInfo.disToPlayer + Random.Range (2, 4);
		float newDeltaX = newDir * newDis;
		Vector3 newPos = transform.position + Vector3.right * newDeltaX;
		if (newPos.x >= mapMaxX)
			newDeltaX *= -1f;
		else if (newPos.x <= mapMinX)
			newDeltaX *= -1f;
		return transform.position + Vector3.right * newDeltaX;
	}
	private Vector3 GetCloseMovingPosition ()
	{
		float newDir = disInfo.dirToPlayer;
		float newDeltaX = newDir * disInfo.disToPlayer;
		Vector3 newPos = transform.position + Vector3.right * newDeltaX;
		newPos.x += -disInfo.dirToPlayer * 0.55f;
		newPos.x = Mathf.Clamp (newPos.x, mapMinX, mapMaxX);
		return newPos;
	}
	private Vector3 GetCenterPosition ()
	{
		Vector3 newPos = transform.position;
		newPos.x = (mapMinX + mapMaxX) * 0.5f;
		return newPos;
	}
	private Vector3 GetPlayerBackPosition(float addDis = 2f)
	{
		float newDis = (disInfo.disToPlayer < 4f) ? 8f : disInfo.disToPlayer + addDis;
		float newDeltaX = disInfo.dirToPlayer * newDis;
		Vector3 newPos = transform.position + Vector3.right * newDeltaX;
		newPos.x = Mathf.Clamp (newPos.x, mapMinX, mapMaxX);
		return newPos;
	}
	//	End of Help Func Block

	public void WarmupPattern (StilettoPatternType pType)
	{
		patternData.WarmupPattern (pType.ToString ());
		if (!patternData.CheckUseable (pType.ToString (), patternIndex))
			return;
		if (!patternBuffer.Contains (patternData.patternDic [pType.ToString ()])) {
			patternBuffer.Add (patternData.patternDic[pType.ToString()]);
		}
	}
	private void ResetPattern (StilettoPatternType pType)
	{
		patternBuffer.Remove (patternData.patternDic [pType.ToString()]);
	}
	private void SetWarningGhostingColor ()
	{
		var pattern = (nowPattern == null) ? patternBuffer[0] : nowPattern;
		if (pattern.patternType == EnemyAttackPatternType.DIRECT)
		{
			if (pattern.GetCounterType(patternIndex) == SkillCounterType.NONE)
				ghosting.color = noneParryWarnColor;
			else
				ghosting.color = directWarnColor;
		}
		else if (pattern.patternType == EnemyAttackPatternType.COMBO)
		{
			if (pattern.GetCounterType(patternIndex) == SkillCounterType.NONE)
				ghosting.color = noneParryWarnColor;
			else
				ghosting.color = comboWarnColor;
		}
	}
	public bool UseNextSkill ()
	{
		skipAttackAnim = true;
		//isStunByParry = true;
		if (!nowPattern.UseNextSkill (ref patternIndex)) 
		{
			patternIndex = Mathf.Min (patternIndex + 1, nowPattern.skillBuffer.Count - 1);
			return false;
		}
		SetWarningGhostingColor ();
		return true;
	}
}
