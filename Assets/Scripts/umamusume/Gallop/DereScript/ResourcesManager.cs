using Cute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    //�ǂݍ��݂̏I����҂�
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
    /// HashSet���o�R���邱�Ƃŏd�������A�Z�b�g���폜
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
    /// ����ŃA�Z�b�g��ǂݍ���
    /// </summary>
    /// <param name="assetList"></param>
    /// <param name="numParallelTasks">����������</param>
    /// <param name="exec">�A�N�V����</param>
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
                yield return 0;
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
                yield return 0;
            }
            yield break;
        }
    }

    /// <summary>
    /// �A�Z�b�g���X�g��ǂݍ���
    /// </summary>
    public IEnumerator LoadAssetGroup(List<string> assetList)
    {
        yield return LoadAssetGroupAsync(assetList, null);

        yield return WaitLoadResource();
    }

    /// <summary>
    /// �A�Z�b�g���X�g��񓯊��œǂݍ���
    /// </summary>
    /// <param name="assetList"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator LoadAssetGroupAsync(List<string> assetList, Action callback)
    {
        while (loadSemaphore)
        {
            yield return null;
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
            //�񓯊��Łc
            //StartCoroutine(ParallelMusicListExec(soundAssetList));
        }
        if (otherAssetList != null)
        {
            parallelCorutines.Add(StartCoroutine(ParallelAssetListExec(otherAssetList)));
        }

        yield return new WaitForFixedUpdate();
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
    /// Object���擾����
    /// Object���i�[���ꂽAssetBundle�͂��炩����LoadAsset���Ă����K�v������
    /// </summary>
    public UnityEngine.Object LoadObject(string objectName)
    {
        return LoadObject<UnityEngine.Object>(objectName);
    }

    /// <summary>
    /// Object�����̌^�Ŏ擾����
    /// Object���i�[���ꂽAssetBundle�͂��炩����LoadAsset���Ă����K�v������
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
    /// �I�u�W�F�N�g���ǂݍ��݉\�����m�F����
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
    /// Asset�t�@�C�������[�h���Ď����֓o�^����
    /// �����t�@�C���ǂݍ��ޏꍇ��LoadAssetGroupAsync���g�p����
    /// </summary>
    public IEnumerator LoadAsset(string assetName, Action callback)
    {
        if (!bundleDictionary.ContainsKey(assetName))
        {
            //�}�j�t�F�X�g�ɂȂ��Ȃ�I��
            if (!AssetManager.instance.CheckExistFileInManifest(assetName))
            {
                if (callback != null)
                {
                    callback();
                }
                yield break;
            }

            float timeOut = 0f;

            //Local�̃t�@�C���m�F
            while (!AssetManager.instance.CheckFileFromFilename(assetName))
            {
                //�^�C���A�E�g10�b
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

            //�񓯊��œǂݍ���
            AssetBundleCreateRequest req = AssetManager.instance.LoadAssetFromNameAsync(assetName);
            if (req == null)
            {
                if (callback != null)
                {
                    callback();
                }
                yield break;
            }

            //�҂�
            yield return req;

            AssetBundle bundle = req.assetBundle;

            List<AssetObject> registlist = new List<AssetObject>();
            string[] pathlist;
            pathlist = bundle.GetAllAssetNames();

            foreach (string AssetNames in pathlist)
            {
                UnityEngine.Object obj = bundle.LoadAsset(AssetNames);
                if (obj != null)
                {
                    registlist.Add(new AssetObject(AssetNames, obj));

                    //shader���X�g�ɒǉ�
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
    /// ���y�����[�h����
    /// </summary>
    private IEnumerator CacheMusic(string musicName, Action callback)
    {
        if (!audioclipDictionary.ContainsKey(musicName))
        {
            //�}�j�t�F�X�g�ɂȂ��Ȃ�I��
            if (!AssetManager.instance.CheckExistFileInManifest(musicName))
            {
                if (callback != null)
                {
                    callback();
                }
                yield break;
            }

            float timeOut = 0f;

            //Local�̃t�@�C���m�F
            while (!AssetManager.instance.CheckFileFromFilename(musicName))
            {
                //�^�C���A�E�g20�b
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

            //�L���b�V���t�@�C���쐬����уp�X�̎擾
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
                // ���[�h�����܂őҋ@
                yield return www;

                AudioClip audioclip;
                audioclip = www.GetAudioClip();

                // ���[�h�J�n�܂őҋ@
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