using System;
using System.Collections.Generic;
using UnityEngine;
using static Gallop.Live.Cutt.LiveTimelineDefine;

namespace Gallop.Live.Cutt
{
    [Flags]
    public enum LiveTimelineKeyAttribute
    {
        Disable = 1,
        CameraDelayEnable = 2,
        CameraDelayInherit = 4,
        KeyCommonBitMask = 32768,
        kAttrCheek = 65536,
        kAttrTeary = 131072,
        kAttrTearful = 262144,
        kAttrTeardrop = 524288,
        kAttrMangame = 2097152,
        kAttrFaceShadow = 4194304,
        kAttrFaceShadowVisible = 8388608,
}

    public enum LiveTimelineKeyDataListAttr
    {
        Disable = 1
    }

    public enum TimelineKeyPlayMode
    {
        Always = 0,
        LightOnly = 1,
        DefaultOver = 2
    }

    public enum LiveCameraInterpolateType
    {
        None = 0,
        Linear = 1,
        Curve = 2,
        Ease = 3
    }

    public enum LiveCameraPositionType
    {
        Direct = 0,
        Character = 1
    }

    [Flags]
    public enum LiveCharaPositionFlag
    {
        Place01 = 1,
        Place02 = 2,
        Place03 = 4,
        Place04 = 8,
        Place05 = 16,
        Place06 = 32,
        Place07 = 64,
        Place08 = 128,
        Place09 = 256,
        Place10 = 512,
        Place11 = 1024,
        Place12 = 2048,
        Place13 = 4096,
        Place14 = 8192,
        Place15 = 16384,
        Place16 = 32768,
        Place17 = 65536,
        Place18 = 131072,
        Center = 1,
        Left = 2,
        Right = 4,
        Side = 6,
        Back = 262136,
        Other = 262142,
        All = 262143
    }

    public enum LiveCameraCharaParts
    {
        Face = 0,
        Waist = 1,
        LeftHandWrist = 2,
        RightHandAttach = 3,
        Chest = 4,
        Foot = 5,
        InitFaceHeight = 6,
        InitWaistHeight = 7,
        InitChestHeight = 8,
        RightHandWrist = 9,
        LeftHandAttach = 10,
        ConstFaceHeight = 11,
        ConstChestHeight = 12,
        ConstWaistHeight = 13,
        ConstFootHeight = 14,
        Position = 15,
        PositionWithoutOffset = 16,
        InitialHeightFace = 17,
        InitialHeightChest = 18,
        InitialHeightWaist = 19,
        Max = 20
    }

    public enum LiveCameraCullingLayer
    {
        None = 0,
        TransparentFX = 1,
        Background3d_NotReflect = 2,
        Background3d = 4,
        Character3d = 8,
        Character3d_0 = 16,
        Character3d_1 = 32,
        Character3d_NotReflect = 64,
        NotLayerDefault = 128,
        NotLayer3d = 256,
        Effect = 512
    }

    public enum LiveCameraBgColorType
    {
        Direct = 0,
        CharacterImageColorMain = 1,
        CharacterImageColorSub = 2,
        CharacterUIColorMain = 3,
        CharacterUIColorSub = 4
    }

    [System.Serializable]
    public class LiveTimelineKeyDataListTemplate<T> : ILiveTimelineKeyDataList where T : LiveTimelineKey
    {
        [SerializeField]
        private LiveTimelineKeyDataListAttr _attribute;

        [SerializeField]
        private TimelineKeyPlayMode _playMode;

        [SerializeField]
        private Color _baseColor;

        [SerializeField]
        private string _description;

        public LiveTimelineKeyIndex thisTimeKeyIndex = new LiveTimelineKeyIndex();

        public LiveTimelineKeyIndex TimeKeyIndex => thisTimeKeyIndex;

        public List<T> thisList = new List<T>();

        private int _lastFindIndex = -1;


        public LiveTimelineKeyDataListAttr attribute
        {
            get
            {
                return _attribute;
            }
            set
            {
                _attribute = value;
            }
        }

