using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class HundStopState : ActionState {
	//	End HELPER Block
	private Hund hund;
	public Timer timer;
	#region implemented abstract members of ActionStateNode
	public override bool CommonCheckEnter (ActionState fromNode, object infoParam)
	{
		if (fromNode == this)
			return false;
		return true;
	}
	public override bool CommonCheckExit ()
	{
		if (CheckAnimationEnd() &&
			timer.CheckTimer() &&
		 	hund.GetNowAnimationName() == "Tackle_End") 
		{
			OnAnimationEnd ();
			return true;
		}
		return false;
	}

	public override void CommonEnter ()
	{
		timer.timer = 0f;
		timer.duration = 0.5f;
		hund = (hund == null) ? targetActor as Hund : hund;
		PlayAnimation(0, "Tackle_End", false, 0f, false);
		isAnimationEnd = false;
        GameSystemService.Instance.VibrateCameraByAttack(0.6f, 24f, 0.3f);
        //targetActor.rigid.velocity = Vector3.zero;
        targetActor.rigid.AddForce(Vector3.right * targetActor.lookDirection * 3.5f, ForceMode.Impulse);
    }
	public override void CommonUpdate()
	{
		timer.IncTimer(Time.fixedDeltaTime);
	}
	public override void CommonExit ()
	{
		EnemyAIHelper.HundAIHelper.SetDirty(3);
        base.CommonExit ();
	}
	#endregion
}
