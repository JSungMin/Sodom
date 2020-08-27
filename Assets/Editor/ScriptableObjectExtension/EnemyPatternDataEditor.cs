using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemyPatternDataEditor : EditorWindow {
	public EnemyPatternInfoData patternInfoData;
	private int viewIndex = 1;

	[MenuItem ("Window/Enemy Pattern Data Editor %&p")]
	static void Init()
	{
		EditorWindow.GetWindow (typeof(EnemyPatternDataEditor));
	}

	void OnEnable()
	{
		if (EditorPrefs.HasKey("ObjectPath"))
		{
			string objectPath = EditorPrefs.GetString ("ObjectPath");
			patternInfoData = AssetDatabase.LoadAssetAtPath (objectPath, typeof(EnemyPatternInfoData)) as EnemyPatternInfoData;
		}
	}

	void OnGUI()
	{
		GUILayout.BeginVertical ();
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Pattern Information Editor", EditorStyles.boldLabel);
		if (null != patternInfoData)
		{
			if (GUILayout.Button ("Show Pattern List"))
			{
				EditorUtility.FocusProjectWindow ();
				Selection.activeObject = patternInfoData;
			}
		} 
		else
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Space (10f);
			if (GUILayout.Button("Create New Pattern", GUILayout.ExpandWidth(false)))
			{
				CreatePatternData ();
			}
			if (GUILayout.Button ("Open Existing Pattern Data", GUILayout.ExpandWidth(false))) 
			{
				OpenPatternData ();
			}
			GUILayout.EndHorizontal ();
		}
		if (GUILayout.Button ("Open Pattern List"))
		{
			OpenPatternData ();
		}
		if (GUILayout.Button ("New Pattern List"))
		{
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = patternInfoData;
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
	}

	void CreatePatternData()
	{
		viewIndex = 1;
		patternInfoData = EnemyPatternInfoData.CreatePatternInfoList ();
		if (patternInfoData)
		{
			string relPath = AssetDatabase.GetAssetPath (patternInfoData);
			EditorPrefs.SetString ("ObjectPath", relPath);
		}
	}

	void OpenPatternData()
	{
		string absPath = EditorUtility.OpenFilePanel ("Select Pattern Data", "", "");
		if (absPath.StartsWith (Application.dataPath))
		{
			string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
			patternInfoData = AssetDatabase.LoadAssetAtPath (relPath, typeof(EnemyPatternInfoData)) as EnemyPatternInfoData;
			if (patternInfoData) {
				EditorPrefs.SetString("ObjectPath", relPath);
			}
		}
	}
}
