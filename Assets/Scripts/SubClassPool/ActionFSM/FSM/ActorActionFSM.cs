using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public abstract class ActorActionFSM : MonoBehaviour {
	public GameSystemService serviceInstance;
	public Actor targetActor;
	protected FrameBaseActor frameActor;
	protected SpineBaseActor spineActor;
	public ActionState nowState;
	public ActionState prevState;

	//protected List<ActionState> stateList = new List<ActionState>();

	public List<ActionStateInfo> stateInfoList = new List<ActionStateInfo> ();
	public Dictionary<Type, ActionState> stateMap = new Dictionary<Type, ActionState>();
	protected List<ActionState> stateChainBuffer = null;
	protected int stateChainIndex = -1;
	public string chainName = "";
	public bool nowStateEnd = false;
	public List<ActionState> ActiveStateChain
	{
		get{
			return stateChainBuffer;
		}
		set{
			stateChainBuffer = value;
			stateChainIndex = 0;
		}
	} 
	public virtual void Start()
	{
		serviceInstance = GameSystemService.Instance;
	}

	public virtual void FixedUpdate()
	{
		if (null != nowState)
			nowState.UpdateAction();
	}

	//	여기서 stateList를 채워 넣음
	public virtual void InitFSMStates ()
	{
		//	Init States
		BuildStates ();
		//	Pooling States
		BuildStatesPool ();
		//	Set Transition Graph
		BuildTransitionGraph ();
		//	Set Animation End Listener
		if (targetActor.animatorType == AnimationType.FRAME) {
			frameActor = targetActor as FrameBaseActor;
			frameActor.Add_AnimationEndSubscriber (OnStateAnimationEnd);
		} else {
			spineActor = targetActor as SpineBaseActor;
			spineActor.Add_AnimationEndSubscriber (OnStateAnimationEnd);

		}
	}

	public T GetSpecifiedActor<T> () where T : Actor 
	{
		return targetActor as T;
	}

	protected abstract void BuildStates();
	protected abstract void BuildStatesPool();
	protected abstract void BuildTransitionGraph ();


	protected virtual void SetDefaultState (ActionState defaultState)
	{
		nowState = defaultState;
		TryTransferAction<ActionState>(nowState.stateInfo.stateName);
	}
	public void AddStateToMap(ActionState state)
	{
		stateMap[state.GetType()] = state;
	}
	//	T is To State
	public void AddStateToTransition<T1,T2>() where T1 : ActionState where T2 : ActionState
	{
		stateMap[typeof(T1)].transableNodeList.Add(typeof(T2));
	}
	public virtual void OnStateAnimationEnd (object sender, EventArgumentNamespace.FrameAnimationLoopArg arg)
	{
		nowState.isAnimationEnd = true;
	}
	public virtual void OnStateAnimationEnd (object sender, EventArgumentNamespace.SpineAnimationLoopArg arg)
	{
		nowState.isAnimationEnd = true;
	}
	public virtual void BreakStateChain()
	{
        Debug.Log("Break Chain : " + chainName);
		if (null != targetActor.StateChainBroken)
			targetActor.StateChainBroken(chainName);
		chainName = "";
		stateChainIndex = -1;
		stateChainBuffer = null;
	}
    public virtual void StartStateChain(string chainName, List<ActionState> states)
    {
        Debug.Log("START CHAIN FROM: " + this.chainName + " TO : " + chainName);
        this.chainName = chainName;
        ActiveStateChain = states;
        UpdateStateChain();
    }
   
    public virtual bool UpdateStateChain()
    {
        if (stateChainIndex < 0)
            return false;
        if (stateChainBuffer.Count <= stateChainIndex)
            return false;
        if (TryTransferAction(stateChainBuffer[stateChainIndex]))
        {
            stateChainIndex++;
        }
        else
        {
            Debug.LogError("CHAIN FAIL : " + chainName);
            return false;
        }
        return true;
    }
    public void SetStateChain (string chainName, List<ActionState> states)
    {
        this.chainName = chainName;
        ActiveStateChain = states;
    }
	public bool IsChainActivated()
	{
		return (stateChainIndex == -1) ? false : true;
	}
	
	public virtual void OnStateEnd (ActionState eState)
	{
        if (IsChainActivated())
        {
            if (eState != stateChainBuffer[stateChainIndex - 1])
            {
                BreakStateChain();
                return;
            }
            bool updateResult = UpdateStateChain();

            if (updateResult)
            {
                eState.isStateExited = true;
            }
            else
            {
                BreakStateChain();
            }
        }
		for (int i = 0; i < eState.transableNodeList.Count; i++)
		{
			if (TryTransferAction (stateMap[eState.transableNodeList[i]])) {
				eState.isStateExited = true;
				break;
			}
		}
	}
	public T GetState<T> () where T : ActionState
	{
		if (stateMap.ContainsKey(typeof(T))) 
		{
				return (T)stateMap [typeof(T)];
		}
		Debug.LogError ("NO STATE : " + typeof(T).ToString());
		return null;
		/*return stateList.Find (delegate(ActionState obj) {
			if (stateName == obj.stateInfo.stateName)
				return true;	
			return false;
		});*/
	}

	public T GetSpecifiedFSM<T> () where T : ActorActionFSM
	{
		return (T)this;
	}

	//	exceptionHandler는 newState가 시작되기전 실행될 함수다.
	private bool SwapState<T>(T newState, object infoParam = null) where T : ActionState
	{
		if (!newState.checkEnter(nowState, infoParam))
			return false;
		if (nowState.checkExit ())
			nowState.StopAction ();
		prevState = nowState;
		if (null != newState.OnStateSwap)
			newState.OnStateSwap (infoParam);
		nowState = newState;
		if (prevState != nowState && targetActor.tag != "Player")
			Debug.Log(targetActor.actorInfo.actor_name + " Transfer Action FROM  : " + prevState.stateInfo.stateName + " To : " + nowState.stateInfo.stateName);
		nowState.StartAction ();
		return true;
	}

	public bool TryTransferAction<T> (object infoParam = null) where T : ActionState
	{
		if (!stateMap.ContainsKey (typeof(T))) {
			Debug.LogError ("No State : " + typeof(T).FullName + " FROM : " + nowState.stateInfo.stateName);
			return false;
		}
		T state = stateMap [typeof(T)] as T;
        return SwapState<T> (state, infoParam);
	}
    public bool TryTransferAction (ActionState newState, object infoParam = null)
    {
        if (!stateMap.ContainsKey(newState.GetType()))
        {
            Debug.LogError("No State : " + newState.GetType().Name+ " FROM : " + nowState.stateInfo.stateName);
            return false;
        }
        return SwapState<ActionState>(newState, infoParam);
    }
}