using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class WakeUpState : ActionState {
	public void EditStateInfo (string animName)
	{
		this.stateInfo.animName = animName;
	}
	public SkillDamageType GetDamageType ()
	{
		return fsm.GetState<DamagedState>().damageInfo.skillInfo.skillDamageType;
	}
	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		var damType = GetDamageType ();
		if (fromState == this)
			return false;
		if (!targetActor.actorInfo.isGrounded)
			return false;
		if (fromState == fsm.GetState<DownState>())
			return true;
		if (damType != SkillDamageType.BLOW)
			return false;
		return true;
	}
	public override bool CommonCheckExit ()
	{
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
		targetActor.SetUnbeatable (true, 0.5f);
	}
	public override void CommonExit ()
	{
		targetActor.SetMoveable (true);
		isAnimationEnd = false;
		targetActor.rigid.velocity = Vector3.zero;
	}
}
