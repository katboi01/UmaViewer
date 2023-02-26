using System;
using System.Collections.Generic;
using Cutt;
using UnityEngine;

public class A2UController : MonoBehaviour
{
    public struct NodeDesc
    {
        public Color _color;

        public float _startSec;

        public float _speed;

        public byte _opacity;

        public bool _useFlicker;

        public void Init()
        {
            _color = Color.white;
            _startSec = 0f;
            _speed = 1f;
            _opacity = 100;
            _useFlicker = true;
        }
    }

    public struct NodeWork
    {
        public float _flickSec;

        public float _prevAlpha;

        public float _originalScale;

        public void Init()
        {
            _flickSec = 0f;
            _prevAlpha = 0f;
            _originalScale = 1f;
        }
    }

    public class SpritePair
    {
        public SpriteRenderer _first;

        public SpriteRenderer _second;

        public SpritePair()
        {
            _first = null;
            _second = null;
        }
    }

    public struct HashNode
    {
        public GameObject _composition;

        public Transform _transform;

        public Animator _animator;

        public List<SpritePair> _spritePairs;

        public List<SpriteRenderer> _sprites;

        public int _nameHash;

        public float _frameRate;

        public uint _maxFrame;

        public NodeDesc _desc;

        public bool _isEnabled;

        public NodeWork[] _work;

        public void Init(GameObject composition)
        {
            _composition = composition;
            _transform = composition.transform;
            _animator = composition.GetComponent<Animator>();
            _nameHash = FNVHash.Generate(composition.name);
            _spritePairs = new List<SpritePair>();
            _sprites = new List<SpriteRenderer>();
            _frameRate = 30f;
            _maxFrame = 0u;
            _desc = default(NodeDesc);
            _desc.Init();
            _isEnabled = false;
            _work = null;
        }
    }

    public struct PrefabDesc
    {
        public string name;

        public string path;
    }

    public struct GameObjectDesc
    {
        public string name;

        public string prefabPath;
    }

    public struct InitContext
    {
        public int flickRandomSeed;

        public uint flickCount;

        public float flickStepSec;

        public uint flickMin;

        public uint flickMax;

        public string[] texturePathList;

        public string[] multiSpritePathList;

        public PrefabDesc[] prefabs;

        public GameObjectDesc[] gameObjecs;
    }

    public struct UpdateContext
    {
        public Color spriteColor;

        public Vector2 position;

        public Vector2 scale;

        public float rotationZ;

        public uint textureIndex;

        public int appearanceRandomSeed;

        public float spriteAppearance;

        public int slopeRandomSeed;

        public float spriteMinSlope;

        public float spriteMaxSlope;

        public float spriteScale;

        public float spriteOpacity;

        public float startSec;

        public float speed;

        public bool isFlick;

        public bool enable;
    }
    protected HashNode[] _a2uNodes = new HashNode[0];

    protected List<Sprite> _spriteList;

    protected A2U.Appearance _appearance = new A2U.Appearance();

    protected A2U.Flicker _flicker = new A2U.Flicker();

    protected A2U.Random _random = new A2U.Random();

    public HashNode[] a2uNodes => _a2uNodes;

