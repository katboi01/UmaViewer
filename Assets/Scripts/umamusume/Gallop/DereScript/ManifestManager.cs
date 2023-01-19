using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Sqlite3Plugin;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class ManifestManager : MonoBehaviour
{
    private static ManifestManager _instance;
    public static ManifestManager instance
    {
        get
        {
            return _instance;
        }
    }

    private const string _manifestName = @"{0}_AHigh_SHigh";
    private string manifestName
    {
        get
        {
            //OS設定を取得
            string osname = OSConfig.osName;
            return string.Format(_manifestName,osname);
        }
    }
    private const string localmanifestName = @"manifest.db";
    private const string _manifestURLbase = @"https://asset-starlight-stage.akamaized.net/dl/{0}/manifests/{1}";

    public bool isError = false;
        
    /// <summary>
    /// マニュフェストURL
    /// </summary>
    private string getManifestURL(string version)
    {
        return string.Format(_manifestURLbase, version, manifestName);
    }

    /// <summary>
    /// ローカルのマニュフェストパス
    /// </summary>
    public string manifestPath
    {
        get
        {
            return Path.Combine(PathHandler.instance.manifestroot, localmanifestName);
        }
    }


    /// <summary>
    /// ファイルリスト
    /// </summary>
    private List<URLData> datalists = null;
    private Dictionary<String, URLData> namedictionary = null;
    private Dictionary<String, URLData> hashdictionary = null;
    
    /// <summary>
    ///　読み込みが完了しているか
    /// </summary>
    public bool isLoad
    {
        get
        {
            if(datalists != null && datalists.Count > 0)
            {
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// manifestがローカルに保存されているか
    /// </summary>
    public bool isLocalSave
    {
        get
        {
            if (File.Exists(manifestPath)) { return true; }
            else { return false; }
        }
    }

    // Use this for initialization
    void Start()
    {
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

            StartCoroutine(LoadManifestDB());
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    /// <summary>
    /// Hash値からURLを求める
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public URLData GetURLdataFromHash(string hash)
    {
        if (hash == null || hash == "")
        {
            return null;
        }

        URLData data = null;
        if (hashdictionary.TryGetValue(hash, out data))
        {
            return data;
        }

        return null;
    }

    /// <summary>
    /// ファイル名からURLを求める
    /// </summary>
    public URLData GetURLdataFromName(string name)
    {
        if (name == null || name == "")
        {
            return null;
        }

        URLData data = null;
        if (namedictionary.TryGetValue(name, out data))
        {
            return data;
        }
        return null;
    }

    /// <summary>
    /// デレステのサイトからマニフェストをDLしてくる
    /// </summary>
    public IEnumerator DownloadManifest(string _version)
    {
        string url = getManifestURL(_version);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.UserAgent = "UnityPlayer/2020.3.8f1 (UnityWebRequest/1.0, libcurl/7.52.0-DEV)";
        request.Headers.Add("X-Unity-Version", "2020.3.8f1");
        var stream = request.GetResponse().GetResponseStream();
        byte[] results = new byte[stream.Length];
        stream.Read(results, 0, results.Length);
        stream.Flush();
        byte[] _manifestData;
        _manifestData = LZ4.LZ4Util.decompress(results); //LZ4を展開する
        File.WriteAllBytes(manifestPath, _manifestData);
        yield break;
        //UnityWebRequest www = UnityWebRequest.Get(url);
        //www.SetRequestHeader("X-Unity-Version", "2020.3.8f1");
        //yield return www.SendWebRequest();
        //if (www.isHttpError || www.isNetworkError)
        //{
        //    Debug.LogError(www.error);
        //    Debug.LogError(www.url);
        //}
        //else
        //{
        //    byte[] results = www.downloadHandler.data;
        //    byte[] _manifestData;
        //    _manifestData = LZ4.LZ4Util.decompress(results); //LZ4を展開する
        //    File.WriteAllBytes(manifestPath, _manifestData);
        //}
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void init()
    {
        isError = false;
        if (datalists != null)
        {
            datalists.Clear();
            datalists = null;
        }
        if (namedictionary != null)
        {
            namedictionary.Clear();
            namedictionary = null;
        }
        if (hashdictionary != null)
        {
            hashdictionary.Clear();
            hashdictionary = null;
        }
    }
    public IEnumerator LoadManifestDB()
    {
        yield return StartCoroutine(AsyncLoadManifest(LoadTask));
    }

    private IEnumerator AsyncLoadManifest(Action exec)
    {
        exec();

        while (true)
        {
            if(datalists != null && datalists.Count > 0)
            {
                break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// ManifestDBファイルを読み込みメソッド
    /// 時間が掛かるためAsyncで実行してください
    /// </summary>
    /// <returns></returns>
    private void LoadTask()
    {
        //マニフェストDB
        DBProxy manifestDB = null;

        init();

        if (!isLocalSave)
        {
            isError = true;
            print("マニフェストファイルが存在しません");
            return;
        }
        manifestDB = new DBProxy();
        manifestDB.Open(manifestPath);

        var _datalists = new List<URLData>(70000);
        var _namedictionary = new Dictionary<string, URLData>(70000);
        var _hashdictionary = new Dictionary<string, URLData>(70000);

        using (Query query = manifestDB.Query("SELECT name,hash FROM manifests"))
        {
            while (query.Step())
            {
                URLData s = new URLData();
                s._name = query.GetText(0);
                s.hash = query.GetText(1);
                _datalists.Add(s);

                if (!_namedictionary.ContainsKey(s.name)) _namedictionary.Add(s.name, s);
                if (!_hashdictionary.ContainsKey(s.hash)) _hashdictionary.Add(s.hash, s);
            }
        }

        datalists = _datalists;
        namedictionary = _namedictionary;
        hashdictionary = _hashdictionary;


        manifestDB.CloseDB();
        manifestDB = null;
    }

    public void DeleteManifest()
    {
        if (isLocalSave)
        {
            File.Delete(manifestPath);
        }
    }
}