        public TimelineKeyPlayMode playMode
        {
            get
            {
                return _playMode;
            }
            set
            {
                _playMode = value;
            }
        }

        public LiveTimelineKey this[int index]
        {
            get
            {
                return thisList[index];
            }
            set
            {
                thisList[index] = value as T;
            }
        }

        public int Count => thisList.Count;

        public int depthCounter => 0;

        //TO DO -> binary search algorithm
        public LiveTimelineKeyIndex FindCurrentKey(float currentTime)
        {
            if (thisList.Count > 0)
            {
                int ret = BinarySearchKey(0, thisList.Count - 1, currentTime);
                if (ret == -1)
                {
                    return null;
                }
                thisTimeKeyIndex.index = ret;
                thisTimeKeyIndex.key = thisList[ret];

                if (ret + 1 != thisList.Count)
                {
                    thisTimeKeyIndex.nextKey = thisList[ret + 1];
                }
                else
                {
                    thisTimeKeyIndex.nextKey = null;
                }
                if (ret - 1 >= 0)
                {
                    thisTimeKeyIndex.prevKey = thisList[ret - 1];
                }
                else
                {
                    thisTimeKeyIndex.prevKey = null;
                }
                return thisTimeKeyIndex;
            }

            return null;
        }

        public int BinarySearchKey(int low, int high, float time)
        {
            float frame = time * 60;
            int mid = (low + high) / 2;

            if (high < 0)
            {
                return -1;
            }
            else if (low == high || (frame >= thisList[mid].frame && frame < thisList[mid + 1].frame))
            {
                return mid;
            }
            else
            {
                if (frame >= thisList[mid].frame)
                {
                    return BinarySearchKey(mid + 1, high, time);
                }
                else
                {
                    return BinarySearchKey(low, mid - 1, time);
                }
            }
        }

        public LiveTimelineKeyIndex FindCurrentKeyLinear(float currentTime)
        {
            for (int i = thisList.Count - 1; i >= 0; i--)
            {
                if (currentTime >= thisList[i].FrameSecond)
                {
                    thisTimeKeyIndex.index = i;
                    thisTimeKeyIndex.key = thisList[i];
                    if (i + 1 != thisList.Count)
                    {
                        thisTimeKeyIndex.nextKey = thisList[i + 1];
                    }
                    else
                    {
                        thisTimeKeyIndex.nextKey = null;
                    }
                    if (i - 1 >= 0)
                    {
                        thisTimeKeyIndex.prevKey = thisList[i - 1];
                    }
                    else
                    {
                        thisTimeKeyIndex.prevKey = null;
                    }
                    return thisTimeKeyIndex;
                }
            }

            return null;
        }

        public LiveTimelineKeyIndex UpdateCurrentKey(float currentTime)
        {
            if (thisTimeKeyIndex.nextKey != null)
            {
                if (currentTime >= thisTimeKeyIndex.nextKey.FrameSecond)
                {
                    thisTimeKeyIndex.index++;
                    thisTimeKeyIndex.key = thisList[thisTimeKeyIndex.index];
                    if (thisTimeKeyIndex.index + 1 != thisList.Count)
                    {
                        thisTimeKeyIndex.nextKey = thisList[thisTimeKeyIndex.index + 1];
                    }
                    else
                    {
                        thisTimeKeyIndex.nextKey = null;
                    }
                    if (thisTimeKeyIndex.index - 1 >= 0)
                    {
                        thisTimeKeyIndex.prevKey = thisList[thisTimeKeyIndex.index - 1];
                    }
                    else
                    {
                        thisTimeKeyIndex.prevKey = null;
                    }
                }
            }

            return thisTimeKeyIndex;
        }

        public bool HasAttribute(LiveTimelineKeyDataListAttr attr)
        {
            return (attribute & attr) == attr;
        }

        public bool EnablePlayModeTimeline(TimelinePlayerMode playerMode)
        {
            return _playMode switch
            {
                TimelineKeyPlayMode.Always => true,
                TimelineKeyPlayMode.LightOnly => playerMode == TimelinePlayerMode.Light,
                TimelineKeyPlayMode.DefaultOver => playerMode != TimelinePlayerMode.Light,
                _ => true,
            };
        }

