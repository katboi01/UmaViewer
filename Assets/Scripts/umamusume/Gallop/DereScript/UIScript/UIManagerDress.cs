using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagerDress : MonoBehaviour
{
    /*アイコン用prefab*/
    [SerializeField]
    private GameObject DressView;

    /// <summary>
    /// アイコン辞書
    /// </summary>
    private Dictionary<int, Sprite> iconDic;

    /// <summary>
    /// ドレス辞書
    /// </summary>
    private Dictionary<int, SQDressData> dressDic;

    /// <summary>
    /// 共通衣装リスト
    /// </summary>
    private List<SQDressData> commonDresses;

    /// <summary>
    /// エイプリルフール衣装リスト
    /// </summary>
    private List<SQDressData> aprilDresses;

    /// <summary>
    /// 共通衣装リスト
    /// </summary>
    private List<SQDressData> shopDresses;

    /// <summary>
    /// 共通衣装辞書
    /// キャラIDから衣装を返す辞書
    /// </summary>
    private Dictionary<int, List<SQDressData>> shopDoressDic;

    /// <summary>
    /// SSR衣装リスト
    /// </summary>
    private List<SQDressData> ssrDresses;

    /// <summary>
    /// キャラIDからSSRドレスリストを返す辞書
    /// </summary>
    private Dictionary<int, List<SQDressData>> ssrDressDic;


    public bool isLoad
    {
        get
        {
            if (MasterDBManager.instance.isError || ManifestManager.instance.isError)
            {
                return false;
            }
            return true;
        }
    }

    // Use this for initialization
    void Start()
    {
        CreateSaveIcon();
        StartCoroutine(CreateSpriteDic());
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 衣装関連のデータをいったん破棄して読み込み直す
    /// </summary>
    public void ResetData()
    {
        if (ssrDressDic != null)
        {
            ssrDressDic.Clear();
            ssrDressDic = null;
        }
        if (iconDic != null)
        {
            iconDic.Clear();
            iconDic = null;
        }

        CreateSaveIcon();
        StartCoroutine(CreateSpriteDic());
    }


    /// <summary>
    /// セーブに入っているIconのSpriteを先に生成したい
    /// </summary>
    private void CreateSaveIcon()
    {
        for (int i = 0; i < 10; i++)
        {
            //int iconID = SaveManager.GetInt(string.Format("dress{0}_icon", i), -1);
            int iconID = SaveManager.GetDressIcon(i);
            if (iconID > 0)
            {
                StartCoroutine(CreateSprite(iconID));
            }
        }
    }

    /// <summary>
    /// 共通衣装などのアイコンを生成
    /// </summary>
    private IEnumerator CreateSpriteDic()
    {
        if (iconDic == null)
        {
            iconDic = new Dictionary<int, Sprite>();
        }

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

        //ドレスリストを作成
        List<SQDressData> dressList = new List<SQDressData>();

        //共通衣装
        var common = MasterDBManager.instance.GetCommonDresses();
        if (common != null)
        {
            commonDresses = common;
            dressList.AddRange(common);
        }

        //ショップ衣装
        var shop = MasterDBManager.instance.GetShopDressesIcon();
        if (shop != null)
        {
            dressList.AddRange(shop);
            shopDresses = shop;

            //ショップ衣装辞書も取得
            shopDoressDic = MasterDBManager.instance.GetShopDresses();
        }

        //エイプリルフール衣装
        var april = MasterDBManager.instance.GetAprilDresses();
        if (april != null)
        {
            aprilDresses = april;
        }

        //SSR衣装
        var ssr = MasterDBManager.instance.GetALLSSRDresses();
        if (ssr != null)
        {
            ssrDressDic = new Dictionary<int, List<SQDressData>>(200);
            foreach (var tmp in ssr)
            {
                List<SQDressData> listdata = null;
                if (ssrDressDic.TryGetValue(tmp.charaID, out listdata))
                {
                    listdata.Add(tmp);
                }
                else
                {
                    listdata = new List<SQDressData>();
                    listdata.Add(tmp);
                    ssrDressDic.Add(tmp.charaID, listdata);
                }
            }
            ssrDresses = ssr;
            //dressList.AddRange(ssr);
        }

        DownloadDressIcons(dressList);


        AddAprilIconSprite();

        //1フレーム待って、セーブのほうを先に割り込ませる
        yield return null;

        //アイコン辞書に登録
        foreach (var tmp in dressList)
        {
            StartCoroutine(CreateSprite(tmp.dressIconKey));
        }
    }

    private void AddSpriteDicResource(int iconID, string resourcename, bool changePlusIcon)
    {
        Sprite spr = Resources.Load<Sprite>(resourcename);
        if (spr == null) { return; }

        if (iconDic.ContainsKey(iconID))
        {
            if (iconDic[iconID] == null)
            {
                iconDic[iconID] = spr;
            }
        }
        else
        {
            iconDic.Add(iconID, spr);
        }
        if (changePlusIcon)
        {
            if (iconDic.ContainsKey(iconID + 1))
            {
                if (iconDic[iconID + 1] == null)
                {
                    iconDic[iconID + 1] = spr;
                }
            }
            else
            {
                iconDic.Add(iconID + 1, spr);
            }
        }
    }

    /// <summary>
    /// エイプリルフール曲を追加
    /// </summary>
    private void AddAprilIconSprite()
    {
        AddSpriteDicResource(22, "icon_dress_mv", false);
        AddSpriteDicResource(900001, "chihiro_dress_icon", true);
        AddSpriteDicResource(900009, "dress_icon_cute", true);
        AddSpriteDicResource(900011, "dress_icon_cool", true);
        AddSpriteDicResource(900013, "dress_icon_passion", true);
        AddSpriteDicResource(900019, "kirarin_dress_icon", true);
        AddSpriteDicResource(900021, "kirarin_dress_icon", true);
        AddSpriteDicResource(900026, "staff_dress_icon", true);
        AddSpriteDicResource(900028, "sana_dress_icon", true);
        AddSpriteDicResource(900030, "pinya_dress_icon", true);
        AddSpriteDicResource(900074, "hiromi_dress_icon", true);
        AddSpriteDicResource(900076, "yuzu_dress_icon", true);
    }

    /// <summary>
    /// Spriteを生成し辞書に登録する
    /// </summary>
    public IEnumerator CreateSprite(int iconID)
    {
        //存在しない場合は終わり
        if (iconID <= 0)
        {
            yield break;
        }

        string name = SQDressData.getIconfilename(iconID);

        //辞書が存在しない
        if (iconDic == null)
        {
            iconDic = new Dictionary<int, Sprite>();
        }

        //既に登録されている
        if (iconDic.ContainsKey(iconID))
        {
            yield break;
        }
        else
        {
            //仮登録
            iconDic.Add(iconID, null);
        }

        //Localのファイル確認
        while (!AssetManager.instance.CheckFileFromFilename(name))
        {
            if (!AssetManager.instance.CheckExistFileInManifest(name))
            {
                iconDic.Remove(iconID);
                yield break;
            }
            yield return null;
        }

        //非同期で読み込み
        AssetBundleCreateRequest req = AssetManager.instance.LoadAssetFromNameAsync(name);
        //待ち
        while (!req.isDone)
        {
            yield return null;
        }
        AssetBundle bundle = req.assetBundle;

        Sprite tmpSp = null;
        Texture2D tex2D = null;
        //非同期でオブジェクトを読み込み
        AssetBundleRequest textures = bundle.LoadAllAssetsAsync<Texture2D>();
        while (!textures.isDone)
        {
            yield return null;
        }

        foreach (var obj in textures.allAssets)
        {
            if (obj is Texture2D)
            {
                tex2D = obj as Texture2D;
                break;
            }

        }
        Rect rect = new Rect(0, 0, tex2D.width, tex2D.height);
        tmpSp = Sprite.Create(tex2D, rect, Vector2.zero);

        bundle.Unload(false);


        //既に登録されている
        if (iconDic.ContainsKey(iconID))
        {
            if (iconDic[iconID] == null)
            {
                iconDic[iconID] = tmpSp;
            }
        }
    }

    /// <summary>
    /// Spriteを取得する
    /// </summary>
    public Sprite GetIconSprite(int iconID)
    {
        if (iconDic == null)
        {
            return null;
        }
        if (iconDic.ContainsKey(iconID))
        {
            return iconDic[iconID];
        }
        else
        {
            print("Dress icon ID : " + iconID + " がアイコン辞書にありません");
        }
        return null;
    }

    /// <summary>
    /// キャラIDからショップ衣装のリストを返す
    /// </summary>
    public List<SQDressData> GetShopDressesFromCharaID(int charaID)
    {
        if (shopDoressDic != null)
        {
            List<SQDressData> data = null;
            if (shopDoressDic.TryGetValue(charaID, out data))
            {
                return data;
            }
        }
        return new List<SQDressData>();
    }

    /// <summary>
    /// 共通衣装のリストを返す
    /// </summary>
    public List<SQDressData> GetCommonDresses()
    {
        return commonDresses;
    }

    public SQDressData GetDefaultDress()
    {
        return commonDresses[0];
    }

    /// <summary>
    /// SSR衣装のリストを返す
    /// </summary>
    public List<SQDressData> GetALLSSRDresses()
    {
        return ssrDresses;
    }

    /// <summary>
    /// キャラIDからSSR衣装のリストを返す
    /// </summary>
    public List<SQDressData> GetSSRDressesFromCharaID(int charaID)
    {
        if (ssrDressDic != null)
        {
            List<SQDressData> data = null;
            if (ssrDressDic.TryGetValue(charaID, out data))
            {
                return data;
            }
        }
        return new List<SQDressData>();
    }

    /// <summary>
    /// キャラIDからエイプリルフール衣装のリストを返す
    /// </summary>
    public List<SQDressData> GetAprilDressesFromCharaID(int charaID)
    {
        List<SQDressData> list = new List<SQDressData>();
        if (aprilDresses != null)
        {
            foreach (var tmp in aprilDresses)
            {
                if (tmp.charaID == charaID)
                {
                    list.Add(tmp);
                }
            }
        }
        return list;
    }

    /// <summary>
    /// キャラIDからエイプリルフール衣装のリストを返す
    /// </summary>
    public List<SQDressData> GetAprilDresses()
    {
        List<SQDressData> list = new List<SQDressData>();
        if (aprilDresses != null)
        {
            foreach (var tmp in aprilDresses)
            {
                if (tmp.charaID < 700)
                {
                    list.Add(tmp);
                }
            }
        }
        return list;
    }

    /// <summary>
    /// キャラIDから着れる衣装のリストを返す
    /// </summary>
    public List<SQDressData> GetDressDataFromCharaID(int charaID)
    {
        List<SQDressData> dressList = new List<SQDressData>();

        //エイプリルフールキャラは共通衣装を着れないため
        //最上静香,ジュリアは除外
        if ((charaID > 100 && charaID < 600) && charaID != 313 && charaID != 314)
        {
            dressList.AddRange(commonDresses);
            //ショップ衣装
            var shop = GetShopDressesFromCharaID(charaID);
            dressList.AddRange(shop);
        }
        var april = GetAprilDressesFromCharaID(charaID);
        dressList.AddRange(april);

        //SSR衣装
        var ssr = GetSSRDressesFromCharaID(charaID);
        dressList.AddRange(ssr);

        return dressList;
    }

    /// <summary>
    /// キャラIDからアイコンファイルのDLを行う
    /// </summary>
    public IEnumerator DownloadAndCreateDressIcons(int charaID)
    {
        if (iconDic == null)
        {
            iconDic = new Dictionary<int, Sprite>();
        }

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

        List<SQDressData> dressList = new List<SQDressData>(GetDressDataFromCharaID(charaID));

        DownloadDressIcons(dressList);

        //アイコン辞書に登録
        foreach (var tmp in dressList)
        {
            StartCoroutine(CreateSprite(tmp.dressIconKey));
        }
    }

    /// <summary>
    /// 衣装リストを入力してアイコンファイルをDLする
    /// </summary>
    public void DownloadDressIcons(List<SQDressData> dressDatas)
    {
        int dressCount = dressDatas.Count;
        List<string> dressIcons = new List<string>(dressCount);
        dressDic = new Dictionary<int, SQDressData>();

        foreach (var tmp in dressDatas)
        {
            if (!dressDic.ContainsKey(tmp.dressIconKey))
            {
                dressDic.Add(tmp.dressIconKey, tmp);
                dressIcons.Add(SQDressData.getIconfilename(tmp.dressIconKey));
            }
        }

        //ファイルのDL
        StartCoroutine(AssetManager.instance.DownloadFromFilenamesAsync(dressIcons));
    }

    /// <summary>
    /// DressViewを生成して表示
    /// </summary>
    public void CreateDressView(int currentChara, int clickPlace)
    {
        //インスタンスの生成
        GameObject obj = Instantiate(DressView);
        GameObject baseObj = GameObject.Find("Canvas");
        obj.transform.SetParent(baseObj.transform, false);

        UIDressView dv = obj.GetComponent<UIDressView>();
        dv.SetCharaID(currentChara);
        dv.SetClickPlace(clickPlace);
    }

    /// <summary>
    /// SSRアイコンを切り替えた場合
    /// </summary>
    public void ReloadSSRIcons()
    {
        GameObject obj = GameObject.Find("Canvas");
        Transform tf = obj.transform.Find("DressView");
        if (tf != null)
        {
            var tmp = tf.gameObject;
            Destroy(tmp); //DressViewを消しちゃうんDA！
        }
    }

    /// <summary>
    /// ユニークIDからDressDataを返す
    /// </summary>
    public SQDressData GetDressDataFromKeyID(int keyID)
    {
        bool isCommon = false;
        bool isShop = false;
        bool isApril = false;
        bool isSSR = false;

        if (keyID < 100) { isCommon = true; }
        if (keyID > 1000 && keyID < 9000) { isSSR = true; }
        if (keyID > 9000 && keyID < 10000) { isApril = true; }
        if (keyID > 7000000) { isShop = true; }

        if (isSSR)
        {
            foreach (var tmp in ssrDresses)
            {
                //SSRはカードID
                if (tmp.activeDressID == keyID)
                {
                    return tmp;
                }
            }
        }
        else if (isApril)
        {
            foreach (var tmp in aprilDresses)
            {
                if (tmp.activeDressID == keyID)
                {
                    return tmp;
                }
            }
        }
        else if (isCommon)
        {
            foreach (var tmp in commonDresses)
            {
                if (tmp.activeDressID == keyID)
                {
                    return tmp;
                }
            }
        }
        else if (isShop)
        {
            foreach (var tmp in shopDoressDic)
            {
                foreach (var tmp2 in tmp.Value)
                {
                    if (tmp2.activeDressID == keyID)
                    {
                        return tmp2;
                    }
                }
            }
        }
        else
        {
            print("dressCheckError!");
        }
        return null;
    }

    public SQDressData[] GetCurrentDresses()
    {
        SQDressData[] ret = new SQDressData[5];

        for (int i = 0; i < 5; i++)
        {
            int dressid = SaveManager.GetDress(i);
            if (dressid != 0)
            {
                ret[i] = GetDressDataFromKeyID(dressid);
            }
            else
            {
                ret[i] = null;
            }
        }

        return ret;
    }

    public SQDressData[] GetAnotherDresses()
    {
        SQDressData[] ret = new SQDressData[4];
        for (int i = 0; i < 4; i++)
        {
            int dressid = SaveManager.GetDress(i + 5);
            if (dressid != 0)
            {
                ret[i] = GetDressDataFromKeyID(dressid);
            }
            else
            {
                ret[i] = null;
            }
        }
        return ret;
    }

    public SQDressData GetPhotoStudioDress()
    {
        int dressid = SaveManager.GetDress(9);

        if (dressid == 0)
        {
            int rand = Random.Range(0, commonDresses.Count);

            return commonDresses[rand];
        }

        return GetDressDataFromKeyID(dressid);
    }
}