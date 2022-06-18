using UnityEngine;
using UnityEngine.UI;
namespace HSVPicker
{
    [RequireComponent(typeof(RawImage)), ExecuteInEditMode()]
    public class ColorSliderImage : MonoBehaviour
    {
        public ColorPicker picker;

        /// <summary>
        /// Which value this slider can edit.
        /// </summary>
        public ColorValues type;

        public Slider.Direction direction;

        private RawImage image;

        private RectTransform rectTransform
        {
            get
            {
                return transform as RectTransform;
            }
        }

        private void Awake()
        {
            image = GetComponent<RawImage>();

            if(Application.isPlaying)
                RegenerateTexture();
        }

        private void OnEnable()
        {
            if (picker != null && Application.isPlaying)
            {
                picker.onValueChanged.AddListener(ColorChanged);
                picker.onHSVChanged.AddListener(HSVChanged);
            }
        }

        private void OnDisable()
        {
            if (picker != null)
            {
                picker.onValueChanged.RemoveListener(ColorChanged);
                picker.onHSVChanged.RemoveListener(HSVChanged);
            }
        }

        private void OnDestroy()
        {
            if (image.texture != null)
                DestroyImmediate(image.texture);
        }

        private void ColorChanged(Color newColor)
        {
            switch (type)
            {
                case ColorValues.R:
                case ColorValues.G:
                case ColorValues.B:
                case ColorValues.Saturation:
                case ColorValues.Value:
                    RegenerateTexture();
                    break;
                case ColorValues.A:
                case ColorValues.Hue:
                default:
                    break;
            }
        }

        private void HSVChanged(float hue, float saturation, float value)
        {
            switch (type)
            {
                case ColorValues.R:
                case ColorValues.G:
                case ColorValues.B:
                case ColorValues.Saturation:
                case ColorValues.Value:
                    RegenerateTexture();
                    break;
                case ColorValues.A:
                case ColorValues.Hue:
                default:
                    break;
            }
        }

        private void RegenerateTexture()
        {
            Color32 baseColor = picker != null ? picker.CurrentColor : Color.black;

            float h = picker != null ? picker.H : 0;
            float s = picker != null ? picker.S : 0;
            float v = picker != null ? picker.V : 0;

            Texture2D texture;
            Color32[] colors;

            bool vertical = direction == Slider.Direction.BottomToTop || direction == Slider.Direction.TopToBottom;
            bool inverted = direction == Slider.Direction.TopToBottom || direction == Slider.Direction.RightToLeft;

            int size;
            switch (type)
            {
                case ColorValues.R:
                case ColorValues.G:
                case ColorValues.B:
                case ColorValues.A:
                    size = 255;
                    break;
                case ColorValues.Hue:
                    size = 360;
                    break;
                case ColorValues.Saturation:
                case ColorValues.Value:
                    size = 100;
                    break;
                default:
                    throw new System.NotImplementedException("");
            }
            if (vertical)
                texture = new Texture2D(1, size);
            else
                texture = new Texture2D(size, 1);

            texture.hideFlags = HideFlags.DontSave;
            colors = new Color32[size];

            switch (type)
            {
                case ColorValues.R:
                    for (byte i = 0; i < size; i++)
                    {
                        colors[inverted ? size - 1 - i : i] = new Color32(i, baseColor.g, baseColor.b, 255);
                    }
                    break;
                case ColorValues.G:
                    for (byte i = 0; i < size; i++)
                    {
                        colors[inverted ? size - 1 - i : i] = new Color32(baseColor.r, i, baseColor.b, 255);
                    }
                    break;
                case ColorValues.B:
                    for (byte i = 0; i < size; i++)
                    {
                        colors[inverted ? size - 1 - i : i] = new Color32(baseColor.r, baseColor.g, i, 255);
                    }
                    break;
                case ColorValues.A:
                    for (byte i = 0; i < size; i++)
                    {
                        colors[inverted ? size - 1 - i : i] = new Color32(i, i, i, 255);
                    }
                    break;
                case ColorValues.Hue:
                    for (int i = 0; i < size; i++)
                    {
                        colors[inverted ? size - 1 - i : i] = HSVUtil.ConvertHsvToRgb(i, 1, 1, 1);
                    }
                    break;
                case ColorValues.Saturation:
                    for (int i = 0; i < size; i++)
                    {
                        colors[inverted ? size - 1 - i : i] = HSVUtil.ConvertHsvToRgb(h * 360, (float)i / size, v, 1);
                    }
                    break;
                case ColorValues.Value:
                    for (int i = 0; i < size; i++)
                    {
                        colors[inverted ? size - 1 - i : i] = HSVUtil.ConvertHsvToRgb(h * 360, s, (float)i / size, 1);
                    }
                    break;
                default:
                    throw new System.NotImplementedException("");
            }
            texture.SetPixels32(colors);
            texture.Apply();

            if (image.texture != null)
                DestroyImmediate(image.texture);
            image.texture = texture;

            switch (direction)
            {
                case Slider.Direction.BottomToTop:
                case Slider.Direction.TopToBottom:
                    image.uvRect = new Rect(0, 0, 2, 1);
                    break;
                case Slider.Direction.LeftToRight:
                case Slider.Direction.RightToLeft:
                    image.uvRect = new Rect(0, 0, 1, 2);
                    break;
                default:
                    break;
            }
        }

    }
}