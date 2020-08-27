using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventArgumentNamespace;
using ActorConditionNamespace;

public class GameSystemService : MonoBehaviour {
	public static GameSystemService instance;
	[Header("Manager Area")]
	public SaveManager saveManager;
	public PlayerInputManager playerInputManager;
	public ActorConditionManager conditionManager;
	public ActorInteractManager interactManager;
	public CameraManager camManager;
	public SoundManager soundManager;
	public UIManager uiManager;
	[Header("Timer Area")]
	public GameSystemTimer systemTimer;
	public PlayerComboTimer comboTimer;
	[Header("Factory Area")]
	public SkillFactory skillFactory;
	public QuestFactory questFactory;

	[Header("Utility Property")]
	public float damageVibrationAmount = 0.25f;
	public AnimationCurve damageVibrationCurve;
	public AnimationCurve padVibrateCurve;
	public List<Actor> inGameActorList;
	public Camera renderCamera;

	[HideInInspector]
	public GameObject bulletPool;
	public GameState gameState;
	public delegate void GameStateDel (GameState newState);
	public GameStateDel setGameStateEvent;
	public GameObject pausePanel;
	public GameObject settingPanel;
	public GameObject soundPanel;
	public GameObject keyPanel;
	public GameObject deadPanel;
	public List<GameObject> pauseButtonList = new List<GameObject>();
	public List<GameObject> pauseButtonShdwList = new List<GameObject>();

	public static bool usePadVibration = true;

	public static GameSystemService Instance
	{
		get {
			if (null == instance)
			{
                instance = FindObjectOfType<GameSystemService>();
                if (null == instance)
                {
				    instance = new GameObject ("GSS").AddComponent<GameSystemService> ();
                    //	Initialize Managers
                    instance.InitializeManagers();
                }
			}
			return instance;
		}
	}
	private void InitializeObjectPool()
	{
		bulletPool = GameObject.Find ("BULLET_POOL");
		if (null == bulletPool) {
			bulletPool = new GameObject ("BULLET_POOL");
		}
	}
	private void InitializeManagers ()
	{
		if (null == skillFactory)
		{
			skillFactory = GameObject.FindObjectOfType<SkillFactory> ();
			if (null == skillFactory) {
				skillFactory = new GameObject ("SF").AddComponent<SkillFactory> ();
				skillFactory.transform.parent = transform;
			}
			skillFactory.Init ();
            Debug.Log("SF");
        }
		if (null == playerInputManager)
		{
			playerInputManager = GameObject.FindObjectOfType<PlayerInputManager>();
			if (null == playerInputManager)
			{
				playerInputManager = new GameObject("PIM").AddComponent<PlayerInputManager> ();
				playerInputManager.transform.parent = transform;
			}
			playerInputManager.Init ();
            Debug.Log("PIM");
        }
		if (null == soundManager)
		{
			soundManager = GameObject.FindObjectOfType<SoundManager>();
			if (null == soundManager)
			{
				soundManager = new GameObject("SM").AddComponent<SoundManager> ();
				soundManager.transform.parent = transform;
			}
			soundManager.Init ();
            Debug.Log("SM");
        }
		if (null == comboTimer) {
			comboTimer = GameObject.FindObjectOfType<PlayerComboTimer> ();
			if (null == comboTimer) {
				comboTimer = new GameObject ("CT").AddComponent<PlayerComboTimer> ();
				comboTimer.transform.parent = transform;
			}
			comboTimer.Init ();
            Debug.Log("CT");
        }
		if (null == conditionManager) {
            conditionManager = GameObject.FindObjectOfType<ActorConditionManager>();      
			if (null == conditionManager) {
				conditionManager = new GameObject ("ACM").AddComponent<ActorConditionManager> ();
				conditionManager.transform.parent = transform;
				conditionManager.Init ();
			}
            Debug.Log("ACM");
            inGameActorList = new List<Actor> (GameObject.FindObjectsOfType<Actor> ());
		}
		if (null == interactManager)
			interactManager = new ActorInteractManager ();
		if (null == uiManager) {
			uiManager = new UIManager ();
			uiManager.Init ();
			SetUIRenderCamera (renderCamera);
            Debug.Log("UIM");
        }
		if (null == camManager) {
			camManager = GameObject.FindObjectOfType<CameraManager> ();
			if (null == camManager) {
				camManager = new GameObject ("CM").AddComponent<CameraManager> ();
				camManager.transform.parent = transform;
			}
			camManager.Init ();
            Debug.Log("CM");
        }
        Debug.Log("GSS Manager Completly Setup");
	}

