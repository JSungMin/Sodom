using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextDebugger : MonoBehaviour {
    public static List<string> debugContextPool = new List<string>();
    public static int frameCount = 0;
    public static int textLineCount = 0;
    public int maxLineNum = 20;
    public Text text;
    public Timer clearTimer;

	// Use this for initialization
	void Awake () {
        Application.logMessageReceived += Log;
        StartCoroutine(IContextClearer());
	}
    public static void Log(string message, string stackTrace, LogType logType)
    {
        string context = logType.ToString() + " : " + message;
        debugContextPool.Add(context);
        textLineCount++;
    }

    IEnumerator IContextClearer()
    {
        while (true)
        {
            clearTimer.IncTimer(Time.unscaledDeltaTime);
            if (clearTimer.CheckTimer())
            {
                int overCount = textLineCount - maxLineNum;
                for (int i = 0; i < overCount; i++)
                {
                    int endlIndex = text.text.IndexOf('\n') + 1;
                    text.text = text.text.Remove(0, endlIndex);
                    textLineCount--;
                }
                clearTimer.Reset();
            }
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        frameCount++;
        if (null == text)
            return;

        for (int i = 0; i < debugContextPool.Count; i++)
        {
            text.text += debugContextPool[i] + "\n";
        }
        debugContextPool.Clear();
    }
}
