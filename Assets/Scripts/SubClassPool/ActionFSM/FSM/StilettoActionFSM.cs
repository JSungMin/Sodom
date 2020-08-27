using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public class StilettoActionFSM : ActorActionFSM {

	public IdleState idleState;
	public WalkState walkState;
	public AttackState attackState;
	public StunState stunState;
	public DamagedState damagedState;
	public DeadState deadState;
	public FallState fallState;
	public JumpState jumpState;

	#region implemented abstract members of ActorActionFSM

	protected override void BuildStates ()
	{
		idleState.Initialize(this);
		idleState.BuildState (
			idleState.CheckEnterByPlayer, 
			idleState.AlwaysReturnTrue,
			idleState.CommonEnter,
			idleState.DoNothing,
			idleState.CommonExit
		);
		walkState.Initialize (this);
		walkState.BuildState (
			walkState.CommonCheckEnter,
			walkState.CommonCheckExit
		);
		attackState.Initialize (this);
		attackState.BuildState (
			attackState.CheckEnterByEnemy,
			attackState.CommonCheckExit,
			attackState.EnterByEnemy,
			attackState.CommonUpdate,
			attackState.CommonExit,
			delegate(object infoParam) {
				attackState.EditStateInfo((SkillInfo)infoParam);
			}
		);
		damagedState.Initialize (this);
		damagedState.BuildState (
			damagedState.CommonCheckEnter,
			damagedState.CommonCheckExit,
			damagedState.CommonEnter,
			damagedState.CommonUpdate,
			damagedState.CommonExit,
			delegate(object infoParam) {
				damagedState.EditStateInfo ((DamageInfo)infoParam);
			}
		);
		damagedState.knockoutTime = 4f;
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
		stunState.knockoutTime = 3f;
		deadState.Initialize (this);
		deadState.BuildState (
			deadState.CommonCheckEnter,
			deadState.CommonCheckExit,
			deadState.CommonEnter,
			deadState.CommonUpdate,
			deadState.CommonExit
		);
		fallState.Initialize (this);
		fallState.BuildState (null);
		fallState.EditStateInfo ("Dash_Back");
		jumpState.Initialize (this);
		jumpState.BuildState (null);

		nowState = idleState;
	}

	protected override void BuildStatesPool ()
	{
		
	}

	protected override void BuildTransitionGraph ()
	{
		AddStateToTransition<IdleState, FallState>();
		AddStateToTransition<WalkState, FallState>();
		AddStateToTransition<WalkState, IdleState>();
		AddStateToTransition<AttackState, IdleState>();
		AddStateToTransition<AttackState, FallState>();
		AddStateToTransition<AttackState, DeadState>();
		AddStateToTransition<DamagedState, IdleState>();
		AddStateToTransition<StunState, IdleState>();
		AddStateToTransition<StunState, DeadState>();
		AddStateToTransition<FallState, IdleState>();
		AddStateToTransition<JumpState, IdleState>();
		AddStateToTransition<JumpState, FallState>();
	}

	public override void InitFSMStates ()
	{
		targetActor = GetComponent<Actor> ();
		if (targetActor.animatorType == AnimationType.FRAME)
			targetActor = GetComponent<EnemyFrameBase> ();
		else if (targetActor.animatorType == AnimationType.SPINE)
			targetActor = GetComponent<EnemySpineBase> ();
		//	위에서 구체화한 Build Process가 진행됨
		base.InitFSMStates ();
		//if (null != targetActor.GetComponent<Rigidbody>())
		//	runState.targetRigid = targetActor.GetComponent<Rigidbody> ();
	}
	#endregion
}
