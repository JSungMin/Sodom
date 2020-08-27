using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using InformationNamespace;

[Serializable]
public class CommandNode
{
	public KeyCode element;
	public CommandNode parent;
	public List<CommandNode> childs;
	public SkillInfo useableSkill;
	public int depth;

	public CommandNode ()
	{
		childs = new List<CommandNode> ();
	}
	public CommandNode (KeyCode e)
	{
		element = e;
		childs = new List<CommandNode> ();
	}
	public CommandNode (KeyCode e, SkillInfo skill)
	{
		element = e;
		useableSkill = skill;
		childs = new List<CommandNode> ();
	}
	public void AddChild (CommandNode child)
	{
		child.depth = depth + 1;
		child.parent = this;
		childs.Add (child);
	}
	public void AddChild (CommandNode parent, CommandNode child)
	{
		child.depth = parent.depth + 1;
		child.parent = parent;
		parent.childs.Add (child);
	}
	public void RemoveChild (CommandNode child)
	{
		childs.AddRange (child.childs);
		foreach (var cc in child.childs) {
			cc.parent = this;
		}
		childs.Remove (child);
	}
	public bool MakeSkillTree (SkillInfo skill, CommandNode head, List<KeyCode> command, int depth, int targetDepth)
	{
		Stack<CommandNode> stack = new Stack<CommandNode> ();
		foreach (var child in head.childs)
		{
			stack.Push (child);
		}
		while (stack.Count != 0)
		{
			var pop = stack.Pop ();
			//	string context = "";
			//	for (int i = 0; i < depth; i++)
			//	{
			//		context += "\t";
			//	}
			//	Debug.Log (context + pop.element);
			if (pop.element == command [depth]) {
				if (depth == targetDepth) {
					pop.useableSkill = skill;
					return true;
				}
				return MakeSkillTree (skill, pop, command, ++depth, targetDepth);
			}
		}
		if (depth == targetDepth) {
			head.useableSkill = skill;
			head.useableSkill.lastNode = false;
			head.useableSkill.entityType = skill.entityType;
			return true;
		}
		head.AddChild (new CommandNode (command [depth]));
		return MakeSkillTree (skill, head, command, depth, targetDepth);
	}
	public void NodeLeafCheck (CommandNode head)
	{
		Stack<CommandNode> stack = new Stack<CommandNode> ();
		foreach (var child in head.childs) {
			stack.Push (child);
		}
		while (stack.Count != 0) {
			var pop = stack.Pop ();
			NodeLeafCheck (pop);
		}
		if (head.childs.Count == 0)
		{
			if (null == head.useableSkill) 
			{
				return;
			}
			if (head.useableSkill.lastNode == false)
				head.useableSkill.lastNode = true;
		}
	}
}

[CreateAssetMenu(fileName = "SkillData", menuName = "Character/Skill/List", order = 1)]
public class SkillInfoList : ScriptableObject {
	public bool isBuilded = false;
	public AllSkillInfo allPlayerSkillInfo = new AllSkillInfo();
	public CommandNode gSkillTreeHead = new CommandNode ();
	public CommandNode aSkillTreeHead = new CommandNode();
	public Dictionary<string,SkillInfo> skillInfoDic = new Dictionary<string, SkillInfo>();
	public Dictionary<KeyValuePair<SkillCounterType, SkillCounterType>,bool> skillCounterGraph = new Dictionary<KeyValuePair<SkillCounterType, SkillCounterType>, bool>();