	void Awake ()
	{
        Debug.Log("Awake GSS");
        Debug.Log ("Awake GSS");
		Physics.gravity = Vector3.down * 20f;
		instance = this;
        Application.targetFrameRate = 60;
		InitializeObjectPool ();
		//	Initialize Managers
		InitializeManagers ();
	}
	void OnEnable()
	{
		Time.timeScale = 1f;
	}
	void OnDisable()
	{
		//GamePad.SetVibration(0, 0f, 0f);
	}
	private IEnumerator IStopFrame (float duration)
	{
		Time.timeScale = 0;
		yield return new WaitForSecondsRealtime (duration);
		Time.timeScale = 1f;
	}
	private IEnumerator IVibratePad (float leftAmount, float rightAmount, float duration)
	{
		float timer = 0f;
		float delta = 0f;
		float curve = 0f;
		while (timer <= duration)
		{
			timer += Time.deltaTime;
			delta = timer / duration;
			curve = padVibrateCurve.Evaluate(delta);
			//GamePad.SetVibration(0, leftAmount * curve, rightAmount * curve);
			yield return null;
		}
		//GamePad.SetVibration(0, 0f, 0f);
	}
	//	Exposed Function Zone
	//	아래 올 수 있는 Function들은 모두 Timer, Manager에 구현되어 있는 함수들을 중개해주는 것만 가능하다.
	//	이외에 In Game 동작 중 필요한 함수들은 InGameUtility 클래스에 구현되어 있다.

