using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTracker : MonoBehaviour
{
    //bool HorseLoaded = false;
    //public Transform HeadNeck, HeadHead, HorseNeck, HorseHead;

    //private void Start()
    //{
    //    HeadHead = FindBoneInChildren(transform, "Head");
    //    HeadNeck = FindBoneInChildren(transform, "Neck");
    //}

    //private void LateUpdate()
    //{
    //    if (UmaViewerBuilder.Instance.Body != null)
    //    {
    //        if (HorseHead == null)
    //        {
    //            HorseHead = FindBoneInChildren(UmaViewerBuilder.Instance.Body.transform, "Head");
    //        }
    //        if (HorseNeck == null)
    //        {
    //            HorseNeck = FindBoneInChildren(UmaViewerBuilder.Instance.Body.transform, "Neck");
    //        }

    //        HeadNeck.transform.position = HorseNeck.transform.position;
    //        HeadNeck.transform.rotation = HorseNeck.transform.rotation;
    //        HeadHead.transform.position = HorseHead.transform.position;
    //        HeadHead.transform.rotation = HorseHead.transform.rotation;
    //    }
    //}

    //public Transform FindBoneInChildren(Transform trans, string name)
    //{
    //    foreach (Transform t in trans)
    //    {
    //        if (t.name == name)
    //        {
    //            return t;
    //        }
    //        else
    //        {
    //            Transform candidate = FindBoneInChildren(t, name);
    //            if (candidate != null) return candidate;
    //        }
    //    }
    //    return null;
    //}
}
