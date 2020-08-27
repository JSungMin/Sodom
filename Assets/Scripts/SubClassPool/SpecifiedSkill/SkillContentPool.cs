using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public class SkillContentPool {
	public static GameSystemService serviceInstance;

	public static Dictionary<string, Action<Actor, SkillInfo>> onAttackContentMap = new Dictionary<string, Action<Actor, SkillInfo>> ();
	public static Dictionary<string, Action<DamageInfo>> onDamagedContentMap = new Dictionary<string, Action<DamageInfo>>();

	public static void InitializeContentMap ()
	{
		serviceInstance = GameSystemService.Instance;

		onAttackContentMap ["ActiveSuperArmor"] = ActiveSuperArmor;
		onAttackContentMap ["VibrateCamera"] = VibrateCamera;
		onAttackContentMap ["TeleportConsiderCollider"] = TeleportConsiderCollider;
		onAttackContentMap ["TeleportWithAttack"] = TeleportWithAttack;
		onAttackContentMap ["ForwardDash"] = ForwardDash;
		onAttackContentMap ["BackDash"] = BackDash;
		onAttackContentMap ["ReleaseSuperArmor"] = ReleaseSuperArmor;
		onAttackContentMap ["StopVelocityX"] = StopVelocityX;
		onAttackContentMap ["StopVelocityY"] = StopVelocityY;
		onAttackContentMap ["StopVelocityZ"] = StopVelocityZ;
		onAttackContentMap ["StopVelocity"] = StopVelocity;

		onDamagedContentMap ["SlowMotion"] = SlowMotion;
		onDamagedContentMap ["SmashVictim"] = SmashVictim;
		onDamagedContentMap ["SmashForward"] = SmashForward;
		onDamagedContentMap ["SmashBackward"] = SmashBackward;
		onDamagedContentMap ["VibrateCamera"] = VibrateCamera;
		onDamagedContentMap ["LaunchVictim"] = LaunchVictim;
		onDamagedContentMap ["DownBlow"] = DownBlow;
	}

	public static void ExecuteOnAttackEnterContent (string contentName, Actor actor, SkillInfo skillInfo)
	{
		if (onAttackContentMap.ContainsKey(contentName))
			onAttackContentMap [contentName].Invoke (actor,skillInfo);
	}
	public static void ExecuteOnAttackUpdateContent (string contentName, Actor actor, SkillInfo skillInfo)
	{
		if (onAttackContentMap.ContainsKey(contentName))
			onAttackContentMap [contentName].Invoke (actor, skillInfo);
	}
	public static void ExecuteOnAttackExitContent (string contentName, Actor actor, SkillInfo skillInfo)
	{
		if (onAttackContentMap.ContainsKey(contentName))
			onAttackContentMap [contentName].Invoke (actor, skillInfo);
	}

	public static void ExecuteOnDamagedEnterContent (string contentName, DamageInfo damagedInfo)
	{
		if (onDamagedContentMap.ContainsKey(contentName))
			onDamagedContentMap [contentName].Invoke (damagedInfo);
	}
	public static void ExecuteOnDamagedUpdateContent (string contentName, DamageInfo damagedInfo)
	{
		if (onDamagedContentMap.ContainsKey(contentName))
			onDamagedContentMap [contentName].Invoke (damagedInfo);
	}
	public static void ExecuteOnDamagedExitContent (string contentName, DamageInfo damagedInfo)
	{
		if (onDamagedContentMap.ContainsKey(contentName))
			onDamagedContentMap [contentName].Invoke (damagedInfo);
	}
	//	Attack or Damaged OnEnter Contnet Region
	public static void VibrateCamera (Actor actor, SkillInfo usedSkill)
	{
		switch (usedSkill.vibrateAmount) {
		case CameraVibrateAmount.WEAK:
			serviceInstance.VibrateCameraByAttack (0.2f, 4f, 0.2f);
			break;
		case CameraVibrateAmount.MIDDLE:
			serviceInstance.VibrateCameraByAttack (0.4f, 4f, 0.2f);
			break;
		case CameraVibrateAmount.STRONG:
			serviceInstance.VibrateCameraByAttack (0.7f, 5f, 0.2f);
			break;
		default:

			break;
		}
	}
	public static void VibrateCamera (DamageInfo damageInfo)
	{
		VibrateCamera (damageInfo.skillInfo.vibrateAmount);
	}
	public static void VibrateCamera (CameraVibrateAmount amount)
	{
		switch (amount) {
		case CameraVibrateAmount.WEAK:
			serviceInstance.VibrateCameraByAttack (0.1f, 10f, 0.1f);
			break;
		case CameraVibrateAmount.MIDDLE:
			serviceInstance.VibrateCameraByAttack (0.25f, 10f, 0.1f);
			break;
		case CameraVibrateAmount.STRONG:
			serviceInstance.VibrateCameraByAttack (0.4f, 10f, 0.1f);
			break;
		default:

			break;
		}
	}
	public static void TeleportConsiderCollider (Actor actor, SkillInfo usedSkill)
	{
		var dir = usedSkill.teleportDir;
		dir.x *= actor.GetLookDirection ();
		actor.bodyCollider.bounds.Expand (Vector3.left * 0.1f);
		var hits = actor.rigid.SweepTestAll (dir, usedSkill.teleportAmount);
		if (hits.Length != 0) 
		{
			var newPos = (hits[0].point - dir.x * (hits[0].collider.bounds.extents.x + actor.bodyCollider.bounds.extents.x) * Vector3.right);
			newPos.y -= actor.bodyCollider.bounds.extents.y;
			actor.transform.position = newPos;
		} 
		else {
			actor.transform.position = actor.transform.position + dir * usedSkill.teleportAmount;
		}
		actor.bodyCollider.bounds.Expand (Vector3.right * 0.1f);
	}
	public static void TeleportWithAttack (Actor actor, SkillInfo usedSkill)
	{
		var dir = usedSkill.teleportDir;
		dir.x *= actor.GetLookDirection ();
		var cols = Physics.OverlapBox (actor.bodyCollider.bounds.center + dir * usedSkill.teleportAmount * 0.5f, 
			new Vector3 (usedSkill.teleportAmount * 0.5f, actor.bodyCollider.bounds.extents.y),
			Quaternion.identity,
			1 << LayerMask.NameToLayer ("MapCollider"));
		var dis = 0f;
		for (int i = 0; i < cols.Length; i++)
		{
			if (cols [i].tag == "Ground") {
				continue;
			}
			var newPos = new Vector3 (
				cols [i].bounds.center.x - dir.x * (cols [i].bounds.extents.x + actor.bodyCollider.bounds.extents.x),
				actor.transform.position.y,
				actor.transform.position.z);
			dis = newPos.x - actor.transform.position.x;
			actor.transform.position = newPos;
			Debug.Log ("Teleport");
			return;
		}
		dis = actor.transform.position.x + dir.x * usedSkill.teleportAmount - actor.transform.position.x;
		actor.transform.position = actor.transform.position + dir * usedSkill.teleportAmount;
	}
	public static void StopVelocityX (Actor actor, SkillInfo usedSkill)
	{
		var vel = actor.rigid.velocity;
		vel.x = 0;
		actor.rigid.velocity = vel;
	}
	public static void StopVelocityY (Actor actor, SkillInfo usedSkill)
	{
		var vel = actor.rigid.velocity;
		vel.y = 0;
		actor.rigid.velocity = vel;
	}
	public static void StopVelocityZ (Actor actor, SkillInfo usedSkill)
	{
		var vel = actor.rigid.velocity;
		vel.z = 0;
		actor.rigid.velocity = vel;
	}
	public static void StopVelocity (Actor actor, SkillInfo usedSkill)
	{
		actor.rigid.velocity = Vector3.zero;
	}


	// State OnEnter Content Region
	public static void ActiveSuperArmor(Actor actor, SkillInfo usedSkill)
	{
		actor.actorInfo.isSuperarmor = true;
	}
	public static void LaunchVictim (DamageInfo damageInfo)
	{
		var prev = damageInfo.victim.rigid.velocity;
		prev.y = 0f;
		prev.x = Mathf.Sign (prev.x);
		damageInfo.victim.rigid.velocity = prev;
		var dirX = (damageInfo.victim.transform.position - damageInfo.attacker.transform.position).x;
		dirX = Mathf.Sign (dirX);
		damageInfo.victim.rigid.AddForce (Vector3.up * damageInfo.skillInfo.launchAmount + dirX * Vector3.right * damageInfo.skillInfo.smashAmount, ForceMode.Impulse);
		damageInfo.victim.SetLookDirection (dirX);
	}
	public static void SmashVictim (DamageInfo damageInfo)
	{
		damageInfo.victim.rigid.velocity = Vector3.zero;
		var dirX = (damageInfo.victim.transform.position - damageInfo.attacker.transform.position).x;
		damageInfo.victim.rigid.AddForce ((Vector3.right * Mathf.Sign(dirX) + Vector3.up * 0.3f).normalized * damageInfo.skillInfo.smashAmount, ForceMode.Impulse);
		damageInfo.victim.SetLookDirection (Mathf.Sign(dirX));
	}
	public static void SmashForward (DamageInfo damageInfo)
	{
		damageInfo.victim.rigid.velocity = Vector3.zero;
		var dirX = damageInfo.attacker.lookDirection;
		damageInfo.victim.rigid.AddForce ((Vector3.right * Mathf.Sign(dirX) + Vector3.up * 0.3f).normalized * damageInfo.skillInfo.smashAmount, ForceMode.Impulse);
		damageInfo.victim.SetLookDirection (Mathf.Sign(dirX));
	}
	public static void SmashBackward (DamageInfo damageInfo)
	{
		damageInfo.victim.rigid.velocity = Vector3.zero;
		var dirX = -damageInfo.attacker.lookDirection;
		damageInfo.victim.rigid.AddForce ((Vector3.right * Mathf.Sign(dirX) + Vector3.up * 0.3f).normalized * damageInfo.skillInfo.smashAmount, ForceMode.Impulse);
		damageInfo.victim.SetLookDirection (Mathf.Sign(dirX));
	}
	public static void DownBlow (DamageInfo damageInfo)
	{
		var prev = damageInfo.victim.rigid.velocity;
		prev.y = 0f;
		prev.x = Mathf.Sign (prev.x);
		damageInfo.victim.rigid.velocity = prev;
		damageInfo.victim.rigid.AddForce (Vector3.down * damageInfo.skillInfo.launchAmount, ForceMode.Impulse);
	}
	public static void SlowMotion (DamageInfo damageInfo)
	{
		serviceInstance.SlowMotionCamera (0.1f, 0.05f);
	}
	//	End	Region

	// State OnUpdate Content Region
	public static void ForwardDash(Actor actor, SkillInfo usedSkill)
	{
		actor.rigid.AddForce (Vector3.right * actor.GetLookDirection () * usedSkill.dashAmount, ForceMode.Impulse);
	}
	public static void BackDash(Actor actor, SkillInfo usedSkill)
	{
		var prev = actor.rigid.velocity;
		prev.x = -actor.GetLookDirection () * usedSkill.dashAmount;
		actor.rigid.velocity = prev;
	}
	//	End Region

	// State OnExit Content Region
	public static void ReleaseSuperArmor(Actor actor, SkillInfo usedSkill)
	{
		actor.actorInfo.isSuperarmor = false;
	}
	// 	End Region

}
