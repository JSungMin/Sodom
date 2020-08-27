using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

/* 이 스테이트는 HundHideManage로 부터 TryTransferToAction호출을 통해 이전 된다.
 * HundHideManager를 통해 Hund는 플레이어로부터 가장 멀리 있는 벽 끝에서부터 시작해
 * 반대쪽 벽까지 뛴다. 이 뛰는 동안 일정 주기마다 유도성이 있는 미사일(혹은 에너지 볼)
 * 을 발사한다.
 */
[System.Serializable]
public class HundDarkRunEndState : ActionState
{
    public Hund hund;   

    //	End HELPER Block
    #region implemented abstract members of ActionStateNode
    public bool CommonCheckEnter(ActionState fromNode, object infoParam)
    {
        return true;
    }
    public bool CommonCheckExit()
    {
        if (CheckAnimationEnd())
        {
            OnAnimationEnd();
            return true;
        }
        return false;
    }

    public override void CommonEnter()
    {
        hund = (hund == null) ? targetActor as Hund : hund;
        isAnimationEnd = false;
        PlayAnimation(0, "Stop", false, 0f);
        //  나머지 부분은 HundHideManager에서 초기화
    }
    public override void CommonUpdate()
    {
       
    }
    public override void CommonExit()
    {
        base.CommonExit();
        //  나머지 부분은 HundHideManager에서 마무리
    }

    #endregion
}
