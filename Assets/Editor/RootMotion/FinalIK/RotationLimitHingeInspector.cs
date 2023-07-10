using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK
{

    /*
	 * Custom inspector for RotationLimitHinge
	 * */
    [CustomEditor(typeof(RotationLimitHinge))]
    [CanEditMultipleObjects]
    public class RotationLimitHingeInspector : RotationLimitInspector
    {

        private RotationLimitHinge script { get { return target as RotationLimitHinge; } }

        #region Inspector

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            // Draw the default inspector
            DrawDefaultInspector();

            if (GUI.changed) EditorUtility.SetDirty(script);
        }

        #endregion Inspector

        #region Scene

        void OnSceneGUI()
        {
            // Set defaultLocalRotation so that the initial local rotation will be the zero point for the rotation limit
            if (!Application.isPlaying && !script.defaultLocalRotationOverride) script.defaultLocalRotation = script.transform.localRotation;
            if (script.axis == Vector3.zero) return;

            // Quick Editing Tools
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, 10, 200, 50), "Rotation Limit Hinge", "Window");

            // Rotating display
            if (GUILayout.Button("Rotate display 90 degrees"))
            {
                if (!Application.isPlaying) Undo.RecordObject(script, "Rotate Display");
                script.zeroAxisDisplayOffset += 90;
                if (script.zeroAxisDisplayOffset >= 360) script.zeroAxisDisplayOffset = 0;
            }

            GUILayout.EndArea();
            Handles.EndGUI();

            // Normalize the main axes
            Vector3 axis = Direction(script.axis.normalized);
            Vector3 cross = Direction(Quaternion.AngleAxis(script.zeroAxisDisplayOffset, script.axis) * script.crossAxis.normalized);

            // Axis vector
            DrawArrow(script.transform.position, axis, colorDefault, "Axis", 0.02f);

            if (script.useLimits)
            {
                // Zero rotation vector
                DrawArrow(script.transform.position, cross * 0.5f, colorDefault, " 0", 0.02f);

                // Arcs for the rotation limit
                Handles.color = colorDefaultTransparent;
                Handles.DrawSolidArc(script.transform.position, axis, cross, script.min, 0.5f);
                Handles.DrawSolidArc(script.transform.position, axis, cross, script.max, 0.5f);
            }

            Handles.color = colorDefault;
            GUI.color = colorDefault;

            Inspector.CircleCap(0, script.transform.position, Quaternion.LookRotation(axis, cross), 0.5f);

            if (!script.useLimits) return;

            // Handles for adjusting rotation limits in the scene
            Quaternion minRotation = Quaternion.AngleAxis(script.min, axis);
            Handles.DrawLine(script.transform.position, script.transform.position + minRotation * cross);

            Quaternion maxRotation = Quaternion.AngleAxis(script.max, axis);
            Handles.DrawLine(script.transform.position, script.transform.position + maxRotation * cross);

            // Undoable scene handles
            float min = script.min;
            min = DrawLimitHandle(min, script.transform.position + minRotation * cross, Quaternion.identity, 0.5f, "Min", -10);
            if (min != script.min)
            {
                if (!Application.isPlaying) Undo.RecordObject(script, "Min Limit");
                script.min = min;
            }

            float max = script.max;
            max = DrawLimitHandle(max, script.transform.position + maxRotation * cross, Quaternion.identity, 0.5f, "Max", 10);
            if (max != script.max)
            {
                if (!Application.isPlaying) Undo.RecordObject(script, "Max Limit");
                script.max = max;
            }

            Handles.color = Color.white;
            GUI.color = Color.white;
        }

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