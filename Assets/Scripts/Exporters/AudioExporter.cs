using System.Collections;
using System.Collections.Generic;
using NAudio.Lame;
using UmaMusumeAudio;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;

public class AudioExporter
{
    public static IEnumerator ExportAudio(List<UmaWaveStream> a, string path, int index = -1) {

        var l = new List<ISampleProvider>();
        if (index == -1)
        {
            foreach (var z in a)
            {
                l.Add(GetSampleFixedLength(z));
            }
        } else
        {
            l.Add(GetSampleFixedLength(a[index]));
        }

        var wf = a[0].WaveFormat;

        var mixer = new MixingSampleProvider(l);
        IWaveProvider sourceProvider = new SampleToWaveProvider16(mixer);


        using (var writer = new LameMP3FileWriter(path, new NAudio.Wave.WZT.WaveFormat(wf.SampleRate, wf.Channels), 320))
        {
            byte[] buffer = new byte[wf.AverageBytesPerSecond]; // Read about 1 second of audio at a time
            int bytesRead;

            // Loop to read from the source IWaveProvider and write to the MP3 writer
            // The Read method returns 0 when there's no more data
            int safeguard = 0;
            while ((bytesRead = sourceProvider.Read(buffer, 0, buffer.Length)) > 0)
            {
                writer.Write(buffer, 0, bytesRead);
                if(++safeguard % 100 == 0)
                {
                    if(safeguard % 500 == 0)
                    {
                        UmaViewerUI.Instance.ShowMessage("Possible loop detected! Close UmaViewer if this message continues appearing", UIMessageType.Warning);
                    }
                    yield return 0;
                }
            }
            UmaViewerUI.Instance.ShowMessage("Export complete", UIMessageType.Default);
        }
    }

    private static ISampleProvider GetSampleFixedLength(UmaWaveStream sample)
    {
        var blah = sample.ToSampleProvider().Take(sample.TotalTime);
        return blah;
    }
}
