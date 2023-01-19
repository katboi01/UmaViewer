using Cutt;
using UnityEngine;

public abstract class BaseEventData
{
    public abstract int GetEventType();
}

public class BaseEventType
{
    private readonly string _eventName;

    private int _eventId;

    private bool _isInit;

    public string eventName => _eventName;

    public BaseEventType(string eventName)
    {
        _eventName = eventName;
    }

    public void Init()
    {
        if (!_isInit)
        {
            _eventId = FNVHash.Generate(_eventName);
            _isInit = true;
        }
    }

    public virtual int GetEventType()
    {
        Init();
        return _eventId;
    }
}

public class GeneralEventDefine
{
    public class EventType : BaseEventType
    {
        public EventType(string eventName)
            : base("General" + eventName)
        {
        }
    }

    public abstract class EventData : BaseEventData
    {
    }

    public class WildcardEventData : EventData
    {
        private BaseEventType _eventType;

        public WildcardEventData(BaseEventType eventType)
        {
            _eventType = eventType;
        }

        public override int GetEventType()
        {
            return _eventType.GetEventType();
        }
    }

    public class SelectDressEventData : EventData
    {
        private int _selectDressId = 0;

        private int _selectClosetId = 0;

        private int _cardId = 0;

        public int selectDressId
        {
            get
            {
                return _selectDressId;
            }
            set
            {
                _selectDressId = value;
            }
        }

        public int selectClosetId
        {
            get
            {
                return _selectClosetId;
            }
            set
            {
                _selectClosetId = value;
            }
        }

        public int cardId
        {
            get
            {
                return _cardId;
            }
            set
            {
                _cardId = value;
            }
        }

        public override int GetEventType()
        {
            return EVENT_TYPE_SELECT_DRESS.GetEventType();
        }
    }

    public class ScreenCapturePrepareEventData : EventData
    {
        private bool _saveTerminal;

        public bool saveTerminal
        {
            get
            {
                return _saveTerminal;
            }
            set
            {
                _saveTerminal = value;
            }
        }

        public override int GetEventType()
        {
            return EVENT_TYPE_SCREEN_CAPTURE_PREPARE.GetEventType();
        }
    }

    public class ScreenCaptureStartEventData : EventData
    {
        private int _layerMask;

        private float _textureWidth = Screen.width;

        private float _textureHeight = Screen.height;

        public int layerMask
        {
            get
            {
                return _layerMask;
            }
            set
            {
                _layerMask = value;
            }
        }

        public float textureWidth
        {
            get
            {
                return _textureWidth;
            }
            set
            {
                _textureWidth = value;
            }
        }

        public float textureHeight
        {
            get
            {
                return _textureHeight;
            }
            set
            {
                _textureHeight = value;
            }
        }

        public override int GetEventType()
        {
            return EVENT_TYPE_SCREEN_CAPTURE_START.GetEventType();
        }
    }

    public class SendCaptureTextureEventData : EventData
    {
        private int _layer;

        private RenderTexture _renderTexture;

        public int layer
        {
            get
            {
                return _layer;
            }
            set
            {
                _layer = value;
            }
        }

        public RenderTexture renderTexture
        {
            get
            {
                return _renderTexture;
            }
            set
            {
                _renderTexture = value;
            }
        }

        public override int GetEventType()
        {
            return EVENT_TYPE_SEND_CAPTURE_TEXTURE.GetEventType();
        }
    }

    public static readonly EventType EVENT_TYPE_END_SCENE = new EventType("EndScene");

    public static readonly EventType EVENT_TYPE_OPEN_DRESS_SELECT = new EventType("OpenDressSelect");

    public static readonly EventType EVENT_TYPE_SELECT_DRESS = new EventType("SelectDress");

    public static readonly EventType EVENT_TYPE_SCREEN_CAPTURE_PREPARE = new EventType("ScreenCapturePrepare");

    public static readonly EventType EVENT_TYPE_SCREEN_CAPTURE_START = new EventType("ScreenCaptureStart");

    public static readonly EventType EVENT_TYPE_SCREEN_CAPTURE_End = new EventType("ScreenCaptureEnd");

    public static readonly EventType EVENT_TYPE_SEND_CAPTURE_TEXTURE = new EventType("SendCaptureTexture");

    public static readonly EventType EVENT_TYPE_OPEN_ERROR_POPUP = new EventType("OpenErrorPopup");

    public static readonly EventType EVENT_TYPE_LIVE_PAUSE_START = new EventType("LivePauseStart");

    public static readonly EventType EVENT_TYPE_LIVE_PAUSE_END = new EventType("LivePauseEnd");
}
