using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventArgumentNamespace
{
	public class SystemTimerArgs : EventArgs
	{
		protected object element;

		public SystemTimerArgs () {	}
		public SystemTimerArgs (object e)
		{
			element = e;
		}
		public object GetElement()
		{
			return element;
		}
	}

	public class ConditionEventArg  : EventArgs{
		protected object element;

		public ConditionEventArg () {	}
		public ConditionEventArg (object e)
		{
			element = e;
		}
		public object GetElement()
		{
			return element;
		}
	}
	public class ActorCollisionEventArg : EventArgs{
		public Collision col;

		public ActorCollisionEventArg (Collision col)
		{
			this.col = col;
		}
	}
	public class ActorLandEventArg : EventArgs{
		public Actor actor;
		public Collider plane;

		public ActorLandEventArg (){	}
		public ActorLandEventArg (Actor a, Collider c)
		{
			actor = a;
			plane = c;
		}
	}
	public class ActorAirEvnetArg : EventArgs{
		public Actor actor;

		public ActorAirEvnetArg (){	}
		public ActorAirEvnetArg (Actor a)
		{
			actor = a;
		}
	}
	public class ActorAttackEventArg : EventArgs{
		public Actor attacker;
		public InformationNamespace.SkillInfo usedSkill;

		public ActorAttackEventArg (){	}
		public ActorAttackEventArg (Actor a, InformationNamespace.SkillInfo s)
		{
			attacker = a;
			usedSkill = s;
		}
	}
	public class ActorDamagedEventArg : EventArgs{
		public InformationNamespace.DamageInfo damageInfo;

		public ActorDamagedEventArg (){	}
		public ActorDamagedEventArg (InformationNamespace.DamageInfo d)
		{
			damageInfo = d;
		}
	}
	public class ActorWalkEventArg : EventArgs{
		public Actor mover;
		public float dir;

		public ActorWalkEventArg (){}
		public ActorWalkEventArg (Actor actor, float dir)
		{
			mover = actor;
			this.dir = dir;
		}
	}
	public class ActorSitEventArg : EventArgs{
		public Actor sitter;

		public ActorSitEventArg (){}
		public ActorSitEventArg (Actor actor)
		{
			sitter = actor;
		}
	}
	public class ActorJumpEventArg : EventArgs{
		public Actor jumper; 

		public ActorJumpEventArg() {}
		public ActorJumpEventArg(Actor actor)
		{
			this.jumper = actor;
		}
	}
	public class ActorRollingEventArg : EventArgs{
		public Actor roller;

		public ActorRollingEventArg (){}
		public ActorRollingEventArg (Actor actor)
		{
			this.roller = actor;
		}
	}
	public class ActorEquipEventArg : EventArgs{
		public Actor actor;
		public InformationNamespace.ItemInfo equipItem;

		public ActorEquipEventArg(){}
		public ActorEquipEventArg(Actor actor, InformationNamespace.ItemInfo equipItem)
		{
			this.actor = actor;
			this.equipItem = equipItem;
		}
	}
	public class ActorRootEventArg : EventArgs{
		public Actor actor;
		public InformationNamespace.ItemInfo rootItem;

		public ActorRootEventArg(){}
		public ActorRootEventArg(Actor actor, InformationNamespace.ItemInfo rootItem)
		{
			this.actor = actor;
			this.rootItem = rootItem;
		}
	}
	public class ActorInteractEventArg : EventArgs{
		public Actor sender;
		public Actor interactWith;

		public ActorInteractEventArg(){}
		public ActorInteractEventArg(Actor sender, Actor interactWith)
		{
			this.sender = sender;
			this.interactWith = interactWith;
		}
	}
	public class ActionStateEventArg : EventArgs{
		public ActionState actionState;

		public ActionStateEventArg (ActionState state)
		{
			actionState = state;
		}
	}
	//	Frame Base Actor의 Animation이 1회 루프 돌 때 마다 호출
	//	되는 이벤트의 인자
	public class FrameAnimationLoopArg : EventArgs{
		public Animator animator;
		public string animName;

		public FrameAnimationLoopArg() {	}
		public FrameAnimationLoopArg(Animator animator, string aName)
		{
			this.animator = animator;
			this.animName = aName;
		}

	}
	public class SpineAnimationLoopArg : EventArgs{
		public Spine.Unity.SkeletonAnimation animator;
		public string animName;

		public SpineAnimationLoopArg() {	}
		public SpineAnimationLoopArg(Spine.Unity.SkeletonAnimation animator, string aName)
		{
			this.animator = animator;
			this.animName = aName;
		}
	}
	public class RoomClearedEventArg : EventArgs{
		public Room room;

		public RoomClearedEventArg(){	}
		public RoomClearedEventArg(Room room)
		{
			this.room = room;
		}
	}
	public class KeyEventArgs : EventArgs
	{
		public KeyCode element;

		public KeyEventArgs (){	}
		public KeyEventArgs (KeyCode e)
		{
			element = e;
		}
	}
	public class SaveArgs<T> : EventArgs
	{
		protected T element;

		public SaveArgs () {	}
		public SaveArgs (T e)
		{
			element = e;
		}
		public T GetElement()
		{
			return element;
		}
	}
	public class LoadArgs<T> : EventArgs
	{
		protected T element;

		public LoadArgs () {	}
		public LoadArgs (T e)
		{
			element = e;
		}
		public T GetElement()
		{
			return element;
		}
	}

	public class SaveActorArgs : SaveArgs<Actor>
	{
		public string actorInfo;
		public string actorEquipInfo;
		public string actorInvenInfo;
		public string actorQuestProgressInfo;

		public SaveActorArgs ()
		{
			element = null;
		}
		public SaveActorArgs (Actor a)
		{
			element = a;
		}

		public Actor GetActor()
		{
			return element;
		}
	}
	public class LoadActorArgs : LoadArgs<Actor>
	{
		public LoadActorArgs ()
		{
			element = null;
		}
		public LoadActorArgs (Actor e)
		{
			element = e;
		}

		public Actor GetActor()
		{
			return element;
		}
	}
}
