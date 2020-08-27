using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateBehaviourPool {

	public static Dictionary<string, Action<ActionState>> behaviourMap = new Dictionary<string, Action<ActionState>>();

	public void InitializeStateBehaviour ()
	{

	}

}
