using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class DownState : ActionState {

	public float downTime = 0.5f;
	private float downTimer = 0f;
	public void EditStateInfo (string animName)
	{
		this.stateInfo.animName = animName;
	}
	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		if (fromState == this)
			return false;
		return true;
	}
	public override bool CommonCheckExit ()
	{
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (downTimer <= downTime)
			return false;
		if (isAnimationEnd)
			return true;
		return false;
	}

	public override void CommonEnter ()
	{
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0.25f);
		targetActor.SetMoveable (false);
		targetActor.SetUnbeatable (true, downTime);
		downTimer = 0f;
	}
	public override void CommonUpdate ()
	{
		downTimer += Time.deltaTime;
	}
	public override void CommonExit ()
	{
		targetActor.SetMoveable (true);
		isAnimationEnd = false;
		targetActor.rigid.velocity = Vector3.zero;
	}
}
