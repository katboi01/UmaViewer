using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK
{

    /*
	 * Custom inspector for RotationLimitAngle.
	 * */
    [CustomEditor(typeof(RotationLimitAngle))]
    [CanEditMultipleObjects]
    public class RotationLimitAngleInspector : RotationLimitInspector
    {

        private RotationLimitAngle script { get { return target as RotationLimitAngle; } }

        #region Inspector

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            // Draw the default inspector
            DrawDefaultInspector();

            script.limit = Mathf.Clamp(script.limit, 0, 180);

            if (GUI.changed) EditorUtility.SetDirty(script);
        }

        #endregion Inspector

        #region Scene

        void OnSceneGUI()
        {
            // Set defaultLocalRotation so that the initial local rotation will be the zero point for the rotation limit
            if (!Application.isPlaying && !script.defaultLocalRotationOverride) script.defaultLocalRotation = script.transform.localRotation;
            if (script.axis == Vector3.zero) return;

            DrawRotationSphere(script.transform.position);

            // Display the main axis
            DrawArrow(script.transform.position, Direction(script.axis), colorDefault, "Axis", 0.02f);

            Vector3 swing = script.axis.normalized;

            // Display limits
            lastPoint = script.transform.position;

            for (int i = 0; i < 360; i += 2)
            {
                Quaternion offset = Quaternion.AngleAxis(i, swing);
                Quaternion limitedRotation = Quaternion.AngleAxis(script.limit, offset * script.crossAxis);

                Vector3 limitedDirection = Direction(limitedRotation * swing);

                Handles.color = colorDefaultTransparent;

                Vector3 limitPoint = script.transform.position + limitedDirection;

                if (i == 0) zeroPoint = limitPoint;

                Handles.DrawLine(script.transform.position, limitPoint);

                if (i > 0)
                {
                    Handles.color = colorDefault;
                    Handles.DrawLine(limitPoint, lastPoint);
                    if (i == 358) Handles.DrawLine(limitPoint, zeroPoint);
                }

                lastPoint = limitPoint;
            }

            Handles.color = Color.white;
        }

        private Vector3 lastPoint, zeroPoint;

        /*
		 * Converting directions from local space to world space
		 * */
        private Vector3 Direction(Vector3 v)
        {
            if (script.transform.parent == null) return script.defaultLocalRotation * v;
            return script.transform.parent.rotation * (script.defaultLocalRotation * v);
        }

        #endregion Scene
    }
}
