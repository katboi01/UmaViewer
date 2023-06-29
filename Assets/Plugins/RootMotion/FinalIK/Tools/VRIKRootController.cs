using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK
{

    public class VRIKRootController : MonoBehaviour
    {

        public Vector3 pelvisTargetRight { get; private set; }

        private Transform pelvisTarget;
        private Transform leftFootTarget;
        private Transform rightFootTarget;
        private VRIK ik;

        void Awake()
        {
            ik = GetComponent<VRIK>();
            ik.solver.OnPreUpdate += OnPreUpdate;
            Calibrate();
        }

        public void Calibrate()
        {
            if (ik == null)
            {
                Debug.LogError("No VRIK found on VRIKRootController's GameObject.", transform);
                return;
            }
            pelvisTarget = ik.solver.spine.pelvisTarget;
            leftFootTarget = ik.solver.leftLeg.target;
            rightFootTarget = ik.solver.rightLeg.target;
            if (pelvisTarget != null) pelvisTargetRight = Quaternion.Inverse(pelvisTarget.rotation) * ik.references.root.right;
        }

        public void Calibrate(VRIKCalibrator.CalibrationData data)
        {
            if (ik == null)
            {
                Debug.LogError("No VRIK found on VRIKRootController's GameObject.", transform);
                return;
            }
            pelvisTarget = ik.solver.spine.pelvisTarget;
            leftFootTarget = ik.solver.leftLeg.target;
            rightFootTarget = ik.solver.rightLeg.target;
            if (pelvisTarget != null)
            {
                pelvisTargetRight = data.pelvisTargetRight;
            }
        }

        void OnPreUpdate()
        {
            if (!enabled) return;

            if (pelvisTarget != null)
            {
                ik.references.root.position = new Vector3(pelvisTarget.position.x, ik.references.root.position.y, pelvisTarget.position.z);

                Vector3 f = Vector3.Cross(pelvisTarget.rotation * pelvisTargetRight, ik.references.root.up);
                f.y = 0f;
                ik.references.root.rotation = Quaternion.LookRotation(f);

                ik.references.pelvis.position = Vector3.Lerp(ik.references.pelvis.position, pelvisTarget.position, ik.solver.spine.pelvisPositionWeight);
                ik.references.pelvis.rotation = Quaternion.Slerp(ik.references.pelvis.rotation, pelvisTarget.rotation, ik.solver.spine.pelvisRotationWeight);
            }
            else if (leftFootTarget != null && rightFootTarget != null)
            {
                ik.references.root.position = Vector3.Lerp(leftFootTarget.position, rightFootTarget.position, 0.5f);
            }
        }

        void OnDestroy()
        {
            if (ik != null) ik.solver.OnPreUpdate -= OnPreUpdate;
        }

    }
}
