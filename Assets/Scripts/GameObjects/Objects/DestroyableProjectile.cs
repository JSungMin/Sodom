using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AttackCollider))]
public class DestroyableProjectile : MonoBehaviour {
	public float lifeTime = -1f;
	public int reflectLimit = 0;
	private float lifeTimer = 0f;
	private int reflectCount = 0;
	private Rigidbody rigid;
	public ParticleSystem attackParticle;
	private AttackCollider ac;
	public GameObject deadParticle;
	public float bounce;
	[SerializeField]
	private bool destroyFlag = false;

	void Start ()
	{
		rigid = GetComponent<Rigidbody> ();
		ac = GetComponent<AttackCollider> ();
	}

	// Update is called once per frame
	void Update () {
		if (destroyFlag)
		{
			if (attackParticle.particleCount == 0)
				DestroyObject (gameObject);
		}
		if (lifeTime <= 0f)
			return;
		lifeTimer += Time.deltaTime;
		if (lifeTimer >= lifeTime)
		{
			OnDestroyProjectile ();
		}
	}

	public void OnCollisionEnter (Collision col)
	{
		if (reflectLimit <= 0)
			return;
		if (col.gameObject.layer == LayerMask.NameToLayer("MapCollider")) 
		{
			reflectCount++;
			rigid.velocity *= bounce;
			if (reflectCount >= reflectLimit) 
			{
				OnDestroyProjectile ();
			}
		}
	}
	public void OnCollisionStay (Collision col)
	{
		if (reflectLimit <= 0)
			return;
		if (col.gameObject.layer == LayerMask.NameToLayer("MapCollider")) 
		{
			reflectCount++;
			rigid.velocity *= bounce;
			if (reflectCount >= reflectLimit) 
			{
				OnDestroyProjectile ();
			}
		}
	}
	public void OnDestroyProjectile()
	{
		if (destroyFlag)
			return;
		if (attackParticle == null)
			return;
		attackParticle.Stop ();
		destroyFlag = true;
		if (deadParticle == null)
			return;
		var newParticle = GameObject.Instantiate (deadParticle);
		newParticle.transform.position = transform.position;
		newParticle.GetComponent<ParticleSystem> ().Play();
		for (int i = 0; i < newParticle.transform.childCount; i++)
		{
			var childParticle = newParticle.transform.GetChild(i).GetComponent<ParticleSystem>();
			if (null != childParticle)
			{
				childParticle.Stop();
			}
		}
		var deadAc = newParticle.GetComponent<AttackCollider> ();
		if (null == deadAc)
			return;
		deadAc.Initialize (ac.attacker, ac.skillInfo);
		deadAc.victimDictionary = ac.victimDictionary;
	}
}
