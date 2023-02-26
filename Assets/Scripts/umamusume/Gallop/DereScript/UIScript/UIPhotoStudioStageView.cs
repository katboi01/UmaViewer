using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class UIPhotoStudioStageView : MonoBehaviour {

    [SerializeField]
    private GameObject stageIcon;

    private Transform content;

    private List<SQPhotoStudioStageData> stageList = null;

    private List<GameObject> stageBtnList = new List<GameObject>();

    // Use this for initialization
    void Start ()
    {
        base.name = base.name.Replace("(Clone)", "");
        StartCoroutine(CreateStageButtons());
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    private IEnumerator CreateStageButtons()
    {
        //Selectから渡されるまでまち
        while (true)
        {
            if (stageList != null)
            {
                break;
            }
            yield return null;
        }

        //親をセット
        content = transform.Find("Content/StageList/Viewport/Content").GetComponent<RectTransform>();

        //ボタンを生成
        foreach (var tmp in stageList)
        {
            GameObject btn = Instantiate(stageIcon);
            stageBtnList.Add(btn);
            //並列で実行
            StartCoroutine(MakeButton(tmp, btn));
        }
    }

    /// <summary>
    /// ボタン生成単体
    /// </summary>
    private IEnumerator MakeButton(SQPhotoStudioStageData data, GameObject btn)
    {
        btn.name = data.bg_fileid.ToString();

        //曲名を整形
        string text = data.disp_name;
        //改行を付与
        text = text.Replace("\\n", "\n");
        text = text.Replace("ステージ ", "ステージ\n");
        text = text.Replace("スクリーン ", "スクリーン\n");
        btn.transform.GetComponentInChildren<Text>().text = text;

        //ボタンにアクションを設定
        Button button = btn.transform.GetComponentInChildren<Button>();
        AddButtonEvent(button, data);

        //親をセット
        btn.transform.SetParent(content, false);

        //Managerを取得
        UIManagerPhotoStudioStage md = GameObject.Find("UIManager").GetComponent<UIManagerPhotoStudioStage>();
        Sprite icondata = null;

        //ICON取得待ち
        while (true)
        {
            icondata = md.GetIconSprite(data.bg_fileid);
            if (icondata != null)
            {
                break;
            }
            yield return null;
        }

        try
        {
            //ボタンアイコン変更
            Image img = btn.transform.Find("IconImage").GetComponent<Image>();
            img.sprite = icondata;
        }
        catch (MissingComponentException e)
        {
            //ボタンの生成前にMusicViewを閉じた場合エラーをはくため
            print(e);
        }
    }

    // ボタンに機能を付与する
    void AddButtonEvent(Button button, SQPhotoStudioStageData data)
    {
        button.onClick.AddListener(() => {
            this.OnClick(data);
            base.gameObject.SetActive(false);
        });
    }

    //Musicが選択されたとき
    private void OnClick(SQPhotoStudioStageData data)
    {
        SelectStage(data.id, data.bg_fileid);
    }

    public void SetStageList(List<SQPhotoStudioStageData> datalist)
    {
        stageList = datalist;
    }


    private void SelectStage(int stageID, int stageIcon = -1)
    {
        if (stageIcon == -1)
        {
            stageIcon = stageID;
        }
        //アイコンセット
        UIPhotoStudioStageSelect photostudioselect = GameObject.Find("Canvas/Panel/PhotoStudio/PhotoStudioStageSelect").GetComponent<UIPhotoStudioStageSelect>();
        StartCoroutine(photostudioselect.StageIconSetter(stageID, stageIcon));
    }

    public void OnViewClose()
    {
        base.gameObject.SetActive(false);
    }
}
