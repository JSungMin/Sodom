using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class GeneratorOffState : ActionState
{
    public ElectricGenerator generator;
	public DamageInfo damageInfo;
    //	End HELPER Block

    #region implemented abstract members of ActionStateNode
    public void EditStateInfo (DamageInfo damageInfo)
	{
		this.damageInfo = damageInfo;
		//this.stateInfo.animName = targetActor.actorInfo.actor_name + "_Damaged";
	}
	public override bool CommonCheckEnter(ActionState fromNode, object infoParam)
    {
        return true;
    }
    public override bool CommonCheckExit()
    {
        return false;
    }

    public override void CommonEnter()
    {
        generator = (generator == null) ? targetActor as ElectricGenerator : generator;
        generator.SetUnbeatable(true);
		PlayAnimation(0, "OFF", false, 0f, false);
        
		isAnimationEnd = false;
    }
    public override void CommonUpdate()
    {
    }
    public override void CommonExit()
    {
        base.CommonExit();
		 generator.SetUnbeatable(false);
    }

    #endregion
}
