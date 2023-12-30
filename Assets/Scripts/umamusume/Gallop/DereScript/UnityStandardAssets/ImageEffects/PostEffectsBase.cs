using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class PostEffectsBase : MonoBehaviour
    {
        private static bool supportDX11 = false;

        private static bool supportImageEffects = true;

        private static bool supportHDRTextures = true;

        private static bool supportDepthTexture = true;

        protected Camera _camera;

        protected bool isSupported = true;

        private void Awake()
        {
            supportImageEffects = SystemInfo.supportsImageEffects;
            supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
            supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
            supportDepthTexture = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
        }

        protected Material CheckShaderAndCreateMaterial(Shader s, Material m2Create)
        {
            if (!s)
            {
                Debug.Log("Missing shader in " + ToString());
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
                Debug.Log("The shader " + s.ToString() + " on effect " + ToString() + " is not supported on this platform!");
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

        protected Material CreateMaterial(Shader s, Material m2Create)
        {
            if (!s)
            {
                Debug.Log("Missing shader in " + ToString());
                return null;
            }
            if ((bool)m2Create && m2Create.shader == s && s.isSupported)
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

        private void OnEnable()
        {
            isSupported = true;
        }

        protected bool CheckSupport()
        {
            return CheckSupport(needDepth: false);
        }

        public virtual bool CheckResources()
        {
            Debug.LogWarning("CheckResources () for " + ToString() + " should be overwritten.");
            return isSupported;
        }

        protected void Start()
        {
            CheckResources();
        }

        protected bool CheckSupport(bool needDepth)
        {
            isSupported = true;
            if (!supportImageEffects)
            {
                NotSupported();
                return false;
            }
            if (needDepth && !supportDepthTexture)
            {
                NotSupported();
                return false;
            }
            if (needDepth && _camera == null)
            {
                _camera = GetComponent<Camera>();
            }
            return true;
        }

        protected bool CheckSupport(bool needDepth, bool needHdr)
        {
            if (!CheckSupport(needDepth))
            {
                return false;
            }
            if (needHdr && !supportHDRTextures)
            {
                NotSupported();
                return false;
            }
            return true;
        }

        public bool Dx11Support()
        {
            return supportDX11;
        }

        protected void ReportAutoDisable()
        {
            Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
        }

        private bool CheckShader(Shader s)
        {
            Debug.Log("The shader " + s.ToString() + " on effect " + ToString() + " is not part of the Unity 3.2+ effects suite anymore. For best performance and quality, please ensure you are using the latest Standard Assets Image Effects (Pro only) package.");
            if (!s.isSupported)
            {
                NotSupported();
                return false;
            }
            return false;
        }

        protected void NotSupported()
        {
            base.enabled = false;
            isSupported = false;
        }

        protected void DrawBorder(RenderTexture dest, Material material)
        {
            RenderTexture.active = dest;
            bool flag = true;
            GL.PushMatrix();
            GL.LoadOrtho();
            for (int i = 0; i < material.passCount; i++)
            {
                material.SetPass(i);
                float y;
                float y2;
                if (flag)
                {
                    y = 1f;
                    y2 = 0f;
                }
                else
                {
                    y = 0f;
                    y2 = 1f;
                }
                float x = 0f + 1f / (dest.width * 1f);
                float y3 = 0f;
                float y4 = 1f;
                GL.Begin(7);
                GL.TexCoord2(0f, y);
                GL.Vertex3(0f, y3, 0.1f);
                GL.TexCoord2(1f, y);
                GL.Vertex3(x, y3, 0.1f);
                GL.TexCoord2(1f, y2);
                GL.Vertex3(x, y4, 0.1f);
                GL.TexCoord2(0f, y2);
                GL.Vertex3(0f, y4, 0.1f);
                float x2 = 1f - 1f / (dest.width * 1f);
                x = 1f;
                y3 = 0f;
                y4 = 1f;
                GL.TexCoord2(0f, y);
                GL.Vertex3(x2, y3, 0.1f);
                GL.TexCoord2(1f, y);
                GL.Vertex3(x, y3, 0.1f);
                GL.TexCoord2(1f, y2);
                GL.Vertex3(x, y4, 0.1f);
                GL.TexCoord2(0f, y2);
                GL.Vertex3(x2, y4, 0.1f);
                x = 1f;
                y3 = 0f;
                y4 = 0f + 1f / (dest.height * 1f);
                GL.TexCoord2(0f, y);
                GL.Vertex3(0f, y3, 0.1f);
                GL.TexCoord2(1f, y);
                GL.Vertex3(x, y3, 0.1f);
                GL.TexCoord2(1f, y2);
                GL.Vertex3(x, y4, 0.1f);
                GL.TexCoord2(0f, y2);
                GL.Vertex3(0f, y4, 0.1f);
                x = 1f;
                y3 = 1f - 1f / (dest.height * 1f);
                y4 = 1f;
                GL.TexCoord2(0f, y);
                GL.Vertex3(0f, y3, 0.1f);
                GL.TexCoord2(1f, y);
                GL.Vertex3(x, y3, 0.1f);
                GL.TexCoord2(1f, y2);
                GL.Vertex3(x, y4, 0.1f);
                GL.TexCoord2(0f, y2);
                GL.Vertex3(0f, y4, 0.1f);
                GL.End();
            }
            GL.PopMatrix();
        }
    }
}