    private void Update()
    {
        if (_a2uNodes == null)
        {
            return;
        }
        int i = 0;
        for (int num = _a2uNodes.Length; i < num; i++)
        {
            if (_a2uNodes[i]._isEnabled)
            {
                float duration = _flicker.duration;
                int num2 = 0;
                List<SpritePair> spritePairs = _a2uNodes[i]._spritePairs;
                int num3 = 0;
                int count = spritePairs.Count;
                while (num3 < count)
                {
                    DoUpdateFrick(duration, i, num2);
                    DoPreUpdateSpriteColor(spritePairs[num3]._first, i, num2);
                    DoPreUpdateSpriteColor(spritePairs[num3]._second, i, num2);
                    num3++;
                    num2++;
                }
                List<SpriteRenderer> sprites = _a2uNodes[i]._sprites;
                int num4 = 0;
                int count2 = sprites.Count;
                while (num4 < count2)
                {
                    DoUpdateFrick(duration, i, num2);
                    DoPreUpdateSpriteColor(sprites[num4], i, num2);
                    num4++;
                    num2++;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (_a2uNodes == null)
        {
            return;
        }
        uint num = 0u;
        for (uint num2 = (uint)_a2uNodes.Length; num < num2; num++)
        {
            if (_a2uNodes[num]._isEnabled)
            {
                DoLateUpdateSprites(num);
            }
        }
    }

    public void Init(ref InitContext context)
    {
        PrefabDesc[] prefabs = context.prefabs;
        int num = prefabs.Length;
        A2U.Loader loader = A2U.loader;
        loader.BeginCache(num);
        for (int i = 0; i < num; i++)
        {
            PrefabDesc prefabDesc = prefabs[i];
            loader.LoadGameObject(prefabDesc.path, isCacheEnabled: true);
        }
        GameObjectDesc[] gameObjecs = context.gameObjecs;
        int num2 = gameObjecs.Length;
        for (int j = 0; j < num2; j++)
        {
            GameObjectDesc gameObjectDesc = gameObjecs[j];
            GameObject obj = loader.InstanciateGameObject(gameObjectDesc.prefabPath, isCacheEnabled: true);
            obj.name = gameObjectDesc.name;
            GameObject gameObject = new GameObject
            {
                name = gameObjectDesc.name + "_root"
            };
            obj.transform.SetParent(gameObject.transform);
            gameObject.transform.SetParent(base.transform);
            GameObjectUtility.SetLayer(29, gameObject.transform);
        }
        loader.EndCache();
        _spriteList = A2U.loader.LoadSprites(context.texturePathList, context.multiSpritePathList);
        int childCount = base.transform.childCount;
        if (childCount != 0)
        {
            _a2uNodes = new HashNode[childCount];
            int maxCount = 0;
            for (int k = 0; k < childCount; k++)
            {
                GameObject obj2 = base.transform.GetChild(k).gameObject.transform.GetChild(0).gameObject;
                HashNode hashNode = MakeHashNode(obj2, ref maxCount);
                _a2uNodes[k] = hashNode;
                StopAnimation(obj2);
            }
            _appearance.Init(maxCount);
            DoSetupFricker(context.flickRandomSeed, Math.Max(context.flickCount, 0u), Mathf.Max(context.flickStepSec, 0f), Math.Max(context.flickMin, 0u), Math.Max(context.flickMax, 0u));
        }
    }

    public virtual void Final()
    {
    }

    public void UpdateComposition(int nameHash, ref UpdateContext context)
    {
        int num = FindNodeIndex(nameHash);
        if (_a2uNodes.Length <= num)
        {
            return;
        }
        HashNode node = _a2uNodes[num];
        GameObject composition = node._composition;
        DoUpdateCompositionTransform(composition.transform.parent.gameObject, context.position, context.rotationZ, context.scale);
        uint maxFrame = _a2uNodes[num]._maxFrame;
        float num2 = Mathf.Max(context.startSec, 0f) % (float)maxFrame;
        float speed = Mathf.Max(0f, context.speed);
        _a2uNodes[num]._desc._startSec = num2;
        _a2uNodes[num]._desc._speed = speed;
        if (_a2uNodes[num]._isEnabled != context.enable)
        {
            if (context.enable)
            {
                StartAnimation(ref node, num2);
                DoSetupEnabled(num, isEnabled: true);
            }
            else
            {
                StopAnimation(ref node);
                DoSetupEnabled(num, isEnabled: false);
            }
        }
        _a2uNodes[num]._isEnabled = context.enable;
        if (context.enable)
        {
            SetAnimationSpeed(ref node, speed);
            NodeDesc desc = default(NodeDesc);
            desc.Init();
            desc._color = context.spriteColor;
            desc._opacity = (byte)Mathf.FloorToInt(Mathf.Max(0f, context.spriteOpacity) * 100f + 0.5f);
            desc._useFlicker = context.isFlick;
            Sprite texture = null;
            if (_spriteList != null && context.textureIndex < _spriteList.Count)
            {
                texture = _spriteList[(int)context.textureIndex];
            }
            DoUpdateCompositionParameter(num, texture, ref desc, Mathf.Max(0f, context.spriteScale), context.appearanceRandomSeed, context.spriteAppearance, context.slopeRandomSeed, context.spriteMinSlope, context.spriteMaxSlope);
        }
    }

    public int FindNodeIndex(int compositionNameHash)
    {
        int num = _a2uNodes.Length;
        for (int i = 0; i < num; i++)
        {
            if (compositionNameHash == _a2uNodes[i]._nameHash)
            {
                return i;
            }
        }
        return num;
    }

    private void DoSetupFricker(int randomSeed, uint flickCount, float stepSec, uint min, uint max)
    {
        _flicker.Generate(randomSeed, flickCount, stepSec, min, max);
    }

    public bool DoUpdateCompositionTransform(int namehash, Vector2 pos, float rotZ, Vector2 scale)
    {
        int num = FindNodeIndex(namehash);
        GameObject composition = _a2uNodes[num]._composition;
        if (null == composition)
        {
            return false;
        }
        DoUpdateCompositionTransform(composition, pos, rotZ, scale);
        return true;
    }

    private void DoUpdateCompositionTransform(GameObject composition, Vector2 pos, float rotZ, Vector2 scale)
    {
        composition.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        composition.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, rotZ));
        composition.transform.localScale = new Vector3(scale.x, scale.y, 0f);
    }

    public void DoUpdateCompositionParameter(int nodeIndex, Sprite texture, ref NodeDesc desc, float scale, int appearanceRandomSeed, float appearanceCount, int slopeSeed, float minSlope, float maxSlope)
    {
        List<SpritePair> spritePairs = _a2uNodes[nodeIndex]._spritePairs;
        List<SpriteRenderer> sprites = _a2uNodes[nodeIndex]._sprites;
        int num = spritePairs.Count + sprites.Count;
        uint count = (uint)Mathf.FloorToInt((float)num * Mathf.Clamp(appearanceCount, 0f, 1f) + 0.5f);
        _appearance.Generate(appearanceRandomSeed, (uint)num, count);
        _a2uNodes[nodeIndex]._desc._opacity = desc._opacity;
        _a2uNodes[nodeIndex]._desc._color = desc._color;
        _a2uNodes[nodeIndex]._desc._useFlicker = desc._useFlicker;
        DoUpdateSprites(nodeIndex, texture, scale, slopeSeed, minSlope, maxSlope, _a2uNodes[nodeIndex]._isEnabled);
    }

    private void DoUpdateSprites(int nodeIndex, Sprite texture, float scale, int slopeSeed, float minSlope, float maxSlope, bool isEnabled)
    {
        _random.Begin(slopeSeed);
        float min = Mathf.Clamp(minSlope, -180f, 180f);
        float max = Mathf.Clamp(maxSlope, -180f, 180f);
        int num = 0;
        List<SpritePair> spritePairs = _a2uNodes[nodeIndex]._spritePairs;
        int num2 = 0;
        int count = spritePairs.Count;
        while (num2 < count)
        {
            SpritePair spritePair = spritePairs[num2];
            float rotZ = _random.Get(min, max);
            bool isAppearance = _appearance.IsAppeared(num);
            float originalScale = _a2uNodes[nodeIndex]._work[num]._originalScale;
            DoUpdateSprite(spritePair._first, texture, originalScale * scale, rotZ, isAppearance, isEnabled);
            DoUpdateSprite(spritePair._second, texture, originalScale * scale, rotZ, isAppearance, isEnabled);
            num2++;
            num++;
        }
        List<SpriteRenderer> sprites = _a2uNodes[nodeIndex]._sprites;
        int num3 = 0;
        int count2 = sprites.Count;
        while (num3 < count2)
        {
            SpriteRenderer sprite = sprites[num3];
            float rotZ2 = _random.Get(min, max);
            bool isAppearance2 = _appearance.IsAppeared(num);
            float originalScale2 = _a2uNodes[nodeIndex]._work[num]._originalScale;
            DoUpdateSprite(sprite, texture, originalScale2 * scale, rotZ2, isAppearance2, isEnabled);
            num3++;
            num++;
        }
        _random.End();
    }

    private void DoUpdateSprite(SpriteRenderer sprite, Sprite texture, float scale, float rotZ, bool isAppearance, bool isEnabled)
    {
        if (!(sprite == null))
        {
            if (null != texture)
            {
                sprite.sprite = texture;
            }
            sprite.transform.localScale = new Vector3(scale, scale, 1f);
            sprite.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
            sprite.enabled = isAppearance && isEnabled;
        }
    }

    private void DoUpdateFrick(float duration, int nodeIndex, int workIndex)
    {
        if (_a2uNodes[nodeIndex]._desc._useFlicker)
        {
            float num = _a2uNodes[nodeIndex]._work[workIndex]._flickSec + Time.deltaTime;
            if (num > duration)
            {
                num -= duration;
            }
            _a2uNodes[nodeIndex]._work[workIndex]._flickSec = num;
        }
    }

    private void DoPreUpdateSpriteColor(SpriteRenderer sprite, int nodeIndex, int workIndex)
    {
        if (!(sprite == null))
        {
            Color color = sprite.color;
            _a2uNodes[nodeIndex]._work[workIndex]._prevAlpha = color.a;
            color.a = -1f;
            sprite.color = color;
        }
    }

    private void DoLateUpdateSprites(uint nodeIndex)
    {
        Color color = _a2uNodes[nodeIndex]._desc._color;
        int num = 0;
        List<SpritePair> spritePairs = _a2uNodes[nodeIndex]._spritePairs;
        int num2 = 0;
        int count = spritePairs.Count;
        while (num2 < count)
        {
            SpritePair spritePair = spritePairs[num2];
            DoLateUpdateSprite(spritePair._first, nodeIndex, num, color);
            DoLateUpdateSprite(spritePair._second, nodeIndex, num, color);
            num2++;
            num++;
        }
        List<SpriteRenderer> sprites = _a2uNodes[nodeIndex]._sprites;
        int num3 = 0;
        int count2 = sprites.Count;
        while (num3 < count2)
        {
            DoLateUpdateSprite(sprites[num3], nodeIndex, num, color);
            num3++;
            num++;
        }
    }

    private void DoLateUpdateSprite(SpriteRenderer sprite, uint nodeIndex, int workIndex, Color color)
    {
        if (!(sprite == null))
        {
            float num = 1f;
            if (_a2uNodes[nodeIndex]._desc._useFlicker)
            {
                num = _flicker.GetValue(_a2uNodes[nodeIndex]._work[workIndex]._flickSec);
            }
            Color color2 = color;
            if (sprite.color.a < 0f)
            {
                color2.a = _a2uNodes[nodeIndex]._work[workIndex]._prevAlpha;
            }
            else
            {
                color2.a = sprite.color.a * ((float)(int)_a2uNodes[nodeIndex]._desc._opacity * 0.01f);
            }
            color2.a *= num;
            sprite.color = color2;
        }
    }

    private void DoSetupEnabled(int index, bool isEnabled)
    {
        float startSec = _a2uNodes[index]._desc._startSec;
        float step = _flicker.step;
        int count = _flicker.count;
        int num = 0;
        List<SpritePair> spritePairs = _a2uNodes[index]._spritePairs;
        int num2 = 0;
        int count2 = spritePairs.Count;
        while (num2 < count2)
        {
            SpritePair spritePair = spritePairs[num2];
            float flickSec = GetFlickSec(startSec, step, num, count);
            DoSetupEnabledImpl(spritePair._first, index, num, isEnabled, flickSec);
            DoSetupEnabledImpl(spritePair._second, index, num, isEnabled, flickSec);
            num2++;
            num++;
        }
        List<SpriteRenderer> sprites = _a2uNodes[index]._sprites;
        int num3 = 0;
        int count3 = sprites.Count;
        while (num3 < count3)
        {
            float flickSec2 = GetFlickSec(startSec, step, num, count);
            DoSetupEnabledImpl(sprites[num3], index, num, isEnabled, flickSec2);
            num3++;
            num++;
        }
    }

    private void DoSetupEnabledImpl(SpriteRenderer sprite, int nodeIndex, int workIndex, bool isEnabled, float flickSec)
    {
        if (!(sprite == null))
        {
            sprite.enabled = isEnabled;
            _a2uNodes[nodeIndex]._work[workIndex]._flickSec = flickSec;
        }
    }

    private float GetFlickSec(float startSec, float step, int workIndex, int flickCount)
    {
        float num = startSec + step * (float)workIndex;
        float num2 = num / step;
        int num3 = (int)num2 % (flickCount - 1);
        float num4 = num2 - (float)(int)num2;
        return _flicker.NormalizeSec(num + ((float)num3 + num4) * step);
    }

    private static Vector2 DoFindAddSpriteRendererByDepthFirst(GameObject go, List<SpriteRenderer> outList, List<int> outIds)
    {
        Vector3 vector = new Vector3(0f, 0f, 0f);
        int childCount = go.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject gameObject = go.transform.GetChild(i).gameObject;
            int nameNumber = GetNameNumber(gameObject.name);
            DoFindAddSpriteRendererByDepthFirstImpl(gameObject, outList, outIds, nameNumber);
            vector += gameObject.transform.position;
        }
        return vector / childCount;
    }

