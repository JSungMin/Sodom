using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using InformationNamespace;

public class SkillFactory : MonoBehaviour {
	public SkillInfoList savedPlayerSkillInfoList;
	public SkillInfoList savedEnemySkillInfoList;

	public void Init()
	{
		savedPlayerSkillInfoList.BuildDictionaries ();
		savedEnemySkillInfoList.BuildDictionaries ();
		SkillContentPool.InitializeContentMap ();
	}
	public void SyncLearnedSkill (ActorInfo enemy)
	{
		foreach (var skill in savedEnemySkillInfoList.allPlayerSkillInfo.groundSkillList)
		{
			if (!enemy.learnedSkillNameList.Contains(skill.skillName))
				enemy.learnedSkillNameList.Add (skill.skillName);
		}
		enemy.SyncSkillMapWithList ();
	}

	public SkillInfo GetPlayerSkillInfoByName (string skillName)
	{
		if (!savedPlayerSkillInfoList.skillInfoDic.ContainsKey (skillName)) {
			Debug.LogError ("Not Contain in SkillInfoList : " + skillName);
			return null;
		}
		return savedPlayerSkillInfoList.skillInfoDic [skillName];
	}
	public SkillInfo GetEnemySkillInfoByName (string skillName)
	{
		if (!savedEnemySkillInfoList.skillInfoDic.ContainsKey (skillName)) {
			Debug.LogError ("Not Contain in SkillInfoList : " + skillName);
			return null;
		}
		return savedEnemySkillInfoList.skillInfoDic [skillName];
	}

	public bool CheckSkillCounterValidation(SkillCounterType victim, SkillCounterType attacker)
	{
		var tmpKeyPair = new KeyValuePair<SkillCounterType, SkillCounterType> (attacker, victim);
		return savedPlayerSkillInfoList.skillCounterGraph.ContainsKey(tmpKeyPair);
	}
}
