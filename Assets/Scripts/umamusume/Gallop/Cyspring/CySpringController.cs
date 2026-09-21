using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Threading;

namespace Gallop
{
    public class CySpringController : MonoBehaviour
    {
        public const float DEFAULT_WARMUPTIME = 1f;
        public const float DEFAULT_WIND_POWERSCALE_RATE = 1f;
        private const int PARTS_NUM = 3;
        private int _targetCySpringFpsMode = 1;

        /// Which step rate the solver is told to assume: 1 = 60fps, 0 = 30fps. This reaches the
        /// native plugin as its is60FPS argument and changes how the Verlet carry-over is scaled,
        /// so it has to match the rate the game is actually stepped at. Settable so a headless
        /// capture, which forces its own fixed step, can keep the two in agreement.
        public bool Is60FpsMode
        {
            get { return _targetCySpringFpsMode == 1; }
            set { _targetCySpringFpsMode = value ? 1 : 0; }
        }
        private static volatile bool _simulationTimeOutError;

        public enum Parts
        {
            [InspectorName("头部")]
            Head = 0,

            [InspectorName("身体")]
            Body = 1,

            [InspectorName("尾巴")]
            Tail = 2
        }

        public enum SpringUpdateMode
        {
            [InspectorName("正常更新")]
            ModeNormal = 0,

            [InspectorName("强制更新")]
            ModeForceUpdate = 1,

            [InspectorName("停止更新")]
            ModeStop = 2
        }

        [Header("官方数据容器输入")]
        [SerializeField, InspectorName("头部数据容器")]
        private CySpringDataContainer _headDataContainerAsset;

        [SerializeField, InspectorName("身体数据容器")]
        private CySpringDataContainer _bodyDataContainerAsset;

        [SerializeField, InspectorName("胸部数据容器")]
        private CySpringDataContainer _bustDataContainerAsset;

        [SerializeField, InspectorName("尾巴数据容器")]
        private CySpringDataContainer _tailDataContainerAsset;

        [Header("运行时设置")]
        [SerializeField, InspectorName("髋部骨骼")]
        private Transform _hipTransform;

        [SerializeField, InspectorName("在 LateUpdate 中自动更新")]
        private bool _autoUpdateInLateUpdate = false;

        [SerializeField, InspectorName("使用线程")]
        private bool _useThread; // kept for API compatibility; this bridge runs single-thread by default.

        private CySpring[] _springArray;
        private CySpringCollision[] _collisionArray;
        private CySpringCollisionRuntimeData[] _envColArray;

        // Official owner fields. Kept as object so this can compile even when CySpringOwner is not restored yet.
        private CySpringOwner _modelController;
        private GameObject _ownerObject;
        private Transform _ownerTransform;

        private Vector3 _ownerPrePosition;
        private Quaternion _ownerPreRotation;
        private Vector3 _hipPrePosition;
        private float _hipMoveDistance;

        private float[] _springRateArray;
        private float _springRate = 1.0f;
        private Vector3 _previousPos;
        private Quaternion _previousRot;
        private Vector3 _prevLocalScale;

        [Header("角色变换与更新设置")]
        [InspectorName("影响角色位置")]
        public bool IsAffectOwnerPotision;

        [InspectorName("影响角色旋转")]
        public bool IsAffectOwnerRotation;

        [InspectorName("影响角色缩放")]
        public bool IsAffectOwnerScale;

        [InspectorName("应用尾巴变换")]
        public bool IsApplyTailTransform = true;

        [InspectorName("跳过预更新")]
        public bool IsSkipUpdatePre;

        private Vector3 _ownerTransformPosition;
        private Quaternion _ownerTransformRotation;
        private Vector3 _ownerTransformLocalScale;

        private SkirtController _skirtController;
        private volatile bool _doneUpdate;
        private bool _isUseThread;
        private bool _initialized;
        private bool _resetFlag;
        private readonly bool[] _resetPartsFlag = new bool[PARTS_NUM];
        private readonly bool[] _nonResetPartsFlag = new bool[PARTS_NUM];
        private bool _isPlaying;

        private string[] _ignoredCySpringRootBoneArray;
        private float _timescale = 1.0f;
        private float _windPowerRate = 1.0f;
        private float[] _windPowerScaleArrayRate;
        private readonly bool[] _hasWindArray = new bool[PARTS_NUM];
        private readonly CySpringWindParam[] _windParamArray = new CySpringWindParam[PARTS_NUM];
        private readonly CySpringWindParam[] _addWindParamArray = new CySpringWindParam[PARTS_NUM];
        private readonly bool[] _shouldBlendWindParamArray = new bool[PARTS_NUM];
        private bool _isEnableDummyWind;
        private Vector3 _dummyWindDir;
        private bool _isCalcCorrectScale;

        private readonly List<CySpringBoneBase> _tmpSpringBoneList = new List<CySpringBoneBase>();
        private bool _isWarmingUpCySpringReserved;
        private float _warmingUpTimeReserved;
        private int _springResetDelayFrame;
        private Action _onCompleteWarmingUpCySpringReserve;

        private CySpring.RootParentWork _rootBoneParentWork;
        private bool _validRootBoneParentWork;

        private Dictionary<string, Transform> _transformCacheDic;

        /// <summary>
        /// Optional scale resolver matching the owner virtual scale function in the official controller.
        /// Argument true is used for Head/Tail collision scale; false is used for Body scale.
        /// If not assigned, 1.0f is used.
        /// </summary>
        public bool HasSpringRateArray => _springRateArray != null;

        public float SpringRate
        {
            get => _springRate;
            set
            {
                _springRate = value;
                if (_springRateArray != null)
                {
                    for (int i = 0; i < _springRateArray.Length; i++)
                        _springRateArray[i] = value;
                }
            }
        }

        public Vector3 PreviousPos
        {
            get => _previousPos;
            set => _previousPos = value;
        }

        public Quaternion PreviousRot
        {
            get => _previousRot;
            set => _previousRot = value;
        }

        public bool IsPlaying => _isPlaying;
        public SpringUpdateMode UpdateMode { get; set; }
        public bool IsUpdateScale { get; set; }

        public float TimeScale
        {
            get => _timescale;
            set => _timescale = value;
        }

        public float AdditionalWindTimeScale { get; set; } = 1.0f;

        public float BlendRate
        {
            set
            {
                if (_springArray == null)
                    return;

                for (int i = 0; i < _springArray.Length; i++)
                {
                    if (_springArray[i] != null)
                        _springArray[i].BlendRate = value;
                }
            }
        }

        public float StiffnessRate
        {
            set => SetStiffnessRate(value);
        }

        public float WindPowerRate
        {
            get => _windPowerRate;
            set => _windPowerRate = value;
        }

        public float[] WindPowerScaleArrayRate
        {
            get => _windPowerScaleArrayRate;
            set => _windPowerScaleArrayRate = value;
        }

