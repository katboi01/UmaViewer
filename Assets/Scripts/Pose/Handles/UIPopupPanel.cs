using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupPanel : MonoBehaviour
{
    public UIHandle Owner;
    public float Offset;
    public Transform Content;
    public TMPro.TMP_Text NameLabel;
    public Dictionary<GameObject, System.Func<bool>> ConditionalButtons = new Dictionary<GameObject, System.Func<bool>>();

    private int _side;

    public UIPopupPanel Init(UIHandle owner)
    {
        Owner = owner;
        transform.SetAsFirstSibling();
        NameLabel.text = owner.Target.name;
        return this;
    }

    public void UpdateManual(Camera camera)
    {
        if (camera.WorldToScreenPoint(Owner.transform.position).x < camera.pixelWidth / 2 - _side)
        {
            _side = -1;
        }
        else
        {
            _side = 1;
        }

        float pos = _side * camera.pixelWidth / 20 + Offset;
        transform.position = camera.WorldToScreenPoint(Owner.transform.position) + pos * Vector3.right;

        foreach (var kv in ConditionalButtons)
        {
            kv.Key.gameObject.SetActive(kv.Value.Invoke());
        }
    }

    public void AddButton(string name, System.Action callback)
    {
        Button b = Instantiate(UmaViewerUI.Instance.HandleManager.Pfb_PopupButton, Content).GetComponent<Button>();
        b.onClick.AddListener(()=>callback.Invoke());
        b.GetComponentInChildren<TMPro.TMP_Text>().text = name;
    }

    public void AddConditionalButton(string name, System.Func<bool> condition, System.Action callback)
    {
        Button b = Instantiate(UmaViewerUI.Instance.HandleManager.Pfb_PopupButton, Content).GetComponent<Button>();
        b.onClick.AddListener(() => callback.Invoke());
        b.GetComponentInChildren<TMPro.TMP_Text>().text = name;
        ConditionalButtons.Add(b.gameObject, condition);
    }
}
