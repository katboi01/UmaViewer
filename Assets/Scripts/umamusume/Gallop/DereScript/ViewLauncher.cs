using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Cutt;
using Stage;
using Cute;

/// <summary>
/// menuからviewへ情報を連携する
/// </summary>
public class ViewLauncher : MonoBehaviour
{
    private static ViewLauncher _instance;
    public static ViewLauncher instance
    {
        get
        {
            return _instance;
        }
    }

    //public SQMusicData music;

    public bool isRich = true;

    public CharaDirector[] charaDirector;

    public LiveDirector liveDirector;

    public List<string> shaderFiles
    {
        get
        {
            List<string> filenames = new List<string>();

            filenames.Add("3d_shader.unity3d");
            filenames.Add("3d_shader01.unity3d");
            filenames.Add("3d_shader02.unity3d");
            filenames.Add("3d_shader03.unity3d");
            filenames.Add("3d_shader04.unity3d");
            filenames.Add("3d_shader05.unity3d");

            return filenames;
        }
    }

    public List<string> assetFiles
    {
        get
        {
            List<string> filenames = new List<string>();

            filenames.Add("3d_uvm_texture.unity3d");
            filenames.Add("3d_stage_common.unity3d");
            filenames.Add("3d_stage_common_hq.unity3d");
            filenames.Add("3d_lightshuft_0001.unity3d");
            filenames.Add("3d_lightshuft_0002.unity3d");
            filenames.Add("3d_lightshuft_0003.unity3d");
            filenames.Add("3d_a2u_0001.unity3d");
            filenames.Add("3d_a2u_0002.unity3d");
            filenames.Add("3d_a2u_0003.unity3d");
            filenames.Add("3d_a2u_0004.unity3d");
            filenames.Add("3d_a2u_0005.unity3d");
            filenames.Add("3d_a2u_0006.unity3d");

            return filenames;
        }
    }
    
    // Use this for initialization
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void Awake()
    {
        if (_instance != null)
        {
            UnityEngine.Object.Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public void SetValue(CharaDirector[] _charaDirector)
    {
        charaDirector = _charaDirector;
    }

    public List<string> GetCommonAssetList()
    {
        List<string> list = new List<string>();
        list.AddRange(shaderFiles);
        list.AddRange(assetFiles);

        return list;
    }

    public List<string> GetStageAssetList()
    {
        List<string> list = new List<string>();
        if (liveDirector != null)
        {
            list.AddRange(liveDirector.GetAssetFiles());
        }
        return list;
    }

    public List<string> GetCharaAssetList()
    {
        List<string> list = new List<string>();
        foreach (var tmp in charaDirector)
        {
            //list.AddRange(tmp.GetAssetFiles());
            var chara = tmp.characterData;
            Character3DBase.GetAssetBundleList(list, chara.charaId, chara.activeDressId, chara.activeDressId);
            if (tmp.isAppendDress)
            {
                var appendCharacterData = tmp.appendCharacterData;
                Character3DBase.GetAssetBundleList(list, appendCharacterData.charaId, appendCharacterData.activeDressId, appendCharacterData.activeDressId);
            }
        }

        return list;
    }

    public void LiveEnd()
    {
        charaDirector = null;
        liveDirector = null;
        //isRich = false;
    }

    public void Init()
    {
        charaDirector = null;
        liveDirector = null;
    }

    public void Clear()
    {
        charaDirector = null;
        liveDirector = null;
    }
}
