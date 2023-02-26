using System;
using UnityEngine;

namespace Gallop
{
    [Serializable]
    public class CharaShaderEffectData : ScriptableObject 
    {
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
                        mat.SetVector("_UVEmissiveScroll", new Vector4(p[0], p[1], p[2],p[3]));
                        mat.SetVector("_UVEmissiveRange", new Vector4(p[4], p[5], p[6], p[7]));
                    }
                }
            }
        }
    }
}
