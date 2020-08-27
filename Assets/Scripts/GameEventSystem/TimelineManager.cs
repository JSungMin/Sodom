using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;

public class TimelineManager : MonoBehaviour {
	public List <PlayableDirector> directorsInScene = new List<PlayableDirector>();
	public List <TimelineAsset>	timelineList = new List<TimelineAsset>();		//	현재 씬에서 사용될 TimelineAsset List입니다.

	// Use this for initialization
	void Start () {
		directorsInScene = new List<PlayableDirector> (GameObject.FindObjectsOfType<PlayableDirector> ());
		//timelineList = new List<TimelineAsset> (Resources.LoadAll<TimelineAsset> (PathPool.GetTimelineDirectoryPath(SceneManager.GetActiveScene().name)));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
