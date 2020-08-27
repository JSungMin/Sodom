using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MaskedTriggerEvents : MonoBehaviour {
	public LayerMask mask;
	public string[] tags;
	public UnityEvent onEnter;
	public UnityEvent onStay;
	public UnityEvent onExit;

	void OnTriggerEnter(Collider other)
	{	
		if (((1 << other.gameObject.layer) & mask.value) >= 1)
		{
			bool checkTag = false;
			for (int i = 0; i < tags.Length; i++)
			{
				if (other.gameObject.tag == tags[i])
				{
					checkTag = true;
					break;
				}
			}
			if (null != onEnter && checkTag)
			{
				onEnter.Invoke();
			}
		}
	}
	void OnTriggerStay(Collider other)
	{
		if (((1 << other.gameObject.layer) & mask.value) >= 1)
		{
			bool checkTag = false;
			for (int i = 0; i < tags.Length; i++)
			{
				if (other.gameObject.tag == tags[i])
				{
					checkTag = true;
					break;
				}
			}
			if (null != onStay && checkTag)
			{
				onStay.Invoke();
			}
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if (((1 << other.gameObject.layer) & mask.value) >= 1)
		{
			bool checkTag = false;
			for (int i = 0; i < tags.Length; i++)
			{
				if (other.gameObject.tag == tags[i])
				{
					checkTag = true;
					break;
				}
			}
			if (null != onExit && checkTag)
			{
				onExit.Invoke();
			}
		}
	}
}
