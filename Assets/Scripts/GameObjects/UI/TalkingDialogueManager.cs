
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkingDialogueManager : MonoBehaviour {
	public TalkingDialogue[] dialogues;
	public int curIndx;
	public TalkingDialogue curDialogue;
	public AnimationCurve dialougeBGCurve;
	
	public void SetCurrentDialogue (int indx)
	{
		curIndx = indx;
		curDialogue = dialogues[indx];
		Debug.Log(curDialogue.name);
		ViewingCurrentDialogue();
	}
	public void SetEmptyDialogueToStopGetInput()
	{
		curDialogue = null;
	}
	public void ViewingCurrentDialogue()
	{
		curDialogue.OnStartViewing();
	}
	public void SetUserToControlable(Actor actor)
	{
		actor.enabled = true;
		GameSystemService.Instance.playerInputManager.enabled = true;
		SetEmptyDialogueToStopGetInput();
		var directorManger = GameObject.FindObjectOfType<OpeningSceneManager>();
		if (null == directorManger)
			return;
		for (int i = 0;i < directorManger.directors.Count;i++)
		{
			directorManger.directors[i].director.Stop();
		}
	}
	public void SetUserToNotControlable(Actor actor)
	{
		actor.enabled = false;
		GameSystemService.Instance.playerInputManager.enabled = false;
	}

	// Update is called once per frame
	void Update () {
		if (curDialogue == null)
			return;
		if (Input.anyKeyDown)	
		{
			ViewingCurrentDialogue();
		}
	}
}
