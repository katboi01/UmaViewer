using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LibMMD.Unity3D
{
    public class MaterialMorphRemover
    {
        public static void HideMaterialMorphs(MMDModel mmdModel)
        {
            foreach (UnityEngine.Material material in mmdModel.SkinnedMeshRenderer.sharedMaterials)
            {
                if (mmdModel.MaterialMorphNames.Contains(material.name))
                {
                    material.shader = Shader.Find("Custom/VoidShader");
                }
            }    
        }
    }
}