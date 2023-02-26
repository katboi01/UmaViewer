using System;
using UnityEngine;

public class ShaderLodManager : MonoBehaviour
{
    [Serializable]
    public class ShaderLevelData
    {
        public Shader low2;

        public Shader low1;

        public Shader original;

        public Shader high1;

        public Shader high2;

        public Shader this[int idx]
        {
            get
            {
                switch (idx + 2)
                {
                    case 0:
                        return this.low2;
                    case 1:
                        return this.low1;
                    case 2:
                        return this.original;
                    case 3:
                        return this.high1;
                    case 4:
                        return this.high2;
                    default:
                        return null;
                }
            }
        }
    }

    [SerializeField]
    private ShaderLodManager.ShaderLevelData[] _shaderLevelDataList;

    public void SwitchShader(int level)
    {
        if (level == 0)
        {
            return;
        }
        Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
        int num;
        if (level < 0)
        {
            num = 1;
        }
        else
        {
            num = -1;
        }
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            Shader shader = componentsInChildren[i].sharedMaterial.shader;
            Shader shader2 = null;
            for (int j = 0; j < this._shaderLevelDataList.Length; j++)
            {
                ShaderLodManager.ShaderLevelData shaderLevelData = this._shaderLevelDataList[j];
                if (!(shader != shaderLevelData[0]))
                {
                    for (int num2 = level; num2 != 0; num2 += num)
                    {
                        shader2 = shaderLevelData[num2];
                        if (shader2 != null)
                        {
                            break;
                        }
                    }
                    if (shader2 != null)
                    {
                        break;
                    }
                }
            }
            if (shader2 != null)
            {
                Material sharedMaterial = componentsInChildren[i].sharedMaterial;
                Material material = UnityEngine.Object.Instantiate<Material>(sharedMaterial);
                material.shader = shader2;
                for (int j = i; j < componentsInChildren.Length; j++)
                {
                    if (componentsInChildren[j].sharedMaterial == sharedMaterial)
                    {
                        componentsInChildren[j].sharedMaterial = material;
                    }
                }
            }
        }
    }
}
