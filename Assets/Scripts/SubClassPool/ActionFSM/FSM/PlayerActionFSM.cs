using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
[RequireComponent(typeof(Player))]
public class PlayerActionFSM : ActorActionFSM {

	public IdleState idleState;
	public WalkStartState walkStartState;
	public WalkState walkState;
	public WalkEndState walkEndState;
	public SitState sitState;
	public StandState standState;
	public AttackState attackState;
	public StunState stunState;
	public CounterState counterState;
	public DamagedState damagedState;
	public DownState 	downState;			//	벽꿍과 같은 상황 이후 넘어지는 State
	public WakeUpState	wakeUpState;		//	Blow Skill에 의한 Damage를 입을시 실행
	public DeadState deadState;
	public FallState fallState;
	public InteractState interactState;
	public JumpState jumpState;
	public RollingState rollingState;

	#region implemented abstract members of ActorActionFSM

	protected override void BuildStates ()
	{
		idleState.Initialize (this);
		idleState.BuildState (
			idleState.CheckEnterByPlayer, 
			idleState.AlwaysReturnTrue,
			idleState.CommonEnter,
			idleState.DoNothing,
			idleState.CommonExit
		);
		walkStartState.Initialize (this);
		walkStartState.BuildState (
			walkStartState.CheckEnterByPlayer,
			walkStartState.CheckExitByPlayer
		);
		walkState.Initialize (this);
		walkState.BuildState (
			walkState.CheckEnterByPlayer,
			walkState.CheckExitByPlayer
		);
		walkEndState.Initialize (this);
		walkEndState.BuildState (
			walkEndState.CheckEnterByPlayer,
			walkEndState.CheckExitByPlayer
		);
		sitState.Initialize (this);
		sitState.BuildState (null, null, null, null, null, null);
		standState.Initialize (this);
		standState.BuildState (null, null, null, null, null, null);
		attackState.Initialize (this);
		attackState.BuildState (
			attackState.CheckEnterByPlayer,
			attackState.CheckExitByPlayer,
			attackState.EnterByPlayer,
			attackState.CommonUpdate,
			attackState.CommonExit,
			delegate(object infoParam) {
				attackState.EditStateInfo((SkillInfo)infoParam);
			}
		);
		stunState.Initialize (this);
		stunState.BuildState (
			stunState.CommonCheckEnter,
			stunState.CommonCheckExit,
			stunState.CommonEnter,
			stunState.CommonUpdate,
			stunState.CommonExit,
			delegate(object infoParam) {
				stunState.EditStateInfo ((DamageInfo)infoParam);
			}
		);
		counterState.Initialize (this);
		counterState.BuildState (
			counterState.CommonCheckEnter,
			counterState.CommonCheckExit,
			counterState.CommonEnter,
			counterState.CommonUpdate,
			counterState.CommonExit,
			delegate(object infoParam) {
				counterState.EditStateInfo ((DamageInfo)infoParam);
			}
		);
		damagedState.Initialize (this);
		damagedState.BuildState (
			damagedState.CheckEnterByPlayer,
			damagedState.CheckExitByPlayer,
			damagedState.EnterByPlayer,
			damagedState.UpdateByPlayer,
			damagedState.CommonExit,
			delegate(object infoParam) {
				damagedState.EditStateInfo ((DamageInfo)infoParam);
			}
		);
		downState.Initialize (this);
		downState.BuildState (
			downState.CommonCheckEnter,
			downState.CommonCheckExit,
			downState.CommonEnter,
			downState.CommonUpdate,
			downState.CommonExit
		);
		wakeUpState.Initialize (this);
		wakeUpState.BuildState (
			wakeUpState.CommonCheckEnter,
			wakeUpState.CommonCheckExit,
			wakeUpState.CommonEnter,
			wakeUpState.CommonUpdate,
			wakeUpState.CommonExit
		);
		deadState.Initialize (this);
		deadState.BuildState (
			deadState.CommonCheckEnter, 
			deadState.CommonCheckExit,
			deadState.CommonEnter, 
			deadState.CommonUpdate, 
			deadState.CommonExit
		);
		fallState.Initialize (this);
		fallState.BuildState (null, null, null, null, null, null);
		interactState = new InteractState (this);
		interactState.BuildState (delegate(object infoParam) {
			interactState.interactWith = ((InteractInfo)infoParam).interactWith;
		});
		jumpState.Initialize (this);
		jumpState.BuildState (
			jumpState.CommonCheckEnter,
			jumpState.CommonCheckExit,
			jumpState.CommonEnter,
			jumpState.UpdateByPlayer,
			jumpState.CommonExit
		);
		rollingState.Initialize (this);
		rollingState.BuildState (null, null, null, null, null, null);

		nowState = idleState;
	}

	protected override void BuildStatesPool ()
	{
		/*for (int i = 0; i < stateInfoList.Count; i++)
		{
			var stateInfo = stateInfoList [i];
			if (stateMap.ContainsKey(stateInfo.stateName))
				stateMap [stateInfo.stateName].stateInfo = stateInfo;
		}*/
	}
	protected override void BuildTransitionGraph ()
	{
		AddStateToTransition<IdleState, FallState>();
		AddStateToTransition<WalkStartState, WalkState>();
		AddStateToTransition<WalkStartState, FallState>();
		AddStateToTransition<WalkStartState, IdleState>();
		AddStateToTransition<WalkState, WalkEndState>();
		AddStateToTransition<WalkState, FallState>();
		AddStateToTransition<WalkEndState, IdleState>();
		AddStateToTransition<WalkEndState, FallState>();
		AddStateToTransition<SitState, StandState>();
		AddStateToTransition<SitState, FallState>();
		AddStateToTransition<SitState, JumpState>();
		AddStateToTransition<StandState, IdleState>();
		AddStateToTransition<StandState, FallState>();
		AddStateToTransition<AttackState, IdleState>();
		AddStateToTransition<AttackState, FallState>();
		AddStateToTransition<CounterState, IdleState>();
		AddStateToTransition<DamagedState, WakeUpState>();
		AddStateToTransition<DamagedState, IdleState>();
		AddStateToTransition<StunState, IdleState>();
		AddStateToTransition<DownState, WakeUpState>();
		AddStateToTransition<WakeUpState, IdleState>();
		AddStateToTransition<FallState, IdleState>();
		AddStateToTransition<JumpState, IdleState>();
		AddStateToTransition<JumpState, FallState>();
		AddStateToTransition<RollingState, IdleState>();
		AddStateToTransition<RollingState, FallState>();
	}

	public override void InitFSMStates ()
	{
		targetActor = GetComponent<Player> ();
		//	위에서 구체화한 Build Process가 진행됨
		base.InitFSMStates ();
	}

	#endregion
}
