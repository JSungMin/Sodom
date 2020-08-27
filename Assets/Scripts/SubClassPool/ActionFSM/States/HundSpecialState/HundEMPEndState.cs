using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class HundEMPEndState : ActionState
{
    public Hund hund;
    public Timer stateTimer;    //	등속도 운동으로 걸어가는데 필요한 Timer

    //	End HELPER Block

    #region implemented abstract members of ActionStateNode
    public override bool CommonCheckEnter(ActionState fromNode, object infoParam)
    {
        return true;
    }
    public override bool CommonCheckExit()
    {
        if (stateTimer.CheckTimer())
        {
            OnAnimationEnd();
            return true;
        }
        return false;
    }

    public override void CommonEnter()
    {
        hund = (hund == null) ? targetActor as Hund : hund;
        PlayAnimation(0, "Idle", true, 0f, false);
        isAnimationEnd = false;
        stateTimer.timer = 0;
        stateTimer.duration = 1f;
    }
    public override void CommonUpdate()
    {
        stateTimer.IncTimer(Time.deltaTime);
    }
    public override void CommonExit()
    {
        base.CommonExit();
    }

    #endregion
}
