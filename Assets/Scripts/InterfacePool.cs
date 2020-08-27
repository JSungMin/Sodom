using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InformationNamespace;
using EventArgumentNamespace;

public interface ISaveableActor {
	bool Init();
	void Save(object sender, SaveActorArgs arg);
	void Load(object sender, LoadActorArgs arg);
}
