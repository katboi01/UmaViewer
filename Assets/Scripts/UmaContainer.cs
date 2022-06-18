using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UmaContainer : MonoBehaviour
{
    public Transform UmaNeckBone;
    public Transform UmaHeadBone;
    public Transform HeadNeckBone;
    public Transform HeadHeadBone;
    public GameObject Body;
    public GameObject Tail;
    public List<Texture2D> TailTextures = new List<Texture2D>();
    public GameObject Head;
    public Animator UmaAnimator;
    public AnimatorOverrideController OverrideController;


    private void LateUpdate()
    {
        if (Body != null && Head != null)
        {
            if (UmaNeckBone == null) UmaNeckBone = FindBoneInChildren(Body.transform, "Neck");
            if (UmaHeadBone == null) UmaHeadBone = FindBoneInChildren(Body.transform, "Head");
            if (HeadNeckBone == null) HeadNeckBone = FindBoneInChildren(Head.transform, "Neck");
            if (HeadHeadBone == null) HeadHeadBone = FindBoneInChildren(Head.transform, "Head");
            HeadNeckBone.transform.SetPositionAndRotation(UmaNeckBone.position, UmaNeckBone.rotation);
            HeadHeadBone.transform.SetPositionAndRotation(UmaHeadBone.position, UmaHeadBone.rotation);
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
        Destroy(gameObject);
    }
}
