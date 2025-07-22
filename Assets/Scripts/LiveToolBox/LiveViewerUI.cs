using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class LiveViewerUI : MonoBehaviour
{
    public static LiveViewerUI Instance;

    public UnityEngine.UI.Slider ProgressBar;

    public RectTransform BottonUITransform;

    public Dropdown FrameRateDropDown;

    public GameObject RecordingUI;

    public Text RecordingText;

    public Text LyricsText;

    public List<UmaLyricsData> CurrentLyrics = new List<UmaLyricsData>();

    float targetHeight = 0;
    float height;
    private void Awake()
    {
        height = BottonUITransform.rect.height;
        targetHeight = 0;
        Invoke(nameof(HideSlider), 1.5f);
        Instance = this;
        //TrueProgressBar = (UnityEngine.UIElements.Slider)ProgressBar;
    }

    public void OnMouse(bool isEnter)
    {
        if (isEnter)
        {
            targetHeight = 0;
            CancelInvoke();
        }
        else
        {
            Invoke(nameof(HideSlider), 2.5f);
        }
    }

    private void FixedUpdate()
    {
        BottonUITransform.anchoredPosition = Vector2.Lerp(BottonUITransform.anchoredPosition, new Vector2(0, targetHeight), Time.fixedDeltaTime * 5);
    }

    private void HideSlider()
    {
        targetHeight = -height;
    }

    public void SetFrameRate(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = int.Parse(FrameRateDropDown.options[fps].text);
    }

    public void UpdateLyrics(float time)
    {
        var text = UmaUtility.GetCurrentLyrics(time, CurrentLyrics);
        LyricsText.text = text;
    }
}
