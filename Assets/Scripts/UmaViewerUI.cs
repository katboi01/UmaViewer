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
        Debug.Log("Looking for " + child.name + " in " + mainObject.name);
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
        LoadedAssetsPanel.sizeDelta = new Vector2(0, LoadedAssetCount * 20);
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
            container.Name.text = container.name = costumeId;
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(costumeList.content, container.GetComponent<Image>());
                StartCoroutine(Builder.LoadUma(umaId, costumeId, mini));
            });
        }
        //Common costumes
        List<int> costumes = new List<int>();
        nameVar = mini ? "pfb_mbdy0" : $"pfb_bdy0";
        foreach (var entry in Main.AbList.Where(a => a.Name.StartsWith("3d/chara/") && a.Name.Contains("/body/") && !a.Name.Contains("/clothes/") && a.Name.Contains(nameVar)))
        {
            int id = int.Parse(Path.GetFileName(entry.Name).Split('_')[1].Substring(mini? 4: 3));
            if (!costumes.Contains(id))
            {
                costumes.Add(id);
                var container = Instantiate(UmaContainerPrefab, costumeList.content).GetComponent<UmaUIContainer>();
                string costumeId = container.Name.text = container.name = id.ToString().PadLeft(4,'0');
                container.Button.onClick.AddListener(() => {
                    HighlightChildImage(costumeList.content, container.GetComponent<Image>());
                    StartCoroutine(Builder.LoadUma(umaId, costumeId, mini));
                });
            }
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
}
