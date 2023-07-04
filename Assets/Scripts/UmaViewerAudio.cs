using CriWare;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UmaViewerAudio
{
    static public void LoadLiveSoundCri(int songid, UmaDatabaseEntry SongAwb)
    {
        Debug.Log(SongAwb.Name);

        //获取总线数
        Debug.Log(CriAtomExAcf.GetNumDspSettings());

        string busName = CriAtomExAcf.GetDspSettingNameByIndex(0);

        var dspSetInfo = new CriAtomExAcf.AcfDspSettingInfo();



        Debug.Log(CriAtomExAcf.GetDspSettingInformation(busName, out dspSetInfo));

        Debug.Log(dspSetInfo.name);
        Debug.Log(dspSetInfo.numExtendBuses);
        Debug.Log(dspSetInfo.numBuses);
        Debug.Log(dspSetInfo.numSnapshots);
        Debug.Log(dspSetInfo.snapshotStartIndex);

        var busInfo = new CriAtomExAcf.AcfDspBusInfo();

        for (ushort i = 0; i < dspSetInfo.numExtendBuses; i++)
        {
            Debug.Log(i);
            Debug.Log(CriAtomExAcf.GetDspBusInformation(i, out busInfo));

            Debug.Log(busInfo.name);
            Debug.Log(busInfo.volume);
            Debug.Log(busInfo.numFxes);
            Debug.Log(busInfo.numBusLinks);
        }

        //获取Acb文件和Awb文件的路径
        string nameVar = SongAwb.Name.Split('.')[0].Split('/').Last();

        //使用Live的Bgm
        //nameVar = $"snd_bgm_live_{songid}_oke";

        LoadSound Loader = (LoadSound)ScriptableObject.CreateInstance("LoadSound");
        LoadSound.UmaSoundInfo soundInfo = Loader.getSoundPath(nameVar);

        //音频组件添加路径，载入音频
        CriAtom.AddCueSheet(nameVar, soundInfo.acbPath, soundInfo.awbPath);

        //获得当前音频信息
        CriAtomEx.CueInfo[] cueInfoList;
        List<string> cueNameList = new List<string>();
        cueInfoList = CriAtom.GetAcb(nameVar).GetCueInfoList();
        foreach (CriAtomEx.CueInfo cueInfo in cueInfoList)
        {
            cueNameList.Add(cueInfo.name);
            Debug.Log(cueInfo.type);
            Debug.Log(cueInfo.userData);
        }

        //创建播放器
        CriAtomSource source = new GameObject("CuteAudioSource").AddComponent<CriAtomSource>();
        source.transform.SetParent(GameObject.Find("AudioManager/AudioControllerBgm").transform);
        source.cueSheet = nameVar;

        //播放
        source.Play(cueNameList[0]);

        //source.SetBusSendLevelOffset(1, 1);


        /*
        for (int i = 0; i < 17; i++)
        {
            Debug.Log(source.player.GetParameterFloat32((CriAtomEx.Parameter)i));
        }
        */

    }
}
