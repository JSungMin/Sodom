using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Timer {
	public float timer;
	public float duration;

	public void Initialize ()
	{
		timer = 0f;
	}
	public void Initialize (float duration)
	{
		this.duration = duration;
		timer = 0f;
	}
	public void IncTimer (float amount)
	{
		timer += amount;
	}
    public void ClampZeroToDuration()
    {
        timer = Mathf.Clamp(timer, 0f, duration);
    }
	public bool CheckTimer ()
	{
		return timer >= duration;
	}
	public float GetRatio()
	{
		return timer / duration;
	}
	public void Reset ()
	{
		timer = 0f;
	}

}
