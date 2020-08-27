using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace BossSpace.MetatronSpace.SubInfo
{
	[System.Serializable]
	public class TutorialInfo
	{
		public bool isTutorial = true;
		public int highParryCount = 2;
		public int middleParryCount = 2;
		public int lowParryCount = 2;
		public int comboParryCount = 2;
		public InformationNamespace.SkillInfo playerSkillInfo;
		public GameObject tutoUI01, tutoUI02, tutoUI03, tutoUI04, tutoUI05;
	}
	[System.Serializable]
	public class SpeedInfo
	{
		public float walkSpeed = 3f; 
	}


	[System.Serializable]
	public class DisInfo 
	{
		public float disInExShort = 2f;
		public float disInShort = 4f;
		public float disInFar = 6f;
		public float disToPlayer;
		public float dirToPlayer;
	}
	[System.Serializable]
	public class TwoPointMoveInfo
	{
		public bool isMoving;
		public Timer moveTimer;
		public Timer coolTimer;
		public Spine.Unity.Modules.SkeletonGhost moveGhost;
		public AnimationCurve moveCurve;
		public List<Transform> points;

		public ParticleSystem predictEffect;
		public ParticleSystem landEffect;

		public void Initialize ()
		{
			isMoving = true;
			moveTimer.Initialize ();
			coolTimer.Initialize ();
			moveGhost.ghostingEnabled = true;
		}
		public void IncMoveTimer (float amount)
		{
			moveTimer.IncTimer (amount);
		}
		public void IncCoolTimer (float amount)
		{
			coolTimer.IncTimer (amount);
		}
		public bool CheckMoveTimer ()
		{
			return moveTimer.CheckTimer ();
		}
		public bool CheckCoolTimer ()
		{
			return coolTimer.CheckTimer ();
		}
		public float EvaluateCurve ()
		{
			return moveCurve.Evaluate (moveTimer.timer / moveTimer.duration);
		}
		public float EvaluateCurve (float timer, float duration)
		{
			return moveCurve.Evaluate (timer / duration);
		}
		public Vector3 Origin {
			get{
				return points [0].position;
			}
			set{
				points [0].position = value;
			}
		}
		public Vector3 Destination { 
			get {
				return points [1].position;
			}
			set {
				points [1].position = value;
			}
		}
		public Vector3 GetPosition (float lerpValue)
		{
			return Vector3.Lerp (points [0].position, points [1].position, lerpValue); 
		}
		public void Reset ()
		{
			isMoving = false;
			moveTimer.Reset ();
			coolTimer.Reset ();
			moveGhost.ghostingEnabled = false;
		}
	}
	public static class MetatronFlag
	{
		public static Metatron metatronInstance;
		public static Metatron _metatronInstance
		{
			get {
				if (null == metatronInstance)
					return metatronInstance = GameObject.FindObjectOfType<Metatron> ();
				return metatronInstance;
			}
		}
		public static bool IsAttackState
		{
			get
			{
				return _metatronInstance.fsm.nowState == _metatronInstance.fsm.GetState<AttackState>();
			}
		}
		public static bool IsStunState
		{
			get
			{
				return _metatronInstance.fsm.nowState == _metatronInstance.fsm.GetState<StunState>();
			}
		}
		public static bool IsMoving
		{
			get {
				return _metatronInstance.tpMoveInfo.isMoving;
			}
			set {
				if (value)
					_metatronInstance.tpMoveInfo.Initialize ();
				else
					_metatronInstance.tpMoveInfo.Reset ();
			}
		}
		public static bool IsNormal
		{
			get 
			{
				return (!IsStunState && !IsMoving && !IsAttackState);
			}
		}
		public static bool DisInExShort
		{
			get
			{
				return _metatronInstance.disInfo.disToPlayer <= _metatronInstance.disInfo.disInExShort;	
			}
		}
		public static bool DisInShort
		{
			get 
			{
				return _metatronInstance.disInfo.disToPlayer <= _metatronInstance.disInfo.disInShort;
			}
		}
		public static bool DisInMiddle
		{
			get 
			{
				return  _metatronInstance.disInfo.disToPlayer > _metatronInstance.disInfo.disInShort &&
						_metatronInstance.disInfo.disToPlayer < _metatronInstance.disInfo.disInFar;
			}
		}
		public static bool DisInFar
		{
			get 
			{
				return _metatronInstance.disInfo.disToPlayer >= _metatronInstance.disInfo.disInFar;
			}
		}
	}
}