using UnityEngine;
using UnityEngine.UI;

namespace HSVPicker
{
    [RequireComponent(typeof(BoxSlider), typeof(RawImage)), ExecuteInEditMode()]
    public class SVBoxSlider : MonoBehaviour
    {
        public ColorPicker picker;

        private BoxSlider slider;
        private RawImage image;

        private int textureWidth = 128;
        private int textureHeight = 128;

        private float lastH = -1;
        private bool listen = true;

        public RectTransform rectTransform
        {
            get
            {
                return transform as RectTransform;
            }
        }

        private void Awake()
        {
            slider = GetComponent<BoxSlider>();
            image = GetComponent<RawImage>();
            if(Application.isPlaying)
            {
                RegenerateSVTexture ();
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying && picker != null)
            {
                slider.onValueChanged.AddListener(SliderChanged);
                picker.onHSVChanged.AddListener(HSVChanged);
            }
        }

        private void OnDisable()
        {
            if (picker != null)
            {
                slider.onValueChanged.RemoveListener(SliderChanged);
                picker.onHSVChanged.RemoveListener(HSVChanged);
            }
        }

        private void OnDestroy()
        {
            if ( image.texture != null )
            {
                DestroyImmediate (image.texture);
            }
        }

        private void SliderChanged(float saturation, float value)
        {
            if (listen)
            {
                picker.AssignColor(ColorValues.Saturation, saturation);
                picker.AssignColor(ColorValues.Value, value);
            }
            listen = true;
        }

        private void HSVChanged(float h, float s, float v)
        {
            if (!lastH.Equals(h))
            {
                lastH = h;
                RegenerateSVTexture();
            }

            if (!s.Equals(slider.normalizedValue))
            {
                listen = false;
                slider.normalizedValue = s;
            }

            if (!v.Equals(slider.normalizedValueY))
            {
                listen = false;
                slider.normalizedValueY = v;
            }
        }

        private void RegenerateSVTexture()
        {
            double h = picker != null ? picker.H * 360 : 0;

            if ( image.texture != null )
                DestroyImmediate (image.texture);

            var texture = new Texture2D (textureWidth, textureHeight);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.hideFlags = HideFlags.DontSave;

            for ( int s = 0; s < textureWidth; s++ )
            {
                Color[] colors = new Color[textureHeight];
                for ( int v = 0; v < textureHeight; v++ )
                {
                    colors[v] = HSVUtil.ConvertHsvToRgb (h, (float)s / textureWidth, (float)v / textureHeight, 1);
                }
                texture.SetPixels (s, 0, 1, textureHeight, colors);
            }
            texture.Apply();

            image.texture = texture;
            
        }
    }
}