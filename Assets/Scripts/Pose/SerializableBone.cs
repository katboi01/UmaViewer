using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableBone
{
    public string Name = "";
    public string ParentName = "";
    public List<BoneTags> Tags = new List<BoneTags>();
    public SerializableTransform Transform = new SerializableTransform();

    /// <summary> Reference to original bone for runtime. Not saved to file. </summary>
    [NonSerialized] public Transform Bone;

    public SerializableBone(){}

    public SerializableBone(Transform t, bool generateTags = true, List<BoneTags> tags = null)
    {
        if(tags == null) tags = new List<BoneTags>();

        if (generateTags)
        {
            if (t.name.EndsWith("_L"))
            {
                tags.Add(BoneTags.Left);
            }
            else if (t.name.EndsWith("_R"))
            {
                tags.Add(BoneTags.Right);
            }

            if(t.GetComponentInParent<DynamicBone>() != null)
            {
                tags.Add(BoneTags.Dynamic);
            }
        }

        Name = t.name;
        Tags = tags;
        Transform = new SerializableTransform(t, Space.Self);
        ParentName = t.parent == null ? "root" : t.parent.name;

        Bone = t;
    }

    //feel free to add more. do not change order!
    public enum BoneTags
    {
        Left,
        Right,
        Dynamic,
        Humanoid,
        Finger,
        Face,
        IK,
    }
}
