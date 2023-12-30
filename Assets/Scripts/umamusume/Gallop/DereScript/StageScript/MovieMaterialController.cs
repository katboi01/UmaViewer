using System;
using UnityEngine;

public class MovieMaterialController : MaterialController
{
    public enum MovieType : byte
    {
        MainMonitor,
        SubMonitor,
        Parts,
        Overlay
    }

    [Serializable]
    public class MoveMaterialInfo
    {
        public Material _movieMaterial;

        public MovieType _movieType;

        public Define.RenderOrder _renderOrder = Define.RenderOrder.Geometry;
    }

    public MonitorController _monitorContoroller = new MonitorController();

    private static void SetupMaterial(MoveMaterialInfo[] materialInfos, ref Material[] materials, MaterialExtensionData materialExtensionData)
    {
        materials = new Material[materialInfos.Length];
        materialExtensionData.renderQueueOffsets = new int[materialInfos.Length];
        for (int i = 0; i < materialInfos.Length; i++)
        {
            materials[i] = materialInfos[i]._movieMaterial;
            if (materialInfos[i]._renderOrder == Define.RenderOrder.Geometry || materialInfos[i]._renderOrder == 0)
            {
                materialExtensionData.renderQueueOffsets[i] = 2001 + i;
            }
            else
            {
                materialExtensionData.renderQueueOffsets[i] = 3070 + i;
            }
        }
    }

    public static MovieMaterialController[] Initialize(ref MoveMaterialInfo[] materialInfos, MaterialExtensionData materialExtensionData)
    {
        if (materialInfos == null)
        {
            return null;
        }
        if (materialInfos.Length == 0)
        {
            return null;
        }
        MovieMaterialController[] controllers = null;
        Material[] materials = null;
        SetupMaterial(materialInfos, ref materials, materialExtensionData);
        MaterialController.Initialize<MovieMaterialController>(out controllers, ref materials, materialExtensionData);
        if (materials != null && materials.Length != 0)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                materialInfos[i]._movieMaterial = materials[i];
            }
        }
        return controllers;
    }

    public static void SetRenderQueue(MoveMaterialInfo[] materialInfos, MaterialExtensionData extensionData)
    {
        if (materialInfos != null)
        {
            if (extensionData == null)
            {
                extensionData = new MaterialExtensionData();
            }
            Material[] materials = null;
            SetupMaterial(materialInfos, ref materials, extensionData);
            MaterialController.SetRenderQueue(materials, extensionData);
        }
    }
}
