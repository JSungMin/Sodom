using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Water2D : MonoBehaviour {

	private Material mat;

	public GameObject waterInfoObj;

	public float flowBaseSpeed = 0.25f;
	public float flowDetailSpeed = 0.5f;

	public float flowBaseAmplitude = 0.05f;
	public float flowDetailAmplitude = 0.1f;

	void PrintDeviceInfo ()
	{
		Debug.Log ("Graphic Device Version : " + SystemInfo.graphicsDeviceVersion);
		Debug.Log ("Graphic Type : " + SystemInfo.graphicsDeviceType);
		Debug.Log ("NPOT Support : " + SystemInfo.npotSupport);
		Debug.Log ("Start On Top : " + SystemInfo.graphicsUVStartsAtTop);
		Debug.Log ("Graphic Shader Level : " + SystemInfo.graphicsShaderLevel);
	}



	// Use this for initialization
	void Start () {
		mat = GetComponent<SpriteRenderer> ().material;
		PrintDeviceInfo ();
	}

	public void CreateWaterInfoObject (Vector3 pos, Color info, Water2D_Collider.CollideType ctype)
	{
		var tmpWaterInfo = GameObject.Instantiate (waterInfoObj);
		tmpWaterInfo.transform.position = pos;
		var wc = tmpWaterInfo.GetComponent<Water2D_Collider> ();
		wc.collideType = ctype;
		wc.spriteRenderer.color = info;
	}

	void OnTriggerEnter (Collider col)
	{
		if (col.CompareTag("WaterCollider")) 
		{
			Debug.Log ("Collide Enter");
			Debug.Log ("Col POS X : " + ((col.transform.position.x - transform.position.x + 6f)/12));
			Debug.Log ("Col POS Y : " + ((col.transform.position.y - transform.position.y + 1.5f)/3f));
			CreateWaterInfoObject (col.transform.position, Color.white, Water2D_Collider.CollideType.Enter);
		}
	}
	void OnTriggerStay (Collider col)
	{
		if (col.CompareTag("WaterCollider")) 
		{
			Debug.Log ("Collide Stay");
			//CreateWaterInfoObject (col.transform.position, Color.white, Water2D_Collider.CollideType.Stay);
		}
	}
	void OnTriggerExit (Collider col)
	{
		if (col.CompareTag("WaterCollider")) 
		{
			Debug.Log ("Collide Exit");
			CreateWaterInfoObject (col.transform.position, Color.white, Water2D_Collider.CollideType.Exit);
		}
	}

	// Update is called once per frame
	void Update () {
		mat.SetFloat ("_BaseScrollSpeed", flowBaseSpeed);
		mat.SetFloat ("_DetailScrollSpeed", flowDetailSpeed);

		mat.SetFloat ("_BaseMagnitude", flowBaseAmplitude);
		mat.SetFloat ("_DetailMagnitude", flowDetailAmplitude);
	}
}
