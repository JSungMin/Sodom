using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public class InteractState : ActionState {
	public Actor interactWith;

	public InteractState() : base(){	}
	public InteractState(ActorActionFSM fsm) : base (fsm,null)
	{
		this.stateInfo = new ActionStateInfo ("InteractState", targetActor.actorInfo.actor_name + "_Interact");
	}
	public InteractState (ActorActionFSM fsm, ActionStateInfo sInfo) : base(fsm, sInfo)	{	}

	#region implemented abstract members of ActionState

	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		return true;
	}

	public override bool CommonCheckExit ()
	{
		if (!CheckAnimationEnd ())
			return false;
		return true;
	}

	public override void CommonEnter()
	{
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
	}

	public override void CommonExit()
	{
		isAnimationEnd = false;
	}

	public override void CommonUpdate ()
	{
		
	}

	#endregion
}
