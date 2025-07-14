using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Lame;
using System.IO;
using System;
using UmaMusumeAudio;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;

public class AudioExporter
{

    public static void ExportAudio(List<UmaWaveStream> a, string path) {
        
        var l = new List<ISampleProvider>();
        foreach (var z in a)
        {
            var blah = z.ToSampleProvider();
            l.Add(blah);
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
            while ((bytesRead = sourceProvider.Read(buffer, 0, buffer.Length)) > 0)
            {
                writer.Write(buffer, 0, bytesRead);
            }
        }
    }

}
