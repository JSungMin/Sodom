using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InformationNamespace
{
	/*	Parent Property
		public string name;					//	Quest의 이름이다.
		public string preQuest="";			//	먼저 클리어 되어있어야 하는 퀘스트의 이름이다. ""는 조건이 없는 거다.
		public QuestType questType;			//	Derived Class를 구분하기 위한 Type 명시다.
		public bool isCleared;				//	퀘스트의 클리어 여부를 나타낸다.
	*/
	[Serializable]
	public class HuntingQuestInfo : QuestInfo
	{
		[SerializeField]
		protected string targetName;
		[SerializeField]
		protected int targetCount;
		[SerializeField]
		protected int nowCount;

		public HuntingQuestInfo (
			string 					questName, 
			string 					tName,
			int 					tCount, 
			List<QuestRewardInfo> 	rewardInfo)
		{
			this.questName = questName;
			this.rewardInfoList = rewardInfo;
			targetName = tName;
			targetCount = tCount;
		}

		public string GetTargetName ()
		{
			return targetName;
		}
		public void SetTargetCount (int count)
		{
			targetCount = count;
		}
		public int GetTargetCount()
		{
			return targetCount;
		}
		public bool CheckSatisfy (Actor deadActor)
		{
			if (deadActor.actorInfo.actor_name == targetName)
			{
				nowCount++;
				return true;
			}
			return false;
		}
		public bool CheckClear()
		{
			if (nowCount >= targetCount)
			{
				isCleared = true;
				return isCleared;
			}
			return false;
		}
		public void ProgressQuest(object sender, EventArgumentNamespace.ActorDamagedEventArg  arg)
		{
			var result = CheckSatisfy (arg.damageInfo.victim);
			if (result)
				result = CheckClear ();
			if (result)
				GiveAward ();
		}
		public void GiveAward()
		{
			Debug.Log ("Clear Quest : " + questName);
		}
	}
	[Serializable]
	public class EliminateQuestInfo : QuestInfo
	{
		protected string roomName;		//	제거해야 할 몬스터의 room 위치를 기반으로 클리어 여부를 매김
		protected int targetCount;
		protected int nowCount;

		public EliminateQuestInfo (
			string 					questName, 
			RoomInfo 				roomInfo,
			List<QuestRewardInfo> 	rewardInfo)
		{
			this.questName = questName;
			this.rewardInfoList = rewardInfo;
			roomName = roomInfo.roomName;
			targetCount = roomInfo.enemyInfoList.Count;
			nowCount = roomInfo.deadEnemyCount;
		}
		public string GetTargetRoomName ()
		{
			return roomName;
		}
		public void SetTargetCount (int count)
		{
			targetCount = count;
		}
		public int GetTargetCount()
		{
			return targetCount;
		}
		public bool CheckClear(Room room)
		{
			if (room.roomInfo.roomName == roomName)
				return true;
			return false;
		}
		public void ProgressQuest(object sender, EventArgumentNamespace.RoomClearedEventArg arg)
		{
			var result = CheckClear (arg.room);
			if (result)
				GiveAward ();
		}
		public void GiveAward()
		{

		}
	}
	[Serializable]
	public class SkillTrainingQuestInfo : QuestInfo
	{
		protected string skillName;		//	제거해야 할 몬스터의 room 위치를 기반으로 클리어 여부를 매김
		protected int targetCount;
		protected int nowCount;

		public SkillTrainingQuestInfo (
			string 					questName, 
			string 					sName, 
			int 					tCount,
			List<QuestRewardInfo>	rewardInfo)
		{
			this.questName = questName;
			this.rewardInfoList = rewardInfo;
			skillName = sName;
			targetCount = tCount;
		}

		public void SetTargetCount (int count)
		{
			targetCount = count;
		}
		public int GetTargetCount()
		{
			return targetCount;
		}
		public bool CheckSatisfy (SkillInfo skillInfo)
		{
			if (skillInfo.skillName == skillName) {
				nowCount++;
				return true;
			}
			return false;
		}

		public bool CheckClear()
		{
			if (nowCount >= targetCount)
			{
				isCleared = true;
				return isCleared;
			}
			return false;
		}
		public void ProgressQuest(object sender, EventArgumentNamespace.ActorDamagedEventArg  arg)
		{
			var result = CheckSatisfy (arg.damageInfo.skillInfo);
			if (result)
				result = CheckClear ();
			if (result)
				GiveAward ();
		}
		public void GiveAward()
		{

		}
	}
	[Serializable]
	public class CollectionQuestInfo : QuestInfo
	{
		public string itemName;
		protected int targetCount;
		protected int nowCount;

		public CollectionQuestInfo (
			string 					questName,
			string 					iName, 
			int 					tCount, 
			Player 					actor,
			List<QuestRewardInfo>	rewardInfo)
		{
			this.questName = questName;
			this.rewardInfoList = rewardInfo;
			itemName = iName;
			targetCount = tCount;
			nowCount = actor.invenInfo.GetItemAmount (iName);
		}

		public void SetTargetCount (int count)
		{
			targetCount = count;
		}
		public int GetTargetCount()
		{
			return targetCount;
		}
		public bool CheckClear(Actor actor)
		{
			if (actor.actorInfo.actorType != ActorType.PLAYER)
				return false;
			var player = actor as Player;
			nowCount = player.invenInfo.GetItemAmount (itemName);
			if (nowCount >= targetCount)
			{
				isCleared = true;
				return isCleared;
			}
			return false;
		}
		public void ProgressQuest(object sender, EventArgumentNamespace.ActorRootEventArg  arg)
		{
			if (arg.rootItem.itemName != itemName)
				return;
			var result = CheckClear (arg.actor);
			if (result)
				GiveAward ();
		}
		public void GiveAward()
		{

		}
	}
	[Serializable]
	public class InteractionQuestInfo : QuestInfo
	{
		//	상호작용해야하는 NPC의 이름
		public string targetName;
		//	목표 상호작용 횟수
		[SerializeField]
		protected int targetCount;
		[SerializeField]
		protected int nowCount;

		public InteractionQuestInfo (
			string 					questName,
			string					interactedActorName,
			int 					tCount,
			List<QuestRewardInfo>	rewardInfo
		)
		{
			this.questName = questName;
			this.rewardInfoList = rewardInfo;
			targetName = interactedActorName;
			targetCount = tCount;
		}

		public void SetTargetCount (int count)
		{
			targetCount = count;
		}
		public int GetTargetCount()
		{
			return targetCount;
		}
		public bool CheckSatisfy (Actor interactedActor)
		{
			if (interactedActor.actorInfo.actor_name == targetName) {
				nowCount++;
				return true;
			}
			return false;
		}
		public bool CheckClear()
		{
			if (nowCount >= targetCount)
			{
				isCleared = true;
				return isCleared;
			}
			return false;
		}
		public void ProgressQuest(object sender, EventArgumentNamespace.ActorInteractEventArg  arg)
		{
			var result = CheckSatisfy (arg.interactWith);
			if (result)
				result = CheckClear ();
			if (result)
				GiveAward ();
		}
		public void GiveAward()
		{

		}
	}
	[Serializable]
	public class MoveQuestInfo : QuestInfo
	{
		//	Warp Gate 이름
		public string targetName;

		public MoveQuestInfo (
			string 					questName,
			string 					tName,
			List<QuestRewardInfo>	rewardInfo)
		{
			this.questName = questName;
			this.rewardInfoList = rewardInfo;
			targetName = tName;
		}
			
		public bool CheckClear(Actor actor)
		{
			if (actor.actorInfo.actorType != ActorType.OBJECT)
				return false;
			return false;
		}
		public void ProgressQuest(object sender, EventArgumentNamespace.ActorInteractEventArg  arg)
		{
			var result = CheckClear (arg.interactWith);
			if (result)
				GiveAward ();
		}
		public void GiveAward()
		{

		}
	}
}