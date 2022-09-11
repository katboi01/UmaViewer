using System;
using System.IO;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    public static Screenshot Instance;

    private void Awake()
    {
        Instance = this;
    }

    public static void TakeScreenshot()
    {
        var image = GrabFrame(Screen.width, Screen.height);

        string fileName = Application.dataPath + "/../Screenshots/" + string.Format("UmaViewer_{0}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));
        byte[] pngShot = ImageConversion.EncodeToPNG(image);
        Directory.CreateDirectory(Application.dataPath + "/../Screenshots");
        File.WriteAllBytes(fileName + ".png", pngShot);

        Destroy(image);
    }

    public static Texture2D GrabFrame(int width, int height)
    {
        var dimensions = GetResolution(width, height);
        width = dimensions.x;
        height = dimensions.y;

        Camera cam = Camera.main;
        int oldMask = cam.cullingMask;
        var bak_cam_clearFlags = cam.clearFlags;
        cam.cullingMask = ~LayerMask.GetMask("UI");
        cam.clearFlags = CameraClearFlags.Depth;

        var tex_color = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var grab_area = new Rect(0, 0, width, height);

        RenderTexture.active = render_texture;
        cam.targetTexture = render_texture;

        cam.Render();
        tex_color.ReadPixels(grab_area, 0, 0);
        tex_color.Apply();

        cam.clearFlags = bak_cam_clearFlags;
        cam.cullingMask = oldMask;
        cam.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(render_texture);

        return tex_color;
    }

    public static Vector2Int GetResolution(int width, int height)
    {
        if (width == 0 && height == 0)
        {
            width = Screen.width;
            height = Screen.height;
        }
        else if (width == 0)
        {
            float ratio = (float)Screen.width / (float)Screen.height;
            width = Mathf.RoundToInt((float)height * ratio);
        }
        else if (height == 0)
        {
            float ratio = (float)Screen.height / (float)Screen.width;
            height = Mathf.RoundToInt((float)width * ratio);
        }
        return new Vector2Int(width, height);
    }
}