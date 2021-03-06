using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UmaViewerUI : MonoBehaviour
{
    public static UmaViewerUI Instance;
    private UmaViewerMain Main => UmaViewerMain.Instance;
    private UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    //normal models
    public ScrollRect CharactersList;
    public ScrollRect CostumeList;
    public ScrollRect AnimationSetList;
    public ScrollRect AnimationList;
    //mini models
    public ScrollRect MiniCharactersList;
    public ScrollRect MiniCostumeList;
    public ScrollRect MiniAnimationSetList;
    public ScrollRect MiniAnimationList;
    //other
    public ScrollRect PropList;
    public ScrollRect SceneList;

    //settings
    public TMPro.TMP_InputField SSWidth, SSHeight;

    public List<GameObject> TogglablePanels = new List<GameObject>();

    public GameObject UmaContainerPrefab;

    private int LoadedAssetCount = 0;
    [SerializeField] private RectTransform LoadedAssetsPanel;
    [SerializeField] private TMPro.TextMeshProUGUI LoadedAssetsText;

    public Color UIColor1, UIColor2;

    private void Awake()
    {
        Instance = this;
    }

    public void HighlightChildImage(Transform mainObject, Image child)
    {
        //Debug.Log("Looking for " + child.name + " in " + mainObject.name);
        foreach(var t in mainObject.GetComponentsInChildren<Image>())
        {
            if (t.transform.parent != mainObject) continue;
            t.color = t == child ? UIColor2 : UIColor1;
        }
    }

    public void LoadedAssetsAdd(string asset)
    {
        LoadedAssetCount++;
        if (LoadedAssetCount > 1)
            LoadedAssetsText.text += "\n";
        LoadedAssetsText.text += asset;
        LoadedAssetsPanel.sizeDelta = new Vector2(0, LoadedAssetCount * LoadedAssetsText.fontSize * 2.4f);
    }

    public void LoadedAssetsClear()
    {
        LoadedAssetCount = 0;
        LoadedAssetsText.text = "";
        LoadedAssetsPanel.sizeDelta = Vector2.zero;
    }

    public void LoadModelPanels()
    {
        var container = Instantiate(UmaContainerPrefab, AnimationSetList.content).GetComponent<UmaUIContainer>();
        container.Name.text = container.name = "type00";
        var imageInstance = container.GetComponent<Image>();
        container.Button.onClick.AddListener(() => {
            HighlightChildImage(AnimationSetList.content, imageInstance);
            ListAnimations(-1, false);
        });

        foreach (var chara in Main.Characters.OrderBy(c => c.Id))
        {
            var charaInstance = chara;
            container = Instantiate(UmaContainerPrefab, CharactersList.content).GetComponent<UmaUIContainer>();
            container.Name.text = container.name = chara.Id + " " + chara.Name;
            var imageInstance1 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(CharactersList.content, imageInstance1);
                ListCostumes(charaInstance.Id, false);
            });

            container = Instantiate(UmaContainerPrefab, AnimationSetList.content).GetComponent<UmaUIContainer>();
            container.Name.text = container.name = chara.Id + " " + chara.Name;
            var imageInstance2 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(AnimationSetList.content, imageInstance2);
                ListAnimations(charaInstance.Id, false);
            });
        }
    }

    public void LoadMiniModelPanels()
    {
        var container = Instantiate(UmaContainerPrefab, MiniAnimationSetList.content).GetComponent<UmaUIContainer>();
        container.Name.text = container.name = "type00";
        var imageInstance = container.GetComponent<Image>();
        container.Button.onClick.AddListener(() => {
            HighlightChildImage(MiniAnimationSetList.content, imageInstance);
            ListAnimations(-1, true);
        });

        foreach (var chara in Main.Characters.OrderBy(c => c.Id))
        {
            var charaInstance = chara;
            container = Instantiate(UmaContainerPrefab, MiniCharactersList.content).GetComponent<UmaUIContainer>();
            container.Name.text = container.name = chara.Id + " " + chara.Name;
            var imageInstance1 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(MiniCharactersList.content, imageInstance1);
                ListCostumes(charaInstance.Id, true);
            });

            container = Instantiate(UmaContainerPrefab, MiniAnimationSetList.content).GetComponent<UmaUIContainer>();
            container.Name.text = container.name = chara.Id + " " + chara.Name;
            var imageInstance2 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(MiniAnimationSetList.content, imageInstance2);
                ListAnimations(charaInstance.Id, true);
            });
        }
    }

    public void LoadPropPanel()
    {
        foreach (var prop in Main.AbList.Where(a=>a.Name.Contains("pfb_chr_prop") && !a.Name.Contains("clothes")))
        {
            var propInstance = prop;
            var container = Instantiate(UmaContainerPrefab, PropList.content).GetComponent<UmaUIContainer>();
            container.Name.text = container.name = Path.GetFileName(prop.Name);
            var imageInstance1 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(PropList.content, imageInstance1);
                Builder.LoadProp(propInstance);
            });
        }
    }

    public void LoadMapPanel()
    {
        foreach (var scene in Main.AbList.Where(a => a.Name.StartsWith("3d/env") && Path.GetFileName(a.Name).StartsWith("pfb_")))
        {
            var sceneInstance = scene;
            var container = Instantiate(UmaContainerPrefab, SceneList.content).GetComponent<UmaUIContainer>();
            container.Name.text = container.name = Path.GetFileName(scene.Name);
            var imageInstance1 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(SceneList.content, imageInstance1);
                Builder.LoadProp(sceneInstance);
            });
        }
    }

    void ListCostumes(int umaId, bool mini)
    {
        ScrollRect costumeList = mini ? MiniCostumeList : CostumeList;
        for (int i = costumeList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(costumeList.content.GetChild(i).gameObject);
        }
        string nameVar = mini ? $"pfb_mbdy{umaId}" : $"pfb_bdy{umaId}";
        foreach (var entry in Main.AbList.Where(a => !a.Name.Contains("clothes") && a.Name.Contains(nameVar)))
        {
            var container = Instantiate(UmaContainerPrefab, costumeList.content).GetComponent<UmaUIContainer>();
            string[] split = entry.Name.Split('_');
            string costumeId = split[split.Length - 1];
            container.Name.text = container.name = GetCostumeName(costumeId);
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(costumeList.content, container.GetComponent<Image>());
                StartCoroutine(Builder.LoadUma(umaId, costumeId, mini));
            });
        }
        //Common costumes
        List<string> costumes = new List<string>();
        nameVar = mini ? "pfb_mbdy0" : $"pfb_bdy0";
        foreach (var entry in Main.AbList.Where(a => a.Name.StartsWith("3d/chara/") && a.Name.Contains("/body/") && !a.Name.Contains("/clothes/") && a.Name.Contains(nameVar)))
        {
            string id = Path.GetFileName(entry.Name);
            id = id.Split('_')[1].Substring(mini ? 4 : 3) + "_" + id.Split('_')[2] + "_" + id.Split('_')[3];
            if (!costumes.Contains(id))
            {
                costumes.Add(id);
                string costumeId = id;
                var container = Instantiate(UmaContainerPrefab, costumeList.content).GetComponent<UmaUIContainer>();
                container.Name.text = container.name = GetCostumeName(id);
                container.Button.onClick.AddListener(() => {
                    HighlightChildImage(costumeList.content, container.GetComponent<Image>());
                    StartCoroutine(Builder.LoadUma(umaId, costumeId, mini));
                });
            }
        }
    }

    public static string GetCostumeName(string costumeId)
    {
        switch (costumeId)
        {
            case "00":
                return "Default";
            case "90":
                return "Upgraded";
            case "0001_00_01":
                return "Race Shorts";
            case "0001_00_02":
                return "Race Bloomers";
            case "0002_00_00":
                return "School Short Sleeves";
            case "0002_00_03":
                return "School Short Sleeves Big Belly";
            case "0002_01_00":
                return "School Long Sleeves";
            case "0002_01_03":
                return "School Long Sleeves Big Belly";
            case "0003_00_01":
                return "Tracksuit Shorts";
            case "0003_00_02":
                return "Tracksuit Bloomers";
            case "0003_01_01":
                return "Tracksuit Long Pants";
            case "0003_01_02":
                return "Tracksuit Rolled Up Pants";
            case "0004_00_00":
                return "Swimsuit";
            case "0004_01_00":
                return "Towel";
            default:
                return costumeId;
        }
    }

    void ListAnimations(int umaId, bool mini)
    {
        ScrollRect animationList = mini ? MiniAnimationList : AnimationList;
        for (int i = animationList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(animationList.content.GetChild(i).gameObject);
        }

        var filteredList = mini ?
            Main.AbList.Where(a => a.Name.StartsWith(UmaDatabaseController.MotionPath) && !a.Name.Contains($"mirror") && a.Name.Contains($"mini") && !a.Name.Contains($"facial") && !a.Name.Contains($"_cam"))
            :
            Main.AbList.Where(a => a.Name.StartsWith(UmaDatabaseController.MotionPath) && !a.Name.Contains($"mirror") && !a.Name.Contains($"mini") && !a.Name.Contains($"facial") && !a.Name.Contains($"_cam"));
        
        if (umaId == -1)
        {
            foreach (var entry in filteredList.Where(a=> a.Name.Contains($"/type00")))
            {
                var entryInstance = entry;
                var container = Instantiate(UmaContainerPrefab, animationList.content).GetComponent<UmaUIContainer>();
                container.Name.text = container.name = Path.GetFileName(entry.Name);
                container.Button.onClick.AddListener(() => {
                    HighlightChildImage(animationList.content, container.GetComponent<Image>());
                    Builder.LoadAsset(entryInstance);
                });
            }
        }
        else
            foreach (var entry in filteredList.Where(a => a.Name.Contains($"chara/chr{umaId}")))
            {
                var entryInstance = entry;
                var container = Instantiate(UmaContainerPrefab, animationList.content).GetComponent<UmaUIContainer>();
                container.Name.text = container.name = Path.GetFileName(entry.Name);
                container.Button.onClick.AddListener(() => {
                    HighlightChildImage(animationList.content, container.GetComponent<Image>());
                    Builder.LoadAsset(entryInstance);
                });
            }
    }

    /// <summary> Toggles one object ON and all others from UI.TogglablePanels list OFF </summary>
    public void ToggleUIPanel(GameObject go)
    {
        if (go.activeSelf || !TogglablePanels.Contains(go))
        {
            go.SetActive(!go.activeSelf);
            return;
        }

        foreach (var panel in TogglablePanels)
        {
            panel.SetActive(panel == go);
        }
    }

    public void ChangeBackground(int index)
    {
        if(index == 0)
        {
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        }
        else
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    public void ChangeBackgroundColor(Color color)
    {
        Camera.main.backgroundColor = color;
    }
}
