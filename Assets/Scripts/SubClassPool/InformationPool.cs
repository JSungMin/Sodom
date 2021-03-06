using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using EventArgumentNamespace;
using ActorConditionNamespace;

namespace InformationNamespace
{
	//	32 byte
	[Serializable]
	public class BasicStatInfo
	{
		//	기본 능력치
		public float physicalPower;
		public float physicalDefense;
		public float specialPower;
		public float specialDefense;
		public float life;
		public float maxLife;
		public float energy;
		public float maxEnergy;
		public float moveSpeed;
		public float jumpSpeed;
		public float attackSpeed;
		public float combatExperience;
		public float immunity;
		public float quickness;			

		public float recyclablityLife;
		public float recyclablityEnergy;

		public void AddStat(BasicStatInfo stat)
		{
			physicalPower += stat.physicalPower;
			physicalDefense += stat.physicalDefense;
			specialPower += stat.specialPower;
			specialDefense += stat.specialDefense;
			life += stat.life;
			maxLife += stat.maxLife;
			energy += stat.energy;
			maxEnergy += stat.maxEnergy;
			moveSpeed += stat.moveSpeed;
			attackSpeed += stat.attackSpeed;
			combatExperience += stat.combatExperience;
			immunity += stat.immunity;
			quickness += stat.quickness;

			recyclablityLife += stat.recyclablityLife;
			recyclablityEnergy += stat.recyclablityEnergy;
		}
		public BasicStatInfo SubStat(BasicStatInfo stat)
		{
			physicalPower -= stat.physicalPower;
			physicalDefense -= stat.physicalDefense;
			specialPower -= stat.specialPower;
			specialDefense -= stat.specialDefense;
			life -= stat.life;
			maxLife -= stat.maxLife;
			energy -= stat.energy;
			maxEnergy -= stat.maxEnergy;
			moveSpeed -= stat.moveSpeed;
			attackSpeed -= stat.attackSpeed;
			combatExperience -= stat.combatExperience;
			immunity -= stat.immunity;
			quickness -= stat.quickness;

			recyclablityLife -= stat.recyclablityLife;
			recyclablityEnergy -= stat.recyclablityEnergy;
			return this;
		}
	}
	[Serializable]
	public class ActorInfo
	{
		public int id;
		public string actor_name;
		public ActorType actorType;

		public bool isGrounded		= true;		// Rigidbody의 Velocity.y가 0에 근사하면 true, 아니면 false
		public bool isSuperarmor	= false;
		public bool isWallCollision = false;	// 벽꿍했는가에 대한 Flag
		public bool isStunByParry	= false;	// Parry에 의해 Stun 됐는가 (다른 요인에 의해 Stun 됐을때 구분)	
		public bool canDoAirBehaviour = true;
		public AnimationCurve rollingSpeedCurve;

		//	Skill Information
		public List<string> learnedSkillNameList;
		public Dictionary<string, SkillInfo> learnedSkillMap;

		public ActorConditionInfo conditionInfo;

		//	Stat Block
		public BasicStatInfo basicStat;
		public BasicStatInfo additionalBasicStat;
		private BasicStatInfo totalBasicStat;

		public string stage_name;
		public string roomName;
		[HideInInspector]
		public Vector3 lastPosition;
		[HideInInspector]
		public Vector3 lastScale;
		[HideInInspector]
		public Vector3 lastVelocity;
		//	Init Block Start
		public void Init(Actor actor)
		{
			conditionInfo = new ActorConditionInfo
			{
				actor = actor
			};
			//	Make Empty Condition Pool
			PoolingConditions (actor);
			ApplyStat ();
			SyncSkillMapWithList ();
		}

