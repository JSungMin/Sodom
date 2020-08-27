using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EventArgumentNamespace;

public class ElectricGenerator : EnemyFrameBase {
	public ColorFlicker damFlicker;
	private bool isWork = true;
	public bool IsWork{
		get{
			return isWork;
		}
		set{
			if (value)
			{
				isWork = true;
				if (null != onWork)
					onWork.Invoke();
			}
			else
			{
				isWork = false;
				if (null != onBroken)
					onBroken.Invoke();
			}
		}
	}
	public UnityEvent onWork;
	public UnityEvent onBroken;
	// Use this for initialization
	public new void Start () {
		base.Start();
		player = GameObject.FindObjectOfType<Player> ();
		damFlicker.Initialize();
		RaiseActorDamaged +=  (object sender, ActorDamagedEventArg e) =>  {
			if (!isWork)
			{
				damFlicker.Initialize();
				StartCoroutine (damFlicker.OnDamageFlick);
			}
		};
		fsm.GetState<DamagedState>().checkEnter = delegate(ActionState fromState, object infoParam)
		{
			//	ElectricGenerator는 Idle이 곧 낮이라는 뜻 => 맞으면 안됨
			if (isWork)
				return false;
			return true;
		};
		fsm.GetState<DeadState>().OnEnter = delegate()
		{
			GeneratorOn();
		};
	}
	
	public void GeneratorOn()
	{
		fsm.TryTransferAction<IdleState>();
		IsWork = true;
	}
	public void GeneratorOff()
	{
		fsm.TryTransferAction<GeneratorOffState>();
		IsWork = false;
	}

	// Update is called once per frame
	public void FixedUpdate () {
		
	}
}
