using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
[System.Serializable]
public class RollingState : ActionState {
	public Rigidbody targetRigid;
	public float rollingSpeed;

	public float unbeatableTime = 0.25f;

	public override void Initialize(ActorActionFSM fsm)
	{
		base.Initialize(fsm);
		targetRigid = targetActor.rigid;
	}
	public void EditStateInfo (string animName)
	{
		stateInfo.animName = animName;
	}

	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (fromState == this)
			return false;
		if (fromState == fsm.GetState<DamagedState>())
			return false;
		if (fromState == fsm.GetState<JumpState>())
			return false;
        if(fromState == fsm.GetState<AttackState>())
        {
            PlayEffectAnimation("None");
            return true;
            //if (targetActor.activeACIndex == -1)
            //else
            //    GameSystemService.Instance.playerInputManager.StopRollingAging();
        }
		if (!targetActor.GetMoveable ())
			return false;
		return true;
	}
	public override bool CommonCheckExit ()
	{
		if (isAnimationEnd)
			return true;
		return false;
	}

	public override void CommonEnter ()
	{
		isAnimationEnd = false;
		rollingSpeed = targetActor.actorInfo.GetTotalBasicInfo ().moveSpeed * 1.25f;
		targetActor.SetUnbeatable (true, unbeatableTime);
		targetActor.SetMoveable (false);

        PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
	}
	public override void CommonUpdate()
	{
		var adjustVelocity = targetRigid.velocity;
		adjustVelocity.x = Mathf.Sign(-targetActor.transform.localScale.x) * rollingSpeed * (targetActor.actorInfo.CalcRollingSpeedGraph (targetActor.animPlaybackTime));
		targetActor.rigid.velocity = adjustVelocity;
	}
	public override void CommonExit ()
	{
		GameSystemService.Instance.ClearSkillBuffer ();
		//targetActor.SetUnbeatable (false);
		targetActor.SetMoveable (true);
		isAnimationEnd = false;
		var adjustVelocity = targetRigid.velocity;
		adjustVelocity.x = 0f;
		targetActor.rigid.AddForce (Vector3.right * Mathf.Sign (-targetActor.transform.localScale.x), ForceMode.Impulse);
		//(fsm.walkState as PlayerWalkState).cycleCount = 1;
		targetActor.rigid.velocity = adjustVelocity;
	}
}
