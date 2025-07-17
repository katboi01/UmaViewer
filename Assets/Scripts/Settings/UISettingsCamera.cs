using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsCamera : MonoBehaviour
{
    public TMPro.TMP_Dropdown AAModeDropdown;
    public TMPro.TMP_Dropdown CameraModeDropdown;

    [Header("Free Camera")]
    public GameObject FreeCameraSettingsTab;
    public Slider FOVFree;
    public Slider CameraRotationFree;
    public Slider MovementSpeedFree;
    public Slider RotationSpeedSlider;

    [Header("Orbit Camera")]
    public GameObject OrbitCameraSettingsTab;
    public Slider FOVOrbit;
    public Slider CameraDistance;
    public Slider CameraHeightSlider;
    public Slider CameraRotationOrbit;
    public Slider TargetHeightSlider;
    public Slider ZoomSpeedSlider;
    public Slider MovementSpeedOrbit;

    [Space]
    public Toggle UseAnimationCamera;

    public int CameraMode
    {
        get { return CameraModeDropdown.value; }
        set { CameraModeDropdown.value = value; UpdateSettingsPanel(); }
    }

    public float FOV
    {
        get { return CameraMode == 0 ? FOVOrbit.value : FOVFree.value; }
        set { 
            if (CameraMode == 0)
            {
                FOVOrbit.value = value;
            }
            else
            {
                FOVFree.value = value;
            }
        }
    }

    public float CameraRotation
    {
        get { return CameraMode == 0 ? CameraRotationOrbit.value : CameraRotationFree.value; }
        set
        {
            if (CameraMode == 0)
            {
                CameraRotationOrbit.value = value;
            }
            else
            {
                CameraRotationFree.value = value;
            }
        }
    }

    public float ZoomSpeed
    {
        get { return ZoomSpeedSlider.value; }
        set { ZoomSpeedSlider.value = value; }
    }

    public float MovementSpeed
    {
        get { return CameraMode == 0 ? MovementSpeedOrbit.value : MovementSpeedFree.value; }
        set
        {
            if (CameraMode == 0)
            {
                MovementSpeedOrbit.value = value;
            }
            else
            {
                MovementSpeedFree.value = value;
            }
        }
    }

    public float RotationSpeed
    {
        get { return RotationSpeedSlider.value; }
        set { RotationSpeedSlider.value = value; }
    }

    public float CameraHeight
    {
        get { return CameraHeightSlider.value; }
        set { CameraHeightSlider.value = value; }
    }

    public float TargetHeight
    {
        get { return TargetHeightSlider.value; }
        set { TargetHeightSlider.value = value; }
    }

    public void UpdateSettingsPanel()
    {
        if (CameraMode == 0)
        {
            OrbitCameraSettingsTab.SetActive(true);
            FreeCameraSettingsTab.SetActive(false);
        }
        else
        {
            FreeCameraSettingsTab.SetActive(true);
            OrbitCameraSettingsTab.SetActive(false);
        }
    }

    /// <summary> Converts values 1-4 to valid AA values </summary>
    public void ChangeAntiAliasing(int value)
    {
        int[] aaValues = { 0, 2, 4, 8 };

        QualitySettings.antiAliasing = aaValues[value];

        if (Config.Instance.AntiAliasing != value)
        {
            Config.Instance.AntiAliasing = value;
            Config.Instance.UpdateConfig(false);
        }
    }
}
