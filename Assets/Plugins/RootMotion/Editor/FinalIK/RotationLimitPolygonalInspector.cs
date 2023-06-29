using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace RootMotion.FinalIK
{

    /*
	 * Custom inspector for RotationLimitPolygonal
	 * */
    [CustomEditor(typeof(RotationLimitPolygonal))]
    [CanEditMultipleObjects]
    public class RotationLimitPolygonalInspector : RotationLimitInspector
    {

        /*
		 * Used for quick symmetric editing in the scene
		 * */
        public enum Symmetry
        {
            Off,
            X,
            Y,
            Z
        }

        private RotationLimitPolygonal script { get { return target as RotationLimitPolygonal; } }
        private RotationLimitPolygonal clone;
        private int selectedPoint = -1, deletePoint = -1, addPoint = -1;
        private float degrees = 90;
        private Symmetry symmetry;

        #region Inspector

        public void OnEnable()
        {
            // If initialized, set up the default polygon
            if (script.points == null || (script.points != null && script.points.Length < 3))
            {
                script.ResetToDefault();
                EditorUtility.SetDirty(script);
            }
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            // Clamping values
            script.twistLimit = Mathf.Clamp(script.twistLimit, 0, 180);
            script.smoothIterations = Mathf.Clamp(script.smoothIterations, 0, 3);

            DrawDefaultInspector();

            if (GUI.changed) EditorUtility.SetDirty(script);
        }

        #endregion Inspector

        #region Scene

        public void OnSceneGUI()
        {
            GUI.changed = false;

            // Set defaultLocalRotation so that the initial local rotation will be the zero point for the rotation limit
            if (!Application.isPlaying && !script.defaultLocalRotationOverride) script.defaultLocalRotation = script.transform.localRotation;
            if (script.axis == Vector3.zero) return;

            // Quick Editing Tools
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, 10, 550, 140), "Rotation Limit Polygonal", "Window");

            // Cloning values from another RotationLimitPolygonal
            EditorGUILayout.BeginHorizontal();
            if (Inspector.Button("Clone From", "Make this rotation limit identical to another", script, GUILayout.Width(220))) CloneLimit();
            clone = (RotationLimitPolygonal)EditorGUILayout.ObjectField("", clone, typeof(RotationLimitPolygonal), true);
            EditorGUILayout.EndHorizontal();

            // Symmetry
            symmetry = (Symmetry)EditorGUILayout.EnumPopup("Symmetry", symmetry, GUILayout.Width(220));

            // Flipping
            EditorGUILayout.BeginHorizontal();
            if (Inspector.Button("Flip X", "Flip points along local X axis", script, GUILayout.Width(100))) FlipLimit(0);
            if (Inspector.Button("Flip Y", "Flip points along local Y axis", script, GUILayout.Width(100))) FlipLimit(1);
            if (Inspector.Button("Flip Z", "Flip points along local Z axis", script, GUILayout.Width(100))) FlipLimit(2);
            GUILayout.Label("Flip everything along axis");
            EditorGUILayout.EndHorizontal();

            // Rotating
            EditorGUILayout.BeginHorizontal();
            if (Inspector.Button("Rotate X", "Rotate points along X axis by Degrees", script, GUILayout.Width(100))) RotatePoints(degrees, Vector3.right);
            if (Inspector.Button("Rotate Y", "Rotate points along Y axis by Degrees", script, GUILayout.Width(100))) RotatePoints(degrees, Vector3.up);
            if (Inspector.Button("Rotate Z", "Rotate points along Z axis by Degrees", script, GUILayout.Width(100))) RotatePoints(degrees, Vector3.forward);

            degrees = EditorGUILayout.FloatField("Degrees", degrees, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            // Smooth/Optimize
            EditorGUILayout.BeginHorizontal();
            if (Inspector.Button("Smooth", "Double the points", script)) Smooth();
            if (Inspector.Button("Optimize", "Delete every second point", script)) Optimize();
            EditorGUILayout.EndHorizontal();

            GUILayout.EndArea();
            Handles.EndGUI();

            // Rebuild reach cones
            script.BuildReachCones();

            // Draw a white transparent sphere
            DrawRotationSphere(script.transform.position);

            // Draw Axis
            DrawArrow(script.transform.position, Direction(script.axis), colorDefault, "Axis", 0.02f);

            // Display limit points
            for (int i = 0; i < script.points.Length; i++)
            {
                Color color = GetColor(i); // Paint the point in green or red if it belongs to an invalid reach cone

                Handles.color = color;
                GUI.color = color;

                // Line from the center to the point and the label
                Handles.DrawLine(script.transform.position, script.transform.position + Direction(script.points[i].point));
                Handles.Label(script.transform.position + Direction(script.points[i].point + new Vector3(-0.02f, 0, 0)), " " + i.ToString());

                // Selecting points
                Handles.color = colorHandles;
                if (Inspector.DotButton(script.transform.position + Direction(script.points[i].point), script.transform.rotation, 0.02f, 0.02f))
                {
                    selectedPoint = i;
                }

                Handles.color = Color.white;
                GUI.color = Color.white;

                // Limit point GUI
                if (i == selectedPoint)
                {
                    Handles.BeginGUI();

                    GUILayout.BeginArea(new Rect(Screen.width - 240, Screen.height - 180, 230, 130), "Limit Point " + i.ToString(), "Window");

                    if (Inspector.Button("Delete", "Delete this point", script))
                    {
                        if (script.points.Length > 3)
                        {
                            // Using the deletePoint index here because we dont want to delete points from the array that we are iterating
                            deletePoint = i;
                        }
                        else if (!Warning.logged) script.LogWarning("Polygonal Rotation Limit should have at least 3 limit points");
                    }
                    if (Inspector.Button("Add Point", "Add a new point next to this one", script))
                    {
                        addPoint = i;
                    }

                    // Store point for undo
                    Vector3 oldPoint = script.points[i].point;

                    // Manual input for the point position
                    Inspector.AddVector3(ref script.points[i].point, "Point", script, GUILayout.Width(210));

                    EditorGUILayout.Space();

                    // Tangent weight
                    Inspector.AddFloat(ref script.points[i].tangentWeight, "Tangent Weight", "Weight of this point's tangent. Used in smoothing.", script, -Mathf.Infinity, Mathf.Infinity, GUILayout.Width(150));

                    GUILayout.EndArea();

                    Handles.EndGUI();

                    // Moving Points
                    Vector3 pointWorld = Handles.PositionHandle(script.transform.position + Direction(script.points[i].point), Quaternion.identity);
                    Vector3 newPoint = InverseDirection(pointWorld - script.transform.position);
                    if (newPoint != script.points[i].point)
                    {
                        if (!Application.isPlaying) Undo.RecordObject(script, "Move Limit Point");
                        script.points[i].point = newPoint;
                    }

                    // Symmetry
                    if (symmetry != Symmetry.Off && script.points.Length > 3 && oldPoint != script.points[i].point)
                    {
                        RotationLimitPolygonal.LimitPoint symmetryPoint = GetClosestPoint(Symmetrize(oldPoint, symmetry));
                        if (symmetryPoint != script.points[i])
                        {
                            symmetryPoint.point = Symmetrize(script.points[i].point, symmetry);
                        }
                    }
                }

                // Normalize the point
                script.points[i].point = script.points[i].point.normalized;
            }

            // Display smoothed polygon
            for (int i = 0; i < script.P.Length; i++)
            {
                Color color = GetColor(i);

                // Smoothed triangles are transparent
                Handles.color = new Color(color.r, color.g, color.b, 0.25f);
                Handles.DrawLine(script.transform.position, script.transform.position + Direction(script.P[i]));

                Handles.color = color;

                if (i < script.P.Length - 1) Handles.DrawLine(script.transform.position + Direction(script.P[i]), script.transform.position + Direction(script.P[i + 1]));
                else Handles.DrawLine(script.transform.position + Direction(script.P[i]), script.transform.position + Direction(script.P[0]));

                Handles.color = Color.white;
            }

            // Deleting points
            if (deletePoint != -1)
            {
                DeletePoint(deletePoint);
                selectedPoint = -1;
                deletePoint = -1;
            }

            // Adding points
            if (addPoint != -1)
            {
                AddPoint(addPoint);
                addPoint = -1;
            }

            if (GUI.changed) EditorUtility.SetDirty(script);
        }

        private Color GetColor(int i)
        {
            // Paint the polygon in red if the reach cone is invalid
            return script.reachCones[i].isValid ? colorDefault : colorInvalid;
        }

        /*
		 * Doubles the number of Limit Points
		 * */
        private void Smooth()
        {
            int length = script.points.Length;
            for (int i = 0; i < length; i++)
            {
                AddPoint(i + i);
            }
        }

        /*
		 * Reduces the number of Limit Points
		 * */
        private void Optimize()
        {
            for (int i = 1; i < script.points.Length; i++)
            {
                if (script.points.Length > 3) DeletePoint(i);
            }
        }

        /*
		 * Flips the rotation limit along the axis
		 * */
        private void FlipLimit(int axis)
        {
            script.axis[axis] = -script.axis[axis];

            foreach (RotationLimitPolygonal.LimitPoint limitPoint in script.points) limitPoint.point[axis] = -limitPoint.point[axis];
            Array.Reverse(script.points);
            script.BuildReachCones();
        }

        private void RotatePoints(float degrees, Vector3 axis)
        {
            foreach (RotationLimitPolygonal.LimitPoint limitPoint in script.points) limitPoint.point = Quaternion.AngleAxis(degrees, axis) * limitPoint.point;
            script.BuildReachCones();
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
		 * Inverse of Direction(Vector3 v)
		 * */
        private Vector3 InverseDirection(Vector3 v)
        {
            if (script.transform.parent == null) return Quaternion.Inverse(script.defaultLocalRotation) * v;
            return Quaternion.Inverse(script.defaultLocalRotation) * Quaternion.Inverse(script.transform.parent.rotation) * v;
        }

        /*
		 * Removing Limit Points
		 * */
        private void DeletePoint(int p)
        {
            RotationLimitPolygonal.LimitPoint[] newPoints = new RotationLimitPolygonal.LimitPoint[0];

            for (int i = 0; i < script.points.Length; i++)
            {
                if (i != p)
                {
                    Array.Resize(ref newPoints, newPoints.Length + 1);
                    newPoints[newPoints.Length - 1] = script.points[i];
                }
            }

            script.points = newPoints;
            script.BuildReachCones();
        }

        /*
		 * Creating new Limit Points
		 * */
        private void AddPoint(int p)
        {
            RotationLimitPolygonal.LimitPoint[] newPoints = new RotationLimitPolygonal.LimitPoint[script.points.Length + 1];

            for (int i = 0; i < p + 1; i++) newPoints[i] = script.points[i];

            newPoints[p + 1] = new RotationLimitPolygonal.LimitPoint();

            Vector3 nextPoint = Vector3.forward;
            if (p < script.points.Length - 1) nextPoint = script.points[p + 1].point;
            else nextPoint = script.points[0].point;

            newPoints[p + 1].point = Vector3.Lerp(script.points[p].point, nextPoint, 0.5f);

            for (int i = p + 2; i < newPoints.Length; i++) newPoints[i] = script.points[i - 1];

            script.points = newPoints;
            script.BuildReachCones();
        }

        /*
		 * Clone properties from another RotationLimitPolygonal
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
            script.smoothIterations = clone.smoothIterations;
            script.points = new RotationLimitPolygonal.LimitPoint[clone.points.Length];
            for (int i = 0; i < script.points.Length; i++)
            {
                script.points[i] = (RotationLimitPolygonal.LimitPoint)CloneObject(clone.points[i]);
            }
            script.BuildReachCones();
        }

        private static object CloneObject(object o)
        {
            Type t = o.GetType();
            object clone = Activator.CreateInstance(t);
            foreach (FieldInfo fi in t.GetFields())
            {
                fi.SetValue(clone, fi.GetValue(o));
            }
            return clone;
        }

        /*
		 * Flipping vectors for symmetry
		 * */
        private static Vector3 Symmetrize(Vector3 v, Symmetry symmetry)
        {
            switch (symmetry)
            {
                case Symmetry.X: return new Vector3(-v.x, v.y, v.z);
                case Symmetry.Y: return new Vector3(v.x, -v.y, v.z);
                case Symmetry.Z: return new Vector3(v.x, v.y, -v.z);
                default: return v;
            }
        }

        /*
		 * Returns closest point to a position. Used for symmetric editing
		 * */
        private RotationLimitPolygonal.LimitPoint GetClosestPoint(Vector3 v)
        {
            float closestDistace = Mathf.Infinity;
            RotationLimitPolygonal.LimitPoint closestPoint = null;

            foreach (RotationLimitPolygonal.LimitPoint limitPoint in script.points)
            {
                if (limitPoint.point == v) return limitPoint;

                float d = Vector3.Distance(limitPoint.point, v);
                if (d < closestDistace)
                {
                    closestPoint = limitPoint;
                    closestDistace = d;
                }
            }

            return closestPoint;
        }

        #endregion Scene
    }
}