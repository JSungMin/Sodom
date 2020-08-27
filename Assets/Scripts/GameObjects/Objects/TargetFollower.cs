using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollower : MonoBehaviour {
	public Transform target;
	public float strength;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (null != target && target.gameObject.activeSelf)
		{
			transform.position = Vector3.Lerp(transform.position, target.position, strength * Time.deltaTime);
		}
	}
}
