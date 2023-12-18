using UnityEngine;
using UnityEngine.UI;

public class UIHandle : MonoBehaviour
{
    /// <summary> Object that "owns" the handle </summary>
    public GameObject Owner;

    /// <summary> Object that the handle refers to. Cast in inheriting class </summary>
    public Transform Target;

    /// <summary> Display object on the UI </summary>
    public GameObject Handle;

    /// <summary> Collider for selecting the handle on UI </summary>
    public SphereCollider Collider;

    /// <summary> Object that appears when right-clicking the handle </summary>
    public UIPopupPanel Popup;

    public LineRenderer LineRenderer;

    protected SerializableTransform _defaultTransform;
    protected float _baseScale = 1;
    protected bool _forceDisplayOff = false;

    public virtual UIHandle Init(GameObject owner, Transform target)
    {
        Target = target;
        return Init(owner);
    }

    public virtual UIHandle Init(GameObject owner)
    {
        HandleManager.RegisterHandle(this);

        gameObject.layer = LayerMask.NameToLayer("UIHandle");
        Target = Target == null? owner.transform : Target;
        Owner = owner;

        Handle = Instantiate(UmaViewerUI.Instance.HandleManager.Pfb_HandleDisplay, UmaViewerUI.Instance.HandlesPanel);
        Handle.transform.SetAsFirstSibling();

        Popup = Instantiate(UmaViewerUI.Instance.HandleManager.Pfb_Popup, UmaViewerUI.Instance.HandlesPanel).Init(this);
        Popup.gameObject.SetActive(false);

        _defaultTransform = new SerializableTransform(Target, Space.Self);

        var collider = gameObject.AddComponent<SphereCollider>();
        collider.center = Vector3.zero;

        Collider = collider;

        return this;
    }

    public virtual void UpdateManual(Camera camera, bool linesEnabled)
    {
        //Made it like this to minimize calls to Camera.main and UI.Inst.Manager.LinesEnabled. Done by HandleManager.

        if (Handle != null && camera != null && transform != null)
        {
            Handle.transform.localScale = Vector3.one * Mathf.Clamp(_baseScale / Vector3.Distance(camera.transform.position, transform.position), 0.1f, 1);

            if (Collider.transform.localScale.x != 0 && transform.lossyScale.x != 0)
            {
                Collider.radius = 35 /* magic number */ * (1 / transform.lossyScale.x) * GetRadiusOnScreen(camera, Collider.transform.position, Handle.transform.localScale.x);
            }

            Handle.transform.position = camera.WorldToScreenPoint(Collider.transform.TransformPoint(Collider.center));

            if (ShouldBeHidden())
            {
                if (Handle.activeSelf == true)
                {
                    ToggleActive(false);
                }
            }
            else
            {
                bool isOnScreen =
                    !(Handle.transform.position.x < 0 || Handle.transform.position.y < 0 || Handle.transform.position.z < 0
                    || Handle.transform.position.x > Screen.width || Handle.transform.position.y > Screen.height);

                if (Handle.activeSelf != isOnScreen)
                {
                    ToggleActive(!Handle.activeSelf);
                }
            }
        }

        if(Handle.activeSelf && LineRenderer != null)
        {
            if (LineRenderer.enabled != linesEnabled)
            {
                LineRenderer.enabled = linesEnabled;
            }
            if (linesEnabled)
            {
                LineRenderer.SetPositions(new Vector3[] { (Target).position, (Target).parent.position });
            }
        }
    }

    protected virtual bool ShouldBeHidden()
    {
        return _forceDisplayOff;
    }

    /// <summary> For future, `offset` param will allow popups to be spaced out when selecting more than 1 handle at a time. </summary>
    public void TogglePopup(int offset = 0)
    {
        Popup.Offset = offset * Popup.GetComponent<RectTransform>().sizeDelta.x;
        UmaViewerUI.Instance.ToggleVisible(Popup.gameObject);
    }