    private static void DoFindAddSpriteRendererByDepthFirstImpl(GameObject go,List<SpriteRenderer> outList,List<int> outIds, int number)
    {
        int childCount = go.transform.childCount;
        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                DoFindAddSpriteRendererByDepthFirstImpl(go.transform.GetChild(i).gameObject, outList, outIds, number);
            }
            return;
        }
        SpriteRenderer[] components = go.GetComponents<SpriteRenderer>();
        int j = 0;
        for (int num = components.Length; j < num; j++)
        {
            outList.Add(components[j]);
            outIds.Add(number);
        }
    }

    private static int GetNameNumber(string name)
    {
        int num = name.IndexOf(A2U.PAIR_BODY, StringComparison.Ordinal);
        if (num < 0)
        {
            return -1;
        }
        num += A2U.PAIR_BODY.Length;
        int num2 = name.IndexOf(A2U.PAIR_SUFFIX, StringComparison.Ordinal);
        if (num2 < 0)
        {
            num2 = name.Length;
        }
        num2 -= num;
        string s = name.Substring(num, num2);
        int result = -1;
        if (!int.TryParse(s, out result))
        {
            return -1;
        }
        return result;
    }

    private static void SetSpritePairs(List<SpritePair> outPiars, List<SpriteRenderer> outSprites, List<SpriteRenderer> sprites, List<int> ids)
    {
        int i = 0;
        for (int count = ids.Count; i < count; i++)
        {
            int num = ids[i];
            SpriteRenderer spriteRenderer = sprites[i];
            if (num < 0)
            {
                outSprites.Add(spriteRenderer);
                continue;
            }
            int num2 = num;
            int j = outPiars.Count;
            for (int num3 = num2 + 1; j < num3; j++)
            {
                outPiars.Add(new SpritePair());
            }
            if (null == outPiars[num2]._first)
            {
                outPiars[num2]._first = spriteRenderer;
            }
            else if (null == outPiars[num2]._second)
            {
                outPiars[num2]._second = spriteRenderer;
            }
        }
    }

    private static void GetAnimationInfo(ref HashNode node, GameObject composition)
    {
        AnimationClip obj = ((node._animator != null) ? node._animator : composition.GetComponent<Animator>()).runtimeAnimatorController.animationClips[0];
        float frameRate = obj.frameRate;
        float length = obj.length;
        node._frameRate = frameRate;
        node._maxFrame = (uint)Mathf.CeilToInt(length * frameRate);
    }

    private static void StartAnimation(GameObject composition, float sec = 0f)
    {
        Animator component = composition.GetComponent<Animator>();
        component.enabled = true;
        DoStartAnimation(component, sec);
    }

    private static void StartAnimation(ref HashNode node, float sec = 0f)
    {
        Animator animator = ((node._animator != null) ? node._animator : node._composition.GetComponent<Animator>());
        if (animator != null)
        {
            animator.enabled = true;
            DoStartAnimation(animator, sec);
        }
    }

    private static void DoStartAnimation(Animator animator, float sec)
    {
        animator.PlayInFixedTime(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, -1, sec);
    }

    private static void StopAnimation(GameObject composition)
    {
        composition.GetComponent<Animator>().enabled = false;
    }

    private static void StopAnimation(ref HashNode node)
    {
        Animator animator = ((node._animator != null) ? node._animator : node._composition.GetComponent<Animator>());
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    private static void SetAnimationSec(GameObject composition, float sec, bool isPause = false)
    {
        Animator component = composition.GetComponent<Animator>();
        DoStartAnimation(component, sec);
        DoSetAnimationPause(component, isPause);
    }

    private static void SetAnimationSec(ref HashNode node, float sec, bool isPause = false)
    {
        Animator animator = ((node._animator != null) ? node._animator : node._composition.GetComponent<Animator>());
        if (animator != null)
        {
            DoStartAnimation(animator, sec);
            DoSetAnimationPause(animator, isPause);
        }
    }

    private static void DoSetAnimationPause(Animator animator, bool isPause)
    {
        animator.speed = (isPause ? 0f : 1f);
    }

    private static void SetAnimationSpeed(GameObject composition, float speed)
    {
        DoSetAnimationSpeed(composition.GetComponent<Animator>(), speed);
    }

    private static void SetAnimationSpeed(ref HashNode node, float speed)
    {
        Animator animator = ((node._animator != null) ? node._animator : node._composition.GetComponent<Animator>());
        if (animator != null)
        {
            DoSetAnimationSpeed(animator, speed);
        }
    }

    private static void DoSetAnimationSpeed(Animator animator, float speed)
    {
        animator.speed = speed;
    }

    private static HashNode MakeHashNode(GameObject target, ref int maxCount)
    {
        HashNode node = default(HashNode);
        node.Init(target);
        GetAnimationInfo(ref node, target);
        List<SpriteRenderer> list = new List<SpriteRenderer>();
        List<int> list2 = new List<int>();
        Vector2 vector = DoFindAddSpriteRendererByDepthFirst(target, list, list2);
        SetSpritePairs(node._spritePairs, node._sprites, list, list2);
        target.transform.localPosition = -vector;
        int num = node._spritePairs.Count + node._sprites.Count;
        if (num > 0)
        {
            if (maxCount < num)
            {
                maxCount = num;
            }
            if (node._spritePairs.Count > 0)
            {
                node._desc._color = node._spritePairs[0]._first.color;
            }
            else
            {
                node._desc._color = node._sprites[0].color;
            }
            node._work = new NodeWork[num];
            for (int i = 0; i < num; i++)
            {
                node._work[i].Init();
                if (i < node._spritePairs.Count)
                {
                    node._work[i]._originalScale = node._spritePairs[i]._first.transform.localScale.x;
                }
                else
                {
                    node._work[i]._originalScale = node._sprites[i - node._spritePairs.Count].transform.localScale.x;
                }
            }
        }
        return node;
    }
}