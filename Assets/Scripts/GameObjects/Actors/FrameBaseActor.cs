using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventArgumentNamespace;

public class FrameBaseActor : Actor {
	[HideInInspector]
	public Animator animator;
	public Animator effectAnimator;
	public string curAnimName;

    [HideInInspector]
	public int nowCycleCount = 0;
    [HideInInspector]
    public int prevCycleCount = 0;

	private event EventHandler<FrameAnimationLoopArg> RaiseAnimationEnd;

	public override void ActorInit()
	{
		animator = GetComponent<Animator> ();
		base.ActorInit ();
		Debug.Log (actorInfo.actor_name + "_Frame Actor Init");
	}
	public void PlayAnimation (int index, string name, bool useLoop, float timeOffset)
	{
		ResetCount ();
		if (!useLoop)
			animator.Play (name, index, timeOffset);
		else {
			animator.Play (name, index);
		}
		curAnimName = name;
	}
	public void PlayAnimation (int index, string name, bool useLoop, float timeOffset, bool overlap = false)
	{
		ResetCount ();
		if (overlap) 
		{
			animator.Play (name, index, timeOffset);
		} 
		else 
		{
			if (!useLoop)
				animator.Play (name, index, timeOffset);
			else 
			{
				animator.Play (name, index);
			}
		}
		curAnimName = name;
	}
	public void PlayEffectAnimation (string name)
	{
		if (null != effectAnimator)
			effectAnimator.Play (name);
	}
    public void StopEffectAnimation ()
    {
        if (null != effectAnimator)
            effectAnimator.StopPlayback();
    }
	public void ResetCount ()
	{
		nowCycleCount = 0;
		prevCycleCount = 0;
	}
	public float GetAnimationPlayTime()
	{
		return animPlaybackTime;
	}

	public void Add_AnimationEndSubscriber (EventHandler<FrameAnimationLoopArg> arg)
	{
		RaiseAnimationEnd += arg;
	}
	public void Remove_AnimationEndSubscriber (EventHandler<FrameAnimationLoopArg> arg)
	{
		RaiseAnimationEnd -= arg;
	}
	public virtual void Update()
	{ 
		var animationState = animator.GetCurrentAnimatorStateInfo (0);
		if (animationState.length == 0f)
			animPlaybackTime = 0f;
		else
			animPlaybackTime = animationState.normalizedTime;

		nowCycleCount = (int)animPlaybackTime;

		if (nowCycleCount > prevCycleCount)
		{
			if (null != fsm.nowState) {
				if (animationState.IsName(fsm.nowState.stateInfo.animName))
				{
					//Debug.Log("ANIM END : " + fsm.nowState.stateInfo.animName);
					var handler = RaiseAnimationEnd;
					if (null != handler) {
						handler (this, new FrameAnimationLoopArg(animator, curAnimName));
					}
				}
			}
		}
		prevCycleCount = nowCycleCount;
	}
}
