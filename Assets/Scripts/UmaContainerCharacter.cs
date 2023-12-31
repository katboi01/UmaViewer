using Gallop;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using static SerializableBone;

public class UmaContainerCharacter : UmaContainer
{
    public CharaEntry CharaEntry;
    public DataRow CharaData;
    public GameObject Body;
    public GameObject Tail;
    public GameObject Head;
    public GameObject Hair;

    public List<Texture2D> TailTextures = new List<Texture2D>();

    [Header("Animator")]
    public Animator UmaAnimator;
    public AnimatorOverrideController OverrideController;
    public Animator UmaFaceAnimator;
    public AnimatorOverrideController FaceOverrideController;
    public bool isAnimatorControl;

    [Header("Body")]
    public GameObject UpBodyBone;
    public Vector3 UpBodyPosition;
    public Quaternion UpBodyRotation;

    [Header("Face")]
    public FaceDrivenKeyTarget FaceDrivenKeyTarget;
    public FaceEmotionKeyTarget FaceEmotionKeyTarget;
    public FaceOverrideData FaceOverrideData;
    public GameObject HeadBone;
    public Transform TrackTarget;
    public float EyeHeight;
    public bool EnableEyeTracking = true;
    public Material FaceMaterial;

    [Header("Cheek")]
    public Texture CheekTex_0;
    public Texture CheekTex_1;
    public Texture CheekTex_2;

    [Header("Manga")]
    public List<GameObject> LeftMangaObject = new List<GameObject>();
    public List<GameObject> RightMangaObject = new List<GameObject>();

    [Header("Tear")]
    public GameObject StaticTear_L;
    public GameObject StaticTear_R;
    public GameObject TearPrefab_0;
    public GameObject TearPrefab_1;
    public List<TearController> TearControllers = new List<TearController>();

    [Header("Generic")]
    public bool IsGeneric = false;
    public string VarCostumeIdShort, VarCostumeIdLong, VarSkin, VarHeight, VarSocks, VarBust;
    public List<Texture2D> GenericBodyTextures = new List<Texture2D>();

    [Header("Mini")]
    public bool IsMini = false;
    public List<Texture2D> MiniHeadTextures = new List<Texture2D>();

    [Header("Mob")]
    public bool IsMob = false;
    public DataRow MobDressColor;
    public DataRow MobHeadColor;
    public List<Texture2D> MobHeadTextures = new List<Texture2D>();

    [Header("Physics")]
    public bool EnablePhysics = true;
    public List<CySpringDataContainer> cySpringDataContainers;
    public GameObject PhysicsContainer;
    public float BodyScale = 1;

    private float dragdistance;
    private Vector3 dragStartPos;
    private Collider dragCollider;
    private BipedIK IK;
    private PuppetMaster PuppetMaster;
    private List<Transform> _humanoidBones;

    public void Initialize(bool smile)
    {
        TrackTarget = new GameObject("TrackTarget").transform;
        TrackTarget.SetParent(transform);
        TrackTarget.position = HeadBone.transform.TransformPoint(0, 0, 10);

        UpBodyPosition = UpBodyBone.transform.localPosition;
        UpBodyRotation = UpBodyBone.transform.localRotation;

        //Models must be merged before handling extra morphs
        if (FaceDrivenKeyTarget && smile)
            FaceDrivenKeyTarget.ChangeMorphWeight(FaceDrivenKeyTarget.MouthMorphs[3], 1);

        CreateIK();
    }

    public void MergeModel()
    {
        if (!Body) return;
        var bodyBones = Body.GetComponentInChildren<SkinnedMeshRenderer>().bones.ToDictionary(bone => bone.name, bone => bone.transform);
        List<Transform> emptyBones = new List<Transform>();
        emptyBones.Add(Body.transform.Find("Position/Hip/Tail_Ctrl"));
        while (Body.transform.childCount > 0)
        {
            Body.transform.GetChild(0).SetParent(transform);
        }
        Body.SetActive(false); //for debugging

        //MergeHead
        if (Head)
        {
            var headskins = Head.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer headskin in headskins)
            {
                MergeBone(headskin, bodyBones, ref emptyBones);
            }
            var eyes = new GameObject("Eyes");
            eyes.transform.SetParent(transform);
            while (Head.transform.childCount > 0)
            {
                var child = Head.transform.GetChild(0);
                child.SetParent(child.name.Contains("info") ? eyes.transform : transform);
            }
            Head.SetActive(false); //for debugging
        }


        //MergeTail
        if (Tail)
        {
            var tailskin = Tail.GetComponentInChildren<SkinnedMeshRenderer>();
            MergeBone(tailskin, bodyBones, ref emptyBones);
            while (Tail.transform.childCount > 0)
            {
                var child = Tail.transform.GetChild(0);
                child.SetParent(transform);
            }
            Tail.SetActive(false); //for debugging
        }


        emptyBones.ForEach(a => { if (a) Destroy(a.gameObject); });

        //MergeAvatar
        UmaAnimator = gameObject.AddComponent<Animator>();
        UmaAnimator.avatar = AvatarBuilder.BuildGenericAvatar(gameObject, gameObject.name);
        OverrideController = Instantiate(UmaViewerBuilder.Instance.OverrideController);
        UmaAnimator.runtimeAnimatorController = OverrideController;

