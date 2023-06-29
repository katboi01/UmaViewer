using UnityEditor;
using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK
{

    /*
	 * Custom inspector for RotationLimitSpline
	 * */
    [CustomEditor(typeof(RotationLimitSpline))]
    [CanEditMultipleObjects]
    public class RotationLimitSplineInspector : RotationLimitInspector
    {

        // Determines if we are dragging the handle's limits or angle
        public enum ScaleMode
        {
            Limit,
            Angle
        }

        // In Smooth TangentMode, in and out tangents will always be the same, Independent allowes for difference
        public enum TangentMode
        {
            Smooth,
            Independent
        }

        private RotationLimitSpline script { get { return target as RotationLimitSpline; } }
        private RotationLimitSpline clone;
        private ScaleMode scaleMode;
        private TangentMode tangentMode;
        private int selectedHandle = -1, deleteHandle = -1, addHandle = -1;

        #region Inspector

        public void OnEnable()
        {
            // Check if RotationLimitspline is properly set up, if not, reset to defaults
            if (script.spline == null)
            {
                script.spline = new AnimationCurve(defaultKeys);
                EditorUtility.SetDirty(script);
            }
            else if (script.spline.keys.Length < 4)
            {
                script.spline.keys = defaultKeys;
                EditorUtility.SetDirty(script);
            }
        }

        /*
		 * Returns the default keyframes for the RotationLimitspline
		 * */
        private static Keyframe[] defaultKeys
        {
            get
            {
                Keyframe[] k = new Keyframe[5];
                // Values for a simple elliptic spline
                k[0].time = 0.01f;
                k[0].value = 30;
                k[0].inTangent = 0.01f;
                k[0].outTangent = 0.01f;
                k[1].time = 90;
                k[1].value = 45;
                k[1].inTangent = 0.01f;
                k[1].outTangent = 0.01f;
                k[2].time = 180;
                k[2].value = 30;
                k[2].inTangent = 0.01f;
                k[2].outTangent = 0.01f;
                k[3].time = 270;
                k[3].value = 45;
                k[3].inTangent = 0.01f;
                k[3].outTangent = 0.01f;
                k[4].time = 360;
                k[4].value = 30;
                k[4].inTangent = 0.01f;
                k[4].outTangent = 0.01f;
                return k;
            }
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            DrawDefaultInspector();

            script.twistLimit = Mathf.Clamp(script.twistLimit, 0, 180);

            if (GUI.changed) EditorUtility.SetDirty(script);
        }

        /*
		 * Make sure the keyframes and tangents are valid
		 * */
        private void ValidateKeyframes(Keyframe[] keys)
        {
            keys[keys.Length - 1].value = keys[0].value;
            keys[keys.Length - 1].time = keys[0].time + 360;
            keys[keys.Length - 1].inTangent = keys[0].inTangent;
        }

        #endregion Inspector

        #region Scene

        void OnSceneGUI()
        {
            GUI.changed = false;
            // Get the keyframes of the AnimationCurve to be manipulated
            Keyframe[] keys = script.spline.keys;

            // Set defaultLocalRotation so that the initial local rotation will be the zero point for the rotation limit
            if (!Application.isPlaying && !script.defaultLocalRotationOverride) script.defaultLocalRotation = script.transform.localRotation;
            if (script.axis == Vector3.zero) return;

            // Make the curve loop
            script.spline.postWrapMode = WrapMode.Loop;
            script.spline.preWrapMode = WrapMode.Loop;

            DrawRotationSphere(script.transform.position);

            // Display the main axis
            DrawArrow(script.transform.position, Direction(script.axis), colorDefault, "Axis", 0.02f);

            Vector3 swing = script.axis.normalized;

            // Editing tools GUI
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, 10, 440, 100), "Rotation Limit Spline", "Window");

            // Scale Mode and Tangent Mode
            GUILayout.BeginHorizontal();
            scaleMode = (ScaleMode)EditorGUILayout.EnumPopup("Drag Handle", scaleMode);
            tangentMode = (TangentMode)EditorGUILayout.EnumPopup("Drag Tangents", tangentMode);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (Inspector.Button("Rotate 90 degrees", "Rotate rotation limit around axis.", script, GUILayout.Width(220)))
            {
                if (!Application.isPlaying) Undo.RecordObject(script, "Handle Value");
                for (int i = 0; i < keys.Length; i++) keys[i].time += 90;
            }

            // Cloning values from another RotationLimitSpline
            EditorGUILayout.BeginHorizontal();
            if (Inspector.Button("Clone From", "Make this rotation limit identical to another", script, GUILayout.Width(220)))
            {
                CloneLimit();
                keys = script.spline.keys;
            }
            clone = (RotationLimitSpline)EditorGUILayout.ObjectField("", clone, typeof(RotationLimitSpline), true);
            EditorGUILayout.EndHorizontal();

            GUILayout.EndArea();
            Handles.EndGUI();

            // Draw keyframes
            for (int i = 0; i < keys.Length - 1; i++)
            {
                float angle = keys[i].time;

                // Start drawing handles
                Quaternion offset = Quaternion.AngleAxis(angle, swing);
                Quaternion rotation = Quaternion.AngleAxis(keys[i].value, offset * script.crossAxis);
                Vector3 position = script.transform.position + Direction(rotation * swing);
                Handles.Label(position, "  " + i.ToString());

                // Dragging Values
                if (selectedHandle == i)
                {
                    Handles.color = colorHandles;
                    switch (scaleMode)
                    {
                        case ScaleMode.Limit:
                            float inputValue = keys[i].value;
                            inputValue = Mathf.Clamp(Inspector.ScaleValueHandleSphere(inputValue, position, Quaternion.identity, 0.5f, 0), 0.01f, 180);
                            if (keys[i].value != inputValue)
                            {
                                if (!Application.isPlaying) Undo.RecordObject(script, "Handle Value");
                                keys[i].value = inputValue;
                            }
                            break;
                        case ScaleMode.Angle:
                            float inputTime = keys[i].time;
                            inputTime = Inspector.ScaleValueHandleSphere(inputTime, position, Quaternion.identity, 0.5f, 0);
                            if (keys[i].time != inputTime)
                            {
                                if (!Application.isPlaying) Undo.RecordObject(script, "Handle Angle");
                                keys[i].time = inputTime;
                            }
                            break;
                    }
                }

                // Handle select button
                if (selectedHandle != i)
                {
                    Handles.color = Color.blue;
                    if (Inspector.SphereButton(position, script.transform.rotation, 0.05f, 0.05f))
                    {
                        selectedHandle = i;
                    }
                }

                // Tangents
                if (selectedHandle == i)
                {
                    // Evaluate positions before and after the key to get the tangent positions
                    Vector3 prevPosition = GetAnglePosition(keys[i].time - 1);
                    Vector3 nextPosition = GetAnglePosition(keys[i].time + 1);

                    // Draw handles for the tangents
                    Handles.color = Color.white;
                    Vector3 toNext = (nextPosition - position).normalized * 0.3f;
                    float outTangent = keys[i].outTangent;
                    outTangent = Inspector.ScaleValueHandleSphere(outTangent, position + toNext, Quaternion.identity, 0.2f, 0);

                    Vector3 toPrev = (prevPosition - position).normalized * 0.3f;
                    float inTangent = keys[i].inTangent;
                    inTangent = Inspector.ScaleValueHandleSphere(inTangent, position + toPrev, Quaternion.identity, 0.2f, 0);

                    if (outTangent != keys[i].outTangent || inTangent != keys[i].inTangent) selectedHandle = i;

                    // Make the other tangent match the dragged tangent (if in "Smooth" TangentMode)
                    switch (tangentMode)
                    {
                        case TangentMode.Smooth:
                            if (outTangent != keys[i].outTangent)
                            {
                                if (!Application.isPlaying) Undo.RecordObject(script, "Tangents");
                                keys[i].outTangent = outTangent;
                                keys[i].inTangent = outTangent;
                            }
                            else if (inTangent != keys[i].inTangent)
                            {
                                if (!Application.isPlaying) Undo.RecordObject(script, "Tangents");
                                keys[i].outTangent = inTangent;
                                keys[i].inTangent = inTangent;
                            }
                            break;
                        case TangentMode.Independent:
                            if (outTangent != keys[i].outTangent)
                            {
                                if (!Application.isPlaying) Undo.RecordObject(script, "Tangents");
                                keys[i].outTangent = outTangent;
                            }
                            else if (inTangent != keys[i].inTangent)
                            {
                                if (!Application.isPlaying) Undo.RecordObject(script, "Tangents");
                                keys[i].inTangent = inTangent;
                            }
                            break;
                    }

                    // Draw lines and labels to tangent handles
                    Handles.color = Color.white;
                    GUI.color = Color.white;

                    Handles.DrawLine(position, position + toNext);
                    Handles.Label(position + toNext, " Out");

                    Handles.DrawLine(position, position + toPrev);
                    Handles.Label(position + toPrev, " In");
                }
            }

            // Selected Point GUI
            if (selectedHandle != -1)
            {
                Handles.BeginGUI();
                GUILayout.BeginArea(new Rect(Screen.width - 240, Screen.height - 200, 230, 150), "Handle " + selectedHandle.ToString(), "Window");

                if (Inspector.Button("Delete", "Delete this handle", script))
                {
                    if (keys.Length > 4)
                    {
                        deleteHandle = selectedHandle;
                    }
                    else if (!Warning.logged) script.LogWarning("Spline Rotation Limit should have at least 3 handles");
                }
                if (Inspector.Button("Add Handle", "Add a new handle next to this one", script))
                {
                    addHandle = selectedHandle;
                }

                // Clamp the key angles to previous and next handle angles
                float prevTime = 0, nextTime = 0;
                if (selectedHandle < keys.Length - 2) nextTime = keys[selectedHandle + 1].time;
                else nextTime = keys[0].time + 360;

                if (selectedHandle == 0) prevTime = keys[keys.Length - 2].time - 360;
                else prevTime = keys[selectedHandle - 1].time;

                // Angles
                float inputTime = keys[selectedHandle].time;
                inputTime = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Angle", "Angle of the point (0-360)."), inputTime), prevTime, nextTime);

                if (keys[selectedHandle].time != inputTime)
                {
                    if (!Application.isPlaying) Undo.RecordObject(script, "Handle Angle");
                    keys[selectedHandle].time = inputTime;
                }

                // Limits
                float inputValue = keys[selectedHandle].value;
                inputValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Limit", "Max angular limit from Axis at this angle"), inputValue), 0, 180);
                if (keys[selectedHandle].value != inputValue)
                {
                    if (!Application.isPlaying) Undo.RecordObject(script, "Handle Limit");
                    keys[selectedHandle].value = inputValue;
                }

                // In Tangents
                float inputInTangent = keys[selectedHandle].inTangent;
                inputInTangent = EditorGUILayout.FloatField(new GUIContent("In Tangent", "In tangent of the handle point on the spline"), inputInTangent);
                if (keys[selectedHandle].inTangent != inputInTangent)
                {
                    if (!Application.isPlaying) Undo.RecordObject(script, "Handle In Tangent");
                    keys[selectedHandle].inTangent = inputInTangent;
                }

                // Out tangents
                float inputOutTangent = keys[selectedHandle].outTangent;
                inputOutTangent = EditorGUILayout.FloatField(new GUIContent("Out Tangent", "Out tangent of the handle point on the spline"), inputOutTangent);
                if (keys[selectedHandle].outTangent != inputOutTangent)
                {
                    if (!Application.isPlaying) Undo.RecordObject(script, "Handle Out Tangent");
                    keys[selectedHandle].outTangent = inputOutTangent;
                }

                GUILayout.EndArea();
                Handles.EndGUI();
            }

            // Make sure the keyframes are valid;
            ValidateKeyframes(keys);

            // Replace the AnimationCurve keyframes with the manipulated keyframes
            script.spline.keys = keys;

            // Display limits
            for (int i = 0; i < 360; i += 2)
            {
                float evaluatedLimit = script.spline.Evaluate((float)i);
                Quaternion offset = Quaternion.AngleAxis(i, swing);
                Quaternion evaluatedRotation = Quaternion.AngleAxis(evaluatedLimit, offset * script.crossAxis);
                Quaternion testRotation = Quaternion.AngleAxis(179.9f, offset * script.crossAxis);

                Quaternion limitedRotation = script.LimitSwing(testRotation);

                Vector3 evaluatedDirection = evaluatedRotation * swing;
                Vector3 limitedDirection = limitedRotation * swing;

                // Display the limit points in red if they are out of range
                bool isValid = Vector3.Distance(evaluatedDirection, limitedDirection) < 0.01f && evaluatedLimit >= 0;
                Color color = isValid ? colorDefaultTransparent : colorInvalid;

                Vector3 limitPoint = script.transform.position + Direction(evaluatedDirection);

                Handles.color = color;
                if (i == 0) zeroPoint = limitPoint;

                Handles.DrawLine(script.transform.position, limitPoint);

                if (i > 0)
                {
                    Handles.color = isValid ? colorDefault : colorInvalid;
                    Handles.DrawLine(limitPoint, lastPoint);
                    if (i == 358) Handles.DrawLine(limitPoint, zeroPoint);
                }

                lastPoint = limitPoint;
            }

            // Deleting points
            if (deleteHandle != -1)
            {
                DeleteHandle(deleteHandle);
                selectedHandle = -1;
                deleteHandle = -1;
            }

            // Adding points
            if (addHandle != -1)
            {
                AddHandle(addHandle);
                addHandle = -1;
            }

            Handles.color = Color.white;
            if (GUI.changed) EditorUtility.SetDirty(script);
        }

        private Vector3 lastPoint, zeroPoint;

        /*
		 * Return the evaluated position for the specified angle
		 * */
        private Vector3 GetAnglePosition(float angle)
        {
            Vector3 swing = script.axis.normalized;
            Quaternion offset = Quaternion.AngleAxis(angle, swing);
            Quaternion rotation = Quaternion.AngleAxis(script.spline.Evaluate(angle), offset * script.crossAxis);
            return script.transform.position + Direction(rotation * swing);
        }

        /*
		 * Converting directions from local space to world space
		 * */
        private Vector3 Direction(Vector3 v)
        {
            if (script.transform.parent == null) return script.defaultLocalRotation * v;
            return script.transform.parent.rotation * (script.defaultLocalRotation * v);
        }

        /*
		 * Removing Handles
		 * */
        private void DeleteHandle(int p)
        {
            Keyframe[] keys = script.spline.keys;
            Keyframe[] newKeys = new Keyframe[0];

            for (int i = 0; i < keys.Length; i++)
            {
                if (i != p)
                {
                    Array.Resize(ref newKeys, newKeys.Length + 1);
                    newKeys[newKeys.Length - 1] = keys[i];
                }
            }

            script.spline.keys = newKeys;
        }

        /*
		 * Creating new Handles
		 * */
        private void AddHandle(int p)
        {
            Keyframe[] keys = script.spline.keys;
            Keyframe[] newKeys = new Keyframe[keys.Length + 1];

            for (int i = 0; i < p + 1; i++) newKeys[i] = keys[i];

            float nextTime = 0;
            if (p < keys.Length - 1) nextTime = keys[p + 1].time;
            else nextTime = keys[0].time;

            float newTime = Mathf.Lerp(keys[p].time, nextTime, 0.5f);
            float newValue = script.spline.Evaluate(newTime);

            newKeys[p + 1] = new Keyframe(newTime, newValue);

            for (int i = p + 2; i < newKeys.Length; i++) newKeys[i] = keys[i - 1];

            script.spline.keys = newKeys;
        }

        /*
		 * Clone properties from another RotationLimitSpline
		 * */
        private void CloneLimit()
        {
            if (clone == null) return;
            if (clone == script)
            {
                script.LogWarning("Can't clone from self.");
                return;
            }

            script.axis = clone.axis;
            script.twistLimit = clone.twistLimit;
            script.spline.keys = clone.spline.keys;
        }

        #endregion Scene
    }
}