using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class HundSuddenAttackEndState : ActionState
{
	public Hund hund;
    public Timer disappearTimer = new Timer{duration = 0.5f};
    //	End HELPER Block

    #region implemented abstract members of ActionStateNode
	public override bool CommonCheckEnter(ActionState fromNode, object infoParam)
    {
        return true;
    }
    public override bool CommonCheckExit()
    {
        return disappearTimer.CheckTimer();
    }

    public override void CommonEnter()
    {
        hund = (hund == null) ? targetActor as Hund : hund;
		isAnimationEnd = false;
        disappearTimer.Reset();
    }
    public override void CommonUpdate()
    {
        disappearTimer.IncTimer(Time.deltaTime);
    }
    public override void CommonExit()
    {
        base.CommonExit();
    }

    #endregion
}
