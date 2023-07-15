using CriWareFormats;
using Gallop;
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UmaMusumeAudio;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class UmaViewerBuilder : MonoBehaviour
{
    public static UmaViewerBuilder Instance;
    public static UmaViewerMain Main => UmaViewerMain.Instance;
    UmaViewerUI UI => UmaViewerUI.Instance;

    public List<AssetBundle> Loaded;
    public List<Shader> ShaderList = new List<Shader>();
    public Material TransMaterialCharas;
    public Material TransMaterialProps;
    public UmaContainer CurrentUMAContainer;
    public UmaContainer CurrentOtherContainer;

    public UmaHeadData CurrentHead;

    public Shader hairShader;
    public Shader faceShader;
    public Shader eyeShader;
    public Shader cheekShader;
    public Shader eyebrowShader;
    public Shader alphaShader;
    public Shader bodyAlphaShader;
    public Shader bodyBehindAlphaShader;

    public List<AudioSource> CurrentAudioSources = new List<AudioSource>();
    public List<UmaLyricsData> CurrentLyrics = new List<UmaLyricsData>();

    public AnimatorOverrideController OverrideController;
    public AnimatorOverrideController FaceOverrideController;
    public AnimatorOverrideController CameraOverrideController;
    public Animator AnimationCameraAnimator;
    public Camera AnimationCamera;

    private bool ShadersLoaded = false;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator LoadUma(CharaEntry chara, string costumeId, bool mini)
    {
        int id = chara.Id;
        CurrentUMAContainer = new GameObject($"Chara_{id}_{costumeId}").AddComponent<UmaContainer>();
        CurrentUMAContainer.CharaEntry = chara;

        if (mini)
        {
            CurrentUMAContainer.CharaData = UmaDatabaseController.ReadCharaData(chara);
            LoadMiniUma(chara, costumeId);
        }
        else if (chara.IsMob)
        {
            CurrentUMAContainer.CharaData = UmaDatabaseController.ReadCharaData(chara);
            LoadMobUma(chara, costumeId, loadMotion: true);
        }
        else if (UI.isHeadFix && CurrentHead != null && CurrentHead.chara.IsMob)
        {
            CurrentUMAContainer.CharaData = UmaDatabaseController.ReadCharaData(CurrentHead.chara);
            LoadMobUma(CurrentHead.chara, costumeId, chara.Id, true);
        }
        else
        {
            CurrentUMAContainer.CharaData = UmaDatabaseController.ReadCharaData(chara);
            LoadNormalUma(chara, costumeId, true);
        }

        yield break;
    }

    public void LoadLiveUma(List<LiveCharacterSelect> characters)
    {
        UmaAssetManager.UnloadAllBundle();
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].CharaEntry.Name != "")
            {
                CurrentUMAContainer = new GameObject($"Chara_{characters[i].CharaEntry.Id}_{characters[i].CostumeId}").AddComponent<UmaContainer>();

                if (characters[i].CharaEntry.IsMob)
                {
                    CurrentUMAContainer.IsLive = true;
                    CurrentUMAContainer.CharaData = UmaDatabaseController.ReadCharaData(characters[i].CharaEntry);
                    CurrentUMAContainer.transform.parent = Gallop.Live.Director.instance.charaObjs[i];
                    LoadMobUma(characters[i].CharaEntry, characters[i].CostumeId);
                }
                else
                {
                    CurrentUMAContainer.IsLive = true;
                    CurrentUMAContainer.CharaData = UmaDatabaseController.ReadCharaData(characters[i].CharaEntry);
                    CurrentUMAContainer.transform.parent = Gallop.Live.Director.instance.charaObjs[i];
                    LoadNormalUma(characters[i].CharaEntry, characters[i].CostumeId);
                }
            }
        }
    }

    private void LoadNormalUma(CharaEntry chara, string costumeId, bool loadMotion = false)
    {
        int id = chara.Id;
        DataRow charaData = CurrentUMAContainer.CharaData;
        bool genericCostume = CurrentUMAContainer.IsGeneric = costumeId.Length >= 4;
        string skin, height, socks, bust, sex, shape, costumeIdShort = "";
        skin = charaData["skin"].ToString();
        height = charaData["height"].ToString();
        socks = charaData["socks"].ToString();
        bust = charaData["bust"].ToString();
        sex = charaData["sex"].ToString();
        shape = charaData["shape"].ToString();

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

            string body = UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/pfb_bdy{costumeId}_{height}_{shape}_{bust}";
            UmaViewerMain.Instance.AbList.TryGetValue(body, out asset);
        }
        else UmaViewerMain.Instance.AbList.TryGetValue(UmaDatabaseController.BodyPath + $"bdy{id}_{costumeId}/pfb_bdy{id}_{costumeId}", out asset);

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
            foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith(UmaDatabaseController.BodyPath)
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
            if (Main.AbList.TryGetValue(UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_cloth00", out _))
            {
                var asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_cloth00"];
                RecursiveLoadAsset(asset1);
                asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_bust{bust}_cloth00"];
                RecursiveLoadAsset(asset1);
            }
        }
        else
        {
            RecursiveLoadAsset(asset);
            //Load Physics
            var asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{id}_{costumeId}/clothes/pfb_bdy{id}_{costumeId}_cloth00"];
            RecursiveLoadAsset(asset1);
            asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{id}_{costumeId}/clothes/pfb_bdy{id}_{costumeId}_bust_cloth00"];
            RecursiveLoadAsset(asset1);
        }

        // Record Head Data
        int head_id;
        string head_costumeId;
        int tailId = Convert.ToInt32(charaData["tail_model_id"]);

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
                tailId = tailId,
                chara = chara
            };
        }

        string head = UmaDatabaseController.HeadPath + $"chr{head_id}_{head_costumeId}/pfb_chr{head_id}_{head_costumeId}";
        asset = null;
        Main.AbList.TryGetValue(head, out asset);

        bool isDefaultHead = false;
        //Some costumes don't have custom heads
        if (head_costumeId != "00" && asset == null)
        {
            asset = Main.AbList[UmaDatabaseController.HeadPath + $"chr{head_id}_00/pfb_chr{head_id}_00"];
            isDefaultHead = true;
        }

        if (asset != null)
        {
            //Load Hair Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"{UmaDatabaseController.HeadPath}chr{head_id}_{head_costumeId}/textures")))
            {
                RecursiveLoadAsset(asset1);
            }

            //Load Head
            RecursiveLoadAsset(asset);

            //Load Physics
            if (isDefaultHead)
            {
                var asset1 = Main.AbList[UmaDatabaseController.HeadPath + $"chr{id}_00/clothes/pfb_chr{id}_00_cloth00"];
                RecursiveLoadAsset(asset1);
            }
            else
            {
                var asset1 = Main.AbList[UmaDatabaseController.HeadPath + $"chr{id}_{costumeId}/clothes/pfb_chr{id}_{costumeId}_cloth00"];
                RecursiveLoadAsset(asset1);
            }
        }

        if (tailId != 0)
        {
            string tailName = $"tail{tailId.ToString().PadLeft(4, '0')}_00";
            string tailPath = $"3d/chara/tail/{tailName}/";
            string tailPfb = tailPath + $"pfb_{tailName}";
            asset = null;
            Main.AbList.TryGetValue(tailPfb, out asset);
            if (asset != null)
            {
                foreach (var asset2 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_{head_id}") || a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_0000")))
                {
                    RecursiveLoadAsset(asset2);
                }

                RecursiveLoadAsset(asset);

                //Load Physics
                var asset1 = Main.AbList[$"{tailPath}clothes/pfb_{tailName}_cloth00"];
                RecursiveLoadAsset(asset1);
            }
            else
            {
                Debug.Log("no tail");
            }
        }

        CurrentUMAContainer.LoadPhysics();
        CurrentUMAContainer.SetDynamicBoneEnable(UI.DynamicBoneEnable);
        LoadFaceMorph(id, costumeId);
        CurrentUMAContainer.TearControllers.ForEach(a => a.SetDir(a.CurrentDir));
        CurrentUMAContainer.HeadBone = (GameObject)CurrentUMAContainer.Body.GetComponent<AssetHolder>()._assetTable["head"];
        CurrentUMAContainer.EyeHeight = CurrentUMAContainer.Head.GetComponent<AssetHolder>()._assetTableValue["head_center_offset_y"];
        CurrentUMAContainer.MergeModel();
        CurrentUMAContainer.SetHeight(-1);
        CurrentUMAContainer.Initialize(!UI.isTPose);
        if (!UI.isTPose && loadMotion)
        {
            if (Main.AbList.TryGetValue($"3d/motion/event/body/chara/chr{id}_00/anm_eve_chr{id}_00_idle01_loop", out UmaDatabaseEntry entry))
            {
                LoadAsset(entry);
            }
        }
    }

    private void LoadMobUma(CharaEntry chara, string costumeId, int bodyid = -1, bool loadMotion = false)
    {
        int id = chara.Id;
        CurrentUMAContainer.IsMob = chara.IsMob;
        CurrentUMAContainer.MobDressColor = UmaDatabaseController.ReadMobDressColor(CurrentUMAContainer.CharaData["dress_color_id"].ToString());
        CurrentUMAContainer.MobHeadColor = UmaDatabaseController.ReadMobHairColor(CurrentUMAContainer.CharaData["chara_hair_color"].ToString());

        DataRow charaData = CurrentUMAContainer.CharaData;
        bool genericCostume = CurrentUMAContainer.IsGeneric = costumeId.Length >= 4;
        string skin, height, socks, bust, sex, shape, costumeIdShort = "";
        string faceid, hairid, personality;

        faceid = charaData["chara_face_model"].ToString();
        hairid = charaData["chara_hair_model"].ToString();
        personality = charaData["default_personality"].ToString();
        skin = charaData["chara_skin_color"].ToString();
        height = "0";
        socks = charaData["socks"].ToString();
        bust = charaData["chara_bust_size"].ToString();
        sex = charaData["sex"].ToString();
        shape = "0";

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
            Main.AbList.TryGetValue(body, out asset);
        }
        else Main.AbList.TryGetValue($"{UmaDatabaseController.BodyPath}bdy{bodyid}_{costumeId}/pfb_bdy{bodyid}_{costumeId}", out asset);

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
            foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith(UmaDatabaseController.BodyPath)
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
            if (Main.AbList.TryGetValue(UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_cloth00", out _))
            {
                var asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_cloth00"];
                RecursiveLoadAsset(asset1);
                asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_bust{bust}_cloth00"];
                RecursiveLoadAsset(asset1);
            }
        }
        else
        {
            RecursiveLoadAsset(asset);
            //Load Physics
            var asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{bodyid}_{costumeId}/clothes/pfb_bdy{bodyid}_{costumeId}_cloth00"];
            RecursiveLoadAsset(asset1);
            asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{bodyid}_{costumeId}/clothes/pfb_bdy{bodyid}_{costumeId}_bust_cloth00"];
            RecursiveLoadAsset(asset1);
        }

        // Record Head Data
        int head_id = 1;
        string head_costumeId = "00";
        int tailId = 1;

        CurrentHead = new UmaHeadData
        {
            id = id,
            costumeId = costumeId,
            tailId = tailId,
            chara = chara
        };

        var head_s = head_id.ToString().PadLeft(4, '0');
        string head = UmaDatabaseController.HeadPath + $"chr{head_s}_{head_costumeId}/pfb_chr{head_s}_{head_costumeId}_face{faceid.PadLeft(3, '0')}";
        string hair = UmaDatabaseController.HeadPath + $"chr{head_s}_{head_costumeId}/pfb_chr{head_s}_{head_costumeId}_hair{hairid.PadLeft(3, '0')}";
        Main.AbList.TryGetValue(head, out asset);
        Main.AbList.TryGetValue(hair, out UmaDatabaseEntry hairasset);
        bool isDefaultHead = true;


        if (asset != null && hairasset != null)
        {
            //Load Face And Hair Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"{UmaDatabaseController.HeadPath}chr{head_s}_{head_costumeId}/textures")))
            {
                if (asset1.Name.Contains($"tex_chr{head_s}_{head_costumeId}_face{faceid.PadLeft(3, '0')}_0")
                    || asset1.Name.Contains($"tex_chr{head_s}_{head_costumeId}_face{faceid.PadLeft(3, '0')}_{skin}"))
                {
                    RecursiveLoadAsset(asset1);
                }

                if (asset1.Name.Contains($"tex_chr{head_s}_{head_costumeId}_hair{hairid.PadLeft(3, '0')}"))
                {
                    RecursiveLoadAsset(asset1);
                }
            }

            //Load face
            RecursiveLoadAsset(asset);
            //Load hair
            RecursiveLoadAsset(hairasset);

            CurrentUMAContainer.MergeHairModel();

            //Load Physics
            if (isDefaultHead)
            {
                if (Main.AbList.TryGetValue($"{UmaDatabaseController.HeadPath}chr{head_s}_00/clothes/pfb_chr{head_s}_00_hair{hairid.PadLeft(3, '0')}_cloth00", out UmaDatabaseEntry asset1))
                {
                    RecursiveLoadAsset(asset1);
                }
            }
        }

        if (tailId != 0)
        {
            string tailName = $"tail{tailId.ToString().PadLeft(4, '0')}_00";
            string tailPath = $"3d/chara/tail/{tailName}/";
            string tailPfb = tailPath + $"pfb_{tailName}";
            asset = UmaViewerMain.Instance.AbChara.FirstOrDefault(a => a.Name == tailPfb);
            if (asset != null)
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_{head_s}") || a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_0000")))
                {
                    RecursiveLoadAsset(asset1);
                }
                RecursiveLoadAsset(asset);


                //Load Physics
                if (Main.AbList.TryGetValue($"{tailPath}clothes/pfb_{tailName}_cloth00", out UmaDatabaseEntry asset2))
                {
                    RecursiveLoadAsset(asset2);
                }
            }
            else
            {
                Debug.Log("no tail");
            }
        }

        CurrentUMAContainer.LoadPhysics(); //Need to load physics before loading FacialMorph
        CurrentUMAContainer.SetDynamicBoneEnable(UI.DynamicBoneEnable);

        //Load FacialMorph
        LoadFaceMorph(id, costumeId);

        CurrentUMAContainer.TearControllers.ForEach(a => a.SetDir(a.CurrentDir));
        CurrentUMAContainer.HeadBone = (GameObject)CurrentUMAContainer.Body.GetComponent<AssetHolder>()._assetTable["head"];
        CurrentUMAContainer.EyeHeight = CurrentUMAContainer.Head.GetComponent<AssetHolder>()._assetTableValue["head_center_offset_y"];
        CurrentUMAContainer.MergeModel();
        CurrentUMAContainer.SetHeight(-1);
        CurrentUMAContainer.Initialize(!UI.isTPose);

        if (!UI.isTPose && loadMotion)
        {
            if (Main.AbList.TryGetValue($"3d/motion/event/body/type00/anm_eve_type00_homestand{personality.PadLeft(2, '0')}_loop", out UmaDatabaseEntry entry))
            {
                LoadAsset(entry);
            }
        }
    }

    private void LoadMiniUma(CharaEntry chara, string costumeId)
    {
        int id = chara.Id;
        DataRow charaData = CurrentUMAContainer.CharaData;
        CurrentUMAContainer.IsMini = true;
        bool isGeneric = CurrentUMAContainer.IsGeneric = costumeId.Length >= 4;
        string skin = charaData["skin"].ToString(),
               height = charaData["height"].ToString(),
               socks = charaData["socks"].ToString(),
               bust = charaData["bust"].ToString(),
               sex = charaData["sex"].ToString(),
               costumeIdShort = "";
        bool customHead = true;

        UmaDatabaseEntry asset = null;
        if (isGeneric)
        {
            costumeIdShort = costumeId.Remove(costumeId.LastIndexOf('_'));
            string body = $"3d/chara/mini/body/mbdy{costumeIdShort}/pfb_mbdy{costumeId}_0";
            Main.AbList.TryGetValue(body,out asset);
        }
        else Main.AbList.TryGetValue($"3d/chara/mini/body/mbdy{id}_{costumeId}/pfb_mbdy{id}_{costumeId}",out asset);
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
        Main.AbList.TryGetValue(hair, out asset);
        if (costumeId != "00" && asset == null)
        {
            customHead = false;
            Main.AbList.TryGetValue($"3d/chara/mini/head/mchr{id}_00/pfb_mchr{id}_00_hair", out asset);
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
        Main.AbList.TryGetValue(head,out asset);
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
        var matList = new List<MeshRenderer>(CurrentUMAContainer.GetComponentsInChildren<MeshRenderer>());
        var eyemat = matList.FirstOrDefault(a => a.gameObject.name.Equals("M_Eye"));
        var mouthmat = matList.FirstOrDefault(a => a.gameObject.name.Equals("M_Mouth"));
        var eyebrowLmat = matList.FirstOrDefault(a => a.gameObject.name.Equals("M_Mayu_L"));
        var eyebrowRmat = matList.FirstOrDefault(a => a.gameObject.name.Equals("M_Mayu_R"));
        UI.LoadFacialPanelsMini(eyemat.material, eyebrowLmat.material, eyebrowRmat.material, mouthmat.material);

        if(Main.AbList.TryGetValue($"3d/motion/mini/event/body/chara/chr{id}_00/anm_min_eve_chr{id}_00_idle01_loop",out asset))
        {
            LoadAsset(asset);
        }
    }

    public void LoadProp(UmaDatabaseEntry entry)
    {
        UnloadProp();
        UmaAssetManager.UnloadAllBundle();

        CurrentOtherContainer = new GameObject(Path.GetFileName(entry.Name)).AddComponent<UmaContainer>();
        RecursiveLoadAsset(entry);
    }

    public void LoadLive(LiveEntry live, List<LiveCharacterSelect> characters)
    {
        GameObject MainLive = new GameObject("Live");
        GameObject Director = new GameObject("Director");
        List<GameObject> transferObjs = new List<GameObject>() {
                    MainLive,
                    GameObject.Find("CriWare"),
                    GameObject.Find("CriWareLibraryInitializer"),
                    GameObject.Find("ViewerMain"),
                    GameObject.Find("Directional Light"),
                    GameObject.Find("GlobalShaderController"),
                    GameObject.Find("AudioManager")
                };

        UmaSceneController.LoadScene("LiveScene",
            delegate ()
            {
                // Move the GameObject (you attach this in the Inspector) to the newly loaded Scene
                transferObjs.ForEach(o => SceneManager.MoveGameObjectToScene(o, SceneManager.GetSceneByName("LiveScene")));

                characters.ForEach(a =>
                {
                    if (a.CharaEntry == null || a.CostumeId == "")
                    {
                        //a.CharaEntry = Main.Characters[Random.Range(0, Main.Characters.Count)];
                        //a.CostumeId = "00";
                    }
                });//fill empty

                Gallop.Live.Director mController = Director.AddComponent<Gallop.Live.Director>();
                mController.live = live;
                Instantiate(mController, MainLive.transform);

                Destroy(Director);

                LoadLiveUma(characters);

            },
            delegate ()
            {
                Gallop.Live.Director.instance.InitializeUI();
                Gallop.Live.Director.instance.InitializeTimeline(characters, UI.LiveMode);
                Gallop.Live.Director.instance.Play(live.MusicId, characters);
            }
        );
    }



    //Use CriWare Library
    /*
    public CriAtomSource LoadLiveSoundCri(int songid, UmaDatabaseEntry SongAwb)
    {
        //清理
        if (CurrentAudioSources.Count > 0)
        {
            var tmp = CurrentAudioSources[0];
            CurrentAudioSources.Clear();
            Destroy(tmp.gameObject);
            UI.ResetAudioPlayer();
        }

        Debug.Log(SongAwb.Name);

        //获取总线数

        Debug.Log(CriAtomExAcf.GetNumDspSettings());

        string busName = CriAtomExAcf.GetDspSettingNameByIndex(0);

        var dspSetInfo = new CriAtomExAcf.AcfDspSettingInfo();



        Debug.Log(CriAtomExAcf.GetDspSettingInformation(busName, out dspSetInfo));

        Debug.Log(dspSetInfo.name);
        Debug.Log(dspSetInfo.numExtendBuses);
        Debug.Log(dspSetInfo.numBuses);
        Debug.Log(dspSetInfo.numSnapshots);
        Debug.Log(dspSetInfo.snapshotStartIndex);

        var busInfo = new CriAtomExAcf.AcfDspBusInfo();

        for(ushort i = 0; i < dspSetInfo.numExtendBuses; i++)
        {
            Debug.Log(i);
            Debug.Log(CriAtomExAcf.GetDspBusInformation(i, out busInfo));

            Debug.Log(busInfo.name);
            Debug.Log(busInfo.volume);
            Debug.Log(busInfo.numFxes);
            Debug.Log(busInfo.numBusLinks);
        }

        //获取Acb文件和Awb文件的路径
        string nameVar = SongAwb.Name.Split('.')[0].Split('/').Last();

        //使用Live的Bgm
        //nameVar = $"snd_bgm_live_{songid}_oke";

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
            Debug.Log(cueInfo.type);
            Debug.Log(cueInfo.userData);
        }

        //创建播放器
        CriAtomSource source = new GameObject("CuteAudioSource").AddComponent<CriAtomSource>();
        source.transform.SetParent(GameObject.Find("AudioManager/AudioControllerBgm").transform);
        source.cueSheet = nameVar;

        //播放
        source.Play(cueNameList[0]);

        return source;

        //source.SetBusSendLevelOffset(1, 1);


    }
    */

    //Use decrypt function
    public void LoadLiveSound(int songid, UmaDatabaseEntry SongAwb, bool needLyrics = true)
    {
        //load character voice
        if (SongAwb != null)
        {
            PlaySound(SongAwb);
        }

        //load BG
        string nameVar = $"snd_bgm_live_{songid}_oke";
        UmaDatabaseEntry BGawb = Main.AbSounds.FirstOrDefault(a => a.Name.Contains(nameVar) && a.Name.EndsWith("awb"));
        if (BGawb != null)
        {
            var BGclip = LoadAudio(BGawb);
            if (BGclip.Count > 0)
            {
                AddAudioSource(BGclip[0]);
            }
        }

        if (needLyrics)
        {
            LoadLiveLyrics(songid);
        }
    }

    public void PlaySound(UmaDatabaseEntry SongAwb, int subindex = -1)
    {
        if (CurrentAudioSources.Count > 0)
        {
            var tmp = CurrentAudioSources[0];
            CurrentAudioSources.Clear();
            Destroy(tmp.gameObject);
            UI.ResetAudioPlayer();
        }
        if (subindex == -1)
        {
            foreach (AudioClip clip in LoadAudio(SongAwb))
            {
                AddAudioSource(clip);
            }
        }
        else
        {
            AddAudioSource(LoadAudio(SongAwb)[subindex]);
        }

    }

    private void AddAudioSource(AudioClip clip)
    {
        AudioSource source;
        if (CurrentAudioSources.Count > 0)
        {
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
        string awbPath = awb.FilePath;
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
        UmaDatabaseEntry lyricsAsset = Main.AbList[lyricsVar];
        AssetBundle bundle = UmaAssetManager.LoadAssetBundle(lyricsAsset);
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

    public void RecursiveLoadAsset(UmaDatabaseEntry entry, bool IsSubAsset = false, Transform SetParent = null)
    {
        if (!string.IsNullOrEmpty(entry.Prerequisites))
        {
            foreach (string prerequisite in entry.Prerequisites.Split(';'))
            {
                RecursiveLoadAsset(Main.AbList[prerequisite], true, SetParent);
            }
        }
        LoadAsset(entry, IsSubAsset, SetParent);
    }

    public void LoadAsset(UmaDatabaseEntry entry, bool IsSubAsset = false, Transform SetParent = null)
    {
        Debug.Log("Loading " + entry.Name);
        var bundle = UmaAssetManager.LoadAssetBundle(entry, isRecursive: false);
        LoadBundle(bundle, IsSubAsset, SetParent);
    }

    private void LoadBundle(AssetBundle bundle, bool IsSubAsset = false, Transform SetParent = null)
    {
        if (bundle.name == "shader.a")
        {
            if (ShadersLoaded)
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
                bodyAlphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoontser.shader");
                bodyBehindAlphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoonbehindtser.shader");
                ShadersLoaded = true;
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
                        if (UI.LiveTime)
                        {
                            break;
                        }

                        if (CurrentUMAContainer && CurrentUMAContainer.UmaAnimator && CurrentUMAContainer.UmaAnimator.runtimeAnimatorController)
                        {
                            Debug.Log("LiveTime" + UI.LiveTime.ToString());
                            LoadAnimation(aClip);
                            break;
                        }

                        if (aClip.name.Contains("tear"))
                        {
                            break;
                        }
                        UmaAssetManager.UnloadAllBundle();
                        break;
                    }
                case GameObject go:
                    {
                        if (bundle.name.Contains("cloth"))
                        {
                            if (!CurrentUMAContainer.PhysicsContainer)
                            {
                                CurrentUMAContainer.PhysicsContainer = new GameObject("PhysicsController");
                                CurrentUMAContainer.PhysicsContainer.transform.SetParent(CurrentUMAContainer.transform);
                            }
                            Instantiate(go, CurrentUMAContainer.PhysicsContainer.transform);
                        }
                        else if (bundle.name.Contains("/head/"))
                        {
                            if (bundle.name.Contains("_hair") && !bundle.name.Contains("mchr"))
                            {
                                LoadHair(go);
                            }
                            else
                            {
                                LoadHead(go);
                            }
                        }
                        else if (bundle.name.Contains("/body/"))
                        {
                            LoadBody(go);
                        }
                        else if (bundle.name.Contains("/tail/"))
                        {
                            LoadTail(go);
                        }
                        else if (bundle.name.Contains("pfb_chr_tear"))
                        {
                            LoadTear(go);
                        }
                        else
                        {
                            if (!IsSubAsset)
                            {
                                LoadProp(go, SetParent);
                            }
                        }
                        break;
                    }
                case Shader sha:
                    if (!ShaderList.Contains(sha))
                    {
                        ShaderList.Add(sha);
                    }
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
                    else if (bundle.name.Contains("_face") || bundle.name.Contains("_hair"))
                    {
                        if (CurrentUMAContainer.IsMob)
                            CurrentUMAContainer.MobHeadTextures.Add(tex2D);
                    }
                    break;
            }
        }
    }

    private void LoadBody(GameObject go)
    {
        CurrentUMAContainer.Body = Instantiate(go, CurrentUMAContainer.transform);
        CurrentUMAContainer.UmaAnimator = CurrentUMAContainer.Body.GetComponent<Animator>();

        if (CurrentUMAContainer.IsMini)
        {
            CurrentUMAContainer.UpBodyBone = CurrentUMAContainer.Body.transform.Find("Position/Hip").gameObject;
        }
        else
        {
            CurrentUMAContainer.UpBodyBone = CurrentUMAContainer.Body.GetComponent<AssetHolder>()._assetTable["upbody_ctrl"] as GameObject;
        }

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

                        if (m.shader.name.Contains("Noline") && m.shader.name.Contains("TSER"))
                        {
                            var s = ShaderList.Find(a => a.name == m.shader.name.Replace("Noline", "")); //Generic costume shader need to change manually.
                            if (s)
                            {
                                m.shader = s;
                            }
                        }

                        //BodyAlapha's shader need to change manually.
                        if (m.name.Contains("bdy"))
                        {
                            if (m.name.Contains("Alpha"))
                            {
                                m.shader = bodyAlphaShader;
                            }
                            else
                            {
                                //some costume use area texture
                                var areaTex = UmaViewerMain.Instance.AbChara.FirstOrDefault(a => a.Name.StartsWith(UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/textures") && a.Name.EndsWith("area"));
                                if (areaTex != null)
                                {
                                    RecursiveLoadAsset(areaTex);
                                    m.SetTexture("_MaskColorTex", textures.FirstOrDefault(t => t.name.Contains(costumeIdShort) && t.name.EndsWith("area")));
                                    SetMaskColor(m, CurrentUMAContainer.IsMob ? CurrentUMAContainer.MobDressColor : CurrentUMAContainer.CharaData, CurrentUMAContainer.IsMob);
                                }
                            }
                        }

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
        else
        {
            foreach (Renderer r in CurrentUMAContainer.Body.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    //BodyAlapha's shader need to change manually.
                    if (m.name.Contains("bdy") && m.name.Contains("Alpha"))
                    {
                        m.shader = bodyAlphaShader;
                    }
                }
            }
        }
    }

    private void LoadHead(GameObject go)
    {
        var isMob = CurrentUMAContainer.IsMob;
        var textures = CurrentUMAContainer.MobHeadTextures;
        GameObject head = Instantiate(go, CurrentUMAContainer.transform);
        CurrentUMAContainer.Head = head;

        //Some setting for Head
        CurrentUMAContainer.EnableEyeTracking = UI.EnableEyeTracking;

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
                    if (isMob)
                    {
                        if (m.name.EndsWith("eye"))
                        {
                            m.SetTexture("_MainTex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("eye0")));
                            m.SetTexture("_High0Tex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("hi00")));
                            m.SetTexture("_High1Tex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("hi01")));
                            m.SetTexture("_High2Tex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("hi02")));
                            m.SetTexture("_MaskColorTex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("area")));
                            SetMaskColor(m, CurrentUMAContainer.MobHeadColor, "eye", false);
                        }
                        if (m.name.EndsWith("face"))
                        {
                            m.SetTexture("_MainTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("diff")));
                            m.SetTexture("_ToonMap", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("shad_c")));
                            m.SetTexture("_TripleMaskMap", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("base")));
                            m.SetTexture("_OptionMaskMap", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("ctrl")));
                            m.SetTexture("_MaskColorTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("area") && !t.name.Contains("_eye")));
                            SetMaskColor(m, CurrentUMAContainer.MobHeadColor, "mayu", true);
                        }
                        if (m.name.EndsWith("mayu"))
                        {
                            m.SetTexture("_MainTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("diff")));
                            m.SetTexture("_MaskColorTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("area") && !t.name.Contains("_eye")));
                            SetMaskColor(m, CurrentUMAContainer.MobHeadColor, "mayu", true);
                        }
                    }

                    //Glasses's shader need to change manually.
                    if (r.name.Contains("Hair") && r.name.Contains("Alpha"))
                    {
                        m.shader = alphaShader;
                    }

                    //Blush Setting
                    if (r.name.Contains("Cheek"))
                    {
                        if (isMob)
                        {
                            CurrentUMAContainer.CheekTex_0 = CurrentUMAContainer.MobHeadTextures.FindLast(a => a.name.Contains("cheek0"));
                            CurrentUMAContainer.CheekTex_1 = CurrentUMAContainer.MobHeadTextures.FindLast(a => a.name.Contains("cheek1"));
                        }
                        else
                        {
                            var table = CurrentUMAContainer.Head.GetComponent<AssetHolder>()._assetTable;
                            CurrentUMAContainer.CheekTex_0 = table["cheek0"] as Texture;
                            CurrentUMAContainer.CheekTex_1 = table["cheek1"] as Texture;
                        }
                    }
                    switch (m.shader.name)
                    {
                        case "Gallop/3D/Chara/MultiplyCheek":
                            m.shader = cheekShader; ;
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

        //shader effect
        var assetholder = head.GetComponent<AssetHolder>();
        if (assetholder)
        {
            CurrentUMAContainer.ShaderEffectData = assetholder._assetTable["chara_shader_effect"] as CharaShaderEffectData;
            if (CurrentUMAContainer.ShaderEffectData)
            {
                CurrentUMAContainer.ShaderEffectData.Initialize();
            }
        }
    }

    private void LoadHair(GameObject go)
    {
        GameObject hair = Instantiate(go, CurrentUMAContainer.transform);
        CurrentUMAContainer.Hair = hair;
        var textures = CurrentUMAContainer.MobHeadTextures;
        foreach (Renderer r in hair.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {

                //Glasses's shader need to change manually.
                if (r.name.Contains("Hair") && r.name.Contains("Alpha"))
                {
                    m.shader = alphaShader;
                }

                if (m.name.EndsWith("_hair"))
                {
                    m.SetTexture("_MainTex", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("diff")));
                    m.SetTexture("_ToonMap", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("shad_c")));
                    m.SetTexture("_TripleMaskMap", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("base")));
                    m.SetTexture("_OptionMaskMap", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("ctrl")));
                    m.SetTexture("_MaskColorTex", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("area")));
                    SetMaskColor(m, CurrentUMAContainer.MobHeadColor, "hair", true);
                }

                switch (m.shader.name)
                {
                    case "Gallop/3D/Chara/ToonHair/TSER":
                        m.shader = hairShader;
                        m.SetFloat("_CylinderBlend", 0.25f);
                        break;
                    default:
                        Debug.Log(m.shader.name);
                        // m.shader = Shader.Find("Nars/UmaMusume/Body");
                        break;
                }
            }
        }
    }

    private void LoadTail(GameObject go)
    {
        CurrentUMAContainer.Tail = Instantiate(go, CurrentUMAContainer.transform);
        var textures = CurrentUMAContainer.TailTextures;
        foreach (Renderer r in CurrentUMAContainer.Tail.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                m.SetTexture("_MainTex", textures.FirstOrDefault(t => t.name.EndsWith("diff")));
                m.SetTexture("_ToonMap", textures.FirstOrDefault(t => t.name.Contains("shad")));
                m.SetTexture("_TripleMaskMap", textures.FirstOrDefault(t => t.name.Contains("base")));
                m.SetTexture("_OptionMaskMap", textures.FirstOrDefault(t => t.name.Contains("ctrl")));
                if (CurrentUMAContainer.IsMob)
                {
                    SetMaskColor(m, CurrentUMAContainer.MobHeadColor, "tail", true);
                }
            }
        }
    }

    private void LoadProp(GameObject go, Transform SetParent = null)
    {
        var container = CurrentOtherContainer;
        var prop = Instantiate(go, SetParent ? SetParent : container.transform);

        /*
        foreach (Renderer r in prop.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                //Shaders can be differentiated by checking m.shader.name
                m.shader = Shader.Find("Unlit/Transparent Cutout");
            }
        }
        */
    }

    public void LoadAssetPath(string path, Transform SetParent)
    {
        RecursiveLoadAsset(UmaViewerMain.Instance.AbList[path], false, SetParent);
    }

    private void LoadTear(GameObject go)
    {

        if (CurrentUMAContainer)
        {
            if (go.name.EndsWith("000"))
            {
                CurrentUMAContainer.TearPrefab_0 = go;
            }
            else if (go.name.EndsWith("001"))
            {
                CurrentUMAContainer.TearPrefab_1 = go;
            }
        }
    }

    private void LoadFaceMorph(int id, string costumeId)
    {
        if (!CurrentUMAContainer.Head) return;
        var locatorEntry = Main.AbList["3d/animator/drivenkeylocator"];
        var bundle = UmaAssetManager.LoadAssetBundle(locatorEntry);
        var locator = Instantiate(bundle.LoadAsset("DrivenKeyLocator"), CurrentUMAContainer.transform) as GameObject;
        locator.name = "DrivenKeyLocator";

        var headBone = (GameObject)CurrentUMAContainer.Head.GetComponent<AssetHolder>()._assetTable["head"];
        var eyeLocator_L = headBone.transform.Find("Eye_target_locator_L");
        var eyeLocator_R = headBone.transform.Find("Eye_target_locator_R");

        var mangaEntry = new List<UmaDatabaseEntry>()
        {
            Main.AbList["3d/effect/charaemotion/pfb_eff_chr_emo_eye_000"],
            Main.AbList["3d/effect/charaemotion/pfb_eff_chr_emo_eye_001"],
            Main.AbList["3d/effect/charaemotion/pfb_eff_chr_emo_eye_002"],
            Main.AbList["3d/effect/charaemotion/pfb_eff_chr_emo_eye_003"],
        };
        var mangaObjects = new List<GameObject>();
        mangaEntry.ForEach(entry =>
        {
            AssetBundle ab = UmaAssetManager.LoadAssetBundle(entry);
            var obj = ab.LoadAsset(Path.GetFileNameWithoutExtension(entry.Name)) as GameObject;
            obj.SetActive(false);

            var leftObj = Instantiate(obj, eyeLocator_L.transform);
            new List<Renderer>(leftObj.GetComponentsInChildren<Renderer>()).ForEach(a => a.material.renderQueue = -1);
            CurrentUMAContainer.LeftMangaObject.Add(leftObj);

            var RightObj = Instantiate(obj, eyeLocator_R.transform);
            if (RightObj.TryGetComponent<AssetHolder>(out var holder))
            {
                if (holder._assetTableValue["invert"] > 0)
                    RightObj.transform.localScale = new Vector3(-1, 1, 1);
            }
            new List<Renderer>(RightObj.GetComponentsInChildren<Renderer>()).ForEach(a => { a.material.renderQueue = -1; });
            CurrentUMAContainer.RightMangaObject.Add(RightObj);
        });

        var tearEntry = new List<UmaDatabaseEntry>() {
            Main.AbList["3d/chara/common/tear/tear000/pfb_chr_tear000"],
            Main.AbList["3d/chara/common/tear/tear001/pfb_chr_tear001"],
        };

        if (tearEntry.Count > 0)
        {
            tearEntry.ForEach(a => RecursiveLoadAsset(a));
        }

        if (CurrentUMAContainer.TearPrefab_0 && CurrentUMAContainer.TearPrefab_1)
        {
            var p0 = CurrentUMAContainer.TearPrefab_0;
            var p1 = CurrentUMAContainer.TearPrefab_1;
            var t = headBone.transform;
            CurrentUMAContainer.TearControllers.Add(new TearController(t, Instantiate(p0, t), Instantiate(p1, t), 0, 1));
            CurrentUMAContainer.TearControllers.Add(new TearController(t, Instantiate(p0, t), Instantiate(p1, t), 1, 1));
            CurrentUMAContainer.TearControllers.Add(new TearController(t, Instantiate(p0, t), Instantiate(p1, t), 0, 0));
            CurrentUMAContainer.TearControllers.Add(new TearController(t, Instantiate(p0, t), Instantiate(p1, t), 1, 0));
        }

        var firsehead = CurrentUMAContainer.Head;
        var origin_faceDriven = firsehead.GetComponent<AssetHolder>()._assetTable["facial_target"] as FaceDrivenKeyTarget;
        var faceDriven = ScriptableObject.CreateInstance<FaceDrivenKeyTarget>();
        faceDriven._eyebrowTarget = origin_faceDriven._eyebrowTarget;
        faceDriven._eyeTarget = origin_faceDriven._eyeTarget;
        faceDriven._mouthTarget = origin_faceDriven._mouthTarget;
        faceDriven.name = $"char{id}_{costumeId}_face_target";

        var earDriven = firsehead.GetComponent<AssetHolder>()._assetTable["ear_target"] as DrivenKeyTarget;
        var faceOverride = firsehead.GetComponent<AssetHolder>()._assetTable["face_override"] as FaceOverrideData;
        faceDriven._earTarget = earDriven._targetFaces;
        CurrentUMAContainer.FaceDrivenKeyTarget = faceDriven;
        CurrentUMAContainer.FaceDrivenKeyTarget.Container = CurrentUMAContainer;
        CurrentUMAContainer.FaceOverrideData = faceOverride;
        faceOverride?.SetEnable(UI.EnableFaceOverride);
        faceDriven.DrivenKeyLocator = locator.transform;
        faceDriven.Initialize(UmaUtility.ConvertArrayToDictionary(firsehead.GetComponentsInChildren<Transform>()));

        var emotionDriven = ScriptableObject.CreateInstance<FaceEmotionKeyTarget>();
        emotionDriven.name = $"char{id}_{costumeId}_emotion_target";
        CurrentUMAContainer.FaceEmotionKeyTarget = emotionDriven;
        emotionDriven.FaceDrivenKeyTarget = faceDriven;
        emotionDriven.FaceEmotionKey = UmaDatabaseController.Instance.FaceTypeData;
        emotionDriven.Initialize();
    }

    private void LoadAnimation(AnimationClip clip)
    {
        if (clip.name.EndsWith("_S"))
        {
            CurrentUMAContainer.OverrideController["clip_s"] = clip;
        }
        else if (clip.name.EndsWith("_E"))
        {
            CurrentUMAContainer.OverrideController["clip_e"] = clip;
        }
        else if (clip.name.Contains("tail"))
        {
            if (CurrentUMAContainer.IsMini) return;
            CurrentUMAContainer.UpBodyReset();
            CurrentUMAContainer.OverrideController["clip_t"] = clip;
            CurrentUMAContainer.UmaAnimator.Play("motion_t", 1, 0);
        }
        else if (clip.name.Contains("face"))
        {
            if (CurrentUMAContainer.IsMini) return;
            LoadFaceAnimation(clip);
        }
        else if (clip.name.Contains("ear"))
        {
            if (CurrentUMAContainer.IsMini) return;
            LoadEarAnimation(clip);
        }
        else if (clip.name.Contains("pos"))
        {
            if (CurrentUMAContainer.IsMini) return;
            CurrentUMAContainer.OverrideController["clip_p"] = clip;
            CurrentUMAContainer.UmaAnimator.Play("motion_1", 2, 0);
        }
        else if (clip.name.Contains("cam"))
        {
            SetPreviewCamera(clip);
        }
        else if (clip.name.Contains("_loop"))
        {
            CurrentUMAContainer.UpBodyReset();
            if (CurrentUMAContainer.isAnimatorControl && CurrentUMAContainer.FaceDrivenKeyTarget)
            {
                CurrentUMAContainer.FaceDrivenKeyTarget.ResetLocator();
                CurrentUMAContainer.isAnimatorControl = false;
            }

            var facialMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name + "_face"));
            if (facialMotion != null)
            {
                RecursiveLoadAsset(facialMotion);
            }
            var earMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name + "_ear"));
            if (earMotion != null)
            {
                RecursiveLoadAsset(earMotion);
            }

            UmaDatabaseEntry motion_e = null, motion_s = null;
            motion_s = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name.Replace("_loop", "_s")));
            if (motion_s != null)
            {
                RecursiveLoadAsset(motion_s);
            }

            if (CurrentUMAContainer.OverrideController["clip_2"].name.Contains("_loop"))
            {
                motion_e = Main.AbMotions.FirstOrDefault(a =>
                a.Name.EndsWith(CurrentUMAContainer.OverrideController["clip_2"].name.Replace("_loop", "_e"))
                && !a.Name.Contains("hom_")); //home end animation not for interpolation

                if (motion_e != null)
                {
                    RecursiveLoadAsset(motion_e);
                }
            }

            SetPreviewCamera(null);
            CurrentUMAContainer.OverrideController["clip_1"] = CurrentUMAContainer.OverrideController["clip_2"];
            CurrentUMAContainer.OverrideController["clip_2"] = clip;

            CurrentUMAContainer.UmaAnimator.Play("motion_1", -1, 0);
            CurrentUMAContainer.UmaAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e" : ((motion_s != null) ? "next_s" : "next"));
        }
        else
        {
            if (CurrentUMAContainer.FaceDrivenKeyTarget)
            {
                CurrentUMAContainer.FaceDrivenKeyTarget.ResetLocator();
                CurrentUMAContainer.isAnimatorControl = false;
            }
            CurrentUMAContainer.UpBodyReset();
            CurrentUMAContainer.UmaAnimator.Rebind();
            CurrentUMAContainer.OverrideController["clip_2"] = clip;
            // If Cut-in, play immediately without state interpolation
            if (clip.name.Contains("crd") || clip.name.Contains("res_chr"))
            {
                var facialMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name + "_face"));
                var cameraMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name + "_cam"));
                var earMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name + "_ear"));
                var posMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name + "_pos"));

                if (facialMotion != null)
                {
                    RecursiveLoadAsset(facialMotion);
                }

                if (earMotion != null)
                {
                    RecursiveLoadAsset(earMotion);
                }

                if (cameraMotion != null)
                {
                    RecursiveLoadAsset(cameraMotion);
                }

                if (posMotion != null)
                {
                    RecursiveLoadAsset(posMotion);
                }

                if (CurrentUMAContainer.IsMini)
                {
                    SetPreviewCamera(null);
                }

                if (clip.name.Contains("cti_crd"))
                {
                    string[] param = clip.name.Split('_');
                    if (param.Length > 4)
                    {
                        int index = int.Parse(param[4]);
                        if (index == 1)
                        {
                            foreach (var entry in Main.AbMotions.Where(a => a.Name.Contains($"{param[0]}_{param[1]}_{param[2]}_{param[3]}_")))
                            {
                                UmaAssetManager.LoadAssetBundle(entry, isRecursive: false);
                            }
                        }
                        index++;
                        var nextMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith($"{param[0]}_{param[1]}_{param[2]}_{param[3]}_0{index}"));

                        var aevent = new AnimationEvent
                        {
                            time = clip.length * 0.99f,
                            stringParameter = (nextMotion != null ? nextMotion.Name : null),
                            functionName = (nextMotion != null ? "SetNextAnimationCut" : "SetEndAnimationCut")
                        };
                        clip.AddEvent(aevent);
                    }
                }

                CurrentUMAContainer.UmaAnimator.Play("motion_2", 0, 0);
            }
            else
            {
                SetPreviewCamera(null);

                //Some animations have facial animation
                var facialMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name + "_face"));
                if (facialMotion != null)
                {
                    RecursiveLoadAsset(facialMotion);
                }
                var earMotion = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name + "_ear"));
                if (earMotion != null)
                {
                    RecursiveLoadAsset(earMotion);
                }

                CurrentUMAContainer.UmaAnimator.Play("motion_2", 0, 0);
            }
        }
    }

    public void LoadFaceAnimation(AnimationClip clip)
    {
        if (clip.name.Contains("_S"))
        {
            CurrentUMAContainer.FaceOverrideController["clip_s"] = clip;
        }
        else if (clip.name.Contains("_E"))
        {
            CurrentUMAContainer.FaceOverrideController["clip_e"] = clip;
        }
        else if (clip.name.Contains("_loop"))
        {
            CurrentUMAContainer.isAnimatorControl = true;
            CurrentUMAContainer.FaceDrivenKeyTarget.ResetLocator();
            UmaDatabaseEntry motion_e = null, motion_s = null;
            motion_s = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name.Replace("_loop", "_s")));
            if (motion_s != null)
            {
                RecursiveLoadAsset(motion_s);
            }

            if (CurrentUMAContainer.FaceOverrideController["clip_2"].name.Contains("_loop"))
            {
                motion_e = Main.AbMotions.FirstOrDefault(a =>
                a.Name.EndsWith(CurrentUMAContainer.FaceOverrideController["clip_2"].name.Replace("_loop", "_e"))
                && !a.Name.Contains("hom_"));

                if (motion_e != null)
                {
                    RecursiveLoadAsset(motion_e);
                }
            }

            CurrentUMAContainer.FaceOverrideController["clip_1"] = CurrentUMAContainer.FaceOverrideController["clip_2"];
            CurrentUMAContainer.FaceOverrideController["clip_2"] = clip;
            CurrentUMAContainer.UmaFaceAnimator.Play("motion_1", 0, 0);
            CurrentUMAContainer.UmaFaceAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e" : ((motion_s != null) ? "next_s" : "next"));
        }
        else
        {
            CurrentUMAContainer.isAnimatorControl = true;
            CurrentUMAContainer.FaceDrivenKeyTarget.ResetLocator();
            CurrentUMAContainer.FaceOverrideController["clip_2"] = clip;
            CurrentUMAContainer.UmaFaceAnimator.Play("motion_2", 0, 0);
        }
    }

    public void LoadEarAnimation(AnimationClip clip)
    {
        if (clip.name.Contains("_S"))
        {
            CurrentUMAContainer.FaceOverrideController["clip_s_ear"] = clip;
        }
        else if (clip.name.Contains("_E"))
        {
            CurrentUMAContainer.FaceOverrideController["clip_e_ear"] = clip;
        }
        else if (clip.name.Contains("_loop"))
        {
            UmaDatabaseEntry motion_e = null, motion_s = null;
            motion_s = Main.AbMotions.FirstOrDefault(a => a.Name.EndsWith(clip.name.Replace("_loop", "_s")));
            if (motion_s != null)
            {
                RecursiveLoadAsset(motion_s);
            }

            if (CurrentUMAContainer.FaceOverrideController["clip_2_ear"].name.Contains("_loop"))
            {
                motion_e = Main.AbMotions.FirstOrDefault(a =>
                a.Name.EndsWith(CurrentUMAContainer.FaceOverrideController["clip_2_ear"].name.Replace("_loop", "_e"))
                && !a.Name.Contains("hom_"));

                if (motion_e != null)
                {
                    RecursiveLoadAsset(motion_e);
                }
            }

            CurrentUMAContainer.FaceOverrideController["clip_1_ear"] = CurrentUMAContainer.FaceOverrideController["clip_2_ear"];
            CurrentUMAContainer.FaceOverrideController["clip_2_ear"] = clip;
            CurrentUMAContainer.UmaFaceAnimator.Play("motion_1", 1, 0);
            CurrentUMAContainer.UmaFaceAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e_ear" : ((motion_s != null) ? "next_s_ear" : "next_ear"));
        }
        else
        {
            if (CurrentUMAContainer.FaceOverrideController["clip_2"].name == "clip_2")
            {
                CurrentUMAContainer.isAnimatorControl = true;
                CurrentUMAContainer.FaceDrivenKeyTarget.ResetLocator();
            }
            CurrentUMAContainer.FaceOverrideController["clip_2_ear"] = clip;
            CurrentUMAContainer.UmaFaceAnimator.Play("motion_2", 1, 0);
        }
    }

    public void SetPreviewCamera(AnimationClip clip)
    {
        if (clip)
        {
            if (!AnimationCameraAnimator.runtimeAnimatorController)
            {
                AnimationCameraAnimator.runtimeAnimatorController = Instantiate(CameraOverrideController);
            }
            (AnimationCameraAnimator.runtimeAnimatorController as AnimatorOverrideController)["clip_1"] = clip;
            AnimationCamera.enabled = true;
            AnimationCameraAnimator.Play("motion_1", 0, 0);
            CurrentUMAContainer.SetHeight(0);
        }
        else
        {
            AnimationCamera.enabled = false;
            CurrentUMAContainer.SetHeight(-1);
        }
    }

    public Sprite LoadCharaIcon(string id)
    {
        string value = $"chara/chr{id}/chr_icon_{id}";
        if (UmaViewerMain.Instance.AbList.TryGetValue(value, out UmaDatabaseEntry entry))
        {
            AssetBundle assetBundle = UmaAssetManager.LoadAssetBundle(entry, true);
            if (assetBundle.Contains($"chr_icon_{id}"))
            {
                Texture2D texture = (Texture2D)assetBundle.LoadAsset($"chr_icon_{id}");
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                assetBundle.Unload(false);
                return sprite;
            }
        }
        return null;
    }

    public Sprite LoadMobCharaIcon(string id)
    {
        string value = $"mob/mob_chr_icon_{id}_000001_01";
        if (UmaViewerMain.Instance.AbList.TryGetValue(value, out UmaDatabaseEntry entry))
        {
            string path = entry.FilePath;
            AssetBundle assetBundle = UmaAssetManager.LoadAssetBundle(entry, true);
            if (assetBundle.Contains($"mob_chr_icon_{id}_000001_01"))
            {
                Texture2D texture = (Texture2D)assetBundle.LoadAsset($"mob_chr_icon_{id}_000001_01");
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                assetBundle.Unload(false);
                return sprite;
            }
        }
        return null;
    }

    public Sprite LoadSprite(UmaDatabaseEntry item)
    {
        AssetBundle assetBundle = UmaAssetManager.LoadAssetBundle(item, true);
        Texture2D texture = (Texture2D)assetBundle.LoadAsset(assetBundle.GetAllAssetNames()[0]);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        assetBundle.Unload(false);
        return sprite;
    }

    public Sprite LoadLiveIcon(int musicid)
    {
        string value = $"live/jacket/jacket_icon_l_{musicid}";

        if (UmaViewerMain.Instance.AbList.TryGetValue(value, out UmaDatabaseEntry entry))
        {
            AssetBundle assetBundle = UmaAssetManager.LoadAssetBundle(entry, true);
            if (assetBundle.Contains($"jacket_icon_l_{musicid}"))
            {
                Texture2D texture = (Texture2D)assetBundle.LoadAsset($"jacket_icon_l_{musicid}");
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                assetBundle.Unload(false);
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

    public void UnloadUma()
    {
        if (CurrentUMAContainer != null)
        {
            //It seems that OnDestroy will executed after new model loaded, which cause new FacialPanels empty...
            UmaViewerUI.Instance.currentFaceDrivenKeyTarget = null;
            UmaViewerUI.Instance.LoadEmotionPanels(null);
            UmaViewerUI.Instance.LoadFacialPanels(null);
            if (UmaViewerUI.Instance.MaterialsList)
                foreach (Transform t in UmaViewerUI.Instance.MaterialsList.content)
                {
                    Destroy(t.gameObject);
                }

            if (CurrentUMAContainer.transform.parent && CurrentUMAContainer.transform.parent.name.Contains("Root"))
            {
                Destroy(CurrentUMAContainer.transform.parent.gameObject);
            }
            else
            {
                Destroy(CurrentUMAContainer.gameObject);
            }
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
            if (CurrentUMAContainer.FaceDrivenKeyTarget)
            {
                CurrentUMAContainer.FaceDrivenKeyTarget.ClearAllWeights();
                CurrentUMAContainer.FaceDrivenKeyTarget.ChangeMorph();
            }
        }
    }

    private void SetMaskColor(Material mat, DataRow colordata, bool IsMob)
    {
        mat.EnableKeyword("USE_MASK_COLOR");
        Color c1, c2, c3, c4, c5, c6, t1, t2, t3, t4, t5, t6;
        if (IsMob)
        {
            ColorUtility.TryParseHtmlString(colordata["color_r1"].ToString(), out c1);
            ColorUtility.TryParseHtmlString(colordata["color_r2"].ToString(), out c2);
            ColorUtility.TryParseHtmlString(colordata["color_g1"].ToString(), out c3);
            ColorUtility.TryParseHtmlString(colordata["color_g2"].ToString(), out c4);
            ColorUtility.TryParseHtmlString(colordata["color_b1"].ToString(), out c5);
            ColorUtility.TryParseHtmlString(colordata["color_b2"].ToString(), out c6);
            ColorUtility.TryParseHtmlString(colordata["toon_color_r1"].ToString(), out t1);
            ColorUtility.TryParseHtmlString(colordata["toon_color_r2"].ToString(), out t2);
            ColorUtility.TryParseHtmlString(colordata["toon_color_g1"].ToString(), out t3);
            ColorUtility.TryParseHtmlString(colordata["toon_color_g2"].ToString(), out t4);
            ColorUtility.TryParseHtmlString(colordata["toon_color_b1"].ToString(), out t5);
            ColorUtility.TryParseHtmlString(colordata["toon_color_b2"].ToString(), out t6);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#" + colordata["image_color_main"].ToString(), out c1);
            ColorUtility.TryParseHtmlString("#" + colordata["image_color_sub"].ToString(), out c2);
            ColorUtility.TryParseHtmlString("#" + colordata["ui_color_sub"].ToString(), out c3);
            ColorUtility.TryParseHtmlString("#" + colordata["ui_color_sub"].ToString(), out c4);
            ColorUtility.TryParseHtmlString("#" + colordata["ui_training_color_1"].ToString(), out c5);
            ColorUtility.TryParseHtmlString("#" + colordata["ui_training_color_2"].ToString(), out c6);
            float toonstrength = 0.8f;
            t1 = c1 * toonstrength;
            t2 = c2 * toonstrength;
            t3 = c3 * toonstrength;
            t4 = c4 * toonstrength;
            t5 = c5 * toonstrength;
            t6 = c6 * toonstrength;
        }

        mat.SetColor("_MaskColorR1", c1);
        mat.SetColor("_MaskColorR2", c2);
        mat.SetColor("_MaskColorG1", c3);
        mat.SetColor("_MaskColorG2", c4);
        mat.SetColor("_MaskColorB1", c5);
        mat.SetColor("_MaskColorB2", c6);
        mat.SetColor("_MaskToonColorR1", t1);
        mat.SetColor("_MaskToonColorR2", t2);
        mat.SetColor("_MaskToonColorG1", t3);
        mat.SetColor("_MaskToonColorG2", t4);
        mat.SetColor("_MaskToonColorB1", t5);
        mat.SetColor("_MaskToonColorB2", t6);
    }

    private void SetMaskColor(Material mat, DataRow colordata, string prefix, bool hastoon)
    {
        mat.EnableKeyword("USE_MASK_COLOR");
        Color c1, c2, c3, c4, c5, c6, t1, t2, t3, t4, t5, t6;
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_r1"].ToString(), out c1);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_r2"].ToString(), out c2);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_g1"].ToString(), out c3);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_g2"].ToString(), out c4);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_b1"].ToString(), out c5);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_b2"].ToString(), out c6);
        mat.SetColor("_MaskColorR1", c1);
        mat.SetColor("_MaskColorR2", c2);
        mat.SetColor("_MaskColorG1", c3);
        mat.SetColor("_MaskColorG2", c4);
        mat.SetColor("_MaskColorB1", c5);
        mat.SetColor("_MaskColorB2", c6);
        if (hastoon)
        {
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_r1"].ToString(), out t1);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_r2"].ToString(), out t2);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_g1"].ToString(), out t3);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_g2"].ToString(), out t4);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_b1"].ToString(), out t5);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_b2"].ToString(), out t6);
            mat.SetColor("_MaskToonColorR1", t1);
            mat.SetColor("_MaskToonColorR2", t2);
            mat.SetColor("_MaskToonColorG1", t3);
            mat.SetColor("_MaskToonColorG2", t4);
            mat.SetColor("_MaskToonColorB1", t5);
            mat.SetColor("_MaskToonColorB2", t6);
        }
    }

    public void UnloadAllBundle() => UmaAssetManager.UnloadAllBundle(true);

    private void FixedUpdate()
    {
        if (AnimationCamera && AnimationCamera.enabled == true)
        {
            var fov = AnimationCamera.gameObject.transform.parent.transform.localScale.x;
            AnimationCamera.fieldOfView = fov;
        }
    }
}
