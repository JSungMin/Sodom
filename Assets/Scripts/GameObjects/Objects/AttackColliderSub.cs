using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using InformationNamespace;

public class AttackColliderSub : MonoBehaviour {
	[HideInInspector]
	public AttackCollider parent;
	public virtual void Initialize (AttackCollider attackCollider)
	{
		parent = attackCollider;
	}

	private void OnTriggerEnter (Collider col)
	{
		if (parent.skillInfo.IsEmpty ())
			return;
		if (col.gameObject.layer != LayerMask.NameToLayer ("Actor"))
			return;
		if (parent.attacker.gameObject.CompareTag (col.gameObject.tag))
			return;
		if (parent.victimDictionary.ContainsKey (col.gameObject.name))
			return;
		var victim = col.gameObject.GetComponent<Actor> ();
		if (null == victim)
			return;
		var damageInfo = new DamageInfo (parent.attacker, victim, parent.skillInfo);
		bool result = victim.OnActorDamaged(damageInfo);
		if (!result && !victim.actorInfo.isSuperarmor)
		{
			return;
		}
		if (parent.skillInfo.successSfxType != AudioType.PLAYER_COUNTER && 
			parent.skillInfo.successSfxType != AudioType.NONE)
		{
			parent.attacker.StopSound(parent.skillInfo.useSfxType);
			parent.attacker.PlaySound(parent.skillInfo.successSfxType);
		}
		parent.victimDictionary.Add (col.gameObject.name, victim);
		if (parent.OnAttackSuccess != null)
			parent.OnAttackSuccess ();
		parent.attacker.OnActorAttackSuccess (damageInfo);
		if (victim.actorInfo.GetLife () <= 0f) {
			parent.attacker.OnActorKill (damageInfo);
			victim.OnActorDead(damageInfo);
		}
	}

	private void OnTriggerStay (Collider col)
	{
		if (parent.skillInfo.IsEmpty ())
			return;
		if (col.gameObject.layer != LayerMask.NameToLayer ("Actor"))
			return;
		if (parent.attacker.gameObject.CompareTag (col.gameObject.tag))
			return;
		if (parent.victimDictionary.ContainsKey (col.gameObject.name))
			return;
		var victim = col.gameObject.GetComponent<Actor> ();
		if (null == victim)
			return;
		if (parent.skillInfo.animName == "Tackle")
		{
			Debug.Log("돌진돌진돌진돌진돌진돌진돌진돌진돌진돌진돌진돌진돌진돌진돌진");
		}
		else 
		{
			Debug.Log("NOT 똘찐 : " + parent.skillInfo.animName);
		}
		var damageInfo = new DamageInfo (parent.attacker, victim, parent.skillInfo);
		bool result = victim.OnActorDamaged(damageInfo);
		if (!result && !victim.actorInfo.isSuperarmor)
			return;
		if (parent.skillInfo.successSfxType != AudioType.PLAYER_COUNTER && 
			parent.skillInfo.successSfxType != AudioType.NONE)
		{
			parent.attacker.StopSound(parent.skillInfo.useSfxType);
			parent.attacker.PlaySound(parent.skillInfo.successSfxType);
		}
		parent.victimDictionary.Add (col.gameObject.name, victim);
		if (parent.OnAttackSuccess != null)
			parent.OnAttackSuccess ();
		parent.attacker.OnActorAttackSuccess (damageInfo);
		if (victim.actorInfo.GetLife () <= 0f) {
			parent.attacker.OnActorKill (damageInfo);
		}
	}
}
