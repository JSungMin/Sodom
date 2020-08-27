using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInfoNamespace
{
	[System.Serializable]
	public class BaseBehaviourInfo {
		public float sightLength;
		public float sightAngle;
		public float delayToNextPattern;
		public AIPatternType nextPattern;

		public bool CheckDelayTimer (float delayTimer)
		{
			if (delayTimer >= delayToNextPattern) {
				return true;
			}
			return false;
		}
	}
	
	[System.Serializable]
	public class IdleBehaviourInfo : BaseBehaviourInfo
	{
			
	}
	[System.Serializable]
	public class PatrolBehaviourInfo : BaseBehaviourInfo
	{
		public float walkTimeLimit = 0.5f;
		public float restTimeLimit = 0.5f;
		private float patrolTimer = 0f;
		private int patrolIndex;
		public AxisType patrolAxisType;
		public List<Vector3> patrolPoints = new List<Vector3>();
		private bool isWalking = true;

		public void SetIsWalking (bool val)
		{
			isWalking = val;
		}
		public bool GetIsWalking ()
		{
			return isWalking;
		}
		public bool CheckWalkTimer ()
		{
			if (patrolTimer >= walkTimeLimit)
				return true;
			return false;
		}
		public bool CheckRestTimer ()
		{
			if (patrolTimer >= restTimeLimit)
				return true;
			return false;
		}
		public void UpdatePatrolTimer ()
		{
			patrolTimer += Time.deltaTime;
		}
		public int GetPatrolIndex()
		{
			return patrolIndex;
		}
		public int UpdatePatrolIndex()
		{
			patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
			return patrolIndex;
		}
		public void ResetPatrolTimer()
		{
			patrolTimer = 0f;
		}
		public Vector3 GetCurrentPatrolPoint ()
		{
			return patrolPoints [patrolIndex];
		}
	}
	[System.Serializable]
	public class ChaseBehaviourInfo : BaseBehaviourInfo
	{
		//	AttackSightLength 는 AttackBehaviourInfo에서 Get
		//	AttackSightAngle 은 AttackBehaviourInfo에서 Get
		public float targetLostTime;
		[SerializeField]
		private float targetLostTimer;

		public bool useRoaming = true;
		public bool isRoaming = false;
		public int roamingCount = 2;
		public int currentRoamingCount = 0;
		public float roamingTime = 0.25f;
		private float roamingTimer = 0f;

		public AIPatternType afterRoamingPattern;

		public bool CheckTargetLostTimer ()
		{
			if (targetLostTimer >= targetLostTime)
				return true;
			return false;
		}

		public bool CheckRoamingTimer ()
		{
			if (roamingTimer >= roamingTime)
				return true;
			return false;
		}
		public bool CheckRoamingCount ()
		{
			if (currentRoamingCount >= roamingCount)
				return true;
			return false;
		}

		public void ResetTargetLostTimer()
		{
			targetLostTimer = 0f;
		}
		public void ResetRoamingTimer ()
		{
			roamingTimer = 0f;
		}
		public void ResetRoamingCount ()
		{
			currentRoamingCount = 0;
		}
		public void ResetAllRoamingValues ()
		{
			ResetRoamingTimer ();
			ResetRoamingCount ();
			this.isRoaming = false;
		}

		public void UpdateTargetLostTimer()
		{
			targetLostTimer += Time.deltaTime;
		}
		public void UpdateRoamingTimer ()
		{
			roamingTimer += Time.deltaTime;
		}
	}
	[System.Serializable]
	public class AttackBehaviourInfo : BaseBehaviourInfo
	{
		public float attackDelay = 1f;
		public float attackDelayTimer = 0f;
		public InformationNamespace.AttackInfo defaultAttackBehaviour;
		public List<InformationNamespace.AttackInfo> attackBehaviourInfoList = new List<InformationNamespace.AttackInfo>();
	}

	[System.Serializable]
	public class DeadBehaviourInfo : BaseBehaviourInfo
	{
		public ParticleSystem deadEffect;
		public AnimationCurve disappearCurve;
		public UnityEngine.Events.UnityEvent onDeadEvent;
	}
}
