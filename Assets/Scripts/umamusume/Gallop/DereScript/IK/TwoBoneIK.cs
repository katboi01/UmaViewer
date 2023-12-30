using UnityEngine;

namespace IK
{
    public class TwoBoneIK
    {
        private class JointInfo
        {
            public Transform transform;

            public float distance;
        }

        public enum EffectorRotationType
        {
            None,
            World,
            Local
        }

        private const int JointNum = 3;

        private const int Root = 0;

        private const int Center = 1;

        private const int EndEffector = 2;

        private JointInfo[] _joint;

        public Vector3 TargetPosition { get; set; }

        public Vector3 JointTargetPosition { get; set; }

        public Transform EndEffectorTransform => _joint[EndEffector].transform;

        public float BlendAlpha { get; set; }

        public Quaternion EndEffectorRotation { get; set; }

        public EffectorRotationType EndEffectorRotationType { get; set; }

        public void Initialize(Transform endEffector)
        {
            Transform transform = endEffector;
            _joint = new JointInfo[JointNum];

            _joint[EndEffector] = new JointInfo();
            _joint[EndEffector].transform = transform;
            transform = transform.parent;

            _joint[Center] = new JointInfo();
            _joint[Center].transform = transform;
            transform = transform.parent;

            _joint[Root] = new JointInfo();
            _joint[Root].transform = transform;
            for (int i = 0; i < 2; i++)
            {
                _joint[i].distance = Vector3.Distance(_joint[i].transform.position, _joint[i + 1].transform.position);
            }
        }

        public void Solve()
        {
            float blendAlpha = BlendAlpha;
            if (blendAlpha <= 0f)
            {
                return;
            }
            Vector3 positionRoot = _joint[Root].transform.position;
            Vector3 positionCenter = _joint[Center].transform.position;
            Vector3 positionEndEffector = _joint[EndEffector].transform.position;
            float distanceRoot = _joint[Root].distance;
            float distanceCenter = _joint[Center].distance;

            Vector3 vector = TargetPosition - positionRoot;
            float num = vector.magnitude;
            if (num < 1E-06f)
            {
                num = 1E-06f;
                vector = Helper.AxisUp;
            }
            else
            {
                vector /= num;
            }
            Vector3 vector2 = JointTargetPosition - positionRoot;
            Vector3 axis;
            if (vector2.sqrMagnitude < 1E-12f)
            {
                axis = Helper.AxisUp;
            }
            else
            {
                Vector3 axis2 = Vector3.Cross(vector, vector2);
                if (axis2.sqrMagnitude < 1E-12f)
                {
                    Helper.FindBestAxisVectors(axis2, out axis2, out axis);
                }
                else
                {
                    axis = Vector3.Normalize(vector2 - vector * Vector3.Dot(vector2, vector));
                }
            }
            Vector3 vector3 = TargetPosition;
            Vector3 vector4 = positionCenter;
            float num2 = distanceRoot + distanceCenter;
            if (num > num2)
            {
                vector3 = positionRoot + num2 * vector;
                vector4 = positionRoot + distanceRoot * vector;
            }
            else
            {
                float num3 = 2f * distanceRoot * num;
                float num4 = ((num3 != 0f) ? ((distanceRoot * distanceRoot + num * num - distanceCenter * distanceCenter) / num3) : 0f);
                if (num4 > 1f || num4 < -1f)
                {
                    if (distanceRoot > distanceCenter)
                    {
                        vector4 = positionRoot + distanceRoot * vector;
                        vector3 = vector4 - distanceCenter * vector;
                    }
                    else
                    {
                        vector4 = positionRoot - distanceRoot * vector;
                        vector3 = vector4 + distanceCenter * vector;
                    }
                }
                else
                {
                    float f = Mathf.Acos(num4);
                    bool num5 = num4 < 0f;
                    float num6 = distanceRoot * Mathf.Sin(f);
                    float num7 = distanceRoot * distanceRoot - num6 * num6;
                    float num8 = ((num7 > 0f) ? Mathf.Sqrt(num7) : 0f);
                    if (num5)
                    {
                        num8 *= -1f;
                    }
                    vector4 = positionRoot + num8 * vector + num6 * axis;
                }
            }
            Vector3 vector5 = Vector3.Normalize(vector4 - positionRoot);
            Vector3 vector6 = Vector3.Normalize(positionCenter - positionRoot);
            if (Vector3.Dot(vector6, vector5) < 0.999999f)
            {
                Quaternion quaternion = Quaternion.Lerp(Helper.QuaternionIdentity, Quaternion.FromToRotation(vector6, vector5), blendAlpha);
                _joint[Root].transform.rotation = quaternion * _joint[Root].transform.rotation;
                positionCenter = _joint[Center].transform.position;
                positionEndEffector = _joint[EndEffector].transform.position;
            }
            Vector3 vector7 = Vector3.Normalize(vector3 - positionCenter);
            Vector3 vector8 = Vector3.Normalize(positionEndEffector - positionCenter);
            if (Vector3.Dot(vector8, vector7) < 0.999999f)
            {
                Quaternion quaternion2 = Quaternion.Lerp(Helper.QuaternionIdentity, Quaternion.FromToRotation(vector8, vector7), blendAlpha);
                _joint[Center].transform.rotation = quaternion2 * _joint[Center].transform.rotation;
            }
            switch (EndEffectorRotationType)
            {
                case EffectorRotationType.Local:
                    _joint[EndEffector].transform.localRotation = Quaternion.Lerp(_joint[EndEffector].transform.localRotation, EndEffectorRotation, blendAlpha);
                    break;
                case EffectorRotationType.World:
                    _joint[EndEffector].transform.rotation = Quaternion.Lerp(_joint[EndEffector].transform.rotation, EndEffectorRotation, blendAlpha);
                    break;
            }
        }
    }
}