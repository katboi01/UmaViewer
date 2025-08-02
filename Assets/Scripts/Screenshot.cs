using System;
using System.Collections;
using System.IO;
using uGIF;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Image = uGIF.Image;

public class Screenshot : MonoBehaviour
{
    public static Screenshot Instance;
    bool recording = false;

    static UISettingsAnimation AnimationSettings => UmaViewerUI.Instance.AnimationSettings;
    static UISettingsScreenshot ScreenshotSettings => UmaViewerUI.Instance.ScreenshotSettings;

    private void Awake()
    {
        Instance = this;
    }

    public static void TakeScreenshot()
    {
        var camera = GetActiveCamera();
        int width = ScreenshotSettings.Width;
        int height = ScreenshotSettings.Height;
        width = width == -1 ? Screen.width : width;
        height = height == -1 ? Screen.height : height;
        var image = GrabFrame(camera, width, height, ScreenshotSettings.Transparent);

#if UNITY_ANDROID && !UNITY_EDITOR
        string fileDirectory = Application.persistentDataPath + "/../Screenshots/";
#else
        string fileDirectory = Application.dataPath + "/../Screenshots/";
#endif

        string fileName = fileDirectory + string.Format("UmaViewer_{0}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));
        byte[] pngShot = ImageConversion.EncodeToPNG(image);
        Directory.CreateDirectory(fileDirectory);
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
        ScreenshotSettings.GifButton.interactable = false;
        StartCoroutine(RecordGif(ScreenshotSettings.GifWidth, ScreenshotSettings.GifHeight, ScreenshotSettings.GifTransparent, 10)); //(int)UmaViewerUI.Instance.GifQuality.value));
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
        var clipLength = animeClip.length;
        var frameRate = animeClip.frameRate;
        var clipFrameCount = Mathf.RoundToInt(clipLength * frameRate);
        
        StartCoroutine(CaptureToGIFCustom.Instance.Encode(frameRate, quality));

        AnimationSettings.ChangeSpeed(0);
        AnimationSettings.ChangeProgress(0);
        yield return new WaitForSeconds(1); //wait for dynamicBone to settle;

        while (frame < clipFrameCount)
        {
            AnimationSettings.ChangeProgress((float)frame / clipFrameCount);
            yield return new WaitForEndOfFrame();
            var tex = GrabFrame(camera, width, height, transparent);
            CaptureToGIFCustom.Instance.Frames.Add(new Image(tex));
            Destroy(tex);
            frame++;
        }

        recording = false;
        AnimationSettings.ChangeProgress(0);
        AnimationSettings.ChangeSpeed(1);
        ppLayer.enabled = oldPpState;
        CaptureToGIFCustom.Instance.stop = true;
        ScreenshotSettings.GifButton.interactable = true;
    }

    public void BeginRecordImageSequence()
    {
        if (recording || UmaViewerBuilder.Instance.CurrentUMAContainer == null) return;
        recording = true;
        ScreenshotSettings.SequenceButton.interactable = false;
        StartCoroutine(RecordSequence(ScreenshotSettings.GifWidth, ScreenshotSettings.GifHeight, ScreenshotSettings.GifTransparent, 10, ScreenshotSettings.GifFPS)); //(int)UmaViewerUI.Instance.GifQuality.value));
    }

    private IEnumerator RecordSequence(int width, int height, bool transparent, int quality, int frameRateOverride = -1)
    {
        ScreenshotSettings.SequenceButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Recording...";
        string folderName = string.Format("UmaViewer_{0}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));
#if UNITY_ANDROID && !UNITY_EDITOR
        string fileDirectory = Application.persistentDataPath + "/../Screenshots/" + folderName;
#else
        string fileDirectory = Application.dataPath + "/../Screenshots/" + folderName;
#endif
        Directory.CreateDirectory(fileDirectory);

        var camera = GetActiveCamera();
        var ppLayer = camera.GetComponent<PostProcessLayer>();
        bool oldPpState = ppLayer.enabled;
        ppLayer.enabled = false;

        var uma = UmaViewerBuilder.Instance.CurrentUMAContainer;
        var animator = uma.UmaAnimator;
        if (animator == null) yield break;

        int frame = 0;
        var animeClip = uma.OverrideController["clip_2"];
        var clipLength = animeClip.length;
        var frameRate = animeClip.frameRate;
        var clipFrameCount = Mathf.RoundToInt(clipLength * (frameRateOverride == -1? frameRate : frameRateOverride));
        var maxNameLength = clipFrameCount.ToString().Length + 1;

        AnimationSettings.ChangeSpeed(0);
        AnimationSettings.ChangeProgress(0);
        yield return new WaitForSeconds(1); //wait for dynamicBone to settle;

        while (frame < clipFrameCount)
        {
            AnimationSettings.ChangeProgress((float)frame / clipFrameCount);
            yield return new WaitForEndOfFrame();
            var tex = GrabFrame(camera, width, height, transparent);
            var bytes = tex.EncodeToPNG();

            var fileName = $"{fileDirectory}/{frame.ToString().PadLeft(maxNameLength, '0')}.png";
            File.WriteAllBytes(fileName, bytes);

            Destroy(tex);
            frame++;
        }

        recording = false;
        AnimationSettings.ChangeProgress(0);
        AnimationSettings.ChangeSpeed(1);
        ppLayer.enabled = oldPpState;
        ScreenshotSettings.SequenceButton.interactable = true;
        ScreenshotSettings.SequenceButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Record PNG Sequence";

        var fullpath = Path.GetFullPath(folderName);
        UmaViewerUI.Instance.ShowMessage($"PNG Sequence saved: {fullpath}", UIMessageType.Success);
    }

    public static Texture2D GrabFrame(Camera cam, int width, int height, bool transparent = true)
    {
        var dimensions = GetResolution(width, height);
        width = dimensions.x;
        height = dimensions.y;

        int oldMask = cam.cullingMask;
        var oldClearFlags = cam.clearFlags;
        Color oldBG = cam.backgroundColor;

        cam.cullingMask = ~LayerMask.GetMask("UI");
        if (transparent)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(0, 0, 0, 0);
        }

        var tex_color = new Texture2D(width, height, TextureFormat.ARGB32, false);
        int aaLevel = QualitySettings.antiAliasing == 0? 1 : QualitySettings.antiAliasing; //RT uses '1' for no antialiasing
        var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32,RenderTextureReadWrite.Default, aaLevel);
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