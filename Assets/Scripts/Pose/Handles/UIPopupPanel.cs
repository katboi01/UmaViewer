using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        NameLabel.text = owner.name;
        return this;
    }

    private void Update()
    {
        var cam = Camera.current;

        if (cam.WorldToScreenPoint(Owner.transform.position).x < cam.pixelWidth / 2 - _side)
        {
            _side = -1;
        }
        else
        {
            _side = 1;
        }

        float pos = _side * cam.pixelWidth / 20 + Offset;
        transform.position = cam.WorldToScreenPoint(Owner.transform.position) + pos * Vector3.right;

        foreach (var kv in ConditionalButtons)
        {
            kv.Key.gameObject.SetActive(kv.Value.Invoke());
        }
    }

    public void AddButton(string name, System.Action callback)
    {
        Button b = Instantiate(UmaViewerUI.Instance.HandleManager.Pfb_PopupButton, Content).GetComponent<Button>();
        b.onClick.AddListener(()=>callback.Invoke());
        b.GetComponentInChildren<Text>().text = name;
    }

    public void AddConditionalButton(string name, System.Func<bool> condition, System.Action callback)
    {
        Button b = Instantiate(UmaViewerUI.Instance.HandleManager.Pfb_PopupButton, Content).GetComponent<Button>();
        b.onClick.AddListener(() => callback.Invoke());
        b.GetComponentInChildren<Text>().text = name;
        ConditionalButtons.Add(b.gameObject, condition);
    }
}
