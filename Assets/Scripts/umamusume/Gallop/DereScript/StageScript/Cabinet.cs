// Cabinet
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Cabinet : ScriptableObject
{
    [Serializable]
    public abstract class Entity
    {
        public const int INVALID_KEY = -1;

        [SerializeField]
        private int _key = -1;

        [NonSerialized]
        private bool _builded;

        public int key => _key;

        public bool builded
        {
            get
            {
                return _builded;
            }
            protected set
            {
                _builded = value;
            }
        }

        public Entity(int entityKey)
        {
            _key = entityKey;
        }

        public Entity Clone(int newKey = -1)
        {
            Entity entity = OnClone(newKey);
            if (entity != null)
            {
                if (entity.key == -1)
                {
                    entity._key = _key;
                }
                entity.builded = _builded;
            }
            return entity;
        }

        public virtual void Build()
        {
            _builded = true;
        }

        [Conditional("UNITY_EDITOR")]
        public virtual void DrawGUI()
        {
        }

        [Conditional("UNITY_EDITOR")]
        public virtual void Save()
        {
        }

        protected abstract Entity OnClone(int newKey);
    }

    [Serializable]
    public class Container<ENTITY> : Entity where ENTITY : Entity
    {
        [SerializeField]
        private List<ENTITY> _lstEntity;

        [NonSerialized]
        private Dictionary<int, ENTITY> _mapEntity;

        public List<ENTITY> lstEntity => _lstEntity;

        public Dictionary<int, ENTITY> mapEntity => _mapEntity;

        public int entityCount => _lstEntity.Count;

        public Container(int containerKey)
            : base(containerKey)
        {
            Reserve();
            Sort((ENTITY a, ENTITY b) => a.key - b.key);
        }

        public void Reserve()
        {
            if (_lstEntity == null)
            {
                _lstEntity = new List<ENTITY>();
            }
            if (_mapEntity == null)
            {
                _mapEntity = new Dictionary<int, ENTITY>();
            }
        }

        public void Add(int entityKey)
        {
            if (base.builded)
            {
                ENTITY val = Activator.CreateInstance(typeof(ENTITY), entityKey) as ENTITY;
                if (!_mapEntity.ContainsKey(val.key))
                {
                    val.Build();
                    _lstEntity.Add(val);
                    _mapEntity.Add(val.key, val);
                }
            }
        }

        public void Add(ENTITY entity)
        {
            if (base.builded && !_mapEntity.ContainsKey(entity.key))
            {
                entity.Build();
                _lstEntity.Add(entity);
                _mapEntity.Add(entity.key, entity);
            }
        }

        public ENTITY Get(int key)
        {
            ENTITY value = null;
            if (base.builded)
            {
                _mapEntity.TryGetValue(key, out value);
            }
            return value;
        }

        public void Remove(int key)
        {
            ENTITY value = null;
            if (base.builded)
            {
                _mapEntity.TryGetValue(key, out value);
                if (value != null)
                {
                    _lstEntity.Remove(value);
                    _mapEntity.Remove(key);
                }
            }
        }

        public void Remove(ENTITY entity)
        {
            Remove(entity.key);
        }

        public void Clear()
        {
            if (base.builded)
            {
                _lstEntity.Clear();
                _mapEntity.Clear();
            }
        }

        public void Sort(Comparison<ENTITY> fnComparison)
        {
            _lstEntity.Sort(fnComparison);
        }

        public virtual bool OnBuild()
        {
            return true;
        }

        public sealed override void Build()
        {
            Reserve();
            _mapCabinet.Clear();
            foreach (ENTITY item in _lstEntity)
            {
                item.Build();
                if (!_mapEntity.ContainsKey(item.key))
                {
                    _mapEntity.Add(item.key, item);
                }
            }
            base.builded = OnBuild();
        }

        protected override Entity OnClone(int newKey)
        {
            Container<ENTITY> container = new Container<ENTITY>(newKey);
            foreach (ENTITY item in _lstEntity)
            {
                ENTITY val = item.Clone() as ENTITY;
                if (val != null)
                {
                    container.lstEntity.Add(val);
                }
            }
            container.Build();
            return container;
        }

        public override void Save()
        {
            foreach (ENTITY item in _lstEntity)
            {
                _ = item;
            }
        }

        public override void DrawGUI()
        {
            foreach (ENTITY item in _lstEntity)
            {
                _ = item;
            }
        }
    }

    [Serializable]
    public class Condition
    {
        public enum eOperator
        {
            None,
            And,
            Or
        }

        public enum eCategory
        {
            None = 0,
            CharaId = 1,
            DressId = 2,
            CharaAttrType = 4,
            SongId = 0x8000,
            SongAttrType = 0x10000
        }

        public enum eComparer
        {
            Equal,
            Greater,
            Less,
            LEqual,
            GEqual,
            NotEqual
        }

        [Serializable]
        public class Record
        {
            public eOperator op;

            public eCategory cat;

            public string param = string.Empty;
        }

        public const int INVALID_VALUE = -1;

        public Condition clone => new Condition();
    }

    public static string ASSET_ROOT_PATH = string.Format("{0}3D", "Assets/_StageWork/Resources/");

    public static string ASSET_FOLDER_NAME = "Cabinet";

    public static string ASSET_PATH = $"{ASSET_ROOT_PATH}/{ASSET_FOLDER_NAME}";

    public static string ASSET_BUNDLE_NAME = "3d_cabinet.unity3d";

    private static Dictionary<Type, Cabinet> _mapCabinet = new Dictionary<Type, Cabinet>();

    [NonSerialized]
    private Dictionary<int, Entity> _mapEntity = new Dictionary<int, Entity>();

    public Dictionary<int, Entity> mapEntity => _mapEntity;

    public static void Load<CABINET>(string assetName, bool isUseAsssetBundle) where CABINET : Cabinet
    {
        CABINET val = null;
        ResourcesManager instance = SingletonMonoBehaviour<ResourcesManager>.instance;
        if (instance != null)
        {
            string objectName = $"3D/{ASSET_FOLDER_NAME}/{assetName}";
            val = instance.LoadObject<CABINET>(objectName);
        }
        if (val != null)
        {
            val.Build();
            _mapCabinet.Add(typeof(CABINET), val);
        }
    }

    public static IEnumerator Load(bool isUseAsssetBundle = true)
    {
        Release();
        Load<DressCabinet>("dress_cabinet", isUseAsssetBundle);
        yield break;
    }

    public static void Release()
    {
        _mapCabinet.Clear();
    }

    public static T GetCabinet<T>() where T : Cabinet
    {
        Cabinet value = null;
        Type typeFromHandle = typeof(T);
        _mapCabinet.TryGetValue(typeFromHandle, out value);
        return value as T;
    }

    public void CreateEntity<ENTITY>(int key, ref ENTITY entity) where ENTITY : Entity
    {
        if (entity == null)
        {
            entity = Activator.CreateInstance(typeof(ENTITY), key) as ENTITY;
        }
        if (!_mapEntity.ContainsKey(key) && entity != null)
        {
            _mapEntity.Add(key, entity);
        }
    }

    public Entity GetEntity(int key)
    {
        Entity value = null;
        _mapEntity.TryGetValue(key, out value);
        return value;
    }

    public virtual void Build()
    {
        foreach (KeyValuePair<int, Entity> item in _mapEntity)
        {
            item.Value.Build();
        }
    }

    [Conditional("UNITY_EDITOR")]
    public virtual void DrawGUI()
    {
        foreach (KeyValuePair<int, Entity> item in _mapEntity)
        {
            _ = item.Value;
        }
    }

    [Conditional("UNITY_EDITOR")]
    public virtual void Save(Action fnSave)
    {
        foreach (KeyValuePair<int, Entity> item in _mapEntity)
        {
            _ = item.Value;
        }
        fnSave();
    }
}
