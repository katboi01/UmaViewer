using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cutt;
using System;
using UnityEngine.SceneManagement;
using System.IO;

public class LiveController : MonoBehaviour
{
    class TitleBG
    {
        GameObject baseObject;

        Image image;

        Image titleBG;
        Text title;
        Text lyricist;
        Text composer;

        public TitleBG(GameObject game)
        {
            baseObject = game;

            image = baseObject.GetComponent<Image>();
            titleBG = baseObject.transform.Find("titleBG").GetComponent<Image>();
            title = game.transform.Find("title").GetComponent<Text>();
            lyricist = game.transform.Find("lyricist").GetComponent<Text>();
            composer = game.transform.Find("composer").GetComponent<Text>();
        }

        public void SetValue(SQMusicData data)
        {
            string disc = data.name;
            disc = disc.Replace("\\n", "");
            title.text = disc;
            lyricist.text = "作詞:" + data.lyricist;
            composer.text = "作曲:" + data.composer;
        }
        public Color sourceColor()
        {
            return image.color;
        }

        public void alpha(float alpha)
        {
            Color color = titleBG.color;
            color.a = alpha;

            titleBG.color = color;
            title.color = color;
            lyricist.color = color;
            composer.color = color;
        }

        public void alphaBG(float alpha)
        {
            Color color = image.color;

            color.a = alpha;
            image.color = color;
        }

        public void SetDisable()
        {
            baseObject.SetActive(false);
        }
    }

    [SerializeField]
    private GameObject _Music;
    [SerializeField]
    private GameObject _Director;
        
    private MusicManager musicManager;
    private Director director;

    private float liveTotalTime = 0f;

    private GameObject loadingbg;
    private TitleBG titleBG;
    private PlaySeeker playSeeker;
    private LiveDebug liveDebug;

    private ScreenShot screenShot;
    
    //自動でメニューへ戻る
    private bool isAutoReturnMenu = false;
    //タイトルを表示
    private bool isInitTitle = false;

    //生成が開始されているか
    private bool isStart = false;
    
    private void Awake()
    {
        musicManager = _Music.GetComponent<MusicManager>();

        director = _Director.GetComponent<Director>();

        loadingbg = GameObject.Find("Canvas/LoadingBG").gameObject;
        var title = GameObject.Find("Canvas/TitleBG").gameObject;
        titleBG = new TitleBG(title);

        playSeeker = GameObject.Find("Canvas/SeekBar").GetComponent<PlaySeeker>();
        liveDebug = GameObject.Find("Canvas/Debug").GetComponent<LiveDebug>();

        int ret = SaveManager.GetInt("ReturnTitleEndStage");
        if (ret == 1)
        {
            isAutoReturnMenu = true;
        }
        int inittitle = SaveManager.GetInt("InitTitle");
        if (inittitle == 1)
        {
            isInitTitle = true;
        }

        screenShot = base.gameObject.GetComponent<ScreenShot>();

        SQMusicData data = ViewLauncher.instance.liveDirector.sqMusicData;
        titleBG.SetValue(data);

        if (!isInitTitle)
        {
            titleBG.SetDisable();
        }
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(WaitStarting());
    }

    // Update is called once per frame
    void Update()
    {
        if (isStart)
        {
            //Escapeキーが押されたら終了
            if (Input.GetButtonDown("ReturnMenu"))
            {
                StartCoroutine( DestroyViewScene());
            }
            else if (Input.GetButtonDown("Pause"))
            {
                playSeeker.OnClickButton();
            }
            else if (Input.GetButtonDown("SeekBar"))
            {
                playSeeker.ChangeVisible();
                liveDebug.ChangeVisible();
            }
            else if (Input.GetButtonDown("ScreenShot"))
            {
                StartCoroutine(screenShot.GetScreenshot());
            }
            else if (Input.GetButtonDown("VerticalScreenShot"))
            {
                StartCoroutine(screenShot.GetScreenshot(true));
            }
        }

        //自動リターン
        if (isAutoReturnMenu)
        {
            if (musicManager.currentTime > liveTotalTime + 4f)
            {
                StartCoroutine(DestroyViewScene());
            }
        }
    }

