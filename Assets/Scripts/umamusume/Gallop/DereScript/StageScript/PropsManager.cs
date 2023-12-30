using Cutt;
using System.Collections.Generic;
using UnityEngine;

public class PropsManager
{
    private List<Props>[] _listProps = new List<Props>[15]
    {
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>(),
        new List<Props>()
    };
    public List<Props> GetPropsList(int idxChara)
    {
        List<Props> result = null;
        if (idxChara >= 0 && idxChara < _listProps.Length)
        {
            result = _listProps[idxChara];
        }
        return result;
    }

    public bool Create(int idxChara, string propsName, GameObject parentObject, CharacterObject characterObject, LiveTimelinePropsSettings.PropsConditionGroup propData, bool isUseAssetbundle, Camera targetCamera = null, bool applyCharaRotation = false)
    {
        GameObject gameObject = ResourcesManager.instance.LoadObject(GetModelResPath(propsName)) as GameObject;
        if (gameObject == null)
        {
            return false;
        }
        Props component = gameObject.GetComponent<Props>();
        if (component == null)
        {
            Object.Destroy(gameObject);
            return false;
        }
        GameObject gameObject2 = Object.Instantiate(gameObject);
        component = gameObject2.GetComponent<Props>();
        component.Init(parentObject, characterObject, _listProps[idxChara].Count, propData, targetCamera, applyCharaRotation);
        _listProps[idxChara].Add(component);
        PropsExtendController component2 = gameObject2.GetComponent<PropsExtendController>();
        if ((bool)component2)
        {
            component2.Initialize(idxChara);
        }
        return true;
    }

    public void Destroy(int idxChara)
    {
        if (idxChara < 0 || idxChara >= _listProps.Length)
        {
            return;
        }
        foreach (Props item in _listProps[idxChara])
        {
            Object.DestroyImmediate(item.gameObject);
        }
        _listProps[idxChara].Clear();
    }

    public void SetScale(int idxChara, float bodyScaleSubScale)
    {
        List<Props> list = _listProps[idxChara];
        for (int i = 0; i < list.Count; i++)
        {
            list[i].SetScale(bodyScaleSubScale);
        }
    }

    public void ResetAttach(int idxChara)
    {
        List<Props> list = _listProps[idxChara];
        for (int i = 0; i < list.Count; i++)
        {
            list[i].gameObject.transform.parent = null;
            list[i].SetCuttScale(-1);
            list[i].ResetAttach();
            GameObjectUtility.ResetTransformLocalParam(list[i].gameObject.transform);
        }
    }

    public void Reattach(int idxChara, GameObject parentObject, CharacterObject characterObject, int heightId = -1)
    {
        List<Props> list = _listProps[idxChara];
        for (int i = 0; i < list.Count; i++)
        {
            list[i].StartAttach(parentObject, characterObject);
            GameObjectUtility.ResetTransformLocalParam(list[i].gameObject.transform);
            list[i].SetCuttScale(heightId);
        }
    }

    public void ChangeAttach(int idxChara, ref PropsAttachUpdateInfo updateInfo)
    {
        List<Props> list = _listProps[idxChara];
        int propsID = updateInfo.propsID;
        if (propsID < 0 || propsID >= list.Count)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].ChangeAttach(updateInfo.attachJointHash, ref updateInfo.offsetPosition, ref updateInfo.rotation);
                if (updateInfo.changeAnimation)
                {
                    list[i].ChangeAnimationClip(updateInfo.animationId, updateInfo.animationSpeed, updateInfo.animationOffset);
                }
            }
        }
        else
        {
            list[propsID].ChangeAttach(updateInfo.attachJointHash, ref updateInfo.offsetPosition, ref updateInfo.rotation);
            if (updateInfo.changeAnimation)
            {
                list[propsID].ChangeAnimationClip(updateInfo.animationId, updateInfo.animationSpeed, updateInfo.animationOffset);
            }
        }
    }

    public void ChangeRenderState(int idxChara, Vector4 vecColor, ref PropsUpdateInfo updateInfo)
    {
        List<Props> list = _listProps[idxChara];
        int propsID = updateInfo.propsID;
        if (propsID < 0 || propsID >= list.Count)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].drawAfterImage = updateInfo.drawAfterImage;
                list[i].ChangeRenderState(vecColor, ref updateInfo);
            }
        }
        else
        {
            list[propsID].drawAfterImage = updateInfo.drawAfterImage;
            list[propsID].ChangeRenderState(vecColor, ref updateInfo);
        }
    }
    public void SetCutScale(int idxChara, int heightId = -1)
    {
        List<Props> list = _listProps[idxChara];
        for (int i = 0; i < list.Count; i++)
        {
            list[i].SetCuttScale(heightId);
        }
    }

    public void UpdateMotion(int idxChara, ref PropsUpdateInfo updateInfo)
    {
        List<Props> list = _listProps[idxChara];
        for (int i = 0; i < list.Count; i++)
        {
            list[i].UpdateMotion(updateInfo.currentTime);
        }
    }

    public void SetLayer(int idxChara, int layer)
    {
        if (idxChara < 0 || idxChara >= _listProps.Length)
        {
            return;
        }
        foreach (Props item in _listProps[idxChara])
        {
            GameObjectUtility.SetLayer(layer, item.transform);
        }
    }

    public void SetMusicScoreTime(float time)
    {
        List<Props>[] listProps = _listProps;
        for (int i = 0; i < listProps.Length; i++)
        {
            foreach (Props item in listProps[i])
            {
                item.musicScoreTime = time;
            }
        }
    }

    public static string GetAssetBundleName(int id)
    {
        return $"3d_props_{id:0000}.unity3d";
    }

    public static string GetModelResPath(string props_name)
    {
        return string.Format("3D/Props/Model/props_{0:0000}/Prefabs/pf_props_{0:0000}", props_name);
    }
}
