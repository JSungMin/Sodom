using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.PostProcessing;

public class CameraManager : MonoBehaviour {
	private Camera mainCam;
	private CinemachineVirtualCamera virtualCam;

	public NoiseSettings attackVibration;
	public CinemachineBasicMultiChannelPerlin nowVibration;
	public PostProcessingBehaviour ppBehaviour;

	public float slowMotionStartFOV = 3.5f;
	public float slowMotionFOV = 2f;
	public IEnumerator slowmotion;
	public IEnumerator zoommotion;
    public IEnumerator chromaticEffect;

	public CamTarget targetFollower;
	public Collider normalCamRect;
	public Collider zoomCamRect;

	public void Init()
	{
		mainCam = Camera.main;
		virtualCam = GameObject.FindObjectOfType<CinemachineVirtualCamera> ();
		ppBehaviour = mainCam.GetComponent<PostProcessingBehaviour> ();
		ppBehaviour.profile.chromaticAberration.enabled = false;
		ppBehaviour.profile.vignette.enabled = false;
		ppBehaviour.profile.grain.enabled = false;
	}

    public void VibrateCameraByAttack(float amplie, float freq, float duration)
	{
		if (null == virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin> ())
			nowVibration = virtualCam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin> ();
		else
		{
			if (null == nowVibration)
				nowVibration = virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin> ();
		}
		nowVibration.m_NoiseProfile = attackVibration;
		nowVibration.m_AmplitudeGain = amplie;
		nowVibration.m_FrequencyGain = freq;
		StopCoroutine ("IVibrateCamera");
		StartCoroutine ("IVibrateCamera",duration);
	}

	public void PlayerDeadEffect ()
	{
		StartCoroutine ("IPlayerDeadEffect", 0.1f);
		ZoomInCamera (1.15f, 0.25f);
	}

	private IEnumerator IPlayerDeadEffect(float duration)
	{
		var timer = 0f;
		//ppBehaviour.profile.grain.enabled = true;
		ppBehaviour.profile.chromaticAberration.enabled = true;
		var chroSet = ppBehaviour.profile.chromaticAberration.settings;
		//ppBehaviour.profile.vignette.enabled = true;
		chroSet.intensity = 0f;
		var chroDur = duration * 0.5f;
		while (timer <= duration) {
			if (timer <= chroDur)
				chroSet.intensity = Mathf.Lerp (0f, 1f, timer / chroDur);
			else
				chroSet.intensity = Mathf.Lerp (1f, 0f, timer / duration);
			ppBehaviour.profile.chromaticAberration.settings = chroSet;
			timer += Time.deltaTime;
			yield return null;
		}
		chroSet.intensity = 0f;
		ppBehaviour.profile.chromaticAberration.settings = chroSet;
		//ppBehaviour.profile.vignette.enabled = false;
		//ppBehaviour.profile.grain.enabled = false;
		ppBehaviour.profile.chromaticAberration.enabled = false;
	}

	public void SetCameraViewSize (float camViewSize)
	{
		virtualCam.m_Lens.OrthographicSize = camViewSize;
		slowMotionStartFOV = camViewSize;
		slowMotionFOV = Mathf.Max (1f, camViewSize - 1.5f);
	}

	public void SlowMotionCamera (float slowRatio, float duration)
	{
		if (null != slowmotion)
			StopCoroutine (slowmotion);
		slowmotion = ISlowMotionCamera (slowRatio, duration);
		StartCoroutine (slowmotion);
	}
	public void StopSlowMotionCamera ()
	{
		if (null != slowmotion)
			StopCoroutine (slowmotion);
		StartCoroutine (IEndUpSlowMotion (Time.timeScale, 0.1f));
	}

