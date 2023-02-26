using System;
using UnityEngine;

namespace Stage
{
    public class CharacterFlareCollisionParameter : ScriptableObject
    {
        [Serializable]
        public class BoneCollisionParameter
        {
            [Serializable]
            public class CollisionParameter
            {
                public enum Type
                {
                    Sphere,
                    Box,
                    Capsule
                }

                public enum Direction
                {
                    X_Axis,
                    Y_Axis,
                    Z_Axis
                }

                public Type _type;

                public float _radius;

                public float _height;

                public Vector3 _center;

                public Vector3 _size;

                public Direction _direction;
            }

            public string _targetBoneName;

            public CollisionParameter[] _collisions;
        }

        public BoneCollisionParameter[] _boneCollisionParameters;

        public BoneCollisionParameter[] boneCollisionParameters
        {
            get
            {
                return _boneCollisionParameters;
            }
            set
            {
                _boneCollisionParameters = value;
            }
        }
    }
}
