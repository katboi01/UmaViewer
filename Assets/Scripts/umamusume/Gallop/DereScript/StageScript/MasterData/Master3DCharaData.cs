using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3d_chara_data.unity3dの3D/Live/3d_chara_dataを読み込んで生成する
/// 3D/Live/3d_chara_dataはcsv形式のためArrayList形式で入力する
/// </summary>
public class Master3DCharaData
{
    public struct BoundsBoxSizeData
    {
        public float headWidth;

        public float bodyWidth;

        public BoundsBoxSizeData(float head_width, float body_width)
        {
            headWidth = head_width;
            bodyWidth = body_width;
        }
    }

    private class CharaDataBoundsBoxSize
    {
        public Dictionary<int, BoundsBoxSizeData> boundsBoxSizeDictionary = new Dictionary<int, BoundsBoxSizeData>();
    }

    public class CharaDataSpecialFace
    {
        public enum eCheek
        {
            Default,
            Off,
            On
        }

        public Dictionary<int, int[]> srcFaceDictionary = new Dictionary<int, int[]>();

        public Dictionary<int, eCheek[]> srcCheekDictionary = new Dictionary<int, eCheek[]>();
    }

    public class CharaDataWork
    {
        public Dictionary<int, int[]> cacheSplFaceDictionary;

        public Dictionary<int, CharaDataSpecialFace.eCheek[]> cacheSplCheekDictionary;

        public int cacheFindId;

        public int cacheFaceSlotIdx;

        public int cacheFaceId;

        public bool cacheCheekFlag;

        public CharaDataSpecialFace.eCheek cacheCheek;

        public HashSet<int> cacheCheekUsableDress;

        public bool CheckUsableCheek(int dressId)
        {
            bool result = cacheCheekFlag;
            if (cacheCheekUsableDress != null && cacheCheekUsableDress.Count > 0)
            {
                result = cacheCheekUsableDress.Contains(dressId);
            }
            return result;
        }
    }

    public struct LoadSettingData
    {
        public int subId;

        public bool isCenterOnly;
    }

    private class CharaDataLoadSongFace
    {
        public Dictionary<int, LoadSettingData> loadFaceDictionary = new Dictionary<int, LoadSettingData>();
    }

    public class IBehaviorData
    {
        private HashSet<int> _setExclude = new HashSet<int>();

        public static T ConvToEnum<T>(string str)
        {
            return (T)Enum.Parse(typeof(T), str, ignoreCase: true);
        }

        protected void SetExcludeList(string strExclude)
        {
            _setExclude.Clear();
            if (!string.IsNullOrEmpty(strExclude))
            {
                string[] array = strExclude.Split('.');
                for (int i = 0; i < array.Length; i++)
                {
                    _setExclude.Add(int.Parse(array[i]));
                }
            }
        }

        public bool IsExclude(int cardID)
        {
            return _setExclude.Contains(cardID);
        }
    }

    public class GravityControlData : IBehaviorData
    {
        public class Record
        {
            private int _groupId;

            private eType _type;

            private HashSet<int> _setParams = new HashSet<int>();

            private Vector3 _gravityDirection = Vector3.down;

            public int groupdId
            {
                get
                {
                    return _groupId;
                }
                set
                {
                    _groupId = value;
                }
            }

            public eType type
            {
                get
                {
                    return _type;
                }
                set
                {
                    _type = value;
                }
            }

            public Vector3 gravityDirection
            {
                get
                {
                    return _gravityDirection;
                }
                set
                {
                    _gravityDirection = value;
                }
            }

            public void ParseParams(string[] strParams)
            {
                string[] array = strParams[5].Split('.');
                for (int i = 0; i < array.Length; i++)
                {
                    int item = int.Parse(array[i]);
                    if (!_setParams.Contains(item))
                    {
                        _setParams.Add(item);
                    }
                }
                _gravityDirection.x = (float)double.Parse(strParams[6]);
                _gravityDirection.y = (float)double.Parse(strParams[7]);
                _gravityDirection.z = (float)double.Parse(strParams[8]);
                _gravityDirection.Normalize();
            }