        public void Insert(int index, LiveTimelineKey item)
        {
            thisList.Insert(index, item as T);
        }

        public void Add(LiveTimelineKey item)
        {
            thisList.Add(item as T);
        }

        public void Clear()
        {
            thisList.Clear();
        }

        public bool Remove(LiveTimelineKey item)
        {
            return thisList.Remove(item as T);
        }

        public void RemoveAt(int index)
        {
            thisList.RemoveAt(index);
        }
        public IEnumerator<LiveTimelineKey> GetEnumerator()
        {
            return ToEnumerable().GetEnumerator();
        }

        public List<LiveTimelineKey> GetRange(int index, int count)
        {
            return thisList.GetRange(index, count).ConvertAll((Converter<T, LiveTimelineKey>)((T x) => x));
        }

        public IEnumerable<LiveTimelineKey> ToEnumerable()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        public LiveTimelineKey At(int index)
        {
            return (index >= 0 && index < thisList.Count) ? thisList[index] : null;
        }

        public LiveTimelineKey[] ToArray()
        {
            return thisList.ToArray();
        }

        public List<LiveTimelineKey> ToList()
        {
            return thisList.ConvertAll((Converter<T, LiveTimelineKey>)((T x) => x));
        }

        public bool Contains(LiveTimelineKey key)
        {
            return FindIndex(key) >= 0;
        }

        public int FindIndex(LiveTimelineKey key)
        {
            BinSearch(out var ret, out var _, key.frame, 0, Count - 1, Count);
            return ret.index;
        }

        public FindKeyResult FindKeyCached(float frame, bool forceRefind)
        {
            FindKeyCached(frame, forceRefind, out var current, out var _);
            return current;
        }

        public void FindKeyCached(float frame, bool forceRefind, out FindKeyResult current, out FindKeyResult next)
        {
            if (forceRefind || _lastFindIndex < 0)
            {
                FindKey(out current, out next, frame);
            }
            else
            {
                FindCurrentKeyNeighbor(frame, _lastFindIndex, out current, out next);
            }
            _lastFindIndex = current.index;
        }

        public FindKeyResult FindCurrentKey(int frame)
        {
            FindKey(out var ret, out var _, frame);
            return ret;
        }

        public void FindKey(out FindKeyResult ret, out FindKeyResult next, float frame)
        {
            int count = thisList.Count;
            if (count == 0)
            {
                ret.index = -1;
                ret.key = null;
                next.index = -1;
                next.key = null;
            }
            else
            {
                BinSearch(out ret, out next, frame, 0, count - 1, count);
            }
        }

        private void BinSearch(out FindKeyResult ret, out FindKeyResult next, float frame, int indexS, int indexE, int listSize)
        {
            int num = (indexE - indexS >> 1) + indexS;
            T val = thisList[num];
            if (num + 1 < listSize)
            {
                T val2 = thisList[num + 1];
                if (val.frame <= frame && frame < val2.frame)
                {
                    ret.key = val;
                    ret.index = num;
                    next.key = val2;
                    next.index = num + 1;
                    return;
                }
                if (frame < val.frame)
                {
                    indexE = num;
                    if (indexE > indexS)
                    {
                        BinSearch(out ret, out next, frame, indexS, indexE, listSize);
                        return;
                    }
                }
                else
                {
                    indexS = num + 1;
                    if (indexS <= indexE)
                    {
                        BinSearch(out ret, out next, frame, indexS, indexE, listSize);
                        return;
                    }
                }
            }
            else if (val.frame <= frame)
            {
                ret.key = val;
                ret.index = num;
                next.key = null;
                next.index = -1;
                return;
            }
            ret.key = null;
            ret.index = -1;
            next.key = null;
            next.index = -1;
        }

        public void FindKeyLinear(out LiveTimelineKey curKey, out LiveTimelineKey nextKey, int curFrame)
        {
            curKey = null;
            nextKey = null;
            int count = thisList.Count;
            for (int i = 0; i < count; i++)
            {
                if (thisList[i].frame > curFrame)
                {
                    nextKey = thisList[i];
                    break;
                }
                curKey = thisList[i];
            }
        }

