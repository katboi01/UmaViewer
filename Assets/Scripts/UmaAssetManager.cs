using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
//管理 AssetBundle 的加载、引用计数、依赖关系和释放
public class UmaAssetManager : MonoBehaviour
{
    private sealed class BundleHandle
    {
        public string Name;
        public UmaDatabaseEntry Entry;
        public AssetBundle Bundle;
        public UmaAssetBundleStream Stream;
        public int RefCount;
        public bool IsLoaded;
        public bool NeverUnload;
    }

    private readonly struct LoadItem
    {
        public readonly UmaDatabaseEntry Entry;
        public readonly bool NeverUnload;

        public LoadItem(UmaDatabaseEntry entry, bool neverUnload)
        {
            Entry = entry;
            NeverUnload = neverUnload;
        }
    }

    public static UmaAssetManager instance;

    private readonly Dictionary<string, AssetBundle> LoadedBundles =
        new Dictionary<string, AssetBundle>(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, AssetBundle> NeverUnload =
        new Dictionary<string, AssetBundle>(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, BundleHandle> Handles =
        new Dictionary<string, BundleHandle>(StringComparer.OrdinalIgnoreCase);

    private const int MaxLoadsPerFrame = 4;

    public static Shader HairShader,
        FaceShader,
        EyeShader,
        CheekShader,
        EyebrowShader,
        AlphaShader,
        BodyAlphaShader,
        BodyBehindAlphaShader;

    public static event Action<UmaDatabaseEntry> OnLoadedBundleUpdate;
    public static event Action<UmaDatabaseEntry> OnLoadedBundleRemove;
    public static event Action OnLoadedBundleClear;
    public static event Action<int, int, string> OnLoadProgressChange;

    public static Coroutine LoadCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()
    {
        ReleaseAllInternal(false, true, false);
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        ReleaseAllInternal(false, true, false);
        instance = null;
    }

    public static void PreLoadAndRun(List<UmaDatabaseEntry> entries, Action onDone)
    {
        if (instance == null)
        {
            Debug.LogError("[UmaAssetManager] instance is null.");
            onDone?.Invoke();
            return;
        }

        if (LoadCoroutine != null)
            return;

        LoadCoroutine = instance.StartCoroutine(instance.PreLoadAsset(entries, onDone));
    }

    private IEnumerator PreLoadAsset(List<UmaDatabaseEntry> entries, Action onDone)
    {
        List<UmaDatabaseEntry> roots = DeduplicateEntries(entries);
        List<UmaDatabaseEntry> downloadEntries = ExpandUniqueEntries(roots);

        if (Config.Instance.WorkMode == WorkMode.Standalone)
        {
            yield return UmaViewerDownload.DownloadAssets(
                downloadEntries,
                UmaSceneController.instance.LoadingProgressChange);
        }

        // 每个根请求保留自己的依赖引用；共享依赖会像官方一样增加多次 refCount。
        List<LoadItem> loadItems = new List<LoadItem>();

        for (int i = 0; i < roots.Count; i++)
        {
            UmaDatabaseEntry root = roots[i];
            List<UmaDatabaseEntry> requests = SearchAB(UmaViewerMain.Instance, root);

            for (int j = 0; j < requests.Count; j++)
            {
                UmaDatabaseEntry request = requests[j];
                if (request != null)
                    loadItems.Add(new LoadItem(request, false));
            }
        }

        int completed = 0;

        for (int i = 0; i < loadItems.Count; i++)
        {
            LoadItem item = loadItems[i];
            if (WebFileMount.Active)
                yield return WebFileMount.Ensure(item.Entry.Path);
            AcquireOne(item.Entry, item.NeverUnload);

            completed++;
            OnLoadProgressChange?.Invoke(completed, loadItems.Count, "Loading");
            // 分帧加载，避免单帧耗时过长
            if (completed % MaxLoadsPerFrame == 0)
                yield return null;
        }

        OnLoadProgressChange?.Invoke(-1, loadItems.Count, null);
        LoadCoroutine = null;
        onDone?.Invoke();
    }

    // WebGL: fault an entry and its transitive dependencies into MEMFS so a following
    // synchronous LoadAssetBundle finds every file on disk. Call from a coroutine.
    public static IEnumerator EnsureBundleFiles(UmaDatabaseEntry entry)
    {
        if (!WebFileMount.Active || entry == null || instance == null) yield break;
        foreach (var request in SearchAB(UmaViewerMain.Instance, entry))
            if (request != null)
                yield return WebFileMount.Ensure(request.Path);
    }

    public static AssetBundle LoadAssetBundle(
        UmaDatabaseEntry entry,
        bool neverUnload = false,
        bool isRecursive = true)
    {
        if (instance == null || entry == null)
            return null;

        if (isRecursive)
        {
            List<UmaDatabaseEntry> requests = SearchAB(UmaViewerMain.Instance, entry);

            for (int i = 0; i < requests.Count; i++)
            {
                UmaDatabaseEntry request = requests[i];
                if (request != null)
                    AcquireOne(request, neverUnload);
            }
        }
        else
        {
            AcquireOne(entry, neverUnload);
        }

        return Get(entry);
    }

    public static void UnloadAssetBundle(UmaDatabaseEntry entry, bool unloadAllObjects)
    {
        if (instance == null || entry == null)
            return;

        List<UmaDatabaseEntry> requests = SearchAB(UmaViewerMain.Instance, entry);

        // 官方 TryUnload 从依赖请求数组尾部开始反向 Dereference。
        for (int i = requests.Count - 1; i >= 0; i--)
        {
            UmaDatabaseEntry request = requests[i];
            if (request != null)
                Dereference(request.Name, unloadAllObjects, false);
        }
    }

    private static bool AcquireOne(UmaDatabaseEntry entry, bool neverUnload)
    {
        if (instance == null || entry == null || string.IsNullOrEmpty(entry.Name))
            return false;

        string key = NormalizeName(entry.Name);
        BundleHandle handle = GetOrCreateHandle(key, entry);

        if (neverUnload)
            handle.NeverUnload = true;

        // 兼容项目中仍然直接调用 AssetBundle.Unload的旧代码
        // Unity销毁 AssetBundle后，托管引用仍然存在，但bundle == null会返回 true 这时必须清掉旧Handle和 Stream,再从文件重新加载
        if (entry.IsAssetBundle && handle.IsLoaded && handle.Bundle == null)
            ResetExternallyUnloadedHandle(handle);

        if (handle.IsLoaded)
        {
            handle.RefCount++;
            SyncLegacyDictionaries(handle);
            return true;
        }

        string filePath = entry.FilePath;

        if (!File.Exists(filePath))
        {
            // WebGL faults files in on the coroutine load paths; a synchronous miss here just
            // means this bundle (e.g. a boot icon) was not pre-mounted. Skip it quietly.
            if (WebFileMount.Active)
                return false;
            Debug.LogError($"{entry.Name} - {filePath} does not exist");
            UmaViewerUI.Instance?.ShowMessage(
                $"{entry.Name} - {filePath} does not exist",
                UIMessageType.Error);
            return false;
        }

        if (!entry.IsAssetBundle)
        {
            handle.Bundle = null;
            handle.Stream = null;
            handle.IsLoaded = true;
            handle.RefCount++;
            SyncLegacyDictionaries(handle);
            OnLoadedBundleUpdate?.Invoke(entry);
            return true;
        }

        AssetBundle bundle = null;
        UmaAssetBundleStream stream = null;

        try
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // WebGL rejects Standalone-target bundles; patch the target byte and load from
                // memory (the decrypt happens up front rather than through a stream).
                byte[] raw;
                if (!entry.IsEncrypted)
                {
                    raw = File.ReadAllBytes(filePath);
                }
                else
                {
                    using var s = new UmaAssetBundleStream(filePath, entry.FKey);
                    raw = new byte[s.Length];
                    int got = 0;
                    while (got < raw.Length)
                    {
                        int n = s.Read(raw, got, raw.Length - got);
                        if (n <= 0) break;
                        got += n;
                    }
                }
                bundle = AssetBundle.LoadFromMemory(WebGLBundlePatch.Patch(raw));
            }
            else if (!entry.IsEncrypted)
            {
                bundle = AssetBundle.LoadFromFile(filePath);
            }
            else
            {
                // 官方 LoadOne：同一个 Handle 已加载时只 ++refCount，不重复打开文件。
                stream = new UmaAssetBundleStream(filePath, entry.FKey);
                bundle = AssetBundle.LoadFromStream(stream);
            }

            if (bundle == null)
            {
                stream?.Dispose();
                Debug.LogError(filePath + " exists and doesn't work");
                UmaViewerUI.Instance?.ShowMessage(
                    filePath + " exists and doesn't work",
                    UIMessageType.Error);
                return false;
            }

            handle.Bundle = bundle;
            handle.Stream = stream;
            handle.IsLoaded = true;
            handle.RefCount++;

            RegisterLoadedBundle(handle);
            return true;
        }
        catch (Exception exception)
        {
            if (bundle != null)
            {
                try
                {
                    bundle.Unload(true);
                }
                catch
                {
                    // 保留原始异常
                }
            }

            stream?.Dispose();
            handle.Bundle = null;
            handle.Stream = null;
            handle.IsLoaded = false;
            handle.RefCount = 0;
            Debug.LogException(exception);
            return false;
        }
    }

