using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;

public class EnemyAIHelper {
	/*	Common State Check Helper Block
	 * 
	 *	--IsAttackState for Check Attack State
	 *	--IsStunState	for Check Stun State
	 *	--IsStunByParry for 캐릭터가 Parry에 의해서 기절했었는지 검사
	 */	
	public static bool IsAttackState (ActorActionFSM fsm)
	{
		return fsm.nowState == fsm.GetState<AttackState> ();
	}
	public static bool IsStunState (ActorActionFSM fsm)
	{
		return fsm.nowState == fsm.GetState<StunState> ();
	}
	public static bool IsPlayerDamaged(Actor player)
	{
		return player.fsm.nowState == player.fsm.GetState<DamagedState> () ||
		player.fsm.nowState == player.fsm.GetState<DownState> () ||
		player.fsm.nowState == player.fsm.GetState<DeadState> ();
	}
	public static bool IsStunByParry(Actor actor)
	{
		return actor.actorInfo.isStunByParry;
	}
	/*	Boss Pattern or Skill Helper Block
	 * 
	 *	--IsEmptyPatternBuffer for EnemySpineBase
	 *	--IsEmptyPatternBuffer for EnemyFrameBase
	 *	--CheckUsedSkill			: Enemy가 사용한 스킬이 파라미터의 스킬과 같은지 검사해준다. 
	 *	--ConsumePatternBuffer		: Enemy의 PatternBuffer 내 사용가능한 스킬이 있다면 해당 스킬을 사용=>AttackState 한다.
	 *	--AddPatternToBuffer		: Enemy의 패턴을 PatternBuffer 리스트에 추가 및 정렬해준다.
	 *	--WarmupPattern				: Enemy의 특정 스킬의 선딜레이 타이머를 작동시킴
	 *	--RemovePatternFromBuffer 	: Enemy의 특정 스킬이 Buffer에 있을때 제거시킴
	 *	--RemoveAllPatternFromBuffer: Enemy의 모든 스킬이 Buffer에서 제거된다.
	 *	--ResetSkill				: Skill 즉 Pattern의 Warmup과 Cool Timer을 모두 0으로 초기화
	 */
	public static bool IsEmptyPatternBuffer (EnemySpineBase actor)
	{
		return actor.patternBuffer.Count == 0;
	}
	public static bool IsEmptyPatternBuffer (EnemyFrameBase actor)
	{
		return actor.patternBuffer.Count == 0;
	}