        public FindKeyResult FindCurrentKeyNeighbor(float frame, int baseIndex)
        {
            FindCurrentKeyNeighbor(frame, baseIndex, out var ret, out var _);
            return ret;
        }

        public void FindCurrentKeyNeighbor(float frame, int baseIndex, out FindKeyResult ret, out FindKeyResult next)
        {
            ret.key = null;
            ret.index = -1;
            next.key = null;
            next.index = -1;
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            int count = thisList.Count;
            int num = 0;
            bool flag = false;
            while (!flag)
            {
                flag = true;
                int num2 = baseIndex + num;
                int num3 = num2 + 1;
                if (num2 < count)
                {
                    liveTimelineKey = thisList[num2];
                    liveTimelineKey2 = null;
                    if (num3 < count)
                    {
                        liveTimelineKey2 = thisList[num3];
                    }
                    if (liveTimelineKey.frame <= frame)
                    {
                        if (liveTimelineKey2 == null)
                        {
                            ret.key = liveTimelineKey;
                            ret.index = num2;
                            break;
                        }
                        if (frame < liveTimelineKey2.frame)
                        {
                            ret.key = liveTimelineKey;
                            ret.index = num2;
                            next.key = liveTimelineKey2;
                            next.index = num3;
                            break;
                        }
                    }
                    flag = false;
                }
                if (num > 0)
                {
                    num2 = baseIndex - num;
                    num3 = num2 + 1;
                    if (num2 >= 0)
                    {
                        liveTimelineKey = thisList[num2];
                        liveTimelineKey2 = null;
                        if (num3 < count)
                        {
                            liveTimelineKey2 = thisList[num3];
                        }
                        if (liveTimelineKey.frame <= frame)
                        {
                            if (liveTimelineKey2 == null)
                            {
                                ret.key = liveTimelineKey;
                                ret.index = num2;
                                break;
                            }
                            if (frame < liveTimelineKey2.frame)
                            {
                                ret.key = liveTimelineKey;
                                ret.index = num2;
                                next.key = liveTimelineKey2;
                                next.index = num3;
                                break;
                            }
                        }
                        flag = false;
                    }
                }
                num++;
            }
        }


    }

    [System.Serializable]
    public class LiveTimelineKeyIndex
    {
        public int index = -1;
        public LiveTimelineKey prevKey = null;
        public LiveTimelineKey key = null;
        public LiveTimelineKey nextKey = null;
    }

    [System.Serializable]
    public abstract class LiveTimelineKey
    {
        public int frame;
        public LiveTimelineKeyAttribute attribute;

        public LiveTimelineKeyDataType dataType;

        private double _framesecond = -999;

        public double FrameSecond { 
            get{
                if (_framesecond == -999)
                {
                    _framesecond = (double)frame / 60;
                }
                return _framesecond;
               }
           set => _framesecond = value; 
        }
    }

    [System.Serializable]
    public abstract class LiveTimelineKeyWithInterpolate : LiveTimelineKey
    {
        public LiveCameraInterpolateType interpolateType;
        public AnimationCurve curve;
        public LiveTimelineEasing.Type easingType;
    }

    [Serializable]
    public class LiveTimelineKeyTimescaleData : LiveTimelineKey
    {
        public float Timescale;
    }

//正在处理这里
[System.Serializable]
    public class LiveTimelineKeyCameraPositionData : LiveTimelineKeyWithInterpolate
    {
        public LiveCameraPositionType setType;
        public Vector3 position;
        public Vector3 charaPos;
        public Vector3[] bezierPoints;
        public LiveCharaPositionFlag charaRelativeBase;
        public LiveCameraCharaParts charaRelativeParts;
        public float traceSpeed;
        public float nearClip;
        public float farClip;
        public LiveCameraCullingLayer cullingLayer;
        public LiveCameraBgColorType BgColorType;
        public Color BgColor;
        public int BgColorTargetCharacterIndex;

        public Vector3 offset = Vector3.zero;

