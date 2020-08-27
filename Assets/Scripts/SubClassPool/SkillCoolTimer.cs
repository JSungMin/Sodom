using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class SkillCoolTimer {
	public Actor targetActor;
	public List<SkillCoolInfo> skillCoolList = new List<SkillCoolInfo>();
	public Dictionary<string, SkillCoolInfo> skillCoolDic = new Dictionary<string, SkillCoolInfo>();

	public void Initialize(Actor actor)
	{
		targetActor = actor;
		var actorInfo = targetActor.actorInfo;
		for (int i = 0; i < actorInfo.learnedSkillNameList.Count; i++)
		{
			var skillName = actorInfo.learnedSkillNameList [i];
			var skillCoolInfo = new SkillCoolInfo(actor, actorInfo.GetLearnedSkill (skillName));
			skillCoolDic [skillName] = skillCoolInfo;
			skillCoolList.Add (skillCoolInfo);
		}
	}
	//	보스몹들이 주로 사용합니다.
	//	몬스터들이 스킬을 사용하기전 선딜레이를 줍니다.
	public void Warmup(int index)
	{
		if (!skillCoolList [index].IsUseable ()) 
		{
			skillCoolList [index].OnWarmup ();
		}
	}
	public void Warmup(string skillName)
	{
		if (!skillCoolDic [skillName].IsUseable ()) 
		{
			skillCoolDic [skillName].OnWarmup ();
		}
	}
	public bool IsWarmedUp (int index)
	{
		if (skillCoolList [index].warmupTimer >= skillCoolList [index].skillInfo.warmupTime)
			return true;
		return false;
	}
	public bool IsWarmedUp (string skillName)
	{
		if (skillCoolDic [skillName].warmupTimer >= skillCoolDic [skillName].skillInfo.warmupTime)
			return true;
		return false;
	}
	public bool IsCooled (int index)
	{
		if (skillCoolList [index].coolTimer >= skillCoolList [index].skillInfo.cooldownTime)
			return true;
		return false;
	}
	public bool IsCooled (string skillName)
	{
		if (skillCoolDic [skillName].coolTimer >= skillCoolDic [skillName].skillInfo.cooldownTime)
			return true;
		return false;
	}
	public void Cooling(int index)
	{
		if (!skillCoolList [index].IsUseable ()) 
		{
			skillCoolList [index].OnCooling ();
		}
	}
	public void Cooling(string skillName)
	{
		if (!skillCoolDic [skillName].IsUseable ()) 
		{
			skillCoolDic [skillName].OnCooling ();
		}
	}
	public void CoolingAll ()
	{
		for (int i = 0; i < skillCoolList.Count; i++)
		{
			if (!skillCoolList [i].IsUseable ()) {
				skillCoolList [i].OnCooling ();
			}
		}
	}
	public void ResetSkill (int index)
	{
		skillCoolList [index].OnReset ();
	}
	public void ResetSkill (string skillName)
	{
		skillCoolDic [skillName].OnReset ();
	}
	public void ResetSkill ()
	{
		for (int i = 0; i < skillCoolList.Count; i++)
		{
			skillCoolList [i].OnReset ();
		}
	}
	public bool IsUseable (int index)
	{
		return skillCoolList[index].IsUseable();
	}
	public bool IsUseable (string skillName)
	{
		return skillCoolDic [skillName].IsUseable ();
	}
	public void MakeFullCharge (int index)
	{
		skillCoolList [index].coolTimer = skillCoolList [index].skillInfo.cooldownTime;
		skillCoolList [index].warmupTimer = skillCoolList [index].skillInfo.warmupTime;
	}
	public void MakeFullCharge (string skillName)
	{
		skillCoolDic [skillName].coolTimer = skillCoolDic [skillName].skillInfo.cooldownTime;
		skillCoolDic [skillName].warmupTimer = skillCoolDic [skillName].skillInfo.warmupTime;
	}
	public int SkillCount ()
	{
		return skillCoolList.Count;
	}
	public SkillInfo GetSkill (int index)
	{
		return skillCoolList [index].skillInfo;
	}
	public SkillInfo GetSkill (string skillName)
	{
		if (skillCoolDic.ContainsKey(skillName))
			return skillCoolDic [skillName].skillInfo;
		Debug.LogError("INVAILD ACCESS : " + skillName);
		return null;
	}
}
