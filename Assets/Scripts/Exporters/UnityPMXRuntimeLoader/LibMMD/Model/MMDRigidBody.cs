using UnityEngine;

namespace LibMMD.Model
{
    public class MMDRigidBody
    {
        public enum RigidBodyShape : byte
        {
            RigidShapeSphere = 0x00,
            RigidShapeBox = 0x01,
            RigidShapeCapsule = 0x02
        }

        public enum RigidBodyType : byte
        {
            RigidTypeKinematic = 0x00,
            RigidTypePhysics = 0x01,
            RigidTypePhysicsStrict = 0x02,
            RigidTypePhysicsGhost = 0x03
        }

        public string Name { get; set; }
        public string NameEn { get; set; }
        public int AssociatedBoneIndex { get; set; }
        public int CollisionGroup { get; set; }
        public ushort CollisionMask { get; set; }
        public RigidBodyShape Shape { get; set; }
        public Vector3 Dimemsions { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public float Mass { get; set; }
        public float TranslateDamp { get; set; }
        public float RotateDamp { get; set; }
        public float Restitution { get; set; }
        public float Friction { get; set; }
        public RigidBodyType Type { get; set; }
    }
}