using UnityEditor;
using UnityEngine;

namespace RootMotion
{

    // Custom drawer for the LargeHeader attribute
    [CustomPropertyDrawer(typeof(InspectorComment))]
    public class InspectorCommentDrawer : DecoratorDrawer
    {
        // Used to calculate the height of the box
        public static Texture2D lineTex = null;
        private GUIStyle style;

        InspectorComment comment { get { return ((InspectorComment)attribute); } }

        // Get the height of the element
        public override float GetHeight()
        {
            style = GetStyle();
            return style.CalcHeight(new GUIContent(comment.name), EditorGUIUtility.currentViewWidth) + 10f;

            //return base.GetHeight() * 1.5f;
        }

        // Override the GUI drawing for this attribute
        public override void OnGUI(Rect pos)
        {
            // Get the color the line should be
            Color color = Color.white;
            switch (comment.color.ToString().ToLower())
            {
                case "white": color = Color.white; break;
                case "red": color = Color.red; break;
                case "blue": color = Color.blue; break;
                case "green": color = Color.green; break;
                case "gray": color = Color.gray; break;
                case "grey": color = Color.grey; break;
                case "black": color = Color.black; break;
            }

            color *= 0.5f;

            style = GetStyle();
            
            GUI.color = color;

            Rect labelRect = pos;
            //labelRect.y += 10;
            EditorGUI.LabelField(labelRect, new GUIContent(comment.name), style);

            GUI.color = Color.white;
        }

        private GUIStyle GetStyle()
        {
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 10;
            style.fontStyle = FontStyle.Normal;
            style.wordWrap = true;
            style.alignment = TextAnchor.LowerLeft;
            return style;
        }
    }
}