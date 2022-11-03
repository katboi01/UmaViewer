using CriWare;
using CriWareFormats;
using Gallop;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UmaMusumeAudio;
using UnityEngine;
using static UmaViewerMain;

public class UmaViewerBuilder : MonoBehaviour
{
    public static UmaViewerBuilder Instance;
    UmaViewerMain Main => UmaViewerMain.Instance;
    UmaViewerUI UI => UmaViewerUI.Instance;

    public List<AssetBundle> Loaded;
    public List<Shader> ShaderList = new List<Shader>();
    public Material TransMaterialCharas;
    public Material TransMaterialProps;
    public UmaContainer CurrentUMAContainer;
    public UmaContainer CurrentLiveContainer;
    public UmaContainer CurrentOtherContainer;

    public UmaHeadData CurrentHead;

    public Shader hairShader;
    public Shader faceShader;
    public Shader eyeShader;
    public Shader cheekShader;
    public Shader eyebrowShader;
    public Shader alphaShader;

    public List<AudioSource> CurrentAudioSources = new List<AudioSource>();
    public List<UmaLyricsData> CurrentLyrics = new List<UmaLyricsData>();

    public AnimatorOverrideController OverrideController;
    public AnimatorOverrideController FaceOverrideController;
    public AnimatorOverrideController CameraOverrideController;
    public Animator CurrentCameraAnimator;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator LoadUma(int id, string costumeId, bool mini)
    {
        UnloadUma();
        CurrentUMAContainer = new GameObject($"Chara_{id}_{costumeId}").AddComponent<UmaContainer>();

        UnloadAllBundle();

        yield return UmaViewerDownload.DownloadText($"https://www.tracenacademy.com/api/CharaData/{id}", txt =>
        {
            Debug.Log(txt);
            CurrentUMAContainer.CharaData = JObject.Parse(txt);
            if (mini)
            {
                LoadMiniUma(id, costumeId);
            }
            else
            {
                LoadNormalUma(id, costumeId);
            }
        });
    }

