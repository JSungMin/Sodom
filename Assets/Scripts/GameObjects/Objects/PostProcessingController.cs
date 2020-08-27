using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Events;

public class PostProcessingController : MonoBehaviour {
	public Camera cam;
	public PostProcessingBehaviour ppBehaviour;
	[System.Serializable]
	public struct VignettePattern
	{
		public bool useVigGrad;
		public bool useVigTarget;
		public Transform vigTarget;
		public AnimationCurve vigIntensityCurve;
		public AnimationCurve vigSmoothCurve;
		public Timer vigTimer;
	}
	[System.Serializable]
	public struct ColorAdjustPattern
	{
		public bool isLoop;
		public bool useChannelMix;
		public Vector3 rChannel, gChannel, bChannel;
		public Timer colorAdjustTimer;
		public UnityEvent OnStartPattern;
		public UnityEvent OnFinishPattern;
	}
	public int vignettePatternIdx = -1;
	public int colorAdjustPatternIdx = -1;
	public List<VignettePattern> vignettePatterns;
	public List<ColorAdjustPattern> colorAdjustPatterns;
	
	public UnityEvent OnStartPattern;

	void Start()
	{
		var gradSet = ppBehaviour.profile.colorGrading.settings;
		var prevRed = gradSet.channelMixer.red;
		gradSet.channelMixer.red = Vector3.right;
		gradSet.channelMixer.green = Vector3.up;
		gradSet.channelMixer.blue = Vector3.forward;
		ppBehaviour.profile.colorGrading.settings = gradSet;
		if (OnStartPattern.GetPersistentEventCount() != 0)
			OnStartPattern.Invoke();	
	}

	// Update is called once per frame
	void Update () 
	{
		OnVignettePattern();	
		OnColorAdjustPattern();
	}
	public void SetVignettePattern (int index)
	{
		vignettePatternIdx = index;
	}
	public void SetColorAdjustPattern (int index)
	{
		colorAdjustPatternIdx = index;
		if (0 != colorAdjustPatterns[index].OnStartPattern.GetPersistentEventCount())
			colorAdjustPatterns[index].OnStartPattern.Invoke();
	}
	void OnVignettePattern ()
	{
		if (vignettePatternIdx == -1)
			return;
		var nowPattern = vignettePatterns[vignettePatternIdx];
		if (nowPattern.useVigGrad)
		{
			ppBehaviour.profile.vignette.enabled = true;
			var vigSet = ppBehaviour.profile.vignette.settings;
			vigSet.intensity = nowPattern.vigIntensityCurve.Evaluate(nowPattern.vigTimer.GetRatio());
			vigSet.smoothness = nowPattern.vigSmoothCurve.Evaluate(nowPattern.vigTimer.GetRatio());
		
			if (nowPattern.useVigTarget)
			{
				vigSet.center = Camera.main.WorldToViewportPoint(nowPattern.vigTarget.position);
			}

			ppBehaviour.profile.vignette.settings = vigSet;
			nowPattern.vigTimer.IncTimer(Time.deltaTime);
			if (nowPattern.vigTimer.CheckTimer())
			{
				nowPattern.vigTimer.Reset();
			}
		}	
	}
	void OnColorAdjustPattern ()
	{
		if (colorAdjustPatternIdx == -1)
			return;
		var nowPattern = colorAdjustPatterns[colorAdjustPatternIdx];
		if (nowPattern.useChannelMix)
		{
			ppBehaviour.profile.colorGrading.enabled = true;
			var gradSet = ppBehaviour.profile.colorGrading.settings;
			var prevRed = gradSet.channelMixer.red;
			var prevBlue = gradSet.channelMixer.blue;
			var prevGreen = gradSet.channelMixer.green;
			gradSet.channelMixer.red = Vector3.Lerp (prevRed, nowPattern.rChannel, nowPattern.colorAdjustTimer.GetRatio());
			gradSet.channelMixer.green = Vector3.Lerp (prevGreen, nowPattern.gChannel, nowPattern.colorAdjustTimer.GetRatio());
			gradSet.channelMixer.blue = Vector3.Lerp (prevBlue, nowPattern.bChannel, nowPattern.colorAdjustTimer.GetRatio());
			ppBehaviour.profile.colorGrading.settings = gradSet;
			nowPattern.colorAdjustTimer.IncTimer(Time.deltaTime);
			if (nowPattern.colorAdjustTimer.CheckTimer())
			{
				if (nowPattern.isLoop)
				{
					nowPattern.colorAdjustTimer.Reset();
				}
				else
				{
					colorAdjustPatternIdx = -1;
					nowPattern.colorAdjustTimer.timer = nowPattern.colorAdjustTimer.duration;
				}
				if (nowPattern.OnFinishPattern.GetPersistentEventCount() != 0)
					nowPattern.OnFinishPattern.Invoke();
			}			
		}
	}
}
