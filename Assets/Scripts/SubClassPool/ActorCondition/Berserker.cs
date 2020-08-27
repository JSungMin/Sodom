using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActorConditionNamespace
{
	[Serializable]
	public class Berserker : ConditionApplier
	{
		public float ratio = 0.5f;
		public float duration_burf;
		public float duration_deburf;
		public bool isBurf = true;

		#region implemented abstract members of ConditionApplier
		public override void Init(Actor target, ActorConditionType conType, float duration, float effectiveness)
		{
			base.Init(target, conType, duration, effectiveness);
		}
		public override void UpdateApplierInfo(float duration, float effectiveness)
		{
			base.UpdateApplierInfo(duration, effectiveness);
			duration_burf = duration * ratio;
			duration_deburf = duration * (1 - ratio); 
			isBurf = true;
		}
		public override void StartTimer (float duration, float effectiveness)
		{
			Debug.Log("Actor : " + targetActor.name + ", " + conditionType.ToString() + " Start");
			base.StartTimer (duration, effectiveness);
			Apply();	//  Apply Burf;
		}
		public override void UpdateTimer(object sender, EventArgumentNamespace.ConditionEventArg arg)
		{
			if (isBurf)
				duration_burf -= Time.deltaTime;
			else
				duration_deburf -= Time.deltaTime;

			if (!Check())
				RemoveApplier();
		}
		protected override bool Check()
		{
			if (!IsBusy)
				return false;
			if (isBurf && duration_burf > 0)
				return true;
			else if (isBurf && duration_burf <= 0)
			{
				isBurf = false;
				Apply();
				return true;
			}
			else if (!isBurf && duration_deburf > 0)
				return true;
			return false;
		}
		protected override void Apply()
		{
			if (isBurf)
			{
				foreach (var applierList in targetActor.actorInfo.conditionInfo.applierPool)
				{
					foreach (var applier in applierList) {
						if (applier.conditionType != ActorConditionType.BERSERKER)
							conditionInfo.RemoveCondition (applier.conditionType, applier);
					}
				}
				targetActor.actorInfo.additionalBasicStat.immunity += 100f;
				targetActor.actorInfo.additionalBasicStat.attackSpeed += effectiveness;
			}
			else
			{
				targetActor.actorInfo.additionalBasicStat.immunity -= 100f;
				targetActor.actorInfo.additionalBasicStat.attackSpeed -= effectiveness;
			}
		}

		public override void StopTimer ()
		{
			base.StopTimer ();	// 강제적으로 상태 해제시 Stop By Forced가 호출됨.
		}
		protected override void StopByForced ()
		{
			if (isBurf) {
				targetActor.actorInfo.additionalBasicStat.immunity -= 100f;
			} else {
				targetActor.actorInfo.additionalBasicStat.attackSpeed -= effectiveness;
			}
		}
		protected override void RemoveApplier()
		{
			flag_finish = true;
			targetActor.actorInfo.additionalBasicStat.attackSpeed -= effectiveness;
			StopTimer();
			conditionInfo.RemoveCondition(conditionType, this);
		}
		#endregion
	}

}