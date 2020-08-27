using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DeadState : ActionState {
	public DamageInfo damageInfo;
	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		if (fromState == this)
			return false;
		if (targetActor.actorInfo.GetLife () <= 0f)
			return true;
		return false;
	}
	public override bool CommonCheckExit ()
	{
		return false;
	}
	public override void CommonEnter ()
	{
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
		targetActor.SetMoveable (false);
		GameSystemService.Instance.SlowMotionCamera (0.1f, 0.1f);
		GameSystemService.Instance.camManager.PlayerDeadEffect ();
		targetActor.rigid.velocity = Vector3.zero;
	}
	public override void CommonExit ()
	{
		base.CommonExit ();
		//SceneManager.LoadScene (0);
	}
}
