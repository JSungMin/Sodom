using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class WalkEndState : ActionState {
	public Rigidbody targetRigid;

	public override void Initialize(ActorActionFSM fsm)
	{
		base.Initialize(fsm);
		targetRigid = targetActor.rigid;
	}
	public virtual void EditStateInfo (string animName)
	{
		stateInfo.animName = animName;
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
		//	TODO : 이후 플레이어의 지상, 공중 상태 체크필요
		return true;
	}
	public bool CheckExitByPlayer ()
	{
		if (!targetActor.GetMoveable ())
			return true;
		if (PlayerInputManager.pressedSitInput)
			return true;
		if (PlayerInputManager.pressedMoveInput)
			return true;
		if (isAnimationEnd)
			return true;
		return false;
	}
	public override void CommonEnter ()
	{
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, true, 0f);
		targetRigid.AddForce (targetActor.GetMoveSpeed () * Vector3.right * targetActor.lookDirection, ForceMode.Impulse);
	}
	public override void CommonExit ()
	{
		var adjustVelocity = targetRigid.velocity;
		adjustVelocity.x = 0f;
		targetRigid.velocity = adjustVelocity;
		isAnimationEnd = false;
	}
}
