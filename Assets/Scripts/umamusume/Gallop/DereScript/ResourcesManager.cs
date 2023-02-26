using Cute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    private static ResourcesManager _instance;
    public static ResourcesManager instance
    {
        get
        {
            return _instance;
        }
    }

    private bool loadSemaphore;

    private Dictionary<string, AssetBundle> bundleDictionary = new Dictionary<string, AssetBundle>();

    private Dictionary<string, AssetBundleObject> objectDictionary = new Dictionary<string, AssetBundleObject>();

    private Dictionary<string, AudioClip> audioclipDictionary = new Dictionary<string, AudioClip>();

    private Dictionary<string, Shader> shaderDictionary = new Dictionary<string, Shader>();

    //読み込みの終了を待つ
    List<Coroutine> parallelCorutines = new List<Coroutine>(2);

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
        initResource();
        print("Destory Resource");

        _instance = null;
    }

    private void Start()
    {
    }

    public void Destroy()
    {
        UnityEngine.Object.Destroy(base.gameObject);
    }

    public void initResource()
    {
        foreach (var tmp in bundleDictionary)
        {
            AssetBundle bundle = tmp.Value;
            bundle.Unload(true);
        }
        bundleDictionary.Clear();
        objectDictionary.Clear();
        shaderDictionary.Clear();
        audioclipDictionary.Clear();
    }

    /// <summary>
    /// HashSetを経由することで重複したアセットを削除
    /// </summary>
    private List<string> FilterAssetList(List<string> assetList)
    {
        if (assetList.Count < 2)
        {
            return assetList;
        }
        HashSet<string> collection = new HashSet<string>(assetList);
        List<string> list = new List<string>(collection);
        if (list.Count != assetList.Count)
        {
        }
        return list;
    }

    /// <summary>
    /// 並列でアセットを読み込み
    /// </summary>
    /// <param name="assetList"></param>
    /// <param name="numParallelTasks">同時処理数</param>
    /// <param name="exec">アクション</param>
    private IEnumerator ParallelAssetListExec(List<string> assetList)
    {
        assetList = FilterAssetList(assetList);
        int totalJobs = assetList.Count;
        int finishedJobs = 0;
        while (true)
        {
            int currentJobs = assetList.Count;
            for (int i = 0; i < currentJobs; i++)
            {
                StartCoroutine(LoadAsset(assetList[i], delegate { finishedJobs++; }));
            }
            while (finishedJobs < totalJobs)
            {
                yield return (object)0;
            }
            yield break;
        }
    }

    private IEnumerator ParallelMusicListExec(List<string> assetList)
    {
        assetList = FilterAssetList(assetList);
        int totalJobs = assetList.Count;
        int finishedJobs = 0;
        while (true)
        {
            int currentJobs = assetList.Count;
            for (int i = 0; i < currentJobs; i++)
            {
                StartCoroutine(CacheMusic(assetList[i], delegate { finishedJobs++; }));
            }
            while (finishedJobs < totalJobs)
            {
                yield return (object)0;
            }
            yield break;
        }
    }

    /// <summary>
    /// アセットリストを読み込む
    /// </summary>
    public IEnumerator LoadAssetGroup(List<string> assetList)
    {
        yield return LoadAssetGroupAsync(assetList, null);

        yield return WaitLoadResource();
    }

    /// <summary>
    /// アセットリストを非同期で読み込み
    /// </summary>
    /// <param name="assetList"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator LoadAssetGroupAsync(List<string> assetList, Action callback)
    {
        while (loadSemaphore)
        {
            yield return (object)null;
        }
        SetLoadProcessLock(true);
        List<string> soundAssetList = null;
        List<string> otherAssetList = null;

        for (int i = 0; i < assetList.Count; i++)
        {
            string item = assetList[i];
            if (item.EndsWith(".acb"))
            {
                if (soundAssetList == null)
                {
                    soundAssetList = new List<string>();
                }
                soundAssetList.Add(item);
            }
            else
            {
                if (otherAssetList == null)
                {
                    otherAssetList = new List<string>();
                }
                otherAssetList.Add(item);
            }
        }

        if (soundAssetList != null)
        {
            //parallelCorutines.Add(StartCoroutine(ParallelMusicListExec(soundAssetList)));
            //非同期で…
            //StartCoroutine(ParallelMusicListExec(soundAssetList));
        }
        if (otherAssetList != null)
        {
            parallelCorutines.Add(StartCoroutine(ParallelAssetListExec(otherAssetList)));
        }

        yield return (object)new WaitForFixedUpdate();
        SetLoadProcessLock(false);
        if (callback != null)
        {
            callback();
        }
    }

    public IEnumerator WaitLoadResource()
    {
        for (int i = 0; i < parallelCorutines.Count; i++)
        {
            yield return parallelCorutines[i];
        }
        parallelCorutines.Clear();
    }

    public bool ExistsAssetBundleManifest(string assetName)
    {
        if (AssetManager.instance.CheckExistFileInManifest(assetName))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Objectを取得する
    /// Objectが格納されたAssetBundleはあらかじめLoadAssetしておく必要がある
    /// </summary>
    public UnityEngine.Object LoadObject(string objectName)
    {
        return LoadObject<UnityEngine.Object>(objectName);
    }

    /// <summary>
    /// Objectを特定の型で取得する
    /// Objectが格納されたAssetBundleはあらかじめLoadAssetしておく必要がある
    /// </summary>
    public T LoadObject<T>(string objectName) where T : UnityEngine.Object
    {
        object obj = LoadObject(objectName, typeof(T));
        if (obj != null)
        {
            return (T)obj;
        }
        return null;
    }

    private object LoadObject(string objectName, Type type)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return null;
        }
        string searchKey = objectName.ToLower();
        foreach (KeyValuePair<string, AssetBundleObject> item in objectDictionary)
        {
            AssetBundleObject value = item.Value;
            for (int i = 0; i < value.objectArray.Count; i++)
            {
                if (value.objectArray[i].basePath.Contains(searchKey) && value.objectArray[i].baseObject != null && (type == typeof(UnityEngine.Object) || value.objectArray[i].baseObject.GetType() == type))
                {
                    return value.objectArray[i].baseObject;
                }
            }
        }
        Debug.Log("Object Missing!: " + objectName);
        return null;
    }

    /// <summary>
    /// オブジェクトが読み込み可能かを確認する
    /// </summary>
    public bool CheckLoadObject(string objectName, Type type = null)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return false;
        }
        string searchKey = objectName.ToLower();
        foreach (KeyValuePair<string, AssetBundleObject> item in objectDictionary)
        {
            AssetBundleObject value = item.Value;
            for (int i = 0; i < value.objectArray.Count; i++)
            {
                if (value.objectArray[i].basePath.Contains(searchKey) && value.objectArray[i].baseObject != null)
                {
                    if (type != null)
                    {
                        if (value.objectArray[i].baseObject.GetType() == type)
                        {
                            return true;
                        }
                    }
                    return true;
                }
            }
        }
        return false;
    }

    private void SetLoadProcessLock(bool enable_lock)
    {
        loadSemaphore = enable_lock;
    }

    public IEnumerator GameInitialize()
    {
        while (!AssetManager.instance.isManifestLoad) yield return null;
        while (!MasterDBManager.instance.isLoadDB) yield return null;
    }

    public Shader GetShader(string shadername)
    {
        Shader shader = null;
        if (shaderDictionary.TryGetValue(shadername.ToLower(), out shader))
        {
            return shader;
        }
        print("ShaderNotfound:" + shadername);
        return null;
    }

    public bool CheckAudioClip(string audioname)
    {
        return audioclipDictionary.ContainsKey(audioname.ToLower());
    }

    public AudioClip GetAudioClip(string audioname)
    {
        AudioClip audioClip = null;
        if (audioclipDictionary.TryGetValue(audioname.ToLower(), out audioClip))
        {
            return audioClip;
        }
        print("AudioNotfound:" + audioname);
        return null;
    }

    /// <summary>
    /// Assetファイルをロードして辞書へ登録する
    /// 複数ファイル読み込む場合はLoadAssetGroupAsyncを使用する
    /// </summary>
    public IEnumerator LoadAsset(string assetName, Action callback)
    {
        if (!bundleDictionary.ContainsKey(assetName))
        {
            //マニフェストにないなら終了
            if (!AssetManager.instance.CheckExistFileInManifest(assetName))
            {
                if (callback != null)
                {
                    callback();
                }
                yield break;
            }

            float timeOut = 0f;

            //Localのファイル確認
            while (!AssetManager.instance.CheckFileFromFilename(assetName))
            {
                //タイムアウト10秒
                timeOut += Time.deltaTime;
                if (timeOut > 10f)
                {
                    if (callback != null)
                    {
                        callback();
                    }
                    yield break;
                }
                yield return null;
            }

            //非同期で読み込み
            AssetBundleCreateRequest req = AssetManager.instance.LoadAssetFromNameAsync(assetName);
            if (req == null)
            {
                if (callback != null)
                {
                    callback();
                }
                yield break;
            }

            //待ち
            yield return req;

            AssetBundle bundle = req.assetBundle;

            List<AssetObject> registlist = new List<AssetObject>();
            string[] pathlist;
            pathlist = bundle.GetAllAssetNames();

            foreach (string AssetNames in pathlist)
            {
                UnityEngine.Object obj = bundle.LoadAsset(AssetNames);
                if(obj != null)
                {
                    registlist.Add(new AssetObject(AssetNames, obj));

                    //shaderリストに追加
                    if (obj is Shader)
                    {
                        if (obj.name != "")
                        {
                            string str = Path.GetFileNameWithoutExtension(obj.name.ToLower());
                            if (!shaderDictionary.ContainsKey(str))
                            {
                                shaderDictionary.Add(str, obj as Shader);
                            }
                        }
                    }
                }
            }

            SetAssetDic(assetName, bundle, registlist);
            bundleDictionary.Add(assetName, bundle);
        }

        if (callback != null)
        {
            callback();
        }
    }


    /// <summary>
    /// 音楽をロードする
    /// </summary>
    private IEnumerator CacheMusic(string musicName, Action callback)
    {
        if (!audioclipDictionary.ContainsKey(musicName))
        {
            //マニフェストにないなら終了
            if (!AssetManager.instance.CheckExistFileInManifest(musicName))
            {
                if (callback != null)
                {
                    callback();
                }
                yield break;
            }

            float timeOut = 0f;

            //Localのファイル確認
            while (!AssetManager.instance.CheckFileFromFilename(musicName))
            {
                //タイムアウト20秒
                timeOut += Time.deltaTime;
                if (timeOut > 20f)
                {
                    if (callback != null)
                    {
                        callback();
                    }
                    yield break;
                }
                yield return null;
            }

            //キャッシュファイル作成およびパスの取得
            string musicfile = PathHandler.instance.GetCachePath(musicName);
            while (!File.Exists(musicfile))
            {
                yield return null;
            }

            musicfile = @"file:///" + musicfile;
            //print(musicfile);

            yield return AssetManager.instance.WaitACBDecodeProgress();

            using (WWW www = new WWW(musicfile))
            {
                // ロード完了まで待機
                yield return www;

                AudioClip audioclip;
                audioclip = www.GetAudioClip();

                // ロード開始まで待機
                while (audioclip.loadState == AudioDataLoadState.Unloaded)
                {
                    yield return null;
                }

                audioclipDictionary.Add(musicName, audioclip);
            }
        }

        if (callback != null)
        {
            callback();
        }
    }


    private void SetAssetDic(string filename, AssetBundle assetbundle, List<AssetObject> objectList)
    {
        AssetBundleObject value;
        if (objectDictionary.TryGetValue(filename, out value))
        {
            for (int i = 0; i < value.objectArray.Count; i++)
            {
                value.objectArray[i].DestroyImmediate();
            }
            value.objectArray.Clear();
            value.Unload(true);
            value.SetAssetBundle(assetbundle);
            value.objectArray = objectList;
        }
        else
        {
            value = new AssetBundleObject();
            value.SetAssetBundle(assetbundle);
            value.objectArray = objectList;

            objectDictionary.Add(filename, value);
        }
    }
}