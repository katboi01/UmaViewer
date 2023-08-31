using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UmaContainerProp : UmaContainer
{
    public void LoadProp(UmaDatabaseEntry entry, Transform SetParent = null)
    {
        var go = entry.Get<GameObject>();
        var prop = Instantiate(go, SetParent ? SetParent : this.transform);

        /*
        foreach (Renderer r in prop.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                //Shaders can be differentiated by checking m.shader.name
                m.shader = Shader.Find("Unlit/Transparent Cutout");
            }
        }
        */
    }
}
