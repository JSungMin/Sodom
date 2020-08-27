using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;
using BossSpace.MetatronSpace;
using BossSpace.MetatronSpace.SubInfo;
using BossSpace.MetatronSpace.Behaviour;
using Spine.Unity;

public class Metatron : EnemySpineBase {
	private float defaultMoveSpeed;
	public AnimationCurve walkSpeedCurve;
	public Color directWarnColor = new Color(0.404f, 0f, 0f, 0f);
	public Color comboWarnColor = new Color(0.514f, 0.05f, 0.714f, 0f);	
	public Color noneParryWarnColor = new Color(0f, 0.635f, 0.69f, 0f);
	public Material mat;

	public LaserLauncher laserLauncher;
	public Spine.Unity.Modules.SkeletonGhost ghosting;
	
	//	상황 조건 변수
	public bool isPlayerGround;
	public TwoPointMoveInfo tpMoveInfo;
	public SpeedInfo speedInfo;
	public int phaseIndex = 0;

	public Color deadColor;
	public Animator eyeLaserEffect;
	public ParticleSystem spearParticle;
	public ParticleSystem landingParticle;
	public ParticleSystem spearDirtParticle;
	//	Damage Flicker : 데미지 받았을 때 유닛을 깜빡거리게 함
	public ColorFlicker damFlicker;

	//	END TEST CODE BLOCK

	public bool CheckUsedSkill (string skillName)
	{
		if (GetUsedSkill ().skillName == skillName)
			return true;
		return false;
	}

	public new void OnCollisionEnter (Collision col)
	{
		rigid.AddForce (col.relativeVelocity,ForceMode.Impulse);
	}
	public void OnStunEnter (object sender, ActorDamagedEventArg e)
	{
		serviceInstance.VibrateCameraByAttack (1f, 8f, 0.1f);
		OnSkillEffectEnd ();
		rigid.velocity = Vector3.zero;
		Debug.Log ("On Enter Stun");
		if (null != nowPattern)
			nowPattern.ResetPattern ();
		nowPattern = null;
		patternIndex = 0;
		parriedCount = 0;
		if (patternBuffer.Count != 0)
			patternBuffer.RemoveAt (0);
		if (actorInfo.GetLife () <= 0f)
		{
			StartCoroutine (IChangeColor (deadColor, 0.35f));
		}
	}

