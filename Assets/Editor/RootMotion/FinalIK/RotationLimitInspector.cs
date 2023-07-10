using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK
{

    /*
	 * Base class for all RotationLimitInspector containing common helper methods and drawing instructions
	 * */
    public class RotationLimitInspector : Editor
    {

        #region Public methods

        // Universal color pallettes
        public static Color colorDefault { get { return new Color(0.0f, 1.0f, 1.0f, 1.0f); } }

        public static Color colorDefaultTransparent
        {
            get
            {
                Color d = colorDefault;
                return new Color(d.r, d.g, d.b, 0.2f);
            }
        }

        public static Color colorHandles { get { return new Color(1.0f, 0.5f, 0.25f, 1.0f); } }
        public static Color colorRotationSphere { get { return new Color(1.0f, 1.0f, 1.0f, 0.1f); } }
        public static Color colorInvalid { get { return new Color(1.0f, 0.3f, 0.3f, 1.0f); } }
        public static Color colorValid { get { return new Color(0.2f, 1.0f, 0.2f, 1.0f); } }

        /*
		 * Draws the default rotation limit sphere to the scene
		 * */
        public static void DrawRotationSphere(Vector3 position)
        {
            Handles.color = colorRotationSphere;
            Inspector.SphereCap(0, position, Quaternion.identity, 2.0f);
            Handles.color = Color.white;
        }

        /*
		 * Draws a custom arrow to the scene
		 * */
        public static void DrawArrow(Vector3 position, Vector3 direction, Color color, string label = "", float size = 0.01f)
        {
            Handles.color = color;
            Handles.DrawLine(position, position + direction);
            Inspector.SphereCap(0, position + direction, Quaternion.identity, size);
            Handles.color = Color.white;

            if (label != "")
            {
                GUI.color = color;
                Handles.Label(position + direction, label);
                GUI.color = Color.white;
            }
        }

        /*
		 * Draws a handle for adjusting rotation limits in the scene
		 * */
        public static float DrawLimitHandle(float limit, Vector3 position, Quaternion rotation, float radius, string label, float openingValue)
        {
            limit = Inspector.ScaleValueHandleSphere(limit, position, rotation, radius, 1);

            string labelInfo = label + ": " + limit.ToString();

            // If value is 0, draw a button to 'open' the value, because we cant scale 0
            if (limit == 0)
            {
                labelInfo = "Open " + label;

                if (Inspector.SphereButton(position, rotation, radius * 0.2f, radius * 0.07f))
                {
                    limit = openingValue;
                }
            }

            Handles.Label(position, labelInfo);

            return limit;
        }

        #endregion
    }
}