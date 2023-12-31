using System;

namespace LibMMD.Unity3D
{
    public enum MMDConfigSwitch
    {
        AsConfig = 0,
        ForceTrue = 1,
        ForceFalse = 2
    }

    public class MMDUnityConfig
    {
        public MMDConfigSwitch EnableDrawSelfShadow;
        public MMDConfigSwitch EnableCastShadow;
        public MMDConfigSwitch EnableEdge;
        
        public static bool DealSwitch(MMDConfigSwitch switchVal, bool configVal)
        {
            switch (switchVal)
            {
                case MMDConfigSwitch.AsConfig:
                    return configVal;
                case MMDConfigSwitch.ForceTrue:
                    return true;
                case MMDConfigSwitch.ForceFalse:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("switchVal", switchVal, null);
            }
        }
    }
}