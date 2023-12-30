using System.Collections.Generic;
using UnityEngine;

public static class A2U
{
    public enum Blend
    {
        Normal,
        Add,
        Multiply,
        Screen,
        Overlay
    }

    public enum Order
    {
        PreImageEffect,
        InImageEffect,
        PostImageEffect
    }

    public class Appearance
    {
        private bool[] _data;

        public void Init(int count)
        {
            _data = new bool[count];
        }

        public void Generate(int seed, uint maxCount, uint count)
        {
            UnityEngine.Random.State state = UnityEngine.Random.state;
            DoGenerate(seed, maxCount, count);
            UnityEngine.Random.state = state;
        }

        private void DoGenerate(int seed, uint maxCount, uint count)
        {
            UnityEngine.Random.InitState(seed);
            for (int i = 0; i < maxCount; i++)
            {
                _data[i] = i < count;
            }
            for (int j = 0; j < maxCount; j++)
            {
                bool flag = _data[j];
                int num = (int)UnityEngine.Random.Range(0f, maxCount);
                _data[j] = _data[num];
                _data[num] = flag;
            }
        }

        public bool IsAppeared(int index)
        {
            if (index < _data.Length)
            {
                return _data[index];
            }
            return false;
        }
    }

    public class Flicker
    {
        private float[] _data;

        private float _step;

        public int count => _data.Length;

        public float duration => _data.Length * _step;

        public float step => _step;

        public void Generate(int seed, uint count, float step, uint min, uint max)
        {
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed);
            DoGenerate(count, step, min, max);
            UnityEngine.Random.state = state;
        }

        private void DoGenerate(uint count, float step, uint min, uint max)
        {
            if (count < 0)
            {
                count = 1u;
            }
            float[] array = new float[count];
            int minInclusive = (int)System.Math.Min(min, 100u);
            int num = (int)System.Math.Min(max, 100u);
            for (int i = 0; i < count; i++)
            {
                int num2 = UnityEngine.Random.Range(minInclusive, num + 1);
                array[i] = num2 * 0.01f;
            }
            _step = step;
            _data = array;
        }

        public float GetValue(float sec)
        {
            float num = sec / _step;
            int num2 = (int)num % count;
            int num3 = (num2 + 1) % count;
            float t = num - (int)num;
            return Mathf.Lerp(_data[num2], _data[num3], t);
        }

