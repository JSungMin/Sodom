using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

namespace ActorConditionNamespace
{
	[Serializable]
	public class Poisioning : ConditionApplier {
		public float applyTick = 0.2f;
		public int applyCount = 0;

		#region implemented abstract members of ConditionApplier
		public override void Init (Actor target, ActorConditionType conType, float duration, float effectiveness)
		{
			base.Init(target, conType,duration, effectiveness);
		}
		public override void UpdateApplierInfo (float duration, float effectiveness)
		{
			base.UpdateApplierInfo (duration, effectiveness);
			applyCount = Mathf.RoundToInt(duration / applyTick);
		}
		protected override bool Check()
		{
			if (applyCount > 0)
			{
				return true;
			}
			return false;
		}
		public override void UpdateTimer(object sender, EventArgumentNamespace.ConditionEventArg arg)
		{
			playTime += Time.deltaTime;
			if (Check())
			{
				if (playTime >= applyTick)
				{
					Apply();
					playTime = 0f;
					applyCount--;
				}
			}
			else
			{
				RemoveApplier();
			}
		}

		protected override void Apply ()
		{
			targetActor.actorInfo.SubLife (effectiveness);
		}
		protected override void RemoveApplier()
		{
			StopTimer();
			conditionInfo.RemoveCondition(conditionType, this);
		}
		#endregion
	}
}