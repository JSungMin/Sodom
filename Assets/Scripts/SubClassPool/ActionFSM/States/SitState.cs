using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class SitState : ActionState {
	public void EditStateInfo (string animName)
	{
		stateInfo.animName = animName;
	}

	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		if (fromState == this)
			return false;
		if (fromState == fsm.GetState<JumpState>())
			return false;
		//if (!PlayerInputManager.pressedSitInput)
		//	return false;
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (!targetActor.GetMoveable ())
			return false;
		return true;
	}
	public override bool CommonCheckExit ()
	{
		if (PlayerInputManager.pressedMoveInput &&
			!PlayerInputManager.pressedSitInput) 
		{
			return true;
		}
		if (!PlayerInputManager.pressedSitInput) 
		{
			return true;
		}
		return false;
	}

	public override void CommonEnter ()
	{
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
	}
	public override void CommonExit ()
	{
        //PlayerInputManager.pressedSitInput = false;
		isAnimationEnd = false;
	}
}
