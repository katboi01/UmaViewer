using UnityEngine;

public class A2UPostEffect : MonoBehaviour
{
    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (null == A2U.camera)
        {
            Graphics.Blit(src, dst);
        }
        else
        {
            A2U.manager.a2uCamera.a2uRenderer.DoRenderImage(src, dst);
        }
    }
}
