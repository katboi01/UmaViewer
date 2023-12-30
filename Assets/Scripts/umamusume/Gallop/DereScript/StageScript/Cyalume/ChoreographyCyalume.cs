using Stage;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChoreographyCyalume
{
    public enum ChoreographyType
    {
        Oioi = 0,
        Uhoi = 1,
        Bye = 2,
        Fufu = 3,
        PPPH = 4,
        Stop = 5,
        Pause = 6,
        None = 7,
        CommandStart = 5
    }

    public enum ColorPattern
    {
        All1,
        Random2,
        Random3,
        Random4,
        Random5,
        Line2,
        Line4,
        Div2,
        Div3,
        Div4,
        Div5,
        SetMobColor,
        Max
    }

    private enum CsvLabel
    {
        StartTime,
        ChoreographyType,
        BPM,
        ColorPattern,
        Color1,
        Color2,
        Color3,
        Color4,
        Color5,
        Width1,
        Width2,
        Width3,
        Width4,
        Width5,
        ChoreographyType3D,
        BPM3D,
        StartImage
    }

    public enum ColorDataType
    {
        Mesh,
        Texture
    }

    public class ChoreographyCyalumePattern
    {
        public ChoreographyType _choreographyType;

        public ColorPattern _colorPattern;

        public int _colorCount;

        public MasterCyalumeColorData.CyalumeColorData[] _colorData = new MasterCyalumeColorData.CyalumeColorData[5];

        public float[] _colorWidth = new float[5];

        public bool IsTypeChoreography => _choreographyType < ChoreographyType.Stop;
    }

    public class ChoreographyCyalumeData : ChoreographyCyalumePattern
    {
        public int _dataNo;

        public int _patternID;

        public float _startTime;

        public float _playSpeed;

        public float _playSpeed3D;

        public int _startFrame;

        public float _playBpm => _playSpeed * 180f;
    }

    public const int MAX_COLOR_COUNT = 5;

    public const int MAX_CHOREOGRAPHY_PATTERN = 5;

    public const int MAX_MESH_PATTERN = 10;

    public const float BASE_BPM = 180f;

    public const float BASE_BPM_FRAME = 0.333333343f;

    public const float BASE_BPM_ANIMETION_FRAME = 40f;

    public const float INVALID_BPM_3D = -1f;

    public const float DEFAULT_BPM_3D = 1f;

    private static readonly Type _typeofChoreographyType = typeof(ChoreographyType);

    private static readonly Type _typeofColorPattern = typeof(ColorPattern);

    private static readonly int[] _colorCountTable = new int[12]
    {
        1, 2, 3, 4, 5, 2, 4, 2, 3, 4,
        5, 1
    };

    private static readonly Type _typeofChoreographyType3D = typeof(ChoreographyType);

    private static readonly Type _typeChoreographyTypeStartFrame = typeof(ChoreographyType);

    private MasterCyalumeColorData _masterCyalumeColorData;

    private ChoreographyCyalumeData[] _choreographyCyalumeDataes;

    private ChoreographyCyalumePattern[] _choreographyPatterns;

    private ColorDataType _colorDataType;

    public ChoreographyCyalumePattern[] choreographyPatterns => _choreographyPatterns;

    public ColorDataType colorDataType => _colorDataType;

    public int GetDataNum()
    {
        if (_choreographyCyalumeDataes != null)
        {
            return _choreographyCyalumeDataes.Length;
        }
        return 0;
    }

    public static float BpmToPlaySpeed(float bpm)
    {
        return bpm / 180f;
    }

    public static float PlaySpeedToAnimationTime(float speed)
    {
        return 40f / (speed * 60f);
    }

    public ChoreographyCyalume(MasterCyalumeColorData masterCyalumeColorData)
    {
        _masterCyalumeColorData = masterCyalumeColorData;
    }

    private void CreateChoreographyPattern()
    {
        if (_choreographyCyalumeDataes == null || _choreographyCyalumeDataes.Length == 0)
        {
            return;
        }
        ColorPattern colorPattern = _choreographyCyalumeDataes[0]._colorPattern;
        if ((uint)(colorPattern - 1) <= 3u)
        {
            _colorDataType = ColorDataType.Texture;
        }
        else
        {
            _colorDataType = ColorDataType.Mesh;
        }
        List<ChoreographyCyalumePattern> list = new List<ChoreographyCyalumePattern>();
        if (_colorDataType == ColorDataType.Texture)
        {
            for (int i = 0; i < _choreographyCyalumeDataes.Length; i++)
            {
                int count = list.Count;
                ChoreographyCyalumePattern choreographyCyalumePattern = _choreographyCyalumeDataes[i];
                if (!choreographyCyalumePattern.IsTypeChoreography || choreographyCyalumePattern._colorPattern == ColorPattern.SetMobColor)
                {
                    continue;
                }
                int j;
                for (j = 0; j < list.Count; j++)
                {
                    ChoreographyCyalumePattern choreographyCyalumePattern2 = list[j];
                    if (choreographyCyalumePattern2._choreographyType == choreographyCyalumePattern._choreographyType && choreographyCyalumePattern2._colorPattern == choreographyCyalumePattern._colorPattern && choreographyCyalumePattern2._colorCount == choreographyCyalumePattern._colorCount)
                    {
                        int k;
                        for (k = 0; k < choreographyCyalumePattern2._colorCount && !(choreographyCyalumePattern2._colorData[k]._inColor != choreographyCyalumePattern._colorData[k]._inColor) && !(choreographyCyalumePattern2._colorData[k]._outColor != choreographyCyalumePattern._colorData[k]._outColor) && choreographyCyalumePattern2._colorWidth[k] == choreographyCyalumePattern._colorWidth[k]; k++)
                        {
                        }
                        if (k == choreographyCyalumePattern2._colorCount)
                        {
                            break;
                        }
                    }
                }
                _choreographyCyalumeDataes[i]._patternID = j;
                if (j == count)
                {
                    list.Add(_choreographyCyalumeDataes[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < _choreographyCyalumeDataes.Length; i++)
            {
                int count = list.Count;
                ChoreographyCyalumePattern choreographyCyalumePattern = _choreographyCyalumeDataes[i];
                if (!choreographyCyalumePattern.IsTypeChoreography || choreographyCyalumePattern._colorPattern == ColorPattern.SetMobColor)
                {
                    continue;
                }
                int j;
                for (j = 0; j < list.Count; j++)
                {
                    ChoreographyCyalumePattern choreographyCyalumePattern2 = list[j];
                    if (choreographyCyalumePattern2._choreographyType == choreographyCyalumePattern._choreographyType && choreographyCyalumePattern2._colorPattern == choreographyCyalumePattern._colorPattern && choreographyCyalumePattern2._colorCount == choreographyCyalumePattern._colorCount)
                    {
                        int k;
                        for (k = 0; k < choreographyCyalumePattern2._colorCount && !(choreographyCyalumePattern2._colorData[k]._inColor != choreographyCyalumePattern._colorData[k]._inColor) && !(choreographyCyalumePattern2._colorData[k]._outColor != choreographyCyalumePattern._colorData[k]._outColor) && choreographyCyalumePattern2._colorWidth[k] == choreographyCyalumePattern._colorWidth[k]; k++)
                        {
                        }
                        if (k == choreographyCyalumePattern2._colorCount)
                        {
                            break;
                        }
                    }
                }
                if (j == count)
                {
                    list.Add(_choreographyCyalumeDataes[i]);
                }
                if (j >= 10)
                {
                    _choreographyCyalumeDataes[i]._patternID = 9;
                }
                else
                {
                    _choreographyCyalumeDataes[i]._patternID = j;
                }
            }
        }
        _choreographyPatterns = list.ToArray();
    }

    public bool LoadFromArrayList(ArrayList records)
    {
        if (records == null)
        {
            return false;
        }
        List<ChoreographyCyalumeData> list = new List<ChoreographyCyalumeData>();
        for (int i = 0; i < records.Count; i++)
        {
            ArrayList arrayList = records[i] as ArrayList;
            string[] array = arrayList.ToArray(typeof(string)) as string[];
            if (array[0].Length == 0 || array[2].Length == 0 || array[3].Length == 0)
            {
                continue;
            }
            ChoreographyCyalumeData choreographyCyalumeData = new ChoreographyCyalumeData();
            choreographyCyalumeData._dataNo = i;
            choreographyCyalumeData._startTime = float.Parse(array[0]);
            choreographyCyalumeData._playSpeed = BpmToPlaySpeed(float.Parse(array[2]));
            choreographyCyalumeData._choreographyType = (ChoreographyType)Enum.Parse(_typeofChoreographyType, array[1]);
            if (choreographyCyalumeData.IsTypeChoreography)
            {
                if (array[4].Length == 0)
                {
                    continue;
                }
                choreographyCyalumeData._colorPattern = (ColorPattern)Enum.Parse(_typeofColorPattern, array[3]);
                choreographyCyalumeData._colorCount = _colorCountTable[(int)choreographyCyalumeData._colorPattern];
                for (int j = 0; j < 5; j++)
                {
                    if (array[4 + j].Length == 0)
                    {
                        choreographyCyalumeData._colorData[j] = MasterCyalumeColorData._defaultColor;
                    }
                    else
                    {
                        int hashCode = array[4 + j].GetHashCode();
                        if (_masterCyalumeColorData.dictionary.TryGetValue(hashCode, out var value))
                        {
                            choreographyCyalumeData._colorData[j] = value;
                        }
                        else
                        {
                            choreographyCyalumeData._colorData[j] = MasterCyalumeColorData._defaultColor;
                        }
                    }
                    if (array[9 + j].Length == 0)
                    {
                        choreographyCyalumeData._colorWidth[j] = 0f;
                    }
                    else
                    {
                        choreographyCyalumeData._colorWidth[j] = float.Parse(array[9 + j]);
                    }
                }
            }
            if (15 < arrayList.Count)
            {
                if (array[15] != "")
                {
                    choreographyCyalumeData._playSpeed3D = BpmToPlaySpeed(float.Parse(array[15]));
                }
                else
                {
                    choreographyCyalumeData._playSpeed3D = -1f;
                }
            }
            else
            {
                choreographyCyalumeData._playSpeed3D = 1f;
            }
            choreographyCyalumeData._choreographyType = ChoreographyType.Oioi;
            if (14 < arrayList.Count)
            {
                if (array[14] != "")
                {
                    choreographyCyalumeData._choreographyType = (ChoreographyType)Enum.Parse(_typeofChoreographyType3D, array[14]);
                }
                else
                {
                    choreographyCyalumeData._choreographyType = ChoreographyType.None;
                }
            }
            if (16 < arrayList.Count)
            {
                choreographyCyalumeData._startFrame = (int)Enum.Parse(_typeChoreographyTypeStartFrame, array[16]);
            }
            else
            {
                choreographyCyalumeData._startFrame = 0;
            }
            list.Add(choreographyCyalumeData);
        }
        if (list.Count > 0)
        {
            _choreographyCyalumeDataes = list.ToArray();
        }
        CreateChoreographyPattern();
        return true;
    }

    public bool Load(string strPath)
    {
        /* DereViewerÇ≈ÇÕmusicDirectorÇ…CyalumeArrayÇämï€ÇµÇƒÇ¢ÇÈÇΩÇﬂÅAÇªÇ±Ç©ÇÁéÊìæÇ∑ÇÈ
         * 
        ResourcesManager instance = SingletonMonoBehaviour<ResourcesManager>.instance;
        if (instance == null)
        {
            return false;
        }
        if (_masterCyalumeColorData == null)
        {
            return false;
        }
        string text = null;
        text = instance.LoadGenericCSV(strPath);
        if (text == null)
        {
            return false;
        }
        ArrayList arrayList = null;
        arrayList = Utility.ConvertCSV(text);
        */
        if (ViewLauncher.instance != null && ViewLauncher.instance.liveDirector != null)
        {
            ArrayList arrayList = ViewLauncher.instance.liveDirector.MusicScoreCyalumeArray;
            return LoadFromArrayList(arrayList);
        }
        return false;
    }

    public ChoreographyCyalumeData getChoreographyDataFromTime(float time)
    {
        if (_choreographyCyalumeDataes == null)
        {
            return null;
        }
        ChoreographyCyalumeData result = null;
        for (int num = _choreographyCyalumeDataes.Length - 1; num >= 0; num--)
        {
            if (_choreographyCyalumeDataes[num] != null && time >= _choreographyCyalumeDataes[num]._startTime)
            {
                result = _choreographyCyalumeDataes[num];
                break;
            }
        }
        return result;
    }

    public ChoreographyCyalumeData getChoreographyDataFromNo(int no)
    {
        if (_choreographyCyalumeDataes == null)
        {
            return null;
        }
        if (no < 0)
        {
            return null;
        }
        if (_choreographyCyalumeDataes.Length <= no)
        {
            return null;
        }
        return _choreographyCyalumeDataes[no];
    }
}
