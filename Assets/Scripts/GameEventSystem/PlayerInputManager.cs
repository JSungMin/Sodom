using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;

public class PlayerInputManager : MonoBehaviour {
    private GameSystemService serviceInstance;
	private SkillFactory skillFactory;
	private SkillInfo highParry;
	private SkillInfo middleParry;
	private SkillInfo lowParry;
	public Player player;
	public static float xAxis, yAxis, rolling;
	public static bool escape;
	private static float pX, pY, pR, dX, dY;
    public static PlayerInputManager instance;
	public static bool pressedMoveInput = false;
	public static bool pressedSitInput = false;

	public static int NUMBER_OF_COMBO_KEY;
	public event EventHandler<KeyEventArgs> RaiseKeyDownEvent;
	public event EventHandler<KeyEventArgs> RaiseKeyUpEvent;
	public event EventHandler<KeyEventArgs> RaiseKeyPressedEvent;
	public event EventHandler<KeyEventArgs> RaiseAttackKeyDownEvent;
	public event EventHandler<KeyEventArgs> RaiseJumpKeyPressedEvent;
	public event EventHandler<KeyEventArgs> RaiseSitKeyPressedEvent;
	public event EventHandler<KeyEventArgs> RaiseInteractEvent;
	public event EventHandler<KeyEventArgs> RaiseMoveKeyDownEvent;
	public event EventHandler<KeyEventArgs> RaiseMoveKeyPressedEvent;
	public event EventHandler<KeyEventArgs> RaiseMoveKeyUpEvent;
	public event EventHandler<KeyEventArgs> RaiseRollingEvent;

	public static Dictionary<string, KeyInputInfo> keyMap = new Dictionary<string, KeyInputInfo>();

    [Header("이동키")]
	public List<KeyInputInfo> move_List = new List<KeyInputInfo>();
	private KeyInputInfo left_key;
	private KeyInputInfo right_key;
    [Header("아이템, 스킬 퀵슬롯 키")]
	public List<KeyInputInfo> quick_List = new List<KeyInputInfo> ();
    [Header("공격키")]
	public List<KeyInputInfo> attack_List = new List<KeyInputInfo> ();
    [Header("점프키")]
	public List<KeyInputInfo> jump_List = new List<KeyInputInfo> ();
	[Header("구르기")]
	public List<KeyInputInfo> rolling_List = new List<KeyInputInfo> ();
    [Header("상호작용")]
	public List<KeyInputInfo> interact_List = new List<KeyInputInfo>();
    [Header("UI 퀵슬롯")]
	public List<KeyInputInfo> ui_List = new List<KeyInputInfo> ();
	[Header("옵션")]
	public KeyInputInfo option = new KeyInputInfo();

