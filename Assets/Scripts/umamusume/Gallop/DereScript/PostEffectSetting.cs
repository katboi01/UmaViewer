using UnityEngine;

public static class PostEffectSetting
{
    public static bool config_PostEffect = false;

    public static bool config_Globalfog = false;

    public static bool config_SunShafts = false;

    public static bool config_TiltShift = false;

    public static bool config_IndirectLightShafts = false;

    public static bool config_A2U = false;

    public static bool config_ColorCorrection = false;

    public static bool config_Bloom = false;

    public static bool config_Dof = false;

    public static bool config_Diffusion = false;

    public static bool config_screenOverlay = false;

    public static bool config_Depth = false;

    public static bool captureScreenShot = false;

    public static bool config_AntiAliasing = false;

    public static RenderTexture ScreenShot = null;

    public static void LoadSave()
    {
        //デバッグが有効かどうか確認
        int save = SaveManager.GetInt("PostEffectDebug", 1);
        if (save == 1)
        {
            //デバッグが有効ならばセーブから読み込み
            PostEffectSetting.LoadSaveEnable();
        }
        else
        {
            //デバッグが無効の場合はすべて有効
            PostEffectSetting.config_PostEffect = true;
            PostEffectSetting.config_Globalfog = true;
            PostEffectSetting.config_SunShafts = true;
            PostEffectSetting.config_TiltShift = true;
            PostEffectSetting.config_IndirectLightShafts = true;
            PostEffectSetting.config_A2U = true;
            PostEffectSetting.config_ColorCorrection = true;
            PostEffectSetting.config_Bloom = true;
            PostEffectSetting.config_Dof = true;
            PostEffectSetting.config_Diffusion = true;
            PostEffectSetting.config_screenOverlay = true;
            PostEffectSetting.config_AntiAliasing = true;
            PostEffectSetting.config_Depth = false;
        }
    }

    /// <summary>
    /// セーブデータを読み込む
    /// </summary>
    private static void LoadSaveEnable()
    {
        int save = SaveManager.GetInt("config_PostEffect", 1);
        if (save == 1)
        {
            PostEffectSetting.config_PostEffect = true;
        }

        save = SaveManager.GetInt("config_GlobalFog", 1);
        if (save == 1)
        {
            PostEffectSetting.config_Globalfog = true;
        }

        save = SaveManager.GetInt("config_SunShafts", 1);
        if (save == 1)
        {
            PostEffectSetting.config_SunShafts = true;
        }

        save = SaveManager.GetInt("config_TiltShift", 1);
        if (save == 1)
        {
            PostEffectSetting.config_TiltShift = true;
        }

        save = SaveManager.GetInt("config_IndirectLightShafts", 1);
        if (save == 1)
        {
            PostEffectSetting.config_IndirectLightShafts = true;
        }

        save = SaveManager.GetInt("config_A2U", 1);
        if (save == 1)
        {
            PostEffectSetting.config_A2U = true;
        }

        save = SaveManager.GetInt("config_ColorCorrection", 1);
        if (save == 1)
        {
            PostEffectSetting.config_ColorCorrection = true;
        }

        save = SaveManager.GetInt("config_Bloom", 1);
        if (save == 1)
        {
            PostEffectSetting.config_Bloom = true;
        }

        save = SaveManager.GetInt("config_Dof", 1);
        if (save == 1)
        {
            PostEffectSetting.config_Dof = true;
        }

        save = SaveManager.GetInt("config_Diffusion", 1);
        if (save == 1)
        {
            PostEffectSetting.config_Diffusion = true;
        }

        save = SaveManager.GetInt("config_ScreenOverlay", 1);
        if (save == 1)
        {
            PostEffectSetting.config_screenOverlay = true;
        }

        save = SaveManager.GetInt("config_AntiAliasing", 1);
        if (save == 1)
        {
            PostEffectSetting.config_AntiAliasing = true;
        }

        //デプスはセーブには保存されない
        PostEffectSetting.config_Depth = false;
    }

    /// <summary>
    /// 現在の設定を保存する
    /// 設定画面のみで使用
    /// </summary>
    public static void Save()
    {
        SaveManager.SetInt("config_PostEffect", PostEffectSetting.config_PostEffect ? 1 : 0);
        SaveManager.SetInt("config_Globalfog", PostEffectSetting.config_Globalfog ? 1 : 0);
        SaveManager.SetInt("config_SunShafts", PostEffectSetting.config_SunShafts ? 1 : 0);
        SaveManager.SetInt("config_TiltShift", PostEffectSetting.config_TiltShift ? 1 : 0);
        SaveManager.SetInt("config_IndirectLightShafts", PostEffectSetting.config_IndirectLightShafts ? 1 : 0);
        SaveManager.SetInt("config_A2U", PostEffectSetting.config_A2U ? 1 : 0);
        SaveManager.SetInt("config_ColorCorrection", PostEffectSetting.config_ColorCorrection ? 1 : 0);
        SaveManager.SetInt("config_Bloom", PostEffectSetting.config_Bloom ? 1 : 0);
        SaveManager.SetInt("config_Dof", PostEffectSetting.config_Dof ? 1 : 0);
        SaveManager.SetInt("config_Diffusion", PostEffectSetting.config_Diffusion ? 1 : 0);
        SaveManager.SetInt("config_AntiAliasing", PostEffectSetting.config_AntiAliasing ? 1 : 0);
        SaveManager.SetInt("config_screenOverlay", PostEffectSetting.config_screenOverlay ? 1 : 0);

        SaveManager.Save();
    }
}