using UnityEngine;

namespace LibMMD.Model
{
    public class MMDJoint
    {
        public MMDJoint()
        {
            AssociatedRigidBodyIndex = new int[2];
        }

        public string Name { get; set; }
        public string NameEn { get; set; }
        public int[] AssociatedRigidBodyIndex { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 PositionLowLimit { get; set; }
        public Vector3 PositionHiLimit { get; set; }
        public Vector3 RotationLowLimit { get; set; }
        public Vector3 RotationHiLimit { get; set; }
        public Vector3 SpringTranslate { get; set; }
        public Vector3 SpringRotate { get; set; }
    }
}