using UnityEngine;

public class UISettingContent : MonoBehaviour
{

    private ToggleSwitch[] toggleList = null;

    [SerializeField]
    private GameObject EffectSetting;
    [SerializeField]
    GameObject DBNotice;

    bool[] initValue = null;

    // Use this for initialization
    void Start()
    {
        ValueInit();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 2回目に設定を開いたとき
    /// </summary>
    private void OnEnable()
    {
        ValueInit();
    }

    /// <summary>
    /// 設定を閉じたとき
    /// </summary>
    private void OnDisable()
    {
        CheckChange();
    }

    /// <summary>
    /// 設定を開いた時点での値を確保する
    /// </summary>
    /// <returns></returns>
    private void ValueInit()
    {
        toggleList = base.GetComponentsInChildren<ToggleSwitch>();

        if (toggleList != null)
        {
            initValue = new bool[toggleList.Length];
            for (int i = 0; i < toggleList.Length; i++)
            {
                initValue[i] = toggleList[i].isOn;
            }
        }
    }

    /// <summary>
    /// 設定変更によってメニューを更新する処理
    /// </summary>
    private void CheckChange()
    {
        if (toggleList != null)
        {
            for (int i = 0; i < toggleList.Length; i++)
            {
                if (initValue[i] != toggleList[i].isOn)
                {
                    switch (toggleList[i].toggleName)
                    {
                        case "SSRIcon":
                            SQDressData.isPlus = toggleList[i].isOn;
                            ReloadIcons();
                            break;

                        case "ScreenMode":
                            OnChangeScreenMode();
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// ドレスアイコンの更新を反映
    /// </summary>
    private void ReloadIcons()
    {
        ReLoadDressIcons();

        Transform transform = GameObject.Find("Canvas/Panel").transform;

        Transform idolInfo = transform.Find("LiveContent/IdolInfo");
        if (idolInfo.gameObject.activeInHierarchy)
        {
            idolInfo.GetComponent<UIIdolSelect>().ReLoadIcons();
        }

        Transform photoStudioIdolSelect = transform.Find("PhotoStudio/PhotoStudioIdolSelect");
        if (photoStudioIdolSelect.gameObject.activeInHierarchy)
        {
            photoStudioIdolSelect.GetComponent<UIPhotoStudioChara>().ReLoadIcons();
        }
    }

    /// <summary>
    /// SSRアイコン設定を変更したときに使用
    /// </summary>
    public void ReLoadDressIcons()
    {
        UIManagerDress dressManager = GameObject.Find("UIManager").GetComponent<UIManagerDress>();

        for (int i = 0; i < 10; i++)
        {
            int dressid = SaveManager.GetDress(i);

            if (dressid == 0) continue; //なにもセットされていない場合

            SQDressData data = dressManager.GetDressDataFromKeyID(dressid);

            if (data.activeDressID > 0 && data.dressIconKey > 0)
            {
                SaveManager.SetDress(i, data.activeDressID);
                SaveManager.SetDressIcon(i, data.dressIconKey);

                SaveManager.Save();
            }
        }

        //AppendSSRアイコンの切り替え
        dressManager.ReloadSSRIcons();
    }

    /// <summary>
    /// エフェクト設定を開く
    /// </summary>
    public void OnClickEffectSetting()
    {
        //prefabを実装
        GameObject gameObject = GameObject.Instantiate(EffectSetting);

        Transform transform = GameObject.Find("Canvas/ViewSetter").transform;

        //ViewSetter下に実装
        gameObject.transform.SetParent(transform, false);
    }

    /// <summary>
    /// DBUpdateボタンを押したときの動作
    /// </summary>
    public void OnClickDBUpdateButton()
    {
        GameObject obj = Instantiate(DBNotice);
        GameObject baseObj = GameObject.Find("Canvas");
        obj.transform.SetParent(baseObj.transform, false);

        var instance = obj.GetComponent<UIDBNotice>();
        instance.UpdateDatabase();
    }

    public void OnChangeScreenMode()
    {
#if UNITY_EDITOR
#elif UNITY_STANDALONE
        if (toggleList != null)
        {
            for (int i = 0; i < toggleList.Length; i++)
            {
                if (initValue[i] != toggleList[i].isOn)
                {
                    switch (toggleList[i].toggleName)
                    {
                        case "ScreenMode":
                             if (toggleList[i].isOn)
                            {
                                SaveManager.SetInt("ScreenWidth", Screen.width);
                                SaveManager.SetInt("ScreenHeight", Screen.height);
                                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
                            }
                            else
                            {
                                int width = SaveManager.GetInt("ScreenWidth", 1280);
                                int height = SaveManager.GetInt("ScreenHeight", 720);
                                Screen.SetResolution(width, height, false);
                            }
                            break;
                    }
                }
            }
        }
                           
#endif
    }
}
