using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public class PathPool {
	public static Dictionary<string, string> timelineDirPathMap = new Dictionary<string, string>()
	{
		{"Tutorial", "Timeline/Tutorial"},
		{"Stage01", "Timeline/Stage01"},
		{"Stage02", "Timeline/Stage02"},
		{"Stage03", "Timeline/Stage03"}
	};
	public static Dictionary<string, string> npcConversationPathMap = new Dictionary<string, string>()
	{
		{"Stage01_NPC01", Application.dataPath + "/Resources/Conversation/Script/Stage01_NPC01"},
		{"Stage01_NPC02", Application.dataPath + "/Resources/Conversation/Script/Stage01_NPC02"},
		{"Stage01_NPC03", Application.dataPath + "/Resources/Conversation/Script/Stage01_NPC03"}
	};
	public static Dictionary<string, string> conversationPathMap = new Dictionary<string, string>()
	{
		{"Tutorial_01", Application.dataPath +"/Resources/Conversation/Script/Tutorial_01"},
		{"Tutorial_02", Application.dataPath +"/Resources/Conversation/Script/Tutorial_02"},
		{"Tutorial_03", Application.dataPath +"/Resources/Conversation/Script/Tutorial_03"}
	};

	public static string GetTimelineDirectoryPath (string stageName)
	{
		if (timelineDirPathMap.ContainsKey (stageName))
			return timelineDirPathMap [stageName];
		return null;
	}

	public static string GetConversationFilePath (string convName)
	{
		string result;
		if (conversationPathMap.TryGetValue (convName, out result))
			return result;
		return null;
	}

	public static string GetNPCConversationFilePath (string npcName)
	{
		string result;
		if (npcConversationPathMap.TryGetValue (npcName, out result))
			return result;
		return null;
	}

	public static string GetQuestPath (GameDataInfo dataInfo)
	{
		return Application.dataPath + "/SaveDatas/" + dataInfo.path + "/" + dataInfo.path + "_QuestList";
	}
	public static string GetDefaultQuestPath (string stageName)
	{
		return Application.dataPath + "/SaveDatas/Default/QuestInfo/" + stageName;
	}
	public static string GetDefaultQuestDirectoryPath ()
	{
		return Application.dataPath + "/SaveDatas/Default/QuestInfo";
	}
}