    private static BundleHandle GetOrCreateHandle(string key, UmaDatabaseEntry entry)
    {
        if (!instance.Handles.TryGetValue(key, out BundleHandle handle))
        {
            handle = new BundleHandle
            {
                Name = key,
                Entry = entry
            };
            instance.Handles.Add(key, handle);
        }
        else if (handle.Entry == null)
        {
            handle.Entry = entry;
        }

        return handle;
    }

    private static void ResetExternallyUnloadedHandle(BundleHandle handle)
    {
        if (handle == null)
            return;

        // AssetBundle已经被外部代码Unload,先关闭我们仍持有的加密Stream
        if (handle.Stream != null)
        {
            try
            {
                handle.Stream.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        handle.Bundle = null;
        handle.Stream = null;
        handle.IsLoaded = false;
        handle.RefCount = 0;

        if (instance != null)
        {
            instance.LoadedBundles.Remove(handle.Name);
            instance.NeverUnload.Remove(handle.Name);
        }
    }

    private static void RegisterLoadedBundle(BundleHandle handle)
    {
        AssetBundle bundle = handle.Bundle;

        if (bundle != null && bundle.name == "shader.a")
        {
            handle.NeverUnload = true;

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // The DX-only bundle shaders don't load on WebGL (they log "not supported on this
                // GPU"). Bind the in-project ports by name instead, which is also what the merge
                // swap would resolve them to, so nothing renders through the bundle shader.
                EyeShader = Shader.Find("Gallop/3D/Chara/ToonEye/T");
                FaceShader = Shader.Find("Gallop/3D/Chara/ToonFace/TSER");
                HairShader = Shader.Find("Gallop/3D/Chara/ToonHair/TSER");
                AlphaShader = Shader.Find("Gallop/3D/Chara/AlphaNolineToonHair/TSER");
                CheekShader = Shader.Find("Gallop/3D/Chara/MultiplyCheek");
                EyebrowShader = Shader.Find("Gallop/3D/Chara/ToonMayu");
                BodyAlphaShader = Shader.Find("Gallop/3D/Chara/AlphaNolineToon/TSER");
                BodyBehindAlphaShader = Shader.Find("Gallop/3D/Chara/AlphaNolineToonBehind/TSER");
            }
            else
            {
                EyeShader = bundle.LoadAsset<Shader>(
                    "assets/_gallop/resources/shader/3d/character/charactertooneyet.shader");
                FaceShader = bundle.LoadAsset<Shader>(
                    "assets/_gallop/resources/shader/3d/character/charactertoonfacetser.shader");
                HairShader = bundle.LoadAsset<Shader>(
                    "assets/_gallop/resources/shader/3d/character/charactertoonhairtser.shader");
                AlphaShader = bundle.LoadAsset<Shader>(
                    "assets/_gallop/resources/shader/3d/character/characteralphanolinetoonhairtser.shader");
                CheekShader = bundle.LoadAsset<Shader>(
                    "assets/_gallop/resources/shader/3d/character/charactermultiplycheek.shader");
                EyebrowShader = bundle.LoadAsset<Shader>(
                    "assets/_gallop/resources/shader/3d/character/charactertoonmayu.shader");
                BodyAlphaShader = bundle.LoadAsset<Shader>(
                    "assets/_gallop/resources/shader/3d/character/characteralphanolinetoontser.shader");
                BodyBehindAlphaShader = bundle.LoadAsset<Shader>(
                    "assets/_gallop/resources/shader/3d/character/characteralphanolinetoonbehindtser.shader");
            }
        }

        SyncLegacyDictionaries(handle);

        if (!handle.NeverUnload && handle.Entry != null)
            OnLoadedBundleUpdate?.Invoke(handle.Entry);
    }

    private static void SyncLegacyDictionaries(BundleHandle handle)
    {
        instance.LoadedBundles[handle.Name] = handle.Bundle;

        if (handle.NeverUnload)
            instance.NeverUnload[handle.Name] = handle.Bundle;
        else
            instance.NeverUnload.Remove(handle.Name);
    }

    private static void Dereference(string name, bool unloadAllObjects, bool force)
    {
        if (instance == null || string.IsNullOrEmpty(name))
            return;

        string key = NormalizeName(name);

        if (!instance.Handles.TryGetValue(key, out BundleHandle handle))
            return;

        if (!handle.IsLoaded)
            return;

        if (handle.RefCount > 0)
            handle.RefCount--;

        if (force || (handle.RefCount <= 0 && !handle.NeverUnload))
            UnloadHandle(handle, unloadAllObjects);
    }

    private static void UnloadHandle(BundleHandle handle, bool unloadAllObjects)
    {
        AssetBundle bundle = handle.Bundle;
        UmaAssetBundleStream stream = handle.Stream;

        // Unity要求先Unload AssetBundle再释放传给LoadFromStream的Stream
        if (bundle != null)
        {
            try
            {
                bundle.Unload(unloadAllObjects);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        if (stream != null)
        {
            try
            {
                stream.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        handle.Bundle = null;
        handle.Stream = null;
        handle.IsLoaded = false;
        handle.RefCount = 0;

        instance.LoadedBundles.Remove(handle.Name);
        instance.NeverUnload.Remove(handle.Name);

        if (handle.Entry != null)
            OnLoadedBundleRemove?.Invoke(handle.Entry);
    }


    //Editor 脚本编译前调用Unload(false)保留已实例化资源
    //但关闭 AssetBundle 容器和底层文件 Stream
    public static void ReleaseAllForEditorCompilation()
    {
        if (instance == null)
            return;

        if (LoadCoroutine != null)
        {
            try
            {
                instance.StopCoroutine(LoadCoroutine);
            }
            catch
            {
                // Coroutine 可能已经结束。
            }
            LoadCoroutine = null;
        }

        instance.ReleaseAllInternal(false, true, true);

        // 清理由旧实现遗留、等待终结器关闭的 FileStream。
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private void ReleaseAllInternal(
        bool unloadAllObjects,
        bool includeNeverUnload,
        bool notifyClear)
    {
        List<BundleHandle> snapshot = Handles.Values.ToList();

        for (int i = 0; i < snapshot.Count; i++)
        {
            BundleHandle handle = snapshot[i];
            if (!includeNeverUnload && handle.NeverUnload)
                continue;

            UnloadHandle(handle, unloadAllObjects);
        }

        if (includeNeverUnload)
        {
            Handles.Clear();
            LoadedBundles.Clear();
            NeverUnload.Clear();
        }
        else
        {
            List<string> removeKeys = Handles
                .Where(pair => !pair.Value.NeverUnload)
                .Select(pair => pair.Key)
                .ToList();

            for (int i = 0; i < removeKeys.Count; i++)
                Handles.Remove(removeKeys[i]);
        }

        if (notifyClear)
            OnLoadedBundleClear?.Invoke();
    }
    
    /// 返回顺序保持根资源在前、依赖在后；释放时反向遍历。
    public static List<UmaDatabaseEntry> SearchAB(UmaViewerMain main, UmaDatabaseEntry entry)
    {
        if (entry == null)
            return new List<UmaDatabaseEntry>();

        UmaDatabaseEntry dependencyEntry = ResolveLaserCandidateIfMissing(main, entry);

        if (dependencyEntry == null || string.IsNullOrEmpty(dependencyEntry.Prerequisites))
            return new List<UmaDatabaseEntry> { entry };

        if (entry.CachedPrerequisites != null)
        {
            return new List<UmaDatabaseEntry> { entry }
                .Concat(entry.CachedPrerequisites)
                .ToList();
        }

        List<UmaDatabaseEntry> prerequisites = new List<UmaDatabaseEntry>();
        string[] dependencyNames = dependencyEntry.Prerequisites.Split(';');

        for (int i = 0; i < dependencyNames.Length; i++)
        {
            string dependencyName = dependencyNames[i]?.Trim();
            if (string.IsNullOrEmpty(dependencyName))
                continue;

            if (main == null || main.AbList == null ||
                !main.AbList.TryGetValue(dependencyName, out UmaDatabaseEntry dependency) ||
                dependency == null)
            {
                Debug.LogWarning(
                    $"[SearchAB] Missing prerequisite '{dependencyName}' for '{dependencyEntry.Name}'");
                continue;
            }

            prerequisites.AddRange(SearchAB(main, dependency));
        }

        entry.CachedPrerequisites = prerequisites;

        return new List<UmaDatabaseEntry> { entry }
            .Concat(prerequisites)
            .ToList();
    }

    private static UmaDatabaseEntry ResolveLaserCandidateIfMissing(
        UmaViewerMain main,
        UmaDatabaseEntry entry)
    {
        if (entry == null)
            return null;

        if (entry.Name != "3d/effect/live/pfb_eff_live_laser_01")
            return entry;

        if (File.Exists(entry.Path))
            return entry;

        if (main == null || main.AbList == null)
            return entry;

        string[] candidates =
        {
            "3d/effect/live/pfb_eff_live_laser_02",
            "3d/effect/live/pfb_eff_live_laser_03",
            "3d/effect/live/pfb_eff_live_laser_04"
        };

        for (int i = 0; i < candidates.Length; i++)
        {
            string candidateName = candidates[i];

            if (main.AbList.TryGetValue(candidateName, out UmaDatabaseEntry candidate) &&
                candidate != null && File.Exists(candidate.Path))
            {
                Debug.LogWarning(
                    $"[LaserFallback] '{entry.Name}' missing -> use '{candidateName}'");
                return candidate;
            }
        }

        return entry;
    }

    private static List<UmaDatabaseEntry> DeduplicateEntries(IEnumerable<UmaDatabaseEntry> entries)
    {
        if (entries == null)
            return new List<UmaDatabaseEntry>();

        return entries
            .Where(entry => entry != null && !string.IsNullOrEmpty(entry.Name))
            .GroupBy(entry => NormalizeName(entry.Name), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static List<UmaDatabaseEntry> ExpandUniqueEntries(IEnumerable<UmaDatabaseEntry> roots)
    {
        List<UmaDatabaseEntry> expanded = new List<UmaDatabaseEntry>();

        if (roots != null)
        {
            foreach (UmaDatabaseEntry root in roots)
                expanded.AddRange(SearchAB(UmaViewerMain.Instance, root));
        }

        return DeduplicateEntries(expanded);
    }

    private static string NormalizeName(string name)
    {
        return name?.Trim().ToLowerInvariant();
    }

    public static AssetBundle Get(string name)
    {
        if (instance == null || string.IsNullOrEmpty(name))
            return null;

        string key = NormalizeName(name);

        if (!instance.LoadedBundles.TryGetValue(key, out AssetBundle bundle))
            return null;

        if (bundle != null)
            return bundle;

        // 字典里是已经被外部 AssetBundle.Unload 销毁的 Unity Object。
        if (instance.Handles.TryGetValue(key, out BundleHandle staleHandle))
            ResetExternallyUnloadedHandle(staleHandle);
        else
            instance.LoadedBundles.Remove(key);

        return null;
    }

    public static AssetBundle Get(UmaDatabaseEntry entry)
    {
        return entry != null ? Get(entry.Name) : null;
    }

    // 保留给现有脚本使用
    public static void AddOrUpdate(string name, AssetBundle bundle, bool neverUnload = false)
    {
        if (instance == null || string.IsNullOrEmpty(name))
            return;

        string key = NormalizeName(name);
        BundleHandle handle = GetOrCreateHandle(key, null);

        handle.Bundle = bundle;
        handle.IsLoaded = bundle != null;
        handle.NeverUnload |= neverUnload;

        SyncLegacyDictionaries(handle);
    }

    public static bool Exist(string name)
    {
        if (instance == null || string.IsNullOrEmpty(name))
            return false;

        string key = NormalizeName(name);
        return instance.Handles.TryGetValue(key, out BundleHandle handle) &&
               handle.IsLoaded && handle.Bundle != null;
    }

    public static bool Exist(UmaDatabaseEntry entry)
    {
        return entry != null && Exist(entry.Name);
    }

    public static bool Exist(AssetBundle bundle)
    {
        return instance != null && bundle != null &&
               instance.LoadedBundles.ContainsValue(bundle);
    }

    public static void UnloadAllBundle(bool unloadAllObjects = false)
    {
        if (instance == null)
            return;

        instance.ReleaseAllInternal(unloadAllObjects, false, false);

        if (unloadAllObjects)
        {
            UmaViewerBuilder builder = UmaViewerBuilder.Instance;
            if (builder != null)
            {
                if (builder.CurrentUMAContainer != null)
                    builder.UnloadUma();

                if (builder.CurrentOtherContainer != null)
                    Destroy(builder.CurrentOtherContainer.gameObject);
            }
        }

        OnLoadedBundleClear?.Invoke();
    }

    public static Texture2D FindTextureInLoadedBundlesByShortName(string shortName)
    {
        if (instance == null || string.IsNullOrEmpty(shortName))
            return null;

        foreach (KeyValuePair<string, AssetBundle> pair in instance.LoadedBundles)
        {
            AssetBundle bundle = pair.Value;
            if (bundle == null)
                continue;

            string[] assetNames;
            try
            {
                assetNames = bundle.GetAllAssetNames();
            }
            catch
            {
                continue;
            }

            string hit = assetNames.FirstOrDefault(assetName =>
                assetName.EndsWith("/" + shortName, StringComparison.OrdinalIgnoreCase) ||
                assetName.EndsWith(shortName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(hit))
                return bundle.LoadAsset<Texture2D>(hit);
        }

        return null;
    }

    public static IEnumerable<AssetBundle> EnumerateLoadedBundles()
    {
        if (instance == null)
            yield break;

        foreach (AssetBundle bundle in instance.LoadedBundles.Values)
        {
            if (bundle != null)
                yield return bundle;
        }
    }

    public static int GetOpenEncryptedStreamCount()
    {
        if (instance == null)
            return 0;

        return instance.Handles.Values.Count(handle => handle.Stream != null);
    }

    public static int GetLoadedHandleCount()
    {
        if (instance == null)
            return 0;

        return instance.Handles.Values.Count(handle => handle.IsLoaded);
    }
}
