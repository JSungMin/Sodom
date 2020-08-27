using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;

public class PlayerComboTimer : MonoBehaviour {
	//	Instance Pool
	private GameSystemService serviceInstance;
	private Player player;
	private PlayerActionFSM playerFSM;
	//private PlayerActionFSM playerFSM;
	//	Collection Pool
	public List<KeyCode> inputList = new List<KeyCode>();
	public List<SkillInfo> bufferSkillList = new List<SkillInfo> ();
	public List<float> bufferWatingTimeList = new List<float> ();
	private Queue<CommandNode> indexedChildQ = new Queue<CommandNode>();
	//	Indexer Pool
	public int comboIndex = 0;
	public int nor_comboIndex = 0;
	public float applyTick = .3f;
	public float inputTimer = .0f;
	public float nor_combo_maintain_duration = 0f;
	//	Flag Pool
	public bool flag_Run_Apply_Timer = false;

	public delegate void SkillBuildUp (SkillInfo skillInfo);
	public static SkillBuildUp OnSkillBuildUp;

	public void Init ()
	{
		player = GameObject.FindObjectOfType<Player> ();
		if (null != player)
		{
			playerFSM = player.fsm as PlayerActionFSM;
			player.RaiseActorLand += ClearSkillStack;
			player.RaiseActorAir += ClearSkillStack;
		}
		serviceInstance = GameSystemService.Instance;
		StartCoroutine ("ComboApply");
		serviceInstance.Add_AttackKeyDown_Subscriber (TraverseComboTree);
		serviceInstance.Add_GamePause_Subscriber (StopApplyTimer);
		serviceInstance.Add_GameResume_Subscriber (RunApplyTimer);
	}
		
	//	FSM 스테이트가 공격에서 다른 State로 변화될 때 사용됨
	public void ClearSkillStack()
	{
		comboIndex = 0;
		inputTimer = 0f;
		bufferSkillList.Clear ();
		inputList.Clear ();
		indexedChildQ.Clear ();
	}
	public void ClearSkillBuffer ()
	{
		inputTimer = applyTick;
	}
	public void ClearSkillStack(object sender, ActorLandEventArg arg)
	{
	//	Debug.Log ("Clear Skill Stack By : " + sender.ToString());
		player.actorInfo.canDoAirBehaviour = false;
		//(player.fsm.attackState as PlayerAttackState).usedAirAttack = false;
		ClearSkillStack ();
	}
	public void ClearSkillStack(object sender, ActorAirEvnetArg arg)
	{
	//	Debug.Log ("Clear Skill Stack By : " + sender.ToString());
		ClearSkillStack ();
	}
	public bool CheckSkillBufferEmpty ()
	{
		if (bufferSkillList.Count == 0)
			return true;
		for (int i = 0; i < bufferSkillList.Count; i++) {
			if (bufferSkillList [i].IsEmpty ())
				return true;
		}
		return false;
	}

	private IEnumerator ComboApply()
	{
		flag_Run_Apply_Timer = true;
		while (true)
		{			
			yield return null;
			if (!CheckSkillBufferEmpty ())
			{
				//	커맨드 입력이 완료된 스킬에 대한 처리
				for (int i = 0; i < bufferSkillList.Count; i++) 
				{
					if (!player.OnActorAttackTry(bufferSkillList [i])) 
					{
						break;
					}
					if (bufferSkillList [i].lastNode) 
					{
						inputTimer = applyTick;
						break;
					}
					inputTimer = 0f;
					bufferSkillList.RemoveAt (i);
				}
			}
			else
			{
				inputTimer += Time.deltaTime;
			}
			//	Command를 통한 Skill 사용 타이머
			if (inputTimer >= applyTick)
			{
				//	Reset Data To get input command
				ClearSkillStack();
			}
		}
		flag_Run_Apply_Timer = false;
	}

	private bool ValidateCommand (List<KeyCode> commandList, SkillInfo skill)
	{
		if (commandList.Count != skill.skillCommand.Count)
			return false;
		int count = commandList.Count;
		for (int i = 0; i < count; i++)
		{
			if (commandList[i] != skill.skillCommand[i])
			{
				return false;
			}
		}
		if (!player.skillCoolTimer.IsUseable (skill.skillName))
			return false;
		if (skill.prevSkillName == "")
			return true;
		//if (player.fsm.attackState.skillInfo.skillName != skill.prevSkillName)
		//	return false;
		return true;
	}
	//	return value는 flag_normalAttack을 지정합니다.
	private bool InitializeGroundSkills (KeyEventArgs k)
	{
		if (inputList.Count == 1) 
		{
			foreach (var node in serviceInstance.GetGroundSkillTree ().childs)
			{
				indexedChildQ.Enqueue (node);
			}
		}
		return false;
	}
	private bool InitializeAirSkills (KeyEventArgs k)
	{
		if (inputList.Count == 1)
		{
			foreach (var node in serviceInstance.GetAirSkillTree ().childs)
			{
				indexedChildQ.Enqueue (node);
			}
		}
		return false;
	}

	public void AddBufferSkill (SkillInfo skillInfo)
	{
		bufferSkillList.Add (skillInfo);
	}

	void BuildUpBufferSkill ()
	{
		bool noneCombo = true;
		while (indexedChildQ.Count != 0) 
		{
			var pop = indexedChildQ.Peek ();
			if (pop.element == inputList [comboIndex - 1])
			{
				noneCombo = false;
			}
			if (comboIndex < pop.depth)
				break;
			indexedChildQ.Dequeue ();
			//	Visit
			if (null != pop.useableSkill) 
			{
				if (ValidateCommand (inputList, pop.useableSkill))
				{
					bufferSkillList.Add (pop.useableSkill);
					Debug.Log ("List UP : " + pop.useableSkill.skillName);
					if (null != OnSkillBuildUp)
						OnSkillBuildUp (pop.useableSkill);
					//	노드의 자식이 없으면 스킬 바로 리스트 업
					if (pop.childs.Count == 0)
					{
						inputList.Clear ();
						indexedChildQ.Clear ();
						comboIndex = 0;
						break;
					} 
					else 
					{
						inputTimer = 0f;
					}
				}
			}
			for (int i = 0; i < pop.childs.Count; i++) 
			{
				indexedChildQ.Enqueue (pop.childs [i]);
			}
		}
		if (noneCombo) 
		{
			ClearSkillStack ();
		}
	}

	//	BFS를 Step in 시키며 ComboTree를 오름
	private void TraverseComboTree (object obj, KeyEventArgs k)
	{
		inputList.Add (k.element);
		inputTimer = 0f;
		comboIndex++;
		bool flag_normalAttack = false;
		if (player.actorInfo.isGrounded)
			flag_normalAttack = InitializeGroundSkills (k);
		else
			flag_normalAttack = InitializeAirSkills (k);
		//	Ground Normal Attack에 대한 예외처리
		if (flag_normalAttack) 
		{
			Debug.Log ("NORMAL");
			return;
		}
		//	Skill에 대한 처리
		BuildUpBufferSkill ();
	}

	//	Game 상황에 따라 비용이 드는 ApplyTimer의 실행을 중재하는데 사용되는 함수들 이다.
	private void RunApplyTimer (object obj, SystemTimerArgs arg)
	{
		if (!flag_Run_Apply_Timer) 
		{
			StartCoroutine ("ApplyTimer");
		}
	}
	private void StopApplyTimer (object obj, SystemTimerArgs arg)
	{
		if (flag_Run_Apply_Timer)
			StopCoroutine ("ApplyTimer");
	}
}
