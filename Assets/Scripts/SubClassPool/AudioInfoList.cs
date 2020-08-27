using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SoundInfoNamespace;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SoundInfoNamespace
{
	[Serializable]
	public class AudioSourceInfo
	{
		public AudioType audioType;
		public AudioClip clip;
		public float volume = 1f;
		public float pitch = 1f;
		public AudioMixerGroup mixerGroup;
	}
	[Serializable]
	public class AudioSnapshotInfo
	{
		public SnapshotType snapshotType;
		public AudioMixerSnapshot snapshot;
	}
}

[CreateAssetMenu(fileName = "AudioMappingData", menuName = "Audio/Audio Map", order = 1)]
public class AudioInfoList : ScriptableObject {
	public AudioMixer mixer;
	public List<AudioSourceInfo>						audioInfoList;
	public List<AudioSnapshotInfo>						snapshotInfoList;

	#if UNITY_EDITOR
	public static AudioInfoList CreateAudioInfoList()
	{
		AudioInfoList asset = ScriptableObject.CreateInstance<AudioInfoList> ();
		AssetDatabase.CreateAsset (asset, "Assets/Resources/SFX/AudioMappingData.asset");
		AssetDatabase.SaveAssets ();
		return asset;
	}
	public static void ClearAudioInfoList (AudioInfoList target)
	{
		target.snapshotInfoList.Clear();
		target.audioInfoList.Clear();
	}
	public static void CopyAudioInfoList (AudioInfoList from, AudioInfoList to)
	{
		for (int i = 0; i < from.audioInfoList.Count; i++)
		{
			to.audioInfoList.Add (from.audioInfoList[i]);
		}
		for (int i = 0; i < from.snapshotInfoList.Count; i++)
		{
			to.snapshotInfoList.Add (from.snapshotInfoList[i]);
		}
	}
	#endif
}
