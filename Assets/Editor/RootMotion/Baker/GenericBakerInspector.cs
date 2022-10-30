using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion
{
    [CustomEditor(typeof(GenericBaker))]
    public class GenericBakerInspector : BakerInspector
    {
        private GenericBaker script { get { return target as GenericBaker; } }

        private MonoScript monoScript;

        void OnEnable()
        {
            // Changing the script execution order
            if (!Application.isPlaying)
            {
                monoScript = MonoScript.FromMonoBehaviour(script);
                int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
                if (currentExecutionOrder != 15001) MonoImporter.SetExecutionOrder(monoScript, 15001);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawKeyframeSettings(script as Baker);
            DrawGenericKeyframeSettings(script);
            DrawModeSettings(script as Baker);
            DrawButtons(script as Baker);

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(script);
            }
        }

        private void DrawGenericKeyframeSettings(GenericBaker script)
        {
            if (script.isBaking) return;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("markAsLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("root"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rootNode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ignoreList"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bakePositionList"), true);
        }
    }
}

