﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[System.Serializable]
public class FallState : ActionState {
	Vector3[] threePoint = new Vector3[3];
	public float dir;

	public void EditStateInfo (string animName)
	{
		stateInfo.animName = animName;
	}

	public override bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		if (targetActor.actorInfo.isGrounded)
			return false;
		if (fromState == this)
			return false;
		if (fromState == fsm.GetState<DamagedState>())
			return false;
		if (targetActor.rigid.velocity.y >= 0f)
			return false;
		return true;
	}
	public override bool CommonCheckExit ()
	{
		if (targetActor.actorInfo.isGrounded)
			return true;
		return false;
	}

	public override void CommonEnter ()
	{
		targetActor.SetMoveable (true);
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, false, 0f);
	}
	public override void CommonExit ()
	{
		targetActor.SetMoveable (true);
		base.CommonExit ();
	}
	public override void CommonUpdate ()
	{
		if (Input.GetKey (PlayerInputManager.keyMap ["LEFT"].input_key) && Input.GetKey (PlayerInputManager.keyMap ["RIGHT"].input_key)) 
		{
			if (!targetActor.GetMoveable ())
				return;
			var prevSpeed = targetActor.rigid.velocity;
			prevSpeed.x = 0f;
			targetActor.rigid.velocity = prevSpeed;
			targetActor.SetMoveable (false);
		}
		else if (PlayerInputManager.pressedMoveInput) {
			targetActor.SetMoveable (true);
			var dir = targetActor.lookDirection;
			var prevSpeed = targetActor.rigid.velocity;
			threePoint[0] = targetActor.bodyCollider.bounds.center;
			threePoint[1] = threePoint[0];
			threePoint[1].y = targetActor.bodyCollider.bounds.min.y;
			threePoint[2] = threePoint[0];
			threePoint[2].y = targetActor.bodyCollider.bounds.max.y;

			for (int t = 0; t < 3; t++) {
				var objs = Physics.RaycastAll (threePoint[t], Vector3.right * dir, targetActor.bodyCollider.bounds.extents.x * 1.01f);
				if (objs.Length != 0) {
					for (int i = 0; i < objs.Length; i++)
					{
						var obj = objs [i];
						if (obj.collider.gameObject.layer == LayerMask.NameToLayer ("MapCollider")) {
							prevSpeed.x = 0;
							targetActor.rigid.velocity = prevSpeed;
							return;
						}
					}
				}
			}
			prevSpeed.x = dir * targetActor.GetMoveSpeed();
			targetActor.rigid.velocity = prevSpeed;
		}
		else {
			var prevSpeed = targetActor.rigid.velocity;
			prevSpeed.x = 0f;
			targetActor.rigid.velocity = prevSpeed;
		}
	}
}