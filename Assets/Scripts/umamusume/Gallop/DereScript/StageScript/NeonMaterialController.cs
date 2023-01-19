using System;
using UnityEngine;

public class NeonMaterialController : MaterialController
{
    [Serializable]
    public class MoveMaterialInfo
    {
        public Material _mainMaterial;

        public Material _backMaterial;
    }

    public class NeonMaterialExtensionData : MaterialExtensionData
    {
        public int renderMainQueueOffset;

        public int renderBackQueueOffset;
    }

    public static NeonMaterialController[] Initialize(ref MoveMaterialInfo[] materialInfos, NeonMaterialExtensionData materialExtensionData)
    {
        if (materialInfos == null)
        {
            return null;
        }
        if (materialInfos.Length == 0)
        {
            return null;
        }
        NeonMaterialController[] controllers = null;
        Material[] array = null;
        Material[] array2 = null;
        array = new Material[materialInfos.Length];
        array2 = new Material[materialInfos.Length];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = materialInfos[i]._mainMaterial;
            array2[i] = materialInfos[i]._backMaterial;
        }
        materialExtensionData.renderQueueOffsets = new int[1];
        materialExtensionData.renderQueueOffsets[0] = materialExtensionData.renderMainQueueOffset;
        MaterialController.Initialize<NeonMaterialController>(out controllers, ref array, materialExtensionData);
        materialExtensionData.renderQueueOffsets[0] = materialExtensionData.renderBackQueueOffset;
        MaterialController.CreateInstanceMaterial(controllers, ref array2, materialExtensionData);
        if (array != null && array.Length != 0)
        {
            for (int i = 0; i < array.Length; i++)
            {
                materialInfos[i]._mainMaterial = array[i];
            }
        }
        if (array2 != null && array2.Length != 0)
        {
            for (int i = 0; i < array2.Length; i++)
            {
                materialInfos[i]._backMaterial = array2[i];
            }
        }
        return controllers;
    }
}