        public float NormalizeSec(float sec)
        {
            float num = sec / _step;
            int num2 = (int)num % count;
            float num3 = num - (int)num;
            return (num2 + num3) * _step;
        }
    }

    public class Loader
    {
        private struct Cache
        {
            public UnityEngine.Object obj;

            public int hash;
        }

        private static readonly string ROOT_PREFAB = "Prefab/A2URoot";

        private Cache[] _cache;

        private int _cacheCount;

        private int _cacheTop;

        public void BeginCache(int count)
        {
            _cacheCount = 0;
            _cacheTop = 0;
            _cache = new Cache[count];
        }

        public void EndCache()
        {
            int i = 0;
            for (int num = _cache.Length; i < num; i++)
            {
                _cache[i].obj = null;
            }
            _cacheCount = 0;
            _cacheTop = 0;
            _cache = null;
        }

        private void PushCache(int hash, UnityEngine.Object obj)
        {
            _cache[_cacheTop].hash = hash;
            _cache[_cacheTop].obj = obj;
            int num = _cache.Length;
            _cacheTop = (_cacheTop + 1) % num;
            _cacheCount = System.Math.Min(_cacheCount + 1, num);
        }

        private int FindCache(int hash)
        {
            int i = 0;
            for (int cacheCount = _cacheCount; i < cacheCount; i++)
            {
                if (hash == _cache[i].hash)
                {
                    return i;
                }
            }
            return _cacheCount;
        }

        public GameObject LoadGameObject(string path, bool isCacheEnabled, bool isLocalAssetBundle = false)
        {
            if (!isCacheEnabled)
            {
                return Resources.Load<GameObject>(path);
                //return ResourcesManager.instance.LoadObject<GameObject>(path);
            }
            int hashCode = path.GetHashCode();
            int num = FindCache(hashCode);
            if (num < _cacheCount)
            {
                return (GameObject)_cache[num].obj;
            }
            GameObject gameObject = ResourcesManager.instance.LoadObject<GameObject>(path);
            PushCache(hashCode, gameObject);
            return gameObject;
        }

        public GameObject InstanciateGameObject(string path, bool isCacheEnabled, bool isLocalAssetBundle = false)
        {
            return UnityEngine.Object.Instantiate(LoadGameObject(path, isCacheEnabled, isLocalAssetBundle));
        }

        public List<Sprite> LoadSprites(string[] path, string[] multiSpritePath)
        {
            List<Sprite> list = new List<Sprite>();
            int i = 0;
            for (int num = path.Length; i < num; i++)
            {
                Texture2D texture2D = SingletonMonoBehaviour<ResourcesManager>.instance.LoadObject<Texture2D>(path[i]);
                A2UMultiSprite a2UMultiSprite = SingletonMonoBehaviour<ResourcesManager>.instance.LoadObject<A2UMultiSprite>(multiSpritePath[i]);
                if (a2UMultiSprite != null && texture2D != null)
                {
                    int num2 = a2UMultiSprite._spriteInfos.Length;
                    int j = 0;
                    for (int num3 = num2; j < num3; j++)
                    {
                        A2UMultiSprite.SpriteInfo spriteInfo = a2UMultiSprite._spriteInfos[j];
                        Sprite sprite = Sprite.Create(texture2D, spriteInfo.rect, spriteInfo.pivot, spriteInfo.pixelsToUnits, spriteInfo.extrude, spriteInfo.meshType, spriteInfo.border);
                        sprite.name = spriteInfo.name;
                        list.Add(sprite);
                    }
                }
            }
            return list;
        }

        public GameObject InstanciateManager()
        {
            return InstanciateGameObject(ROOT_PREFAB, isCacheEnabled: false, isLocalAssetBundle: true);
        }
    }

    public class Random
    {
        private UnityEngine.Random.State _origin;

        public void Begin(int seed)
        {
            _origin = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed);
        }

        public int Get(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public float Get(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public void End()
        {
            UnityEngine.Random.state = _origin;
        }
    }

    public class Renderer
    {
        private enum BlendPass
        {
            Normal,
            Add,
            Multiply,
            Screen,
            Overlay
        }

        private RenderTexture _texture;

        private Material _material;

        private bool _isEnabled;

        private BlendPass _blendPass;

        private Order _order = Order.PostImageEffect;

        private int _texturePropertyId;

        public bool isEnabled
        {
            get
            {
                if (_isEnabled && null != _texture)
                {
                    return null != _material;
                }
                return false;
            }
            set
            {
                _isEnabled = value;
            }
        }

        public Order order
        {
            get
            {
                return _order;
            }
            set
            {
                _order = value;
            }
        }

        public void Init(RenderTexture texture, Material material)
        {
            _texturePropertyId = Shader.PropertyToID("_ColorBuffer");
            _texture = texture;
            _material = material;
        }

        public virtual void Final()
        {
            _texture = null;
            _material = null;
        }

        public void DoRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (!isEnabled)
            {
                Graphics.Blit(src, dst);
                return;
            }
            _material.SetTexture(_texturePropertyId, _texture);
            Graphics.Blit(src, dst, _material, (int)_blendPass);
        }

        public void SetBlendMode(Blend blendMode)
        {
            switch (blendMode)
            {
                case Blend.Normal:
                    _blendPass = BlendPass.Normal;
                    break;
                case Blend.Add:
                    _blendPass = BlendPass.Add;
                    break;
                case Blend.Multiply:
                    _blendPass = BlendPass.Multiply;
                    break;
                case Blend.Screen:
                    _blendPass = BlendPass.Screen;
                    break;
                case Blend.Overlay:
                    _blendPass = BlendPass.Overlay;
                    break;
            }
        }
    }

    public static readonly string PAIR_BODY = "Pair";

    public static readonly string PAIR_SUFFIX = "_L";

    private static A2UManager _manager = null;

    private static A2UCamera _camera = null;

    private static Loader _loader = null;

    public static A2UManager manager => _manager;

    public static A2UCamera camera => _camera;

    public static Loader loader => _loader;

    public static bool isEnabled
    {
        get
        {
            if (null != _camera)
            {
                return _camera.a2uRenderer.isEnabled;
            }
            return false;
        }
        set
        {
            if (null != _camera)
            {
                _camera.a2uRenderer.isEnabled = value;
            }
        }
    }

    /// <summary>
    /// Managerを指定した型で返す
    /// </summary>
    public static T GetManager<T>() where T : A2UManager
    {
        return (T)_manager;
    }

    public static A2UManager Init(GameObject parent, int rtWidth, int rtHeight)
    {
        _loader = new Loader();
        GameObject gameObject = _loader.InstanciateManager();
        gameObject.transform.SetParent(parent.transform);
        _manager = gameObject.GetComponent<A2UManager>();
        _manager.InitCamera(rtWidth, rtHeight);
        _camera = _manager.a2uCamera;
        return _manager;
    }

    public static void Final()
    {
        if (null != _manager)
        {
            _manager.Final();
            _manager = null;
        }
        _camera = null;
        _loader = null;
    }

    public static bool IsRenderingOrder(Order order)
    {
        if (null != _camera)
        {
            return order == _camera.a2uRenderer.order;
        }
        return false;
    }

    public static void DoRenderImage(RenderTexture src, RenderTexture dst)
    {
        _camera.a2uRenderer.DoRenderImage(src, dst);
    }
}
