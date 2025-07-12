using Newtonsoft.Json;
using RootMotion;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PoseManager : MonoBehaviour
{
    public ScrollRect SavedPoseList;

    public UIPoseContainer Pfb_PoseContainer;

    public TMPro.TMP_Text[] TooltipLabels;

    [NonSerialized]
    public bool PoseModeOn = false;

    [SerializeField]
    [Header("Toggles")]
    private Toggle
        _loadRoot;

    [SerializeField]
    private Toggle
        _loadPosition,
        _loadRotation,
        _loadScale,
        _loadMorphs,
        _loadFaceBones,
        _loadPhysics;

    public List<UIPoseContainer> SavedPoses = new List<UIPoseContainer>();

    public GameObject HandleCanvas;
    public GameObject HelpPanel;

    public FullBodyBipedIK PoseIK;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            HelpPanel.SetActive(!HelpPanel.activeSelf);
        }
    }

    public static PoseLoadOptions GetLoadOptions()
    {
        var pm = UmaViewerUI.Instance.PoseManager;
        return new PoseLoadOptions()
        {
            Root        = pm._loadRoot.isOn,
            Position    = pm._loadPosition.isOn,
            Rotation    = pm._loadRotation.isOn,
            Scale       = pm._loadScale.isOn,
            Morphs      = pm._loadMorphs.isOn,
            FaceBones   = pm._loadFaceBones.isOn,
            Physics     = pm._loadPhysics.isOn
        };
    }

    public void LoadLocalPoseFiles()
    {
        Directory.CreateDirectory(Application.dataPath + "/../Poses");
        foreach (var file in Directory.GetFiles($"{Application.dataPath}/../Poses/"))
        {
            Debug.Log(file);
            try
            {
                var data = JsonConvert.DeserializeObject<PoseData>(File.ReadAllText(file));
                if (data != null)
                {
                    UIPoseContainer.Create(data);
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                Debug.LogError(file + " is not a valid pose file.");
            }
        }
    }

    public static void SetPoseModeStatic(bool value)
    {
        UmaViewerUI.Instance.PoseManager.SetPoseMode(value);
    }

    /// <summary> Used on UI buttons </summary>
    public void SetPoseMode(bool value)
    {
        if (value == PoseModeOn) return;

        if (value)
        {
            EnablePoseMode();
        }
        else
        {
            DisablePoseMode();
        }
    }

    public static void SetTooltip(int panelId, string text)
    {
        UmaViewerUI.Instance.PoseManager.TooltipLabels[panelId].text = text;
    }

    public void EnablePoseMode()
    {
        if (PoseModeOn) return;

        var modelSettings = UmaViewerUI.Instance.ModelSettings;
        var builder = UmaViewerBuilder.Instance;
        modelSettings.SetEyeTrackingEnable(false);
        modelSettings.SetDynamicBoneEnable(false);
        HandleCanvas.SetActive(true);

        if (builder.CurrentUMAContainer && builder.CurrentUMAContainer.UmaAnimator)
        {
            PoseIK = builder.CurrentUMAContainer.CreatePoseIK();
            builder.CurrentUMAContainer.UmaAnimator.enabled = false;
        }

        PoseModeOn = true;
    }

    public void DisablePoseMode()
    {
        if (!PoseModeOn) return;

        UIPoseContainer.CreateBackupFromScene();

        var modelSettings = UmaViewerUI.Instance.ModelSettings;
        var builder = UmaViewerBuilder.Instance;
        modelSettings.SetEyeTrackingEnable(true);
        modelSettings.SetDynamicBoneEnable(true);
        HandleCanvas.SetActive(false);

        if (builder.CurrentUMAContainer != null && builder.CurrentUMAContainer.UmaAnimator != null)
        {
            builder.CurrentUMAContainer.UmaAnimator.enabled = true;
        }

        if (PoseIK)
        {
            Destroy(PoseIK);
        }

        PoseModeOn = false;
    }
}