using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyParticle : MonoBehaviour {
	public float lifeTime;
	private float lifeTimer = 0f;
	private ParticleSystem particleSystem;
	// Use this for initialization
	void Start () {
		particleSystem = GetComponent<ParticleSystem> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (particleSystem.isStopped)
			DestroyObject (gameObject);
		if (lifeTime == 0)
			return;
		lifeTimer += Time.deltaTime;
		if (lifeTimer >= lifeTime)
			DestroyObject (gameObject);
	}
}
