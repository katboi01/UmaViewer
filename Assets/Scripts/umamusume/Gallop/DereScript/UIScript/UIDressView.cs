using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIDressView : MonoBehaviour
{

    [SerializeField]
    private GameObject dressIcon;

    [SerializeField]
    private List<GameObject> dressBtnList;

    [SerializeField]
    private List<GameObject> dressBtnListAppend;

    private int charaID = -1;
    private int clickPlace = -1;

    private GameObject CharaDress;
    private Transform content;

    private GameObject AllDress;
    private Transform contentAppend;

    private ToggleSwitch AppendSwitch;

    private UIManagerDress uIManagerDress;

    private void Awake()
    {
        base.name = base.name.Replace("(Clone)", "");
        WindowSizeSetter();

        CharaDress = transform.Find("Content/DressList").gameObject;
        AllDress = transform.Find("Content/DressListAppend").gameObject;
        
        content = transform.Find("Content/DressList/Viewport/Content");
        contentAppend = transform.Find("Content/DressListAppend/Viewport/Content");

        AppendSwitch = transform.Find("Header/AppendDress").GetComponent<ToggleSwitch>();

        uIManagerDress = GameObject.Find("UIManager").GetComponent<UIManagerDress>();

        OnChangePageToggle();
                
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
        setSize.x = 67f;

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
        StartCoroutine(CreateALLDressButtons());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        //別キャラ衣装はデフォルトで非選択
        AppendSwitch.isOn = true;
        StartCoroutine(CreateDressButtons());
    }

    private void OnDisable()
    {
        RemoveIcons();
    }

    /// <summary>
    /// 閉じるボタンクリック
    /// </summary>
    public void OnDressViewClose()
    {
        base.gameObject.SetActive(false);
        //Destroy(base.gameObject);
    }

    public void SetCharaID(int chara)
    {
        charaID = chara;
    }

    public void SetClickPlace(int place)
    {
        clickPlace = place;
    }

    private void RemoveIcons()
    {
        if(dressBtnList != null && dressBtnList.Count > 0)
        {
            foreach(var tmp in dressBtnList)
            {
                Destroy(tmp);
            }
            dressBtnList.Clear();
            dressBtnList = null;
        }
    }

    private IEnumerator CreateALLDressButtons()
    {
        //DB読み込み待ち
        while (true)
        {
            if (MasterDBManager.instance.isLoadDB && AssetManager.instance.isManifestLoad)
            {
                break;
            }
            if (MasterDBManager.instance.isError || ManifestManager.instance.isError)
            {
                yield break;
            }
            yield return null;
        }

        List<SQDressData> dressList = new List<SQDressData>();

        //SSR衣装
        var ssr = uIManagerDress.GetALLSSRDresses();
        dressList.AddRange(ssr);
        var april = uIManagerDress.GetAprilDresses();
        dressList.AddRange(april);

        uIManagerDress.DownloadDressIcons(dressList);
        //アイコン辞書に登録
        foreach (var tmp in dressList)
        {
            StartCoroutine(uIManagerDress.CreateSprite(tmp.dressIconKey));
        }

        yield return null;

        dressBtnListAppend = new List<GameObject>(dressList.Count);
        foreach (var tmp in dressList)
        {
            GameObject btn = Instantiate(dressIcon);
            dressBtnListAppend.Add(btn);
            //並列で実行
            StartCoroutine(MakeButton(tmp, btn, contentAppend));
        }
    }

    private IEnumerator CreateDressButtons()
    {
        //DB読み込み待ち
        while (true)
        {
            if (MasterDBManager.instance.isLoadDB && AssetManager.instance.isManifestLoad)
            {
                break;
            }
            if (MasterDBManager.instance.isError || ManifestManager.instance.isError)
            {
                yield break;
            }
            yield return null;
        }

        //キャラIDセット待ち
        while (charaID == -1)
        {
            yield return null;
        }

        List<SQDressData> dressList = new List<SQDressData>();
        //ちひろ、最上、ジュリアは共通衣装選択可能にする
        if(charaID == 20 || charaID == 313 || charaID == 314)
        {
            dressList.AddRange(uIManagerDress.GetCommonDresses());
        }
        dressList.AddRange(uIManagerDress.GetDressDataFromCharaID(charaID));

        yield return null;

        //Managerを取得
        StartCoroutine(uIManagerDress.DownloadAndCreateDressIcons(charaID));

        dressBtnList = new List<GameObject>(dressList.Count);
        foreach (var tmp in dressList)
        {
            GameObject btn = Instantiate(dressIcon);
            dressBtnList.Add(btn);
            //並列で実行
            StartCoroutine(MakeButton(tmp, btn,content));
        }

    }

    /// <summary>
    /// ボタン生成単体
    /// </summary>
    private IEnumerator MakeButton(SQDressData data, GameObject buttonObj,Transform parent)
    {
        //keyをオブジェクト名に設定。iconkeyではない
        buttonObj.name = data.activeDressID.ToString();

        //ドレス名を名前に設定
        string text = data.dressKeyName;
        text = text.Replace("］", "］\n");
        buttonObj.transform.GetComponentInChildren<Text>().text = text;

        //ボタンにアクションを設定
        Button button = buttonObj.transform.GetComponentInChildren<Button>();
        AddDressButtonEvent(button, data);

        //親を設定
        buttonObj.transform.SetParent(parent, false);

        //Managerを取得
        Sprite icondata = null;

        //ICON取得待ち
        while (true)
        {
            icondata = uIManagerDress.GetIconSprite(data.dressIconKey);
            if (icondata != null)
            {
                break;
            }
            yield return null;
        }

        try
        {
            //ボタンアイコン変更
            Image img = buttonObj.transform.Find("Button").GetComponent<Image>();
            img.sprite = icondata;
        }
        catch (MissingComponentException e)
        {
            //ボタンの生成前にDressViewを閉じた場合エラーをはくため
            print(e);
        }
    }

    // ボタンに機能を付与する
    private void AddDressButtonEvent(Button button, SQDressData data)
    {
        button.onClick.AddListener(() =>
        {
            this.OnClick(data);
            base.gameObject.SetActive(false);
            //Destroy(base.gameObject);
        });
    }

    private void OnClick(SQDressData data)
    {
        SelectDress(data.activeDressID, data.dressIconKey);
    }

    private void SelectDress(int dressKey, int dressIcon = -1)
    {
        if (dressIcon == -1)
        {
            dressIcon = dressKey;
        }


        //アイコンセット
        UIIdolSelect idolselect = GameObject.Find("Canvas/Panel/LiveContent/IdolInfo").GetComponent<UIIdolSelect>();
        StartCoroutine(idolselect.DressIconSetter(dressKey, dressIcon, this.clickPlace));

        /*
        if (clickPlace < 9)
        {
            //アイコンセット
            UIIdolSelect idolselect = GameObject.Find("Canvas/Panel/LiveContent/IdolInfo").GetComponent<UIIdolSelect>();
            StartCoroutine(idolselect.DressIconSetter(dressKey, dressIcon, this.clickPlace));
        }
        else
        {
            //フォトスタジオ
            UIPhotoStudioChara photoStudioChara = GameObject.Find("Canvas/Panel/PhotoStudio/PhotoStudioIdolSelect").GetComponent<UIPhotoStudioChara>();
            if (photoStudioChara != null)
            {
                StartCoroutine(photoStudioChara.DressIconSetter(dressKey, dressIcon, this.clickPlace));
            }
        }
        */
    }

    public void OnChangePageToggle()
    {
        if (AppendSwitch.isOn)
        {
            CharaDress.SetActive(true);
            AllDress.SetActive(false);
        }
        else
        {
            CharaDress.SetActive(false);
            AllDress.SetActive(true);
        }
    }
}