using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserLauncher : MonoBehaviour {
	public Actor actor;
	public SpriteRenderer laserRenderer;
	public GameObject laserEndObj;
	public Transform root;
	public LayerMask laserMask;
	public AttackCollider laserCollider;
	private Vector3 startPoint;
	private RaycastHit hit;

	public Light laserLight;
	public float lightIntensity = 1.5f;

	public Light laserEndLight;
	public float endLightIntensity = 0.5f;

	//	Graphic Setting Variables
	public float laserThickness = 0.125f;
	bool useFadeEffect = true;
	public float fadeInTime = 0.25f;
	public float fadeOutTime = 0.5f;
	public AnimationCurve fadeInCurve;
	public AnimationCurve fadeOutCurve;

	public float duration = 2f;
	public float timer = 0f;
	public float startAngle;
	public float endAngle;

	//	if set to true then 한 번 레이저가 여러번의 타격을 입힙니다.
	public bool useAutoRefresh = false;
	public int maxHit = 5;
	private int curHit = 0;
	public float refreshRate = 0.1f;
	public float refreshTimer = 0f;

	public bool isActived = false;
	private float timeScale = 1f;

	public void Start ()
	{
		laserLight.intensity = 0f;
		actor = GetComponentInParent<Actor> ();
		laserRenderer = GetComponent<SpriteRenderer> ();
		laserRenderer.enabled = false;
		laserCollider.OnAttackSuccess += delegate() {
			curHit++;
		};
	}

	public void OnLaunch (string skillName)
	{
		curHit = 0;
		transform.localRotation = Quaternion.AngleAxis (startAngle, Vector3.forward);
		laserCollider.gameObject.SetActive (true);
		laserRenderer.enabled = true;
		refreshTimer = 0f;
		laserCollider.GetComponent<AttackCollider> ().Initialize (actor, actor.GetLearndSkill (skillName));
		if (useFadeEffect) {
			StopCoroutine ("IFadeOut");
			StartCoroutine ("IFadeIn");
		}
	}
	public void OnRelease ()
	{
		isActived = false;
		if (useFadeEffect)
			StartCoroutine ("IFadeOut");
		else
			laserRenderer.enabled = false;
	}

	public IEnumerator IFadeIn ()
	{
		isActived = true;
		var timer = 0f;
		var prevScale = transform.localScale;
		laserEndObj.GetComponent<ParticleSystem> ().Play ();
		laserEndLight.gameObject.SetActive (true);
		laserEndLight.intensity = endLightIntensity;
		while (timer <= fadeInTime) {
			timer += Time.deltaTime;
			var fadeValue = fadeInCurve.Evaluate (timer / duration);
			prevScale.y = laserThickness * fadeValue;
			laserLight.intensity = lightIntensity * fadeValue;
			transform.localScale = prevScale;
			yield return new WaitForEndOfFrame();
		}
		//laserCollider.gameObject.SetActive (false);	
		laserLight.intensity = lightIntensity;
		prevScale.y = laserThickness;
		transform.localScale = prevScale;
	}
	public IEnumerator IFadeOut ()
	{
		var timer = 0f;
		var prevScale = transform.localScale;
		laserCollider.Release (actor);
		laserCollider.gameObject.SetActive (false);
		isActived = false;
		while (timer <= fadeOutTime) {
			timer += Time.deltaTime;
			var fadeValue = fadeOutCurve.Evaluate (timer / duration);
			prevScale.y = laserThickness * fadeValue;
			laserLight.intensity = lightIntensity * fadeValue;
			laserEndLight.intensity = endLightIntensity * fadeValue;
			transform.localScale = prevScale;
			yield return new WaitForEndOfFrame();
		}
		laserLight.intensity = 0f;
		this.timer = 0f;
		laserEndObj.GetComponent<ParticleSystem> ().Stop();
		laserEndLight.gameObject.SetActive (false);
		prevScale.y = 0f;
		transform.localScale = prevScale;
		laserRenderer.enabled = false;
		dirPos = Mathf.Lerp (startAngle, endAngle, 0f);
		if (actor.lookDirection == 1)
			dir = Quaternion.Euler (0, 0, dirPos - root.localRotation.z) * Vector3.right;
		else
			dir = Quaternion.Euler (0, 0, -(dirPos - root.localRotation.z)) * Vector3.left;
		if (Physics.Raycast (startPoint, dir.normalized, out hit, 100, laserMask.value)) 
		{
			laserEndObj.transform.position = hit.point;
			laserEndObj.transform.localRotation = Quaternion.AngleAxis(-dirPos, Vector3.forward);
			transform.localScale = new Vector3 (hit.distance, transform.localScale.y, transform.localScale.z);
		}
	}
	private float dirPos;
	private Vector3 dir;
	// Update is called once per frame
	void Update () {
		startPoint = transform.position;
		dirPos = Mathf.Lerp (startAngle, endAngle, timer / duration);
		if (isActived) 
		{
			timer += Time.deltaTime;
			transform.localRotation = Quaternion.AngleAxis (dirPos, Vector3.forward);
			if (timer >= duration) 
			{
				timer = 0f;
				OnRelease ();
				return;
			}
			dir = Vector3.zero;
			if (actor.lookDirection == 1)
				dir = Quaternion.Euler (0, 0, dirPos - root.localRotation.z) * Vector3.right;
			else
				dir = Quaternion.Euler (0, 0, -(dirPos - root.localRotation.z)) * Vector3.left;
			
			if (Physics.Raycast (startPoint, dir.normalized, out hit, 100, laserMask.value)) 
			{
				laserEndObj.transform.position = hit.point;
				laserEndObj.transform.localRotation = Quaternion.AngleAxis(-dirPos, Vector3.forward);
				transform.localScale = new Vector3 (hit.distance, transform.localScale.y, transform.localScale.z);
			}
			if (!useAutoRefresh)
				return;
			if (curHit >= maxHit)
				return;
			refreshTimer += Time.deltaTime;
			if (refreshTimer >= refreshRate) 
			{
				refreshTimer = 0f;
				laserCollider.victimDictionary.Clear ();
			}
		}
	}
}
