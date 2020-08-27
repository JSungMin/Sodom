using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class BossAppearUIEffect : MonoBehaviour {

	public Sprite nameImg;
	public Image namePlate;

	public Vector2 ImageNativeSize;

	Vector2 namePlatePos;

	void Start () {
		namePlate.sprite = nameImg;
		namePlate.SetNativeSize();
		ImageNativeSize = namePlate.rectTransform.sizeDelta;
		namePlatePos = namePlate.rectTransform.position;
		resetNamePlate();
	}
	
	// Update is called once per frame
	void Update () {
		// if(Input.GetKeyDown(KeyCode.F)){
		// 	StopCoroutine(SequentialSize());
		// 	StartCoroutine(SequentialSize());
		// }

	}
	public void ActivateSequentialSize()
	{
		StartCoroutine(SequentialSize());
	}
	IEnumerator SequentialSize(){
		Timer temptimer = new Timer();
		temptimer.duration = 1;
		while(!temptimer.CheckTimer()){
			namePlate.color = Color.Lerp(new Color(1,1,1,0), new Color(1,1,1,1), temptimer.GetRatio());
			namePlate.rectTransform.sizeDelta = Vector2.Lerp(new Vector2(0,namePlate.rectTransform.sizeDelta.y), ImageNativeSize,temptimer.GetRatio());
			temptimer.IncTimer(Time.deltaTime);
			yield return null;
		}
	}

	public void resetNamePlate(){
		namePlate.color = new Color(1,1,1,0);
		namePlate.rectTransform.sizeDelta = new Vector2(0,namePlate.rectTransform.sizeDelta.y);
		namePlate.rectTransform.position = namePlatePos;
	}

}
