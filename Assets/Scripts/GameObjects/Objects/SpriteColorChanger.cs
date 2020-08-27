using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteColorChanger : MonoBehaviour {
	public SpriteRenderer sRenderer;
	public Gradient color;
	public Timer timer;
	public bool useTimerBase = true;
	public bool useDistanceBase = false;
	public Transform disTarget;
	public float maxColorDis;
	public float disRatio = 0f;

    private bool tmBaseTemp = false;
    private bool disBaseTemp = false;

    public void SpriteAppearLerp()
    {

        if (sRenderer.color != new Color(1, 1, 1, 0))
        {
            sRenderer.color = new Color(1, 1, 1, 0);
        }
        if (useTimerBase)
        {
            tmBaseTemp = true;
        }
        if (useDistanceBase)
        {
            disBaseTemp = true;
        }
        useTimerBase = false;
        useDistanceBase = false;
        StartCoroutine(lerpColor());

    }

    IEnumerator lerpColor()
    {
        Timer colorTimer = new Timer { duration = 2.5f };
        while (!colorTimer.CheckTimer())
        {
            colorTimer.IncTimer(Time.deltaTime);
            sRenderer.color = Color.Lerp(new Color(1,1,1,0), new Color(1, 1, 1, 1), colorTimer.GetRatio());
            yield return null;
        }
        if (tmBaseTemp)
        {
            useTimerBase = true;
        }
        if (disBaseTemp)
        {
            useDistanceBase = true;
        }
        
    }
	public float TimerRatio
	{
		get {
			return timer.timer / timer.duration;
		}
	}
	public float DistanceRatio
	{
		get{
			return Vector3.Distance(disTarget.position, transform.position)/maxColorDis; 
		}
	}

	// Use this for initialization
	void Start () {
		if (null == sRenderer)
			sRenderer = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (useTimerBase)
		{
			var newColor = color.Evaluate (TimerRatio);
			sRenderer.color = newColor;
			timer.IncTimer (Time.deltaTime);
			if (timer.CheckTimer ())
				timer.Reset ();
		}
		else if (useDistanceBase)
		{
			disRatio = DistanceRatio;
			var newColor = color.Evaluate (DistanceRatio);
			sRenderer.color = newColor;
		}
	}
}
