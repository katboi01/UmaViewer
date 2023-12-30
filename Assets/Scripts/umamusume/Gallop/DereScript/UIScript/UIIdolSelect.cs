using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIIdolSelect : MonoBehaviour
{
    //prefab
    [SerializeField]
    private GameObject UnitView;

    protected GameObject UIManager = null;

    [SerializeField]
    public GameObject[] charas;
    [SerializeField]
    public GameObject[] dresses;

    private GameObject IdolSelect;
    private GameObject IdolSelectAnother;

    private ToggleSwitch IdolSwitch;

    private void Awake()
    {
        Transform switchtrans = base.transform.Find("IdolSelectSwitch");
        if (switchtrans != null) { IdolSwitch = switchtrans.GetComponent<ToggleSwitch>(); }

        Transform livetrans = base.transform.Find("IdolSelect");
        if (livetrans != null) { IdolSelect = livetrans.gameObject; }

        livetrans = base.transform.Find("IdolSelectAnother");
        if (livetrans != null) { IdolSelectAnother = livetrans.gameObject; }
    }

    // Use this for initialization
    void Start()
    {
        UIManager = GameObject.Find("UIManager");

        //CharaViewの呼び出しに使う番号を付与
        for (int i = 0; i < charas.Length; i++)
        {
            Button button = charas[i].transform.GetComponentInChildren<Button>();
            AddCharaViewEvent(button, i);
        }
        //DressViewの呼び出しに使う番号を付与
        for (int i = 0; i < dresses.Length; i++)
        {
            Button button = dresses[i].transform.GetComponentInChildren<Button>();
            AddDressViewEvent(button, i);
        }

        //セーブデータを読み込む
        LoadingSavedata();

        OnChangeIdolSelectToggle();
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

    private void OnEnable()
    {
        if (UIManager != null)
        {
            ReLoadIcons();
        }
    }

    /// <summary>
    /// セーブデータを読み込み
    /// </summary>
    private void LoadingSavedata()
    {
        //セーブの読み込み
        int isplus = SaveManager.GetInt("SSRIcon");
        if (isplus == 1)
        {
            SQDressData.isPlus = true;
        }

        //アイコンファイルを列挙
        List<string> filelist = new List<string>(30);

        int[] chara = new int[charas.Length];
        int[] charaIcon = new int[charas.Length];
        for (int i = 0; i < chara.Length; i++)
        {
            chara[i] = SaveManager.GetChara(i);
            charaIcon[i] = SaveManager.GetCharaIcon(i);
            if (chara[i] != 0)
            {
                filelist.Add(string.Format("chara_icon_{0:000}_m.unity3d", charaIcon[i]));
            }
        }


        int[] dress = new int[dresses.Length];
        int[] dressIcon = new int[dresses.Length];
        for (int i = 0; i < dresses.Length; i++)
        {
            dress[i] = SaveManager.GetDress(i);
            dressIcon[i] = SaveManager.GetDressIcon(i);
            if (dress[i] != 0)
            {
                filelist.Add(SQDressData.getIconfilename(dressIcon[i]));
            }
        }

        //アイコンファイルをDL
        StartCoroutine(AssetManager.instance.DownloadFromFilenamesAsync(filelist));

        //アイコンに設定
        for (int i = 0; i < charas.Length; i++)
        {
            if (chara[i] != 0)
            {
                StartCoroutine(CharaIconSetter(chara[i], charaIcon[i], i));
            }
        }

        for (int i = 0; i < dresses.Length; i++)
        {
            if (dress[i] != 0)
            {
                StartCoroutine(DressIconSetter(dress[i], dressIcon[i], i));
            }
        }
    }

    /// <summary>
    /// キャラアイコンを変更する
    /// </summary>
    public void SetCharaIcon(int charaID, int charaIcon, int clickPlace)
    {
        StartCoroutine(CharaIconSetter(charaID, charaIcon, clickPlace));
    }

    /// <summary>
    /// キャラアイコンを変更する
    /// </summary>
    public virtual IEnumerator CharaIconSetter(int charaID, int charaIcon, int charaPlace)
    {
        if (charaPlace < 0)
        {
            yield break;
        }

        if (charaIcon <= 0)
        {
            Sprite sprite = Resources.Load<Sprite>("RandomChara");
            Image img = charas[charaPlace].GetComponent<Image>();
            img.sprite = sprite;

            yield break;
        }
        //キャラクタが変更になった場合、ドレスアイコンを変更
        int currentChara = SaveManager.GetChara(charaPlace);
        if (currentChara != charaID)
        {
            //dressIcon=1 ※初期ドレスに設定
            int setDress = 1;
            int setDressIcon = 1;
            switch (charaID)
            {
                case 20:
                    setDress = 9001;
                    setDressIcon = 900001;
                    break;
                case 48:
                    setDress = 9002;
                    setDressIcon = 900009;
                    break;
                case 49:
                    setDress = 9003;
                    setDressIcon = 900011;
                    break;
                case 50:
                    setDress = 9004;
                    setDressIcon = 900013;
                    break;
                case 83:
                    setDress = 9005;
                    setDressIcon = 900019;
                    break;
                case 672:
                    setDress = 9007;
                    setDressIcon = 900026;
                    break;
                case 682:
                    setDress = 9008;
                    setDressIcon = 900028;
                    break;
                case 701:
                    setDress = 9009;
                    setDressIcon = 900030;
                    break;
                case 725:
                    setDress = 9031;
                    setDressIcon = 900074;
                    break;
                case 726:
                    setDress = 9032;
                    setDressIcon = 900076;
                    break;
            }
            StartCoroutine(DressIconSetter(setDress, setDressIcon, charaPlace));
        }

        UIManagerChara uIManagerChara = UIManager.GetComponent<UIManagerChara>();

        //スプライトを割り込みで生成
        //無ければ作るし、あったら戻ってくる
        StartCoroutine(uIManagerChara.CreateSprite(charaIcon));

        SaveManager.SetChara(charaPlace, charaID);
        SaveManager.SetCharaIcon(charaPlace, charaIcon);
        SaveManager.Save();

        Sprite sp = uIManagerChara.GetIconSprite(charaIcon);
        if (sp == null)
        {
            //nullが返ってくる場合はまだSpriteが作成中の時に呼んでしまった可能性がある

            while (true)
            {
                sp = uIManagerChara.GetIconSprite(charaIcon);
                if (sp != null)
                {
                    break;
                }
                yield return null;
            }
        }
        Image image = charas[charaPlace].GetComponent<Image>();
        image.sprite = sp;

        //センターの変更の場合はソロ曲チェック
        if (charaPlace == 2)
        {
            UIManagerMusic uIManagerMusic = UIManager.GetComponent<UIManagerMusic>();
            while (!uIManagerMusic.isLoad)
            {
                yield return null;
            }
            uIManagerMusic.CheckSoloSong();
        }
    }

    /// <summary>
    /// ドレスアイコンを変更する
    /// </summary>
    public void SetDressIcon(int dressID, int dressIcon, int clickPlace)
    {
        StartCoroutine(DressIconSetter(dressID, dressIcon, clickPlace));
    }

    /// <summary>
    /// ドレスアイコンを変更する
    /// </summary>
    public virtual IEnumerator DressIconSetter(int dressID, int dressIcon, int dressPlace)
    {
        if (dressPlace < 0)
        {
            yield break;
        }

        if (dressIcon < 0)
        {
            Sprite sprite = Resources.Load<Sprite>("RandomDress");
            Image img = dresses[dressPlace].GetComponent<Image>();
            img.sprite = sprite;

            yield break;
        }

        UIManagerDress uIManagerDress = UIManager.GetComponent<UIManagerDress>();

        //割り込みで読み込む
        StartCoroutine(uIManagerDress.CreateSprite(dressIcon));

        SaveManager.SetDress(dressPlace, dressID);
        SaveManager.SetDressIcon(dressPlace, dressIcon);
        SaveManager.Save();

        Sprite sp = uIManagerDress.GetIconSprite(dressIcon);
        if (sp == null)
        {
            while (true)
            {
                sp = uIManagerDress.GetIconSprite(dressIcon);
                if (sp != null)
                {
                    break;
                }
                yield return null;
            }
        }
        Image image = dresses[dressPlace].GetComponent<Image>();
        image.sprite = sp;
    }

    /// <summary>
    /// キャラボタンにイベントを設置
    /// </summary>
    protected void AddCharaViewEvent(Button button, int num)
    {
        button.onClick.AddListener(() =>
        {
            this.OnClickCharaSelect(num);
        });
    }

    /// <summary>
    /// イベント処理
    /// </summary>
    protected void OnClickCharaSelect(int num)
    {
        //print("キャラ選択アイコン：" + num);
        GameObject obj = GameObject.Find("Canvas");
        Transform tf = obj.transform.Find("CharaView");
        if (tf != null)
        {
            tf.gameObject.SetActive(true);
            var tmp = tf.gameObject.GetComponent<UICharaView>();
            tmp.SetClickPlace(num);
        }
        else
        {
            UIManagerChara mc = UIManager.GetComponent<UIManagerChara>();
            if (mc.isLoad)
            {
                mc.CreateCharaView(num);
            }
        }
    }

    /// <summary>
    /// ドレスボタンにイベントを設置
    /// </summary>
    protected void AddDressViewEvent(Button button, int num)
    {
        button.onClick.AddListener(() =>
        {
            this.OnClickDressSelect(num);
        });
    }

    /// <summary>
    /// イベント処理
    /// </summary>
    protected void OnClickDressSelect(int num)
    {
        //print("ドレス選択アイコン:" + num);
        GameObject obj = GameObject.Find("Canvas");
        Transform tf = obj.transform.Find("DressView");

        int currentChara = SaveManager.GetChara(num);
        if (currentChara <= 0)
        {
            //選択されていない時は0、初期値は-1
            print("キャラクタが選択されていません");
        }
        else
        {
            UIManagerDress md = UIManager.GetComponent<UIManagerDress>();

            if (tf != null)
            {
                var tmp = tf.gameObject.GetComponent<UIDressView>();
                tmp.SetCharaID(currentChara);
                tmp.SetClickPlace(num);
                tf.gameObject.SetActive(true);
            }
            else
            {
                if (md.isLoad)
                {
                    md.CreateDressView(currentChara, num);
                }
            }
        }
    }

    public virtual void ReLoadIcons()
    {
        LoadingSavedata();
    }

    public void OnClickReset()
    {
        ResetSelection();
    }

    public void OnClickRandom()
    {
        StartCoroutine(RandomSelect());
    }

    private void ResetSelection()
    {
        for (int i = 0; i < charas.Length; i++)
        {
            StartCoroutine(CharaIconSetter(-1, -1, i));
            SaveManager.SetChara(i, 0);
            SaveManager.SetCharaIcon(i, 0);
        }

        for (int i = 0; i < dresses.Length; i++)
        {
            StartCoroutine(DressIconSetter(-1, -1, i));
            SaveManager.SetDress(i, 0);
            SaveManager.SetDressIcon(i, 0);
        }
        SaveManager.Save();
    }

    private IEnumerator RandomSelect()
    {
        UIManagerChara mc = UIManager.GetComponent<UIManagerChara>();
        UIManagerDress md = UIManager.GetComponent<UIManagerDress>();
        int save = SaveManager.GetInt("RandomMemberSet");
        bool onlyAnother = false;
        //アナザーメンバーが選択されている
        if (IdolSelectAnother.activeInHierarchy)
        {
            onlyAnother = true;
        }

        SQCharaData[] charaDatas = new SQCharaData[charas.Length];

        for (int i = 0; i < charaDatas.Length; i++)
        {
            if (onlyAnother)
            {
                //0~4まではスキップ
                if (i < 5) continue;
            }
            charaDatas[i] = mc.GetRandomCharaDataWithoutAprilfool();
            if (charaDatas[i] == null) { yield break; }
            StartCoroutine(CharaIconSetter(charaDatas[i].charaID, charaDatas[i].iconID, i));
            StartCoroutine(md.DownloadAndCreateDressIcons(charaDatas[i].charaID));
        }

        yield return null;

        for (int i = 0; i < charaDatas.Length; i++)
        {
            if (onlyAnother)
            {
                //0~4まではスキップ
                if (i < 5) continue;
            }

            if (charaDatas[i].charaID > 0)
            {
                //ドレスが指定されていない場合の処理
                switch (save)
                {
                    case 0:
                        //StartCoroutine(DressIconSetter(1, 1, i));
                        break;
                    case 1:
                        {
                            List<SQDressData> dressList = md.GetDressDataFromCharaID(charaDatas[i].charaID);
                            if (dressList != null)
                            {
                                int iResult2 = Random.Range(0, dressList.Count);

                                StartCoroutine(DressIconSetter(dressList[iResult2].activeDressID, dressList[iResult2].dressIconKey, i));
                            }
                        }
                        break;
                    case 2:
                        {
                            List<SQDressData> dressList = md.GetSSRDressesFromCharaID(charaDatas[i].charaID);
                            if (dressList != null && dressList.Count != 0)
                            {
                                int iResult2 = Random.Range(0, dressList.Count);

                                StartCoroutine(DressIconSetter(dressList[iResult2].activeDressID, dressList[iResult2].dressIconKey, i));
                            }
                            else
                            {
                                goto case 1;
                            }
                        }
                        break;
                }
            }
            /*
            List<SQDressData> dressList = null;
            if (RandomMemberSet == 0)
            {
                //デフォルト衣装
                StartCoroutine(DressIconSetter(0, 0, i));
            }
            else if (RandomMemberSet == 2)
            {
                dressList = MasterDBManager.instance.GetSSRDressesFromCharaID(charaDatas[i].charaID);
            }
            if (dressList == null || dressList.Count == 0)
            {
                dressList = md.GetDressDataFromCharaID(charaDatas[i].charaID);
            }
            if (dressList != null && dressList.Count > 0)
            {
                int iResult2 = Random.Range(0, dressList.Count);

                StartCoroutine(AssetManager.instance.DownloadFromFilename(SQDressData.getIconfilename(dressList[iResult2].iconkey)));
                StartCoroutine(DressIconSetter(dressList[iResult2].key, dressList[iResult2].iconkey, i));
            }
            */
        }
    }

    /// <summary>
    /// IdolSelectとIdolSelectAnotherを切り替えるスイッチ
    /// </summary>
    public void OnChangeIdolSelectToggle()
    {
        if (IdolSwitch.isOn)
        {
            IdolSelect.SetActive(true);
            IdolSelectAnother.SetActive(false);
        }
        else
        {
            IdolSelect.SetActive(false);
            IdolSelectAnother.SetActive(true);
        }
    }

    public void OnClickUnitSelectButton()
    {
        //print("ドレス選択アイコン:" + num);
        GameObject obj = GameObject.Find("Canvas");
        Transform tf = obj.transform.Find("IdolUnitListView");

        if (tf != null)
        {
            //var tmp = tf.gameObject.GetComponent<UIIdolUnitListView>();
            tf.gameObject.SetActive(true);
        }
        else
        {
            //prefabを実装
            GameObject gameObject = GameObject.Instantiate(UnitView);

            Transform transform = GameObject.Find("Canvas").transform;

            //Panel下に実装
            gameObject.transform.SetParent(transform, false);
        }
    }
}