        //Materials
        foreach (var rend in gameObject.GetComponentsInChildren<Renderer>())
        {
            for (int i = 0; i < rend.sharedMaterials.Length; i++)
            {
                Material mat = rend.sharedMaterials[i];

                var matHlp = Materials.FirstOrDefault(m => m.Mat == mat);
                if (matHlp == null)
                {
                    matHlp = new MaterialHelper()
                    {
                        Mat = mat,
                        Renderers = new Dictionary<Renderer, List<int>>(),
                        Toggle = Instantiate(UmaViewerUI.Instance.UmaContainerTogglePrefab, UmaViewerUI.Instance.MaterialsList.content)
                    };
                    matHlp.Toggle.Name = mat.name;
                    matHlp.Toggle.Toggle.onValueChanged.AddListener((value) => { Debug.Log("e"); matHlp.ToggleMaterials(value); });
                    Materials.Add(matHlp);
                }

                if (!matHlp.Renderers.ContainsKey(rend))
                {
                    matHlp.Renderers.Add(rend, new List<int> { i });
                }
                else
                {
                    matHlp.Renderers[rend].Add(i);
                }
            }
        }
    }

    public void MergeHairModel()
    {
        if (!Head || !Hair) return;

        var bodyBones = Head.GetComponentInChildren<SkinnedMeshRenderer>().bones.ToDictionary(bone => bone.name, bone => bone.transform);
        List<Transform> emptyBones = new List<Transform>();

        var headHolder = Head.GetComponent<AssetHolder>();
        var hairHolder = Hair.GetComponent<AssetHolder>();

        headHolder._assetTable.list.AddRange(hairHolder._assetTable.list);
        headHolder._assetTableValue.list.AddRange(hairHolder._assetTableValue.list);

        //MergeHair
        var hairskins = Hair.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer hairskin in hairskins)
        {
            hairskin.gameObject.transform.SetParent(Head.transform);
            MergeBone(hairskin, bodyBones, ref emptyBones);
        }
        Hair.gameObject.SetActive(false);

        emptyBones.ForEach(a => { if (a) Destroy(a.gameObject); });
    }

    public void SetHeight(int scale = 0)
    {
        if (scale == 0)
        {
            BodyScale = 1;
        }
        else if (scale == -1)
        {
            BodyScale = (Convert.ToInt32(CharaData[IsMob ? "chara_height" : "scale"]) / 160.7529f);
        }
        else
        {
            BodyScale = (scale / 160.7529f);
        }
        transform.Find("Position").localScale = new Vector3(BodyScale, BodyScale, BodyScale);
    }

    public void MergeBone(SkinnedMeshRenderer from, Dictionary<string, Transform> targetBones, ref List<Transform> emptyBones)
    {
        if(targetBones.TryGetValue(from.rootBone.name, out Transform rootbone))
        {
            from.rootBone = rootbone;
            Transform[] tmpBone = new Transform[from.bones.Length];
            for (int i = 0; i < tmpBone.Length; i++)
            {
                if (targetBones.TryGetValue(from.bones[i].name, out Transform targetbone))
                {
                    tmpBone[i] = targetbone;
                    from.bones[i].position = targetbone.position;
                    while (from.bones[i].transform.childCount > 0)
                    {
                        from.bones[i].transform.GetChild(0).SetParent(targetbone);
                    }
                    emptyBones.Add(from.bones[i]);
                }
                else
                {
                    tmpBone[i] = from.bones[i];
                }
            }
            from.bones = tmpBone;
        };
    }

    public void LoadPhysics()
    {
        cySpringDataContainers = new List<CySpringDataContainer>(PhysicsContainer.GetComponentsInChildren<CySpringDataContainer>());
        var bones = new Dictionary<string, Transform>();
        foreach (var bone in GetComponentsInChildren<Transform>())
        {
            if (!bones.ContainsKey(bone.name))
                bones.Add(bone.name, bone);
        }

        var colliders = new Dictionary<string, Transform>();

        for (int i = 0; i < cySpringDataContainers.Count; i++)
        {
            colliders = UmaUtility.MergeDictionaries(colliders, cySpringDataContainers[i].InitiallizeCollider(bones));
        }

        for (int i = 0; i < cySpringDataContainers.Count; i++)
        {
            cySpringDataContainers[i].InitializePhysics(bones, colliders);
        }
    }

    public void SetPuppetMasterMode(PuppetMaster.Mode mode)
    {
        PuppetMaster.mode = mode;
    }

    public void SetDynamicBoneEnable(bool isOn)
    {
        if (IsMini) return;
        EnablePhysics = isOn;
        foreach (CySpringDataContainer cySpring in cySpringDataContainers)
        {
            cySpring.EnablePhysics(isOn);
        }
    }

    public void SetEyeTracking(bool isOn)
    {
        EnableEyeTracking = isOn;
    }

    public void SetFaceOverrideData(bool isOn)
    {
        FaceOverrideData?.SetEnable(isOn);
    }

    private void FixedUpdate()
    {
        if (IsMini) return;

        if (TrackTarget && EnableEyeTracking && !isAnimatorControl)
        {
            if (IK && !IK.enabled)
            {
                IK.enabled = true;
                if (PuppetMaster.mode != PuppetMaster.Mode.Active)
                {
                    SetPuppetMasterMode(PuppetMaster.Mode.Active);
                }
            }

            var finalRotation = FaceDrivenKeyTarget.GetEyeTrackRotation(TrackTarget.transform.position);
            FaceDrivenKeyTarget.SetEyeTrack(finalRotation);

            var cam = Camera.main;
            var distance = Mathf.Clamp(cam.transform.InverseTransformPoint(HeadBone.transform.position).magnitude - 0.1f, 0, 2);
            var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
            var worldPos = cam.ScreenToWorldPoint(mousePos);

            //Hold D key to Drag Body
            if (Input.GetKey(KeyCode.D))
            {
                if (dragCollider && dragCollider.attachedRigidbody)
                {
                    if (PuppetMaster)
                    {
                        PuppetMaster.pinWeight = 0.75f;
                        PuppetMaster.muscleWeight = 0.25f;
                    }
                    dragCollider.attachedRigidbody.isKinematic = true;
                    dragCollider.transform.position = Vector3.Lerp(dragCollider.transform.position, cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, dragdistance)) - dragStartPos, Time.fixedDeltaTime * 4);
                    //LookAt Mouse when Dragging
                    TrackTarget.position = Vector3.Lerp(TrackTarget.position, dragCollider.transform.position, Time.fixedDeltaTime * 3);
                }
                else
                {
                    if (Physics.Raycast(cam.ScreenPointToRay(mousePos), out RaycastHit hit))
                    {
                        dragCollider = hit.collider;
                        dragStartPos = (hit.point - hit.collider.transform.position);
                        dragdistance = cam.transform.InverseTransformPoint(hit.point).magnitude;
                    }
                    //LookAt Camera
                    TrackTarget.position = Vector3.Lerp(TrackTarget.position, cam.transform.position, Time.fixedDeltaTime * 3);
                }
            }
            else
            {
                if (PuppetMaster)
                {
                    PuppetMaster.pinWeight = 1;
                    PuppetMaster.muscleWeight = 1;
                }
                //LookAt Camera
                TrackTarget.position = Vector3.Lerp(TrackTarget.position, cam.transform.position, Time.fixedDeltaTime * 3);
                if (dragCollider && dragCollider.attachedRigidbody) dragCollider.attachedRigidbody.isKinematic = false;
                dragCollider = null;
            }
        }
        else
        {
            if (IK && IK.enabled)
            {
                IK.enabled = false;
                if (PuppetMaster && PuppetMaster.mode != PuppetMaster.Mode.Kinematic)
                {
                    SetPuppetMasterMode(PuppetMaster.Mode.Kinematic);
                }
            }
        }

        if (isAnimatorControl)
        {
            FaceDrivenKeyTarget.ProcessLocator();
        }

        if (FaceMaterial)
        {
            if (isAnimatorControl)
            {
                FaceMaterial.SetVector("_FaceForward", Vector3.zero);
                FaceMaterial.SetVector("_FaceUp", Vector3.zero);
                FaceMaterial.SetVector("_FaceCenterPos", Vector3.zero);

            }
            else
            {
                //Used to calculate facial shadows
                FaceMaterial.SetVector("_FaceForward", HeadBone.transform.forward);
                FaceMaterial.SetVector("_FaceUp", HeadBone.transform.up);
                FaceMaterial.SetVector("_FaceCenterPos", HeadBone.transform.position);
            }
            FaceMaterial.SetMatrix("_faceShadowHeadMat", HeadBone.transform.worldToLocalMatrix);
        }

        TearControllers.ForEach(a => a.UpdateOffset());

        //Apply UV Animation

        if(HeadShaderEffectData != null)
        {
            HeadShaderEffectData.updateUV(Time.fixedDeltaTime);
        }

        if(BodyShaderEffectData != null)
        {
            BodyShaderEffectData.updateUV(Time.fixedDeltaTime);
        }
    }

    public void UpBodyReset()
    {
        if (UpBodyBone)
        {
            UpBodyBone.transform.localPosition = UpBodyPosition;
            UpBodyBone.transform.localRotation = UpBodyRotation;
        }
    }

    public void CreateIK()
    {
        if (IsMini) return;
        var container = this;
        var animator = UmaAnimator;
        BipedRagdollReferences r = BipedRagdollReferences.FromAvatar(animator);
        BipedRagdollCreator.Options options = BipedRagdollCreator.AutodetectOptions(r);

        var ik = container.gameObject.AddComponent<BipedIK>();
        ik.references.root = container.transform;
        ik.references.pelvis = r.hips;
        ik.references.spine = new Transform[] { r.spine, r.chest };
        ik.references.leftThigh = r.leftUpperLeg;
        ik.references.leftCalf = r.leftLowerLeg;
        ik.references.leftFoot = r.leftFoot;
        ik.references.rightThigh = r.rightUpperLeg;
        ik.references.rightCalf = r.rightLowerLeg;
        ik.references.rightFoot = r.rightFoot;
        ik.references.leftUpperArm = r.leftUpperArm;
        ik.references.leftForearm = r.leftLowerArm;
        ik.references.leftHand = r.leftHand;
        ik.references.rightUpperArm = r.rightUpperArm;
        ik.references.rightForearm = r.rightLowerArm;
        ik.references.rightHand = r.rightHand;
        ik.references.head = r.head;

        new List<IKSolver>(ik.solvers.ikSolvers).ForEach(i => i.IKPositionWeight = 0);
        new List<IKSolverLimb>(ik.solvers.limbs).ForEach(i => i.IKRotationWeight = 0);
        ik.solvers.lookAt.IKPositionWeight = 1;
        ik.solvers.lookAt.headWeight = 0.8f;
        ik.solvers.lookAt.bodyWeight = 0.25f;
        ik.solvers.lookAt.clampWeightHead = 0.62f;
        ik.solvers.lookAt.target = TrackTarget.transform;
        ik.solvers.lookAt.spineWeightCurve = new AnimationCurve(new Keyframe[2] { new Keyframe(0f, 1f), new Keyframe(1f, 0.3f) });
        IK = ik;

        BipedRagdollCreator.Create(r, options);
        PuppetMaster = PuppetMaster.SetUp(container.transform, 8, 9);
        PuppetMaster.solverIterationCount = 3;
        PuppetMaster.FlattenHierarchy();
    }

    public void LoadTextures(UmaDatabaseEntry entry)
    {
        foreach(Texture2D tex2D in entry.GetAll<Texture2D>())
        {
            if (entry.Name.Contains("/mini/head"))
            {
                MiniHeadTextures.Add(tex2D);
            }
            else if (entry.Name.Contains("/tail/"))
            {
                TailTextures.Add(tex2D);
            }
            else if (entry.Name.Contains("bdy0"))
            {
                GenericBodyTextures.Add(tex2D);
            }
            else if (entry.Name.Contains("_face") || entry.Name.Contains("_hair"))
            {
                if (IsMob)
                    MobHeadTextures.Add(tex2D);
            }
        }
    }

    public void LoadBody(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        Body = Instantiate(go, transform);
        UmaAnimator = Body.GetComponent<Animator>();

        if (IsMini)
        {
            UpBodyBone = Body.transform.Find("Position/Hip").gameObject;
        }
        else
        {
            UpBodyBone = Body.GetComponent<AssetHolder>()._assetTable["upbody_ctrl"] as GameObject;
        }

        if (IsGeneric)
        {
            List<Texture2D> textures = GenericBodyTextures;
            string costumeIdShort = VarCostumeIdShort,
                   costumeIdLong = VarCostumeIdLong,
                   height = VarHeight,
                   skin = VarSkin,
                   socks = VarSocks,
                   bust = VarBust;

            foreach (Renderer r in Body.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    string mainTex = "", toonMap = "", tripleMap = "", optionMap = "", zekkenNumberTex = "";

                    if (IsMini)
                    {
                        m.SetTexture("_MainTex", textures[0]);
                    }
                    else
                    {

                        if (m.shader.name.Contains("Noline") && m.shader.name.Contains("TSER"))
                        {
                            var s = Builder.ShaderList.Find(a => a.name == m.shader.name.Replace("Noline", "")); //Generic costume shader need to change manually.
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
                                m.shader = UmaAssetManager.BodyAlphaShader;
                            }
                            else
                            {
                                //some costume use area texture
                                var areaTex = UmaViewerMain.Instance.AbChara.FirstOrDefault(a => a.Name.StartsWith(UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/textures") && a.Name.EndsWith("area"));
                                if (areaTex != null)
                                {
                                    LoadTextures(areaTex);
                                    m.SetTexture("_MaskColorTex", textures.FirstOrDefault(t => t.name.Contains(costumeIdShort) && t.name.EndsWith("area")));
                                    SetMaskColor(m, IsMob ? MobDressColor : CharaData, IsMob);
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
            foreach (Renderer r in Body.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    //BodyAlapha's shader need to change manually.
                    if (m.name.Contains("bdy") && m.name.Contains("Alpha"))
                    {
                        m.shader = UmaAssetManager.BodyAlphaShader;
                    }
                }
            }
        }

        var assetholder = Body.GetComponent<AssetHolder>();
        if (assetholder)
        {
            BodyShaderEffectData = assetholder._assetTable["chara_shader_effect"] as CharaShaderEffectData;
            if (BodyShaderEffectData)
            {
                BodyShaderEffectData.Initialize();
            }
        }
    }

    public void LoadHead(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        var textures = MobHeadTextures;
        GameObject head = Instantiate(go, transform);
        Head = head;

        AssetTable table = null;
        if (!IsMini)
        {
            table = Head.GetComponent<AssetHolder>()._assetTable;
        }

        //Some setting for Head
        EnableEyeTracking = UI.EnableEyeTracking;

        foreach (Renderer r in head.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                if (head.name.Contains("mchr"))
                {
                    if (r.name.Contains("Hair"))
                    {
                        Tail = head;
                    }
                    if (r.name == "M_Face")
                    {
                        m.SetTexture("_MainTex", MiniHeadTextures.First(t => t.name.Contains("face") && t.name.Contains("diff")));
                    }
                    if (r.name == "M_Cheek")
                    {
                        m.CopyPropertiesFromMaterial(Builder.TransMaterialCharas);
                        m.SetTexture("_MainTex", MiniHeadTextures.First(t => t.name.Contains("cheek")));
                    }
                    if (r.name == "M_Mouth")
                    {
                        m.SetTexture("_MainTex", MiniHeadTextures.First(t => t.name.Contains("mouth")));
                    }
                    if (r.name == "M_Eye")
                    {
                        m.SetTexture("_MainTex", MiniHeadTextures.First(t => t.name.Contains("eye")));
                    }
                    if (r.name.StartsWith("M_Mayu_"))
                    {
                        m.SetTexture("_MainTex", MiniHeadTextures.First(t => t.name.Contains("mayu")));
                    }
                }
                else
                {
                    if (IsMob)
                    {
                        if (m.name.EndsWith("eye"))
                        {
                            m.SetTexture("_MainTex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("eye0")));
                            m.SetTexture("_High0Tex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("hi00")));
                            m.SetTexture("_High1Tex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("hi01")));
                            m.SetTexture("_High2Tex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("hi02")));
                            m.SetTexture("_MaskColorTex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("area")));
                            SetMaskColor(m, MobHeadColor, "eye", false);
                        }
                        if (m.name.EndsWith("face"))
                        {
                            m.SetTexture("_MainTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("diff")));
                            m.SetTexture("_ToonMap", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("shad_c")));
                            m.SetTexture("_TripleMaskMap", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("base")));
                            m.SetTexture("_OptionMaskMap", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("ctrl")));
                            m.SetTexture("_MaskColorTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("area") && !t.name.Contains("_eye")));
                            SetMaskColor(m, MobHeadColor, "mayu", true);
                        }
                        if (m.name.EndsWith("mayu"))
                        {
                            m.SetTexture("_MainTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("diff")));
                            m.SetTexture("_MaskColorTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("area") && !t.name.Contains("_eye")));
                            SetMaskColor(m, MobHeadColor, "mayu", true);
                        }
                    }

                    //Glasses's shader need to change manually.
                    if (r.name.Contains("Hair") && r.name.Contains("Alpha"))
                    {
                        m.shader = UmaAssetManager.AlphaShader;
                    }

                    //Blush Setting
                    if(r.name.Contains("Cheek"))
                    {
                        r.gameObject.SetActive(false);
                        if (IsMob)
                        {
                            CheekTex_0 = MobHeadTextures.FindLast(a => a.name.Contains("cheek0"));
                            CheekTex_1 = MobHeadTextures.FindLast(a => a.name.Contains("cheek1"));
                        }
                        else
                        {
                            CheekTex_0 = table["cheek0"] as Texture;
                            CheekTex_1 = table["cheek1"] as Texture;
                        }
                    }

                    if(Main.AbList.TryGetValue("3d/chara/common/textures/tex_chr_tear00", out var tearEntry))
                    {
                       var ab =  UmaAssetManager.LoadAssetBundle(tearEntry, true, false);
                       var tex = ab.LoadAsset<Texture>("tex_chr_tear00");
                       StaticTear_L = table["tearmesh_l"] as GameObject;
                       StaticTear_R = table["tearmesh_r"] as GameObject;
                       StaticTear_L.GetComponent<Renderer>().material.mainTexture = tex;
                       StaticTear_R.GetComponent<Renderer>().material.mainTexture = tex;
                    }

                    switch (m.shader.name)
                    {
                        case "Gallop/3D/Chara/MultiplyCheek":
                            m.shader = UmaAssetManager.CheekShader; ;
                            break;
                        case "Gallop/3D/Chara/ToonFace/TSER":
                            m.shader = UmaAssetManager.FaceShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            m.SetColor("_RimColor", new Color(0, 0, 0, 0));
                            break;
                        case "Gallop/3D/Chara/ToonEye/T":
                            m.shader = UmaAssetManager.EyeShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            break;
                        case "Gallop/3D/Chara/ToonHair/TSER":
                            m.shader = UmaAssetManager.HairShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            break;
                        case "Gallop/3D/Chara/ToonMayu":
                            m.shader = UmaAssetManager.EyebrowShader;
                            m.renderQueue += 1; //fix eyebrows disappearing sometimes
                            break;
                        default:
                            Debug.Log(m.shader.name);
                            // m.shader = Shader.Find("Nars/UmaMusume/Body");
                            break;
                    }
                }

                m.SetFloat("_StencilMask", CharaEntry.Id);
            }
        }

        //shader effect
        var assetholder = head.GetComponent<AssetHolder>();
        if (assetholder)
        {
            HeadShaderEffectData = assetholder._assetTable["chara_shader_effect"] as CharaShaderEffectData;
            if (HeadShaderEffectData)
            {
                HeadShaderEffectData.Initialize();
            }
        }
    }

    public void LoadHair(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        GameObject hair = Instantiate(go, transform);
        Hair = hair;
        var textures = MobHeadTextures;
        foreach (Renderer r in hair.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {

                //Glasses's shader need to change manually.
                if (r.name.Contains("Hair") && r.name.Contains("Alpha"))
                {
                    m.shader = UmaAssetManager.AlphaShader;
                }

                if (m.name.EndsWith("_hair"))
                {
                    m.SetTexture("_MainTex", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("diff")));
                    m.SetTexture("_ToonMap", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("shad_c")));
                    m.SetTexture("_TripleMaskMap", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("base")));
                    m.SetTexture("_OptionMaskMap", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("ctrl")));
                    m.SetTexture("_MaskColorTex", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("area")));
                    SetMaskColor(m, MobHeadColor, "hair", true);
                }

                switch (m.shader.name)
                {
                    case "Gallop/3D/Chara/ToonHair/TSER":
                        m.shader = UmaAssetManager.HairShader;
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

    public void LoadTail(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        Tail = Instantiate(go, transform);
        var textures = TailTextures;
        foreach (Renderer r in Tail.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                m.SetTexture("_MainTex", textures.FirstOrDefault(t => t.name.EndsWith("diff")));
                m.SetTexture("_ToonMap", textures.FirstOrDefault(t => t.name.Contains("shad")));
                m.SetTexture("_TripleMaskMap", textures.FirstOrDefault(t => t.name.Contains("base")));
                m.SetTexture("_OptionMaskMap", textures.FirstOrDefault(t => t.name.Contains("ctrl")));
                if (IsMob)
                {
                    SetMaskColor(m, MobHeadColor, "tail", true);
                }
            }
        }
    }

    public void LoadTear(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        if (go.name.EndsWith("000"))
        {
            TearPrefab_0 = go;
        }
        else if (go.name.EndsWith("001"))
        {
            TearPrefab_1 = go;
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

    public void LoadPhysics(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        if (!PhysicsContainer)
        {
            PhysicsContainer = new GameObject("PhysicsController");
            PhysicsContainer.transform.SetParent(transform);
        }
        Instantiate(go, PhysicsContainer.transform);
    }

    public void LoadAnimation(UmaDatabaseEntry entry)
    {
        if (UI.LiveTime)
        {
            return;
        }

        var aClip = entry.Get<AnimationClip>();

        if (UmaAnimator)
        {
            Debug.Log("LiveTime" + UI.LiveTime.ToString());
            aClip.name = entry.Name; // Need a complete path to find dependencies
            LoadAnimation(aClip);
            return;
        }

        if (aClip.name.Contains("tear"))
        {
            return;
        }
    }

    private void LoadAnimation(AnimationClip clip)
    {
        if (clip.name.EndsWith("_s"))
        {
            OverrideController["clip_s"] = clip;
        }
        else if (clip.name.EndsWith("_e"))
        {
            OverrideController["clip_e"] = clip;
        }
        else if (clip.name.Contains("tail"))
        {
            if (IsMini) return;
            UpBodyReset();
            OverrideController["clip_t"] = clip;
            UmaAnimator.Play("motion_t", 1, 0);
        }
        else if (clip.name.EndsWith("_face"))
        {
            if (IsMini) return;
            LoadFaceAnimation(clip);
        }
        else if (clip.name.Contains("_ear"))
        {
            if (IsMini) return;
            LoadEarAnimation(clip);
        }
        else if (clip.name.EndsWith("_pos"))
        {
            if (IsMini) return;
            OverrideController["clip_p"] = clip;
            UmaAnimator.Play("motion_1", 2, 0);
        }
        else if (clip.name.EndsWith("_cam"))
        {
            Builder.SetPreviewCamera(clip);
        }
        else if (clip.name.Contains("_loop"))
        {
            UpBodyReset();
            if (isAnimatorControl && FaceDrivenKeyTarget)
            {
                FaceDrivenKeyTarget.ResetLocator();
                isAnimatorControl = false;
            }

            if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/facial")}_face", out UmaDatabaseEntry entry))
            {
                LoadAnimation(entry);
            }

            if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/facial")}_ear", out entry))
            {
                LoadAnimation(entry);
            }

            UmaDatabaseEntry motion_e = null, motion_s = null;
            if (Main.AbList.TryGetValue(clip.name.Replace("_loop", "_s"), out motion_s))
            {
                LoadAnimation(motion_s);
            }

            if (OverrideController["clip_2"].name.Contains("_loop"))
            {
                if (!OverrideController["clip_2"].name.Contains("hom_"))//home end animation not for interpolation
                {
                    if (Main.AbList.TryGetValue(OverrideController["clip_2"].name.Replace("_loop", "_e"), out motion_e))
                    {
                        LoadAnimation(motion_e);
                    }
                }
            }

            Builder.SetPreviewCamera(null);
            OverrideController["clip_1"] = OverrideController["clip_2"];
            OverrideController["clip_2"] = clip;
            UmaAnimator.Play("motion_1", -1, 0);
            UmaAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e" : ((motion_s != null) ? "next_s" : "next"));
        }
        else
        {
            if (FaceDrivenKeyTarget)
            {
                FaceDrivenKeyTarget.ResetLocator();
                isAnimatorControl = false;
            }
            UpBodyReset();
            UmaAnimator.Rebind();
            OverrideController["clip_2"] = clip;
            // If Cut-in, play immediately without state interpolation

            if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/facial")}_face", out UmaDatabaseEntry facialMotion))
            {
                LoadAnimation(facialMotion);
            }

            if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/facial")}_ear", out UmaDatabaseEntry earMotion))
            {
                LoadAnimation(earMotion);
            }

            if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/camera")}_cam", out UmaDatabaseEntry cameraMotion))
            {
                LoadAnimation(cameraMotion);
            }
            else
            {
                Builder.SetPreviewCamera(null);
            }

            if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/position")}_pos", out UmaDatabaseEntry posMotion))
            {
                LoadAnimation(posMotion);
            }

            if (IsMini)
            {
                Builder.SetPreviewCamera(null);
            }

            if (clip.name.Contains("crd") || clip.name.Contains("res_chr"))
            {

                if (clip.name.Contains("_cti_crd"))
                {
                    var dir = Path.GetDirectoryName(clip.name).Replace("\\", "/");
                    string[] param = Path.GetFileName(clip.name).Split('_');
                    if (param.Length > 4)
                    {
                        int index = int.Parse(param[4]);
                        if (index == 1)
                        {
                            var cur = index + 1;
                            while (true)
                            {
                                var nextSearch = $"{dir}/{param[0]}_{param[1]}_{param[2]}_{param[3]}_{cur.ToString().PadLeft(2, '0')}";
                                if (Main.AbList.TryGetValue(nextSearch, out UmaDatabaseEntry result))
                                {
                                    UmaAssetManager.LoadAssetBundle(result);
                                    cur++;
                                }
                                else break;
                            }
                        }

                        index++;
                        var next = $"{dir}/{param[0]}_{param[1]}_{param[2]}_{param[3]}_{index.ToString().PadLeft(2, '0')}";
                        if (Main.AbList.TryGetValue(next, out UmaDatabaseEntry nextMotion))
                        {
                            var aevent = new AnimationEvent
                            {
                                time = clip.length * 0.99f,
                                stringParameter = (nextMotion != null ? nextMotion.Name : null),
                                functionName = (nextMotion != null ? "SetNextAnimationCut" : "SetEndAnimationCut")
                            };
                            clip.AddEvent(aevent);
                        }
                    }
                }

            }

            UmaAnimator.Play("motion_2", 0, 0);
        }
    }

    private void LoadFaceAnimation(AnimationClip clip)
    {
        if (clip.name.Contains("_s_"))
        {
            FaceOverrideController["clip_s"] = clip;
        }
        else if (clip.name.Contains("_e_"))
        {
            FaceOverrideController["clip_e"] = clip;
        }
        else if (clip.name.Contains("_loop"))
        {
            isAnimatorControl = true;
            FaceDrivenKeyTarget.ResetLocator();
            UmaDatabaseEntry motion_e = null;
            UmaDatabaseEntry motion_s = null;
            if (Main.AbList.TryGetValue(clip.name.Replace("_loop", "_s"), out motion_s))
            {
                LoadAnimation(motion_s);
            }

            if (FaceOverrideController["clip_2"].name.Contains("_loop"))
            {
                if (!FaceOverrideController["clip_2"].name.Contains("hom_"))//home end animation not for interpolation
                {
                    if (Main.AbList.TryGetValue(FaceOverrideController["clip_2"].name.Replace("_loop", "_e"), out motion_e))
                    {
                        LoadAnimation(motion_e);
                    }
                }
            }

            FaceOverrideController["clip_1"] = FaceOverrideController["clip_2"];
            FaceOverrideController["clip_2"] = clip;
            UmaFaceAnimator.Play("motion_1", 0, 0);
            UmaFaceAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e" : ((motion_s != null) ? "next_s" : "next"));
        }
        else
        {
            isAnimatorControl = true;
            FaceDrivenKeyTarget.ResetLocator();
            FaceOverrideController["clip_2"] = clip;
            UmaFaceAnimator.Play("motion_2", 0, 0);
        }
    }

    private void LoadEarAnimation(AnimationClip clip)
    {
        if (clip.name.Contains("_s_"))
        {
            FaceOverrideController["clip_s_ear"] = clip;
        }
        else if (clip.name.Contains("_e_"))
        {
            FaceOverrideController["clip_e_ear"] = clip;
        }
        else if (clip.name.Contains("_loop"))
        {
            UmaDatabaseEntry motion_e = null;
            UmaDatabaseEntry motion_s = null;
            if (Main.AbList.TryGetValue(clip.name.Replace("_loop", "_s"), out motion_s))
            {
                LoadAnimation(motion_s);
            }

            if (FaceOverrideController["clip_2_ear"].name.Contains("_loop"))
            {
                if (!FaceOverrideController["clip_2_ear"].name.Contains("hom_"))//home end animation not for interpolation
                {
                    if (Main.AbList.TryGetValue(FaceOverrideController["clip_2_ear"].name.Replace("_loop", "_e"), out motion_e))
                    {
                        LoadAnimation(motion_e);
                    }
                }
            }

            FaceOverrideController["clip_1_ear"] = FaceOverrideController["clip_2_ear"];
            FaceOverrideController["clip_2_ear"] = clip;
            UmaFaceAnimator.Play("motion_1", 1, 0);
            UmaFaceAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e_ear" : ((motion_s != null) ? "next_s_ear" : "next_ear"));
        }
        else
        {
            if (FaceOverrideController["clip_2"].name == "clip_2")
            {
                isAnimatorControl = true;
                FaceDrivenKeyTarget.ResetLocator();
            }
            FaceOverrideController["clip_2_ear"] = clip;
            UmaFaceAnimator.Play("motion_2", 1, 0);
        }
    }

    public void SetNextAnimationCut(string cutName)
    {
        UmaViewerMain.Instance.AbList.TryGetValue(cutName, out UmaDatabaseEntry asset);
        LoadAnimation(asset);
    }

    public void SetEndAnimationCut()
    {
        UmaViewerUI.Instance.AnimationPause();
    }

    public void LoadFaceMorph(int id, string costumeId)
    {
        if (!Head) return;
        var locatorEntry = Main.AbList["3d/animator/drivenkeylocator"];
        var bundle = UmaAssetManager.LoadAssetBundle(locatorEntry);
        var locator = Instantiate(bundle.LoadAsset("DrivenKeyLocator"), transform) as GameObject;
        locator.name = "DrivenKeyLocator";

        var headBone = (GameObject)Head.GetComponent<AssetHolder>()._assetTable["head"];
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
            new List<Renderer>(leftObj.GetComponentsInChildren<Renderer>(true)).ForEach(a => { 
                a.material.SetFloat("_StencilMask", id);
                a.material.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
                a.material.SetFloat("_StencilOp", (float)UnityEngine.Rendering.StencilOp.Keep);
                a.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            });
            LeftMangaObject.Add(leftObj);

            var RightObj = Instantiate(obj, eyeLocator_R.transform);
            if (RightObj.TryGetComponent<AssetHolder>(out var holder))
            {
                if (holder._assetTableValue["invert"] > 0)
                    RightObj.transform.localScale = new Vector3(-1, 1, 1);
            }
            new List<Renderer>(RightObj.GetComponentsInChildren<Renderer>(true)).ForEach(a => { 
                a.material.SetFloat("_StencilMask", id);
                a.material.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
                a.material.SetFloat("_StencilOp", (float)UnityEngine.Rendering.StencilOp.Keep);
                a.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            });
            RightMangaObject.Add(RightObj);
        });

        var tearEntry = new List<UmaDatabaseEntry>() {
            Main.AbList["3d/chara/common/tear/tear000/pfb_chr_tear000"],
            Main.AbList["3d/chara/common/tear/tear001/pfb_chr_tear001"],
        };

        if (tearEntry.Count > 0)
        {
            tearEntry.ForEach(a => LoadTear(a));
        }

        if (TearPrefab_0 && TearPrefab_1)
        {
            var p0 = TearPrefab_0;
            var p1 = TearPrefab_1;
            var t = headBone.transform;
            TearControllers.Add(new TearController(CharaEntry.Id, t, Instantiate(p0, t), Instantiate(p1, t), 0, 1));
            TearControllers.Add(new TearController(CharaEntry.Id, t, Instantiate(p0, t), Instantiate(p1, t), 1, 1));
            TearControllers.Add(new TearController(CharaEntry.Id, t, Instantiate(p0, t), Instantiate(p1, t), 0, 0));
            TearControllers.Add(new TearController(CharaEntry.Id, t, Instantiate(p0, t), Instantiate(p1, t), 1, 0));
        }

        var firsehead = Head;
        var faceDriven = Instantiate(firsehead.GetComponent<AssetHolder>()._assetTable["facial_target"]) as FaceDrivenKeyTarget;

        //Need Instantiate or not?
        var earDriven = firsehead.GetComponent<AssetHolder>()._assetTable["ear_target"] as DrivenKeyTarget;
        var faceOverride = firsehead.GetComponent<AssetHolder>()._assetTable["face_override"] as FaceOverrideData;

        faceDriven._earTarget = earDriven._targetFaces;
        FaceDrivenKeyTarget = faceDriven;
        FaceDrivenKeyTarget.Container = this;
        FaceOverrideData = faceOverride;
        faceOverride?.SetEnable(UI.EnableFaceOverride);
        faceDriven.DrivenKeyLocator = locator.transform;
        faceDriven.Initialize(UmaUtility.ConvertArrayToDictionary(firsehead.GetComponentsInChildren<Transform>()));

        var emotionDriven = ScriptableObject.CreateInstance<FaceEmotionKeyTarget>();
        emotionDriven.name = $"char{id}_{costumeId}_emotion_target";
        FaceEmotionKeyTarget = emotionDriven;
        emotionDriven.FaceDrivenKeyTarget = faceDriven;
        emotionDriven.FaceEmotionKey = UmaDatabaseController.Instance.FaceTypeData;
        emotionDriven.Initialize();
    }

    public void SetupBoneHandles()
    {
        if (IsLive) return;
        UIHandleCharacterRoot rootHandle = UIHandleCharacterRoot.CreateAsChild(this);

        var humanBones = GetHumanBones();
        foreach(var bone in humanBones)
        {
            List<BoneTags> tags = new List<BoneTags>() { BoneTags.Humanoid };
            if (bone.name.EndsWith("_L")) tags.Add(BoneTags.Left);
            if (bone.name.EndsWith("_R")) tags.Add(BoneTags.Right);

            if(bone.name != "Hip")
            {
                var handle = UIHandleBone.CreateAsChild(bone, tags).WithLineRenderer();
                rootHandle.ChildHandles.Add(handle);
            }
            else
            {
                var handle = UIHandleBone.CreateAsChild(bone, tags);
                rootHandle.ChildHandles.Add(handle);
            }
        }

        var wrist_L = humanBones.Find(b => b.name == "Wrist_L");
        foreach(var fingerBone in wrist_L.transform.GetComponentsInChildren<Transform>())
        {
            if(fingerBone.name.StartsWith("Index") || fingerBone.name.StartsWith("Middle") || fingerBone.name.StartsWith("Ring") || fingerBone.name.StartsWith("Pinky") || fingerBone.name.StartsWith("Thumb"))
            {
                List<BoneTags> tags = new List<BoneTags>() { BoneTags.Left, BoneTags.Finger };

                var handle = UIHandleBone.CreateAsChild(fingerBone, tags).SetScale(0.25f).WithLineRenderer();
                rootHandle.ChildHandles.Add(handle);
            }
        }

        var wrist_R = humanBones.Find(b => b.name == "Wrist_R");
        foreach (var fingerBone in wrist_R.transform.GetComponentsInChildren<Transform>())
        {
            if (fingerBone.name.StartsWith("Index") || fingerBone.name.StartsWith("Middle") || fingerBone.name.StartsWith("Ring") || fingerBone.name.StartsWith("Pinky") || fingerBone.name.StartsWith("Thumb"))
            {
                List<BoneTags> tags = new List<BoneTags>() { BoneTags.Right, BoneTags.Finger };

                var handle = UIHandleBone.CreateAsChild(fingerBone, tags).SetScale(0.25f).WithLineRenderer();
                rootHandle.ChildHandles.Add(handle);
            }
        }

        var allBones = SaveBones();
        foreach(var bone in allBones.Where(b => b.Tags.Contains(BoneTags.Dynamic)))
        {
            var handle = UIHandleBone.CreateAsChild(bone.Bone, bone.Tags).SetColor(Color.gray).SetScale(0.15f).WithLineRenderer();
            rootHandle.ChildHandles.Add(handle);
        }
    }

    public List<SerializableBone> SaveBones()
    {
        var boneList = new List<SerializableBone>();

        GatherSerializableBonesRecursive(Position, boneList, 0);

        return boneList;
    }

    private void GatherSerializableBonesRecursive(Transform current, List<SerializableBone> bones, int depth)
    {
        for(int i = 0; i < current.childCount; i++)
        {
            GatherSerializableBonesRecursive(current.GetChild(i), bones, depth + 1);
        }

        //this can be optimized by keeping track if the bone is dynamic
        //and generating a list of tags beforehand
        //otherwise getComponentInParent() is called for every bone
        var bone = new SerializableBone(current, true);
        
        if(depth == 0)
        {
            //make it independent from character name
            bone.ParentName = "root";
        }

        bones.Add(bone);
    }

    public bool LoadBones(PoseData data, PoseLoadOptions options)
    {
        var currentBones = SaveBones();

        foreach (var bone in currentBones)
        {
            if (!options.Root && bone.ParentName == "root") continue;
            if (!options.Physics && bone.Tags.Contains(BoneTags.Dynamic)) continue;

            if (bone.Bone == null)
            {
                Debug.LogError($"Target bone {bone.Name} does not reference a bone in the scene.");
                continue;
            }

            var savedBone = data.Bones.FirstOrDefault(b => b.Name == bone.Name && b.ParentName == bone.ParentName);
            if (savedBone == null) continue;

            savedBone.Transform.ApplyTo(bone.Bone, options);
        }

        //could add a 'bone not found' threshold for returning false
        return true;
    }

    public List<Transform> GetHumanBones()
    {
        if(_humanoidBones == null)
        {
            var animator = UmaAnimator;

            List<Transform> list = new List<Transform>(animator.gameObject.GetComponentsInChildren<Transform>());

            _humanoidBones = new List<Transform>()
            {
                list.Find(t => t.name.Equals("Hip")),
                list.Find(t => t.name.Equals("Waist")),
                list.Find(t => t.name.Equals("Spine")),
                list.Find(t => t.name.Equals("Chest")),
                list.Find(t => t.name.Equals("Neck")),
                list.Find(t => t.name.Equals("Head")),

                list.Find(t => t.name.Equals("Shoulder_L")),
                list.Find(t => t.name.Equals("Arm_L")),
                list.Find(t => t.name.Equals("Elbow_L")),
                list.Find(t => t.name.Equals("Wrist_L")),

                list.Find(t => t.name.Equals("Shoulder_R")),
                list.Find(t => t.name.Equals("Arm_R")),
                list.Find(t => t.name.Equals("Elbow_R")),
                list.Find(t => t.name.Equals("Wrist_R")),

                list.Find(t => t.name.Equals("Thigh_L")),
                list.Find(t => t.name.Equals("Knee_L")),
                list.Find(t => t.name.Equals("Ankle_L")),
                list.Find(t => t.name.Equals("Toe_L")),

                list.Find(t => t.name.Equals("Thigh_R")),
                list.Find(t => t.name.Equals("Knee_R")),
                list.Find(t => t.name.Equals("Ankle_R")),
                list.Find(t => t.name.Equals("Toe_R"))
            };
        }

        return _humanoidBones.Where(b => b != null).ToList();
    }

    public List<SerializableMorph> SaveMorphs(bool changedOnly)
    {
        if (FaceDrivenKeyTarget == null)
        {
            return new List<SerializableMorph>();
        }

        if (changedOnly)
        {
            return FaceDrivenKeyTarget.AllMorphs.Where(morph => morph.weight != 0).Select(morph => new SerializableMorph(morph)).ToList();
        }
        else
        {
            return FaceDrivenKeyTarget.AllMorphs.Select(morph => new SerializableMorph(morph)).ToList();
        }
    }

    public bool LoadMorphs(PoseData data)
    {
        var currentMorphs = SaveMorphs(false);

        foreach (var morph in currentMorphs)
        {
            var savedMorph = data.Morphs.FirstOrDefault(b => b.Name == morph.Name);
            if (savedMorph == null)
            {
                morph.Morph.weight = 0;
            }
            else
            {
                savedMorph.ApplyTo(this, morph.Morph, false);
            }
        }

        this.FaceDrivenKeyTarget.ChangeMorph();

        return true;
    }
}