        public Vector3 posDirect = Vector3.zero;

        public bool newBezierCalcMethod;

        public CullingLayer cullingMask = defCameraCullingLayer;

        public float outlineZOffset = 1f;

        public CharacterLOD characterLODMask = (CharacterLOD)((uint)outlineLODMask + (uint)shaderLODMask);

        protected const CullingLayer defCameraCullingLayer = (CullingLayer)0x7FE;

        protected const CharacterLOD outlineLODMask = (CharacterLOD)0x3FF;

        protected const CharacterLOD shaderLODMask = (CharacterLOD)0x1FF8000;

        public int GetCullingMask()
        {
            return GetCullingMask(cullingMask);
        }

        protected static int GetCullingMask(CullingLayer layer)
        {
            int num = 257;
            if ((layer & CullingLayer.TransparentFX) != 0)
            {
                num |= 2;
            }
            if ((layer & CullingLayer.Background3D_NotReflect) != 0)
            {
                num |= 0x80000;
            }
            if ((layer & CullingLayer.Background3d) != 0)
            {
                num |= 0x100000;
            }
            if ((layer & CullingLayer.Character3d) != 0)
            {
                num |= 0x200000;
            }
            if ((layer & CullingLayer.Character3d_0) != 0)
            {
                num |= 0x400000;
            }
            if ((layer & CullingLayer.Character3d_1) != 0)
            {
                num |= 0x800000;
            }
            if ((layer & CullingLayer.Character3d_2) != 0)
            {
                num |= 0x1000000;
            }
            if ((layer & CullingLayer.Character3d_3) != 0)
            {
                num |= 0x2000000;
            }
            if ((layer & CullingLayer.Character3d_4) != 0)
            {
                num |= 0x4000000;
            }
            if ((layer & CullingLayer.Character3D_NotReflect) != 0)
            {
                num |= 0x8000000;
            }
            if ((layer & CullingLayer.Background3D_Other) != 0)
            {
                num |= 0x40000;
            }
            return num;

            /*
            uint bittest = (uint)layer;

            uint result = (uint)LayerMask.GetMask("Default");

            if ((bittest & (uint)CullingLayer.TransparentFX) > 0)
                result |= (uint)LayerMask.GetMask("TransparentFX");
            if ((bittest & (uint)CullingLayer.Background3D_NotReflect) > 0)
                result |= (uint)LayerMask.GetMask("background_NotReflect");
            if ((bittest & (uint)CullingLayer.Background3d) > 0)
                result |= (uint)LayerMask.GetMask("background");
            if ((bittest & (uint)CullingLayer.Character3d) > 0)
                result |= (uint)LayerMask.GetMask("charas");
            if ((bittest & (uint)CullingLayer.Character3d_0) > 0)
                result |= (uint)LayerMask.GetMask("chara1");
            if ((bittest & (uint)CullingLayer.Character3d_1) > 0)
                result |= (uint)LayerMask.GetMask("chara2");
            if ((bittest & (uint)CullingLayer.Character3d_2) > 0)
                result |= (uint)LayerMask.GetMask("chara3");
            if ((bittest & (uint)CullingLayer.Character3d_3) > 0)
                result |= (uint)LayerMask.GetMask("chara4");
            if ((bittest & (uint)CullingLayer.Character3d_4) > 0)
                result |= (uint)LayerMask.GetMask("chara5");
            if ((bittest & (uint)CullingLayer.Character3D_NotReflect) > 0)
                result |= (uint)LayerMask.GetMask("otherChara");
            if ((bittest & (uint)CullingLayer.Background3D_Other) > 0)
                result |= (uint)LayerMask.GetMask("background_Other");
            return (int)result;
            */
        }

        public static int GetDefaultCullingMask()
        {
            return GetCullingMask(defCameraCullingLayer);
        }

        public virtual Vector3 GetValue(LiveTimelineControl timelineControl)
        {
            return GetValue(timelineControl, setType, containOffset: true);
        }

