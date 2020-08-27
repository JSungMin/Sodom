using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveBackground : MonoBehaviour {
	public Vector3 initPos;
	public Transform focusTarget;
	public float depth = 0f;
	// Use this for initialization
	void Start ()
	{
		if (null == focusTarget)
			focusTarget = Player.instance.transform;
		initPos = transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		var focusPos = focusTarget.position;
		var deltaPos = initPos - focusPos;
		deltaPos.y = 0f;
		deltaPos.z = 0f;
		transform.position = initPos + deltaPos * depth;
	}
}
