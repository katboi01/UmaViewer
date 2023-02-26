using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class UICharaRefine : MonoBehaviour {

    [SerializeField]
    private bool[] visibleSelect = new bool[] { true, true, true, true };
    private bool[] lastValue = new bool[] { true, true, true, true };

    private Toggle Cute = null;
    private Toggle Cool = null;
    private Toggle Passion = null;
    private Toggle Other = null;

    private ToggleSwitch pageToggle = null;

    private InputField inputField;
    private string searchString = "";

    GameObject Select = null;
    GameObject Search = null;

    private bool enterWait = false;

    // Use this for initialization
    void Start()
    {
        base.name = base.name.Replace("(Clone)", "");

        LoadSavedata();
    }

    private void Awake()
    {
        var toggles = transform.Find("Content/Select/Type/ContentArea/Cute");
        if(toggles != null)
        {
            Cute = toggles.GetComponent<Toggle>();
        }
        toggles = transform.Find("Content/Select/Type/ContentArea/Cool");
        if (toggles != null)
        {
            Cool = toggles.GetComponent<Toggle>();
        }
        toggles = transform.Find("Content/Select/Type/ContentArea/Passion");
        if (toggles != null)
        {
            Passion = toggles.GetComponent<Toggle>();
        }
        toggles = transform.Find("Content/Select/Type/ContentArea/Other");
        if (toggles != null)
        {
            Other = toggles.GetComponent<Toggle>();
        }
        var toggleSwitch = transform.Find("Header/CharaRefineSelect");
        if (toggleSwitch != null)
        {
            pageToggle = toggleSwitch.GetComponent<ToggleSwitch>();
        }
        var inputfield = transform.Find("Content/Search/InputField");
        if (inputfield != null)
        {
            inputField = inputfield.GetComponent<InputField>();
        }

        Select = transform.Find("Content/Select").gameObject;
        Search = transform.Find("Content/Search").gameObject;

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

    private void OnEnable()
    {
        LoadSavedata();
        enterWait = false;
        if (Search.activeInHierarchy)
        {
            inputField.ActivateInputField();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (enterWait && Input.GetKeyDown(KeyCode.Return))
        {
            OnClickOK();
        }
    }


    /// <summary>
    /// セーブデータを読み込んで反映
    /// </summary>
    private void LoadSavedata()
    {
        visibleSelect = new bool[] { true, true, true, true };
        lastValue = new bool[] { true, true, true, true };

        if (SaveManager.GetInt("CharaCuteVisible") == -1)
        {
            visibleSelect[0] = lastValue[0] = false;
        }
        if (SaveManager.GetInt("CharaCoolVisible") == -1)
        {
            visibleSelect[1] = lastValue[1] = false;
        }
        if (SaveManager.GetInt("CharaPassionVisible") == -1)
        {
            visibleSelect[2] = lastValue[2] = false;
        }
        if (SaveManager.GetInt("CharaOtherVisible") == -1)
        {
            visibleSelect[3] = lastValue[3] = false;
        }

        searchString = SaveManager.GetString("SearchCharaString");
        inputField.text = searchString;

        Select.SetActive(false);
        Search.SetActive(false);
        if (SaveManager.GetInt("CharaRefineSelect") == 1)
        {
            Select.SetActive(true);
        }
        else
        {
            Search.SetActive(true);
        }

        Cute.isOn = visibleSelect[0];
        Cool.isOn = visibleSelect[1];
        Passion.isOn = visibleSelect[2];
        Other.isOn = visibleSelect[3];
    }

    /// <summary>
    /// 属性表示ボタン変更（キュート）
    /// </summary>
    /// <param name="b"></param>
    public void OnToggleChangeCute(bool b)
    {
        bool value = Cute.isOn;
        if (value)
        {
            visibleSelect[0] = true;
            //SaveManager.SetInt("CharaCuteVisible", 1);
        }
        else
        {
            visibleSelect[0] = false;
            //SaveManager.SetInt("CharaCuteVisible", -1);
        }
        //SaveManager.Save();
    }

    /// <summary>
    /// 属性表示ボタン変更（クール）
    /// </summary>
    /// <param name="b"></param>
    public void OnToggleChangeCool(bool b)
    {

        bool value = Cool.isOn;
        if (value)
        {
            visibleSelect[1] = true;
            //SaveManager.SetInt("CharaCoolVisible", 1);
        }
        else
        {
            visibleSelect[1] = false;
           // SaveManager.SetInt("CharaCoolVisible", -1);

        }
        //SaveManager.Save();
    }

    /// <summary>
    /// 属性表示ボタン変更（パッション）
    /// </summary>
    /// <param name="b"></param>
    public void OnToggleChangePassion(bool b)
    {

        bool value = Passion.isOn;
        if (value)
        {
            visibleSelect[2] = true;
            //SaveManager.SetInt("CharaPassionVisible", 1);
        }
        else
        {
            visibleSelect[2] = false;
            //SaveManager.SetInt("CharaPassionVisible", -1);

        }
        //SaveManager.Save();
    }

    /// <summary>
    /// 属性表示ボタン変更（その他）
    /// </summary>
    /// <param name="b"></param>
    public void OnToggleChangeOther(bool b)
    {

        bool value = Other.isOn;
        if (value)
        {
            visibleSelect[3] = true;
            //SaveManager.SetInt("CharaPassionVisible", 1);
        }
        else
        {
            visibleSelect[3] = false;
            //SaveManager.SetInt("CharaPassionVisible", -1);

        }
        //SaveManager.Save();
    }

    private void Save()
    {
        int value = 0;
        value = visibleSelect[0] ?  1 : -1;
        if(visibleSelect[0] != lastValue[0]) SaveManager.SetInt("CharaCuteVisible", value);

        value = visibleSelect[1] ? 1 : -1;
        if (visibleSelect[1] != lastValue[1]) SaveManager.SetInt("CharaCoolVisible", value);

        value = visibleSelect[2] ? 1 : -1;
        if (visibleSelect[2] != lastValue[2]) SaveManager.SetInt("CharaPassionVisible", value);

        value = visibleSelect[3] ? 1 : -1;
        if (visibleSelect[3] != lastValue[3]) SaveManager.SetInt("CharaOtherVisible", value);

        searchString = inputField.text;
        SaveManager.SetString("SearchCharaString", searchString);

        SaveManager.Save();

        lastValue[0] = visibleSelect[0];
        lastValue[1] = visibleSelect[1];
        lastValue[2] = visibleSelect[2];
        lastValue[3] = visibleSelect[3];
    }

    private void rollback()
    {
        //LoadSavedata();
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
        rollback();
        base.gameObject.SetActive(false);
    }

    /// <summary>
    /// 初期化ボタンをクリック
    /// </summary>
    public void OnClickClear()
    {
        if (Cute.isOn && Cool.isOn && Passion.isOn && Other.isOn && inputField.text == "")
        {
            visibleSelect = new bool[] { false, false, false, false };
        }
        else
        {
            visibleSelect = new bool[] { true, true, true, true };
        }
        Cute.isOn = visibleSelect[0];
        Cool.isOn = visibleSelect[1];
        Passion.isOn = visibleSelect[2];
        Other.isOn = visibleSelect[3];

        searchString = "";
        inputField.text = searchString;
    }

    public void OnChangePageToggle()
    {
        if (pageToggle.isOn)
        {
            Select.SetActive(true);
            Search.SetActive(false);
        }
        else
        {
            Select.SetActive(false);
            Search.SetActive(true);
            inputField.ActivateInputField();
        }
    }

    public void OnEndInputText()
    {
        enterWait = true;
    }
}
