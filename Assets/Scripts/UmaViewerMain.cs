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

    [Header("Asset Memory")]
    public bool ShadersLoaded = false;
    public Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();

    enum Test
    {
        a,b,c,d
    }
    private void Awake()
    {
        Application.targetFrameRate = -1;
        Instance = this;
        AbList = UmaDatabaseController.Instance.MetaEntries.ToList();
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
            foreach (var item in UmaLiveData)
            {
                if (!Lives.Where(c => c.MusicId == (int)item["musicId"]).Any())
                {
                    Lives.Add(new LiveEntry()
                    {
                        MusicId = (int)item["musicId"],
                        songName = (string)item["songName"]
                    });
                }
            }
            UI.LoadLivePanels();
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
        public int Id;
    }

    [System.Serializable]
    public class LiveEntry
    {
        public int MusicId;
        public string songName;
    }
}
