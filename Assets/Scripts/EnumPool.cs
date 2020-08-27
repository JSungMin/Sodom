using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
	MENU,	//	Game Main Menu State
	RUNNING,//	Game Running State
	PAUSE	//	Game Pause and Setting
}
public enum UISettingState
{
	PAUSE,
	SETTING,
	SOUND,
	KEYMAP
}
public enum ActorType
{
	PLAYER,
	ENEMY,
	NPC,
	OBJECT
}
public enum CameraVibrateAmount
{
	NONE = 0,
	WEAK,
	MIDDLE,
	STRONG
}
public enum AnimationType
{
	FRAME,
	SPINE
}
public enum ActorConditionType
{
	SLEEP = 0,              //	수면
	DISCHARGE = 1,        	//	방전
	BURN = 2,               //	화상
	FROSTBITE = 3,        	//	동상    
    ICED = 4,               //	빙결
	BERSERKER = 5,			//	버서커
	PARALYSIS = 6,			//	마비
	STUN = 7,				//	기절
	FRACTURE = 8,			//	동상
	POISIONING = 9,			//	중독
	BLOODING = 10,			//	출혈
	UNBEATABLE = 11			//	무적
}
public enum EnemyAttackPatternType
{
	DIRECT,
	COMBO,
	COMBO2
}
public enum SkillType
{
	NORMAL = 0,
	SPECIAL,
	CHARGE,
	EVASION
}
public enum SkillEntityType
{
	GROUND,
	AIR
}
public enum SkillApplyRectType
{
	PART,			//	부분 범위 내 모든 객체에 적용
	PART_NUMERIC,	//	부분 범위 내 제한된 양만큼의 객체에만 적용
	GLOBAL,			//	ROOM 전체에 자신을 제외하고 적용
	SELF			//	자기한테만 적용
}
public enum SkillCounterType
{
	NONE,
	HIGH,
	MIDDLE,
	LOW
}
public enum SkillDamageType
{
	NORMAL,
	THROW,
	BLOW,
	AIRBORN
}
public enum ConsumeResourceType
{
	NONE,
	HP,
	MP
}
public enum AttackResultType
{
	NORMAL,
	CRITICAL,
	MISS
}
public enum QuestType
{
	HUNTING,			//	특정 Enemy를 잡아야 하는 미션, OnActorKill Event Handler 사용
	ELIMINATION,		//	해당 라운드에 있는 적들을 모두 제거해야 클리어 가능한 미션
	SKILL_TRAINING,		//	특정 커맨드를 특정 양만큼 반복하면 클리어 가능한 미션
	COLLECTION,			//	특정 아이템을 모아와야 클리어 가능한 미션
	INTERACTION,		//	특정 Actor 혹은 Interactive Object와 상호작용을 완료하면 클리어 가능한 미션
	MOVE				//	특정 장소로 이동을 하면 클리어
}
public enum AIPatternType
{
	IDLE,
	PATROL,
	CHASE,
	ATTACK_READY,
	ATTACK,
	ATTACK_END,
	DAMAGED,
	DEAD
}
public enum VolumeParam
{
	Volume_Master,
	Volume_Background,
	Volume_SFX
}
public enum AudioType
{
	NONE,
	HIT_GROUND,
	PLAYER_WALL_COLLISION,
	PLAYER_ATTACK01,
	PLAYER_ATTACK_READY01,
	PLAYER_ATTACK02,
	PLAYER_COUNTER,
	PLAYER_DAMAGED,
	METATRON_LASER,
	METATRON_IMPACT_GROUND,
	METATRON_ATTACK_READY,
	METATRON_DRAG_GROUND,
	METATRON_STAB,
	METATRON_WALK,
	METATRON_FLY_ATTACK,
	METATRON_DEAD,
	HUND_TACKLE,
	HUND_SHORT_BITE,
	HUND_MIDDLE_BITE,
	HUND_HIDE,
	HUND_APPEAR,
	ELECTRIC_DAMAGED,
	GLASS_BROKEN
}
public enum SnapshotType
{
	NORMAL,
	COUNTER
}
public enum AxisType
{
	X_AXIS,
	Y_AXIS,
	Z_AXIS
}
public enum ObjectType
{
	OFFENSIVE,
	DEPENSIVE,
	ITEM,
	STORYWARP			//	Story 진행을 위한 도착지역을 의미한다.
}

public enum AIAttackConditionType
{
	DEFAULT,
	CHECK_LIFE,
	CHECK_ENERGY
}
//	ObjectType은 InterctiveObject의 타입이고
//	InteractType은 Interact 했을때의 발생하는 Event의 형태다.
public enum InteractType
{
	INTERACTION,		//	Interactive Object와의 상호작용
	CONVERSATION,		//	NPC와의 대화
	ATTACK,				//	Actor가 일반적인 공격이 아닌 상호작용을 통해 공격함
	DAMAGED,			//	Actor가 상호작용을 통해 Damage를 입음
	QUEST,				//	Player가 상호작용을 통해 특정 Quest를 받음
	REWARD,				//	Player가 상호작용을 통해 특정 Reward를 받음
	POPUI				//	Player가 상호작용을 통해 특정 UI를 호출함
}
public enum ItemType
{
	WEAPON,
	SHIELD,
	GOODS			//	소모품
}
public enum WeaponType
{
	SWORD,
	GUN
}
public enum GoodsType
{
	POTION,
	ETC
}
public enum ShieldType
{
	SHIELD,
	HELMET
}
public enum ComparisionType
{
	LESS,
	GREATER,
	EQUAL
}
public enum TalkingEventType
{
	DELAY = 0,
	COLORING = 1,
	SOUND = 2,
	VIBRATE = 3,
	JUMPING = 4,
	SEND_MESSAGE = 5,
    END_LINE = 6
}