using System.Linq;
using UnityEngine;

public static class GameObjectExtensions
{
    public static GameObject[] GetChildren(this GameObject self, bool includeInactive = false)
    {
        return (from c in self.GetComponentsInChildren<Transform>(includeInactive)
                where c != self.transform
                select c.gameObject).ToArray<GameObject>();
    }
}
