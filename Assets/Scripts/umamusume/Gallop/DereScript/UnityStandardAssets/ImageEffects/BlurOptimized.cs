using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Blur/Blur (Optimized)")]
    public class BlurOptimized : PostEffectsBase
    {
        [Range(0f, 2f)]
        public int downsample = 1;

        [Range(0f, 10f)]
        public float blurSize = 3f;

        [Range(1f, 4f)]
        public int blurIterations = 2;

        public Shader blurShader;

        private Material blurMaterial;

        public override bool CheckResources()
        {
            CheckSupport(needDepth: false);
            blurMaterial = CheckShaderAndCreateMaterial(blurShader, blurMaterial);
            if (blurMaterial == null)
            {
                return false;
            }
            if (!isSupported)
            {
                ReportAutoDisable();
            }
            return isSupported;
        }

        public void OnDisable()
        {
            if ((bool)blurMaterial)
            {
                Object.DestroyImmediate(blurMaterial);
            }
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!CheckResources())
            {
                Graphics.Blit(source, destination);
                return;
            }
            float num = 1f / (1f * (1 << downsample));
            blurMaterial.SetVector("_Parameter", new Vector4(blurSize * num, (0f - blurSize) * num, 0f, 0f));
            source.filterMode = FilterMode.Bilinear;
            int width = source.width >> downsample;
            int height = source.height >> downsample;
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0, source.format);
            renderTexture.filterMode = FilterMode.Bilinear;
            Graphics.Blit(source, renderTexture, blurMaterial, 0);
            int num2 = 0;
            for (int i = 0; i < blurIterations; i++)
            {
                float num3 = i * 1f;
                blurMaterial.SetVector("_Parameter", new Vector4(blurSize * num + num3, (0f - blurSize) * num - num3, 0f, 0f));
                RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, source.format);
                temporary.filterMode = FilterMode.Bilinear;
                Graphics.Blit(renderTexture, temporary, blurMaterial, 1 + num2);
                RenderTexture.ReleaseTemporary(renderTexture);
                renderTexture = temporary;
                temporary = RenderTexture.GetTemporary(width, height, 0, source.format);
                temporary.filterMode = FilterMode.Bilinear;
                Graphics.Blit(renderTexture, temporary, blurMaterial, 2 + num2);
                RenderTexture.ReleaseTemporary(renderTexture);
                renderTexture = temporary;
            }
            Graphics.Blit(renderTexture, destination);
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }
}