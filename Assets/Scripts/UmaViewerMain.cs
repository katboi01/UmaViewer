using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    public List<UmaDatabaseEntry> Motions = new List<UmaDatabaseEntry>();

    [Header("Asset Memory")]
    public bool ShadersLoaded = false;
    public Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = -1;
        AbList = UmaDatabaseController.Instance.MetaEntries.ToList();
        Motions = AbList.Where(ab => ab.Name.StartsWith(UmaDatabaseController.MotionPath)).ToList();
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

        yield return UmaViewerDownload.DownloadText("https://www.tracenacademy.com/api/BasicLiveDataInfo", txt =>
        {
            UmaLiveData = JArray.Parse(txt);
            var asset = AbList.FirstOrDefault(a => a.Name.Equals("livesettings"));
            if(asset != null)
            {
                string filePath = UmaDatabaseController.GetABPath(asset);
                if (File.Exists(filePath))
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
                    foreach (var item in UmaLiveData)
                    {
                        if (!Lives.Where(c => c.MusicId == (int)item["musicId"]).Any())
                        {
                            if (bundle.Contains((string)item["musicId"]))
                            {
                                TextAsset liveData = bundle.LoadAsset<TextAsset>((string)item["musicId"]);

                                Lives.Add(new LiveEntry(liveData.text)
                                {
                                    MusicId = (int)item["musicId"],
                                    songName = (string)item["songName"]
                                });
                            }
                        }
                    }
                    UI.LoadLivePanels();
                }
            }
        });
    }

    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    [System.Serializable]
    public class CharaEntry
    {
        public string Name;
        public Sprite Icon;
        public int Id;
    }

    [System.Serializable]
    public class LiveEntry
    {
        public int MusicId;
        public string songName;
        public string BackGroundId;
        public List<string[]> LiveSettings = new List<string[]>();

        public LiveEntry(string data)
        {
            string[] lines = data.Split("\n"[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                LiveSettings.Add(lines[i].Split(','));
            }
            BackGroundId = LiveSettings[1][2];
        }
    }
    
}
