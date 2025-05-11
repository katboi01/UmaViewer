using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public class LiveTimelineCharaLocator : ILiveTimelineCharactorLocator
    {
        public UmaContainerCharacter UmaContainer;

        public Dictionary<string, Transform> Bones;

        private LiveCharaPosition _liveCharaStandingPosition;

        public LiveTimelineCharaLocator(UmaContainerCharacter umaContainer)
        {
            UmaContainer = umaContainer;
            Bones = new Dictionary<string, Transform>();
            foreach (var bone in umaContainer.GetComponentsInChildren<Transform>())
            {
                if (!Bones.ContainsKey(bone.name))
                {
                    Bones.Add(bone.name, bone);
                }
            }
            HeadHeght = (Bones["Position"].InverseTransformPoint(Bones["Head"].position).y + 0.1f) * UmaContainer.BodyScale;
            WaistHeght = Bones["Position"].InverseTransformPoint(Bones["Waist"].position).y * UmaContainer.BodyScale;
            ChestHeght = Bones["Position"].InverseTransformPoint(Bones["Chest"].position).y * UmaContainer.BodyScale;
            Debug.Log($"InfoChara {UmaContainer.CharaEntry.Name}: {HeadHeght}, {WaistHeght}, {ChestHeght}");
        }

        float HeadHeght;
        float WaistHeght;
        float ChestHeght;

        public bool liveCharaVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public LiveCharaPosition liveCharaStandingPosition { get => _liveCharaStandingPosition; set => _liveCharaStandingPosition = value; }

        public Vector3 liveCharaInitialPosition { get => _liveCharaInitialPosition; set => _liveCharaInitialPosition = value; }

        private Vector3 _liveCharaInitialPosition = Vector3.zero;

        public Vector3 liveCharaPosition => throw new NotImplementedException();

        public Quaternion liveCharaPositionLocalRotation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Vector3 liveCharaHeadPosition => UmaContainer.HeadBone.transform.position + new Vector3(0, 0.1f, 0);

        public Vector3 liveCharaWaistPosition => Bones["Waist"].position;

        public Vector3 liveCharaLeftHandWristPosition => Bones["Wrist_L"].position;

        public Vector3 liveCharaLeftHandAttachPosition => Bones["Hand_Attach_L"].position; 

        public Vector3 liveCharaRightHandAttachPosition => Bones["Hand_Attach_R"].position;

        public Vector3 liveCharaRightHandWristPosition => Bones["Wrist_R"].position;

        public Vector3 liveCharaChestPosition => Bones["Chest"].position;

        public Vector3 liveCharaFootPosition => new Vector3(liveCharaChestPosition.x, 0, liveCharaChestPosition.z);

        public Vector3 liveCharaConstHeightHeadPosition => new Vector3(liveCharaInitialPosition.x, HeadHeght, liveCharaInitialPosition.z);

        public Vector3 liveCharaConstHeightWaistPosition => new Vector3(liveCharaInitialPosition.x, WaistHeght, liveCharaInitialPosition.z);

        public Vector3 liveCharaConstHeightChestPosition => new Vector3(liveCharaInitialPosition.x, ChestHeght, liveCharaInitialPosition.z);

        public Vector3 liveCharaInitialHeightHeadPosition => new Vector3(liveCharaInitialPosition.x, HeadHeght, liveCharaInitialPosition.z);

        public Vector3 liveCharaInitialHeightWaistPosition => new Vector3(liveCharaInitialPosition.x, WaistHeght, liveCharaInitialPosition.z);

        public Vector3 liveCharaInitialHeightChestPosition => new Vector3(liveCharaInitialPosition.x, ChestHeght, liveCharaInitialPosition.z);

        public Vector3 liveCharaScale => Bones["Position"].localScale;

        public Transform liveParentDefaultTransform { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Transform liveParentTransform { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Transform liveRootTransform => throw new NotImplementedException();

        public int liveCharaHeightLevel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public float liveCharaHeightValue => 160 * Bones["Position"].localScale.y;

        public float liveCharaHeightRatioBase { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float liveCharaHeightRatio { get => 1; set => throw new NotImplementedException(); }
        public Vector3 liveCharaFormationHeightRateOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsPositionNodePositionAddParent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsCastShadow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float CySpringRate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int LayerIndex { set => throw new NotImplementedException(); }
        public Color EmissiveColor { set => throw new NotImplementedException(); }
    }
}
