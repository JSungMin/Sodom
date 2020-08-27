using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PatternData", menuName = "Character/Boss/Pattern List", order = 2)]
public class EnemyPatternInfoData : ScriptableObject {
	public bool isBuilded = false;
	public List<EnemyAttackPatternInfo> patternList = new List<EnemyAttackPatternInfo>();
	public Dictionary<string, EnemyAttackPatternInfo> patternDic = new Dictionary<string, EnemyAttackPatternInfo>();

	private Actor actor;

	#if UNITY_EDITOR
	public static EnemyPatternInfoData CreatePatternInfoList()
	{
		EnemyPatternInfoData asset = ScriptableObject.CreateInstance<EnemyPatternInfoData> ();
		AssetDatabase.CreateAsset (asset, "Assets/SaveDatas/Default/BossPattern.asset");
		AssetDatabase.SaveAssets ();
		return asset;
	}
	#endif
	//	TODO : Start나 Awake에서 호출되야 합니다.
	public void BuildPattern (Actor targetActor)
	{
		actor = targetActor;
		for (int i = 0; i < patternList.Count; i++)
		{
			patternList [i].Init (targetActor);
			patternDic [patternList [i].patternName] = patternList [i];
		}
		isBuilded = true;
	}
	public void WarmupPattern (string patternName)
	{
		if (!patternDic.ContainsKey (patternName))
			Debug.LogError ("Enemey not contain pattern : " + patternName);
		patternDic [patternName].WarmupPattern ();
	}
	public void CoolingPattern (string patternName)
	{
		patternDic [patternName].CoolingPattern ();
	}
	public bool CheckUseable (string patternName, int patternIndex)
	{
		return patternDic [patternName].CheckUseable (patternIndex);
	}
	public bool UseSkill (string patternName, ref int patternIndex)
	{
		return patternDic [patternName].UseSkill (ref patternIndex);
	}
	public bool UseSkill (int index, ref int patternIndex)
	{
		return patternList [index].UseSkill (ref patternIndex);
	}
}
