using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableDialogBox : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject dialogBox;
    private bool isDragging = false;
    private Vector3 offset;

    void Update()
    {
        if (isDragging)
        {
            // 计算鼠标的新位置并更新dialogBox的位置
            Vector3 newPosition = Input.mousePosition + offset;
            dialogBox.transform.position = newPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 检查是否按下鼠标中键
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            // 计算鼠标相对于dialogBox的位置偏移
            offset = dialogBox.transform.position - Input.mousePosition;
            isDragging = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 检查是否松开鼠标中键
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            isDragging = false;
        }
    }
}
