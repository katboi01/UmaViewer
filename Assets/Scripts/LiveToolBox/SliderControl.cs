using UnityEngine;
using UnityEngine.EventSystems;

public class SliderControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool is_Touched = false;
    public bool is_Outed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        is_Touched = true;
        Debug.Log("Dowm");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        is_Touched = false;
        is_Outed = true;
        Debug.Log("Up");
    }
}
