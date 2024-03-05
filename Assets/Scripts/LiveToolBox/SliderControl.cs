using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool is_Touched = false;
    public bool is_Outed = false;

    public Button PauseButton;

    private void Awake()
    {
        PauseButton.onClick.AddListener(OnPauseButtonClick);
    }

    private void OnPauseButtonClick()
    {
        if (is_Touched)
        {
            OnPointerUp(null);
        }
        else
        {
            OnPointerDown(null);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        is_Touched = true;
        PauseButton.GetComponentInChildren<Text>().text = "¡ø";
        Debug.Log("Pause");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        is_Touched = false;
        is_Outed = true;
        PauseButton.GetComponentInChildren<Text>().text = "¡þ";
        Debug.Log("Play");
    }

}
