using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineTransformData : ILiveTimelineGroupDataWithName
    {
        public struct TransformBaseData
        {
            public Vector3 position;

            public Quaternion rotation;

            public Vector3 scale;
        }

        public delegate void TimelineTransformUpdateDelegate(Transform transform, ref TransformBaseData updateData, ref TransformBaseData startData);

        private const string default_name = "Transform Object";

        public LiveTimelineKeyTransformDataList keys = new LiveTimelineKeyTransformDataList();

        public bool enablePosition = true;

        public bool enableRotate = true;

        public bool enableScale;

        public TimelineTransformUpdateDelegate OnUpdateTimelineTransform = LiveTimelineTransformData.TimelineTransformUpdateXXX;

        public LiveTimelineTransformData() : base("Transform Object") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }

        public override void UpdateStatus()
        {
            base.UpdateStatus();
            if (enablePosition)
            {
                if (enableRotate)
                {
                    if (enableScale)
                    {
                        OnUpdateTimelineTransform = TimelineTransformUpdatePRS;
                    }
                    else
                    {
                        OnUpdateTimelineTransform = TimelineTransformUpdatePRX;
                    }
                }
                else if (enableScale)
                {
                    OnUpdateTimelineTransform = TimelineTransformUpdatePXS;
                }
                else
                {
                    OnUpdateTimelineTransform = TimelineTransformUpdatePXX;
                }
            }
            else if (enableRotate)
            {
                if (enableScale)
                {
                    OnUpdateTimelineTransform = TimelineTransformUpdateXRS;
                }
                else
                {
                    OnUpdateTimelineTransform = TimelineTransformUpdateXRX;
                }
            }
            else if (enableScale)
            {
                OnUpdateTimelineTransform = TimelineTransformUpdateXXS;
            }
            else
            {
                OnUpdateTimelineTransform = TimelineTransformUpdateXXX;
            }
        }

        public static void TimelineTransformUpdateXXX(Transform transform, ref TransformBaseData updateData, ref TransformBaseData startData)
        {
            transform.localPosition = startData.position;
            transform.localRotation = startData.rotation;
            transform.localScale = startData.scale;
        }

        public static void TimelineTransformUpdatePXX(Transform transform, ref TransformBaseData updateData, ref TransformBaseData startData)
        {
            transform.localPosition = updateData.position;
            transform.localRotation = startData.rotation;
            transform.localScale = startData.scale;
        }

        public static void TimelineTransformUpdateXRX(Transform transform, ref TransformBaseData updateData, ref TransformBaseData startData)
        {
            transform.localPosition = startData.position;
            transform.localRotation = updateData.rotation;
            transform.localScale = startData.scale;
        }

        public static void TimelineTransformUpdateXXS(Transform transform, ref TransformBaseData updateData, ref TransformBaseData startData)
        {
            transform.localPosition = startData.position;
            transform.localRotation = startData.rotation;
            transform.localScale = updateData.scale;
        }

        public static void TimelineTransformUpdatePRX(Transform transform, ref TransformBaseData updateData, ref TransformBaseData startData)
        {
            transform.localPosition = updateData.position;
            transform.localRotation = updateData.rotation;
            transform.localScale = startData.scale;
        }

        public static void TimelineTransformUpdatePXS(Transform transform, ref TransformBaseData updateData, ref TransformBaseData startData)
        {
            transform.localPosition = updateData.position;
            transform.localRotation = startData.rotation;
            transform.localScale = updateData.scale;
        }

        public static void TimelineTransformUpdateXRS(Transform transform, ref TransformBaseData updateData, ref TransformBaseData startData)
        {
            transform.localPosition = startData.position;
            transform.localRotation = updateData.rotation;
            transform.localScale = updateData.scale;
        }

        public static void TimelineTransformUpdatePRS(Transform transform, ref TransformBaseData updateData, ref TransformBaseData startData)
        {
            transform.localPosition = updateData.position;
            transform.localRotation = updateData.rotation;
            transform.localScale = updateData.scale;
        }
    }
}
