using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SoundInfoNamespace;

public class SoundManager : MonoBehaviour {
	public AudioInfoList audioMapData;
	public Dictionary<AudioType, AudioSourceInfo> 		audioInfoDic = new Dictionary<AudioType, AudioSourceInfo>();
	public Dictionary<SnapshotType, AudioSnapshotInfo> 	snapshotInfoDic = new Dictionary<SnapshotType, AudioSnapshotInfo>();
	// Use this for initialization
	public void Init ()
	{
		audioInfoDic.Clear();
		snapshotInfoDic.Clear();
		for (int i = 0; i < audioMapData.audioInfoList.Count; i++)
		{
			audioInfoDic[audioMapData.audioInfoList[i].audioType] = audioMapData.audioInfoList[i];
		}
		for (int i = 0; i < audioMapData.snapshotInfoList.Count; i++)
		{
			snapshotInfoDic[audioMapData.snapshotInfoList[i].snapshotType] = audioMapData.snapshotInfoList[i];
		}
	}
	public void SetVolume (VolumeParam paramName, float volume)
	{
		audioMapData.mixer.SetFloat(paramName.ToString(), volume);
	}
	public float GetVolume (VolumeParam paramName)
	{
		var volume = 0f; 
		audioMapData.mixer.GetFloat(paramName.ToString(), out volume);
		return volume;
	}
	public void PlaySound (AudioType audioType, float delay = 0)
	{
		if (audioType == AudioType.NONE)
			return;
		var sources = GameObject.FindObjectsOfType<AudioSource>();
		for (int i = 0; i < sources.Length; i++)
		{
			var source = sources[i];
			if (!source.isPlaying)
			{
				var info = audioInfoDic[audioType];
				source.clip = info.clip;
				source.volume = info.volume;
				source.pitch = info.pitch;
				source.outputAudioMixerGroup = info.mixerGroup;
				float pitchConst = Mathf.Pow(source.pitch, 2f);
				ulong freq = (ulong)(source.clip.frequency * delay * pitchConst);
				source.Play(freq);
				return;
			}
		}
	}
	public void PlaySound(AudioType audioType, bool overlap, float delay = 0)
	{
		if (!overlap)
		{
			PlaySound(audioType, delay);
			return;
		}
		if (audioType == AudioType.NONE)
			return;
		var sources = GameObject.FindObjectsOfType<AudioSource>();
		for (int i = 0; i < sources.Length; i++)
		{
			var source = sources[i];
			if (!source.isPlaying)
			{
				var info = audioInfoDic[audioType];
				source.clip = info.clip;
				source.volume = info.volume;
				source.pitch = info.pitch;
				source.outputAudioMixerGroup = info.mixerGroup;
				float pitchConst = Mathf.Pow(source.pitch, 2f);
				ulong freq = (ulong)(source.clip.frequency * delay * pitchConst);
				source.Play(freq);
				return;
			}
		}
	}
	public void StopSound (AudioType audioType)
	{
		if (audioType == AudioType.NONE)
			return;
		var sources = GameObject.FindObjectsOfType<AudioSource>();
		for (int i = 0; i < sources.Length; i++)
		{
			var source = sources[i];
			if (source.isPlaying && source.clip == audioInfoDic[audioType].clip)
			{
				source.Stop();
			}
		}
	}
	public void SetSnapshot (SnapshotType snapshotType, float transTime)
	{
		snapshotInfoDic[snapshotType].snapshot.TransitionTo(transTime);
	}
	public IEnumerator IPlayCounterSFX(float durationIn, float retainDuration, float durationOut)
	{
		SetSnapshot(SnapshotType.COUNTER, durationIn);
		GameObject.FindObjectOfType<CamTarget>().EditFollowWeight(0,0f);
		float timer = 0f;
		while (timer <= durationIn) 
		{
			timer += Time.deltaTime;
			yield return null;
		}
		timer = 0f;
		while (timer <= retainDuration) {
			timer += Time.deltaTime;
			yield return null;
		}
		SetSnapshot(SnapshotType.NORMAL, durationOut);
		GameObject.FindObjectOfType<CamTarget>().EditFollowWeight(0,0.5f);
	}
}
