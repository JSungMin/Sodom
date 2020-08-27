using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public class HundActionFSM : ActorActionFSM {

	public IdleState idleState;
	public WalkState walkState;
	public AttackState attackState;
	public StunState stunState;
	public DamagedState damagedState;
	public DeadState deadState;
	public FallState fallState;
	public JumpState jumpState;

	//	Special State For HUND
	public HundEMPState empState;
    public HundEMPEndState empEndState;
	public HundChaseState chaseState;
	public HundTackleState tackleState;
	public HundTackleSubState tackleSubState;
	public HundStopState stopState;
	public HundBackStepState backStepState;
    public HundDarkRunState darkRunState;
    public HundDarkRunEndState darkRunEndState;
	public HundSuddenAttackEndState suddenAttackEndState;

	#region implemented abstract members of ActorActionFSM

	protected override void BuildStates ()
	{
		idleState.Initialize (this);
		idleState.BuildState (
			idleState.CheckEnterByHund, 
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
		damagedState.knockoutTime = 3f;
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
		stunState.knockoutTime = 2f;
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
		fallState.EditStateInfo ("WeaponIdle");
		jumpState.Initialize (this);
		jumpState.BuildState (null);
		//	EMP State BUILD
		empState.Initialize (this);
		empState.BuildState (
			empState.CommonCheckEnter,
			empState.CommonCheckExit,
			empState.CommonEnter,
			empState.CommonUpdate,
			empState.CommonExit
		);
        empEndState.Initialize(this);
        empEndState.BuildState(
            empEndState.CommonCheckEnter,
            empEndState.CommonCheckExit,
            empEndState.CommonEnter,
            empEndState.CommonUpdate,
            empEndState.CommonExit
        );
		backStepState.Initialize(this);
		backStepState.BuildState(
			backStepState.CommonCheckEnter,
			backStepState.CommonCheckExit,
			backStepState.CommonEnter,
			backStepState.CommonUpdate,
			backStepState.CommonExit
		);
		chaseState.Initialize (this);
		chaseState.BuildState (
			chaseState.CommonCheckEnter,
			chaseState.CommonCheckExit,
			chaseState.CommonEnter,
			chaseState.CommonUpdate,
			chaseState.CommonExit,
			delegate (object infoParam){
				chaseState.EditStateInfo((SkillInfo)infoParam);
			}
		);
		tackleState.Initialize(this);
		tackleState.BuildState (
			tackleState.CommonCheckEnter,
			tackleState.CommonCheckExit,
			tackleState.CommonEnter,
			tackleState.CommonUpdate,
			tackleState.CommonExit,
			delegate (object infoParam){
				tackleState.EditStateInfo((SkillInfo)infoParam);
			}
		);
		tackleSubState.Initialize(this);
		tackleSubState.BuildState(
			tackleSubState.CommonCheckEnter,
			tackleSubState.CommonCheckExit,
			tackleSubState.CommonEnter,
			tackleSubState.CommonUpdate,
			tackleSubState.CommonExit
		);
		stopState.Initialize(this);
		stopState.BuildState(
			stopState.CommonCheckEnter,
			stopState.CommonCheckExit,
			stopState.CommonEnter,
			stopState.CommonUpdate,
			stopState.CommonExit
		);
        darkRunState.Initialize(this);
        darkRunState.BuildState(
            darkRunState.CommonCheckEnter,
            darkRunState.CommonCheckExit,
            darkRunState.CommonEnter,
            darkRunState.CommonUpdate,
            darkRunState.CommonExit
        );
        darkRunEndState.Initialize(this);
        darkRunEndState.BuildState(
            darkRunEndState.CommonCheckEnter,
            darkRunEndState.CommonCheckExit,
            darkRunEndState.CommonEnter,
            darkRunEndState.CommonUpdate,
            darkRunEndState.CommonExit
        );
		suddenAttackEndState.Initialize(this);
		suddenAttackEndState.BuildState(
            suddenAttackEndState.CommonCheckEnter,
            suddenAttackEndState.CommonCheckExit,
            suddenAttackEndState.CommonEnter,
            suddenAttackEndState.CommonUpdate,
            suddenAttackEndState.CommonExit
        );
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
		//AddStateToTransition<HundBackStepState, IdleState>();
		//AddStateToTransition<HundEMPState, IdleState>();
		//AddStateToTransition<HundTackleState, HundTackleSubState>();
		AddStateToTransition<HundStopState, IdleState>();
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
