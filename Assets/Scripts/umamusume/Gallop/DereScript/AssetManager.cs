using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Events;
using System.Threading;
using System.Net;

/// <summary>
/// アセットファイルを管理するクラス
/// </summary>
public class AssetManager : MonoBehaviour
{
    private static AssetManager _instance;

    public static AssetManager instance
    {
        get
        {
            return _instance;
        }
    }

    public GameObject DLProgress = null;

    //DLリスト
    private List<URLData> dlList = new List<URLData>();

    /// <summary>
    /// DL中のファイルリスト
    /// 二重保存防止のため
    /// </summary>
    private List<string> proccessingFilesDic = new List<string>();

    /// <summary>
    /// DL実行中かどうか
    /// </summary>
    private bool process = false;

    /// <summary>
    /// ACBデコード中かどうか
    /// </summary>
    private bool acbdecodeing = false;

    /// <summary>
    /// 現在のDL完了数
    /// </summary>
    private int dlcount = 0;

    /// <summary>
    /// DL要求総数
    /// </summary>
    private int dltotal = 0;

    /// <summary>
    /// 同時ダウンロード制限数
    /// </summary>
    private const int dlproclimit = 5;

    /// <summary>
    /// 現在のDL実行プロセス数
    /// </summary>
    private int dlproc = 0;

    // Use this for initialization
    void Start()
    {
        Init();
    }

    void Update()
    {
        //DL希望
        if (process)
        {
            for (int i = dlproc; i < dlproclimit; i++)
            {
                if (dlList.Count > 0)
                {
                    URLData data = dlList[0];
                    dlList.RemoveAt(0);
                    dlproc++;

                    StartCoroutine(download(data));
                }
                else
                {
                    break;
                }
            }

            //DL処理が終了した
            if (dlList.Count == 0 && dlproc == 0)
            {
                process = false;
                dlcount = 0;
                dltotal = 0;
                //全てDLし終わったため、二重チェックもクリア
                proccessingFilesDic.Clear();
            }
        }
        RewriteProgress();
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
        _instance = null;
    }

    //DLProgressに表示するテキスト
    public string DLProgressText
    {
        get
        {
            string text = "";

            if (process)
            {
                text = "ダウンロード中：" + dlcount + " / " + dltotal;
            }
            else if (acbdecodeing)
            {
                text = "ACBファイルをデコードしています";
            }
            return text;
        }
    }

    public void Init()
    {
        if (dlList != null)
        {
            dlList.Clear();
        }
        if(proccessingFilesDic != null)
        {
            proccessingFilesDic.Clear();
        }
        process = false;
        acbdecodeing = false;
        dlcount = 0;
        dltotal = 0;
        dlproc = 0;
    }

    /// <summary>
    /// 状態テキストを書き換える
    /// </summary>
    private void RewriteProgress()
    {
        if (DLProgress != null)
        {
            DLProgress.GetComponent<UIDLProgress>().SetText(DLProgressText);
        }
    }

    /// <summary>
    /// マニフェストが読み込まれているか
    /// </summary>
    public bool isManifestLoad
    {
        get
        {
            if (ManifestManager.instance != null)
            {
                return ManifestManager.instance.isLoad;
            }
            else { return false; }
        }
    }

    /// <summary>
    /// ファイルのダウンロード終了待ち
    /// </summary>
    public IEnumerator WaitDownloadProgress()
    {
        while (process)
        {
            yield return null;
        }
    }

    /// <summary>
    /// ファイル名からファイルをDLする
    /// ダウンロードされて、ファイルが生成されるまで待つ
    /// 既にファイルが存在している場合はそのまま終了する
    /// </summary>
    public IEnumerator DownloadFromFilename(string filename)
    {
        StartCoroutine(DownloadFromFilenameAsync(filename));

        while (!CheckFileFromFilename(filename))
        {
            yield return null;

            //DLエラーでファイルが生成されない場合はDL処理の終了で抜ける
            if (!process)
            {
                break;
            }
        }
    }

