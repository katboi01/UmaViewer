using CriWareFormats;
using Gallop;
using Gallop.Live;
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UmaMusumeAudio;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class UmaViewerBuilder : MonoBehaviour
{
    public static UmaViewerBuilder Instance;
    static UmaViewerMain Main => UmaViewerMain.Instance;
    static UmaViewerUI UI => UmaViewerUI.Instance;
    static UISettingsModel ModelSettings => UmaViewerUI.Instance.ModelSettings;

    public List<Shader> ShaderList = new List<Shader>();
    public Material TransMaterialCharas;
    public UmaContainerCharacter CurrentUMAContainer;
    public UmaContainer CurrentOtherContainer;

    public UmaHeadData CurrentHead;

    public List<AudioSource> CurrentAudioSources = new List<AudioSource>();
    public List<UmaLyricsData> CurrentLyrics = new List<UmaLyricsData>();

    // Used for keeping track for exports
    public List<UmaDatabaseEntry> CurrentLiveSoundAWB = new List<UmaDatabaseEntry>();
    public int CurrentLiveSoundAWBIndex = -1;

    public AnimatorOverrideController OverrideController;
    public AnimatorOverrideController FaceOverrideController;
    public AnimatorOverrideController CameraOverrideController;
    public Animator AnimationCameraAnimator;
    public Camera AnimationCamera;

    public GameObject LiveControllerPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator LoadUma(CharaEntry chara, string costumeId, bool mini, string haedCostumeId = "")
    {
        int id = chara.Id;
        var umaContainer = new GameObject($"Chara_{id}_{costumeId}").AddComponent<UmaContainerCharacter>();
        CurrentUMAContainer = umaContainer;

        if (mini)
        {
            umaContainer.CharaData = UmaDatabaseController.ReadCharaData(chara);
            LoadMiniUma(umaContainer, chara, costumeId);
        }
        else if (chara.IsMob)
        {
            umaContainer.CharaData = UmaDatabaseController.ReadCharaData(chara);
            LoadMobUma(umaContainer, chara, costumeId, loadMotion: true);
        }
        else if (ModelSettings.IsHeadFix && CurrentHead != null && CurrentHead.chara.IsMob)
        {
            umaContainer.CharaData = UmaDatabaseController.ReadCharaData(CurrentHead.chara);
            LoadMobUma(umaContainer, CurrentHead.chara, costumeId, chara.Id, true);
        }
        else
        {
            umaContainer.CharaData = UmaDatabaseController.ReadCharaData(chara);
            LoadNormalUma(umaContainer, chara, costumeId, true, haedCostumeId);
        }

        yield break;
    }

    public void LoadLiveUma(List<LiveCharacterSelect> characters)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].CharaEntry.Name != "")
            {
                var umaContainer = new GameObject($"Chara_{characters[i].CharaEntry.Id}_{characters[i].CostumeId}").AddComponent<UmaContainerCharacter>();
                umaContainer.IsLive = true;
                umaContainer.CharaData = UmaDatabaseController.ReadCharaData(characters[i].CharaEntry);
                var charObjs = Gallop.Live.Director.instance.charaObjs;
                umaContainer.transform.parent = charObjs[i];
                umaContainer.transform.localPosition = new Vector3();

                if (characters[i].CharaEntry.IsMob)
                {
                    LoadMobUma(umaContainer, characters[i].CharaEntry, characters[i].CostumeId);
                }
                else
                {
                    LoadNormalUma(umaContainer, characters[i].CharaEntry, characters[i].CostumeId, false, characters[i].HeadCostumeId);
                }

                Gallop.Live.Director.instance.CharaContainerScript.Add(umaContainer);
            }
        }
    }

    private void LoadNormalUma(UmaContainerCharacter umaContainer, CharaEntry chara, string costumeId, bool loadMotion = false, string haedCostumeId = "")
    {
        int id = chara.Id;
        umaContainer.CharaEntry = chara;
        DataRow charaData = umaContainer.CharaData;
        bool genericCostume = umaContainer.IsGeneric = costumeId.Length >= 4;
        string skin, height, socks, bust, sex, shape, costumeIdShort = "";
        skin = charaData["skin"].ToString();
        height = charaData["height"].ToString();
        socks = charaData["socks"].ToString();
        bust = charaData["bust"].ToString();
        sex = charaData["sex"].ToString();
        shape = charaData["shape"].ToString();

        UmaDatabaseEntry asset = null;

        umaContainer.VarCostumeIdLong = costumeId;
        if (genericCostume)
        {
            costumeIdShort = costumeId.Remove(costumeId.LastIndexOf('_'));
            umaContainer.VarCostumeIdShort = costumeIdShort;
            umaContainer.VarBust = bust;
            umaContainer.VarSkin = skin;
            umaContainer.VarSocks = socks;
            umaContainer.VarHeight = height;

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
            UmaViewerUI.Instance?.ShowMessage("No body, can't load!", UIMessageType.Error);
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
                umaContainer.LoadTextures(asset1);
            }
            //Load Body
            umaContainer.LoadBody(asset);

            //Load Physics
            if (Main.AbList.TryGetValue(UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_cloth00", out _))
            {
                var asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_cloth00"];
                umaContainer.LoadPhysics(asset1);
                asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_bust{bust}_cloth00"];
                umaContainer.LoadPhysics(asset1);
            }
        }
        else
        {
            umaContainer.LoadBody(asset);
            //Load Physics
            var asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{id}_{costumeId}/clothes/pfb_bdy{id}_{costumeId}_cloth00"];
            umaContainer.LoadPhysics(asset1);
            asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{id}_{costumeId}/clothes/pfb_bdy{id}_{costumeId}_bust_cloth00"];
            umaContainer.LoadPhysics(asset1);
        }

        // Record Head Data
        int head_id;
        string head_costumeId;
        int tailId = Convert.ToInt32(charaData["tail_model_id"]);

        if (ModelSettings.IsHeadFix && CurrentHead != null)
        {
            head_id = CurrentHead.id;
            head_costumeId = CurrentHead.costumeId;
            tailId = CurrentHead.tailId;
        }
        else
        {
            head_id = id;
            head_costumeId = string.IsNullOrEmpty(haedCostumeId) ? costumeId  : haedCostumeId;

            CurrentHead = new UmaHeadData
            {
                id = id,
                costumeId = head_costumeId,
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
                umaContainer.LoadTextures(asset1);
            }

            //Load Head
            umaContainer.LoadHead(asset);

            //Load Physics
            if (isDefaultHead)
            {
                var asset1 = Main.AbList[UmaDatabaseController.HeadPath + $"chr{id}_00/clothes/pfb_chr{id}_00_cloth00"];
                umaContainer.LoadPhysics(asset1);
            }
            else
            {
                var asset1 = Main.AbList[UmaDatabaseController.HeadPath + $"chr{head_id}_{head_costumeId}/clothes/pfb_chr{head_id}_{head_costumeId}_cloth00"];
                umaContainer.LoadPhysics(asset1);
            }
        }



        //修改(载入专用尾巴相关)
        if (tailId > 0)
        {
            string costumePrefixForTail = $"{id}_{costumeId}";
            string exclusiveTailName = $"tail{costumePrefixForTail}";
            string exclusiveTailPath = $"3d/chara/tail/{exclusiveTailName}/";
            string exclusiveTailPfb = $"{exclusiveTailPath}pfb_{exclusiveTailName}";

            if (Main.AbList.TryGetValue(exclusiveTailPfb, out asset))
            {
                umaContainer.LoadExclusiveTail(asset);

                string clothPath = $"{exclusiveTailPath}clothes/pfb_{exclusiveTailName}_cloth00";
                if (Main.AbList.TryGetValue(clothPath, out var clothAsset))
                {
                    umaContainer.LoadPhysics(clothAsset);
                }
            }
            else
            {
                string tailName = $"tail{tailId.ToString().PadLeft(4, '0')}_00";
                string tailPath = $"3d/chara/tail/{tailName}/";
                string tailPfb = $"{tailPath}pfb_{tailName}";
                asset = null;

                if (Main.AbList.TryGetValue(tailPfb, out asset) && asset != null)
                {
                    foreach (var asset2 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_{head_id}") || a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_0000")))
                    {
                        umaContainer.LoadTextures(asset2);
                    }

                    umaContainer.LoadTail(asset);

                    string clothPath = $"{tailPath}clothes/pfb_{tailName}_cloth00";
                    if (Main.AbList.TryGetValue(clothPath, out var clothAsset))
                    {
                        umaContainer.LoadPhysics(clothAsset);
                    }
                }
                else
                {
                    Debug.Log("no tail");
                }
            }
        }



        umaContainer.LoadPhysics();
        umaContainer.SetDynamicBoneEnable(ModelSettings.DynamicBoneEnable);
        umaContainer.LoadFaceMorph(id, costumeId);
        umaContainer.TearControllers.ForEach(a => a.SetDir(a.CurrentDir));
        umaContainer.HeadBone = (GameObject)umaContainer.Body.GetComponent<AssetHolder>()._assetTable["head"];
        umaContainer.EyeHeight = umaContainer.Head.GetComponent<AssetHolder>()._assetTableValue["head_center_offset_y"];
        umaContainer.MergeModel();
        umaContainer.SetHeight(-1);
        umaContainer.Initialize(!ModelSettings.IsTPose);

        umaContainer.Position = umaContainer.transform.Find("Position");
        umaContainer.SetupBoneHandles();

        if (!ModelSettings.IsTPose && loadMotion)
        {
            if (Main.AbList.TryGetValue($"3d/motion/event/body/chara/chr{id}_00/anm_eve_chr{id}_00_idle01_loop", out UmaDatabaseEntry entry))
            {
                umaContainer.LoadAnimation(entry);
            }
        }


        //修改(载入通用服装ColorSet相关)
        UI.ClearColorSetButtons();
        UI.LoadColorSetButtons();


    }

    private void LoadMobUma(UmaContainerCharacter umaContainer, CharaEntry chara, string costumeId, int bodyid = -1, bool loadMotion = false)
    {
        int id = chara.Id;
        umaContainer.CharaEntry = chara;
        umaContainer.IsMob = chara.IsMob;
        umaContainer.MobDressColor = UmaDatabaseController.ReadMobDressColor(umaContainer.CharaData["dress_color_id"].ToString());
        umaContainer.MobHeadColor = UmaDatabaseController.ReadMobHairColor(umaContainer.CharaData["chara_hair_color"].ToString());

        DataRow charaData = umaContainer.CharaData;
        bool genericCostume = umaContainer.IsGeneric = costumeId.Length >= 4;
        string skin, height, socks, bust, sex, shape, costumeIdShort = "";
        string faceid, hairid, personality;

        faceid = charaData["chara_face_model"].ToString();
        hairid = charaData["chara_hair_model"].ToString();
        personality = charaData["default_personality"].ToString();
        skin = charaData["chara_skin_color"].ToString();
        height = "1";
        socks = charaData["socks"].ToString();
        bust = charaData["chara_bust_size"].ToString();
        sex = charaData["sex"].ToString();
        shape = "0";

        UmaDatabaseEntry asset = null;
        if (genericCostume)
        {
            costumeIdShort = costumeId.Remove(costumeId.LastIndexOf('_'));
            umaContainer.VarCostumeIdShort = costumeIdShort;
            umaContainer.VarCostumeIdLong = costumeId;
            umaContainer.VarBust = bust;
            umaContainer.VarSkin = skin;
            umaContainer.VarSocks = socks;
            umaContainer.VarHeight = height;

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
            UmaViewerUI.Instance?.ShowMessage("No body, can't load!", UIMessageType.Error);
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
                umaContainer.LoadTextures(asset1);
            }

            //Load Body
            umaContainer.LoadBody(asset);

            //Load Physics
            if (Main.AbList.TryGetValue(UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_cloth00", out _))
            {
                var asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_cloth00"];
                umaContainer.LoadPhysics(asset1);
                asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/clothes/pfb_bdy{costumeIdShort}_bust{bust}_cloth00"];
                umaContainer.LoadPhysics(asset1);
            }
        }
        else
        {
            umaContainer.LoadBody(asset);
            //Load Physics
            var asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{bodyid}_{costumeId}/clothes/pfb_bdy{bodyid}_{costumeId}_cloth00"];
            umaContainer.LoadPhysics(asset1);
            asset1 = Main.AbList[UmaDatabaseController.BodyPath + $"bdy{bodyid}_{costumeId}/clothes/pfb_bdy{bodyid}_{costumeId}_bust_cloth00"];
            umaContainer.LoadPhysics(asset1);
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
                    umaContainer.LoadTextures(asset1);
                }

                if (asset1.Name.Contains($"tex_chr{head_s}_{head_costumeId}_hair{hairid.PadLeft(3, '0')}"))
                {
                    umaContainer.LoadTextures(asset1);
                }
            }

            //Load face
            umaContainer.LoadHead(asset);
            //Load hair
            umaContainer.LoadHair(hairasset);

            umaContainer.MergeHairModel();

            //Load Physics
            if (isDefaultHead)
            {
                if (Main.AbList.TryGetValue($"{UmaDatabaseController.HeadPath}chr{head_s}_00/clothes/pfb_chr{head_s}_00_hair{hairid.PadLeft(3, '0')}_cloth00", out UmaDatabaseEntry asset1))
                {
                    umaContainer.LoadPhysics(asset1);
                }
            }
        }

        if (tailId > 0)
        {
            string tailName = $"tail{tailId.ToString().PadLeft(4, '0')}_00";
            string tailPath = $"3d/chara/tail/{tailName}/";
            string tailPfb = tailPath + $"pfb_{tailName}";
            asset = UmaViewerMain.Instance.AbChara.FirstOrDefault(a => a.Name == tailPfb);
            if (asset != null)
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_{head_s}") || a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_0000")))
                {
                    umaContainer.LoadTextures(asset1);
                }
                umaContainer.LoadTail(asset);


                //Load Physics
                if (Main.AbList.TryGetValue($"{tailPath}clothes/pfb_{tailName}_cloth00", out UmaDatabaseEntry asset2))
                {
                    umaContainer.LoadPhysics(asset2);
                }
            }
            else
            {
                Debug.Log("no tail");
            }
        }

        umaContainer.LoadPhysics(); //Need to load physics before loading FacialMorph
        umaContainer.SetDynamicBoneEnable(ModelSettings.DynamicBoneEnable);

        //Load FacialMorph
        umaContainer.LoadFaceMorph(id, costumeId);

        umaContainer.TearControllers.ForEach(a => a.SetDir(a.CurrentDir));
        umaContainer.HeadBone = (GameObject)umaContainer.Body.GetComponent<AssetHolder>()._assetTable["head"];
        umaContainer.EyeHeight = umaContainer.Head.GetComponent<AssetHolder>()._assetTableValue["head_center_offset_y"];
        umaContainer.MergeModel();
        umaContainer.SetHeight(-1);
        umaContainer.Initialize(!ModelSettings.IsTPose);

        umaContainer.Position = umaContainer.transform.Find("Position");
        umaContainer.SetupBoneHandles();

        if (!ModelSettings.IsTPose && loadMotion)
        {
            if (Main.AbList.TryGetValue($"3d/motion/event/body/type00/anm_eve_type00_homestand{personality.PadLeft(2, '0')}_loop", out UmaDatabaseEntry entry))
            {
                umaContainer.LoadAnimation(entry);
            }
        }


        //修改(载入通用服装ColorSet相关)
        UI.ClearColorSetButtons();


    }

    private void LoadMiniUma(UmaContainerCharacter umaContainer, CharaEntry chara, string costumeId)
    {
        int id = chara.Id;
        umaContainer.CharaEntry = chara;
        DataRow charaData = umaContainer.CharaData;
        umaContainer.IsMini = true;
        bool isGeneric = umaContainer.IsGeneric = costumeId.Length >= 4;
        int tailId = Convert.ToInt32(charaData["tail_model_id"]);
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
            UmaViewerUI.Instance?.ShowMessage("No body, can't load!", UIMessageType.Error);
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
                umaContainer.LoadTextures(asset1);
            }
            //Load Body
            umaContainer.LoadBody(asset);
        }
        else
            umaContainer.LoadBody(asset);

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
                    umaContainer.LoadTextures(asset1);
                }
            }
            else
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"3d/chara/mini/head/mchr{id}_00/textures")))
                {
                    umaContainer.LoadTextures(asset1);
                }
            }

            //Load Hair (don't know why loadhead is used)
            umaContainer.LoadHead(asset);
        }

        string head = $"3d/chara/mini/head/mchr0001_00/pfb_mchr0001_00_face0";
        Main.AbList.TryGetValue(head,out asset);
        if (asset != null)
        {
            //Load Head Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"3d/chara/mini/head/mchr0001_00/textures/tex_mchr0001_00_face0_{skin}")))
            {
                umaContainer.LoadTextures(asset1);
            }
            //Load Head
            umaContainer.LoadHead(asset);
        }

        if (tailId > 0)
        {
            string tailName = $"mtail{tailId.ToString().PadLeft(4, '0')}_00";
            string tailPath = $"3d/chara/mini/tail/{tailName}/";
            string tailPfb = tailPath + $"pfb_{tailName}";
            asset = null;
            Main.AbList.TryGetValue(tailPfb, out asset);
            if (asset != null)
            {
                foreach (var asset2 in UmaViewerMain.Instance.AbChara.Where(a => a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_{id.ToString().PadLeft(4,'0')}")))
                {
                    umaContainer.LoadTextures(asset2);
                }

                umaContainer.LoadTail(asset);

                //Load Physics
                var asset1 = Main.AbList[$"{tailPath}clothes/pfb_{tailName}_cloth00"];
                umaContainer.LoadPhysics(asset1);
            }
            else
            {
                Debug.Log("no tail");
            }
        }

        umaContainer.MergeModel();
        var matList = new List<MeshRenderer>(umaContainer.GetComponentsInChildren<MeshRenderer>());
        var eyemat = matList.FirstOrDefault(a => a.gameObject.name.Equals("M_Eye"));
        var mouthmat = matList.FirstOrDefault(a => a.gameObject.name.Equals("M_Mouth"));
        var eyebrowLmat = matList.FirstOrDefault(a => a.gameObject.name.Equals("M_Mayu_L"));
        var eyebrowRmat = matList.FirstOrDefault(a => a.gameObject.name.Equals("M_Mayu_R"));
        UI.LoadFacialPanelsMini(eyemat.material, eyebrowLmat.material, eyebrowRmat.material, mouthmat.material);

        umaContainer.Position = umaContainer.transform.Find("Position");
        umaContainer.SetupBoneHandles();

        if (Main.AbList.TryGetValue($"3d/motion/mini/event/body/chara/chr{id}_00/anm_min_eve_chr{id}_00_idle01_loop",out asset))
        {
            umaContainer.LoadAnimation(asset);
        }


        //修改(载入通用服装ColorSet相关)
        UI.ClearColorSetButtons();


    }

    public void LoadProp(UmaDatabaseEntry entry)
    {
        UnloadProp();
        UmaAssetManager.UnloadAllBundle();

        var prop = new GameObject(Path.GetFileName(entry.Name)).AddComponent<UmaContainerProp>();
        prop.LoadProp(entry);

        CurrentOtherContainer = prop;
    }

    public void LoadLive(LiveEntry live, List<LiveCharacterSelect> characters)
    {
        characters.ForEach(a =>
        {
            if (a.CharaEntry == null || a.CostumeId == "")
            {
                a.CharaEntry = Main.Characters[Random.Range(0, Main.Characters.Count / 2)];
                a.CostumeId = "0002_00_00";
            }
        });//fill empty

        UmaAssetManager.PreLoadAndRun(Director.GetLiveAllVoiceEntry(live.MusicId, characters), 
        delegate
        {
            UmaSceneController.LoadScene("LiveScene",
            delegate
            {
                GameObject MainLive = Instantiate(LiveControllerPrefab);
                Director mController = MainLive.GetComponentInChildren<Director>();
                mController.live = live;
                mController.IsRecordVMD = UI.isRecordVMD;
                mController.RequireStage = UI.isRequireStage;

                List<GameObject> transferObjs = new List<GameObject>() {
                            MainLive,
                            GameObject.Find("ViewerMain"),
                            GameObject.Find("Directional Light"),
                            GameObject.Find("GlobalShaderController"),
                            GameObject.Find("AudioManager")
                };

                // Move the GameObject (you attach this in the Inspector) to the newly loaded Scene
                transferObjs.ForEach(o => SceneManager.MoveGameObjectToScene(o, SceneManager.GetSceneByName("LiveScene")));
                mController.Initialize();

                var actual_member_count = mController._liveTimelineControl.data.worksheetList[0].charaMotSeqList.Count;
                if (actual_member_count > characters.Count)
                {
                    Debug.LogWarning($"actual member count is {actual_member_count} current {characters.Count}");
                    var actual_characters = new List<LiveCharacterSelect>();
                    for (int i = 0; i < actual_member_count; i++)
                    {
                        actual_characters.Add(characters[i % characters.Count]);
                    }
                    characters = actual_characters;
                }
                LoadLiveUma(characters);

                var Lyrics = LoadLiveLyrics(live.MusicId);
                if (Lyrics != null)
                {
                    LiveViewerUI.Instance.CurrentLyrics = Lyrics;
                }
            },
            delegate
            {
                Director.instance.InitializeUI();
                Director.instance.InitializeTimeline(characters, UI.LiveMode);
                Director.instance.InitializeMusic(live.MusicId, characters);
                Director.instance.Play();
            });
        });
    }

    //Use decrypt function
    public void LoadLiveSound(int songid, UmaDatabaseEntry SongAwb, bool needLyrics = true)
    {
        CurrentLiveSoundAWBIndex = -1; // mix awb together
        ClearLiveSounds();
        //load character voice
        if (SongAwb != null)
        {
            PlaySound(SongAwb);
            AddLiveSound(SongAwb);
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
                AddLiveSound(BGawb);
            }
        }

        if (needLyrics)
        {
            LoadLiveLyrics(songid);
        }
    }

    public void loadLivePreviewSound(int songid)
    {
        string nameVar = $"snd_bgm_live_{songid}_preview_02";
        UmaDatabaseEntry previewAwb = Main.AbSounds.FirstOrDefault(a => a.Name.Contains(nameVar) && a.Name.EndsWith("awb"));
        if (previewAwb != null)
        {
            PlaySound(previewAwb, volume : 0.4f, loop : true);
        }
    }

    public void PlaySound(UmaDatabaseEntry SongAwb, int subindex = -1, float volume = 1, bool loop = false)
    {
        CurrentLyrics.Clear();
        if (CurrentAudioSources.Count > 0)
        {
            var tmp = CurrentAudioSources[0];
            CurrentAudioSources.Clear();
            Destroy(tmp.gameObject);
            UI.AudioSettings.ResetPlayer();
        }
        if (subindex == -1)
        {
            foreach (AudioClip clip in LoadAudio(SongAwb))
            {
                AddAudioSource(clip, volume, loop);
            }
        }
        else
        {
            AddAudioSource(LoadAudio(SongAwb)[subindex], volume, loop);
        }

    }

    public void SetLastAudio(UmaDatabaseEntry AudioAwb, int index)
    {
        ClearLiveSounds();
        AddLiveSound(AudioAwb);
        CurrentLiveSoundAWBIndex = index;
    }

    private void AddAudioSource(AudioClip clip, float volume = 1, bool loop = false)
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
        source.volume = volume;
        source.loop = loop;
        source.Play();
    }



    public List<UmaWaveStream> LoadAudioStreams(UmaDatabaseEntry awb)
    {
        var streams = new List<UmaWaveStream>();
        string awbPath = awb.FilePath;
        if (!File.Exists(awbPath)) return streams;

        FileStream awbFile = File.OpenRead(awbPath);
        AwbReader awbReader = new AwbReader(awbFile);

        foreach (Wave wave in awbReader.Waves)
        {
            var stream = new UmaWaveStream(awbReader, wave.WaveId);
            streams.Add(stream);
        }
         

       return streams;
    }

    public static List<AudioClip> LoadAudio(UmaDatabaseEntry awb)
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

    public List<UmaLyricsData> LoadLiveLyrics(int songid)
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
                        text = (words.Length > 1) ? words[1] : ""
                    };
                    CurrentLyrics.Add(lyricsData);
                }
                catch { }
            }
        }
        return CurrentLyrics;
    }

    public void LoadAssetPath(string path, Transform SetParent)
    {
        Instantiate(UmaViewerMain.Instance.AbList[path].Get<GameObject>(), SetParent);
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
            CurrentUMAContainer?.SetHeight(0);
        }
        else
        {
            AnimationCamera.enabled = false;
            CurrentUMAContainer?.SetHeight(-1);
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
        PoseManager.SetPoseModeStatic(false);

        if (CurrentUMAContainer != null)
        {
            //It seems that OnDestroy will executed after new model loaded, which cause new FacialPanels empty...
            UI.currentFaceDrivenKeyTarget = null;
            UI.LoadEmotionPanels(null);
            UI.LoadFacialPanels(null);
            if (UI.ModelSettings.MaterialsList)
                foreach (Transform t in UI.ModelSettings.MaterialsList.content)
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
        var umaContainer = CurrentUMAContainer;
        if (umaContainer != null && umaContainer.FaceDrivenKeyTarget != null)
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
            if (umaContainer.FaceDrivenKeyTarget)
            {
                umaContainer.FaceDrivenKeyTarget.ClearAllWeights();
                umaContainer.FaceDrivenKeyTarget.ChangeMorph();
            }
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

    private void AddLiveSound(UmaDatabaseEntry entry)
    {
        UmaAssetManager.LoadAssetBundle(entry, false, false);
        CurrentLiveSoundAWB.Add(entry);
    }

    private void ClearLiveSounds()
    {
        foreach (var liveSound in CurrentLiveSoundAWB)
        {
            UmaAssetManager.UnloadAssetBundle(liveSound, true);
        }
        CurrentLiveSoundAWB.Clear();
    }
}
