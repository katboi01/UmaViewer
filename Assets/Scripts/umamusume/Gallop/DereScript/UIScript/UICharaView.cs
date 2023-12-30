using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UICharaView : MonoBehaviour
{
    [SerializeField]
    private GameObject charaIcon;

    [SerializeField]
    private GameObject CharaRefineView;

    [SerializeField]
    private GameObject CharaSortView;

    private GameObject SortChangeBtn;

    private GameObject SortSelectBtn;

    private Sprite[] icons = null;

    private List<SQCharaData> charaList = null;

    private bool[] visibleSelect = new bool[] { true, true, true, true };
    private string searchString = "";
    private int sortSelect;
    private bool sortOrder;

    private int clickPlace = -1;

    private Transform content;

    private void Awake()
    {
        SortChangeBtn = base.transform.Find("Header/SortingButton/RightButton").gameObject;
        SortSelectBtn = base.transform.Find("Header/SortingButton/LeftButton").gameObject;

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
        setSize.x = 47f;

        //横コンテンツ数は最大10個
        for (int i = 0; i < 10; i++)
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
        base.name = base.name.Replace("(Clone)", "");

        //セーブの読み込み
        LoadSavedata();

        StartCoroutine(CreateCharaButtons());

        ReViewSortSelect();
        ReViewSortChange();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetClickPlace(int place)
    {
        clickPlace = place;
    }

    private void LoadSavedata()
    {
        visibleSelect = new bool[] { true, true, true, true };

        if (SaveManager.GetInt("CharaCuteVisible") == -1)
        {
            visibleSelect[0] = false;
        }
        if (SaveManager.GetInt("CharaCoolVisible") == -1)
        {
            visibleSelect[1] = false;
        }
        if (SaveManager.GetInt("CharaPassionVisible") == -1)
        {
            visibleSelect[2] = false;
        }
        if (SaveManager.GetInt("CharaOtherVisible") == -1)
        {
            visibleSelect[3] = false;
        }
        searchString = SaveManager.GetString("SearchCharaString");
        sortSelect = SaveManager.GetInt("CharaSortSelect");
        var tmp = SaveManager.GetInt("CharaSortOrder");
        sortOrder = tmp != 1; //1がtrue
    }

    /// <summary>
    /// キャラボタンの再描画
    /// </summary>
    public void ReVisible()
    {
        LoadSavedata();
        ChangeVisible();
        CharaSorting();
        ReViewSortSelect();
        ReViewSortChange();
    }

    /// <summary>
    /// キャラリストをセットする
    /// </summary>
    public void SetCharaList(List<SQCharaData> _charalist)
    {
        this.charaList = _charalist;
    }

    /// <summary>
    /// キャラクタの並び替えを行う
    /// </summary>
    private void CharaSorting()
    {
        IOrderedEnumerable<SQCharaData> sort = GetSortOrder();
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
    /// アイコンの表示を切り替え
    /// </summary>
    private void ChangeVisible()
    {
        foreach (var tmp in charaList)
        {
            GameObject gameObject = tmp.iconObject;

            bool isVisible = checkVisible(tmp);
            if (isVisible)
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
    private bool checkVisible(SQCharaData data)
    {
        bool isVisible = true;
        int type = data.type - 1;

        if (type < 4)
        {
            if (!visibleSelect[type])
            {
                isVisible = false;
            }
        }
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
    public void OnCharaViewClose()
    {
        base.gameObject.SetActive(false);
        //Destroy(base.gameObject);
    }

    /// <summary>
    /// 絞り込み表示画面の表示
    /// </summary>
    public void OnClickRefine()
    {
        GameObject obj = GameObject.Find("Canvas");
        Transform tf = obj.transform.Find("CharaRefineView");
        if (tf != null)
        {
            tf.gameObject.SetActive(true);
        }
        else
        {
            GameObject obj2 = Instantiate(CharaRefineView);
            GameObject baseObj = GameObject.Find("Canvas");
            obj2.transform.SetParent(baseObj.transform, false);
        }
    }

    /// <summary>
    /// ソート設定の切り替え
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
        SaveManager.SetInt("CharaSortSelect", sortSelect);

        SaveManager.Save();

        ReVisible();
        /*
        GameObject obj = GameObject.Find("Canvas");
        Transform tf = obj.transform.Find("CharaSortView");
        if (tf != null)
        {
            tf.gameObject.SetActive(true);
        }
        else
        {
            GameObject obj2 = Instantiate(CharaSortView);
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
            SaveManager.SetInt("CharaSortOrder", 0);
        }
        else
        {
            SaveManager.SetInt("CharaSortOrder", 1);
        }
        SaveManager.Save();

        ReVisible();
    }

    /// <summary>
    /// ソート順に並んだLINQイテレータを取得
    /// </summary>
    private IOrderedEnumerable<SQCharaData> GetSortOrder()
    {
        //セーブからソート順を指定
        IOrderedEnumerable<SQCharaData> sort = null;
        if (sortOrder)
        {
            switch (sortSelect)
            {
                case 0:
                    sort = charaList.OrderBy(x => x.charaID);
                    break;
                case 1:
                    sort = charaList.OrderBy(x => x.name_kana);
                    break;
            }
        }
        else
        {
            switch (sortSelect)
            {
                case 0:
                    sort = charaList.OrderByDescending(x => x.charaID);
                    break;
                case 1:
                    sort = charaList.OrderByDescending(x => x.name_kana);
                    break;
            }
        }
        return sort;
    }

    /// <summary>
    /// キャラボタンリストを生成する
    /// </summary>
    private IEnumerator CreateCharaButtons()
    {
        //contentエリアを取得
        content = transform.Find("Content/CharaList/Viewport/Content").gameObject.GetComponent<RectTransform>();

        //charalistセット待ち
        while (true)
        {
            if (charaList != null)
            {
                break;
            }
            yield return null;
        }

        //セーブからソート順を指定
        IOrderedEnumerable<SQCharaData> sort = GetSortOrder();
        if (sort != null)
        {
            //ボタンを生成
            foreach (var tmp in sort)
            {
                GameObject btn = Instantiate(charaIcon);
                //並列で実行
                StartCoroutine(MakeButton(tmp, btn));
            }
        }
        else
        {
            //ボタンを生成
            foreach (var tmp in charaList)
            {
                GameObject btn = Instantiate(charaIcon);
                //並列で実行
                StartCoroutine(MakeButton(tmp, btn));
            }
        }
    }

    /// <summary>
    /// ボタン生成単体
    /// </summary>
    private IEnumerator MakeButton(SQCharaData data, GameObject btn)
    {
        btn.name = data.charaID.ToString();

        data.iconObject = btn;

        //曲名を名前に設定
        string text = data.name;
        text = text.Replace("コミュ用", "プチ");
        btn.transform.GetComponentInChildren<Text>().text = text;

        //ボタンにアクションを設定
        Button button = btn.transform.GetComponentInChildren<Button>();
        AddButtonEvent(button, data);

        //親をセット
        btn.transform.SetParent(content, false);

        //表示設定
        if (!checkVisible(data))
        {
            btn.SetActive(false);
        }

        //Managerを取得
        UIManagerChara mc = GameObject.Find("UIManager").GetComponent<UIManagerChara>();
        Sprite icondata = null;

        //ICON取得待ち
        while (true)
        {
            icondata = mc.GetIconSprite(data.iconID);
            if (icondata != null)
            {
                break;
            }
            yield return null;
        }

        try
        {
            //ボタンアイコン変更
            Image img = btn.transform.Find("Button").GetComponent<Image>();
            img.sprite = icondata;
        }
        catch (MissingComponentException e)
        {
            //ボタンの生成前にMusicViewを閉じた場合エラーをはくため
            print(e);
        }
    }

    /// <summary>
    /// ボタンに機能を付与する
    /// </summary>
    private void AddButtonEvent(Button button, SQCharaData data)
    {
        button.onClick.AddListener(() =>
        {
            this.OnClick(data);
            base.gameObject.SetActive(false);
            //Destroy(base.gameObject); //選択したらCharaViewを破棄する
        });
    }

    /// <summary>
    /// キャラアイコンを選択
    /// </summary>
    private void OnClick(SQCharaData data)
    {
        SellectChara(data.charaID, data.iconID);
    }

    /// <summary>
    /// キャラボタンにキャラを設定する
    /// </summary>
    private void SellectChara(int charaID, int charaIcon = -1)
    {
        if (charaIcon == -1)
        {
            charaIcon = charaID;
        }

        //アイコンセット
        UIIdolSelect idolselect = GameObject.Find("Canvas/Panel/LiveContent/IdolInfo").GetComponent<UIIdolSelect>();
        StartCoroutine(idolselect.CharaIconSetter(charaID, charaIcon, this.clickPlace));
        /*
        if (this.clickPlace < 9)
        {
            //アイコンセット
            UIIdolSelect idolselect = GameObject.Find("Canvas/Panel/LiveContent/IdolInfo").GetComponent<UIIdolSelect>();
            StartCoroutine(idolselect.CharaIconSetter(charaID, charaIcon, this.clickPlace));
        }
        else
        {
            //フォトスタジオ
            UIPhotoStudioChara photoStudioChara = GameObject.Find("Canvas/Panel/PhotoStudio/PhotoStudioIdolSelect").GetComponent<UIPhotoStudioChara>();
            if (photoStudioChara != null)
            {
                StartCoroutine(photoStudioChara.CharaIconSetter(charaID, charaIcon, this.clickPlace));
            }
        }
        */
        //キャラ選択時にDressアイコンをDLする
        StartCoroutine(GameObject.Find("UIManager").GetComponent<UIManagerDress>().DownloadAndCreateDressIcons(charaID));
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