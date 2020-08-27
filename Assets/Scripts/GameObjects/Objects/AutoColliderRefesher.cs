using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AttackCollider))]
public class AutoColliderRefesher : MonoBehaviour {
	public AttackCollider attackCollider;
	public bool useAutoRefresh = false;
	public int maxHit = 5;
	private int curHit = 0;
	public float refreshRate = 0.1f;
	public float refreshTimer = 0f;

	// Use this for initialization
	void Start () {
		attackCollider = GetComponent<AttackCollider> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!useAutoRefresh)
			return;
		if (curHit >= maxHit)
			return;
		refreshTimer += Time.deltaTime;
		if (refreshTimer >= refreshRate) 
		{
			refreshTimer = 0f;
			attackCollider.victimDictionary.Clear ();
		}
	}
}
