using I18N.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class UIHandle : MonoBehaviour
{
    /// <summary> Object that "owns" the handle </summary>
    public GameObject Owner;

    /// <summary> Object that the handle refers to. Cast in inheriting class </summary>
    public object Target;

    /// <summary> Display object on the UI </summary>
    public GameObject Handle;

    /// <summary> Collider for selecting the handle on UI </summary>
    public SphereCollider Collider;

    /// <summary> Object that appears when right-clicking the handle </summary>
    public UIPopupPanel Popup;

    protected SerializableTransform _defaultTransform;
    protected float _baseScale = 4;
    protected bool _forceDisplayOff = false;

    public virtual UIHandle Init(GameObject owner, object target)
    {
        Target = target;
        return Init(owner);
    }

    public virtual UIHandle Init(GameObject owner)
    {
        gameObject.layer = LayerMask.NameToLayer("UIHandle");
        Target = Target == null? owner : Target;
        Owner = owner;

        Popup = Instantiate(UmaViewerUI.Instance.HandleManager.Pfb_Popup, UmaViewerUI.Instance.HandlesPanel).Init(this);
        Popup.gameObject.SetActive(false);

        Handle = Instantiate(UmaViewerUI.Instance.HandleManager.Pfb_HandleDisplay, UmaViewerUI.Instance.HandlesPanel);
        Handle.transform.SetAsFirstSibling();

        _defaultTransform = new SerializableTransform(transform, Space.Self);

        var collider = gameObject.AddComponent<SphereCollider>();
        collider.center = Vector3.zero;

        Collider = collider;

        return this;
    }

    protected virtual void Update()
    {
        var cam = Camera.main;
        if (Handle != null && cam != null && transform != null)
        {
            Handle.transform.localScale = Vector3.one * Mathf.Clamp(_baseScale / Vector3.Distance(cam.transform.position, transform.position), 0.1f, 1);

            if (Collider.transform.localScale.x != 0 && transform.lossyScale.x != 0)
            {
                Collider.radius = 35 /* magic number */ * (1 / transform.lossyScale.x) * GetRadiusOnScreen(Collider.transform.position, Handle.transform.localScale.x);
            }

            Handle.transform.position = cam.WorldToScreenPoint(Collider.transform.TransformPoint(Collider.center));

            if (_forceDisplayOff)
            {
                if (Handle.activeSelf == true)
                {
                    Handle.SetActive(false);
                }
            }
            else
            {
                bool isOnScreen =
                    !(Handle.transform.position.x < 0 || Handle.transform.position.y < 0 || Handle.transform.position.z < 0
                    || Handle.transform.position.x > Screen.width || Handle.transform.position.y > Screen.height);

                if (Handle.activeSelf != isOnScreen)
                {
                    Handle.SetActive(!Handle.activeSelf);
                }
            }
        }
    }

    public void TogglePopup(int offset)
    {
        //Popup.GetComponent<PopupController>().offset = offset * Popup.GetComponent<RectTransform>().sizeDelta.x;
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
        Popup.GetComponentInChildren<Text>().text = name;
        return this;
    }

    public UIHandle SetOffset(Vector3 offset)
    {
        Collider.center = offset;
        return this;
    }

    public float GetRadiusOnScreen(Vector3 position, float screenSize)
    {
        Vector3 a = Camera.main.WorldToScreenPoint(position);
        Vector3 b = new Vector3(a.x, a.y + screenSize, a.z);

        Vector3 aa = Camera.main.ScreenToWorldPoint(a);
        Vector3 bb = Camera.main.ScreenToWorldPoint(b);

        return (aa - bb).magnitude;
    }

    public UIHandle ClearForDeletion()
    {
        Popup = null;
        Handle = null;
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
    }

    public UIHandle PositionReset()
    {
        transform.localPosition = _defaultTransform.Position;
        return this;
    }

    public UIHandle RotationReset()
    {
        transform.localEulerAngles = _defaultTransform.Rotation;
        return this;
    }

    public UIHandle ScaleReset()
    {
        transform.localScale = _defaultTransform.Scale;
        return this;
    }

    public void ToggleActive(bool value)
    {
        this.enabled = value;
        Handle.SetActive(value);
        Collider.enabled = value;
        Popup.gameObject.SetActive(false);
    }

    public void ToggleVisible(bool value)
    {
        _forceDisplayOff = value;
    }
}
