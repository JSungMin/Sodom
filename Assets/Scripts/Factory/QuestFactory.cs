using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using InformationNamespace;

public class QuestFactory  : MonoBehaviour{
	public static bool isLoaded = false;
	//public string questPath = "SaveDatas/QuestList";
	//public string defaultQuestDir = "SaveDatas/Default/QuestInfo";
	private GameSystemService serviceInstance;
	public Player player;
	public AllQuestInfo allQuestInfo = new AllQuestInfo();

	//	Load from QuestInfo
	private Dictionary<string, Action<QuestInfo>> questActionDic = new Dictionary<string, Action<QuestInfo>>();
	public Dictionary<string, QuestInfo> questInfoDic = new Dictionary<string, QuestInfo> ();

	public void Init ()
	{
		player = GameObject.FindObjectOfType<Player> ();
		serviceInstance = GameSystemService.Instance;
		isLoaded = LoadQuestInfo ();
		if (isLoaded) {
			serviceInstance = GameSystemService.Instance;
			foreach (var questInfo in allQuestInfo.huntingQuestList) {
				questInfoDic[questInfo.questName] = questInfo;
				AddQuestAction (questInfo);
			}
			foreach(var questInfo in allQuestInfo.eliminationQuestList)
			{
				questInfoDic[questInfo.questName] = questInfo;
				AddQuestAction (questInfo);
			}
			foreach(var questInfo in allQuestInfo.skillTrainingQuestList)
			{
				questInfoDic[questInfo.questName] = questInfo;
				AddQuestAction (questInfo);
			}
			foreach(var questInfo in allQuestInfo.collectionQuestList)
			{
				questInfoDic[questInfo.questName] = questInfo;
				AddQuestAction (questInfo);
			}
			foreach(var questInfo in allQuestInfo.interactionQuestList)
			{
				questInfoDic[questInfo.questName] = questInfo;
				AddQuestAction (questInfo);
			}
			foreach(var questInfo in allQuestInfo.moveQuestList)
			{
				questInfoDic[questInfo.questName] = questInfo;
				AddQuestAction (questInfo);
			}
		} else {
			Debug.LogError ("Failed Load Quest Info");
		}
	}

	public bool StartQuest (string questName)
	{
		if (!questActionDic.ContainsKey (questName))
			return false;
		questActionDic [questName].Invoke(questInfoDic[questName]);
		return true;
	}
	public void AddQuestAction (QuestInfo questInfo)
	{
		switch (questInfo.questType) {
		case QuestType.HUNTING:
			questActionDic[questInfo.questName] = AddHuntingQuest;
			break;
		case QuestType.ELIMINATION:
			questActionDic[questInfo.questName] = AddEliminationQuest;
			break;
		case QuestType.SKILL_TRAINING:
			questActionDic[questInfo.questName] = AddSkillTrainingQuest;
			break;
		case QuestType.COLLECTION:
			questActionDic[questInfo.questName] = AddCollectionQuest;
			break;
		case QuestType.INTERACTION:
			questActionDic[questInfo.questName] = AddInteractionQuest;
			break;
		case QuestType.MOVE:
			questActionDic[questInfo.questName] = AddMoveQuest;
			break;
		}
	}

	private void AddHuntingQuest(QuestInfo questInfo)
	{
		var huntingQuest = questInfo as HuntingQuestInfo;
		serviceInstance.Add_ActorKill_Subscriber (player, huntingQuest.ProgressQuest);
	}
	private void AddEliminationQuest(QuestInfo questInfo)
	{
		var eliminationQuest = questInfo as EliminateQuestInfo;
		var room = Room.RoomDictionary [eliminationQuest.GetTargetRoomName()];
		if (null == room)
			return;
		serviceInstance.Add_RoomCleared_Subscriber (
			room,
			eliminationQuest.ProgressQuest);
	}
	private void AddSkillTrainingQuest(QuestInfo questInfo)
	{
		var trainingQuest = questInfo as SkillTrainingQuestInfo;
		serviceInstance.Add_ActorAttackSuccess_Subscriber (player, trainingQuest.ProgressQuest);
	}
	private void AddCollectionQuest(QuestInfo questInfo)
	{
		var collectionQuest = questInfo as CollectionQuestInfo;
		serviceInstance.Add_ActorRoot_Subscriber (player, collectionQuest.ProgressQuest);
	}
	private void AddInteractionQuest(QuestInfo questInfo)
	{
		var interactionQuest = questInfo as InteractionQuestInfo;
		serviceInstance.Add_ActorInteract_Subscriber (player, interactionQuest.ProgressQuest);
	}
	private void AddMoveQuest(QuestInfo questInfo)
	{
		var moveQuest = questInfo as MoveQuestInfo;
		serviceInstance.Add_ActorInteract_Subscriber (player, moveQuest.ProgressQuest);
	}

	public QuestInfo GetQuestInfoByName (string questName)
	{
		return questInfoDic [questName];
	}

