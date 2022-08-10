using Gallop;
using Newtonsoft.Json.Linq;
using RootMotion.Dynamics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UmaContainer : MonoBehaviour
{
    public JObject CharaData;
    public GameObject Body;
    public GameObject Tail;
    public List<Texture2D> TailTextures = new List<Texture2D>();
    public Animator UmaAnimator;
    public AnimatorOverrideController OverrideController;

    [Header("Face")]
    public List<AnimationClip> FacialClips = new List<AnimationClip>();
    public FaceDrivenKeyTarget FaceDrivenKeyTargets;


    [Header("Generic")]
    public bool IsGeneric = false;
    public string VarCostumeIdShort, VarCostumeIdLong, VarSkin, VarHeight, VarSocks, VarBust;
    public List<Texture2D> GenericBodyTextures = new List<Texture2D>();

    [Header("Mini")]
    public bool IsMini = false;
    public List<Texture2D> MiniHeadTextures = new List<Texture2D>();

    [Header("Head Assembly")]
    public Transform UmaNeckBone;
    public Transform UmaHeadBone;
    public List<GameObject> Heads = new List<GameObject>();
    public List<Transform> HeadNeckBones = new List<Transform>();
    public List<Transform> HeadHeadBones = new List<Transform>();
    private void LateUpdate()
    {
        if (Body != null && Heads.Count > 0)
        {
            if (UmaNeckBone == null) UmaNeckBone = FindBoneInChildren(Body.transform, "Neck");
            if (UmaHeadBone == null) UmaHeadBone = FindBoneInChildren(Body.transform, "Head");
            for (int i = 0; i < Heads.Count; i++)
            {
                HeadNeckBones[i].transform.SetPositionAndRotation(UmaNeckBone.position, UmaNeckBone.rotation);
                HeadHeadBones[i].transform.SetPositionAndRotation(UmaHeadBone.position, UmaHeadBone.rotation);
            }
        }
    }

    public static Transform FindBoneInChildren(Transform trans, string name)
    {
        foreach (Transform t in trans)
        {
            if (t.name == name)
            {
                return t;
            }
            else
            {
                Transform candidate = FindBoneInChildren(t, name);
                if (candidate != null) return candidate;
            }
        }
        return null;
    }

    public void OnDestroy()
    {
        if (TryGetComponent<PuppetMaster>(out PuppetMaster t))
        {
            Destroy(this);
        }
        else { Destroy(gameObject); }
        
    }
}
