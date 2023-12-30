using Cutt;
using System.Collections.Generic;
using UnityEngine;

public class GimmickController
{
    private Dictionary<int, Gimmick> _mapGimmick = new Dictionary<int, Gimmick>();

    public void Initialize(GameObject objParent)
    {
        if (!(objParent != null))
        {
            return;
        }
        Gimmick[] componentsInChildren = objParent.GetComponentsInChildren<Gimmick>();
        foreach (Gimmick gimmick in componentsInChildren)
        {
            int key = FNVHash.Generate(gimmick.name);
            if (!_mapGimmick.ContainsKey(key) && gimmick.Initialize())
            {
                _mapGimmick.Add(key, gimmick);
            }
        }
    }

    public void SetShaderBehavior(int hash, LiveTimelineKeyShaderControlData.eBehaviorFlag behaviorFlags)
    {
        Gimmick value = null;
        if (_mapGimmick.TryGetValue(hash, out value))
        {
            value.SetShaderBehavior(behaviorFlags);
        }
    }
}
