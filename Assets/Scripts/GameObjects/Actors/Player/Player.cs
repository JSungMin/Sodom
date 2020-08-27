using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using EventArgumentNamespace;
using InformationNamespace;

[RequireComponent(typeof(PlayerActionFSM))]
public class Player : FrameBaseActor, ISaveableActor {
	public static Player instance;
	public EquipInfo equipInfo;
	public InventoryInfo invenInfo;
	public QuestProgressInfo questInfo;
	public Vector3 interactHalfExtend = Vector3.one * 0.5f;
	public LayerMask ignoreInteractMask;

	public Transform attackParticlePivot;
	public GameObject attackParticle;
	public GameObject counterParticle;

	public ColorFlicker damFlicker;
	public UnityEngine.Audio.AudioMixer audioMixer;
	public IEnumerator counterSFX;

	public event EventHandler<ActorCollisionEventArg>	RaiseActorWallCollision;

	// Use this for initialization
	public new void Start ()	
    {
		base.Start ();
		//serviceInstance.LoadGame ();
		if (null == instance)
			instance = this;
		//	Implement FSM Transition Graph
		//	fsm.InitFSMStates ();	ActorInit에서 이제 처리함
		//  Implement Init Func in SaveableActor
		Init ();   
		actorInfo.stage_name = SceneManager.GetActiveScene ().name;

		//	Subscribe Events
		serviceInstance.Add_MoveKeyDown_Subscriber (HandleMoveStartEvent);
		serviceInstance.Add_InteractKeyDown_Subscriber (HandleInteractEvent);
		serviceInstance.Add_MoveKeyPressed_Subscriber (HandleMoveStartEvent);
		serviceInstance.Add_MoveKeyUp_Subscriber (HandleMoveStopEvent);

		RaiseActorCollisionEnter += HandleGroundEnter;
		RaiseActorCollisionEnter += HandleWallCollision;
		RaiseActorCollisionStay += HandleGroundStay;
		RaiseActorCollisionExit += HandleGroundExit;
		RaiseActorLand += HandleLandEvent;
		RaiseActorAir += HandleAirEvent;
		RaiseActorAttackSuccess += HandleAttackParticle;
		RaiseActorCounter += HandleCounterParitcle;
		RaiseActorCounter += (object sender, ActorDamagedEventArg e) => {
			if (null != counterSFX)
				StopCoroutine (counterSFX);
			counterSFX = serviceInstance.soundManager.IPlayCounterSFX(0.05f, 0.4f, 0.05f);
			StartCoroutine(counterSFX);
			PlaySound (AudioType.PLAYER_COUNTER);
			serviceInstance.VibratePad(2.5f, 2.5f, 0.5f);
		};

		damFlicker.Initialize ();
        var rollingState = fsm.GetState<RollingState>();
        rollingState.OnEnter += delegate ()
        {
            serviceInstance.ClearSkillBuffer();
        };
		var damageState = fsm.GetState<DamagedState>();
		damageState.OnEnter += delegate() {
			damFlicker.Initialize ();
			StartCoroutine (damFlicker.OnDamageFlick);
			float vibrateAmount = 0f;
			vibrateAmount = (int)damageState.damageInfo.skillInfo.vibrateAmount * 5f;
			float dir = Mathf.Sign(damageState.damageInfo.attacker.transform.position.x - transform.position.x);
			if (dir < 0f)
				serviceInstance.VibratePad(vibrateAmount, 0f, 0.5f);
			else if (dir > 0f)
				serviceInstance.VibratePad(0f, vibrateAmount, 0.5f);
		};
		fsm.GetState<DeadState>().OnEnter += delegate() {
			serviceInstance.deadPanel.SetActive(true);
		};
		fsm.GetState<DeadState> ().OnUpdate += delegate() {
			if (Input.anyKeyDown)
			{
				Time.timeScale = 1f;
				SceneManager.LoadScene (actorInfo.stage_name);
			}
		};
	}

