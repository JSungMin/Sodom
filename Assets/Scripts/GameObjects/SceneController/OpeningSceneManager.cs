using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;

public class OpeningSceneManager : MonoBehaviour {
	[System.Serializable]
	public struct DirectorWrapper
	{
		public bool isFinished;
		public PlayableDirector director;
		public Timer timelineTimer;
		public UnityEvent OnTimelineEnd;
		public void OnStopped(PlayableDirector director)
		{
			if (director.state == PlayState.Paused)
				return;
			isFinished = true;
			if (OnTimelineEnd != null)
			{
				OnTimelineEnd.Invoke();
			}
		}
	}
	public List<DirectorWrapper> directors;
	public PlayableDirector curDirector;
	private DirectorWrapper curDw;
	public int dwIndx = -1;
	public UnityEvent OnStartEvent;
	// Use this for initialization
	void Start () {
		if (OnStartEvent.GetPersistentEventCount() != 0)
			OnStartEvent.Invoke();
		for (int i = 0; i < directors.Count; i++)
		{
			directors[i].director.stopped += directors[i].OnStopped;
			directors[i].timelineTimer.duration = (float)directors[i].director.playableAsset.duration;
		}
	}
	public void StartDirector (int idx)
	{
		dwIndx = idx;
		curDw = directors[idx];
		curDw.isFinished = false;
		curDw.timelineTimer.Reset();
		curDirector = curDw.director;
		curDirector.Play();
		Debug.Log(curDirector.name);
	}
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			LoadScene("Stage01");	
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			LoadScene("Stage02");
		}
		else if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			LoadScene("Stage03");
		}
		else if (Input.GetKeyDown(KeyCode.M))
		{
			LoadScene("StartScene");
		}
		if (null == curDirector)
			return;
		curDw.timelineTimer.IncTimer(Time.deltaTime);
		if (curDw.timelineTimer.CheckTimer() && !curDw.isFinished)
		{
			if (curDirector.extrapolationMode == DirectorWrapMode.Hold)
			{
				curDw.isFinished = true;
				directors[dwIndx] = curDw;
				if (curDw.OnTimelineEnd != null)
				{
					curDw.OnTimelineEnd.Invoke();
				}
			}
			else if (curDirector.extrapolationMode == DirectorWrapMode.Loop)
			{
				StartDirector(dwIndx);
			}
		}
	}
}
