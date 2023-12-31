using System.Collections.Generic;
using UnityEngine;

public class UIHandleCharacterRoot : UIHandle
{
    public List<UIHandle> ChildHandles = new List<UIHandle>();

    public static UIHandleCharacterRoot CreateAsChild(UmaContainer owner)
    {
        var handle = new GameObject(owner.name + "_Handle").AddComponent<UIHandleCharacterRoot>();
        handle.transform.parent = owner.Position;
        handle.transform.localPosition = Vector3.zero;
        handle.transform.localScale = Vector3.one;
        handle.Target = owner.Position;
        handle.Init(owner.Position.gameObject).SetColor(Color.red).SetScale(0.8f);
        return handle;
    }

    public override UIHandle Init(GameObject owner)
    {
        base.Init(owner);

        Popup.AddButton("Reset All",        TransformResetAll);
        Popup.AddButton("Reset Position",   TransformResetPosition);
        Popup.AddButton("Reset Rotation",   TransformResetRotation);
        Popup.AddButton("Reset Scale",      TransformResetScale);
        Popup.AddButton("Reset to T-Pose",  TransformResetAllChildren);

        return this;
    }

    public void TransformResetAllChildren()
    {
        List<HandleManager.RuntimeGizmoUndoData> datas = new List<HandleManager.RuntimeGizmoUndoData>();

        for(int i = 0; i < ChildHandles.Count; i++)
        {
            var handle = ChildHandles[i];
            var bone = handle.Target;

            var undoData = new HandleManager.RuntimeGizmoUndoData();
            undoData.OldPosition = new SerializableTransform(bone, Space.World);
            handle.TransformReset();
            undoData.NewPosition = bone;

            datas.Add(undoData);
        }

        HandleManager.RegisterRuntimeGizmoUndoActions.Invoke(datas);
    }
}