using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class ScreenShot : MonoBehaviour
{
    private RenderTexture _colorTexture;

    private RenderTexture _depthTexture;

    private RenderTexture _resultTexture;

    private Director _director;

    private FinalizeCamera _finalizeCamera;

    //スクリーンショットの設定
    private int screenshotSetting = 0;

    private Vector2Int resolution;

    private void Awake()
    {
        _director = GameObject.Find("Director").GetComponent<Director>();

        //セーブの読み込み
        screenshotSetting = SaveManager.GetInt("ScreenShot");

        switch (screenshotSetting)
        {
            case 0:
                resolution.x = Screen.width;
                resolution.y = Screen.height;
                break;
            case 1:
                resolution.x = 1920;
                resolution.y = 1080;
                break;
            case 2:
                resolution.x = 3840;
                resolution.y = 2160;
                break;
        }
        int width = resolution.x;
        int height = resolution.y;

        _colorTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        _depthTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
        _resultTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
    }

    // Use this for initialization
    IEnumerator Start()
    {
        while (true)
        {
            if (_director.assetTaskState == Director.eAssetTaskState.Done)
            {
                break;
            }
            yield return null;
        }

        _finalizeCamera = GameObject.Find("Director").GetComponent<FinalizeCamera>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        //解放
        RenderTexture.ReleaseTemporary(_colorTexture);
        RenderTexture.ReleaseTemporary(_depthTexture);
        RenderTexture.ReleaseTemporary(_resultTexture);
        _colorTexture = null;
        _depthTexture = null;
        _resultTexture = null;
    }

    public IEnumerator GetScreenshot(bool vertical = false)
    {
        if (_director != null && _finalizeCamera != null && PostEffectSetting.ScreenShot == null)
        {
            if (screenshotSetting == 0)
            {
                if (_colorTexture.width != Screen.width || _colorTexture.height != Screen.height)
                {
                    _colorTexture.Release();
                    _colorTexture.width = Screen.width;
                    _colorTexture.height = Screen.height;
                    _colorTexture.Create();
                    _depthTexture.Release();
                    _depthTexture.width = Screen.width;
                    _depthTexture.height = Screen.height;
                    _depthTexture.Create();
                    _resultTexture.Release();
                    _resultTexture.width = Screen.width;
                    _resultTexture.height = Screen.height;
                    _resultTexture.Create();
                }
            }

            _director.SetScreenShotTexture(_colorTexture, _depthTexture, _resultTexture);

            //フラグを立てておく（PostEffectLive3Dで確認しスクリーンショットを取得する）
            PostEffectSetting.captureScreenShot = true;

            //取り終わるまで待ち
            while (PostEffectSetting.captureScreenShot)
            {
                yield return null;
            }

            //戻して
            _director.ReSetRenderTexture();

            yield return new WaitForEndOfFrame();

            //保存処理
            StartCoroutine(SaveScreenShot());
        }
    }

    public IEnumerator SaveScreenShot()
    {
        // アクティブなレンダーテクスチャをキャッシュしておく
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = PostEffectSetting.ScreenShot;

        Texture2D screenShot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        if (SystemInfo.supportsAsyncGPUReadback)
        {
            //非同期処理
            var reqest = AsyncGPUReadback.Request(renderTexture);

            //取り終わるまで待ち
            while (true)
            {
                if (reqest.hasError)
                {
                    Debug.Log("GPU readback error detected.");
                    yield break;
                }
                else if (reqest.done)
                {
                    break;
                }
                yield return null;
            }

            Unity.Collections.NativeArray<Color32> buffer = reqest.GetData<Color32>();
            screenShot.LoadRawTextureData(buffer);
            screenShot.Apply();
        }
        else
        {
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            screenShot.Apply();
            RenderTexture.active = currentRT;
        }

        byte[] bytes = screenShot.EncodeToPNG();
        UnityEngine.Object.Destroy(screenShot);

        PathHandler instance = SingletonMonoBehaviour<PathHandler>.instance;
        string tmpdir = instance.screenshotPath;
        string captureFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
        var capturePath = Path.Combine(tmpdir, captureFileName);

        print("ScreenShot:" + capturePath);
        File.WriteAllBytes(capturePath, bytes);

        RenderTexture.ReleaseTemporary(PostEffectSetting.ScreenShot);
        PostEffectSetting.ScreenShot = null;
    }

}