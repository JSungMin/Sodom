using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public class ParticleAttackCollider : AttackColliderSub {

	private ParticleSystem ps;
	private List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
	private List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();
	public override void Initialize (AttackCollider attackCollider)
	{
		base.Initialize (attackCollider);
		ps = GetComponent<ParticleSystem> ();
		ps.trigger.SetCollider (0, parent.player.bodyCollider);
	}

	private void OnParticleCollision (GameObject obj)
	{
		if (parent.skillInfo.IsEmpty ())
			return;
		if (obj.layer != LayerMask.NameToLayer ("Actor"))
			return;
		if (parent.attacker.gameObject.CompareTag (obj.tag))
			return;
		if (parent.victimDictionary.ContainsKey (obj.name))
			return;
		var victim = obj.GetComponent<Actor> ();
		if (null == victim)
			return;
		var damageInfo = new DamageInfo (parent.attacker, victim, parent.skillInfo);
		bool result = victim.OnActorDamaged(damageInfo);
		if (!result && !victim.actorInfo.isSuperarmor)
			return;
		parent.victimDictionary.Add (obj.name, victim);
		if (parent.OnAttackSuccess != null)
			parent.OnAttackSuccess ();
		parent.attacker.OnActorAttackSuccess (damageInfo);
		if (victim.actorInfo.GetLife () <= 0f) {
			parent.attacker.OnActorKill (damageInfo);
		}
	}
	private void OnParticleTrigger ()
	{
		int numEnter = ps.GetTriggerParticles (ParticleSystemTriggerEventType.Enter, enter);
		int numInside = ps.GetTriggerParticles (ParticleSystemTriggerEventType.Inside, inside);
		if (numEnter > 0 || numInside > 0) 
		{
			OnParticleCollision (parent.player.gameObject);
		}
	}
}
