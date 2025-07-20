using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using NAudio.Lame;
using NAudio.Wave.WZT;
using System.IO;

public class Test : MonoBehaviour
{
    public InputField input;
    public Button button;

    // Use this for initialization
    IEnumerator Start()
    {
        string testWav = "testWav.wav";
        string testMp3 = "testMp3.mp3";
        WWW www = new WWW(GetFileStreamingAssetsPath(testWav));
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            string saveWavPath = Path.Combine(Application.persistentDataPath, testWav);
            File.WriteAllBytes(saveWavPath, www.bytes);
            button.onClick.AddListener(() =>
            {
                string saveMp3Path = Path.Combine(Application.persistentDataPath, testMp3);
                var bitRate = string.IsNullOrEmpty(input.text) ? LAMEPreset.ABR_128 : (LAMEPreset)int.Parse(input.text);
                WaveToMP3(saveWavPath, saveMp3Path, bitRate);
                StartCoroutine(PlayMp3(saveMp3Path));
            });
        }
    }

    public virtual string GetFileStreamingAssetsPath(string path)
    {
        string filePath = null;
#if UNITY_EDITOR
        filePath = string.Format("file://{0}/StreamingAssets/{1}", Application.dataPath, path);
#elif UNITY_ANDROID
        filePath = string.Format("jar:file://{0}!/assets/{1}", Application.dataPath, path);
#else
        filePath = string.Format("file://{0}/Raw/{1}", Application.dataPath, path);
#endif
        return filePath;
    }

    public static void WaveToMP3(string waveFileName, string mp3FileName, LAMEPreset bitRate = LAMEPreset.ABR_128)
    {
        using (var reader = new WaveFileReader(waveFileName))
        using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, bitRate))
            reader.CopyTo(writer);
    }

    private IEnumerator PlayMp3(string path)
    {
        if (File.Exists(path))
        {
            var www = new WWW("file:///" + path);
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                var audioSource = GetComponent<AudioSource>();
                audioSource.clip = www.GetAudioClip();
                audioSource.Play();
            }
        }
    }
}
