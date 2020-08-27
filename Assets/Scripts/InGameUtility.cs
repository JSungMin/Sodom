using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public class InGameUtility {

	public static void KnockbackToActor (Actor targetActor, Vector3 dir, float amount)
	{
		var rigid = targetActor.GetComponent<Rigidbody> ();
		rigid.velocity = dir * amount;
		//rigid.AddForce (dir * amount * 10);
	}
}
