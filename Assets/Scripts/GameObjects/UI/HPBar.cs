using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class HPBar : MonoBehaviour {
	public Actor actor;
	private Material contMat;
	private Material shdwMat;

	public Image contImageRenderer;
	public Image backImageRenderer;
	public Image shdwImageRenderer;

	public Vector3 size;

	public Color defaultColor;
	public Color damagedColor;
    public Gradient shadowGradient;

    public Timer colorFadeTimer = new Timer { duration = 0.5f };
    public Timer vibrateFadeTimer = new Timer { duration = 0.3f };
	
	private Vector3 defaultPos;
	public float vibrationPower = 0.02f;
	public AnimationCurve vixCurve;
	public AnimationCurve viyCurve;
	private Vector3 shdwSize;

	private Animator anim;

    public UnityEvent onValueChanged;

	public void Start ()
	{
        vibrateFadeTimer.timer = vibrateFadeTimer.duration;
		defaultPos = transform.localPosition;
		contImageRenderer.rectTransform.sizeDelta = new Vector2(size.x, size.y);
		backImageRenderer.rectTransform.sizeDelta = new Vector2(size.x, size.y);
		shdwImageRenderer.rectTransform.sizeDelta = new Vector2(size.x, size.y);
		contMat = contImageRenderer.material;
		shdwMat = shdwImageRenderer.material;
		shdwSize.x = size.x;
		actor.RaiseActorStun += (object sender, EventArgumentNamespace.ActorDamagedEventArg e) =>
		{
            colorFadeTimer.timer = colorFadeTimer.duration;
            vibrateFadeTimer.Reset();
			shdwSize.x = shdwImageRenderer.rectTransform.rect.width;
            if (onValueChanged != null)
                onValueChanged.Invoke();
        };
		actor.RaiseActorDamaged += (object sender, EventArgumentNamespace.ActorDamagedEventArg e) => 
		{
            colorFadeTimer.timer = colorFadeTimer.duration;
            vibrateFadeTimer.Reset();
            shdwSize.x = shdwImageRenderer.rectTransform.rect.width;
            if (onValueChanged != null)
                onValueChanged.Invoke();
        };

		defaultColor.r *= 1.45f;
		defaultColor.g *= 1.45f;
		defaultColor.g *= 1.45f;
	}

	public void Update()
	{
		var viRatio = vibrateFadeTimer.GetRatio();
        var coRatio = colorFadeTimer.GetRatio();
		transform.localPosition = defaultPos +
			Vector3.right * vixCurve.Evaluate (viRatio) * vibrationPower +
			Vector3.up * viyCurve.Evaluate (viRatio)* vibrationPower;

        vibrateFadeTimer.IncTimer(Time.deltaTime);
        vibrateFadeTimer.ClampZeroToDuration();

        shdwImageRenderer.color = shadowGradient.Evaluate(coRatio);
        //shdwImageRenderer.color = Color.Lerp (damagedColor, defaultColor, coRatio);
        colorFadeTimer.IncTimer(-Time.deltaTime);
        colorFadeTimer.ClampZeroToDuration();
       
		if (actor.gameObject.activeSelf)
			contImageRenderer.rectTransform.sizeDelta = new Vector3 (size.x * actor.actorInfo.GetLifePercent (), size.y, size.z);
		var newSize = size;
		newSize.x = Mathf.Lerp (contImageRenderer.rectTransform.sizeDelta.x, shdwSize.x, coRatio);
		shdwImageRenderer.rectTransform.sizeDelta = newSize;
	}
}
