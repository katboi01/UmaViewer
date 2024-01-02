using UnityEngine;
using UnityEngine.EventSystems;

public class UIElementDragger : EventTrigger
{
    private bool dragging;
    public Transform alsoDrag;
    public Vector3 alsoDragOffset = Vector3.zero;
    private void Start()
    {
        alsoDrag = transform.parent.GetChild(0);
        if (alsoDrag)
        {
            alsoDragOffset = transform.position - alsoDrag.position;
        }
    }

    public void Update()
    {
        if (dragging)
        {
            transform.position = new Vector2(Mathf.Clamp(Input.mousePosition.x, 10, Screen.width-10), Mathf.Clamp(Input.mousePosition.y, 10, Screen.height-10));
            if (alsoDrag)
            {
                alsoDrag.position = transform.position - alsoDragOffset;
            }
        }
        else
        {
            transform.position = new Vector2(Mathf.Clamp(transform.position.x, 10, Screen.width - 10), Mathf.Clamp(transform.position.y, 10, Screen.height - 10));
            if (alsoDrag)
            {
                alsoDrag.position = transform.position - alsoDragOffset;
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
    }
}
