using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwissArmyKnife;
using TMPro;

///=================================================================================================================	<summary>
/// Handles text display                                                                             					</summary>
///=================================================================================================================
public class GameCanvas : Singleton<GameCanvas> {

    [Tooltip("Main text component.")]
    public TMP_Text text;
    [Tooltip("Title text component.")]
    public TMP_Text titleText;
    
    public delegate void TextEvent();
    public static event TextEvent OnTextEnded;  // Throw when texts display end

    private void Awake()
    {
        text.enabled = false;
        titleText.enabled = false;
    }

    /// <summary>
    /// Begin displaying given texts one by one on main display
    /// </summary>
    /// <param name="texts"> Texts array to be displayed </param>
    public void ShowText(string[] texts, float fadeTime = 1f, bool callback = true)
    {
        StartCoroutine(Show_Routine(text, texts, fadeTime, callback));
    }

    /// <summary>
    /// Display a single string in title position
    /// </summary>
    /// <param name="title">Title text</param>
    public void ShowTitle(string title)
    {
        StartCoroutine(Show_Routine(titleText, new string[]{ title }, 3f, false));
    }

    /// <summary>
    /// Simple routine displaying text(s)
    /// </summary>
    /// <param name="txt">Where should we display ?</param>
    /// <param name="texts">Text(s) to display</param>
    /// <param name="fadeTime">Time to show text</param>
    /// <param name="callback">Should we call back ?</param>
    private IEnumerator Show_Routine(TMP_Text txt, string[] texts, float fadeTime, bool callback = true)
    {
        CanvasGroup _group = txt.GetComponent<CanvasGroup>();
        txt.enabled = true;
        int i = 0;
        while (i < texts.Length)
        {
            _group.alpha = 1f;

            txt.text = texts[i];

            for(float t = 0f; t <= fadeTime; t += Time.deltaTime)
            {
                _group.alpha = 1f - (t / fadeTime);
                yield return null;
            }

            i++;
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        _group.alpha = 0;
        txt.enabled = false;

        if(OnTextEnded != null && callback)
        {
            OnTextEnded();
        }
    }
}
