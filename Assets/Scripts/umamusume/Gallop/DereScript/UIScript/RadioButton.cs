using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デレステ仕様のラジオボタン
/// </summary>
public class RadioButton : MonoBehaviour
{
    /// <summary>
    /// 破棄時に毎回保存をするか
    /// </summary>
    public bool autoSave = true;

    public int select = -1;

    /// <summary>
    /// セーブがない時のデフォルト値を指定する
    /// </summary>
    public int defaultValue;

    private int tmpValue = -1;

    [System.Serializable]
    public class IntegerCallback : UnityEngine.Events.UnityEvent<int> { }

    [SerializeField]
    private IntegerCallback OnChangeToggle = new IntegerCallback();

    /// <summary>
    /// トグル名をそのままセーブに使用する
    /// </summary>
    public string toggleName;

    /// <summary>
    /// 表示可になった
    /// </summary>
    public bool isStart = false;

    private Toggle[] Toggles = null;

    // Use this for initialization
    void Start()
    {
        toggleName = base.name;

        Toggles = base.GetComponentsInChildren<Toggle>();

        int save = SaveManager.GetInt(toggleName, defaultValue);

        if (save > 0)
        {
            if (save < Toggles.Length)
            {
                Toggles[save].isOn = true;
                tmpValue = select = save;//確保
            }
        }
        isStart = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// セレクタが変更された
    /// </summary>
    /// <param name="value"></param>
    public void OnToggleStateCheck(bool value)
    {
        //ステータス変更イベントのうちOnの物だけ使う
        if (value)
        {
            if (Toggles != null)
            {
                for (int i = 0; i < Toggles.Length; i++)
                {
                    if (Toggles[i].isOn)
                    {
                        OnChangeToggle.Invoke(i);
                        select = i;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 閉じられたときにセーブ
    /// </summary>
    private void OnDestroy()
    {
        if (autoSave)
        {
            Save();
        }
    }

    /// <summary>
    /// 非表示になったときもセーブ
    /// </summary>
    private void OnDisable()
    {
        if (autoSave)
        {
            Save();
        }
    }

    /// <summary>
    /// 明示的にセーブするときはこちら
    /// </summary>
    public void Save()
    {
        if (Toggles != null)
        {
            if (select != tmpValue)
            {
                if (Toggles[select].isOn)
                {
                    SaveManager.SetInt(toggleName, select);
                    print(toggleName + " " + select);
                    SaveManager.Save();
                    tmpValue = select;
                }

            }
        }
    }
}
