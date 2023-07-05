using CriWare;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CriWare.CriAtomExCategory.ReactDuckerParameter;

public class UmaViewerAudio
{

    static UmaViewerMain Main => UmaViewerMain.Instance;
    public struct UmaSoundInfo
    {
        public string acbPath;
        public string awbPath;
    }

    static public UmaSoundInfo getSoundPath(string name)
    {
        Debug.Log(name);
        UmaSoundInfo info;
        info.acbPath = Main.AbSounds.FirstOrDefault(a => a.Name.Contains(name) && a.Name.EndsWith("acb")).FilePath;
        info.awbPath = Main.AbSounds.FirstOrDefault(a => a.Name.Contains(name) && a.Name.EndsWith("awb")).FilePath;
        return info;
    }

    static public CriAtomSource ApplyCueSheet(string cueName)
    {
        UmaSoundInfo soundInfo = getSoundPath(cueName);
        CriAtom.AddCueSheet(cueName, soundInfo.acbPath, soundInfo.awbPath);

        //´´½¨²¥·ÅÆ÷
        CriAtomSource source = new GameObject("CuteAudioSource").AddComponent<CriAtomSource>();
        source.transform.SetParent(GameObject.Find("AudioManager/AudioControllerBgm").transform);
        source.cueSheet = cueName;

        source.use3dPositioning = false;

        return source;
    }

    public enum PartForm
    {
        center = 0,
        left = 1,
        right = 2
    }

    static public void AlterUpdate(float _liveCurrentTime, PartEntry partInfo, List<CriAtomSource> liveVocal)
    {
        var timeLineData = partInfo.PartSettings["time"];
        var targetIndex = 0;
        for (int i = timeLineData.Count - 1; i >= 0; i--)
        {
            if (_liveCurrentTime >= (double)timeLineData[i] / 1000)
            {
                targetIndex = i;
                break;
            }
        }

        var volumeMixer = 0;
        var useMixer = false;

        float _999_count = 0;

        for (int i = 0; i < liveVocal.Count; i++)
        {
            string partName = ((PartForm)i).ToString();

            if (partInfo.PartSettings.ContainsKey(partName)){
                var value = partInfo.PartSettings[partName][targetIndex];

                if (value == 0)
                {
                    liveVocal[i].volume = 0;
                    volumeMixer += 1;
                }
                else
                {
                    liveVocal[i].volume = 1;
                    liveVocal[i].player.SetSelectorLabel("BGM_Selector", $"SelectorLabel_{value-1}");
                }

                
                float volume;
                if (partInfo.PartSettings.ContainsKey(partName + "_vol"))
                {
                    volume = partInfo.PartSettings[partName + "_vol"][targetIndex];
                    if(volume != 999)
                    {
                        liveVocal[i].volume *= volume;
                    }
                    else
                    {
                        _999_count += 1;
                    }
                }  
            }
        }

        if (_999_count == liveVocal.Count)
        {
            useMixer = true;
        }

        if (useMixer)
        {
            switch (volumeMixer)
            {
                case 0:
                    liveVocal[0].volume = 0.7056f;
                    if(liveVocal.Count > 2)
                    {
                        liveVocal[2].volume = 0.56f;
                    }
                    if (liveVocal.Count > 1)
                    {
                        liveVocal[1].volume = 0.56f;

                    }
                    break;
                case 1:
                    for (int i = 0; i < liveVocal.Count; i++)
                    {
                        if (liveVocal[i].volume != 0)
                        {
                            liveVocal[i].volume = 0.7031f;
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < liveVocal.Count; i++)
                    {
                        if (liveVocal[i].volume != 0)
                        {
                            liveVocal[i].volume = 0.9954f;
                        }
                    }
                    break;
                default:
                    for (int i = 0; i < liveVocal.Count; i++)
                    {
                        liveVocal[i].volume = 0;
                    }
                    break;
            }
        }
    }
}
