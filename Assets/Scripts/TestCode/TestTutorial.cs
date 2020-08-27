using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventArgumentNamespace;
using UnityEngine.SceneManagement;

public class TestTutorial : MonoBehaviour {
	private GameSystemService serviceInstance;
	public Player player;
	public GameObject counterHighConv;
	public GameObject counterMiddleConv;
	public GameObject counterLowConv;
	public GameObject comboCounterConv;
	public GameObject rolliingConv;
	public int lowCounter = 1;
	public int middleCounter = 1;
	public int highCounter = 1;
	public int comboCounter = 1;
	public int rollingCounter = 1;

	public static bool isTutorial = true;

	public delegate void CounterEvent (int type);
	public CounterEvent OnCounterEvent;

	private InformationNamespace.SkillInfo tmpSkillInfo = null;

	public IEnumerator IHighCounterInput ()
	{
		var loop = true;
		var soundSources = GameObject.FindObjectsOfType<AudioSource>();
		for (int i = 0; i < soundSources.Length; i++)
		{
			soundSources[i].Pause();
		}
		while (loop) 
		{
			if (tmpSkillInfo != null) 
			{
				if (tmpSkillInfo.skillName == "Counter_High_Ready")
					loop = false;
			}
			yield return new WaitForEndOfFrame();
		}
		for (int i = 0; i < soundSources.Length; i++)
		{
			soundSources[i].UnPause();
		}
		Time.timeScale = 1f;
		counterHighConv.SetActive (false);
		Debug.Log ("End Key Input");
	}
	public IEnumerator IMiddleCounterInput ()
	{
		var loop = true;
		var soundSources = GameObject.FindObjectsOfType<AudioSource>();
		for (int i = 0; i < soundSources.Length; i++)
		{
			soundSources[i].Pause();
		}
		while (loop) 
		{
			if (tmpSkillInfo != null) 
			{
				if (tmpSkillInfo.skillName == "Counter_Middle_Ready")
					loop = false;
			}
			yield return new WaitForEndOfFrame();
		}
		for (int i = 0; i < soundSources.Length; i++)
		{
			soundSources[i].UnPause();
		}
		Time.timeScale = 1f;
		counterMiddleConv.SetActive (false);
		Debug.Log ("End Key Input");
	}
	public IEnumerator ILowCounterInput ()
	{
		var loop = true;
		var soundSources = GameObject.FindObjectsOfType<AudioSource>();
		for (int i = 0; i < soundSources.Length; i++)
		{
			soundSources[i].Pause();
		}
		while (loop) 
		{
			if (tmpSkillInfo != null) 
			{
				if (tmpSkillInfo.skillName == "Counter_Low_Ready")
					loop = false;
			}
			yield return new WaitForEndOfFrame();
		}
		for (int i = 0; i < soundSources.Length; i++)
		{
			soundSources[i].UnPause();
		}
		Time.timeScale = 1f;
		counterLowConv.SetActive (false);
		Debug.Log ("End Key Input");
	}
	public IEnumerator IComboCounterInput ()
	{
		var loop = true;
		var soundSources = GameObject.FindObjectsOfType<AudioSource>();
		for (int i = 0; i < soundSources.Length; i++)
		{
			soundSources[i].Pause();
		}
		while (loop) 
		{
			if (Input.anyKeyDown) 
			{
				loop = false;
			}
			yield return null;
		}
		for (int i = 0; i < soundSources.Length; i++)
		{
			soundSources[i].UnPause();
		}
		Time.timeScale = 1f;
		comboCounterConv.SetActive (false);
		Debug.Log ("End Key Input");
	}
	public IEnumerator IRollingInput ()
	{
		var loop = true;
		var soundSources = GameObject.FindObjectsOfType<AudioSource>();
		for (int i = 0; i < soundSources.Length; i++)
		{
			soundSources[i].Pause();
		}
		while (loop) 
		{
			if (PlayerInputManager.rolling != 0f) 
			{
				loop = false;
			}
			yield return null;
		}
		for (int i = 0; i < soundSources.Length; i++)
		{
			soundSources[i].UnPause();
		}
		Time.timeScale = 1f;
		rolliingConv.SetActive (false);
		Debug.Log ("End Key Input");
	}



	// Use this for initialization
	public void Init () {
		PlayerComboTimer.OnSkillBuildUp += delegate(InformationNamespace.SkillInfo skillInfo) {
			tmpSkillInfo = skillInfo;
		};
		serviceInstance = GameSystemService.Instance;
		player = GameObject.FindObjectOfType<Player> ();
		var result = serviceInstance.questFactory.StartQuest ("quest_Z01_02_01");
		if (!result)
			Debug.LogError ("Can't Accept Quest");
	
		OnCounterEvent += (int type) => {
			if (!isTutorial)
				return;
			if (!player.actorInfo.isGrounded)
				return;
			switch (type)
			{
			case 0 :
				if (highCounter > 0)
				{
					highCounter--;
					counterHighConv.SetActive (true);
					Time.timeScale = 0.0001f;
					StartCoroutine ("IHighCounterInput");
				}
				break;
			case 1 :
				if (middleCounter > 0)
				{
					middleCounter--;
					counterMiddleConv.SetActive (true);
					Time.timeScale = 0.0001f;
					StartCoroutine ("IMiddleCounterInput");
				}
				break;
			case 2 :
				if (lowCounter > 0)
				{
					lowCounter--;
					counterLowConv.SetActive (true);
					Time.timeScale = 0.0001f;
					StartCoroutine ("ILowCounterInput");
				}
				break;
			case 3 :
				if (comboCounter > 0)
				{
					comboCounter--;
					comboCounterConv.SetActive (true);
					Time.timeScale = 0.0001f;
					StartCoroutine ("IComboCounterInput");
				}
				break;
			case 5 : 
				if (rollingCounter > 0)
				{
					rollingCounter--;
					rolliingConv.SetActive (true);
					Time.timeScale = 0.0001f;
					StartCoroutine ("IRollingInput");
				}
			break;
			}
		};
		//	Add Quest
		//serviceInstance.questManager.AddQuest("Tutorial01");
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			isTutorial = !isTutorial;
		}
		//if (Input.GetKeyDown (KeyCode.S)) {
		//	serviceInstance.SaveGame ();
		//}
		//if (Input.GetKeyDown(KeyCode.O))
		//{
		//	serviceInstance.LoadGame ();
		//}
		//if (Input.GetKeyDown(KeyCode.I))
		//{
		//	player.LearnSkill ("TEST01");
		//}
	}
}
