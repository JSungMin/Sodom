﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActorConditionNamespace
{
	[Serializable]
	public class Unbeatable : ConditionApplier {
		#region implemented abstract members of ConditionApplier
		public override void Init(Actor target, ActorConditionType conType, float duration, float effectiveness)
		{
			base.Init(target, conType, duration, effectiveness);
		}
		public override void UpdateApplierInfo(float duration, float effectiveness)
		{
			base.UpdateApplierInfo(duration, effectiveness);
		}
		public override void StartTimer (float duration, float effectiveness)
		{
			//Debug.Log("Actor : " + targetActor.name + ", " + conditionType.ToString() + " Start");
			playTime = 0f;
			UpdateApplierInfo(duration, effectiveness);
			serviceInstance.Add_ApplierTimer_Subscriber(UpdateTimer);
			if (Check())
				Apply();
			IsBusy = true;
		}
		public override void UpdateTimer(object sender, EventArgumentNamespace.ConditionEventArg arg)
		{
			duration -= Time.deltaTime;
			if (!Check())
			{
				RemoveApplier();
			}
		}
		protected override bool Check()
		{
			return base.Check();
		}
		protected override void Apply()
		{
			//  TODO : Animation 혹은 Image Effect 적용
		}
		protected override void RemoveApplier()
		{
			//  TODO : Animation 혹은 Image Effect 해제
			StopTimer();
			conditionInfo.RemoveCondition(conditionType, this);
		}
		#endregion
	}

}