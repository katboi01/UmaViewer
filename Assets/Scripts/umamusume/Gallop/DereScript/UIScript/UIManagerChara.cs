using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManagerChara : MonoBehaviour
{
    /*アイコン用prefab*/
    [SerializeField]
    private GameObject CharaView;

    /*キャラクタリスト*/
    private List<SQCharaData> charaList;

    private Dictionary<int, SQCharaData> charaDic;
    private Dictionary<int, Sprite> iconDic;

    public bool isLoad
    {
        get
        {
            if (charaList != null)
            {
                if (charaList.Count > 0)
                {
                    return true;
                }
            }
            return false;
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
    /// キャラクタ関連のデータをいったん破棄して読み込み直す
    /// </summary>
    public void ResetData()
    {
        if (charaList != null)
        {
            charaList.Clear();
            charaList = null;
        }
        if (charaDic != null)
        {
            charaDic.Clear();
            charaDic = null;
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
            int iconID = SaveManager.GetCharaIcon(i);
            if (iconID > 0)
            {
                StartCoroutine(CreateSprite(iconID));
            }
        }
    }

    /// <summary>
    /// CharaIcon用のSprite辞書を生成する
    /// </summary>
    private IEnumerator CreateSpriteDic()
    {
        if (iconDic == null)
        {
            iconDic = new Dictionary<int, Sprite>();
        }
        if (charaDic == null)
        {
            charaDic = new Dictionary<int, SQCharaData>();
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
        //キャラリストを取得
        charaList = MasterDBManager.instance.GetAllCharas();
        int charaCount = charaList.Count;

        //キャラリストを取得
        var aprilfools = MasterDBManager.instance.GetAprilfoolCharas();

        charaList.AddRange(aprilfools);

        //キャラリストを取得
        var vrcharas = MasterDBManager.instance.GetVRCharas();
        charaList.AddRange(vrcharas);

        List<string> charaIcons = new List<string>(charaCount);
        foreach (var tmp in charaList)
        {
            charaIcons.Add(string.Format("chara_icon_{0:000}_m.unity3d", tmp.iconID));
            charaDic.Add(tmp.charaID, tmp);
        }
        //コルーチンでDLを投げる
        StartCoroutine(AssetManager.instance.DownloadFromFilenamesAsync(charaIcons));

        AddAprilIconSprite();

        //1フレーム待って、セーブのほうを先に割り込ませる
        yield return null;

        //アイコン作成
        foreach (var tmp in charaList)
        {
            //並列で実行
            StartCoroutine(CreateSprite(tmp.iconID));
        }
    }

    private void AddSpriteDicResource(int iconID, string resourcename)
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
    }

    /// <summary>
    /// エイプリルフール曲を追加
    /// </summary>
    private void AddAprilIconSprite()
    {
        AddSpriteDicResource(20, "chara_icon_chihiro");
        AddSpriteDicResource(48, "putiuzuki_icon");
        AddSpriteDicResource(49, "putirin_icon");
        AddSpriteDicResource(50, "putimio_icon");
        AddSpriteDicResource(83, "kirarinLobo_Icon");
        AddSpriteDicResource(672, "staff_icon");
        AddSpriteDicResource(682, "sana_icon");
        AddSpriteDicResource(701, "pinya_icon");
        AddSpriteDicResource(725, "hiromi_icon");
        AddSpriteDicResource(726, "yuzu_icon");
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

        string name = string.Format("chara_icon_{0:000}_m.unity3d", iconID);

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
            //マニフェストに存在しない
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
                if (obj.name.IndexOf("m") >= 0)
                {
                    tex2D = obj as Texture2D;
                    break;
                }
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
        return null;
    }

    /// <summary>
    /// CharaViewを生成し表示
    /// </summary>
    public void CreateCharaView(int ClickPlace)
    {
        GameObject obj = Instantiate(CharaView);
        GameObject baseObj = GameObject.Find("Canvas");
        obj.transform.SetParent(baseObj.transform, false);

        UICharaView charaView = obj.GetComponent<UICharaView>();

        //クリック場所をセット
        charaView.SetClickPlace(ClickPlace);
        //キャラリストをセット
        charaView.SetCharaList(charaList);
    }


    /// <summary>
    /// 現在選択されているキャラデータを取得する
    /// </summary>
    public SQCharaData[] GetCurrentCharas()
    {
        SQCharaData[] ret = new SQCharaData[5];
        for (int i = 0; i < 5; i++)
        {
            int charaid = SaveManager.GetChara(i);
            SQCharaData data;

            if (charaDic.TryGetValue(charaid, out data))
            {
                ret[i] = data;
            }
            else
            {
                /*
                //見つからなかったらランダム
                data = GetRandomCharaData();

                //特殊キャラの場合は引き直し
                while (data.charaID < 100)
                {
                    data = GetRandomCharaData();
                }
                ret[i] = data;
                */
                ret[i] = null;
            }
        }
        return ret;
    }
    
    /// <summary>
    /// 追加メンバーのキャラデータを取得する
    /// </summary>
    public SQCharaData[] GetAnotherCharas()
    {
        SQCharaData[] ret = new SQCharaData[4];
        for (int i = 0; i < 4; i++)
        {
            int charaid = SaveManager.GetChara(i + 5);
            SQCharaData data;

            if (charaDic.TryGetValue(charaid, out data))
            {
                ret[i] = data;
            }
            else
            {
                /*
                //見つからなかったらランダム
                data = GetRandomCharaData();

                //特殊キャラの場合は引き直し
                while (data.charaID < 100)
                {
                    data = GetRandomCharaData();
                }
                ret[i] = data;
                */
                ret[i] = null;
            }
        }
        return ret;
    }
    
    /// <summary>
    /// フォトスタジオ用のキャラデータを取得する
    /// </summary>
    public SQCharaData GetPhotoStudioChara()
    {
        int charaid = SaveManager.GetChara(9);

        SQCharaData data = null;

        if (charaDic.TryGetValue(charaid, out data))
        {
            return data;
        }
        else
        {
            //見つからなかったらランダム
            data = GetRandomCharaData();

            //特殊キャラの場合は引き直し
            while (data.charaID < 100)
            {
                data = GetRandomCharaData();
            }
            return data;
        }
    }
    
    public SQCharaData GetCharaData(int charaID)
    {
        SQCharaData data = null;
        if (charaDic.TryGetValue(charaID, out data))
        {
            return data;
        }
        return null;
    }

    /// <summary>
    /// キャラリストからランダムに１つ返す
    /// </summary>
    /// <returns></returns>
    public SQCharaData GetRandomCharaData()
    {
        if (charaList != null)
        {
            int i = Random.Range(0, charaList.Count);

            return charaList[i];
        }
        return null;
    }

    public SQCharaData GetRandomCharaDataWithoutAprilfool()
    {
        SQCharaData data;
        while (true)
        {
            data = GetRandomCharaData();
            if (data == null)
            {
                return null;
            }
            if (data.charaID > 100 && data.charaID < 600)
            {
                return data;
            }
        }
    }

    /// <summary>
    /// 総キャラクタ数を取得する
    /// </summary>
    public int CharaCount()
    {
        return charaList.Count;
    }
}