	public static bool CheckUsedSkill (string skillName, EnemySpineBase enemy)
	{
		if (enemy.GetUsedSkill ().skillName == skillName)
			return true;
		return false;
	}
	public static SkillInfo GetSkillInfo (string skillName, EnemySpineBase enemy)
	{
		return enemy.skillCoolTimer.GetSkill (skillName);
	}
	public static EnemyAttackPatternInfo GetPatternInfo (string patternName, EnemySpineBase enemy)
	{
		if (enemy.patternData.patternDic.ContainsKey(patternName))
			return enemy.patternData.patternDic[patternName];
		Debug.LogError("ERROR INVAILD ACCESS : " + patternName);
		return null;
	}
	public static void ConsumePatternBuffer(EnemySpineBase enemy, DelegatePool.BoolOneParam chkPatternUsable)
	{
		if (enemy.nowPattern == null) 
		{
			if (enemy.patternBuffer.Count == 0)
				return;
			if (chkPatternUsable (enemy.patternBuffer [0])) 
			{
				if (enemy.patternBuffer [0].UseSkill (ref enemy.patternIndex)) {
					enemy.nowPattern = enemy.patternBuffer [0];
					enemy.patternBuffer.RemoveAt(0);
					Debug.Log ("Pattern Start : " + enemy.nowPattern.patternName);
				} else {
					Debug.Log ("Pattern Deleted : " + enemy.patternBuffer[0].patternName);
					enemy.patternBuffer[0].ResetPattern();
					enemy.patternBuffer.RemoveAt (0);
				}
			}
			else 
			{
				if (enemy.patternBuffer.Count == 0)
					return;
				Debug.Log ("Pattern Deleted : " + enemy.patternBuffer[0].patternName);
				enemy.patternBuffer[0].ResetPattern();
				enemy.patternBuffer.RemoveAt (0);
			}
		}
		else
		{
			if (enemy.patternIndex != 0)
			{
				enemy.nowPattern.ReadyPattern (enemy.patternIndex);
			}
			if (!enemy.nowPattern.UseSkill (ref enemy.patternIndex))
			{
				if (enemy.patternIndex == 0) 
				{
					enemy.patternBuffer.Sort (delegate(EnemyAttackPatternInfo x, EnemyAttackPatternInfo y) {
						return (x.patternPriority > y.patternPriority) ? -1 : 1;	
					});
					enemy.patternBuffer.Remove (enemy.nowPattern);
				}
			}
		}
	}
	public static void AddPatternToBuffer (string skillName, EnemySpineBase enemy)
	{
		var tmpPattern = enemy.patternData.patternDic [skillName];
		tmpPattern.ReadyPattern (0);
		if (!enemy.patternBuffer.Contains (tmpPattern))
		{
			enemy.patternBuffer.Add (tmpPattern);
			enemy.patternBuffer.Sort (delegate(EnemyAttackPatternInfo x, EnemyAttackPatternInfo y) {
				return (x.patternPriority > y.patternPriority) ? -1 : 1;	
			});
		}
	}
	public static void RemovePatternFromBuffer (string skillName, EnemySpineBase enemy)
	{
		enemy.patternBuffer.Remove (enemy.patternData.patternDic [skillName]);
	}
	public static void ClearPatternBuffer (EnemySpineBase enemy)
	{
		enemy.patternBuffer.Clear ();
	}
	public static void ClearPatternBuffer (EnemyFrameBase enemy)
	{
		enemy.patternBuffer.Clear ();
	}
	public static void WarmupPattern (string skillName, EnemySpineBase enemy)
	{
		enemy.patternData.WarmupPattern (skillName);
		if (!enemy.patternData.CheckUseable (skillName.ToString (), enemy.patternIndex))
			return;
		if (!enemy.patternBuffer.Contains (enemy.patternData.patternDic [skillName])) {
			enemy.patternBuffer.Add (enemy.patternData.patternDic[skillName]);
		}
	}
	public static void ResetPattern (string skillName, EnemySpineBase enemy)
	{
		enemy.skillCoolTimer.ResetSkill (skillName);
	}
	/*	Enemy Distance Information Helper Block
	 *	--UpdateDistanceInfo 플레이어와의 거리 계산 및 방향 계산
	 * 	--DisInExShort	플레이어와의 거리가 초근접인지 검사
	 * 	--DiInShort		플레이어와의 거리가 근접인지 검사
	 * 	--DisInMiddle	플레이어와의 거리가 중거리인지 검사
	 * 	--DisInFar		플레이어와의 거리가	원거리인지 검사
	 */
	public static void UpdateDistanceInfo (EnemySpineBase actor)
	{
		actor.disInfo.disToPlayer = Vector3.Distance (actor.transform.position, actor.player.transform.position);
		actor.disInfo.dirToPlayer = Mathf.Sign (actor.player.transform.position.x - actor.transform.position.x);
	}
	public static void UpdateDistanceInfo (EnemyFrameBase actor)
	{
		actor.disInfo.disToPlayer = Vector3.Distance (actor.transform.position, actor.transform.position);
		actor.disInfo.dirToPlayer = Mathf.Sign (actor.player.transform.position.x - actor.transform.position.x);
	}
	public static bool DisInExShort (EnemyFrameBase enemy)
	{
		return enemy.disInfo.disToPlayer <= enemy.disInfo.disInExShort;
	}
	public static bool DisInShort (EnemyFrameBase enemy)
	{
		return enemy.disInfo.disToPlayer > enemy.disInfo.disInExShort &&
			enemy.disInfo.disToPlayer <= enemy.disInfo.disInShort;
	}
	public static bool DisInMiddle (EnemyFrameBase enemy)
	{
		return enemy.disInfo.disToPlayer > enemy.disInfo.disInShort &&
			enemy.disInfo.disToPlayer <= enemy.disInfo.disInFar;
	}
	public static bool DisInFar (EnemyFrameBase enemy)
	{
		return enemy.disInfo.disToPlayer >= enemy.disInfo.disInFar;
	}
	public static bool DisInExShort (EnemySpineBase enemy)
	{
		return enemy.disInfo.disToPlayer <= enemy.disInfo.disInExShort;
	}
	public static bool DisInShort (EnemySpineBase enemy)
	{
		return enemy.disInfo.disToPlayer > enemy.disInfo.disInExShort &&
			enemy.disInfo.disToPlayer <= enemy.disInfo.disInShort;
	}
	public static bool DisInMiddle (EnemySpineBase enemy)
	{
		return enemy.disInfo.disToPlayer > enemy.disInfo.disInShort &&
			enemy.disInfo.disToPlayer <= enemy.disInfo.disInFar;
	}
	public static bool DisInFar (EnemySpineBase enemy)
	{
		return enemy.disInfo.disToPlayer >= enemy.disInfo.disInFar;
	}

