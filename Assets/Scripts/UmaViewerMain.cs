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
    public List<UmaDatabaseEntry> AbList = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbMotions = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbSounds = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbChara = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbEffect = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> CostumeList = new List<UmaDatabaseEntry>();

    [Header("Asset Memory")]
    public bool ShadersLoaded = false;
    public Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();

    private void Awake()
    {
        Instance = this;
        new Config();
        Application.targetFrameRate = 120;
        AbList = UmaDatabaseController.Instance.MetaEntries.ToList();
        AbChara = AbList.Where(ab => ab.Name.StartsWith(UmaDatabaseController.CharaPath)).ToList();
        AbMotions = AbList.Where(ab => ab.Name.StartsWith(UmaDatabaseController.MotionPath)).ToList();
        AbEffect = AbList.Where(ab => ab.Name.StartsWith(UmaDatabaseController.EffectPath)).ToList();
        AbSounds = AbList.Where(ab => ab.Name.EndsWith(".awb") || ab.Name.EndsWith(".acb")).ToList();
        CostumeList = AbList.Where(ab => ab.Name.StartsWith(UmaDatabaseController.CostumePath)).ToList();
    }

    private IEnumerator Start()
    {
        Dictionary<int, string> enNames = new Dictionary<int, string>();
        Dictionary<int, string> mobNames = new Dictionary<int, string>();

        if(Config.Instance.WorkMode == WorkMode.Standalone)
        {
            UI.ShowMessage("Initializing...",UIMessageType.Default);
        }

        //Main chara names (En only)
        if (Config.Instance.Language == Language.En)
        {
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
        foreach (var item in UmaCharaData)
        {
            var id = Convert.ToInt32(item["id"]);
            if (!Characters.Where(c => c.Id == id).Any())
            {
                Characters.Add(new CharaEntry()
                {
                    Name = enNames.ContainsKey(id) ? enNames[id] : item["charaname"].ToString(),
                    Icon = UmaViewerBuilder.Instance.LoadCharaIcon(id.ToString()),
                    Id = id,
                    ThemeColor = "#" + item["ui_nameplate_color_1"].ToString()
                });
            }
        }

        var MobCharaData = UmaDatabaseController.Instance.MobCharaData;
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

        UI.LoadModelPanels();
        UI.LoadMiniModelPanels();
        UI.LoadPropPanel();
        UI.LoadMapPanel();

        var asset = AbList.FirstOrDefault(a => a.Name.Equals("livesettings"));
        if (asset != null)
        {
            string filePath = asset.FilePath;
            if (File.Exists(filePath))
            {
                AssetBundle bundle = UmaViewerBuilder.LoadOrGet(asset);
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
                UI.LoadLivePanels();
            }
        }

        if (Config.Instance.WorkMode == WorkMode.Standalone)
        {
            UI.ShowMessage("Done.", UIMessageType.Close);
        }
    }

    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

}
