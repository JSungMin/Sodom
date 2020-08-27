using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using EventArgumentNamespace;

public class SpineBaseActor : Actor {
	public SkeletonAnimation animator;
	public string[] curAnimName;
	public int trackIndex = 0;

	private event EventHandler<SpineAnimationLoopArg> RaiseAnimationEnd;

	public override void ActorInit ()
	{
		animator = GetComponent<SkeletonAnimation> ();
		animator.state.Complete += SpineAnimationEndHandler;
		base.ActorInit ();
	}
	public void SpineAnimationEndHandler (Spine.TrackEntry trackEntry)
	{
		if (curAnimName[trackEntry.TrackIndex] == trackEntry.Animation.Name)
		{
			var handler = RaiseAnimationEnd;
			if (null != handler) {
				handler (this, new SpineAnimationLoopArg(animator, trackEntry.Animation.Name));
			}
		}
	}
	public void Add_AnimationEndSubscriber (EventHandler<SpineAnimationLoopArg> arg)
	{
		RaiseAnimationEnd += arg;
	}
	public void Remove_AnimationEndSubscriber (EventHandler<SpineAnimationLoopArg> arg)
	{
		RaiseAnimationEnd -= arg;
	}
	public void PlayAnimation (int index, string name, bool useLoop, float timeScale)
	{
		trackIndex = index;
		if (curAnimName [index] == name)
			return;
		else {
			animator.state.SetAnimation (index, name, useLoop).TimeScale = timeScale;
			curAnimName [index] = name;
		}
	}
	public void PlayAnimation (int index, string name, bool useLoop, float timeScale, bool overlap)
	{
		trackIndex = index;
		if (curAnimName [index] == name && !overlap)
			return;
		else {
			animator.state.SetAnimation (index, name, useLoop).TimeScale = timeScale;
			curAnimName [index] = name;
		}
	}
}
