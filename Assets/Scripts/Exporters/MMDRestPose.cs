using UnityEngine;

/// The rest pose a PMX is bound in and a VMD is recorded relative to. ModelExporter and
/// UnityHumanoidVMDRecorder both call Apply, so the two can never disagree.
///
/// Rebind clears every animated bone to the default pose (InitBoneTransform covers only the
/// body-skinned bones, so without it any other bone keeps whatever was last animated), then
/// ResetBodyPose/UpBodyReset put the body bones at their runtime positions, then the upper arms
/// are lowered into the standard MMD A-pose so the result plays on standard models too.
public static class MMDRestPose
{
    /// Standard MMD models hold the upper arm this far below horizontal (Miku: 31 deg).
    public const float ArmDownDegrees = 35f;

    /// Puts the character in the rest pose. Returns whether the body animator was enabled
    /// beforehand; it is left disabled so the next Update cannot re-pose the model.
    public static bool Apply(UmaContainerCharacter container)
    {
        var animator = container.UmaAnimator;
        bool wasEnabled = animator != null && animator.enabled;
        if (animator != null)
        {
            animator.Rebind();
            animator.enabled = false;
        }
        container.ResetBodyPose();
        container.UpBodyReset();
        LowerArm(container.transform, "Arm_L", "Elbow_L");
        LowerArm(container.transform, "Arm_R", "Elbow_R");
        return wasEnabled;
    }

    /// Rotate the upper arm so the Arm->Elbow direction tilts ArmDownDegrees toward world down,
    /// in the plane spanned by the arm and the vertical. Children (elbow, wrist, fingers) follow.
    static void LowerArm(Transform root, string armName, string elbowName)
    {
        Transform arm = null, elbow = null;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == armName) arm = t;
            else if (t.name == elbowName) elbow = t;
        }
        if (arm == null || elbow == null) return;

        Vector3 dir = (elbow.position - arm.position).normalized;
        Vector3 axis = Vector3.Cross(dir, Vector3.down);
        if (axis.sqrMagnitude < 1e-8f) return;   // arm already vertical
        arm.rotation = Quaternion.AngleAxis(ArmDownDegrees, axis.normalized) * arm.rotation;
    }
}