            public bool CheckContains(int id)
            {
                return _setParams.Contains(id);
            }
        }

        public enum eType
        {
            None = 0,
            Facial = 1,
            Eye = 2,
            INVALID = 3,
            MAX = 3
        }

        private Dictionary<int, List<Record>> _mapRecords = new Dictionary<int, List<Record>>();

        public List<Record> GetRecordList(eType type)
        {
            List<Record> value = null;
            _mapRecords.TryGetValue((int)type, out value);
            return value;
        }

        public bool Parse(string[] strParams)
        {
            int groupdId = int.Parse(strParams[3]);
            eType eType = IBehaviorData.ConvToEnum<eType>(strParams[4]);
            int key = (int)eType;
            List<Record> value = null;
            if (!_mapRecords.TryGetValue(key, out value))
            {
                value = new List<Record>();
                _mapRecords.Add(key, value);
            }
            Record record = new Record
            {
                type = eType,
                groupdId = groupdId
            };
            record.ParseParams(strParams);
            value.Add(record);
            return true;
        }
    }

    public class FacialOverrideData : IBehaviorData
    {
        private Dictionary<int, int> _mapOverride = new Dictionary<int, int>();

        public bool Parse(string[] strParams)
        {
            _mapOverride.Add(int.Parse(strParams[3]), int.Parse(strParams[4]));
            return true;
        }

        public int Override(int facialId)
        {
            int value = facialId;
            if (!_mapOverride.TryGetValue(facialId, out value))
            {
                value = facialId;
            }
            return value;
        }
    }

    public class AvertOnesEyeData : IBehaviorData
    {
        private HashSet<int> _facialFlagList = new HashSet<int>();

        public float DistEpsilon { get; private set; }

        public float DegEpsilon { get; private set; }

        public AvertOnesEyeData()
        {
            DistEpsilon = 0f;
            DegEpsilon = 0f;
        }

        public bool Parse(string[] strParams)
        {
            DistEpsilon = float.Parse(strParams[3]);
            DegEpsilon = float.Parse(strParams[4]);
            SetExcludeList(strParams[5]);
            SetFacialFlag(strParams[6]);
            return true;
        }

        public static bool IsSame(AvertOnesEyeData a, AvertOnesEyeData b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.DistEpsilon != b.DistEpsilon || a.DegEpsilon != b.DegEpsilon)
            {
                return false;
            }
            return true;
        }

        private void SetFacialFlag(string strFacialFlag)
        {
            _facialFlagList.Clear();
            if (!string.IsNullOrEmpty(strFacialFlag))
            {
                string[] array = strFacialFlag.Split('.');
                for (int i = 0; i < array.Length; i++)
                {
                    _facialFlagList.Add(int.Parse(array[i]));
                }
            }
        }

