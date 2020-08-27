using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAimProjectile : MonoBehaviour {
	public bool useDelay = false;
	public Timer delayAimTimer;
	public Transform aimTarget;
	public Rigidbody rigid;
	public float correctionStr;
	private Vector3 dirToTarget;
	public Vector3 curDir;
	public float force;
	// Use this for initialization
	public void Initialize(Transform aimTarget, Vector3 curDir)
	{
		this.aimTarget = aimTarget;
		this.curDir = curDir;
		rigid = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (useDelay)
		{
			if (!delayAimTimer.CheckTimer())
			{
				rigid.velocity = curDir * force;
				delayAimTimer.IncTimer(Time.deltaTime);
				return;
			}
		}
		dirToTarget = (aimTarget.position - transform.position).normalized;
		curDir = Vector3.Lerp(curDir, dirToTarget, correctionStr * Time.deltaTime);
		rigid.velocity = curDir * force;
	}
}
