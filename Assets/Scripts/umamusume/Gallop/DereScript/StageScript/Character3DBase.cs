using Cutt;
using Stage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Character3DBase : MonoBehaviour
{
    /// <summary>
    /// 汗の表示位置を指定する
    /// </summary>
    public class SweatLocatorInfo
    {
        public Transform locator;

        public GameObject sweatObj;

        public Transform sweatTrans;
    }

    /// <summary>
    /// リソースデータのクオリティ
    /// </summary>
    public enum eResourceQuality
    {
        LowPolygon = 0,
        HighPolygon = 1,
        Rich = 2,
        Invalid = 3,
        MAX = 3
    }

    /// <summary>
    /// レンダリング順序
    /// </summary>
    public enum eRenderQueue
    {
        Geometry = 1999,
        Transparent = 2999
    }

    /// <summary>
    /// 頭モデルの読み出し方法の指定
    /// </summary>
    public enum eHeadLoadType
    {
        Default,
        DressModel
    }

    /// <summary>
    /// モデルタイプ
    /// </summary>
    public enum eModelType : byte
    {
        Head = 1,
        Body,
        Accessory
    }

    /// <summary>
    /// キャラクタモデルに関する各種情報
    /// </summary>
    public class CharacterData
    {
        /// <summary>
        /// モデルのサイズ(4種類)
        /// </summary>
        public enum HeightModelType
        {
            S,
            M,
            L,
            LL
        }

        public enum eDressType
        {
            DressA,
            DressB,
            DressC
        }

        /// <summary>
        /// 初期値
        /// </summary>
        public const int INVALID_VALUE = 0;

        /// <summary>
        /// カードID
        /// </summary>
        protected int _cardId;

        /// <summary>
        /// キャラクタID
        /// </summary>
        protected int _charaId;

        /// <summary>
        /// 身長
        /// </summary>
        protected int _heightId;

        /// <summary>
        /// 体重
        /// </summary>
        protected int _weightId;

        /// <summary>
        /// バストサイズ
        /// </summary>
        protected int _bustId;

        /// <summary>
        /// 肌の色
        /// </summary>
        protected int _skinId;

        /// <summary>
        /// 色(パッション/キュート/クール)
        /// </summary>
        protected int _colorId;

        /// <summary>
        /// 
        /// </summary>
        protected int _height;

        protected int _heightWithoutHeel;

        protected int _attribute;

        /// <summary>
        /// ドレスID(モデル固有値)
        /// </summary>
        protected int _dressId;

        /// <summary>
        /// アクセサリID
        /// </summary>
        protected int _accessoryId;

        /// <summary>
        /// ドレスチェンジ時のドレスID
        /// </summary>
        protected int _changeDressId;

        /// <summary>
        /// 簡易モデルかどうか
        /// </summary>
        protected bool _isDressGradeDown;

        /// <summary>
        /// SSRドレスを着ているか
        /// </summary>
        protected bool _isChangedSSRDress;

        /// <summary>
        /// ドレスタイプ
        /// </summary>
        protected eDressType _dressType;

        /// <summary>
        /// チェンジ時のキャラデータ
        /// </summary>
        protected CharacterData[] _spareDataArray;

        /// <summary>
        /// 当たり判定に使用する境界ボックス
        /// </summary>
        protected Master3DCharaData.BoundsBoxSizeData _boundsBoxSizeData = Master3DCharaData.defaultBoundsBoxSizeData;

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

        public int charaId => _charaId;

        public int heightId => _heightId;

        public int weightId => _weightId;

        public int bustId => _bustId;

        public int skinId => _skinId;

        public int colorId => _colorId;

        public int height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        public int heightWithoutHeel
        {
            get
            {
                return _heightWithoutHeel;
            }
            set
            {
                _heightWithoutHeel = value;
            }
        }

        public int attribute => _attribute;

        public int originDressId => _dressId;

        public int originAccessoryId => _accessoryId;

        public virtual int activeDressId => GetActiveDressId(_charaId, _changeDressId, _dressId, _attribute);

        public virtual int activeAccessoryId => GetActiveAccessoryId(_charaId, _changeDressId, _accessoryId, _attribute);

        public int changeDressId
        {
            get
            {
                return _changeDressId;
            }
            set
            {
                _changeDressId = value;
            }
        }

        public bool isDressGradeDown => _isDressGradeDown;

        public bool isChangedSSRDress => _isChangedSSRDress;

        public Master3DCharaData.BoundsBoxSizeData boundsBoxSizeData
        {
            get
            {
                return _boundsBoxSizeData;
            }
            set
            {
                _boundsBoxSizeData = value;
            }
        }

        public eDressType dressType => _dressType;

        public CharacterData[] spareDataArray => _spareDataArray;

        public static int GetDefaultDressTypeCount()
        {
            return 2;
        }

        public static int GetActiveDressId(int charaId, int dressId, int cardModelId, int attribute)
        {
            if (dressId == 0)
            {
                return cardModelId;
            }
            int num = DressUtil.ConvertUnitDressModelId(dressId, charaId, (DressUtil.eAttributeType)attribute);
            if (num != dressId)
            {
                return num;
            }
            bool flag = DressUtil.GetDressOpenType(dressId) == 4;
            if (dressId < 100)
            {
                if (flag)
                {
                    return cardModelId;
                }
                return dressId;
            }
            bool flag4 = StageUtil.IsModelCommonDressId(cardModelId);
            if (flag4 && dressId > 100)
            {
                num = dressId % 100;
                if (flag)
                {
                    return cardModelId;
                }
                return num;
            }
            return cardModelId;
        }

        public static int GetActiveAccessoryId(int charaId, int dressId, int cardModelId, int attribute)
        {
            if (dressId == 0)
            {
                return cardModelId;
            }
            int num = DressUtil.ConvertUnitDressModelId(dressId, charaId, (DressUtil.eAttributeType)attribute);
            if (num != dressId)
            {
                return num;
            }
            bool flag = DressUtil.GetDressOpenType(dressId) == 4;
            if (dressId < 100)
            {
                if (flag)
                {
                    return cardModelId;
                }
                return dressId;
            }
            if (StageUtil.IsModelCommonAccId(cardModelId) && dressId > 100)
            {
                num = dressId % 100;
                if (flag)
                {
                    return cardModelId;
                }
                return num;
            }
            return cardModelId;
        }

        public CharacterData(int cardId, int charaId, int dressId, int accessoryId, int heightId, int weightId, int bustId, int skinId, int colorId, int height, int change_dressId, int attribute)
        {
            Initialize(cardId, charaId, dressId, accessoryId, heightId, weightId, bustId, skinId, colorId, height, change_dressId, attribute);
        }

        public CharacterData(CharacterData src, int dressId = 0, int accId = 0)
        {
            dressId = ((dressId != 0) ? dressId : src.activeDressId);
            accId = ((accId != 0) ? accId : src.activeAccessoryId);
            Initialize(src.cardId, src.charaId, src.originDressId, accId, src.heightId, src.weightId, src.bustId, src.skinId, src.colorId, dressId, src._heightWithoutHeel, src.attribute);
        }

        public CharacterData(MasterCardData.CardData card, MasterCharaData.CharaData chara, int change_dressId)
        {
            if (card != null && chara != null)
            {
                int dressID = 0;
                if (card.openDressId == 0) { dressID = 1; }
                else { dressID = card.openDressId; }

                Initialize(card.id, card.charaId, dressID, 1, chara.modelHeightId, chara.modelWeightId, chara.modelBustId, chara.modelSkinId, chara.attribute - 1, change_dressId, chara.height, card.attribute);
            }
        }

        /*
        public CharacterData(WorkCardData.CardData card, int change_dressId)
        {
            if (card != null)
            {
                Initialize(card.GetImageCardId(), card.GetCharaId(), StageUtil.GetModelDressId(card.GetImageCardId()), StageUtil.GetModelAccId(card.GetImageCardId()), card.GetModelHeightId(), card.GetModelWeightId(), card.GetModelBustId(), card.GetModelSkinId(), card.GetModelColorId(), change_dressId, card.GetHeight(), card.GetAttribute());
            }
        }
        */

        private void Initialize(int cardId, int charaId, int dressId, int accessoryId, int heightId, int weightId, int bustId, int skinId, int colorId, int height, int change_dress_id, int attribute)
        {
            _cardId = cardId;
            _charaId = charaId;
            _dressId = dressId;
            _accessoryId = accessoryId;
            _heightId = heightId;
            _weightId = weightId;
            _bustId = bustId;
            _skinId = skinId;
            _colorId = colorId;
            _height = height + 8;
            _heightWithoutHeel = height;
            _changeDressId = change_dress_id;
            _attribute = attribute;
        }

        public void InitializeSpare(int spareCount)
        {
            if (_spareDataArray == null && spareCount >= 1)
            {
                _spareDataArray = new CharacterData[spareCount];
            }
        }

        public void CreateSpare(int dressId = 0, int accId = 0, eDressType dressType = eDressType.DressB)
        {
            if (_spareDataArray == null)
            {
                _spareDataArray = new CharacterData[1];
            }
            int num = (int)(dressType - 1);
            if (_spareDataArray[num] == null)
            {
                _spareDataArray[num] = new CharacterData(this, dressId, accId);
                _spareDataArray[num]._dressType = dressType;
                _spareDataArray[num]._spareDataArray = null;
            }
        }

        public void GetLoadCharacterAssetBundle(List<string> list, eResourceQuality resourceQuality, int headIndex, int headTextureIndex, bool isSubHeadIndex, List<string> lstPartsCode, bool useLocalTexture = false)
        {
            if (list != null && _dressId != 0 && _charaId != 0)
            {
                GetLoadAssetBundle(list, _charaId, activeDressId, activeAccessoryId, _heightId, _weightId, _bustId, _skinId, _colorId, headIndex, headTextureIndex, isSubHeadIndex, useLocalTexture, lstPartsCode, resourceQuality);
            }
        }


        /// <summary>
        /// キャラクタ限定のオブジェクト（シューコの扇子や仁奈の手袋等）
        /// </summary>
        public void GetCharacterPropsList(int index, LiveTimelineControl liveTimelineControl, ref List<string> outListPropsNames, List<LiveTimelinePropsSettings.PropsConditionGroup> outListProps, bool isCheckOverlap, int cardId)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < liveTimelineControl.data.propsSetteings.propsDataGroup.Length; i++)
            {
                LiveTimelinePropsSettings.PropsDataGroup propsDataGroup = liveTimelineControl.data.propsSetteings.propsDataGroup[i];
                for (int j = 0; j < propsDataGroup.propsConditionGroup.Length; j++)
                {
                    LiveTimelinePropsSettings.PropsConditionGroup propsConditionGroup = propsDataGroup.propsConditionGroup[j];
                    if (string.IsNullOrEmpty(propsConditionGroup.propsName))
                    {
                        continue;
                    }
                    bool flag = true;
                    for (int k = 0; k < propsConditionGroup.propsConditionData.Length; k++)
                    {
                        LiveTimelinePropsSettings.PropsConditionData propsConditionData = propsConditionGroup.propsConditionData[k];
                        if (propsConditionGroup.propsConditionData.Length == 1 && propsConditionData.Type == LiveTimelinePropsSettings.PropsConditionType.Default)
                        {
                            break;
                        }
                        switch (propsConditionData.Type)
                        {
                            case LiveTimelinePropsSettings.PropsConditionType.CharaAttribute:
                                if (_colorId != propsConditionData.Value)
                                {
                                    flag = false;
                                }
                                break;
                            case LiveTimelinePropsSettings.PropsConditionType.CharaPosition:
                                if (index != propsConditionData.Value)
                                {
                                    flag = false;
                                }
                                break;
                            case LiveTimelinePropsSettings.PropsConditionType.CharaID:
                                if (_charaId != propsConditionData.Value)
                                {
                                    flag = false;
                                }
                                break;
                            case LiveTimelinePropsSettings.PropsConditionType.CardID:
                                if (cardId == propsConditionData.Value)
                                {
                                    break;
                                }
                                flag = false;
                                if (false)
                                {
                                    int num = ConvertModelIdSSRDress(_changeDressId, _charaId);
                                    if (num != 0 && num == propsConditionData.Value)
                                    {
                                        flag = ((!CheckSameCardPropsData(propsDataGroup)) ? true : false);
                                    }
                                }
                                break;
                            case LiveTimelinePropsSettings.PropsConditionType.DressType:
                                flag = isSelectPropsTargetDress(propsConditionData.Value, activeDressId, _dressId);
                                break;
                            case LiveTimelinePropsSettings.PropsConditionType.DressID:
                                if (activeDressId != propsConditionData.Value)
                                {
                                    flag = false;
                                }
                                break;
                        }
                        if (!flag)
                        {
                            if (propsConditionGroup.satisfiesAllConditions)
                            {
                                break;
                            }
                            if (k + 1 < propsConditionGroup.propsConditionData.Length)
                            {
                                flag = true;
                            }
                        }
                    }
                    if (!flag)
                    {
                        continue;
                    }
                    string text = propsConditionGroup.propsName;
                    if (propsConditionGroup.random && propsConditionGroup.randomNameArray.Length != 0)
                    {
                        int num2 = UnityEngine.Random.Range(0, propsConditionGroup.randomNameArray.Length);
                        text = text + "_" + propsConditionGroup.randomNameArray[num2];
                    }
                    if (propsConditionGroup.alterPropsMode == LiveTimelinePropsSettings.AlterPropsMode.LeftHanded && MasterDBManager.instance.masterCharaData.Get(_charaId).hand == 3002)
                    {
                        text += "_alt";
                    }
                    if (isCheckOverlap)
                    {
                        if (!list.Contains(text))
                        {
                            list.Add(text);
                        }
                    }
                    else
                    {
                        list.Add(text);
                        outListProps?.Add(propsConditionGroup);
                    }
                    break;
                }
            }
            if (outListPropsNames == null)
            {
                outListPropsNames = list;
            }
            else if (isCheckOverlap)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (!outListPropsNames.Contains(list[i]))
                    {
                        outListPropsNames.Add(list[i]);
                    }
                }
            }
            else
            {
                outListPropsNames.AddRange(list);
            }
        }

        public static int ConvertModelIdSSRDress(int dressId, int charaId)
        {
            List<MasterCardData.CardData> listWithCharaIdOrderByIdAsc = MasterDBManager.instance.masterCardData.GetListWithCharaIdOrderByIdAsc(charaId);
            for (int i = 0; i < listWithCharaIdOrderByIdAsc.Count; i++)
            {
                if (listWithCharaIdOrderByIdAsc[i].openDressId == dressId)
                {
                    return listWithCharaIdOrderByIdAsc[i].id;
                }
            }
            return 0;
        }

        private bool CheckSameCardPropsData(LiveTimelinePropsSettings.PropsDataGroup propsDataGroup)
        {
            for (int i = 0; i < propsDataGroup.propsConditionGroup.Length; i++)
            {
                LiveTimelinePropsSettings.PropsConditionGroup propsConditionGroup = propsDataGroup.propsConditionGroup[i];
                if (string.IsNullOrEmpty(propsConditionGroup.propsName))
                {
                    continue;
                }
                for (int j = 0; j < propsConditionGroup.propsConditionData.Length; j++)
                {
                    LiveTimelinePropsSettings.PropsConditionData propsConditionData = propsConditionGroup.propsConditionData[j];
                    if (propsConditionGroup.propsConditionData.Length == 1 && propsConditionData.Type == LiveTimelinePropsSettings.PropsConditionType.Default)
                    {
                        break;
                    }
                    if (propsConditionData.Type == LiveTimelinePropsSettings.PropsConditionType.CardID && _cardId == propsConditionGroup.propsConditionData[j].Value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool isSelectPropsTargetDress(int targetDress, int dressId, int modelId)
        {
            int currentDressId = DressUtil.GetCurrentDressId(dressId, modelId);
            int num = 1 << (int)DressUtil.GetDressType(currentDressId);
            if ((targetDress & num) > 0)
            {
                return true;
            }
            return false;
        }
    }

    protected enum eCloth
    {
        Body,
        Face,
        Acc,
        Acc2,
        MAX
    }

    /// <summary>
    /// モデル組み立て用情報
    /// </summary>
    public class CreateInfo
    {
        public enum eDummyType
        {
            None = 0,
            Cute = 1,
            Cool = 2,
            Passion = 3,
            Invalid = 4,
            MAX = 4
        }

        public enum eDebugStep
        {
            OverrideQuality = 0,
            DefaultDress = 1,
            DummyModel = 2,
            Max = 3,
            Invalid = 3
        }

        public enum eDebugFlag
        {
            None = 0,
            OverrideQuality = 1,
            DefaultDress = 2,
            DummyModel = 4
        }

        public const eResourceQuality DEFAULT_RESOURCE_QUALITY = eResourceQuality.HighPolygon;

        public const int DEFAULT_DRESS_ID = 1;

        private bool _isBootDirect;

        private bool _useAssetBundle;

        private eResourceQuality _resourceQuality = eResourceQuality.HighPolygon;

        private eDebugFlag _debugFlags;

        private int _index;

        private int _activeDressId;

        private int _activeAccessoryId;

        private CharacterData _charaData;

        private bool _isSubHeadIndex;

        private int _headIndex;

        private int _headTextureIndex;

        private bool _mergeMaterial;

        private List<string> _lstPartsCode = new List<string>();

        //private TextureComposite.Meta _textureCompositeMeta;

        private CySpringCollisionComponent.ePurpose _cyspringPurpose = CySpringCollisionComponent.ePurpose.Generic;

        private LiveTimelineData.CharacterPositionMode _positionMode;

        private HashSet<Camera> _renderTargetCameraList = new HashSet<Camera>();

        public bool isBootDirect
        {
            get
            {
                return _isBootDirect;
            }
            set
            {
                _isBootDirect = value;
            }
        }

        public bool useAssetBundle
        {
            get
            {
                return _useAssetBundle;
            }
            set
            {
                _useAssetBundle = value;
            }
        }

        public eResourceQuality resourceQuality
        {
            get
            {
                return _resourceQuality;
            }
            set
            {
                _resourceQuality = value;
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

        public int activeDressId => _activeDressId;

        public int activeAccessoryId => _activeAccessoryId;

        public CharacterData charaData
        {
            get
            {
                return _charaData;
            }
            set
            {
                _charaData = value;
                if (_charaData != null)
                {
                    _activeDressId = _charaData.activeDressId;
                    _activeAccessoryId = _charaData.activeAccessoryId;
                }
                else
                {
                    _activeDressId = 0;
                    _activeAccessoryId = 0;
                }
            }
        }

        public bool isSubHeadIndex
        {
            get
            {
                return _isSubHeadIndex;
            }
            set
            {
                _isSubHeadIndex = value;
            }
        }

        public int headIndex
        {
            get
            {
                return _headIndex;
            }
            set
            {
                _headIndex = value;
            }
        }

        public int headTextureIndex
        {
            get
            {
                return _headTextureIndex;
            }
            set
            {
                _headTextureIndex = value;
            }
        }

        public bool mergeMaterial
        {
            get
            {
                return _mergeMaterial;
            }
            set
            {
                _mergeMaterial = value;
            }
        }

        public List<string> lstPartsCode => _lstPartsCode;

        /*
        public TextureComposite.Meta textureCompositeMeta
        {
            get
            {
                return _textureCompositeMeta;
            }
            set
            {
                _textureCompositeMeta = value;
            }
        }
        */

        public CySpringCollisionComponent.ePurpose cyspringPurpose
        {
            get
            {
                return _cyspringPurpose;
            }
            set
            {
                _cyspringPurpose = value;
            }
        }

        public LiveTimelineData.CharacterPositionMode positionMode
        {
            get
            {
                return _positionMode;
            }
            set
            {
                _positionMode = value;
            }
        }

        public HashSet<Camera> renderTargetCameraList => _renderTargetCameraList;

        public bool loadAccessory => StageUtil.IsModelCommonDressId(_activeDressId);

        public void Reset(CharacterData charaData = null, eResourceQuality resourceQuality = eResourceQuality.Invalid)
        {
            _index = 0;
            _isBootDirect = false;
            _useAssetBundle = false;
            _mergeMaterial = false;
            _resourceQuality = ((resourceQuality != eResourceQuality.Invalid) ? resourceQuality : _resourceQuality);
            _debugFlags = eDebugFlag.None;
            _isSubHeadIndex = false;
            _headIndex = 0;
            _headTextureIndex = 0;
            _lstPartsCode.Clear();
            //_textureCompositeMeta = null;
            _cyspringPurpose = CySpringCollisionComponent.ePurpose.Generic;
            _positionMode = LiveTimelineData.CharacterPositionMode.Immobility;
            this.charaData = ((charaData != null) ? charaData : this.charaData);
            _renderTargetCameraList.Clear();
        }

        public void SetRenderTargetCameraList(Camera renderTargetCamera)
        {
            _renderTargetCameraList.Clear();
            if (renderTargetCamera != null)
            {
                _renderTargetCameraList.Add(renderTargetCamera);
            }
        }

        public void SetRenderTargetCameraList(List<Camera> lstRenderTargetCamera)
        {
            _renderTargetCameraList.Clear();
            foreach (Camera item in lstRenderTargetCamera)
            {
                if (!_renderTargetCameraList.Contains(item) && item != null)
                {
                    _renderTargetCameraList.Add(item);
                }
            }
        }

        public void SetPartsCode(List<string> partsCodeList)
        {
            _lstPartsCode.Clear();
            if (partsCodeList != null && partsCodeList.Count > 0)
            {
                _lstPartsCode.AddRange(partsCodeList);
            }
        }

        public void ClearPartsCode()
        {
            _lstPartsCode.Clear();
        }

        public bool CheckAssemblable()
        {
            bool result = false;
            if (_charaData != null && _lstPartsCode != null && _lstPartsCode.Count > 1)
            {
                result = true;
            }
            return result;
        }

        public bool CheckDebugFlag(eDebugFlag flag)
        {
            return (_debugFlags & flag) != 0;
        }

        public void SetDebugData(eDebugFlag flags)
        {
            Action action = delegate
            {
                _isSubHeadIndex = false;
                _headIndex = 0;
                _headTextureIndex = 0;
                _lstPartsCode.Clear();
                //_textureCompositeMeta = null;
                _cyspringPurpose = CySpringCollisionComponent.ePurpose.Generic;
            };
            if ((flags & eDebugFlag.OverrideQuality) != 0)
            {
                _debugFlags |= eDebugFlag.OverrideQuality;
                _resourceQuality = eResourceQuality.HighPolygon;
            }
            if ((flags & eDebugFlag.DefaultDress) != 0)
            {
                _debugFlags |= eDebugFlag.DefaultDress;
                action();
                _charaData.changeDressId = 1;
                _activeDressId = _charaData.activeDressId;
                _activeAccessoryId = _charaData.activeAccessoryId;
            }
            if ((flags & eDebugFlag.DummyModel) != 0)
            {
                _debugFlags |= eDebugFlag.DummyModel;
                action();
                charaData = new CharacterData(100001, 101, 1, 0, 1, 0, 1, 1, 0, 0, 159, 1);
            }
        }
    }

    public abstract class Gimmick
    {
        public enum eCategory
        {
            Unknown,
            TransformAnimator
        }

        public Gimmick()
        {
        }

        public void Init()
        {
            OnInit();
        }

        public void Update(GimmickController.UpdateInfo updateInfo)
        {
            OnUpdate(updateInfo);
        }

        public void Reset()
        {
            OnReset();
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnUpdate(GimmickController.UpdateInfo updateInfo)
        {
        }

        protected virtual void OnReset()
        {
        }

        public abstract eCategory GetCategory();
    }

    public class GimmickController : RequestExecutor<GimmickRequest>
    {
        public class UpdateInfo
        {
            private bool _pause;

            private float _currentTime;

            public bool pause
            {
                get
                {
                    return _pause;
                }
                set
                {
                    _pause = value;
                }
            }

            public float currentTime
            {
                get
                {
                    return _currentTime;
                }
                set
                {
                    _currentTime = value;
                }
            }

            public UpdateInfo()
            {
                Reset();
            }

            public void Reset()
            {
                _pause = false;
                _currentTime = 0f;
            }
        }

        private Character3DBase _owner;

        private Dictionary<int, Gimmick> _mapGimmick = new Dictionary<int, Gimmick>();

        private Gimmick[] _arrGimmick = new Gimmick[0];

        private UpdateInfo _updateInfo = new UpdateInfo();

        public Character3DBase owner => _owner;

        public override Type type => typeof(GimmickController);

        public Gimmick GetGimmick(Gimmick.eCategory category)
        {
            Gimmick value = null;
            _mapGimmick.TryGetValue((int)category, out value);
            return value;
        }

        private void InitTransformAnimator(DressCabinet.TransformAnimatorRecord record)
        {
            if (record == null)
            {
                return;
            }
            List<TransformAnimator.Track> lstTrack = new List<TransformAnimator.Track>();
            Action<DressCabinet.TransformAnimatorRecord.Info> action = delegate (DressCabinet.TransformAnimatorRecord.Info info)
            {
                foreach (Parts lstPart in _owner.lstParts)
                {
                    if (lstPart.category == info.partsCategory)
                    {
                        Transform transform = lstPart.transformCollector.Find(info.jointName);
                        if (transform != null)
                        {
                            TransformAnimator.Track item = new TransformAnimator.Track
                            {
                                useTrigger = info.useTrigger,
                                triggerId = info.triggerId,
                                useAutoPlay = info.useAutoPlay,
                                autoPlayTime = info.autoPlayTime,
                                useAngleLimit = info.useAngleLimit,
                                limitAngle = info.limitAngle,
                                useTimeLimit = info.useTimeLimit,
                                limitTime = info.limitTime,
                                target = transform,
                                rotate = info.rotate,
                                data = info.data
                            };
                            lstTrack.Add(item);
                        }
                    }
                }
            };
            foreach (DressCabinet.TransformAnimatorRecord.Info item2 in record.lstInfo)
            {
                action(item2);
            }
            TransformAnimator transformAnimator = new TransformAnimator();
            transformAnimator.SetTrack(lstTrack);
            int category = (int)transformAnimator.GetCategory();
            if (!_mapGimmick.ContainsKey(category))
            {
                _mapGimmick.Add(category, transformAnimator);
            }
        }

        public void Initialize(Character3DBase owner, DressCabinet.Cache cache)
        {
            _owner = owner;
            InitTransformAnimator(cache.transformAnimatorRecord);
            if (_mapGimmick.Count > 0)
            {
                _arrGimmick = new Gimmick[_mapGimmick.Count];
                _mapGimmick.Values.CopyTo(_arrGimmick, 0);
            }
            if (_arrGimmick != null)
            {
                Gimmick[] arrGimmick = _arrGimmick;
                for (int i = 0; i < arrGimmick.Length; i++)
                {
                    arrGimmick[i]?.Init();
                }
            }
        }

        public void Update()
        {
            Gimmick[] arrGimmick = _arrGimmick;
            for (int i = 0; i < arrGimmick.Length; i++)
            {
                arrGimmick[i].Update(_updateInfo);
            }
        }

        public void Release()
        {
        }

        public void SetPause(bool bPause)
        {
            _updateInfo.pause = bPause;
        }

        public void SetCurrentTime(float time)
        {
            _updateInfo.currentTime = time;
        }

        protected override void OnExecuteRequest(GimmickRequest request)
        {
            TransformAnimator transformAnimator = GetGimmick(Gimmick.eCategory.TransformAnimator) as TransformAnimator;
            if (transformAnimator != null)
            {
                GimmickRequest.TransformAnimatorRequestParameter transformAnimatorRequestParameter = request.transformAnimatorRequestParameter;
                int triggerId = transformAnimatorRequestParameter.triggerId;
                if (transformAnimatorRequestParameter.CheckFlag(GimmickRequest.TransformAnimatorRequestParameter.eChangeFlag.Active))
                {
                    bool active = transformAnimatorRequestParameter.active;
                    transformAnimator.SetActive(active, triggerId);
                }
                if (transformAnimatorRequestParameter.CheckFlag(GimmickRequest.TransformAnimatorRequestParameter.eChangeFlag.AutoPlay))
                {
                    bool autoPlay = transformAnimatorRequestParameter.autoPlay;
                    transformAnimator.SetAutoPlay(autoPlay, triggerId);
                }
            }
        }
    }

    public class RenderController : IDisposable
    {
        public const float DEF_OUTLINE_Z_OFFSET = 0.0015f;

        private Character3DBase _owner;

        private bool _initMaterialPack;

        private bool _initRenderCommand;

        private bool _initInitialize;

        private int _propertyIdColor = MaterialPack.INVALID_ID;

        private int _propertyIdCameraFov = MaterialPack.INVALID_ID;

        private int _propertyIdOutlineZOffset = MaterialPack.INVALID_ID;

        private MaterialPropertyBlock _mtrlProp;

        private MaterialPack.UpdateInfo _mtrlUpdateInfo = new MaterialPack.UpdateInfo();

        private HashSet<Parts> _lstParts = new HashSet<Parts>();

        private Dictionary<Material, MaterialPack> _mapMaterialPack = new Dictionary<Material, MaterialPack>();

        private RenderCommand.UpdateInfo _renderCommandUpdateInfo = new RenderCommand.UpdateInfo();

        private Dictionary<RenderCommand.eCategory, RenderCommand> _mapRenderCommand = new Dictionary<RenderCommand.eCategory, RenderCommand>();

        private MaterialPack[] _mapMaterialPackTable;

        private RenderCommand[] _mapRenderCommandTable;

        private Parts[] _partsListTable;

        private bool _disposed;

        public MaterialPropertyBlock mtrlProp => _mtrlProp;

        public MaterialPack.UpdateInfo materialUpdateInfo => _mtrlUpdateInfo;

        public RenderCommand.UpdateInfo renderCommandUpdateInfo => _renderCommandUpdateInfo;

        public bool enableOutlineLOD
        {
            get
            {
                return _renderCommandUpdateInfo.enableOutlineLOD;
            }
            set
            {
                _renderCommandUpdateInfo.enableOutlineLOD = value;
            }
        }

        public bool enableShaderLOD
        {
            get
            {
                return _renderCommandUpdateInfo.enableShaderLOD;
            }
            set
            {
                _renderCommandUpdateInfo.enableShaderLOD = value;
            }
        }

        public float distanceOutlineLOD
        {
            get
            {
                return _renderCommandUpdateInfo.distanceForOutlineLOD;
            }
            set
            {
                _renderCommandUpdateInfo.distanceForOutlineLOD = value;
            }
        }

        public float distanceShaderLOD
        {
            get
            {
                return _renderCommandUpdateInfo.distanceForShaderLOD;
            }
            set
            {
                _renderCommandUpdateInfo.distanceForShaderLOD = value;
            }
        }

        ~RenderController()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Cleanup();
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public void SetPause(bool bPause)
        {
            _mtrlUpdateInfo.pause = bPause;
        }

        public void SetColor(ref Vector4 clr)
        {
            if (_mtrlProp != null)
            {
                _mtrlProp.SetVector(_propertyIdColor, clr);
            }
            _mtrlUpdateInfo.charaColor = clr;
        }

        public void SetCameraFOV(float fov)
        {
            if (_mtrlProp != null)
            {
                _mtrlProp.SetFloat(_propertyIdCameraFov, fov);
            }
        }

        public void SetOutlineZOffset(float zoffset)
        {
            if (_mtrlProp != null)
            {
                _mtrlProp.SetFloat(_propertyIdOutlineZOffset, zoffset * 0.0015f);
            }
        }

        public void SetShaderKeyword(MaterialPack.eShaderKeyword keyword, bool sw)
        {
            string shaderKeyword = MaterialPack.GetShaderKeyword(keyword);
            foreach (KeyValuePair<Material, MaterialPack> item in _mapMaterialPack)
            {
                MaterialPack value = item.Value;
                if (value.CheckShaderKeywordSwitch(keyword) != sw)
                {
                    value.SetShaderKeyword(keyword, sw);
                }
            }
            foreach (KeyValuePair<RenderCommand.eCategory, RenderCommand> item2 in _mapRenderCommand)
            {
                item2.Value.SetShaderKeyword(shaderKeyword, sw);
            }
        }

        public void RegisterCamera(Camera targetCamera)
        {
            if (!(targetCamera != null))
            {
                return;
            }
            foreach (KeyValuePair<RenderCommand.eCategory, RenderCommand> item in _mapRenderCommand)
            {
                item.Value.Register(targetCamera);
            }
        }

        public void UnregisterCamera(Camera targetCamera)
        {
            if (!(targetCamera != null))
            {
                return;
            }
            foreach (KeyValuePair<RenderCommand.eCategory, RenderCommand> item in _mapRenderCommand)
            {
                item.Value.Unregister(targetCamera);
            }
        }

        public bool GetRenderCommand(RenderCommand.eCategory cat, out RenderCommand renderCommand)
        {
            return _mapRenderCommand.TryGetValue(cat, out renderCommand);
        }

        public void Initialize(List<Parts> lstParts, Character3DBase owner, Camera renderTargetCamera)
        {
            Initialize(lstParts, owner, new HashSet<Camera> { renderTargetCamera });
        }

        public void Initialize(List<Parts> lstParts, Character3DBase owner, HashSet<Camera> renderTargetCameraList = null)
        {
            if (_initMaterialPack)
            {
                return;
            }
            _owner = owner;
            Action<Parts> action = delegate (Parts parts)
            {
                if (!_lstParts.Contains(parts))
                {
                    _lstParts.Add(parts);
                }
                foreach (KeyValuePair<Material, MaterialPack> item in parts.mapMaterialPack)
                {
                    if (!_mapMaterialPack.ContainsKey(item.Key))
                    {
                        _mapMaterialPack.Add(item.Key, item.Value);
                    }
                }
            };
            foreach (Parts lstPart in lstParts)
            {
                if (lstPart != null)
                {
                    action(lstPart);
                }
            }
            _mtrlProp = new MaterialPropertyBlock();
            _propertyIdColor = Shader.PropertyToID("_CharaColor");
            _propertyIdCameraFov = Shader.PropertyToID("_CameraFov");
            _propertyIdOutlineZOffset = Shader.PropertyToID("_outlineZOffset");
            _mtrlProp.SetVector(_propertyIdColor, Color.white);
            _mtrlProp.SetFloat(_propertyIdCameraFov, 60f);
            _mtrlProp.SetFloat(_propertyIdOutlineZOffset, 0.0015f);
            _initMaterialPack = true;
            _mapMaterialPackTable = new MaterialPack[_mapMaterialPack.Count];
            _mapMaterialPack.Values.CopyTo(_mapMaterialPackTable, 0);
            _partsListTable = new Parts[_lstParts.Count];
            _lstParts.CopyTo(_partsListTable);
            if (owner != null && renderTargetCameraList != null && renderTargetCameraList.Count > 0)
            {
                CreateRenderCommand(renderTargetCameraList);
            }
            _initInitialize = true;
        }

        public void CreateRenderCommand(Camera renderTargetCamera)
        {
            CreateRenderCommand(new HashSet<Camera> { renderTargetCamera });
        }

        public void CreateRenderCommand(HashSet<Camera> renderTargetCameraList)
        {
            if (!_initMaterialPack || _initRenderCommand)
            {
                return;
            }
            Action<RenderCommand.eCategory> action = delegate (RenderCommand.eCategory category)
            {
                RenderCommand value = null;
                if (!_mapRenderCommand.TryGetValue(category, out value))
                {
                    value = RenderCommand.Create(_owner, category, _lstParts);
                    if (value != null)
                    {
                        _mapRenderCommand.Add(category, value);
                    }
                }
            };
            Action<RenderCommand> action2 = delegate (RenderCommand renderCommand)
            {
                if (renderCommand != null)
                {
                    foreach (Camera renderTargetCamera in renderTargetCameraList)
                    {
                        renderCommand.Register(renderTargetCamera);
                    }
                }
            };
            action(RenderCommand.eCategory.Outline);
            foreach (KeyValuePair<RenderCommand.eCategory, RenderCommand> item in _mapRenderCommand)
            {
                action2(item.Value);
            }
            _mapRenderCommandTable = new RenderCommand[_mapRenderCommand.Count];
            _mapRenderCommand.Values.CopyTo(_mapRenderCommandTable, 0);
            _initRenderCommand = true;
        }

        public void Update()
        {
            if (_initInitialize && _initRenderCommand)
            {
                MaterialPack[] mapMaterialPackTable = _mapMaterialPackTable;
                for (int i = 0; i < mapMaterialPackTable.Length; i++)
                {
                    mapMaterialPackTable[i].Update(_mtrlUpdateInfo);
                }
                Parts[] partsListTable = _partsListTable;
                for (int i = 0; i < partsListTable.Length; i++)
                {
                    partsListTable[i].renderer.SetPropertyBlock(_mtrlProp);
                }
                RenderCommand[] mapRenderCommandTable = _mapRenderCommandTable;
                for (int i = 0; i < mapRenderCommandTable.Length; i++)
                {
                    mapRenderCommandTable[i].Update(_renderCommandUpdateInfo);
                }
            }
        }

        public void Cleanup()
        {
            foreach (KeyValuePair<RenderCommand.eCategory, RenderCommand> item in _mapRenderCommand)
            {
                item.Value.Dispose();
            }
            _mapRenderCommand.Clear();
            _mapMaterialPack.Clear();
            _lstParts.Clear();
            _mapRenderCommandTable = null;
            _mapMaterialPackTable = null;
            _partsListTable = null;
            _initRenderCommand = false;
            _initMaterialPack = false;
            _initInitialize = false;
        }
    }

    public class TransformAnimator : Gimmick
    {
        public class Track
        {
            public enum eState
            {
                Ready,
                Run,
                Stop
            }

            private bool _active;

            private eState _state;

            private float _elapsedTime;

            private Vector4 _currentRotateAxis = Vector4.zero;

            private Vector4 _accumulatedAngle = Vector4.zero;

            public bool useTrigger;

            public int triggerId;

            public bool useAutoPlay;

            public float autoPlayTime;

            public bool useAngleLimit;

            public Vector4 limitAngle = Vector4.zero;

            public bool useTimeLimit;

            public float limitTime;

            public Transform target;

            public eRotate rotate;

            public Vector4 data = Vector4.zero;

            public bool active
            {
                get
                {
                    return _active;
                }
                set
                {
                    _active = value;
                }
            }

            public eState state
            {
                get
                {
                    return _state;
                }
                private set
                {
                    _elapsedTime = 0f;
                    _state = value;
                }
            }

            public void Init()
            {
                _active = !useTrigger & !useAutoPlay;
            }

            private bool CalcRotation(int idxAxis, float frame)
            {
                bool result = false;
                if (!useAngleLimit || limitAngle[idxAxis] == 0f || _accumulatedAngle[idxAxis] < limitAngle[idxAxis])
                {
                    result = true;
                    _currentRotateAxis[idxAxis] = data[idxAxis] / frame;
                    _accumulatedAngle[idxAxis] += _currentRotateAxis[idxAxis];
                }
                return result;
            }

            public void UpdateState(float curTime)
            {
                switch (state)
                {
                    case eState.Ready:
                        if (_active)
                        {
                            state = eState.Run;
                        }
                        else if (useAutoPlay)
                        {
                            if (_elapsedTime <= autoPlayTime)
                            {
                                _elapsedTime += Time.deltaTime;
                                break;
                            }
                            _active = true;
                            state = eState.Run;
                        }
                        break;
                    case eState.Run:
                        if (useTimeLimit && limitTime <= _elapsedTime)
                        {
                            _active = false;
                            state = eState.Stop;
                        }
                        else
                        {
                            _elapsedTime += Time.deltaTime;
                        }
                        break;
                }
            }

            public void Animate(float curTime, bool reverse = false)
            {
                float num = 1f / Time.deltaTime;
                switch (rotate)
                {
                    case eRotate.Angle:
                        if (false | CalcRotation(0, num) | CalcRotation(1, num) | CalcRotation(2, num))
                        {
                            target.Rotate(_currentRotateAxis, Space.Self);
                        }
                        break;
                    case eRotate.Axis:
                        if (!useAngleLimit || limitAngle.w == 0f || _accumulatedAngle.w < limitAngle.w)
                        {
                            _currentRotateAxis.x = data.x;
                            _currentRotateAxis.y = data.y;
                            _currentRotateAxis.z = data.z;
                            _currentRotateAxis.w = data.w / num;
                            _accumulatedAngle.w += _currentRotateAxis.w;
                            target.Rotate(_currentRotateAxis.normalized, _currentRotateAxis.w, Space.Self);
                        }
                        break;
                }
            }
        }

        public enum eRotate
        {
            Angle,
            Axis
        }

        private Track[] _arrTrack = new Track[0];

        public Track[] arrTrack => _arrTrack;

        public void SetTrack(List<Track> lstTrack)
        {
            if (lstTrack != null && lstTrack.Count > 0)
            {
                _arrTrack = lstTrack.ToArray();
            }
        }

        public void SetActive(bool active)
        {
            Track[] array = _arrTrack;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].active = active;
            }
        }

        public void SetActive(bool active, int triggerId)
        {
            Track[] array = _arrTrack;
            foreach (Track track in array)
            {
                if (track.useTrigger && track.triggerId == triggerId)
                {
                    track.active = active;
                }
            }
        }

        public void SetAutoPlay(bool autoPlay)
        {
            Track[] array = _arrTrack;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].useAutoPlay = autoPlay;
            }
        }

        public void SetAutoPlay(bool autoPlay, int triggerId)
        {
            Track[] array = _arrTrack;
            foreach (Track track in array)
            {
                if (track.useTrigger && track.triggerId == triggerId)
                {
                    track.useAutoPlay = autoPlay;
                }
            }
        }

        public override eCategory GetCategory()
        {
            return eCategory.TransformAnimator;
        }

        protected override void OnInit()
        {
            Track[] array = _arrTrack;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Init();
            }
        }

        protected override void OnUpdate(GimmickController.UpdateInfo updateInfo)
        {
            if (updateInfo.pause)
            {
                return;
            }
            Track[] array = _arrTrack;
            foreach (Track track in array)
            {
                track.UpdateState(updateInfo.currentTime);
                if (track.state == Track.eState.Run)
                {
                    track.Animate(updateInfo.currentTime);
                }
            }
        }
    }

    /// <summary>
    /// 3Dモデルに適応するテクスチャを格納する
    /// </summary>
    public class MultiTextures
    {
        public enum eMap : uint
        {
            Diffuse = 0u,
            Specular = 1u,
            Control = 2u,
            AlphaMask = 3u,
            MAX = 4u,
            Invalid = 4u
        }

        private Texture2D[] _arrTexture = new Texture2D[4];

        public Texture2D[] arrTexture => _arrTexture;

        /// <summary>
        /// ディフューズ(拡散反射光)マップ用テクスチャ
        /// </summary>
        public Texture2D diffuse
        {
            get
            {
                return _arrTexture[0];
            }
            set
            {
                _arrTexture[0] = value;
            }
        }

        /// <summary>
        /// スペキュラ(鏡面反射)マップ用テクスチャ
        /// </summary>
        public Texture2D specular
        {
            get
            {
                return _arrTexture[1];
            }
            set
            {
                _arrTexture[1] = value;
            }
        }

        /// <summary>
        /// コントロールマップ用テクスチャ
        /// </summary>
        public Texture2D control
        {
            get
            {
                return _arrTexture[2];
            }
            set
            {
                _arrTexture[2] = value;
            }
        }

        /// <summary>
        /// アルファマスク用テクスチャ
        /// </summary>
        public Texture2D alphaMask
        {
            get
            {
                return _arrTexture[3];
            }
            set
            {
                _arrTexture[3] = value;
            }
        }

        public MultiTextures()
        {
        }

        public MultiTextures(Texture2D inDiffuse, Texture2D inControll, Texture2D inSpecular, Texture2D inAlphaMask = null)
        {
            diffuse = inDiffuse;
            control = inControll;
            specular = inSpecular;
            alphaMask = inAlphaMask;
        }

        public bool CheckTexture(eMap map)
        {
            if (!(_arrTexture[(uint)map] != null))
            {
                return false;
            }
            return true;
        }

        public Texture2D GetTexture(eMap map)
        {
            return _arrTexture[(uint)map];
        }

        public void SetTexture(eMap map, Texture2D texture)
        {
            _arrTexture[(uint)map] = texture;
        }

        public void SetTexture(MultiTextures multiTextures)
        {
            int num = 4;
            for (int i = 0; i < num; i++)
            {
                _arrTexture[i] = multiTextures._arrTexture[i];
            }
        }
    }

    /// <summary>
    /// キャラクタに対する各モデルパーツに適応するテクスチャを格納する
    /// </summary>
    public class TexturePack
    {
        public enum eCategory
        {
            Accessory = 1,
            Head = 2,
            Object = 3,
            Body = 4,
            Cheek = 10,
            MAX = 11,
            INVALID = 11
        }

        private MultiTextures[] _arrMultitextures = new MultiTextures[11];

        private Dictionary<int, MultiTextures> _mapMultiTextures = new Dictionary<int, MultiTextures>();

        public static int MakeKey(eCategory category, string partsCode = null)
        {
            int num = 0;
            if (Parts.IsValidPartsCode(partsCode))
            {
                num = int.Parse(partsCode);
            }
            return ((int)category << 16) | num;
        }

        public static eCategory ExtractCategory(int key)
        {
            return (eCategory)(key >> 16);
        }

        public static string ExtractPartsCode(int key)
        {
            string text = (key & 0xFFFF).ToString();
            if (!Parts.IsValidPartsCode(text))
            {
                return string.Empty;
            }
            return text;
        }

        public MultiTextures CreateMultiTextures(eCategory category, string partsCode = null)
        {
            MultiTextures multiTextures = Get(category, partsCode);
            if (multiTextures == null)
            {
                multiTextures = new MultiTextures();
                Set(category, partsCode, multiTextures);
            }
            return multiTextures;
        }

        public MultiTextures Get(eCategory category, string partsCode = null)
        {
            int key = MakeKey(category, partsCode);
            MultiTextures value = null;
            _mapMultiTextures.TryGetValue(key, out value);
            return value;
        }

        public void Set(eCategory category, MultiTextures multiTextures)
        {
            Set(category, null, multiTextures);
        }

        public void Set(eCategory category, string partsCode, MultiTextures multiTextures)
        {
            if (multiTextures != null)
            {
                int key = MakeKey(category, partsCode);
                if (_mapMultiTextures.ContainsKey(key))
                {
                    _mapMultiTextures.Remove(key);
                }
                _mapMultiTextures.Add(key, multiTextures);
            }
        }

        public void Clear()
        {
            _mapMultiTextures.Clear();
        }
    }

    /// <summary>
    /// キャラクタのマテリアルの管理をする
    /// </summary>
    public abstract class MaterialPack
    {
        public class UpdateInfo
        {
            private bool _pause;

            private Color _charaColor = Color.white;

            private bool _updatedFaceFlag;

            private int _faceFlag = -1;

            private bool _useVtxClrB;

            private float _lerpDiffuse;

            private float _lerpGradation = 1f;

            public bool pause
            {
                get
                {
                    return _pause;
                }
                set
                {
                    _pause = value;
                }
            }

            public Color charaColor
            {
                get
                {
                    return _charaColor;
                }
                set
                {
                    _charaColor = value;
                }
            }

            public bool updatedFaceFlag
            {
                get
                {
                    return _updatedFaceFlag;
                }
                set
                {
                    _updatedFaceFlag = value;
                }
            }

            public int faceFlag
            {
                get
                {
                    return _faceFlag;
                }
                set
                {
                    if (_faceFlag != value)
                    {
                        _faceFlag = value;
                        _updatedFaceFlag = true;
                    }
                    else
                    {
                        _updatedFaceFlag = false;
                    }
                }
            }

            public bool useVtxClrB
            {
                get
                {
                    return _useVtxClrB;
                }
                set
                {
                    _useVtxClrB = value;
                }
            }

            public float lerpDiffuse
            {
                get
                {
                    return _lerpDiffuse;
                }
                set
                {
                    _lerpDiffuse = value;
                }
            }

            public float lerpGradation
            {
                get
                {
                    return _lerpGradation;
                }
                set
                {
                    _lerpGradation = value;
                }
            }
        }

        public enum eMaterialCategory
        {
            Face = 0,
            Object = 1,
            Cheek = 2,
            Mayu = 3,
            Unknown = 4,
            MAX = 5,
            Invalid = 5
        }

        public enum eShaderCategory
        {
            CharaDefault = 0,
            CharaDefaultRich = 1,
            Animation = 2,
            Luminous = 3,
            Scroll = 4,
            Lerp = 5,
            Flake = 6,
            ColorBlend = 7,
            Distortion = 8,
            Unknown = 9,
            MAX = 10,
            Invalid = 10
        }

        public enum eShaderKeyword
        {
            DUMMY = 0,
            ENABLE_LUMINOUS = 1,
            TRANSPARENCY = 2,
            INVERSE_TRANSPARENCY = 3,
            MAX = 4,
            Invalid = 4
        }

        public enum eCapability
        {
            None,
            Outline
        }

        private static readonly Dictionary<int, string> _mapMaterialKeyword = new Dictionary<int, string>
        {
            { 1, "_obj" },
            { 2, "_cheek" },
            { 3, "_mayu" }
        };

        private static readonly Dictionary<int, string> _mapShaderName = new Dictionary<int, string>
        {
            { 0, "cygames/3dlive/chara/charadefault" },
            { 1, "cygames/3dlive/chara/charadefaultrich" },
            { 2, "cygames/3dlive/chara/charatexanim" },
            { 3, "cygames/3dlive/chara/charatexluminous" },
            { 4, "cygames/3dlive/chara/charatexscroll" },
            { 5, "cygames/3dlive/chara/charatexlerpdiffuse" },
            { 6, "cygames/3dlive/chara/charatexflake" },
            { 7, "cygames/3dlive/chara/charatexcolorblend" },
            { 8, "cygames/3dlive/chara/charatexdistortion" }
        };

        private static readonly string[] _arrShaderKeyword = new string[4] { "DUMMY", "ENABLE_LUMINOUS", "TRANSPARENCY", "INVERSE_TRANSPARENCY" };

        public static int INVALID_ID = -1;

        protected bool _active;

        protected bool _rich;

        protected eMaterialCategory _materialCategory = eMaterialCategory.Unknown;

        protected eShaderCategory _shaderCategory = eShaderCategory.Unknown;

        protected Renderer _targetRenderer;

        protected Material _targetMaterial;

        protected int _subMeshIndex;

        private MultiTextures _multiTextures = new MultiTextures();

        private bool[] _arrShaderKeywordSwitch = new bool[4];

        public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                if (_active)
                {
                    OnActive();
                }
            }
        }

        public bool rich => _rich;

        public eMaterialCategory materialCategory => _materialCategory;

        public eShaderCategory shaderCategory => _shaderCategory;

        public Renderer targetRenderer => _targetRenderer;

        public Material targetMaterial => _targetMaterial;

        public int subMeshIndex => _subMeshIndex;

        public string materialName
        {
            get
            {
                if (!(_targetMaterial != null))
                {
                    return string.Empty;
                }
                return _targetMaterial.name;
            }
        }

        public string shaderName
        {
            get
            {
                if (!(_targetMaterial != null))
                {
                    return string.Empty;
                }
                return _targetMaterial.shader.name;
            }
        }

        private static bool CheckRichMaterial(Material mtrl)
        {
            return mtrl.name.ToLower().Contains("_hq");
        }

        private static int FindCategory(string name, Dictionary<int, string> container, int defKey)
        {
            string text = name.ToLower();
            foreach (KeyValuePair<int, string> item in container)
            {
                if (text.Contains(item.Value))
                {
                    return item.Key;
                }
            }
            return defKey;
        }

        public static eMaterialCategory CheckMaterialCategory(Material mtrl)
        {
            eMaterialCategory defKey = eMaterialCategory.Unknown;
            return (eMaterialCategory)FindCategory(mtrl.name, _mapMaterialKeyword, (int)defKey);
        }

        public static eShaderCategory CheckShaderCategory(Material mtrl)
        {
            eShaderCategory defKey = eShaderCategory.Unknown;
            return (eShaderCategory)FindCategory(mtrl.shader.name, _mapShaderName, (int)defKey);
        }

        public static MaterialPack Create(Renderer renderer, Material mtrl, int subIndex, MultiTextures multiTextures)
        {
            MaterialPack materialPack = null;
            materialPack = CheckShaderCategory(mtrl) switch
            {
                eShaderCategory.Animation => new TextureAnimation(),
                eShaderCategory.Luminous => new TextureLuminous(),
                eShaderCategory.Scroll => new TextureScroll(),
                eShaderCategory.Lerp => new TextureLerpDiffuse(),
                eShaderCategory.Flake => new TextureFlake(),
                eShaderCategory.ColorBlend => new TextureColorBlend(),
                eShaderCategory.Distortion => new TextureDistortion(),
                _ => new StandardMaterial(),
            };
            if (materialPack != null)
            {
                materialPack = (materialPack.Init(renderer, mtrl, subIndex, multiTextures) ? materialPack : null);
            }
            return materialPack;
        }

        public static string GetShaderKeyword(eShaderKeyword keyword)
        {
            return _arrShaderKeyword[(int)keyword];
        }

        public bool CheckCapability(eCapability caps)
        {
            return (GetCapability() & caps) != 0;
        }

        public bool CheckShaderKeywordSwitch(eShaderKeyword keyword)
        {
            bool result = false;
            if (eShaderKeyword.DUMMY < keyword && keyword < eShaderKeyword.MAX)
            {
                result = _arrShaderKeywordSwitch[(int)keyword];
            }
            return result;
        }

        protected bool Init(Renderer renderer, Material material, int subMeshIndex, MultiTextures multiTextures)
        {
            _rich = CheckRichMaterial(material);
            _materialCategory = CheckMaterialCategory(material);
            _targetRenderer = renderer;
            _targetMaterial = material;
            _subMeshIndex = subMeshIndex;
            if (multiTextures != null)
            {
                SetTexture(multiTextures);
            }
            UpdateShaderKeywordSwitch();
            _active = OnInit();
            return _active;
        }

        public void Update(UpdateInfo updateInfo)
        {
            if (_active)
            {
                OnUpdate(updateInfo);
            }
        }

        public Texture2D GetTexture(MultiTextures.eMap map)
        {
            return _multiTextures.GetTexture(map);
        }

        public void SetTexture(MultiTextures.eMap map, Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }
            if (_multiTextures != null)
            {
                _multiTextures.SetTexture(map, texture);
            }
            switch (map)
            {
                case MultiTextures.eMap.Diffuse:
                    _targetMaterial.mainTexture = texture;
                    _targetMaterial.SetTexture("_MainTex", texture);
                    if (_materialCategory == eMaterialCategory.Object || _materialCategory == eMaterialCategory.Unknown)
                    {
                        _targetMaterial.SetTexture("_OutlineTex", texture);
                    }
                    break;
                case MultiTextures.eMap.Specular:
                    _targetMaterial.SetTexture("_SpecTex", texture);
                    break;
                case MultiTextures.eMap.Control:
                    _targetMaterial.SetTexture("_ControlMap", texture);
                    break;
                case MultiTextures.eMap.AlphaMask:
                    _targetMaterial.SetTexture("_MainTexAlpha", texture);
                    break;
            }
        }

        public void SetTexture(MultiTextures multiTextures)
        {
            for (MultiTextures.eMap eMap = MultiTextures.eMap.Diffuse; eMap < MultiTextures.eMap.MAX; eMap++)
            {
                SetTexture(eMap, multiTextures.GetTexture(eMap));
            }
        }

        public void SetShaderKeyword(eShaderKeyword keyword, bool sw)
        {
            string keyword2 = _arrShaderKeyword[(int)keyword];
            if (sw)
            {
                _targetMaterial.EnableKeyword(keyword2);
            }
            else
            {
                _targetMaterial.DisableKeyword(keyword2);
            }
            _arrShaderKeywordSwitch[(int)keyword] = sw;
        }

        public void UpdateShaderKeywordSwitch()
        {
            for (eShaderKeyword eShaderKeyword = eShaderKeyword.ENABLE_LUMINOUS; eShaderKeyword < eShaderKeyword.MAX; eShaderKeyword++)
            {
                string shaderKeyword = GetShaderKeyword(eShaderKeyword);
                bool flag = _targetMaterial.IsKeywordEnabled(shaderKeyword);
                _arrShaderKeywordSwitch[(int)eShaderKeyword] = flag;
            }
        }

        protected virtual bool OnInit()
        {
            return true;
        }

        protected virtual void OnUpdate(UpdateInfo updateInfo)
        {
        }

        public virtual void OnActive()
        {
        }

        public abstract eCapability GetCapability();
    }

    /// <summary>
    /// 通常のモデルへ適応されるテクスチャマテリアル
    /// </summary>
    public class StandardMaterial : MaterialPack
    {
        private static readonly string[] _arrShaderName = new string[3] { "Cygames/3DLive/Chara/CharaPetit", "Cygames/3DLive/Chara/CharaDefault", "Cygames/3DLive/Chara/CharaDefaultRich" };

        public override eCapability GetCapability()
        {
            eCapability result = eCapability.None;
            Shader shader = base.targetMaterial.shader;
            string[] arrShaderName = _arrShaderName;
            foreach (string strB in arrShaderName)
            {
                if (string.Compare(shader.name, strB, ignoreCase: true) == 0)
                {
                    result = eCapability.Outline;
                    break;
                }
            }
            return result;
        }
    }

    /// <summary>
    /// テクスチャアニメーション用のマテリアルを管理する
    /// </summary>
    public class TextureAnimation : MaterialPack
    {
        public enum eCondition
        {
            None,
            Facial
        }

        private const int INDEX_DEFAULT_TRACK = 0;

        private const int SAMPLE_COUNT = 10000;

        private readonly int _shdPropID_frameOffset = MaterialPack.INVALID_ID;

        private readonly int _shdPropID_frameOption = MaterialPack.INVALID_ID;

        private readonly int _shdPropID_luminousColor = MaterialPack.INVALID_ID;

        private Vector4 _frameInfo = Vector4.zero;

        private Vector2 _frameOffset = Vector2.zero;

        private float _curFrame;

        private float _chkTime = 1f;

        private float _accumulatedTime;

        private bool[] _luminousSwitch = new bool[0];

        private Vector4[] _arrTrack;

        private Vector4 _curTrackInfo = Vector4.zero;

        private bool _useTrack;

        private int _trackCount;

        private int _curTrackIdx;

        private Dictionary<int, Dictionary<int, int>> _mapCondition = new Dictionary<int, Dictionary<int, int>>();

        private bool _randomFrameSwitch;

        private bool _fixedFrameSwitch;

        private int _specificalFrame;

        private int _prevFrame;

        public TextureAnimation()
        {
            _shdPropID_frameOffset = Shader.PropertyToID("_frameOffset");
            _shdPropID_frameOption = Shader.PropertyToID("_frameOption");
            _shdPropID_luminousColor = Shader.PropertyToID("_texAnimLuminousColor");
        }

        private bool[] BuildLuminousSwitch(Material mtrl, ref Vector4 frameInfo)
        {
            int @int = mtrl.GetInt("luminousSwitch");
            bool[] array = new bool[(int)frameInfo.w];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (((@int & (1 << i)) != 0) ? true : false);
            }
            return array;
        }

        private void BuildTrackCondition()
        {
            for (int i = 0; i < _trackCount; i++)
            {
                Vector4 vector = _arrTrack[i];
                int key = (int)vector.z;
                Dictionary<int, int> value = null;
                if (!_mapCondition.TryGetValue(key, out value))
                {
                    value = new Dictionary<int, int>();
                    _mapCondition.Add(key, value);
                }
                int key2 = (int)vector.w;
                if (!value.ContainsKey(key2))
                {
                    value.Add(key2, i);
                }
            }
        }

        private void SetTrack(int idx, bool bForce = false)
        {
            if (_curTrackIdx != idx || bForce)
            {
                _curTrackIdx = idx;
                _curTrackInfo = _arrTrack[_curTrackIdx];
                _curFrame = _curTrackInfo.x;
            }
        }

        private void UpdateTrack(eCondition condition, int param)
        {
            Dictionary<int, int> value = null;
            if (_mapCondition.TryGetValue((int)condition, out value))
            {
                if (value.ContainsKey(param))
                {
                    SetTrack(value[param]);
                }
                else
                {
                    SetTrack(0);
                }
            }
            else
            {
                SetTrack(0);
            }
        }

        protected override bool OnInit()
        {
            _chkTime = 1f / _targetMaterial.GetFloat("frequency");
            _frameInfo = _targetMaterial.GetVector("frameInfo");
            _luminousSwitch = BuildLuminousSwitch(_targetMaterial, ref _frameInfo);
            _frameInfo.x /= _targetMaterial.mainTexture.width;
            _frameInfo.y /= _targetMaterial.mainTexture.height;
            int maxFrameIndex = (int)(_frameInfo.w - 1f);
            int num = (_targetMaterial.HasProperty("useTrack") ? _targetMaterial.GetInt("useTrack") : 0);
            _useTrack = num != 0;
            _trackCount = (_targetMaterial.HasProperty("trackCount") ? _targetMaterial.GetInt("trackCount") : 0);
            if (_useTrack && _trackCount > 0)
            {
                _arrTrack = new Vector4[_trackCount];
                Action<int> action = delegate (int idx)
                {
                    string name = $"track{idx}";
                    if (_targetMaterial.HasProperty(name) && idx < _arrTrack.Length)
                    {
                        Vector4 vector2 = _targetMaterial.GetVector(name);
                        vector2.x = Mathf.Clamp(vector2.x - 1f, 0f, maxFrameIndex);
                        vector2.y = Mathf.Clamp(vector2.y - 1f, 0f, maxFrameIndex);
                        _arrTrack[idx] = vector2;
                    }
                };
                for (int i = 0; i < _trackCount; i++)
                {
                    action(i);
                }
            }
            else
            {
                _trackCount = 1;
                _arrTrack = new Vector4[_trackCount];
                _arrTrack[0] = new Vector4(0f, maxFrameIndex, 0f, 0f);
            }
            BuildTrackCondition();
            SetTrack(0, bForce: true);
            if (!_useTrack && _targetMaterial.HasProperty(_shdPropID_frameOption))
            {
                Vector4 vector = _targetMaterial.GetVector(_shdPropID_frameOption);
                _randomFrameSwitch = vector.x != 0.0;
                _fixedFrameSwitch = vector.y != 0.0;
                if (_randomFrameSwitch)
                {
                    _specificalFrame = (int)Time.time % (int)_frameInfo.w;
                }
                else if (_fixedFrameSwitch)
                {
                    _specificalFrame = (int)vector.z - 1;
                    _specificalFrame = Mathf.Clamp(_specificalFrame, 0, maxFrameIndex);
                }
            }
            else
            {
                _randomFrameSwitch = false;
                _fixedFrameSwitch = false;
            }
            return true;
        }

        private void DefaultFrameCounter(UpdateInfo updateInfo)
        {
            if (updateInfo.updatedFaceFlag)
            {
                UpdateTrack(eCondition.Facial, updateInfo.faceFlag);
            }
            if (_accumulatedTime >= _chkTime)
            {
                _curFrame += 1f;
                if (_curFrame > _curTrackInfo.y)
                {
                    _curFrame = _curTrackInfo.x;
                }
                _accumulatedTime = 0f;
            }
        }

        private bool SpecificalFrameCounter()
        {
            bool flag = false;
            if (_accumulatedTime >= _chkTime)
            {
                if (!_fixedFrameSwitch && _randomFrameSwitch)
                {
                    _specificalFrame = UnityEngine.Random.Range(0, 10000) % (int)_frameInfo.w;
                    if (_prevFrame == _specificalFrame)
                    {
                        _specificalFrame = (_specificalFrame + 1) % (int)_frameInfo.w;
                    }
                    _prevFrame = _specificalFrame;
                    flag = true;
                }
                else if (_fixedFrameSwitch)
                {
                    flag = true;
                }
                if (flag)
                {
                    _curFrame = _specificalFrame;
                    _accumulatedTime = 0f;
                }
            }
            return flag;
        }

        protected override void OnUpdate(UpdateInfo updateInfo)
        {
            if (_targetMaterial != null)
            {
                if (!updateInfo.pause)
                {
                    _accumulatedTime += Time.deltaTime;
                }
                if (!SpecificalFrameCounter())
                {
                    DefaultFrameCounter(updateInfo);
                }
                float num = Mathf.Floor(_curFrame % _frameInfo.z);
                float num2 = Mathf.Floor((_curFrame - num) / _frameInfo.z);
                _frameOffset.x = num * _frameInfo.x;
                _frameOffset.y = num2 * _frameInfo.y;
                _targetMaterial.SetVector(_shdPropID_frameOffset, _frameOffset);
                Color color = (_luminousSwitch[(int)_curFrame] ? Color.white : updateInfo.charaColor);
                _targetMaterial.SetVector(_shdPropID_luminousColor, color);
            }
        }

        public override eCapability GetCapability()
        {
            return eCapability.Outline;
        }
    }

    /// <summary>
    /// カラーブレンドを使用するテクスチャ用のマテリアルを管理する
    /// </summary>
    public class TextureColorBlend : MaterialPack
    {
        public enum eBlendMode
        {
            Additive,
            Multiply
        }

        private int _shdPropID_blendInfo = MaterialPack.INVALID_ID;

        private eBlendMode _blendMode;

        private AnimationCurve _clrCurve;

        private AnimationCurve _lumCurve;

        private Color _clrStart = Color.white;

        private Color _clrEnd = Color.white;

        private float _clrSec = 1f;

        private float _lumSec = 1f;

        private float _clrProgress;

        private float _lumProgress;

        private void LoadShaderProperty()
        {
            _targetMaterial.DisableKeyword("ADDITIVE");
            _targetMaterial.DisableKeyword("MULTIPLY");
            if (_targetMaterial.HasProperty("_blendMode"))
            {
                _blendMode = (eBlendMode)_targetMaterial.GetInt("_blendMode");
                switch (_blendMode)
                {
                    case eBlendMode.Additive:
                        _targetMaterial.EnableKeyword("ADDITIVE");
                        break;
                    case eBlendMode.Multiply:
                        _targetMaterial.EnableKeyword("MULTIPLY");
                        break;
                }
            }
            if (_targetMaterial.HasProperty("_clrStart"))
            {
                _clrStart = _targetMaterial.GetColor("_clrStart");
            }
            if (_targetMaterial.HasProperty("_clrEnd"))
            {
                _clrEnd = _targetMaterial.GetColor("_clrEnd");
            }
            float b = 0.0166666675f;
            if (_targetMaterial.HasProperty("_lumSec"))
            {
                _lumSec = _targetMaterial.GetFloat("_lumSec");
                _lumSec = Mathf.Max(_lumSec, b);
            }
            if (_targetMaterial.HasProperty("_clrSec"))
            {
                _clrSec = _targetMaterial.GetFloat("_clrSec");
                _clrSec = Mathf.Max(_clrSec, b);
            }
            Func<string, AnimationCurve> func = delegate (string prefix)
            {
                AnimationCurve animationCurve = new AnimationCurve();
                string name = $"{prefix}CurveCnt";
                if (!_targetMaterial.HasProperty(name))
                {
                    return animationCurve;
                }
                int @int = _targetMaterial.GetInt(name);
                for (int i = 0; i < @int; i++)
                {
                    string name2 = $"{prefix}CurvePt{i + 1}";
                    if (_targetMaterial.HasProperty(name2))
                    {
                        Vector4 vector = _targetMaterial.GetVector(name2);
                        animationCurve.AddKey(new Keyframe(vector.x, vector.y, vector.z, vector.w));
                    }
                }
                return animationCurve;
            };
            _clrCurve = func("_clr");
            _lumCurve = func("_lum");
        }

        protected override bool OnInit()
        {
            _shdPropID_blendInfo = Shader.PropertyToID("_blendInfo");
            LoadShaderProperty();
            return true;
        }

        protected override void OnUpdate(UpdateInfo updateInfo)
        {
            if (!(_targetMaterial != null))
            {
                return;
            }
            float num = Time.deltaTime / _lumSec;
            float num2 = Time.deltaTime / _clrSec;
            if (!updateInfo.pause)
            {
                _lumProgress += num;
                if (_lumProgress > 1f)
                {
                    _lumProgress %= 1f;
                }
                _clrProgress += num2;
                if (_clrProgress > 1f)
                {
                    _clrProgress %= 1f;
                }
            }
            Color color = Color.Lerp(_clrStart, _clrEnd, _clrCurve.Evaluate(_clrProgress));
            switch (_blendMode)
            {
                case eBlendMode.Additive:
                    color = Color.Lerp(Color.black, color, color.a);
                    break;
                case eBlendMode.Multiply:
                    color = Color.Lerp(Color.white, color, color.a);
                    break;
            }
            color.a = _lumCurve.Evaluate(_lumProgress);
            _targetMaterial.SetColor(_shdPropID_blendInfo, color);
        }

        public override eCapability GetCapability()
        {
            return eCapability.Outline;
        }
    }

    /// <summary>
    /// 歪み（ディストーション）テクスチャ用のマテリアルを管理する
    /// </summary>
    public class TextureDistortion : MaterialPack
    {
        public enum eBlendMode
        {
            Additive,
            SoftAdditive
        }

        private int _propId_colorR = MaterialPack.INVALID_ID;

        private int _propId_colorG = MaterialPack.INVALID_ID;

        private int _propId_colorB = MaterialPack.INVALID_ID;

        private int _propId_layerR = MaterialPack.INVALID_ID;

        private int _propId_layerG = MaterialPack.INVALID_ID;

        private int _propId_layerB = MaterialPack.INVALID_ID;

        private int _propId_matLayerInfo = MaterialPack.INVALID_ID;

        private int _propId_blendMode = MaterialPack.INVALID_ID;

        private int _propId_luminousSwitch = MaterialPack.INVALID_ID;

        private int _propId_luminousPower = MaterialPack.INVALID_ID;

        private int _propId_luminousColor = MaterialPack.INVALID_ID;

        private Color _colorR = Color.white;

        private Color _colorG = Color.white;

        private Color _colorB = Color.white;

        private Vector4 _layerR = Vector4.zero;

        private Vector4 _layerG = Vector4.zero;

        private Vector4 _layerB = Vector4.zero;

        private Matrix4x4 _matLayerInfo = Matrix4x4.zero;

        private float _elapsedTime;

        private eBlendMode _blendMode;

        private bool _luminousSwitch;

        private Color _luminousPower = Color.clear;

        private Color _luminousColor = Color.white;

        private void LoadShaderProperty()
        {
            _colorR = _targetMaterial.GetColor(_propId_colorR);
            _colorG = _targetMaterial.GetColor(_propId_colorG);
            _colorB = _targetMaterial.GetColor(_propId_colorB);
            _layerR = _targetMaterial.GetVector(_propId_layerR);
            _layerG = _targetMaterial.GetVector(_propId_layerG);
            _layerB = _targetMaterial.GetVector(_propId_layerB);
            _colorR *= _colorR.a;
            _colorG *= _colorG.a;
            _colorB *= _colorB.a;
            if (0f < _colorG.a)
            {
                _targetMaterial.EnableKeyword("USE_LAYER_G");
            }
            else
            {
                _targetMaterial.DisableKeyword("USE_LAYER_G");
            }
            if (0f < _colorB.a)
            {
                _targetMaterial.EnableKeyword("USE_LAYER_B");
            }
            else
            {
                _targetMaterial.DisableKeyword("USE_LAYER_B");
            }
            _blendMode = (eBlendMode)_targetMaterial.GetFloat(_propId_blendMode);
            _targetMaterial.DisableKeyword("ADDITIVE");
            _targetMaterial.DisableKeyword("SOFT_ADDITIVE");
            _targetMaterial.DisableKeyword("WEIGHT_ADDITIVE");
            _targetMaterial.DisableKeyword("WEIGHT_MULTIPLY");
            switch (_blendMode)
            {
                case eBlendMode.Additive:
                    _targetMaterial.EnableKeyword("ADDITIVE");
                    break;
                case eBlendMode.SoftAdditive:
                    _targetMaterial.EnableKeyword("SOFT_ADDITIVE");
                    break;
            }
            if (_targetMaterial.HasProperty(_propId_luminousSwitch))
            {
                _luminousSwitch = _targetMaterial.GetInt(_propId_luminousSwitch) == 1;
            }
            else
            {
                _luminousSwitch = false;
            }
            if (_targetMaterial.HasProperty(_propId_luminousPower))
            {
                _luminousPower = _targetMaterial.GetColor(_propId_luminousPower);
            }
            else if (_luminousSwitch)
            {
                _luminousPower = new Color(1f, 1f, 1f, 1f);
            }
        }

        protected override bool OnInit()
        {
            _propId_colorR = Shader.PropertyToID("_colorR");
            _propId_colorG = Shader.PropertyToID("_colorG");
            _propId_colorB = Shader.PropertyToID("_colorB");
            _propId_layerR = Shader.PropertyToID("_layerR");
            _propId_layerG = Shader.PropertyToID("_layerG");
            _propId_layerB = Shader.PropertyToID("_layerB");
            _propId_matLayerInfo = Shader.PropertyToID("_matLayerInfo");
            _propId_blendMode = Shader.PropertyToID("_blendMode");
            _propId_luminousSwitch = Shader.PropertyToID("_luminousSwitch");
            _propId_luminousPower = Shader.PropertyToID("_luminousPower");
            _propId_luminousColor = Shader.PropertyToID("_luminousColor");
            LoadShaderProperty();
            return true;
        }

        protected override void OnUpdate(UpdateInfo updateInfo)
        {
            if (_targetMaterial != null)
            {
                if (!updateInfo.pause)
                {
                    _elapsedTime += Time.deltaTime;
                }
                if (_luminousSwitch && CheckShaderKeywordSwitch(eShaderKeyword.ENABLE_LUMINOUS))
                {
                    _matLayerInfo.SetRow(0, _colorR * Color.Lerp(updateInfo.charaColor, Color.white, _luminousPower.r));
                    _matLayerInfo.SetRow(1, _colorG * Color.Lerp(updateInfo.charaColor, Color.white, _luminousPower.g));
                    _matLayerInfo.SetRow(2, _colorB * Color.Lerp(updateInfo.charaColor, Color.white, _luminousPower.b));
                    _luminousColor = Color.Lerp(updateInfo.charaColor, Color.white, _luminousPower.a);
                }
                else
                {
                    _matLayerInfo.SetRow(0, _colorR * updateInfo.charaColor);
                    _matLayerInfo.SetRow(1, _colorG * updateInfo.charaColor);
                    _matLayerInfo.SetRow(2, _colorB * updateInfo.charaColor);
                    _luminousColor = updateInfo.charaColor;
                }
                _matLayerInfo[0, 3] = _layerR.x * _elapsedTime;
                _matLayerInfo[3, 0] = _layerR.y * _elapsedTime;
                _matLayerInfo[1, 3] = _layerG.x * _elapsedTime;
                _matLayerInfo[3, 1] = _layerG.y * _elapsedTime;
                _matLayerInfo[2, 3] = _layerG.x * _elapsedTime;
                _matLayerInfo[3, 2] = _layerB.y * _elapsedTime;
                _targetMaterial.SetMatrix(_propId_matLayerInfo, _matLayerInfo);
                _targetMaterial.SetColor(_propId_luminousColor, _luminousColor);
            }
        }

        public override eCapability GetCapability()
        {
            return eCapability.Outline;
        }
    }

    /// <summary>
    /// 破片(フレーク)テクスチャ用のマテリアルを管理する
    /// </summary>
    public class TextureFlake : MaterialPack
    {
        private int _shdPropID_color = MaterialPack.INVALID_ID;

        private Color _prevFlakeColor = Color.white;

        private float _prevIntensity;

        private Color _flakeColor = Color.white;

        private float _flakeIntensity = 1f;

        private Color _calcedColor = Color.white;

        private void LoadShaderProperty()
        {
            _flakeColor = _targetMaterial.GetColor("_flakeColor");
            _flakeIntensity = _targetMaterial.GetFloat("_flakeIntensity");
        }

        protected override bool OnInit()
        {
            _shdPropID_color = Shader.PropertyToID("_calcedColor");
            LoadShaderProperty();
            return true;
        }

        protected override void OnUpdate(UpdateInfo updateInfo)
        {
            if (_targetMaterial != null && !updateInfo.pause && (_prevIntensity != _flakeIntensity || _prevFlakeColor != _flakeColor))
            {
                _calcedColor.r = _flakeColor.r * _flakeIntensity;
                _calcedColor.g = _flakeColor.g * _flakeIntensity;
                _calcedColor.b = _flakeColor.b * _flakeIntensity;
                _calcedColor.a = _flakeColor.a;
                _targetMaterial.SetColor(_shdPropID_color, _calcedColor);
            }
        }

        public override eCapability GetCapability()
        {
            return eCapability.Outline;
        }
    }

    /// <summary>
    /// 補間拡散光を使用するテクスチャ用のマテリアルを管理する
    /// </summary>
    public class TextureLerpDiffuse : MaterialPack
    {
        private const string USE_VERTEX_COLOR_B = "USE_VERTEX_COLOR_B";

        private int _shdPropID_lerpFactor = MaterialPack.INVALID_ID;

        private int _shdPropID_gradationFactor = MaterialPack.INVALID_ID;

        private bool _useVtxClrB = true;

        private float _lerpFactor;

        private float _gradationFactor;

        private float _accumulatedTime;

        private void LoadShaderProperty()
        {
            _useVtxClrB = false;
            if (_targetMaterial.HasProperty("_vertexColorChB"))
            {
                _useVtxClrB = _targetMaterial.GetInt("_vertexColorChB") != 0;
            }
            if (_useVtxClrB)
            {
                _targetMaterial.EnableKeyword("USE_VERTEX_COLOR_B");
            }
            else
            {
                _targetMaterial.DisableKeyword("USE_VERTEX_COLOR_B");
            }
        }

        protected override bool OnInit()
        {
            _shdPropID_lerpFactor = Shader.PropertyToID("_lerpFactor");
            _shdPropID_gradationFactor = Shader.PropertyToID("_gradationFactor");
            LoadShaderProperty();
            return true;
        }

        protected override void OnUpdate(UpdateInfo updateInfo)
        {
            if (!(_targetMaterial != null))
            {
                return;
            }
            if (!updateInfo.pause)
            {
                _accumulatedTime += Time.deltaTime;
            }
            if (_lerpFactor != updateInfo.lerpDiffuse)
            {
                _lerpFactor = updateInfo.lerpDiffuse;
                _targetMaterial.SetFloat(_shdPropID_lerpFactor, _lerpFactor);
            }
            if (_gradationFactor != updateInfo.lerpGradation)
            {
                _gradationFactor = updateInfo.lerpGradation;
                _targetMaterial.SetFloat(_shdPropID_gradationFactor, _gradationFactor);
            }
            if (_useVtxClrB != updateInfo.useVtxClrB)
            {
                _useVtxClrB = updateInfo.useVtxClrB;
                if (_useVtxClrB)
                {
                    _targetMaterial.EnableKeyword("USE_VERTEX_COLOR_B");
                }
                else
                {
                    _targetMaterial.DisableKeyword("USE_VERTEX_COLOR_B");
                }
            }
        }

        public override eCapability GetCapability()
        {
            return eCapability.Outline;
        }
    }

    /// <summary>
    /// 発光するテクスチャ用のマテリアルを管理する
    /// </summary>
    public class TextureLuminous : MaterialPack
    {
        public enum eScroll
        {
            SrcY,
            SrcX
        }

        private int _shdPropID_ScrPos = MaterialPack.INVALID_ID;

        private Vector2 _range = Vector2.zero;

        private float _accumulatedTime;

        private float _lumScrSpd = 1f;

        private bool _lumScrRev;

        private AnimationCurve _lumScrCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

        private void LoadShaderProperty()
        {
            if (_targetMaterial.GetInt("_lumScroll") == 1)
            {
                _targetMaterial.EnableKeyword("SCROLL_X");
            }
            else
            {
                _targetMaterial.DisableKeyword("SCROLL_X");
            }
            _lumScrRev = _targetMaterial.GetInt("_lumScrRev") == 1;
            _lumScrSpd = _targetMaterial.GetFloat("_lumScrSpd");
            Action<string> action = delegate (string propName)
            {
                Vector4 vector = _targetMaterial.GetVector(propName);
                _lumScrCurve.AddKey(new Keyframe(vector.x, vector.y, vector.z, vector.w));
            };
            _lumScrCurve = new AnimationCurve();
            int @int = _targetMaterial.GetInt("_lumCurveCtrlPtCnt");
            for (int i = 0; i < @int; i++)
            {
                string obj = $"_lumCurveCtrlPt{i + 1}";
                action(obj);
            }
            _range.x = _lumScrCurve.keys[0].time;
            _range.y = _lumScrCurve.keys[@int - 1].time;
        }

        protected override bool OnInit()
        {
            _shdPropID_ScrPos = Shader.PropertyToID("_lumScrPos");
            LoadShaderProperty();
            return true;
        }

        protected override void OnUpdate(UpdateInfo updateInfo)
        {
            if (!updateInfo.pause)
            {
                if (!_lumScrRev)
                {
                    if (_accumulatedTime >= _range.y)
                    {
                        _accumulatedTime = _range.x;
                    }
                    _accumulatedTime += Time.deltaTime * _lumScrSpd;
                }
                else
                {
                    if (_range.x > _accumulatedTime)
                    {
                        _accumulatedTime = _range.y;
                    }
                    _accumulatedTime -= Time.deltaTime * _lumScrSpd;
                }
            }
            float num = _lumScrCurve.Evaluate(_accumulatedTime);
            _targetMaterial.SetFloat(_shdPropID_ScrPos, 1f - num);
        }

        public override eCapability GetCapability()
        {
            return eCapability.Outline;
        }
    }

    /// <summary>
    /// スクロールテクスチャを使用するマテリアルを管理する
    /// </summary>
    public class TextureScroll : MaterialPack
    {
        public enum eDirection
        {
            X,
            Y
        }

        public enum eAppendLayer
        {
            None,
            R,
            RG,
            RGB
        }

        public const float DEF_TEXTURE_SIZE = 1024f;

        private readonly int _shdPropID_scroll = MaterialPack.INVALID_ID;

        private readonly int _shdPropID_pendulum = MaterialPack.INVALID_ID;

        private readonly int _shdPropID_power = MaterialPack.INVALID_ID;

        private bool _isRich;

        private eDirection _direction;

        private eAppendLayer _appendLayer;

        private Vector4 _scrollInfo = Vector4.zero;

        private Vector4 _pendulumInfo = Vector4.zero;

        private Vector4 _powerInfo = Vector4.one;

        private Vector4 _scroll = Vector4.zero;

        private Vector4 _pendulum = Vector4.zero;

        private Vector4 _scrollCheckTime = Vector4.one;

        private Vector4 _pendulumCheckTime = Vector4.one;

        private Vector4 _scrollElapsedTime = Vector4.zero;

        private Vector4 _pendulumElapsedTime = Vector4.zero;

        private Vector4 _pendulumElapsedAngle = Vector4.zero;

        public TextureScroll()
        {
            _shdPropID_scroll = Shader.PropertyToID("_scroll");
            _shdPropID_pendulum = Shader.PropertyToID("_pendulum");
            _shdPropID_power = Shader.PropertyToID("_power");
        }

        private void CalcCheckTime(ref Vector4 chkTime, ref Vector4 speed)
        {
            chkTime.x = 1f / speed.x;
            chkTime.y = 1f / speed.y;
            chkTime.w = 1f / speed.z;
            chkTime.z = 1f / speed.w;
        }

        private void LoadShaderProperty()
        {
            Vector4 speed = _targetMaterial.GetVector("scrollSpeed");
            Vector4 speed2 = _targetMaterial.GetVector("pendulumSpeed");
            CalcCheckTime(ref _scrollCheckTime, ref speed);
            CalcCheckTime(ref _pendulumCheckTime, ref speed2);
            _scrollInfo = _targetMaterial.GetVector("scrollInfo");
            _pendulumInfo = _targetMaterial.GetVector("pendulumInfo");
            _powerInfo = _targetMaterial.GetVector("powerInfo");
            _targetMaterial.SetVector(_shdPropID_power, _powerInfo);
            float num = 1024f;
            if (_targetMaterial.HasProperty("_textureSize"))
            {
                num = _targetMaterial.GetFloat("_textureSize");
            }
            _isRich = _targetMaterial.GetInt("_isRich") != 0;
            _direction = (eDirection)_targetMaterial.GetInt("direction");
            _targetMaterial.DisableKeyword("SCROLL_X");
            _targetMaterial.DisableKeyword("SCROLL_Y");
            float num2 = 1f / num;
            _scrollInfo *= num2;
            _pendulumInfo *= num2;
            switch (_direction)
            {
                case eDirection.X:
                    _targetMaterial.EnableKeyword("SCROLL_X");
                    break;
                case eDirection.Y:
                    _targetMaterial.EnableKeyword("SCROLL_Y");
                    break;
            }
            _appendLayer = (eAppendLayer)_targetMaterial.GetInt("appendLayer");
            _targetMaterial.DisableKeyword("ENABLE_LAYER_R");
            _targetMaterial.DisableKeyword("ENABLE_LAYER_RG");
            _targetMaterial.DisableKeyword("ENABLE_LAYER_RGB");
            if (_isRich)
            {
                switch (_appendLayer)
                {
                    case eAppendLayer.R:
                        _targetMaterial.EnableKeyword("ENABLE_LAYER_R");
                        break;
                    case eAppendLayer.RG:
                        _targetMaterial.EnableKeyword("ENABLE_LAYER_RG");
                        break;
                    case eAppendLayer.RGB:
                        _targetMaterial.EnableKeyword("ENABLE_LAYER_RGB");
                        break;
                }
            }
        }

        protected override bool OnInit()
        {
            LoadShaderProperty();
            _targetMaterial.SetVector(_shdPropID_scroll, Vector4.zero);
            _targetMaterial.SetVector(_shdPropID_pendulum, Vector4.zero);
            _targetMaterial.SetVector(_shdPropID_power, Vector4.one);
            return true;
        }

        protected override void OnUpdate(UpdateInfo updateInfo)
        {
            if (!(_targetMaterial != null))
            {
                return;
            }
            float deltaTime = Time.deltaTime;
            if (!updateInfo.pause)
            {
                _scrollElapsedTime.x += deltaTime;
                _scrollElapsedTime.y += deltaTime;
                _scrollElapsedTime.z += deltaTime;
                _scrollElapsedTime.w += deltaTime;
                _pendulumElapsedTime.x += deltaTime;
                _pendulumElapsedTime.y += deltaTime;
                _pendulumElapsedTime.z += deltaTime;
                _pendulumElapsedTime.w += deltaTime;
            }
            bool flag = false;
            if (_scrollElapsedTime.x >= _scrollCheckTime.x)
            {
                _scroll.x += _scrollInfo.x;
                _scroll.x %= 1f;
                _scrollElapsedTime.x = 0f;
                flag = true;
            }
            if (_scrollElapsedTime.y >= _scrollCheckTime.y)
            {
                _scroll.y += _scrollInfo.y;
                _scroll.y %= 1f;
                _scrollElapsedTime.y = 0f;
                flag = true;
            }
            if (_scrollElapsedTime.z >= _scrollCheckTime.z)
            {
                _scroll.z += _scrollInfo.z;
                _scroll.z %= 1f;
                _scrollElapsedTime.z = 0f;
                flag = true;
            }
            if (_scrollElapsedTime.w >= _scrollCheckTime.w)
            {
                _scroll.w += _scrollInfo.w;
                _scroll.w %= 1f;
                _scrollElapsedTime.w = 0f;
                flag = true;
            }
            if (flag)
            {
                _targetMaterial.SetVector(_shdPropID_scroll, _scroll);
                flag = false;
            }
            if (_isRich)
            {
                if (_pendulumElapsedTime.x >= _pendulumCheckTime.x)
                {
                    _pendulum.x = _pendulumInfo.x * Mathf.Cos(_pendulumElapsedAngle.x);
                    _pendulumElapsedAngle.x += deltaTime;
                    _pendulumElapsedTime.x = 0f;
                    flag = true;
                }
                if (_pendulumElapsedTime.y >= _pendulumCheckTime.y)
                {
                    _pendulum.y = _pendulumInfo.y * Mathf.Cos(_pendulumElapsedAngle.y);
                    _pendulumElapsedAngle.y += deltaTime;
                    _pendulumElapsedTime.y = 0f;
                    flag = true;
                }
                if (_pendulumElapsedTime.z >= _pendulumCheckTime.z)
                {
                    _pendulum.z = _pendulumInfo.z * Mathf.Cos(_pendulumElapsedAngle.z);
                    _pendulumElapsedAngle.z += deltaTime;
                    _pendulumElapsedTime.z = 0f;
                    flag = true;
                }
                if (_pendulumElapsedTime.w >= _pendulumCheckTime.w)
                {
                    _pendulum.w = _pendulumInfo.w * Mathf.Cos(_pendulumElapsedAngle.w);
                    _pendulumElapsedAngle.w += deltaTime;
                    _pendulumElapsedTime.w = 0f;
                    flag = true;
                }
                if (flag)
                {
                    _targetMaterial.SetVector(_shdPropID_pendulum, _pendulum);
                }
            }
        }

        public override eCapability GetCapability()
        {
            return eCapability.Outline;
        }
    }

    /// <summary>
    /// アクセサリ用の3Dモデルを管理する
    /// </summary>
    public class AccessoryParts : Parts
    {
        public enum eType
        {
            Main,
            Sub,
            MAX
        }

        public enum eTransform
        {
            M_Acc
        }

        public const int SORTING_ORDER = -100;

        private eType _type;

        private string _attachmentNode = string.Empty;

        public eType type => _type;

        public string attachmentNode
        {
            get
            {
                return _attachmentNode;
            }
            set
            {
                _attachmentNode = value;
            }
        }

        public AccessoryParts(GameObject gameObject, string partsCode, int index, TexturePack texturePack)
            : base(gameObject, partsCode, index, texturePack)
        {
            _category = eCategory.Accessory;
        }

        public void Initialize(eType type, AccParam.Info info)
        {
            if (info != null)
            {
                Quaternion localRotation = _transform.localRotation;
                switch (type)
                {
                    case eType.Main:
                        _attachmentNode = info.attachMain;
                        _transform.localPosition = info.offsetMain;
                        _transform.localScale = info.scaleMain;
                        localRotation.eulerAngles = info.rotateMain;
                        _transform.localRotation = localRotation;
                        break;
                    case eType.Sub:
                        _attachmentNode = info.attachSub;
                        _transform.localPosition = info.offsetSub;
                        _transform.localScale = info.scaleSub;
                        localRotation.eulerAngles = info.rotateSub;
                        _transform.localRotation = localRotation;
                        break;
                }
            }
            _type = type;
            _initialized = true;
        }

        public override int GetSortingOrder()
        {
            return -100;
        }

        protected override void OnCreate()
        {
            _transformCollector.Collect();
            _transformCollector.Build<eTransform>();
            _renderer = _transform.GetComponentInChildren<Renderer>();
            CreateMaterialPack(_renderer);
        }

        protected override MultiTextures OnGetMultiTextures(Material mtrl)
        {
            MultiTextures result = null;
            if (_texturePack != null)
            {
                result = _texturePack.Get(TexturePack.eCategory.Accessory);
            }
            return result;
        }
    }

    /// <summary>
    /// 胴体用の3Dモデルを管理する
    /// </summary>
    public class BodyParts : Parts
    {
        public enum eTransform
        {
            M_Body,
            Root,
            Position,
            Head,
            Neck,
            Hand_Attach_L,
            Hand_Attach_R,
            Position_Attach_00,
            Position_Attach_01,
            Wrist_L,
            Wrist_R,
            Hip,
            Chest,
            Waist,
            Knee_L,
            Ankle_R,
            Ankle_L,
            Pole_Leg_L,
            Pole_Leg_R,
            Pole_Arm_L,
            Pole_Arm_R,
            Eff_Leg_L,
            Eff_Leg_R,
            Eff_Wrist_L,
            Eff_Wrist_R
        }

        public const int SORTING_ORDER = -100;

        public const float BOUNDS_CENTER_Y = -0.1f;

        public const float BOUNDS_HEIGHT_RATE = 0.575f;

        private Transform[] _arrTransformCache;

        private Animation _animation;

        private AnimationState _animationState;

        public Animation animation => _animation;

        public AnimationState animationState
        {
            get
            {
                return _animationState;
            }
            set
            {
                _animationState = value;
            }
        }

        public BodyParts(GameObject gameObject, string partsCode, int index, TexturePack texturePack)
            : base(gameObject, partsCode, index, texturePack)
        {
            _category = eCategory.Body;
        }

        public Transform GetTransform(eTransform index)
        {
            return _arrTransformCache[(int)index];
        }

        public void RemoveAnimator()
        {
            if (_animator != null)
            {
                UnityEngine.Object.Destroy(_animator);
            }
            _animator = null;
        }

        protected void CreateAttachObject(Transform parentTransform, eTransform createPos)
        {
            int num = (int)createPos;
            if (!(_arrTransformCache[num] != null))
            {
                GameObject gameObject = new GameObject(createPos.ToString());
                gameObject.transform.parent = parentTransform;
                _arrTransformCache[num] = gameObject.transform;
                GameObjectUtility.ResetTransformLocalParam(_arrTransformCache[num]);
            }
        }

        public override int GetSortingOrder()
        {
            return -100;
        }

        protected override void OnCreate()
        {
            _transformCollector.Collect();
            _arrTransformCache = _transformCollector.Build<eTransform>();
            Transform parentTransform = GetTransform(eTransform.Position);
            CreateAttachObject(parentTransform, eTransform.Position_Attach_00);
            CreateAttachObject(parentTransform, eTransform.Position_Attach_01);
            _renderer = GetTransform(eTransform.M_Body).GetComponent<Renderer>();
            _renderer.sortingOrder = GetSortingOrder();
            CreateMaterialPack(_renderer);
            _animation = _renderer.GetComponent<Animation>();
            if (_animation == null)
            {
                _animation = _gameObject.AddComponent<Animation>();
            }
            _initialized = true;
        }

        protected override MultiTextures OnGetMultiTextures(Material mtrl)
        {
            MultiTextures result = null;
            if (_texturePack != null)
            {
                string text = _partsCode;
                if (!Parts.IsValidPartsCode(text))
                {
                    text = Parts.ExtractPartsCode(mtrl.name);
                }
                result = _texturePack.Get(TexturePack.eCategory.Body, text);
            }
            return result;
        }
    }

    /// <summary>
    /// 頭部用の3Dモデルを管理する
    /// </summary>
    public class HeadParts : Parts
    {
        public enum eTransform
        {
            M_Head,
            M_Cheek,
            Head,
            Neck,
            Head_Height,
            Head_Attach,
            Eye_locator_L,
            Eye_locator_R,
            Eye_L,
            Eye_R,
            Eye_range_L_01,
            Eye_range_L_02
        }

        public const int SORTING_ORDER = -110;

        private const int RENDER_QUEUE_OFFSET = -2;

        private const int MAYU_RENDER_QUEUE_OFFSET = -1;

        protected Renderer _cheekRenderer;

        protected Transform[] _arrTransformCache;

        private Character3DBase_CheekRenderer _cheekRendererComponent;

        public Renderer cheekRenderer => _cheekRenderer;

        public HeadParts(GameObject gameObject, string partsCode, int index, TexturePack texturePack)
            : base(gameObject, partsCode, index, texturePack)
        {
            _category = eCategory.Head;
        }

        public Transform GetTransform(eTransform index)
        {
            return _arrTransformCache[(int)index];
        }

        private void SetupCheekRenderer()
        {
            _cheekRendererComponent = _renderer.gameObject.AddComponent<Character3DBase_CheekRenderer>();
            _cheekRendererComponent.Initialize(_cheekRenderer);
        }

        public void EnableCheekLOD(bool enable)
        {
            if (_cheekRendererComponent != null)
            {
                _cheekRendererComponent.EnableLOD = enable;
            }
        }

        public void SetDistanceForCheekLOD(float distance)
        {
            if (_cheekRendererComponent != null)
            {
                _cheekRendererComponent.DistanceForLOD = distance;
            }
        }

        public override int GetSortingOrder()
        {
            return -110;
        }

        public override void SetRenderQueue(eRenderQueue renderQueue, int nOffset)
        {
            int num = (int)(renderQueue + nOffset);
            foreach (KeyValuePair<Material, MaterialPack> item in _mapMaterialPack)
            {
                eRenderingOrder eRenderingOrder = eRenderingOrder.Default;
                MaterialPack value = item.Value;
                Material targetMaterial = value.targetMaterial;
                if (targetMaterial.HasProperty(MTRL_PROP_RENDERING_ORDER))
                {
                    eRenderingOrder = (eRenderingOrder)targetMaterial.GetInt(MTRL_PROP_RENDERING_ORDER);
                }
                if (eRenderingOrder == eRenderingOrder.Default)
                {
                    if (value.materialCategory == MaterialPack.eMaterialCategory.Mayu)
                    {
                        targetMaterial.renderQueue = num + -1;
                    }
                    else
                    {
                        targetMaterial.renderQueue = num + -2;
                    }
                }
                else
                {
                    targetMaterial.renderQueue = num + Parts.GetRenderQueueOffset(eRenderingOrder);
                }
            }
        }

        public override void SetVisible(bool setActive)
        {
            base.SetVisible(setActive);
            if (_cheekRenderer != null)
            {
                _cheekRenderer.enabled = setActive;
            }
            if (_cheekRendererComponent != null)
            {
                _cheekRendererComponent.IsVisible = setActive;
            }
        }

        protected override void OnCreate()
        {
            _transformCollector.Collect();
            _arrTransformCache = _transformCollector.Build<eTransform>();
            Func<eTransform, Renderer> func = delegate (eTransform index)
            {
                Renderer renderer = null;
                Transform transform = _arrTransformCache[(int)index];
                if (transform != null)
                {
                    renderer = transform.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        CreateMaterialPack(renderer);
                    }
                }
                return renderer;
            };
            _renderer = func(eTransform.M_Head);
            _cheekRenderer = func(eTransform.M_Cheek);
            SetupCheekRenderer();
            SetRenderQueue(eRenderQueue.Geometry, 0);
            _initialized = true;
        }

        protected override MultiTextures OnGetMultiTextures(Material mtrl)
        {
            return MaterialPack.CheckMaterialCategory(mtrl) switch
            {
                MaterialPack.eMaterialCategory.Cheek => _texturePack.Get(TexturePack.eCategory.Cheek),
                MaterialPack.eMaterialCategory.Object => _texturePack.Get(TexturePack.eCategory.Object),
                _ => _texturePack.Get(TexturePack.eCategory.Head),
            };
        }
    }

    /// <summary>
    /// 3Dモデルパーツの抽象化クラス
    /// </summary>
    public abstract class Parts
    {
        public enum eCategory
        {
            Head = 0,
            Body = 1,
            Accessory = 2,
            Unknown = 3,
            MAX = 4,
            Invalid = 4
        }

        public enum eRenderingOrder
        {
            Default,
            A,
            B,
            C
        }

        protected const int RENDER_QUEUE_OFFSET_BASE = 6;

        protected readonly string MTRL_PROP_RENDERING_ORDER = "_RenderingOrder";

        public const int INVALID_PARTS_CODE = -1;

        public const int PARTS_CODE_LENGTH = 4;

        public const int PARTS_CODE_INDEX_IN_NAME = 2;

        public const char DEFAULT_NAME_SEPARATOR = '_';

        protected bool _assembled;

        protected bool _initialized;

        protected eCategory _category = eCategory.MAX;

        protected string _partsCode = string.Empty;

        protected int _index;

        protected GameObject _gameObject;

        protected Transform _transform;

        protected TransformCollector _transformCollector;

        protected Renderer _renderer;

        protected Animator _animator;

        protected TexturePack _texturePack;

        protected Dictionary<Material, MaterialPack> _mapMaterialPack = new Dictionary<Material, MaterialPack>();

        protected CharacterFlareCollisionParameter _FlareCollisionParameter;

        protected ClothController.Cloth _cloth;

        public bool assembled => _assembled;

        public bool initialized => _initialized;

        public eCategory category => _category;

        public string partsCode => _partsCode;

        public int index => _index;

        public string partsCodeWithoutPackageIndex => GetPartsCodeWithoutPackageIndex(_partsCode);

        public int packageIndex => GetPackageIndex(_partsCode);

        public int assemblePriority => GetAssemblePriority(_partsCode);

        public int partsId => GetPartsId(_partsCode);

        public GameObject gameObject => _gameObject;

        public Transform transform => _transform;

        public TransformCollector transformCollector => _transformCollector;

        public Renderer renderer => _renderer;

        public MeshRenderer meshRenderer => _renderer as MeshRenderer;

        public SkinnedMeshRenderer skinnedMeshRenderer => _renderer as SkinnedMeshRenderer;

        public Animator animator => _animator;

        public TexturePack texturePack => _texturePack;

        public Dictionary<Material, MaterialPack> mapMaterialPack => _mapMaterialPack;

        public CharacterFlareCollisionParameter FlareCollisionParameter
        {
            get
            {
                return _FlareCollisionParameter;
            }
            set
            {
                _FlareCollisionParameter = value;
            }
        }

        public ClothController.Cloth cloth
        {
            get
            {
                return _cloth;
            }
            set
            {
                _cloth = value;
            }
        }

        public static bool IsValidPartsCode(string partsCode)
        {
            bool result = false;
            int result2 = 0;
            if (!string.IsNullOrEmpty(partsCode) && partsCode.Length == 4 && int.TryParse(partsCode, out result2))
            {
                result = result2 >= 0;
            }
            return result;
        }

        public static bool IsValidPartsCode(List<string> lstPartsCode)
        {
            bool result = false;
            if (lstPartsCode != null && lstPartsCode.Count > 0)
            {
                bool flag = true;
                foreach (string item in lstPartsCode)
                {
                    flag &= IsValidPartsCode(item);
                }
                result = flag;
            }
            return result;
        }

        public static string ExtractPartsCode(string name, int index = 2, char separator = '_')
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            string[] array = name.Split(separator);
            index = ((index >= 0 && index < array.Length) ? index : (array.Length - 1));
            return array[index].Trim();
        }

        public static string MakePartsCode(int packageIndex, int assemblePriority, int partsIndex)
        {
            return $"{packageIndex:0}{assemblePriority:0}{partsIndex:00}";
        }

        public static int GetSubPartsCode(string partsCode, int startIndex, int length)
        {
            if (!string.IsNullOrEmpty(partsCode) && partsCode.Length == 4)
            {
                return int.Parse(partsCode.Substring(startIndex, length));
            }
            return -1;
        }

        public static int GetPackageIndex(string partsCode)
        {
            return GetSubPartsCode(partsCode, 0, 1);
        }

        public static void SetPackageIndex(ref string partsCode, int index)
        {
            int num = int.Parse(partsCode) % 1000;
            partsCode = (num + index * 1000).ToString();
        }

        public static int GetAssemblePriority(string partsCode)
        {
            return GetSubPartsCode(partsCode, 1, 1);
        }

        public static int GetPartsId(string partsCode)
        {
            return GetSubPartsCode(partsCode, 2, 2);
        }

        public static string GetPartsCodeWithoutPackageIndex(string partsCode)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(partsCode) && partsCode.Length == 4)
            {
                result = $"0{partsCode.Substring(1, 3):000}";
            }
            return result;
        }

        public static int GetPartsCodeNumber(string partsCode)
        {
            int result = 0;
            try
            {
                if (IsValidPartsCode(partsCode))
                {
                    return int.Parse(partsCode);
                }
                return result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static Parts CreateParts(eCategory category, GameObject gameObject, string partsCode, int index, TexturePack texturePack)
        {
            Parts result = null;
            switch (category)
            {
                case eCategory.Head:
                    result = new HeadParts(gameObject, partsCode, index, texturePack);
                    break;
                case eCategory.Body:
                    result = new BodyParts(gameObject, partsCode, index, texturePack);
                    break;
                case eCategory.Accessory:
                    result = new AccessoryParts(gameObject, partsCode, index, texturePack);
                    break;
            }
            return result;
        }

        protected static int GetRenderQueueOffset(eRenderingOrder order)
        {
            return Mathf.Clamp((int)(6 - order), 0, 6);
        }

        protected Parts(GameObject gameObject, string partsCode, int index, TexturePack texturePack)
        {
            _gameObject = UnityEngine.Object.Instantiate(gameObject);
            _partsCode = ((partsCode != null) ? partsCode : string.Empty);
            _index = index;
            _texturePack = texturePack;
            _transform = _gameObject.transform;
            _transformCollector = new TransformCollector(_transform);
            _renderer = _gameObject.GetComponentInChildren<Renderer>();
            _animator = _gameObject.GetComponent<Animator>();
            if (!IsValidPartsCode(_partsCode))
            {
                OnCreate();
            }
        }

        protected void CreateMaterialPack(Renderer targetRenderer)
        {
            if (targetRenderer == null)
            {
                return;
            }
            Material[] materials = targetRenderer.materials;
            int num = materials.Length;
            for (int i = 0; i < num; i++)
            {
                Material material = materials[i];
                if (!_mapMaterialPack.ContainsKey(material))
                {
                    MultiTextures multiTextures = GetMultiTextures(material);
                    MaterialPack materialPack = MaterialPack.Create(targetRenderer, material, i, multiTextures);
                    if (materialPack != null)
                    {
                        _mapMaterialPack.Add(material, materialPack);
                    }
                }
            }
        }

        public MaterialPack GetMaterialPack(Material mtrl)
        {
            MaterialPack value = null;
            _mapMaterialPack.TryGetValue(mtrl, out value);
            return value;
        }

        protected MultiTextures GetMultiTextures(Material mtrl)
        {
            if (_texturePack == null || mtrl == null)
            {
                return null;
            }
            MultiTextures multiTextures = null;
            if (mtrl.HasProperty("_UseFaceTex") && (int)mtrl.GetFloat("_UseFaceTex") == 1)
            {
                multiTextures = _texturePack.Get(TexturePack.eCategory.Object);
            }
            if (multiTextures == null && mtrl.HasProperty("_TexturePack"))
            {
                TexturePack.eCategory eCategory = (TexturePack.eCategory)mtrl.GetFloat("_TexturePack");
                multiTextures = _texturePack.Get(eCategory);
            }
            if (multiTextures == null)
            {
                multiTextures = OnGetMultiTextures(mtrl);
            }
            return multiTextures;
        }

        public void Release()
        {
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
                _gameObject = null;
            }
        }

        public void SetParent(Transform parent, bool worldPositionStays = false)
        {
            if (_initialized && _transform != null)
            {
                _transform.SetParent(parent, worldPositionStays);
            }
        }

        public Transform FindNode(string nodeName)
        {
            Transform transform = _transformCollector.Find(nodeName);
            if (transform == null && _transform != null)
            {
                transform = GameObjectUtility.FindChild(nodeName, _transform);
            }
            return transform;
        }

        public Transform FindNode<T>(int index) where T : IConvertible
        {
            return _transformCollector.Find<T>(index);
        }

        public IEnumerator Assemble(Parts slave, bool mergeMaterial)
        {
            _assembled = false;
            if (IsValidPartsCode(_partsCode))
            {
                PartsAssembler assembler = new PartsAssembler();
                yield return assembler.Build(this, slave, mergeMaterial);
                if (assembler.assembled)
                {
                    _assembled = assembler.assembled;
                    OnAssembled(assembler);
                    OnCreate();
                }
            }
        }

        protected virtual void OnAssembled(PartsAssembler assembler)
        {
            if (_assembled)
            {
                _partsCode = string.Empty;
            }
        }

        public virtual void SetVisible(bool setActive)
        {
            _renderer.enabled = setActive;
        }

        public virtual void SetRenderQueue(eRenderQueue renderQueue, int nOffset)
        {
            foreach (KeyValuePair<Material, MaterialPack> item in _mapMaterialPack)
            {
                Material targetMaterial = item.Value.targetMaterial;
                eRenderingOrder eRenderingOrder = eRenderingOrder.Default;
                if (targetMaterial.HasProperty(MTRL_PROP_RENDERING_ORDER))
                {
                    eRenderingOrder = (eRenderingOrder)targetMaterial.GetInt(MTRL_PROP_RENDERING_ORDER);
                }
                if (eRenderingOrder == eRenderingOrder.Default)
                {
                    targetMaterial.renderQueue = (int)(renderQueue + nOffset);
                }
                else
                {
                    targetMaterial.renderQueue = (int)(renderQueue + GetRenderQueueOffset(eRenderingOrder));
                }
            }
        }

        public abstract int GetSortingOrder();

        protected abstract void OnCreate();

        protected abstract MultiTextures OnGetMultiTextures(Material mtrl);
    }

    /// <summary>
    /// パーツを組み立てるためのクラス
    /// </summary>
    public class PartsAssembler
    {
        private const float MIN_BOUNDS_DEPTH = 0.3f;

        private bool _assembled;

        private bool _mergeMaterial;

        private Parts _masterParts;

        private Parts _slaveParts;

        private List<KeyValuePair<int, int>> _lstUVIndexOffset = new List<KeyValuePair<int, int>>();

        private Dictionary<string, int> _mapBoneTable = new Dictionary<string, int>();

        private Dictionary<int, int>[] _arrBoneIndexMap;

        private List<Bounds> _lstBounds = new List<Bounds>();

        private List<Vector2> _lstUV = new List<Vector2>();

        private List<Transform> _lstBone = new List<Transform>();

        private List<BoneWeight> _lstBoneWeight = new List<BoneWeight>();

        private List<Matrix4x4> _lstBindPose = new List<Matrix4x4>();

        private List<Material> _lstMaterial = new List<Material>();

        private List<CombineInstance> _lstCombineInstance = new List<CombineInstance>();

        public bool assembled => _assembled;

        public static bool IsSupport(Parts.eCategory category)
        {
            if (category == Parts.eCategory.Body)
            {
                return true;
            }
            return false;
        }

        private void Calc2PartsUV()
        {
            _ = _lstUV.Count;
            int count = _lstUVIndexOffset.Count;
            for (int i = 0; i < count; i++)
            {
                int key = _lstUVIndexOffset[i].Key;
                int value = _lstUVIndexOffset[i].Value;
                float num = i * 0.5f;
                for (int j = key; j < value; j++)
                {
                    Vector2 value2 = _lstUV[j];
                    value2.x = (value2.x - num) * 2f;
                    _lstUV[j] = value2;
                }
            }
        }

        private IEnumerator CollectModelInfo(int partsIndex, Parts parts)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = parts.skinnedMeshRenderer;
            Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
            Transform[] bones = skinnedMeshRenderer.bones;
            Matrix4x4[] bindposes = sharedMesh.bindposes;
            BoneWeight[] boneWeights = sharedMesh.boneWeights;
            Matrix4x4 localToWorldMatrix = skinnedMeshRenderer.transform.localToWorldMatrix;
            Matrix4x4 worldToLocalMatrix = skinnedMeshRenderer.transform.worldToLocalMatrix;
            _lstBounds.Add(skinnedMeshRenderer.bounds);
            int num = bones.Length;
            for (int i = 0; i < num; i++)
            {
                Transform transform = bones[i];
                Matrix4x4 matrix4x = bindposes[i];
                int value = 0;
                if (!_mapBoneTable.TryGetValue(transform.name, out value))
                {
                    value = _lstBone.Count;
                    _mapBoneTable.Add(transform.name, value);
                    _lstBone.Add(transform);
                    _lstBindPose.Add(matrix4x * worldToLocalMatrix);
                }
                _arrBoneIndexMap[partsIndex].Add(i, value);
            }
            int num2 = boneWeights.Length;
            for (int j = 0; j < num2; j++)
            {
                BoneWeight item = boneWeights[j];
                item.boneIndex0 = _arrBoneIndexMap[partsIndex][item.boneIndex0];
                item.boneIndex1 = _arrBoneIndexMap[partsIndex][item.boneIndex1];
                item.boneIndex2 = _arrBoneIndexMap[partsIndex][item.boneIndex2];
                item.boneIndex3 = _arrBoneIndexMap[partsIndex][item.boneIndex3];
                _lstBoneWeight.Add(item);
            }
            if (!_mergeMaterial)
            {
                _lstMaterial.AddRange(skinnedMeshRenderer.sharedMaterials);
                int count = _lstUV.Count;
                int value2 = count + sharedMesh.uv.Length;
                KeyValuePair<int, int> item2 = new KeyValuePair<int, int>(count, value2);
                _lstUVIndexOffset.Add(item2);
                _lstUV.AddRange(sharedMesh.uv);
            }
            CombineInstance item3 = default(CombineInstance);
            item3.mesh = sharedMesh;
            item3.transform = localToWorldMatrix;
            item3.subMeshIndex = 0;
            _lstCombineInstance.Add(item3);
            yield break;
        }

        private IEnumerator UpdateModelInfo(Parts parts)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = parts.skinnedMeshRenderer;
            Mesh mesh2 = (skinnedMeshRenderer.sharedMesh = new Mesh());
            mesh2.CombineMeshes(_lstCombineInstance.ToArray(), _mergeMaterial, useMatrices: false, hasLightmapData: false);
            mesh2.name = "CombinedMesh";
            mesh2.boneWeights = _lstBoneWeight.ToArray();
            mesh2.bindposes = _lstBindPose.ToArray();
            mesh2.RecalculateBounds();
            skinnedMeshRenderer.rootBone = _lstBone[0];
            skinnedMeshRenderer.bones = _lstBone.ToArray();
            Material[] array;
            if (_mergeMaterial)
            {
                Material sharedMaterial = skinnedMeshRenderer.sharedMaterial;
                array = new Material[1]
                {
                    new Material(sharedMaterial.shader)
                    {
                        name = "mt_merged"
                    }
                };
            }
            else
            {
                Calc2PartsUV();
                mesh2.uv = _lstUV.ToArray();
                int count = _lstMaterial.Count;
                array = new Material[count];
                for (int i = 0; i < count; i++)
                {
                    Material material = _lstMaterial[i];
                    array[i] = new Material(material.shader)
                    {
                        name = material.name
                    };
                }
            }
            skinnedMeshRenderer.sharedMaterials = array;
            Material[] materials = skinnedMeshRenderer.materials;
            foreach (Material material2 in materials)
            {
                string name = material2.name;
                int num = name.IndexOf(' ');
                if (num > 0 && num < name.Length)
                {
                    material2.name = name.Remove(num);
                }
            }
            Bounds localBounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Bounds lstBound in _lstBounds)
            {
                localBounds.Encapsulate(lstBound);
            }
            if (localBounds.extents.z < 0.3f)
            {
                Vector3 extents = localBounds.extents;
                extents.z = 0.3f;
                localBounds.extents = extents;
            }
            skinnedMeshRenderer.localBounds = localBounds;
            yield break;
        }

        protected IEnumerator RebuildTransformHierarchy()
        {
            foreach (Transform item in _lstBone)
            {
                Transform parent = item.parent;
                if (!(parent == null))
                {
                    int value = 0;
                    if (_mapBoneTable.TryGetValue(parent.name, out value))
                    {
                        Transform parent2 = _lstBone[value];
                        item.SetParent(parent2);
                    }
                }
            }
            Transform[] componentsInChildren = _lstBone[0].gameObject.GetComponentsInChildren<Transform>();
            Dictionary<string, List<Transform>> dictionary = new Dictionary<string, List<Transform>>();
            Dictionary<string, Transform> dictionary2 = new Dictionary<string, Transform>();
            Transform[] array = componentsInChildren;
            foreach (Transform transform in array)
            {
                string name = transform.name;
                List<Transform> value2 = null;
                if (!dictionary.TryGetValue(name, out value2))
                {
                    value2 = new List<Transform>();
                    dictionary.Add(name, value2);
                }
                value2.Add(transform);
            }
            foreach (Transform item2 in _lstBone)
            {
                dictionary2.Add(item2.name, item2);
            }
            foreach (KeyValuePair<string, List<Transform>> item3 in dictionary)
            {
                List<Transform> value3 = item3.Value;
                if (value3.Count < 2)
                {
                    continue;
                }
                List<GameObject> list = new List<GameObject>();
                int count = value3.Count;
                for (int j = 0; j < count; j++)
                {
                    Transform transform2 = value3[j];
                    Transform value4 = null;
                    if ((!dictionary2.TryGetValue(transform2.name, out value4) || !(value4 == transform2)) && transform2.childCount <= 0)
                    {
                        list.Add(transform2.gameObject);
                    }
                }
                if (list.Count == 0)
                {
                    for (int k = 1; k < count; k++)
                    {
                        list.Add(value3[k].gameObject);
                    }
                }
                foreach (GameObject item4 in list)
                {
                    UnityEngine.Object.Destroy(item4);
                }
            }
            yield break;
        }

        public IEnumerator Build(Parts master, Parts slave, bool mergeMaterial)
        {
            if (master != slave)
            {
                _masterParts = master;
                _slaveParts = slave;
                _mergeMaterial = mergeMaterial;
                _arrBoneIndexMap = new Dictionary<int, int>[2]
                {
                    new Dictionary<int, int>(),
                    new Dictionary<int, int>()
                };
                _ = _masterParts.skinnedMeshRenderer;
                yield return CollectModelInfo(0, _masterParts);
                yield return CollectModelInfo(1, _slaveParts);
                yield return UpdateModelInfo(_masterParts);
                yield return RebuildTransformHierarchy();
                _assembled = true;
            }
        }

        public void Clear()
        {
            _assembled = false;
            _mergeMaterial = false;
            _masterParts = null;
            _slaveParts = null;
            _lstUVIndexOffset.Clear();
            _mapBoneTable.Clear();
            _arrBoneIndexMap = null;
            _lstBounds.Clear();
            _lstUV.Clear();
            _lstBone.Clear();
            _lstBoneWeight.Clear();
            _lstBindPose.Clear();
            _lstMaterial.Clear();
            _lstCombineInstance.Clear();
        }
    }

    public class DrawOutline : RenderCommand
    {
        private static readonly int OUTLINE_PASS = 0;

        private static readonly string OUTLINE_PASS_NAME = "Outline";

        private static readonly string OUTLINE_SHADER_NAME = "CharaOutline";
        //private static readonly string OUTLINE_SHADER_NAME = "Hidden/CharaOutline";

        private static readonly string COMMAND_BUFFER_NAME = "Outline";

        private Transform _rootTransform;

        private static bool CheckUsable(MaterialPack mtrlPack)
        {
            bool flag = false;
            Material targetMaterial = mtrlPack.targetMaterial;
            flag = mtrlPack.CheckCapability(MaterialPack.eCapability.Outline);
            int passCount = targetMaterial.passCount;
            for (int i = 0; i < passCount; i++)
            {
                if (string.Compare(targetMaterial.GetPassName(i), OUTLINE_PASS_NAME, ignoreCase: true) == 0)
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        protected override CameraEvent GetCameraEvent()
        {
            return CameraEvent.AfterForwardOpaque;
        }

        protected override string GetBufferName()
        {
            return $"Character_{_owner.createInfo.index}_{_owner.data.charaId}_{COMMAND_BUFFER_NAME}";
        }

        protected override bool OnInitialize(HashSet<Parts> lstParts, int maxCameraNum)
        {
            bool result = false;

            //Outlineを描画するかどうかをここで確定する
            int save = SaveManager.GetInt("Outline");
            if (save == 0) return result;

            //Shader shader = Shader.Find(OUTLINE_SHADER_NAME);
            Shader shader = ResourcesManager.instance.GetShader(OUTLINE_SHADER_NAME);

            if (shader == null)
            {
                return result;
            }
            foreach (Parts lstPart in lstParts)
            {
                foreach (KeyValuePair<Material, MaterialPack> item2 in lstPart.mapMaterialPack)
                {
                    MaterialPack value = item2.Value;
                    if (CheckUsable(value))
                    {
                        RenderInfo item = new RenderInfo(shader, value, maxCameraNum)
                        {
                            shaderPass = OUTLINE_PASS
                        };
                        _lstRenderInfo.Add(item);
                    }
                }
            }
            result = _lstRenderInfo.Count > 0;
            if (result)
            {
                _rootTransform = _owner.bodyRoot;
            }
            return result;
        }

        protected override void OnUpdate(UpdateInfo updateInfo, CameraInfo cameraInfo, int cameraIndex)
        {
            if (!updateInfo.enableOutlineLOD)
            {
                if (cameraInfo.bufferSize == 0)
                {
                    SetBuffer(cameraInfo, cameraIndex);
                }
                return;
            }
            Vector3 position = cameraInfo.cameraTransform.position;
            float num = cameraInfo.camera.fieldOfView / 360f;
            float num2 = (position - _rootTransform.position).sqrMagnitude * num * num;
            float distanceForOutlineLOD = updateInfo.distanceForOutlineLOD;
            if (num2 > distanceForOutlineLOD * distanceForOutlineLOD)
            {
                if (cameraInfo.bufferSize > 0)
                {
                    cameraInfo.commandBuffer.Clear();
                }
            }
            else if (cameraInfo.bufferSize == 0)
            {
                SetBuffer(cameraInfo, cameraIndex);
            }
        }
    }

    public abstract class RenderCommand : IDisposable
    {
        public class RenderInfo : IDisposable
        {
            protected bool[] _renderable;

            protected Renderer _renderer;

            protected Material _source;

            protected Material _material;

            protected int _subMeshIndex;

            protected int _shaderPass;

            private bool _disposed;

            public bool[] renderable
            {
                get
                {
                    return _renderable;
                }
                set
                {
                    _renderable = value;
                }
            }

            public Renderer renderer => _renderer;

            public Material material => _material;

            public Material source => _source;

            public int subMeshIndex => _subMeshIndex;

            public int shaderPass
            {
                get
                {
                    return _shaderPass;
                }
                set
                {
                    _shaderPass = ((value < _material.passCount) ? value : 0);
                }
            }

            public Bounds bounds => _renderer.bounds;

            public int passCount => _material.passCount;

            public RenderInfo(Shader shader, MaterialPack mtrlPack, int cameraNum)
            {
                _renderer = mtrlPack.targetRenderer;
                _renderable = new bool[cameraNum];
                _source = mtrlPack.targetMaterial;
                _material = new Material(shader);
                _material.CopyPropertiesFromMaterial(_source);
                _subMeshIndex = mtrlPack.subMeshIndex;
            }

            ~RenderInfo()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    if (_material != null)
                    {
                        UnityEngine.Object.DestroyImmediate(_material);
                    }
                    _renderer = null;
                    _material = null;
                    GC.SuppressFinalize(this);
                    _disposed = true;
                }
            }
        }

        protected class CameraInfo : IDisposable
        {
            private CameraEvent _cameraEvent = CameraEvent.AfterSkybox;

            private ICameraSharedCache _cameraCache;

            private CommandBuffer _commandBuffer;

            private bool _disposed;

            public CameraEvent cameraEvent => _cameraEvent;

            public Camera camera => _cameraCache.camera;

            public Transform cameraTransform => _cameraCache.transform;

            public Plane[] frustumPlanes => _cameraCache.frustumPlanes;

            public CommandBuffer commandBuffer => _commandBuffer;

            public int bufferSize => _commandBuffer.sizeInBytes;

            public bool valid
            {
                get
                {
                    if (!_disposed && _cameraCache != null)
                    {
                        return _commandBuffer != null;
                    }
                    return false;
                }
            }

            public CameraInfo(Camera targetCamera, CameraEvent cameraEvent, string bufferName)
            {
                _cameraEvent = cameraEvent;
                _cameraCache = CameraSharedCachePool.CreateSharedCache(targetCamera);
                _commandBuffer = new CommandBuffer();
                _commandBuffer.name = bufferName;
                _cameraCache.camera.AddCommandBuffer(cameraEvent, _commandBuffer);
            }

            ~CameraInfo()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    if (_cameraCache != null && _commandBuffer != null)
                    {
                        _cameraCache.camera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
                        _commandBuffer.Release();
                        _commandBuffer = null;
                    }
                    _cameraCache = null;
                    GC.SuppressFinalize(this);
                    _disposed = true;
                }
            }
        }

        public class UpdateInfo
        {
            private bool _enableOutlineLOD = true;

            private bool _enableShaderLOD = true;

            private float _distanceForOutlineLOD = (float)Math.PI;

            private float _distanceForShaderLOD = (float)Math.PI;

            public float distanceForOutlineLOD
            {
                get
                {
                    return _distanceForOutlineLOD;
                }
                set
                {
                    _distanceForOutlineLOD = value;
                }
            }

            public float distanceForShaderLOD
            {
                get
                {
                    return _distanceForShaderLOD;
                }
                set
                {
                    _distanceForShaderLOD = value;
                }
            }

            public bool enableOutlineLOD
            {
                get
                {
                    return _enableOutlineLOD;
                }
                set
                {
                    _enableOutlineLOD = value;
                }
            }

            public bool enableShaderLOD
            {
                get
                {
                    return _enableShaderLOD;
                }
                set
                {
                    _enableShaderLOD = value;
                }
            }
        }

        public enum eCategory
        {
            Opaque = 0,
            BluredTransparent = 1,
            Outline = 2,
            MAX_COUNT = 3,
            Unknown = 3
        }

        private const int MAX_CAMERA_INFO_COUNT = 16;

        private bool _initialized;

        protected Character3DBase _owner;

        protected GameObject _ownerGameObject;

        protected eCategory _category = eCategory.MAX_COUNT;

        protected List<RenderInfo> _lstRenderInfo = new List<RenderInfo>();

        protected Dictionary<Camera, CameraInfo> _mapCameraInfo = new Dictionary<Camera, CameraInfo>();

        private CameraInfo[] _cameraInfos;

        private int _cameraInfoCount;

        private bool _disposed;

        public eCategory category => _category;

        public List<RenderInfo> lstRenderInfo => _lstRenderInfo;

        public static RenderCommand Create(Character3DBase owner, eCategory category, HashSet<Parts> lstParts, int maxCameraInfoCount = 16)
        {
            RenderCommand renderCommand = null;
            if (category == eCategory.Outline)
            {
                renderCommand = new DrawOutline();
            }
            if (renderCommand != null && !renderCommand.Initialize(owner, lstParts, maxCameraInfoCount))
            {
                renderCommand.Dispose();
                renderCommand = null;
            }
            return renderCommand;
        }

        ~RenderCommand()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                foreach (RenderInfo item in _lstRenderInfo)
                {
                    item.Dispose();
                }
                foreach (KeyValuePair<Camera, CameraInfo> item2 in _mapCameraInfo)
                {
                    item2.Value.Dispose();
                }
                _lstRenderInfo.Clear();
                _mapCameraInfo.Clear();
                ClearCameraInfos();
            }
            _disposed = true;
        }

        private void AddCameraInfo(CameraInfo cameraInfo)
        {
            _cameraInfos[_cameraInfoCount++] = cameraInfo;
        }

        private void RemoveCameraInfo(CameraInfo cameraInfo)
        {
            int i = 0;
            int cameraInfoCount;
            for (cameraInfoCount = _cameraInfoCount; i < cameraInfoCount && _cameraInfos[i] != cameraInfo; i++)
            {
            }
            if (i != cameraInfoCount)
            {
                for (int num = cameraInfoCount - 1; i != num; i++)
                {
                    _cameraInfos[i] = _cameraInfos[i + 1];
                }
                _cameraInfos[i] = null;
                _cameraInfoCount--;
            }
        }

        private void ClearCameraInfos()
        {
            int i = 0;
            for (int cameraInfoCount = _cameraInfoCount; i < cameraInfoCount; i++)
            {
                _cameraInfos[i] = null;
            }
            _cameraInfoCount = 0;
        }

        public bool Initialize(Character3DBase owner, HashSet<Parts> lstParts, int maxCameraInfoCount = 16)
        {
            if (_initialized)
            {
                return false;
            }
            _owner = owner;
            _ownerGameObject = _owner.gameObject;
            _cameraInfos = new CameraInfo[maxCameraInfoCount];
            if (OnInitialize(lstParts, maxCameraInfoCount))
            {
                _initialized = true;
            }
            return _initialized;
        }

        public void Update(UpdateInfo updateInfo)
        {
            int i = 0;
            for (int cameraInfoCount = _cameraInfoCount; i < cameraInfoCount; i++)
            {
                CameraInfo cameraInfo = _cameraInfos[i];
                Camera camera = cameraInfo.camera;
                if (camera.isActiveAndEnabled && cameraInfo.valid)
                {
                    int num = 1 << _ownerGameObject.layer;
                    if (_owner.liveCharaVisible && (camera.cullingMask & num) != 0)
                    {
                        FrustumCulling(cameraInfo, i);
                        OnUpdate(updateInfo, cameraInfo, i);
                    }
                    else if (cameraInfo.bufferSize > 0)
                    {
                        cameraInfo.commandBuffer.Clear();
                    }
                }
            }
        }

        public RenderInfo FindRenderInfo(Material material)
        {
            foreach (RenderInfo item in _lstRenderInfo)
            {
                if (item.source == material)
                {
                    return item;
                }
            }
            return null;
        }

        public void Register(Camera targetCamera)
        {
            if (!_mapCameraInfo.ContainsKey(targetCamera))
            {
                CameraInfo cameraInfo = new CameraInfo(targetCamera, GetCameraEvent(), GetBufferName());
                _mapCameraInfo.Add(targetCamera, cameraInfo);
                AddCameraInfo(cameraInfo);
            }
        }

        public void Unregister(Camera targetCamera)
        {
            CameraInfo value = null;
            if (_mapCameraInfo.TryGetValue(targetCamera, out value))
            {
                RemoveCameraInfo(value);
                value.Dispose();
                _mapCameraInfo.Remove(targetCamera);
            }
        }

        public void SetShaderKeyword(string strKeyword, bool sw)
        {
            foreach (RenderInfo item in _lstRenderInfo)
            {
                if (sw)
                {
                    item.material.EnableKeyword(strKeyword);
                }
                else
                {
                    item.material.DisableKeyword(strKeyword);
                }
            }
        }

        protected void FrustumCulling(CameraInfo cameraInfo, int cameraIndex)
        {
            bool flag = false;
            foreach (RenderInfo item in _lstRenderInfo)
            {
                bool flag2 = GeometryUtility.TestPlanesAABB(cameraInfo.frustumPlanes, item.bounds);
                if (item.renderable[cameraIndex] != flag2)
                {
                    item.renderable[cameraIndex] = flag2;
                    flag = true;
                }
            }
            if (flag && cameraInfo.bufferSize > 0)
            {
                cameraInfo.commandBuffer.Clear();
            }
        }

        protected virtual void SetBuffer(CameraInfo cameraInfo, int cameraIndex)
        {
            CommandBuffer commandBuffer = cameraInfo.commandBuffer;
            foreach (RenderInfo item in _lstRenderInfo)
            {
                if (item.renderable[cameraIndex])
                {
                    commandBuffer.DrawRenderer(item.renderer, item.material, item.subMeshIndex, item.shaderPass);
                }
            }
        }

        protected virtual void OnUpdate(UpdateInfo updateInfo, CameraInfo cameraInfo, int cameraIndex)
        {
            if (cameraInfo.bufferSize == 0)
            {
                SetBuffer(cameraInfo, cameraIndex);
            }
        }

        public void RegisterCommandBuffer(Camera targetCamera)
        {
            if (_mapCameraInfo.TryGetValue(targetCamera, out var value))
            {
                targetCamera.AddCommandBuffer(value.cameraEvent, value.commandBuffer);
            }
        }

        public void UnregisterCommandBuffer(Camera targetCamera)
        {
            if (_mapCameraInfo.TryGetValue(targetCamera, out var value))
            {
                targetCamera.RemoveCommandBuffer(value.cameraEvent, value.commandBuffer);
            }
        }

        protected abstract CameraEvent GetCameraEvent();

        protected abstract string GetBufferName();

        protected abstract bool OnInitialize(HashSet<Parts> lstParts, int maxCameraNum);
    }

    public class GimmickRequest : IRequest
    {
        public class TransformAnimatorRequestParameter
        {
            public enum eChangeFlag
            {
                NoChange,
                Active,
                AutoPlay
            }

            private int _triggerId;

            private bool _active;

            private bool _autoPlay;

            private eChangeFlag _changeFlags;

            public eChangeFlag changeFlags => _changeFlags;

            public int triggerId
            {
                get
                {
                    return _triggerId;
                }
                set
                {
                    _triggerId = value;
                }
            }

            public bool active
            {
                get
                {
                    return _active;
                }
                set
                {
                    _changeFlags |= eChangeFlag.Active;
                    _active = value;
                }
            }

            public bool autoPlay
            {
                get
                {
                    return _autoPlay;
                }
                set
                {
                    _changeFlags |= eChangeFlag.AutoPlay;
                    _autoPlay = value;
                }
            }

            public bool CheckFlag(eChangeFlag flag)
            {
                return (_changeFlags & flag) == flag;
            }

            public void Reset()
            {
                _triggerId = 0;
                _active = false;
                _autoPlay = false;
                _changeFlags = eChangeFlag.NoChange;
            }
        }

        private TransformAnimatorRequestParameter _transformAnimatorRequestParameter = new TransformAnimatorRequestParameter();

        public Type type => typeof(GimmickRequest);

        public TransformAnimatorRequestParameter transformAnimatorRequestParameter => _transformAnimatorRequestParameter;

        public void Initialize()
        {
            _transformAnimatorRequestParameter.Reset();
        }
    }

    public interface IRequest
    {
        Type type { get; }

        void Initialize();
    }

    public interface IRequestExecutor
    {
        Type type { get; }

        IRequest request { get; }

        void ExecuteRequest(IRequest request);
    }

    public abstract class RequestExecutor<REQUEST> : IRequestExecutor where REQUEST : IRequest, new()
    {
        private REQUEST _request = new REQUEST();

        public IRequest request => _request;

        public abstract Type type { get; }

        public void ExecuteRequest(IRequest request)
        {
            if (request != null && this.request.type == request.type)
            {
                OnExecuteRequest((REQUEST)request);
            }
        }

        protected abstract void OnExecuteRequest(REQUEST request);
    }

    /// <summary>
    /// 読み込み時にCharaDirectorからよみこむかどうか
    /// </summary>
    private bool isLoadFromDirector = true;

    protected const float SCALE_BASE = 1.02f;

    protected const float HEEL_HEIGHT = 8f;

    public const string MouseAnimatorStateSpeedName = "MouseSpeed";

    public const string EyeAnimatorStateSpeedName = "EyeSpeed";

    protected int _index;

    protected DressCabinet.Cache _cabinetCache = new DressCabinet.Cache();

    protected CreateInfo _createInfo = new CreateInfo();

    /// <summary>
    /// 他キャラ衣装
    /// </summary>
    protected CreateInfo _appendCreateInfo = new CreateInfo();

    /// <summary>
    /// 他キャラ衣装
    /// </summary>
    private CharaDirector _charaDirector = null;

    private bool _isSetting;

    private bool _isVisible = true;

    private bool _isMeshActiveAuto = true;

    /// <summary>
    /// 物理演算での当たり判定値(実サイズより1.02倍)
    /// </summary>
    private float _bodyScale = 1f;

    /// <summary>
    /// キャラの身長とモデルサイズから算出される拡大率
    /// </summary>
    private float _bodySubScale = 1f;

    /// <summary>
    /// 計算で使用される拡大率
    /// </summary>
    private float _bodyScaleSubScale = 1f;

    private float _charaHeight;

    protected Transform _parentTransform;

    protected Transform _cachedTransform;

    protected Transform _faceNodeHead;

    protected Transform _faceNodeNeck;

    protected Transform _bodyNodeHead;

    protected Transform _bodyNodeNeck;

    protected Transform _bodyRoot;

    protected TransformCollector _effectSpot;

    protected Camera _mainCamera;

    protected HeadParts _headParts;

    protected BodyParts _bodyParts;

    protected List<BodyParts> _lstBodyParts = new List<BodyParts>();

    protected List<AccessoryParts> _lstAccessoryParts = new List<AccessoryParts>();

    protected List<Parts> _lstParts = new List<Parts>();

    protected TexturePack _texturePack;

    protected RenderController _renderController;

    protected LipSyncController _lipSyncController;

    protected EyeTraceController _eyeTraceController;

    protected Vector3 _eyeTargetOffset;

    protected GimmickController _gimmickController;

    protected List<string> _listAnimationClipName = new List<string>();

    // protected Dictionary<Parts.eCategory, TextureComposite.Importer> _mapTexCompImporter;

    protected GameObject _sweatLocatorObj;

    protected Material _sweatMat;

    protected List<SweatLocatorInfo> _sweatLocators = new List<SweatLocatorInfo>();

    private const float WARMING_UP_CLOTH_TIME = 1.2f;

    private float _cyspringUpdateTime;

    private bool _isClothUpdate;

    private bool _isResetCloth;

    private bool _isWarmUpCloth;

    protected Vector3 _bakPosition = Vector3.zero;

    protected Quaternion _bakRotation = Quaternion.identity;

    private bool _isResetClothAll;

    private bool _isResetClothWarmingUp;

    private const float RESET_WARMING_UP_CLOTH_TIME = 0.6f;

    private CySpringCollisionComponent.ePurpose _cyspringPurpose;

    private ClothController _clothController;

    private const string BODY_OFTEN_ACCESS_TRANSFORM_JOINT = "Hip";

    private const string HEAD_OFTEN_ACCESS_TRANSFORM_JOINT = "Head";

    private ResourcesManager _resourcesManager;

    private Queue<KeyValuePair<string, Func<IEnumerator>>> _queTask = new Queue<KeyValuePair<string, Func<IEnumerator>>>();

    private const float DefaultCollisionScale = 1f;

    private const float CollisionScaleBase = 154f;

    private HashSet<IRequest> _setRequest = new HashSet<IRequest>();

    private Dictionary<Type, HashSet<IRequestExecutor>> _mapRequestExecutor = new Dictionary<Type, HashSet<IRequestExecutor>>();

    public int index => _index;

    public bool isSetting => _isSetting;

    public CreateInfo createInfo
    {
        get
        {
            return _createInfo;
        }
        set
        {
            _createInfo = value;
        }
    }

    /// <summary>
    /// 他キャラ衣装
    /// </summary>
    public CreateInfo appendCreateInfo
    {
        get
        {
            return _appendCreateInfo;
        }
        set
        {
            _appendCreateInfo = value;
        }
    }

    public CharacterData data
    {
        get
        {
            return _createInfo.charaData;
        }
        set
        {
            _createInfo.charaData = value;
        }
    }

    /// <summary>
    /// 他キャラ衣装
    /// </summary>
    public CharacterData appendData
    {
        get
        {
            return _appendCreateInfo.charaData;
        }
        set
        {
            _appendCreateInfo.charaData = value;
        }
    }

    public CharaDirector charaDirector
    {
        get
        {
            return _charaDirector;
        }
    }

    public Camera mainCamera
    {
        get
        {
            return _mainCamera;
        }
        set
        {
            _mainCamera = value;
        }
    }

    public bool isMeshActiveAuto
    {
        get
        {
            return _isMeshActiveAuto;
        }
        set
        {
            _isMeshActiveAuto = value;
        }
    }

    public float bodyScaleSubScale => _bodyScaleSubScale;

    public float charaHeight => _charaHeight;

    public List<Parts> lstParts => _lstParts;

    public Transform bodyNodeNeck => _bodyNodeNeck;

    public Transform bodyNodeHead => _bodyNodeHead;

    public Transform bodyRoot => _bodyRoot;

    public TransformCollector effectSpot => _effectSpot;

    public RenderController renderController => _renderController;

    public EyeTraceController eyeTraceController => _eyeTraceController;

    public GimmickController gimmickController => _gimmickController;

    public bool liveCharaVisible
    {
        get
        {
            return _isVisible;
        }
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                SetMeshActive(_isVisible);
                OnLiveCharaVisible(_isVisible);
                if (_lipSyncController != null)
                {
                    _lipSyncController._isRender = _isVisible;
                }
                if (_clothController != null)
                {
                    _clothController.EnableUnionCollision(_isVisible);
                }
            }
        }
    }

    public Vector3 eyeTargetOffset
    {
        get
        {
            return _eyeTargetOffset;
        }
        set
        {
            _eyeTargetOffset = value;
        }
    }

    public ClothController clothController => _clothController;

    public bool isClothUpdate
    {
        get
        {
            return _isClothUpdate;
        }
        set
        {
            _isClothUpdate = value;
        }
    }

    public bool isWarmUpCloth
    {
        get
        {
            return _isWarmUpCloth;
        }
        set
        {
            _isWarmUpCloth = value;
        }
    }

    public bool isResetClothAll
    {
        get
        {
            return _isResetClothAll;
        }
        set
        {
            _isResetClothAll = value;
        }
    }

    public bool isResetClothWarmingUp
    {
        get
        {
            return _isResetClothWarmingUp;
        }
        set
        {
            _isResetClothWarmingUp = value;
        }
    }

    public CySpringCollisionComponent.ePurpose cyspringPurpose
    {
        get
        {
            return _cyspringPurpose;
        }
        set
        {
            _cyspringPurpose = value;
        }
    }

    public int remainTask => _queTask.Count;

    /// <summary>
    /// キャラクタに必要なアセットファイルを読み込む
    /// </summary>
    public static void GetLoadAssetBundle(List<string> list, int charaId, int dressId, int accessoryId, int heightId, int weightId, int bustId, int skinId, int colorId, int headIndex, int headTextureIndex, bool isSubHeadIndex, bool useLocalTexture, List<string> lstPartsCode, eResourceQuality resourceQuality)
    {
        DressCabinet.Cache dressCabinetCache = new DressCabinet.Cache();
        dressCabinetCache.Initialize(dressId, charaId);
        int bodyIndex = dressCabinetCache.bodyIndex;
        int bodyTextureIndex = dressCabinetCache.bodyTexIndex;
        if (dressCabinetCache.headIndex != 0)
        {
            isSubHeadIndex = false;
            headIndex = dressCabinetCache.headIndex;
        }
        if (dressCabinetCache.headTexIndex != 0)
        {
            headTextureIndex = dressCabinetCache.headTexIndex;
        }
        Action<string, List<string>> _fnAppendAssetBundleName = delegate (string assetBundleName, List<string> lstAssetBundleName)
        {
            if (!string.IsNullOrEmpty(assetBundleName) && ResourcesManager.instance.ExistsAssetBundleManifest(assetBundleName) && !lstAssetBundleName.Contains(assetBundleName))
            {
                lstAssetBundleName.Add(assetBundleName);
            }
        };
        string bodyClothName = CharaDirector.AssetBundle.GetBodyClothName(dressId, bodyIndex);
        string headModelName = CharaDirector.AssetBundle.GetHeadModelName(charaId, dressId, headIndex, isSubHeadIndex, resourceQuality);
        string headTextureName = CharaDirector.AssetBundle.GetHeadTextureName(charaId, dressId, headIndex, headTextureIndex, isSubHeadIndex, resourceQuality);
        _fnAppendAssetBundleName(bodyClothName, list);
        _fnAppendAssetBundleName(headModelName, list);
        _fnAppendAssetBundleName(headTextureName, list);
        Action<MultiTextures.eMap> action = delegate (MultiTextures.eMap map)
        {
            string bodyTextureName = CharaDirector.AssetBundle.GetBodyTextureName(charaId, dressId, skinId, bustId, colorId, bodyTextureIndex, resourceQuality, map);
            _fnAppendAssetBundleName(bodyTextureName, list);
        };
        bool bAddDiffuse = false;
        bool bAddSpecular = false;
        bool flag = false;
        if (Parts.IsValidPartsCode(lstPartsCode))
        {
            foreach (string item in lstPartsCode)
            {
                string bodyModelName = CharaDirector.AssetBundle.GetBodyModelName(dressId, heightId, weightId, bustId, 0, item, resourceQuality);
                _fnAppendAssetBundleName(bodyModelName, list);
            }
            bAddDiffuse = !useLocalTexture;
            bAddSpecular = !useLocalTexture;
            flag = !useLocalTexture;
        }
        else
        {
            string bodyModelName2 = CharaDirector.AssetBundle.GetBodyModelName(dressId, heightId, weightId, bustId, bodyIndex, string.Empty, resourceQuality);
            _fnAppendAssetBundleName(bodyModelName2, list);
            bAddDiffuse = !useLocalTexture;
            bAddSpecular = !useLocalTexture;
            flag = true;
        }
        if (bAddDiffuse)
        {
            action(MultiTextures.eMap.Diffuse);
        }
        if (resourceQuality == eResourceQuality.Rich)
        {
            if (bAddSpecular)
            {
                action(MultiTextures.eMap.Specular);
            }
            if (flag)
            {
                action(MultiTextures.eMap.Control);
            }
        }
        if (Director.instance != null && Director.instance.IsEnableFlare())
        {
            string headFlareColliderName = CharaDirector.AssetBundle.GetHeadFlareColliderName(charaId, dressId, headIndex, isSubHeadIndex);
            string bodyFlareColliderName = CharaDirector.AssetBundle.GetBodyFlareColliderName(dressId, heightId, weightId, bustId, bodyIndex);
            _fnAppendAssetBundleName(headFlareColliderName, list);
            _fnAppendAssetBundleName(bodyFlareColliderName, list);
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                Action<int, AccessoryParts.eType> action2 = delegate (int accIndex, AccessoryParts.eType type)
                {
                    string accessoryFlareColliderName = CharaDirector.AssetBundle.GetAccessoryFlareColliderName(accessoryId, accIndex, type);
                    if (!string.IsNullOrEmpty(accessoryFlareColliderName))
                    {
                        _fnAppendAssetBundleName(accessoryFlareColliderName, list);
                    }
                };
                if (dressCabinetCache.validAcc)
                {
                    if (dressCabinetCache.loadMain)
                    {
                        action2(dressCabinetCache.mainAccIndex, AccessoryParts.eType.Main);
                    }
                    if (dressCabinetCache.loadSub && dressCabinetCache.usableSubAccIndex)
                    {
                        action2(dressCabinetCache.subAccIndex, AccessoryParts.eType.Sub);
                    }
                }
                else
                {
                    action2(dressCabinetCache.mainAccIndex, AccessoryParts.eType.Main);
                    if (dressCabinetCache.usableSubAccIndex)
                    {
                        action2(dressCabinetCache.subAccIndex, AccessoryParts.eType.Sub);
                    }
                }
            }
        }
        if (!StageUtil.IsModelCommonDressId(dressId))
        {
            return;
        }
        Action<int, int, AccessoryParts.eType> action3 = delegate (int accIndex, int accTexIndex, AccessoryParts.eType type)
        {
            string accessoryModelName = CharaDirector.AssetBundle.GetAccessoryModelName(accessoryId, accIndex, type, resourceQuality);
            _fnAppendAssetBundleName(accessoryModelName, list);
            if (bAddDiffuse)
            {
                string accessoryTextureName = CharaDirector.AssetBundle.GetAccessoryTextureName(accessoryId, colorId, dressCabinetCache.accTexDiversity, accTexIndex, resourceQuality, MultiTextures.eMap.Diffuse);
                _fnAppendAssetBundleName(accessoryTextureName, list);
            }
            if (resourceQuality == eResourceQuality.Rich)
            {
                if (bAddSpecular)
                {
                    string accessoryTextureName2 = CharaDirector.AssetBundle.GetAccessoryTextureName(accessoryId, colorId, dressCabinetCache.accTexDiversity, accTexIndex, resourceQuality, MultiTextures.eMap.Specular);
                    _fnAppendAssetBundleName(accessoryTextureName2, list);
                }
                string accessoryTextureName3 = CharaDirector.AssetBundle.GetAccessoryTextureName(accessoryId, colorId, dressCabinetCache.accTexDiversity, accTexIndex, resourceQuality, MultiTextures.eMap.Control);
                _fnAppendAssetBundleName(accessoryTextureName3, list);
            }
        };
        int arg = (dressCabinetCache.accTexDiversity ? dressCabinetCache.accTexIndex : 0);
        string accessoryParameterName = CharaDirector.AssetBundle.GetAccessoryParameterName(accessoryId);
        if (!string.IsNullOrEmpty(accessoryParameterName))
        {
            _fnAppendAssetBundleName(accessoryParameterName, list);
        }
        if (dressCabinetCache.validAcc)
        {
            if (dressCabinetCache.loadMain)
            {
                action3(dressCabinetCache.mainAccIndex, arg, AccessoryParts.eType.Main);
            }
            if (dressCabinetCache.loadSub && dressCabinetCache.usableSubAccIndex)
            {
                action3(dressCabinetCache.subAccIndex, arg, AccessoryParts.eType.Sub);
            }
        }
        else
        {
            action3(dressCabinetCache.mainAccIndex, arg, AccessoryParts.eType.Main);
            if (dressCabinetCache.usableSubAccIndex)
            {
                action3(dressCabinetCache.subAccIndex, arg, AccessoryParts.eType.Sub);
            }
            else
            {
                action3(1, 0, AccessoryParts.eType.Sub);
            }
        }
    }

    public static CharacterData CreateCharacterData(int id_character, int id_dress, int id_accessory)
    {
        CharacterData result = null;
        MasterCharaData.CharaData charaData = MasterDBManager.instance.masterCharaData.Get(id_character);
        if (charaData != null)
        {
            result = new CharacterData(charaData.baseCardId, charaData.charaId, id_dress, id_accessory, charaData.modelHeightId, charaData.modelWeightId, charaData.modelBustId, charaData.modelSkinId, charaData.attribute - 1, charaData.height, id_dress, charaData.attribute);
        }
        return result;
    }

    public static void GetAssetBundleList(List<string> lstAssetBundle, int id_character, int id_dress, int id_accessory)
    {
        MasterDressColorData masterDressColorData = MasterDBManager.instance.masterDressColorData;
        CharacterData charaData = CreateCharacterData(id_character, id_dress, id_accessory);
        if (charaData == null)
        {
            return;
        }
        List<string> lstTmp = new List<string>();
        HashSet<string> hashSet = new HashSet<string>();
        Action<eResourceQuality, bool> action = delegate (eResourceQuality quality, bool isSubHeadIndex)
        {
            int headTextureIndex = 0;
            List<MasterDressColorData.DressColorData> listWithCharaIdAndModelTypeOrderByDressIdAsc = masterDressColorData.GetListWithCharaIdAndModelTypeOrderByDressIdAsc(id_character, 1);
            for (int i = 0; i < listWithCharaIdAndModelTypeOrderByDressIdAsc.Count; i++)
            {
                if (id_dress == listWithCharaIdAndModelTypeOrderByDressIdAsc[i].dressId)
                {
                    headTextureIndex = listWithCharaIdAndModelTypeOrderByDressIdAsc[i].colorId;
                    break;
                }
            }
            for (int j = 0; j < 8; j++)
            {
                charaData.GetLoadCharacterAssetBundle(lstTmp, quality, j, headTextureIndex, isSubHeadIndex, null);
            }
        };
        for (eResourceQuality eResourceQuality = eResourceQuality.LowPolygon; eResourceQuality <= eResourceQuality.Rich; eResourceQuality++)
        {
            action(eResourceQuality, arg2: true);
            action(eResourceQuality, arg2: false);
        }
        foreach (string item in lstTmp)
        {
            if (!hashSet.Contains(item) && ResourcesManager.instance.ExistsAssetBundleManifest(item))
            {
                hashSet.Add(item);
            }
        }
        lstAssetBundle.AddRange(hashSet);
    }

    public static bool GetAssetBundleList(List<string> lstAssetBundle, int id_card)
    {
        bool result = false;
        MasterCardData.CardData cardData = MasterDBManager.instance.masterCardData.Get(id_card);
        if (cardData != null)
        {
            int num = cardData.openDressId;
            int id_character = cardData.charaId;
            if (!StageUtil.IsModelCommonDressId(num))
            {
                int count = lstAssetBundle.Count;
                GetAssetBundleList(lstAssetBundle, id_character, num, num);
                result = count < lstAssetBundle.Count;
            }
        }
        return result;
    }

    public static bool GetAssetBundleList(out List<string> lstAssetBundle, List<int> lstCardId)
    {
        bool result = false;
        MasterDBManager instance = MasterDBManager.instance;
        _ = ResourcesManager.instance;
        lstAssetBundle = new List<string>();
        foreach (int item in lstCardId)
        {
            MasterCardData.CardData cardData = instance.masterCardData.Get(item);
            if (cardData != null)
            {
                int num = cardData.openDressId;
                int id_character = cardData.charaId;
                if (!StageUtil.IsModelCommonDressId(num))
                {
                    GetAssetBundleList(lstAssetBundle, id_character, num, num);
                }
            }
        }
        return result;
    }

    public IEnumerator LoadAnimationAssetBundle(List<string> listAnimationName)
    {
        if (data != null && listAnimationName != null && listAnimationName.Count > 0)
        {
            //yield return StartCoroutine(ResourcesManager.instance.DownloadAssetGroup(listAnimationName, null));
            yield return StartCoroutine(AssetManager.instance.DownloadFromFilenamesAsync(listAnimationName));
            yield return AssetManager.instance.WaitDownloadProgress();
            yield return StartCoroutine(ResourcesManager.instance.LoadAssetGroupAsync(listAnimationName, null));
        }
    }

    public void Initialize(MasterCardData.CardData cardData, MasterCharaData.CharaData charaData, Camera camera, int change_dressId, eResourceQuality resourceQuality)
    {
        _createInfo.Reset();
        if (cardData != null && charaData != null)
        {
            _createInfo.charaData = new CharacterData(cardData, charaData, change_dressId);
            _createInfo.resourceQuality = resourceQuality;
            _mainCamera = camera;
            _cachedTransform = base.transform;
        }
    }

    /*
    public void Initialize(WorkCardData.CardData cardData, Camera camera, int change_dressId, eResourceQuality resourceQuality)
    {
        _createInfo.Reset();
        if (cardData != null)
        {
            _createInfo.charaData = new CharacterData(cardData, change_dressId);
            _createInfo.resourceQuality = resourceQuality;
            _mainCamera = camera;
            _cachedTransform = base.transform;
        }
    }
    */

    public void Initialize(CharaDirector cDirector, Camera camera, eResourceQuality resourceQuality)
    {
        _createInfo.Reset();
        if (cDirector != null && cDirector.characterData != null)
        {
            _charaDirector = cDirector;
            _createInfo.charaData = cDirector.characterData;
            _createInfo.resourceQuality = resourceQuality;
            _mainCamera = camera;
            _cachedTransform = base.transform;

            if (cDirector.isAppendDress)
            {
                _appendCreateInfo.Reset();
                _appendCreateInfo.charaData = cDirector.appendCharacterData;
            }
        }
    }

    public void CreateRenderCommand(Camera renderTargetCamera)
    {
        if (renderTargetCamera != null)
        {
            _renderController.CreateRenderCommand(renderTargetCamera);
        }
    }

    protected virtual void Awake()
    {
        base.name = "Character3D";
    }

    protected virtual void Update()
    {
        if (!isSetting)
        {
            UpdateCheckSetting();
            _ = isSetting;
        }
    }

    protected virtual void LateUpdate()
    {
        AlterLateUpdate();
    }

    protected virtual void FixedUpdate()
    {
        AlterFixedUpdate();
    }

    protected virtual void OnDestroy()
    {
        if (_gimmickController != null)
        {
            _gimmickController.Release();
            _gimmickController = null;
        }
        if (_renderController != null)
        {
            _renderController.Cleanup();
            _renderController = null;
        }
    }

    protected virtual void OnPreUpdateBodyTransform()
    {
    }

    public virtual void AlterLateUpdate()
    {
        if (!isSetting)
        {
            return;
        }
        RequestProcess();
        OnPreUpdateBodyTransform();
        if (_mainCamera != null && _headParts.renderer != null)
        {
            int num = (int)(_mainCamera.worldToCameraMatrix.MultiplyPoint(_bodyRoot.position).z * 10f) + -110;
            _headParts.renderer.sortingOrder = ((num > -110) ? (-110) : num);
        }
        if (_bodyParts.animation != null && _bodyParts.animation.isPlaying && _bodyRoot != null)
        {
            switch (_createInfo.positionMode)
            {
                case LiveTimelineData.CharacterPositionMode.Immobility:
                    {
                        float y = _bodyRoot.localPosition.y * _bodyScaleSubScale;
                        _bodyRoot.localPosition = new Vector3(_bodyRoot.localPosition.x, y, _bodyRoot.localPosition.z);
                        break;
                    }
                case LiveTimelineData.CharacterPositionMode.Relative:
                    {
                        Vector3 localPosition = _bodyRoot.localPosition * _bodyScaleSubScale;
                        _bodyRoot.localPosition = localPosition;
                        break;
                    }
            }
        }
        //体の位置と頭の位置を合わせる
        if ((bool)_faceNodeHead && (bool)_faceNodeNeck && (bool)_bodyNodeHead && (bool)_bodyNodeNeck)
        {
            _faceNodeNeck.rotation = _bodyNodeNeck.rotation;
            _faceNodeNeck.position = _bodyNodeNeck.position;
            _faceNodeHead.rotation = _bodyNodeHead.rotation;
            _faceNodeHead.position = _bodyNodeHead.position;


            //頭サイズが小さく首に切れ目が出来るので修正(暫定対応)
            if (charaDirector.isAppendDress)
            {
                var pos = _faceNodeNeck.position;
                pos.y -= 0.002f;
                _faceNodeNeck.position = pos;

                var pos2 = _faceNodeHead.position;
                pos2.y -= 0.002f;
                _faceNodeHead.position = pos2;
            }

        }
        OnUpdateController();
        if (_isResetCloth || _isResetClothAll)
        {
            if (_isResetClothAll)
            {
                ResetCloth(_isResetClothAll);
            }
            else
            {
                ResetCloth();
            }
        }
        if (_isWarmUpCloth || _isResetClothWarmingUp)
        {
            if (_isResetClothWarmingUp)
            {
                WarmUpCloth(0f, 0.6f);
            }
            else
            {
                WarmUpCloth();
            }
        }
        if (isClothUpdate && _clothController != null && _clothController.Update(Time.deltaTime))
        {
            _cyspringUpdateTime = Time.realtimeSinceStartup;
        }
        if (_gimmickController != null)
        {
            _gimmickController.Update();
        }
    }

    protected void UpdateCloth()
    {
        if (isSetting && Time.realtimeSinceStartup - _cyspringUpdateTime >= 1f / Application.targetFrameRate && isClothUpdate && _clothController != null && _clothController.Update(0f, isGather: false))
        {
            _cyspringUpdateTime = Time.realtimeSinceStartup;
        }
    }

    public virtual void AlterFixedUpdate()
    {
        UpdateCloth();
    }

    protected bool CheckBaseSetting()
    {
        if (_headParts == null || _bodyParts == null)
        {
            return false;
        }
        if (_headParts.animator == null || _bodyParts.animation == null || _bodyParts.animationState == null)
        {
            return false;
        }
        if (_clothController == null)
        {
            return false;
        }
        if (!_clothController.isReady)
        {
            return false;
        }
        return true;
    }

    protected virtual bool CheckSetting()
    {
        return CheckBaseSetting();
    }

    protected virtual void SetBodyBounds(float bodyHeight)
    {
        if (_bodyParts != null)
        {
            float bodyWidth = data.boundsBoxSizeData.bodyWidth;
            SkinnedMeshRenderer skinnedMeshRenderer = _bodyParts.skinnedMeshRenderer;
            if (skinnedMeshRenderer != null)
            {
                Bounds localBounds = skinnedMeshRenderer.localBounds;
                localBounds.center = new Vector3(localBounds.center.x, -0.1f, localBounds.center.z);
                localBounds.extents = new Vector3(localBounds.extents.x * bodyWidth, _charaHeight * 0.575f, localBounds.extents.z * bodyWidth);
                skinnedMeshRenderer.localBounds = localBounds;
            }
        }
    }

    protected virtual void _SetupNotReflectLayer()
    {
        if (_createInfo.resourceQuality == eResourceQuality.Rich && null != _sweatLocatorObj)
        {
            GameObjectUtility.SetLayer(27, _sweatLocatorObj.gameObject.transform);
        }
    }

    protected void UpdateController()
    {
        if ((bool)_mainCamera)
        {
            _renderController.SetCameraFOV(_mainCamera.fieldOfView);
        }
        if (_lipSyncController != null)
        {
            MaterialPack.UpdateInfo materialUpdateInfo = _renderController.materialUpdateInfo;
            MaterialPropertyBlock mtrlProp = _renderController.mtrlProp;
            materialUpdateInfo.faceFlag = _lipSyncController.currentFaceFlag;
            _renderController.Update();
            if (_lipSyncController._cheekRenderer != null)
            {
                _lipSyncController._cheekRenderer.SetPropertyBlock(_lipSyncController.GetCheekMaterialPropertyBlock(mtrlProp));
            }
        }
        else
        {
            _renderController.Update();
        }
    }

    protected virtual void OnUpdateController()
    {
        UpdateController();
    }

    protected virtual void OnLiveCharaVisible(bool bVisible)
    {
    }

    protected void ResetResource()
    {
        _isSetting = false;
        _bodyRoot = null;
        _bodyNodeNeck = null;
        _bodyNodeHead = null;
        _faceNodeNeck = null;
        _faceNodeHead = null;
        if (_gimmickController != null)
        {
            _gimmickController.Release();
            _gimmickController = null;
        }
        /*
        if (_mapTexCompImporter != null)
        {
            _mapTexCompImporter.Clear();
            _mapTexCompImporter = null;
        }
        */
        if (_clothController != null)
        {
            _clothController.Release();
            _clothController = null;
        }
        if (_renderController != null)
        {
            _renderController.Cleanup();
            _renderController = null;
        }
        foreach (Parts lstPart in _lstParts)
        {
            lstPart.Release();
        }
        _lstParts.Clear();
        _headParts = null;
        _bodyParts = null;
        _lstAccessoryParts.Clear();
        _texturePack = null;
        _createInfo.Reset();
    }

    public void SetEyeTarget(GameObject target)
    {
        if (!(_eyeTraceController == null))
        {
            _eyeTraceController._eyeObject[0].target = target;
            _eyeTraceController._eyeObject[1].target = target;
        }
    }

    public void SetMeshActive(bool isActive)
    {
        foreach (Parts lstPart in _lstParts)
        {
            lstPart.SetVisible(isActive);
        }
    }

    public void SetAnimationTime(float time, bool isPause = false)
    {
        if (_bodyParts.animationState != null)
        {
            _bodyParts.animationState.time = time;
        }
        if (isPause)
        {
            PauseAnimation(isPause: true);
        }
    }

    public float GetAnimationTime()
    {
        if (_bodyParts.animationState == null)
        {
            return 0f;
        }
        if (!(_bodyParts.animationState.time > _bodyParts.animationState.length))
        {
            return _bodyParts.animationState.time;
        }
        return _bodyParts.animationState.length;
    }

    public float GetAnimationTotalTime()
    {
        if (_bodyParts.animationState == null)
        {
            return 0f;
        }
        return _bodyParts.animationState.length;
    }

    public void PauseAnimation(bool isPause)
    {
        float speed = (isPause ? 0f : 1f);
        if (_bodyParts.animationState != null)
        {
            _bodyParts.animationState.speed = speed;
        }
        if (_headParts != null && _headParts.animator != null)
        {
            _headParts.animator.speed = speed;
        }
    }

    private void UpdateCheckSetting()
    {
        _isSetting = false;
        if (CheckSetting())
        {
            SetResetCloth();
            if (_index > 5)
            {
                GameObjectUtility.SetLayer(26, base.transform);
            }
            else
            {
                GameObjectUtility.SetLayer(22 + (_index - 1), base.transform);
            }
            _SetupNotReflectLayer();
            _isSetting = true;
            _isClothUpdate = true;
            if (_isMeshActiveAuto)
            {
                SetMeshActive(liveCharaVisible);
            }
        }
    }

    /// <summary>
    /// Timeline以外からアニメーションをセットする場合
    /// </summary>
    public void SetBodyAnimation(AnimationClip clip, bool isRemoveClip = true, WrapMode wrapMode = WrapMode.ClampForever)
    {
        _bodyParts.animationState = null;
        if (_bodyParts == null || _bodyParts.animation == null || clip == null)
        {
            return;
        }
        Animation animation = _bodyParts.animation;
        string text = clip.name;
        clip.frameRate = Application.targetFrameRate;
        if (isRemoveClip)
        {
            for (int i = 0; i < _listAnimationClipName.Count; i++)
            {
                animation.RemoveClip(_listAnimationClipName[i]);
            }
            _listAnimationClipName.Clear();
        }
        animation.AddClip(clip, text);
        _listAnimationClipName.Add(text);
        _bodyParts.animationState = animation[text];
        animation.Play(text);
        animation.wrapMode = wrapMode;
        _bodyParts.RemoveAnimator();
    }

    public void UpdateSweatLocator(ref SweatLocatorUpdateInfo updateInfo)
    {
        for (int i = 0; i < _sweatLocators.Count; i++)
        {
            SweatLocatorInfo sweatLocatorInfo = _sweatLocators[i];
            if (!(null == sweatLocatorInfo.sweatObj))
            {
                LiveTimelineKeySweatLocatorData.LocatorInfo locatorInfo = updateInfo.locatorInfo[i];
                if (sweatLocatorInfo.sweatObj.activeSelf != locatorInfo.isVisible)
                {
                    sweatLocatorInfo.sweatObj.SetActive(locatorInfo.isVisible);
                }
                if (sweatLocatorInfo.sweatObj.activeSelf)
                {
                    sweatLocatorInfo.sweatTrans.localPosition = locatorInfo.offset;
                    sweatLocatorInfo.sweatTrans.localRotation = Quaternion.Euler(locatorInfo.offsetAngle);
                }
            }
        }
        if (null != _sweatMat)
        {
            int propertyID = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.ColorPower);
            _sweatMat.SetFloat(propertyID, updateInfo.alpha);
        }
    }

    public void SetRenderQueue(eRenderQueue renderQueue, int nOffset = 0)
    {
        foreach (Parts lstPart in _lstParts)
        {
            lstPart.SetRenderQueue(renderQueue, nOffset);
        }
    }

    protected static int GetClothIndex(eCloth cloth)
    {
        return (int)cloth;
    }

    public IEnumerator SetCySpringController()
    {
        yield return _clothController.BindSpring();
        ResetCloth();
    }

    public void ResetCloth()
    {
        foreach (Parts lstPart in _lstParts)
        {
            if (lstPart.cloth != null)
            {
                lstPart.cloth.Reset(lstPart != _bodyParts);
            }
        }
        _isResetCloth = false;
    }

    public void ResetCloth(bool bForceReset)
    {
        foreach (Parts lstPart in _lstParts)
        {
            if (lstPart.cloth != null)
            {
                lstPart.cloth.Reset(bForceReset);
            }
        }
        _isResetCloth = false;
        _isResetClothAll = false;
    }

    public void SetResetCloth()
    {
        _isResetCloth = true;
    }

    public void WarmUpCloth()
    {
        WarmUpCloth(1.2f, 1.2f);
    }

    public void WarmUpCloth(float fCollisionOffSec, float fCollisionOnSec)
    {
        CySpringForceUpdate(fCollisionOffSec, fCollisionOnSec);
        _isWarmUpCloth = false;
        _isResetClothWarmingUp = false;
    }

    public void SetWarmUpCloth()
    {
        _isWarmUpCloth = true;
    }

    public void CySpringForceUpdate(float fCollisionOffSec, float fCollisionOnSec)
    {
        _clothController.ForceUpdate(fCollisionOffSec, fCollisionOnSec);
    }

    public void ResetSpecificSpringRatio(float frameRate, float gravityRatio)
    {
        for (int i = 0; i < _clothController.clothCount; i++)
        {
            CySpring spring = _clothController.GetSpring(i);
            if (spring != null)
            {
                spring.SetSpecificSpringRatio(frameRate, 1f, gravityRatio);
            }
        }
    }

    public void CheckAndSetSpringValue(int songId, float frameRate, float gravityRatio)
    {
        List<MasterLive3dcharaSpring.Live3dcharaSpring> listWithMusicIdAndCharaIdOrderByDressIdAsc = MasterDBManager.instance.masterLive3dcharaSpring.GetListWithMusicIdAndCharaIdOrderByDressIdAsc(songId, data.charaId);
        int activeDressId = data.activeDressId;
        for (int i = 0; i < listWithMusicIdAndCharaIdOrderByDressIdAsc.Count; i++)
        {
            if (activeDressId != listWithMusicIdAndCharaIdOrderByDressIdAsc[i].dressId)
            {
                continue;
            }
            float fSpringFactor = listWithMusicIdAndCharaIdOrderByDressIdAsc[i].bodyRatio * 0.01f;
            float fSpringFactor2 = listWithMusicIdAndCharaIdOrderByDressIdAsc[i].headRatio * 0.01f;
            foreach (Parts lstPart in _lstParts)
            {
                if (lstPart.cloth != null && lstPart.cloth.spring != null)
                {
                    CySpring spring = lstPart.cloth.spring;
                    if (lstPart.category == Parts.eCategory.Body)
                    {
                        spring.SetSpecificSpringRatio(frameRate, fSpringFactor, gravityRatio);
                    }
                    else
                    {
                        spring.SetSpecificSpringRatio(frameRate, fSpringFactor2, gravityRatio);
                    }
                }
            }
            break;
        }
    }

    public CySpringCollisionComponent GetUnionCollision()
    {
        return _clothController.unionCollision;
    }

    public void UnbindUnionCollision(CySpringCollisionComponent comp)
    {
        for (int i = 0; i < _clothController.clothCount; i++)
        {
            CySpring spring = _clothController.GetSpring(i);
            if (spring != null)
            {
                spring.UnbindUnionCollision(comp);
            }
        }
    }

    public void BindUnionCollision(CySpringCollisionComponent comp)
    {
        for (int i = 0; i < _clothController.clothCount; i++)
        {
            CySpring spring = _clothController.GetSpring(i);
            if (spring != null)
            {
                spring.BindUnionCollision(comp);
            }
        }
    }

    private T LoadResource<T>(string path) where T : class
    {
        return _resourcesManager.LoadObject(path) as T;
    }

    private Texture2D LoadTexture(string path)
    {
        return LoadResource<Texture2D>(path);
    }

    private bool CheckAccessoryManifest(int activeAccId, eResourceQuality resourceQuality)
    {
        bool result = true;
        if (!_createInfo.isBootDirect)
        {
            Func<int, AccessoryParts.eType, bool> func = delegate (int accIndex, AccessoryParts.eType type)
            {
                bool result2 = true;
                string accessoryModelName = CharaDirector.AssetBundle.GetAccessoryModelName(activeAccId, accIndex, type, resourceQuality);
                if (string.IsNullOrEmpty(accessoryModelName) || !_resourcesManager.ExistsAssetBundleManifest(accessoryModelName))
                {
                    result2 = false;
                }
                return result2;
            };
            if (_cabinetCache.validAcc)
            {
                if (_cabinetCache.loadMain && !func(_cabinetCache.mainAccIndex, AccessoryParts.eType.Main))
                {
                    result = false;
                }
                if (_cabinetCache.loadSub && _cabinetCache.usableSubAccIndex && !func(_cabinetCache.subAccIndex, AccessoryParts.eType.Sub))
                {
                    result = false;
                }
            }
            else
            {
                if (!func(_cabinetCache.mainAccIndex, AccessoryParts.eType.Main))
                {
                    result = false;
                }
                if (_cabinetCache.usableSubAccIndex && !func(_cabinetCache.subAccIndex, AccessoryParts.eType.Sub))
                {
                    result = false;
                }
            }
        }
        return result;
    }

    private Parts CreateParts(string path, Parts.eCategory category, string partsCode, int index, TexturePack texturePack)
    {
        Parts result = null;
        GameObject gameObject = LoadResource<GameObject>(path);
        if (gameObject != null)
        {
            result = Parts.CreateParts(category, gameObject, partsCode, index, texturePack);
        }
        return result;
    }

    public IEnumerator CreateResource()
    {
        _resourcesManager = ResourcesManager.instance;
        if (_createInfo.charaData == null || _createInfo.charaData.charaId == 0)
        {
            yield break;
        }
        _createInfo.lstPartsCode.Sort();
        OnPrepareTask();
        while (_queTask.Count > 0)
        {
            _ = Time.time;
            KeyValuePair<string, Func<IEnumerator>> keyValuePair = _queTask.Dequeue();
            if (keyValuePair.Value != null)
            {
                yield return keyValuePair.Value();
            }
        }
    }

    private void ResetLoadProgress()
    {
        if (_headParts != null)
        {
            _headParts.Release();
            _headParts = null;
        }
        foreach (BodyParts lstBodyPart in _lstBodyParts)
        {
            lstBodyPart?.Release();
        }
        _bodyParts = null;
        _lstBodyParts.Clear();
        foreach (AccessoryParts lstAccessoryPart in _lstAccessoryParts)
        {
            lstAccessoryPart?.Release();
        }
        _lstAccessoryParts.Clear();
        OnPrepareTask();
    }

    private void PushTask(string key, Func<IEnumerator> fnTask)
    {
        KeyValuePair<string, Func<IEnumerator>> item = new KeyValuePair<string, Func<IEnumerator>>(key, fnTask);
        _queTask.Enqueue(item);
    }

    protected virtual void OnPrepareTask()
    {
        _queTask.Clear();
        PushTask("Character3DBase.InitializeCabinetCache", InitializeCabinetCache);
        PushTask("Character3DBase.CreateTexturePackTask", CreateTexturePackTask);
        PushTask("Character3DBase.CreatePartsTask", CreatePartsTask);
        PushTask("Character3DBase.LoadCySpringDataTask", LoadCySpringDataTask);
        PushTask("Character3DBase.AssembleTask", AssembleTask);
        PushTask("Character3DBase.InstallTask", InstallTask);
        PushTask("Character3DBase.LoadOptionalDataTask", LoadOptionalDataTask);
    }

    private IEnumerator InitializeCabinetCache()
    {
        if (_cabinetCache != null)
        {
            _cabinetCache.Initialize(_createInfo.activeDressId, _createInfo.charaData.charaId);
            if (_cabinetCache.headIndex != 0)
            {
                _createInfo.isSubHeadIndex = false;
                _createInfo.headIndex = _cabinetCache.headIndex;
            }
        }
        yield break;
    }

    private IEnumerator CreateTexturePackTask()
    {
        if (_texturePack == null)
        {
            _texturePack = new TexturePack();
        }
        else
        {
            _texturePack.Clear();
        }
        //ImportCompositeTexture();
        LoadHeadTexture(_texturePack);
        if (_createInfo.CheckAssemblable() && !_createInfo.mergeMaterial)
        {
            foreach (string item in _createInfo.lstPartsCode)
            {
                LoadBodyTexture(_texturePack, item);
            }
        }
        else
        {
            LoadBodyTexture(_texturePack);
        }
        if (_createInfo.loadAccessory && !charaDirector.isAppendDress) //アペンドの時はアクセサリをオフにする
        {
            LoadAccessoryTexture(_texturePack);
        }
        yield break;
    }
    /*
    private void ImportCompositeTexture()
    {
        TextureComposite.Meta textureCompositeMeta = _createInfo.textureCompositeMeta;
        if (textureCompositeMeta != null && textureCompositeMeta.valid && textureCompositeMeta.isExistInfo)
        {
            _mapTexCompImporter = new Dictionary<Parts.eCategory, TextureComposite.Importer>();
            List<Parts.eCategory> partsList = textureCompositeMeta.GetPartsList();
            TextureCompositeWorker.Task.eType import = TextureCompositeWorker.Task.eType.DiffuseColor;
            if (_createInfo.resourceQuality == eResourceQuality.Rich)
            {
                import = (TextureCompositeWorker.Task.eType)14;
            }
            for (int i = 0; i < partsList.Count; i++)
            {
                Parts.eCategory eCategory = partsList[i];
                TextureComposite.Importer importer = new TextureComposite.Importer(eCategory);
                importer.Import(textureCompositeMeta, import);
                _mapTexCompImporter.Add(eCategory, importer);
            }
        }
    }
    */
    private void LoadHeadTexture(TexturePack texturePack)
    {
        eResourceQuality resourceQuality = _createInfo.resourceQuality;
        bool isSubHeadIndex = _createInfo.isSubHeadIndex;
        int charaId = data.charaId;
        int dressId = _createInfo.activeDressId;
        int headIndex = _createInfo.headIndex;
        int headTextureIndex = _createInfo.headTextureIndex;
        if (_cabinetCache.headTexIndex != 0)
        {
            headTextureIndex = _cabinetCache.headTexIndex;
        }

        MultiTextures multiTextures = texturePack.CreateMultiTextures(TexturePack.eCategory.Head);
        MultiTextures multiTextures2 = texturePack.CreateMultiTextures(TexturePack.eCategory.Object);
        MultiTextures multiTextures3 = texturePack.CreateMultiTextures(TexturePack.eCategory.Cheek);
        Func<MultiTextures.eMap, Texture2D> func = delegate (MultiTextures.eMap map)
        {
            string faceTexturePath;
            if (isLoadFromDirector)
            {
                faceTexturePath = charaDirector.GetFaceTexturePath(map);
            }
            else
            {
                faceTexturePath = CharaDirector.Asset.GetFaceTexturePath(charaId, dressId, headIndex, headTextureIndex, isSubHeadIndex, map, resourceQuality);
            }
            return LoadTexture(faceTexturePath);
        };
        multiTextures.diffuse = func(MultiTextures.eMap.Diffuse);
        if (resourceQuality == eResourceQuality.Rich)
        {
            multiTextures.specular = func(MultiTextures.eMap.Specular);
            multiTextures.control = func(MultiTextures.eMap.Control);
        }
        Func<MultiTextures.eMap, Texture2D> func2 = delegate (MultiTextures.eMap map)
        {
            string objectTexturePath;
            if (isLoadFromDirector)
            {
                objectTexturePath = charaDirector.GetObjectTexturePath(map);
            }
            else
            {
                objectTexturePath = CharaDirector.Asset.GetObjectTexturePath(charaId, dressId, headIndex, headTextureIndex, isSubHeadIndex, map, resourceQuality);
            }
            return LoadTexture(objectTexturePath);
        };
        if (!StageUtil.IsModelCommonDressId(dressId) || _cabinetCache.useHeadObjTex)
        {
            multiTextures2.diffuse = func2(MultiTextures.eMap.Diffuse);
            if (resourceQuality == eResourceQuality.Rich)
            {
                multiTextures2.specular = func2(MultiTextures.eMap.Specular);
                multiTextures2.control = func2(MultiTextures.eMap.Control);
            }
        }
        Func<MultiTextures.eMap, Texture2D> func3 = delegate (MultiTextures.eMap map)
        {
            string cheekTexturePath;
            if (isLoadFromDirector)
            {
                cheekTexturePath = charaDirector.GetCheekTexturePath(map);
            }
            else
            {
                cheekTexturePath = CharaDirector.Asset.GetCheekTexturePath(charaId, dressId, headIndex, isSubHeadIndex, map, resourceQuality);
            }
            return LoadTexture(cheekTexturePath);
        };
        multiTextures3.diffuse = func3(MultiTextures.eMap.Diffuse);
        multiTextures3.alphaMask = func3(MultiTextures.eMap.AlphaMask);
    }

    private void LoadBodyTexture(TexturePack texturePack, string partsCode = null)
    {
        //アペンドの場合
        if (charaDirector.isAppendDress)
        {
            LoadAppendBodyTexture(texturePack, partsCode);
            return;
        }

        int dressId = _createInfo.activeDressId;
        int activeAccessoryId = _createInfo.activeAccessoryId;
        int skinId = data.skinId;
        int charaId = data.charaId;
        int colorId = data.colorId;
        int bustId = data.bustId;
        bool arg = true;
        if (_createInfo.CheckAssemblable() && !_createInfo.isBootDirect && !_createInfo.mergeMaterial)
        {
            arg = false;
        }
        MultiTextures bodyMultiTextures = texturePack.CreateMultiTextures(TexturePack.eCategory.Body, partsCode);
        Action<MultiTextures.eMap, bool> action = delegate (MultiTextures.eMap map, bool bLoad)
        {
            if (!bodyMultiTextures.CheckTexture(map) && bLoad)
            {
                string bodyTexturePath;
                if (isLoadFromDirector)
                {
                    bodyTexturePath = charaDirector.GetBodyTexturePath(partsCode, map);
                }
                else
                {
                    bodyTexturePath = CharaDirector.Asset.GetBodyTexturePath(charaId, dressId, skinId, bustId, colorId, _cabinetCache.bodyIndex, partsCode, _createInfo.resourceQuality, map);
                }
                Texture2D texture = (string.IsNullOrEmpty(bodyTexturePath) ? null : LoadTexture(bodyTexturePath));
                bodyMultiTextures.SetTexture(map, texture);
            }
        };
        //SetCompositeTexture(Parts.eCategory.Body, ref bodyMultiTextures);
        action(MultiTextures.eMap.Diffuse, arg);
        if (_createInfo.resourceQuality == eResourceQuality.Rich)
        {
            action(MultiTextures.eMap.Specular, arg);
            action(MultiTextures.eMap.Control, arg2: true);
        }
    }

    /// <summary>
    /// アペンドモデルの構築
    /// </summary>
    private void LoadAppendBodyTexture(TexturePack texturePack, string partsCode = null)
    {
        int dressId = _createInfo.activeDressId;
        int activeAccessoryId = _createInfo.activeAccessoryId;
        int skinId = data.skinId;
        int charaId = data.charaId;
        int colorId = data.colorId;
        int bustId = data.bustId;
        bool arg = true;
        if (_createInfo.CheckAssemblable() && !_createInfo.isBootDirect && !_createInfo.mergeMaterial)
        {
            arg = false;
        }
        MultiTextures bodyMultiTextures = texturePack.CreateMultiTextures(TexturePack.eCategory.Body, partsCode);
        Action<MultiTextures.eMap, bool> action = delegate (MultiTextures.eMap map, bool bLoad)
        {
            if (!bodyMultiTextures.CheckTexture(map) && bLoad)
            {
                string bodyTexturePath;
                if (isLoadFromDirector)
                {
                    bodyTexturePath = charaDirector.GetAppendBodyTexturePath(partsCode, map);
                }
                else
                {
                    bodyTexturePath = CharaDirector.Asset.GetBodyTexturePath(charaId, dressId, skinId, bustId, colorId, _cabinetCache.bodyTexIndex, partsCode, _createInfo.resourceQuality, map);
                }
                Texture2D texture = (string.IsNullOrEmpty(bodyTexturePath) ? null : LoadTexture(bodyTexturePath));
                bodyMultiTextures.SetTexture(map, texture);
            }
        };
        //SetCompositeTexture(Parts.eCategory.Body, ref bodyMultiTextures);
        action(MultiTextures.eMap.Diffuse, arg);
        if (_createInfo.resourceQuality == eResourceQuality.Rich)
        {
            action(MultiTextures.eMap.Specular, arg);
            action(MultiTextures.eMap.Control, arg2: true);
        }
    }


    private void LoadAccessoryTexture(TexturePack texturePack)
    {
        eResourceQuality resourceQuality = _createInfo.resourceQuality;
        bool useAssetBundle = _createInfo.useAssetBundle;
        int activeAccId = _createInfo.activeAccessoryId;
        int colorId = data.colorId;
        bool accTexDiversity = false;
        int accTexIndex = 0;
        if (_cabinetCache.validAcc)
        {
            accTexDiversity = _cabinetCache.accTexDiversity;
            accTexIndex = _cabinetCache.accTexIndex;
        }
        MultiTextures accTextures = texturePack.CreateMultiTextures(TexturePack.eCategory.Accessory);
        if (!CheckAccessoryManifest(activeAccId, resourceQuality))
        {
            return;
        }
        Action<MultiTextures.eMap> action = delegate (MultiTextures.eMap map)
        {
            Texture2D texture = null;
            if (!(accTextures.GetTexture(map) != null))
            {
                string accessoryTexturePath;
                if (isLoadFromDirector)
                {
                    accessoryTexturePath = charaDirector.GetAccessoryTexturePath(map);
                }
                else
                {
                    accessoryTexturePath = CharaDirector.Asset.GetAccessoryTexturePath(activeAccId, colorId, accTexDiversity, accTexIndex, resourceQuality, map);
                }
                if (!string.IsNullOrEmpty(accessoryTexturePath))
                {
                    texture = LoadTexture(accessoryTexturePath);
                }
                accTextures.SetTexture(map, texture);
            }
        };
        //SetCompositeTexture(Parts.eCategory.Accessory, ref accTextures);
        action(MultiTextures.eMap.Diffuse);
        if (resourceQuality == eResourceQuality.Rich)
        {
            action(MultiTextures.eMap.Specular);
            action(MultiTextures.eMap.Control);
        }
    }

    /*
    private void SetCompositeTexture(Parts.eCategory partsCategory, ref MultiTextures multiTexture)
    {
        TextureComposite.Importer value = null;
        if (_mapTexCompImporter != null && _mapTexCompImporter.TryGetValue(partsCategory, out value))
        {
            if (_createInfo.resourceQuality == eResourceQuality.Rich)
            {
                multiTexture.diffuse = value.GetTexture(TextureCompositeWorker.Task.eType.DiffuseColorHQ);
                multiTexture.specular = value.GetTexture(TextureCompositeWorker.Task.eType.SpecularColor);
                multiTexture.control = value.GetTexture(TextureCompositeWorker.Task.eType.ControlMap);
            }
            else
            {
                multiTexture.diffuse = value.GetTexture(TextureCompositeWorker.Task.eType.DiffuseColor);
                multiTexture.specular = null;
                multiTexture.control = null;
            }
        }
        else
        {
            multiTexture.diffuse = null;
            multiTexture.specular = null;
            multiTexture.control = null;
        }
    }
    */
    private IEnumerator CreatePartsTask()
    {
        CreateHeadParts(_texturePack);
        CreateBodyParts(_texturePack);
        if (_createInfo.loadAccessory && !charaDirector.isAppendDress) //アペンドの時はアクセサリをオフにする
        {
            CreateAccessoryParts(_texturePack);
        }
        yield break;
    }

    private void CreateHeadParts(TexturePack texturePack)
    {
        string headModelPath;
        if (isLoadFromDirector)
        {
            headModelPath = charaDirector.GetHeadModelPath();
        }
        else
        {
            headModelPath = CharaDirector.Asset.GetHeadModelPath(_createInfo.charaData.charaId, _createInfo.activeDressId, _createInfo.headIndex, _createInfo.isSubHeadIndex, _createInfo.resourceQuality);
        }
        _headParts = CreateParts(headModelPath, Parts.eCategory.Head, null, 0, texturePack) as HeadParts;
    }

    private void CreateBodyParts(TexturePack texturePack)
    {
        //appendモデル呼び出し
        if (charaDirector.isAppendDress)
        {
            CreateAppendBodyParts(texturePack);
            return;
        }

        _lstBodyParts.Clear();
        Func<string, BodyParts> func = delegate (string partsCode)
        {
            string bodyModelPath;
            if (isLoadFromDirector)
            {
                bodyModelPath = charaDirector.GetBodyModelPath(partsCode);
            }
            else
            {
                bodyModelPath = CharaDirector.Asset.GetBodyModelPath(_createInfo.activeDressId, data.heightId, data.weightId, data.bustId, _cabinetCache.bodyIndex, partsCode, _createInfo.resourceQuality);
            }
            return CreateParts(bodyModelPath, Parts.eCategory.Body, partsCode, _cabinetCache.bodyIndex, texturePack) as BodyParts;
        };
        if (_createInfo.CheckAssemblable())
        {
            List<string> lstPartsCode = _createInfo.lstPartsCode;
            for (int i = 0; i < lstPartsCode.Count; i++)
            {
                string text = lstPartsCode[i];
                BodyParts bodyParts = func(text);
                if (bodyParts == null && Parts.GetPackageIndex(text) > 0)
                {
                    text = Parts.GetPartsCodeWithoutPackageIndex(text);
                    bodyParts = func(text);
                }
                if (bodyParts != null)
                {
                    _lstBodyParts.Add(bodyParts);
                    lstPartsCode[i] = text;
                }
            }
        }
        else
        {
            BodyParts bodyParts2 = func(string.Empty);
            if (bodyParts2 != null)
            {
                _lstBodyParts.Add(bodyParts2);
            }
        }
    }

    /// <summary>
    /// アペンドキャラのボディモデルを呼び出す
    /// </summary>
    /// <param name="texturePack"></param>
    private void CreateAppendBodyParts(TexturePack texturePack)
    {
        _lstBodyParts.Clear();
        Func<string, BodyParts> func = delegate (string partsCode)
        {
            string bodyModelPath;
            if (isLoadFromDirector)
            {
                bodyModelPath = charaDirector.GetAppendBodyModelPath(partsCode);
            }
            else
            {
                //Director経由ではないときは通常モデル呼び出し
                bodyModelPath = CharaDirector.Asset.GetBodyModelPath(_createInfo.activeDressId, data.heightId, data.weightId, data.bustId, _cabinetCache.bodyIndex, partsCode, _createInfo.resourceQuality);
            }
            return CreateParts(bodyModelPath, Parts.eCategory.Body, partsCode, _cabinetCache.bodyIndex, texturePack) as BodyParts;
        };
        if (_createInfo.CheckAssemblable())
        {
            List<string> lstPartsCode = _createInfo.lstPartsCode;
            for (int i = 0; i < lstPartsCode.Count; i++)
            {
                string text = lstPartsCode[i];
                BodyParts bodyParts = func(text);
                if (bodyParts == null && Parts.GetPackageIndex(text) > 0)
                {
                    text = Parts.GetPartsCodeWithoutPackageIndex(text);
                    bodyParts = func(text);
                }
                if (bodyParts != null)
                {
                    _lstBodyParts.Add(bodyParts);
                    lstPartsCode[i] = text;
                }
            }
        }
        else
        {
            BodyParts bodyParts2 = func(string.Empty);
            if (bodyParts2 != null)
            {
                _lstBodyParts.Add(bodyParts2);
            }
        }
    }


    private void CreateAccessoryParts(TexturePack texturePack)
    {
        _lstAccessoryParts.Clear();
        eResourceQuality resourceQuality = _createInfo.resourceQuality;
        _ = _createInfo.useAssetBundle;
        int activeAccId = _createInfo.activeAccessoryId;
        if (!CheckAccessoryManifest(activeAccId, resourceQuality))
        {
            return;
        }
        Action<AccessoryParts.eType, AccParam.Info> action = delegate (AccessoryParts.eType type, AccParam.Info accInfo)
        {
            int accIndex = 0;
            if (_cabinetCache.validAcc)
            {
                switch (type)
                {
                    case AccessoryParts.eType.Main:
                        accIndex = _cabinetCache.mainAccIndex;
                        break;
                    case AccessoryParts.eType.Sub:
                        accIndex = _cabinetCache.subAccIndex;
                        break;
                }
            }
            else if (accInfo != null && type == AccessoryParts.eType.Sub)
            {
                accIndex = accInfo.idxSub;
            }
            string accessoryModelPath;
            if (isLoadFromDirector)
            {
                accessoryModelPath = charaDirector.GetAccessoryModelPath(type, accIndex);
            }
            else
            {
                accessoryModelPath = CharaDirector.Asset.GetAccessoryModelPath(activeAccId, accIndex, resourceQuality, type);
            }
            AccessoryParts accessoryParts = CreateParts(accessoryModelPath, Parts.eCategory.Accessory, null, accIndex, texturePack) as AccessoryParts;
            if (accessoryParts != null)
            {
                accessoryParts.Initialize(type, accInfo);
                _lstAccessoryParts.Add(accessoryParts);
            }
        };
        string accessoryParameterPath;
        if (isLoadFromDirector)
        {
            accessoryParameterPath = charaDirector.GetAccessoryParameterPath();
        }
        else
        {
            accessoryParameterPath = CharaDirector.Asset.GetAccessoryParameterPath(_createInfo.activeAccessoryId);
        }
        AccParam accParam = _resourcesManager.LoadObject(accessoryParameterPath) as AccParam;
        if (!(accParam != null))
        {
            return;
        }
        accParam.BuildMap();
        AccParam.Info info = accParam.GetInfo(data.charaId);
        if (_cabinetCache.validAcc)
        {
            if (_cabinetCache.loadMain)
            {
                action(AccessoryParts.eType.Main, info);
            }
            if (_cabinetCache.loadSub && _cabinetCache.usableSubAccIndex)
            {
                action(AccessoryParts.eType.Sub, info);
            }
        }
        else
        {
            action(AccessoryParts.eType.Main, info);
            if (info != null && info.idxSub != 0)
            {
                action(AccessoryParts.eType.Sub, info);
            }
        }
        foreach (AccessoryParts lstAccessoryPart in _lstAccessoryParts)
        {
            if (string.IsNullOrEmpty(lstAccessoryPart.attachmentNode))
            {
                lstAccessoryPart.attachmentNode = accParam._attachmentNode;
            }
        }
    }

    private IEnumerator LoadOptionalDataTask()
    {
        if (Director.instance != null && Director.instance.IsEnableFlare())
        {
            yield return LoadFlareData();
        }
        if (_createInfo.resourceQuality == eResourceQuality.Rich)
        {
            yield return LoadSweat();
        }
    }

    private IEnumerator LoadFlareData()
    {
        if (charaDirector.isAppendDress)
        {
            yield return LoadAppendFlareData();
            yield break;
        }

        Action<GameObject, string> action = delegate (GameObject objTarget, string path)
        {
            if (objTarget != null && !string.IsNullOrEmpty(path))
            {
                CharacterFlareCollisionParameter characterFlareCollisionParameter = _resourcesManager.LoadObject(path) as CharacterFlareCollisionParameter;
                if (characterFlareCollisionParameter != null)
                {
                    CharacterFlareCollision.AddCollisionToObject(objTarget, characterFlareCollisionParameter);
                }
            }
        };
        string headFlareColliderPath;
        if (isLoadFromDirector)
        {
            headFlareColliderPath = charaDirector.GetHeadFlareColliderPath();
        }
        else
        {
            headFlareColliderPath = CharaDirector.Asset.GetHeadFlareColliderPath(data.charaId, _createInfo.activeDressId, _createInfo.headIndex, _createInfo.isSubHeadIndex);
        }
        action(_headParts.gameObject, headFlareColliderPath);
        string bodyFlareColliderPath;
        if (isLoadFromDirector)
        {
            bodyFlareColliderPath = charaDirector.GetBodyFlareColliderPath();
        }
        else
        {
            bodyFlareColliderPath = CharaDirector.Asset.GetBodyFlareColliderPath(_createInfo.activeDressId, data.heightId, data.weightId, data.bustId, _cabinetCache.bodyIndex);
        }
        action(_bodyParts.gameObject, bodyFlareColliderPath);
        foreach (AccessoryParts lstAccessoryPart in _lstAccessoryParts)
        {
            string accessoryFlareColliderPath;
            if (isLoadFromDirector)
            {
                accessoryFlareColliderPath = charaDirector.GetAccessoryFlareColliderPath(lstAccessoryPart.index, lstAccessoryPart.type);
            }
            else
            {
                accessoryFlareColliderPath = CharaDirector.Asset.GetAccessoryFlareColliderPath(_createInfo.activeAccessoryId, lstAccessoryPart.index, lstAccessoryPart.type);
            }
            action(lstAccessoryPart.gameObject, accessoryFlareColliderPath);
        }
        yield break;
    }

    /// <summary>
    /// アペンドモデルのフレア値を取得
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadAppendFlareData()
    {
        Action<GameObject, string> action = delegate (GameObject objTarget, string path)
        {
            if (objTarget != null && !string.IsNullOrEmpty(path))
            {
                CharacterFlareCollisionParameter characterFlareCollisionParameter = _resourcesManager.LoadObject(path) as CharacterFlareCollisionParameter;
                if (characterFlareCollisionParameter != null)
                {
                    CharacterFlareCollision.AddCollisionToObject(objTarget, characterFlareCollisionParameter);
                }
            }
        };
        string headFlareColliderPath;
        if (isLoadFromDirector)
        {
            headFlareColliderPath = charaDirector.GetHeadFlareColliderPath();
        }
        else
        {
            headFlareColliderPath = CharaDirector.Asset.GetHeadFlareColliderPath(data.charaId, _createInfo.activeDressId, _createInfo.headIndex, _createInfo.isSubHeadIndex);
        }
        action(_headParts.gameObject, headFlareColliderPath);
        string bodyFlareColliderPath;
        if (isLoadFromDirector)
        {
            bodyFlareColliderPath = charaDirector.GetAppendBodyFlareColliderPath();
        }
        else
        {
            bodyFlareColliderPath = CharaDirector.Asset.GetBodyFlareColliderPath(_createInfo.activeDressId, data.heightId, data.weightId, data.bustId, _cabinetCache.bodyIndex);
        }
        action(_bodyParts.gameObject, bodyFlareColliderPath);
        foreach (AccessoryParts lstAccessoryPart in _lstAccessoryParts)
        {
            string accessoryFlareColliderPath;
            if (isLoadFromDirector)
            {
                accessoryFlareColliderPath = charaDirector.GetAccessoryFlareColliderPath(lstAccessoryPart.index, lstAccessoryPart.type);
            }
            else
            {
                accessoryFlareColliderPath = CharaDirector.Asset.GetAccessoryFlareColliderPath(_createInfo.activeAccessoryId, lstAccessoryPart.index, lstAccessoryPart.type);
            }
            action(lstAccessoryPart.gameObject, accessoryFlareColliderPath);
        }
        yield break;
    }

    private IEnumerator LoadSweat()
    {
        string sweatLocatorPath;
        if (isLoadFromDirector)
        {
            sweatLocatorPath = charaDirector.GetSweatLocatorPath();
        }
        else
        {
            sweatLocatorPath = CharaDirector.Asset.GetSweatLocatorPath(data.charaId, _createInfo.activeDressId, _createInfo.headIndex, _createInfo.isSubHeadIndex);
        }
        GameObject gameObject = _resourcesManager.LoadObject(sweatLocatorPath) as GameObject;
        if (null != gameObject)
        {
            _sweatLocatorObj = UnityEngine.Object.Instantiate(gameObject);
            _sweatLocatorObj.transform.SetParent(_headParts.GetTransform(HeadParts.eTransform.Head), worldPositionStays: false);
            _sweatLocators.Clear();
            int childCount = _sweatLocatorObj.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                SweatLocatorInfo sweatLocatorInfo = new SweatLocatorInfo();
                sweatLocatorInfo.locator = _sweatLocatorObj.transform.GetChild(i);
                sweatLocatorInfo.sweatObj = null;
                _sweatLocators.Add(sweatLocatorInfo);
            }
            _sweatLocators.Sort((SweatLocatorInfo a, SweatLocatorInfo b) => string.Compare(a.locator.name, b.locator.name));
        }
        yield break;
    }

    private IEnumerator LoadCySpringDataTask()
    {
        if (charaDirector.isAppendDress)
        {
            yield return LoadAppendCySpringDataTask();
            yield break;
        }

        Action<Parts, string, string, string> action = delegate (Parts parts, string pathColAsset, string pathPrmAsset, string frequentRootParentName)
        {
            if (!string.IsNullOrEmpty(pathColAsset) && !string.IsNullOrEmpty(pathPrmAsset))
            {
                CySpringCollisionDataAsset cySpringCollisionDataAsset = _resourcesManager.LoadObject<CySpringCollisionDataAsset>(pathColAsset);
                CySpringParamDataAsset cySpringParamDataAsset = _resourcesManager.LoadObject<CySpringParamDataAsset>(pathPrmAsset);
                if (cySpringCollisionDataAsset != null && cySpringParamDataAsset != null)
                {
                    parts.cloth = new ClothController.Cloth(parts.category, cySpringCollisionDataAsset, cySpringParamDataAsset, frequentRootParentName);
                }
            }
        };
        int activeDressId = _createInfo.activeDressId;
        int activeAccessoryId = _createInfo.activeAccessoryId;
        int headIndex = _createInfo.headIndex;
        bool isSubHeadIndex = _createInfo.isSubHeadIndex;
        if (_headParts != null)
        {
            string headCollisionPath;
            string headClothPath;
            if (isLoadFromDirector)
            {
                headCollisionPath = charaDirector.GetHeadCollisionPath();
                headClothPath = charaDirector.GetHeadClothPath();
            }
            else
            {
                headCollisionPath = CharaDirector.Asset.GetHeadCollisionPath(data.charaId, activeDressId, headIndex, isSubHeadIndex);
                headClothPath = CharaDirector.Asset.GetHeadClothPath(data.charaId, activeDressId, headIndex, isSubHeadIndex);
            }
            action(_headParts, headCollisionPath, headClothPath, "Head");
        }
        foreach (BodyParts lstBodyPart in _lstBodyParts)
        {
            string bodyCollisionPath;
            string bodyClothPath;
            if (isLoadFromDirector)
            {
                bodyCollisionPath = charaDirector.GetBodyCollisionPath();
                bodyClothPath = charaDirector.GetBodyClothPath();
            }
            else
            {
                bodyCollisionPath = CharaDirector.Asset.GetBodyCollisionPath(activeDressId, _cabinetCache.bodyIndex);
                bodyClothPath = CharaDirector.Asset.GetBodyClothPath(activeDressId, _cabinetCache.bodyIndex);
            }
            action(lstBodyPart, bodyCollisionPath, bodyClothPath, "Hip");
        }
        foreach (AccessoryParts lstAccessoryPart in _lstAccessoryParts)
        {
            string accessoryCollisionPath;
            string accessoryClothPath;
            if (isLoadFromDirector)
            {
                accessoryCollisionPath = charaDirector.GetAccessoryCollisionPath(lstAccessoryPart.index, lstAccessoryPart.type);
                accessoryClothPath = charaDirector.GetAccessoryClothPath(lstAccessoryPart.index, lstAccessoryPart.type);
            }
            else
            {
                accessoryCollisionPath = CharaDirector.Asset.GetAccessoryCollisionPath(activeAccessoryId, lstAccessoryPart.index, lstAccessoryPart.type);
                accessoryClothPath = CharaDirector.Asset.GetAccessoryClothPath(activeAccessoryId, lstAccessoryPart.index, lstAccessoryPart.type);
            }
            action(lstAccessoryPart, accessoryCollisionPath, accessoryClothPath, string.Empty);
        }
        yield break;
    }

    private IEnumerator LoadAppendCySpringDataTask()
    {
        Action<Parts, string, string, string> action = delegate (Parts parts, string pathColAsset, string pathPrmAsset, string frequentRootParentName)
        {
            if (!string.IsNullOrEmpty(pathColAsset) && !string.IsNullOrEmpty(pathPrmAsset))
            {
                CySpringCollisionDataAsset cySpringCollisionDataAsset = _resourcesManager.LoadObject<CySpringCollisionDataAsset>(pathColAsset);
                CySpringParamDataAsset cySpringParamDataAsset = _resourcesManager.LoadObject<CySpringParamDataAsset>(pathPrmAsset);
                if (cySpringCollisionDataAsset != null && cySpringParamDataAsset != null)
                {
                    parts.cloth = new ClothController.Cloth(parts.category, cySpringCollisionDataAsset, cySpringParamDataAsset, frequentRootParentName);
                }
            }
        };
        int activeDressId = _createInfo.activeDressId;
        int activeAccessoryId = _createInfo.activeAccessoryId;
        int headIndex = _createInfo.headIndex;
        bool isSubHeadIndex = _createInfo.isSubHeadIndex;
        if (_headParts != null)
        {
            string headCollisionPath;
            string headClothPath;
            if (isLoadFromDirector)
            {
                headCollisionPath = charaDirector.GetHeadCollisionPath();
                headClothPath = charaDirector.GetHeadClothPath();
            }
            else
            {
                headCollisionPath = CharaDirector.Asset.GetHeadCollisionPath(data.charaId, activeDressId, headIndex, isSubHeadIndex);
                headClothPath = CharaDirector.Asset.GetHeadClothPath(data.charaId, activeDressId, headIndex, isSubHeadIndex);
            }
            action(_headParts, headCollisionPath, headClothPath, "Head");
        }
        foreach (BodyParts lstBodyPart in _lstBodyParts)
        {
            string bodyCollisionPath;
            string bodyClothPath;
            if (isLoadFromDirector)
            {
                bodyCollisionPath = charaDirector.GetAppendBodyCollisionPath();
                bodyClothPath = charaDirector.GetAppendBodyClothPath();
            }
            else
            {
                bodyCollisionPath = CharaDirector.Asset.GetBodyCollisionPath(activeDressId, _cabinetCache.bodyIndex);
                bodyClothPath = CharaDirector.Asset.GetBodyClothPath(activeDressId, _cabinetCache.bodyIndex);
            }
            action(lstBodyPart, bodyCollisionPath, bodyClothPath, "Hip");
        }
        foreach (AccessoryParts lstAccessoryPart in _lstAccessoryParts)
        {
            string accessoryCollisionPath;
            string accessoryClothPath;
            if (isLoadFromDirector)
            {
                accessoryCollisionPath = charaDirector.GetAccessoryCollisionPath(lstAccessoryPart.index, lstAccessoryPart.type);
                accessoryClothPath = charaDirector.GetAccessoryClothPath(lstAccessoryPart.index, lstAccessoryPart.type);
            }
            else
            {
                accessoryCollisionPath = CharaDirector.Asset.GetAccessoryCollisionPath(activeAccessoryId, lstAccessoryPart.index, lstAccessoryPart.type);
                accessoryClothPath = CharaDirector.Asset.GetAccessoryClothPath(activeAccessoryId, lstAccessoryPart.index, lstAccessoryPart.type);
            }
            action(lstAccessoryPart, accessoryCollisionPath, accessoryClothPath, string.Empty);
        }
        yield break;
    }

    private IEnumerator AssembleTask()
    {
        _bodyParts = _lstBodyParts[0];
        if (_createInfo.CheckAssemblable())
        {
            List<BodyParts> lstRemove = new List<BodyParts>();
            foreach (BodyParts slaveParts in _lstBodyParts)
            {
                yield return _bodyParts.Assemble(slaveParts, _createInfo.mergeMaterial);
                if (_bodyParts.assembled)
                {
                    lstRemove.Add(slaveParts);
                }
            }
            foreach (BodyParts item in lstRemove)
            {
                item.Release();
                _lstBodyParts.Remove(item);
            }
        }
        _lstParts.Clear();
        _lstParts.Add(_headParts);
        foreach (BodyParts lstBodyPart in _lstBodyParts)
        {
            _lstParts.Add(lstBodyPart);
        }
        foreach (AccessoryParts lstAccessoryPart in _lstAccessoryParts)
        {
            _lstParts.Add(lstAccessoryPart);
        }
    }

    private IEnumerator InstallTask()
    {
        if (_lstParts.Count != 0)
        {
            TransformCaching();
            SetScale();
            SetHierarchy();
            _renderController = new RenderController();
            _renderController.Initialize(_lstParts, this, _createInfo.renderTargetCameraList);
            yield return InitializeFacialController();
            yield return InitializeClothContoller();
            InitializeGimmickController();
            InitializeRequestExecutor();
        }
    }

    private void TransformCaching()
    {
        if (_headParts != null)
        {
            _faceNodeHead = _headParts.GetTransform(HeadParts.eTransform.Head);
            _faceNodeNeck = _headParts.GetTransform(HeadParts.eTransform.Neck);
        }
        if (_bodyParts != null)
        {
            _bodyNodeHead = _bodyParts.GetTransform(BodyParts.eTransform.Head);
            _bodyNodeNeck = _bodyParts.GetTransform(BodyParts.eTransform.Neck);
            _bodyRoot = _bodyParts.GetTransform(BodyParts.eTransform.Root);
        }
        _effectSpot = new TransformCollector(base.gameObject.transform);
    }

    private void SetScale()
    {
        float num = 0f;
        Transform transform = _headParts.GetTransform(HeadParts.eTransform.Head_Height);
        if (transform != null)
        {
            num = transform.position.y;
        }

        //体キャラでの_bodySubScale
        float headSubScale = 1f;

        if (charaDirector.isAppendDress)
        {
            //アペンドキャラ
            float body_BodySize = bodyNodeNeck.position.y;

            //オリジナルキャラ
            float head_BodySize = charaDirector.originalBodyNodeNeckSize;
            float head_ModelSize = head_BodySize + num;
            float head_CharaSize = data.height / 100f;

            //キャラの高さを更新
            _charaHeight = head_ModelSize;

            //体モデルの根っこ
            Vector3 position3 = _bodyRoot.position;
            _bodyScale = position3.y / 1.02f;
            //_bodyScale = charaDirector.originalBodyRootSize / 1.02f;

            headSubScale = head_CharaSize / head_ModelSize;
            _bodySubScale = head_BodySize * headSubScale / body_BodySize;
            _bodyScaleSubScale = _bodyScale * _bodySubScale;
        }
        else
        {
            _charaHeight = _bodyNodeNeck.position.y + num;
            _bodyScale = _bodyRoot.position.y / 1.02f;
            _bodySubScale = data.height / _charaHeight / 100f;
            _bodyScaleSubScale = _bodyScale * _bodySubScale;
        }
        Transform transform2 = null;
        switch (_createInfo.positionMode)
        {
            case LiveTimelineData.CharacterPositionMode.Immobility:
                {
                    float y = _bodyRoot.localPosition.y * _bodySubScale;
                    _bodyRoot.localPosition = new Vector3(_bodyRoot.localPosition.x, y, _bodyRoot.localPosition.z);
                    transform2 = _bodyRoot;
                    break;
                }
            case LiveTimelineData.CharacterPositionMode.Relative:
                _bodyRoot.localPosition *= _bodySubScale;
                transform2 = _bodyRoot;
                break;
            case LiveTimelineData.CharacterPositionMode.None:
                transform2 = _bodyParts.GetTransform(BodyParts.eTransform.Position);
                break;
        }

        //体のサイズ
        transform2.localScale = new Vector3(_bodySubScale, _bodySubScale, _bodySubScale);

        //頭のサイズ
        if (_faceNodeNeck != null)
        {
            if (charaDirector.isAppendDress)
            {
                _faceNodeNeck.localScale = new Vector3(headSubScale, headSubScale, headSubScale);
            }
            else
            {
                _faceNodeNeck.localScale = transform2.localScale;
            }
        }
        if (_headParts != null)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = _headParts.skinnedMeshRenderer;
            float headWidth = data.boundsBoxSizeData.headWidth;
            Bounds localBounds = skinnedMeshRenderer.localBounds;
            localBounds.extents = new Vector3(localBounds.extents.x * headWidth, localBounds.extents.y, localBounds.extents.z * headWidth);
            skinnedMeshRenderer.localBounds = localBounds;
        }
        SetBodyBounds(_charaHeight - num);
    }

    private void SetHierarchy()
    {
        _headParts.SetParent(_cachedTransform);
        _bodyParts.SetParent(_cachedTransform);
        Func<Parts, AccessoryParts, bool> func = delegate (Parts parent, AccessoryParts parts)
        {
            bool result = false;
            Transform transform = parent.FindNode(parts.attachmentNode);
            if (transform != null)
            {
                parts.SetParent(transform);
                result = true;
            }
            return result;
        };
        foreach (AccessoryParts lstAccessoryPart in _lstAccessoryParts)
        {
            if (!func(_headParts, lstAccessoryPart) && func(_bodyParts, lstAccessoryPart))
            {
                break;
            }
        }
    }

    private IEnumerator InitializeFacialController()
    {
        _lipSyncController = GetComponent<LipSyncController>();
        if (_lipSyncController == null)
        {
            _lipSyncController = base.gameObject.AddComponent<LipSyncController>();
            yield return null;
        }
        _eyeTraceController = GetComponent<EyeTraceController>();
        if (_eyeTraceController == null)
        {
            _eyeTraceController = base.gameObject.AddComponent<EyeTraceController>();
            yield return null;
        }
        Action<int, Transform, Transform> obj = delegate (int idxEye, Transform tmLocator, Transform tmEye)
        {
            if (tmLocator != null)
            {
                _eyeTraceController._eyeObject[idxEye].body = tmLocator.gameObject;
            }
            if (tmEye != null)
            {
                _eyeTraceController._eyeObject[idxEye].eye = tmEye.gameObject;
            }
        };
        obj(0, _headParts.GetTransform(HeadParts.eTransform.Eye_locator_L), _headParts.GetTransform(HeadParts.eTransform.Eye_L));
        obj(1, _headParts.GetTransform(HeadParts.eTransform.Eye_locator_R), _headParts.GetTransform(HeadParts.eTransform.Eye_R));
        if (_eyeTraceController._eyeObject[0].eye != null)
        {
            Transform transform = _headParts.GetTransform(HeadParts.eTransform.Eye_range_L_01);
            Transform transform2 = _headParts.GetTransform(HeadParts.eTransform.Eye_range_L_02);
            if (transform != null && transform2 != null)
            {
                Vector3 localPosition = _eyeTraceController._eyeObject[0].eye.transform.localPosition;
                float num = Mathf.Abs(Vector3.Distance(localPosition, new Vector3(transform.localPosition.x, localPosition.y, localPosition.z)));
                float num2 = Mathf.Abs(Vector3.Distance(localPosition, new Vector3(transform2.localPosition.x, localPosition.y, localPosition.z)));
                float y = Mathf.Abs(Vector3.Distance(localPosition, new Vector3(localPosition.x, transform2.localPosition.y, localPosition.z)));
                float num3 = Mathf.Abs(Vector3.Distance(localPosition, new Vector3(localPosition.x, transform.localPosition.y, localPosition.z)));
                _eyeTraceController._eyeObject[0].UpdateRange(new Rect(num, y, 0f - num2, 0f - num3));
                _eyeTraceController._eyeObject[1].UpdateRange(new Rect(0f - num, y, num2, 0f - num3));
            }
        }
        yield return null;
        if (_headParts == null)
        {
            yield break;
        }
        string faceAnimatorPath;
        if (isLoadFromDirector)
        {
            faceAnimatorPath = charaDirector.GetFaceAnimatorPath();
        }
        else
        {
            faceAnimatorPath = CharaDirector.Asset.GetFaceAnimatorPath(data.charaId, _createInfo.activeDressId, _createInfo.headIndex, _createInfo.isSubHeadIndex);
        }
        RuntimeAnimatorController runtimeAnimatorController = _resourcesManager.LoadObject(faceAnimatorPath) as RuntimeAnimatorController;
        if (!(runtimeAnimatorController != null) || !(_lipSyncController != null))
        {
            yield break;
        }
        Animator animator = _headParts.animator;
        if (animator != null)
        {
            animator.runtimeAnimatorController = runtimeAnimatorController;
            _lipSyncController.Initialize(animator);
            _lipSyncController._cheekRenderer = _headParts.cheekRenderer;
            if (_lipSyncController._cheekRenderer != null)
            {
                _lipSyncController._cheekRenderer.enabled = false;
            }
        }
    }

    private IEnumerator InitializeClothContoller()
    {
        _clothController = new ClothController(base.gameObject);
        Action<Parts> action = delegate (Parts parts)
        {
            ClothController.Cloth cloth = parts.cloth;
            if (cloth != null)
            {
                List<string> list = null;
                list = ((parts.category != Parts.eCategory.Body) ? null : _createInfo.lstPartsCode);
                cloth.Build(list);
                _clothController.AddCloth(cloth);
            }
        };
        action(_headParts);
        action(_bodyParts);
        foreach (AccessoryParts lstAccessoryPart in _lstAccessoryParts)
        {
            action(lstAccessoryPart);
        }
        float bodyCollisionScale = 1f;
        if (StageUtil.IsModelCommonDressId(_createInfo.activeDressId))
        {
            bodyCollisionScale = _createInfo.charaData.heightWithoutHeel / 154f;
        }
        yield return _clothController.CreateCollision(_createInfo.cyspringPurpose, _bodyScale, bodyCollisionScale);
        if ((_createInfo.cyspringPurpose & CySpringCollisionComponent.ePurpose.Union) == 0)
        {
            yield return _clothController.BindSpring();
        }
    }

    private void InitializeGimmickController()
    {
        if (_gimmickController == null)
        {
            _gimmickController = new GimmickController();
        }
        _gimmickController.Initialize(this, _cabinetCache);
    }

    private void InitializeRequestExecutor()
    {
        RegisterRequestExecutor(_gimmickController);
    }

    public void CreateCharacterOptionResources(GameObject go)
    {
        for (int i = 0; i < _sweatLocators.Count; i++)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(go);
            gameObject.transform.SetParent(_sweatLocators[i].locator, worldPositionStays: false);
            gameObject.SetActive(value: false);
            _sweatLocators[i].sweatObj = gameObject;
            _sweatLocators[i].sweatTrans = gameObject.transform;
            Renderer component = gameObject.GetComponent<Renderer>();
            if (null != component)
            {
                if (null == _sweatMat)
                {
                    _sweatMat = UnityEngine.Object.Instantiate(component.sharedMaterial);
                }
                component.sharedMaterial = _sweatMat;
                component.sortingOrder = _index;
            }
        }
    }

    public static bool ExistTexture(string path, string fileName, string mask = "3d_{0}.unity3d")
    {
        string assetName = string.Format(mask, fileName);
        return ResourcesManager.instance.ExistsAssetBundleManifest(assetName);
    }

    public void Request<REQUEST>(Action<REQUEST, IRequestExecutor> fnInscription) where REQUEST : IRequest
    {
        Type typeFromHandle = typeof(REQUEST);
        HashSet<IRequestExecutor> value = null;
        if (!_mapRequestExecutor.TryGetValue(typeFromHandle, out value))
        {
            return;
        }
        foreach (IRequestExecutor item in value)
        {
            if (!_setRequest.Contains(item.request))
            {
                item.request.Initialize();
                fnInscription((REQUEST)item.request, item);
                _setRequest.Add(item.request);
            }
        }
    }

    public EXECUTOR RegisterRequestExecutor<EXECUTOR>(EXECUTOR executor, bool create = false) where EXECUTOR : IRequestExecutor, new()
    {
        if (executor == null && create)
        {
            executor = new EXECUTOR();
        }
        if (executor != null)
        {
            Type type = executor.request.type;
            HashSet<IRequestExecutor> value = null;
            if (!_mapRequestExecutor.TryGetValue(type, out value))
            {
                value = new HashSet<IRequestExecutor>();
                _mapRequestExecutor.Add(type, value);
            }
            if (!value.Contains(executor))
            {
                value.Add(executor);
            }
        }
        return executor;
    }

    public void RequestProcess()
    {
        foreach (IRequest item in _setRequest)
        {
            HashSet<IRequestExecutor> value = null;
            if (!_mapRequestExecutor.TryGetValue(item.type, out value) || value == null)
            {
                continue;
            }
            foreach (IRequestExecutor item2 in value)
            {
                item2.ExecuteRequest(item);
            }
        }
        _setRequest.Clear();
    }
}
