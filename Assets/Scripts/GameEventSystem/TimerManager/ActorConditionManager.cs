using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;

public class ActorConditionManager : MonoBehaviour {
    public static int conditionNum = 12;
    public static int[] overlap_limit = { 1,1,1,1,1,1,1,1,6,5,10,1 };

    public List<ActorConditionInfo> conditionInfoList = new List<ActorConditionInfo>();

	public event EventHandler<ConditionEventArg> RaiseChildThreadUpdate;

	public void Init()
	{
		foreach (var actor in GameSystemService.Instance.inGameActorList)
		{
			conditionInfoList.Add (actor.actorInfo.conditionInfo);
		}
	}

    public static int IntegerToConditionType(int v)
    {
        return (int)((ActorConditionType)v);
    }

    void LateUpdate()
    {
        SyncCondition();
    }

    //	Called in GameSystemService Update
    public void SyncCondition()
    {
        EventHandler<ConditionEventArg> handler = RaiseChildThreadUpdate;
        if (null != handler)
        {
            handler(this, null);
        }
    }
}
