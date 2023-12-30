using UnityEngine;

public class UICharaSort : MonoBehaviour
{
    private RadioButton radioButton = null;
    private int initValue;

    private void Awake()
    {
        radioButton = base.GetComponentInChildren<RadioButton>();
        radioButton.autoSave = false;
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
        }

        GetComponent<RectTransform>().localScale = mylocalScale;
        GetComponent<RectTransform>().sizeDelta = mysize;
    }

    // Use this for initialization
    void Start()
    {
        base.name = base.name.Replace("(Clone)", "");

        LoadSavedata();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        LoadSavedata();
    }

    /// <summary>
    /// セーブデータを読み込んで反映
    /// </summary>
    private void LoadSavedata()
    {
        initValue = SaveManager.GetInt("CharaSortSelect");
    }

    private void Save()
    {
        int nowvalue = radioButton.select;
        if (initValue != nowvalue)
        {
            radioButton.Save();
        }
    }

    /// <summary>
    /// 決定ボタンクリック
    /// </summary>
    public void OnClickOK()
    {
        Save();

        GameObject obj = GameObject.Find("Canvas/CharaView");
        obj.GetComponent<UICharaView>().ReVisible();

        base.gameObject.SetActive(false);
    }

    /// <summary>
    /// 閉じるボタンクリック
    /// </summary>
    public void OnClickClose()
    {
        base.gameObject.SetActive(false);
    }
}
