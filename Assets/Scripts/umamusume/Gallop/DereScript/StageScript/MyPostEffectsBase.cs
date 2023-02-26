using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("")]
public abstract class MyPostEffectsBase : MonoBehaviour
{
    protected bool supportHDRTextures = true;

    protected bool supportDX11;

    protected bool isSupported = true;

    private bool mStarted;

    private bool isSupportDepth;

    private bool isSupportImageEffect;

    protected bool isCheckSupportInitialize;

    public Material CheckShaderAndCreateMaterial(Shader s, Material m2Create)
    {
        if (s == null)
        {
            base.enabled = false;
            return null;
        }
        if (s.isSupported && (bool)m2Create && m2Create.shader == s)
        {
            return m2Create;
        }
        if (!s.isSupported)
        {
            NotSupported();
            return null;
        }
        m2Create = new Material(s);
        m2Create.hideFlags = HideFlags.DontSave;
        if ((bool)m2Create)
        {
            return m2Create;
        }
        return null;
    }

    public Material CreateMaterial(Shader s, Material m2Create)
    {
        if (s == null)
        {
            return null;
        }
        if (m2Create != null && m2Create.shader == s && s.isSupported)
        {
            return m2Create;
        }
        if (!s.isSupported)
        {
            return null;
        }
        m2Create = new Material(s);
        m2Create.hideFlags = HideFlags.DontSave;
        if ((bool)m2Create)
        {
            return m2Create;
        }
        return null;
    }

    public abstract bool CheckResources();

    public bool CheckSupport(bool needDepth, out bool isUsableDepth)
    {
        isSupported = true;
        isUsableDepth = false;
        if (!isCheckSupportInitialize)
        {
            supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
            supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
            isSupportDepth = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
            isSupportImageEffect = SystemInfo.supportsImageEffects;
            if (!isSupportImageEffect)
            {
                NotSupported();
                return false;
            }
            isCheckSupportInitialize = true;
        }
        else if (!isSupportImageEffect)
        {
            NotSupported();
            return false;
        }
        if (needDepth && isSupportDepth)
        {
            isUsableDepth = true;
        }
        return true;
    }

    public void ReportAutoDisable()
    {
    }

    public void NotSupported()
    {
        isSupported = false;
    }

    private void OnEnable()
    {
        isCheckSupportInitialize = false;
        if (mStarted)
        {
            CheckResources();
        }
    }

    protected virtual void Start()
    {
        mStarted = true;
        CheckResources();
    }
}
