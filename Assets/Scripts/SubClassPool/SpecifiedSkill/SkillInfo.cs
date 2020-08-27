using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InformationNamespace
{
	[Serializable]
	public class SkillCoolInfo
	{
		private Actor targetActor;
		public float coolTimer = 0f;
		public float warmupTimer = 0f;
		public SkillInfo skillInfo;

		public delegate void CoolBehaviour ();
		public CoolBehaviour OnReady;
		public CoolBehaviour OnWarmup;
		public CoolBehaviour OnReset;

		public SkillCoolInfo ()
		{
			coolTimer = 0f;
			skillInfo = new SkillInfo ();
		}
		public SkillCoolInfo (Actor actor, SkillInfo skillInfo)
		{
			targetActor = actor;
			coolTimer = 0f;
			this.skillInfo = skillInfo;
			OnWarmup += delegate() 
			{
				warmupTimer += Time.deltaTime;
				if (warmupTimer >= skillInfo.warmupTime) {
					if (!IsUseable ())
						return;
					if (null != OnReady)
						OnReady ();

				}
			};
			OnReset += delegate () {
				coolTimer = 0f;
				warmupTimer = 0f;
			};
		}

		public bool IsUseable ()
		{
			if (coolTimer < skillInfo.cooldownTime) {
				return false;
			}
			if (warmupTimer < skillInfo.warmupTime) {
				return false;
			}
			if (skillInfo.consumeType == ConsumeResourceType.HP) {
				if (targetActor.actorInfo.GetLife () < skillInfo.consumeAmount)
					return false;
			}
			else if (skillInfo.consumeType == ConsumeResourceType.MP) {
				if (targetActor.actorInfo.GetEnergy () < skillInfo.consumeAmount)
					return false;
			}
			return true;
		}
		public void OnCooling ()
		{
			coolTimer += Time.deltaTime;
			if (coolTimer >= skillInfo.cooldownTime) {
				if (!IsUseable ())
					return;
				if (null != OnReady)
					OnReady ();

			}
		}
	}
	[Serializable]
	public class SkillInfo
	{
		public string skillName;
		public string prevSkillName;
		public int animIndex;
		public string animName;
		public string fxName;							//	이펙트 효과
		public AudioType useSfxType;
		public AudioType successSfxType;
		public bool animLoop = true;
		public bool isLearned = false;
		public SkillEntityType entityType;			//	Ground Skill과 Air Skill를 구분
		public SkillType skillType;					//	스킬의 종류
		public SkillCounterType skillCounterType;
		public SkillDamageType skillDamageType;		//	Skill에 맞았을때 Damage 애니메이션 결정
		public ConsumeResourceType consumeType;
		public List<KeyCode> skillCommand;				//	스킬 사용을 위한 Command List
		public bool lastNode;							//	스킬 연계 중 마지막 스킬인가
		public List<ActorConditionType> conditionEffects;		//	Actor에게 어떤 상태변화를 미치는지를 정한다.
		public List<float> durations;					//	각 스킬의 상태이상 적용 기간
		public List<float> effectiveness;				//	각 상태이상의 효력이며, skill_effect_type과 list의 count가 같아야 함
		public float damage;
		public float animationSpeed = 1;
		public float warmupTime;						//	스킬 선 딜레이
		public float cooldownTime;						//	스킬 후 딜레이
		public float consumeAmount;						//	에너지 소모양 
		public float knockoutTime;						//	피격시 Stun Duration
		public bool useInertia = false;					//	Attack에서 관성을 사용 할 것인가

		[Header("- Attack State Event List")]
		public List<string> onAttackEnterEventList = new List<string> ();
		public List<string> onAttackUpdateEventList = new List<string>();
		public List<string> onAttackExitEventList = new List<string>();

		[Header("- Damaged State Event List")]
		public List<string> onDamagedEnterEventList = new List<string> ();
		public List<string> onDamagedUpdateEventList = new List<string>();
		public List<string> onDamagedExitEventList = new List<string>();
		[Space (10f)]

		public CameraVibrateAmount vibrateAmount;	//	공격에 의해 카메라가 얼마나 흔들릴 것인가.
		public float forwardStepAmount;				//	기본적으로 공격시 얼만큼 전진하는 지를 정함
		public float dashAmount;					//	SkillContent 내 Dash 시 얼만큼 dash 할지 정함
		public float smashAmount;					//	얼만큼 날려버릴지 정함
		public float launchAmount;					//	얼만큼 띄울지를 정함
		public Vector3 teleportDir;					//	현재 위치에서 어느 방향으로 갈지 정함
		public float teleportAmount;				//	현재 위치에서 얼만큼 이동할지 정함

		public SkillInfo ()
		{
			skillName = "NONAME";
			skillType = SkillType.NORMAL;

		} 
		public SkillInfo (
			string name, 
			string animName,
			bool isLearned, 
			SkillType skillType, 
			List<KeyCode> skillCommand,
			List<ActorConditionType> conditions,
			List<float> durations,
			List<float> effectiveness,
			float warmup_delay,
			float cooldownDelay,
			float consumeAmount,
			string fxName
		)
		{
			this.skillName = name;
			this.animName = animName;
			this.isLearned = isLearned;
			this.skillType = skillType;
			this.skillCommand = skillCommand;
			this.conditionEffects = conditions;
			this.durations = durations;
			this.effectiveness = effectiveness;
			if (conditions.Count != durations.Count || 
				durations.Count != effectiveness.Count ||
				effectiveness.Count != conditions.Count)
			{
				Debug.LogError ("SKILL : " + name + ", Foramt Error");
			}
			this.cooldownTime = cooldownDelay;
			this.consumeAmount = consumeAmount;
			this.fxName = fxName;
		}
		public bool IsEmpty ()
		{
			if (skillName == "NONAME") {
				return true;
			}
			return false;
		}
		public bool HasCondition (ActorConditionType t)
		{
			for (int i = 0; i < conditionEffects.Count; ++i)
			{
				if (t == conditionEffects[i])
				{
					return true;
				}
			}
			return false;
		}
	}
}
