using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion.Dynamics
{
    [CustomEditor(typeof(PuppetMasterProp))]
    public class PuppetMasterPropInspector : Editor
    {

        private PuppetMasterProp script { get { return target as PuppetMasterProp; } }

        private GUIStyle style = new GUIStyle();
        private GUIStyle miniLabelStyle = new GUIStyle();
        private static Color pro = new Color(0.5f, 0.7f, 0.3f, 1f);
        private static Color free = new Color(0.2f, 0.3f, 0.1f, 1f);

        public override void OnInspectorGUI()
        {
            if (script == null) return;
            serializedObject.Update();

            style.wordWrap = true;
            style.normal.textColor = EditorGUIUtility.isProSkin ? pro : free;

            miniLabelStyle.wordWrap = true;
            miniLabelStyle.fontSize = 10;
            miniLabelStyle.normal.textColor = EditorStyles.miniLabel.normal.textColor;

            DrawDefaultInspector();

            script.muscleProps.group = Muscle.Group.Prop;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