    private void LoadNormalUma(int id, string costumeId)
    {
        JObject charaData = CurrentUMAContainer.CharaData;
        bool genericCostume = CurrentUMAContainer.IsGeneric = costumeId.Length >= 4;
        string skin = (string)charaData["skin"],
               height = (string)charaData["height"],
               socks = (string)charaData["socks"],
               bust = (string)charaData["bust"],
               sex = (string)charaData["sex"],
               shape = (string)charaData["shape"],
               costumeIdShort = "";

        UmaDatabaseEntry asset = null;
        if (genericCostume)
        {
            costumeIdShort = costumeId.Remove(costumeId.LastIndexOf('_'));
            CurrentUMAContainer.VarCostumeIdShort = costumeIdShort;
            CurrentUMAContainer.VarCostumeIdLong = costumeId;
            CurrentUMAContainer.VarBust = bust;
            CurrentUMAContainer.VarSkin = skin;
            CurrentUMAContainer.VarSocks = socks;
            CurrentUMAContainer.VarHeight = height;

            // Pattern for generic body type is as follows:
            //
            // (costume id)_(body_type_sub)_(body_setting)_(height)_(shape)_(bust)
            //
            // body_type_sub is used for variants like the summer/winter uniform or the swimsuit/towel
            // body_setting is used for subvariants of each variant like the big belly version of the uniform, and the genders for the tracksuits
            //
            // Some models will naturally be missing due to how this system is designed.

            string body = "";
            body = UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/pfb_bdy{costumeId}_{height}_{shape}_{bust}";

            Debug.Log("Looking for " + body);
            asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == body);
        }
        else asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == UmaDatabaseController.BodyPath + $"bdy{id}_{costumeId}/pfb_bdy{id}_{costumeId}");

        if (asset == null)
        {
            Debug.LogError("No body, can't load!");
            return;
        }

        else if (genericCostume)
        {
            string texPattern1 = "", texPattern2 = "", texPattern3 = "", texPattern4 = "", texPattern5 = "";
            switch (costumeId.Split('_')[0])
            {
                case "0001":
                    texPattern1 = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}_0{socks}";
                    texPattern2 = $"tex_bdy{costumeIdShort}_00_0_{bust}";
                    texPattern3 = $"tex_bdy{costumeIdShort}_zekken";
                    texPattern4 = $"tex_bdy{costumeIdShort}_00_waku";
                    texPattern5 = $"tex_bdy{costumeIdShort}_num";
                    break;
                case "0003":
                    texPattern1 = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}";
                    texPattern2 = $"tex_bdy{costumeIdShort}_00_0_{bust}";
                    break;
                case "0006": //last var is color?
                    texPattern1 = $"tex_bdy{costumeId}_{skin}_{bust}_0{0}";
                    texPattern2 = $"tex_bdy{costumeId}_0_{bust}_00_";
                    break;
                default:
                    texPattern1 = $"tex_bdy{costumeId}_{skin}_{bust}";
                    texPattern2 = $"tex_bdy{costumeId}_0_{bust}";
                    break;
            }
            Debug.Log(texPattern1 + " " + texPattern2);
            //Load Body Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith(UmaDatabaseController.BodyPath)
                && (a.Name.Contains(texPattern1)
                || a.Name.Contains(texPattern2)
                || (string.IsNullOrEmpty(texPattern3) ? false : a.Name.Contains(texPattern3))
                || (string.IsNullOrEmpty(texPattern4) ? false : a.Name.Contains(texPattern4))
                || (string.IsNullOrEmpty(texPattern5) ? false : a.Name.Contains(texPattern5)))))
            {
                RecursiveLoadAsset(asset1);
            }
            //Load Body
            RecursiveLoadAsset(asset);

            //Load Physics
            foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith(UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes")))
            {
                if (asset1.Name.Contains("cloth00") && asset1.Name.Contains("bust" + bust))
                    RecursiveLoadAsset(asset1);
            }
        }
        else
        {
            RecursiveLoadAsset(asset);

            //Load Physics
            foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith(UmaDatabaseController.BodyPath + $"bdy{id}_{costumeId}/clothes")))
            {
                if (asset1.Name.Contains("cloth00"))
                    RecursiveLoadAsset(asset1);
            }
        }


        // Record Head Data
        int head_id;
        string head_costumeId;
        int tailId = (int)charaData["tailModelId"];

        //The tailId of 9006 is 1, but the character has no tail
        if (id == 9006) tailId = -1;

        if (UI.isHeadFix && CurrentHead != null)
        {
            head_id = CurrentHead.id;
            head_costumeId = CurrentHead.costumeId;
            tailId = CurrentHead.tailId;
        }
        else
        {
            head_id = id;
            head_costumeId = costumeId;

            CurrentHead = new UmaHeadData
            {
                id = id,
                costumeId = costumeId,
                tailId = tailId
            };
        }

        string head = UmaDatabaseController.HeadPath + $"chr{head_id}_{head_costumeId}/pfb_chr{head_id}_{head_costumeId}";
        asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == head);
        bool isDefaultHead = false;
        //Some costumes don't have custom heads
        if (head_costumeId != "00" && asset == null)
        {
            asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == UmaDatabaseController.HeadPath + $"chr{head_id}_00/pfb_chr{head_id}_00");
            isDefaultHead = true;
        }

        if (asset != null)
        {
            //Load Hair Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"{UmaDatabaseController.HeadPath}chr{head_id}_{head_costumeId}/textures")))
            {
                RecursiveLoadAsset(asset1);
            }
            //Load Head
            RecursiveLoadAsset(asset);

            //Load Physics
            if (isDefaultHead)
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"{UmaDatabaseController.HeadPath}chr{head_id}_00/clothes")))
                {
                    if (asset1.Name.Contains("cloth00"))
                        RecursiveLoadAsset(asset1);
                }
            }
            else
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"{UmaDatabaseController.HeadPath}chr{head_id}_{head_costumeId}/clothes")))
                {
                    if (asset1.Name.Contains("cloth00"))
                        RecursiveLoadAsset(asset1);
                }
            }
        }


        if (tailId != 0)
        {
            string tailName = $"tail{tailId.ToString().PadLeft(4, '0')}_00";
            string tailPath = $"3d/chara/tail/{tailName}/";
            string tailPfb = tailPath + $"pfb_{tailName}";
            asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == tailPfb);
            if (asset != null)
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_{head_id}") || a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_0000")))
                {
                    RecursiveLoadAsset(asset1);
                }
                RecursiveLoadAsset(asset);


                //Load Physics
                foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"{tailPath}clothes")))
                {
                    if (asset1.Name.Contains("cloth00"))
                        RecursiveLoadAsset(asset1);
                }
            }
            else
            {
                Debug.Log("no tail");
            }
        }

        //Load FacialMorph
        if (CurrentUMAContainer.Head)
        {
            var firsehead = CurrentUMAContainer.Head;
            var FaceDriven = firsehead.GetComponent<AssetHolder>()._assetTable.list.Find(a => { return a.Key == "facial_target"; }).Value as FaceDrivenKeyTarget;
            CurrentUMAContainer.FaceDrivenKeyTarget = FaceDriven;
            CurrentUMAContainer.FaceDrivenKeyTarget.Container = CurrentUMAContainer;
            FaceDriven.Initialize(firsehead.GetComponentsInChildren<Transform>().ToList());

            var emotionDriven = ScriptableObject.CreateInstance<FaceEmotionKeyTarget>();
            emotionDriven.name = $"char{id}_{costumeId}_emotion_target";
            CurrentUMAContainer.FaceEmotionKeyTarget = emotionDriven;
            emotionDriven.FaceDrivenKeyTarget = FaceDriven;
            emotionDriven.FaceEmotionKey = UmaDatabaseController.Instance.FaceTypeData.ToList();
            emotionDriven.Initialize();
        }

        CurrentUMAContainer.MergeModel();
        CurrentUMAContainer.LoadPhysics();
        LoadAsset(UmaViewerMain.Instance.AbMotions.FirstOrDefault(a => a.Name.EndsWith($"anm_eve_chr{id}_00_idle01_loop")));
    }

    private void LoadMiniUma(int id, string costumeId)
    {
        JObject charaData = CurrentUMAContainer.CharaData;
        CurrentUMAContainer.IsMini = true;
        bool isGeneric = CurrentUMAContainer.IsGeneric = costumeId.Length >= 4;
        string skin = (string)charaData["skin"],
               height = (string)charaData["height"],
               socks = (string)charaData["socks"],
               bust = (string)charaData["bust"],
               sex = (string)charaData["sex"],
               costumeIdShort = "";
        bool customHead = true;

        UmaDatabaseEntry asset = null;
        if (isGeneric)
        {
            costumeIdShort = costumeId.Remove(costumeId.LastIndexOf('_'));
            string body = $"3d/chara/mini/body/mbdy{costumeIdShort}/pfb_mbdy{costumeId}_0";
            asset = UmaViewerMain.Instance.AbChara.FirstOrDefault(a => a.Name == body);
        }
        else asset = UmaViewerMain.Instance.AbChara.FirstOrDefault(a => a.Name == $"3d/chara/mini/body/mbdy{id}_{costumeId}/pfb_mbdy{id}_{costumeId}");
        if (asset == null)
        {
            Debug.LogError("No body, can't load!");
            return;
        }
        else if (isGeneric)
        {
            string texPattern1 = "";
            switch (costumeId.Split('_')[0])
            {
                case "0003":
                    texPattern1 = $"tex_mbdy{costumeIdShort}_00_{skin}_{0}";
                    break;
                default:
                    texPattern1 = $"tex_mbdy{costumeId}_{skin}_{0}";
                    break;
            }
            //Load Body Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith("3d/chara/mini/body/") && a.Name.Contains(texPattern1)))
            {
                RecursiveLoadAsset(asset1);
            }
            //Load Body
            RecursiveLoadAsset(asset);
        }
        else
            RecursiveLoadAsset(asset);

        string hair = $"3d/chara/mini/head/mchr{id}_{costumeId}/pfb_mchr{id}_{costumeId}_hair";
        asset = UmaViewerMain.Instance.AbChara.FirstOrDefault(a => a.Name == hair);
        if (costumeId != "00" && asset == null)
        {
            customHead = false;
            asset = UmaViewerMain.Instance.AbChara.FirstOrDefault(a => a.Name == $"3d/chara/mini/head/mchr{id}_00/pfb_mchr{id}_00_hair");
        }
        if (asset != null)
        {
            //Load Hair Textures
            if (customHead)
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"3d/chara/mini/head/mchr{id}_{costumeId}/textures")))
                {
                    RecursiveLoadAsset(asset1);
                }
            }
            else
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"3d/chara/mini/head/mchr{id}_00/textures")))
                {
                    RecursiveLoadAsset(asset1);
                }
            }

            //Load Hair
            RecursiveLoadAsset(asset);
        }

        string head = $"3d/chara/mini/head/mchr0001_00/pfb_mchr0001_00_face0";
        asset = UmaViewerMain.Instance.AbChara.FirstOrDefault(a => a.Name == head);
        if (asset != null)
        {
            //Load Head Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"3d/chara/mini/head/mchr0001_00/textures/tex_mchr0001_00_face0_{skin}")))
            {
                RecursiveLoadAsset(asset1);
            }
            //Load Head
            RecursiveLoadAsset(asset);
        }

        CurrentUMAContainer.MergeModel();
    }

    public void LoadProp(UmaDatabaseEntry entry)
    {
        UnloadProp();
        UnloadAllBundle();

        CurrentOtherContainer = new GameObject(Path.GetFileName(entry.Name)).AddComponent<UmaContainer>();
        RecursiveLoadAsset(entry);
    }

    public void LoadLive(LiveEntry live)
    {
        var asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name.EndsWith("cutt_son" + live.MusicId));
        var BGasset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name.EndsWith($"pfb_env_live{live.BackGroundId}_controller000"));
        if (asset == null || BGasset == null) return;

        UnloadLive();
        UnloadAllBundle();
        CurrentLiveContainer = new GameObject(Path.GetFileName(asset.Name)).AddComponent<UmaContainer>();
        RecursiveLoadAsset(asset);
        RecursiveLoadAsset(BGasset);

    }

    //Use CriWare Library
    public void LoadLiveSoundCri(int songid, UmaDatabaseEntry SongAwb)
    {
        //清理
        if (CurrentAudioSources.Count > 0)
        {
            var tmp = CurrentAudioSources[0];
            CurrentAudioSources.Clear();
            Destroy(tmp.gameObject);
            UI.ResetAudioPlayer();
        }

        //获取Acb文件和Awb文件的路径
        string nameVar = SongAwb.Name.Split('.')[0].Split('/').Last();

        //使用Live的Bgm
        nameVar = $"snd_bgm_live_{songid}_oke";

        LoadSound Loader = (LoadSound)ScriptableObject.CreateInstance("LoadSound");
        LoadSound.UmaSoundInfo soundInfo = Loader.getSoundPath(nameVar);

        //音频组件添加路径，载入音频
        CriAtom.AddCueSheet(nameVar, soundInfo.acbPath, soundInfo.awbPath);

        //获得当前音频信息
        CriAtomEx.CueInfo[] cueInfoList;
        List<string> cueNameList = new List<string>();
        cueInfoList = CriAtom.GetAcb(nameVar).GetCueInfoList();
        foreach (CriAtomEx.CueInfo cueInfo in cueInfoList)
        {
            cueNameList.Add(cueInfo.name);
        }

        //创建播放器
        CriAtomSource source = new GameObject("CuteAudioSource").AddComponent<CriAtomSource>();
        source.transform.SetParent(GameObject.Find("AudioManager/AudioControllerBgm").transform);
        source.cueSheet = nameVar;

        //播放
        source.Play(cueNameList[0]);
    }

    //Use decrypt function
    public void LoadLiveSound(int songid, UmaDatabaseEntry SongAwb)
    {
        if (CurrentAudioSources.Count > 0)
        {
            var tmp = CurrentAudioSources[0];
            CurrentAudioSources.Clear();
            Destroy(tmp.gameObject);
            UI.ResetAudioPlayer();
        }

        foreach (AudioClip clip in LoadAudio(SongAwb))
        {
            AddAudioSource(clip);
        }

        string nameVar = $"snd_bgm_live_{songid}_oke";
        UmaDatabaseEntry BGawb = Main.AbList.FirstOrDefault(a => a.Name.Contains(nameVar) && a.Name.EndsWith("awb"));
        if (BGawb != null)
        {
            var BGclip = LoadAudio(BGawb);
            if (BGclip.Count > 0)
            {
                AddAudioSource(BGclip[0]);
            }
        }

        LoadLiveLyrics(songid);
    }

    private void AddAudioSource(AudioClip clip)
    {
        AudioSource source;
        if (CurrentAudioSources.Count > 0)
        {

            if (Mathf.Abs(CurrentAudioSources[0].clip.length - clip.length) > 3) return;
            source = CurrentAudioSources[0].gameObject.AddComponent<AudioSource>();
        }
        else
        {
            source = new GameObject("SoundController").AddComponent<AudioSource>();
        }
        CurrentAudioSources.Add(source);
        source.clip = clip;
        source.Play();
    }

    public List<AudioClip> LoadAudio(UmaDatabaseEntry awb)
    {
        List<AudioClip> clips = new List<AudioClip>();
        UmaViewerUI.Instance.LoadedAssetsAdd(awb);
        string awbPath = UmaDatabaseController.GetABPath(awb); ;
        if (!File.Exists(awbPath)) return clips;

        FileStream awbFile = File.OpenRead(awbPath);
        AwbReader awbReader = new AwbReader(awbFile);

        foreach (Wave wave in awbReader.Waves)
        {
            var stream = new UmaWaveStream(awbReader, wave.WaveId);
            var sampleProvider = stream.ToSampleProvider();

            int channels = stream.WaveFormat.Channels;
            int bytesPerSample = stream.WaveFormat.BitsPerSample / 8;
            int sampleRate = stream.WaveFormat.SampleRate;

            AudioClip clip = AudioClip.Create(
                Path.GetFileNameWithoutExtension(awb.Name) + "_" + wave.WaveId.ToString(),
                (int)(stream.Length / channels / bytesPerSample),
                channels,
                sampleRate,
                true,
                data => sampleProvider.Read(data, 0, data.Length),
                position => stream.Position = position * channels * bytesPerSample);

            clips.Add(clip);
        }

        return clips;
    }

    public void LoadLiveLyrics(int songid)
    {
        if (CurrentLyrics.Count > 0) CurrentLyrics.Clear();

        string lyricsVar = $"live/musicscores/m{songid}/m{songid}_lyrics";
        UmaDatabaseEntry lyricsAsset = Main.AbList.FirstOrDefault(a => a.Name.Contains(lyricsVar));
        if (lyricsAsset != null)
        {
            string filePath = UmaDatabaseController.GetABPath(lyricsAsset);
            if (File.Exists(filePath))
            {
                AssetBundle bundle;
                if (Main.LoadedBundles.ContainsKey(lyricsAsset.Name))
                {
                    bundle = Main.LoadedBundles[lyricsAsset.Name];
                }
                else
                {
                    UI.LoadedAssetsAdd(lyricsAsset);
                    bundle = AssetBundle.LoadFromFile(filePath);
                    Main.LoadedBundles.Add(lyricsAsset.Name, bundle);
                }

                TextAsset asset = bundle.LoadAsset<TextAsset>(Path.GetFileNameWithoutExtension(lyricsVar));
                string[] lines = asset.text.Split("\n"[0]);

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] words = lines[i].Split(',');
                    if (words.Length > 0)
                    {
                        try
                        {
                            UmaLyricsData lyricsData = new UmaLyricsData()
                            {
                                time = float.Parse(words[0]) / 1000,
                                text = (words.Length > 1) ? words[1].Replace("[COMMA]", "，") : ""
                            };
                            CurrentLyrics.Add(lyricsData);
                        }
                        catch { }
                    }
                }
            }
        }
    }

    private void RecursiveLoadAsset(UmaDatabaseEntry entry, bool IsSubAsset = false)
    {
        if (!string.IsNullOrEmpty(entry.Prerequisites))
        {
            foreach (string prerequisite in entry.Prerequisites.Split(';'))
            {
                if (prerequisite.StartsWith(UmaDatabaseController.CharaPath))
                {
                    RecursiveLoadAsset(Main.AbChara.FirstOrDefault(ab => ab.Name == prerequisite), true);
                }
                else if (prerequisite.StartsWith(UmaDatabaseController.MotionPath))
                {
                    RecursiveLoadAsset(Main.AbMotions.FirstOrDefault(ab => ab.Name == prerequisite), true);
                }
                else
                    RecursiveLoadAsset(Main.AbList.FirstOrDefault(ab => ab.Name == prerequisite), true);
            }
        }
        LoadAsset(entry, IsSubAsset);
    }

    public void LoadAsset(UmaDatabaseEntry entry, bool IsSubAsset = false)
    {
        Debug.Log("Loading " + entry.Name);
        if (Main.LoadedBundles.ContainsKey(entry.Name)) return;

        string filePath = UmaDatabaseController.GetABPath(entry);
        if (File.Exists(filePath))
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
            if (bundle == null)
            {
                Debug.Log(filePath + " exists and doesn't work");
                return;
            }
            Main.LoadedBundles.Add(entry.Name, bundle);
            UI.LoadedAssetsAdd(entry);
            LoadBundle(bundle, IsSubAsset);
        }
        else
        {
            Debug.LogError($"{entry.Name} - {filePath} does not exist");
        }
    }

    private void LoadBundle(AssetBundle bundle, bool IsSubAsset = false)
    {
        if (bundle.name == "shader.a")
        {
            if (Main.ShadersLoaded)
            {
                return;
            }
            else
            {
                hairShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonhairtser.shader");
                faceShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonfacetser.shader");
                eyeShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertooneyet.shader");
                cheekShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactermultiplycheek.shader");
                eyebrowShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonmayu.shader");
                alphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoonhairtser.shader");
                Main.ShadersLoaded = true;
            }
        }

        foreach (string name in bundle.GetAllAssetNames())
        {
            object asset = bundle.LoadAsset(name);

            if (asset == null) { continue; }
            Debug.Log("Bundle:" + bundle.name + "/" + name + $" ({asset.GetType()})");
            switch (asset)
            {
                case AnimationClip aClip:
                    {
                        if (CurrentUMAContainer && CurrentUMAContainer.UmaAnimator && CurrentUMAContainer.UmaAnimator.runtimeAnimatorController)
                        {
                            LoadAnimation(aClip);
                        }

                        if (!CurrentLiveContainer)
                            UnloadBundle(bundle, false);
                        break;
                    }
                case GameObject go:
                    {
                        if (bundle.name.Contains("cloth"))
                        {
                            if (!CurrentUMAContainer.PhysicsController)
                            {
                                CurrentUMAContainer.PhysicsController = new GameObject("PhysicsController");
                                CurrentUMAContainer.PhysicsController.transform.SetParent(CurrentUMAContainer.transform);
                            }
                            Instantiate(go, CurrentUMAContainer.PhysicsController.transform);
                        }
                        else if (bundle.name.Contains("/head/"))
                        {
                            LoadHead(go);
                        }
                        else if (bundle.name.Contains("/body/"))
                        {
                            LoadBody(go);
                        }
                        else if (bundle.name.Contains("/tail/"))
                        {
                            LoadTail(go);
                        }
                        else
                        {
                            if (!IsSubAsset)
                            {
                                LoadProp(go);
                            }
                        }
                        break;
                    }
                case Shader sha:
                    ShaderList.Add(sha);
                    break;
                case Texture2D tex2D:

                    if (bundle.name.Contains("/mini/head"))
                    {
                        CurrentUMAContainer.MiniHeadTextures.Add(tex2D);
                    }
                    else if (bundle.name.Contains("/tail/"))
                    {
                        CurrentUMAContainer.TailTextures.Add(tex2D);
                    }
                    else if (bundle.name.Contains("bdy0"))
                    {
                        CurrentUMAContainer.GenericBodyTextures.Add(tex2D);
                    }
                    break;
            }
        }
    }

    private void LoadBody(GameObject go)
    {
        CurrentUMAContainer.Body = Instantiate(go, CurrentUMAContainer.transform);
        CurrentUMAContainer.UmaAnimator = CurrentUMAContainer.Body.GetComponent<Animator>();

        if (CurrentUMAContainer.IsGeneric)
        {
            List<Texture2D> textures = CurrentUMAContainer.GenericBodyTextures;
            string costumeIdShort = CurrentUMAContainer.VarCostumeIdShort,
                   costumeIdLong = CurrentUMAContainer.VarCostumeIdLong,
                   height = CurrentUMAContainer.VarHeight,
                   skin = CurrentUMAContainer.VarSkin,
                   socks = CurrentUMAContainer.VarSocks,
                   bust = CurrentUMAContainer.VarBust;

            foreach (Renderer r in CurrentUMAContainer.Body.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    string mainTex = "", toonMap = "", tripleMap = "", optionMap = "", zekkenNumberTex = "";
                    if (CurrentUMAContainer.IsMini)
                    {

                        m.SetTexture("_MainTex", textures[0]);
                    }
                    else
                    {
                        switch (costumeIdShort.Split('_')[0]) //costume ID
                        {
                            case "0001":
                                switch (r.sharedMaterials.ToList().IndexOf(m))
                                {
                                    case 0:
                                        mainTex = $"tex_bdy{costumeIdShort}_00_waku0_diff";
                                        toonMap = $"tex_bdy{costumeIdShort}_00_waku0_shad_c";
                                        tripleMap = $"tex_bdy{costumeIdShort}_00_waku0_base";
                                        optionMap = $"tex_bdy{costumeIdShort}_00_waku0_ctrl";
                                        break;
                                    case 1:
                                        mainTex = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}_{socks.PadLeft(2, '0')}_diff";
                                        toonMap = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}_{socks.PadLeft(2, '0')}_shad_c";
                                        tripleMap = $"tex_bdy{costumeIdShort}_00_0_{bust}_00_base";
                                        optionMap = $"tex_bdy{costumeIdShort}_00_0_{bust}_00_ctrl";
                                        break;
                                    case 2:
                                        int color = UnityEngine.Random.Range(0, 4);
                                        mainTex = $"tex_bdy0001_00_zekken{color}_{bust}_diff";
                                        toonMap = $"tex_bdy0001_00_zekken{color}_{bust}_shad_c";
                                        tripleMap = $"tex_bdy0001_00_zekken0_{bust}_base";
                                        optionMap = $"tex_bdy0001_00_zekken0_{bust}_ctrl";
                                        break;
                                }

                                zekkenNumberTex = $"tex_bdy0001_00_num{UnityEngine.Random.Range(1, 18):d2}";
                                break;
                            case "0003":
                                mainTex = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}_diff";
                                toonMap = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}_shad_c";
                                tripleMap = $"tex_bdy{costumeIdShort}_00_0_{bust}_base";
                                optionMap = $"tex_bdy{costumeIdShort}_00_0_{bust}_ctrl";
                                break;
                            case "0006":
                                mainTex = $"tex_bdy{costumeIdLong}_{skin}_{bust}_{"00"}_diff";
                                toonMap = $"tex_bdy{costumeIdLong}_{skin}_{bust}_{"00"}_shad_c";
                                tripleMap = $"tex_bdy{costumeIdLong}_0_{bust}_00_base";
                                optionMap = $"tex_bdy{costumeIdLong}_0_{bust}_00_ctrl";
                                break;
                            case "0009":
                                mainTex = $"tex_bdy{costumeIdLong}_{skin}_{bust}_{"00"}_diff";
                                toonMap = $"tex_bdy{costumeIdLong}_{skin}_{bust}_{"00"}_shad_c";
                                tripleMap = $"tex_bdy{costumeIdLong}_0_{bust}_00_base";
                                optionMap = $"tex_bdy{costumeIdLong}_0_{bust}_00_ctrl";
                                break;
                            default:
                                mainTex = $"tex_bdy{costumeIdLong}_{skin}_{bust}_diff";
                                toonMap = $"tex_bdy{costumeIdLong}_{skin}_{bust}_shad_c";
                                tripleMap = $"tex_bdy{costumeIdLong}_0_{bust}_base";
                                optionMap = $"tex_bdy{costumeIdLong}_0_{bust}_ctrl";
                                break;

                        }
                        Debug.Log("Looking for texture " + mainTex);
                        m.SetTexture("_MainTex", textures.FirstOrDefault(t => t.name == mainTex));
                        m.SetTexture("_ToonMap", textures.FirstOrDefault(t => t.name == toonMap));
                        m.SetTexture("_TripleMaskMap", textures.FirstOrDefault(t => t.name == tripleMap));
                        m.SetTexture("_OptionMaskMap", textures.FirstOrDefault(t => t.name == optionMap));

                        if (!string.IsNullOrEmpty(zekkenNumberTex))
                            m.SetTexture("_ZekkenNumberTex", textures.FirstOrDefault(t => t.name == zekkenNumberTex));
                    }
                }
            }
        }
    }

    private void LoadHead(GameObject go)
    {
        GameObject head = Instantiate(go, CurrentUMAContainer.transform);
        CurrentUMAContainer.Head = head;

        foreach (Renderer r in head.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                if (head.name.Contains("mchr"))
                {
                    if (r.name.Contains("Hair"))
                    {
                        CurrentUMAContainer.Tail = head;
                    }
                    if (r.name == "M_Face")
                    {
                        m.SetTexture("_MainTex", CurrentUMAContainer.MiniHeadTextures.First(t => t.name.Contains("face") && t.name.Contains("diff")));
                    }
                    if (r.name == "M_Cheek")
                    {
                        m.CopyPropertiesFromMaterial(TransMaterialCharas);
                        m.SetTexture("_MainTex", CurrentUMAContainer.MiniHeadTextures.First(t => t.name.Contains("cheek")));
                    }
                    if (r.name == "M_Mouth")
                    {
                        m.SetTexture("_MainTex", CurrentUMAContainer.MiniHeadTextures.First(t => t.name.Contains("mouth")));
                    }
                    if (r.name == "M_Eye")
                    {
                        m.SetTexture("_MainTex", CurrentUMAContainer.MiniHeadTextures.First(t => t.name.Contains("eye")));
                    }
                    if (r.name.StartsWith("M_Mayu_"))
                    {
                        m.SetTexture("_MainTex", CurrentUMAContainer.MiniHeadTextures.First(t => t.name.Contains("mayu")));
                    }
                }
                else
                {
                    //Glasses's shader need to change manually.
                    if (r.name.Contains("Hair") && r.name.Contains("Alpha"))
                    {
                        m.shader = alphaShader;
                    }
                    switch (m.shader.name)
                    {
                        case "Gallop/3D/Chara/MultiplyCheek":
                            m.shader = cheekShader;
                            m.CopyPropertiesFromMaterial(TransMaterialCharas);
                            break;
                        case "Gallop/3D/Chara/ToonFace/TSER":
                            m.shader = faceShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            m.SetColor("_RimColor", new Color(0, 0, 0, 0));
                            break;
                        case "Gallop/3D/Chara/ToonEye/T":
                            m.shader = eyeShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            break;
                        case "Gallop/3D/Chara/ToonHair/TSER":
                            m.shader = hairShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            break;
                        case "Gallop/3D/Chara/ToonMayu":
                            m.shader = eyebrowShader;
                            break;
                        default:
                            Debug.Log(m.shader.name);
                            // m.shader = Shader.Find("Nars/UmaMusume/Body");
                            break;
                    }
                }
            }
        }

        //foreach (var anim in Main.AbList.Where(a => a.Name.StartsWith("3d/chara/head/chr0001_00/facial/")))
        //{
        //    RecursiveLoadAsset(anim);
        //}
    }

    private void LoadTail(GameObject gameObject)
    {
        CurrentUMAContainer.Tail = Instantiate(gameObject, CurrentUMAContainer.transform);
        var textures = CurrentUMAContainer.TailTextures;
        foreach (Renderer r in CurrentUMAContainer.Tail.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                m.SetTexture("_MainTex", textures.FirstOrDefault(t => t.name.EndsWith("diff")));
                m.SetTexture("_ToonMap", textures.FirstOrDefault(t => t.name.Contains("shad")));
                m.SetTexture("_TripleMaskMap", textures.FirstOrDefault(t => t.name.Contains("base")));
                m.SetTexture("_OptionMaskMap", textures.FirstOrDefault(t => t.name.Contains("ctrl")));
            }
        }
    }

    private void LoadProp(GameObject go)
    {
        var container = (go.name.Contains("Cutt_son") || go.name.Contains("pfb_env_live")) ? CurrentLiveContainer : CurrentOtherContainer;
        var prop = Instantiate(go, container.transform);
        foreach (Renderer r in prop.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                //Shaders can be differentiated by checking m.shader.name
                m.shader = Shader.Find("Unlit/Transparent Cutout");
            }
        }
    }

    private void LoadAnimation(AnimationClip clip)
    {
        bool needTransit = false;
        if (clip.name.EndsWith("_S"))
        {
            CurrentUMAContainer.OverrideController["clip_s"] = clip;
        }
        else if (clip.name.EndsWith("_E"))
        {
            CurrentUMAContainer.OverrideController["clip_e"] = clip;
        }
        else if (clip.name.EndsWith("_loop"))
        {
            var motion_s = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name.Replace("_loop", "_s")));
            var motion_e = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(CurrentUMAContainer.OverrideController["clip_2"].name.Replace("_loop", "_e")));
            needTransit = (motion_s != null && motion_e != null);
            if (needTransit)
            {
                RecursiveLoadAsset(motion_e);
                RecursiveLoadAsset(motion_s);
            }

            var lastTime = CurrentUMAContainer.UmaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            CurrentUMAContainer.OverrideController["clip_1"] = CurrentUMAContainer.OverrideController["clip_2"];
            CurrentUMAContainer.OverrideController["clip_2"] = clip;

            CurrentUMAContainer.UmaAnimator.Play("motion_1", -1);
            CurrentUMAContainer.UmaAnimator.SetTrigger(needTransit ? "next_s" : "next");
        }
        else if (clip.name.Contains("face"))
        {
            CurrentUMAContainer.FaceOverrideController["clip_1"] = clip;
            CurrentUMAContainer.FaceDrivenKeyTarget.ResetLocator();
            CurrentUMAContainer.isAnimatorControl = true;
            CurrentUMAContainer.UmaFaceAnimator.Play("motion_1", 0, 0);
        }
        else if (clip.name.Contains("cam"))
        {
            var overrideController = Instantiate(CameraOverrideController);
            overrideController["clip_1"] = clip;
            CurrentCameraAnimator = Camera.main.gameObject.GetComponent<Animator>();
            CurrentCameraAnimator.runtimeAnimatorController = overrideController;
            CurrentCameraAnimator.Play("motion_1", 0, 0);
        }
        else
        {
            CurrentUMAContainer.OverrideController["clip_1"] = CurrentUMAContainer.OverrideController["clip_2"];
            var lastTime = CurrentUMAContainer.UmaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            CurrentUMAContainer.OverrideController["clip_2"] = clip;
            // If Cut-in, play immediately without state interpolation
            if (clip.name.Contains("crd") || clip.name.Contains("res_chr"))
            {
                var facialMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name + "_face"));
                var cameraMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name + "_cam"));

                if (facialMotion != null)
                {
                    RecursiveLoadAsset(facialMotion);
                }
                if(cameraMotion != null)
                {
                    RecursiveLoadAsset(cameraMotion);
                }

                CurrentUMAContainer.UmaAnimator.Play("motion_2", 0, 0);
                CurrentUMAContainer.OverrideController["clip_2"].wrapMode = WrapMode.Loop;
            }
            else
            {
                CurrentUMAContainer.isAnimatorControl = false;
                CurrentUMAContainer.FaceDrivenKeyTarget.ResetLocator();

                if(CurrentCameraAnimator)
                CurrentCameraAnimator.GetComponent<Animator>().runtimeAnimatorController = null;

                CurrentUMAContainer.UmaAnimator.Play("motion_1", 0, lastTime);
                CurrentUMAContainer.UmaAnimator.SetTrigger(needTransit ? "next_s" : "next");
            }

        }

    }

    private void UnloadBundle(AssetBundle bundle, bool unloadAllObjects)
    {
        var entry = Main.LoadedBundles.FirstOrDefault(b => b.Value == bundle);
        if (entry.Key != null)
        {
            Main.LoadedBundles.Remove(entry.Key);
        }
        bundle.Unload(unloadAllObjects);
    }

    public void UnloadAllBundle(bool unloadAllObjects = false)
    {

        foreach (var bundle in Main.LoadedBundles)
        {
            bundle.Value.Unload(unloadAllObjects);
        }
        if (unloadAllObjects)
        {
            if (CurrentUMAContainer) Destroy(CurrentUMAContainer);
            if (CurrentLiveContainer) Destroy(CurrentLiveContainer);
            if (CurrentOtherContainer) Destroy(CurrentOtherContainer);
        }
        Main.LoadedBundles.Clear();
        UI.LoadedAssetsClear();
    }

    public Sprite LoadCharaIcon(string id)
    {
        string value = $"chara/chr{id}/chr_icon_{id}";
        var entry = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name.Equals(value));
        string path = UmaDatabaseController.GetABPath(entry);
        if (File.Exists(path))
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
            if (assetBundle.Contains($"chr_icon_{id}"))
            {
                Texture2D texture = (Texture2D)assetBundle.LoadAsset($"chr_icon_{id}");
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                return sprite;
            }
        }
        return null;
    }

    public void UnloadProp()
    {
        if (CurrentOtherContainer != null)
        {
            Destroy(CurrentOtherContainer.gameObject);
        }
    }

    public void UnloadLive()
    {
        if (CurrentLiveContainer != null)
        {
            Destroy(CurrentLiveContainer.gameObject);
        }
    }
    public void UnloadUma()
    {
        if (CurrentUMAContainer != null)
        {
            //It seems that OnDestroy will executed after new model loaded, which cause new FacialPanels empty...
            UmaViewerUI.Instance.currentFaceDrivenKeyTarget = null;
            UmaViewerUI.Instance.LoadEmotionPanels(null);
            UmaViewerUI.Instance.LoadFacialPanels(null);
            Destroy(CurrentUMAContainer.gameObject);
        }
    }

    public void ClearMorphs()
    {
        if (CurrentUMAContainer != null && CurrentUMAContainer.FaceDrivenKeyTarget != null)
        {
            foreach (var container in UI.EmotionList.GetComponentsInChildren<UmaUIContainer>())
            {
                if (container.Slider != null)
                    container.Slider.value = 0;
            }
            foreach (var container in UI.FacialList.GetComponentsInChildren<UmaUIContainer>())
            {
                if (container.Slider != null)
                    container.Slider.SetValueWithoutNotify(0);
            }
            CurrentUMAContainer.FaceDrivenKeyTarget.ClearMorph();
            CurrentUMAContainer.FaceDrivenKeyTarget.ChangeMorph();
        }
    }
}
