using Cutt;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorController : MonoBehaviour
{
    [SerializeField]
    private string _projectorName = string.Empty;

    private int _projectorNameHash;

    private Material[] _projectorMaterials;

    private Transform _tm;

    private Renderer[] _renderers;

    private Animation _animation;

    private Dictionary<int, AnimationState> _animationStates = new Dictionary<int, AnimationState>();

    private Dictionary<int, string> _animClipName = new Dictionary<int, string>();

    private MaterialPropertyBlock _materialPropertyBlock;

    private static bool s_bInitShaderPropertyToID = false;

    private static int s_MulColor0PropertyID;

    private static int s_ColorPowerPropertyID;

    private static int s_SortingOrderOffset;

    private bool _isStart;

    public Material[] projectorMaterials
    {
        set
        {
            _projectorMaterials = value;
        }
    }

    public static void ResetSortingOrderOffset()
    {
        s_SortingOrderOffset = 0;
    }

    private void Awake()
    {
        if (!s_bInitShaderPropertyToID)
        {
            s_bInitShaderPropertyToID = true;
            s_MulColor0PropertyID = Shader.PropertyToID("_MulColor0");
            s_ColorPowerPropertyID = Shader.PropertyToID("_ColorPower");
        }
    }

    private void Start()
    {
        _tm = base.transform;
        _projectorNameHash = FNVHash.Generate(_projectorName);
        _renderers = GetComponentsInChildren<Renderer>();
        _animation = GetComponent<Animation>();
        if (_animation != null && _animation.GetClipCount() > 0)
        {
            _animationStates.Clear();
            _animClipName.Clear();
            foreach (AnimationState item in _animation)
            {
                int startIndex = item.clip.name.LastIndexOf("_", StringComparison.Ordinal) + 1;
                int key = int.Parse(item.clip.name.Substring(startIndex)) - 1;
                _animationStates.Add(key, item);
                _animClipName.Add(key, item.clip.name);
            }
        }
        Renderer[] renderers = _renderers;
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingOrder += s_SortingOrderOffset;
        }
        s_SortingOrderOffset++;
        _materialPropertyBlock = new MaterialPropertyBlock();
        _materialPropertyBlock.SetColor(s_MulColor0PropertyID, Color.white);
        _materialPropertyBlock.SetFloat(s_ColorPowerPropertyID, 1f);
        _isStart = true;
    }

    public void UpdateStatus(ref ProjectorUpdateInfo updateInfo, float power)
    {
        if (!_isStart || updateInfo.data.nameHash != _projectorNameHash)
        {
            return;
        }
        _tm.position = updateInfo.position;
        Vector3 vector = default(Vector3);
        vector.x = updateInfo.rotateXZ.x % 360f;
        vector.y = updateInfo.rotate % 360f;
        vector.z = updateInfo.rotateXZ.y % 360f;
        _tm.eulerAngles = vector;
        vector.x = updateInfo.size.x;
        vector.y = 1f;
        vector.z = updateInfo.size.y;
        _tm.localScale = vector;
        if (_projectorMaterials != null && updateInfo.materialID >= 0 && updateInfo.materialID < _projectorMaterials.Length)
        {
            Material sharedMaterial = _projectorMaterials[updateInfo.materialID];
            _materialPropertyBlock.SetColor(s_MulColor0PropertyID, updateInfo.color1);
            _materialPropertyBlock.SetFloat(s_ColorPowerPropertyID, updateInfo.power * power);
            bool flag = true;
            if (updateInfo.power * power <= Mathf.Epsilon)
            {
                flag = false;
            }
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = flag;
                _renderers[i].sharedMaterial = sharedMaterial;
                _renderers[i].SetPropertyBlock(_materialPropertyBlock);
            }
        }
        if (_animationStates != null && _animation != null && _animationStates.ContainsKey(updateInfo.motionID))
        {
            int motionID = updateInfo.motionID;
            AnimationState animationState = _animationStates[motionID];
            string animation = _animClipName[motionID];
            if (!_animation.IsPlaying(animation))
            {
                _animation.Stop();
                _animation.Play(animation);
            }
            animationState.time = updateInfo.progressTime * updateInfo.speed;
            animationState.enabled = true;
            _animation.Sample();
            animationState.enabled = false;
        }
    }
}
