using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Gallop.Live.Cutt.LiveTimelineDefine;

namespace Gallop.Live.Cutt
{
    public struct TransformUpdateInfo
    {
        public LiveTimelineTransformData data;
        public TransformBaseData updateData;
    }

    public delegate void TransformUpdateInfoDelegate(ref TransformUpdateInfo updateInfo);
}