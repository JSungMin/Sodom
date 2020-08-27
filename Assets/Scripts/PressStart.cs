using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PressStart : MonoBehaviour {

	public int index = 0;
	// Use this for initialization
	void Start () {
        Debug.Log(GameSystemService.instance);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.anyKeyDown){
            SceneManager.UnloadSceneAsync(index - 1);
			SceneManager.LoadScene(index);
		}
	}
}
