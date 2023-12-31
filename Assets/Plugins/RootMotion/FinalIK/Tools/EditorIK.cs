using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Updates any Final IK component in Editor mode
    /// </summary>
    [ExecuteInEditMode]
    public class EditorIK : MonoBehaviour
    {
        private IK ik;

        void Start()
        {
            ik = GetComponent<IK>();

            ik.GetIKSolver().Initiate(ik.transform);
        }

        void Update()
        {
            if (ik == null) return;

            if (ik.fixTransforms) ik.GetIKSolver().FixTransforms();

            // Apply animation here if necessary

            ik.GetIKSolver().Update();
        }
    }
}