        public CySpringOwner OwnerModelController => _modelController;
        public GameObject OwnerGameObject => _ownerObject;

        public static CySpringController AddController(GameObject character, Transform hip)
        {
            return AddController(character, hip, null);
        }

        public static CySpringController AddController(GameObject character, Transform hip, CySpringOwner ownerCltr)
        {
            if (character == null)
                return null;

            CySpringController controller = character.GetComponent<CySpringController>();
            if (controller == null)
                controller = character.AddComponent<CySpringController>();

            controller.SetOwner(character, hip, ownerCltr);
            return controller;
        }

        private void Awake()
        {
            if (_ownerObject == null)
                SetOwner(gameObject, _hipTransform, _modelController);

            Init();
        }

        private void LateUpdate()
        {
            if (!_autoUpdateInLateUpdate)
                return;

            BeginSimulation(Time.deltaTime, false);
            EndSimulation();
        }

        public void SetOwner(GameObject owner, Transform hip, CySpringOwner ownerCltr)
        {
            _modelController = ownerCltr;
            _ownerObject = owner != null ? owner : gameObject;
            _ownerTransform = _ownerObject != null ? _ownerObject.transform : transform;
            _hipTransform = hip;

            if (_ownerTransform != null)
            {
                _ownerPrePosition = _ownerTransform.position;
                _ownerPreRotation = _ownerTransform.rotation;
                _previousPos = _ownerPrePosition;
                _previousRot = _ownerPreRotation;
                _prevLocalScale = _ownerTransform.localScale;
            }

            _hipPrePosition = _hipTransform != null ? _hipTransform.position : Vector3.zero;
        }

        public void Init()
        {
            if (_springArray == null || _springArray.Length != PARTS_NUM)
                _springArray = new CySpring[PARTS_NUM];

            if (_collisionArray == null || _collisionArray.Length != PARTS_NUM)
                _collisionArray = new CySpringCollision[PARTS_NUM];

            if (_springRateArray == null || _springRateArray.Length != PARTS_NUM)
            {
                _springRateArray = new float[PARTS_NUM];
                for (int i = 0; i < PARTS_NUM; i++)
                    _springRateArray[i] = _springRate;
            }

            for (int i = 0; i < PARTS_NUM; i++)
            {
                if (_windParamArray[i] == null)
                    _windParamArray[i] = new CySpringWindParam();

                if (_addWindParamArray[i] == null)
                    _addWindParamArray[i] = new CySpringWindParam();
            }

            if (_ownerTransform == null)
                _ownerTransform = transform;

            _isPlaying = true;
            if (_timescale == 0.0f)
                _timescale = 1.0f;
            if (_windPowerRate == 0.0f)
                _windPowerRate = 1.0f;
        }

        /// <summary>
        /// Official-style direct bridge for projects that already have DataContainer components in the scene/prefab.
        /// This bypasses the official owner resource table, but keeps the official Head+Bust / Body / Tail loading order.
        /// </summary>
        public void LoadFromDataContainers(
            Dictionary<string, Transform> transformCacheDic,
            CySpringDataContainer head,
            CySpringDataContainer body,
            CySpringDataContainer bust,
            CySpringDataContainer tail,
            CySpringCollisionRuntimeData[] envHead = null,
            CySpringCollisionRuntimeData[] envBody = null,
            CySpringCollisionRuntimeData[] envTail = null,
            float headScale = -1.0f,
            float bodyScale = -1.0f,
            float tailScale = -1.0f)
        {
            BeginLoadDirect(head, body, bust, tail, transformCacheDic);

            if (headScale < 0.0f)
                headScale = GetOwnerScale(true);
            if (bodyScale < 0.0f)
                bodyScale = GetOwnerScale(false);
            if (tailScale < 0.0f)
                tailScale = GetOwnerScale(true);

            LoadHeadCollisionParamSequence(_transformCacheDic, headScale, bodyScale);
            LoadHeadSpringParamSequence(envHead, headScale, bodyScale);
            LoadBodyCollisionParamSequence(_transformCacheDic, bodyScale);
            LoadBodySpringParamSequence(envBody, bodyScale);
            LoadTailCollisionParamSequence(_transformCacheDic, tailScale);
            LoadTailSpringParamSequence(envTail, bodyScale);

            EndLoad();
        }

        public void BeginLoadDirect(
            CySpringDataContainer head,
            CySpringDataContainer body,
            CySpringDataContainer bust,
            CySpringDataContainer tail,
            Dictionary<string, Transform> transformCacheDic = null)
        {
            ReleaseLoadObject();
            Init();

            _transformCacheDic = transformCacheDic ?? BuildTransformCache(_ownerTransform != null ? _ownerTransform : transform);
            _headDataContainerAsset = head;
            _bodyDataContainerAsset = body;
            _bustDataContainerAsset = bust;
            _tailDataContainerAsset = tail;

            // Official flag copied from Bust.UseCorrectScaleCalc in LoadHeadCollisionParamSequence.
            _isCalcCorrectScale = _bustDataContainerAsset != null && _bustDataContainerAsset.UseCorrectScaleCalc;
        }

        public void EndLoad()
        {
            SetupSpringParentWork();
            ReleaseLoadObject();
            _initialized = true;
        }

        public void LoadHeadCollisionParamSequence(Dictionary<string, Transform> transformCacheDic)
        {
            LoadHeadCollisionParamSequence(transformCacheDic, GetOwnerScale(true), GetOwnerScale(false));
        }

        private void LoadHeadCollisionParamSequence(Dictionary<string, Transform> transformCacheDic, float headScale, float bodyScale)
        {
            if (_headDataContainerAsset == null)
                return;

            CySpringCollisionDataAsset main = ToCollisionAsset(_headDataContainerAsset);
            CySpringCollisionDataAsset add = null;
            float addScale = headScale;

            if (_bustDataContainerAsset != null)
            {
                add = ToCollisionAsset(_bustDataContainerAsset);
                _isCalcCorrectScale = _bustDataContainerAsset.UseCorrectScaleCalc;
                if (_isCalcCorrectScale)
                    addScale = bodyScale;
            }

            LoadCollisionParam(Parts.Head, main, add, headScale, addScale, transformCacheDic);
        }

        public void LoadHeadSpringParamSequence(CySpringCollisionRuntimeData[] envCollisionArray = null)
        {
            LoadHeadSpringParamSequence(envCollisionArray, GetOwnerScale(true), GetOwnerScale(false));
        }

        private void LoadHeadSpringParamSequence(CySpringCollisionRuntimeData[] envCollisionArray, float headScale, float bodyScale)
        {
            if (_headDataContainerAsset == null)
                return;

            LoadSpringParam(
                Parts.Head,
                ToSpringAsset(_headDataContainerAsset),
                ToSpringAsset(_bustDataContainerAsset),
                headScale,
                bodyScale,
                _ignoredCySpringRootBoneArray,
                envCollisionArray);
        }

