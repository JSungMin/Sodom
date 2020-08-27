using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TankPatternType
{
	찌르기 = 0,
	레이저,
	몸통박치기,
	상단베기,
	하단베기_찌르기,
	박기_레이저,
	두번찌르기,
	박기_하단베기_찌르기,
	하단베기,
	도약,
	연속도약
}
public enum HundPatternType
{
	상단뜯기 = 0,	//	Bite_High1
	중단뜯기01,		//	Bite_Middle1
	중단뜯기02,		//	Bite_Middle2
	중단뜯기03,
	하단뜯기01,		//	Bite_Low1
	하단뜯기02,
	중단_하단_중단뜯기,
	돌진,			//	LowCharge
	투사체,			//	Ranged_Attack1
	투사체_준비,
	투사체_하단,
	투사체_상단,
	급습,			//	HideAttack_High_Root
	꼬리치기_중단,		//	TailAttack_Middle1
	꼬리치기_상단,
	쇼크웨이브,		//	Shock
	백스텝,
	추적,
	추적_중단뜯기02,
	추적_하단뜯기01,
	미사일발사
}
public enum StilettoPatternType
{
	상단킥,
	상단찌르기,
	중단찌르기,
	중단베기,
	하단찌르기,
	상단킥_중단찌르기_중단베기,
	하단찌르기_중단찌르기,
	샷건,
	샷건_준비,
	샷건_샷,
	샷건_완료,
	발도,
	발도_준비,
	발도_완료,
	분노,
	즉시사격,
	슬라이딩_준비,
	슬라이딩_완료,
	슬라이딩_즉시사격,
	상단킥_상단찌르기,
	중단찌르기_즉시사격
}

public class BossPatternEnumPool {}
