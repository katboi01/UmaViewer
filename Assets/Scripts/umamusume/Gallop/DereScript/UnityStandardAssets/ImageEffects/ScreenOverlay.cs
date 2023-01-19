using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Other/Screen Overlay")]
    public class ScreenOverlay : PostEffectsBase
    {
        public enum OverlayBlendMode
        {
            Additive,
            ScreenBlend,
            Multiply,
            Overlay,
            AlphaBlend
        }

        public OverlayBlendMode blendMode = OverlayBlendMode.Overlay;

        public float intensity = 1f;

        public Texture2D texture;

        public Shader overlayShader;

        private Material overlayMaterial;

        public override bool CheckResources()
        {
            CheckSupport(needDepth: false);
            overlayMaterial = CheckShaderAndCreateMaterial(overlayShader, overlayMaterial);
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
            Vector4 value = new Vector4(1f, 0f, 0f, 1f);
            overlayMaterial.SetVector("_UV_Transform", value);
            overlayMaterial.SetFloat("_Intensity", intensity);
            overlayMaterial.SetTexture("_Overlay", texture);
            Graphics.Blit(source, destination, overlayMaterial, (int)blendMode);
        }
    }
}
