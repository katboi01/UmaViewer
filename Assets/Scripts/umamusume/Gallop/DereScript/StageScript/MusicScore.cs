using System;
using System.Collections.Generic;

public class MusicScore
{
    private const int NUM_PARAM_COUNT = 8;

    public const int GAP_TEST_MUSIC_ID = 0;

    private List<MusicScoreKey> _keys = new List<MusicScoreKey>();

    private Dictionary<int, List<MusicScoreKey>> _slideGroupDic = new Dictionary<int, List<MusicScoreKey>>();

    private bool _isContinuePlay;

    private int _notesCount;

    private int _appearKeyIndex;

    private float _skillTimeLimit;

    private float _endTime;

    private int _lastKeyId;

    private int _lastSyncKeyId;

    private bool _isExistSlideNote;

    public List<MusicScoreKey> keys
    {
        get
        {
            return _keys;
        }
    }

    public bool isContinuePlay
    {
        get
        {
            return _isContinuePlay;
        }
        set
        {
            _isContinuePlay = value;
        }
    }

    public int notesCount
    {
        get
        {
            return _notesCount;
        }
    }

    public int appearKeyIndex
    {
        get
        {
            return _appearKeyIndex;
        }
    }

    public float skillTimeLimit
    {
        get
        {
            return _skillTimeLimit;
        }
    }

    public float endTime
    {
        get
        {
            return _endTime;
        }
    }

    public int lastKeyId
    {
        get
        {
            return _lastKeyId;
        }
    }

    public int lastSyncKeyId
    {
        get
        {
            return _lastSyncKeyId;
        }
    }

    public void ClearScore()
    {
        _keys.Clear();
        _slideGroupDic.Clear();
        _notesCount = 0;
        _appearKeyIndex = 0;
        _skillTimeLimit = 0f;
        _endTime = 0f;
    }