	#if UNITY_EDITOR
	public static SkillInfoList CreateSkillInfoList()
	{
		SkillInfoList asset = ScriptableObject.CreateInstance<SkillInfoList> ();
		AssetDatabase.CreateAsset (asset, "Assets/SaveDatas/Default/SkillInfo/SkillData.asset");
		AssetDatabase.SaveAssets ();
		return asset;
	}
	#endif
    public void CopySkillInfoFromGround(string skillName)
    {
        SkillInfo copiedInfo = allPlayerSkillInfo.groundSkillList.Find(
            (SkillInfo info) => {
                if (info.skillName == skillName)
                    return true;
                return false;
            }
        );
        if (copiedInfo != null)
        {
            copiedInfo.skillName += "_Copied";
            allPlayerSkillInfo.groundSkillList.Add(copiedInfo);
        }
    }
    public void CopySkillInfoFromAir(string skillName)
    {
        SkillInfo copiedInfo = allPlayerSkillInfo.airSkilllist.Find(
            (SkillInfo info) => {
                if (info.skillName == skillName)
                    return true;
                return false;
            }
        );
        if (copiedInfo != null)
        {
            copiedInfo.skillName += "_Copied";
            allPlayerSkillInfo.airSkilllist.Add(copiedInfo);
        }
    }
	private void AddSkillCounterGraphNode (SkillCounterType victim, SkillCounterType attacker)
	{
		var tmpKeyPair = new KeyValuePair<SkillCounterType, SkillCounterType> (attacker, victim);
		skillCounterGraph [tmpKeyPair] = true;
	}
	//	Called From Editor
	public void BuildSkillTree ()
	{
		isBuilded = true;
		gSkillTreeHead = new CommandNode ();
		aSkillTreeHead = new CommandNode ();
		//	Set Fisrt Childs
		foreach (var skill in allPlayerSkillInfo.groundSkillList)
		{
			if (null == gSkillTreeHead.childs.Find (delegate(CommandNode obj) {
				return skill.skillCommand [0] == obj.element;
			}))
			{
				gSkillTreeHead.AddChild (new CommandNode (skill.skillCommand[0]));
			}
		}
		foreach (var skill in allPlayerSkillInfo.airSkilllist)
		{
			if (null == aSkillTreeHead.childs.Find (delegate(CommandNode obj) {
				return skill.skillCommand [0] == obj.element;
			}))
			{
				aSkillTreeHead.AddChild (new CommandNode (skill.skillCommand[0]));
			}
		}
		//	Make Skill Tree using skill tree head
		foreach (var skill in allPlayerSkillInfo.groundSkillList) {
			gSkillTreeHead.MakeSkillTree (skill, gSkillTreeHead, skill.skillCommand, 0, skill.skillCommand.Count);
		}
		foreach (var skill in allPlayerSkillInfo.airSkilllist) {
			aSkillTreeHead.MakeSkillTree (skill, aSkillTreeHead, skill.skillCommand, 0, skill.skillCommand.Count);
		}
		gSkillTreeHead.NodeLeafCheck (gSkillTreeHead);
		aSkillTreeHead.NodeLeafCheck (aSkillTreeHead);
	}

	// Called From SkillFactory
	public void BuildDictionaries()
	{
		foreach (var skill in allPlayerSkillInfo.groundSkillList) {
			if (skillInfoDic.ContainsKey (skill.skillName))
				return;
			skill.entityType = SkillEntityType.GROUND;
			skillInfoDic.Add (skill.skillName, skill);
		}
		foreach (var skill in allPlayerSkillInfo.airSkilllist) {
			if (skillInfoDic.ContainsKey (skill.skillName))
				return;
			skill.entityType = SkillEntityType.AIR;
			skillInfoDic.Add (skill.skillName, skill);
		}
		foreach (var skill in allPlayerSkillInfo.anyTimeSkillList) {
			if (skillInfoDic.ContainsKey (skill.skillName))
				return;
			skillInfoDic.Add (skill.skillName, skill);
		}
		AddSkillCounterGraphNode (SkillCounterType.HIGH, SkillCounterType.HIGH);
		AddSkillCounterGraphNode (SkillCounterType.MIDDLE, SkillCounterType.MIDDLE);
		AddSkillCounterGraphNode (SkillCounterType.LOW, SkillCounterType.LOW);
	}
}
