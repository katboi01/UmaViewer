﻿using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineGazingObjectData : ILiveTimelineGroupDataWithName
    {
        public enum Axis
        {
            X,
            Y,
            Z,
            X_Negative,
            Y_Negative,
            Z_Negative
        }

        private const string default_name = "Gazing Object";

        public LiveTimelineKeyGazingObjectDataList keys = new LiveTimelineKeyGazingObjectDataList();

        public LiveTimelineGazingObjectData.Axis forwardAxis = Axis.Z;

        private static readonly Quaternion[] forwardAxisFixRotations;

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }

        static LiveTimelineGazingObjectData()
        {
            forwardAxisFixRotations = new Quaternion[6]
            {
            Quaternion.FromToRotation(Vector3.forward, Vector3.left),
            Quaternion.FromToRotation(Vector3.forward, Vector3.down),
            Quaternion.identity,
            Quaternion.FromToRotation(Vector3.forward, Vector3.right),
            Quaternion.FromToRotation(Vector3.forward, Vector3.up),
            Quaternion.FromToRotation(Vector3.forward, Vector3.back)
            };
        }

        public void ApplyFixRotation(ref Quaternion rot)
        {
            rot *= forwardAxisFixRotations[(int)forwardAxis];
        }
    }
}