        public void LoadBodyCollisionParamSequence(Dictionary<string, Transform> transformCacheDic)
        {
            LoadBodyCollisionParamSequence(transformCacheDic, GetOwnerScale(false));
        }

        private void LoadBodyCollisionParamSequence(Dictionary<string, Transform> transformCacheDic, float bodyScale)
        {
            if (_bodyDataContainerAsset == null)
                return;

            LoadCollisionParam(
                Parts.Body,
                ToCollisionAsset(_bodyDataContainerAsset),
                null,
                bodyScale,
                1.0f,
                transformCacheDic);
        }

        public void LoadBodySpringParamSequence(CySpringCollisionRuntimeData[] envCollisionArray = null)
        {
            LoadBodySpringParamSequence(envCollisionArray, GetOwnerScale(false));
        }

        private void LoadBodySpringParamSequence(CySpringCollisionRuntimeData[] envCollisionArray, float bodyScale)
        {
            if (_bodyDataContainerAsset == null)
                return;

            LoadSpringParam(
                Parts.Body,
                ToSpringAsset(_bodyDataContainerAsset),
                null,
                bodyScale,
                bodyScale,
                _ignoredCySpringRootBoneArray,
                envCollisionArray);
        }

        public void LoadTailCollisionParamSequence(Dictionary<string, Transform> transformCacheDic)
        {
            LoadTailCollisionParamSequence(transformCacheDic, GetOwnerScale(true));
        }

        private void LoadTailCollisionParamSequence(Dictionary<string, Transform> transformCacheDic, float tailScale)
        {
            if (_tailDataContainerAsset == null)
                return;

            LoadCollisionParam(
                Parts.Tail,
                ToCollisionAsset(_tailDataContainerAsset),
                null,
                tailScale,
                1.0f,
                transformCacheDic);
        }

        public void LoadTailSpringParamSequence(CySpringCollisionRuntimeData[] envCollisionArray = null)
        {
            LoadTailSpringParamSequence(envCollisionArray, GetOwnerScale(false));
        }

        private void LoadTailSpringParamSequence(CySpringCollisionRuntimeData[] envCollisionArray, float bodyScale)
        {
            if (_tailDataContainerAsset == null)
                return;

            LoadSpringParam(
                Parts.Tail,
                ToSpringAsset(_tailDataContainerAsset),
                null,
                bodyScale,
                bodyScale,
                _ignoredCySpringRootBoneArray,
                envCollisionArray);
        }

        private void LoadCollisionParam(
            Parts parts,
            CySpringCollisionDataAsset paramAsset,
            CySpringCollisionDataAsset paramAsset2,
            float legacyScale,
            float addLegacyScale,
            Dictionary<string, Transform> transformCacheDic)
        {
            int index = (int)parts;
            if (!IsValidPart(index) || paramAsset == null)
                return;

            if (_collisionArray[index] == null)
                _collisionArray[index] = new CySpringCollision(gameObject);

            _collisionArray[index].Create(
                paramAsset,
                paramAsset2,
                legacyScale,
                addLegacyScale,
                transformCacheDic,
                TryFindOtherTransform);
        }

        private void LoadSpringParam(
            Parts parts,
            CySpringParamDataAsset paramAsset,
            CySpringParamDataAsset paramAsset2,
            float legacyScale,
            float legacyScale2,
            string[] ignoredRoot,
            CySpringCollisionRuntimeData[] envCollisionArray)
        {
            int index = (int)parts;
            if (!IsValidPart(index) || paramAsset == null)
                return;

            if (_springArray[index] == null)
                _springArray[index] = new CySpring(gameObject);

            List<CySpringCollisionRuntimeData> runtimeCollisionList = null;
            if (_collisionArray[index] != null)
                runtimeCollisionList = _collisionArray[index].RuntimeDataList;

            // Official LoadSpringParam special cases:
            // Body removes collision names used by Tail spring data.
            // Tail uses Body collision runtime list filtered by Tail spring collision names.
            if (parts == Parts.Body && runtimeCollisionList != null && _tailDataContainerAsset != null)
            {
                HashSet<string> tailCollisionNames = GetCollisionNameSet(ToSpringAsset(_tailDataContainerAsset));
                runtimeCollisionList = CreateGetBodyRuntimeDataList(runtimeCollisionList, tailCollisionNames);
            }
            else if (parts == Parts.Tail)
            {
                List<CySpringCollisionRuntimeData> bodyRuntimeList =
                    _collisionArray[(int)Parts.Body] != null ? _collisionArray[(int)Parts.Body].RuntimeDataList : null;

                HashSet<string> tailCollisionNames = GetCollisionNameSet(paramAsset);
                runtimeCollisionList = CreateGetTailRuntimeDataList(bodyRuntimeList, tailCollisionNames);
            }

            if (envCollisionArray != null && envCollisionArray.Length > 0)
                _springArray[index].ApplyEnvCollision(envCollisionArray);

            _springArray[index].Bind(
                paramAsset,
                paramAsset2,
                legacyScale,
                legacyScale2,
                runtimeCollisionList,
                ignoredRoot);
        }

        public void BeginSimulation(float elapsedTime, bool isUseThread = true)
        {
    
            if (_springArray == null || !_initialized || !_isPlaying)
                return;

            if (_ownerTransform == null)
                _ownerTransform = transform;

    // BeginSimulation 官方这里 ownerTransform 为空基本直接不走后续
            if (_ownerTransform == null)
                return;

    // 保存当前 owner posture。
    // 这三个值会被 UpdatePrevousPosture 临时改 ownerTransform 前使用，
    // EndSimulation 再恢复。
            _ownerTransformPosition = _ownerTransform.position;
            _ownerTransformRotation = _ownerTransform.rotation;
            _ownerTransformLocalScale = _ownerTransform.localScale;

    // official:
    // if (IsSkipUpdatePre)
    // {
    //     UpdatePrevousPosture();
    //     OnReset();
    //     return;
    // }
            if (IsSkipUpdatePre)
            {
                UpdatePrevousPosture();
                OnReset();
                return;
            }

    // 官方风处理在 UpdatePrevousPosture 之前。
    // 你这个 helper 如果内部等价 SetupWind loop，就保留。
            SetupWindForActiveParts();

    // 官方这里会把 ownerTransform 临时归零/identity/scale-normalize。
            UpdatePrevousPosture();

    // reset 在 UpdatePrevousPosture 后。
            OnReset();

    // hipMoveDistance 在 UpdatePrevousPosture + OnReset 后算。
            if (_hipTransform != null)
                _hipMoveDistance = Vector3.Distance(_hipTransform.position, _hipPrePosition);
            else
                _hipMoveDistance = 0.0f;

    // 官方 GatherRootParentPosture：
    // 每帧把 root parent transform 的 position/rotation 写入 RootParentWork。
            if (_validRootBoneParentWork && _rootBoneParentWork != null)
            _rootBoneParentWork.GatherPosture();

            for (int i = 0; i < PARTS_NUM; i++)
            {
                CySpring spring = _springArray[i];
                if (spring == null)
                    continue;

                if (_modelController == null)
                    throw new NullReferenceException();

                // official:
                // spring + 184 = controller._previousPos
                spring.CurrentCharaPosition = _previousPos;

                // official:
                // if (_isCalcCorrectScale)
                //     owner.vtable + 472 = GetTotalScale()
                // else
                //     owner.vtable + 456 = GetBodyScale()
                float legacyScale = _isCalcCorrectScale ? _modelController.GetTotalScale() : _modelController.GetBodyScale();

                // official:
                // owner.vtable + 440 = GetCySpringCorrectScale(bool)
                // bool 参数是 i == 0，所以只有 Head 是 true
                float scale = _modelController.GetCySpringCorrectScale(i == (int)Parts.Head);

                spring.GatherSpring(elapsedTime, scale, legacyScale, AdditionalWindTimeScale, IsUpdateScale);
            }

            if (_skirtController != null)
                {
                _skirtController.UpdateSkirtArgForCySpring();
                }
            _isUseThread = isUseThread;
            _doneUpdate = false;

            
            if (_isUseThread)
            {
                CySpringThread.QueueUserWorkItem(this);
            }
            else
            {
                UpdateSpringThread();
            }
        }

