using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.Collections;

public class UmaViewerMain : MonoBehaviour
{
    public static UmaViewerMain Instance;
    private UmaViewerUI UI => UmaViewerUI.Instance;
    private UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

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
        new Config();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;

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

    private IEnumerator Start()
    {
        if (AbList == null) yield break;
        int loadingStep = 0;
        int loadingStepsTotal = 11;
        var UmaCharaData = UmaDatabaseController.Instance.CharaData;
        var MobCharaData = UmaDatabaseController.Instance.MobCharaData;
        var loadingUI = UmaSceneController.instance;
        Dictionary<int, string> enNames = new Dictionary<int, string>();
        Dictionary<int, string> mobNames = new Dictionary<int, string>();

        //Main chara names (En only)
        if (Config.Instance.Language == Language.En)
        {
            loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Downloading Character Data");
            yield return UmaViewerDownload.DownloadText("https://www.tracenacademy.com/api/BasicCharaDataInfo", txt =>
            {
                if (string.IsNullOrEmpty(txt)) return;
                foreach (var item in JArray.Parse(txt))
                {
                    if (!enNames.ContainsKey((int)item["charaId"]))
                    {
                        enNames.Add((int)item["charaId"], item["charaNameEnglish"].ToString());
                    }
                }
            });
        }

        //Mob names (EN & JP)
        loadingUI.LoadingProgressChange(loadingStep++, loadingStepsTotal, "Downloading Mob Data");
        yield return UmaViewerDownload.DownloadText("https://www.tracenacademy.com/api/BasicMobDataInfo", txt =>
        {
            if (string.IsNullOrEmpty(txt)) return;
            foreach (var item in JArray.Parse(txt))
            {
                if (!mobNames.ContainsKey((int)item["mobId"]))
                {
                    mobNames.Add((int)item["mobId"], Config.Instance.Language == Language.Jp ? item["mobName"].ToString() : item["mobNameEnglish"].ToString());
                }
            }
        });

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
                    EnName = enNames.ContainsKey(id) ? enNames[id] : "",
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
            var name = mobNames.ContainsKey(id) ? mobNames[id] : "";
            MobCharacters.Add(new CharaEntry()
            {
                Name = string.IsNullOrEmpty(name)? $"Mob_{id}" : name,
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
        var shaders = UmaAssetManager.LoadAssetBundle(AbList["shader"], true);
        Builder.ShaderList = new List<Shader>(shaders.LoadAllAssets<Shader>()); 
    }

    public void ChangeAA(int val)
    {
        
    }


    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

}
