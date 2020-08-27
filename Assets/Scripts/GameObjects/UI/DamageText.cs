using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;

public class DamageText : MonoBehaviour {
	private bool isBusy = false;
	public Text text;
	private RectTransform rectTrans;
	private CanvasRenderer canvasRenderer;
	public Vector3 damaged_dir;
	[Space(10f)]
	public AnimationCurve move_x;
	public AnimationCurve move_y;
	public float noise_x, noise_y;			//	startPos에 랜덤 성을 부여
	[Space(10f)]
	public AnimationCurve size_x;
	public AnimationCurve size_y;
	public float sizeAmount;
	[Space(10f)]
	public Gradient colorGradient;
	public AnimationCurve colorTransCurve;
	[Space(10f)]
	private float playbackTime = 0f;
	public float duration = 1f;
	public float speed_x = 50f;
	public float speed_y = 50f;
	private Vector3 startPos;

	// Use this for initialization
	void OnEnable () {
		text = GetComponent<Text> ();
		rectTrans = text.GetComponent<RectTransform> ();
		canvasRenderer = text.GetComponent<CanvasRenderer> ();
	}
	
	private IEnumerator IPlayTextAnimation ()
	{
		isBusy = true;
		var tmpPos = rectTrans.position;
		var tmpScale = rectTrans.localScale;
		var normalizedTime = playbackTime / duration;
		var effectColor = Color.black;
		while (playbackTime <= duration)
		{
			normalizedTime = playbackTime / duration;
			tmpPos.x = speed_x * move_x.Evaluate (normalizedTime) * damaged_dir.x;
			tmpPos.y = speed_y * move_y.Evaluate (normalizedTime) * damaged_dir.y;
			rectTrans.position = startPos + tmpPos;

			tmpScale.x = sizeAmount * size_x.Evaluate (normalizedTime);
			tmpScale.y = sizeAmount * size_y.Evaluate (normalizedTime);
			rectTrans.localScale = tmpScale;

			text.color = colorGradient.Evaluate (colorTransCurve.Evaluate (normalizedTime));
			effectColor.a = text.color.a;
			text.GetComponent<Outline> ().effectColor = effectColor;
			yield return null;
			playbackTime += Time.deltaTime;
		}
		tmpPos.x = speed_x * move_x.Evaluate (1f) * damaged_dir.x;
		tmpPos.y = speed_y * move_y.Evaluate (1f) * damaged_dir.y;
		rectTrans.position = startPos + tmpPos;

		tmpScale.x = sizeAmount * size_x.Evaluate (1f);
		tmpScale.y = sizeAmount * size_y.Evaluate (1f);
		rectTrans.localScale = tmpScale;

		text.color = colorGradient.Evaluate (1f);
		var tColor = Color.black;
		tColor.a = text.color.a;
		text.GetComponent<Outline> ().effectColor = tColor;
		isBusy = false;
		canvasRenderer.cull = true;
	}

	public void PlayUIAnimation(Vector3 startScreenPos, string damage)
	{
		canvasRenderer.cull = false;
		playbackTime = 0f;
		startPos = startScreenPos;
		damaged_dir.x += Mathf.Sign(damaged_dir.x)*Random.Range (0f, noise_x);
		damaged_dir.y += Mathf.Sign(damaged_dir.y)*Random.Range (0f, noise_y);
		text.text = damage;
		StartCoroutine ("IPlayTextAnimation");
	}

	public void StopUIAnimation()
	{
		gameObject.SetActive (false);
		playbackTime = 0f;
		StopCoroutine ("IPlayTextAnimation");
	}
	public bool IsBusy ()
	{
		return isBusy;
	}
}
