using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using InformationNamespace;
using EventArgumentNamespace;

// Publisher
[Serializable]
public class SaveManager {
	public GameDataInfo nowGameData;
	[HideInInspector]
	public QuestFactory questFactory;

	public event EventHandler<SaveActorArgs> RaiseSaveActorEvent;
	public event EventHandler<LoadActorArgs> RaiseLoadActorEvent;

	// 외부 노출 Method Block
	public void PackingGameData ()
	{
		SavePlayer (Player.instance);
		string dirPath = Application.dataPath + "/SaveDatas/" + nowGameData.path;
		string path = Application.dataPath + "/SaveDatas/"+ nowGameData.path + "/" + nowGameData.path;
		if (!Directory.Exists (dirPath))
			Directory.CreateDirectory (dirPath);
		File.WriteAllText(path, JsonUtility.ToJson(nowGameData));
		questFactory.SaveQuestInfo (path);
	}
	public void UnPackingGameData()
	{
		string dirPath = Application.dataPath + "/SaveDatas/" + nowGameData.path;
		string path = Application.dataPath + "/SaveDatas/" + nowGameData.path + "/" + nowGameData.path;
		if (!File.Exists (path)) {
			Debug.LogError ("Doesn't have Save File");
			return;
		}

		nowGameData = JsonUtility.FromJson<GameDataInfo> (File.ReadAllText (path));
		Player savedActor = new Player ();
		savedActor.actorInfo = JsonUtility.FromJson<ActorInfo> (nowGameData.playerActorInfo);
		savedActor.equipInfo = JsonUtility.FromJson<EquipInfo> (nowGameData.playerEquipInfo);
		savedActor.invenInfo = JsonUtility.FromJson<InventoryInfo> (nowGameData.playerInvenInfo);
		savedActor.questInfo = JsonUtility.FromJson<QuestProgressInfo> (nowGameData.playerQuestProgressInfo);
		savedActor.actorInfo.AddEnergy (100f);
		questFactory.Init ();
		LoadPlayer (savedActor);
	}
	// 내부 작업용 Method Block
	private void SavePlayer(Actor actor)
	{
		var newArg = new SaveActorArgs (actor);
		OnSavePlayer (newArg);
		nowGameData.sceneName = SceneManager.GetActiveScene ().name;
		nowGameData.playerActorInfo = newArg.actorInfo;
		nowGameData.playerEquipInfo = newArg.actorEquipInfo;
		nowGameData.playerInvenInfo = newArg.actorInvenInfo;
		nowGameData.playerQuestProgressInfo = newArg.actorQuestProgressInfo;
	}
	private void LoadPlayer(Actor actor)
	{
		var newArg = new LoadActorArgs (actor);
		OnLoadPlayer (newArg);
	}

	protected virtual void OnSavePlayer(SaveActorArgs arg)
	{
		EventHandler<SaveActorArgs> handler = RaiseSaveActorEvent;
		if (null != handler)
        {
			Debug.Log ("Save Call From : " + arg.GetActor().actorInfo.actor_name);
			handler (this, arg);
		}
		else {
			Debug.Log ("Save Actor Handler is Null");
		}
	}
	protected virtual void OnLoadPlayer(LoadActorArgs arg)
	{
		EventHandler<LoadActorArgs> handler = RaiseLoadActorEvent;
		if (null != handler)
        {
			Debug.Log ("Try to Load Actor : " + arg.GetActor().actorInfo.actor_name);
			handler (this, arg);
		} 
		else {
			Debug.Log ("Load Actor Handler is Null");
		}
	}
}
