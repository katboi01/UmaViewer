using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEffectSettingView : MonoBehaviour
{
    [SerializeField]
    private GameObject contentPrefab;
    private GameObject content;

    private void Awake()
    {
        base.name = base.name.Replace("(Clone)", "");
        Transform transform = base.transform.Find("Content/ScrollView/Viewport/Content");
        content = GameObject.Instantiate(contentPrefab);
        content.transform.SetParent(transform, false);

        WindowSizeSetter();
    }

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

            //縦長に変更
            if (windowSize.y > 1160f)
            {
                mysize.y = 1100f;
            }
        }

        GetComponent<RectTransform>().localScale = mylocalScale;
        GetComponent<RectTransform>().sizeDelta = mysize;
    }

    // Use this for initialization
    void Start()
    {
        ScrollRect scrollRect = base.transform.Find("Content/ScrollView").GetComponent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 閉じるボタンクリック
    /// </summary>
    public void OnClockClose()
    {
        Destroy(base.gameObject);
    }
}