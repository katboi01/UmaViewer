using Live3D;
using UnityEngine;

public class MaterialController
{
    public class MaterialExtensionData
    {
        public string[] materialNames;

        public GameObject rootGameObject;

        public int[] renderQueueOffsets;

        public void Clear()
        {
            materialNames = null;
            rootGameObject = null;
            renderQueueOffsets = null;
        }
    }

    public Renderer[] _Renderer;

    public bool Start(ref Material material, string materialName, GameObject gameObject)
    {
        if (material == null)
        {
            return false;
        }
        Common.GetRendererFromMaterial(out _Renderer, ref material, materialName, gameObject);
        if (_Renderer == null)
        {
            _Renderer = new Renderer[0];
        }
        return true;
    }

    protected static bool CreateInstanceMaterial<T>(T[] controllers, ref Material[] materials, MaterialExtensionData materialExtensionData) where T : new()
    {
        for (int i = 0; i < controllers.Length; i++)
        {
            MaterialController materialController = controllers[i] as MaterialController;
            if (materialExtensionData.materialNames == null)
            {
                if (!materialController.Start(ref materials[i], null, materialExtensionData.rootGameObject))
                {
                    return false;
                }
            }
            else if (!materialController.Start(ref materials[i], materialExtensionData.materialNames[i], materialExtensionData.rootGameObject))
            {
                return false;
            }
        }
        SetRenderQueue(materials, materialExtensionData);
        return true;
    }

    protected static bool Initialize<T>(out T[] controllers, ref Material[] materials, MaterialExtensionData materialExtensionData) where T : new()
    {
        controllers = null;
        if (materials == null)
        {
            return false;
        }
        if (materials.Length == 0)
        {
            return false;
        }
        if (materialExtensionData.rootGameObject == null)
        {
            return false;
        }
        controllers = new T[materials.Length];
        for (int i = 0; i < controllers.Length; i++)
        {
            controllers[i] = new T();
        }
        return CreateInstanceMaterial(controllers, ref materials, materialExtensionData);
    }

    public static MaterialController[] Initialize(ref Material[] material, MaterialExtensionData materialExtensionData)
    {
        MaterialController[] controllers = null;
        Initialize<MaterialController>(out controllers, ref material, materialExtensionData);
        return controllers;
    }

    public static void SetRenderQueue(Material[] materials, MaterialExtensionData extensionData)
    {
        if (materials == null || materials.Length == 0)
        {
            return;
        }
        int num;
        int num2;
        if (extensionData == null || extensionData.renderQueueOffsets == null || extensionData.renderQueueOffsets.Length == 0)
        {
            num = 0;
            num2 = 2000;
        }
        else
        {
            num = extensionData.renderQueueOffsets.Length;
            num2 = extensionData.renderQueueOffsets[0];
        }
        for (int i = 0; i < materials.Length; i++)
        {
            if (i < num)
            {
                materials[i].renderQueue = extensionData.renderQueueOffsets[i] + i;
            }
            else
            {
                materials[i].renderQueue = num2 + i;
            }
        }
    }
}
