using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{

    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Camera/Tilt Shift (Lens Blur)")]
    internal class TiltShift : PostEffectsBase
    {
        public enum TiltShiftMode
        {
            TiltShiftMode,
            IrisMode
        }

        public enum TiltShiftQuality
        {
            Preview,
            Normal,
            High
        }

        public TiltShiftMode mode;

        public TiltShiftQuality quality = TiltShiftQuality.Normal;

        [Range(0f, 15f)]
        public float blurArea = 1f;

        [Range(0f, 25f)]
        public float maxBlurSize = 5f;

        [Range(0f, 1f)]
        public int downsample;

        public Shader tiltShiftShader;

        private Material tiltShiftMaterial;

        public override bool CheckResources()
        {
            CheckSupport(needDepth: true);
            tiltShiftMaterial = CheckShaderAndCreateMaterial(tiltShiftShader, tiltShiftMaterial);
            if (!isSupported)
            {
                ReportAutoDisable();
            }
            return isSupported;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!CheckResources())
            {
                Graphics.Blit(source, destination);
                return;
            }
            tiltShiftMaterial.SetFloat("_BlurSize", (maxBlurSize < 0f) ? 0f : maxBlurSize);
            tiltShiftMaterial.SetFloat("_BlurArea", blurArea);
            source.filterMode = FilterMode.Bilinear;
            RenderTexture renderTexture = destination;
            if ((float)downsample > 0f)
            {
                renderTexture = RenderTexture.GetTemporary(source.width >> downsample, source.height >> downsample, 0, source.format);
                renderTexture.filterMode = FilterMode.Bilinear;
            }
            int num = (int)quality;
            num *= 2;
            Graphics.Blit(source, renderTexture, tiltShiftMaterial, (mode == TiltShiftMode.TiltShiftMode) ? num : (num + 1));
            if (downsample > 0)
            {
                tiltShiftMaterial.SetTexture("_Blurred", renderTexture);
                Graphics.Blit(source, destination, tiltShiftMaterial, 6);
            }
            if (renderTexture != destination)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }
    }
}