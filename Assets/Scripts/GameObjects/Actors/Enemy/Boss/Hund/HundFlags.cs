using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace BossSpace.HundSpace.SubInfo
{
	[System.Serializable]
	public class SpeedInfo
	{
		public float madRunSpeed = 12.5f, 
					 walkSpeed = 12.5f * 0.3f, 
					 chargeSpeed = 12.5f * 0.6f;
	}
	[System.Serializable]
	public class MadnessInfo
	{
		public bool 	isMedness = false;
		public float 	medTimer = 0f;
		public float 	medDuration = 5f, medTime = 5f;

		public void IncMednessTimer (float amount)
		{
			medTimer += amount;
		}
		public bool CheckTimer ()
		{
			return medTimer > medTime;
		}
		public void Reset ()
		{
			isMedness = false;
			medTimer = 0f;
		}
	}
}