	public override void OnParriedInDirect (object sender, DamageInfo d)
	{
		skillCoolTimer.ResetSkill(GetUsedSkill().skillName);
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
			if (patternIndex + 1 < nowPattern.skillBuffer.Count)
				patternIndex++;
			return;
		}
		SetWarningGhostingColor ();
	}
	public override void OnParriedInCombo2 (object sender, DamageInfo d)
	{
		
	}

	public void OnPatternCompleted ()
	{
		Debug.Log ("Pattern Completed : " + nowPattern.patternName);
		patternIndex = 0;
		parriedCount = 0;
		nowPattern.ResetPattern();
		if (patternBuffer.Count != 0)
			patternBuffer.RemoveAt (0);
		nowPattern = null;
	}

	// Use this for initialization
	public new void Start () {
		base.Start ();
        Debug.Log("Metatron Init");
		mat.SetColor ("_Color", Color.white);
		defaultMoveSpeed = GetMoveSpeed ();
		damFlicker.Initialize ();
		player = GameObject.FindObjectOfType<Player> ();

		//	Boss Pattern Build;
		patternData.BuildPattern (this);
		//	Actor Event Listener Initialize
		BuildActorEventListener ();
		//	FSM Event Listener Initialize
		BuildFSMEventListener ();
		//	Spine Event Listener Initialize
		BuildSpineEventListener ();

		ptEffectPos = Vector3.up;
	}

	//	Build Event Listener Block
	private void BuildActorEventListener ()
	{
		RaiseActorAttackSuccess += (object sender, ActorDamagedEventArg e) =>  {
			foreach (var ac in attackColliderList) 
			{
                //ac.Release (this);
                ac.Release (e.damageInfo.victim);
			}
		};
		RaiseActorDamaged += (object sender, ActorDamagedEventArg e) =>  {
			Debug.Log ("Meta Damaged : " + e.damageInfo.skillInfo.skillName);
			if (actorInfo.isStunByParry &&
				e.damageInfo.skillInfo.skillCounterType == SkillCounterType.NONE) 
			{
				damFlicker.Initialize ();
				StartCoroutine (damFlicker.OnDamageFlick);
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
		fsm.GetState<WalkState>().OnUpdate = delegate () {
			var adjustVelocity = rigid.velocity;			
			if (System.Single.IsNaN (animPlaybackTime))
			{
				adjustVelocity.x = Mathf.Sign (lookDirection) * GetMoveSpeed () * walkSpeedCurve.Evaluate (0f);
			}
			else
			{
				adjustVelocity.x = Mathf.Sign (lookDirection) * GetMoveSpeed () * walkSpeedCurve.Evaluate (animPlaybackTime);
			}
			rigid.velocity = adjustVelocity;
		};
		//	Assign Enemy Attack Warning Color Func
		fsm.GetState<AttackState>().OnEnter += delegate () {
			ghosting.ghostingEnabled = true;
			SetWarningGhostingColor ();
			if (!CheckUsedSkill("레이저"))
			{
				PlaySound (AudioType.METATRON_ATTACK_READY);	
			}
			else
			{
				serviceInstance.VibratePad(10f, 10f, 1f);
			}
			if (CheckUsedSkill("하단베기"))
			{
				PlaySound (AudioType.METATRON_DRAG_GROUND, false, 0.2f);
			}
			else if (CheckUsedSkill("찌르기"))
			{
				PlaySound (AudioType.METATRON_STAB, false, 0.15f);
			}
			else if (CheckUsedSkill("빠른찌르기"))
			{
				PlaySound (AudioType.METATRON_STAB, false, 0.15f);
			}
			else if (CheckUsedSkill("몸통박치기"))
			{
				PlaySound (AudioType.METATRON_STAB, false, 0.2f);
			}
		};
		fsm.GetState<AttackState>().OnExit += delegate() 
		{
			if (!actorInfo.isStunByParry)
				patternIndex++;
			if (patternIndex == nowPattern.skillBuffer.Count)
			{
				OnPatternCompleted();
			}
		};
		fsm.GetState<DeadState>().OnEnter += delegate() {
			PlaySound (AudioType.METATRON_DEAD);
			OnSkillEffectEnd();
		};
		//float timer = 0f;
		fsm.GetState<DeadState>().OnUpdate += delegate() {
			//timer += Time.deltaTime;
			//if (timer >= 2f)
			//	UnityEngine.SceneManagement.SceneManager.LoadScene ("Stage02");
		};
	}
	private void BuildSpineEventListener ()
	{
		animator.state.End += (Spine.TrackEntry trackEntry) => {
			/*if (isStunByParry)
			{
				Debug.Log ("Animation Interrupted");
				SpineAnimationEndHandler(trackEntry);
				isStunByParry = false;
			}*/
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
			else if (e.Data.Name == "Jump_Start")
			{
				var vel = disInfo.disToPlayer / 1.5f * 4.5f;
				rigid.AddForce (new Vector3 (vel * GetLookDirection(), 9.5f), ForceMode.Impulse);
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
		//ghosting.ghostingEnabled = true;
		if (CheckUsedSkill("하단베기"))
		{
			spearParticle.transform.localRotation = Quaternion.Euler(Vector3.zero);
			spearParticle.Play();
		}
		else if (CheckUsedSkill("상단베기"))
		{
			spearParticle.transform.localRotation = Quaternion.Euler(Vector3.forward * 180f);
			spearDirtParticle.Play ();
			PlaySound (AudioType.METATRON_IMPACT_GROUND);
		}
	}
	private void OnSkillEffectEnd ()
	{
		if (CheckUsedSkill("하단베기"))
		{
			spearParticle.Stop();
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
		if (CheckUsedSkill("찌르기"))
		{
			activeACIndex = 0;
		}
		else if (CheckUsedSkill("레이저"))
		{
			PlaySound(AudioType.METATRON_LASER);
			laserLauncher.OnLaunch (GetUsedSkill().skillName);
			activeACIndex = -1;
			eyeLaserEffect.Play("Shot");
		}
		else if (CheckUsedSkill("몸통박치기"))
		{
			SkillContentPool.ForwardDash (this, GetUsedSkill());
			activeACIndex = 0;
		}
		else if (CheckUsedSkill("도약"))
		{
			activeACIndex = 0;
			PlaySound(AudioType.METATRON_FLY_ATTACK, true);
			serviceInstance.VibratePad(5f, 5f, 0.5f);
		}
		else if (CheckUsedSkill("하단베기"))
		{
			activeACIndex = 0;
		}
		else if (CheckUsedSkill("상단베기"))
		{
			activeACIndex = 0;
		}
		else if (CheckUsedSkill("빠른찌르기"))
		{
			activeACIndex = 0;
		}
		if (activeACIndex != -1)
			HandleAttackEvent(activeACIndex);
	}
	private void OnSkillHitEnd ()
	{
		if (CheckUsedSkill ("레이저"))
		{
			laserLauncher.OnRelease();
		}
		else if (CheckUsedSkill("도약"))
		{
			serviceInstance.VibrateCameraByAttack (0.75f, 8f, 0.25f);
			if (!landingParticle.isPlaying)
				landingParticle.Play ();
		}
		if (activeACIndex != -1)
			HandleAttackEndEvent (activeACIndex);
    }
 
    //	End of Spine Event Handler Block
    public void FixedUpdate ()
	{
		//base.Update ();
		if (actorInfo.GetLifePercent () <= 0f) {
			return;
		}
		skillCoolTimer.CoolingAll ();

		//	Initialize 조건변수
		disInfo.disToPlayer = Vector3.Distance (transform.position, player.transform.position);
		disInfo.dirToPlayer = Mathf.Sign (player.transform.position.x - transform.position.x);
		isPlayerGround = player.actorInfo.isGrounded;
		var prevPhase = phaseIndex;
		phaseIndex = (actorInfo.GetLifePercent() > 0.5f) ? 0 : 1;
		if (prevPhase != phaseIndex)
		{
			AddPatternToBuffer ("도약");
		}
	
		//	세부조건
		if (!isPlayerGround) 
		{
			WarmupPattern (TankPatternType.상단베기);
			if (MetatronFlag.DisInFar) 
				WarmupPattern (TankPatternType.레이저);
		}
		switch (phaseIndex) 
		{
		case 0:
			if (MetatronFlag.DisInShort)
			{
				WarmupPattern (TankPatternType.찌르기);
				WarmupPattern (TankPatternType.상단베기);
				WarmupPattern (TankPatternType.레이저);
			} 
			else if (MetatronFlag.DisInMiddle) 
			{
				WarmupPattern (TankPatternType.몸통박치기);
				WarmupPattern (TankPatternType.상단베기);
				WarmupPattern (TankPatternType.하단베기);
				ResetPattern (TankPatternType.레이저);
			}
			else if (MetatronFlag.DisInFar) 
			{
				//WarmupPattern (TankPatternType.박기_레이저);
				WarmupPattern (TankPatternType.레이저);
				//WarmupPattern (TankPatternType.도약);

				ResetPattern (TankPatternType.찌르기);
				ResetPattern (TankPatternType.상단베기);
				ResetPattern (TankPatternType.하단베기);
			}
			break;
		case 1:
			if (MetatronFlag.DisInShort)
			{
				WarmupPattern (TankPatternType.찌르기);
				WarmupPattern (TankPatternType.상단베기);
				WarmupPattern (TankPatternType.연속도약);
				WarmupPattern (TankPatternType.레이저);
				//WarmupPattern (TankPatternType.초강타);
			} 
			else if (MetatronFlag.DisInMiddle) 
			{
				//WarmupPattern (TankPatternType.박기_레이저);
				WarmupPattern (TankPatternType.몸통박치기);
				WarmupPattern (TankPatternType.상단베기);
				//WarmupPattern (Boss01PatternType.하단베기);
				WarmupPattern (TankPatternType.하단베기_찌르기);
				WarmupPattern (TankPatternType.박기_하단베기_찌르기);
				WarmupPattern (TankPatternType.연속도약);
			}
			else if (MetatronFlag.DisInFar) 
			{
				WarmupPattern (TankPatternType.레이저);
				//WarmupPattern (TankPatternType.박기_레이저);
				WarmupPattern (TankPatternType.도약);

				ResetPattern (TankPatternType.찌르기);
				ResetPattern (TankPatternType.상단베기);
				ResetPattern (TankPatternType.하단베기_찌르기);
			}
			break;
		}

		if (GetMoveable ())
			SetLookDirection (Mathf.Sign (player.transform.position.x - transform.position.x));

		//	일반 모션
		if (patternBuffer.Count == 0) 
		{
			if (disInfo.disToPlayer >= disInfo.disInShort)
				fsm.TryTransferAction<WalkState> ();
			else
				fsm.TryTransferAction<IdleState> ();
		}
		if (actorInfo.isStunByParry)
			return;
		//	Skill Buffer 해소
		if (nowPattern == null) 
		{
			if (patternBuffer.Count == 0)
				return;
		
			if (patternBuffer [0].UseSkill (ref patternIndex))
			{
				nowPattern = patternBuffer [0];
				Debug.Log ("Pattern Start : " + nowPattern.patternName);
			}
			else 
			{
				patternBuffer.RemoveAt (0);
			}
		} 
		else 
		{
			if (patternIndex != 0)
			{
				nowPattern.ReadyPattern (patternIndex);
			}
		
			if (!nowPattern.UseSkill (ref patternIndex))
			{
				if (patternIndex == 0) 
				{
					patternBuffer.Sort (delegate(EnemyAttackPatternInfo x, EnemyAttackPatternInfo y) {
						return (x.patternPriority < y.patternPriority) ? -1 : 1;	
					});
					patternBuffer.Remove (nowPattern);
				}
			}
		}
	}
	//	조건변수 Help Func Block

	//	End of Help Func Block

	public void AddPatternToBuffer (string patternName)
	{
		var tmpPattern = patternData.patternDic [patternName];
		tmpPattern.ReadyPattern (0);
		if (!patternBuffer.Contains (tmpPattern))
		{
			patternBuffer.Add (tmpPattern);
			patternBuffer.Sort (delegate(EnemyAttackPatternInfo x, EnemyAttackPatternInfo y) {
				return (x.patternPriority > y.patternPriority) ? -1 : 1;	
			});
		}
	}

	private void WarmupPattern (TankPatternType pType)
	{
		patternData.WarmupPattern (pType.ToString ());
		if (!patternData.CheckUseable (pType.ToString (), patternIndex))
			return;
		if (!patternBuffer.Contains (patternData.patternDic [pType.ToString ()])) {
			patternBuffer.Add (patternData.patternDic[pType.ToString()]);
		}
	}
	private void ResetPattern (TankPatternType pType)
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
	private IEnumerator IChangeColor (Color color, float duration)
	{
		float timer = 0f;
		while (timer <= duration) {
			mat.SetColor ("_Color", Color.Lerp(mat.GetColor("_Color"), deadColor , timer/duration));
			timer += Time.deltaTime;
			yield return null;
		}
	}
}
