using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineObjectData : ILiveTimelineGroupDataWithName
    {
        public delegate void TimelineTransformUpdateDelegate(Transform transform, ref LiveTimelineTransformData.TransformBaseData updateData, ref LiveTimelineTransformData.TransformBaseData startData);

        private const string default_name = "Object Object";

        public LiveTimelineKeyObjectDataList keys = new LiveTimelineKeyObjectDataList();

        public bool enablePosition = true;

        public bool enableRotate = true;

        public bool enableScale = true;

        public TimelineTransformUpdateDelegate OnUpdateTimelineTransform = TimelineTransformUpdateXXX;

        public LiveTimelineObjectData() : base("Object Object") { }

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


        public static void TimelineTransformUpdateXXX(Transform transform, ref LiveTimelineTransformData.TransformBaseData updateData, ref LiveTimelineTransformData.TransformBaseData startData)
        {
            transform.localPosition = startData.position;
            transform.localRotation = startData.rotation;
            transform.localScale = startData.scale;
        }

        public static void TimelineTransformUpdatePXX(Transform transform, ref LiveTimelineTransformData.TransformBaseData updateData, ref LiveTimelineTransformData.TransformBaseData startData)
        {
            transform.localPosition = updateData.position;
            transform.localRotation = startData.rotation;
            transform.localScale = startData.scale;
        }

        public static void TimelineTransformUpdateXRX(Transform transform, ref LiveTimelineTransformData.TransformBaseData updateData, ref LiveTimelineTransformData.TransformBaseData startData)
        {
            transform.localPosition = startData.position;
            transform.localRotation = updateData.rotation;
            transform.localScale = startData.scale;
        }

        public static void TimelineTransformUpdateXXS(Transform transform, ref LiveTimelineTransformData.TransformBaseData updateData, ref LiveTimelineTransformData.TransformBaseData startData)
        {
            transform.localPosition = startData.position;
            transform.localRotation = startData.rotation;
            transform.localScale = updateData.scale;
        }

        public static void TimelineTransformUpdatePRX(Transform transform, ref LiveTimelineTransformData.TransformBaseData updateData, ref LiveTimelineTransformData.TransformBaseData startData)
        {
            transform.localPosition = updateData.position;
            transform.localRotation = updateData.rotation;
            transform.localScale = startData.scale;
        }

        public static void TimelineTransformUpdatePXS(Transform transform, ref LiveTimelineTransformData.TransformBaseData updateData, ref LiveTimelineTransformData.TransformBaseData startData)
        {
            transform.localPosition = updateData.position;
            transform.localRotation = startData.rotation;
            transform.localScale = updateData.scale;
        }

        public static void TimelineTransformUpdateXRS(Transform transform, ref LiveTimelineTransformData.TransformBaseData updateData, ref LiveTimelineTransformData.TransformBaseData startData)
        {
            transform.localPosition = startData.position;
            transform.localRotation = updateData.rotation;
            transform.localScale = updateData.scale;
        }

        public static void TimelineTransformUpdatePRS(Transform transform, ref LiveTimelineTransformData.TransformBaseData updateData, ref LiveTimelineTransformData.TransformBaseData startData)
        {
            transform.localPosition = updateData.position;
            transform.localRotation = updateData.rotation;
            transform.localScale = updateData.scale;
        }
    }
}