    private sealed class CySpringThread
    {   
        private sealed class TaskInfo
        {
            public CySpringController cySpring;
        }

        private static CySpringThread _instance;

        public static CySpringThread Instance
        {
            get
            {
                if (_instance == null)
                {
                // 官方 ctor(queueSize, threadNum)
                // private const int DEFAULT_QUEUE_SIZE = 0x80;
                // private const int DEFAULT_THREAD_NUM = 1;
                //_instance = new CySpringThread(DEFAULT_QUEUE_SIZE, DEFAULT_THREAD_NUM);
                    _instance = new CySpringThread(128, 1);
                }

                return _instance;
            }
        }

        private readonly Thread[] _threadPoolArray;      // +0x10
        private readonly TaskInfo[] _taskQueueArray;    // +0x18
        private int _nPutPointer;                       // +0x20
        private int _nGetPointer;                       // +0x24
        private int _numTasks;                          // +0x28
        private readonly AutoResetEvent _putNotification; // +0x30
        private readonly AutoResetEvent _getNotification; // +0x38
        private readonly Semaphore _semaphore;          // +0x40
        private bool _quit;                             // +0x48

        private CySpringThread(int queueSize, int threadNum)
        {
            if (threadNum == 0)
                threadNum = SystemInfo.processorCount;

            _threadPoolArray = new Thread[threadNum];
            _taskQueueArray = new TaskInfo[queueSize];

            for (int i = 0; i < _taskQueueArray.Length; i++)
                _taskQueueArray[i] = new TaskInfo();

            _nPutPointer = 0;
            _nGetPointer = 0;
            _numTasks = 0;

            _putNotification = new AutoResetEvent(false);
            _getNotification = new AutoResetEvent(false);

            if (threadNum > 1)
            {
                _semaphore = new Semaphore(0, queueSize);

                for (int i = 0; i < threadNum; i++)
                {
                    Thread t = new Thread(_ThreadFunc);
                    _threadPoolArray[i] = t;
                    t.Start();
                }
            }
            else
            {
                Thread t = new Thread(_SingleThreadFunc);
                _threadPoolArray[0] = t;
                t.Start();
            }
        }

        public static void QueueUserWorkItem(CySpringController cySpring)
        {
            if (cySpring == null)
                return;

            Instance._EnqueueTask(cySpring);
        }

        public static void DequeueUserWorkItem(CySpringController cySpring)
        {
            CySpringThread inst = Instance;
            TaskInfo[] queue = inst._taskQueueArray;

            for (int i = 0; i < queue.Length; i++)
            {
                CySpringController queued = queue[i].cySpring;
                if (queued == cySpring)
                    queue[i].cySpring = null;
            }
        }

        public static void Terminate()
        {
            if (_instance != null)
                _instance._TerminateThreads();
        }

        private void _EnqueueTask(CySpringController cySpring)
        {
            while (_numTasks == _taskQueueArray.Length)
                _getNotification.WaitOne();

            _taskQueueArray[_nPutPointer].cySpring = cySpring;

            _nPutPointer++;
            if (_nPutPointer == _taskQueueArray.Length)
                _nPutPointer = 0;

            if (_threadPoolArray.Length != 1)
            {
                Interlocked.Increment(ref _numTasks);
                _semaphore.Release();
            }
            else
            {
                if (Interlocked.Increment(ref _numTasks) == 1)
                    _putNotification.Set();
            }
        }

        private void _SingleThreadFunc()
        {
            while (!_quit)
            {
                if (_numTasks == 0)
                {
                    while (true)
                    {
                        _putNotification.WaitOne();

                        if (_quit)
                            return;

                        if (_numTasks != 0)
                            break;
                    }
                }

                int index = _nGetPointer;
                int next = index + 1;
                _nGetPointer = next;

                TaskInfo[] queue = _taskQueueArray;
                CySpringController cySpring = queue[index].cySpring;

                if (next == queue.Length)
                 _nGetPointer = 0;

                int newTaskCount = Interlocked.Decrement(ref _numTasks);

                // 官方条件：decrement 后 == queue.Length - 1
            // 也就是队列刚刚从 full 变成 not full。
                if (newTaskCount == queue.Length - 1)
                _getNotification.Set();

                if (cySpring != null)
                cySpring.UpdateSpringThread();
            }
        }

        private void _ThreadFunc()
        {
            while (!_quit)
            {
                _semaphore.WaitOne();

                if (_quit)
                    return;

                int index;
                int next;

                do
                {
                    index = _nGetPointer;
                    next = index + 1;
                    if (next == _taskQueueArray.Length)
                        next = 0;
                }
                while (Interlocked.CompareExchange(ref _nGetPointer, next, index) != index);

                CySpringController cySpring = _taskQueueArray[index].cySpring;

                int newTaskCount = Interlocked.Decrement(ref _numTasks);

                if (newTaskCount == _taskQueueArray.Length - 1)
                    _getNotification.Set();

                if (cySpring != null)
                    cySpring.UpdateSpringThread();
            }
        }