	public void SaveQuestInfo()
	{
		File.WriteAllText (PathPool.GetQuestPath(serviceInstance.GetNowGameDataInfo()), JsonUtility.ToJson (allQuestInfo));
		//File.WriteAllText (questPath, JsonUtility.ToJson (allQuestInfo));
	}
	public void SaveQuestInfo(string path)
	{
		path += "_QuestList";
		File.WriteAllText (path, JsonUtility.ToJson (allQuestInfo));
	}


	//	out 명시를 통해, 기존 Dictionary 데이터는 초기화 필요없음을 명시
	private bool LoadQuestInfo ()
	{
		var tmpQuestPath = PathPool.GetQuestPath(serviceInstance.GetNowGameDataInfo());
		if (File.Exists(tmpQuestPath))
		{
			var data = File.ReadAllText (tmpQuestPath);
			allQuestInfo = JsonUtility.FromJson<AllQuestInfo> (data);

			return true;
		}
		var tmpDefaultQuestDir = PathPool.GetDefaultQuestDirectoryPath ();
		tmpQuestPath = tmpDefaultQuestDir + "/" + SceneNamePool.sceneName01;
		//	Try Load Default Quest Info
		if (File.Exists (tmpQuestPath)) {
			var defaultData = File.ReadAllText (tmpQuestPath);
			allQuestInfo = JsonUtility.FromJson<AllQuestInfo> (defaultData);
			return true;
		}
		//	Write Default Quest Info
		DefaultQuestInfoList (SceneNamePool.sceneName01);
		if (!Directory.Exists (tmpDefaultQuestDir)) {
			Directory.CreateDirectory (tmpDefaultQuestDir);
			File.WriteAllText (tmpQuestPath, JsonUtility.ToJson (allQuestInfo));
			return true;
		}
		if (!File.Exists (tmpQuestPath)) {
			File.WriteAllText (tmpQuestPath, JsonUtility.ToJson (allQuestInfo));
			return true;
		}
		return false;
	}
	private void DefaultQuestInfoList(string stageName)
	{
		switch (stageName) {
		case SceneNamePool.sceneName01:
			DefaultStage01Quest ();
			break;
		case "Stage02":

			break;
		default:
			Debug.LogError ("Wrong Stage Name");
			break;
		}
	}

	private void DefaultStage01Quest ()
	{
		List<QuestRewardInfo> reward_Z01_01 = new List<QuestRewardInfo> ();
		reward_Z01_01.Add (new QuestRewardInfo (10,0,"",0));
		InteractionQuestInfo quest_Z01_01_01 = new InteractionQuestInfo ("quest_Z01_01_01", "NPC_A", 1, null);
		InteractionQuestInfo quest_Z01_01_02 = new InteractionQuestInfo ("quest_Z01_01_02", "NPC_B", 1, null);
		InteractionQuestInfo quest_Z01_01_03 = new InteractionQuestInfo ("quest_Z01_01_03", "NPC_C", 1, reward_Z01_01);
		allQuestInfo.AddQuest<InteractionQuestInfo> (quest_Z01_01_01);
		allQuestInfo.AddQuest<InteractionQuestInfo> (quest_Z01_01_02);
		allQuestInfo.AddQuest<InteractionQuestInfo> (quest_Z01_01_03);

		List<QuestRewardInfo> reward_Z01_02 = new List<QuestRewardInfo> ();
		reward_Z01_02.Add (new QuestRewardInfo (10, 0, "", 0));
		HuntingQuestInfo quest_Z01_02_01 = new HuntingQuestInfo ("quest_Z01_02_01", "Small_Gate01", 1, null);
		MoveQuestInfo quest_Z01_02_02 = new MoveQuestInfo ("quest_Z01_02_02", "Warp_환풍구", reward_Z01_02);
		allQuestInfo.AddQuest<HuntingQuestInfo> (quest_Z01_02_01);
		allQuestInfo.AddQuest<MoveQuestInfo> (quest_Z01_02_02);

		List<QuestRewardInfo> reward_Z01_03 = new List<QuestRewardInfo> ();
		reward_Z01_03.Add (new QuestRewardInfo(10, 0, "", 0));
		InteractionQuestInfo quest_Z01_03 = new InteractionQuestInfo ("quest_Z01_03", "Bed", 1,reward_Z01_03);
		allQuestInfo.AddQuest<InteractionQuestInfo> (quest_Z01_03);

		List<QuestRewardInfo> reward_Z01_04 = new List<QuestRewardInfo> ();
		reward_Z01_04.Add (new QuestRewardInfo (10, 0, "", 0));
		HuntingQuestInfo quest_Z01_04_01 = new HuntingQuestInfo ("quest_Z01_04_01", "Door01", 1,null);
		MoveQuestInfo quest_Z01_04_02 = new MoveQuestInfo ("quest_Z01_04_02", "Warp_격리구역_복도",reward_Z01_04);
		allQuestInfo.AddQuest<HuntingQuestInfo>(quest_Z01_04_01);
		allQuestInfo.AddQuest<MoveQuestInfo>(quest_Z01_04_02);
	}		

	public void Update()
	{
		if (!isLoaded)
			return;
		if (Input.GetKeyDown (KeyCode.Q))
			SaveQuestInfo ();
	}
}
