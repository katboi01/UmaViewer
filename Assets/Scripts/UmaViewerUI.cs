using Gallop;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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
    public ScrollRect MaterialsList;
    public PageManager ScenePageCtrl;
    public ScrollRect MessageScrollRect;
    public Text MessageText;

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


    [Header("audio")]
    public Slider AudioSlider;
    public Button AudioPlayButton;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI ProgressText;
    public Text LyricsText;

    [Header("animations")]
    public Slider AnimationSlider;
    public Slider AnimationSpeedSlider;
    public Button AnimationPlayButton;
    public TextMeshProUGUI AnimationTitleText;
    public TextMeshProUGUI AnimationSpeedText;
    public TextMeshProUGUI AnimationProgressText;

    [Header("backgrounds")]
    public ScrollRect BackGroundList;
    public PageManager BackGroundPageCtrl;
    public GameObject BG_Canvas;
    public GameObject BG_HSVPickerObj;
    public Image BG_Image;

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
    public TMP_InputField SSWidth;
    public TMP_InputField SSHeight;
    public Toggle SSTransparent;
    public TMP_InputField GifWidth;
    public TMP_InputField GifHeight;
    public Toggle GifTransparent;
    public Slider GifQuality;
    public TextMeshProUGUI GifQualityLabel;
    public Slider GifSlider;
    public Button GifButton;
    public Button VMDButton;
    public Button UpdateDBButton;
    public TMP_Dropdown WorkModeDropdown;
    public TMP_Dropdown LanguageDropdown;
    public List<GameObject> TogglablePanels = new List<GameObject>();
    public List<GameObject> TogglableFacials = new List<GameObject>();

    [Header("prefabs")]
    public GameObject UmaContainerPrefab;
    public GameObject UmaContainerCostumePrefab;
    public GameObject UmaContainerLivePrefab;
    public GameObject UmaContainerSliderPrefab;
    public GameObject UmaContainerAssetsPrefab;
    public UmaUIContainer UmaContainerTogglePrefab;

    private int LoadedAssetCount = 0;
    [SerializeField] private RectTransform LoadedAssetsPanel;

    public Color UIColor1, UIColor2;

    public bool isCriware = false;
    public bool isHeadFix = false;
    public bool isTPose = false;
    public bool DynamicBoneEnable = true;
    public bool EnableEyeTracking = true;
    public bool EnableFaceOverride = true;

    public bool LiveTime = false;

    public FaceDrivenKeyTarget currentFaceDrivenKeyTarget;

    private IEnumerator UpdateResVerCoroutine;

    private void Awake()
    {
        Instance = this;
        AudioPlayButton.onClick.AddListener(AudioPause);
        AudioSlider.onValueChanged.AddListener(AudioProgressChange);
        AnimationPlayButton.onClick.AddListener(AnimationPause);
        AnimationSlider.onValueChanged.AddListener(AnimationProgressChange);
        AnimationSpeedSlider.onValueChanged.AddListener(AnimationSpeedChange);
    }
    private void Start()
    {
        WorkModeDropdown.SetValueWithoutNotify((int)Config.Instance.WorkMode);
        LanguageDropdown.SetValueWithoutNotify((int)Config.Instance.Language);
        UpdateDBButton.interactable = (Config.Instance.WorkMode == WorkMode.Standalone);
    }

    private void Update()
    {
        if (Builder.CurrentAudioSources.Count > 0)
        {
            AudioSource MianSource = Builder.CurrentAudioSources[0];
            if (MianSource.clip)
            {
                TitleText.text = MianSource.clip.name;
                ProgressText.text = string.Format("{0} / {1}", ToTimeFormat(MianSource.time), ToTimeFormat(MianSource.clip.length));
                AudioSlider.SetValueWithoutNotify(MianSource.time / MianSource.clip.length);
                LyricsText.text = GetCurrentLyrics(MianSource.time);
            }
        }

        if (Builder.CurrentUMAContainer != null && Builder.CurrentUMAContainer.OverrideController != null)
        {
            if (Builder.CurrentUMAContainer.OverrideController["clip_2"].name != "clip_2")
            {
                bool isLoop = Builder.CurrentUMAContainer.OverrideController["clip_2"].name.Contains("_loop");
                var AnimeState = Builder.CurrentUMAContainer.UmaAnimator.GetCurrentAnimatorStateInfo(0);
                var AnimeClip = Builder.CurrentUMAContainer.OverrideController["clip_2"];
                if (AnimeClip && Builder.CurrentUMAContainer.UmaAnimator.speed != 0)
                {
                    var normalizedTime = (isLoop) ? Mathf.Repeat(AnimeState.normalizedTime, 1) : Mathf.Min(AnimeState.normalizedTime, 1);
                    AnimationTitleText.text = AnimeClip.name;
                    AnimationProgressText.text = string.Format("{0} / {1}", ToFrameFormat(normalizedTime * AnimeClip.length, AnimeClip.frameRate), ToFrameFormat(AnimeClip.length, AnimeClip.frameRate));
                    AnimationSlider.SetValueWithoutNotify(normalizedTime);
                }
            }
        }
    }

    public void HighlightChildImage(Transform mainObject, UmaUIContainer child)
    {
        foreach (var t in mainObject.GetComponentsInChildren<UmaUIContainer>())
        {
            if (t.transform.parent != mainObject) continue;
            t.ToggleImage.enabled = (t == child);
        }
    }

    public void LoadedAssetsAdd(UmaDatabaseEntry entry)
    {
        if (!LoadedAssetsPanel) return;
        foreach (UmaUIContainer ui in LoadedAssetsPanel.GetComponentsInChildren<UmaUIContainer>())
        {
            if (ui.Name == Path.GetFileName(entry.Name)) return;
        }

        LoadedAssetCount++;
        string filePath = entry.FilePath;
        var container = Instantiate(UmaContainerAssetsPrefab, LoadedAssetsPanel).GetComponent<UmaUIContainer>();
        container.Name = Path.GetFileName(entry.Name);
        container.Button.onClick.AddListener(() => { Process.Start("explorer.exe", "/select," + filePath); });
        LoadedAssetsPanel.sizeDelta = new Vector2(0, LoadedAssetCount * 35);
    }

    public void LoadedAssetsClear()
    {
        LoadedAssetCount = 0;
        foreach (UmaUIContainer ui in LoadedAssetsPanel.GetComponentsInChildren<UmaUIContainer>())
        {
            Destroy(ui.gameObject);
        }
        LoadedAssetsPanel.sizeDelta = Vector2.zero;
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
            container3.Name = container3.name = chara.Id + " " + chara.Name;
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
            container4.Name = container4.name = chara.Id + " " + chara.Name;
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

            pageentry.Name = chara.Name;
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
            container2.Name = container2.name = chara.Id + " " + chara.Name;
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
            container3.Name = container3.name = chara.Id + " " + chara.Name;
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

    public void InstantiateFacialPart(GameObject UmaContainerSliderPrefab, FaceDrivenKeyTarget target, List<FacialMorph> targetMorph, ScrollRect TargetList)
    {
        foreach (FacialMorph morph in targetMorph)
        {
            if (morph is FacialOtherMorph extraMorph)
            {
                foreach (var prop in extraMorph.BindProperties)
                {
                    var container = Instantiate(UmaContainerSliderPrefab, TargetList.content).GetComponent<UmaUIContainer>();
                    Debug.Log(morph.name);
                    container.Name = $"{morph.locator.name}({prop.Type})";
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
                            container.Slider.maxValue = prop.BindPrefab.Count - 1;
                            container.Slider.minValue = 0;
                            break;
                        case BindProperty.BindType.EyeSelect:
                            container.Slider.wholeNumbers = true;
                            container.Slider.maxValue = prop.BindPrefab.Count / 2 - 1;
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
    }

    public void LoadFacialPanels(FaceDrivenKeyTarget target)
    {
        currentFaceDrivenKeyTarget = target;

        foreach (UmaUIContainer ui in FacialList.GetComponentsInChildren<UmaUIContainer>(true))
        {
            Destroy(ui.gameObject);
        }

        if (target == null) return;

        InstantiateFacialPart(UmaContainerSliderPrefab, target, target.EarMorphs, EarList);
        InstantiateFacialPart(UmaContainerSliderPrefab, target, target.EyeBrowMorphs, EyeBrowList);
        InstantiateFacialPart(UmaContainerSliderPrefab, target, target.EyeMorphs, EyeList);
        InstantiateFacialPart(UmaContainerSliderPrefab, target, target.MouthMorphs, MouthList);

        List<FacialMorph> targetMorph = new List<FacialMorph>(target.OtherMorphs);
        InstantiateFacialPart(UmaContainerSliderPrefab, target, targetMorph, OtherList);
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
            container.Slider.value = emotion.Weight;
            container.Slider.maxValue = 1;
            container.Slider.minValue = 0;
            container.Slider.onValueChanged.AddListener((a) => { target.ChangeEmotionWeight(emotion, a); });
        }
    }

    public void LoadLivePanels()
    {
        foreach (var live in Main.Lives.OrderBy(c => c.MusicId))
        {
            var liveInstance = live;
            var container = Instantiate(UmaContainerLivePrefab, LiveList.content).GetComponent<UmaUIContainer>();
            container.Name = " " + live.MusicId + " " + live.SongName;
            container.Image.sprite = live.Icon;
            container.Image.enabled = container.Image.sprite;
            container.Button.onClick.AddListener(() =>
            {
                HighlightChildImage(LiveList.content, container);
                ShowLiveSelectPannel(live);
            });

            var CharaContainer = Instantiate(UmaContainerLivePrefab, LiveSoundList.content).GetComponent<UmaUIContainer>();
            CharaContainer.Name = " " + live.MusicId + " " + live.SongName;
            CharaContainer.Image.sprite = live.Icon;
            CharaContainer.Image.enabled = CharaContainer.Image.sprite;
            CharaContainer.Button.onClick.AddListener(() =>
            {
                HighlightChildImage(LiveSoundList.content, container);
                ListLiveSounds(live.MusicId);
            });
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
        foreach (var prop in Main.AbList.Where(a => a.Name.Contains("pfb_chr_prop") && !a.Name.Contains("clothes")))
        {
            var pageentry = new PageManager.Entry();
            pageentry.Name = Path.GetFileName(prop.Name);
            pageentry.OnClick = (container) =>
            {
                HighlightChildImage(PropList.content, container);
                Builder.LoadProp(prop);
            };
            pageentrys.Add(pageentry);
        }
        PropPageCtrl.Initialize(pageentrys, PropList);
    }

    public void LoadMapPanel()
    {
        var pageentrys = new List<PageManager.Entry>();
        foreach (var scene in Main.AbList.Where(a => ((a.Name.StartsWith("3d/env") && Path.GetFileName(a.Name).StartsWith("pfb_")) || a.Name.StartsWith("cutt/cutt_son") && Path.GetFileName(a.Name).StartsWith("cutt_son"))))
        {
            var pageentry = new PageManager.Entry();
            pageentry.Name = Path.GetFileName(scene.Name);
            pageentry.FontSize = 19;
            pageentry.OnClick = (container) =>
            {
                HighlightChildImage(SceneList.content, container);
                Builder.LoadProp(scene);
            };
            pageentrys.Add(pageentry);
        }
        ScenePageCtrl.Initialize(pageentrys, SceneList);
    }

    public void PlayLive()
    {
        if (currentLive == null) return;
        var selectlist = LiveSelectList.content.GetComponentsInChildren<LiveCharacterSelect>();
        if (selectlist != null)
        {
            LiveTime = true;
            SetEyeTrackingEnable(false);
            Builder.LoadLive(currentLive, new List<LiveCharacterSelect>(selectlist));
            LiveSelectPannel.SetActive(false);
        }
    }

    void ShowLiveSelectPannel(LiveEntry entry)
    {
        LiveSelectPannel.SetActive(true);
        for (int i = LiveSelectList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(LiveSelectList.content.GetChild(i).gameObject);
        }

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
                    t.GetComponentInChildren<Text>().color = (t == chara ? Color.white : Color.black);
                }
            });
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
            foreach (var entry in Main.AbList.Where(a => !a.Name.Contains("clothes") && a.Name.Contains(nameVar)))
            {
                var container = Instantiate(UmaContainerCostumePrefab, costumeList.content).GetComponent<UmaUIContainer>();
                string[] split = entry.Name.Split('_');
                string costumeId = split[split.Length - 1];
                var dressdata = Main.Costumes.FirstOrDefault(a => (a.CharaId == achara.Id && a.BodyTypeSub == int.Parse(costumeId)));
                container.Name = container.name = GetCostumeName(costumeId, (dressdata == null ? costumeId : dressdata.DressName));
                container.Image.sprite = (dressdata == null ? CostumeIconDefault : dressdata.Icon);
                container.Image.enabled = true;
                container.Button.onClick.AddListener(() =>
                {
                    if (LiveSelectPannel.activeInHierarchy && CurrentSeletChara)
                    {
                        CurrentSeletChara.SetValue(achara, costumeId, container.Image.sprite);
                    }
                    else
                    {
                        HighlightChildImage(costumeList.content, container);
                        StartCoroutine(Builder.LoadUma(achara, costumeId, mini));
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
        foreach (var entry in Main.AbChara.Where(a => a.Name.Contains("/body/") && !a.Name.Contains("/clothes/") && a.Name.Contains(nameVar)))
        {
            string id = Path.GetFileName(entry.Name);
            id = id.Split('_')[1].Substring(mini ? 4 : 3) + "_" + id.Split('_')[2] + "_" + id.Split('_')[3];
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
                container.Button.onClick.AddListener(() =>
                {
                    if (LiveSelectPannel.activeInHierarchy && CurrentSeletChara)
                    {
                        CurrentSeletChara.SetValue(chara, costumeId, container.Image.sprite);
                    }
                    else
                    {
                        HighlightChildImage(costumeList.content, container);
                        StartCoroutine(Builder.LoadUma(chara, costumeId, mini));
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
                if (isCriware)
                {
                    Builder.LoadLiveSoundCri(songid, entry);
                }
                else
                {
                    Builder.LoadLiveSound(songid, entry);
                }
            });
        }

    }

    void ListNormalSubSounds(UmaDatabaseEntry awb)
    {
        for (int i = NormalSubSoundList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(NormalSubSoundList.content.GetChild(i).gameObject);
        }

        var subSounds = Builder.LoadAudio(awb);

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
                    Builder.LoadAsset(entry);
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
                var entryInstance = entry;
                var container = Instantiate(UmaContainerPrefab, animationList.content).GetComponent<UmaUIContainer>();
                container.Name = container.name = Path.GetFileName(entry.Name);
                container.FontSize = 20;
                container.Button.onClick.AddListener(() =>
                {
                    HighlightChildImage(animationList.content, container);
                    Builder.LoadAsset(entryInstance);
                    LoadedAnimation();
                });
            }
            pageManager.ResetCtrl();
        }
        else if (umaId == -3)
        {
            foreach (var entry in Main.AbMotions.Where(a => a.Name.Contains($"type00_ear") && !a.Name.EndsWith("driven") && !a.Name.Contains("touch")))
            {
                var entryInstance = entry;
                var container = Instantiate(UmaContainerPrefab, animationList.content).GetComponent<UmaUIContainer>();
                container.Name = container.name = Path.GetFileName(entry.Name);
                container.FontSize = 20;
                container.Button.onClick.AddListener(() =>
                {
                    HighlightChildImage(animationList.content, container);
                    Builder.LoadAsset(entryInstance);
                    LoadedAnimation();
                });
            }
            pageManager.ResetCtrl();
        }
        else
        {
            //Common animations
            foreach (var entry in filteredList.Where(a => a.Name.Contains($"chara/chr{umaId}") && !a.Name.Contains("pose")))
            {
                var entryInstance = entry;
                var container = Instantiate(UmaContainerPrefab, animationList.content).GetComponent<UmaUIContainer>();
                container.Name = container.name = Path.GetFileName(entry.Name);
                container.FontSize = 20;
                container.Button.onClick.AddListener(() =>
                {
                    HighlightChildImage(animationList.content, container);
                    Builder.LoadAsset(entryInstance);
                    LoadedAnimation();
                });
            }

            //Skill animations
            foreach (var entry in filteredList.Where(a => a.Name.Contains($"card/body/crd{umaId}")))
            {
                var entryInstance = entry;
                var container = Instantiate(UmaContainerPrefab, animationList.content).GetComponent<UmaUIContainer>();
                container.Name = container.name = Path.GetFileName(entry.Name);
                container.FontSize = 20;
                container.Button.onClick.AddListener(() =>
                {
                    HighlightChildImage(animationList.content, container);
                    Builder.LoadAsset(entryInstance);
                    LoadedAnimation();
                });
            }
            pageManager.ResetCtrl();
        }
    }

    void ListBackgrounds()
    {
        var bglist = Main.AbList.Where(a => a.Name.StartsWith("bg/bg"));
        var pageentrys = new List<PageManager.Entry>();
        foreach (var entry in bglist)
        {
            var pageentry = new PageManager.Entry();
            pageentry.Name = Path.GetFileName(entry.Name);
            pageentry.Sprite = Builder.LoadSprite(entry);
            if (pageentry.Sprite == null) continue;
            pageentry.OnClick = (container) =>
            {
                HighlightChildImage(BackGroundList.content, container);
                BG_Image.sprite = pageentry.Sprite;
                BG_Image.SetVerticesDirty();
            };

            if (BG_Image.sprite == null)
            {
                BG_Image.sprite = pageentry.Sprite;
                BG_Image.SetVerticesDirty();
            }
            pageentrys.Add(pageentry);
        }
        BackGroundPageCtrl.Initialize(pageentrys, BackGroundList);
    }

    public void SetCriWare(bool value)
    {
        isCriware = value;
    }

    public void SetHeadFix(bool value)
    {
        isHeadFix = value;
    }

    public void SetTPose(bool value)
    {
        isTPose = value;
    }

    public void UpdateGifQualityLabel(float value)
    {
        GifQualityLabel.text = $"Quality: {(int)value} (default: 10)";
    }

    string getCharaName(string id)
    {
        var entry = Main.Characters.FirstOrDefault(a => a.Id.ToString().Equals(id));
        return (entry == null) ? id.ToString() : entry.Name;
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
        if (!Builder.CurrentUMAContainer || !Builder.CurrentUMAContainer.UmaAnimator) return;
        AnimationSlider.SetValueWithoutNotify(0);
        // Reset settings by Panel
        Builder.CurrentUMAContainer.UmaAnimator.speed = AnimationSpeedSlider.value;
        Builder.AnimationCameraAnimator.speed = AnimationSpeedSlider.value;
        if (Builder.CurrentUMAContainer.UmaFaceAnimator)
            Builder.CurrentUMAContainer.UmaFaceAnimator.speed = AnimationSpeedSlider.value;
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

    public void ChangeBackground(int index)
    {
        BackGroundPageCtrl.ResetCtrl();
        switch (index)
        {
            case 0:
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                break;
            case 1:
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                break;
            case 2:
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                ListBackgrounds();
                break;
            default:
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                break;
        }

        BG_HSVPickerObj.SetActive(index != 2);
        BG_Canvas.SetActive(index == 2);
        BackGroundPageCtrl.transform.parent.gameObject.SetActive(index == 2);
    }

    public void ChangeBackgroundColor(Color color)
    {
        Camera.main.backgroundColor = color;
    }

    public void SetDynamicBoneEnable(bool isOn)
    {
        DynamicBoneEnable = isOn;
        if (Builder.CurrentUMAContainer)
        {
            Builder.CurrentUMAContainer.SetDynamicBoneEnable(isOn);
        }
    }

    public void SetEyeTrackingEnable(bool isOn)
    {
        EnableEyeTracking = isOn;
        if (Builder.CurrentUMAContainer)
        {
            Builder.CurrentUMAContainer.EnableEyeTracking = isOn;
        }
    }

    public void SetFaceOverrideEnable(bool isOn)
    {
        EnableFaceOverride = isOn;
        if (Builder.CurrentUMAContainer && Builder.CurrentUMAContainer.FaceOverrideData)
        {
            Builder.CurrentUMAContainer.FaceOverrideData.Enable = isOn;
        }
    }

    public void AudioPause()
    {
        if (Builder.CurrentAudioSources.Count > 0)
        {
            AudioSource MainSource = Builder.CurrentAudioSources[0];
            var state = MainSource.isPlaying;
            foreach (AudioSource source in Builder.CurrentAudioSources)
            {
                if (state)
                {
                    source.Pause();
                }
                else if (source.clip)
                {
                    source.Play();
                }
                else
                {
                    source.Stop();
                }
            }
        }
    }

    public void AudioProgressChange(float val)
    {
        if (Builder.CurrentAudioSources.Count > 0)
        {
            foreach (AudioSource source in Builder.CurrentAudioSources)
            {
                if (source.clip)
                {
                    source.time = source.clip.length * val;
                }
            }
        }
    }

    public void AnimationPause()
    {
        if (!Builder.CurrentUMAContainer || !Builder.CurrentUMAContainer.UmaAnimator) return;
        if (Builder.OverrideController.animationClips.Length > 0)
        {
            var animator = Builder.CurrentUMAContainer.UmaAnimator;
            var animator_face = Builder.CurrentUMAContainer.UmaFaceAnimator;
            var animator_cam = Builder.AnimationCameraAnimator;
            var AnimeState = animator.GetCurrentAnimatorStateInfo(0);
            var state = animator.speed > 0f;
            if (state)
            {
                animator.speed = 0;
                if (animator_face)
                    animator_face.speed = 0;
                animator_cam.speed = 0;
            }
            else if (AnimeState.normalizedTime < 1f)
            {
                animator.speed = AnimationSpeedSlider.value;
                animator_cam.speed = AnimationSpeedSlider.value;
                if (animator_face)
                    animator_face.speed = AnimationSpeedSlider.value;
            }
            else
            {
                animator.speed = AnimationSpeedSlider.value;
                animator.Play(0, 0, 0);
                animator.Play(0, 2, 0);
                animator_cam.speed = AnimationSpeedSlider.value;
                animator_cam.Play(0, -1, 0);
                if (animator_face)
                {
                    animator_face.speed = AnimationSpeedSlider.value;
                    animator_face.Play(0, 0, 0);
                    animator_face.Play(0, 1, 0);
                }
            }

        }
    }

    public void AnimationProgressChange(float val)
    {
        if (!Builder.CurrentUMAContainer) return;
        var animator = Builder.CurrentUMAContainer.UmaAnimator;
        var animator_face = Builder.CurrentUMAContainer.UmaFaceAnimator;
        var animator_cam = Builder.AnimationCameraAnimator;
        if (animator != null)
        {
            var AnimeClip = Builder.CurrentUMAContainer.OverrideController["clip_2"];

            // Pause and Seek;
            animator.speed = 0;
            animator.Play(0, 0, val);
            animator.Play(0, 2, val);
            if (animator_cam.runtimeAnimatorController)
            {
                animator_cam.speed = 0;
                animator_cam.Play(0, -1, val);
            }
            if (animator_face)
            {
                animator_face.speed = 0;
                animator_face.Play(0, 0, val);
                animator_face.Play(0, 1, val);
            }

            AnimationProgressText.text = string.Format("{0} / {1}", ToFrameFormat(val * AnimeClip.length, AnimeClip.frameRate), ToFrameFormat(AnimeClip.length, AnimeClip.frameRate));
        }
    }

    public void AnimationSpeedChange(float val)
    {
        AnimationSpeedText.text = string.Format("Speed: {0:F2}", val);
        if (!Builder.CurrentUMAContainer || !Builder.CurrentUMAContainer.UmaAnimator) return;
        Builder.CurrentUMAContainer.UmaAnimator.speed = val;
        Builder.AnimationCameraAnimator.speed = val;
        if (Builder.CurrentUMAContainer.UmaFaceAnimator)
        {
            Builder.CurrentUMAContainer.UmaFaceAnimator.speed = val;
        }
    }

    public void ResetAudioPlayer()
    {
        TitleText.text = "No Audio";
        ProgressText.text = "00:00:00 / 00:00:00";
        AudioSlider.SetValueWithoutNotify(0);
        LyricsText.text = "";
    }

    public static string ToTimeFormat(float time)
    {

        int seconds = (int)time;
        int hour = seconds / 3600;
        int minute = seconds % 3600 / 60;
        seconds = seconds % 3600 % 60;
        return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, minute, seconds);
    }

    public static string ToFrameFormat(float time, float frameRate)
    {
        int frames = Mathf.FloorToInt(time % 1 * frameRate);
        int seconds = (int)time;
        int minute = seconds % 3600 / 60;
        seconds = seconds % 3600 % 60;
        return string.Format("{0:D2}m:{1:D2}s:{2:D2}f", minute, seconds, frames);
    }

    public string GetCurrentLyrics(float time)
    {
        for (int i = Builder.CurrentLyrics.Count - 1; i >= 0; i--)
        {
            if (Builder.CurrentLyrics[i].time < time)
            {
                return Builder.CurrentLyrics[i].text;
            }
        }
        return "";
    }

    public void RecordVMD()
    {
        var buttonText = VMDButton.GetComponentInChildren<TextMeshProUGUI>();
        if (!UmaViewerBuilder.Instance.CurrentUMAContainer || UmaViewerBuilder.Instance.CurrentUMAContainer.IsMini)
        {
            buttonText.text = string.Format("<color=#FF0000>{0}</color>", "Need Normal UMA");
            return;
        }

        var container = UmaViewerBuilder.Instance.CurrentUMAContainer;
        var camera = UmaViewerBuilder.Instance.AnimationCamera;

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
                recorder.SaveVMD(container.name);
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

    public void UpdateGameDB()
    {
        if (UpdateResVerCoroutine != null && Config.Instance.WorkMode != WorkMode.Standalone) return;
        UmaDatabaseController.Instance.CloseAllConnection();
        ManifestDB dB = new ManifestDB();
        UpdateResVerCoroutine = dB.UpdateResourceVersion(delegate (string msg, UIMessageType type) { ShowMessage(msg, type); });
        StartCoroutine(UpdateResVerCoroutine);
    }

    public void ChangeLanguage(int lang)
    {
        if ((int)Config.Instance.Language != lang)
        {
            Config.Instance.Language = (Language)lang;
            Config.Instance.UpdateConfig();
        }
    }

    public void ChangeWorkMode(int mode)
    {
        if ((int)Config.Instance.WorkMode != mode)
        {
            Config.Instance.WorkMode = (WorkMode)mode;
            Config.Instance.UpdateConfig();
        }
    }

    public void ChangeDataPath()
    {
        var path = StandaloneFileBrowser.OpenFolderPanel("Select Folder", Config.Instance.MainPath, false);
        if (path != null && path.Length > 0 && !string.IsNullOrEmpty(path[0]))
        {
            if (path[0] != Config.Instance.MainPath)
            {
                Config.Instance.MainPath = path[0];
                Config.Instance.UpdateConfig();
            }
        }
    }

    public void ChangeOutlineWidth(float val)
    {
        Shader.SetGlobalFloat("_GlobalOutlineWidth", val);
    }

    public void ShowMessage(string msg, UIMessageType type)
    {
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

    public void ExportModel()
    {
        if (Builder.CurrentUMAContainer)
        {
            var container = Builder.CurrentUMAContainer;
            var path = StandaloneFileBrowser.SaveFilePanel("Save PMX File", Config.Instance.MainPath, container.gameObject.name, "pmx");
            if (!string.IsNullOrEmpty(path))
            {
                ModelExporter.exportModel(path,container.gameObject);
                TextureExporter.exportAllTexture(Path.GetDirectoryName(path),container.gameObject);
            }
        }
    }
}
