using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SerializableBone;

public class UIHandleBone : UIHandle
{
    public Transform Bone => Target;

    public List<BoneTags> Tags;

    public static UIHandleBone CreateAsChild(Transform owner, List<BoneTags> boneTags)
    {
        return CreateAsChild(owner.gameObject, boneTags);
    }

    public static UIHandleBone CreateAsChild(GameObject owner, List<BoneTags> boneTags)
    {
        var handle = new GameObject(owner.name + "_Handle").AddComponent<UIHandleBone>();
        handle.transform.parent = owner.transform;
        handle.transform.localPosition = Vector3.zero;
        handle.transform.localScale = Vector3.one;
        handle.Init(owner, boneTags).SetScale(0.65f);
        return handle;
    }

    public UIHandleBone Init(GameObject owner, List<BoneTags> boneTags)
    {
        Target = owner.transform;
        Tags = boneTags;
        base.Init(owner);
        if (Tags.Contains(BoneTags.Left)) SetColor(Color.green);
        if (Tags.Contains(BoneTags.Right)) SetColor(Color.blue);
        
        Popup.AddButton("Reset All",        TransformResetAll);
        Popup.AddButton("Reset Position",   TransformResetPosition);
        Popup.AddButton("Reset Rotation",   TransformResetRotation);
        Popup.AddButton("Reset Scale",      TransformResetScale);

        return this;
    }

    protected override bool ShouldBeHidden()
    {
        return _forceDisplayOff || !UmaViewerUI.Instance.HandleManager.EnabledHandles.Intersect(Tags).Any();
    }
}