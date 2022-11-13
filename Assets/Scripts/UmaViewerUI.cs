using Gallop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UmaViewerUI : MonoBehaviour
{
    public static UmaViewerUI Instance;
    private UmaViewerMain Main => UmaViewerMain.Instance;
    private UmaViewerBuilder Builder => UmaViewerBuilder.Instance;
    
    public CanvasScaler canvasScaler;

    [Header("normal models")]
    public ScrollRect CharactersList;
    public ScrollRect CostumeList;
    public ScrollRect AnimationSetList;
    public ScrollRect AnimationList;
    [Header("mini models")]
    public ScrollRect MiniCharactersList;
    public ScrollRect MiniCostumeList;
    public ScrollRect MiniAnimationSetList;
    public ScrollRect MiniAnimationList;
    [Header("other")]
    public ScrollRect PropList;
    public ScrollRect SceneList;

    public ScrollRect EmotionList;
    public Transform FacialList;
    public ScrollRect EarList;
    public ScrollRect EyeBrowList;
    public ScrollRect EyeList;
    public ScrollRect MouthList;
    public ScrollRect LiveList;
    public ScrollRect LiveSoundList;
    public ScrollRect LiveCharaSoundList;

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
    public List<GameObject> TogglablePanels = new List<GameObject>();
    public List<GameObject> TogglableFacials = new List<GameObject>();

    public GameObject UmaContainerPrefab;
    public GameObject UmaContainerSliderPrefab;
    public GameObject UmaContainerAssetsPrefab;
    private int LoadedAssetCount = 0;
    [SerializeField] private RectTransform LoadedAssetsPanel;

    public Color UIColor1, UIColor2;

    public bool isCriware = false;
    public bool isHeadFix = false;
    public bool EnableEyeTracking = true;
    public bool IsCheekBlushing = false;

    public FaceDrivenKeyTarget currentFaceDrivenKeyTarget;

    private void Awake()
    {
        Instance = this;
        AudioPlayButton.onClick.AddListener(AudioPause);
        AudioSlider.onValueChanged.AddListener(AudioProgressChange);
        AnimationPlayButton.onClick.AddListener(AnimationPause);
        AnimationSlider.onValueChanged.AddListener(AnimationProgressChange);
        AnimationSpeedSlider.onValueChanged.AddListener(AnimationSpeedChange);
        if (Application.platform == RuntimePlatform.Android)
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
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
                bool isLoop = Builder.CurrentUMAContainer.OverrideController["clip_2"].name.EndsWith("_loop");
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

    public void HighlightChildImage(Transform mainObject, Image child)
    {
        foreach(var t in mainObject.GetComponentsInChildren<Image>())
        {
            if (t.transform.parent != mainObject) continue;
            t.color = t == child ? UIColor2 : UIColor1;
        }
    }

    public void LoadedAssetsAdd(UmaDatabaseEntry entry)
    {
        LoadedAssetCount++;
        string filePath = UmaDatabaseController.GetABPath(entry);
        var container =  Instantiate(UmaContainerAssetsPrefab, LoadedAssetsPanel).GetComponent<UmaUIContainer>();
        container.Name = Path.GetFileName(entry.Name) + "\n" + entry.Url;
        container.Button.onClick.AddListener(() => {Process.Start("explorer.exe", "/select," + filePath);});
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
        var container = Instantiate(UmaContainerPrefab, AnimationSetList.content).GetComponent<UmaUIContainer>();
        container.Name = container.name = "Generic";
        var imageInstance = container.GetComponent<Image>();
        container.Button.onClick.AddListener(() => {
            HighlightChildImage(AnimationSetList.content, imageInstance);
            ListAnimations(-1, false);
        });

        container = Instantiate(UmaContainerPrefab, AnimationSetList.content).GetComponent<UmaUIContainer>();
        container.Name = container.name = "Tail";
        imageInstance = container.GetComponent<Image>();
        container.Button.onClick.AddListener(() => {
            HighlightChildImage(AnimationSetList.content, imageInstance);
            ListAnimations(-2, false);
        });

        foreach (var chara in Main.Characters.OrderBy(c => c.Id))
        {
            var charaInstance = chara;

            container = Instantiate(UmaContainerPrefab, CharactersList.content).GetComponent<UmaUIContainer>();
            container.Name = container.name = chara.Id + " " + chara.Name;
            var imageInstance1 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(CharactersList.content, imageInstance1);
                ListCostumes(charaInstance.Id, false);
            });

            if (chara.Icon)
            {
                container.Image.sprite = chara.Icon;
                container.Image.enabled = true;
            }

            container = Instantiate(UmaContainerPrefab, AnimationSetList.content).GetComponent<UmaUIContainer>();
            container.Name = container.name = chara.Id + " " + chara.Name;
            var imageInstance2 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(AnimationSetList.content, imageInstance2);
                ListAnimations(charaInstance.Id, false);
            });

            if (chara.Icon)
            {
                container.Image.sprite = chara.Icon;
                container.Image.enabled = true;
            }
        }
    }

    public void InstantiateFacialPart(GameObject UmaContainerSliderPrefab, FaceDrivenKeyTarget target, List<FacialMorph> targetMorph, ScrollRect TargetList)
    {
        foreach (FacialMorph morph in targetMorph)
        {
            var container = Instantiate(UmaContainerSliderPrefab, TargetList.content).GetComponent<UmaUIContainer>();
            container.Name = morph.name + " (" + morph.tag + ")";
            container.Slider.value = morph.weight;
            container.Slider.maxValue = 1;
            container.Slider.minValue = morph.tag.Contains("Range") ? -1 : 0;
            container.Slider.onValueChanged.AddListener((a) => { target.ChangeMorphWeight(morph, a); });
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

    }

    public void UpdateFacialPanels()
    {
        LoadFacialPanels(currentFaceDrivenKeyTarget);
    }

    public void LoadEmotionPanels(FaceEmotionKeyTarget target)
    {
        foreach (UmaUIContainer ui in EmotionList.content.GetComponentsInChildren<UmaUIContainer>())
        {
            Destroy(ui.gameObject);
        }

        if (target == null) return;

        foreach(var emotion in target.FaceEmotionKey)
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
            var container = Instantiate(UmaContainerPrefab, LiveList.content).GetComponent<UmaUIContainer>();
            container.Name = " "+ live.MusicId + " " + live.SongName;
            var imageInstance1 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(LiveList.content, imageInstance1);
                Builder.LoadLive(live);
            });

            var CharaContainer = Instantiate(UmaContainerPrefab, LiveSoundList.content).GetComponent<UmaUIContainer>();
            CharaContainer.Name = " " + live.MusicId + " " + live.SongName;
            var CharaimageInstance1 = CharaContainer.GetComponent<Image>();
            CharaContainer.Button.onClick.AddListener(() => {
                HighlightChildImage(LiveSoundList.content, CharaimageInstance1);
                ListLiveSounds(live.MusicId);
            });
        }
    }

    public void LoadMiniModelPanels()
    {
        var container = Instantiate(UmaContainerPrefab, MiniAnimationSetList.content).GetComponent<UmaUIContainer>();
        container.Name = container.name = "General";
        var imageInstance = container.GetComponent<Image>();
        container.Button.onClick.AddListener(() => {
            HighlightChildImage(MiniAnimationSetList.content, imageInstance);
            ListAnimations(-1, true);
        });

        foreach (var chara in Main.Characters.OrderBy(c => c.Id))
        {
            var charaInstance = chara;
            container = Instantiate(UmaContainerPrefab, MiniCharactersList.content).GetComponent<UmaUIContainer>();
            container.Name = container.name = chara.Id + " " + chara.Name;
            var imageInstance1 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(MiniCharactersList.content, imageInstance1);
                ListCostumes(charaInstance.Id, true);
            });

            if (chara.Icon)
            {
                container.Image.sprite = chara.Icon;
                container.Image.enabled = true;
            }

            container = Instantiate(UmaContainerPrefab, MiniAnimationSetList.content).GetComponent<UmaUIContainer>();
            container.Name = container.name = chara.Id + " " + chara.Name;
            var imageInstance2 = container.GetComponent<Image>();
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(MiniAnimationSetList.content, imageInstance2);
                ListAnimations(charaInstance.Id, true);
            });

            if (chara.Icon)
            {
                container.Image.sprite = chara.Icon;
                container.Image.enabled = true;
            }
        }
    }

    public void LoadPropPanel()
    {
        foreach (var prop in Main.AbList.Where(a=>a.Name.Contains("pfb_chr_prop") && !a.Name.Contains("clothes")))
        {
            var propInstance = prop;
            var container = Instantiate(UmaContainerPrefab, PropList.content).GetComponent<UmaUIContainer>();
            container.Name = container.name = Path.GetFileName(prop.Name);
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
            container.Name = container.name = Path.GetFileName(scene.Name);
            container.FontSize = 19;
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
            container.Name = container.name = GetCostumeName(costumeId);
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(costumeList.content, container.GetComponent<Image>());
                StartCoroutine(Builder.LoadUma(umaId, costumeId, mini));
            });
        }
        //Common costumes
        List<string> costumes = new List<string>();
        nameVar = mini ? "pfb_mbdy0" : $"pfb_bdy0";
        foreach (var entry in Main.AbChara.Where(a => a.Name.Contains("/body/") && !a.Name.Contains("/clothes/") && a.Name.Contains(nameVar)))
        {
            string id = Path.GetFileName(entry.Name);
            id = id.Split('_')[1].Substring(mini ? 4 : 3) + "_" + id.Split('_')[2] + "_" + id.Split('_')[3];
            if (!costumes.Contains(id))
            {
                costumes.Add(id);
                string costumeId = id;
                var container = Instantiate(UmaContainerPrefab, costumeList.content).GetComponent<UmaUIContainer>();
                container.Name = container.name = GetCostumeName(id);
                container.Button.onClick.AddListener(() => {
                    HighlightChildImage(costumeList.content, container.GetComponent<Image>());
                    StartCoroutine(Builder.LoadUma(umaId, costumeId, mini));
                });
            }
        }
    }

    public void SetCriWare(bool value)
    {
        isCriware = value;
    }

    public void SetHeadFix(bool value)
    {
        isHeadFix = value;
    }

    public void UpdateGifQualityLabel(float value)
    {
        GifQualityLabel.text = $"Quality: {(int)value} (default: 10)";
    }

    void ListLiveSounds(int songid)
    {
        for (int i = LiveCharaSoundList.content.childCount - 1; i >= 0; i--)
        {
            Destroy(LiveCharaSoundList.content.GetChild(i).gameObject);
        }
        string nameVar = $"snd_bgm_live_{songid}_chara";
        foreach (var entry in Main.AbList.Where(a => a.Name.Contains(nameVar) && a.Name.EndsWith("awb")))
        {
            var container = Instantiate(UmaContainerPrefab, LiveCharaSoundList.content).GetComponent<UmaUIContainer>();
            string[] split = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
            string name = split[split.Length - 2] + getCharaName(split[split.Length - 2]) + " " + split[split.Length - 1];
            container.Name = name;
            container.Button.onClick.AddListener(() => {
                HighlightChildImage(LiveCharaSoundList.content, container.GetComponent<Image>());
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

    string getCharaName(string id)
    {
        var entry = Main.Characters.FirstOrDefault(a => a.Id.ToString().Equals(id));
        return (entry == null) ? id.ToString() : entry.Name;
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

        var filteredList = Main.AbMotions.Where(a => !a.Name.Contains($"mirror")
        && (mini ? a.Name.Contains($"mini") : !a.Name.Contains($"mini"))
        && !a.Name.Contains($"facial") 
        && !a.Name.Contains($"_cam")
        && !a.Name.EndsWith($"_s")
        && !a.Name.EndsWith($"_e")
        );
        
        if (umaId == -1)
        {
            foreach (var entry in filteredList.Where(a=> a.Name.Contains($"/type00") && !a.Name.Contains($"/tail")))
            {
                var entryInstance = entry;
                var container = Instantiate(UmaContainerPrefab, animationList.content).GetComponent<UmaUIContainer>();
                container.Name = container.name = Path.GetFileName(entry.Name);
                container.FontSize = 19;
                container.Button.onClick.AddListener(() => {
                    HighlightChildImage(animationList.content, container.GetComponent<Image>());
                    Builder.LoadAsset(entryInstance);
                    LoadedAnimation();
                });
            }
        }
        else if(umaId == -2)
        {
            foreach (var entry in filteredList.Where(a => a.Name.Contains($"/tail")))
            {
                var entryInstance = entry;
                var container = Instantiate(UmaContainerPrefab, animationList.content).GetComponent<UmaUIContainer>();
                container.Name = container.name = Path.GetFileName(entry.Name);
                container.FontSize = 19;
                container.Button.onClick.AddListener(() => {
                    HighlightChildImage(animationList.content, container.GetComponent<Image>());
                    Builder.LoadAsset(entryInstance);
                    LoadedAnimation();
                });
            }
        }
        else
        {
            //Common animations
            foreach (var entry in filteredList.Where(a => a.Name.Contains($"chara/chr{umaId}") && !a.Name.Contains("pose")))
            {
                var entryInstance = entry;
                var container = Instantiate(UmaContainerPrefab, animationList.content).GetComponent<UmaUIContainer>();
                container.Name = container.name = Path.GetFileName(entry.Name);
                container.FontSize = 19;
                container.Button.onClick.AddListener(() => {
                    HighlightChildImage(animationList.content, container.GetComponent<Image>());
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
                container.FontSize = 19;
                container.Button.onClick.AddListener(() => {
                    HighlightChildImage(animationList.content, container.GetComponent<Image>());
                    Builder.LoadAsset(entryInstance);
                    LoadedAnimation();
                });
            }
        }
    }

    public void LoadedAnimation()
    {
        if (!Builder.CurrentUMAContainer || !Builder.CurrentUMAContainer.UmaAnimator) return;
        AnimationSlider.SetValueWithoutNotify(0);
        // Reset settings by Panel
        Builder.CurrentUMAContainer.UmaAnimator.speed = AnimationSpeedSlider.value;
        Builder.PreviewCameraAnimator.speed = AnimationSpeedSlider.value;
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

    public void SetDynamicBoneEnable(bool isOn)
    {
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

    public void SetBlushing(bool isOn)
    {
        IsCheekBlushing = isOn;
        if (Builder.CurrentUMAContainer)
        {
            if (Builder.CurrentUMAContainer.CheekMaterial)
            {
                if (isOn)
                {
                    Builder.CurrentUMAContainer.CheekMaterial.SetTexture("_MainTex", Builder.CurrentUMAContainer.CheekTex);
                }
                else
                {
                    Builder.CurrentUMAContainer.CheekMaterial.SetTexture("_MainTex", null);
                } 
            }
        }
    }

    public void AudioPause()
    {
        if (Builder.CurrentAudioSources.Count>0)
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
        if (Builder.CurrentAudioSources.Count>0)
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
            var animator_cam = Builder.PreviewCameraAnimator;
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
        var animator_cam = Builder.PreviewCameraAnimator;
        if (animator != null)
        {
            var AnimeClip = Builder.CurrentUMAContainer.OverrideController["clip_2"];
            
            // Pause and Seek;
            animator.speed = 0;
            animator.Play(0, 0, val);
            animator.Play(0, 2, val);
            animator_cam.speed = 0;
            animator_cam.Play(0, -1, val);
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
        AnimationSpeedText.text = string.Format("Animation Speed: {0:F2}", val);
        if (!Builder.CurrentUMAContainer || !Builder.CurrentUMAContainer.UmaAnimator) return;
        Builder.CurrentUMAContainer.UmaAnimator.speed = val;
        Builder.PreviewCameraAnimator.speed = val;
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
        for(int i = Builder.CurrentLyrics.Count-1; i >=0; i--)
        {
            if(Builder.CurrentLyrics[i].time< time)
            {
                return Builder.CurrentLyrics[i].text;
            }
        }
        return "";
    }

    public void RecordVMD()
    {
        var buttonText = VMDButton.GetComponentInChildren<TextMeshProUGUI>();
        if (!UmaViewerBuilder.Instance.CurrentUMAContainer|| UmaViewerBuilder.Instance.CurrentUMAContainer.IsMini)
        {
            buttonText.text = string.Format("<color=#FF0000>{0}</color>", "Need Normal UMA");
            return;
        }

        var container = UmaViewerBuilder.Instance.CurrentUMAContainer;
        var camera = UmaViewerBuilder.Instance.PreviewCamera;

        var rootbone = container.transform.Find("Position");
        if(rootbone.gameObject.TryGetComponent(out UnityHumanoidVMDRecorder recorder))
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
}