    public void LoadScore(int musicId, int level, string csvString)
    {
        string[] separator = new string[3]
        {
            ",",
            "\r\n",
            "\n"
        };
        string text = null;
        //text = SingletonMonoBehaviour<ResourcesManager>.instance.LoadGenericCSV(string.Format("MusicScores/m{0:D3}/{0}_{1}", musicId, level));
        text = csvString;
        string[] array = text.Split(separator, StringSplitOptions.None);
        int num = array.Length - 1;
        int num2 = 8;
        int num3 = num / num2;
        if (num % num2 > 0)
        {
        }
        float num4 = 0f;
        int[] array2 = new int[5]
        {
            -1,
            -1,
            -1,
            -1,
            -1
        };
        //bool flag = SingletonMonoBehaviour<LocalData>.instance.option.isMirror;
        _isExistSlideNote = false;
        for (int i = 0; i < num3; i++)
        {
            MusicScoreKey musicScoreKey = new MusicScoreKey();
            int num5 = i * num2;
            if (i != 0)
            {
                musicScoreKey._id = short.Parse(array[num5]);
                if (array[num5 + 1] != string.Empty)
                {
                    musicScoreKey._sec = float.Parse(array[num5 + 1]);
                }
                musicScoreKey._type = (MusicScoreKey.eKeyType)int.Parse(array[num5 + 2]);
                if (array[num5 + 3] != string.Empty)
                {
                    musicScoreKey._startPos = int.Parse(array[num5 + 3]);
                }
                if (array[num5 + 4] != string.Empty)
                {
                    musicScoreKey._finishPos = int.Parse(array[num5 + 4]);
                    if (musicScoreKey._finishPos > 0)
                    {
                        if (array2[musicScoreKey._finishPos - 1] == -1 && musicScoreKey._type == MusicScoreKey.eKeyType.Long)
                        {
                            array2[musicScoreKey._finishPos - 1] = musicScoreKey._startPos;
                        }
                        else if (array2[musicScoreKey._finishPos - 1] != -1)
                        {
                            musicScoreKey._startPos = array2[musicScoreKey._finishPos - 1];
                            array2[musicScoreKey._finishPos - 1] = -1;
                        }
                    }
                }
                if (array[num5 + 5] != string.Empty)
                {
                    musicScoreKey._status = int.Parse(array[num5 + 5]);
                }
                if (array[num5 + 6] != string.Empty)
                {
                    musicScoreKey._sync = byte.Parse(array[num5 + 6]);
                }
                if (array[num5 + 7] != string.Empty)
                {
                    musicScoreKey._groupId = byte.Parse(array[num5 + 7]);
                }
                switch (musicScoreKey._type)
                {
                    case MusicScoreKey.eKeyType.Normal:
                    case MusicScoreKey.eKeyType.Long:
                    case MusicScoreKey.eKeyType.Slide:
                        break;
                    case MusicScoreKey.eKeyType.NotesCount:
                        _notesCount = musicScoreKey._status;
                        goto IL_0359;
                    case MusicScoreKey.eKeyType.MusicEnd:
                        _endTime = musicScoreKey._sec;
                        goto IL_0359;
                    default:
                        goto IL_0359;
                }
                if (musicScoreKey._type == MusicScoreKey.eKeyType.Slide)
                {
                    _isExistSlideNote = true;
                }
                num4 = musicScoreKey._sec;
                _lastKeyId = musicScoreKey._id;
                /*
                if ((bool)SingletonMonoBehaviour<TempData>.instance.liveTemp.isMV && musicId != 0)
                {
                    continue;
                }
                if (flag)
                {
                    musicScoreKey._startPos = Mathf.Abs(6 - musicScoreKey._startPos);
                    musicScoreKey._finishPos = Mathf.Abs(6 - musicScoreKey._finishPos);
                    switch (musicScoreKey._status)
                    {
                        case 1:
                            musicScoreKey._status = 2;
                            break;
                        case 2:
                            musicScoreKey._status = 1;
                            break;
                    }
                }
                */
            }
            goto IL_0359;
        IL_0359:
            _keys.Add(musicScoreKey);
            if (musicScoreKey._type == MusicScoreKey.eKeyType.Slide)
            {
                if (!_slideGroupDic.ContainsKey(musicScoreKey._groupId))
                {
                    _slideGroupDic.Add(musicScoreKey._groupId, new List<MusicScoreKey>());
                }
                _slideGroupDic[musicScoreKey._groupId].Add(musicScoreKey);
            }
        }
        foreach (KeyValuePair<int, List<MusicScoreKey>> item in _slideGroupDic)
        {
            _slideGroupDic[item.Key].Sort(delegate (MusicScoreKey x, MusicScoreKey y)
            {
                if (x._sec < y._sec)
                {
                    return -1;
                }
                return 1;
            });
        }
        if (_keys.Count > _lastKeyId && _lastKeyId > 0 && _keys[_lastKeyId]._sync == 1)
        {
            for (int num6 = _lastKeyId - 1; num6 > 0; num6--)
            {
                if (_keys[num6]._sync == 1 && _keys[num6]._sec == _keys[_lastKeyId]._sec)
                {
                    _lastSyncKeyId = num6;
                    break;
                }
            }
        }
        _skillTimeLimit = num4 - 3f;
        /*
        if (!SingletonMonoBehaviour<LiveController>.IsInstanceEmpty())
        {
            SingletonMonoBehaviour<LiveController>.instance.gradePoint = 10000f / (float)_notesCount;
        }
        */
    }

    public void InitScore()
    {
        _appearKeyIndex = 0;
    }

    public MusicScoreKey BornMusicKey(float scoreTime, float noteUseTime)
    {
        if (_appearKeyIndex >= _keys.Count)
        {
            return null;
        }
        float num = noteUseTime;
        if (scoreTime >= _keys[_appearKeyIndex]._sec)
        {
            if (_isContinuePlay)
            {
                /*
                num = LiveUtils.GetNoteAnimeTime(noteUseTime);
                if (SingletonMonoBehaviour<LocalData>.instance.livePlay.IsContainsTapNoteId(_keys[_appearKeyIndex]._id))
                {
                    if (!SingletonMonoBehaviour<LiveController>.IsInstanceEmpty())
                    {
                        SingletonMonoBehaviour<LiveController>.instance.EraseLongKey(_keys[_appearKeyIndex], true);
                    }
                    _appearKeyIndex++;
                    return BornMusicKey(scoreTime, noteUseTime);
                }
                */
            }
            if (scoreTime - _keys[_appearKeyIndex]._sec > num && _keys[_appearKeyIndex]._type == MusicScoreKey.eKeyType.Normal)
            {
                _appearKeyIndex++;
                return BornMusicKey(scoreTime, noteUseTime);
            }
            MusicScoreKey musicScoreKey = _keys[_appearKeyIndex];
            _appearKeyIndex++;
            if (musicScoreKey._type != MusicScoreKey.eKeyType.Normal && musicScoreKey._type != MusicScoreKey.eKeyType.Long && musicScoreKey._type != MusicScoreKey.eKeyType.Slide)
            {
                return BornMusicKey(scoreTime, noteUseTime);
            }
            return musicScoreKey;
        }
        return null;
    }

