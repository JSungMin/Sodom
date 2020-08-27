using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class WalkState : ActionState {
	public Rigidbody targetRigid;
	public float prevXDir = 0;

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
		if (fromState == fsm.GetState<JumpState>())
			return false;
		if (!targetActor.actorInfo.isGrounded)
			return false;
		//	Walk같은 경우 다른 상태가 모두 끝난 후에, 진입가능하다.
		//	이와 같이 다른 상태를 Cancel 할 수 없는 State는 아래 Block을 기입해야 한다.
		if (!targetActor.GetMoveable ())
			return false;
		if (Input.GetKey (PlayerInputManager.keyMap ["LEFT"].input_key) && Input.GetKey (PlayerInputManager.keyMap ["RIGHT"].input_key))
			return false;
		//	TODO : 이후 플레이어의 지상, 공중 상태 체크필요
		return PlayerInputManager.pressedMoveInput;
	}
	public bool CheckExitByPlayer ()
	{
		if (!targetActor.actorInfo.isGrounded)
			return true;
		if (!targetActor.GetMoveable ())
			return true;
		if (PlayerInputManager.pressedSitInput)
			return true;
		if (!PlayerInputManager.pressedMoveInput)
			return true;
		if (Input.GetKey (PlayerInputManager.keyMap ["LEFT"].input_key) && Input.GetKey (PlayerInputManager.keyMap ["RIGHT"].input_key))
			return true;
		return false;
	}
	public bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (!targetActor.GetMoveable ())
			return false;
		return true;
	}
	public bool CommonCheckExit ()
	{
		if (!targetActor.actorInfo.isGrounded)
			return true;
		if (!targetActor.GetMoveable ())
			return true;
		return false;
	}

	public override void CommonEnter ()
	{
		isAnimationEnd = false;
		//if (!targetActor.GetNowAnimationName().Equals(stateInfo.anim_name))
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, true, 0f);
	}
	public override void CommonUpdate ()
	{
		var adjustVelocity = targetRigid.velocity;
		adjustVelocity.x = Mathf.Sign(targetActor.lookDirection) * targetActor.GetMoveSpeed();
		targetActor.rigid.velocity = adjustVelocity;
	}
	public override void CommonExit ()
	{
		var adjustVelocity = targetRigid.velocity;
		adjustVelocity.x = 0f;
		targetRigid.velocity = adjustVelocity;
		isAnimationEnd = false;
		if (stateInfo.animName != stateInfo.defaultAnimName) {
			stateInfo.animName = stateInfo.defaultAnimName;
		}
	}
}
