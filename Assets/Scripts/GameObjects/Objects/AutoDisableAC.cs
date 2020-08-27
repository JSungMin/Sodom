using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDisableAC : MonoBehaviour {
	public Timer timer;
	public AttackCollider ac;
	// Update is called once per frame
	void Update () {
		timer.IncTimer (Time.deltaTime);
		if (timer.CheckTimer ()) 
		{
			ac.isInitialize = false;
			ac.Release (ac.player);
		}
	}
}
