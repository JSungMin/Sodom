using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeadUIActing : MonoBehaviour {
	public Image background;
	//public Image textBackground;
	//public Text deadText;
	//public Gradient bgColorGrad;
	//public Gradient textBgColorGrad;
	//public Gradient deadTextGrad;
	public float blurAmount;
	public AnimationCurve blurCurve;
	// Use this for initialization
	void Start () {
		background = GetComponent<Image>();
	}
	void OnEnable ()
	{
		background.material.SetFloat("_Size", 0f);
		StartCoroutine(IPlayDeadUI(1f));
	}
	void OnDisable()
	{
		background.material.SetFloat("_Size", 0f);
	}
	
	public IEnumerator IPlayDeadUI (float duration)
	{
		float timer = 0f;
		while (timer <= duration)
		{
			timer += Time.unscaledDeltaTime;
			var delta = timer / duration;
			var blurEval = blurCurve.Evaluate(delta) * blurAmount;
			background.material.SetFloat("_Size", blurEval);
			//background.color = bgColorGrad.Evaluate(delta);
			//textBackground.color = textBgColorGrad.Evaluate (delta);
			//deadText.color = deadTextGrad.Evaluate (delta);

			yield return null;
		}
	}
}
