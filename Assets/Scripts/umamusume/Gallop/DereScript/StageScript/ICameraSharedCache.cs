using UnityEngine;

public interface ICameraSharedCache
{
    Camera camera { get; }

    Transform transform { get; }

    Plane[] frustumPlanes { get; }
}