    /// <summary>
    /// LiveViewが破棄されたら呼ばれる
    /// </summary>
    void OnDestroy()
    {
        if (ViewLauncher.instance != null)
        {
            ViewLauncher.instance.LiveEnd();
        }
        if (ResourcesManager.instance != null)
        {
            ResourcesManager.instance.initResource();
        }
        Resources.UnloadUnusedAssets();
    }

    /*
    /// <summary>
    /// スクリーンショットを撮影する
    /// </summary>
    /// <returns></returns>
    public IEnumerator ScreenShot()
    {
        //レンダリングが終わるまで待つ
        yield return new WaitForEndOfFrame();

        //メインカメラを確保
        Camera ArCam = director.mainCamera;

        // アクティブなレンダーテクスチャをキャッシュしておく
        RenderTexture currentRT = RenderTexture.active;

        Vector2Int resolution = new Vector2Int();
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
        Texture2D screenShot = new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, false);
        RenderTexture rt = new RenderTexture(screenShot.width, screenShot.height, 24);
        RenderTexture prev = ArCam.targetTexture;
        ArCam.targetTexture = rt;
        ArCam.Render();
        ArCam.targetTexture = prev;

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        screenShot.Apply();
        RenderTexture.active = currentRT;

        byte[] bytes = screenShot.EncodeToPNG();
        UnityEngine.Object.Destroy(screenShot);

        PathHandler instance = SingletonMonoBehaviour<PathHandler>.instance;
        string tmpdir = instance.screenshotPath;
        string captureFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
        var capturePath = Path.Combine(tmpdir, captureFileName);

        print("ScreenShot:" + capturePath);
        File.WriteAllBytes(capturePath, bytes);
        /*
        if (File.Exists(capturePath)) File.Delete(capturePath);
        ScreenCapture.CaptureScreenshot(capturePath, 2);
        yield return new WaitUntil(() => { return File.Exists(capturePath); });
        
    }
    */

    private void LateUpdate()
    {
    }

    IEnumerator WaitStarting()
    {
        while (true)
        {
            if (musicManager.isLoad)
            {
                if (director.assetTaskState == Director.eAssetTaskState.Done)
                {
                    break;
                }
            }
            yield return null;
        }

        liveTotalTime = musicManager.totalLength;
        
        yield return new WaitForSeconds(0.2f);

        yield return Play();
    }

    /// <summary>
    /// ライブ開始
    /// </summary>
    public IEnumerator Play()
    {
        if (isInitTitle)
        {
            yield return FadeoutBG();
            yield return TitleBGExec();
        }
        else
        {
            yield return FadeoutBG();
        }

        isStart = true;
        musicManager.PlaySong();
    }

    private IEnumerator FadeoutBG()
    {
        Image image = loadingbg.GetComponent<Image>();
        
        while(image.color.a > 0)
        {
            Color color = image.color;
            color.a = color.a - 0.2f;
            if(color.a < 0)
            {
                color.a = 0;
            }
            image.color = color;
            yield return null;
        }

        //loadingbg.SetActive(false);
    }

    /// <summary>
    /// タイトルを表示する
    /// </summary>
    /// <returns></returns>
    private IEnumerator TitleBGExec()
    {

        float colorAlpha = 0f;

        while (colorAlpha < 1f)
        {
            colorAlpha += 0.05f;
            if (colorAlpha > 1)
            {
                colorAlpha = 1;
            }
            titleBG.alpha(colorAlpha);

            yield return null;
        }

        yield return new WaitForSeconds(3f);//1秒

        while (colorAlpha > 0f)
        {
            colorAlpha -= 0.05f;
            if (colorAlpha < 0)
            {
                colorAlpha = 0;
            }
            titleBG.alpha(colorAlpha);

            yield return null;
        }
        //yield return new WaitForSeconds(0.2f);

        colorAlpha = titleBG.sourceColor().a;
        while (colorAlpha > 0f)
        {
            colorAlpha -= 0.05f;
            if (colorAlpha < 0)
            {
                colorAlpha = 0;
            }
            titleBG.alphaBG(colorAlpha);

            yield return null;
        }
    }

    public IEnumerator DestroyViewScene()
    {
        SceneManager.LoadScene("menu");
        yield return null;
        /* 非同期で読み込むとなぜかToggleSwitchが7回読み込まれる
         * 
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("menu");
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        */
    }
}
