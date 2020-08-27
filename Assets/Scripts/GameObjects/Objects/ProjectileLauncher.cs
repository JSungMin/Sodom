using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour {
	public Actor actor;
	public GameObject projectile;
	public string usedSkillName;
	public Vector3 dir;
	public float launchSpeed;

	public bool oneShot = true;
	public bool autoAim = false;
	public bool dirSync = false;
	public bool isMissile = false;
	public Transform aimTarget;
	public float angle;
	public int number;
	public float interval;
	public float lifeTime;

	public Transform root;

	private float eachAngle;
	private float lookDir;

	public void Start ()
	{
		actor = GetComponentInParent<Actor> ();
		eachAngle = angle / number;
	}

	public IEnumerator ILaunch (string skillName)
	{
		float timer = 0f;
		int num = 1;
		Vector3 adjustDir = Quaternion.Euler (Vector3.back * angle * 0.5f * lookDir) * dir;
		if (oneShot) {
			while (num <= number)
			{
				var tmpProjectile = GameObject.Instantiate (projectile, transform.position, Quaternion.identity);
				tmpProjectile.GetComponent<AttackCollider> ().Initialize (actor,actor.GetLearndSkill (skillName));
				var disToTarget = aimTarget.position.x - transform.position.x;
				var newDir = Quaternion.Euler (Vector3.forward * eachAngle * num * lookDir) * adjustDir;
				var newVel = newDir * launchSpeed;
				newVel.x = 10 * disToTarget / newVel.y;
				tmpProjectile.GetComponent<Rigidbody> ().velocity = newVel;
				if (dirSync) {
					var newScale = tmpProjectile.transform.localScale;
					newScale.x = Mathf.Sign (newDir.x) * Mathf.Abs(newScale.x);
					tmpProjectile.transform.localScale = newScale;
				}
				timer = 0f;
				num++;
			}
			yield return null;
		} 
		else 
		{
			while (num <= number)
			{
				timer += Time.deltaTime;
				if (timer >= interval) 
				{
					var tmpProjectile = GameObject.Instantiate (projectile, transform.position, Quaternion.identity);
					tmpProjectile.GetComponent<AttackCollider> ().Initialize (actor,actor.GetLearndSkill (skillName));
					var disToTarget = aimTarget.position.x - transform.position.x;
					var newDir = Quaternion.Euler (Vector3.forward * eachAngle * num * lookDir) * adjustDir;
					var newVel = newDir * launchSpeed;
					newVel.x = 10 * disToTarget / newVel.y;
					tmpProjectile.GetComponent<Rigidbody> ().velocity = newVel;
					if (dirSync) {
						var newScale = tmpProjectile.transform.localScale;
						newScale.x = Mathf.Sign (newDir.x) * Mathf.Abs(newScale.x);
						tmpProjectile.transform.localScale = newScale;
					}
					timer = 0f;
					num++;
				}
				yield return null;
			}
		}
	}
	public void OnLaunch (string skillName, bool useRootDir)
	{
		lookDir = actor.lookDirection;
		if (useRootDir) 
		{
			var tmpProjectile = GameObject.Instantiate (projectile, transform.position, Quaternion.identity);
			tmpProjectile.transform.parent = GameObject.Find("BULLET_POOL").transform;
			tmpProjectile.GetComponent<AttackCollider> ().Initialize (actor,actor.GetLearndSkill (skillName));
			var tmpRigid = tmpProjectile.GetComponent<Rigidbody> ();
			var rootDir = (transform.localPosition).normalized;
			if (dirSync) {
				rootDir.x *= Mathf.Sign(lookDir);
			}
			if (isMissile)
			{
				var missileComp = tmpProjectile.GetComponent<AutoAimProjectile>();
				if (null == missileComp || null == aimTarget)
				{
					Debug.LogError("NO MISSILE COMPONENT");
				}
				missileComp.Initialize(aimTarget, rootDir);
			}
			else if (null != tmpRigid) 
			{
				tmpRigid.velocity = rootDir * launchSpeed;
			}
			var newScale = tmpProjectile.transform.localScale;
			newScale.x = Mathf.Sign (rootDir.x) * Mathf.Abs(newScale.x);
			tmpProjectile.transform.localScale = newScale;
		}
		else 
		{
			StartCoroutine ("ILaunch", skillName);
		}
	}
}
