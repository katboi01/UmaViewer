using Cutt;
using Stage;
using System.Collections.Generic;
using UnityEngine;

public class CharaEffectInfo
{
    public struct SettingData
    {
        public string[] sourcePrefabNames;

        public string loadFormatPath;

        public string defaultName;

        public int renderQueue;
    }

    private GameObject _gameObject;

    private Material[][] _materials;

    private int _renderQueue;

    public GameObject gameObject
    {
        get
        {
            return _gameObject;
        }
        set
        {
            _gameObject = value;
        }
    }

    public int renderQueue
    {
        set
        {
            _renderQueue = value;
        }
    }

    public static void AddAssetBundleList(ref List<string> nameList, LiveTimelineControl liveTimelineControl)
    {
        if (nameList != null && !(liveTimelineControl == null))
        {
            ResourcesManager instance = SingletonMonoBehaviour<ResourcesManager>.instance;
            if (liveTimelineControl.data.shadowPrefabNames == null || liveTimelineControl.data.shadowPrefabNames.Length == 0)
            {
                string text = "3d_charaeffect_shadow_0001.unity3d";
                if (instance.ExistsAssetBundleManifest(text))
                {
                    nameList.Add(text);
                }
            }
            else
            {
                for (int i = 0; i < liveTimelineControl.data.shadowPrefabNames.Length; i++)
                {
                    string text = "3d_charaeffect_" + liveTimelineControl.data.shadowPrefabNames[i] + ".unity3d";
                    if (instance.ExistsAssetBundleManifest(text))
                    {
                        nameList.Add(text);
                    }
                }
            }
            if (liveTimelineControl.data.spotLightPrefabNames == null || liveTimelineControl.data.spotLightPrefabNames.Length == 0)
            {
                string text = "3d_charaeffect_spotlight_0001.unity3d";
                if (instance.ExistsAssetBundleManifest(text))
                {
                    nameList.Add(text);
                }
            }
            else
            {
                for (int i = 0; i < liveTimelineControl.data.spotLightPrefabNames.Length; i++)
                {
                    string text = "3d_charaeffect_" + liveTimelineControl.data.spotLightPrefabNames[i] + ".unity3d";
                    if (instance.ExistsAssetBundleManifest(text))
                    {
                        nameList.Add(text);
                    }
                }
            }
        }
    }

    public static void LoadCharaEffectObjects(ref List<CharaEffectInfo> listCharaEffectInfo, SettingData settingData)
    {
        if (listCharaEffectInfo == null)
        {
            listCharaEffectInfo = new List<CharaEffectInfo>();
        }
        else
        {
            for (int i = 0; i < listCharaEffectInfo.Count; i++)
            {
                Object.DestroyImmediate(listCharaEffectInfo[i].gameObject);
            }
            listCharaEffectInfo.Clear();
        }
        if (settingData.sourcePrefabNames != null)
        {
            ResourcesManager instance = SingletonMonoBehaviour<ResourcesManager>.instance;
            string[] array = (settingData.sourcePrefabNames.Length <= 0) ? new string[1]
            {
                settingData.defaultName
            } : settingData.sourcePrefabNames;
            for (int i = 0; i < array.Length; i++)
            {
                if (!string.IsNullOrEmpty(array[i]))
                {
                    string objectName = string.Format(settingData.loadFormatPath, array[i]);
                    GameObject gameObject = instance.LoadObject(objectName) as GameObject;
                    if (!(gameObject == null))
                    {
                        CharaEffectInfo charaEffectInfo = new CharaEffectInfo();
                        charaEffectInfo.gameObject = gameObject;
                        charaEffectInfo.renderQueue = settingData.renderQueue;
                        listCharaEffectInfo.Add(charaEffectInfo);
                    }
                }
            }
        }
    }

    public GameObject CreateInstance(Transform pareantTransform)
    {
        if (_gameObject == null)
        {
            return null;
        }
        GameObject gameObject = Object.Instantiate(_gameObject);
        if (pareantTransform != null)
        {
            gameObject.transform.SetParent(pareantTransform, false);
        }
        Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
        if (_materials == null)
        {
            _materials = new Material[componentsInChildren.Length][];
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                _materials[i] = new Material[componentsInChildren[i].sharedMaterials.Length];
                for (int j = 0; j < componentsInChildren[i].sharedMaterials.Length; j++)
                {
                    _materials[i][j] = Object.Instantiate(componentsInChildren[i].sharedMaterials[j]);
                    _materials[i][j].renderQueue = _renderQueue;
                }
            }
        }
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            componentsInChildren[i].sharedMaterials = _materials[i];
        }
        return gameObject;
    }
}
