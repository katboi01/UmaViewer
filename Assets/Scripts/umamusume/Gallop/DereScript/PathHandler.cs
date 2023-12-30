using System.IO;
using UnityEngine;

/// <summary>
/// ローカルの保存フォルダ管理
/// </summary>
public class PathHandler : MonoBehaviour
{
    private static PathHandler _instance;
    public static PathHandler instance
    {
        get
        {
            return _instance;
        }
    }

    //private const string localpath = @"DLAsset\";
    private const string _cachepath = @"Cache";
    private const string tmppath = @"Temp";

    private const string hashpath = @"hash";
    private const string assetpath = @"asset";
    private const string soundpath = @"sound";
    private const string sspath = "ScreenShot";

    private const string manifestpath = @"manifestDB";

    public string cacheroot { get { return Path.Combine(cachePath, _cachepath); } }
    public string soundcacheroot { get { return Path.Combine(cachePath, soundpath); } }
    public string manifestroot { get { return Path.Combine(cachePath, manifestpath); } }
    public string hpath { get { return Path.Combine(cachePath, hashpath); } }
    //public string localroot { get { return curPath + localpath; } }
    //public string apath { get { return localroot + assetpath; } }
    //public string spath { get { return localroot + soundpath; } }
    public string localtmp { get { return Path.Combine(curPath, tmppath); } }
    public string screenshotPath { get { return Path.Combine(curPath, sspath); } }

    private string curPath
    {
        get
        {
            return Directory.GetCurrentDirectory();
        }
    }
    private string cachePath
    {
        get
        {
            string ret = "";
            //セーブの確認
            ret = SaveManager.GetString("CachePath");
            if (ret == "")
            {
                //セーブにディレクトリが書かれていない
                ret = Application.persistentDataPath;
                //Unityデフォルトパスを指定してセーブ
                SaveManager.SetString("CachePath", ret);
                SaveManager.Save();
            }
            else
            {
                if (!Directory.Exists(ret))
                {
                    //セーブのディレクトリが存在しない
                    //生成を実施
                    Directory.CreateDirectory(ret);

                    //だめですね
                    if (!Directory.Exists(ret))
                    {
                        ret = Application.persistentDataPath;
                        SaveManager.SetString("CachePath", ret);
                        SaveManager.Save();
                    }
                }
            }

            return ret;
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
            print("ファイルの保存フォルダ: " + cachePath + "");
            CreateDir();
            _instance = this;
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void CreateDir()
    {
        /*
        if (!Directory.Exists(localroot))
        {
            Directory.CreateDirectory(localroot);
        }
        if (!Directory.Exists(apath))
        {
            Directory.CreateDirectory(apath);
        }
        if (!Directory.Exists(spath))
        {
            Directory.CreateDirectory(spath);
        }
        if (!Directory.Exists(localtmp))
        {
            Directory.CreateDirectory(localtmp);
        }
        */
        if (!Directory.Exists(hpath))
        {
            Directory.CreateDirectory(hpath);
        }
        if (!Directory.Exists(manifestroot))
        {
            Directory.CreateDirectory(manifestroot);
        }
        if (!Directory.Exists(cacheroot))
        {
            Directory.CreateDirectory(cacheroot);
        }
        if (!Directory.Exists(soundcacheroot))
        {
            Directory.CreateDirectory(soundcacheroot);
        }
        if (!Directory.Exists(screenshotPath))
        {
            Directory.CreateDirectory(screenshotPath);
        }

    }

    /*
    /// <summary>
    /// ファイル名から、保存するファイルパスを取得
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public string GetLocalPath(string filename)
    {
        string ret = "";
        switch (Path.GetExtension(filename))
        {
            case ".lz4":
                ret = apath + Path.GetFileName(filename);
                break;
            case ".acb":
                ret = spath + Path.GetFileName(filename);
                break;
            default: //lz4なしで呼んだ時(ローカルのファイルはacbを除きlz4圧縮されているため)
                ret = apath + Path.GetFileName(filename) + ".lz4";
                break;
        }
        return ret;
    }
    */

    public string GetTempPath(string filename)
    {
        string ret = "";
        switch (Path.GetExtension(filename))
        {
            case ".lz4":
                ret = Path.Combine(localtmp, Path.GetFileName(filename));
                break;
            case ".acb":
                ret = Path.Combine(localtmp, Path.GetFileName(filename));
                break;
            default: //lz4なしで呼んだ時(ローカルのファイルはacbを除きlz4圧縮されているため)
                ret = Path.Combine(localtmp, Path.GetFileName(filename) + ".lz4");
                break;
        }
        return ret;
    }

    /// <summary>
    /// ファイル名から、キャッシュに置かれたファイルを取得
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public string GetCachePath(string filename)
    {
        string ret = "";
        switch (Path.GetExtension(filename))
        {
            case ".acb":
                string fname = Path.GetFileNameWithoutExtension(filename);
                ret = Path.Combine(soundcacheroot, fname + ".wav");
                break;
            case ".lz4":
            default:
                ret = Path.Combine(cacheroot, Path.GetFileName(filename));
                break;
        }
        return ret;
    }
}

