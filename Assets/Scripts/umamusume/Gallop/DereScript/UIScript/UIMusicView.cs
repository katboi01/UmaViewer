using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class UIMusicView : MonoBehaviour
{

    [SerializeField]
    private GameObject musicIcon;

    private Transform content;

    private List<SQMusicData> musicList = new List<SQMusicData>();

    private bool[] visibleSelect = new bool[] { true, true, true, true, true };

    [SerializeField]
    private GameObject MusicRefineView;

    [SerializeField]
    private GameObject MusicSortView;

    private GameObject SortChangeBtn;

    private GameObject SortSelectBtn;

    private Sprite[] icons = null;

    private bool smartmode = false;
    private string searchString = "";
    private int sortSelect;
    private bool sortOrder;

    private Sprite[] frameSprites = null;

    private void Awake()
    {
        SortChangeBtn = base.transform.Find("Header/SortingButton/RightButton").gameObject;
        SortSelectBtn = base.transform.Find("Header/SortingButton/LeftButton").gameObject;

        LoadFrameSprite();

        icons = new Sprite[2];
        icons[0] = Resources.Load<Sprite>("up");
        icons[1] = Resources.Load<Sprite>("down");

        WindowSizeSetter();
    }

    private void WindowSizeSetter()
    {
        //ウィンドウのサイズを取得
        Vector2 windowSize = GameObject.Find("Canvas").GetComponent<RectTransform>().sizeDelta;

        //レイアウトを取得
        GridLayoutGroup gridLayoutGroup = base.GetComponentInChildren<GridLayoutGroup>();
        float contentXsize = gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x;

        //自分のサイズを取得
        Vector2 mysize = GetComponent<RectTransform>().sizeDelta;

        Vector2 setSize;
        setSize.x = 30f;

        //横コンテンツ数は最大7個
        for (int i = 0; i < 7; i++)
        {
            if (setSize.x + contentXsize > windowSize.x)
            {
                break;
            }
            else
            {
                //横幅に余裕があるなら増やしていく
                setSize.x += contentXsize;
            }
        }

        //縦サイズ
        //デフォルトサイズをセット
        setSize.y = mysize.y;
        //縦長の場合
        if (windowSize.y > windowSize.x)
        {
            if (windowSize.y > 1000f)
            {
                setSize.y = 950f;
            }
        }

        GetComponent<RectTransform>().sizeDelta = setSize;
    }

    // Use this for initialization
    void Start()
    {
        LoadSavedata();
        base.name = base.name.Replace("(Clone)", "");
        StartCoroutine(CreateMusicButtons());
        ReViewSortSelect();
        ReViewSortChange();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// セーブデータを読み込んで反映
    /// </summary>
    private void LoadSavedata()
    {
        //セーブがオンの時だけ処理
        if (SaveManager.GetInt("SmartMode") == 1)
        {
            smartmode = true;
        }

        visibleSelect = new bool[] { true, true, true, true, true };
        if (SaveManager.GetInt("MusicCuteVisible") == -1)
        {
            visibleSelect[0] = false;
        }
        if (SaveManager.GetInt("MusicCoolVisible") == -1)
        {
            visibleSelect[1] = false;
        }
        if (SaveManager.GetInt("MusicPassionVisible") == -1)
        {
            visibleSelect[2] = false;
        }
        if (SaveManager.GetInt("MusicALLVisible") == -1)
        {
            visibleSelect[3] = false;
        }
        if (SaveManager.GetInt("MusicOtherVisible") == -1)
        {
            visibleSelect[4] = false;
        }
        searchString = SaveManager.GetString("SearchMusicString");
        sortSelect = SaveManager.GetInt("MusicSortSelect");
        var tmp = SaveManager.GetInt("MusicSortOrder"); //デフォルトは昇順
        sortOrder = tmp != 1; //1がtrue
    }

    /// <summary>
    /// 使用するスプライトをロード
    /// </summary>
    private void LoadFrameSprite()
    {
        frameSprites = new Sprite[5];
        frameSprites[0] = Resources.Load<Sprite>("MusicFrame_cute");
        frameSprites[1] = Resources.Load<Sprite>("MusicFrame_cool");
        frameSprites[2] = Resources.Load<Sprite>("MusicFrame_passion");
        frameSprites[3] = Resources.Load<Sprite>("MusicFrame_all");
        frameSprites[4] = Resources.Load<Sprite>("MusicFrame_other");
    }

    /// <summary>
    /// 音楽ボタンの再描画
    /// </summary>
    public void ReVisible()
    {
        LoadSavedata();
        MusicSorting();
        ChangeVisible();
        ReViewSortSelect();
        ReViewSortChange();
    }

    public void SetMusicList(List<SQMusicData> datalist)
    {
        musicList = datalist;
    }

    /// <summary>
    /// 曲の並び替えを行う
    /// </summary>
    private void MusicSorting()
    {
        IOrderedEnumerable<SQMusicData> sort = GetSortOrder();
        if (sort != null)
        {
            foreach (var tmp in sort)
            {
                //順番に最背面に移動⇒最終的に順番に並ぶ
                tmp.iconObject.transform.SetAsLastSibling();
            }
        }
    }

    /// <summary>
    /// 表示の切り替えを行う
    /// </summary>
    private void ChangeVisible()
    {
        foreach (var tmp in musicList)
        {
            GameObject gameObject = tmp.iconObject;
            if (checkVisible(tmp))
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// アイコンを表示するかしないかを取得
    /// </summary>
    private bool checkVisible(SQMusicData data)
    {
        //スマートモードかどうか
        bool isVisible = true;
        if (smartmode && !data.smartmode)
        {
            isVisible = false;
        }

        //絞り込みに合致しているか
        int type = data.liveAttribute - 1;
        if (type < 5)
        {
            if (!visibleSelect[type])
            {
                isVisible = false;
            }
        }

        //名前検索に合致しているか
        if (searchString != "")
        {
            if (!data.name.Contains(searchString))
            {
                if (!data.name_kana.Contains(searchString))
                {
                    isVisible = false;
                }
            }
        }
        return isVisible;
    }

    /// <summary>
    /// 閉じるボタンクリック
    /// </summary>
    public void OnMusicViewClose()
    {
        base.gameObject.SetActive(false);
    }

    /// <summary>
    /// スマートモードの切り替え
    /// </summary>
    public void OnChangeSmartToggle(bool value)
    {
        smartmode = value;

        ChangeVisible();
    }

    /// <summary>
    /// 絞り込み表示画面の表示
    /// </summary>
    public void OnClickRefine()
    {
        GameObject obj = GameObject.Find("Canvas");
        Transform tf = obj.transform.Find("MusicRefineView");
        if (tf != null)
        {
            tf.gameObject.SetActive(true);
        }
        else
        {
            GameObject obj2 = Instantiate(MusicRefineView);
            GameObject baseObj = GameObject.Find("Canvas");
            obj2.transform.SetParent(baseObj.transform, false);
        }
    }

    /// <summary>
    /// ソート設定画面の表示
    /// </summary>
    public void OnClickSortSelect()
    {
        switch (sortSelect)
        {
            case 0:
                sortSelect = 1;
                break;
            case 1:
                sortSelect = 0;
                break;
        }
        SaveManager.SetInt("MusicSortSelect", sortSelect);

        SaveManager.Save();

        ReVisible();
        /*
        GameObject obj = GameObject.Find("Canvas");
        Transform tf = obj.transform.Find("MusicSortView");
        if (tf != null)
        {
            tf.gameObject.SetActive(true);
        }
        else
        {
            GameObject obj2 = Instantiate(MusicSortView);
            GameObject baseObj = GameObject.Find("Canvas");
            obj2.transform.SetParent(baseObj.transform, false);
        }
        */
    }

    /// <summary>
    /// ソートの昇順/降順の切り替え
    /// </summary>
    public void OnClickSortChange()
    {
        //ひっくり返すよ
        sortOrder = !sortOrder;

        if (sortOrder)
        {
            SaveManager.SetInt("MusicSortOrder", 0);
        }
        else
        {
            SaveManager.SetInt("MusicSortOrder", 1);
        }
        SaveManager.Save();

        ReVisible();
    }

    /// <summary>
    /// ソート順に並んだLINQイテレータを取得
    /// </summary>
    private IOrderedEnumerable<SQMusicData> GetSortOrder()
    {
        //セーブからソート順を指定
        IOrderedEnumerable<SQMusicData> sort = null;
        if (sortOrder)
        {
            switch (sortSelect)
            {
                case 0:
                    sort = musicList.OrderBy(x => x.music_data_id);
                    break;
                case 1:
                    sort = musicList.OrderBy(x => x.name_kana);
                    break;
            }
        }
        else
        {
            switch (sortSelect)
            {
                case 0:
                    sort = musicList.OrderByDescending(x => x.music_data_id);
                    break;
                case 1:
                    sort = musicList.OrderByDescending(x => x.name_kana);
                    break;
            }
        }
        return sort;
    }

    /// <summary>
    /// 曲ボタンリストを生成する
    /// </summary>
    private IEnumerator CreateMusicButtons()
    {
        while (true)
        {
            if (musicList != null)
            {
                break;
            }
            yield return null;
        }

        //親をセット
        content = transform.Find("Content/MusicList/Viewport/Content").GetComponent<RectTransform>();

        IOrderedEnumerable<SQMusicData> sort = GetSortOrder();
        if (sort != null)
        {
            //ボタンを生成
            foreach (var tmp in sort)
            {
                GameObject btn = Instantiate(musicIcon);
                //並列で実行
                StartCoroutine(MakeButton(tmp, btn));
            }
        }
        else
        {
            //ボタンを生成
            foreach (var tmp in musicList)
            {
                GameObject btn = Instantiate(musicIcon);
                //並列で実行
                StartCoroutine(MakeButton(tmp, btn));
            }
        }
    }

    /// <summary>
    /// ボタン生成単体
    /// </summary>
    private IEnumerator MakeButton(SQMusicData data, GameObject btn)
    {
        btn.name = data.music_data_id.ToString();

        data.iconObject = btn;

        //曲名を整形
        string text = data.name;
        //改行を付与
        text = text.Replace("\\n", "\n");
        btn.transform.GetComponentInChildren<Text>().text = text;

        //ボタンにアクションを設定
        Button button = btn.transform.GetComponentInChildren<Button>();
        AddButtonEvent(button, data);

        if (checkVisible(data))
        {
            btn.SetActive(true);
        }
        else
        {
            btn.SetActive(false);
        }

        int type = data.liveAttribute - 1;
        try
        {
            if (frameSprites != null && frameSprites[type] != null)
            {
                //ボタンアイコン変更
                Image frame = btn.transform.Find("MusicImage/Frame").GetComponent<Image>();
                frame.sprite = frameSprites[type];
                btn.transform.Find("MusicImage/Frame").gameObject.SetActive(true);
            }
        }
        catch (MissingComponentException e)
        {
            //ボタンの生成前にMusicViewを閉じた場合エラーをはくため
            print(e);
        }

        //親をセット
        btn.transform.SetParent(content, false);

        //Managerを取得
        UIManagerMusic md = GameObject.Find("UIManager").GetComponent<UIManagerMusic>();
        Sprite icondata = null;

        //ICON取得待ち
        while (true)
        {
            icondata = md.GetIconSprite(data.jacket_id);
            if (icondata != null)
            {
                break;
            }
            yield return null;
        }

        try
        {
            //ボタンアイコン変更
            Image img = btn.transform.Find("MusicImage").GetComponent<Image>();
            img.sprite = icondata;
        }
        catch (MissingComponentException e)
        {
            //ボタンの生成前にMusicViewを閉じた場合エラーをはくため
            print(e);
        }
    }

    // ボタンに機能を付与する
    void AddButtonEvent(Button button, SQMusicData data)
    {
        button.onClick.AddListener(() =>
        {
            this.OnClick(data);
            base.gameObject.SetActive(false);
            //Destroy(base.gameObject); //選択したらMusicViewを破棄する
        });
    }

    private void OnDisable()
    {
        //base.transform.Find("Content/SmartMode").GetComponent<ToggleSwitch>().Save();
    }

    /// <summary>
    /// アイコンを選択
    /// </summary>
    private void OnClick(SQMusicData data)
    {
        SellectMusic(data.id, data.jacket_id);
    }

    /// <summary>
    /// メイン画面のアイコンを更新
    /// </summary>
    private void SellectMusic(int musicID, int musicIcon = -1)
    {
        if (musicIcon == -1)
        {
            musicIcon = musicID;
        }
        //アイコンセット
        UIMusicSelect musicselect = GameObject.Find("Canvas/Panel/LiveContent/MusicInfo/MusicSelect").GetComponent<UIMusicSelect>();
        StartCoroutine(musicselect.MusicIconSetter(musicID, musicIcon));
    }

    /// <summary>
    /// SortChangeボタンの表示を変更する
    /// </summary>
    private void ReViewSortChange()
    {
        Image icon = SortChangeBtn.transform.Find("Image").GetComponent<Image>();
        Text text = SortChangeBtn.transform.Find("Text").GetComponent<Text>();

        if (sortOrder)
        {
            text.text = "昇順";
            icon.sprite = icons[0];
        }
        else
        {
            text.text = "降順";
            icon.sprite = icons[1];
        }
    }

    /// <summary>
    /// SortSelectボタンの表示を変更する
    /// </summary>
    private void ReViewSortSelect()
    {
        Text text = SortSelectBtn.transform.Find("Text").GetComponent<Text>();

        switch (sortSelect)
        {
            case 0:
                text.text = "ID順";
                break;
            case 1:
                text.text = "五十音順";
                break;
        }
    }
}