	public void ZoomInCamera (float zoomAmount, float duration)
	{
		if (null != zoommotion)
			StopCoroutine (zoommotion);
		zoommotion = IZoomMotionCamera (zoomAmount, duration);
		StartCoroutine (zoommotion);
	}
	public void ZoomOutCamera ()
	{
		if (null != zoommotion)
			StopCoroutine (zoommotion);
		StartCoroutine (IEndUpZoomMotion (Time.timeScale, 0.1f));
	}
    public void ChromaticEffectOn(float duration)
    {
        if (null != chromaticEffect)
            StopCoroutine(chromaticEffect);
        chromaticEffect = IChromaticEffectOn(duration);
        StartCoroutine(chromaticEffect);
    }
    public void ChromaticEffectOff(float duration)
    {
        if (null != chromaticEffect)
            StopCoroutine(chromaticEffect);
        chromaticEffect = IChromaticEffectOff(duration);
        StartCoroutine(chromaticEffect);
    }

	private IEnumerator IVibrateCamera(float duration)
	{
		while (duration >= 0f) {
			duration -= Time.deltaTime;
			yield return null;
		}
		nowVibration.m_AmplitudeGain = 0f;
		nowVibration.m_FrequencyGain = 0f;
	}
    private IEnumerator IChromaticEffectOn(float duration)
    {
        Timer effectTimer = new Timer { duration = duration };
        ppBehaviour.profile.chromaticAberration.enabled = true;
        var chroSet = ppBehaviour.profile.chromaticAberration.settings;
        while (!effectTimer.CheckTimer())
        {
            chroSet.intensity = Mathf.Lerp(0f, 1f, effectTimer.GetRatio());
            ppBehaviour.profile.chromaticAberration.settings = chroSet;
            effectTimer.IncTimer(Time.deltaTime);
            yield return null;
        }
    }
    private IEnumerator IChromaticEffectOff(float duration)
    {
        Timer effectTimer = new Timer { duration = duration };
        var chroSet = ppBehaviour.profile.chromaticAberration.settings;
        while (!effectTimer.CheckTimer())
        {
            chroSet.intensity = Mathf.Lerp(1f, 0f, effectTimer.GetRatio());
            ppBehaviour.profile.chromaticAberration.settings = chroSet;
            effectTimer.IncTimer(Time.deltaTime);
            yield return null;
        }
        ppBehaviour.profile.chromaticAberration.enabled = false;
    }

	private IEnumerator IBuildUpZoomMotion (float zoomAmount, float duration)
	{
		var playbackTime = 0f;
		virtualCam.m_Lens.OrthographicSize = slowMotionStartFOV;
		virtualCam.GetComponent<CinemachineConfiner>().m_BoundingVolume = zoomCamRect;
		targetFollower.EditFollowWeight(0, 0f);

		var chroSet = ppBehaviour.profile.chromaticAberration.settings;
        var vigSet = ppBehaviour.profile.vignette.settings;
        ppBehaviour.profile.vignette.enabled = true;
        ppBehaviour.profile.chromaticAberration.enabled = true;
        chroSet.intensity = 0f;
        vigSet.intensity = 0.25f;
		while (playbackTime <= duration)
		{
			chroSet.intensity = Mathf.Lerp (0f, 1f, playbackTime / duration);
            vigSet.center = Camera.main.WorldToViewportPoint(targetFollower.transform.position);
            ppBehaviour.profile.vignette.settings = vigSet;
            ppBehaviour.profile.chromaticAberration.settings = chroSet;
			virtualCam.m_Lens.OrthographicSize = Mathf.Lerp (slowMotionStartFOV, slowMotionFOV, playbackTime / duration);
			playbackTime += Time.deltaTime;
			yield return null;
		}
		chroSet.intensity = 1f;
		virtualCam.m_Lens.OrthographicSize = slowMotionFOV;
		ppBehaviour.profile.chromaticAberration.settings = chroSet;
		yield return null;
	}
	private IEnumerator IMaintainZoomMotion(float zoomAmount, float duration)
	{
		var playbackTime = 0f;
		virtualCam.m_Lens.OrthographicSize = slowMotionFOV;
		GameSystemService.Instance.StopFrame (0.15f);
        var vigSet = ppBehaviour.profile.vignette.settings;
        while (playbackTime <= duration)
		{
            vigSet.center = Camera.main.WorldToViewportPoint(targetFollower.transform.position);
            ppBehaviour.profile.vignette.settings = vigSet;
            playbackTime += Time.deltaTime;
			yield return null;
		}
		yield return null;
	}
	private IEnumerator IEndUpZoomMotion(float zoomAmount, float duration)
	{
		var playbackTime = 0f;
		var chroSet = ppBehaviour.profile.chromaticAberration.settings;
        var vigSet = ppBehaviour.profile.vignette.settings;
        chroSet.intensity = 1f;
		while (playbackTime <= duration)
		{
			playbackTime += Time.deltaTime;
			chroSet.intensity =  Mathf.Lerp (1f, 0f, playbackTime / duration);
            vigSet.center = Camera.main.WorldToViewportPoint(targetFollower.transform.position);
			ppBehaviour.profile.chromaticAberration.settings = chroSet;
            ppBehaviour.profile.vignette.settings = vigSet;
            virtualCam.m_Lens.OrthographicSize = Mathf.Lerp (slowMotionFOV, slowMotionStartFOV, playbackTime / duration);
			yield return null;
		}
		virtualCam.GetComponent<CinemachineConfiner>().m_BoundingVolume = normalCamRect;
		targetFollower.EditFollowWeight(0, 0.5f);
		chroSet.intensity = 0f;
		ppBehaviour.profile.chromaticAberration.settings = chroSet;
		virtualCam.m_Lens.OrthographicSize = slowMotionStartFOV;
		ppBehaviour.profile.vignette.enabled = false;
        ppBehaviour.profile.chromaticAberration.enabled = false;
        yield return null;
	}

