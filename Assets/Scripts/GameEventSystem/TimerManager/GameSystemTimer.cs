using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;

[Serializable]
public class GameSystemTimer {
	public event EventHandler<SystemTimerArgs> RaiseGamePauseEvent;
	public event EventHandler<SystemTimerArgs> RaiseGameResumeEvent;
	public event EventHandler<SystemTimerArgs> RaiseGameExitEvent;

	public bool flag_Game_Run_State = true;

	public void PauseGame (object sender)
	{
		var newArg = new SystemTimerArgs (sender);
		flag_Game_Run_State = false;
		OnGamePause (newArg);
		Time.timeScale = 0f;
	}
	public void ResumeGame (object sender)
	{
		var newArg = new SystemTimerArgs (sender);
		flag_Game_Run_State = true;
		OnGameResume (newArg);
		Time.timeScale = 1f;
	}
	public void ExitGame (object sender)
	{
		var newArg = new SystemTimerArgs (sender);
		OnGameExit (newArg);
		//	TODO: 메인메뉴로 이동
	}

	protected virtual void OnGamePause (SystemTimerArgs arg)
	{
		EventHandler<SystemTimerArgs> handler = RaiseGamePauseEvent;
		if (null != handler) {
            Debug.Log ("Object " + arg.GetElement().ToString() + " was Affected by Game Pause");
			handler (this, arg);
		} 
		else {
			Debug.Log ("Game Pause Handler is Null");
		}
	}
	protected virtual void OnGameResume (SystemTimerArgs arg)
	{
		EventHandler<SystemTimerArgs> handler = RaiseGameResumeEvent;
		if (null != handler) {
			Debug.Log ("Object " + arg.GetElement().ToString() + " was Affected by Game Resume");
			handler (this, arg);
		}
		else {
			Debug.Log ("Game Resume Handler is Null");
		}
	}
	protected virtual void OnGameExit (SystemTimerArgs arg)
	{
		EventHandler<SystemTimerArgs> handler = RaiseGameExitEvent;
		if (null != handler) {
			Debug.Log ("In Game Will be Exited So Let's Call Destroyer : " + arg.GetElement().ToString());
			handler (this, arg);
		}
	}
}
