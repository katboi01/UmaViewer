using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class UmaAssetManager : MonoBehaviour
{
    public static UmaAssetManager instance;

    private Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();
    private Dictionary<string, AssetBundle> NeverUnload = new Dictionary<string, AssetBundle>();

    public static Shader HairShader,
        FaceShader,
        EyeShader,
        CheekShader,
        EyebrowShader,
        AlphaShader,
        BodyAlphaShader,
        BodyBehindAlphaShader;

    public static event Action<UmaDatabaseEntry> OnLoadedBundleUpdate;
    public static event Action OnLoadedBundleClear;
    public static event Action<int, int, string> OnLoadProgressChange;

    public static Coroutine LoadCoroutine;

    private void Awake()
    {
        if (instance)
        {
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    public static void PreLoadAndRun(List<UmaDatabaseEntry> entries, Action OnDone)
    {
        if (LoadCoroutine != null) return;
        LoadCoroutine = instance.StartCoroutine(instance.PreLoadAsset(entries, OnDone));
    }

    private IEnumerator PreLoadAsset(List<UmaDatabaseEntry> entries, Action OnDone)
    {
        var Main = UmaViewerMain.Instance;
        List<UmaDatabaseEntry> result = new List<UmaDatabaseEntry>();
        foreach (var entry in entries)
        {
            SearchAB(Main, entry, ref result);
        }

        if (Config.Instance.WorkMode == WorkMode.Standalone)
        {
            yield return UmaViewerDownload.DownloadAssets(result.ToList(), UmaSceneController.instance.LoadingProgressChange);
        }

        var percent_num = result.Count / 100;
        for (int i = 0; i < result.Count; i++)
        {   
            var entry = result[i];
            if (!entry.IsAssetBundle)
            {
                var file = entry.FilePath;  // trigger download
                OnLoadProgressChange?.Invoke(i, result.Count, null);
                yield return null;
            }
            else
            {
                var exist = LoadAB(result[i]);
                if (!exist && i % percent_num == 0)
                {
                    OnLoadProgressChange?.Invoke(i, result.Count, null);
                    yield return null;
                }
            }
        }

        OnLoadProgressChange?.Invoke(-1, result.Count, null);
        LoadCoroutine = null;
        OnDone?.Invoke();
    }

    public static AssetBundle LoadAssetBundle(UmaDatabaseEntry entry, bool neverUnload = false, bool isRecursive = true)
    {
        if (Exist(entry)) return Get(entry);
        if (isRecursive)
        {
            var Main = UmaViewerMain.Instance;
            List<UmaDatabaseEntry> result = new List<UmaDatabaseEntry>();
            SearchAB(Main, entry, ref result);
            foreach (var e in result)
            {
                LoadAB(e, neverUnload);
            }
            return Get(entry.Name);
        }

        LoadAB(entry, neverUnload);
        return Get(entry);
    }

    private static bool LoadAB(UmaDatabaseEntry entry, bool neverUnload = false)
    {
        string filePath = entry.FilePath; //downloads the file
        if (Exist(entry.Name))
        {
            return true;
        }
        else if (File.Exists(filePath))
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
            if (!bundle)
            {
                Debug.Log(filePath + " exists and doesn't work");
                UmaViewerUI.Instance?.ShowMessage(filePath + " exists and doesn't work", UIMessageType.Error);
            }
            if (bundle.name == "shader.a")
            {
                neverUnload = true;
                EyeShader       = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertooneyet.shader");
                FaceShader      = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonfacetser.shader");
                HairShader      = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonhairtser.shader");
                AlphaShader     = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoonhairtser.shader");
                CheekShader     = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactermultiplycheek.shader");
                EyebrowShader   = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonmayu.shader");
                BodyAlphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoontser.shader");
                BodyBehindAlphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoonbehindtser.shader");
            }
            AddOrUpdate(entry.Name, bundle, neverUnload);
            if (!neverUnload)
            {
                OnLoadedBundleUpdate?.Invoke(entry);
            }
        }
        else
        {
            Debug.LogError($"{entry.Name} - {filePath} does not exist");
            UmaViewerUI.Instance?.ShowMessage($"{entry.Name} - {filePath} does not exist", UIMessageType.Error);
        }
        return false;
    }

    private static void SearchAB(UmaViewerMain Main, UmaDatabaseEntry entry, ref List<UmaDatabaseEntry> result)
    {
        if (!string.IsNullOrEmpty(entry.Prerequisites))
        {
            foreach (string prerequisite in entry.Prerequisites.Split(';'))
            {
                SearchAB(Main, Main.AbList[prerequisite], ref result);
            }
        }
        result.Add(entry);
    }

    public static AssetBundle Get(string name) => instance.LoadedBundles[name];

    public static AssetBundle Get(UmaDatabaseEntry entry) => Get(entry.Name);

    public static void AddOrUpdate(string name, AssetBundle bundle, bool neverUnload = false)
    {
        if (instance.LoadedBundles.ContainsKey(name))
        {
            instance.LoadedBundles[name] = bundle;
        }
        else
        {
            instance.LoadedBundles.Add(name, bundle);
        }

        if (neverUnload)
        {
            if (instance.NeverUnload.ContainsKey(name))
            {
                instance.NeverUnload[name] = bundle;
            }
            else
            {
                instance.NeverUnload.Add(name, bundle);
            }
        }
    }

    public static bool Exist(string name) => instance.LoadedBundles.ContainsKey(name) && instance.LoadedBundles[name] != null;

    public static bool Exist(UmaDatabaseEntry entry) => Exist(entry.Name);

    public static bool Exist(AssetBundle bundle) => instance.LoadedBundles.ContainsValue(bundle);

    private static void UnloadBundle(AssetBundle bundle, bool unloadAllObjects)
    {
        if (instance.NeverUnload.ContainsValue(bundle)) return;
        var entry = instance.LoadedBundles.FirstOrDefault(b => b.Value == bundle);
        if (entry.Key != null)
        {
            instance.LoadedBundles.Remove(entry.Key);
        }
        bundle.Unload(unloadAllObjects);
    }

    public static void UnloadAllBundle(bool unloadAllObjects = false)
    {
        foreach (var bundle in instance.LoadedBundles)
        {
            if (!instance.NeverUnload.ContainsKey(bundle.Key) && bundle.Value)
            {
                bundle.Value.Unload(unloadAllObjects);
            }
        }

        if (unloadAllObjects)
        {
            var builder = UmaViewerBuilder.Instance;
            if (builder)
            {
                if (builder.CurrentUMAContainer)
                    builder.UnloadUma();
                if (builder.CurrentOtherContainer)
                    Destroy(builder.CurrentOtherContainer.gameObject);
            }
        }

        instance.LoadedBundles.Clear();
        foreach (var bundle in instance.NeverUnload)
        {
            instance.LoadedBundles.Add(bundle.Key, bundle.Value);
        }
        OnLoadedBundleClear?.Invoke();
    }
}

