using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class LiveViewerUI : MonoBehaviour
{
    public static LiveViewerUI Instance;

    public UnityEngine.UI.Slider ProgressBar;

    private void Awake()
    {
        Instance = this;
        //TrueProgressBar = (UnityEngine.UIElements.Slider)ProgressBar;
    }
}
