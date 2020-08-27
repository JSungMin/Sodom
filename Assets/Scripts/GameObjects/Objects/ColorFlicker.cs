using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorFlicker {
	public float flickerRate = 0.05f;
	public float damFlickTimer = 0f;
	public float damFlickDuration = 0.1f;
	private int damFlickCount = 0;
	public Color flickColor = Color.white;
	public Material defaultMat;
	public IEnumerator OnDamageFlick;

	private string tintBlack = "Spine/Skeleton Tint Black";
	private string propName = "_OverlayColor";

	private Color normColor;

	public void Initialize ()
	{
		if (defaultMat.shader.name == tintBlack) {
			propName = "_Black";
			normColor = Color.black;
		}
		else
			normColor = Color.white;
		normColor.a = 0f;
		defaultMat.SetColor (propName, normColor);
		OnDamageFlick = IDamagedFlick();
	}

	private IEnumerator IDamagedFlick ()
	{
		damFlickTimer = 0f;
		damFlickCount = (int)(damFlickDuration / flickerRate);
		while (damFlickCount > 0) 
		{
			if (damFlickCount % 2 == 0) {
				defaultMat.SetColor (propName, flickColor);
			}
			else {
				defaultMat.SetColor (propName, normColor);
			}
 			if (damFlickTimer >= flickerRate) {
				damFlickCount--;	
				damFlickTimer = 0f;
			}
			damFlickTimer += Time.deltaTime;
			yield return null;
		}
	}
}
