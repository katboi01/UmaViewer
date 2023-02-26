using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DressCabinet : Cabinet
{
    public enum eBinder
    {
        AccessorySheet,
        HeadSheet,
        BodySheet,
        TransformAnimator,
        SpareDress
    }

    [Serializable]
    public class AccRecord : Entity
    {
        [SerializeField]
        private int _mainIndex;

        [SerializeField]
        private int _subIndex;

        [SerializeField]
        private bool _loadMain = true;

        [SerializeField]
        private bool _loadSub;

        public int charaId => base.key;

        public int mainIndex
        {
            get
            {
                return _mainIndex;
            }
            set
            {
                _mainIndex = value;
            }
        }

        public int subIndex
        {
            get
            {
                return _subIndex;
            }
            set
            {
                _subIndex = value;
            }
        }

        public bool loadMain
        {
            get
            {
                return _loadMain;
            }
            set
            {
                _loadMain = value;
            }
        }

        public bool loadSub
        {
            get
            {
                return _loadSub;
            }
            set
            {
                _loadSub = value;
            }
        }

        public bool usableMainIndex
        {
            get
            {
                if (_mainIndex > 0)
                {
                    return loadMain;
                }
                return false;
            }
        }

        public bool usableSubIndex
        {
            get
            {
                if (_subIndex > 0)
                {
                    return loadSub;
                }
                return false;
            }
        }

        public AccRecord(int charaId)
            : base(charaId)
        {
        }

        public void Clear()
        {
            _mainIndex = 0;
            _subIndex = 0;
            _loadMain = true;
            _loadSub = false;
        }

        protected override Entity OnClone(int newKey)
        {
            return new AccRecord(newKey)
            {
                _mainIndex = _mainIndex,
                _subIndex = _subIndex,
                _loadMain = _loadMain,
                _loadSub = _loadSub
            };
        }
    }

    [Serializable]
    public class AccSheet : Container<AccRecord>
    {
        public enum eDiversity
        {
            None,
            AnotherModel,
            TextureOnly
        }

        [SerializeField]
        private bool _textureDiversity;

        [SerializeField]
        private eDiversity _diversity;

        public int dressId => base.key;

        public bool textureDiversity
        {
            get
            {
                return _textureDiversity;
            }
            set
            {
                _textureDiversity = value;
            }
        }

        public eDiversity diversity
        {
            get
            {
                return _diversity;
            }
            set
            {
                _diversity = value;
            }
        }

        public AccSheet(int dressId)
            : base(dressId)
        {
        }

        protected override Entity OnClone(int newKey)
        {
            AccSheet accSheet = base.OnClone(newKey) as AccSheet;
            if (accSheet != null)
            {
                accSheet._textureDiversity = _textureDiversity;
            }
            return accSheet;
        }
    }

    [Serializable]
    public class AccSheetBinder : Container<AccSheet>
    {
        public AccSheetBinder(int binderKey)
            : base(binderKey)
        {
        }
    }

    [Serializable]
    public class BodyRecord : Entity
    {
        public enum eDiversity
        {
            AnotherModel = 0,
            TextureOnly = 2
        }

        [SerializeField]
        private eDiversity _diversity;

        [SerializeField]
        private int _index;

        public int charaId => base.key;

        public eDiversity diversity
        {
            get
            {
                return _diversity;
            }
            set
            {
                _diversity = value;
            }
        }

        public int index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }

        public BodyRecord(int charaId) : base(charaId)
        {
        }

        public void Clear()
        {
            _index = 0;
            _diversity = eDiversity.AnotherModel;
        }

        protected override Entity OnClone(int newKey)
        {
            return new BodyRecord(newKey)
            {
                _diversity = _diversity,
                _index = _index
            };
        }
    }

    [Serializable]
    public class BodySheet : Container<BodyRecord>
    {
        public int dressId => base.key;

        public BodySheet(int dressId)
            : base(dressId)
        {
        }
    }

    [Serializable]
    public class BodySheetBinder : Container<BodySheet>
    {
        public BodySheetBinder(int binderKey)
            : base(binderKey)
        {
        }
    }

    public class Cache
    {
        private AccSheet _accSheet;

        private AccRecord _accRecord;

        private HeadRecord _headRecord;

        private BodyRecord _bodyRecord;

        private TransformAnimatorRecord _transformAnimatorRecord;

        public bool validAcc
        {
            get
            {
                if (_accSheet != null)
                {
                    return _accRecord != null;
                }
                return false;
            }
        }

        public bool validHead => _headRecord != null;

        public bool validBody => _bodyRecord != null;

        public int headTexIndex
        {
            get
            {
                if (_headRecord == null || _headRecord.diversity != HeadRecord.eDiversity.TextureOnly)
                {
                    return 0;
                }
                return _headRecord.index;
            }
        }

        public int headIndex
        {
            get
            {
                if (_headRecord == null || _headRecord.diversity != HeadRecord.eDiversity.AnotherModel)
                {
                    return 0;
                }
                return _headRecord.index;
            }
        }

        public bool useHeadObjTex
        {
            get
            {
                if (_headRecord == null || _headRecord.diversity != HeadRecord.eDiversity.AnotherModel)
                {
                    return false;
                }
                return _headRecord.useObjTex;
            }
        }

        public int bodyTexIndex
        {
            get
            {
                if (_bodyRecord == null)
                {
                    return 0;
                }
                return _bodyRecord.index;
            }
        }

        public int bodyIndex
        {
            get
            {
                if (_bodyRecord == null || _bodyRecord.diversity == BodyRecord.eDiversity.TextureOnly)
                {
                    return 0;
                }
                return _bodyRecord.index;
            }
        }

        public bool loadMain
        {
            get
            {
                if (_accRecord == null)
                {
                    return false;
                }
                return _accRecord.loadMain;
            }
        }

        public bool loadSub
        {
            get
            {
                if (_accRecord == null)
                {
                    return false;
                }
                return _accRecord.loadSub;
            }
        }

        public bool accTexDiversity
        {
            get
            {
                if (_accSheet == null)
                {
                    return false;
                }
                if (!_accSheet.textureDiversity)
                {
                    return _accSheet.diversity == AccSheet.eDiversity.TextureOnly;
                }
                return true;
            }
        }

        public bool usableMainAccIndex
        {
            get
            {
                if (_accRecord == null)
                {
                    return false;
                }
                if (_accRecord.usableMainIndex)
                {
                    return _accSheet.diversity == AccSheet.eDiversity.AnotherModel;
                }
                return false;
            }
        }

        public int mainAccIndex
        {
            get
            {
                if (!usableMainAccIndex)
                {
                    return 0;
                }
                return _accRecord.mainIndex;
            }
        }

        public bool usableSubAccIndex
        {
            get
            {
                if (_accRecord != null)
                {
                    return _accRecord.usableSubIndex;
                }
                return false;
            }
        }

        public int subAccIndex
        {
            get
            {
                if (!usableSubAccIndex)
                {
                    return 0;
                }
                return _accRecord.subIndex;
            }
        }

        public int accTexIndex
        {
            get
            {
                int num = 0;
                if (_accRecord != null)
                {
                    num += _accRecord.mainIndex * 100;
                    num += _accRecord.subIndex;
                }
                return num;
            }
        }

        public TransformAnimatorRecord transformAnimatorRecord => _transformAnimatorRecord;

        public void Initialize(int dressId, int charaId)
        {
            DressCabinet cabinet = Cabinet.GetCabinet<DressCabinet>();
            if (!(cabinet == null))
            {
                cabinet.GetAccData(dressId, charaId, out _accSheet, out _accRecord);
                cabinet.GetHeadData(dressId, charaId, out _headRecord);
                cabinet.GetBodyData(dressId, charaId, out _bodyRecord);
                cabinet.GetTransformAnimatorData(dressId, charaId, out _transformAnimatorRecord);
            }
        }

        public void Reset()
        {
            _accSheet = null;
            _accRecord = null;
            _headRecord = null;
            _bodyRecord = null;
            _transformAnimatorRecord = null;
        }
    }

    [Serializable]
    public class HeadRecord : Entity
    {
        public enum eDiversity
        {
            None,
            AnotherModel,
            TextureOnly
        }

        [SerializeField]
        private eDiversity _diversity;

        [SerializeField]
        private int _index;

        [SerializeField]
        private bool _useObjTex;

        public int charaId => base.key;

        public eDiversity diversity
        {
            get
            {
                return _diversity;
            }
            set
            {
                _diversity = value;
            }
        }

        public int index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }

        public bool useObjTex
        {
            get
            {
                return _useObjTex;
            }
            set
            {
                _useObjTex = value;
            }
        }

        public HeadRecord(int charaId)
            : base(charaId)
        {
        }

        public void Clear()
        {
            _diversity = eDiversity.None;
        }

        protected override Entity OnClone(int newKey)
        {
            return new HeadRecord(newKey)
            {
                _diversity = _diversity,
                _index = _index,
                _useObjTex = _useObjTex
            };
        }
    }

    [Serializable]
    public class HeadSheet : Container<HeadRecord>
    {
        public int dressId => base.key;

        public HeadSheet(int dressId)
            : base(dressId)
        {
        }
    }

    [Serializable]
    public class HeadSheetBinder : Container<HeadSheet>
    {
        public HeadSheetBinder(int binderKey)
            : base(binderKey)
        {
        }
    }

    [Serializable]
    public class SpareDressRecord : Entity
    {
        [SerializeField]
        private int[] _spareDressId;

        public int dressId => base.key;

        public SpareDressRecord(int charaId)
            : base(charaId)
        {
            if (_spareDressId == null)
            {
                int defaultDressTypeCount = Character3DBase.CharacterData.GetDefaultDressTypeCount();
                _spareDressId = new int[defaultDressTypeCount];
            }
        }

        public void Clear()
        {
            int defaultDressTypeCount = Character3DBase.CharacterData.GetDefaultDressTypeCount();
            _spareDressId = new int[defaultDressTypeCount];
        }

        public int GetSpareDressId(Character3DBase.CharacterData.eDressType findDressType)
        {
            return _spareDressId[(int)findDressType];
        }

        protected override Entity OnClone(int newKey)
        {
            SpareDressRecord spareDressRecord = new SpareDressRecord(newKey);
            for (int i = 0; i < _spareDressId.Length; i++)
            {
                spareDressRecord._spareDressId[i] = _spareDressId[i];
            }
            return spareDressRecord;
        }
    }

    [Serializable]
    public class SpareDressSheet : Container<SpareDressRecord>
    {
        public int songId => base.key;

        public SpareDressSheet(int songId)
            : base(songId)
        {
        }
    }

    [Serializable]
    public class SpareDressSheetBinder : Container<SpareDressSheet>
    {
        public SpareDressSheetBinder(int binderKey)
            : base(binderKey)
        {
        }
    }

    [Serializable]
    public class TransformAnimatorRecord : Entity
    {
        [Serializable]
        public class Info
        {
            public Character3DBase.Parts.eCategory partsCategory = Character3DBase.Parts.eCategory.Unknown;

            public bool useTrigger;

            public int triggerId;

            public bool useAutoPlay;

            public float autoPlayTime;

            public bool useAngleLimit;

            public Vector4 limitAngle = Vector4.zero;

            public bool useTimeLimit;

            public float limitTime;

            public string jointName = string.Empty;

            public Character3DBase.TransformAnimator.eRotate rotate;

            public Vector4 data = Vector4.zero;

            public Vector3 axis
            {
                get
                {
                    return data;
                }
                set
                {
                    data = value;
                }
            }

            public float angle
            {
                get
                {
                    return data.w;
                }
                set
                {
                    data.w = value;
                }
            }

            public Info clone => new Info
            {
                partsCategory = partsCategory,
                useTrigger = useTrigger,
                triggerId = triggerId,
                useAutoPlay = useAutoPlay,
                autoPlayTime = autoPlayTime,
                useAngleLimit = useAngleLimit,
                limitAngle = limitAngle,
                useTimeLimit = useTimeLimit,
                limitTime = limitTime,
                jointName = jointName,
                rotate = rotate,
                data = data
            };
        }

        [SerializeField]
        private List<Info> _lstInfo = new List<Info>();

        public List<Info> lstInfo => _lstInfo;

        public TransformAnimatorRecord(int charaId)
            : base(charaId)
        {
        }

        protected override Entity OnClone(int newKey)
        {
            TransformAnimatorRecord transformAnimatorRecord = new TransformAnimatorRecord(newKey);
            foreach (Info item in _lstInfo)
            {
                Info clone = item.clone;
                transformAnimatorRecord._lstInfo.Add(clone);
            }
            return transformAnimatorRecord;
        }
    }

    [Serializable]
    public class TransformAnimatorSheet : Container<TransformAnimatorRecord>
    {
        public int dressId => base.key;

        public TransformAnimatorSheet(int dressId)
            : base(dressId)
        {
        }
    }

    [Serializable]
    public class TransformAnimatorSheetBinder : Container<TransformAnimatorSheet>
    {
        public TransformAnimatorSheetBinder(int binderKey)
            : base(binderKey)
        {
        }
    }

    public const string ASSET_NAME = "dress_cabinet";

    public readonly string[] _binderNames = new string[5] { "Accessory差分設定", "Head差分設定", "Body差分設定", "TransformAnimator", "SpareDress" };

    [SerializeField]
    private AccSheetBinder _accSheetBinder;

    [SerializeField]
    private HeadSheetBinder _headSheetBinder;

    [SerializeField]
    private BodySheetBinder _bodySheetBinder;

    [SerializeField]
    private TransformAnimatorSheetBinder _transformAnimatorSheetBinder;

    [SerializeField]
    private SpareDressSheetBinder _spareDressSheetBinder;

    public AccSheetBinder accSheetBinder => _accSheetBinder;

    public HeadSheetBinder headSheetBinder => _headSheetBinder;

    public BodySheetBinder bodySheetBinder => _bodySheetBinder;

    public TransformAnimatorSheetBinder transformAnimatorSheetBinder => _transformAnimatorSheetBinder;

    public SpareDressSheetBinder spareDressSheetBinder => _spareDressSheetBinder;

    public DressCabinet()
    {
        CreateEntity(0, ref _accSheetBinder);
        CreateEntity(1, ref _headSheetBinder);
        CreateEntity(2, ref _bodySheetBinder);
        CreateEntity(3, ref _transformAnimatorSheetBinder);
        CreateEntity(4, ref _spareDressSheetBinder);
    }

    public void GetAccData(int dressId, int charaId, out AccSheet sheet, out AccRecord record)
    {
        sheet = null;
        record = null;
        sheet = _accSheetBinder.Get(dressId);
        if (sheet != null)
        {
            record = sheet.Get(charaId);
        }
    }

    public void GetHeadData(int dressId, int charaId, out HeadRecord record)
    {
        record = null;
        HeadSheet headSheet = _headSheetBinder.Get(dressId);
        if (headSheet != null)
        {
            record = headSheet.Get(charaId);
        }
    }

    public void GetBodyData(int dressId, int charaId, out BodyRecord record)
    {
        record = null;
        BodySheet bodySheet = _bodySheetBinder.Get(dressId);
        if (bodySheet != null)
        {
            record = bodySheet.Get(charaId);
        }
    }

    public void GetTransformAnimatorData(int dressId, int charaId, out TransformAnimatorRecord record)
    {
        record = null;
        TransformAnimatorSheet transformAnimatorSheet = _transformAnimatorSheetBinder.Get(dressId);
        if (transformAnimatorSheet != null)
        {
            record = transformAnimatorSheet.Get(charaId);
        }
    }

    public void GetSpareDressData(int songId, int dressId, out SpareDressRecord record)
    {
        record = null;
        SpareDressSheet spareDressSheet = _spareDressSheetBinder.Get(songId);
        if (spareDressSheet != null)
        {
            record = spareDressSheet.Get(dressId);
        }
    }
}
