using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenFade : MonoBehaviour
{
    private Material _material;

    private int _fadeColorPropID;

    public RenderTexture renderTexture { get; set; }
    public Color fadeColor { get; set; }

    public void Initialize()
    {
        if (_material == null)
        {
            Shader shader = null;
            //PostEffectLive3D.LoadRichPostEffectShader(shader, "Cygames/ImageEffects/ScreenFade", false);
            shader = ResourcesManager.instance.GetShader("ScreenFade"); //DV用
            _material = new Material(shader);
            _material.hideFlags = HideFlags.DontSave;
        }
    }

    private void Awake()
    {
        _fadeColorPropID = Shader.PropertyToID("_Color");
    }

    private void OnDestroy()
    {
        if (_material != null)
        {
            Object.DestroyImmediate(_material);
            _material = null;
        }
    }

    public void Work()
    {
        if (!(_material == null) && !(renderTexture == null) && !Mathf.Approximately(fadeColor.a, 0f))
        {
            _material.SetColor(_fadeColorPropID, fadeColor);
            RenderTexture temporary = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0);
            Graphics.Blit(renderTexture, temporary);
            Graphics.Blit(temporary, renderTexture, _material);
            RenderTexture.ReleaseTemporary(temporary);
        }
    }
}
