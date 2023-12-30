using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
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

    public static void CreateBackupFromScene()
    {
        UmaContainerCharacter container = UmaViewerBuilder.Instance.CurrentUMAContainer;

        if (container == null) return;

        var poseData = new PoseData();
        poseData.Name = "AutoBackup";
        poseData.Date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        poseData.Character = container.name;
        poseData.Description = "Last created pose";
        poseData.ViewerVersion = Application.version;
        poseData.Bones = container.SaveBones();
        poseData.Morphs = container.SaveMorphs(true);

        string fullpath = $"{Application.dataPath}/../Poses/{poseData.Name}.umaPose";
        fullpath = Path.GetFullPath(fullpath);

        try
        {
            Directory.CreateDirectory(Application.dataPath + "/../Poses");

            File.WriteAllText(fullpath, JsonConvert.SerializeObject(poseData));

            //UmaViewerUI.Instance.ShowMessage($"Saved pose: {fullpath}", UIMessageType.Success);

            var poseContainers = UmaViewerUI.Instance.PoseManager.SavedPoseList.content.GetComponentsInChildren<UIPoseContainer>();
            var poseContainer = poseContainers.FirstOrDefault(c => c.NameLabel.text == "AutoBackup");

            if (poseContainer != null)
            {
                poseContainer.Init(poseData);
            }
            else
            {
                UIPoseContainer.Create(poseData);
            }
        }
        catch (Exception e)
        {
            UmaViewerUI.Instance.ShowMessage(e.Message, UIMessageType.Error);
        }
    }

    public void Init(PoseData data)
    {
        PoseData = data;
        
        NameLabel.text = data.Name;
        DescriptionLabel.text = data.Description;
        InfoLabel.text = $"Version: {data.ViewerVersion}\nCharacter: {data.Character}\nSave date: {data.Date}";
    }

    public void SaveNew()
    {
        UmaContainerCharacter container = UmaViewerBuilder.Instance.CurrentUMAContainer;

        if (container == null) return;

        var poseData = new PoseData()
        {
            Name           = NameLabel.text,
            Date           = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
            Character      = container.name,
            Description    = DescriptionLabel.text,
            ViewerVersion  = Application.version,
            Bones          = container.SaveBones(),
            Morphs         = container.SaveMorphs(true)
        };

        var duplicateContainer = CheckIfContainerExists(poseData.Name);
        if (duplicateContainer != null && duplicateContainer != this)
        {
            UmaViewerUI.Instance.ShowMessage($"Another save exists with name {poseData.Name}", UIMessageType.Error);
            return;
        }

        string fullpath = $"{Application.dataPath}/../Poses/{poseData.Name}.umaPose";
        fullpath = Path.GetFullPath(fullpath);

        try
        {
            Directory.CreateDirectory(Application.dataPath + "/../Poses");

            File.WriteAllText(fullpath, JsonConvert.SerializeObject(poseData));

            UmaViewerUI.Instance.ShowMessage($"Saved pose: {fullpath}", UIMessageType.Success);

            UIPoseContainer.Create(poseData);
        }
        catch (Exception e)
        {
            UmaViewerUI.Instance.ShowMessage(e.Message, UIMessageType.Error);
        }
    }

    public void Save()
    {
        UmaContainerCharacter container = UmaViewerBuilder.Instance.CurrentUMAContainer;

        if (container == null) return;

        var poseData = new PoseData()
        {
            Name           = NameLabel.text,
            Date           = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
            Character      = container.name,
            Description    = DescriptionLabel.text,
            ViewerVersion  = Application.version,
            Bones          = container.SaveBones(),
            Morphs         = container.SaveMorphs(true)
        };

        var duplicateContainer = CheckIfContainerExists(poseData.Name);
        if (duplicateContainer != null && duplicateContainer != this)
        {
            UmaViewerUI.Instance.ShowMessage($"Another save exists with name {poseData.Name}", UIMessageType.Error);
            return;
        }

        string fullpath = $"{Application.dataPath}/../Poses/{poseData.Name}.umaPose";
        fullpath = Path.GetFullPath(fullpath);

        try
        {
            Directory.CreateDirectory(Application.dataPath + "/../Poses");

            File.WriteAllText(fullpath, JsonConvert.SerializeObject(poseData));

            UmaViewerUI.Instance.ShowMessage($"Saved pose: {fullpath}", UIMessageType.Success);

            if(poseData.Name != PoseData.Name)
            {
                //Create new container with new data
                UIPoseContainer.Create(poseData);
                //Reset back to old data
                Init(PoseData);
            }
            else
            {
                //Only replace the data if everything succeeded
                Init(poseData);
            }
        }
        catch (Exception e)
        {
            UmaViewerUI.Instance.ShowMessage(e.Message, UIMessageType.Error);
        }
    }

    public void Load()
    {
        PoseManager.SetPoseModeStatic(true);

        UmaContainerCharacter container = UmaViewerBuilder.Instance.CurrentUMAContainer;

        if (container == null) return;

        var loadOptions = PoseManager.GetLoadOptions();

        container.LoadBones(PoseData, loadOptions);

        if (loadOptions.Morphs)
        {
            container.LoadMorphs(PoseData);
        }
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

    private UIPoseContainer CheckIfContainerExists(string poseName)
    {
        var poseContainers = UmaViewerUI.Instance.PoseManager.SavedPoseList.content.GetComponentsInChildren<UIPoseContainer>();
        var poseContainer = poseContainers.FirstOrDefault(c => c.NameLabel.text == poseName);

        return poseContainer;
    }
}
