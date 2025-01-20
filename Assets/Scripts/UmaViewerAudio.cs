using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UmaViewerAudio
{

    static UmaViewerMain Main => UmaViewerMain.Instance;
    public static int LastAudioPartIndex = -1;

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
        public int cur_active_source = - 1;
        public AudioSource activeSource;
        public List<AudioSource> sourceList;

        public void SwitchActiveSource(int index, bool forceUpdate = false)
        {
            if (cur_active_source == index && !forceUpdate) return;
            cur_active_source = index;
            for(int i = 0; i < sourceList.Count; i++)
            {
                if (i + 1 == index)
                {
                    activeSource = sourceList[i];
                    sourceList[i].volume = volume;
                    sourceList[i].panStereo = pan;
                }
                else
                {
                    sourceList[i].volume = 0;
                }
            }
        }

        public void SetVolume(float volume) 
        {
            if (volume == this.volume || !activeSource) return;
            this.volume = volume;
            activeSource.volume = volume;
        }

        public void SetPanStereo(float pan)
        {
            if (pan == this.pan || !activeSource) return;
            this.pan = pan;
            activeSource.panStereo = pan;
        }
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
        foreach(var clip in sounds)
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
        foreach(var source in sourceList.sourceList)
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

    static public void AlterUpdate(float _liveCurrentTime, PartEntry partInfo, List<CuteAudioSource> liveVocal, bool forceUpdate = false)
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

        if (LastAudioPartIndex == targetIndex) return;
        LastAudioPartIndex = targetIndex;


        float volume_rate = 1;
        if (partInfo.PartSettings.ContainsKey("volume_rate"))
        {
            volume_rate = partInfo.PartSettings["volume_rate"][targetIndex];
        }
        
        if (volume_rate == 999) //chorus
        {
            var activeVocal = new List<CuteAudioSource>();
            for (int i = 0; i < liveVocal.Count; i++)
            {
                var vocal = liveVocal[i];
                var partName = vocal.tag;
                var active_index = (int)partInfo.PartSettings[partName][targetIndex];
                vocal.SwitchActiveSource(active_index, forceUpdate);

                if (partInfo.PartSettings.ContainsKey(partName + "_pan"))
                {
                    var part = partInfo.PartSettings[partName + "_pan"];
                    var pan = part[targetIndex];
                    vocal.SetPanStereo(pan == 999 ? 0 : pan);
                }

                if (active_index > 0)
                {
                    activeVocal.Add(vocal);
                }
            }

            switch(activeVocal.Count)
            {
                case 1:
                    activeVocal[0].SetVolume(0.9954f);
                    break;
                case 2:
                    activeVocal[0].SetVolume(0.7031f);
                    activeVocal[1].SetVolume(0.7031f);
                    break;
                case 3:
                    activeVocal[0].SetVolume(0.7056f);
                    activeVocal[1].SetVolume(0.56f);
                    activeVocal[1].SetVolume(0.56f);
                    break;
                default:
                    var per_vol = CalculateApproximateDBValues(0.9954f, activeVocal.Count);
                    activeVocal.ForEach(v => v.SetVolume(per_vol));
                    break;
            }
        }
        else
        {
            for (int i = 0; i < liveVocal.Count; i++)
            {
                var vocal = liveVocal[i];
                var partName = vocal.tag;
                var active_index = (int)partInfo.PartSettings[partName][targetIndex];
                vocal.SwitchActiveSource(active_index, forceUpdate);

                if (partInfo.PartSettings.ContainsKey(partName))
                {

                    if (partInfo.PartSettings.ContainsKey(partName + "_vol"))
                    {
                        var part = partInfo.PartSettings[partName + "_vol"];
                        var volume = part[targetIndex];
                        vocal.SetVolume(volume == 999 ? 0 : volume);
                    }

                    if (partInfo.PartSettings.ContainsKey(partName + "_pan"))
                    {
                        var part = partInfo.PartSettings[partName + "_pan"];
                        var pan = part[targetIndex];
                        vocal.SetPanStereo(pan == 999 ? 0 : pan);
                    }
                }
            }
        }

    }

    static public float CalculateApproximateDBValues(float total_volume, int num_sounds)
    {
        var total_db = 15f;
        var P0 = total_volume * (Math.Pow(10, total_db / 10));
        var P_per_sound = P0 / num_sounds;
        var per_volume = 10 * Math.Log10(P_per_sound / total_volume);
        return (float)(per_volume / total_db);
    }
}
