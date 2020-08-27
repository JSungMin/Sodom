using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AudioInfoListEditor : EditorWindow {
	public AudioInfoList audioInfoList;

	[MenuItem ("Window/Audio Map Editor %&a")]
	// Use this for initialization
	static void Init ()
	{
		EditorWindow.GetWindow (typeof(AudioInfoListEditor));
	}
	void OnEnable()
	{
		if (EditorPrefs.HasKey("ObjectPath"))
		{
			string objectPath = EditorPrefs.GetString ("ObjectPath");
			audioInfoList = AssetDatabase.LoadAssetAtPath (objectPath, typeof(AudioInfoList)) as AudioInfoList;
		}
	}

	void OnGUI()
	{
		GUILayout.BeginVertical ();
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Audio Map Editor", EditorStyles.boldLabel);
		if (null != audioInfoList)
		{
			if (GUILayout.Button ("Show Audio List"))
			{
				EditorUtility.FocusProjectWindow ();
				Selection.activeObject = audioInfoList;
			}
		} 
		else
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Space (10f);
			if (GUILayout.Button("Create New Audio Map", GUILayout.ExpandWidth(false)))
			{
				CreateAudioMap ();
			}
			if (GUILayout.Button ("Open Existing Audio Map", GUILayout.ExpandWidth(false))) 
			{
				OpenAudioMap ();
			}
			GUILayout.EndHorizontal ();
		}
		if (GUILayout.Button ("Open Audio List"))
		{
			OpenAudioMap ();
		}
		if (GUILayout.Button ("New Audio List"))
		{
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = audioInfoList;
		}
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Clear Audio List", EditorStyles.boldLabel);
		if (GUILayout.Button ("Clear"))
		{
			AudioInfoList.ClearAudioInfoList(audioInfoList);
		}
		GUILayout.Label ("Copy Audio List", EditorStyles.boldLabel);
		if (GUILayout.Button ("Copy From"))
		{
			var fromList = GetAudioMap();
			AudioInfoList.CopyAudioInfoList(fromList, audioInfoList);
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
	}

	void CreateAudioMap()
	{
		audioInfoList = AudioInfoList.CreateAudioInfoList ();
		if (audioInfoList)
		{
			string relPath = AssetDatabase.GetAssetPath (audioInfoList);
			EditorPrefs.SetString ("ObjectPath", relPath);
		}
	}
	void OpenAudioMap()
	{
		string absPath = EditorUtility.OpenFilePanel ("Select Audio Map", "", "");
		if (absPath.StartsWith (Application.dataPath))
		{
			string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
			audioInfoList = AssetDatabase.LoadAssetAtPath (relPath, typeof(AudioInfoList)) as AudioInfoList;
			if (audioInfoList) {
				EditorPrefs.SetString("ObjectPath", relPath);
			}
		}
	}
	AudioInfoList GetAudioMap ()
	{
		string absPath = EditorUtility.OpenFilePanel ("Select Audio Map", "", "");
		if (absPath.StartsWith (Application.dataPath))
		{
			string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
			var tmpList = AssetDatabase.LoadAssetAtPath (relPath, typeof(AudioInfoList)) as AudioInfoList;
			if (tmpList) {
				EditorPrefs.SetString("ObjectPath", relPath);
			}
			return tmpList;
		}
		return null;
	}
}