	public void StopFrame (float duration)
	{
		StartCoroutine (IStopFrame (duration));
	}
	public void LoadScene(string name)
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(name);
	}
	public void SetGameState (GameState newState)
	{
		var handler = setGameStateEvent;
		if (null != handler)
		{
			handler.Invoke(newState);
		}
		gameState = newState;
	}
	public void SetGameState (int stateNum)
	{
		SetGameState ((GameState)(stateNum));
	}
	public void VibratePad (float leftAmount, float rightAmount, float duration)
	{
		if (usePadVibration)
			StartCoroutine(IVibratePad(leftAmount, rightAmount, duration));
	}
	public bool TryWalk (Actor actor, float dir)
	{
		return interactManager.OnActorWalk (actor);
	}
	public bool TrySit (Actor actor)
	{
		return interactManager.OnActorSit (actor);
	}
	public bool TryAttack (Actor attacker, InformationNamespace.SkillInfo skillInfo)
	{
		return interactManager.OnActorAttackTry (attacker, skillInfo);
	}
	public bool TryDamage (InformationNamespace.DamageInfo damageInfo)
	{
		return interactManager.OnActorDamaged (damageInfo);
	}
	public bool TryJump (Actor actor)
	{
		return interactManager.OnActorJump (actor);
	}
	public void AddBufferSkill (InformationNamespace.SkillInfo skillInfo)
	{
		comboTimer.AddBufferSkill (skillInfo);
	}
	public void ClearSkillBuffer ()
	{
		comboTimer.ClearSkillBuffer ();
	}
	public bool CheckSkillBufferEmpty ()
	{
		return comboTimer.CheckSkillBufferEmpty ();
	}
	public bool StartQuest (string questName)
	{
		return questFactory.StartQuest (questName);
	}
	public InformationNamespace.QuestInfo GetQuestInfo (string questName)
	{
		return questFactory.GetQuestInfoByName (questName);
	}
	public void AddActorCondition (Actor actor, ActorConditionType con, float duration, float effectiveness)
	{
		actor.actorInfo.conditionInfo.AddCondition (con, duration, effectiveness);
		// Then ObservedList applierList에 새 컨디션 등록
		// Update에 SyncCondition에 영향을 받음
	}
	public void RemoveActorCondition (Actor actor, ActorConditionType con, ConditionApplier applier)
	{
		actor.actorInfo.conditionInfo.RemoveCondition (con, applier);
	}
	public void VibrateCameraByAttack (float amplie, float freq, float duration)
	{
		camManager.VibrateCameraByAttack (amplie, freq, duration);
	}
	public void SlowMotionCamera (float slowRatio, float duration)
	{
		camManager.SlowMotionCamera (slowRatio, duration);
	}
	public void StopSlowMotionCamera()
	{
		camManager.StopSlowMotionCamera ();
	}
	public void ZoomMotionCamera (float zoomAmount, float duration)
	{
		camManager.ZoomInCamera (zoomAmount, duration);
	}
	public void StopZoomMotionCamera (float zoomAmount, float duration)
	{
		camManager.ZoomOutCamera ();
	}
	public InformationNamespace.SkillInfo GetPlayerSkillInfoByName (string skillName)
	{
		return skillFactory.GetPlayerSkillInfoByName (skillName);
	}
	public InformationNamespace.SkillInfo GetEnemySkillInfoByName (string skillName)
	{
		return skillFactory.GetEnemySkillInfoByName (skillName);	
	}
	public void PopAndCloseUIPanel()
	{
		uiManager.PopAndCloseUIPanel();
	}
	public void PushAndOpenUIPanel (int uiSettingState)
	{
		uiManager.PushAndOpenUIPanel(uiSettingState);
	}
	public void PushAndOpenUIPanel (UISettingState uiSettingState)
	{
		uiManager.PushAndOpenUIPanel (uiSettingState);
	}
	public void SetUsePadVibrate (bool use)
	{
		usePadVibration = !usePadVibration;
	}

	public void SaveGame ()
	{
		saveManager.PackingGameData ();	
	}
	public void LoadGame()
	{
		//saveManager.UnPackingGameData ();
	}
	public InformationNamespace.GameDataInfo GetNowGameDataInfo()
	{
		return saveManager.nowGameData;
	}
	public void ExitGame (object sender)
	{
		systemTimer.ExitGame (sender);
	}
	public List<InformationNamespace.KeyInputInfo> GetMoveKeyList()
	{
		return playerInputManager.move_List;
	}
	public void SetUIRenderCamera(Camera camera)
	{
		uiManager.renderCamera = camera;
	}
	public Camera GetUIRenderCamera()
	{
		return uiManager.renderCamera;
	}
	//	Sound Manager UnBoxing Block
	public void PlaySound (AudioType audioType, bool overlap = false, float delay = 0f)
	{
		soundManager.PlaySound(audioType, overlap, delay);
	}
	public void StopSound (AudioType audioType)
	{
		soundManager.StopSound(audioType);
	}
	public void SetSnapshot (SnapshotType snapshotType, float transTime)
	{
		soundManager.SetSnapshot (snapshotType, transTime);
	}
	//	Sound Manager UnBoxing Block End
	public void PopDamageText (Vector3 position, float damage)
	{
		uiManager.PopDamageText (position, damage);
	}
	public void PopCriticalText (Vector3 position, float damage)
	{
		uiManager.PopCriticalText (position, damage);
	}
	public void PopEvasionText (Vector3 position)
	{
		uiManager.PopEvasionText (position);
	}
	public void PopDamageText (Vector3 position, float damage, Vector3 nor_dir)
	{
		uiManager.PopDamageText (position, damage, nor_dir);
	}
	public void PopCriticalText (Vector3 position, float damage, Vector3 nor_dir)
	{
		uiManager.PopCriticalText (position, damage, nor_dir);
	}
	public void PopEvasionText (Vector3 position, Vector3 nor_dir)
	{
		uiManager.PopEvasionText (position, nor_dir);
	}
	public bool IsRunningGame ()
	{
		return systemTimer.flag_Game_Run_State;
	}
	public CommandNode GetGroundSkillTree ()
	{
		return skillFactory.savedPlayerSkillInfoList.gSkillTreeHead;
	}
	public CommandNode GetAirSkillTree()
	{
		return skillFactory.savedPlayerSkillInfoList.aSkillTreeHead;
	}
	public bool CheckSkillCounter(SkillCounterType victim, SkillCounterType attacker)
	{
		return skillFactory.CheckSkillCounterValidation (victim, attacker);
	}
	public InformationNamespace.AllSkillInfo GetAllSkillInfo()
	{
		return skillFactory.savedPlayerSkillInfoList.allPlayerSkillInfo;
	}

    //	Subscriber Register Zone
	public void Add_AttackKeyDown_Subscriber(EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseAttackKeyDownEvent += arg;	
	}
	public void Remove_AttackKeyDown_Subscriber(EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseAttackKeyDownEvent -= arg;
	}
	public void Add_MoveKeyDown_Subscriber (EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseMoveKeyDownEvent += arg;	
	}
	public void Remove_MoveKeyDown_Subscriber (EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseMoveKeyDownEvent -= arg;	
	}
	public void Add_MoveKeyPressed_Subscriber (EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseMoveKeyPressedEvent += arg;	
	}
	public void Remove_MoveKeyPressed_Subscriber (EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseMoveKeyPressedEvent -= arg;	
	}
	public void Add_JumpKeyPressed_Subscriber (EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseJumpKeyPressedEvent += arg;
	}
	public void Remove_JumpKeyPressed_Subscriber (EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseJumpKeyPressedEvent -= arg;
	}
	public void Add_InteractKeyDown_Subscriber (EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseInteractEvent += arg;
	}
	public void Remove_InteractKeyDown_Subscriber (EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseInteractEvent -= arg;
	}
	public void Add_MoveKeyUp_Subscriber (EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseMoveKeyUpEvent += arg;	
	}
	public void Remove_MoveKeyUp_Subscriber (EventHandler<KeyEventArgs> arg)
	{
		playerInputManager.RaiseMoveKeyUpEvent -= arg;	
	}
	public void Add_ActorAttackTry_Subscriber(Actor actor, EventHandler<ActorAttackEventArg> arg)
	{
		actor.RaiseActorAttackTry += arg;
	}
	public void Remove_ActorAttackTry_Subscriber(Actor actor, EventHandler<ActorAttackEventArg> arg)
	{
		actor.RaiseActorAttackTry -= arg;
	}
	public void Add_ActorAttackSuccess_Subscriber(Actor actor, EventHandler<ActorDamagedEventArg> arg)
	{
		actor.RaiseActorAttackSuccess += arg;
	}
	public void Remove_ActorAttackSuccess_Subscriber(Actor actor, EventHandler<ActorDamagedEventArg> arg)
	{
		actor.RaiseActorAttackSuccess -= arg;
	}
	public void Add_ActorKill_Subscriber(Actor actor, EventHandler<ActorDamagedEventArg> arg)
	{
		actor.RaiseActorKill += arg;
	}
	public void Remove_ActorKill_Subscriber(Actor actor, EventHandler<ActorDamagedEventArg> arg)
	{
		actor.RaiseActorKill -= arg;
	}
	public void Add_ActorDead_Subscriber(Actor actor, EventHandler<ActorDamagedEventArg> arg)
	{
		actor.RaiseActorDead += arg;
	}
	public void Remove_ActorDead_Subscriber(Actor actor, EventHandler<ActorDamagedEventArg> arg)
	{
		actor.RaiseActorDead -= arg;
	}
	public void Add_ActorDamaged_Subscriber(Actor actor, EventHandler<ActorDamagedEventArg> arg)
	{
		actor.RaiseActorDamaged += arg;
	}
	public void Remove_ActorDamaged_Subscriber (Actor actor, EventHandler<ActorDamagedEventArg> arg)
	{
		actor.RaiseActorDamaged -= arg;
	}
	public void Add_ActorJump_Subscriber(Actor actor, EventHandler<ActorJumpEventArg> arg)
	{
		actor.RaiseActorJump += arg;
	}
	public void Remove_ActorJump_Subscriber(Actor actor, EventHandler<ActorJumpEventArg> arg)
	{
		actor.RaiseActorJump -= arg;	
	}
	public void Add_ActorEquip_Subscriber(Actor actor, EventHandler<ActorEquipEventArg> arg)
	{
		actor.RaiseActorEquip += arg;
	}
	public void Remove_ActorEquip_Subscriber(Actor actor, EventHandler<ActorEquipEventArg> arg)
	{
		actor.RaiseActorEquip -= arg;
	}
	public void Add_ActorInteract_Subscriber(Actor actor, EventHandler<ActorInteractEventArg> arg)
	{
		actor.RaiseActorInteract += arg;
	}
	public void Remove_ActorInteract_Subscriber(Actor actor, EventHandler<ActorInteractEventArg> arg)
	{
		actor.RaiseActorInteract -= arg;
	}
	public void Add_ActorRoot_Subscriber(Actor actor, EventHandler<ActorRootEventArg> arg)
	{
		actor.RaiseActorRoot += arg;
	}
	public void Romove_ActorRoot_Subscriber(Actor actor, EventHandler<ActorRootEventArg> arg)
	{
		actor.RaiseActorRoot -= arg;
	}
	public void Add_RoomCleared_Subscriber(Room room, EventHandler<RoomClearedEventArg> arg)
	{
		room.RaiseRoomCleared += arg;
	}
	public void Remove_RoomCleared_Subscriber(Room room, EventHandler<RoomClearedEventArg> arg)
	{
		room.RaiseRoomCleared -= arg;
	}
	public void Add_SaveActor_Subscriber(EventHandler<SaveActorArgs> arg)
	{
		saveManager.RaiseSaveActorEvent += arg;
	}
	public void Remove_SaveActor_Subscriber(EventHandler<SaveActorArgs> arg)
	{
		saveManager.RaiseSaveActorEvent -= arg;
	}
	public void Add_LoadActor_Subscriber(EventHandler<LoadActorArgs> arg)
	{
		saveManager.RaiseLoadActorEvent += arg;
	}
	public void Remove_LoadActor_Subscriber(EventHandler<LoadActorArgs> arg)
	{
		saveManager.RaiseLoadActorEvent -= arg;
	}
	public void Add_GamePause_Subscriber(EventHandler<SystemTimerArgs> arg)
	{
		systemTimer.RaiseGamePauseEvent += arg;
	}
	public void Remove_GamePause_Subscriber(EventHandler<SystemTimerArgs> arg)
	{
		systemTimer.RaiseGamePauseEvent -= arg;
	}
	public void Add_GameResume_Subscriber(EventHandler<SystemTimerArgs> arg)
	{
		systemTimer.RaiseGameResumeEvent += arg;
	}
	public void Remove_GameResume_Subscriber(EventHandler<SystemTimerArgs> arg)
	{
		systemTimer.RaiseGameResumeEvent -= arg;
	}
	public void Add_GameExit_Subscriber(EventHandler<SystemTimerArgs> arg)
	{
		systemTimer.RaiseGameExitEvent += arg;
	}
	public void Remove_GameExit_Subscriber(EventHandler<SystemTimerArgs> arg)
	{
		systemTimer.RaiseGameExitEvent -= arg;
	}
	public void Add_ApplierTimer_Subscriber(EventHandler<ConditionEventArg> arg)
	{
		conditionManager.RaiseChildThreadUpdate += arg;
	}
	public void Remove_ApplierTimer_Subscriber(EventHandler<ConditionEventArg> arg)
	{
		conditionManager.RaiseChildThreadUpdate -= arg;
	}
	//	End Of Subscriber Register Zone
}
