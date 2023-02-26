using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiveDebug : MonoBehaviour
{
    private bool debugEnable = false;

    private GameObject rootObject = null;

    private Toggle postEffect = null;
    private Toggle globalFog = null;
    private Toggle sunShafts = null;
    private Toggle tiltShift = null;
    private Toggle indirectLightShafts = null;
    private Toggle a2U = null;
    private Toggle colorCorrection = null;
    private Toggle bloom = null;
    private Toggle dof = null;
    private Toggle diffusion = null;
    private Toggle screenOverlay = null;
    private Toggle antiAliasing = null;
    private Toggle depth = null;

    private bool isStart = false;

    private void Awake()
    {
        int tmp = SaveManager.GetInt("PostEffectDebug");
        int tmp2 = SaveManager.GetInt("PostEffectDebugPanel");

        rootObject = base.transform.Find("BackGround").gameObject;

        if (tmp == 1 && tmp2 == 1)
        {
            debugEnable = true;
        }
        else
        {
            rootObject.SetActive(false);
            debugEnable = false;
            return;
        }

        postEffect = base.transform.Find("BackGround/PostEffect_Chk").GetComponent<Toggle>();
        globalFog = base.transform.Find("BackGround/GlobalFog_Chk").GetComponent<Toggle>();
        sunShafts = base.transform.Find("BackGround/SunShafts_Chk").GetComponent<Toggle>();
        tiltShift = base.transform.Find("BackGround/TiltShift_Chk").GetComponent<Toggle>();
        indirectLightShafts = base.transform.Find("BackGround/IndirectLightShafts_Chk").GetComponent<Toggle>();
        a2U = base.transform.Find("BackGround/A2U_Chk").GetComponent<Toggle>();
        colorCorrection = base.transform.Find("BackGround/ColorCorrection_Chk").GetComponent<Toggle>();
        bloom = base.transform.Find("BackGround/Bloom_Chk").GetComponent<Toggle>();
        dof = base.transform.Find("BackGround/Dof_Chk").GetComponent<Toggle>();
        diffusion = base.transform.Find("BackGround/Diffusion_Chk").GetComponent<Toggle>();
        screenOverlay = base.transform.Find("BackGround/ScreenOverlay_Chk").GetComponent<Toggle>();
        antiAliasing = base.transform.Find("BackGround/AntiAliasing_Chk").GetComponent<Toggle>();
        depth = base.transform.Find("BackGround/Depth_Chk").GetComponent<Toggle>();

        postEffect.isOn = PostEffectSetting.config_PostEffect;
        globalFog.isOn = PostEffectSetting.config_Globalfog;
        sunShafts.isOn = PostEffectSetting.config_SunShafts;
        tiltShift.isOn = PostEffectSetting.config_TiltShift;
        indirectLightShafts.isOn = PostEffectSetting.config_IndirectLightShafts;
        a2U.isOn = PostEffectSetting.config_A2U;
        colorCorrection.isOn = PostEffectSetting.config_ColorCorrection;
        bloom.isOn = PostEffectSetting.config_Bloom;
        dof.isOn = PostEffectSetting.config_Dof;
        diffusion.isOn = PostEffectSetting.config_Diffusion;
        screenOverlay.isOn = PostEffectSetting.config_screenOverlay;
        antiAliasing.isOn = PostEffectSetting.config_AntiAliasing;
        depth.isOn = PostEffectSetting.config_Depth;

        rootObject.SetActive(false);
        isStart = true;

    }
    public void ChangeVisible()
    {
        if (debugEnable)
        {
            if(rootObject != null)
            {
                //ひっくり返す
                rootObject.SetActive(!rootObject.activeSelf);
            }
        }
    }

    public void OnChangePostEffectToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_PostEffect = postEffect.isOn;
        }
    }

    public void OnChangeGlobalFogToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_Globalfog = globalFog.isOn;
        }
    }
    public void OnChangeSunShaftsToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_SunShafts = sunShafts.isOn;
        }
    }
    public void OnChangeTiltShiftToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_TiltShift = tiltShift.isOn;
        }
    }
    public void OnChangeIndirectLightShaftsToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_IndirectLightShafts = indirectLightShafts.isOn;
        }
    }
    public void OnChangeA2UToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_A2U = a2U.isOn;
        }
    }
    public void OnChangeColorCorrectionToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_ColorCorrection = colorCorrection.isOn;
        }
    }
    public void OnChangeBloomToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_Bloom = bloom.isOn;
            //bloomをオフにした場合、DOFもオフになる
            if (!bloom.isOn)
            {
                PostEffectSetting.config_Dof = dof.isOn = false;
            }
        }
    }
    public void OnChangeDofToggle()
    {
        if (isStart)
        {
            //bloomがオンのときだけ切り替えができる
            if (bloom.isOn)
            {
                PostEffectSetting.config_Dof = dof.isOn;
            }
            else
            {
                dof.isOn = false;
            }
        }
    }

    public void OnChangeDiffusionToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_Diffusion = diffusion.isOn;
            if (!diffusion.isOn)
            {
                PostEffectSetting.config_Diffusion = diffusion.isOn = false;
            }
        }
    }

    public void OnChangeScreenOverlayToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_screenOverlay = screenOverlay.isOn;
        }
    }

    public void OnChangeAntiAliasingToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_AntiAliasing = antiAliasing.isOn;
        }
    }

    public void OnChangeDepthToggle()
    {
        if (isStart)
        {
            PostEffectSetting.config_Depth = depth.isOn;
        }
    }
}