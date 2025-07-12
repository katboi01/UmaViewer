using SFB;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsModel : MonoBehaviour
{
    static UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    [SerializeField] private Toggle _lockCharacter;
    [SerializeField] private Toggle _openWithTPose;
    [SerializeField] private Toggle _enablePhysics;
    [SerializeField] private Toggle _lookAtCamera;
    [SerializeField] private Toggle _faceOverride;
    [SerializeField] private Slider _outlineWidthSlider;

    public ScrollRect MaterialsList;

    private bool
        _isHeadFix,
        _isTPose,
        _dynamicBoneEnable = true,
        _enableEyeTracking = true,
        _enableFaceOverride = true;

    private float _outlineWidth;

    public bool IsHeadFix
    {
        get { return _isHeadFix; }
        set { _lockCharacter.isOn = value; }
    }

    public bool IsTPose
    {
        get { return _isTPose; }
        set { _openWithTPose.isOn = value; }
    }

    public bool DynamicBoneEnable
    {
        get { return _dynamicBoneEnable; }
        set { _enablePhysics.isOn = value; }
    }

    public bool EnableEyeTracking
    {
        get { return _enableEyeTracking; }
        set { _lookAtCamera.isOn = value; }
    }

    public bool EnableFaceOverride
    {
        get { return _enableFaceOverride; }
        set { _faceOverride.isOn = value; }
    }

    public float OutlineWidth
    {
        get { return _outlineWidth; }
        set { _outlineWidthSlider.value = value; }
    }

    public void SetHeadFix(bool value)
    {
        _isHeadFix = value;
    }

    public void SetTPose(bool value)
    {
        _isTPose = value;
    }

    public void SetDynamicBoneEnable(bool isOn)
    {
        _dynamicBoneEnable = isOn;
        Builder.CurrentUMAContainer?.SetDynamicBoneEnable(isOn);
    }

    public void SetEyeTrackingEnable(bool isOn)
    {
        _enableEyeTracking = isOn;
        Builder.CurrentUMAContainer?.SetEyeTracking(isOn);
    }

    public void SetFaceOverrideEnable(bool isOn)
    {
        _enableFaceOverride = isOn;
        Builder.CurrentUMAContainer?.SetFaceOverrideData(isOn);
    }

    public void ChangeOutlineWidth(float val)
    {
        _outlineWidth = val;
        Shader.SetGlobalFloat("_GlobalOutlineWidth", val);
    }

    public void ExportModel()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        var container = Builder.CurrentUMAContainer;
        if (container)
        {
            var entry = container.CharaEntry;
            var path = StandaloneFileBrowser.SaveFilePanel("Save PMX File", Config.Instance.MainPath, $"{entry.Id}_{entry.GetName()}", "pmx");
            if (!string.IsNullOrEmpty(path))
            {
                ModelExporter.ExportModel(container, path);
            }
        }
#endif
    }
}
