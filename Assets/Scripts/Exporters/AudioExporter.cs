using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Lame;
using System.IO;
using System;
using UmaMusumeAudio;


public class AudioExporter
{

    //public static void ExportAudio
    public static void ExportAudio(List<UmaWaveStream> a, string path) {
        var first = a[0];
        var writer = new LameMP3FileWriter(path, new NAudio.Wave.WZT.WaveFormat(), 128);
        first.CopyTo(writer);      
        
        
        writer.Close();
    }

    public static MemoryStream AudioClipToStream(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("Oopsie! The AudioClip is null.");
            return null;
        }

        // 1. Get the raw sample data from the AudioClip
        // AudioClip.GetData returns samples as floats, typically -1.0 to 1.0
        float[] samples = new float[clip.samples * clip.channels];
        bool result = clip.GetData(samples, 0);
        if (!result)
        {
            Debug.LogError("Fail to GetData");
            return null;
        }

        // 2. Prepare to convert float samples to 16-bit PCM bytes
        // Each 16-bit sample takes 2 bytes
        byte[] bytes = new byte[samples.Length * 2]; // For 16-bit PCM

        int byteIndex = 0;
        foreach (float sample in samples)
        {
            // Convert float to 16-bit signed integer (short)
            // Range for short is -32768 to 32767
            short pcmSample = (short)(sample * 32767f);

            // Convert short to a byte array (2 bytes)
            byte[] pcmBytes = BitConverter.GetBytes(pcmSample);

            // Copy bytes to our main byte array
            bytes[byteIndex++] = pcmBytes[0];
            bytes[byteIndex++] = pcmBytes[1];
        }

        // 3. Wrap the byte array in a MemoryStream
        // Set the position to 0 so it's ready to be read from the beginning
        MemoryStream audioStream = new MemoryStream(bytes);
        audioStream.Position = 0;

        Debug.Log($"Yay! AudioClip '{clip.name}' converted to a MemoryStream with {audioStream.Length} bytes.");
        return audioStream;
    }
    //public static byte[] ConvertWavToMp3(AudioClip clip, int bitRate)
    //{


    //	//var samples = new float[clip.samples * clip.channels];

    //	//clip.GetData(samples, 0);

    //	//Int16[] intData = new Int16[samples.Length];
    //	////converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

    //	//Byte[] bytesData = new Byte[samples.Length * 2];
    //	////bytesData array is twice the size of
    //	////dataSource array because a float converted in Int16 is 2 bytes.

    //	//float rescaleFactor = 32767; //to convert float to Int16

    //	//for (int i = 0; i < samples.Length; i++)
    //	//{
    //	//	intData[i] = (short)(samples[i] * rescaleFactor);
    //	//	Byte[] byteArr = new Byte[2];
    //	//	byteArr = BitConverter.GetBytes(intData[i]);
    //	//	byteArr.CopyTo(bytesData, i * 2);
    //	//}

    //	//var retMs = new MemoryStream();
    //	//var ms = new MemoryStream(SavWav.HEADER_SIZE + bytesData.Length);
    //	//for (int i = 0; i < SavWav.HEADER_SIZE; i++)
    //	//	ms.WriteByte(new byte());
    //	//ms.Write(bytesData, 0, bytesData.Length);
    //	//SavWav.WriteHeader(ms, clip);

    //	//var rdr = new WaveFileReader(ms);
    //	//var wtr = new LameMP3FileWriter(retMs, rdr.WaveFormat, bitRate);
    //	//rdr.CopyTo(wtr);

    //	return retMs.ToArray();
    //}
}
