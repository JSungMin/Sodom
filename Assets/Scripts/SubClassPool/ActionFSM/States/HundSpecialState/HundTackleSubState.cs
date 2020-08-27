using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class HundTackleSubState : ActionState {
	private Hund hund;
	public EnemyAttackPatternInfo patternInfo;
	public SkillInfo tmpSkillInfo;
	private bool useTurn = false;
	public float minX = 1.5f, maxX = 14.83f;
	//	End HELPER Block

	#region implemented abstract members of ActionStateNode
	public bool CommonCheckEnter (ActionState fromNode, object infoParam)
	{
		if (fromNode == fsm.GetState<HundTackleState>())
			return true;
		return false;
	}
	public bool CommonCheckExit ()
	{
		if (CheckAnimationEnd()) 
		{
			if (!useTurn && hund.GetNowAnimationName() == "Tackle_Ready")
			{
				OnAnimationEnd ();
				return true;
			}
			else if (hund.GetNowAnimationName() == "Tackle_Ready")
			{
				OnAnimationEnd ();
				return true;
			}
		}
		return false;
	}

	public override void CommonEnter ()
	{
		hund = (hund == null) ? targetActor as Hund : hund;
		isAnimationEnd = false;
		hund.rigid.velocity = Vector3.zero;
		hund.ghosting.color = hund.noneParryWarnColor;
		hund.ghosting.ghostingEnabled = true;
		useTurn = false;
		var startPos = hund.transform.position;
		var destPos = startPos;
		var deltaToMinX = Mathf.Abs(startPos.x - minX);
		var deltaToMaxX = Mathf.Abs(startPos.x - maxX);
	
		if (hund.lookDirection != hund.disInfo.dirToPlayer)
		{
			hund.SetLookDirection();
			useTurn = true;
		}
		else
		{
			PlayAnimation(0, "Tackle_Ready", false, 0f);
		}
	}
	public override void CommonUpdate()
	{
		if (useTurn && !hund.isTurning)
		{
			isAnimationEnd = false;
			PlayAnimation(0, "Tackle_Ready", false, 0f);
		}
	}
	public override void CommonExit ()
	{
		base.CommonExit ();
		hund.ghosting.ghostingEnabled = false;
		hund.rigid.velocity = Vector3.zero;
	}
	#endregion
}
