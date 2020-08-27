using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TalkingDialogue : MonoBehaviour {
	[System.Serializable]
	public struct TalkingContent{
		public string rawComment;
		public float width, height;
		public Vector3 startPos;
		public bool isSkipable;
		public Sprite portraitSprite;
		public bool isLeftPortrait;
		[Header("Don't Edit In Editor Mode")]
		public List<TalkingEvent> talkingEvents;
	}
	[System.Serializable]
	public struct TalkingEvent{
		public TalkingEventType eventType;
		public List<string> content;
		public int phaseLength;
	}
	private TalkingDialogueManager dialogueManager;
	public Text commentText;
	public Image commentBackground;
	public Image portrait;
	public Image gradation;
	public Text eventTextObj;
	public TalkingContent[] contents;
	private List<Text> eventTextList = new List<Text>();
	private char[] determineChars = {'{','}'};

	public float typingSpeed = 0.1f;
	public int contentIndex = 0;
	//	0 : normal
	//	1 : Busy
	public int commentUpdatePhase = 0;
	public bool useSkip = false;
	public UnityEvent OnAllContentConsumed;
	
	// Use this for initialization
	void Awake () 
	{
		dialogueManager = GameObject.FindObjectOfType<TalkingDialogueManager>();
		if (portrait != null)
		{
			var prevColor = portrait.color;
			prevColor.a = 0;
			portrait.color = prevColor;
		}
		if (commentBackground != null)
		{
			var prevColor = commentBackground.color;
			prevColor.a = 0;
			commentBackground.color = prevColor;
		}
		if (gradation != null)
		{
			var prevColor = gradation.color;
			prevColor.a = 0;
			gradation.color = prevColor;
		}
		ParseComments();
	}
	public void OnStartViewing()
	{
		var canConsumeContent = (contentIndex < contents.Length);
		if (commentUpdatePhase == 0 && canConsumeContent)
		{
			useSkip = false;
			StartCoroutine(IUpdateComment());
		}
		else if (commentUpdatePhase == 1 && canConsumeContent)
		{
			useSkip = true;
		}
		else if (!canConsumeContent)
		{
			StartCoroutine(IFoldDialogue(0.25f));
		}
	}
	public float paddingAmount = 20f;
	IEnumerator IUpdateComment()
	{
		// 초기화
		int typeCount = 0;
		commentText.text = "";
		commentUpdatePhase = 1;
		int eventCount = 0;
		for (int i = 0; i < eventTextList.Count; i++)
		{
			Destroy(eventTextList[i].gameObject);
		}
		eventTextList.Clear();
		var content = contents[contentIndex];
		commentBackground.rectTransform.localPosition = content.startPos;
		commentText.rectTransform.localPosition = content.startPos;
		commentText.rectTransform.localPosition += (Vector3.right + Vector3.down * 0.5f) * paddingAmount;
		var contentSize = new Vector2(content.width, content.height);
		commentText.rectTransform.sizeDelta = contentSize - Vector2.one * paddingAmount * 2f;
		if (commentBackground.sprite != null)
			yield return ISettingDialogueImages(0.3f);
		
		var rawComment = content.rawComment;
		var talkingEvents = content.talkingEvents;
		while (typeCount != rawComment.Length)
		{
			if (rawComment[typeCount] == '{')
			{
				Debug.Log (eventCount);
				if (talkingEvents[eventCount].eventType == TalkingEventType.COLORING)	//	COLORING = 1
				{
					var newText = GameObject.Instantiate<Text>(eventTextObj, Vector3.zero, Quaternion.identity, commentText.transform);
					newText.rectTransform.localPosition = Vector3.zero;
					newText.rectTransform.sizeDelta = commentText.rectTransform.rect.size;
					eventTextList.Add(newText);
					for (int i = 0; i < typeCount; i++)
					{
						newText.text += " ";
					}
					newText.color = new Color(
						float.Parse(talkingEvents[eventCount].content[0]), 
						float.Parse(talkingEvents[eventCount].content[1]),
						float.Parse(talkingEvents[eventCount].content[2]));
					newText.text += talkingEvents[eventCount].content[3];
					for (int i = 0; i <= talkingEvents[eventCount].phaseLength; i++)
					{
						commentText.text += " ";
					}
				}
				else if (talkingEvents[eventCount].eventType == TalkingEventType.DELAY)	//	DELAY = 0
				{
					yield return new WaitForSeconds(float.Parse(talkingEvents[eventCount].content[0]));
				}
				else if (talkingEvents[eventCount].eventType == TalkingEventType.SOUND)	//	SOUND = 0
				{
					//	Play Sound
				}
                else if (talkingEvents[eventCount].eventType == TalkingEventType.END_LINE)
                {
                    commentText.text += "\n";
                }
				else if (talkingEvents[eventCount].eventType == TalkingEventType.SEND_MESSAGE)
				{
					if (talkingEvents[eventCount].content.Count == 1)
						SendMessage(talkingEvents[eventCount].content[0]);
					else
					{
						SendMessage(talkingEvents[eventCount].content[0], talkingEvents[eventCount].content[1]);
					}
				}
				typeCount += talkingEvents[eventCount].phaseLength + 2;
				eventCount++;
			}
			if (typeCount < rawComment.Length)
			{
				commentText.text += rawComment[typeCount];
				typeCount++;
			}
			if (!useSkip)
				yield return new WaitForSeconds(typingSpeed);
		}
		contentIndex = (contentIndex + 1 > contents.Length) ? contentIndex : contentIndex + 1;
		commentUpdatePhase = 0;
		yield return null;
	}
	IEnumerator ISettingDialogueImages(float duration)
	{
		float timer = 0f;
		var content = contents[contentIndex];
		if (portrait != null && content.portraitSprite != null)
		{
			var prevColor = portrait.color;
			prevColor.a = 1f;
			portrait.color = prevColor;
			portrait.sprite = content.portraitSprite;
		}
		if (commentBackground != null && commentBackground.sprite != null)
		{
			var prevColor = commentBackground.color;
			prevColor.a = 1f;
			commentBackground.color = prevColor;
		}
		if (gradation != null)
		{
			var prevColor = gradation.color;
			prevColor.a = 1f;
			gradation.color = prevColor;
		}
		var contentSize = new Vector2(content.width, content.height);
		var targetBgSize = new Vector2(
			contentSize.x / Mathf.Abs(commentBackground.transform.localScale.x),
			contentSize.y / Mathf.Abs(commentBackground.transform.localScale.y)
		);
		commentBackground.rectTransform.sizeDelta = targetBgSize;
		while (timer <= duration)
		{
			var eval = dialogueManager.dialougeBGCurve.Evaluate(timer/duration);
			commentBackground.transform.localScale = Vector3.Lerp(Vector3.forward + Vector3.right * 3f, Vector3.one * 3f, eval);
			timer += Time.deltaTime;
			if (!useSkip)
			{
				yield return null;
			}
		}
		useSkip = false;
		commentBackground.transform.localScale = Vector3.one * 3f;
		yield return null;
	}
	IEnumerator IFoldDialogue (float duration)
	{
		float timer = 0f;
		commentText.text = "";
		while (timer <= duration)
		{
			var eval = dialogueManager.dialougeBGCurve.Evaluate(timer/duration);
			commentBackground.transform.localScale = Vector3.Lerp(Vector3.one * 3f, Vector3.forward + Vector3.right * 3f, eval);
			timer += Time.deltaTime;
			if (!useSkip)
			{
				yield return null;
			}
		}
        if (portrait.sprite != null)
        {
            var prevPos = portrait.rectTransform.localPosition;
            var targetPos = prevPos;
            var prevPortraitColor = portrait.color;
            if (contents[contentIndex - 1].isLeftPortrait)
                targetPos.x -= 960f;
            else
                targetPos.x += 960f;
            timer = 0f;
            while (timer <= 0.4f)
            {
                var ratio = timer / duration;
                portrait.rectTransform.localPosition = Vector3.Lerp(prevPos, targetPos, ratio);
                prevPortraitColor.a = Mathf.Lerp(1f, 0f, ratio * 1.25f);
                portrait.color = prevPortraitColor;
                timer += Time.deltaTime;
                if (!useSkip)
                    yield return null;
            }
        }
		gradation.color = new Color (1f,1f,1f,0f);
		if (OnAllContentConsumed.GetPersistentEventCount() != 0)
				OnAllContentConsumed.Invoke();
		dialogueManager.SetEmptyDialogueToStopGetInput();
		yield return null;
	}

	void ParseComments()
	{
		for (int i = 0; i < contents.Length; i++)
		{
			var comment = contents[i].rawComment;
			var eventPhases = GetEventPhases(comment);
			foreach (var phase in eventPhases)
			{
				var talkEvent = GetTalkingEventFromPhase(phase);
				contents[i].talkingEvents.Add(talkEvent);
			}
		}	
	}
	List<string> GetEventPhases(string comment)
	{
		bool isEventOpen = false;
		List<string> phases = new List<string>();
		var phase = "";
		for (int i = 0; i < comment.Length; i++)
		{
			if (!isEventOpen && comment[i] == '{')
			{
				isEventOpen = true;
			}
			else if (isEventOpen && comment[i] == '}')
			{
				isEventOpen = false;
				phases.Add(phase);
				phase = "";
			}
			else if (isEventOpen)
			{
				phase += comment[i];
			}
		}
		return phases;
	}
	TalkingEvent GetTalkingEventFromPhase (string phase)
	{
		TalkingEvent talkingEvent = new TalkingEvent();
		talkingEvent.content = new List<string>();
		if (!(phase.Contains("ET:") && phase.Contains("CON:")))
		{
			Debug.LogError("Phase Not Contain Corret Info");
			return talkingEvent;
		}
		int etIndx = phase.IndexOf("ET:") + 3;
		string etData = "";
		bool errFlag = false;
		int i = 0;
		for (i = etIndx; i < phase.Length; i++)
		{
			if (phase[i] == ';')
			{
				errFlag = true;
				break;
			}
			etData += phase[i];
		}
		if (!errFlag)
		{
			Debug.LogError("Phase Not Contain Corret Info");
			return talkingEvent;
		}
		Debug.Log(etData);
		talkingEvent.eventType = (TalkingEventType)int.Parse(etData);
		int conIndx = phase.IndexOf("CON:") + 4;
		string conData = "";
		for (i = conIndx; i < phase.Length; i++)
		{
			if (phase[i] == ';')
			{
				talkingEvent.content.Add(conData);
				conData = "";
				continue;
			}
			conData += phase[i];
		}
		talkingEvent.phaseLength = phase.Length;
		return talkingEvent;
	}
}
