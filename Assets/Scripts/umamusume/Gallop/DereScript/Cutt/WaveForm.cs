using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Cutt
{
    internal class WaveForm
    {
        private const string kWavFolder = "//cygames-fas02/100_projects/083_stage/31_sound/05_SONG/マスター音源/オフセット調整済";

        private const float kAdjust = 10f;

        private FmtInfo _fmtInfo = new FmtInfo();

        private DataInfo _dataInfo = new DataInfo();

        private Dictionary<int, List<Vector3>> _pointsMap = new Dictionary<int, List<Vector3>>();

        private bool _isLoaded;

        public bool IsLoaded
        {
            get
            {
                return _isLoaded;
            }
        }

        public void LoadWav(string SongCueName)
        {
            if (!string.IsNullOrEmpty(SongCueName))
            {
                string path = Path.Combine("//cygames-fas02/100_projects/083_stage/31_sound/05_SONG/マスター音源/オフセット調整済", SongCueName + ".wav");
                if (File.Exists(path))
                {
                    FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    Encoding encoding = Encoding.GetEncoding("Shift_JIS");
                    byte[] array = new byte[4];
                    fileStream.Read(array, 0, 4);
                    fileStream.Read(array, 0, 4);
                    fileStream.Read(array, 0, 4);
                    bool flag = true;
                    while (flag)
                    {
                        fileStream.Read(array, 0, 4);
                        switch (encoding.GetString(array))
                        {
                        case "fmt ":
                            fileStream.Read(array, 0, 4);
                            _fmtInfo.headFMTsize = BitConverter.ToInt32(array, 0);
                            fileStream.Read(array, 0, 2);
                            _fmtInfo.formatID = BitConverter.ToInt32(array, 0);
                            fileStream.Read(array, 0, 2);
                            _fmtInfo.ch = (int)BitConverter.ToUInt32(array, 0);
                            fileStream.Read(array, 0, 4);
                            _fmtInfo.sampleRate = BitConverter.ToInt32(array, 0);
                            fileStream.Read(array, 0, 4);
                            _fmtInfo.dataBytePerSec = BitConverter.ToInt32(array, 0);
                            fileStream.Read(array, 0, 2);
                            _fmtInfo.blockSize = BitConverter.ToInt16(array, 0);
                            fileStream.Read(array, 0, 2);
                            _fmtInfo.bitPerSample = BitConverter.ToUInt16(array, 0);
                            break;
                        case "data":
                        {
                            fileStream.Read(array, 0, 4);
                            _dataInfo.dataSize = BitConverter.ToInt32(array, 0);
                            byte[] array2 = new byte[_dataInfo.dataSize];
                            fileStream.Read(array2, 0, _dataInfo.dataSize);
                            if (_dataInfo.waveBuff != null)
                            {
                                _dataInfo.waveBuff = null;
                            }
                            _dataInfo.waveBuff = new short[GetSampleLength()];
                            int num = 0;
                            for (int i = 0; i < GetSampleLength(); i++)
                            {
                                _dataInfo.waveBuff[i] = (short)(array2[num++] + array2[num++] * 256);
                            }
                            flag = false;
                            break;
                        }
                        }
                    }
                    fileStream.Close();
                    _isLoaded = true;
                }
            }
        }

        public int GetSampleLength()
        {
            if (_dataInfo == null || _fmtInfo == null)
            {
                return 0;
            }
            return _dataInfo.dataSize / _fmtInfo.ch;
        }

        public void CreateWave(Rect area, int fps, int totalFrame, float oneFramePerPixel, int viewFrameCount)
        {
            if (_dataInfo != null && _isLoaded)
            {
                _pointsMap.Clear();
                int num = frameToByte(1, fps);
                int num2 = (int)((float)num / oneFramePerPixel / 10f);
                float num3 = area.y + area.height / 2f;
                for (int i = 0; i < totalFrame; i++)
                {
                    float x = area.x + (float)i * oneFramePerPixel;
                    float num4 = num3;
                    int num5 = frameToByte(i, fps);
                    int num6 = frameToByte(i + 1, fps);
                    List<Vector3> list = new List<Vector3>();
                    if (num6 < _dataInfo.waveBuff.Length)
                    {
                        for (int j = num5; j < num6; j += num2)
                        {
                            float num7 = (float)_dataInfo.waveBuff[j] / 32767f;
                            num4 = num3 + area.height / 2f * num7;
                            list.Add(new Vector3(x, num4));
                        }
                        _pointsMap.Add(i, list);
                    }
                }
            }
        }

        public Vector3[] GetWavePoint(int startFrame, int visibleFrameCount, float oneFramePerPixel, float offsetPixelY)
        {
            if (_pointsMap.Count == 0 || !_isLoaded)
            {
                return null;
            }
            List<Vector3> list = new List<Vector3>();
            float num = oneFramePerPixel * (float)startFrame;
            int num2 = startFrame + visibleFrameCount;
            for (int i = startFrame; i < num2; i++)
            {
                if (_pointsMap.ContainsKey(i))
                {
                    foreach (Vector3 item2 in _pointsMap[i])
                    {
                        Vector3 item = item2;
                        item.x -= num;
                        item.y += offsetPixelY;
                        list.Add(item);
                    }
                }
            }
            return list.ToArray();
        }

        private int frameToByte(int frame, int fps)
        {
            return (int)((float)frame / (float)fps * (float)_fmtInfo.sampleRate * (float)_fmtInfo.ch);
        }
    }
}