        protected virtual Vector3 GetValue(LiveTimelineControl timelineControl, LiveCameraPositionType type, bool containOffset)
        {
            Vector3 vector = position;
            switch (type)
            {
                case LiveCameraPositionType.Direct:
                    vector += posDirect;
                    break;
                case LiveCameraPositionType.Character:
                    vector += timelineControl.GetPositionWithCharacters(charaRelativeBase, charaRelativeParts, charaPos);
                    break;
            }
            if (!containOffset)
            {
                return vector;
            }
            return vector + offset;
        }

        public int GetBezierPointCount()
        {
            if (!HasBezier())
            {
                return 0;
            }
            return bezierPoints.Length;
        }

        public bool HasBezier()
        {
            if (bezierPoints != null)
            {
                return bezierPoints.Length != 0;
            }
            return false;
        }

        public bool necessaryToUseNewBezierCalcMethod
        {
            get
            {
                if (!newBezierCalcMethod)
                {
                    return GetBezierPointCount() > 3;
                }
                return true;
            }
        }

        public Vector3 GetBezierPoint(int index, LiveTimelineControl timelineControl)
        {
            if (HasBezier() && index < bezierPoints.Length)
            {
                return GetValue(timelineControl) + bezierPoints[index];
            }
            return GetValue(timelineControl) + Vector3.zero;
        }

        public void GetBezierPoints(LiveTimelineControl timelineControl, Vector3[] outPoints, int startIndex)
        {
            if (HasBezier())
            {
                for (int i = 0; i < bezierPoints.Length; i++)
                {
                    outPoints[startIndex + i] = GetValue(timelineControl) + bezierPoints[i];
                }
            }
        }
    }

    public enum CullingLayer
    {
        TransparentFX = 1,
        Background3D_NotReflect = 2,
        Background3d = 4,
        Character3d = 8,
        Character3d_0 = 0x10,
        Character3d_1 = 0x20,
        Character3d_2 = 0x40,
        Character3d_3 = 0x80,
        Character3d_4 = 0x100,
        Character3D_NotReflect = 0x200,
        Background3D_Other = 0x400
    }

    public enum CharacterLOD
    {
        Outline_0 = 1,
        Outline_1 = 2,
        Outline_2 = 4,
        Outline_3 = 8,
        Outline_4 = 0x10,
        Outline_5 = 0x20,
        Outline_6 = 0x40,
        Outline_7 = 0x80,
        Outline_8 = 0x100,
        Outline_9 = 0x200,
        Shader_0 = 0x8000,
        Shader_1 = 0x10000,
        Shader_2 = 0x20000,
        Shader_3 = 0x40000,
        Shader_4 = 0x80000,
        Shader_5 = 0x100000,
        Shader_6 = 0x200000,
        Shader_7 = 0x400000,
        Shader_8 = 0x800000,
        Shader_9 = 0x1000000
    }

    public enum TimelinePlayerMode
    {
        Light,
        Default
    }

    public delegate void CameraPosUpdateInfoDelegate(ref CameraPosUpdateInfo updateInfo);

    [Serializable]
    public class LiveTimelineKeyTimescaleDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyTimescaleData>
    {
        public bool _isCheckSameFrame;
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraPositionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCameraPositionData>
    {

    }

    public struct FindKeyResult
    {
        public LiveTimelineKey key;

        public int index;
    }

    public class LiveTimelineWorkSheet : ScriptableObject
    {
        public string version;
        public int targetCameraIndex;
        public bool enableAtRuntime;
        public bool enableAtEdit;
        public float TotalTimeLength;
        public bool Lyrics;
        public LiveTimelineDefine.SheetIndex SheetType;
        [SerializeField] public LiveTimelineKeyTimescaleDataList timescaleKeys;
        [SerializeField] public LiveTimelineKeyCameraPositionDataList cameraPosKeys;
        [SerializeField] public List<LiveTimelineMultiCameraPositionData> multiCameraPosKeys;
        [SerializeField] public List<LiveTimelineMultiCameraLookAtData> multiCameraLookAtKeys;

