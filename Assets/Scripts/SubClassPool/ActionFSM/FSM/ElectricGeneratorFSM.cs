using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public class ElectricGeneratorFSM : ActorActionFSM
{
	public IdleState idleState;
	public AttackState attackState;
	public DamagedState damagedState;
	public DeadState deadState;
	public GeneratorOffState offState;
    protected override void BuildStates()
    {
        idleState.Initialize(this);
		idleState.BuildState(
			idleState.CheckEnterByPlayer,
			idleState.AlwaysReturnTrue,
			idleState.CommonEnter,
			idleState.DoNothing,
			idleState.CommonExit
		);
		attackState.Initialize(this);
		attackState.BuildState(
			attackState.CheckEnterByEnemy,
			attackState.CommonCheckExit,
			attackState.EnterByEnemy,
			attackState.CommonUpdate,
			attackState.CommonExit,	
			delegate(object infoParam) {
				attackState.EditStateInfo((SkillInfo)infoParam);
			}
		);
		damagedState.Initialize(this);
		damagedState.BuildState(
			damagedState.CommonCheckEnter,
			damagedState.CommonCheckExit,
			damagedState.CommonEnter,
			damagedState.CommonUpdate,
			damagedState.CommonExit,
			delegate(object infoParam){
				damagedState.EditStateInfo((DamageInfo)infoParam);
			}
		);
		damagedState.knockoutTime = 1f;
		deadState.Initialize(this);
		deadState.BuildState(
			deadState.CommonCheckEnter,
			deadState.CommonCheckExit,
			deadState.CommonEnter,
			deadState.CommonUpdate,
			deadState.CommonExit
		);
		offState.Initialize(this);
		offState.BuildState(
			offState.CommonCheckEnter,
			offState.CommonCheckExit,
			offState.CommonEnter,
			offState.CommonUpdate,
			offState.CommonExit,
			delegate(object infoParam){
				offState.EditStateInfo((DamageInfo)infoParam);
			}
		);
		nowState = idleState;
    }

    protected override void BuildStatesPool()
    {
       
    }

    protected override void BuildTransitionGraph()
    {
		
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
}
