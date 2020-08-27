using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

[Serializable]
public class ActionState
{
	public ActorActionFSM fsm;
	protected Actor targetActor;
	public ActionStateInfo stateInfo;
	public List<Type> transableNodeList;

	public bool isAnimationEnd = false;
	public bool isStateExited = false;

	public event EventHandler<EventArgumentNamespace.ActionStateEventArg>	RaiseEnterEvent;
	public event EventHandler<EventArgumentNamespace.ActionStateEventArg>	RaiseExitEvent;
	public event EventHandler<EventArgumentNamespace.FrameAnimationLoopArg>	RaiseAnimationEndEvent;

	public delegate bool CheckEnter(ActionState fromState, object infoParam);
	public delegate bool CheckExit ();
	public delegate void SwapBehaviour (object infoParam);
	public delegate void StateBehaviour ();
	public CheckEnter checkEnter;
	public CheckExit checkExit;
	public StateBehaviour OnEnter;
	public StateBehaviour OnUpdate;
	public StateBehaviour OnExit;
	public SwapBehaviour OnStateSwap;

	//	Constructor Set
	public ActionState ()
	{
		stateInfo = new ActionStateInfo ("","");
		//targetActor = fsm.targetActor;
		transableNodeList = new List<Type>();
	}
	public ActionState (ActorActionFSM fsm, ActionStateInfo sInfo)
	{
		this.fsm = fsm;
		stateInfo = sInfo;
		targetActor = fsm.targetActor;
		transableNodeList = new List<Type>();
	}
	public ActionState (ActorActionFSM fsm, ActionStateInfo sInfo, List<Type> nodeList)
	{
		this.fsm = fsm;
		stateInfo = sInfo;
		targetActor = fsm.targetActor;
		transableNodeList = nodeList;
	}
	public virtual void Initialize (ActorActionFSM fsm)
	{
		this.fsm = fsm;
		targetActor = fsm.targetActor;
		fsm.AddStateToMap(this);
	}

	//	Should have call after call Constructor
	public void BuildState (
		CheckEnter checkEnter = null, 
		CheckExit checkExit = null, 
		StateBehaviour enter = null,
		StateBehaviour update = null,
		StateBehaviour exit = null, 
		SwapBehaviour swap = null)
	{
		if (checkEnter == null)
			this.checkEnter = CommonCheckEnter;
		else
			this.checkEnter = checkEnter;
		if (checkExit == null)
			this.checkExit = CommonCheckExit;
		else
			this.checkExit = checkExit;
		if (enter == null)
			this.OnEnter = CommonEnter;
		else
			this.OnEnter = enter;
		if (update == null)
			this.OnUpdate = CommonUpdate;
		else
			this.OnUpdate = update;
		if (exit == null)
			this.OnExit = CommonExit;
		else
			this.OnExit = exit;
		this.OnStateSwap = swap;
	}
	public void BuildState (SwapBehaviour swap = null)
	{
		this.checkEnter = CommonCheckEnter;
		this.checkExit = CommonCheckExit;
		this.OnEnter = CommonEnter;
		this.OnUpdate = CommonUpdate;
		this.OnExit = CommonExit;
		this.OnStateSwap = swap;
	}

	public void AddOnEnterListener(EventHandler<EventArgumentNamespace.ActionStateEventArg> arg)
	{
		RaiseEnterEvent += arg;
	}
	public void AddOnExitListener (EventHandler<EventArgumentNamespace.ActionStateEventArg> arg){
		RaiseExitEvent += arg;
	}
	public void AddAnimationEndListener (EventHandler<EventArgumentNamespace.FrameAnimationLoopArg> arg){
		RaiseAnimationEndEvent += arg;
	}
	//	Exposed Func
	//public abstract bool CheckEnter(ActionState fromNode, object paramToCheck);
	//public abstract bool CheckExit();

	//	Animation이 끝난 상태인가
	public bool CheckAnimationEnd()
	{
		return isAnimationEnd;
	}
	//	기존 노드와 현재 노드가 중복되는가
	public bool CheckIsLoopNode(ActionState fromNode)
	{
		if (this == fromNode)
			return true;
		return false;
	}

