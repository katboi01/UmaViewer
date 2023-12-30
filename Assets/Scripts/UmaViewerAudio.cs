using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UmaViewerAudio
{

    static UmaViewerMain Main => UmaViewerMain.Instance;
    public struct UmaSoundInfo
    {
        public UmaDatabaseEntry awb;
    }

    public class CuteAudioSource
    {
        public string tag;
        public bool enable;
        public float volume;
        public float pan;
        public List<AudioSource> sourceList;
    }

    static public UmaSoundInfo getSoundPath(string name)
    {
        UmaSoundInfo info;
        info.awb = Main.AbSounds.FirstOrDefault(a => a.Name.Contains(name) && a.Name.EndsWith("awb"));
        return info;
    }

    static public CuteAudioSource ApplySound(string cueName, int part)
    {
        UmaSoundInfo soundInfo = getSoundPath(cueName);

        GameObject sourceRoot = new GameObject("CuteAudioSource");
        sourceRoot.transform.SetParent(GameObject.Find("AudioManager/AudioControllerBgm").transform);

        List<AudioSource> sourceList = new List<AudioSource>();

        List<AudioClip> sounds = UmaViewerBuilder.LoadAudio(soundInfo.awb);
        foreach (var clip in sounds)
        {
            AudioSource source = sourceRoot.AddComponent<AudioSource>();
            source.clip = clip;
            sourceList.Add(source);
        }

        Debug.Log(sourceList);

        CuteAudioSource cute = new CuteAudioSource
        {
            tag = ((PartForm)part).ToString(),
            enable = false,
            volume = 1,
            pan = 0,
            sourceList = sourceList
        };

        return cute;
    }

    public static void Play(CuteAudioSource sourceList)
    {
        foreach (var source in sourceList.sourceList)
        {
            source.Play();
        }
    }

    public static void Stop(CuteAudioSource sourceList)
    {
        foreach (var source in sourceList.sourceList)
        {
            source.Stop();
        }
    }

    public static void SetTime(CuteAudioSource sourceList, float time)
    {
        foreach (var source in sourceList.sourceList)
        {
            source.time = time;
        }
    }

    public enum PartForm
    {
        center = 0,
        left = 1,
        right = 2,
        left2 = 3,
        right2 = 4,
        left3 = 5,
        right3 = 6,
    }

    public static void ApplyMainVolume(CuteAudioSource sourceList)
    {
        foreach (var source in sourceList.sourceList)
        {
            source.volume = (sourceList.enable ? sourceList.volume : 0);
            source.panStereo = sourceList.pan;
        }
    }

    static public void AlterUpdate(float _liveCurrentTime, PartEntry partInfo, List<CuteAudioSource> liveVocal)
    {

        var timeLineData = partInfo.PartSettings["time"];

        //need to improve
        var targetIndex = 0;
        for (int i = timeLineData.Count - 1; i >= 0; i--)
        {
            if (_liveCurrentTime >= (double)timeLineData[i] / 1000)
            {
                targetIndex = i;
                break;
            }
        }

        for (int i = 0; i < liveVocal.Count; i++)
        {
            string partName = liveVocal[i].tag;

            if (partInfo.PartSettings.ContainsKey(partName))
            {
                liveVocal[i].enable = partInfo.PartSettings[partName][targetIndex] > 0;

                if (partInfo.PartSettings.ContainsKey(partName + "_vol"))
                {
                    var volume = partInfo.PartSettings[partName + "_vol"][targetIndex];
                    liveVocal[i].volume = (volume == 999 ? 1 : volume);
                }

                if (partInfo.PartSettings.ContainsKey(partName + "_pan"))
                {
                    var pan = partInfo.PartSettings[partName + "_pan"][targetIndex];
                    liveVocal[i].pan = (pan == 999 ? 0 : pan);
                }

            }
        }


        foreach (var cute in liveVocal)
        {
            ApplyMainVolume(cute);
        }
    }
}
