using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class StandState : ActionState {
	public void EditStateInfo (string animName)
	{
		stateInfo.animName = animName;
	}

	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		if (fromState == this)
			return false;
		if (!targetActor.actorInfo.isGrounded)
			return false;
		return true;
	}
	public override bool CommonCheckExit ()
	{
		if (PlayerInputManager.pressedSitInput)
			return true;
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (isAnimationEnd)
			return true;
		return false;
	}

	public override void CommonEnter ()
	{
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
		targetActor.SetMoveable (false);
	}
	public override void CommonExit ()
	{
		targetActor.SetMoveable (true);
		isAnimationEnd = false;
	}
}
