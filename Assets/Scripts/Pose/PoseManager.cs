using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PoseManager : MonoBehaviour
{
    public ScrollRect SavedPoseList;

    public UIPoseContainer Pfb_PoseContainer;

    private bool _isPoseMode = false;

    public void LoadPoses()
    {
        foreach(var file in Directory.GetFiles($"{Application.dataPath}/../Poses/"))
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

    public void SetPoseMode(bool value)
    {
        if (value == _isPoseMode) return;
        if (UmaViewerBuilder.Instance.CurrentUMAContainer == null) return;

        if (value)
        {
            _isPoseMode = true;
            UmaViewerBuilder.Instance.CurrentUMAContainer.UmaAnimator.enabled = false;
        }
        else
        {
            _isPoseMode = false;
            UmaViewerBuilder.Instance.CurrentUMAContainer.UmaAnimator.enabled = true;
        }
    }
}