        //[SerializeField]用于该类在别的脚本里定义的时候
        [SerializeField] public LiveTimelineKeyCameraLookAtDataList cameraLookAtKeys;
        [SerializeField] public LiveTimelineKeyCameraFovDataList cameraFovKeys;
        [SerializeField] public LiveTimelineKeyCameraRollDataList cameraRollKeys;

        [SerializeField] public List<LiveTimelineCharaMotSeqData> charaMotSeqList;

        [SerializeField] public LiveTimelineKeyCameraSwitcherDataList cameraSwitcherKeys;
        [SerializeField] public LiveTimelineKeyLipSyncDataList ripSyncKeys;
        [SerializeField] public LiveTimelineKeyLipSyncDataList ripSync2Keys;

        [SerializeField] public LiveTimelineFacialData facial1Set;
        [SerializeField] public LiveTimelineFacialData[] other4FacialArray;
        [SerializeField] public LiveTimelineFormationOffsetData formationOffsetSet;

        [SerializeField] public List<LiveTimelineGlobalLightData> globalLightDataLists;
        [SerializeField] public List<LiveTimelineBgColor1Data> bgColor1List;
        [SerializeField] public List<LiveTimelineBgColor2Data> bgColor2List;

        [SerializeField] public List<LiveTimelineTransformData> transformList;
        [SerializeField] public List<LiveTimelineObjectData> objectList;

        /*
		//终于可以调用AB包了，虽然后面发现没什么用...说不定什么时候能用到
		private void Start()
		{
			LoadCharaMotion();
		}

		public void LoadCharaMotion()
		{
			foreach(LiveTimelineCharaMotSeqData liveCharaData in charaMotSeqList)
			{
				foreach(LiveTimelineKeyCharaMotionData charaMotionData in liveCharaData.keys.thisList)
				{
					foreach(var motionname in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith("3d/motion/live/body") && a.Name.EndsWith(charaMotionData.motionName)))
					{
						//UmaViewerBuilder.Instance.LoadComponent(motionname);
					}
				}
			}
		}
		*/
    }

    public static class LiveCharaPositionFlag_Helper
    {
        public static LiveCharaPositionFlag Default5 => LiveCharaPositionFlag.Center | LiveCharaPositionFlag.Place02 | LiveCharaPositionFlag.Place03 | LiveCharaPositionFlag.Place04 | LiveCharaPositionFlag.Place05;

        public static LiveCharaPositionFlag Everyone => LiveCharaPositionFlag.All;

        public static bool hasFlag(this LiveCharaPositionFlag This, LiveCharaPosition pos)
        {
            return This.hasFlag((LiveCharaPositionFlag)(1 << (int)pos));
        }

        public static bool hasFlag(this LiveCharaPositionFlag This, int bit)
        {
            return This.hasFlag((LiveCharaPositionFlag)bit);
        }

        public static bool hasFlag(this LiveCharaPositionFlag This, LiveCharaPositionFlag bit)
        {
            return (This & bit) != 0;
        }

        public static bool hasFlag(this LiveTimelineKeyAttribute This, LiveTimelineKeyAttribute bit)
    {
            return (This & bit) != 0;
        }
    }

    public class BezierCalcWork
    {
        public static BezierCalcWork cameraPos = new BezierCalcWork();

        public static BezierCalcWork cameraLookAt = new BezierCalcWork();

        private Vector3[] _points = new Vector3[17];

        public void Set(Vector3 startPos, Vector3 endPos, int bezierNum)
        {
            _points[0] = startPos;
            _points[1 + bezierNum] = endPos;
        }

        public void UpdatePoints(LiveTimelineKeyCameraPositionData posKey, LiveTimelineControl timelineControl)
        {
            posKey.GetBezierPoints(timelineControl, _points, 1);
        }

        public void UpdatePoints(LiveTimelineKeyCameraLookAtData lookAtKey, LiveTimelineControl timelineControl, Vector3 camPos)
        {
            lookAtKey.GetBezierPoints(timelineControl, camPos, _points, 1);
        }

        public void Calc(int bezierNum, float t, out Vector3 pos)
        {
            BezierUtil.Calc(_points, bezierNum + 2, t, out pos);
        }
    }
}

