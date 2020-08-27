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
public class HundDarkRunState : ActionState
{
    public Hund hund;
    public GameObject hundProjectile;
    public Vector3 destPos;
    private Vector3 startPos;
    public float maxX, minX;
    private float disToEmpPos;  //	Set When State Entered	
    public float speed;         //	empPos까지 걸어가는데 필요한 등속도
    public Timer stateTimer;    //	등속도 운동으로 걸어가는데 필요한 Timer
    public Timer projTimer;     //  미사일 발사 주기 계산용 타이머

    //	End HELPER Block
    #region implemented abstract members of ActionStateNode
    public bool CommonCheckEnter(ActionState fromNode, object infoParam)
    {
        if (fromNode == fsm.GetState<StunState>())
            return false;
        if (fromNode == fsm.GetState<DamagedState>())
            return false;
        if (CheckAnimationEnd())
            return false;
        return true;
    }
    public bool CommonCheckExit()
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
        isAnimationEnd = false;
        destPos.y = hund.transform.position.y;
        destPos.z = hund.transform.position.z;
        startPos = hund.transform.position;
        if (Mathf.Abs(startPos.x - minX) >= Mathf.Abs(startPos.x - maxX))
        {
            destPos.x = minX;
            startPos.x = maxX;
        }
        else
        {
            destPos.x = maxX;
            startPos.x = minX;
        }
        hund.transform.position = startPos;
        disToEmpPos = Mathf.Abs(destPos.x - startPos.x);
        stateTimer.timer = 0;
        stateTimer.duration = disToEmpPos / speed;
        EnemyAIHelper.UpdateDistanceInfo(hund);
        hund.SetLookDirection(Mathf.Sign(destPos.x - startPos.x));

        projTimer.Reset();
    }
    public override void CommonUpdate()
    {
        if (!stateTimer.CheckTimer())
        {
            stateTimer.IncTimer(Time.deltaTime);
            PlayAnimation(0, "Run", true, 0f);
            hund.transform.position = Vector3.Lerp(startPos, destPos, stateTimer.GetRatio());
        }
        if (!projTimer.CheckTimer())
        {
            projTimer.IncTimer(Time.deltaTime);
        }
        else
        {
            hund.projectile[0].OnLaunch("미사일발사", true);
            hund.projectile[1].OnLaunch("미사일발사", true);
            projTimer.Reset();
        }
    }
    public override void CommonExit()
    {
        base.CommonExit();
    }

    #endregion
}
