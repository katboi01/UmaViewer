using Gallop;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UmaContainer : MonoBehaviour
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

    [Header("Live")]
    public bool IsLive = false;

    [Header("Other")]
    public CharaShaderEffectData ShaderEffectData;
    public List<MaterialHelper> Materials = new List<MaterialHelper>();


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

        List<Transform> bodybones = new List<Transform>(Body.GetComponentInChildren<SkinnedMeshRenderer>().bones);
        List<Transform> emptyBones = new List<Transform>();
        emptyBones.Add(Body.GetComponentInChildren<SkinnedMeshRenderer>().rootBone.Find("Tail_Ctrl"));
        while (Body.transform.childCount > 0)
        {
            var child = Body.transform.GetChild(0);
            child.SetParent(transform);
        }
        Body.SetActive(false); //for debugging


        //MergeHead
        if (Head)
        {
            var headskins = Head.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer headskin in headskins)
            {
                emptyBones.AddRange(MergeBone(headskin, bodybones));
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
            emptyBones.AddRange(MergeBone(tailskin, bodybones));
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
            for(int i = 0; i < rend.materials.Length; i++)
            {
                Material mat = rend.materials[i];

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

        List<Transform> bodybones = new List<Transform>(Head.GetComponentsInChildren<Transform>());
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
            emptyBones.AddRange(MergeBone(hairskin, bodybones));
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
            BodyScale = (Convert.ToInt32(CharaData[IsMob ? "chara_height" : "scale"]) / 160f);
        }
        else
        {
            BodyScale = (scale / 160f);
        }
        transform.Find("Position").localScale = new Vector3(BodyScale, BodyScale, BodyScale);
    }

    public Transform[] MergeBone(SkinnedMeshRenderer from, List<Transform> targetBones)
    {
        var rootbone = targetBones.FindLast(a => a.name.Equals(from.rootBone.name));
        if (rootbone) from.rootBone = rootbone;

        List<Transform> emptyBones = new List<Transform>();
        Transform[] tmpBone = new Transform[from.bones.Length];
        for (int i = 0; i < tmpBone.Length; i++)
        {
            var targetbone = targetBones.FindLast(a => a.name.Equals(from.bones[i].name));
            if (targetbone)
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
        return emptyBones.ToArray();
    }

    public void LoadPhysics()
    {
        cySpringDataContainers = new List<CySpringDataContainer>(PhysicsContainer.GetComponentsInChildren<CySpringDataContainer>());
        var bones = new List<Transform>(GetComponentsInChildren<Transform>());
        var colliders = new List<GameObject>();

        foreach (CySpringDataContainer spring in cySpringDataContainers)
        {
            colliders.AddRange(spring.InitiallizeCollider(bones));
        }
        foreach (CySpringDataContainer spring in cySpringDataContainers)
        {
            spring.InitializePhysics(bones, colliders);
        }
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

    Collider dragCollider;
    float dragdistance;
    Vector3 dragStartPos;
    private void FixedUpdate()
    {
        if (!IsMini)
        {
            if (TrackTarget && EnableEyeTracking && !isAnimatorControl)
            {
                if (IK && !IK.enabled)
                {
                    IK.enabled = true;
                    if(PuppetMaster.mode != PuppetMaster.Mode.Active)
                    {
                        PuppetMaster.mode = PuppetMaster.Mode.Active;
                    }
                }
                var targetPosotion = TrackTarget.transform.position - HeadBone.transform.up * EyeHeight;
                var deltaPos = HeadBone.transform.InverseTransformPoint(targetPosotion);
                var deltaRotation = Quaternion.LookRotation(deltaPos.normalized, HeadBone.transform.up).eulerAngles;
                if (deltaRotation.x > 180) deltaRotation.x -= 360;
                if (deltaRotation.y > 180) deltaRotation.y -= 360;

                var finalRotation = new Vector2(Mathf.Clamp(deltaRotation.y / 35, -1, 1), Mathf.Clamp(-deltaRotation.x / 25, -1, 1));//Limited to the angle of view 
                FaceDrivenKeyTarget.SetEyeRange(finalRotation.x, finalRotation.y, finalRotation.x, -finalRotation.y);
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
                    if (PuppetMaster.mode != PuppetMaster.Mode.Kinematic)
                    {
                        PuppetMaster.mode = PuppetMaster.Mode.Kinematic;
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
                FaceMaterial.SetFloat("_faceShadowEndY", HeadBone.transform.position.y);
            }

            TearControllers.ForEach(a => a.UpdateOffset());

        }
    }
    public MeshRenderer debug;
    private void Update()
    {
        //if (Input.GetMouseButtonDown(1))
        //{
        //    var cam = Camera.main;
        //    var distance = Mathf.Clamp(cam.transform.InverseTransformPoint(HeadBone.transform.position).magnitude - 0.1f, 0, 2);
        //    var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
        //    var worldPos = cam.ScreenToWorldPoint(mousePos);
        //    if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out RaycastHit hit))
        //    {
        //        hit.rigidbody.AddForce(Camera.main.transform.forward * 50, ForceMode.Impulse);

        //        FaceDrivenKeyTarget.ChangeMorphWeight(FaceDrivenKeyTarget.OtherMorphs[2].BindProperties[0], 2, FaceDrivenKeyTarget.OtherMorphs[2]);
        //        FaceDrivenKeyTarget.ChangeMorphWeight(FaceDrivenKeyTarget.OtherMorphs[2].BindProperties[1], 1, FaceDrivenKeyTarget.OtherMorphs[2]);
        //        Invoke(nameof(Test), 0.3f);
        //    }
        //}
    }

    //public void Test()
    //{
    //    FaceDrivenKeyTarget.ChangeMorphWeight(FaceDrivenKeyTarget.OtherMorphs[2].BindProperties[0], 0, FaceDrivenKeyTarget.OtherMorphs[2]);
    //    FaceDrivenKeyTarget.ChangeMorphWeight(FaceDrivenKeyTarget.OtherMorphs[2].BindProperties[1], 0, FaceDrivenKeyTarget.OtherMorphs[2]);
    //    FaceEmotionKeyTarget.FaceEmotionKey.ForEach(e => { if (e.target) e.Weight = 0; });
    //    var tmp = FaceEmotionKeyTarget.FaceEmotionKey[Random.Range(22, 26)];
    //    tmp.Weight = 1;
    //    FaceDrivenKeyTarget.ChangeMorph();
    //}

    public void SetNextAnimationCut(string cutName)
    {
        var asset = UmaViewerMain.Instance.AbMotions.FirstOrDefault(a => a.Name.Equals(cutName));
        UmaViewerBuilder.Instance.RecursiveLoadAsset(asset);
    }

    public void SetEndAnimationCut()
    {
        UmaViewerUI.Instance.AnimationPause();
    }

    public void UpBodyReset()
    {
        if (UpBodyBone)
        {
            UpBodyBone.transform.localPosition = UpBodyPosition;
            UpBodyBone.transform.localRotation = UpBodyRotation;
        }
    }

    BipedIK IK;
    PuppetMaster PuppetMaster;
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
        PuppetMaster.FlattenHierarchy();
    }


    public class MaterialHelper
    {
        public Material Mat;
        public UmaUIContainer Toggle;
        public Dictionary<Renderer, List<int>> Renderers;
    
        public void ToggleMaterials(bool value)
        {
            foreach(var rend in Renderers)
            {
                Debug.Log(rend.Key.name);
                Material[] mat = new Material[rend.Key.materials.Length];
                for(int i = 0; i < mat.Length; i++)
                {
                    //if material slot is not in list, keep current material
                    //if material slot in list and toggle is on - assign original
                    //if material slot in list and toggle is off - assign invisible
                    mat[i] = rend.Value.Contains(i) ? (value? Mat : UmaViewerBuilder.Instance.TransMaterialCharas ): rend.Key.materials[i];
                }

                rend.Key.materials = mat;
            }
        }
    }
}
