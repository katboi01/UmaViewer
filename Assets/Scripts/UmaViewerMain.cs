using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;

public class UmaViewerMain : MonoBehaviour
{
    public static UmaViewerMain Instance;
    private UmaViewerUI UI => UmaViewerUI.Instance;
    private UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    public JArray UmaCharaData;
    public JArray UmaLiveData;

    public List<CharaEntry> Characters = new List<CharaEntry>();
    public List<LiveEntry> Lives = new List<LiveEntry>();
    public List<UmaDatabaseEntry> AbList = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbMotions = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbSounds = new List<UmaDatabaseEntry>();
    public List<UmaDatabaseEntry> AbChara = new List<UmaDatabaseEntry>();

    [Header("Asset Memory")]
    public bool ShadersLoaded = false;
    public Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = -1;
        AbList = UmaDatabaseController.Instance.MetaEntries.ToList();
        AbChara = AbList.Where(ab => ab.Name.StartsWith(UmaDatabaseController.CharaPath)).ToList();
        AbMotions = AbList.Where(ab => ab.Name.StartsWith(UmaDatabaseController.MotionPath)).ToList();
        AbSounds = AbList.Where(ab => ab.Name.EndsWith(".awb") || ab.Name.EndsWith(".acb")).ToList();
    }

    IEnumerator Start()
    {
        yield return UmaViewerDownload.DownloadText("https://www.tracenacademy.com/api/BasicCharaDataInfo", txt =>
        {
            UmaCharaData = JArray.Parse(txt);
            foreach (var item in UmaCharaData)
            {
                //Debug.Log(item);
                if (!Characters.Where(c => c.Id == (int)item["charaId"]).Any())
                {
                    Characters.Add(new CharaEntry()
                    {
                        Name = (string)item["charaNameEnglish"],
                        Icon = UmaViewerBuilder.Instance.LoadCharaIcon((string)item["charaId"]),
                        Id = (int)item["charaId"]
                    });
                }
            }

            UI.LoadModelPanels();
            UI.LoadMiniModelPanels();
            UI.LoadPropPanel();
            UI.LoadMapPanel();
        });



        var asset = AbList.FirstOrDefault(a => a.Name.Equals("livesettings"));
        if (asset != null)
        {
            string filePath = UmaDatabaseController.GetABPath(asset);
            if (File.Exists(filePath))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
                foreach (var item in UmaDatabaseController.Instance.LiveData)
                {
                    var musicId = Convert.ToInt32(item["music_id"]);
                    var songName = item["songname"].ToString();
                    if (!Lives.Where(c => c.MusicId == musicId).Any())
                    {
                        if (bundle.Contains(musicId.ToString()))
                        {
                            TextAsset liveData = bundle.LoadAsset<TextAsset>(musicId.ToString());

                            Lives.Add(new LiveEntry(liveData.text)
                            {
                                MusicId = musicId,
                                SongName = songName
                            });
                        }
                    }
                }
                UI.LoadLivePanels();
            }
        }
    }

    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

}
