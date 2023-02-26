using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIPhotoStudioStageSelect : MonoBehaviour
{
    protected GameObject UIManager = null;

    [SerializeField]
    public GameObject stage;

    [SerializeField]
    public int currentStage;

    // Use this for initialization
    void Start()
    {
        currentStage = -1;
        UIManager = GameObject.Find("UIManager");

        Button button = stage.transform.GetComponentInChildren<Button>();
        AddCharaViewEvent(button);

        //セーブデータを読み込む
        LoadingSavedata();
    }

    // Update is called once per frame
    void Update()
    {
        //読みに行く
        if (UIManager == null)
        {
            UIManager = GameObject.Find("UIManager");
        }
    }

    private void LoadingSavedata()
    {
        int stagedata = SaveManager.GetInt("stage", -1);
        int stageicon = SaveManager.GetInt("stageicon", -1);

        if (stageicon > 0)
        {
            List<string> filelist = new List<string>(2);
            filelist.Add(string.Format("live_bg2d_bg_live_{00}.unity3d", stageicon));

            StartCoroutine(AssetManager.instance.DownloadFromFilenamesAsync(filelist));
        }

        StartCoroutine(StageIconSetter(stagedata, stageicon));

    }
    public IEnumerator StageIconSetter(int stagedata, int stageicon)
    {
        if (stageicon < 0)
        {
            Sprite sprite = Resources.Load<Sprite>("noneStage");
            Image img = stage.GetComponent<Image>();
            img.sprite = sprite;
        }

        if (stagedata < 0)
        {
            Text text = stage.GetComponentInChildren<Text>();
            text.text = "";
        }

        //データがない場合は終了
        if (stageicon < 0 && stagedata < 0)
        {
            yield break;
        }

        SaveManager.SetInt("stage", stagedata);
        SaveManager.SetInt("stageicon", stageicon);
        SaveManager.Save();

        Sprite sp = UIManager.GetComponent<UIManagerPhotoStudioStage>().GetIconSprite(stageicon);

        if (sp == null)
        {
            //割り込みで読み込む
            yield return UIManager.GetComponent<UIManagerPhotoStudioStage>().CreateSprite(stageicon);

            sp = UIManager.GetComponent<UIManagerPhotoStudioStage>().GetIconSprite(stageicon);
            if (sp == null)
            {
                yield break;
            }
        }
        Image image = stage.GetComponent<Image>();
        image.sprite = sp;
    }


    public void OnClickPhotoStudioSelect()
    {
        Debug.Log("ClickStageSelect");

        GameObject obj = GameObject.Find("Canvas");
        Transform tf = obj.transform.Find("PhotoStudioStageView");
        if (tf != null)
        {
            //既に1回呼び出している
            tf.gameObject.SetActive(true);
        }
        else
        {
            UIManagerPhotoStudioStage ms = UIManager.GetComponent<UIManagerPhotoStudioStage>();
            if (ms.isLoad)
            {
                ms.CreateStageView();
            }
        }
    }

    /// <summary>
    /// キャラボタンにイベントを設置
    /// </summary>
    protected void AddCharaViewEvent(Button button)
    {
        button.onClick.AddListener(() =>
        {
            this.OnClickPhotoStudioSelect();
        });
    }
}