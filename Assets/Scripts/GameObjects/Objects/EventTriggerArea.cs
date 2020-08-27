using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTriggerArea : MonoBehaviour {
	public Collider target;
	public UnityEvent OnEnterTarget;
	public UnityEvent OnExitTarget;

	void OnTriggerEnter(Collider other)
	{
		if (other == target)
		{
			if (0 != OnEnterTarget.GetPersistentEventCount())
			{
				OnEnterTarget.Invoke();
			}
		}	
	}
	void OnTriggerExit(Collider other)
	{
		if (other == target)
		{
			if (0 != OnEnterTarget.GetPersistentEventCount())
			{
				OnExitTarget.Invoke();
			}
		}	
	}	
}