	void InitInputList ()
	{

		for (int i = 0; i < move_List.Count; i++) {
			keyMap [move_List [i].name] = move_List [i];
		}
		for (int i = 0; i < quick_List.Count; i++) {
			keyMap [quick_List [i].name] = quick_List [i];
		}
		for (int i = 0; i < attack_List.Count; i++) {
			keyMap [attack_List [i].name] = attack_List [i];
		}
		for (int i = 0; i < jump_List.Count; i++) {
			keyMap [jump_List [i].name] = jump_List [i];
		}
		for (int i = 0; i < rolling_List.Count; i++) {
			keyMap [rolling_List [i].name] = rolling_List [i];
		}
		for (int i = 0; i < interact_List.Count; i++) {
			keyMap [interact_List [i].name] = interact_List [i];
		}
		for (int i = 0; i < ui_List.Count; i++) {
			keyMap [ui_List [i].name] = ui_List [i];
		}

		left_key = keyMap["LEFT"];
		right_key = keyMap["RIGHT"];

		NUMBER_OF_COMBO_KEY = move_List.Count + attack_List.Count;
	}
	public void InitStaticValues()
	{
		pressedMoveInput = false;
		pressedSitInput = false;
		xAxis = 0f;
		yAxis = 0f;
		rolling = 0f;
		pX = 0f;
		pY = 0f;
		pR = 0f;
		dX = 0f;
		dY = 0f;
// #if UNITY_ANDROID
//         MobileInputManager.Init();
//         MobileInputManager.onMoved += delegate (object obj)
//         {
//             if (MobileInputManager.touchPadDir <= 0)
//             {
//                 pressedSitInput = false;
//                 return;
//             }
//             if (!MobileInputManager.downStateBit)
//                 OnAttackKeyDownEvent(move_List[MobileInputManager.touchPadDir - 1].input_key);
//             OnMoveKeyDownEvent(move_List[MobileInputManager.touchPadDir - 1].input_key);
//         };
//         MobileInputManager.onStationary += delegate (object obj)
//         {
//             if (MobileInputManager.touchPadDir <= 0)
//             {
//                 pressedSitInput = false;
//                 return;
//             }
//             if (!MobileInputManager.downStateBit)
//                 OnAttackKeyDownEvent(move_List[MobileInputManager.touchPadDir-1].input_key);
//             OnMoveKeyPressedEvent(move_List[MobileInputManager.touchPadDir - 1].input_key);
//         };
//         MobileInputManager.onEnded += delegate (object obj)
//         {
//             if (MobileInputManager.touchPadDir <= 0)
//                 return;
//             OnMoveKeyUpEvent(move_List[MobileInputManager.touchPadDir-1].input_key);
//         };
// #endif
	}
    public void Init ()
    {
        instance = this;
        player = GameObject.FindObjectOfType<Player> ();
        serviceInstance = GameSystemService.Instance;
		skillFactory = serviceInstance.skillFactory;
		highParry = skillFactory.GetPlayerSkillInfoByName ("Counter_High_Ready");
		middleParry = skillFactory.GetPlayerSkillInfoByName ("Counter_Middle_Ready");
		lowParry = skillFactory.GetPlayerSkillInfoByName ("Counter_Low_Ready");
		InitInputList ();
		InitStaticValues();
    }