    public UIHandle SetDefaults(Vector3 localPos, Vector3 localRot, Vector3 localScale)
    {
        _defaultTransform.Position  = localPos;
        _defaultTransform.Rotation  = localRot;
        _defaultTransform.Scale     = localScale;
        return this;
    }

    public UIHandle SetColor(Color color)
    {
        color.a = Handle.GetComponent<Image>().color.a;
        Handle.GetComponent<Image>().color = color;
        return this;
    }

    public UIHandle SetScale(float scale)
    {
        this._baseScale = scale;
        return this;
    }

    public UIHandle SetName(string name)
    {
        Handle.gameObject.name = name;
        Popup.NameLabel.text = name;
        return this;
    }

    public UIHandle SetOffset(Vector3 offset)
    {
        Collider.center = offset;
        return this;
    }

    public float GetRadiusOnScreen(Camera cam, Vector3 position, float screenSize)
    {
        Vector3 a = cam.WorldToScreenPoint(position);
        Vector3 b = new Vector3(a.x, a.y + screenSize, a.z);

        Vector3 aa = cam.ScreenToWorldPoint(a);
        Vector3 bb = cam.ScreenToWorldPoint(b);

        return (aa - bb).magnitude;
    }

    public UIHandle WithLineRenderer()
    {
        LineRenderer = Handle.gameObject.AddComponent<LineRenderer>();
        LineRenderer.positionCount  = 2;
        LineRenderer.startWidth     = LineRenderer.endWidth = 0.005f;
        LineRenderer.material       = UmaViewerUI.Instance.HandleManager.LineRendererMaterial;
        LineRenderer.startColor     = LineRenderer.endColor = Handle.GetComponent<Image>().color;
        return this;
    }

    private void OnDestroy()
    {
        if (Popup)
        {
            Destroy(Popup.gameObject);
        }
        if (Handle)
        {
            Destroy(Handle);
        }
        HandleManager.UnregisterHandle(this);
    }

    public void TransformReset(PoseLoadOptions options = null)
    {
        if(options == null)
        {
            options = PoseLoadOptions.All();
        }

        _defaultTransform.ApplyTo(Target);
    }

    public void TransformResetAll()
    {
        var undoData = new HandleManager.RuntimeGizmoUndoData();

        undoData.OldPosition = new SerializableTransform(Target, Space.World);
        TransformReset();
        undoData.NewPosition = Target;

        HandleManager.RegisterRuntimeGizmoUndoAction.Invoke(undoData);
    }

    public void TransformResetPosition()
    {
        var undoData = new HandleManager.RuntimeGizmoUndoData();

        undoData.OldPosition = new SerializableTransform(Target, Space.World);
        TransformReset(new PoseLoadOptions() { Position = true });
        undoData.NewPosition = Target;

        HandleManager.RegisterRuntimeGizmoUndoAction.Invoke(undoData);
    }

    public void TransformResetRotation()
    {
        var undoData = new HandleManager.RuntimeGizmoUndoData();

        undoData.OldPosition = new SerializableTransform(Target, Space.World);
        TransformReset(new PoseLoadOptions() { Rotation = true });
        undoData.NewPosition = Target;

        HandleManager.RegisterRuntimeGizmoUndoAction.Invoke(undoData);
    }

    public void TransformResetScale()
    {
        var undoData = new HandleManager.RuntimeGizmoUndoData();

        undoData.OldPosition = new SerializableTransform(Target, Space.World);
        TransformReset(new PoseLoadOptions() { Scale = true });
        undoData.NewPosition = Target;

        HandleManager.RegisterRuntimeGizmoUndoAction.Invoke(undoData);
    }

    public void ToggleActive(bool value)
    {
        Handle.SetActive(value);
        Collider.enabled = value;
        Popup.gameObject.SetActive(false);
    }

    public void ForceDisplayOff(bool value)
    {
        _forceDisplayOff = value;
    }
}