        public bool IsApplyedFacialFlag(int facialFlag, out bool isFacialFlag)
        {
            if (_facialFlagList.Count == 0)
            {
                isFacialFlag = false;
                return true;
            }
            isFacialFlag = true;
            return _facialFlagList.Contains(facialFlag);
        }
    }

    public class LookDownOnData : IBehaviorData
    {
        public float HeadAngle { get; private set; }

        public float NeckAngle { get; private set; }

        public float ForwardLimit { get; private set; }

        public float BackwardLimit { get; private set; }

        public float ForwardUnit { get; private set; }

        public float BackwardUnit { get; private set; }

        public LookDownOnData()
        {
            HeadAngle = 0f;
            NeckAngle = 0f;
            ForwardLimit = 0f;
            BackwardLimit = 0f;
            ForwardUnit = 0f;
            BackwardUnit = 0f;
        }

        private float GetUnitValue(float fValue)
        {
            if (fValue != 0f)
            {
                return 1f / fValue;
            }
            return 0f;
        }

        public bool Parse(string[] strParams)
        {
            HeadAngle = float.Parse(strParams[3]);
            NeckAngle = float.Parse(strParams[4]);
            ForwardLimit = float.Parse(strParams[5]);
            BackwardLimit = float.Parse(strParams[6]);
            ForwardUnit = GetUnitValue(ForwardLimit);
            BackwardUnit = GetUnitValue(BackwardLimit);
            SetExcludeList(strParams[7]);
            return true;
        }

        public static bool IsSame(LookDownOnData a, LookDownOnData b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.HeadAngle != b.HeadAngle || a.NeckAngle != b.NeckAngle || a.ForwardLimit != b.ForwardLimit || a.BackwardLimit != b.BackwardLimit || a.ForwardUnit != b.ForwardUnit || a.BackwardUnit != b.BackwardUnit)
            {
                return false;
            }
            return true;
        }
    }

    public class Behavior
    {
        public enum eType
        {
            None,
            AvertOnesEye,
            LookDownOn,
            FacialOverride,
            GravityControl
        }

        public AvertOnesEyeData AvertOnesEye { get; protected set; }

        public LookDownOnData LookDownOn { get; protected set; }

        public FacialOverrideData FacialOverride { get; protected set; }

        public GravityControlData GravityControl { get; protected set; }

        public Behavior()
        {
            AvertOnesEye = null;
            LookDownOn = null;
            FacialOverride = null;
            GravityControl = null;
        }

        public bool Parse(string[] strParams)
        {
            bool result = false;
            switch ((eType)Enum.Parse(typeof(eType), strParams[2], ignoreCase: true))
            {
                case eType.AvertOnesEye:
                    AvertOnesEye = new AvertOnesEyeData();
                    result = AvertOnesEye.Parse(strParams);
                    break;
                case eType.LookDownOn:
                    LookDownOn = new LookDownOnData();
                    result = LookDownOn.Parse(strParams);
                    break;
                case eType.FacialOverride:
                    if (FacialOverride == null)
                    {
                        FacialOverride = new FacialOverrideData();
                    }
                    result = FacialOverride.Parse(strParams);
                    break;
                case eType.GravityControl:
                    if (GravityControl == null)
                    {
                        GravityControl = new GravityControlData();
                    }
                    result = GravityControl.Parse(strParams);
                    break;
            }
            return result;
        }

        protected T CheckExcludeCardID<T>(T behaviorData, int cardID) where T : IBehaviorData
        {
            if (behaviorData == null || !behaviorData.IsExclude(cardID))
            {
                return behaviorData;
            }
            return null;
        }

        public Behavior ShallowCopy(int cardID)
        {
            return new Behavior
            {
                AvertOnesEye = AvertOnesEye,
                LookDownOn = CheckExcludeCardID(LookDownOn, cardID),
                FacialOverride = CheckExcludeCardID(FacialOverride, cardID),
                GravityControl = CheckExcludeCardID(GravityControl, cardID)
            };
        }
    }

    public class HeadSelector
    {
        public enum ePriority
        {
            None,
            LoadSongFace
        }

        public enum eCondition
        {
            None,
            DressGradeDown
        }

        private class Record
        {
            public int subId { get; set; }

            public eCondition condition { get; set; }

            public ePriority priority { get; set; }
        }

        private Dictionary<int, Record> _mapRecord = new Dictionary<int, Record>();

        public int GetSubId(Character3DBase.CharacterData charaData, out ePriority priority)
        {
            int result = 0;
            int key = charaData.originDressId;
            priority = ePriority.None;
            Record value = null;
            if (_mapRecord.TryGetValue(key, out value))
            {
                priority = value.priority;
                if (value.condition == eCondition.DressGradeDown)
                {
                    //モデルをグレードダウンする時だけsubモデルを使用する
                    if (charaData.isDressGradeDown)
                    {
                        result = value.subId;
                    }
                }
                else
                {
                    result = value.subId;
                }
            }
            return result;
        }

        public void Parse(string[] column)
        {
            try
            {
                int key = int.Parse(column[2]);
                int subId = int.Parse(column[3]);
                string value = column[4];
                string value2 = column[5];
                Record value3 = new Record
                {
                    subId = subId,
                    condition = (eCondition)Enum.Parse(typeof(eCondition), value, ignoreCase: true),
                    priority = (ePriority)Enum.Parse(typeof(ePriority), value2, ignoreCase: true)
                };
                if (!_mapRecord.ContainsKey(key))
                {
                    _mapRecord.Add(key, value3);
                }
            }
            catch (Exception)
            {
            }
        }
    }

    private enum DataType : byte
    {
        Cheek,
        DefaultFace,
        SpecialFace,
        LoadSongFace,
        BoundsBoxSize,
        Behavior,
        RichBodyTexture,
        HeadSelector,
        Max
    }

    public static readonly BoundsBoxSizeData defaultBoundsBoxSizeData = new BoundsBoxSizeData
    {
        headWidth = 1f,
        bodyWidth = 1f
    };

    public const int MAX_FACE_ID_COUNT = 6;

    private const int SONG_FACE_ID_BASE = 10000;

    private const int SPECIAL_FACE_SLOT_OFFSET = 3;

    private Dictionary<int, object>[] _charaDataDictionarys = new Dictionary<int, object>[8];

    public const int FIND_ID_OFFSET = 10000;

    public void GetBoundsBoxSizeData(out BoundsBoxSizeData outBoundsBoxSizeData, int charId, int dressId, int subId)
    {
        outBoundsBoxSizeData = defaultBoundsBoxSizeData;
        if (_charaDataDictionarys[4].TryGetValue(charId, out var value))
        {
            CharaDataBoundsBoxSize obj = value as CharaDataBoundsBoxSize;
            int key = dressId * 1000 + subId;
            if (obj.boundsBoxSizeDictionary.TryGetValue(key, out var value2))
            {
                outBoundsBoxSizeData = value2;
            }
        }
    }

    public CharaDataWork GetCharaData(int chara_id)
    {
        CharaDataWork charaDataWork = new CharaDataWork();
        object value = null;
        if (_charaDataDictionarys[0].TryGetValue(chara_id, out value))
        {
            charaDataWork.cacheCheekFlag = true;
            charaDataWork.cacheCheekUsableDress = value as HashSet<int>;
        }
        else
        {
            charaDataWork.cacheCheekFlag = false;
            charaDataWork.cacheCheekUsableDress = null;
        }
        if (_charaDataDictionarys[2].TryGetValue(chara_id, out var value2))
        {
            CharaDataSpecialFace charaDataSpecialFace = value2 as CharaDataSpecialFace;
            charaDataWork.cacheSplFaceDictionary = charaDataSpecialFace.srcFaceDictionary;
            charaDataWork.cacheSplCheekDictionary = charaDataSpecialFace.srcCheekDictionary;
        }
        else
        {
            charaDataWork.cacheSplFaceDictionary = null;
            charaDataWork.cacheSplCheekDictionary = null;
        }
        return charaDataWork;
    }

    public bool GetLoadCharaSongSetting(ref LoadSettingData loadSettingData, int charaId, int dressId, int songId, bool isCenter)
    {
        if (!_charaDataDictionarys[3].TryGetValue(songId, out var value))
        {
            return false;
        }
        CharaDataLoadSongFace obj = value as CharaDataLoadSongFace;
        int key = charaId * 10000 + dressId;
        if (!obj.loadFaceDictionary.TryGetValue(key, out loadSettingData))
        {
            return false;
        }
        if (loadSettingData.isCenterOnly)
        {
            return isCenter;
        }
        return true;
    }

    public bool GetBehavior(int chrID, int cardID, out Behavior behavior)
    {
        behavior = null;
        bool result = false;
        object value = null;
        if (_charaDataDictionarys[5].TryGetValue(chrID, out value))
        {
            Behavior behavior2 = value as Behavior;
            if (behavior2 != null)
            {
                behavior = behavior2.ShallowCopy(cardID);
                result = behavior != null;
            }
        }
        return result;
    }

    public HeadSelector GetHeadSelector(int charaId)
    {
        HeadSelector result = null;
        object value = null;
        if (_charaDataDictionarys[7].TryGetValue(charaId, out value))
        {
            result = value as HeadSelector;
        }
        return result;
    }

    public Master3DCharaData(ArrayList records)
    {
        Clear();
        for (int i = 0; i < records.Count; i++)
        {
            ArrayList arrayList = records[i] as ArrayList;
            if (arrayList.Count == 0)
            {
                continue;
            }
            string[] column = arrayList.ToArray(typeof(string)) as string[];
            object value = null;
            if (column[0] == "Cheek")
            {
                if (column.Length < 2)
                {
                    continue;
                }
                int key = int.Parse(column[1]);
                HashSet<int> hashSet = null;
                if (!string.IsNullOrEmpty(column[2]))
                {
                    string[] array = column[2].Split('.');
                    if (array != null && array.Length != 0)
                    {
                        hashSet = new HashSet<int>();
                        int num = array.Length;
                        try
                        {
                            for (int j = 0; j < num; j++)
                            {
                                int item = int.Parse(array[j]);
                                if (!hashSet.Contains(item))
                                {
                                    hashSet.Add(item);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                Dictionary<int, object> dictionary = _charaDataDictionarys[0];
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, null);
                }
                if (hashSet != null && hashSet.Count > 0)
                {
                    dictionary[key] = hashSet;
                }
            }
            else if (column[0] == "DefaultFace")
            {
                if (column.Length >= 3)
                {
                    int key2 = int.Parse(column[1]);
                    int num2 = int.Parse(column[2]);
                    Dictionary<int, object> dictionary = _charaDataDictionarys[1];
                    if (!dictionary.ContainsKey(key2))
                    {
                        object value2 = num2;
                        dictionary.Add(key2, value2);
                    }
                }
            }
            else if (column[0] == "SpecialFace")
            {
                if (column.Length < 4)
                {
                    continue;
                }
                CharaDataSpecialFace charaDataSpecialFace = null;
                int key3 = int.Parse(column[1]);
                int key4 = int.Parse(column[2]);
                int[] arrFaceID = new int[6];
                CharaDataSpecialFace.eCheek[] arrCheek = new CharaDataSpecialFace.eCheek[6];
                Action<int, int> action = delegate (int idx, int col)
                {
                    if (string.IsNullOrEmpty(column[col]))
                    {
                        arrFaceID[idx] = 0;
                        arrCheek[idx] = CharaDataSpecialFace.eCheek.Default;
                    }
                    else
                    {
                        string[] array2 = column[col].Split('.');
                        int num7 = array2.Length;
                        if (num7 > 0)
                        {
                            arrFaceID[idx] = int.Parse(array2[0]);
                            arrCheek[idx] = CharaDataSpecialFace.eCheek.Default;
                            if (num7 > 1)
                            {
                                string strA = array2[1].ToLower();
                                if (string.Compare(strA, "cheek_off") == 0)
                                {
                                    arrCheek[idx] = CharaDataSpecialFace.eCheek.Off;
                                }
                                if (string.Compare(strA, "cheek_on") == 0)
                                {
                                    arrCheek[idx] = CharaDataSpecialFace.eCheek.On;
                                }
                            }
                        }
                        else
                        {
                            arrFaceID[idx] = 0;
                            arrCheek[idx] = CharaDataSpecialFace.eCheek.Default;
                        }
                    }
                };
                for (int k = 0; k < 6; k++)
                {
                    action(k, k + 3);
                }
                Dictionary<int, object> dictionary = _charaDataDictionarys[2];
                if (dictionary.TryGetValue(key3, out value))
                {
                    charaDataSpecialFace = value as CharaDataSpecialFace;
                }
                else
                {
                    charaDataSpecialFace = new CharaDataSpecialFace();
                    dictionary.Add(key3, charaDataSpecialFace);
                }
                if (!charaDataSpecialFace.srcFaceDictionary.ContainsKey(key4))
                {
                    charaDataSpecialFace.srcFaceDictionary.Add(key4, arrFaceID);
                }
                if (!charaDataSpecialFace.srcCheekDictionary.ContainsKey(key4))
                {
                    charaDataSpecialFace.srcCheekDictionary.Add(key4, arrCheek);
                }
            }
            else if (column[0] == "LoadSongFace")
            {
                if (column.Length >= 6)
                {
                    int key5 = int.Parse(column[1]);
                    int num3 = int.Parse(column[2]);
                    int num4 = int.Parse(column[3]);
                    int subId = int.Parse(column[4]);
                    bool isCenterOnly = int.Parse(column[5]) != 0;
                    int key6 = num3 * 10000 + num4;
                    Dictionary<int, object> dictionary = _charaDataDictionarys[3];
                    CharaDataLoadSongFace charaDataLoadSongFace;
                    if (dictionary.TryGetValue(key5, out value))
                    {
                        charaDataLoadSongFace = value as CharaDataLoadSongFace;
                    }
                    else
                    {
                        charaDataLoadSongFace = new CharaDataLoadSongFace();
                        dictionary.Add(key5, charaDataLoadSongFace);
                    }
                    if (!charaDataLoadSongFace.loadFaceDictionary.ContainsKey(key6))
                    {
                        LoadSettingData value3 = new LoadSettingData
                        {
                            isCenterOnly = isCenterOnly,
                            subId = subId
                        };
                        charaDataLoadSongFace.loadFaceDictionary.Add(key6, value3);
                    }
                }
            }
            else if (column[0] == "BoundsBoxSize")
            {
                if (column.Length >= 6)
                {
                    int key7 = int.Parse(column[1]);
                    int num5 = int.Parse(column[2]);
                    int num6 = int.Parse(column[3]);
                    float headWidth = float.Parse(column[4]);
                    float bodyWidth = float.Parse(column[5]);
                    Dictionary<int, object> dictionary = _charaDataDictionarys[4];
                    CharaDataBoundsBoxSize charaDataBoundsBoxSize;
                    if (dictionary.TryGetValue(key7, out value))
                    {
                        charaDataBoundsBoxSize = value as CharaDataBoundsBoxSize;
                    }
                    else
                    {
                        charaDataBoundsBoxSize = new CharaDataBoundsBoxSize();
                        dictionary.Add(key7, charaDataBoundsBoxSize);
                    }
                    int key8 = num5 * 1000 + num6;
                    if (!charaDataBoundsBoxSize.boundsBoxSizeDictionary.ContainsKey(key8))
                    {
                        BoundsBoxSizeData value4 = new BoundsBoxSizeData
                        {
                            headWidth = headWidth,
                            bodyWidth = bodyWidth
                        };
                        charaDataBoundsBoxSize.boundsBoxSizeDictionary.Add(key8, value4);
                    }
                }
            }
            else if (column[0] == "Behavior")
            {
                Dictionary<int, object> dictionary = _charaDataDictionarys[5];
                int key9 = int.Parse(column[1]);
                Behavior behavior = null;
                if (dictionary.TryGetValue(key9, out value))
                {
                    behavior = value as Behavior;
                }
                else
                {
                    behavior = new Behavior();
                    dictionary.Add(key9, behavior);
                }
                behavior?.Parse(column);
            }
            else if (column[0] == "HeadSelector")
            {
                Dictionary<int, object> dictionary = _charaDataDictionarys[7];
                int key10 = int.Parse(column[1]);
                HeadSelector headSelector = null;
                if (dictionary.TryGetValue(key10, out value))
                {
                    headSelector = value as HeadSelector;
                }
                else
                {
                    headSelector = new HeadSelector();
                    dictionary.Add(key10, headSelector);
                }
                headSelector?.Parse(column);
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < 8; i++)
        {
            if (_charaDataDictionarys[i] != null)
            {
                _charaDataDictionarys[i].Clear();
            }
            else
            {
                _charaDataDictionarys[i] = new Dictionary<int, object>();
            }
        }
    }

    public int GetDefaultFaceID(int find_id)
    {
        if (_charaDataDictionarys[1].TryGetValue(find_id, out var value))
        {
            return (int)value;
        }
        return 0;
    }
}