	public void Update()
	{
		pX = xAxis;
		pY = yAxis;
		pR = rolling;
		xAxis = Input.GetAxis("Horizontal");
		yAxis = Input.GetAxis("Vertical");
		rolling = Input.GetAxis("Rolling");
		dX = xAxis - pX;
		dY = yAxis - pY;
		
		escape = Input.GetKeyDown(option.input_key)||Input.GetKeyDown("joystick button 7");
		//	Below Code Make Movement Error;;
		//GamePadExtention.Update();

		switch(serviceInstance.gameState)
		{
			case GameState.MENU:

			break;
			case GameState.RUNNING:
				OnRunning();
			if (escape)
			{
				serviceInstance.SetGameState(GameState.PAUSE);
			}
			break;
			case GameState.PAUSE:
			if (escape)
			{
				serviceInstance.PopAndCloseUIPanel();
			}
			break;
		}
		
	}
	public void OnRunning()
	{
  //      #region GamePad Key Recognization
  //      var down = Input.GetKeyDown (keyMap["DOWN"].input_key);
		//var up = Input.GetKeyDown (keyMap["UP"].input_key);
		//var parry = Input.GetKeyDown (keyMap["ATTACK_SPECIAL"].input_key);
		//if (!parry)
		//	parry = Input.GetKeyDown(KeyCode.JoystickButton3);
		////	Get X Axis And Send Event
		//if (GamePadExtention.GetButtonDown(PadButtonType.RIGHT))
		//{
		//	OnMoveKeyDownEvent (KeyCode.RightArrow);
		//	OnAttackKeyDownEvent (KeyCode.RightArrow);
		//}
		//else if (GamePadExtention.GetButtonPress(PadButtonType.RIGHT))
		//{
		//	OnMoveKeyPressedEvent (KeyCode.RightArrow);
		//}
		//else if (GamePadExtention.GetButtonUp(PadButtonType.RIGHT))
		//{
		//	OnMoveKeyUpEvent (KeyCode.RightArrow);
		//}

		//if (GamePadExtention.GetButtonDown(PadButtonType.LEFT))
		//{
		//	OnMoveKeyDownEvent (KeyCode.LeftArrow);
		//	OnAttackKeyDownEvent (KeyCode.LeftArrow);
		//}
		//else if (GamePadExtention.GetButtonPress(PadButtonType.LEFT))
		//{
		//	OnMoveKeyPressedEvent (KeyCode.LeftArrow);
		//}
		//else if (GamePadExtention.GetButtonUp(PadButtonType.LEFT))
		//{
		//	OnMoveKeyUpEvent (KeyCode.LeftArrow);
		//}

		//if (GamePadExtention.GetButtonDown(PadButtonType.UP))
		//{
		//	OnMoveKeyDownEvent (KeyCode.UpArrow);
		//	OnAttackKeyDownEvent (KeyCode.UpArrow);
		//	up = true;
		//}
		//else if (GamePadExtention.GetButtonPress(PadButtonType.UP))
		//{
		//	OnMoveKeyPressedEvent (KeyCode.UpArrow);
		//}
		//else if (GamePadExtention.GetButtonUp(PadButtonType.UP))
		//{
		//	OnMoveKeyUpEvent (KeyCode.UpArrow);
		//}
		
		//if (GamePadExtention.GetButtonDown(PadButtonType.DOWN))
		//{
		//	OnMoveKeyDownEvent (KeyCode.DownArrow);
		//	OnAttackKeyDownEvent (KeyCode.DownArrow);
		//	down = true;
		//}
		//else if (GamePadExtention.GetButtonPress(PadButtonType.DOWN))
		//{
		//	OnMoveKeyPressedEvent (KeyCode.DownArrow);
		//}
		//else if (GamePadExtention.GetButtonUp(PadButtonType.DOWN))
		//{
		//	OnMoveKeyUpEvent (KeyCode.DownArrow);
		//}

		////	Parry Check
		//if (parry && up)
		//{
		//	serviceInstance.ClearSkillBuffer ();
		//	serviceInstance.comboTimer.AddBufferSkill (highParry);
		//	return;
		//}
		//else if (parry && down)
		//{
		//	serviceInstance.ClearSkillBuffer ();
		//	serviceInstance.comboTimer.AddBufferSkill (lowParry);
		//	return;
		//}
  //      #endregion

        #region PC Keyboard Recognization
        foreach (var input in move_List)
		{
			if (Input.GetKeyDown (input.input_key))
			{
				OnAttackKeyDownEvent (input.input_key);
				OnMoveKeyDownEvent (input.input_key);
			}
			if (Input.GetKeyUp (input.input_key)) 
			{
				OnMoveKeyUpEvent (input.input_key);
			}
			if (Input.GetKey (input.input_key)) 
			{
				OnMoveKeyPressedEvent (input.input_key);
			}
		}
		if (Input.GetKeyDown(KeyCode.JoystickButton2))
		{
			OnAttackKeyDownEvent (KeyCode.A);
		}
		else if (Input.GetKeyDown(KeyCode.JoystickButton3))
		{
			OnAttackKeyDownEvent (KeyCode.S);
		}
		foreach (var input in attack_List)
		{
			if (Input.GetKeyDown (input.input_key)) 
			{
				OnAttackKeyDownEvent (input.input_key);
			}
		}
		foreach (var input in rolling_List)
		{
			if (Input.GetKeyDown (input.input_key))
			{
				OnRollingDownEvent (input.input_key);
			}
		}
		foreach (var input in quick_List)
		{
			if (Input.GetKeyDown (input.input_key))
				OnKeyDownEvent (input.input_key);
		}
		foreach(var input in interact_List)
		{
			if (Input.GetKeyDown (input.input_key))
				OnInteractKeyDownEvent (input.input_key);
		}
		foreach(var input in ui_List)
		{
			if (Input.GetKeyDown (input.input_key))
				OnKeyDownEvent (input.input_key);
		}
		if (Input.GetKey(KeyCode.JoystickButton1))
		{
			OnJumpKeyPressedEvent (KeyCode.Space);
		}
		else
		{
			foreach (var input in jump_List)
			{
				if (Input.GetKey (input.input_key))
					OnJumpKeyPressedEvent (input.input_key);
			}
		}
        #endregion
    }
    public void OnKeyDownEvent(KeyCode k)
	{
		EventHandler<KeyEventArgs> handler = RaiseKeyDownEvent;
		if (null != handler) {
			handler (this, new KeyEventArgs (k));
		} 
	}
	public void OnKeyPressedEvent (KeyCode k)
	{
		EventHandler<KeyEventArgs> handler = RaiseKeyPressedEvent;
		if (null != handler)
		{
			handler(this, new KeyEventArgs(k));
		}
	}
	public void OnKeyUpEvent(KeyCode k)
	{
		EventHandler<KeyEventArgs> handler = RaiseKeyUpEvent;
		if (null != handler)
		{
			handler(this, new KeyEventArgs(k));
		}
	}
	public void OnAttackKeyDownEvent (KeyCode k)
	{
		EventHandler<KeyEventArgs> handler = RaiseAttackKeyDownEvent;
		if (null != handler) {
			var flag_reverse_input = false;
			if (player.transform.localScale.x == 1) {
				flag_reverse_input = true;
			}
			//	정방향을 바라보고 스킬을 사용했을 때
			if (!flag_reverse_input) {
				handler (this, new KeyEventArgs (k));
				return;
			}
			//	인풋과 바라보는 방향이 반대기에, input의 반전이 필요 할 때
			if (k == right_key.input_key)
				k = left_key.input_key;
			else if (k == left_key.input_key)
				k = right_key.input_key;
			handler (this, new KeyEventArgs(k));
		}
	}
	public void OnInteractKeyDownEvent (KeyCode k)
	{
		EventHandler<KeyEventArgs> handler = RaiseInteractEvent;
		if (null != handler) {
			handler (this, new KeyEventArgs (k));
		}
	}
	public void OnMoveKeyDownEvent (KeyCode k)
	{
		EventHandler<KeyEventArgs> handler = RaiseMoveKeyDownEvent;
		if (null != handler) {
			handler (this, new KeyEventArgs (k));
		}
	}
	public void OnMoveKeyPressedEvent (KeyCode k)
	{
		EventHandler<KeyEventArgs> handler = RaiseMoveKeyPressedEvent;
		if (null != handler) {
			handler (this, new KeyEventArgs (k));
		}
	}
	public void OnJumpKeyPressedEvent (KeyCode k)
	{
        player.OnActorJump();
		EventHandler<KeyEventArgs> handler = RaiseJumpKeyPressedEvent;
		if (null != handler)
		{
			handler (this, new KeyEventArgs (k));
		}
	}

	public IEnumerator IRollingBufferAging(float timer)
	{
		while (timer >= 0) {
			player.OnActorRolling ();
			timer -= Time.deltaTime;
			yield return null;
		}
	}
    public void StopRollingAging()
    {
        StopCoroutine("IRollingBufferAging");
    }
    public void StartRollingAging()
    {
        StartCoroutine("IRollingBufferAging", 0.2f);
    }
	public void OnRollingDownEvent (KeyCode k)
	{
		EventHandler<KeyEventArgs> handler = RaiseRollingEvent;
		if (player.OnActorRolling ()) {
			StopCoroutine ("IRollingBufferAging");
		}
		else {
			StartCoroutine ("IRollingBufferAging", 0.2f);
		}
		
		if (null != handler) 
		{
			handler (this, new KeyEventArgs (k));
		}
	}
	public void OnMoveKeyUpEvent (KeyCode k)
	{
		EventHandler<KeyEventArgs> handler = RaiseMoveKeyUpEvent;
		if (null != handler) {
			handler (this, new KeyEventArgs (k));
		}
	}
}
