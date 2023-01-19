using System;
using System.IO;
using System.Threading;
using DereTore.Exchange.Audio.HCA;
using DereTore.Exchange.Archive.ACB;

public class DecodeACB
{
    public static class CgssCipher
    {
        /* ミリシタ
        public static readonly uint Key1 = 0xBC731A85;
        public static readonly uint Key2 = 0x0002B875;
        */

        //デレステ
        public static readonly uint Key1 = 0xf27e3b22;
        public static readonly uint Key2 = 0x00003657;
    }

    public delegate void AsyncDecode(byte[] inData, string outstr);

    public static void Decode(string inputfilename, string outputfilename)
    {
        if (!File.Exists(inputfilename)) { return; }
        if (File.Exists(outputfilename)) { return; }

        byte[] inData = File.ReadAllBytes(inputfilename);

        Decode(inData, outputfilename);
    }

    public static void Decode(byte[] inputData, string outputfilename)
    {
        if (inputData != null)
        {
            AsyncDecode asyncDecode = DecodeProc;
            asyncDecode.BeginInvoke(inputData, outputfilename, null, null); //非同期で実行
        }
    }

    private static void DecodeProc(byte[] inputData, string outputfilename)
    {
        //復号Keyをセット
        var decodeParams = DecodeParams.CreateDefault();
        decodeParams.Key1 = CgssCipher.Key1;
        decodeParams.Key2 = CgssCipher.Key2;

        //HCAの出力設定
        var audioParams = AudioParams.CreateDefault();
        audioParams.InfiniteLoop = false;
        audioParams.SimulatedLoopCount = 0; //ループ回数
        audioParams.OutputWaveHeader = true;

        //Streamの生成
        MemoryStream memoryStream = new MemoryStream(inputData);

        //acbファイルを展開
        using (AcbFile acb = AcbFile.FromStream(memoryStream,0,inputData.Length,"default",true))
        {
            var fileNames = acb.GetFileNames();
            var s = fileNames[0];
            byte[] outdata = null;

            //hcaファイルを展開
            using (var source = acb.OpenDataStream(s))
            {
                try
                {
                    using (var hcaStream = new HcaAudioStream(source, decodeParams, audioParams))
                    {
                        //出力
                        outdata = new byte[hcaStream.Length];
                        hcaStream.Read(outdata, 0, (int)hcaStream.Length);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("HCA Decodeでエラーが発生しました。");
                    Console.WriteLine("File : " + s);
                    Console.WriteLine(e);
                    return;
                }
            }
            if (outdata != null)
            {
                string tmpfile = outputfilename + ".tmp";
                File.WriteAllBytes(tmpfile, outdata);
                File.Move(tmpfile, outputfilename);
            }

            Console.WriteLine(s);
        }
    }

}