		private void PoolingConditions (Actor actor)
		{
			for (int i = 0; i < ActorConditionManager.conditionNum; i++) {
				conditionInfo.overlapInfoList.Add (0);
				conditionInfo.applierPool.Add (new List<ConditionApplier> ());
				for (int j = 0; j <= ActorConditionManager.overlap_limit [i]; j++) {
					var conType = (ActorConditionType)i;
					var tmpAppList = conditionInfo.applierPool [i];
					switch (conType) {
					case ActorConditionType.SLEEP:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Sleep> (actor, conType));
						break;
					case ActorConditionType.DISCHARGE:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Discharge> (actor, conType));
						break;
					case ActorConditionType.BURN:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Burn> (actor, conType));
						break;
					case ActorConditionType.FROSTBITE:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Frostbite> (actor, conType));
						break;
					case ActorConditionType.ICED:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Iced> (actor, conType));
						break;
					case ActorConditionType.BERSERKER:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Berserker> (actor, conType));
						break;
					case ActorConditionType.PARALYSIS:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Paralysisc> (actor, conType));
						break;
					case ActorConditionType.STUN:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Stun> (actor, conType));
						break;
					case ActorConditionType.FRACTURE:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Fracture> (actor, conType));
						break;
					case ActorConditionType.POISIONING:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Poisioning> (actor, conType));
						break;
					case ActorConditionType.BLOODING:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Blooding> (actor, conType));
						break;
					case ActorConditionType.UNBEATABLE:
						tmpAppList.Add (conditionInfo.MakeEmptyApplier<Unbeatable> (actor, conType));
						break;
					}
				}
			}
		}
		public void ApplyStat()
		{
			//	Apply Base Detail Stat

			//	Calculate Total Stat
			totalBasicStat = basicStat;
			totalBasicStat.AddStat (additionalBasicStat);
		}
		//	Init Block End

		public BasicStatInfo GetTotalBasicInfo()
		{
			return totalBasicStat;
		}

		public void AddLife (float life)
		{
			totalBasicStat.life = Mathf.Min (basicStat.life + life, basicStat.maxLife);
		}
		public void SubLife (float life)
		{
			totalBasicStat.life = Mathf.Max (basicStat.life - life, 0f);
		}
		public void SetLife (float energy)
		{
			totalBasicStat.energy = Mathf.Min (energy, totalBasicStat.maxEnergy);;
		}
		public float GetLife ()
		{
			return totalBasicStat.life;
		}
		public float GetLifePercent ()
		{
			return totalBasicStat.life / totalBasicStat.maxLife;
		}
		public float GetEnergyPercent ()
		{
			return totalBasicStat.energy / totalBasicStat.maxEnergy;
		}
		public void AddEnergy (float energy)
		{
			totalBasicStat.energy = Mathf.Min (basicStat.energy + energy, basicStat.maxEnergy);
		}
		public void SubEnergy (float energy)
		{
			totalBasicStat.energy = Mathf.Max (basicStat.energy - energy, 0f);
		}
		public void SetEnergy (float energy)
		{
			totalBasicStat.energy = Mathf.Min (energy, totalBasicStat.maxEnergy);;
		}
		public float GetEnergy ()
		{
			return totalBasicStat.energy;
		}

		//	Skill Utility Block
		public void LearnSkill(string skillName)
		{
			if (actorType == ActorType.PLAYER)
			{
				var result = GameSystemService.Instance.GetPlayerSkillInfoByName (skillName);
				if (null == result)
					return;
				if (learnedSkillMap.ContainsKey (skillName))
					return;
				learnedSkillNameList.Add (result.skillName);
				learnedSkillMap[skillName] = result;
			}
			else if (actorType == ActorType.ENEMY)
			{
				var result = GameSystemService.Instance.GetEnemySkillInfoByName (skillName);
				if (null == result)
					return;
				if (learnedSkillMap.ContainsKey (skillName))
					return;
				learnedSkillNameList.Add (result.skillName);
				learnedSkillMap[skillName] = result;
			}
		}
		public void SyncSkillMapWithList()
		{
			learnedSkillMap = new Dictionary<string, SkillInfo> ();
			for (int i = 0; i < learnedSkillNameList.Count; i++)
			{
				if (learnedSkillMap.ContainsKey (learnedSkillNameList [i]))
					continue;
				if (actorType == ActorType.PLAYER)
					learnedSkillMap [learnedSkillNameList [i]] = GameSystemService.Instance.GetPlayerSkillInfoByName (learnedSkillNameList [i]);
				else if (actorType == ActorType.ENEMY)
					learnedSkillMap [learnedSkillNameList [i]] = GameSystemService.Instance.GetEnemySkillInfoByName (learnedSkillNameList [i]);
			}
		}
		public SkillInfo GetLearnedSkill(string skillName)
		{
			if (learnedSkillMap.ContainsKey(skillName))
				return learnedSkillMap [skillName];
			return null;
		}
		public bool IsActorLearnedSkill(string skillName)
		{
			if (learnedSkillMap.ContainsKey (skillName)) {
				return (learnedSkillMap [skillName] != null) ? true : false;
			}
			return false;
		}
		//	End of Skill Utility Block

		public float CalcRollingSpeedGraph(float norTime)
		{
			return rollingSpeedCurve.Evaluate (norTime);
		}

		// Quest Utility Block

	}

	[Serializable]
	public class ActorConditionInfo
	{
		public Actor actor;
		public List<ActorConditionType> conditionTypeList = new List<ActorConditionType>();	//	캐릭터는 중복되고 다양한 컨디션을 가질 수 있다.
		//  applierPool[x][y] : x 에는 conditionType, y에는 applier index가 요구된다.
		public List<List<ConditionApplier>> applierPool = new List<List<ConditionApplier>>();
		public List<int> overlapInfoList = new List<int>();


		public ActorConditionInfo() {   }
		public ActorConditionInfo(Actor actor)
		{
			this.actor = actor;
		}

		//  Inner Helper Function Block
		public T MakeEmptyApplier<T> (Actor actor, ActorConditionType conType) where T : ConditionApplier, new()
		{
			var newApplier = new T();
			newApplier.Init(actor, conType, 0f, 0f);
			return newApplier;
		}
		private ConditionApplier FindNotBusyApplier (ActorConditionType conType)
		{
			//Debug.Log("Find Not Busy : " + conType + ", " + (int)conType);
			for (int i = 0; i < applierPool[(int)conType].Count; i++)
			{
				var applier = applierPool[(int)conType][i];
				if (!applier.IsBusy)
					return applier;
			}
			return null;
		}
		private ConditionApplier FindBusyApplier (ActorConditionType conType)
		{
			for (int i = 0; i < applierPool[(int)conType].Count; i++)
			{
				var applier = applierPool[(int)conType][i];
				if (applier.IsBusy)
					return applier;
			}
			return null;
		}
		//   End Block

		public void AddCondition (ActorConditionType con, float duration, float effectiveness)
		{
			if (!CheckOverlapValidation(con)) {
				//Debug.Log("중복 Max : " + con.ToString());
				var app = FindBusyApplier(con);
				RemoveCondition(con, app);
			}
			conditionTypeList.Add (con);
			var conVal = (int)con;
			overlapInfoList[conVal]++;
			var applier = FindNotBusyApplier(con);
			applier.StartTimer(duration, effectiveness);
		}
		public void RemoveCondition (ActorConditionType con)
		{
			var app = FindBusyApplier (con);
			RemoveCondition (con, app);
		}
		public void RemoveCondition (ActorConditionType con, ConditionApplier applier)
		{
			conditionTypeList.Remove(con);
			if (null != applier)
				applier.StopTimer();
			int conVal = (int)con;
			overlapInfoList[conVal] = Mathf.Max(0, overlapInfoList[conVal] - 1);
		}
		public bool CheckOverlapValidation (ActorConditionType con)
		{
			var conVal = (int)con;
			if (overlapInfoList[conVal] < ActorConditionManager.overlap_limit[conVal])
				return true;
			return false;
		}
	}
	[Serializable]
	public class EnemyInfo 
	{
		public List<DropItemInfo> dropItemList;
	}
	[Serializable]
	public class ActionStateInfo
	{
		public string stateName;
		public int animIndex;
		public string animName;
		public string defaultAnimName;

		public ActionStateInfo (string state_name, string anim_name)
		{
			this.stateName = state_name;
			this.animName = anim_name;
			this.defaultAnimName = anim_name;
		}
	}

	[Serializable]
	public class InteractInfo
	{
		public Actor interactObj;
		public Actor interactWith;
		public InteractType interactType;

		public bool nowInteract = false;

		public InteractInfo(Actor self, Actor interactWith, InteractType interactType)
		{
			this.interactObj = self;
			this.interactWith = interactWith;
			this.interactType = interactType;
		}
	}
	[Serializable]
	public class EnemyAttackPatternInfo
	{
		//	Initialize on Init Func variables
		private Actor targetActor;
		private ActorActionFSM fsm;
		private SkillCoolTimer skillTimer;

		public string patternName;
		public int patternPriority = 0;
		public List<string> skillBuffer;
		public EnemyAttackPatternType patternType;

		public void Init (Actor actor)
		{
			targetActor = actor;
			fsm = targetActor.fsm;
			skillTimer = targetActor.skillCoolTimer;
		}

		private string GetCurSkillName (int patternIndex)
		{
			return skillBuffer [patternIndex];
		}
		public SkillInfo GetsSkillInfo (int patternIndex)
		{
			return skillTimer.GetSkill (GetCurSkillName (patternIndex));
		}
		public SkillCounterType GetCounterType (int patternIndex)
		{
			return GetsSkillInfo (patternIndex).skillCounterType;
		}

		public void CoolingPattern ()
		{
			for (int i = 0; i < skillBuffer.Count; i++)
			{
				skillTimer.Cooling (skillBuffer [i]);
			}
		}
		public void WarmupPattern ()
		{
			for (int i = 0; i < skillBuffer.Count; i++)
			{
				if (skillTimer.IsCooled(skillBuffer[i]))
					skillTimer.Warmup (skillBuffer [i]);
			}
		}
		public bool CheckUseable (int patternIndex)
		{
			for (int i = patternIndex; i < skillBuffer.Count; i++)
			{
				if (!skillTimer.IsUseable (skillBuffer [i]))
					return false;
			}
			return true;
		}
		public void ResetPattern ()
		{
			for (int i = 0; i < skillBuffer.Count; i++)
			{
				skillTimer.ResetSkill (skillBuffer [i]);
			}
		}
		public void ReadyPattern (int patternIndex)
		{
			for (int i = patternIndex; i < skillBuffer.Count; i++)
			{
				skillTimer.MakeFullCharge (skillBuffer [i]);
			}
		}
		public bool UseSkill (ref int patternIndex)
		{
			var indxSkillName = GetCurSkillName (patternIndex);
			var result = fsm.TryTransferAction <AttackState> (skillTimer.GetSkill (indxSkillName));
			return result;
		}
		public bool UseNextSkill (ref int patternIndex)
		{
			var nextIdx = patternIndex + 1;
			if (nextIdx > skillBuffer.Count - 1)
				return false;
			var result = UseSkill (ref nextIdx);
			if (result)
				patternIndex++;
			return true;
		}
	}

	[Serializable]
	public class DamageInfo
	{
		public Actor attacker;
		public Actor victim;
		public SkillInfo skillInfo;

		public DamageInfo (){	}
		public DamageInfo (Actor attacker, Actor victim, SkillInfo skillInfo)
		{
			this.attacker = attacker;
			this.victim = victim;
			this.skillInfo = skillInfo;
		}

		public bool IsEmpty ()
		{
			if (skillInfo.IsEmpty())
				return false;
			if (null == attacker)
				return false;
			if (null == victim)
				return false;
			return true;
		}

		public bool HasCondition (ActorConditionType t)
		{
			return skillInfo.HasCondition (t);
		}
	}
	[Serializable]
	public class DropItemInfo
	{
		public string itemName;
		public int itemAmount;
	}
	[Serializable]
	public class ItemInfo
	{
		public ItemType itemType;						//	아이템의 종류
		public string itemName;
		public int itemAmount;								//	아이템의 양
		public BasicStatInfo basicStatInfo;

		public int GetSpecificType ()
		{
			switch (itemType)
			{
			case ItemType.GOODS:
				GoodsInfo gInfo = this as GoodsInfo;
				if (null != gInfo)
					return (int)gInfo.goodsType;
				break;
			case ItemType.SHIELD:
				ShieldInfo sInfo = this as ShieldInfo;
				if (null != sInfo)
					return (int)sInfo.shieldType;
				break;
			case ItemType.WEAPON:
				WeaponInfo wInfo = this as WeaponInfo;
				if (null != wInfo)
					return (int)wInfo.weaponType;
				break;
			}
			return -1;
		}
	}
	[Serializable]
	public class GoodsInfo : ItemInfo
	{
		public GoodsType goodsType;
	}
	[Serializable]
	public class WeaponInfo : ItemInfo
	{
		public WeaponType weaponType;
	}
	[Serializable]
	public class ShieldInfo : ItemInfo
	{
		public ShieldType shieldType;
	}
	[Serializable]
	public class EquipInfo
	{
		public WeaponInfo weaponInfo;
		public List<ShieldInfo> shieldInfoList;
	}
	[Serializable]
	public class InventoryInfo
	{
		[SerializeField]
		private List<ItemInfo> itemInfoList = new List<ItemInfo>();
		private Dictionary<string, int> itemCountDictionary = new Dictionary<string, int>();

		public bool TryAddItem(ItemInfo itemInfo)
		{
			itemInfoList.Add (itemInfo);
			itemCountDictionary [itemInfo.itemName] += itemInfo.itemAmount;
			return true;
		}
		public List<ItemInfo> GetItemInfoList ()
		{
			return itemInfoList;
		}
		public int GetItemAmount (string itemName)
		{
			if (itemCountDictionary.ContainsKey (itemName))
				return itemCountDictionary [itemName];
			else {
				Debug.LogError ("Try GetItemAmount But Given Key is not contained");
				return -1;	//	Error
			}
		}
	}
	[Serializable]
	public class QuestProgressInfo
	{
		public List<QuestInfo> questProgressInfoList = new List<QuestInfo>();

		public void AddProgressQuest (QuestInfo questInfo)
		{
			foreach (var element in questProgressInfoList)
			{
				if (element == questInfo)
					return;
			}
			questProgressInfoList.Add (questInfo);
		}

		public void SyncQuestWithListener()
		{
			foreach (var questInfo in questProgressInfoList) {
				GameSystemService.Instance.questFactory.questInfoDic [questInfo.questName] = questInfo;
				GameSystemService.Instance.questFactory.AddQuestAction (questInfo);
				GameSystemService.Instance.questFactory.StartQuest (questInfo.questName);
			}
		}
	}
	[Serializable]
	public class RoomInfo
	{
		public string roomName;
		public List<ActorInfo> enemyInfoList;
		public List<ItemInfo> itemInfoList;

		public Rect roomRect;

		public int deadEnemyCount = 0;
		public bool isCleared;
	}
	[Serializable]
	public class QuestInfo
	{
		public string questName;						//	Quest의 이름이다.
		public string preQuest="";						//	먼저 클리어 되어있어야 하는 퀘스트의 이름이다. ""는 조건이 없는 거다.
		public QuestType questType;						//	Derived Class를 구분하기 위한 Type 명시다.
		public List<QuestRewardInfo> rewardInfoList;	//	Reward Info List
		public bool isCleared = false;					//	퀘스트의 클리어 여부를 나타낸다.
	}
	[Serializable]
	public class QuestRewardInfo
	{
		public float exp;
		public int money;
		public string itemName;
		public int itemAmount;
		public QuestRewardInfo(){	}
		public QuestRewardInfo(float exp, int money, string itemName, int itemAmount)
		{
			this.exp = exp;
			this.money = money;
			this.itemName = itemName;
			this.itemAmount = itemAmount;
		}
	}
	[Serializable]
	public class GameDataInfo
	{
		public string path;
		public string sceneName;

		public string playerActorInfo;
		public string playerEquipInfo;
		public string playerInvenInfo;
		public string playerQuestProgressInfo;
	}
	[Serializable]
	public class AllSkillInfo
	{
		public List<SkillInfo> groundSkillList = new List<SkillInfo>();
		public List<SkillInfo> airSkilllist = new List<SkillInfo>();
		public List<SkillInfo> anyTimeSkillList = new List<SkillInfo>();
	}
	[Serializable]
	public class AllQuestInfo
	{
		public List<HuntingQuestInfo> 		huntingQuestList = new List<HuntingQuestInfo> ();
		public List<EliminateQuestInfo> 	eliminationQuestList = new List<EliminateQuestInfo> ();
		public List<SkillTrainingQuestInfo> skillTrainingQuestList = new List<SkillTrainingQuestInfo> ();
		public List<CollectionQuestInfo>	collectionQuestList = new List<CollectionQuestInfo> ();
		public List<InteractionQuestInfo>	interactionQuestList = new List<InteractionQuestInfo> ();
		public List<MoveQuestInfo> 			moveQuestList = new List<MoveQuestInfo> ();

		public void AddQuest<T> (T questInfo) where T : QuestInfo
		{
			if (questInfo is HuntingQuestInfo)
				huntingQuestList.Add (questInfo as HuntingQuestInfo);
			else if (questInfo is EliminateQuestInfo)
				eliminationQuestList.Add (questInfo as EliminateQuestInfo);
			else if (questInfo is SkillTrainingQuestInfo)
				skillTrainingQuestList.Add (questInfo as SkillTrainingQuestInfo);
			else if (questInfo is CollectionQuestInfo)
				collectionQuestList.Add (questInfo as CollectionQuestInfo);
			else if (questInfo is InteractionQuestInfo)
				interactionQuestList.Add (questInfo as InteractionQuestInfo);
			else if (questInfo is MoveQuestInfo)
				moveQuestList.Add (questInfo as MoveQuestInfo);
		}
	}
	[Serializable]
	public class KeyInputInfo
	{
		public string name;
		public KeyCode input_key;
		//public bool isDown;
		//public bool isPressed;
	}
	[Serializable]
	public class InteractiveObjectInfo
	{
		public ObjectType objType;

		[SerializeField]
		private bool isInteracted = false;
		public int capacity = 0;
		public bool IsInteracted
		{
			get{
				return isInteracted;
			}
			set{
				isInteracted = value;
			}
		}
	}
	[Serializable]
	public class ObjectNameAndCountInfo
	{
		public string name;
		public int count;

		public ObjectNameAndCountInfo (string n, int c)
		{
			name = n;
			count = c;
		}
	}
	[Serializable]
	public class InteractLockInfo
	{
		public bool isLocked;
		public List<ObjectNameAndCountInfo> greaterThenInfo;

		public void CheckAndUnLock (List<ObjectNameAndCountInfo> oncInfo)
		{

		}
	}
	[Serializable]
	public class AttackInfo 
	{
		public AIAttackConditionType conditionType;
		public ComparisionType comparisionType;
		public float comparisionPercent;
		public float occurPercent;
		public SkillInfo skillInfo;
		public UnityEvent attackEvent;

		public float cooldownTimer = 0f;

		private bool CompareValue (float value)
		{
			if (comparisionType == ComparisionType.GREATER)
			{
				if (value > comparisionPercent)
					return true;
			}
			else if (comparisionType == ComparisionType.EQUAL)
			{
				if (value == comparisionPercent)
					return true;
			}
			else if (comparisionType == ComparisionType.LESS)
			{
				if (value < comparisionPercent)
					return true;
			}
			return false;
		}

		public bool CheckCondition (ActorInfo actorInfo)
		{
			switch (conditionType) {
			case AIAttackConditionType.CHECK_LIFE:
				return CompareValue (actorInfo.GetLifePercent());
			case AIAttackConditionType.CHECK_ENERGY:
				return CompareValue (actorInfo.GetEnergyPercent());
			}
			return false;
		}
		public bool CheckCoolTime ()
		{
			if (cooldownTimer >= 0f)
			{
				return false;
			}
			return true;
		}
		public void SetCoolTime ()
		{
			cooldownTimer = skillInfo.cooldownTime;
		}
	}
}
