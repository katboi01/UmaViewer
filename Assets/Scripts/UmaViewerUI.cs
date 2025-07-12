using Gallop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UmaViewerUI : MonoBehaviour
{
    public static UmaViewerUI Instance;
    private UmaViewerMain Main => UmaViewerMain.Instance;
    private UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    public CanvasScaler canvasScaler;

    [Header("normal models")]
    public ScrollRect CharactersList;
    public ScrollRect MobCharactersList;
    public PageManager MobCharactersPageCtrl;
    public ScrollRect CostumeList;
    public ScrollRect HeadCostumeList;
    public ScrollRect MobCostumeList;
    public ScrollRect AnimationSetList;
    public ScrollRect AnimationList;
    public PageManager AnimationPageCtrl;

    [Header("mini models")]
    public ScrollRect MiniCharactersList;
    public ScrollRect MiniCostumeList;
    public ScrollRect MiniAnimationSetList;
    public ScrollRect MiniAnimationList;
    public PageManager MiniAnimationPageCtrl;

    [Header("other")]
    public ScrollRect PropList;
    public PageManager PropPageCtrl;
    public ScrollRect SceneList;
    public PageManager ScenePageCtrl;
    public ScrollRect MessageScrollRect;
    public Text MessageText;
    public PoseManager PoseManager;
    public HandleManager HandleManager;

    [Header("lists")]
    public ScrollRect EmotionList;
    public Transform FacialList;
    public ScrollRect EarList;
    public ScrollRect EyeBrowList;
    public ScrollRect EyeList;
    public ScrollRect MouthList;
    public ScrollRect OtherList;
    public ScrollRect LiveList;
    public ScrollRect LiveSoundList;
    public ScrollRect LiveCharaSoundList;
    public ScrollRect NormalSoundList;
    public ScrollRect NormalSubSoundList;
    public PageManager NormalSoundCtrl;

    [Header("pose mode")]
    public Transform HandlesPanel;

    [Header("live settings")]
    public Sprite CharaIconDefault;
    public Sprite CostumeIconDefault;
    public LiveCharacterSelect CurrentSeletChara;
    public GameObject LiveSelectPrefab;
    public GameObject LiveSelectPannel;
    public GameObject SelectCharacterPannel;
    public ScrollRect LiveSelectList;
    public Image LiveSelectImage;
    public Text LiveSelectInfoText;
    private LiveEntry currentLive;

    [Header("settings")]
    public UISettingsCamera CameraSettings;
    public UISettingsModel ModelSettings;
    public UISettingsSound AudioSettings;
    public UISettingsAnimation AnimationSettings;
    public UISettingsScreenshot ScreenshotSettings;
    public UISettingsAssets AssetSettings;
    public UISettingsOther OtherSettings;
    public List<GameObject> TogglablePanels = new List<GameObject>();
    public List<GameObject> TogglableFacials = new List<GameObject>();

    [Header("prefabs")]
    public GameObject UmaContainerPrefab;
    public GameObject UmaContainerCostumePrefab;
    public GameObject UmaContainerLivePrefab;
    public GameObject UmaContainerSliderPrefab;
    public UmaUIContainer UmaContainerTogglePrefab;

    public Color UIColor1, UIColor2;

    public string CurrentHeadCostumeId = string.Empty;

    [Header("Live")]
    public bool LiveTime = false;
    public bool isRecordVMD;
    public bool isRequireStage = true;
    public Dropdown LiveRecoedToggle;
    public int LiveMode = 1;

    public FaceDrivenKeyTarget currentFaceDrivenKeyTarget;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        OtherSettings.ApplySettings();
        CameraSettings.AAModeDropdown.SetValueWithoutNotify(Config.Instance.AntiAliasing);
        AssetSettings.LoadedAssetsClear();
        UmaAssetManager.OnLoadedBundleUpdate += AssetSettings.LoadedAssetsAdd;
        UmaAssetManager.OnLoadedBundleClear += AssetSettings.LoadedAssetsClear;
        
        PoseManager.LoadLocalPoseFiles();
#if UNITY_ANDROID && !UNITY_EDITOR
        canvasScaler.referenceResolution = new Vector2(1280, 720);
#endif
        StartCoroutine(ApplyGraphicsSettings());
    }

    private void OnDestroy()
    {
        UmaAssetManager.OnLoadedBundleUpdate -= AssetSettings.LoadedAssetsAdd;
        UmaAssetManager.OnLoadedBundleClear -= AssetSettings.LoadedAssetsClear;
    }

    private void Update()
    {
        if (Builder.CurrentAudioSources.Count > 0 && Builder.CurrentAudioSources[0])
        {
            AudioSource MianSource = Builder.CurrentAudioSources[0];
            if (MianSource.clip)
            {
                AudioSettings.UpdateTrack(MianSource);
            }
        }

        var umaContainer = Builder.CurrentUMAContainer;
        if (umaContainer != null && umaContainer.OverrideController != null)
        {
            AnimationSettings.UpdateAnimationInfo(umaContainer);
        }
    }

    /// <summary>Some settings may not be saved when set in Start()</summary>
    public IEnumerator ApplyGraphicsSettings()
    {
        yield return 0;
        CameraSettings.ChangeAntiAliasing(Config.Instance.AntiAliasing);
        GraphicsSettings.renderPipelineAsset = Config.Instance.Region == Region.Global ? null : UmaViewerMain.Instance.DefaultRenderPipeline;
    }

    public void HighlightChildImage(Transform mainObject, UmaUIContainer child)
    {
        foreach (var t in mainObject.GetComponentsInChildren<UmaUIContainer>())
        {
            if (t.transform.parent != mainObject) continue;
            t.ToggleImage.enabled = (t == child);
        }
    }

    public void LoadModelPanels()
    {
        var containerG = Instantiate(UmaContainerPrefab, AnimationSetList.content).GetComponent<UmaUIContainer>();
        containerG.Name = containerG.name = "Generic";
        containerG.Button.onClick.AddListener(() =>
        {
            HighlightChildImage(AnimationSetList.content, containerG);
            ListAnimations(-1, false);
        });

        var containerT = Instantiate(UmaContainerPrefab, AnimationSetList.content).GetComponent<UmaUIContainer>();
        containerT.Name = containerT.name = "Tail";
        containerT.Button.onClick.AddListener(() =>
        {
            HighlightChildImage(AnimationSetList.content, containerT);
            ListAnimations(-2, false);
        });

        var containerE = Instantiate(UmaContainerPrefab, AnimationSetList.content).GetComponent<UmaUIContainer>();
        containerE.Name = containerE.name = "Ear";
        containerE.Button.onClick.AddListener(() =>
        {
            HighlightChildImage(AnimationSetList.content, containerE);
            ListAnimations(-3, false);
        });

        foreach (var chara in Main.Characters.OrderBy(c => c.Id))
        {
            var charaInstance = chara;

            var container3 = Instantiate(UmaContainerPrefab, CharactersList.content).GetComponent<UmaUIContainer>();
            container3.Name = container3.name = chara.Id + " " + chara.GetName();
            container3.Button.onClick.AddListener(() =>
            {
                HighlightChildImage(CharactersList.content, container3);
                ListCostumes(charaInstance, false);
            });

            if (chara.Icon)
            {
                container3.Image.sprite = (chara.Icon == null ? CharaIconDefault : chara.Icon);
                container3.Image.enabled = true;
            }

            var container4 = Instantiate(UmaContainerPrefab, AnimationSetList.content).GetComponent<UmaUIContainer>();
            container4.Name = container4.name = chara.Id + " " + chara.GetName();
            container4.Button.onClick.AddListener(() =>
            {
                HighlightChildImage(AnimationSetList.content, container4);
                ListAnimations(charaInstance.Id, false);
            });

            if (chara.Icon)
            {
                container4.Image.sprite = (chara.Icon == null ? CharaIconDefault : chara.Icon);
                container4.Image.enabled = true;
            }
        }

        var pageentrys = new List<PageManager.Entry>();
        foreach (var chara in Main.MobCharacters.OrderBy(c => c.Id))
        {
            var charaInstance = chara;
            var pageentry = new PageManager.Entry();

            pageentry.Name = chara.GetName();
            pageentry.OnClick = (container) =>
            {
                HighlightChildImage(MobCharactersList.content, container);
                ListCostumes(charaInstance, false);
            };
            if (chara.Icon)
            {
                pageentry.Sprite = chara.Icon;
            }
            pageentrys.Add(pageentry);
        }
        MobCharactersPageCtrl.Initialize(pageentrys, MobCharactersList);
    }

    public void LoadMiniModelPanels()
    {
        var container1 = Instantiate(UmaContainerPrefab, MiniAnimationSetList.content).GetComponent<UmaUIContainer>();
        container1.Name = container1.name = "General";
        container1.Button.onClick.AddListener(() =>
        {
            HighlightChildImage(MiniAnimationSetList.content, container1);
            ListAnimations(-1, true);
        });

        foreach (var chara in Main.Characters.OrderBy(c => c.Id))
        {
            var charaInstance = chara;
            var container2 = Instantiate(UmaContainerPrefab, MiniCharactersList.content).GetComponent<UmaUIContainer>();
            container2.Name = container2.name = chara.Id + " " + chara.GetName();
            container2.Button.onClick.AddListener(() =>
            {
                HighlightChildImage(MiniCharactersList.content, container2);
                ListCostumes(charaInstance, true);
            });

            if (chara.Icon)
            {
                container2.Image.sprite = chara.Icon;
                container2.Image.enabled = true;
            }

            var container3 = Instantiate(UmaContainerPrefab, MiniAnimationSetList.content).GetComponent<UmaUIContainer>();
            container3.Name = container3.name = chara.Id + " " + chara.GetName();
            container3.Button.onClick.AddListener(() =>
            {
                HighlightChildImage(MiniAnimationSetList.content, container3);
                ListAnimations(charaInstance.Id, true);
            });

            if (chara.Icon)
            {
                container3.Image.sprite = chara.Icon;
                container3.Image.enabled = true;
            }
        }
    }

    public void LoadFacialPanels(FaceDrivenKeyTarget target)
    {
        currentFaceDrivenKeyTarget = target;
        if(FacialList)
        foreach (UmaUIContainer ui in FacialList.GetComponentsInChildren<UmaUIContainer>(true))
        {
            Destroy(ui.gameObject);
        }

        if (target == null) return;
        List<FacialMorph> targetMorph = new List<FacialMorph>(target.OtherMorphs);
        Action<GameObject, FaceDrivenKeyTarget, List<FacialMorph>, ScrollRect> action = (UmaContainerSliderPrefab, target, targetMorph, TargetList) =>
        {
            foreach (FacialMorph morph in targetMorph)
            {
                if (morph is FacialOtherMorph extraMorph)
                {
                    foreach (var prop in extraMorph.BindProperties)
                    {
                        var container = Instantiate(UmaContainerSliderPrefab, TargetList.content).GetComponent<UmaUIContainer>();
                        container.Name = $"{morph.name}({prop.Type})";
                        container.Slider.value = prop.Value;
                        switch (prop.Type)
                        {
                            case BindProperty.BindType.Shader:
                            case BindProperty.BindType.Texture:
                                container.Slider.maxValue = 1;
                                container.Slider.minValue = 0;
                                break;
                            case BindProperty.BindType.Select:
                                container.Slider.wholeNumbers = true;
                                container.Slider.maxValue = prop.BindPrefabs.Count - 1;
                                container.Slider.minValue = 0;
                                break;
                            case BindProperty.BindType.EyeSelect:
                                container.Slider.wholeNumbers = true;
                                container.Slider.maxValue = prop.BindPrefabs.Count / 2 - 1;
                                container.Slider.minValue = 0;
                                break;
                            case BindProperty.BindType.Enable:
                            case BindProperty.BindType.TearSelect:
                            case BindProperty.BindType.TearSide:
                                container.Slider.wholeNumbers = true;
                                container.Slider.maxValue = 1;
                                container.Slider.minValue = 0;
                                break;
                            case BindProperty.BindType.TearWeight:
                                container.Slider.maxValue = 1;
                                container.Slider.minValue = 0;
                                break;
                            case BindProperty.BindType.TearSpeed:
                                container.Slider.maxValue = 2;
                                container.Slider.minValue = 0;
                                break;
                        }
                        container.Slider.onValueChanged.AddListener((a) =>
                        {
                            target.ChangeMorphWeight(prop, a, morph);
                        });
                    }
                }
                else
                {
                    var container = Instantiate(UmaContainerSliderPrefab, TargetList.content).GetComponent<UmaUIContainer>();
                    container.Name = $"{morph.name}({morph.tag})";
                    container.Slider.value = morph.weight;
                    container.Slider.maxValue = 1;
                    container.Slider.minValue = morph.tag.Contains("Range") ? -1 : 0;
                    container.Slider.onValueChanged.AddListener((a) => { target.ChangeMorphWeight(morph, a); });
                }
            }
        };
        action(UmaContainerSliderPrefab, target, target.EarMorphs, EarList);
        action(UmaContainerSliderPrefab, target, target.EyeBrowMorphs, EyeBrowList);
        action(UmaContainerSliderPrefab, target, target.EyeMorphs, EyeList);
        action(UmaContainerSliderPrefab, target, target.MouthMorphs, MouthList);
        action(UmaContainerSliderPrefab, target, targetMorph, OtherList);
    }

    public void LoadFacialPanelsMini(Material Eye, Material MayuL, Material MayuR, Material Mouth)
    {
        currentFaceDrivenKeyTarget = null;
        foreach (UmaUIContainer ui in FacialList.GetComponentsInChildren<UmaUIContainer>(true))
        {
            Destroy(ui.gameObject);
        }

        var action = new Action<ScrollRect, string, Material, int>((list, name, mat, type) =>
        {
            if (mat == null) return;
            var container = Instantiate(UmaContainerSliderPrefab, list.content).GetComponent<UmaUIContainer>();
            container.Name = name;
            container.Slider.wholeNumbers = true;
            container.Slider.value = 0;
            container.Slider.maxValue = 15;
            container.Slider.minValue = 0;
            container.Slider.onValueChanged.AddListener((a) =>
            {
                var val = Convert.ToInt32(a);
                if (type == 0)
                {
                    mat.mainTextureOffset = new Vector2(0.25f * (val % 4), -0.25f * (val / 4));
                }
                else if (type == 1)
                {
                    var vector = mat.GetVector("_UVOffset");
                    mat.SetVector("_UVOffset", new Vector4(0.25f * (val % 4), -0.25f * (val / 4), vector.z, vector.w));
                }
                else if (type == 2)
                {
                    var vector = mat.GetVector("_UVOffset");
                    mat.SetVector("_UVOffset", new Vector4(vector.x, vector.y, 0.25f * (val % 4), -0.25f * (val / 4)));
                }
            });
        });

        action(EyeList, "Eye_L_Select", Eye, 2);
        action(EyeList, "Eye_R_Select", Eye, 1);
        action(MouthList, "Mouth_Select", Mouth, 0);
        action(EyeBrowList, "Mayu_L_Select", MayuL, 0);
        action(EyeBrowList, "Mayu_R_Select", MayuR, 0);
    }

    public void UpdateFacialPanels()
    {
        if (currentFaceDrivenKeyTarget)
            LoadFacialPanels(currentFaceDrivenKeyTarget);
    }

    public void LoadEmotionPanels(FaceEmotionKeyTarget target)
    {
        if (EmotionList)
        foreach (UmaUIContainer ui in EmotionList.content.GetComponentsInChildren<UmaUIContainer>())
        {
            Destroy(ui.gameObject);
        }

        if (target == null) return;

        foreach (var emotion in target.FaceEmotionKey)
        {
            if (emotion.label == "Base")
            {
                continue;
            }
            var container = Instantiate(UmaContainerSliderPrefab, EmotionList.content).GetComponent<UmaUIContainer>();
            container.Name = emotion.label;
            container.Slider.value = emotion.weight;
            container.Slider.maxValue = 1;
            container.Slider.minValue = 0;
            container.Slider.onValueChanged.AddListener((a) => { target.ChangeEmotionWeight(emotion, a); });
        }
    }

    public void LoadLivePanels()
    {
        Action<LiveEntry, GameObject, ScrollRect, UnityAction> createContainer = (live, containerPrefab, parent, onClickAction) =>
        {
            var container = Instantiate(containerPrefab, parent.content).GetComponent<UmaUIContainer>();
            container.Name = " " + live.MusicId + " " + live.SongName;
            container.Image.sprite = live.Icon;
            container.Image.enabled = container.Image.sprite;
            container.Button.onClick.AddListener(() =>
            {
                HighlightChildImage(parent.content, container);
                onClickAction.Invoke();
            });
        };

        foreach (var live in Main.Lives.OrderBy(c => c.MusicId))
        {
            createContainer(live, UmaContainerLivePrefab, LiveList, () => ShowLiveSelectPanel(live));
            createContainer(live, UmaContainerLivePrefab, LiveSoundList, () => ListLiveSounds(live.MusicId));
        }
    }

    public void LoadNormalSoundPanels()
    {
        NormalSoundCtrl.Clear();
        var pageentrys = new List<PageManager.Entry>();
        foreach (var entry in Main.AbSounds.Where(a => (!a.Name.Contains("live")) && a.Name.EndsWith("awb")))
        {
            var pageentry = new PageManager.Entry();
            pageentry.Name = Path.GetFileNameWithoutExtension(entry.Name);
            pageentry.OnClick = (container) =>
            {
                HighlightChildImage(NormalSoundList.content, container);
                ListNormalSubSounds(entry);
            };
            pageentrys.Add(pageentry);
        }
        NormalSoundCtrl.Initialize(pageentrys, NormalSoundList);
    }

    public void LoadPropPanel()
    {
        var pageentrys = new List<PageManager.Entry>();
        foreach (var prop in Main.AbList.Where(a => a.Key.Contains("pfb_chr_prop") && !a.Key.Contains("clothes")))
        {
            var pageentry = new PageManager.Entry();
            pageentry.Name = Path.GetFileName(prop.Key);
            pageentry.OnClick = (container) =>
            {
                HighlightChildImage(PropList.content, container);
                Builder.LoadProp(prop.Value);
            };
            pageentrys.Add(pageentry);
        }
        PropPageCtrl.Initialize(pageentrys, PropList);
    }

    public void LoadMapPanel()
    {
        var pageentrys = new List<PageManager.Entry>();
        foreach (var scene in Main.AbList.Where(a => ((a.Key.StartsWith("3d/env") && Path.GetFileName(a.Key).StartsWith("pfb_")) || a.Key.StartsWith("cutt/cutt_son") && Path.GetFileName(a.Key).StartsWith("cutt_son"))))
        {
            var pageentry = new PageManager.Entry();
            pageentry.Name = Path.GetFileName(scene.Key);
            pageentry.FontSize = 19;
            pageentry.OnClick = (container) =>
            {
                HighlightChildImage(SceneList.content, container);
                Builder.LoadProp(scene.Value);
            };
            pageentrys.Add(pageentry);
        }
        ScenePageCtrl.Initialize(pageentrys, SceneList);
    }

    public void PlayLive()
    {
        if (currentLive == null) return;
        PoseManager.SetPoseMode(false);
        var selectlist = LiveSelectList.content.GetComponentsInChildren<LiveCharacterSelect>();
        if (selectlist != null)
        {
            LiveTime = true;
            ModelSettings.SetEyeTrackingEnable(false);
            Builder.LoadLive(currentLive, new List<LiveCharacterSelect>(selectlist));
            LiveSelectPannel.SetActive(false);
        }
    }

    void ShowLiveSelectPanel(LiveEntry entry)
    {
        LiveSelectPannel.SetActive(true);
        Builder.loadLivePreviewSound(entry.MusicId);
        for (int i = LiveSelectList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(LiveSelectList.content.GetChild(i).gameObject);
        }

        if(LiveMode == 1)
        {
            currentLive = entry;
            LiveSelectImage.sprite = entry.Icon;
            LiveSelectInfoText.text = $"{entry.SongName}\nMembers Count: {entry.MemberCount}";
            for (int i = 0; i < entry.MemberCount; i++)
            {
                var chara = Instantiate(LiveSelectPrefab, LiveSelectList.content).GetComponent<LiveCharacterSelect>();
                chara.IndexText.text = (i + 1).ToString();
                chara.GetComponent<Button>().onClick.AddListener(() =>
                {
                    chara.SelectChara(this);
                    foreach (var t in LiveSelectList.content.GetComponentsInChildren<LiveCharacterSelect>())
                    {
                        t.GetComponentInChildren<Text>().color = (t == chara ? Color.green : Color.black);
                    }
                });
            }
        }
        else if(LiveMode == 0)
        {
            currentLive = entry;
            LiveSelectImage.sprite = entry.Icon;

            int mainCount = 0;
            foreach (var motion in Main.AbMotions.Where(a => a.Name.StartsWith($"3d/motion/live/body/son{entry.MusicId}") && Path.GetFileName(a.Name).Split('_').Length == 4))
            {
                mainCount += 1;
            }

            LiveSelectInfoText.text = $"{entry.SongName}\nMain Members Count: {mainCount}";
            for (int i = 0; i < mainCount; i++)
            {
                var chara = Instantiate(LiveSelectPrefab, LiveSelectList.content).GetComponent<LiveCharacterSelect>();
                chara.IndexText.text = (i + 1).ToString();
                chara.GetComponent<Button>().onClick.AddListener(() =>
                {
                    chara.SelectChara(this);
                    foreach (var t in LiveSelectList.content.GetComponentsInChildren<LiveCharacterSelect>())
                    {
                        t.GetComponentInChildren<Text>().color = (t == chara ? Color.white : Color.black);
                    }
                });
            }
        }
        
    }

    void ListCostumes(CharaEntry chara, bool mini)
    {
        ScrollRect costumeList = mini ? MiniCostumeList : chara.IsMob ? MobCostumeList : CostumeList;
        for (int i = costumeList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(costumeList.content.GetChild(i).gameObject);
        }

        Action<CharaEntry> action = delegate (CharaEntry achara)
        {
            string nameVar = mini ? $"pfb_mbdy{achara.Id}" : $"pfb_bdy{achara.Id}";
            string bodyPath = mini ? UmaDatabaseController.MiniBodyPath : UmaDatabaseController.BodyPath;
            foreach (var entry in Main.AbChara.Where(a => a.Name.StartsWith(bodyPath) && !a.Name.Contains("clothes") && a.Name.Contains(nameVar)))
            {
                var container = Instantiate(UmaContainerCostumePrefab, costumeList.content).GetComponent<UmaUIContainer>();
                string[] split = entry.Name.Split('_');
                string costumeId = split[split.Length - 1];
                var dressdata = Main.Costumes.FirstOrDefault(a => (a.CharaId == achara.Id && a.BodyTypeSub == int.Parse(costumeId)));
                container.Name = container.name = GetCostumeName(costumeId, (dressdata == null ? costumeId : dressdata.DressName));
                container.Image.sprite = (dressdata == null ? CostumeIconDefault : dressdata.Icon);
                container.Image.enabled = true;
                container.Id = costumeId;
                container.Button.onClick.AddListener(() =>
                {
                    if (LiveSelectPannel.activeInHierarchy && CurrentSeletChara)
                    {
                        CurrentSeletChara.SetValue(achara, costumeId, container.Image.sprite, CurrentHeadCostumeId);
                    }
                    else
                    {
                        HighlightChildImage(costumeList.content, container);
                        var list = new List<UmaDatabaseEntry>();
                        list.AddRange(Main.AbChara.Where(a => a.Name.StartsWith(UmaDatabaseController.BodyPath) && a.Name.Contains(chara.Id.ToString())));
                        list.AddRange(Main.AbChara.Where(a => a.Name.StartsWith(UmaDatabaseController.HeadPath) && a.Name.Contains(chara.Id.ToString())));
                        list.AddRange(Main.AbChara.Where(a => a.Name.StartsWith("3d/chara/tail")));
                        list.Add(Main.AbList["3d/animator/drivenkeylocator"]);
                        list.Add(Main.AbList[$"3d/motion/event/body/chara/chr{achara.Id}_00/anm_eve_chr{achara.Id}_00_idle01_loop"]);

                        Builder.UnloadUma();
                        //UmaAssetManager.UnloadAllBundle(true);
                        UmaAssetManager.PreLoadAndRun(list , delegate {
                            StartCoroutine(Builder.LoadUma(achara, costumeId, mini, CurrentHeadCostumeId));
                        });
                        //StartCoroutine(Builder.LoadUma(achara, costumeId, mini));
                    }
                });
            }
        };

        if (!chara.IsMob)
        {
            action.Invoke(chara);
        }

        //Common costumes
        List<string> costumes = new List<string>();
        var nameVar = mini ? "pfb_mbdy0" : $"pfb_bdy0";
        string bodyPath = mini ? UmaDatabaseController.MiniBodyPath : UmaDatabaseController.BodyPath;
        foreach (var entry in Main.AbChara.Where(a => a.Name.StartsWith(bodyPath) && !a.Name.Contains("/clothes/") && a.Name.Contains(nameVar)))
        {
            string id = Path.GetFileName(entry.Name);
            string[] split = id.Split('_');
            if (split.Length < 4) continue; //prevents error for mini umas
            id = split[1].Substring(mini ? 4 : 3) + "_" + split[2] + "_" + split[3];
            if (!costumes.Contains(id))
            {
                costumes.Add(id);
                string costumeId = id;
                var container = Instantiate(UmaContainerCostumePrefab, costumeList.content).GetComponent<UmaUIContainer>();
                var data = costumeId.Split('_');
                var bodytype = int.Parse(data[0]);
                var bodysub = int.Parse(data[1]);

                var dressdata = Main.Costumes.FirstOrDefault(a => a.BodyType == bodytype && a.BodyTypeSub == bodysub);
                container.Name = container.name = GetCostumeName(id, (dressdata == null ? id : dressdata.DressName));
                container.Image.sprite = (dressdata == null ? CostumeIconDefault : dressdata.Icon);
                container.Image.enabled = true;
                container.Id = costumeId;
                container.Button.onClick.AddListener(() =>
                {
                    if (LiveSelectPannel.activeInHierarchy && CurrentSeletChara)
                    {
                        CurrentSeletChara.SetValue(chara, costumeId, container.Image.sprite, CurrentHeadCostumeId);
                    }
                    else
                    {
                        HighlightChildImage(costumeList.content, container);
                        var list = new List<UmaDatabaseEntry>();
                        list.AddRange(Main.AbChara.Where(a => a.Name.StartsWith(UmaDatabaseController.BodyPath) && a.Name.Contains(chara.Id.ToString())));
                        list.AddRange(Main.AbChara.Where(a => a.Name.StartsWith(UmaDatabaseController.HeadPath) && a.Name.Contains(chara.Id.ToString())));
                        list.AddRange(Main.AbChara.Where(a => a.Name.StartsWith("3d/chara/tail")));
                        list.Add(Main.AbList["3d/animator/drivenkeylocator"]);
                        var motion_path = $"3d/motion/event/body/chara/chr{chara.Id}_00/anm_eve_chr{chara.Id}_00_idle01_loop";
                        if (!chara.IsMob && !ModelSettings.IsHeadFix && Main.AbList.TryGetValue(motion_path, out var motion_entry))
                        {
                            list.Add(motion_entry);
                        }

                        Builder.UnloadUma();
                        //UmaAssetManager.UnloadAllBundle(true);
                        UmaAssetManager.PreLoadAndRun(list, delegate { StartCoroutine(Builder.LoadUma(chara, costumeId, mini, CurrentHeadCostumeId)); });
                        //StartCoroutine(Builder.LoadUma(achara, costumeId, mini));
                    }
                });
            }
        }

        if(!chara.IsMob && !mini)
        {
            for (int i = HeadCostumeList.content.childCount - 1; i >= 0; i--)
            {
                Destroy(HeadCostumeList.content.GetChild(i).gameObject);
            }
            CurrentHeadCostumeId = string.Empty;
            nameVar = $"/pfb_chr{chara.Id}_";
            foreach (var entry in Main.AbChara.Where(a => a.Name.StartsWith($"{UmaDatabaseController.HeadPath}chr{chara.Id}_") && !a.Name.Contains("clothes") && a.Name.Contains(nameVar)))
            {
                var container = Instantiate(UmaContainerCostumePrefab, HeadCostumeList.content).GetComponent<UmaUIContainer>();
                string[] split = entry.Name.Split('_');
                string head_costumeId = split[split.Length - 1];
                var dressdata = Main.Costumes.FirstOrDefault(a => (a.CharaId == chara.Id && a.BodyTypeSub == int.Parse(head_costumeId)));
                container.Name = container.name = GetCostumeName(head_costumeId, (dressdata == null ? head_costumeId : dressdata.DressName));
                container.Image.sprite = (dressdata == null ? CostumeIconDefault : dressdata.Icon);
                container.Image.enabled = true;
                container.Id = head_costumeId;
                container.Button.onClick.AddListener(() =>
                {
                    CurrentHeadCostumeId = head_costumeId;
                    if (!LiveSelectPannel.activeInHierarchy || !CurrentSeletChara)
                    {
                        HighlightChildImage(HeadCostumeList.content, container);
                    }
                    bool hasBreak = false;
                    foreach (var t in CostumeList.GetComponentsInChildren<UmaUIContainer>())
                    {
                        if (t.ToggleImage.enabled)
                        {
                            hasBreak = true;
                            t.Button.onClick.Invoke();
                            break;
                        }
                        else if(CurrentSeletChara && CurrentSeletChara.CostumeId == t.Id)
                        {
                            hasBreak = true;
                            t.Button.onClick.Invoke();
                            break;
                        }
                    }
                    if (!hasBreak)
                    {
                        //if no costume selected, select first costume
                        if (CostumeList.content.childCount > 0)
                        {
                            CostumeList.content.GetChild(0).GetComponent<UmaUIContainer>().Button.onClick.Invoke();
                        }
                    }
                });
            }
        }
    }

    void ListLiveSounds(int songid)
    {
        for (int i = LiveCharaSoundList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(LiveCharaSoundList.content.GetChild(i).gameObject);
        }
        string nameVar = $"snd_bgm_live_{songid}_chara";
        foreach (var entry in Main.AbSounds.Where(a => a.Name.Contains(nameVar) && a.Name.EndsWith("awb")))
        {
            var container = Instantiate(UmaContainerPrefab, LiveCharaSoundList.content).GetComponent<UmaUIContainer>();
            string[] split = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
            string name = $"{split[split.Length - 2]} {getCharaName(split[split.Length - 2])}";
            container.Name = name;

            var icon = Main.Characters.Find(a => a.Id.ToString().Equals(split[split.Length - 2]));
            if (icon != null)
            {
                container.Image.sprite = icon.Icon;
                container.Image.enabled = true;
            }

            container.Button.onClick.AddListener(() =>
            {
                HighlightChildImage(LiveCharaSoundList.content, container);
                Builder.LoadLiveSound(songid, entry);
            });
        }

    }

    void ListNormalSubSounds(UmaDatabaseEntry awb)
    {
        for (int i = NormalSubSoundList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(NormalSubSoundList.content.GetChild(i).gameObject);
        }

        var subSounds = UmaViewerBuilder.LoadAudio(awb);

        foreach (var sound in subSounds)
        {
            var container = Instantiate(UmaContainerPrefab, NormalSubSoundList.content).GetComponent<UmaUIContainer>();
            container.Name = $"Clip_{subSounds.IndexOf(sound)}";
            container.Button.onClick.AddListener(() =>
            {
                HighlightChildImage(NormalSubSoundList.content, container);
                Builder.PlaySound(awb, subSounds.IndexOf(sound));
            });
        }
    }

    void ListAnimations(int umaId, bool mini)
    {
        ScrollRect animationList = mini ? MiniAnimationList : AnimationList;
        PageManager pageManager = mini ? MiniAnimationPageCtrl : AnimationPageCtrl;
        for (int i = animationList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(animationList.content.GetChild(i).gameObject);
        }

        var filteredList = Main.AbMotions.Where(a => !a.Name.Contains($"mirror")
        && (mini ? a.Name.Contains($"mini") : !a.Name.Contains($"mini"))
        && !a.Name.Contains($"facial")
        && !a.Name.Contains($"_cam")
        && !a.Name.EndsWith($"_s")
        && !a.Name.EndsWith($"_e")
        );

        if (umaId == -1)
        {
            var pageentrys = new List<PageManager.Entry>();
            foreach (var entry in filteredList.Where(a => (a.Name.Contains($"/type0") || a.Name.Contains($"/type99") || a.Name.Contains($"anm_sty_")) && !a.Name.Contains($"/tail") && !a.Name.EndsWith($"_pos") && !a.Name.Contains($"prop") && !a.Name.EndsWith($"_pose") && !a.Name.Contains($"_defaultmotion")))
            {
                var pageentry = new PageManager.Entry();
                pageentry.Name = Path.GetFileName(entry.Name);
                pageentry.FontSize = 20;
                pageentry.OnClick = (container) =>
                {
                    HighlightChildImage(animationList.content, container);
                    (Builder.CurrentUMAContainer)?.LoadAnimation(entry);
                    LoadedAnimation();
                };
                pageentrys.Add(pageentry);
            }
            pageManager.Initialize(pageentrys, animationList);
        }
        else if (umaId == -2)
        {
            foreach (var entry in filteredList.Where(a => a.Name.Contains($"/tail")))
            {
                CreateAnimationSelectionPanel(entry, animationList.content);
            }
            pageManager.ResetCtrl();
        }
        else if (umaId == -3)
        {
            foreach (var entry in Main.AbMotions.Where(a => a.Name.Contains($"type00_ear") && !a.Name.EndsWith("driven") && !a.Name.Contains("touch")))
            {
                CreateAnimationSelectionPanel(entry, animationList.content);
            }
            pageManager.ResetCtrl();
        }
        else
        {
            //Common animations
            foreach (var entry in filteredList.Where(a => a.Name.Contains($"chara/chr{umaId}") && !a.Name.Contains("pose")))
            {
                CreateAnimationSelectionPanel(entry, animationList.content);
            }

            //Skill animations
            foreach (var entry in filteredList.Where(a => a.Name.Contains($"card/body/crd{umaId}")))
            {
                CreateAnimationSelectionPanel(entry, animationList.content);
            }
            pageManager.ResetCtrl();
        }
    }

    void CreateAnimationSelectionPanel(UmaDatabaseEntry entry, Transform parent)
    {
        var container = Instantiate(UmaContainerPrefab, parent).GetComponent<UmaUIContainer>();
        container.Name = container.name = Path.GetFileName(entry.Name);
        container.FontSize = 20;
        container.Button.onClick.AddListener(() =>
        {
            HighlightChildImage(parent, container);
            (Builder.CurrentUMAContainer)?.LoadAnimation(entry);
            LoadedAnimation();
        });
    }

    string getCharaName(string id)
    {
        var entry = Main.Characters.FirstOrDefault(a => a.Id.ToString().Equals(id));
        return (entry == null) ? id.ToString() : entry.GetName();
    }

    public static string GetCostumeName(string costumeId, string defaultname)
    {
        switch (costumeId)
        {
            // case "00":return "Default";
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
                return (defaultname == "00") ? "Default" : defaultname;
        }
    }

    public void LoadedAnimation()
    {
        var container = Builder.CurrentUMAContainer;
        if (!container || !container.UmaAnimator) return;
        AnimationSettings.ProgressSlider.SetValueWithoutNotify(0);
        // Reset settings by Panel
        container.UmaAnimator.speed = AnimationSettings.SpeedSlider.value;
        Builder.AnimationCameraAnimator.speed = AnimationSettings.SpeedSlider.value;
        if (container.UmaFaceAnimator)
            container.UmaFaceAnimator.speed = AnimationSettings.SpeedSlider.value;
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

    public void ToggleUIFacial(GameObject go)
    {
        if (go.activeSelf || !TogglableFacials.Contains(go))
        {
            go.SetActive(!go.activeSelf);
            return;
        }

        foreach (var panel in TogglableFacials)
        {
            panel.SetActive(panel == go);
        }
    }

    public void RecordVMD()
    {
        var container = Builder.CurrentUMAContainer;
        var camera = Builder.AnimationCamera;
        var buttonText = AnimationSettings.VMDButton.GetComponentInChildren<TextMeshProUGUI>();

        if (!container || container.IsMini)
        {
            buttonText.text = string.Format("<color=#FF0000>{0}</color>", "Need Normal UMA");
            return;
        }

        var rootbone = container.transform.Find("Position");
        if (rootbone.gameObject.TryGetComponent(out UnityHumanoidVMDRecorder recorder))
        {
            if (recorder.IsRecording)
            {
                if (camera.enabled)
                {
                    var cameraRecorder = camera.GetComponent<UnityCameraVMDRecorder>();
                    cameraRecorder.StopRecording();
                    cameraRecorder.SaveVMD();
                }
                recorder.StopRecording();
                buttonText.text = "Saving";
                recorder.SaveVMD(container.name, Config.Instance.VmdKeyReductionLevel);
                buttonText.text = "Record VMD";
            }
        }
        else
        {
            var newRecorder = rootbone.gameObject.AddComponent<UnityHumanoidVMDRecorder>();
            newRecorder.Initialize();
            if (!newRecorder.IsRecording)
            {
                if (camera.enabled)
                {
                    var cameraRecorder = camera.gameObject.AddComponent<UnityCameraVMDRecorder>();
                    cameraRecorder.Initialize();
                    cameraRecorder.StartRecording();
                }
                newRecorder.StartRecording();
                buttonText.text = "Recording...";
            }
        }
    }

    public void UpdateLiveMode(int val)
    {
        LiveMode = val;
        if (LiveSelectPannel.activeSelf && currentLive != null)
        {
            foreach (var select in LiveSelectList.content.GetComponentsInChildren<Transform>())
            {
                if (select != LiveSelectList.content.transform)
                {
                    Destroy(select.gameObject);
                }
            }

            switch (LiveMode)
            {
                case 0:
                    LiveSelectImage.sprite = currentLive.Icon;
                    int mainCount = 0;
                    foreach (var motion in Main.AbMotions.Where(a => a.Name.StartsWith($"3d/motion/live/body/son{currentLive.MusicId}") && Path.GetFileName(a.Name).Split('_').Length == 4))
                    {
                        mainCount += 1;
                    }

                    LiveSelectInfoText.text = $"{currentLive.SongName}\nMain Members Count: {mainCount}";
                    for (int i = 0; i < mainCount; i++)
                    {
                        var chara = Instantiate(LiveSelectPrefab, LiveSelectList.content).GetComponent<LiveCharacterSelect>();
                        chara.IndexText.text = (i + 1).ToString();
                        chara.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            chara.SelectChara(this);
                            foreach (var t in LiveSelectList.content.GetComponentsInChildren<LiveCharacterSelect>())
                            {
                                t.GetComponentInChildren<Text>().color = (t == chara ? Color.white : Color.black);
                            }
                        });
                    }
                    break;

                case 1:
                    LiveSelectImage.sprite = currentLive.Icon;
                    LiveSelectInfoText.text = $"{currentLive.SongName}\nMembers Count: {currentLive.MemberCount}";
                    for (int i = 0; i < currentLive.MemberCount; i++)
                    {
                        var chara = Instantiate(LiveSelectPrefab, LiveSelectList.content).GetComponent<LiveCharacterSelect>();
                        chara.IndexText.text = (i + 1).ToString();
                        chara.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            chara.SelectChara(this);
                            foreach (var t in LiveSelectList.content.GetComponentsInChildren<LiveCharacterSelect>())
                            {
                                t.GetComponentInChildren<Text>().color = (t == chara ? Color.white : Color.black);
                            }
                        });
                    }
                    break;
            }
        }
    }

    public void SetLiveRecordMode(bool val) { isRecordVMD = val; }

    public void SetLiveRequireStage(bool val) { isRequireStage = val; }

    public void ShowMessage(string msg, UIMessageType type)
    {
        if (!MessageText) return;
        MessageText.text += type switch
        {
            UIMessageType.Error => string.Format("<color=red>{0}</color>\n", msg),
            UIMessageType.Warning => string.Format("<color=yellow>{0}</color>\n", msg),
            UIMessageType.Success => string.Format("<color=green>{0}</color>\n", msg),
            UIMessageType.Close => string.Format("<color=green>{0}</color>\n", msg),
            _ => $"{msg}\n",
        };
        MessageScrollRect.gameObject.SetActive(type != UIMessageType.Close);
        MessageScrollRect.verticalNormalizedPosition = 0;
    }

    public void ToggleVisible(GameObject go)
    {
        go.SetActive(!go.activeSelf);
    }
}
