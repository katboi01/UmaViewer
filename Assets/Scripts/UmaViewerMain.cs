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

    public List<CharaEntry> Characters = new List<CharaEntry>();
    public List<UmaDatabaseEntry> AbList = new List<UmaDatabaseEntry>();

    [Header("Asset Memory")]
    public bool ShadersLoaded = false;
    public Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();

    private void Awake()
    {
        Application.targetFrameRate = 30;
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

            var container = Instantiate(UI.UmaContainerPrefab, UI.AnimationSetList.content).GetComponent<UmaUIContainer>();
            container.Name.text = container.name = "type00";
            var imageInstance = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                UI.HighlightChildImage(UI.AnimationSetList.content, imageInstance);
                ListAnimations(-1);
            });

            foreach (var chara in Characters.OrderBy(c => c.Id))
            {
                var charaInstance = chara;
                container = Instantiate(UI.UmaContainerPrefab, UI.CharactersList.content).GetComponent<UmaUIContainer>();
                container.Name.text = container.name = chara.Id + " " + chara.Name;
                var imageInstance1 = container.GetComponent<Image>();
                container.Button.onClick.AddListener(() => {
                    UI.HighlightChildImage(UI.CharactersList.content, imageInstance1);
                    ListCostumes(charaInstance.Id);
                });

                container = Instantiate(UI.UmaContainerPrefab, UI.AnimationSetList.content).GetComponent<UmaUIContainer>();
                container.Name.text = container.name = chara.Id + " " + chara.Name;
                var imageInstance2 = container.GetComponent<Image>();
                container.Button.onClick.AddListener(() => {
                    UI.HighlightChildImage(UI.AnimationSetList.content, imageInstance2);
                    ListAnimations(charaInstance.Id);
                });
            }

        });
    }

    void ListCostumes(int umaId)
    {
        for(int i = UI.CostumeList.content.childCount - 1; i >=0; i--)
        {
            Destroy(UI.CostumeList.content.GetChild(i).gameObject);
        }
        foreach(var entry in AbList.Where(a => !a.Name.Contains("clothes") && a.Name.Contains($"pfb_bdy{umaId}")))
        {
            var container = Instantiate(UI.UmaContainerPrefab, UI.CostumeList.content).GetComponent<UmaUIContainer>();
            string[] split = entry.Name.Split('_');
            string costumeId = split[split.Length - 1];
            container.Name.text = container.name = costumeId;
            container.Button.onClick.AddListener(() => {
                UI.HighlightChildImage(UI.CostumeList.content, container.GetComponent<Image>());
                StartCoroutine(Builder.LoadUma(umaId, costumeId));
            });
        }
    }

    void ListAnimations(int umaId)
    {
        for (int i = UI.AnimationList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(UI.AnimationList.content.GetChild(i).gameObject);
        }
        if(umaId == -1)
        {
            foreach (var entry in AbList.Where(a => a.Name.StartsWith(UmaDatabaseController.MotionPath) && a.Name.Contains($"/type00") && !a.Name.Contains($"mirror") && !a.Name.Contains($"mini") && !a.Name.Contains($"facial") && !a.Name.Contains($"_cam")))
            {
                var entryInstance = entry;
                var container = Instantiate(UI.UmaContainerPrefab, UI.AnimationList.content).GetComponent<UmaUIContainer>();
                container.Name.text = container.name = Path.GetFileName(entry.Name);
                container.Button.onClick.AddListener(() => {
                    UI.HighlightChildImage(UI.AnimationList.content, container.GetComponent<Image>());
                    Builder.LoadAsset(entryInstance);
                });
            }
        }
        else
            foreach (var entry in AbList.Where(a => a.Name.StartsWith(UmaDatabaseController.MotionPath) && a.Name.Contains($"chara/chr{umaId}") && !a.Name.Contains($"mirror") && !a.Name.Contains($"mini") && !a.Name.Contains($"facial") && !a.Name.Contains($"_cam")))
            {
                var entryInstance = entry;
                var container = Instantiate(UI.UmaContainerPrefab, UI.AnimationList.content).GetComponent<UmaUIContainer>();
                container.Name.text = container.name = Path.GetFileName(entry.Name);
                container.Button.onClick.AddListener(() => {
                UI.HighlightChildImage(UI.AnimationList.content, container.GetComponent<Image>());
                Builder.LoadAsset(entryInstance);
            });
        }
    }

    [System.Serializable]
    public class CharaEntry
    {
        public string Name;
        public int Id;
    }
}
