using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Rendering;
using Newtonsoft.Json.Linq;

public class UmaViewerMain : MonoBehaviour
{
    private const string EnglishNamesCacheFileName = "english_names.cache";
    private const string EnglishNamesCacheMagic = "UmaViewerEnglishNames";
    private const int EnglishNamesCacheVersion = 1;

    public static UmaViewerMain Instance;
    public static bool WasEscapeConsumedThisFrame { get; private set; }
    private UmaViewerUI UI => UmaViewerUI.Instance;
    private UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    public RenderPipelineAsset DefaultRenderPipeline;
    public List<CharaEntry> Characters = new List<CharaEntry>();
    public List<CharaEntry> MobCharacters = new List<CharaEntry>();
    public List<LiveEntry> Lives = new List<LiveEntry>();
    public List<CostumeEntry> Costumes = new List<CostumeEntry>();
    public Dictionary<string,UmaDatabaseEntry> AbList = new Dictionary<string, UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbMotions = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbSounds = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbChara = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbEffect = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> CostumeList = new List<UmaDatabaseEntry>();

    private void Awake()
    {
        Instance = this;
        // Only build a Config if nothing has already supplied one. The constructor resets
        // MainPath to the default game folder, so constructing unconditionally discarded
        // anything set earlier -- notably CliExporter's --data-path/--region, which run at
        // BeforeSceneLoad. LiveViewerUI.Awake already guards the same way.
        if (Config.Instance == null) new Config();
        ApplyFrameRateLimit();

        // On WebGL the meta/master DBs are faulted into MEMFS in Emscripten preRun
        // (UmaDBPreload.jspre) before any script runs, so this open works here too.
        InitDatabase();
    }

    private void InitDatabase()
    {
        AbList = UmaDatabaseController.Instance.MetaEntries;
        if (AbList == null) return;
        var chara_3d = AbList.Where(ab => ab.Value.Type == UmaFileType._3d_cutt).Select(ab => ab.Value).ToList();
        AbChara = chara_3d.FindAll(ab => ab.Name.StartsWith(UmaDatabaseController.CharaPath));
        AbMotions = chara_3d.FindAll(ab => ab.Name.StartsWith(UmaDatabaseController.MotionPath));
        AbEffect = chara_3d.FindAll(ab => ab.Name.StartsWith(UmaDatabaseController.EffectPath));
        AbSounds = AbList.Where(ab => ab.Value.Type == UmaFileType.sound).Select(ab => ab.Value).ToList();
        var outgame = AbList.Where(ab => ab.Value.Type == UmaFileType.outgame).Select(ab => ab.Value).ToList();
        CostumeList = outgame.FindAll(e => e.Name.StartsWith(UmaDatabaseController.CostumePath));
    }

