using UnityEngine;
namespace HSVPicker
{
    public class ColorPicker : MonoBehaviour
    {

        private float _hue = 0;
        private float _saturation = 0;
        private float _brightness = 0;

        [SerializeField]
        private Color _color = Color.red;

        [Header("Setup")]
        public ColorPickerSetup Setup;

        [Header("Event")]
        public ColorChangedEvent onValueChanged = new ColorChangedEvent();
        public HSVChangedEvent onHSVChanged = new HSVChangedEvent();

        public Color CurrentColor
        {
            get
            {
                return _color;
            }
            set
            {
                if (CurrentColor == value)
                    return;

                _color = value;

                RGBChanged();
            
                SendChangedEvent();
            }
        }

        private void Start()
        {
            Setup.AlphaSlidiers.Toggle(Setup.ShowAlpha);
            Setup.ColorToggleElement.Toggle(Setup.ShowColorSliderToggle);
            Setup.RgbSliders.Toggle(Setup.ShowRgb);
            Setup.HsvSliders.Toggle(Setup.ShowHsv);
            Setup.ColorBox.Toggle(Setup.ShowColorBox);

            HandleHeaderSetting(Setup.ShowHeader);
            UpdateColorToggleText();

            RGBChanged();
            SendChangedEvent();
        }

        public float H
        {
            get
            {
                return _hue;
            }
            set
            {
                if (_hue == value)
                    return;

                _hue = value;

                HSVChanged();

                SendChangedEvent();
            }
        }

        public float S
        {
            get
            {
                return _saturation;
            }
            set
            {
                if (_saturation == value)
                    return;

                _saturation = value;

                HSVChanged();

                SendChangedEvent();
            }
        }

        public float V
        {
            get
            {
                return _brightness;
            }
            set
            {
                if (_brightness == value)
                    return;

                _brightness = value;

                HSVChanged();

                SendChangedEvent();
            }
        }

        public float R
        {
            get
            {
                return _color.r;
            }
            set
            {
                if (_color.r == value)
                    return;

                _color.r = value;

                RGBChanged();

                SendChangedEvent();
            }
        }

        public float G
        {
            get
            {
                return _color.g;
            }
            set
            {
                if (_color.g == value)
                    return;

                _color.g = value;

                RGBChanged();

                SendChangedEvent();
            }
        }

        public float B
        {
            get
            {
                return _color.b;
            }
            set
            {
                if (_color.b == value)
                    return;

                _color.b = value;

                RGBChanged();

                SendChangedEvent();
            }
        }

        private float A
        {
            get
            {
                return _color.a;
            }
            set
            {
                if (_color.a == value)
                    return;

                _color.a = value;

                SendChangedEvent();
            }
        }

        private void RGBChanged()
        {
            HsvColor color = HSVUtil.ConvertRgbToHsv(CurrentColor);

            _hue = color.normalizedH;
            _saturation = color.normalizedS;
            _brightness = color.normalizedV;
        }

        private void HSVChanged()
        {
            Color color = HSVUtil.ConvertHsvToRgb(_hue * 360, _saturation, _brightness, _color.a);

            _color = color;
        }

        private void SendChangedEvent()
        {
            onValueChanged.Invoke(CurrentColor);
            onHSVChanged.Invoke(_hue, _saturation, _brightness);
        }

        public void AssignColor(ColorValues type, float value)
        {
            switch (type)
            {
                case ColorValues.R:
                    R = value;
                    break;
                case ColorValues.G:
                    G = value;
                    break;
                case ColorValues.B:
                    B = value;
                    break;
                case ColorValues.A:
                    A = value;
                    break;
                case ColorValues.Hue:
                    H = value;
                    break;
                case ColorValues.Saturation:
                    S = value;
                    break;
                case ColorValues.Value:
                    V = value;
                    break;
                default:
                    break;
            }
        }

        public void AssignColor(Color color)
        {
            CurrentColor = color;
        }

        public float GetValue(ColorValues type)
        {
            switch (type)
            {
                case ColorValues.R:
                    return R;
                case ColorValues.G:
                    return G;
                case ColorValues.B:
                    return B;
                case ColorValues.A:
                    return A;
                case ColorValues.Hue:
                    return H;
                case ColorValues.Saturation:
                    return S;
                case ColorValues.Value:
                    return V;
                default:
                    throw new System.NotImplementedException("");
            }
        }

        public void ToggleColorSliders()
        {
            Setup.ShowHsv = !Setup.ShowHsv;
            Setup.ShowRgb = !Setup.ShowRgb;
            Setup.HsvSliders.Toggle(Setup.ShowHsv);
            Setup.RgbSliders.Toggle(Setup.ShowRgb);


            UpdateColorToggleText();
        }

        void UpdateColorToggleText()
        {
            if (Setup.ShowRgb)
            {
                Setup.SliderToggleButtonText.text = "RGB";
            }

            if (Setup.ShowHsv)
            {
                Setup.SliderToggleButtonText.text = "HSV";
            }
        }

        private void HandleHeaderSetting(ColorPickerSetup.ColorHeaderShowing setupShowHeader)
        {
            if (setupShowHeader == ColorPickerSetup.ColorHeaderShowing.Hide)
            {
                Setup.ColorHeader.Toggle(false);
                return;
            }

            Setup.ColorHeader.Toggle(true);

            Setup.ColorPreview.Toggle(setupShowHeader != ColorPickerSetup.ColorHeaderShowing.ShowColorCode);
            Setup.ColorCode.Toggle(setupShowHeader != ColorPickerSetup.ColorHeaderShowing.ShowColor);

        }
    }

}