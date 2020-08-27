using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
[RequireComponent(typeof(Animator))]
public class AttackDummy : MonoBehaviour {
	public GameSystemService serviceInstance;
	public Actor fromActor;
	private Animator animator;
	public SkillInfo skillInfo;

	public int nowCycleCount = 0;
	public int prevCycleCount = 0;

	public virtual void Update()
	{ 
		var animationState = animator.GetCurrentAnimatorStateInfo (0);
		nowCycleCount = (int)animationState.normalizedTime;

		if (nowCycleCount > prevCycleCount)
		{
			gameObject.SetActive (false);
		}
		prevCycleCount = nowCycleCount;
	}

	public void Init (Actor fActor, SkillInfo sInfo, float normalizedAnimTime)
	{
		serviceInstance = GameSystemService.Instance;
		fromActor = fActor;
		animator = GetComponent<Animator> ();
		skillInfo = sInfo;
		animator.Play (skillInfo.animName, 0, normalizedAnimTime);
		transform.localScale = fromActor.transform.localScale;
	}

	public void HandleAttackEvent (int colliderIndex)
	{
		List<Actor> victimList = new List<Actor> ();
		fromActor.attackColliderList [colliderIndex].Initialize (fromActor, fromActor.GetUsedSkill());
	}
}
