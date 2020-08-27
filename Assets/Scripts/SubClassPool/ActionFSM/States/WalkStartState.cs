using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
[System.Serializable]
public class WalkStartState : ActionState {
	public Rigidbody targetRigid;
	public float prevXDir = 0;
	public float speed;

	public override void Initialize(ActorActionFSM fsm)
	{
		base.Initialize(fsm);
		targetRigid = targetActor.rigid;
	}
	public virtual void EditStateInfo (string animName)
	{
		stateInfo.animName = animName;
	}
	public virtual void EditStateInfo (float dirX)
	{
		prevXDir = targetActor.lookDirection;
		targetActor.lookDirection = dirX;
	}

	public bool CheckEnterByPlayer (ActionState fromState, object infoParam)
	{
		if (PlayerInputManager.pressedSitInput)
			return false;
		if (fromState == this)
			return false;
		if (fromState == fsm.GetState<JumpState>())
			return false;
		if (fromState == fsm.GetState<WalkState>())
			return false;
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (!targetActor.GetMoveable ())
			return false;
		if (Input.GetKey (PlayerInputManager.keyMap ["LEFT"].input_key) && Input.GetKey (PlayerInputManager.keyMap ["RIGHT"].input_key))
			return false;
		return PlayerInputManager.pressedMoveInput;
	}
	public bool CheckEnterByEnemy (ActionState fromState, object infoParam)
	{
		if (fromState == this)
			return false;
		if (fromState == fsm.GetState<JumpState>())
			return false;
		if (fromState == fsm.GetState<WalkState>())
			return false;
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (!targetActor.GetMoveable ())
			return false;
		return true;
	}
	public bool CheckExitByPlayer ()
	{
		if (!targetActor.GetMoveable ())
			return true;
		if (PlayerInputManager.pressedSitInput)
			return true;
		if (!PlayerInputManager.pressedMoveInput)
			return true;
		if (isAnimationEnd)
			return true;
		return false;
	}
	public bool CheckExitByEnemy ()
	{
		if (!targetActor.GetMoveable ())
			return true;
		if (isAnimationEnd)
			return true;
		return false;
	}

	public override void CommonEnter ()
	{
		isAnimationEnd = false;
		speed = targetActor.actorInfo.GetTotalBasicInfo ().moveSpeed;
		//if (!targetActor.GetNowAnimationName().Equals(stateInfo.anim_name))
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
	}
	public override void CommonUpdate ()
	{
		var adjustVelocity = targetRigid.velocity;
		adjustVelocity.x = Mathf.Sign(targetActor.lookDirection) * speed;
		targetActor.rigid.velocity = adjustVelocity;
	}
	public override void CommonExit ()
	{
		var adjustVelocity = targetRigid.velocity;
		adjustVelocity.x = 0f;
		targetRigid.velocity = adjustVelocity;
		isAnimationEnd = false;
	}
}
