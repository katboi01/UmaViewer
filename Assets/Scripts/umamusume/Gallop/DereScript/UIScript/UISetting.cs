using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISetting : MonoBehaviour {

    [SerializeField]
    private GameObject menuSetting;
    private GameObject menu;
    private float menuScrollRect = 1f;

    [SerializeField]
    private GameObject liveSetting;
    private GameObject live;
    private float liveScrollRect = 1f;

    private ToggleSwitch settingSwitch;
    private ScrollRect scrollRect;

    private void WindowSizeSetter()
    {
        //ウィンドウのサイズを取得
        Vector2 windowSize = GameObject.Find("Canvas").GetComponent<RectTransform>().sizeDelta;
        
        //自分のサイズを取得
        Vector3 mylocalScale = GetComponent<RectTransform>().localScale;
        Vector2 mysize = GetComponent<RectTransform>().sizeDelta;

        //縦長の場合
        if (windowSize.y > windowSize.x)
        {
            //はみ出すため小さくする
            mylocalScale.x = 0.8f;
            mylocalScale.y = 0.8f;

            if (windowSize.y > 1160f)
            {
                mysize.y = 1100f;
            }
        }

        GetComponent<RectTransform>().localScale = mylocalScale;
        GetComponent<RectTransform>().sizeDelta = mysize;
    }

    private void Awake()
    {
        base.name = base.name.Replace("(Clone)", "");
        Transform transform = base.transform.Find("Content/ScrollView/Viewport/Content");

        settingSwitch = base.transform.Find("Header/SettingSwitch").GetComponent<ToggleSwitch>();

        menu = GameObject.Instantiate(menuSetting);
        live = GameObject.Instantiate(liveSetting);
        menu.transform.SetParent(transform, false);
        live.transform.SetParent(transform, false);

        menu.SetActive(false);
        live.SetActive(false);

        int save = SaveManager.GetInt("SettingSwitch");

        if (save == 0)
        {
            live.SetActive(true);
        }
        else
        {
            menu.SetActive(true);
        }

        scrollRect = base.transform.Find("Content/ScrollView").GetComponent<ScrollRect>();

        WindowSizeSetter();
    }
    // Use this for initialization
    void Start()
    {
        StartCoroutine(SetScrollRect(1f));
    }
    
    // Update is called once per frame
    void Update () {
        
    }

    public void OnClickClose()
    {
        base.gameObject.SetActive(false);
    }

    public void OnChangePageToggle()
    {
        if (settingSwitch.isOn)
        {
            liveScrollRect = scrollRect.verticalNormalizedPosition;
            live.SetActive(false);
            menu.SetActive(true);
            StartCoroutine(SetScrollRect(menuScrollRect));
        }
        else
        {
            menuScrollRect = scrollRect.verticalNormalizedPosition;
            live.SetActive(true);
            menu.SetActive(false);
            StartCoroutine(SetScrollRect(liveScrollRect));
        }
    }

    /// <summary>
    /// ScrollRectはすぐには変更されないため1フレーム待つ必要がある
    /// </summary>
    private IEnumerator SetScrollRect(float pos)
    {
        //1フレーム待つ
        yield return null;

        scrollRect.verticalNormalizedPosition = pos;
    }
}
