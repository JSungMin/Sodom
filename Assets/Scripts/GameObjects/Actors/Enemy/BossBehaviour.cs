using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

namespace BossSpace
{
	public class BossBehaviour
	{
		public static IEnumerator starter;
		public static IEnumerator init;
		public static IEnumerator update;
		public static IEnumerator reset;

		public static EnemySpineBase targetActor;
		public static EnemySpineBase actorInstance
		{
			get
			{
				return targetActor = GameObject.FindObjectOfType<EnemySpineBase> ();
			}
		}
		public static T GetBoss<T> () where T : Actor
		{
			if (null == actorInstance)
				return null;
			return targetActor as T;
		}
		protected static IEnumerator IStartBehaviour ()
		{
			yield return init;
			yield return update;
			yield return reset;
		}
	}
}
