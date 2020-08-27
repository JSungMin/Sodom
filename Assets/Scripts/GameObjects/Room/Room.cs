using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;

public class Room : MonoBehaviour {
	public static Cinemachine.CinemachineConfiner confiner;
	public static Dictionary<string, Room> RoomDictionary = new Dictionary<string, Room>();
	public RoomInfo roomInfo;
	public Transform focusObject;
	public Collider boundingVolume;
	public event EventHandler<RoomClearedEventArg> RaiseRoomCleared;

	// Use this for initialization
	void Awake () {
		if (null == confiner) {
			confiner = GameObject.FindObjectOfType<Cinemachine.CinemachineConfiner> ();
		}
		if (!RoomDictionary.ContainsKey (roomInfo.roomName))
			RoomDictionary.Add (roomInfo.roomName, this);
	}


	public void Update()
	{
		if (null == focusObject)
			return;
		if (roomInfo.roomRect.Contains (focusObject.transform.position)) 
		{
			confiner.m_BoundingVolume = boundingVolume;
		}
	}

	public void OnRoomCleared ()
	{
		EventHandler<RoomClearedEventArg> handler = RaiseRoomCleared;
		if (null != handler) {
			handler (this, new RoomClearedEventArg (this));
		}
	}
}
