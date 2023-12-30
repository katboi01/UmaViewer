using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class UmaViewerMain : MonoBehaviour
{
    public static UmaViewerMain Instance;
    private UmaViewerUI UI => UmaViewerUI.Instance;
    private UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    public List<CharaEntry> Characters = new List<CharaEntry>();
    public List<CharaEntry> MobCharacters = new List<CharaEntry>();
    public List<LiveEntry> Lives = new List<LiveEntry>();
    public List<CostumeEntry> Costumes = new List<CostumeEntry>();
    public Dictionary<string, UmaDatabaseEntry> AbList = new Dictionary<string, UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbMotions = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbSounds = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbChara = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbEffect = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> CostumeList = new List<UmaDatabaseEntry>();

    private void Awake()
    {
        Instance = this;
        new Config();
        Application.targetFrameRate = 120;

        AbList = UmaDatabaseController.Instance.MetaEntries;
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
        Dictionary<int, string> enNames = new Dictionary<int, string>();
        Dictionary<int, string> mobNames = new Dictionary<int, string>();
        var loadingUI = UmaSceneController.instance;

        //Main chara names (En only)
        if (Config.Instance.Language == Language.En)
        {
            loadingUI.LoadingProgressChange(0, 11, "Downloading Character Data");
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
        loadingUI.LoadingProgressChange(1, 11, "Downloading Mob Data");
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


        var UmaCharaData = UmaDatabaseController.Instance.CharaData;
        loadingUI.LoadingProgressChange(2, 11, "Loading Character Data");
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

        var MobCharaData = UmaDatabaseController.Instance.MobCharaData;
        loadingUI.LoadingProgressChange(3, 11, "Loading Mob Data");
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
                Name = string.IsNullOrEmpty(name) ? $"Mob_{id}" : name,
                Icon = UmaViewerBuilder.Instance.LoadMobCharaIcon(id.ToString()),
                Id = id,
                IsMob = true
            });
        }

        loadingUI.LoadingProgressChange(4, 11, "Loading Costumes Data");
        yield return null;
        foreach (var item in CostumeList)
        {
            var costume = new CostumeEntry();
            var name = Path.GetFileName(item.Name);
            costume.Id = name.Replace("dress_", "");
            costume.Icon = Builder.LoadSprite(item);
            Costumes.Add(costume);
        }

        var DressData = UmaDatabaseController.Instance.DressData;
        foreach (var data in DressData)
        {
            var costume = Costumes.FirstOrDefault(a => a.Id.Split('_')[0].Contains(data["id"].ToString()));
            if (costume != null)
            {
                costume.CharaId = Convert.ToInt32(data["chara_id"]);
                costume.DressName = data["dressname"].ToString();
                costume.BodyType = Convert.ToInt32(data["body_type"]);
                costume.BodyTypeSub = Convert.ToInt32(data["body_type_sub"]);
            }
        }

        loadingUI.LoadingProgressChange(5, 11, "Loading Live Data");
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

        loadingUI.LoadingProgressChange(6, 11, "Loading UI");
        yield return null;
        UI.LoadModelPanels();
        loadingUI.LoadingProgressChange(7, 11, "Loading UI");
        yield return null;
        UI.LoadMiniModelPanels();
        loadingUI.LoadingProgressChange(8, 11, "Loading UI");
        yield return null;
        UI.LoadPropPanel();
        loadingUI.LoadingProgressChange(9, 11, "Loading UI");
        yield return null;
        UI.LoadMapPanel();
        loadingUI.LoadingProgressChange(10, 11, "Loading UI");
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
