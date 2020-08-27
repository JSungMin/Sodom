using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Water2D_Collider : MonoBehaviour {
	public enum CollideType {Enter = 0,	Stay, Exit};
	public SpriteRenderer spriteRenderer;
	public CollideType collideType;
	private List<Action> collideAction = new List<Action>();

	public float disappearSpeed = 2f;

	public bool DestroyWithTransparent ()
	{
		if (spriteRenderer.color.a <= 0f) {
			DestroyObject (gameObject);
			return true;
		}
		return false;
	}

	public void EnterAction()
	{
		if (!DestroyWithTransparent ()) 
		{
			var color = spriteRenderer.color;
			color.a -= Time.deltaTime * disappearSpeed;
			spriteRenderer.color = color;
			transform.localScale += Vector3.one * Time.deltaTime * 5f;
		}
	}
	public void StayAction()
	{
		if (!DestroyWithTransparent ()) 
		{
			var color = spriteRenderer.color;
			color.a -= Time.deltaTime * disappearSpeed;
			spriteRenderer.color = color;
			transform.localScale += Vector3.one * Time.deltaTime;
		}
	}
	public void ExitAction()
	{
		if (!DestroyWithTransparent ()) 
		{
			var color = spriteRenderer.color;
			color.a -= Time.deltaTime * disappearSpeed;
			spriteRenderer.color = color;
			transform.localScale += Vector3.one * Time.deltaTime * 5f;
		}
	}

	// Use this for initialization
	void Start () {
		if (null == spriteRenderer)
			spriteRenderer = GetComponent<SpriteRenderer> ();
		collideAction.Add (EnterAction);
		collideAction.Add (StayAction);
		collideAction.Add (ExitAction);
	}
	
	// Update is called once per frame
	void Update () {
		collideAction [(int)collideType].Invoke();
	}
}
