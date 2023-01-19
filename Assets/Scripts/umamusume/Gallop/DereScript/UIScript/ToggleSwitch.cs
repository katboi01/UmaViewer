using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ToggleSwitch : MonoBehaviour
{
    /// <summary>
    /// ToggleSwitchの値がオンになっているかどうか
    /// </summary>
    private bool _isOn = false;

    public bool isOn
    {
        get
        {
            if (isStart)
            {
                return _isOn;
            }
            else
            {
                if (noSaveValue) { return DefaultValue; }
                int def = DefaultValue == true ? 1 : 0;
                return SaveManager.GetInt(base.name, def) == 1;
            }
        }
        set
        {
            if (value && isStart)
            {
                rightToggle.isOn = false;
                leftToggle.isOn = true;
            }
        }
    }

    /// <summary>
    /// セーブデータに値を保存しない場合
    /// </summary>
    public bool noSaveValue = false;

    /// <summary>
    /// セーブに値がない場合のデフォルト値
    /// </summary>
    public bool DefaultValue = false;

    /// <summary>
    /// 前回セーブ値との比較をするために残しておく
    /// </summary>
    private bool tmpValue = false;

    [System.Serializable]
    public class BoolenCallback : UnityEngine.Events.UnityEvent<bool> { }

    [SerializeField]
    private BoolenCallback OnChangeToggle = new BoolenCallback();

    private Toggle leftToggle = null;
    private Toggle rightToggle = null;
    private Text leftDown;
    private Text rightDown;

    /// <summary>
    /// ToggleSwitchが使用可能になっているか
    /// </summary>
    private bool isStart = false;

    public string toggleName;

    private void Awake()
    {
        toggleName = base.name;

        leftToggle = base.transform.Find("ToggleLeft").GetComponent<Toggle>();
        rightToggle = base.transform.Find("ToggleRight").GetComponent<Toggle>();
        leftDown = base.transform.Find("ToggleLeft/Checkmark/LeftDown").GetComponent<Text>();
        rightDown = base.transform.Find("ToggleRight/Checkmark/RightDown").GetComponent<Text>();

        int def = DefaultValue == true ? 1 : 0;
        int save = def;

        //セーブを使用しない場合はデフォルト値
        if (!noSaveValue)
        {
            save = SaveManager.GetInt(toggleName, def);
        }

        if (save == 1)
        {
            tmpValue = _isOn = true;
        }
    }

    // Use this for initialization
    void Start()
    {
        if (isOn)
        {
            leftToggle.isOn = true;
        }
        else
        {
            rightToggle.isOn = true;
        }
        TextSet();
        leftToggle.onValueChanged.AddListener(OnToggleStateCheck);

        isStart = true;
    }

    void OnToggleStateCheck(bool value)
    {
        _isOn = value;
        TextSet();

        //イベントを投げる
        OnChangeToggle.Invoke(_isOn);
    }

    void TextSet()
    {
        if (isOn)
        {
            leftDown.enabled = true;
            rightDown.enabled = false;
        }
        else
        {
            leftDown.enabled = false;
            rightDown.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// 閉じられたときにセーブ
    /// </summary>
    private void OnDestroy()
    {
        if (toggleName != "")
        {
            Save();
        }
    }

    /// <summary>
    /// 非表示になったときもセーブ
    /// </summary>
    private void OnDisable()
    {
        if (toggleName != "")
        {
            Save();
        }
    }

    /// <summary>
    /// 明示的にセーブするときはこちら
    /// </summary>
    public void Save()
    {
        //セーブしない
        if (noSaveValue) { return; }

        //変化があったときはセーブ
        if (tmpValue != _isOn)
        {
            if (_isOn)
            {
                SaveManager.SetInt(toggleName, 1);
                print(toggleName + " " + 1);
            }
            else
            {
                SaveManager.SetInt(toggleName, 0);
                print(toggleName + " " + 0);
            }
            SaveManager.Save();
            tmpValue = _isOn;
        }
    }
}