	public virtual void PlayAnimation (int index, string name, bool useLoop, float timeFactor, bool overlap = false)
	{
		//	Play Animation
		switch(targetActor.animatorType)
		{
		case AnimationType.FRAME:
			var frameAnimator = targetActor as FrameBaseActor;
			frameAnimator.PlayAnimation (index, name, useLoop, timeFactor, overlap);
			break;
		case AnimationType.SPINE:
			var spineAnimator = targetActor as SpineBaseActor;
			spineAnimator.PlayAnimation (index, name, useLoop, timeFactor + 1f, overlap);
			break;
		}
	}
	public virtual void PlayEffectAnimation (string animName)
	{
		if (animName == "")
			return;
		switch(targetActor.animatorType)
		{
		case AnimationType.FRAME:
			var frameAnimator = targetActor as FrameBaseActor;
			frameAnimator.PlayEffectAnimation (animName);
			break;
		case AnimationType.SPINE:
			var spineAnimator = targetActor as SpineBaseActor;
			Debug.Log ("Play In Spine Skeleton");
			break;
		}
	}
    public virtual void StopEffectAnimation ()
    {
        switch (targetActor.animatorType)
        {
            case AnimationType.FRAME:
                var frameAnimator = targetActor as FrameBaseActor;
                frameAnimator.StopEffectAnimation();
                break;
            case AnimationType.SPINE:
                var spineAnimator = targetActor as SpineBaseActor;
                Debug.Log("Stop In Spine Skeleton");
                break;
        }
    }
    //	Called From ActorAction TryTransferAction
    public virtual void StartAction()
	{
		isStateExited = false;
		OnEnter ();
	}
	//	Called From ActorActionFSM Update
	public virtual void UpdateAction ()
	{
		if (isAnimationEnd)
			OnAnimationEnd ();
		if (!checkExit ()) {
			OnUpdate ();
		}
		else {
			if (!isStateExited) {
				OnExit ();
				fsm.OnStateEnd (this);
			}
		}
	}
	//	Called From ActorActionFSM TryTransferAction
	public virtual void StopAction()
	{
		OnExit ();
	}

	//	Inner Func
	/*protected virtual void OnEnter()
	{
		EventHandler<EventArgumentNamespace.ActionStateEventArg> handler = RaiseEnterEvent;
		if (null != handler)
			handler (this, new EventArgumentNamespace.ActionStateEventArg (this));
	}
	protected abstract void OnUpdate ();
	protected virtual void OnExit ()
	{
		EventHandler<EventArgumentNamespace.ActionStateEventArg> handler = RaiseExitEvent;
		if (null != handler)
			handler (this, new EventArgumentNamespace.ActionStateEventArg (this));
	}*/
	protected virtual void OnAnimationEnd()
	{
		EventHandler<EventArgumentNamespace.FrameAnimationLoopArg> handler = RaiseAnimationEndEvent;
		if (null != handler) {
			if (targetActor.animatorType == AnimationType.FRAME)
			{
				var frameActor = targetActor as EnemyFrameBase;
				handler (this, new EventArgumentNamespace.FrameAnimationLoopArg (frameActor.animator, frameActor.curAnimName));
			}
		}
	}

	public bool AlwaysReturnFalse ()
	{
		return false;
	}
	public bool AlwayReturnFalse (ActionState fromState, object infoParam)
	{
		return false;
	}
	public bool AlwaysReturnTrue ()
	{
		return true;
	}
	public bool AlwaysReturnTrue (ActionState fromState, object infoParam)
	{
		return true;
	}
	public void DoNothing()
	{
		
	}
	public virtual bool CommonCheckEnter (ActionState fromState, object infoParam)
	{
		return true;
	}
	public virtual bool CommonCheckExit ()
	{
		return false;
	}
	public virtual void CommonEnter ()
	{
		isAnimationEnd = false;
		PlayAnimation (stateInfo.animIndex, stateInfo.animName, true, 0f);
	}
	public virtual void CommonUpdate()
	{
		
	}
	public virtual void CommonExit ()
	{
		isAnimationEnd = false;
	}
}