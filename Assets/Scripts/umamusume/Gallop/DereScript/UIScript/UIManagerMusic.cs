using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagerMusic : MonoBehaviour
{
    /*Prefab*/
    [SerializeField]
    private GameObject MusicView;

    private List<SQMusicData> musicList;
    private Dictionary<int, SQMusicData> musicDic;
    private Dictionary<int, Sprite> iconDic;

    public bool isLoad
    {
        get
        {
            if (musicDic != null)
            {
                if (musicDic.Count > 0)
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
    /// 音楽関連のデータをいったんすべて破棄して読み込み直す
    /// </summary>
    public void ResetData()
    {
        if (musicList != null)
        {
            musicList.Clear();
            musicList = null;
        }
        if (musicDic != null)
        {
            musicDic.Clear();
            musicDic = null;
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
        int iconID = SaveManager.GetInt("music_jacket", -1);
        if (iconID > 0)
        {
            StartCoroutine(CreateSprite(iconID));
        }
    }

    /// <summary>
    /// MusicIcon用のSprite辞書を生成する
    /// </summary>
    /// <returns></returns>
    private IEnumerator CreateSpriteDic()
    {
        if (iconDic == null)
        {
            iconDic = new Dictionary<int, Sprite>();
        }
        if (musicDic == null)
        {
            musicDic = new Dictionary<int, SQMusicData>();
        }
        if (musicList == null)
        {
            musicList = new List<SQMusicData>();
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

        //曲名のリスト生成
        musicList.AddRange(MasterDBManager.instance.GetPlayableMusic());
        //アナザー楽曲
        musicList.AddRange(MasterDBManager.instance.GetAnotherMusic());
        //エイプリルフール曲
        musicList.AddRange(MasterDBManager.instance.GetAprilfoolMusic());

        List<string> jaketnames = new List<string>();
        foreach (var tmp in musicList)
        {
            jaketnames.Add(string.Format("jacket_{0:0000}.unity3d", tmp.jacket_id));
            if (!musicDic.ContainsKey(tmp.id))
            {
                musicDic.Add(tmp.id, tmp);
            }
        }
        //コルーチンでDLを投げる
        StartCoroutine(AssetManager.instance.DownloadFromFilenamesAsync(jaketnames));

        AddAprilIconSprite();

        //アイコン作成
        foreach (var tmp in musicList)
        {
            //並列で実行
            StartCoroutine(CreateSprite(tmp.jacket_id));
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
        AddSpriteDicResource(20160401, "jacket_chihiro");
        AddSpriteDicResource(20180401, "kirarinLobo");
        AddSpriteDicResource(20190401, "jacket_nono");
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

        string name = string.Format("jacket_{0:0000}.unity3d", iconID);

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
    public Sprite GetIconSprite(int jacket_id)
    {
        if (iconDic == null)
        {
            return null;
        }
        if (iconDic.ContainsKey(jacket_id))
        {
            return iconDic[jacket_id];
        }
        return null;
    }

    /// <summary>
    /// MusicViewを生成し表示
    /// </summary>
    public void CreateMusicView()
    {
        GameObject obj = Instantiate(MusicView);
        GameObject baseObj = GameObject.Find("Canvas");
        obj.transform.SetParent(baseObj.transform, false);

        UIMusicView musicView = obj.GetComponent<UIMusicView>();
        musicView.SetMusicList(musicList);
    }


    /// <summary>
    /// 現在選択されている曲データを取得する
    /// </summary>
    public SQMusicData GetCurrentMusic()
    {
        /*
        GameObject baseObj = GameObject.Find("Canvas");
        if (baseObj == null) { return null; }
        Transform MusicSelect = GameObjectUtility.FindChildTransform("MusicSelect", baseObj.transform);
        if(MusicSelect == null) { return null; }

        UIMusicSelect musicSelect = MusicSelect.GetComponent<UIMusicSelect>();
        SQMusicData data;

        if(musicSelect.currentMusic == 0)
        {
            return null;
        }
        else
        {
            if (musicDic.TryGetValue(musicSelect.currentMusic, out data))
            {
                return data;
            }
            else
            {
                data = MasterDBManager.instance.GetMusicDataFromID(musicSelect.currentMusic);
                if(data.name != null)
                {
                    return data;
                }
            }
        }
        */

        SQMusicData data;
        int currentMusic = SaveManager.GetInt("music");

        if (musicDic.TryGetValue(currentMusic, out data))
        {
            return data;
        }
        else
        {
            data = MasterDBManager.instance.GetMusicDataFromID(currentMusic);
            if (data != null && data.name != null)
            {
                return data;
            }
        }

        return null;
    }

    public IEnumerator SetMusicInfo(int musicID)
    {
        SQMusicData data = null;
        GameObject MusicInfo = GameObject.Find("Canvas/Panel/LiveContent/MusicInfo");

        while (!isLoad)
        {
            yield return null;
        }

        if (musicDic.TryGetValue(musicID, out data))
        {
            MusicInfo.GetComponent<UIMusicInfo>().SetData(data);
        }
        else
        {
            MusicInfo.GetComponent<UIMusicInfo>().SetData(musicID);
        }

    }

    public void CheckSoloSong()
    {
        GameObject MusicInfo = GameObject.Find("Canvas/Panel/LiveContent/MusicInfo");
        MusicInfo.GetComponent<UIMusicInfo>().CheckSoloSong();
    }

    public int GetLiveAttribute(int musicID)
    {
        SQMusicData data = null;
        if (musicDic != null)
        {
            if (musicDic.TryGetValue(musicID, out data))
            {
                return data.liveAttribute;
            }
        }
        return -1;
    }
}