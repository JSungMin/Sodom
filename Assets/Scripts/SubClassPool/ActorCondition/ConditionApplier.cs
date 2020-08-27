using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

namespace ActorConditionNamespace
{
	[Serializable]
	public class ConditionApplier
	{
		protected GameSystemService serviceInstance;
        private ActorConditionManager acm;
		public Actor targetActor;
		protected ActorConditionInfo conditionInfo;
		public ActorConditionType conditionType;
		public float duration;
		public float effectiveness;

		public float playTime = 0f;
		public bool IsBusy = false;

		protected bool flag_finish = false;

		public virtual void Init (Actor target, ActorConditionType conType, float duration, float effectiveness)
		{
			//serviceInstance = GameSystemService.Instance;
            serviceInstance = GameObject.FindObjectOfType<GameSystemService>();
            acm = GameObject.FindObjectOfType<ActorConditionManager>();
            targetActor = target;
			conditionInfo = targetActor.actorInfo.conditionInfo;
			conditionType = conType;
			UpdateApplierInfo(duration, effectiveness);
		}
		public virtual void UpdateApplierInfo(float duration, float effectiveness)
		{
			this.duration = duration;
			this.effectiveness = effectiveness;
		}

		public virtual void StartTimer (float duration, float effectiveness)
		{
			Debug.Log("Actor : " + targetActor.name + ", " + conditionType.ToString() + " Start");
			playTime = 0f;
			UpdateApplierInfo(duration, effectiveness);
            acm.RaiseChildThreadUpdate += UpdateTimer;
            //serviceInstance.Add_ApplierTimer_Subscriber(UpdateTimer);
			flag_finish = false;
			IsBusy = true;
		}
		public virtual void StopTimer ()	//	외부에서 강제적으로 호출 가능, 때문에 StopByForced로 예외처리
		{
			//Debug.Log("Actor : " + targetActor.name + ", " + conditionType.ToString() + " Stop");
			if (!flag_finish)
				StopByForced ();
            acm.RaiseChildThreadUpdate -= UpdateTimer;
            //serviceInstance.Remove_ApplierTimer_Subscriber(UpdateTimer);
			IsBusy = false;
		}
		public virtual void UpdateTimer (object sender, EventArgumentNamespace.ConditionEventArg arg)
		{
			duration -= Time.deltaTime;
			if (Check())
			{
				Apply();
			}
			else
			{
				RemoveApplier();
			}
		}
		protected virtual bool Check()
		{
			if (duration >= 0 && IsBusy)
				return true;
			return false;
		}
		protected virtual void Apply(){	}
		protected virtual void RemoveApplier()	//	정상적으로 UpdateTimer가 순회 되야 들어 올 수 있음.
		{
			//  serviceInstance.RemoveApplierTimer(UpdateTimer);
			flag_finish = true;
			StopTimer();
			conditionInfo.RemoveCondition(conditionType, this);
		}
		protected virtual void StopByForced()
		{
			//	TODO : Apply에서 미쳤던 영향을 취소해야 할 경우, 채워넣어야함.
			//	TODO : Animation 상태 초기화
		}
	}
}