    /// <summary>
    /// ファイル名からファイルをDLする
    /// ダウンロードされて、ファイルが生成されるまで待つ
    /// </summary>
    public IEnumerator DownloadFromFilenames(List<string> filenames)
    {
        StartCoroutine(DownloadFromFilenamesAsync(filenames));

        foreach(var tmp in filenames)
        {
            while (!CheckFileFromFilename(tmp))
            {
                yield return null;

                //DLエラーでファイルが生成されない場合はDL処理の終了で抜ける
                if (!process)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// ファイル名からファイルをDLする
    /// </summary>
    public IEnumerator DownloadFromFilenameAsync(string filename)
    {
        if (filename == "") yield break;

        List<string> filelist = new List<string>();
        filelist.Add(filename);

        StartCoroutine(DownloadFromFilenamesAsync(filelist));
    }

    /// <summary>
    /// ファイル名からファイルをDLする
    /// 複数ファイルの場合
    /// 非同期での処理のため、progressがfalseになるまで待つ必要がある
    /// </summary>
    public IEnumerator DownloadFromFilenamesAsync(List<string> filenames)
    {
        while (!isManifestLoad)
        {
            if (ManifestManager.instance.isError)
            {
                yield break;
            }
            yield return null;
        }

        List<URLData> datas = new List<URLData>();

        foreach (var tmp in filenames)
        {
            if (tmp == "") { continue; }

            URLData urldata = ManifestManager.instance.GetURLdataFromName(tmp);
            if (urldata != null)
            {
                if (!CheckExistFile(urldata))
                {
                    //DLListに追加
                    datas.Add(urldata);
                }
            }
            else
            {
                print("Manifestに" + tmp + "が存在しません");
            }
        }
        if (datas.Count > 0)
        {
            //コルーチン開始
            StartCoroutine(AddDownloadFileList(datas));
        }
    }

    /// <summary>
    /// 既にローカルに存在するか確認
    /// </summary>
    private bool CheckExistFile(URLData data)
    {
        if (data == null || data.hash == null || data._name == null)
        {
            return false;
        }

        string hashfile = Path.Combine(PathHandler.instance.hpath, data.hash);
        string cachefile = PathHandler.instance.GetCachePath(data.name);

        //ハッシュが存在する＝既にDLされている
        if (File.Exists(hashfile))
        {
            if (File.Exists(cachefile))
            {
                return true;
            }
        }
        else
        {
            //ファイルの更新があった場合
            if (File.Exists(cachefile))
            {
                File.Delete(cachefile);
            }
        }
        return false;
    }

    /// <summary>
    /// マニフェスト内にファイルが存在するかを確認
    /// </summary>
    public bool CheckExistFileInManifest(string filename)
    {
        URLData urldata = ManifestManager.instance.GetURLdataFromName(filename);
        if (urldata != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// DL対象のファイルをDLリストへ追加する
    /// 実際の処理はUpdateにて随時行われる
    /// </summary>
    private IEnumerator AddDownloadFileList(List<URLData> datas)
    {
        //リストがないなら終わり
        if (datas == null || datas.Count == 0)
        {
            yield break;
        }

        //processがTrueの時はほかでファイルをDL中
        if (process)
        {
            //ファイルを追加
            int datascount = 0;
            foreach (var tmp in datas)
            {
                if (!proccessingFilesDic.Contains(tmp.hash))
                {
                    datascount++;
                    dlList.Add(tmp);
                    proccessingFilesDic.Add(tmp.hash);
                }
            }
            dltotal += datascount;
        }
        else
        {
            process = true;
            int datascount = 0;
            foreach (var tmp in datas)
            {
                if (!proccessingFilesDic.Contains(tmp.hash))
                {
                    datascount++;
                    dlList.Add(tmp);
                    proccessingFilesDic.Add(tmp.hash);
                }
            }

            //DLカウント初期化
            dltotal = datascount;
            dlcount = 0;
        }
    }

    /// <summary>
    /// ファイルのダウンロードを行う
    /// 
    /// </summary>
    public IEnumerator download(URLData url)
    {
        //DL前にもう一度チェック
        if (!CheckExistFile(url))
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.url);
            Debug.LogError(url.url);
            request.Method = "GET";
            request.UserAgent = "UnityPlayer/2020.3.8f1 (UnityWebRequest/1.0, libcurl/7.52.0-DEV)";
            request.Headers.Add("X-Unity-Version", "2020.3.8f1");
            var responce = request.GetResponse();
            var stream = responce.GetResponseStream();
            byte[] results = new byte[stream.Length];
            stream.Read(results, 0, results.Length);
            stream.Flush();
            print("ダウンロード: " + url.name);
            try
            {
                CreateCacheFile(url, results);
            }
            catch (Exception e)
            {
                print(e);
            }
            //UnityWebRequest www = UnityWebRequest.Get(url.url);
            //www.timeout = 180;

            //yield return www.SendWebRequest();

            //if (www.isHttpError || www.isNetworkError)
            //{
            //    Debug.Log(www.error);
            //}
            //else
            //{
            //    byte[] results = www.downloadHandler.data;

            //    print("ダウンロード: " + url.name);
            //    try
            //    {
            //        CreateCacheFile(url, results);
            //    }
            //    catch (Exception e)
            //    {
            //        print(e);
            //    }
            //}
        }
        dlproc--;

        //DL処理終わり
        if (dlproc < 0)
        {
            print("dlproc Error");
        }
        dlcount++;
        RewriteProgress();

        yield break;
    }

    /// <summary>
    /// キャッシュフォルダにファイルを読み込み
    /// </summary>
    /// <returns>キャッシュに保存されたファイルのパス</returns>
    public string GetCachefromName(string filename)
    {
        //キャッシュパスを取得
        string cachepath = PathHandler.instance.GetCachePath(filename);
        if (File.Exists(cachepath))
        {
            return cachepath;
        }
        return null;
    }

    /// <summary>
    /// DLしたバイナリデータをキャッシュフォルダへ展開する
    /// </summary>
    private void CreateCacheFile(URLData url, byte[] originaldata)
    {
        string localpath = PathHandler.instance.GetTempPath(url.filename);
        string hashfile = Path.Combine(PathHandler.instance.hpath, url.hash);
        //キャッシュパスを取得
        string cachepath = PathHandler.instance.GetCachePath(url.name);
        if (File.Exists(cachepath))
        {
            File.Delete(cachepath);
        }

        if (Path.GetExtension(localpath) == ".lz4")
        {
            byte[] newbytes = LZ4.LZ4Util.decompress(originaldata); //LZ4を展開する

            //unity3dファイルの時はプラットフォームの書き換えを行う
            if (localpath.Contains(".unity3d"))
            {
                /*
                if(!AssetUtil.RewriteAssetPlatform(ref newbytes, "2018.2.20f1"))
                {
                    //混在しているのでダメなら旧バージョン
                    AssetUtil.RewriteAssetPlatform(ref newbytes, "2017.4.2f2");
                }

                //Texture2dを書き換え可能に修正。思いのほか軽かったので恒常化。ただし使用する機会はない
                AssetUtil.RewriteReadable(ref newbytes);

                //シェーダを書き換え
                byte[] outbytes = AssetStudio.AssetsManager.ReBuildShaderAsset(newbytes, localpath);
                if(outbytes != null)
                {
                    newbytes = outbytes;
                }
                else
                {
                    throw new Exception("Assetの書き換えに失敗しました： " + Path.GetFileNameWithoutExtension(localpath));
                }
                */
            }
            File.WriteAllBytes(cachepath, newbytes);
            File.Create(hashfile);
        }
        else if (Path.GetExtension(localpath) == ".acb")
        {
            acbdecodeing = true;

            //非同期で投げる
            StartCoroutine(ACBDecode(originaldata, cachepath, hashfile));
        }
        else
        {
            File.WriteAllBytes(cachepath, originaldata);
        }
    }

    /// <summary>
    /// ファイルのダウンロード終了待ち
    /// </summary>
    public IEnumerator WaitACBDecodeProgress()
    {
        while (acbdecodeing)
        {
            yield return null;
        }
    }

    /// <summary>
    /// ACBファイルのデコードを行う
    /// 非同期で処理を投げるため、終了の確認はacbdecodeingがfalseになるのを待つ
    /// </summary>
    private IEnumerator ACBDecode(byte[] data, string cachepath, string hashfile)
    {
        //ACBファイルをデコード
        DecodeACB.Decode(data, cachepath);

        while (!File.Exists(cachepath))
        {
            yield return null;
        }

        File.Create(hashfile);
        acbdecodeing = false;
    }

    /// <summary>
    /// アセット名からAssetBundleを読みだす
    /// </summary>
    public AssetBundle LoadAssetFromName(string filename)
    {
        if (filename.Contains(".unity3d"))
        {

            string name = GetCachefromName(filename);
            if (!File.Exists(name))
            {
                Debug.Log("ファイルを読み込めませんでした：" + filename);
                return null;
            }
            else
            {
                return AssetBundle.LoadFromFile(name);
            }
        }
        else
        {
            //print("Unity3dファイルではありません:" + filename);
            return null;
        }
    }

    /// <summary>
    /// アセット名から非同期処理でAssetBundleを読みだす
    /// </summary>
    public AssetBundleCreateRequest LoadAssetFromNameAsync(string filename)
    {
        if (filename.Contains(".unity3d"))
        {

            string name = GetCachefromName(filename);
            if (!File.Exists(name))
            {
                Debug.Log("ファイルを読み込めませんでした：" + filename);
                return null;
            }
            else
            {
                return AssetBundle.LoadFromFileAsync(name);
            }
        }
        else
        {
            //print("Unity3dファイルではありません:" + filename);
            return null;
        }
    }

    /// <summary>
    /// ファイルが存在するかどうかを確認
    /// </summary>
    public bool CheckFileFromFilename(string filename)
    {
        //キャッシュパスを取得
        string cachepath = PathHandler.instance.GetCachePath(filename);
        if (File.Exists(cachepath))
        {
            return true;
        }

        return false;
    }
}
