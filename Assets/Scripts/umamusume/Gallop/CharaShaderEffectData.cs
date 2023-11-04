using System;
using UnityEngine;

namespace Gallop
{
    [Serializable]
    public class CharaShaderEffectData : ScriptableObject 
    {

        public const int DATA_UTYPE_INDEX = 0;
        public const int DATA_SCROLL_X = 1;
        public const int DATA_VTYPE_INDEX = 2;
        public const int DATA_SCROLL_Y = 3;
        public const int DATA_POWERTYPE_INDEX = 4;
        public const int DATA_POWER_INDEX = 5;
        public const int DATA_SCROLL_X_CURVE_INDEX = 6;
        public const int DATA_SCROLL_Y_CURVE_INDEX = 7;
        public const int DATA_SCROLL_POWER_CURVE_INDEX = 8;
        protected const int DATA_SET_TUEXTURE = 9;
        protected const int DATA_SET_SHADER = 10;
        protected const int DATA_SET_DIRT = 11;
        private const int DATA_NUM = 12;
        private const float INT_TO_FLOAT = 0.001f;

        private int uTypeIndex;
        private int scrollX;
        private int vTypeIndex;
        private int scrollY;
        private int powerTypeIndex;
        private int powerIndex;
        private int xCurveIndex;
        private int yCurveIndex;
        private int powerCurveIndex;
        private int setTuexture;
        private int setShader;
        private int setDirt;

        private float magicClock = 0;


        [SerializeField] 
        private SettingData[] _settingDataArray; 

        [Serializable]
        public class SettingData 
        {
            public EffectType Type; 
            public Material TargetMaterial; 
            public int[] ParamArray; 
            public AnimationCurve[] ParamCurveArray;
        }

        public enum EffectType 
        {
            UVScrollEmissive = 0,
            ReflectionMap = 1
        }

        public void Initialize()
        {
            foreach(var settingData in _settingDataArray)
            {
                if (settingData.TargetMaterial)
                {
                    var mat = settingData.TargetMaterial;
                    if (settingData.Type == EffectType.UVScrollEmissive)
                    {
                        var p = settingData.ParamArray;

                        /*
                        uTypeIndex = p[0];
                        scrollX = p[1];
                        vTypeIndex = p[2];
                        scrollY = p[3];
                        powerTypeIndex = p[4];
                        powerIndex = p[5];
                        xCurveIndex = p[6];
                        yCurveIndex = p[7];
                        powerCurveIndex = p[8];
                        setTuexture = p[9];
                        setShader = p[10];
                        setDirt = p[11];
                        */

                        mat.SetVector("_UVEmissiveRange", new Vector4(p[0], p[1], p[2],p[3]));
                        //mat.SetVector("_UVEmissiveScroll", new Vector4(p[4], p[5], p[6], p[7]));
                    }
                }
            }
        }


        public void updateUV(float deltaTime)
        {
            magicClock += deltaTime;


            foreach (var settingData in _settingDataArray)
            {
                if (settingData.TargetMaterial)
                { 
                    var mat = settingData.TargetMaterial;
                    if (settingData.Type == EffectType.UVScrollEmissive)
                    {
                        var p = settingData.ParamArray;
                        var a = settingData.ParamCurveArray;

                        float _xScroll = calcUV(p[0], p[1], p[6], a);
                        float _yScroll = calcUV(p[2], p[3], p[7], a);
                        float _powerScroll = calcUV(p[4], p[5], p[8], a);

                        Debug.Log(_powerScroll);

                        mat.SetVector("_UVEmissiveScroll", new Vector4(_xScroll, _yScroll, 0, 0));
                        mat.SetFloat("_UVEmissivePower", _powerScroll);
                    }
                        
                }
            }

        }

        public float calcUV(int index, int value, int curveIndex, AnimationCurve[] ParamCurveArray)
        {
            switch(index)
            {
                case 0:
                    return value * 0.001f;
                case 1:
                    return value * magicClock * 0.001f;
                case 2:
                    float result = (value * magicClock * 0.001f) % 2.0f;
                    Debug.Log(result);
                    return result;
                case 5:
                    float ratio = value * magicClock * 0.001f;
                    return ParamCurveArray[curveIndex].Evaluate(ratio - Mathf.Floor(ratio));
                default:
                    return 0;
            }
        }
    }
}