	public void Update()
	{
		base.Update ();
		skillCoolTimer.CoolingAll ();

		if (Input.GetKeyDown (KeyCode.Alpha1)) 
		{
			SceneManager.LoadScene ("Stage01");
		}
		else if (Input.GetKeyDown (KeyCode.Alpha2))
		{
			SceneManager.LoadScene ("Stage02");
		}
		else if (Input.GetKeyDown (KeyCode.Alpha3))
		{
			SceneManager.LoadScene ("Stage03");
		}
	}

	public void HandleGroundEnter (object sender, ActorCollisionEventArg arg)
	{
		if (arg.col.collider.CompareTag ("Ground"))
		{
			if (arg.col.collider.bounds.max.y > bodyCollider.bounds.min.y + 0.01f)
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
	public void HandleWallCollision (object sender, ActorCollisionEventArg arg)
	{
		if (fsm.nowState != fsm.GetState<DamagedState>())
		{
			return;
		}
		if (Mathf.Abs(arg.col.impulse.x) >= 5f)
		{
			Debug.Log ("벽꿍 : " + arg.col.impulse.x);
			serviceInstance.VibrateCameraByAttack (0.65f, 3f, 0.25f);
			SetLookDirection (-Mathf.Sign(arg.col.impulse.x));
			actorInfo.isWallCollision = true;
			if (arg.col.impulse.x > 0f)
				serviceInstance.VibratePad(0f, 2f, 0.5f);
			else if (arg.col.impulse.x < 0f)
				serviceInstance.VibratePad(2f, 0f, 0.5f);
			if (RaiseActorWallCollision != null)
			{
				RaiseActorWallCollision.Invoke(sender, arg);
			}
		}
	}
	public void HandleMoveStartEvent (object sender, KeyEventArgs arg)
	{
		if (!canMoveControl)
			return;
		lookDirection = -(Mathf.Sign(transform.localScale.x));

		if (arg.element == PlayerInputManager.keyMap["LEFT"].input_key)
		{
			lookDirection = -1f;
		}
		else if (arg.element == PlayerInputManager.keyMap["RIGHT"].input_key) 
		{
			lookDirection = 1f;
		}
		else if (arg.element == PlayerInputManager.keyMap["DOWN"].input_key)
		{
			HandleSitEvent ();
			return;
		}
		else
			return;

		PlayerInputManager.pressedMoveInput = true;
		fsm.TryTransferAction<WalkStartState> ();
		var tmpScale = transform.localScale;
		tmpScale.x = -Mathf.Abs (tmpScale.x) * Mathf.Sign(lookDirection);
		transform.localScale = tmpScale;
	}
	public void HandleMoveStopEvent (object sender, KeyEventArgs arg)
	{
		PlayerInputManager.pressedMoveInput = false;
		if (arg.element == PlayerInputManager.keyMap["DOWN"].input_key)
			PlayerInputManager.pressedSitInput = false;
	}
	public void HandleInteractEvent (object sender, KeyEventArgs arg)
	{
		
	}
	public void HandleSitEvent ()
	{
		if (fsm.TryTransferAction<SitState> ())
		    PlayerInputManager.pressedSitInput = true;
	}
	public void HandleLandEvent(object sender, ActorLandEventArg arg)
	{
		//Debug.Log ("Player Land");
		actorInfo.canDoAirBehaviour = true;
		if (rigid.velocity.y <= -10f)
			fsm.TryTransferAction<StandState> ();
		var vel = rigid.velocity;
		vel.y = 0;
		rigid.velocity = vel;
		var adjustPos = transform.position;
		adjustPos.y = arg.plane.bounds.max.y;
		transform.position = adjustPos;
		if (actorInfo.GetLifePercent () <= 0f)
			return;
		var isDammagedState = fsm.nowState == fsm.GetState<DamagedState>();
		if (!isDammagedState)
			return;
		//	Blow효과에 의한 Wake Up 실행
		if (actorInfo.isWallCollision) 
		{
			actorInfo.isWallCollision = false;
			fsm.TryTransferAction<DownState> ();
			//PlayAnimation (0, "Player01_Wake_Up", false, 0f);
		}
		else if (GetDamageType() == SkillDamageType.THROW)
		{
			fsm.TryTransferAction<DownState> ();
		}
	}
	public void HandleAirEvent(object sender, ActorAirEvnetArg arg)
	{
		fsm.TryTransferAction<FallState> ();
		//fsm.TryTransferActionToFall ();
	}
	public void HandleAttackParticle (object sender, ActorDamagedEventArg e)
	{	
		if (e.damageInfo.skillInfo.skillCounterType != SkillCounterType.NONE)
			return;
		var attkParticle = GameObject.Instantiate (attackParticle, attackParticlePivot);
			attkParticle.transform.position = attackParticlePivot.transform.position;
			attkParticle.transform.localScale = new Vector3(-transform.localScale.x, 1f, 1f);
			attkParticle.GetComponent<ParticleSystem>().Play();
		float vibrateAmount = (int)e.damageInfo.skillInfo.vibrateAmount * 5f; 
		serviceInstance.VibratePad(vibrateAmount, vibrateAmount, 0.25f);
	}
	public void HandleCounterParitcle (object sender, ActorDamagedEventArg e)
	{
		var cuntParticle = GameObject.Instantiate (counterParticle, attackParticlePivot);
			cuntParticle.transform.position = attackParticlePivot.transform.position;
			cuntParticle.transform.localScale = new Vector3(-transform.localScale.x, 1f, 1f);
			cuntParticle.GetComponent<ParticleSystem>().Play();
	}
	public void AddQuest (string questName)
	{
		if (serviceInstance.StartQuest (questName))
			questInfo.AddProgressQuest (serviceInstance.GetQuestInfo (questName));
	}

	public override SkillInfo GetUsedSkill ()
	{
		if (recentlyUsedSkillName == "")
			return null;
		return serviceInstance.GetPlayerSkillInfoByName (recentlyUsedSkillName);
	}
	public SkillDamageType GetDamageType ()
	{
		return fsm.GetState<DamagedState> ().GetDamageType ();
	}

	#region ISaveLoadable implementation

	public bool Init ()
	{
		serviceInstance.Add_SaveActor_Subscriber (Save);
		serviceInstance.Add_LoadActor_Subscriber (Load);
		return true;
	}

	public void Save (object sender, SaveActorArgs arg)
	{
		//	모든 상태 이상을 제거한다.
		ResetAllConditions ();
		actorInfo.lastPosition = transform.position;
		actorInfo.lastScale = transform.localScale;
		actorInfo.lastVelocity = rigid.velocity;
		arg.actorInfo = JsonUtility.ToJson(actorInfo);
		arg.actorEquipInfo = JsonUtility.ToJson (equipInfo);
		arg.actorInvenInfo = JsonUtility.ToJson (invenInfo);
		arg.actorQuestProgressInfo = JsonUtility.ToJson (questInfo);
	}

	public void Load (object sender, LoadActorArgs arg)
	{
		actorInfo = arg.GetElement ().actorInfo;
		actorInfo.Init (this);
		if (actorInfo.actorType == ActorType.PLAYER) {
			var player = arg.GetElement () as Player;
			equipInfo = player.equipInfo;
			invenInfo = player.invenInfo;
			questInfo = player.questInfo;
			questInfo.SyncQuestWithListener ();
		}
		actorInfo.SyncSkillMapWithList ();
		if (!actorInfo.isGrounded) {
			OnActorAir (this);
		}
		transform.position = actorInfo.lastPosition;
		transform.localScale = actorInfo.lastScale;
		rigid.velocity = actorInfo.lastVelocity;
	}
	#endregion
}
