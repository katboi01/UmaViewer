using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsSound : MonoBehaviour
{
    static UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI ProgressText;
    public Slider ProgressSlider;
    public Button PlayButton;
    public Text LyricsText;

    internal void UpdateTrack(AudioSource mianSource)
    {
        TitleText.text = mianSource.clip.name;
        ProgressText.text = string.Format("{0} / {1}", ToTimeFormat(mianSource.time), ToTimeFormat(mianSource.clip.length));
        ProgressSlider.SetValueWithoutNotify(mianSource.time / mianSource.clip.length);
        LyricsText.text = UmaUtility.GetCurrentLyrics(mianSource.time, UmaViewerBuilder.Instance.CurrentLyrics);
        LyricsText.text = LyricsText.text;
    }

    public void PauseAudio()
    {
        var sources = Builder.CurrentAudioSources;
        if (sources.Count > 0)
        {
            AudioSource MainSource = sources[0];
            var state = MainSource.isPlaying;
            foreach (AudioSource source in sources)
            {
                if (state)
                {
                    source.Pause();
                }
                else if (source.clip)
                {
                    source.Play();
                }
                else
                {
                    source.Stop();
                }
            }
        }
    }

    public void StopAudio()
    {
        var sources = Builder.CurrentAudioSources;
        foreach (AudioSource source in sources)
        {
            source.Stop();
        }
    }

    public void ResetPlayer()
    {
        TitleText.text = "No Audio";
        ProgressText.text = "00:00:00 / 00:00:00";
        ProgressSlider.SetValueWithoutNotify(0);
        LyricsText.text = "";
    }

    public void AudioProgressChange(float val)
    {
        if (Builder.CurrentAudioSources.Count > 0)
        {
            var time = Builder.CurrentAudioSources[0].clip.length * val;
            foreach (AudioSource source in Builder.CurrentAudioSources)
            {
                if (source.clip)
                {
                    source.time = Mathf.Clamp(time, 0, source.clip.length);
                }
            }
        }
    }

    public static string ToTimeFormat(float time)
    {
        int seconds = (int)time;
        int hour = seconds / 3600;
        int minute = seconds % 3600 / 60;
        seconds = seconds % 3600 % 60;
        return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, minute, seconds);
    }
}
