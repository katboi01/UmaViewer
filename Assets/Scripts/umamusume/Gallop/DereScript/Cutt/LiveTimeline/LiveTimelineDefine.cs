namespace Cutt
{
    public static class LiveTimelineDefine
    {
        public enum MouthType
        {
            N = 6,
            N_SmileClose = 7,
            N_Smile = 8,
            A = 1,
            I = 2,
            U = 3,
            E = 4,
            O = 5,
            Smile = 10,
            SmileClose = 13,
            Wink = 11,
            SadMouth = 12
        }

        public const string kSheetAssetLabel = "CuttSheet";

        public const int kCameraMaxNum = 3;

        public const int kBezierControlPointMax = 15;

        public const int kBezierOldCalcMethodCPMax = 3;

        public const string kMotionResourcesDir = "3D/Chara/Motion/Legacy/";
    }
}
