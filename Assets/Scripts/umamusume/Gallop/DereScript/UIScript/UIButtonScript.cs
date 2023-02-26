using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonScript : MonoBehaviour
{
    [SerializeField]
    GameObject errorPrefab;
    [SerializeField]
    GameObject SettingPrefab;

    private GameObject Live;

    private GameObject Studio;

    private ToggleSwitch HViewSwitch;
    private ToggleSwitch VViewSwitch;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(CheckDBError());

        Transform switchtransH = base.transform.Find("HorizontalMenu/ViewSwitch");
        if (switchtransH != null) { HViewSwitch = switchtransH.GetComponent<ToggleSwitch>(); }

        Transform switchtransV = base.transform.Find("VerticalMenu/Header/ViewSwitch");
        if (switchtransV != null) { VViewSwitch = switchtransV.GetComponent<ToggleSwitch>(); }

        Transform livetrans = base.transform.Find("LiveContent");
        if (livetrans != null) { Live = livetrans.gameObject; }

        Transform studiotrans = base.transform.Find("PhotoStudio");
        if (studiotrans != null) { Studio = studiotrans.gameObject; }

        OnChangeViewSwitchToggle();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// エラーチェック
    /// </summary>
    private IEnumerator CheckDBError()
    {
        while (true)
        {
            //読み込み完了まで待つ
            if (MasterDBManager.instance.isLoadDB && ManifestManager.instance.isLoad)
            {
                break;
            }
            //エラー発生時
            if (MasterDBManager.instance.isError || ManifestManager.instance.isError)
            {
                GameObject obj = Instantiate(errorPrefab);
                obj.transform.SetParent(GameObject.Find("Canvas").transform, false);

                Text text = obj.transform.Find("Content/ErrorText").GetComponent<Text>();
                text.text = "データベースファイルが存在しません。\n設定からDBのダウンロードを行ってください。";
                yield break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 設定ボタンを押したときの動作
    /// </summary>
    public void OnClickMVSettingButton()
    {
        GameObject baseObj = GameObject.Find("Canvas/ViewSetter");
        Transform transform = baseObj.transform.Find("Setting");
        if (transform == null)
        {
            GameObject obj = Instantiate(SettingPrefab);
            obj.transform.SetParent(baseObj.transform, false);
        }
        else
        {
            transform.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// LiveとPhotoStudioを切り替えるスイッチ
    /// </summary>
    public void OnChangeViewSwitchToggle()
    {
        ToggleSwitch ViewSwitch = null;
        if (HViewSwitch != null && HViewSwitch.isActiveAndEnabled)
        {
            ViewSwitch = HViewSwitch;
        }
        else if (VViewSwitch != null && VViewSwitch.isActiveAndEnabled)
        {
            ViewSwitch = VViewSwitch;
        }

        if (ViewSwitch != null)
        {
            if (ViewSwitch.isOn)
            {
                //Live
                Live.SetActive(true);
                Studio.SetActive(false);
            }
            else
            {
                //Studio
                Live.SetActive(false);
                Studio.SetActive(true);
            }
            ViewSwitch.Save();
        }
    }
}