        private void _TerminateThreads()
        {
            if (_quit)
                return;

            _quit = true;

            if (_threadPoolArray == null || _threadPoolArray.Length == 0)
                return;

            if (_threadPoolArray.Length == 1)
            {
                _putNotification.Set();
            }
            else
            {
                _semaphore.Release(_threadPoolArray.Length);
            }

            for (int i = 0; i < _threadPoolArray.Length; i++)
            {
                Thread thread = _threadPoolArray[i];
                if (thread == null)
                    throw new NullReferenceException();

                thread.Join();
            }
        }
    }

        private void UpdateSpringThread()
        {
            if (_springArray == null)
            {
                _doneUpdate = true;
                return;
            }

            bool is60FpsPhysics = _targetCySpringFpsMode == 1;

            for (int i = 0; i < PARTS_NUM; i++)
            {
                CySpring spring = _springArray[i];
                if (spring == null)
                    continue;

                float partSpringRate = GetSpringRate((Parts)i);
                bool hasWind = _hasWindArray[i];
                float windRate = GetWindPowerRate((Parts)i);

                if (hasWind) // 官方这里先不要加 || _isEnableDummyWind
                {
                    spring.UpdateSpringHasWind(_hipMoveDistance, windRate,_timescale, is60FpsPhysics,partSpringRate);
                }
                else
                {
                    spring.UpdateSpring(_hipMoveDistance, _timescale, partSpringRate, is60FpsPhysics);
                }
            }

            _doneUpdate = true;
        }

        public void EndSimulation()
        {
            if (_springArray == null || !_isPlaying || !_initialized)
                return;

            if (_isUseThread)
            {
                float startTime = Time.realtimeSinceStartup;

                while (!_doneUpdate)
                {
                    if (_simulationTimeOutError)
                    {
                        _isPlaying = false;
                        _doneUpdate = true;
                        break;
                    }

                    Thread.Sleep(0);

                    float elapsed = Time.realtimeSinceStartup - startTime;
                    if (elapsed > 10.0f)
                    {
                        _simulationTimeOutError = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(
                    "[CySpringController] SimulationTimeOutError elapsed=" + elapsed,
                    this
                );
#endif

                        _isPlaying = false;
                        _doneUpdate = true;
                        LogTimeOutError(elapsed);
                        break;
                    }
                }
            }

    // 官方这里等价内联 PostSpringAll：
    // Head/Body 只要 spring != null 就 PostSpring。
    // Tail 受 IsApplyTailTransform 控制。
            if (_springArray != null)
            {
                for (int i = 0; i < _springArray.Length; i++)
                {
                    CySpring spring = _springArray[i];
                    if (spring == null)
                        continue;

                    if (i == (int)Parts.Tail && !IsApplyTailTransform)
                        continue;

                    spring.PostSpring();
                }
            }

            if (_hipTransform != null)
                _hipPrePosition = _hipTransform.position;

            if (_ownerTransform == null)
                _ownerTransform = transform;

            if (_ownerTransform == null)
                throw new NullReferenceException();

            if (!IsAffectOwnerPotision)
                _ownerTransform.position = _ownerTransformPosition;

            if (!IsAffectOwnerRotation)
                _ownerTransform.rotation = _ownerTransformRotation;

            if (!IsAffectOwnerScale)
                _ownerTransform.localScale = _ownerTransformLocalScale;

    // 官方 EndSimulation 这里只更新 ownerPrePosition / ownerPreRotation。
    // 不要在这里写 _previousPos / _previousRot / _prevLocalScale。
            _ownerPrePosition = _ownerTransform.position;
            _ownerPreRotation = _ownerTransform.rotation;
        }

        private void LogTimeOutError(float elapsedTime)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(
                "SimulationTimeOutError: Owner=" +
                (_ownerObject != null ? _ownerObject.name : "null") +
                ", ElapsedTime: " + elapsedTime,
                this
            );
#endif
        }

