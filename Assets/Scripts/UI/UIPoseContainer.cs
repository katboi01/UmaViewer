using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UIPoseContainer : MonoBehaviour
{
    public PoseData PoseData;

    public TMPro.TMP_InputField
        NameLabel,
        DescriptionLabel;

    public TMPro.TMP_Text
        InfoLabel;

    [SerializeField]
    private Button
        _btnSave,
        _btnLoad,
        _btnDelete;

    public static UIPoseContainer Create(PoseData data)
    {
        var poseContainer = Instantiate(UmaViewerUI.Instance.PoseManager.Pfb_PoseContainer, UmaViewerUI.Instance.PoseManager.SavedPoseList.content);
        poseContainer.Init(data);
        return poseContainer;
    }

    public void Init(PoseData data)
    {
        PoseData = data;
        
        NameLabel.text = data.Name;
        DescriptionLabel.text = data.Description;
        InfoLabel.text = $"Version: {data.ViewerVersion}\nCharacter: {data.Character}\nSave date: {data.Date}";
    }

    public void Save()
    {
        UmaContainerCharacter container = UmaViewerBuilder.Instance.CurrentUMAContainer;

        if (container == null) return;

        var poseData = new PoseData();
        poseData.Name           = NameLabel.text;
        poseData.Date           = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        poseData.Character      = container.name;
        poseData.Description    = DescriptionLabel.text;
        poseData.ViewerVersion  = Application.version;
        poseData.Bones          = container.SaveBones();

        string fullpath = $"{Application.dataPath}/../Poses/{poseData.Name}.umaPose";
        fullpath = Path.GetFullPath(fullpath);

        try
        {
            Directory.CreateDirectory(Application.dataPath + "/../Poses");

            File.WriteAllText(fullpath, JsonConvert.SerializeObject(poseData));

            UmaViewerUI.Instance.ShowMessage($"Saved pose: {fullpath}", UIMessageType.Success);

            //Only replace the data if everything succeeded
            Init(poseData);
        }
        catch (Exception e)
        {
            UmaViewerUI.Instance.ShowMessage(e.Message, UIMessageType.Error);
        }
    }

    public void Load()
    {
        UmaViewerUI.Instance.PoseManager.SetPoseMode(true);

        UmaContainerCharacter container = UmaViewerBuilder.Instance.CurrentUMAContainer;

        if (container == null) return;

        container.LoadBones(PoseData);
    }

    public void Delete()
    {
        Destroy(gameObject);

        string fullpath = $"{Application.dataPath}/../Poses/{PoseData.Name}.umaPose";
        fullpath = Path.GetFullPath(fullpath);

        if(File.Exists(fullpath))
        {
            File.Delete(fullpath);
        }

        UmaViewerUI.Instance.ShowMessage($"Deleted pose: {fullpath}", UIMessageType.Success);
    }
}
