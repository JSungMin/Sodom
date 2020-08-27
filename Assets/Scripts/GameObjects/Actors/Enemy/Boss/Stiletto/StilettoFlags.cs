using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace BossSpace.StilettoSpace.SubInfo
{
	[System.Serializable]
	public class SpeedInfo
	{
		public float madRunSpeed = 12.5f, 
					 walkSpeed = 12.5f * 0.3f, 
					 chargeSpeed = 12.5f * 0.6f;
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
	[System.Serializable]
	public class BaldoInfo
	{
		public bool isBaldoReady;
		public ParticleSystem effect;
		public Timer attackTimer;
		public Vector3 destPos;
		public int conBaldoNum = 0;

		public void Initialize ()
		{
			isBaldoReady = true;
			attackTimer.Reset ();
		}
		public void Reset ()
		{
			isBaldoReady = false;
			attackTimer.Reset ();
		}
		public void PlayEffect ()
		{
			effect.Play ();
		}
	}
	[System.Serializable]
	public class ShotGunBurstInfo
	{
		public bool isShotGunBurst = false;
		public int burstNum = 3;
		public int maxBurstNum = 3;

		public void Initialize ()
		{
			isShotGunBurst = true;
			burstNum = Random.Range (1, maxBurstNum);
		}
		public void Reset ()
		{
			isShotGunBurst = false;
			burstNum = 0;
		}
	}
	[System.Serializable]
	public class BaldoBurstInfo
	{
		public bool isBaldoBurst = false;
		public int burstNum = 3;
		public int maxBurstNum = 3;

		public void Initialize ()
		{
			isBaldoBurst = true;
			burstNum = Random.Range (1, maxBurstNum);
			Debug.Log (burstNum);
		}
		public void Reset ()
		{
			isBaldoBurst = false;
			burstNum = 0;
		}
	}
	[System.Serializable]
	public class AngerGauge
	{
		public int curGauge = 0;
		public int maxGauge = 100;
		public int remainAngryCount = 0;
		public int maxAngryCount = 3;
		public bool isAngry = false;

		public ParticleSystem angryParticle;

		public delegate void DefaultEvent ();
		public DefaultEvent OnEnterAngry;
		public DefaultEvent OnExitAngry;

		public void IncAngerGauge (float amount)
		{
			curGauge += (int)amount;
			if (CheckAngerGauge ()) 
			{
				if (null != OnEnterAngry) 
				{
					OnEnterAngry.Invoke ();
				}
			}
		}
		public bool CheckAngerGauge ()
		{
			if (curGauge >= maxGauge) 
				return true;
			return false;
		}
		public void DecAngryCount ()
		{
			remainAngryCount = Mathf.Max (0, remainAngryCount - 1);
		}
		public void SetAngerValue ()
		{
			curGauge = maxGauge;
			remainAngryCount = maxAngryCount;
			isAngry = true;
		}
		public void ResetGauge ()
		{
			curGauge = 0;
			remainAngryCount = 0;
			isAngry = false;
		}
	}
	public static class StilettoFlag
	{
		public static Stiletto stillettoInstance;
		public static Stiletto _stilettoInstance
		{
			get {
				if (null == stillettoInstance)
					return stillettoInstance = GameObject.FindObjectOfType<Stiletto> ();
				return stillettoInstance;
			}
		}
		public static bool IsAttackState
		{
			get
			{
				return _stilettoInstance.fsm.nowState == _stilettoInstance.fsm.GetState<AttackState>();
			}
		}
		public static bool IsStunState
		{
			get
			{
				return _stilettoInstance.fsm.nowState == _stilettoInstance.fsm.GetState<StunState>();
			}
		}
		public static bool IsCancleableAttack
		{
			get {
				if (!IsAttackState)
					return false;
				if (_stilettoInstance.nowPattern.patternName == StilettoPatternType.상단킥_중단찌르기_중단베기.ToString ())
					return true;
				return false;
			}
		}
		public static bool IsBaldoAttack
		{
			get {
				return _stilettoInstance.baldoInfo.isBaldoReady;
			}
			set
			{
				if (value)
					_stilettoInstance.baldoInfo.Initialize ();
				else
					_stilettoInstance.baldoInfo.Reset ();
			}
		}
		public static bool IsShotGunBurst
		{
			get	{
				return _stilettoInstance.shotBurstInfo.isShotGunBurst;
			}
			set {
				if (value)
					_stilettoInstance.shotBurstInfo.Initialize ();
				else
					_stilettoInstance.shotBurstInfo.Reset ();
			}
		}
		public static bool IsBaldoBurst
		{
			get {
				return _stilettoInstance.baldoBurstInfo.isBaldoBurst;
			}
			set {
				if (value)
					_stilettoInstance.baldoBurstInfo.Initialize ();
				else
					_stilettoInstance.baldoBurstInfo.Reset ();
			}
		}
		public static bool IsMoving
		{
			get {
				return _stilettoInstance.tpMoveInfo.isMoving;
			}
			set {
				if (value)
					_stilettoInstance.tpMoveInfo.Initialize ();
				else
					_stilettoInstance.tpMoveInfo.Reset ();
			}
		}
		public static bool IsAngry
		{
			get
			{
				return _stilettoInstance.angerInfo.isAngry; 
			}
		}
		public static bool IsOnlyAngry
		{
			get {
				return (!IsStunState && IsAngry && !IsMoving && !IsAttackState && !_stilettoInstance.skipBehaviour);
			}
		}
		public static bool IsNormal
		{
			get 
			{
				return (!IsStunState && !IsAngry && !IsMoving && !IsAttackState && !_stilettoInstance.skipBehaviour);
			}
		}
		public static bool DisInExShort
		{
			get
			{
				return _stilettoInstance.disInfo.disToPlayer <= _stilettoInstance.disInfo.disInExShort;	
			}
		}
		public static bool DisInShort
		{
			get 
			{
				return _stilettoInstance.disInfo.disToPlayer <= _stilettoInstance.disInfo.disInShort;
			}
		}
		public static bool DisInMiddle
		{
			get 
			{
				return  _stilettoInstance.disInfo.disToPlayer > _stilettoInstance.disInfo.disInShort &&
						_stilettoInstance.disInfo.disToPlayer < _stilettoInstance.disInfo.disInFar;
			}
		}
		public static bool DisInFar
		{
			get 
			{
				return _stilettoInstance.disInfo.disToPlayer >= _stilettoInstance.disInfo.disInFar;
			}
		}
	}
}