	private IEnumerator IBuildUpSlowMotion (float slowRatio, float duration)
	{
		var playbackTime = 0f;
		while (playbackTime <= duration)
		{
			Time.timeScale = Mathf.Lerp (1f, slowRatio, playbackTime / duration);
			virtualCam.m_Lens.FieldOfView = Mathf.Lerp (slowMotionStartFOV, slowMotionFOV, playbackTime / duration);
			playbackTime += Time.deltaTime;
			yield return null;
		}
		yield return null;
	}
	private IEnumerator IMaintainSlowMotion(float slowRatio, float duration)
	{
		var playbackTime = 0f;
		Time.timeScale = slowRatio;
		virtualCam.m_Lens.FieldOfView = slowMotionFOV;
		while (playbackTime <= duration)
		{
			playbackTime += Time.deltaTime;
			yield return null;
		}
		yield return null;
	}
	private IEnumerator IEndUpSlowMotion(float slowRatio, float duration)
	{
		var playbackTime = 0f;
		while (playbackTime <= duration)
		{
			Time.timeScale = Mathf.Lerp (slowRatio, 1f, playbackTime/duration);
			playbackTime += Time.deltaTime;
			virtualCam.m_Lens.FieldOfView = Mathf.Lerp (virtualCam.m_Lens.FieldOfView, slowMotionStartFOV, playbackTime / duration);
			yield return null;
		}
		virtualCam.m_Lens.FieldOfView = slowMotionStartFOV;
		Time.timeScale = 1f;
		yield return null;
	}

	private IEnumerator IZoomMotionCamera (float zoomAmount, float duration)
	{
		float buildUpTime = duration * 0.2f;
		float mainTime = duration * 0.5f;
		float endTime = duration * 0.3f;

		yield return IBuildUpZoomMotion (zoomAmount, buildUpTime);
		yield return IMaintainZoomMotion (zoomAmount, mainTime);
		yield return IEndUpZoomMotion (zoomAmount, endTime);
	}

	private IEnumerator ISlowMotionCamera (float slowRatio, float duration)
	{
		float buildUpTime = duration * 0.2f;
		float mainTime = duration * 0.5f;
		float endTime = duration * 0.3f;

		yield return IBuildUpSlowMotion (slowRatio, buildUpTime);
		yield return IMaintainSlowMotion (slowRatio, mainTime);
		yield return IEndUpSlowMotion (slowRatio, endTime);
	}
}
