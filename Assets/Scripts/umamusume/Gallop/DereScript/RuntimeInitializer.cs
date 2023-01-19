using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeInitializer : MonoBehaviour
{
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
#if UNITY_EDITOR
#elif UNITY_STANDALONE
        int isfull = Screen.fullScreen ? 1 : 0;
        SaveManager.SetInt("ScreenMode", isfull);
        if(isfull == 0)
        {
            SaveManager.SetInt("ScreenWidth", Screen.width);
            SaveManager.SetInt("ScreenHeight", Screen.height);
        }
        SaveManager.Save();
#endif
    }

    /// <summary>
    /// 明示的に初期化する場合
    /// </summary>
    public static void CallInitialize()
    {
        InitializeBeforeSceneLoad();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeSceneLoad()
    {
        SaveManager.GetString("先にセーブデータを作成します");

#if UNITY_EDITOR
#elif UNITY_STANDALONE
        //画面の解像度を設定する
        int isFullScreen = SaveManager.GetInt("ScreenMode", 1);
        if(isFullScreen == 1)
        {
            int width = Screen.currentResolution.width;
            int height = Screen.currentResolution.height;

            Screen.SetResolution(width, height, true);

            SaveManager.SetInt("ScreenWidth", width);
            SaveManager.SetInt("ScreenHeight", height);
        }
        else
        {
            int width = SaveManager.GetInt("ScreenWidth", 1280);
            int height = SaveManager.GetInt("ScreenHeight", 720);

            Screen.SetResolution(width, height, false);

            SaveManager.SetInt("ScreenWidth", width);
            SaveManager.SetInt("ScreenHeight", height);
        }
        
        SaveManager.SetInt("ScreenMode", isFullScreen);
        SaveManager.Save();
#endif

        //フレームレートの設定を行う
        int DontSync = SaveManager.GetInt("vSyncSetting", 1); //デフォルトは垂直同期無し
        if (DontSync == 1)
        {
            QualitySettings.vSyncCount = 0;
            int FrameRate = SaveManager.GetInt("FrameRate", 1); //デフォルト60
            switch (FrameRate)
            {
                case 0:
                    Application.targetFrameRate = 30;
                    break;
                case 1:
                    //Every VBlank(60fps)
                    Application.targetFrameRate = 60;
                    break;
                case 2:
                    //Every VBlank(120fps)
                    Application.targetFrameRate = 120;
                    break;
            }
            SaveManager.SetInt("FrameRate", FrameRate);
        }
        else
        {
            QualitySettings.vSyncCount = 1;
            int FrameRate = SaveManager.GetInt("FrameRate", 1); //デフォルト60
            switch (FrameRate)
            {
                case 0:
                    //Every Second VBlank(60fpsの半分)
                    QualitySettings.vSyncCount = 2;
                    Application.targetFrameRate = 60;
                    break;
                case 1:
                    //Every VBlank(60fps)
                    Application.targetFrameRate = 60;
                    break;
                case 2:
                    //Every VBlank(120fps)
                    Application.targetFrameRate = 120;
                    break;
            }
            SaveManager.SetInt("FrameRate", FrameRate);
        }
        SaveManager.SetInt("vSyncSetting", DontSync);
        SaveManager.Save();


        // ScriptableObjectテーブルから情報を取得し設定する。
        var table = Resources.Load("InitializerTable") as InitializerTable;
        if (table != null)
        {
            GameObject obj1 = GameObject.Find("AssetManager");
            if (obj1 == null)
            {
                GameObject assetManager = Instantiate(table.AssetManager);
                assetManager.name = assetManager.name.Replace("(Clone)", "");
                GameObject.DontDestroyOnLoad(assetManager);
            }

            GameObject obj2 = GameObject.Find("ViewLauncher");
            if (obj2 == null)
            {
                GameObject viewLauncher = Instantiate(table.ViewLauncher);
                viewLauncher.name = viewLauncher.name.Replace("(Clone)", "");
                GameObject.DontDestroyOnLoad(viewLauncher);
            }

            GameObject obj3 = GameObject.Find("UIManager");
            if (obj3 == null)
            {
                GameObject uiManager = Instantiate(table.UIManager);
                uiManager.name = uiManager.name.Replace("(Clone)", "");
                GameObject.DontDestroyOnLoad(uiManager);
            }
            
            GameObject obj4 = GameObject.Find("ResourceManager");
            if (obj4 == null)
            {
                GameObject resourceManager = Instantiate(table.ResourcesManager);
                resourceManager.name = resourceManager.name.Replace("(Clone)", "");
                GameObject.DontDestroyOnLoad(resourceManager);
            }
        }
    }
}
