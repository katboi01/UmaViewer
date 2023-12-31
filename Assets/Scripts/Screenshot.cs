using System;
using System.Collections;
using System.IO;
using uGIF;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;
using Image = uGIF.Image;

public class Screenshot : MonoBehaviour
{
    public static Screenshot Instance;
    bool recording = false;

    private void Awake()
    {
        Instance = this;
    }

    public static void TakeScreenshot()
    {
        var camera = GetActiveCamera();
        int width = int.Parse(UmaViewerUI.Instance.SSWidth.text);
        int height = int.Parse(UmaViewerUI.Instance.SSHeight.text);
        width = width == -1 ? Screen.width : width;
        height = height == -1 ? Screen.height : height;
        var image = GrabFrame(camera, width, height, UmaViewerUI.Instance.SSTransparent.isOn);

        string fileName = Application.dataPath + "/../Screenshots/" + string.Format("UmaViewer_{0}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));
        byte[] pngShot = ImageConversion.EncodeToPNG(image);
        Directory.CreateDirectory(Application.dataPath + "/../Screenshots");
        //fixes "/../" in path
        var fullpath = Path.GetFullPath($"{fileName}.png");
        File.WriteAllBytes(fullpath, pngShot);
        UmaViewerUI.Instance.ShowMessage($"Screenshot saved: {fullpath}", UIMessageType.Success);
        Destroy(image);
    }

    public void BeginRecordGif()
    {
        if (recording || UmaViewerBuilder.Instance.CurrentUMAContainer == null) return;
        recording = true;
        UmaViewerUI.Instance.GifButton.interactable = false;
        UmaViewerUI.Instance.GifSlider.value = 0;
        StartCoroutine(RecordGif(int.Parse(UmaViewerUI.Instance.GifWidth.text), int.Parse(UmaViewerUI.Instance.GifHeight.text), UmaViewerUI.Instance.GifTransparent.isOn, 10)); //(int)UmaViewerUI.Instance.GifQuality.value));
    }


    private IEnumerator RecordGif(int width, int height, bool transparent, int quality)
    {
        var camera = GetActiveCamera();
        var ppLayer = camera.GetComponent<PostProcessLayer>();
        bool oldPpState = ppLayer.enabled;
        ppLayer.enabled = false;

        var uma = UmaViewerBuilder.Instance.CurrentUMAContainer;
        var animator = uma.UmaAnimator;
        if (animator == null) yield break;

        int frame = 0;
        var animeClip = uma.OverrideController["clip_2"];
        var clipFrameCount = Mathf.RoundToInt(animeClip.length * animeClip.frameRate);
        StartCoroutine(CaptureToGIFCustom.Instance.Encode(animeClip.frameRate, quality));

        UmaViewerUI.Instance.AnimationSpeedChange(0);
        UmaViewerUI.Instance.AnimationProgressChange(0);
        yield return new WaitForSeconds(1); //wait for dynamicBone to settle;

        while (frame < clipFrameCount)
        {
            UmaViewerUI.Instance.GifSlider.value = (float)frame / clipFrameCount;
            UmaViewerUI.Instance.AnimationProgressChange((float)frame / clipFrameCount);
            yield return new WaitForEndOfFrame();
            var tex = GrabFrame(camera, width, height, transparent, transparent);
            CaptureToGIFCustom.Instance.Frames.Add(new Image(tex));
            Destroy(tex);
            frame++;
        }

        recording = false;
        UmaViewerUI.Instance.AnimationProgressChange(0);
        UmaViewerUI.Instance.AnimationSpeedChange(1);
        ppLayer.enabled = oldPpState;
        CaptureToGIFCustom.Instance.stop = true;
        UmaViewerUI.Instance.GifButton.interactable = true;
        UmaViewerUI.Instance.GifSlider.value = 1;
    }

    public static Texture2D GrabFrame(Camera cam, int width, int height, bool transparent = true, bool gifBackground = false)
    {
        var dimensions = GetResolution(width, height);
        width = dimensions.x;
        height = dimensions.y;

        int oldMask = cam.cullingMask;
        var oldClearFlags = cam.clearFlags;
        Color oldBG = cam.backgroundColor;

        cam.cullingMask = ~LayerMask.GetMask("UI");
        if (gifBackground)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(0, 0, 0, 0);
        }
        else if (transparent)
        {
            cam.clearFlags = CameraClearFlags.Depth;
        }

        var tex_color = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32,RenderTextureReadWrite.Default,8);
        var grab_area = new Rect(0, 0, width, height);

        RenderTexture.active = render_texture;
        cam.targetTexture = render_texture;

        cam.Render();
        tex_color.ReadPixels(grab_area, 0, 0);
        tex_color.Apply();

        cam.clearFlags = oldClearFlags;
        cam.cullingMask = oldMask;
        cam.backgroundColor = oldBG;
        cam.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(render_texture);

        return tex_color;
    }

    private static Camera GetActiveCamera()
    {
        //sometimes animation camera gameobject is enabled even when camera component is disabled
        return UmaViewerBuilder.Instance.AnimationCamera.isActiveAndEnabled ? UmaViewerBuilder.Instance.AnimationCamera : Camera.main;
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