using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlaySeeker : MonoBehaviour
{
    private Slider slider;
    private MusicManager musicManager;

    private GameObject rootObject;

    private Text currentTime;
    private Text totalTime;

    private bool isSet = false;
    private bool isUpdateExec;

    private float total;
    private float current;

    private void Awake()
    {
        musicManager = GameObject.Find("Music").GetComponent<MusicManager>();

        rootObject = base.transform.Find("UItop").gameObject;
        slider = base.transform.Find("UItop/Slider").GetComponent<Slider>();
        totalTime = base.transform.Find("UItop/TotalTime").GetComponent<Text>();
        currentTime = base.transform.Find("UItop/CurrentTime").GetComponent<Text>();

        rootObject.SetActive(false);
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        isUpdateExec = true;
        if (musicManager.isLoad)
        {
            if (!isSet)
            {
                total = slider.maxValue = musicManager.totalLength;

                TimeSpan ts = TimeSpan.FromSeconds(total);
                string curstr = string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
                totalTime.text = curstr;
                isSet = true;
            }
            else
            {
                float cur = current = musicManager.currentTime;
                if (total < current)
                {
                    cur = total;
                }
                slider.value = cur;
                TimeSpan ts = TimeSpan.FromSeconds(cur);
                string curstr = string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
                currentTime.text = curstr;

            }
        }
        isUpdateExec = false;
    }

    public void OnSlideBar(float value)
    {
        if (!isUpdateExec)
        {
            musicManager.currentTime = value;
        }
    }

    public void OnClickButton()
    {
        if (musicManager.isStart)
        {
            if (musicManager.isPause)
            {
                musicManager.ResumeSong();
            }
            else
            {
                musicManager.PauseSong();
            }
        }
    }

    public void ChangeVisible()
    {
        if(rootObject != null)
        {
            bool active = rootObject.activeSelf;
            rootObject.SetActive(!active);
        }
    }
}