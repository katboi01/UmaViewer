using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace HSVPicker
{
    [RequireComponent(typeof(TMP_InputField))]
    public class HexColorField : MonoBehaviour
    {
        public ColorPicker hsvpicker;

        public bool displayAlpha;

        private TMP_InputField hexInputField;

        private void Awake()
        {
            hexInputField = GetComponent<TMP_InputField>();

            // Add listeners to keep text (and color) up to date
            hexInputField.onEndEdit.AddListener(UpdateColor);
            hsvpicker.onValueChanged.AddListener(UpdateHex);
        }

        private void OnDestroy()
        {
            hexInputField.onValueChanged.RemoveListener(UpdateColor);
            hsvpicker.onValueChanged.RemoveListener(UpdateHex);
        }

        private void UpdateHex(Color newColor)
        {
            hexInputField.text = ColorToHex(newColor);
        }

        private void UpdateColor(string newHex)
        {
            Color color;
            if (!newHex.StartsWith("#"))
                newHex = "#"+newHex;
            if (ColorUtility.TryParseHtmlString(newHex, out color))
                hsvpicker.CurrentColor = color;
            else
                Debug.Log("hex value is in the wrong format, valid formats are: #RGB, #RGBA, #RRGGBB and #RRGGBBAA (# is optional)");
        }

        private string ColorToHex(Color32 color)
        {
            return displayAlpha
                ? string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.r, color.g, color.b, color.a)
                : string.Format("#{0:X2}{1:X2}{2:X2}", color.r, color.g, color.b);
        }
    }
}