        public void PostSpringAll()
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < PARTS_NUM; i++)
            {
                CySpring spring = _springArray[i];
                if (spring == null)
                    continue;

                if (i == (int)Parts.Tail && !IsApplyTailTransform)
                    continue;

                spring.PostSpring();
            }
        }

        public void Reset()
        {
            _resetFlag = true;
            ResetNativeCloth();
        }

        public void ResetParts(Parts parts)
        {
            int index = (int)parts;
            if (IsValidPart(index))
                _resetPartsFlag[index] = true;

            ResetPartsNativeCloth(parts);
        }

        private void OnReset()
        {
            if (_resetFlag)
            {
                ResetNativeCloth();
                _resetFlag = false;
            }

            for (int i = 0; i < PARTS_NUM; i++)
            {
                if (!_resetPartsFlag[i])
                    continue;

                if (!_nonResetPartsFlag[i])
                    ResetPartsNativeCloth((Parts)i);

                _resetPartsFlag[i] = false;
            }
        }

        public void Pause()
        {
            _isPlaying = false;
        }

        public void Resume()
        {
            _isPlaying = true;
            UpdatePrevousPosture();
        }

        public void SetForceDisableHipMoveParam(bool isDisable)
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < _springArray.Length; i++)
            {
                if (_springArray[i] != null)
                    _springArray[i].IsForceDisableHipMoveParam = isDisable;
            }
        }

        public void CreateLinkSkirtController(SkirtController skirtController)
        {
            _skirtController = skirtController;

            if (_springArray == null)
                throw new NullReferenceException();

            if (_springArray.Length <= (int)Parts.Body)
                throw new IndexOutOfRangeException();

            CySpring bodySpring = _springArray[(int)Parts.Body];
            if (bodySpring == null)
                throw new NullReferenceException();

            bodySpring.SetSkirtController(skirtController);

            if (bodySpring.RootBoneArray == null)
                throw new NullReferenceException();

            if (_skirtController == null)
                throw new NullReferenceException();

            _skirtController.CreateLinkCySpring(
                bodySpring.RootBoneArray,
                () => _isPlaying
            );
        }

        public void ReserveWarmingUpCySpring(float warmUpTime = DEFAULT_WARMUPTIME, int delayFrame = 0, Action onComplete = null)
        {
            _isWarmingUpCySpringReserved = true;
            _warmingUpTimeReserved = warmUpTime;
            _springResetDelayFrame = delayFrame;
            _onCompleteWarmingUpCySpringReserve = onComplete;
        }

        public void TryUpdateWarmUp()
        {
            if (!_isWarmingUpCySpringReserved)
                return;

            if (_springResetDelayFrame > 0)
            {
                _springResetDelayFrame--;
                return;
            }

            WarmUpCySpring(_warmingUpTimeReserved);
            _isWarmingUpCySpringReserved = false;
            Action complete = _onCompleteWarmingUpCySpringReserve;
            _onCompleteWarmingUpCySpringReserve = null;
            complete?.Invoke();
        }

        private void WarmUpCySpring(float warmUpTime = DEFAULT_WARMUPTIME)
        {
            int frameCount = Mathf.Max(1, Mathf.CeilToInt(warmUpTime * 60.0f));
            float dt = 1.0f / 60.0f;

            for (int i = 0; i < frameCount; i++)
            {
                BeginSimulation(dt, false);
                EndSimulation();
            }
        }

        private float GetTargetCySpringFPS()
        {
            return 60.0f;
        }

        public void SetSuppression(bool enable, float length)
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < _springArray.Length; i++)
                _springArray[i]?.SetSuppression(enable, length);
        }

        public void UpdatePrevousPosture()
        {
            if (_ownerTransform == null)
                _ownerTransform = transform;

            if (_ownerTransform == null)
                throw new NullReferenceException();

            _previousPos = Gallop.Math.VECTOR3_ZERO;
            _previousRot = Gallop.Math.QUATERNION_IDENTITY;

            if (!IsAffectOwnerPotision)
            {
                _previousPos = _ownerTransformPosition;
                _ownerTransform.position = Gallop.Math.VECTOR3_ZERO;
            }

            if (!IsAffectOwnerRotation)
            {
                _previousRot = _ownerTransformRotation;
                _ownerTransform.rotation = Gallop.Math.QUATERNION_IDENTITY;
            }

            if (IsAffectOwnerScale)
                return;

            _prevLocalScale = _ownerTransformLocalScale;

            float scale = _ownerTransform.lossyScale.x;
            if (Gallop.Math.IsFloatEqualLight(scale, 0.0f))
                scale = 0.000001f;

                _ownerTransform.localScale = new Vector3(
                _ownerTransformLocalScale.x / scale,
                _ownerTransformLocalScale.y / scale,
                _ownerTransformLocalScale.z / scale
            );
        }

        public void ResetNativeCloth()
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < _springArray.Length; i++)
                _springArray[i]?.ResetNativeCloth();
        }

        public void ResetPartsNativeCloth(Parts parts)
        {
            int index = (int)parts;
            if (IsValidPart(index))
                _springArray[index]?.ResetNativeCloth();
        }

        public void SetScale(Parts parts, float scale, bool isUpdateNodeAndCollision = false)
        {
            int index = (int)parts;
            if (!IsValidPart(index))
                return;

            _springArray[index]?.ResetScale(scale, scale, isUpdateNodeAndCollision);
            _springArray[index]?.UpdateScaleConnectBone(scale);
            _collisionArray[index]?.SetScale(scale, scale);
        }

        public void SetScale(Parts parts)
        {
            int index = (int)parts;

            if (_collisionArray == null)
                return;

            if (_springArray == null)
                return;

            if (index < 0 || index >= _collisionArray.Length)
                return;

            if (index >= _springArray.Length)
                return;

            CySpringCollision collision = _collisionArray[index];
            if (collision == null)
                return;

            CySpring spring = _springArray[index];
            if (spring == null)
                return;

            if (_modelController == null)
                throw new NullReferenceException();

            bool isHead = parts == Parts.Head;

            float scale = _modelController.GetCySpringCorrectScale(isHead);
            float addScale = _modelController.GetCySpringCorrectScale(false);

            if (isHead && _isCalcCorrectScale)
                addScale = _modelController.GetCySpringCorrectScale(false);

            collision.SetScale(scale, addScale);
            spring.ResetScale(scale, addScale, IsUpdateScale);
            spring.UpdateScaleConnectBone(scale);
            spring.UpdateNativeCollision(scale);
        }

        public bool GetIsCalcCorrectScale()
        {
            return _isCalcCorrectScale;
        }

        public void SetPartsSpringRate(Parts parts, float value)
        {
            if (_springRateArray == null || _springRateArray.Length != PARTS_NUM)
                _springRateArray = new float[PARTS_NUM] { _springRate, _springRate, _springRate };

            int index = (int)parts;
            if (IsValidPart(index))
                _springRateArray[index] = value;
        }

        public void SetStiffnessRate(float value)
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < _springArray.Length; i++)
                _springArray[i]?.SetStiffnessForceRate(value);
        }

        public void SetStiffnessRate(Parts parts, float value)
        {
            int index = (int)parts;
            if (IsValidPart(index))
                _springArray[index]?.SetStiffnessForceRate(value);
        }

        public bool GetHasWind(Parts parts)
        {
            int index = (int)parts;
            return IsValidPart(index) && _hasWindArray[index];
        }

        public void SetHasWind(bool hasWind, Parts parts)
        {
            int index = (int)parts;
            if (IsValidPart(index))
                _hasWindArray[index] = hasWind;
        }

        public void SetHasWindAll(bool hasWind)
        {
            for (int i = 0; i < PARTS_NUM; i++)
                _hasWindArray[i] = hasWind;
        }

        public void BeginWind(CySpringWindParam windParam)
        {
            for (int i = 0; i < PARTS_NUM; i++)
                BeginWind(windParam, (Parts)i);
        }

        public void BeginWind(CySpringWindParam windParam, Parts parts)
        {
            int index = (int)parts;
            if (!IsValidPart(index))
                return;

            _windParamArray[index] = windParam ?? new CySpringWindParam();
            _hasWindArray[index] = true;
        }

        public void EndWind()
        {
            for (int i = 0; i < PARTS_NUM; i++)
                EndWind((Parts)i);
        }

        public void EndWind(Parts parts)
        {
            int index = (int)parts;
            if (IsValidPart(index))
                _hasWindArray[index] = false;
        }

        public void SetEnableCySpringDummyWind(bool isEnable, Vector3 windDir)
        {
            _isEnableDummyWind = isEnable;
            _dummyWindDir = windDir;

            if (!isEnable)
                return;

            for (int i = 0; i < PARTS_NUM; i++)
            {
                _hasWindArray[i] = true;
                if (_windParamArray[i] == null)
                    _windParamArray[i] = new CySpringWindParam();

                _windParamArray[i].Direction = windDir.sqrMagnitude > 0.000001f ? windDir.normalized : Vector3.forward;
                _windParamArray[i].Right = Vector3.Cross(Vector3.up, _windParamArray[i].Direction).normalized;
                if (_windParamArray[i].Right.sqrMagnitude <= 0.000001f)
                    _windParamArray[i].Right = Vector3.right;
            }
        }

        private void SetupWindForActiveParts()
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < PARTS_NUM; i++)
            {
                if (!_hasWindArray[i] && !_isEnableDummyWind)
                    continue;

                CySpring spring = _springArray[i];
                if (spring == null)
                    continue;

                CySpringWindParam main = _windParamArray[i] ?? new CySpringWindParam();
                CySpringWindParam add = _addWindParamArray[i] ?? main;

                float mainCenter = GetCenterWindAngle((Parts)i, main);
                float addCenter = GetCenterWindAngle((Parts)i, add);

                spring.SetupWind((Parts)i, main, mainCenter, add, addCenter);
            }
        }

        private float GetCenterWindAngle(Parts parts, CySpringWindParam windParam)
        {
            // The exact official CalculateWindParam is still a later refinement.
            // This keeps the data path intact while staying deterministic.
            CySpringParamDataAsset data = GetSpringDataAsset(parts);
            if (data == null)
                return 0.0f;

            return Mathf.Lerp(data.CenterWindAngleSlow, data.CenterWindAngleFast, Mathf.Clamp01(_windPowerRate));
        }

        private CySpringParamDataAsset GetSpringDataAsset(Parts parts)
        {
            switch (parts)
            {
                case Parts.Head:
                    return ToSpringAsset(_headDataContainerAsset);
                case Parts.Body:
                    return ToSpringAsset(_bodyDataContainerAsset);
                case Parts.Tail:
                    return ToSpringAsset(_tailDataContainerAsset);
                default:
                    return null;
            }
        }

        public void ApplyEnvCySpringCollisionToHead(CySpringCollisionRuntimeData[] envColArray)
        {
            ApplyEnvCollision(Parts.Head, envColArray);
        }

        public void ApplyEnvCySpringCollisionToBody(CySpringCollisionRuntimeData[] envColArray)
        {
            ApplyEnvCollision(Parts.Body, envColArray);
        }

        public void ApplyEnvCySpringCollisionToTail(CySpringCollisionRuntimeData[] envColArray)
        {
            ApplyEnvCollision(Parts.Tail, envColArray);
        }

        private void ApplyEnvCollision(Parts parts, CySpringCollisionRuntimeData[] envColArray)
        {
            int index = (int)parts;
            if (!IsValidPart(index))
                return;

            _springArray[index]?.ApplyEnvCollision(envColArray);
        }

        public void SetEnableEnvCollision(bool enable)
        {
            if (_springArray == null)
                throw new NullReferenceException();

            if (_springArray.Length == 0)
                return;

            CySpring headSpring = _springArray[(int)Parts.Head];
            if (headSpring == null)
                throw new NullReferenceException();

            CySpringCollisionRuntimeData[] envArray = enable
                ? _envColArray
                : Array.Empty<CySpringCollisionRuntimeData>();

            headSpring.ApplyEnvCollision(envArray);

            SetupSpringParentWork();
        }

        public void SetEnableCySpringBone(bool enable, string rootBoneName, bool isPrefix)
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < _springArray.Length; i++)
                _springArray[i]?.SetEnableCySpringBone(enable, rootBoneName, isPrefix);
        }

        public void SetResetLinkCySpringBone(bool enable, string targetRootBoneName)
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < _springArray.Length; i++)
                _springArray[i]?.SetResetLinkCySpringBone(enable, targetRootBoneName);
        }

        public void SetEnableCySpringBone(bool enable, Parts parts)
        {
            int index = (int)parts;
            if (!IsValidPart(index) || _springArray[index] == null)
                return;

            if (enable)
                _springArray[index].EnableAllCySpringBones();
            else
                _springArray[index].DisableAllCySpringBones();
        }

        public void SetEnableCySpringBones(bool enable, string[] rootBonePrefixArray, bool isPrefix = false)
        {
            if (rootBonePrefixArray == null)
                return;

            for (int i = 0; i < rootBonePrefixArray.Length; i++)
                SetEnableCySpringBone(enable, rootBonePrefixArray[i], isPrefix);
        }

        public void SetEnableAllCySpringBones(bool enable)
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < _springArray.Length; i++)
            {
                if (_springArray[i] == null)
                    continue;

                if (enable)
                    _springArray[i].EnableAllCySpringBones();
                else
                    _springArray[i].DisableAllCySpringBones();
            }
        }

        public void SetEnableCySpringCollision(string name, bool isEnable)
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < _springArray.Length; i++)
                _springArray[i]?.SetEnableCySpringCollision(name, isEnable);
        }

        public void SetEnableCySpringBoneCharaCollision(string name, bool isEnable)
        {
            SetEnableCySpringCollision(name, isEnable);
        }

        public void SetEnableCySpringBoneEnvCollision(Parts parts, string boneName, bool isEnable)
        {
            // Fine-grained env collision switch is inside CySpringBoneBase in the official code.
            // Left as no-op for this bridge layer.
        }

        public CySpringRootBone[] FindRootSpring(string boneName, Parts part, bool isPrefix = false)
        {
            int index = (int)part;
            if (!IsValidPart(index) || _springArray[index] == null)
                return Array.Empty<CySpringRootBone>();

            return _springArray[index].FindRootCySpringBones(boneName, isPrefix);
        }

        public CySpringRootBone[] FindRootSpring(string[] boneNameArray, Parts part, bool isPrefix = false)
        {
            int index = (int)part;
            if (!IsValidPart(index) || _springArray[index] == null)
                return Array.Empty<CySpringRootBone>();

            return _springArray[index].FindRootCySpringBones(boneNameArray, isPrefix);
        }

        public CySpringRootBone[] GetRootSpringArray(Parts part)
        {
            int index = (int)part;
            if (!IsValidPart(index) || _springArray[index] == null)
                return Array.Empty<CySpringRootBone>();

            return _springArray[index].RootBoneArray ?? Array.Empty<CySpringRootBone>();
        }

        public void FindSpring(List<CySpringBoneBase> resultList, Func<string, bool> checkFunc, Parts part, bool firstHitBreak = true)
        {
            int index = (int)part;
            if (!IsValidPart(index) || _springArray[index] == null)
                return;

            _springArray[index].FindCySpringBone(resultList, checkFunc, firstHitBreak);
        }

        public IEnumerable<CySpringBoneBase> FindAllCySpringBones()
        {
            _tmpSpringBoneList.Clear();

            if (_springArray != null)
            {
                for (int i = 0; i < _springArray.Length; i++)
                {
                    if (_springArray[i] != null)
                        _springArray[i].FindCySpringBoneForAll(_tmpSpringBoneList, _ => true, false);
                }
            }

            return _tmpSpringBoneList;
        }

        public void SetEnableCySpringBoneWind(bool enable, string rootBoneName, bool isPrefix)
        {
            if (_springArray == null)
                return;

            for (int i = 0; i < _springArray.Length; i++)
                _springArray[i]?.SetEnableCySpringBoneWind(enable, rootBoneName, isPrefix);
        }

        public bool GetIsAnyRootTailBoneReflectCalc()
        {
            int index = (int)Parts.Tail;
            if (!IsValidPart(index) || _springArray == null || _springArray[index] == null)
                return false;

            CySpringRootBone[] roots = _springArray[index].RootBoneArray;
            if (roots == null)
                return false;

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].ShouldReflectCalcResult)
                    return true;
            }

            return false;
        }

        public void SetNonResetParts(Parts parts, bool isNonReset)
        {
            int index = (int)parts;
            if (IsValidPart(index))
                _nonResetPartsFlag[index] = isNonReset;
        }

        public void SetEnableCySpringBoneWind(bool enable, Parts parts)
        {
            int index = (int)parts;
            if (!IsValidPart(index) || _springArray[index] == null)
                return;

            _springArray[index].SetEnableCySpringBoneWind(enable, string.Empty, true);
        }

        public void Delete()
        {
            if (_springArray != null)
            {
                for (int i = 0; i < _springArray.Length; i++)
                {
                    _springArray[i]?.Delete();
                    _springArray[i] = null;
                }
            }

            if (_collisionArray != null)
            {
                for (int i = 0; i < _collisionArray.Length; i++)
                {
                    _collisionArray[i]?.Delete();
                    _collisionArray[i] = null;
                }
            }

            _initialized = false;
        }

        public static void Destroy(ref CySpringController cySpringController)
        {
            if (cySpringController == null)
                return;

            cySpringController.Delete();

            if (cySpringController.gameObject != null)
                UnityEngine.Object.Destroy(cySpringController);

            cySpringController = null;
        }

        private void OnDestroy()
        {
            Delete();
        }

        private void ReleaseLoadObject()
        {
            // Official destroys temporary instantiated DataContainer GameObjects here.
            // In the direct bridge path the containers are user-owned scene/prefab components, so do not destroy them.
        }

        private void SetupSpringParentWork()
        {
            _validRootBoneParentWork = false;

            if (_springArray == null)
                return;

            CySpring.RootParentInfo info = new CySpring.RootParentInfo();

            for (int i = 0; i < _springArray.Length; i++)
            {
                CySpring spring = _springArray[i];
                if (spring != null)
                    spring.CollectRootParentInfo(info);
            }

            _rootBoneParentWork = CySpring.RootParentInfo.MakeRootParentWork(info);

            for (int i = 0; i < _springArray.Length; i++)
            {
                CySpring spring = _springArray[i];
                if (spring != null)
                    spring.ParentWork = _rootBoneParentWork;
            }

            _validRootBoneParentWork = true;
        }

        private float GetOwnerScale(bool isHead)
        {
            if (_modelController == null)
                throw new NullReferenceException();

            return _modelController.GetCySpringCorrectScale(isHead);
        }

        private float GetSpringRate(Parts parts)
        {
            int index = (int)parts;
            if (_springRateArray != null && IsValidPart(index))
                return _springRateArray[index];

            return _springRate;
        }

        private float GetWindPowerRate(Parts parts)
        {
            int index = (int)parts;
            float scale = 1.0f;
            if (_windPowerScaleArrayRate != null && index >= 0 && index < _windPowerScaleArrayRate.Length)
                scale = _windPowerScaleArrayRate[index];

            return Mathf.Clamp01(_windPowerRate * scale);
        }

        private bool TryFindOtherTransform(string key, out Transform transformResult)
        {
            transformResult = null;

            if (string.IsNullOrEmpty(key))
                return false;

            if (_transformCacheDic != null && _transformCacheDic.TryGetValue(key, out transformResult) && transformResult != null)
                return true;

            Transform root = _ownerTransform != null ? _ownerTransform : transform;
            if (root == null)
                return false;

            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform t = all[i];
                if (t != null && string.Equals(t.name, key, StringComparison.Ordinal))
                {
                    transformResult = t;
                    return true;
                }
            }

            return false;
        }

        private static Dictionary<string, Transform> BuildTransformCache(Transform root)
        {
            Dictionary<string, Transform> dic = new Dictionary<string, Transform>();
            if (root == null)
                return dic;

            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform t = all[i];
                if (t == null || string.IsNullOrEmpty(t.name))
                    continue;

                if (!dic.ContainsKey(t.name))
                    dic.Add(t.name, t);
            }

            return dic;
        }

        private static CySpringCollisionDataAsset ToCollisionAsset(CySpringDataContainer container)
        {
            if (container == null)
                return null;

            CySpringCollisionDataAsset asset = new CySpringCollisionDataAsset();
            asset._dataList = container.collisionParam ?? new List<CySpringCollisionData>();

            return asset;
        }

        private static CySpringParamDataAsset ToSpringAsset(CySpringDataContainer container)
        {
            if (container == null)
                return null;

            CySpringParamDataAsset asset =
                ScriptableObject.CreateInstance<CySpringParamDataAsset>();

            asset.name = container.name + "_RuntimeSpringAsset";
            asset._elements = container.springParam ?? new List<CySpringParamDataElement>();

            asset.SetWindParam(container);
            asset.SetMoveSpringParam(container);
            asset.SetConnectedBoneParam(container);

            return asset;
        }

        private static HashSet<string> GetCollisionNameSet(CySpringParamDataAsset asset)
        {
            HashSet<string> set = new HashSet<string>();
            if (asset == null || asset._elements == null)
                return set;

            for (int i = 0; i < asset._elements.Count; i++)
            {
                CySpringParamDataElement element = asset._elements[i];
                if (element == null)
                    continue;

                AddNames(set, element._collisionNameList);

                if (element._childElements == null)
                    continue;

                for (int j = 0; j < element._childElements.Count; j++)
                {
                    CySpringParamDataChildElement child = element._childElements[j];
                    if (child != null)
                        AddNames(set, child._collisionNameList);
                }
            }

            return set;
        }

        private static void AddNames(HashSet<string> set, List<string> names)
        {
            if (set == null || names == null)
                return;

            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                if (!string.IsNullOrEmpty(name))
                    set.Add(name);
            }
        }

        private static List<CySpringCollisionRuntimeData> CreateGetBodyRuntimeDataList(
            List<CySpringCollisionRuntimeData> srcList,
            HashSet<string> removeNameSet)
        {
            List<CySpringCollisionRuntimeData> result = new List<CySpringCollisionRuntimeData>();
            if (srcList == null)
                return result;

            for (int i = 0; i < srcList.Count; i++)
            {
                CySpringCollisionRuntimeData data = srcList[i];
                if (data == null)
                    continue;

                if (removeNameSet != null && removeNameSet.Contains(data.Name))
                    continue;

                result.Add(data);
            }

            return result;
        }

        private static List<CySpringCollisionRuntimeData> CreateGetTailRuntimeDataList(
            List<CySpringCollisionRuntimeData> srcList,
            HashSet<string> tailNameSet)
        {
            List<CySpringCollisionRuntimeData> result = new List<CySpringCollisionRuntimeData>();
            if (srcList == null)
                return result;

            for (int i = 0; i < srcList.Count; i++)
            {
                CySpringCollisionRuntimeData data = srcList[i];
                if (data == null)
                    continue;

                if (tailNameSet != null && tailNameSet.Contains(data.Name))
                    result.Add(data);
            }

            return result;
        }

        private static bool IsValidPart(int index)
        {
            return index >= 0 && index < PARTS_NUM;
        }

        
 
    }
}