    public MusicScoreKey FindEndLongPush(int id)
    {
        int count = _keys.Count;
        int finishPos = _keys[id]._finishPos;
        for (int i = id + 1; i < count; i++)
        {
            MusicScoreKey musicScoreKey = _keys[i];
            if (finishPos == musicScoreKey._finishPos)
            {
                return musicScoreKey;
            }
        }
        return null;
    }

    public int GetMusicTimeNotesCount(float musicTime)
    {
        int num = 0;
        for (int i = 0; i < _keys.Count && _keys[i]._sec <= musicTime; i++)
        {
            if (_keys[i]._type == MusicScoreKey.eKeyType.Normal || _keys[i]._type == MusicScoreKey.eKeyType.Long || _keys[i]._type == MusicScoreKey.eKeyType.Slide)
            {
                num++;
            }
        }
        return num;
    }

    public bool isExistSlideNote()
    {
        return _isExistSlideNote;
    }

    private MusicScoreKey GetNextKey(MusicScoreKey key)
    {
        int id = key._id;
        if (id + 1 >= _keys.Count)
        {
            return null;
        }
        return _keys[id + 1];
    }

    public MusicScoreKey SearchNextGroupKey(MusicScoreKey key)
    {
        int id = key._id;
        for (int i = 1; i < 10; i++)
        {
            if (id + i >= _keys.Count)
            {
                return null;
            }
            MusicScoreKey musicScoreKey = _keys[id + i];
            if (key._groupId == musicScoreKey._groupId)
            {
                return musicScoreKey;
            }
        }
        return null;
    }

    public bool IsStartSlideKey(int groupId, int id)
    {
        return _slideGroupDic[groupId][0]._id == id;
    }

    public bool IsFinishSlideKey(int groupId, int id)
    {
        int count = _slideGroupDic[groupId].Count;
        return _slideGroupDic[groupId][count - 1]._id == id;
    }

    public MusicScoreKey GetStartSlideKey(MusicScoreKey key)
    {
        int groupId = key._groupId;
        return _slideGroupDic[groupId][0];
    }

    public MusicScoreKey GetPreSlideKey(MusicScoreKey key)
    {
        int groupId = key._groupId;
        int num = _slideGroupDic[groupId].IndexOf(key);
        if (num - 1 < 0)
        {
            return null;
        }
        return _slideGroupDic[groupId][num - 1];
    }

    public MusicScoreKey GetEndSlideKey(int startSlideKeyId)
    {
        MusicScoreKey musicScoreKey = null;
        for (int i = startSlideKeyId - 1; i < _keys.Count; i++)
        {
            if (_keys[i]._id == startSlideKeyId)
            {
                musicScoreKey = _keys[i];
                break;
            }
        }
        if (musicScoreKey == null || !_slideGroupDic.ContainsKey(musicScoreKey._groupId))
        {
            return null;
        }
        //MusicScoreKey musicScoreKey2 = null;
        int count = _slideGroupDic[musicScoreKey._groupId].Count;
        return _slideGroupDic[musicScoreKey._groupId][count - 1];
    }

    public MusicScoreKey GetNextSlideKey(MusicScoreKey key)
    {
        int groupId = key._groupId;
        int num = _slideGroupDic[groupId].IndexOf(key);
        if (num < 0 || num + 1 >= _slideGroupDic[groupId].Count)
        {
            return null;
        }
        return _slideGroupDic[groupId][num + 1];
    }

    public void SetSlideJudge(MusicScoreKey key)
    {
        int groupId = key._groupId;
        int num = _slideGroupDic[groupId].IndexOf(key);
        if (num >= 0)
        {
            _slideGroupDic[groupId][num]._isSlideJudged = true;
        }
    }

    public List<MusicScoreKey> GetSlideGroupList(MusicScoreKey key)
    {
        int groupId = key._groupId;
        return _slideGroupDic[groupId];
    }
}
