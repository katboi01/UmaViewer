
using SerializableTypes;
using UnityEngine;

[System.Serializable]
public class SerializableTransform
{
    public Space Space;
    public SVector3 Position;
    public SVector3 Rotation;
    public SVector3 Scale;

    public SerializableTransform()
    {
        Space = Space.World;
        Scale = Vector3.one;
        Position = Vector3.zero;
        Rotation = Vector3.zero;
    }

    public SerializableTransform(Transform t, Space space)
    {
        Space = space;
        if (space == Space.World)
        {
            Position = t.position;
            Rotation = t.eulerAngles;
            Scale = t.localScale; //lossyscale is read-only, so can't be used later
        }
        else
        {
            Position = t.localPosition;
            Rotation = t.localEulerAngles;
            Scale = t.localScale;
        }
    }

    public void ApplyTo(Transform t)
    {
        if (Space == Space.World)
        {
            t.position = Position;
            t.eulerAngles = Rotation;
            t.localScale = Scale;
        }
        else
        {
            t.localPosition = Position;
            t.localEulerAngles = Rotation;
            t.localScale = Scale;
        }
    }

    public void ApplyTo(Transform t, PoseLoadOptions options)
    {
        if (Space == Space.World)
        {
            if (options.Position) t.position = Position;
            if (options.Rotation) t.eulerAngles = Rotation;
            if (options.Scale) t.localScale = Scale;
        }
        else
        {
            if (options.Position) t.localPosition = Position;
            if (options.Rotation) t.localEulerAngles = Rotation;
            if (options.Scale) t.localScale = Scale;
        }
    }
}