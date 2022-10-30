using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Cyalume
{
    public class AudienceController
    {
        public enum AnimationSetting
        {
            Default = 0,
            SyncCyalume = 1,
            Direct = 2,
            Max = 3
        }

        public enum AnimationBodyRegion
        {
            None = 0,
            RightHand = 1,
            LeftHand = 2,
            BothHands = 3,
            Max = 4
        }

        public enum AnimationCategory
        {
            None = 0,
            Common = 1,
            Unique = 2,
            Max = 3
        }
    }
}
