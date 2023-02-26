using Stage;

public class MusicScoreKey
{
    public enum eKeyType
    {
        None = 0,
        Normal = 1,
        Long = 2,
        Slide = 3,
        FevferStart = 81,
        FevferEnd = 82,
        MusicStart = 91,
        MusicEnd = 92,
        NotesCount = 100
    }

    public enum eStatusType
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    public short _id;

    public float _sec;

    public eKeyType _type;

    public int _startPos;

    public int _finishPos;

    public int _status;

    public byte _sync;

    public byte _groupId;

    public short _longPushPartnerId = -1;

    public bool _isSlideJudged;

    public bool IsLongStartKey()
    {
        return _id < _longPushPartnerId;
    }
}
