using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EventArgumentNamespace;

public class Tutorial_Stage01 : MonoBehaviour {
	public GameObject[] messages;
	public Player player;
	public Metatron metatron;

	public int counterDest = 3, evasionDest = 3;
	public Timer delayTimer = new Timer{duration = 2f};
	public Timer fadeOutTimer = new Timer{duration = 2f};
	public Timer sceneEndTimer = new Timer{duration = 3f};
	public UnityEngine.UI.Image fadeOutImage;

	private bool init = false;
	// Use this for initialization
	public void Start () {
		metatron.mat.SetColor ("_Color", Color.white);
		player.RaiseActorCounter += (object sender, ActorDamagedEventArg e) => {
			if (counterDest > 0)
				counterDest--;
			else 
			{
				messages[0].SetActive(false);
			}
		};
		player.RaiseActorRolling += (object sender, ActorRollingEventArg e) => {
			if (evasionDest > 0)
				evasionDest--;
			else
			{
				messages[1].SetActive(false);
			}
		};
		metatron.RaiseActorDead += (object sender, ActorDamagedEventArg e) =>{
			init = true;
		};
	}
	
	// Update is called once per frame
	void Update () {
		if (init)
		{
			delayTimer.IncTimer(Time.deltaTime);
			if (!delayTimer.CheckTimer())
				return;
			fadeOutTimer.IncTimer(Time.deltaTime);
			fadeOutImage.color = Color.Lerp(new Color(1f,1f,1f,0f), Color.white,fadeOutTimer.GetRatio());
			if (fadeOutTimer.CheckTimer())
			{
				sceneEndTimer.IncTimer(Time.deltaTime);
				if (sceneEndTimer.CheckTimer())
				{
					GameSystemService.Instance.LoadScene("Stage02");
				}
			}
		}
	}
}
