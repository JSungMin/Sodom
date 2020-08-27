using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class HundBackStepState : ActionState {
	private Hund hund;
	private HundActionFSM hundFsm;
	private Player player;
	private Vector3 startPos;
	private Vector3 playerPos;
	public float minPosX = 1.5f;
	public float maxPosX = 14.83f;
	public bool useTurn = false;
	private float wallDir;
	private bool isJumped = false;
	public void EditStateInfo (SkillInfo skillInfo)
	{
	
	}

	//	End HELPER Block

	#region implemented abstract members of ActionStateNode
	public bool CommonCheckEnter (ActionState fromNode, object infoParam)
	{
		if (fromNode == fsm.GetState<StunState>())
			return false;
		if (fromNode == fsm.GetState<DamagedState>())
			return false;
		if (fromNode == fsm.GetState<HundEMPState> ())
			return false;
		if (CheckAnimationEnd ())
			return false;
		if (CheckIsLoopNode (fromNode))
			return false;
		return true;
	}
	public bool CommonCheckExit ()
	{
		if (CheckAnimationEnd() && hund.GetNowAnimationName() == "BackStep")
		{
			OnAnimationEnd ();
			return true;
		}
		return false;
	}

	public override void CommonEnter ()
	{
        //base.CommonEnter();
		EnemyAIHelper.HundAIHelper.ResetDirty();
        EnemyAIHelper.HundAIHelper.IsBackstepPipeline = true;
		hund = (hund == null) ? targetActor as Hund : hund;
		hundFsm = hund.hundFsm;
		player = (player == null) ? hund.player : player;
		hund.rigid.velocity = Vector3.zero;
		startPos = hund.transform.position;
		playerPos = player.transform.position;
		playerPos.y = startPos.y;
		playerPos.z = startPos.z;
		var midPointX = (maxPosX + minPosX) * 0.5f;
		wallDir = (midPointX <= startPos.x) ? 1f : -1f;
		
		if (wallDir != hund.lookDirection && Mathf.Abs(startPos.x - midPointX) >= 4f)
		{
			useTurn = true;
			isJumped = false;
			hund.SetDesireDirection (wallDir);
			hund.SetLookDirection();
		}
		else
		{
			useTurn = false;
			isJumped = true;
			//	BackStep animName
			var vel = hund.disInfo.disToPlayer / 1.5f * 5.5f + 10f;
			hund.rigid.AddForce (Vector3.left * vel * hund.GetLookDirection(), ForceMode.Impulse);
			PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
		}
		isAnimationEnd = false;
	}
	public override void CommonUpdate()
	{
		//	Hund의 Turn이 끝났을 경우
		if (useTurn && !hund.isTurning)
		{
			if (!isJumped)
			{
				//	BackStep Animation 실행
                var vel = hund.disInfo.disToPlayer / 1.5f * 5.5f + 10f;
                hund.rigid.AddForce (Vector3.left * vel * wallDir, ForceMode.Impulse);
                PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
				isJumped = true;
			    isAnimationEnd = false;
			}
		}
	}
	public override void CommonExit ()
	{
		base.CommonExit ();
		hund.rigid.velocity = Vector3.zero;
        hund.patternIndex = 0;
        if (hund.disState == 0)
		{
            hund.nowPattern = EnemyAIHelper.GetPatternInfo("중단뜯기02", hund);
            hundFsm.attackState.EditStateInfo (EnemyAIHelper.GetSkillInfo("중단뜯기02", hund));
		}
		else if (hund.disState == 1)
		{
            hund.nowPattern = EnemyAIHelper.GetPatternInfo("꼬리치기_상단", hund);
            hundFsm.attackState.EditStateInfo (EnemyAIHelper.GetSkillInfo("꼬리치기_상단", hund));
		}
		else if (hund.disState == 2)
		{
            hund.nowPattern = EnemyAIHelper.GetPatternInfo("꼬리치기_중단", hund);
            hundFsm.attackState.EditStateInfo (EnemyAIHelper.GetSkillInfo("꼬리치기_중단", hund));
		}
		else
		{
			hundFsm.BreakStateChain();
			hund.nowPattern = EnemyAIHelper.GetPatternInfo("추적_하단뜯기01",hund);
			hundFsm.attackState.EditStateInfo (EnemyAIHelper.GetSkillInfo("하단뜯기01", hund));
			hundFsm.StartStateChain("Chase02",new List<ActionState>(){
				hundFsm.chaseState,
				hundFsm.attackState
			});
		}
	}
	#endregion
}
