using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] private TMP_Text Text;
    [SerializeField] private Button AcceptButton;
    [SerializeField] private Button RejectButton;
    [SerializeField] private RectTransform PopupBody;

    private System.Action _onAccept;
    private System.Action _onReject;

    public static Popup Create(string text, float width = -1, float height = -1, string acceptText = "Ok", System.Action onAccept = null, string rejectText = "", System.Action onReject = null)
    {
        var popup = Instantiate(Resources.Load<Popup>("Prefabs/FullScreenPopup"), UmaViewerUI.Instance.canvasScaler.transform);

        popup.Text.text = text;

        if (!string.IsNullOrEmpty(acceptText))
        {
            popup._onAccept = onAccept;
            popup.AcceptButton.GetComponentInChildren<TMP_Text>().text = acceptText;
        }
        if (!string.IsNullOrEmpty(rejectText))
        {
            popup._onReject = onReject;
            popup.RejectButton.GetComponentInChildren<TMP_Text>().text = rejectText;
        }

        var sizeDelta = popup.PopupBody.sizeDelta;
        if (width != -1)
        {
            sizeDelta.x = width;
        }
        if(height != -1)
        {
            sizeDelta.y = height;
        }
        popup.PopupBody.sizeDelta = sizeDelta;

        return popup;
    }

    public void OnAcceptButtonClick()
    {
        _onAccept?.Invoke();
        Destroy(this.gameObject);
    }

    public void OnRejectButtonClick()
    {
        _onReject?.Invoke();
        Destroy(this.gameObject);
    }
}
