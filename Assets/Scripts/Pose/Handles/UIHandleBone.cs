using System.Collections.Generic;
using UnityEngine;
using static SerializableBone;

public class UIHandleBone : UIHandle
{
    public Transform Bone => Target as Transform;

    public List<BoneTags> Tags;

    public UIHandleBone Init(GameObject owner, List<BoneTags> boneTags)
    {
        Target = owner.transform;
        Tags = boneTags;
        Init(owner);
        if (Tags.Contains(BoneTags.Left)) SetColor(Color.green);
        if (Tags.Contains(BoneTags.Right)) SetColor(Color.blue);
        SetScale(2f);
        return this;
    }
}