using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager {
	public GameSystemService serviceInstance;

	public Canvas canvas;
	public GameObject text_damage;
	public GameObject text_critical;
	public GameObject text_evasion;
    public GameObject touchUIRoot;

	public Transform text_damage_pool;
	public Transform text_critical_pool;
	public Transform text_evasion_pool;

	public Camera renderCamera;

	public List<UISettingState> uiPanelStack = new List<UISettingState>();
	public Dictionary<UISettingState, GameObject> uiPanelDic = new Dictionary<UISettingState, GameObject>();
	public int pauseButtonIndx = 0;
	public UIManager()
	{
		serviceInstance = GameSystemService.Instance;
	}

	public void Init ()
	{
		text_damage = Resources.Load<GameObject> ("Prefabs/UI/Texts/Text_Damage");
		text_critical = Resources.Load<GameObject> ("Prefabs/UI/Texts/Text_Critical");
		text_evasion = Resources.Load<GameObject> ("Prefabs/UI/Texts/Text_Evasion");
        touchUIRoot = GameObject.Find("Touch_Input_Root");

#if UNITY_ANDROID
        if (touchUIRoot != null)
        {
            touchUIRoot.SetActive(true);
        }
#else
        if (touchUIRoot != null)
        {
            touchUIRoot.SetActive(false);
        }
#endif
        renderCamera = Camera.main;

		canvas = GameObject.Find ("Canvas").GetComponent<Canvas>();
		if (null == text_damage_pool) {
			text_damage_pool = new GameObject ("Text_Damage_Pool").transform;
			text_damage_pool.transform.parent = canvas.transform;
			text_damage_pool.transform.localPosition = Vector3.zero;
			text_damage_pool.transform.localScale = Vector3.one;
		}
		if (null == text_critical_pool) {
			text_critical_pool = new GameObject ("Text_Critical_Pool").transform;
			text_critical_pool.transform.parent = canvas.transform;
			text_critical_pool.transform.localPosition = Vector3.zero;
			text_critical_pool.transform.localScale = Vector3.one;
		}
		if (null == text_evasion_pool) {
			text_evasion_pool = new GameObject ("Text_Evasion_Pool").transform;
			text_evasion_pool.transform.parent = canvas.transform;
			text_evasion_pool.transform.localPosition = Vector3.zero;
			text_evasion_pool.transform.localScale = Vector3.one;
		}

		for (int i = 0; i < 50; ++i)
		{
			var tmpDam = GameObject.Instantiate (text_damage, text_damage_pool).GetComponent<DamageText>();
			var tmpCri = GameObject.Instantiate (text_critical, text_critical_pool).GetComponent<DamageText>();
			var tmpEva = GameObject.Instantiate (text_evasion, text_evasion_pool).GetComponent<DamageText>();
			tmpDam.GetComponent<CanvasRenderer>().cull = (true);
			tmpCri.GetComponent<CanvasRenderer>().cull = (true);
			tmpEva.GetComponent<CanvasRenderer>().cull = (true);
		}

		uiPanelDic[UISettingState.PAUSE] = serviceInstance.pausePanel;
		uiPanelDic[UISettingState.SETTING] = serviceInstance.settingPanel;
		uiPanelDic[UISettingState.SOUND] = serviceInstance.soundPanel;
		uiPanelDic[UISettingState.KEYMAP] = serviceInstance.keyPanel;

		serviceInstance.setGameStateEvent += delegate(GameState newState)
		{
			var prevState = serviceInstance.gameState;
			if (newState == GameState.PAUSE)
			{
				PushAndOpenUIPanel((int)UISettingState.PAUSE);
				var soundSources = GameObject.FindObjectsOfType<AudioSource>();
				for (int i = 0; i < soundSources.Length; i++)
				{
					soundSources[i].Pause();
				}
				Time.timeScale = 0f;
			}
			else if (newState == GameState.RUNNING && prevState == GameState.PAUSE)
			{
				serviceInstance.pausePanel.SetActive(false);
				var soundSources = GameObject.FindObjectsOfType<AudioSource>();
				for (int i = 0; i < soundSources.Length; i++)
				{
					soundSources[i].UnPause();
				}
				Time.timeScale = 1f;
			}
		};
	}
	public void PushAndOpenUIPanel (int uiSettingState)
	{
		UISettingState targetState = (UISettingState)(uiSettingState);
		uiPanelDic[targetState].SetActive (true);
		if (!uiPanelStack.Contains(targetState))
			uiPanelStack.Add (targetState);
	}
	public void PushAndOpenUIPanel(UISettingState uiSettingState)
	{
		uiPanelDic[uiSettingState].SetActive (true);
		if (!uiPanelStack.Contains(uiSettingState))
			uiPanelStack.Add (uiSettingState);
	}
	public void HoverPauseButton (int idx)
	{
		serviceInstance.pauseButtonShdwList[pauseButtonIndx].gameObject.SetActive(false);
		pauseButtonIndx = idx;
		serviceInstance.pauseButtonShdwList[idx].gameObject.SetActive (true);
	}
	public void IncreasePauseButtonIndex()
	{
		pauseButtonIndx = (pauseButtonIndx + 1) % serviceInstance.pauseButtonList.Count;
	}
	public void DecreasePauseButtonIndex()
	{
		pauseButtonIndx = (pauseButtonIndx - 1) % serviceInstance.pauseButtonList.Count;
	}
	public void SelectPauseButton ()
	{
		serviceInstance.pauseButtonList[pauseButtonIndx].GetComponent<Button>().onClick.Invoke();
	}
	public void PopAndCloseUIPanel ()
	{
		if (uiPanelStack.Count != 0)
		{
			var lastCount = uiPanelStack.Count - 1;
			uiPanelDic[uiPanelStack[lastCount]].SetActive(false);
			if (uiPanelStack[lastCount] == UISettingState.PAUSE)
			{
				serviceInstance.SetGameState(GameState.RUNNING);
			}
			uiPanelStack.RemoveAt(lastCount);
		}
	}

	public void PopDamageText (Vector3 position, float damage)
	{
		position = renderCamera.WorldToScreenPoint (position);
		for (int i = 0; i < text_damage_pool.childCount; ++i)
		{
			var tmpDam = text_damage_pool.GetChild (i).GetComponent<DamageText>();
			if (tmpDam.IsBusy ())
				continue;
			tmpDam.damaged_dir = Vector3.one.normalized;
			tmpDam.PlayUIAnimation (position,damage.ToString());
			return;
		}
	}
	public void PopDamageText (Vector3 position, float damage, Vector3 nor_dir)
	{
		position = renderCamera.WorldToScreenPoint (position);
		for (int i = 0; i < text_damage_pool.childCount; ++i)
		{
			var tmpDam = text_damage_pool.GetChild (i).GetComponent<DamageText>();
			if (tmpDam.IsBusy ())
				continue;
			tmpDam.damaged_dir = nor_dir;
			tmpDam.GetComponent<DamageText> ().PlayUIAnimation (position,damage.ToString());
			return;
		}
	}
	public void PopCriticalText (Vector3 position, float damage)
	{
		position = renderCamera.WorldToScreenPoint (position);
		for (int i = 0; i < text_critical_pool.childCount; ++i)
		{
			var tmpCri = text_critical_pool.GetChild (i).GetComponent<DamageText>();
			if (tmpCri.IsBusy ())
				continue;
			tmpCri.damaged_dir = Vector3.one.normalized;
			tmpCri.PlayUIAnimation (position,((int)damage).ToString());
			return;
		}
	}
	public void PopCriticalText (Vector3 position, float damage, Vector3 nor_dir)
	{
		position = renderCamera.WorldToScreenPoint (position);
		for (int i = 0; i < text_critical_pool.childCount; ++i)
		{
			var tmpCri = text_critical_pool.GetChild (i).GetComponent<DamageText>();
			if (tmpCri.IsBusy ())
				continue;
			tmpCri.damaged_dir = nor_dir;
			tmpCri.PlayUIAnimation (position,((int)damage).ToString());
			return;
		}
	}
	public void PopEvasionText (Vector3 position)
	{
		position = renderCamera.WorldToScreenPoint (position);
		for (int i = 0; i < text_evasion_pool.childCount; ++i)
		{
			var tmpEva = text_evasion_pool.GetChild (i).GetComponent<DamageText>();
			if (tmpEva.IsBusy ())
				continue;
			tmpEva.damaged_dir = Vector3.one.normalized;
			tmpEva.PlayUIAnimation (position,"MISS");
			return;
		}
	}
	public void PopEvasionText (Vector3 position, Vector3 nor_dir)
	{
		position = renderCamera.WorldToScreenPoint (position);
		for (int i = 0; i < text_evasion_pool.childCount; ++i)
		{
			var tmpEva = text_evasion_pool.GetChild (i).GetComponent<DamageText>();
			if (tmpEva.IsBusy ())
				continue;
			tmpEva.damaged_dir = nor_dir;
			tmpEva.PlayUIAnimation (position,"MISS");
			return;
		}
	}
	public void CloseDamageText (DamageText damText)
	{
		damText.StopUIAnimation ();
		damText.gameObject.SetActive (false);
	}
}