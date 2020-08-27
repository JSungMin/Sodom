using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SkillInfoListEditor : EditorWindow {
	public SkillInfoList skillInfoList;
	private int viewIndex = 1;
    public bool isSelected = false;
    public string copyField;

	[MenuItem ("Window/Skill Info Editor %&s")]
	static void Init()
	{
		EditorWindow.GetWindow (typeof(SkillInfoListEditor));
	}

	void OnEnable()
	{
		if (EditorPrefs.HasKey("ObjectPath"))
		{
			string objectPath = EditorPrefs.GetString ("ObjectPath");
			skillInfoList = AssetDatabase.LoadAssetAtPath (objectPath, typeof(SkillInfoList)) as SkillInfoList;
		}
	}

	void OnGUI()
	{
		GUILayout.BeginVertical ();
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Skill Information Editor", EditorStyles.boldLabel);
		if (null != skillInfoList)
		{
            isSelected = true;
			if (GUILayout.Button ("Show Skill List"))
			{
				EditorUtility.FocusProjectWindow ();
				Selection.activeObject = skillInfoList;
			}
		} 
		else
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Space (10f);
			if (GUILayout.Button("Create New Skill", GUILayout.ExpandWidth(false)))
			{
				CreateSkillList ();
			}
			if (GUILayout.Button ("Open Existing Skill List", GUILayout.ExpandWidth(false))) 
			{
				OpenSkillList ();
			}
			GUILayout.EndHorizontal ();
		}
		if (GUILayout.Button ("Open Skill List"))
		{
			OpenSkillList ();
		}
		if (GUILayout.Button ("New Skill List"))
		{
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = skillInfoList;
		}
		GUILayout.EndHorizontal ();
        if (isSelected)
        {
            GUILayout.BeginHorizontal();
            copyField = EditorGUILayout.TextField(copyField);
            if (GUILayout.Button("Copy Fom Ground") && copyField != "")
            {
                skillInfoList.CopySkillInfoFromGround(copyField);
            }
            if (GUILayout.Button("Copy Fom Air") && copyField != "")
            {
                skillInfoList.CopySkillInfoFromAir(copyField);
            }
            GUILayout.EndHorizontal();
        }
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Skill Tree Operation", EditorStyles.boldLabel);
		if (GUILayout.Button ("Construct Skill Tree"))
		{
			skillInfoList.BuildSkillTree ();
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
	}

	void CreateSkillList()
	{
		viewIndex = 1;
		skillInfoList = SkillInfoList.CreateSkillInfoList ();
		if (skillInfoList)
		{
			string relPath = AssetDatabase.GetAssetPath (skillInfoList);
			EditorPrefs.SetString ("ObjectPath", relPath);
		}
	}

	void OpenSkillList()
	{
		string absPath = EditorUtility.OpenFilePanel ("Select Skill Information List", "", "");
		if (absPath.StartsWith (Application.dataPath))
		{
			string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
			skillInfoList = AssetDatabase.LoadAssetAtPath (relPath, typeof(SkillInfoList)) as SkillInfoList;
			if (skillInfoList) {
				EditorPrefs.SetString("ObjectPath", relPath);
			}
		}
	}
}
