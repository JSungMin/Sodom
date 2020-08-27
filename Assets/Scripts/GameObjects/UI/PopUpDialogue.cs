using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using EventArgumentNamespace;

[RequireComponent(typeof(Animator))]
public class PopUpDialogue : MonoBehaviour {
	public Transform followTarget;
	[HideInInspector]
	public Animator animator;
	//	UI Property Block
	[HideInInspector]
	public Text text;
	[HideInInspector]
	public CanvasRenderer canvasRenderer;
	[HideInInspector]
	public RectTransform rectTrans;

	public string openAnimName = "Open";
	public string closeAnimName = "Close";

	public bool isBusy = false;

	private event EventHandler<FrameAnimationLoopArg> RaiseAnimationEnd;

	void Awake()
	{
		animator = GetComponent<Animator> ();
		RaiseAnimationEnd += HandleAnimationEnd;
	}

	void OnEnable()
	{
		rectTrans = GetComponent<RectTransform> ();
		canvasRenderer = GetComponent<CanvasRenderer> ();
	}
	
	public void PlayUIAnimation()
	{
		animator.Play (openAnimName);
	}
	public void StopUIAnimation()
	{
		animator.Play (closeAnimName);
	}
	private void HandleAnimationEnd (object sender, FrameAnimationLoopArg arg)
	{

	}
}
