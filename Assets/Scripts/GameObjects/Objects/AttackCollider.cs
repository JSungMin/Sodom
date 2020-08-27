using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public class AttackCollider : MonoBehaviour {
	public Actor attacker;
	[SerializeField]
	public SkillInfo skillInfo;
	public Dictionary<string, Actor> victimDictionary = new  Dictionary<string, Actor>();
	public Player player;

	public List<AttackColliderSub> subColliders = new List<AttackColliderSub> ();

	public bool isInitialize = false;

	public delegate void onAttackSuccess();
	public onAttackSuccess OnAttackSuccess;

	public void Start ()
	{
		player = GameObject.FindObjectOfType<Player> ();
		for (int i = 0; i < subColliders.Count; i++) {
			subColliders [i].Initialize (this);
		}
	}

	public void Initialize (Actor a, SkillInfo sInfo)
	{
		attacker = a;
		skillInfo = sInfo;
		if (sInfo.animName == "Tackle")
		{
			Debug.Log("태클 Init");
		}
		victimDictionary.Clear ();
		isInitialize = true;
	}

	public void Release (Actor a)
	{
		isInitialize = false;
		//if (!victimDictionary.ContainsKey(player.name))
		//	victimDictionary.Add (player.gameObject.name, player);
        if (!victimDictionary.ContainsKey(a.gameObject.name))
            victimDictionary.Add(a.gameObject.name, a);
    }
}
