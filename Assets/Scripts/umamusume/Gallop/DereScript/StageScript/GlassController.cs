using Cutt;
using System.Collections.Generic;
using UnityEngine;

public class GlassController : MonoBehaviour
{
    private Dictionary<int, Glass> _mapGlass = new Dictionary<int, Glass>();

    private void Start()
    {
        Glass[] componentsInChildren = GetComponentsInChildren<Glass>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            int key = FNVHash.Generate(componentsInChildren[i].name);
            if (!_mapGlass.ContainsKey(key))
            {
                _mapGlass.Add(key, componentsInChildren[i]);
            }
        }
    }

    public void UpdateInfo(ref GlassUpdateInfo updateInfo)
    {
        Glass value = null;
        if (_mapGlass.TryGetValue(updateInfo.data.nameHash, out value) && value != null)
        {
            value.UpdateInfo(ref updateInfo);
        }
    }
}
