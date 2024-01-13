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

    public List<BoneTags> EnabledHandles { get => IKMode ? IKHandles : enabledHandles; }
    public bool EnabledLines = true;
    public bool IKMode = false;

    public static bool InteractionInProgress;

    private List<UIHandle> AllHandles = new List<UIHandle>();

    public static System.Action<RuntimeGizmoUndoData> RegisterRuntimeGizmoUndoAction;
    public static System.Action<List<RuntimeGizmoUndoData>> RegisterRuntimeGizmoUndoActions;

    private List<BoneTags> enabledHandles = new List<BoneTags>() { BoneTags.Humanoid };
    private List<BoneTags> IKHandles = new List<BoneTags>() { BoneTags.IK };

    public struct RuntimeGizmoUndoData
    {
        public Transform NewPosition;
        public SerializableTransform OldPosition;
    }

    private void Update()
    {
        var camera = Camera.main;
        var poseModeOn = UmaViewerUI.Instance.PoseManager.PoseModeOn;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            IKMode = !IKMode;
            if (IKMode)
            {
                var ik = UmaViewerUI.Instance.PoseManager.PoseIK;
                if (ik)
                {
                    foreach (var effector in ik.solver.effectors)
                    {
                        if (effector.target)
                        {
                            effector.target.localPosition = Vector3.zero;
                            effector.target.localRotation = Quaternion.identity;
                        }
                    }
                    ik.solver.StoreDefaultLocalState();
                    ik.enabled = true;
                }
            }
            else
            {
                var ik = UmaViewerUI.Instance.PoseManager.PoseIK;
                if (ik)
                {
                    ik.enabled = false;
                }
            }
        };

        foreach (var handle in AllHandles)
        {
            handle.ForceDisplayOff(!poseModeOn);
            handle.UpdateManual(camera, EnabledLines && !IKMode);

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
        if (enabledHandles.Contains(enumTag))
        {
            enabledHandles.Remove(enumTag);
        }
        else
        {
            enabledHandles.Add(enumTag);
        }
    }

    /// <summary> Used on UI buttons </summary>
    public void ToggleLinesVisible(bool value)
    {
        EnabledLines = value;
    }
}