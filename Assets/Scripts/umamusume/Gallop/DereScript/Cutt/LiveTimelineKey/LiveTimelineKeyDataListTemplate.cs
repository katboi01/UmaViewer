using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyDataListTemplate<T> : ILiveTimelineKeyDataList where T : LiveTimelineKey
    {
        [SerializeField]
        private LiveTimelineKeyDataListAttr _attribute;

        [SerializeField]
        private TimelineKeyPlayMode _playMode;

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

        public FindKeyResult FindKeyCached(int frame, bool forceRefind)
        {
            FindKeyCached(frame, forceRefind, out var current, out var _);
            return current;
        }

        public void FindKeyCached(int frame, bool forceRefind, out FindKeyResult current, out FindKeyResult next)
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

        public void FindKey(out FindKeyResult ret, out FindKeyResult next, int frame)
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

        private void BinSearch(out FindKeyResult ret, out FindKeyResult next, int frame, int indexS, int indexE, int listSize)
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

        public FindKeyResult FindCurrentKeyNeighbor(int frame, int baseIndex)
        {
            FindCurrentKeyNeighbor(frame, baseIndex, out var ret, out var _);
            return ret;
        }

        public void FindCurrentKeyNeighbor(int frame, int baseIndex, out FindKeyResult ret, out FindKeyResult next)
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
}
