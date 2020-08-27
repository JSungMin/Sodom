using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour {
	private SoundManager soundManager;
	private Slider slider;
	public VolumeParam volumeParam;
	// Use this for initialization
	void Start () {
		slider = GetComponent<Slider>();
		soundManager = GameSystemService.Instance.soundManager;
		var volume = soundManager.GetVolume (volumeParam) / 80f + 1f;
		slider.value = volume;
		slider.onValueChanged.AddListener (delegate(float arg0) {
			arg0 = (arg0 - 1f) * 80f;
			soundManager.SetVolume(volumeParam, arg0);
		});
	}
}
