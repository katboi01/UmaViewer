using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIDBNotice : MonoBehaviour
{

    private void Awake()
    {
        base.name = base.name.Replace("(Clone)", "");
        WindowSizeSetter();
    }

    private void WindowSizeSetter()
    {
        //ウィンドウのサイズを取得
        Vector2 windowSize = GameObject.Find("Canvas").GetComponent<RectTransform>().sizeDelta;

        //自分のサイズを取得
        Vector3 mylocalScale = GetComponent<RectTransform>().localScale;
        Vector2 mysize = GetComponent<RectTransform>().sizeDelta;

        //縦長の場合
        if (windowSize.y > windowSize.x)
        {
            //はみ出すため小さくする
            mylocalScale.x = 0.7f;
            mylocalScale.y = 0.8f;

            //縦長に変更
            if (windowSize.y > 1160f)
            {
                mysize.y = 1250f;
            }
        }

        GetComponent<RectTransform>().localScale = mylocalScale;
        GetComponent<RectTransform>().sizeDelta = mysize;
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        Resources.UnloadUnusedAssets();
    }

    public void OnClickClose()
    {
        //RuntimeInitializer.CallInitialize();

        //SceneManager.LoadSceneAsync("Menu");

        //ReLoadAllData();

        //Destroy(base.gameObject);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    UnityEngine.Application.Quit();
#endif
    }

    public void ReLoadAllData()
    {
        GameObject UIManager = GameObject.Find("UIManager");
        if (UIManager != null)
        {
            UIManagerChara uIManagerChara = UIManager.GetComponent<UIManagerChara>();
            if (uIManagerChara != null) { uIManagerChara.ResetData(); }

            UIManagerDress uIManagerDress = UIManager.GetComponent<UIManagerDress>();
            if (uIManagerDress != null) { uIManagerDress.ResetData(); }

            UIManagerMusic uIManagerMusic = UIManager.GetComponent<UIManagerMusic>();
            if (uIManagerMusic != null) { uIManagerMusic.ResetData(); }

            UIManagerPhotoStudioStage uIManagerPhotoStudioStage = UIManager.GetComponent<UIManagerPhotoStudioStage>();
            if (uIManagerPhotoStudioStage != null) { uIManagerPhotoStudioStage.ResetData(); }

            UIStartManager uIStartManager = UIManager.GetComponent<UIStartManager>();
            if (uIStartManager != null) { uIStartManager.ResetData(); }
        }

    }

    public void UpdateDatabase()
    {
        StartCoroutine(UpdateDB());
    }

    /// <summary>
    /// ManifestおよびMasterのデータベースの更新処理
    /// </summary>
    private IEnumerator UpdateDB()
    {
        Text text = base.transform.Find("Content/Text").gameObject.GetComponent<Text>();
        text.text = "DB更新中...";


        MasterDBManager.instance.DeleteMasterDB();
        ManifestManager.instance.DeleteManifest();

        string fileVersion = "";
        int filever = 0;
        //バージョンを確認
        VersionCheckerKiraraAPI cv = new VersionCheckerKiraraAPI();
        yield return cv.Check();

        //バージョンが取得できなかった
        if (cv.version == 0)
        {
            VersionCheckerMatsurihimeAPI matsurihime = new VersionCheckerMatsurihimeAPI();
            yield return matsurihime.Check();
            if (matsurihime.version > 0)
            {
                filever = matsurihime.version;
            }
        }
        else
        {
            filever = cv.version;
        }

        if (filever == 0)
        {
            print("バージョンの取得に失敗しました");
            yield break;
        }
        fileVersion = filever.ToString();

        text.text = text.text + "\n" + "ManifestVersion:" + fileVersion;
        yield return null;

        yield return ManifestManager.instance.DownloadManifest(fileVersion);
        yield return MasterDBManager.instance.DownloadMasterDB();

        text.text = text.text + "\n" + "更新完了";
        yield return null;
        yield return null;
        text.text = text.text + "\n" + "下部ボタンから一旦アプリを終了してください";
        yield return null;

        //データソースをいったん全部破棄
        ViewLauncher.instance.Clear();
        AssetManager.instance.Init();
        ResourcesManager.instance.Destroy();
        Destroy(AssetManager.instance.gameObject);
        Destroy(ViewLauncher.instance.gameObject);
        GameObject UIManager = GameObject.Find("UIManager");
        Destroy(UIManager);

        Resources.UnloadUnusedAssets();

        var close = base.transform.Find("Footer/Close").gameObject;
        close.SetActive(true);

    }
}
