using Gallop;
using UnityEngine;

public class TearController
{
    public GameObject TearObject_0;
    public GameObject TearObject_1;

    public Material CurrentMaterial;
    public Material TearMaterial_0;
    public Material TearMaterial_1;

    public int CurrenObject;
    public int CurrentDir;
    public float Weight;
    public float Speed = 1;

    private Animator CurrentAnimator;
    private Animator Animator_0;
    private Animator Animator_1;

    private Transform AttachBone_2_L;
    private Transform AttachBone_2_R;
    private Transform AttachBone_3_L;
    private Transform AttachBone_3_R;

    private AssetHolder CurrentAssetHolder;
    private AssetHolder AssetHolder0;
    private AssetHolder AssetHolder1;

    bool intialized;
    public TearController(int id ,Transform headBone, GameObject tearPrefab_0, GameObject tearPrefab_1, int dir, int currentObject)
    {
        AttachBone_2_L = headBone.transform.Find("Eye_tear_attach_02_L");
        AttachBone_2_R = headBone.transform.Find("Eye_tear_attach_02_R");
        AttachBone_3_L = headBone.transform.Find("Eye_tear_attach_03_L");
        AttachBone_3_R = headBone.transform.Find("Eye_tear_attach_03_R");

        TearObject_0 = tearPrefab_0;
        AssetHolder0 = TearObject_0.GetComponent<AssetHolder>();
        TearMaterial_0 = TearObject_0.GetComponentInChildren<SkinnedMeshRenderer>().material;
        TearMaterial_0.SetFloat("_OffsetFactor", -1000);
        TearMaterial_0.SetFloat("_StencilMask", id);
        TearMaterial_0.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
        TearMaterial_0.SetFloat("_StencilOp", (float)UnityEngine.Rendering.StencilOp.Keep);
        TearMaterial_0.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        Animator_0 = TearObject_0.GetComponent<Animator>();
        var clip = AssetHolder0._assetTable["default_animation"] as AnimationClip;
        var controller = new AnimatorOverrideController(Animator_0.runtimeAnimatorController);
        controller["anm_tear000_00"] = clip;
        Animator_0.runtimeAnimatorController = controller;

        TearObject_1 = tearPrefab_1;
        AssetHolder1 = TearObject_1.GetComponent<AssetHolder>();
        TearMaterial_1 = TearObject_1.GetComponentInChildren<SkinnedMeshRenderer>().material;
        TearMaterial_1.SetFloat("_OffsetFactor", -1000);
        TearMaterial_1.SetFloat("_StencilMask", id);
        TearMaterial_1.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
        TearMaterial_1.SetFloat("_StencilOp", (float)UnityEngine.Rendering.StencilOp.Keep);
        TearMaterial_1.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        Animator_1 = TearObject_1.GetComponent<Animator>();
        clip = AssetHolder1._assetTable["default_animation"] as AnimationClip;
        controller = new AnimatorOverrideController(Animator_1.runtimeAnimatorController);
        controller["anm_tear000_00"] = clip;
        Animator_1.runtimeAnimatorController = controller;

        SetWegiht(0);
        SetDir(dir);
        SetObject(currentObject);

        intialized = true;
    }

    public void SetDir(int dir)
    {
        if (intialized && ((dir > 0 && CurrentDir > 0) || (dir <= 0 && CurrentDir <= 0))) return;

        CurrentDir = dir;
        if (dir > 0)//RightEye
        {
            TearObject_0.transform.position = AttachBone_3_R.position;
            TearObject_1.transform.position = AttachBone_2_R.position;
            TearObject_0.transform.localScale = new Vector3(-1, 1, 1);
            TearObject_1.transform.localScale = new Vector3(-1, 1, 1);
        }
        else//LeftEye
        {
            TearObject_0.transform.position = AttachBone_3_L.position;
            TearObject_1.transform.position = AttachBone_2_L.position;
            TearObject_0.transform.localScale = new Vector3(1, 1, 1);
            TearObject_1.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void SetObject(int curObject)
    {
        if (intialized && ((curObject < 1 && CurrenObject < 1) || (curObject >= 1 && CurrenObject >= 1))) return;

        CurrenObject = curObject;
        CurrentAnimator = (curObject < 1) ? Animator_0 : Animator_1;
        CurrentMaterial = (curObject < 1) ? TearMaterial_0 : TearMaterial_1;
        CurrentAssetHolder = (curObject < 1) ? AssetHolder0 : AssetHolder1;
        if (Weight > 0)
        {
            TearObject_0.SetActive(curObject < 1);
            TearObject_1.SetActive(curObject >= 1);
        }
    }

    public void SetWegiht(float weight)
    {
        if (intialized && (weight <= 0 && Weight <= 0)) return;

        Weight = weight;
        if (weight <= 0)
        {
            TearObject_0.SetActive(false);
            TearObject_1.SetActive(false);
        }
        else
        {
            TearObject_0.SetActive(CurrenObject < 1);
            TearObject_1.SetActive(CurrenObject >= 1);
        }
    }

    public void SetSpeed(float speed)
    {
        if (speed == Speed) return;
        Speed = speed;
        Animator_0.speed = speed;
        Animator_1.speed = speed;
    }

    public void UpdateOffset()
    {
        if (Weight <= 0) return;

        var time = Mathf.Repeat(CurrentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime,1);
        var width = (int)CurrentAssetHolder._assetTableValue["texture_width"];
        var height = (int)CurrentAssetHolder._assetTableValue["texture_height"];
        var partWidth = (int)CurrentAssetHolder._assetTableValue["texture_part_width"];
        var partHeight = (int)CurrentAssetHolder._assetTableValue["texture_part_height"];
        var offset = GetTextureOffset(time, width, height, partWidth, partHeight);
        CurrentMaterial.SetColor("_TexScrollParam", new Color(offset.x, -offset.y, 0, Weight));
    }

    private Vector2 GetTextureOffset(float time,int w,int h,int pw ,int ph)
    {
        int wCount = w / pw;
        int hCount = h / ph;
        int index = (int)(time * (wCount * hCount));
        return new Vector2((index % wCount) / (float)wCount,(index/wCount)/(float)hCount); 
    }

    
} 