    public static void ApplyFrameRateLimit()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Config.Instance.GetTargetFrameRate();
    }

    private void Update()
    {
        WasEscapeConsumedThisFrame = false;
        TryConsumeEscapeForFullScreen();
    }

    public static bool TryConsumeEscapeForFullScreen()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !Screen.fullScreen)
            return false;

        Screen.fullScreenMode = FullScreenMode.Windowed;
        Screen.fullScreen = false;
        WasEscapeConsumedThisFrame = true;
        return true;
    }

    private IEnumerator Start()
    {
        if (AbList == null) yield break;
        int loadingStep = 0;
        int loadingStepsTotal = 10;
        var UmaCharaData = UmaDatabaseController.Instance.CharaData;
        var MobCharaData = UmaDatabaseController.Instance.MobCharaData;
        var loadingUI = UmaSceneController.instance;


        //修改(载入通用服装ColorSet相关)
        var charaDressColor = UmaDatabaseController.Instance.CharaDressColor;



        // Online English names take priority. LocalizeEn remains the offline fallback.
        loadingUI.LoadingProgressChange(
            loadingStep++,
            loadingStepsTotal,
            Config.Instance.Language == Language.En ? "Downloading Translations" : "Loading Translations");
        if (Config.Instance.Language == Language.En)
        {
            string cachePath = Path.Combine(
                Application.persistentDataPath,
                EnglishNamesCacheFileName);
            TryLoadEnglishNamesCache(cachePath);

            yield return UmaViewerDownload.DownloadText(LocalizeEn.TranslationUrl, json =>
            {
                if (string.IsNullOrEmpty(json)) return;

                try
                {
                    if (!TryParseEnglishNames(json, out Dictionary<int, string> charaNames, out Dictionary<int, string> mobNames))
                    {
                        Debug.LogWarning("Online English translations did not contain the required name tables. Using LocalizeEn fallback.");
                        return;
                    }

                    ApplyEnglishNames(charaNames, mobNames);
                    TrySaveEnglishNamesCache(cachePath, charaNames, mobNames);
                    Debug.Log($"Loaded online English names: {charaNames.Count} characters, {mobNames.Count} mobs.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Could not parse online English translations. Using LocalizeEn fallback: {ex.Message}");
                }
            });
        }

        if (Config.Instance.WorkMode == WorkMode.Standalone)
        {
            var umaIcons = UmaCharaData.Select(item => AbList.TryGetValue($"chara/chr{item["id"]}/chr_icon_{item["id"]}", out UmaDatabaseEntry entry) ? entry : null).Where(entry => entry != null);
            var mobIcons = MobCharaData.Select(item => AbList.TryGetValue($"mob/mob_chr_icon_{item["mob_id"]}_000001_01", out UmaDatabaseEntry entry) ? entry : null).Where(entry => entry != null);
            var costumeIcons = CostumeList;
            List<UmaDatabaseEntry> filesToDownload = umaIcons.Concat(mobIcons).Concat(costumeIcons).ToList();
            yield return UmaViewerDownload.DownloadAssets(filesToDownload, UmaSceneController.instance.LoadingProgressChange);
            filesToDownload.Clear();
        }

        loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Loading Character Data");
        yield return null;
        foreach (var item in UmaCharaData)
        {
            var id = Convert.ToInt32(item["id"]);
            if (!Characters.Where(c => c.Id == id).Any())
            {
                Characters.Add(new CharaEntry()
                {
                    Name = item["charaname"].ToString(),
                    EnName = LocalizeEn.GetCharaName(id),
                    Icon = UmaViewerBuilder.Instance.LoadCharaIcon(id.ToString()),
                    Id = id,
                    ThemeColor = "#" + item["ui_nameplate_color_1"].ToString()
                });
            }
        }

        loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Loading Mob Data");
        yield return null;
        foreach (var item in MobCharaData)
        {
            if (Convert.ToInt32(item["use_live"]) == 0) 
            {
                continue;
            }

            var id = Convert.ToInt32(item["mob_id"]);
            MobCharacters.Add(new CharaEntry()
            {
                Name = item["charaname"].ToString(),
                EnName = LocalizeEn.GetMobName(id),
                Icon = UmaViewerBuilder.Instance.LoadMobCharaIcon(id.ToString()),
                Id = id,
                IsMob = true
            });
        }

        loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Loading Costumes Data");
        yield return null;
        foreach (var item in CostumeList)
        {
            var costume = new CostumeEntry();
            var name = Path.GetFileName(item.Name);
            costume.Id = name.Replace("dress_","");
            costume.Icon = Builder.LoadSprite(item);
            Costumes.Add(costume);
        }

        var DressData = UmaDatabaseController.Instance.DressData;
        foreach (var data in DressData)
        {
            var costume = Costumes.FirstOrDefault(a => a.Id.Split('_')[0].Contains(data["id"].ToString()) );
            if (costume != null)
            {
                costume.CharaId = Convert.ToInt32(data["chara_id"]);
                costume.DressName = data["dressname"].ToString();
                costume.BodyType = Convert.ToInt32(data["body_type"]);
                costume.BodyTypeSub = Convert.ToInt32(data["body_type_sub"]);
            }
        }

        loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Loading Live Data");
        yield return null;
        var asset = AbList["livesettings"];
        if (asset != null)
        {
            if (WebFileMount.Active) yield return UmaAssetManager.EnsureBundleFiles(asset);
            string filePath = asset.FilePath;
            if (File.Exists(filePath))
            {
                AssetBundle bundle = UmaAssetManager.LoadAssetBundle(asset, true);
                foreach (var item in UmaDatabaseController.Instance.LiveData)
                {
                    var musicId = Convert.ToInt32(item["music_id"]);
                    var songName = item["songname"].ToString();
                    var membercount = Convert.ToInt32(item["live_member_number"]);
                    var defaultdress = Convert.ToInt32(item["default_main_dress"]);

                    if (!Lives.Where(c => c.MusicId == musicId).Any())
                    {
                        if (bundle.Contains(musicId.ToString()))
                        {
                            TextAsset liveData = bundle.LoadAsset<TextAsset>(musicId.ToString());

                            Lives.Add(new LiveEntry(liveData.text)
                            {
                                MusicId = musicId,
                                SongName = songName,
                                MemberCount = membercount,
                                DefaultDress = defaultdress,
                                Icon = UmaViewerBuilder.Instance.LoadLiveIcon(musicId)
                            }); 
                        }
                    }
                }
            }
        }

        loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Loading UI");
        yield return null;
        UI.LoadModelPanels();
        loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Loading UI");
        yield return null;
        UI.LoadMiniModelPanels();
        loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Loading UI");
        yield return null;
        UI.LoadPropPanel();
        loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Loading UI");
        yield return null;
        UI.LoadMapPanel();
        loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Loading UI");
        yield return null;
        UI.LoadLivePanels();
        loadingUI.LoadingProgressChange(-1, -1);

        //Load Shader First
        if (WebFileMount.Active) yield return UmaAssetManager.EnsureBundleFiles(AbList["shader"]);
        var shaders = UmaAssetManager.LoadAssetBundle(AbList["shader"], true);
        if (shaders != null)
        {
            // The game's bundle shaders are DX-only. On WebGL (GLES3) they have no usable variant
            // and are swapped to the in-project ports at merge, so don't LoadAllAssets them: that
            // force-loads every shader in the bundle and logs "not supported on this GPU" once each
            // (~491 errors). The bundle stays loaded, so the handful a costume actually uses still
            // resolve lazily. ShaderList only feeds the generic-costume fixup, which no-ops empty.
            if (Application.platform != RuntimePlatform.WebGLPlayer)
                Builder.ShaderList = new List<Shader>(shaders.LoadAllAssets<Shader>());
            Gallop.ShaderManager.InitManager();
            Gallop.ShaderManager.WarmupDofBloomShader();
        }
    }

    private static bool TryParseEnglishNames(
        string json,
        out Dictionary<int, string> charaNames,
        out Dictionary<int, string> mobNames)
    {
        JObject translations = JObject.Parse(json);
        charaNames = translations[LocalizeEn.CharaTranslationTableKey]
            ?.ToObject<Dictionary<int, string>>();
        mobNames = translations[LocalizeEn.MobTranslationTableKey]
            ?.ToObject<Dictionary<int, string>>();

        return charaNames != null && charaNames.Count > 0 &&
               mobNames != null && mobNames.Count > 0;
    }

    private static void ApplyEnglishNames(
        Dictionary<int, string> charaNames,
        Dictionary<int, string> mobNames)
    {
        foreach (KeyValuePair<int, string> pair in charaNames)
        {
            if (!string.IsNullOrEmpty(pair.Value))
                LocalizeEn.SetCharaName(pair.Key, pair.Value);
        }

        foreach (KeyValuePair<int, string> pair in mobNames)
        {
            if (!string.IsNullOrEmpty(pair.Value))
                LocalizeEn.SetMobName(pair.Key, pair.Value);
        }
    }

    private static void TryLoadEnglishNamesCache(string cachePath)
    {
        if (!File.Exists(cachePath)) return;

        try
        {
            using (var stream = File.OpenRead(cachePath))
            using (var reader = new BinaryReader(stream))
            {
                if (reader.ReadString() != EnglishNamesCacheMagic ||
                    reader.ReadInt32() != EnglishNamesCacheVersion)
                {
                    throw new InvalidDataException("Unsupported English name cache format.");
                }

                Dictionary<int, string> charaNames = ReadEnglishNameTable(reader);
                Dictionary<int, string> mobNames = ReadEnglishNameTable(reader);
                ApplyEnglishNames(charaNames, mobNames);
                Debug.Log($"Loaded cached English names: {charaNames.Count} characters, {mobNames.Count} mobs.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Could not load the cached English names. Using LocalizeEn fallback: {ex.Message}");
        }
    }

    private static void TrySaveEnglishNamesCache(
        string cachePath,
        Dictionary<int, string> charaNames,
        Dictionary<int, string> mobNames)
    {
        try
        {
            using (var stream = File.Create(cachePath))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(EnglishNamesCacheMagic);
                writer.Write(EnglishNamesCacheVersion);
                WriteEnglishNameTable(writer, charaNames);
                WriteEnglishNameTable(writer, mobNames);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Could not save the updated English name cache: {ex.Message}");
        }
    }

    private static Dictionary<int, string> ReadEnglishNameTable(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > 100000)
            throw new InvalidDataException("Invalid English name cache entry count.");

        var names = new Dictionary<int, string>(count);
        for (int i = 0; i < count; i++)
        {
            int id = reader.ReadInt32();
            string value = reader.ReadString();
            if (!string.IsNullOrEmpty(value))
                names[id] = value;
        }

        return names;
    }

    private static void WriteEnglishNameTable(
        BinaryWriter writer,
        Dictionary<int, string> names)
    {
        List<KeyValuePair<int, string>> validNames = names
            .Where(pair => !string.IsNullOrEmpty(pair.Value))
            .ToList();
        writer.Write(validNames.Count);
        foreach (KeyValuePair<int, string> pair in validNames)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value);
        }
    }

    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

}
