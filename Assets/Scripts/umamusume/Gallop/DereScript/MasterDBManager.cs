using Cute;
using Sqlite3Plugin;
using Stage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MasterDBManager : MonoBehaviour
{
    private static MasterDBManager _instance;
    public static MasterDBManager instance
    {
        get
        {
            return _instance;
        }
    }

    public static bool IsInstanceEmpty()
    {
        return _instance == null;
    }

    //ファイルDB
    private DBProxy masterDB;

    public bool isError = false;

    public const string DB_NAME = "master.db";

    private const string MusicParam = "music_data.name, live_data.music_data_id, live_data.id,live_data.live_bg, live_data.jacket_id, live_data.cyalume, live_data.member_number, music_data.composer,music_data.lyricist, music_info.discription, live_data.v_mv, live_data.chara_all_flag, live_data.type, music_data.name_kana";
    private const string CharaParam = "name,chara_id,model_height_id,model_weight_id,model_bust_id,model_skin_id,type,base_card_id,height,name_kana";

    private const string liveFile = "3d_live.unity3d";

    private const string CharaDataFile = "3d_chara_data.unity3d";

    private const int DEFAULT_DRESS_ID = 1;

    /// <summary>
    /// 3d_live.unity3d
    /// </summary>
    public Master3dLive master3dLive = null;

    /// <summary>
    /// 3d_chara_data.unity3d
    /// </summary>
    public Master3DCharaData master3DCharaData = null;

    /// <summary>
    /// master.db/card_data
    /// </summary>
    public MasterCardData masterCardData = null;

    /// <summary>
    /// master.db/live_3dchara_spring
    /// </summary>
    public MasterLive3dcharaSpring masterLive3dcharaSpring = null;

    /// <summary>
    /// master.db/chara_data
    /// </summary>
    public MasterCharaData masterCharaData = null;

    /// <summary>
    /// master.db/dress_color_data
    /// </summary>
    public MasterDressColorData masterDressColorData = null;

    /// <summary>
    /// master.db/dress_data
    /// </summary>
    public MasterDressData masterDressData = null;

    /// <summary>
    /// master.db/live_data
    /// </summary>
    public MasterLiveData masterLiveData = null;

    /// <summary>
    /// master.db/card_gallery_motion
    /// </summary>
    public MasterCardGalleryMotion masterCardGalleryMotion = null;

    // Use this for initialization
    void Start()
    {
        OpenMasterDB();
        DontDestroyOnLoad(this.gameObject);

        //3d_liveと3d_chara_dataをここで読み込む
        StartCoroutine(LoadMaster3dLive());
        StartCoroutine(Load3DCharaData());

        //StartCoroutine(LoadCabinet());
    }

    // Update is called once per frame
    void Update()
    {
    }

    void Awake()
    {
        if (_instance != null)
        {
            UnityEngine.Object.Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    private void OnDestroy()
    {
        if (masterDB == null) return;

        if (masterDB.IsOpened())
        {
            masterDB.CloseDB();
        }
        _instance = null;
    }

    /// <summary>
    /// ローカルのマニュフェストパス
    /// </summary>
    public string masterPath
    {
        get
        {
            return Path.Combine(PathHandler.instance.manifestroot, DB_NAME);
        }
    }

    /// <summary>
    /// 初期化が終わっているか
    /// </summary>
    public bool isInitDB
    {
        get
        {
            if (isLoadDB)
            {
                if (master3dLive != null && master3DCharaData != null && masterCardData != null)
                {
                    return true;
                }
            }
            return false;
        }
    }



    /// <summary>
    /// DBが使用可能か
    /// </summary>
    public bool isLoadDB
    {
        get
        {
            if (masterDB != null)
            {
                return masterDB.IsOpened();
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// ローカルにDBファイルが存在するか
    /// </summary>
    public bool isLocalSave
    {
        get
        {
            if (File.Exists(masterPath)) { return true; }
            else { return false; }
        }
    }

    /// <summary>
    /// MasterDBを読み込む
    /// </summary>
    public void OpenMasterDB()
    {
        isError = false;

        if (File.Exists(masterPath))
        {
            masterDB = new DBProxy();
            masterDB.Open(masterPath);

            masterCardData = new MasterCardData();
            masterCardData.Load(ref masterDB);

            masterLive3dcharaSpring = new MasterLive3dcharaSpring();
            masterLive3dcharaSpring.Load(ref masterDB);

            masterCharaData = new MasterCharaData();
            masterCharaData.Load(ref masterDB);

            masterDressColorData = new MasterDressColorData();
            masterDressColorData.Load(ref masterDB);

            masterDressData = new MasterDressData();
            masterDressData.Load(ref masterDB);

            masterLiveData = new MasterLiveData();
            masterLiveData.Load(ref masterDB);

            masterCardGalleryMotion = new MasterCardGalleryMotion();
            masterCardGalleryMotion.Load(ref masterDB);
        }
        else
        {
            isError = true;
            print(masterPath + "が読み込めません");
        }
    }

    public void DeleteMasterDB()
    {
        string str = this.masterPath;
        try
        {
            if (isLoadDB)
            {
                masterDB.CloseDB();
            }

            if (isLocalSave)
            {
                File.Delete(this.masterPath);
            }
        }
        catch (Exception e)
        {
            print("MasterDBを削除できません:" + e);
        }

        //エラーをオフ
        isError = false;
    }

    public IEnumerator DownloadMasterDB()
    {
        string name = ManifestManager.instance.manifestPath;
        if (File.Exists(name))
        {
            URLData s = new URLData();
            DBProxy tmpdb = new DBProxy();
            tmpdb.Open(name);
            if (!tmpdb.IsOpened()) yield break;

            using (Query query = tmpdb.Query(@"select name,hash from manifests where name like 'master.mdb'"))
            {
                if (query.Step())
                {
                    s._name = query.GetText(0);
                    s.hash = query.GetText(1);
                }
            }
            tmpdb.CloseDB();
            tmpdb = null;
            yield return AssetManager.instance.download(s);
            string str = AssetManager.instance.GetCachefromName("master.mdb");

            File.Move(str, this.masterPath);
        }
    }

    /// <summary>
    /// Master3dLiveを読み込む
    /// </summary>
    private IEnumerator LoadMaster3dLive()
    {
        if (master3dLive != null)
        {
            master3dLive = null;
        }

        while (true)
        {
            if (isLoadDB && AssetManager.instance.isManifestLoad)
            {
                break;
            }
            if (isError || ManifestManager.instance.isError)
            {
                yield break;
            }
            yield return null;
        }

        //3d_live.unity3dをDL
        yield return AssetManager.instance.DownloadFromFilename(liveFile);
        //AssetBundle読み込み
        AssetBundleCreateRequest req = AssetManager.instance.LoadAssetFromNameAsync(liveFile);
        if (req == null)
        {
            yield break;
        }
        yield return req;

        AssetBundle bundle = req.assetBundle;
        AssetBundleRequest text = bundle.LoadAllAssetsAsync<TextAsset>();
        while (!text.isDone)
        {
            yield return null;
        }

        ArrayList result = null;
        foreach (var obj in text.allAssets)
        {
            if (obj is TextAsset)
            {
                result = Utility.ConvertCSV(obj.ToString(), true);
                break;
            }
        }
        master3dLive = new Master3dLive(result);

        bundle.Unload(false);
    }

    /// <summary>
    /// Master3DCharaDataを読み込む
    /// </summary>
    private IEnumerator Load3DCharaData()
    {
        if (master3DCharaData != null)
        {
            master3DCharaData = null;
        }

        while (true)
        {
            if (isLoadDB && AssetManager.instance.isManifestLoad)
            {
                break;
            }
            if (isError || ManifestManager.instance.isError)
            {
                yield break;
            }
            yield return null;
        }

        //3d_chara_data.unity3dをDL
        yield return AssetManager.instance.DownloadFromFilename(CharaDataFile);
        //AssetBundle読み込み
        AssetBundleCreateRequest req = AssetManager.instance.LoadAssetFromNameAsync(CharaDataFile);
        if (req == null)
        {
            yield break;
        }
        yield return req;

        AssetBundle bundle = req.assetBundle;
        AssetBundleRequest text = bundle.LoadAllAssetsAsync<TextAsset>();
        while (!text.isDone)
        {
            yield return null;
        }

        ArrayList result = null;
        foreach (var obj in text.allAssets)
        {
            if (obj is TextAsset)
            {
                result = Utility.ConvertCSV(obj.ToString(), true);
                break;
            }
        }
        master3DCharaData = new Master3DCharaData(result);

        bundle.Unload(false);
    }

    public IEnumerator LoadCabinet()
    {
        while (true)
        {
            if (isLoadDB && AssetManager.instance.isManifestLoad)
            {
                break;
            }
            if (isError || ManifestManager.instance.isError)
            {
                yield break;
            }
            yield return null;
        }

        yield return AssetManager.instance.DownloadFromFilename(Cabinet.ASSET_BUNDLE_NAME);

        yield return ResourcesManager.instance.LoadAsset(Cabinet.ASSET_BUNDLE_NAME, null);

        yield return StartCoroutine(Cabinet.Load());
    }

    /// <summary>
    /// 再生可能なすべての曲を取得
    /// </summary>
    /// <returns></returns>
    public List<SQMusicData> GetPlayableMusic()
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQMusicData> SQMD = new List<SQMusicData>();
        string sql = string.Format("Select {0} from {1} where {2} order by {3}",
            MusicParam,
            "(music_data inner join live_data on music_data.id = live_data.music_data_id) inner join music_info on music_data.id = music_info.id",
            "live_data.sort < 1900 and live_data.prp_flag and live_data.v_mv != 4 and live_data.v_mv != 12",
            "live_data.sort");

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQMusicData data = new SQMusicData();

                data.name = query.GetText(0);
                data.music_data_id = query.GetInt(1);
                data.id = query.GetInt(2);
                data.live_bg = query.GetInt(3);
                data.jacket_id = query.GetInt(4);
                data.cyalume = query.GetInt(5);
                data.member_number = query.GetInt(6);
                data.composer = query.GetText(7);
                data.lyricist = query.GetText(8);
                data.discription = query.GetText(9);
                int tmp = query.GetInt(10);
                if (tmp == 1) { data.smartmode = true; }
                else { data.smartmode = false; }

                data.chara_all_flag = query.GetInt(11);
                data.type = query.GetInt(12);
                data.name_kana = query.GetText(13);

                //M@GIC☆用
                if (data.id == 10036)
                {
                    data.name += " (Grand Live)";
                }
                //SecretMirage,Harmonics
                if (data.id == 10441 || data.id == 10857)
                {
                    data.name += " (Another)";
                }
                //ハレ晴レユカイ
                if (data.id == 10862)
                {
                    data.name += " (Quintet)";
                }
                SQMD.Add(data);
            }
        }
        return SQMD;
    }

    /// <summary>
    /// アナザー楽曲を取得
    /// </summary>
    /// <returns></returns>
    public List<SQMusicData> GetAnotherMusic()
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQMusicData> SQMD = new List<SQMusicData>();
        string sql = string.Format("Select {0} from {1} where {2} order by {3}",
            MusicParam,
            "(music_data inner join live_data on music_data.id = live_data.music_data_id) inner join music_info on music_data.id = music_info.id",
            "live_data.chara_all_flag = 2 and live_data.release_type < 4",
            "live_data.sort");

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQMusicData data = new SQMusicData();

                data.name = query.GetText(0);
                data.music_data_id = query.GetInt(1);
                data.id = query.GetInt(2);
                data.live_bg = query.GetInt(3);
                data.jacket_id = query.GetInt(4);
                data.cyalume = query.GetInt(5);
                data.member_number = query.GetInt(6);
                data.composer = query.GetText(7);
                data.lyricist = query.GetText(8);
                data.discription = query.GetText(9);
                int tmp = query.GetInt(10);
                if (tmp == 1) { data.smartmode = true; }
                else { data.smartmode = false; }
                data.chara_all_flag = query.GetInt(11);
                data.type = query.GetInt(12);
                data.name_kana = query.GetText(13);

                SQMD.Add(data);
            }
        }

        List<SQMusicData> returndata = new List<SQMusicData>();
        foreach (var tmp in SQMD)
        {
            SQMusicData data = GetAnotherMusicFromName(tmp.name);
            if (data != null)
            {
                returndata.Add(data);
            }
        }


        return returndata;
    }

    /// <summary>
    /// 音楽IDから曲を取得
    /// </summary>
    /// <param name="musicID"></param>
    /// <returns></returns>
    public SQMusicData GetMusicDataFromID(int musicID)
    {
        if (!isLocalSave)
        {
            return null;
        }

        string sql = string.Format("Select {0} from {1} where {2} order by {3}",
            MusicParam,
            "(music_data inner join live_data on music_data.id = live_data.music_data_id) inner join music_info on music_data.id = music_info.id",
            string.Format("live_data.id = {0}", musicID),
            "live_data.sort");

        SQMusicData data = new SQMusicData();

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                data.name = query.GetText(0);
                data.music_data_id = query.GetInt(1);
                data.id = query.GetInt(2);
                data.live_bg = query.GetInt(3);
                data.jacket_id = query.GetInt(4);
                data.cyalume = query.GetInt(5);
                data.member_number = query.GetInt(6);
                data.composer = query.GetText(7);
                data.lyricist = query.GetText(8);
                data.discription = query.GetText(9);
                int tmp = query.GetInt(10);
                if (tmp == 1) { data.smartmode = true; }
                else { data.smartmode = false; }
                data.chara_all_flag = query.GetInt(11);
                data.type = query.GetInt(12);
                data.name_kana = query.GetText(13);

                //M@GIC☆用
                if (data.id == 10036)
                {
                    data.name += " (Grand Live)";
                }
                //SecretMirage,Harmonics
                if (data.id == 10441 || data.id == 10857)
                {
                    data.name += " (Another)";
                }
                //ハレ晴レユカイ
                if (data.id == 10862)
                {
                    data.name += " (Quintet)";
                }
            }
        }
        if (data.name != "")
        {
            return data;
        }
        return null;
    }

    private SQMusicData GetAnotherMusicFromName(string musicName)
    {
        if (!isLocalSave)
        {
            return null;
        }

        string sql = string.Format("Select {0} from {1} where {2} order by {3}",
            MusicParam,
            "(music_data inner join live_data on music_data.id = live_data.music_data_id) inner join music_info on music_data.id = music_info.id",
            string.Format("live_data.prp_flag = 0 and music_data.name = \"{0}\"", musicName),
            "live_data.sort");


        SQMusicData data = new SQMusicData();
        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                data.name = query.GetText(0);
                data.music_data_id = query.GetInt(1);
                data.id = query.GetInt(2);
                data.live_bg = query.GetInt(3);
                data.jacket_id = query.GetInt(4);
                data.cyalume = query.GetInt(5);
                data.member_number = query.GetInt(6);
                data.composer = query.GetText(7);
                data.lyricist = query.GetText(8);
                data.discription = query.GetText(9);
                int tmp = query.GetInt(10);
                if (tmp == 1) { data.smartmode = true; }
                else { data.smartmode = false; }
                data.chara_all_flag = query.GetInt(11);
                data.type = query.GetInt(12);
                data.name_kana = query.GetText(13);

            }
        }
        return data;
    }


    /// <summary>
    /// エイプリルフール曲を取得
    /// </summary>
    /// <returns></returns>
    public List<SQMusicData> GetAprilfoolMusic()
    {
        if (!isLocalSave)
        {
            return null;
        }

        Dictionary<int, SQMusicData> SQMD = new Dictionary<int, SQMusicData>();
        string sql = string.Format("Select {0} from {1} where {2} order by {3}",
            MusicParam,
            "(music_data inner join live_data on music_data.id = live_data.music_data_id) inner join music_info on music_data.id = music_info.id",
            "live_data.id >= 901 and live_data.id <= 910",
            "live_data.sort");

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQMusicData data = new SQMusicData();

                data.name = query.GetText(0);
                data.music_data_id = query.GetInt(1);
                data.id = query.GetInt(2);
                data.live_bg = query.GetInt(3);
                data.jacket_id = query.GetInt(4);
                data.cyalume = query.GetInt(5);
                data.member_number = query.GetInt(6);
                data.composer = query.GetText(7);
                data.lyricist = query.GetText(8);
                data.discription = query.GetText(9);
                int tmp = query.GetInt(10);
                if (tmp == 1) { data.smartmode = true; }
                else { data.smartmode = false; }
                data.chara_all_flag = query.GetInt(11);
                data.type = query.GetInt(12);
                data.name_kana = query.GetText(13);

                switch (data.id)
                {
                    case 901:
                        data.jacket_id = 20160401;
                        break;
                    case 902://プチデレラはアイコンある
                        //data.jacket_id = 20170401;
                        break;
                    case 903:
                        data.jacket_id = 20180401;
                        break;
                    case 907:
                        data.jacket_id = 20190401;
                        break;
                }
                if (!SQMD.ContainsKey(data.live_bg))
                {
                    SQMD.Add(data.live_bg, data);
                }
            }
        }
        return new List<SQMusicData>(SQMD.Values);
    }

    /// <summary>
    /// 全てのキャラクタを取得
    /// </summary>
    public List<SQCharaData> GetAllCharas()
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQCharaData> sqcd = new List<SQCharaData>();

        string sql = string.Format("select {0} from {1} where base_card_id > 0 order by base_card_id",
            CharaParam,
            "chara_data");

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQCharaData data = new SQCharaData();
                data.name = query.GetText(0);
                data.charaID = query.GetInt(1);
                data.modelHeightId = query.GetInt(2);
                data.modelWeightId = query.GetInt(3);
                data.modelBustId = query.GetInt(4);
                data.modelSkinId = query.GetInt(5);
                data.type = query.GetInt(6);
                data.baseCardId = query.GetInt(7);
                data.height = query.GetInt(8);
                data.name_kana = query.GetText(9);

                sqcd.Add(data);
            }
        }
        return sqcd;

    }

    public List<SQCharaData> GetVRCharas()
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQCharaData> sqcd = new List<SQCharaData>();

        string sql = string.Format("select {0} from {1} where {2} order by base_card_id",
            CharaParam,
            "chara_data",
            "chara_id >= 600 and chara_id <= 800"
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQCharaData data = new SQCharaData();
                data.name = query.GetText(0);
                data.charaID = query.GetInt(1);
                data.modelHeightId = query.GetInt(2);
                data.modelWeightId = query.GetInt(3);
                data.modelBustId = query.GetInt(4);
                data.modelSkinId = query.GetInt(5);
                data.type = query.GetInt(6);
                data.baseCardId = query.GetInt(7);
                data.height = query.GetInt(8);
                data.name_kana = query.GetText(9);

                //女性スタッフ/三好紗南/ぴにゃこら太/関裕美/喜多見柚 だけ追加
                if (data.charaID == 672 || data.charaID == 682 || data.charaID == 701 || data.charaID == 725 || data.charaID == 726)
                {
                    sqcd.Add(data);
                }
            }
        }
        return sqcd;

    }

    public List<SQCharaData> GetAprilfoolCharas()
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQCharaData> sqcd = new List<SQCharaData>();

        string sql = string.Format("select {0} from {1} where {2} order by base_card_id",
            CharaParam,
            "chara_data",
            "chara_id >= 20 and chara_id <= 83"
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQCharaData data = new SQCharaData();
                data.name = query.GetText(0);
                data.charaID = query.GetInt(1);
                data.modelHeightId = query.GetInt(2);
                data.modelWeightId = query.GetInt(3);
                data.modelBustId = query.GetInt(4);
                data.modelSkinId = query.GetInt(5);
                data.type = query.GetInt(6);
                data.baseCardId = query.GetInt(7);
                data.height = query.GetInt(8);
                data.name_kana = query.GetText(9);

                if (data.charaID == 83)
                {
                    data.height = 182;//きらりんロボはいったん182cmにする。拡大はDirectorで直接大きさを弄る
                    data.name_kana = "きらりんろぼ";
                }

                sqcd.Add(data);
            }
        }
        return sqcd;

    }

    /// <summary>
    /// 共通衣装を取得
    /// </summary>
    public List<SQDressData> GetCommonDresses()
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQDressData> sqdd = new List<SQDressData>();
        string sql = string.Format("Select {0} from {1} where {2}",
            "id,name",
            "dress_data",
            "id < 100 AND dress_type < 2"
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQDressData data = new SQDressData();
                data.dressID = query.GetInt(0);
                data.dressName = query.GetText(1);

                sqdd.Add(data);
            }
        }
        return sqdd;
    }

    /// <summary>
    /// ショップ衣装を取得
    /// </summary>
    public List<SQDressData> GetShopDressesIcon()
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQDressData> sqdd = new List<SQDressData>();
        string sql = string.Format("Select {0} from {1} where id > 100",
            "id,name",
            "dress_data"
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQDressData data = new SQDressData();
                data.dressID = query.GetInt(0);
                data.dressName = query.GetText(1);

                sqdd.Add(data);
            }
        }
        return sqdd;
    }

    /// <summary>
    /// ショップ衣装の辞書を取得
    /// </summary>
    public Dictionary<int, List<SQDressData>> GetShopDresses()
    {
        if (!isLocalSave)
        {
            return null;
        }

        try
        {
            string sql = string.Format("Select {0} from {1} where {2} order by {3}",
            "*",
            "dress_data join dress_target_2 using (id)",
            "id > 7000000 and dress_type < 3",
            "id");

            using (Query query = this.masterDB.Query(sql))
            {
                Dictionary<int, List<SQDressData>> dressdic = new Dictionary<int, List<SQDressData>>(100);

                while (query.Step())
                {
                    int id = query.GetInt(0);

                    string str = query.GetText(6);
                    string[] splitstr = str.Split(',');

                    foreach (var charaIDstr in splitstr)
                    {
                        int charaID = int.Parse(charaIDstr);
                        if (charaID == 0) { continue; }

                        SQDressData data = new SQDressData();
                        data.dressName = query.GetText(1);
                        data.charaID = charaID;
                        data.dressID = id;

                        List<SQDressData> dresslist = null;
                        if (dressdic.TryGetValue(charaID, out dresslist))
                        {
                            dresslist.Add(data);
                        }
                        else
                        {
                            dresslist = new List<SQDressData>();
                            dresslist.Add(data);
                            dressdic.Add(charaID, dresslist);
                        }
                    }
                }

                return dressdic;
            }
        }
        catch (Exception)
        {
            return GetShopDresses_2();
        }
    }

    /// <summary>
    /// ショップ衣装の辞書を取得
    /// </summary>
    public Dictionary<int, List<SQDressData>> GetShopDresses_2()
    {
        if (!isLocalSave)
        {
            return null;
        }

        try
        {
            string sql = string.Format("Select {0} from {1} where {2} order by {3}",
            "*",
            "dress_data join dress_target using (id)",
            "id > 7000000 and dress_type < 3",
            "id");

            using (Query query = this.masterDB.Query(sql))
            {
                Dictionary<int, List<SQDressData>> dressdic = new Dictionary<int, List<SQDressData>>(100);

                while (query.Step())
                {
                    int id = query.GetInt(0);
                    int charaID;

                    for (int i = 6; i <= 26; i++)
                    {
                        charaID = query.GetInt(i);
                        if (charaID == 0) { continue; }

                        SQDressData data = new SQDressData();
                        data.dressName = query.GetText(1);
                        data.charaID = charaID;
                        data.dressID = id;

                        List<SQDressData> dresslist = null;
                        if (dressdic.TryGetValue(charaID, out dresslist))
                        {
                            dresslist.Add(data);
                        }
                        else
                        {
                            dresslist = new List<SQDressData>();
                            dresslist.Add(data);
                            dressdic.Add(charaID, dresslist);
                        }
                    }
                }

                return dressdic;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    /* 廃止 UIManagerDressで取得してください
    /// <summary>
    ///キャラクタIDから衣装データリストを取得
    /// </summary>
    public List<SQDressData> GetSSRDressesFromCharaID(int chara_id)
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQDressData> sqdd = new List<SQDressData>();

        string sql = string.Format("Select {0} from {1} where {2}",
            "id,name,open_dress_id,evolution_id",
            "card_data",
            string.Format("chara_id = {0} and evolution_id > 0 and open_dress_id > 0", chara_id)
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQDressData data = new SQDressData();

                data.dressID = 100;
                data.cardID = query.GetInt(0);
                data.dressName = query.GetText(1);
                data.dressID = query.GetInt(2);
                data.cardIDPlus = query.GetInt(3);
                data.charaID = chara_id;

                sqdd.Add(data);
            }
        }
        return sqdd;

    }
    */
    /// <summary>
    ///キャラクタIDから衣装データリストを取得
    /// </summary>
    public List<SQDressData> GetALLSSRDresses()
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQDressData> sqdd = new List<SQDressData>();

        string sql = string.Format("Select {0} from {1} where {2}",
            "id,name,open_dress_id,chara_id,attribute,evolution_id,solo_live",
            "card_data",
            string.Format("evolution_id > 0 and evolution_id < 500000 and open_dress_id > 0 and open_dress_id < 9000")
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQDressData data = new SQDressData();

                data.dressID = 100;
                data.dressName = "";
                data.cardID = query.GetInt(0);
                data.cardIDPlus = query.GetInt(5);
                data.charaID = query.GetInt(3);
                data.cardName = query.GetText(1);
                data.type = query.GetInt(4);
                data.openDressID = query.GetInt(2);
                data.soloLive = query.GetInt(6);

                sqdd.Add(data);
            }
        }
        return sqdd;

    }

    /// <summary>
    /// エイプリルフール衣装
    /// </summary>
    public List<SQDressData> GetAprilDresses()
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQDressData> sqdd = new List<SQDressData>();

        string sql = string.Format("Select {0} from {1} where {2}",
            "id,name,open_dress_id,chara_id,attribute,evolution_id",
            "card_data",
            string.Format("evolution_id > 0 and open_dress_id > 9000")
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQDressData data = new SQDressData();

                data.dressID = 100;
                data.dressName = "";
                data.cardID = query.GetInt(0);
                data.cardIDPlus = query.GetInt(5);
                data.charaID = query.GetInt(3);
                data.cardName = query.GetText(1);
                data.type = query.GetInt(4);
                data.openDressID = query.GetInt(2);

                sqdd.Add(data);
            }
        }
        return sqdd;

    }

    /// <summary>
    /// カードIDからキャラクタデータを取得
    /// </summary>
    public SQCharaData GetCharaDataFromCardID(int card_id)
    {
        if (!isLocalSave)
        {
            return null;
        }
        string sql = string.Format("Select {0} from {1} where {2}{3}",
            "chara_data.name,chara_data.chara_id,chara_data.model_height_id,chara_data.model_weight_id,chara_data.model_bust_id,chara_data.model_skin_id,chara_data.type,chara_data.base_card_id,chara_data.height",
            "card_data join chara_data on card_data.chara_id=chara_data.chara_id",
            "card_data.id = ", card_id);

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQCharaData data = new SQCharaData();
                data.name = query.GetText(0);
                data.charaID = query.GetInt(1);
                data.modelHeightId = query.GetInt(2);
                data.modelWeightId = query.GetInt(3);
                data.modelBustId = query.GetInt(4);
                data.modelSkinId = query.GetInt(5);
                data.type = query.GetInt(6);
                data.baseCardId = query.GetInt(7);
                data.height = query.GetInt(8);
                if (data.charaID == 83)
                {
                    data.height = 182;//きらりんロボはいったん182cmにする。拡大はDirectorで直接大きさを弄る
                }

                return data;
            }
        }
        return new SQCharaData();

    }

    /// <summary>
    /// カードIDからドレスデータを取得
    /// </summary>
    public SQDressData GetDressDataFromCardID(int card_id)
    {
        if (!isLocalSave)
        {
            return null;
        }
        string sql = string.Format("Select {0} from {1} where {2}",
            "id,name,open_dress_id,chara_id,attribute,evolution_id",
            "card_data",
            string.Format("id = {0}", card_id));

        SQDressData data = new SQDressData();
        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                data.dressID = 100;
                data.dressName = "";
                data.cardID = query.GetInt(0);
                data.cardIDPlus = query.GetInt(5);
                data.charaID = query.GetInt(3);
                data.cardName = query.GetText(1);
                data.type = query.GetInt(4);
                data.openDressID = query.GetInt(2);
            }
        }
        return data;
    }

    /// <summary>
    /// 共通衣装を取得
    /// </summary>
    /// <returns></returns>
    public SQDressData GetDressDataFromDressID(int dress_id)
    {
        if (!isLocalSave)
        {
            return null;
        }

        List<SQDressData> sqdd = new List<SQDressData>();
        string sql = string.Format("Select {0} from {1} where {2}",
            "id,name",
            "dress_data",
            string.Format("id = {0} AND dress_type < 2", dress_id)
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQDressData data = new SQDressData();
                data.dressID = query.GetInt(0);
                data.dressName = query.GetText(1);

                sqdd.Add(data);
            }
        }
        if (sqdd.Count > 0)
        {
            return sqdd[0];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// サイリウムカラーの取得
    /// </summary>
    /// <returns></returns>
    public MasterCyalumeColorData GetCyalumeColorData()
    {

        if (!isLocalSave)
        {
            return null;
        }

        string sql = string.Format("SELECT {0} FROM {1}",
            "raw",
            "cyalume_color"
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                string text = query.GetText(0);
                ArrayList list = Utility.ConvertCSV(text);
                return new MasterCyalumeColorData(list);
            }
        }
        return null;
    }

    /// <summary>
    /// 曲オリジナルメンバーのリストを取得する
    /// </summary>
    /// <returns></returns>
    public List<SQLiveDataPosition> GetLiveDataPosition()
    {
        if (!isLocalSave)
        {
            return null;
        }
        List<SQLiveDataPosition> retdata = new List<SQLiveDataPosition>();

        string sql = string.Format("SELECT {0} FROM {1}",
            "*",
            "live_data_position"
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQLiveDataPosition data = new SQLiveDataPosition();
                data.liveDataID = query.GetInt(0);
                data.positionNum = query.GetInt(1);
                data.charaPosition1 = query.GetInt(2);
                data.dressPosition1 = query.GetInt(3);
                data.charaPosition2 = query.GetInt(4);
                data.dressPosition2 = query.GetInt(5);
                data.charaPosition3 = query.GetInt(6);
                data.dressPosition3 = query.GetInt(7);
                data.charaPosition4 = query.GetInt(8);
                data.dressPosition4 = query.GetInt(9);
                data.charaPosition5 = query.GetInt(10);
                data.dressPosition5 = query.GetInt(11);

                retdata.Add(data);
            }
        }

        //空ならNullを返す
        if (retdata.Count > 0)
        {
            return retdata;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// フォトスタジオのステージ情報を取得する
    /// </summary>
    public List<SQPhotoStudioStageData> GetPhotoStudioStageData()
    {

        if (!isLocalSave)
        {
            return null;
        }
        List<SQPhotoStudioStageData> retdata = new List<SQPhotoStudioStageData>();

        string sql = string.Format("SELECT {0} FROM {1}",
            "id,disp_order,disp_name,bg_id,bg_type",
            "gallery_stage_list"
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQPhotoStudioStageData data = new SQPhotoStudioStageData();
                data.id = query.GetInt(0);
                data.disp_order = query.GetInt(1);
                data.disp_name = query.GetText(2);
                data.bg_id = query.GetInt(3);
                data.bg_type = query.GetInt(4);

                retdata.Add(data);
            }
        }

        //空ならNullを返す
        if (retdata.Count > 0)
        {
            return retdata;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// フォトスタジオのステージ情報を取得する
    /// </summary>
    public SQPhotoStudioStageData GetPhotoStudioStageDataFromID(int stageID)
    {
        if (!isLocalSave)
        {
            return null;
        }
        List<SQPhotoStudioStageData> retdata = new List<SQPhotoStudioStageData>();

        string sql = string.Format("SELECT {0} FROM {1} where {2}",
            "id,disp_order,disp_name,bg_id,bg_type",
            "gallery_stage_list",
            string.Format("id = {0}", stageID)
            );

        using (Query query = this.masterDB.Query(sql))
        {
            while (query.Step())
            {
                SQPhotoStudioStageData data = new SQPhotoStudioStageData();
                data.id = query.GetInt(0);
                data.disp_order = query.GetInt(1);
                data.disp_name = query.GetText(2);
                data.bg_id = query.GetInt(3);
                data.bg_type = query.GetInt(4);

                retdata.Add(data);
            }
        }

        //空ならNullを返す
        if (retdata.Count > 0)
        {
            return retdata[0];
        }
        else
        {
            return null;
        }
    }

}
