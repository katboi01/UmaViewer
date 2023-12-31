using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SerializableBone;

public class HandleManager : MonoBehaviour
{
    public GameObject Pfb_HandleDisplay;
    public UIPopupPanel Pfb_Popup;
    public Button Pfb_PopupButton;

    public Material LineRendererMaterial;

    public List<BoneTags> EnabledHandles = new List<BoneTags>() { BoneTags.Humanoid };
    public bool EnabledLines = true;

    public static bool InteractionInProgress;

    private List<UIHandle> AllHandles = new List<UIHandle>();

    public static System.Action<RuntimeGizmoUndoData> RegisterRuntimeGizmoUndoAction;
    public static System.Action<List<RuntimeGizmoUndoData>> RegisterRuntimeGizmoUndoActions;

    public struct RuntimeGizmoUndoData
    {
        public Transform NewPosition;
        public SerializableTransform OldPosition;
    }

    private void Update()
    {
        var camera = Camera.main;
        var poseModeOn = UmaViewerUI.Instance.PoseManager.PoseModeOn;

        foreach(var handle in AllHandles)
        {
            handle.ForceDisplayOff(!poseModeOn);
            handle.UpdateManual(camera, EnabledLines);

            if (handle.Popup.gameObject.activeInHierarchy)
            {
                handle.Popup.UpdateManual(camera);
            }
        }
    }

    public static void RegisterHandle(UIHandle handle)
    {
        var hm = UmaViewerUI.Instance.HandleManager;
        
        if (hm.AllHandles.Contains(handle))
        {
            return;
        }

        hm.AllHandles.Add(handle);
    }

    public static void UnregisterHandle(UIHandle handle)
    {
        var hm = UmaViewerUI.Instance.HandleManager;

        if (!hm.AllHandles.Contains(handle))
        {
            return;
        }

        hm.AllHandles.Remove(handle);
    }

    public static void CloseAllPopups()
    {
        var hm = UmaViewerUI.Instance.HandleManager;

        foreach(var handle in hm.AllHandles)
        {
            if (handle.Popup.gameObject.activeInHierarchy)
            {
                handle.TogglePopup();
            }
        }
    }

    /// <summary> Used on UI buttons </summary>
    public void ToggleBonesVisible(string tag)
    {
        var enumTag = (BoneTags)System.Enum.Parse(typeof(BoneTags), tag);
        if (EnabledHandles.Contains(enumTag))
        {
            EnabledHandles.Remove(enumTag);
        }
        else
        {
            EnabledHandles.Add(enumTag);
        }
    }

    /// <summary> Used on UI buttons </summary>
    public void ToggleLinesVisible(bool value)
    {
        EnabledLines = value;
    }
}