	/*	Specific Boss AI Helper Block
	 *	--HundAIHelper 
	 * 	----IsNormal			: !공격상태&!스턴상태&!방향전환상태&!추적상태&!EMPSTATE
	 *	----CheckUsedSkill		: Enemy가 사용한 스킬이 파라미터의 스킬과 같은지 검사해준다.
	 *	----AddPatternToBuffer	: Enemy의 패턴을 PatternBuffer 리스트에 추가 및 정렬해준다.
	 *	----WarmupPattern				: Enemy의 특정 스킬의 선딜레이 타이머를 작동시킴
	 *	----RemovePatternFromBuffer 	: Enemy의 특정 스킬이 Buffer에 있을때 제거시킴
	 *	----ResetSkill				: Skill 즉 Pattern의 Warmup과 Cool Timer을 모두 0으로 초기화
	 */
	public class HundAIHelper
	{
		/*	Dirty Flag Description
			---------------다른 행동과 중첩될 수 있는 상태 변수---
			0bxx01 xxxx(----) = CURRENT TURNING
			0bxx10 0000(----) = CURRENT ATTACKING (TURN BIT와 공존불가)
			0bxx10 xxxx(----) = CURRENT NEED ATTACK (AI에서 Attack 선정 필요)					
			0bx100 xxxx(----) = CURRENT STOP (0은 공존 불가 상태)
			0b1xxx xxxx(----) = CURRENT LIGHT OFF
			0b0xxx xxxx(----) = CURRENT LIGHT ON
			---------------NORMAL STATE------------------------
			0b0000 0000(0x00) = IDLE
			0b0000 0001(0x01) = WALK
			---------------CHASE ATTACK PIPELINE---------------
			0b0000 0010(0x02) = RUN FOR CHASE ATTACK
            0b0000 0011(0x03) = CHASE RUN END
			0b0001 0011(0x13) = CHASE RUN END + TURNING
			0b0010 0011(0x23) = CHASE ATTACK
			---------------BACK STEP PIPELINE------------------
			0b0000 0100(0x04) = BACK STEP
            0b0000 0101(0x05) = BACK STEP END
			0b0010 0101(0x25) = BACK STEP + ATTACK END
			---------------TACKLE PIPELINE---------------------
			0b0000 1000(0x08) = READY TACKLE (SELECT STOP OR TURN)
			0b0001 1001(0x19) = TURN FOR TACKLE
			0b0100 1001(0x49) = STOP FOR TACKLE
			0b0000 1011(0x0B) = STOPPING END
			0b0010 1011(0x2B) = TACKLE ATTACK
			0b0100 1111(0x4F) = STOPPING FOR ENDING TACKLE (이전 Previous Dirty Bit를 통해 처음 Stage와 구분)
			---------------STUN STATE--------------------------
			0b0001 0000 0000(0x100) = STUN STATE
		 */	
		public static Dictionary<int, string> dirtyMap = new Dictionary<int, string>()
		{
			{0x00,"IDLE"}, {0x01,"WALK"}, {0x05,"TURNING"}, {0x08,"RUN FOR CHASE ATTACK"},
			{0x19,"CHASE TURNING"},{0x29,"CHASE ATTACK"},{0x04,"BACK STEP"},{0x24,"BACK STEP+ATTACK"},
			{0x02,"READY TACKLE"},{0x12,"TURN FOR TACKLE"},{0x42,"STOP IN TACKLE"},
			{0x22,"TACKLE ATTACK"}, {0x100,"STUN STATE"}
		};
		public static int dirtyFlag = 0;
		public static int prevDirtyFlag = 0;
		public static void SetDirty (int indx)
		{
			prevDirtyFlag = dirtyFlag;
            if (indx - 1 == 0)
                dirtyFlag |= 1;
            else
                dirtyFlag |= 1 << (indx - 1);
            if (prevDirtyFlag != dirtyFlag)
                PrintDirtyFlag();
		}
		public static void ResetDirty()
		{
			prevDirtyFlag = dirtyFlag;
			dirtyFlag = 0;
		}
		public static void ResetDirty(int indx)
		{
			prevDirtyFlag = dirtyFlag;
			dirtyFlag &= ~(1 << (indx-1));
		}
		public static bool CheckDirtyIndex(int indx)
		{
			return ((dirtyFlag & (1 << (indx-1))) >= 1) ? true : false;
		}
		public static bool CheckDirtyMask (int value)
		{
			return dirtyFlag == value;
		}
		public static bool CheckHighNibble(int value, int offset = 0)
		{
			return ((0xF0 << (offset * 8)) & dirtyFlag) >> (offset * 8) == value ? true : false;
		}
		public static bool CheckLowNibble(int value, int offset = 0)
		{
			return ((0x0F << (offset * 8)) & dirtyFlag) >> (offset * 8) == value ? true : false;
		}
		public static void PrintDirtyFlag()
		{
			string cont = "AI State Flag : ";
			for (int i = sizeof(int) * 8; i > 0; i--)
			{
				if (CheckDirtyIndex(i))
					cont += "1";
				else
					cont += "0"; 
			}
			cont += "\n";
			if (dirtyMap.ContainsKey(dirtyFlag))
			{
				cont += "STATE : " + dirtyMap[dirtyFlag];
			}
			else
			{
				cont += "ERROR : NO_DIRTY_KEY";
			}
			//Debug.Log(cont);
		}
		//	NOT ATTACK|NOT STUN|NOT TRUNNING|NOT CHASE|NOT EMPSTATE
		public static bool IsNormal
		{
			get{
				return CheckDirtyMask(0) | CheckDirtyMask(1);
			}
		}
		public static bool IsTurning
		{
			get
			{
				return CheckDirtyIndex(5);
			}
			set {
				if (value)
					SetDirty(5);
				else
				 	ResetDirty(5);
			}
		}
		public static bool IsAttacking
		{
			get
			{
				return CheckDirtyIndex(6);
			}
			set
			{
				if (value)
					SetDirty(6);
				else
				 	ResetDirty(6);
			}
		}
		public static bool IsNeedAttack()
		{
			return !CheckLowNibble(0) & IsAttacking;
		}
		public static bool IsStopping
		{
			get
			{
				return CheckDirtyIndex(7);
			}
			set
			{
				if (value)
					SetDirty(7);
				else
				 	ResetDirty(7);
			}
		}
		public static bool IsLightOFF
		{
			get
			{
				return CheckDirtyIndex(8);
			}
			set
			{
				if (value)
                {
                    
					SetDirty(8);
                }
				else
				 	ResetDirty(8);
			}
		}
		public static bool IsStunning
		{
			get
			{
				return CheckDirtyIndex(9);
			}
			set
			{
				if (value)
					SetDirty(9);
				else
				 	ResetDirty(9);
			}
		}
		public static bool IsChasePipeline
		{
			get{
				
				return !CheckDirtyIndex(4)&&
				!CheckDirtyIndex(3)&&
				CheckDirtyIndex(2);
			}
			set{
				if (value)
				{
					ResetDirty();
					SetDirty(2);
				}
				else
				 	ResetDirty(2);
			}
		}
		public static bool IsBackstepPipeline
		{
			get
			{
				return !CheckDirtyIndex(4)&&
				CheckDirtyIndex(3);
			}
			set
			{
				if (value)
                {
                    ResetDirty();
					SetDirty(3);
                }
				else
				 	ResetDirty(3);
			}
		}
		public static bool IsTacklePipeline
		{
			get
			{
				return CheckDirtyIndex(4);
			}
			set
			{
				if (value)
                {
                    ResetDirty();
					SetDirty(4);
                }
				else
				 	ResetDirty(4);
			}
		}
		public static bool CheckUsedSkill (HundPatternType skillName, EnemySpineBase enemy)
		{
			return EnemyAIHelper.CheckUsedSkill (skillName.ToString (), enemy);
		}
		public static void AddPatternToBuffer (HundPatternType skillName, EnemySpineBase enemy)
		{
			EnemyAIHelper.AddPatternToBuffer (skillName.ToString (), enemy);
		}
		public static void WarmupPattern (HundPatternType skillName, EnemySpineBase enemy)
		{
			EnemyAIHelper.WarmupPattern (skillName.ToString (), enemy);
		}
		public static void RemovePatternFromBuffer (HundPatternType skillName, EnemySpineBase enemy)
		{
			EnemyAIHelper.RemovePatternFromBuffer (skillName.ToString (), enemy);
		}
		public static void ResetPattern (HundPatternType skillName, EnemySpineBase enemy)
		{
			EnemyAIHelper.ResetPattern (skillName.ToString (), enemy);
		}
	}
	public class MetatronHelper
	{

	}
}