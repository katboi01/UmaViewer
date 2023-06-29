using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK
{

    /// <summary>
    /// Simple VRIK LOD level controller based on renderer.isVisible and camera distance.
    /// </summary>
    public class VRIKLODController : MonoBehaviour
    {

        public Renderer LODRenderer;
        public float LODDistance = 15f;
        public bool allowCulled = true;

        private VRIK ik;

        void Start()
        {
            ik = GetComponent<VRIK>();
        }

        void Update()
        {
            ik.solver.LOD = GetLODLevel();
        }

        // Setting LOD level to 1 saves approximately 20% of solving time. LOD level 2 means IK is culled, with only root position and rotation updated if locomotion enabled.
        private int GetLODLevel()
        {
            if (allowCulled)
            {
                if (LODRenderer == null) return 0;
                if (!LODRenderer.isVisible) return 2;
            }

            // Set LOD to 1 if too far from camera
            float sqrMag = (ik.transform.position - Camera.main.transform.position).sqrMagnitude;
            if (sqrMag > LODDistance * LODDistance) return 1;

            return 0;
        }
    }
}
