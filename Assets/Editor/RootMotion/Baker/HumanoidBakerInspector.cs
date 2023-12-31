using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion
{
    [CustomEditor(typeof(HumanoidBaker))]
    public class HumanoidBakerInspector : BakerInspector
    {

        private HumanoidBaker script { get { return target as HumanoidBaker; } }

        private MonoScript monoScript;

        void OnEnable()
        {
            // Changing the script execution order
            if (!Application.isPlaying)
            {
                monoScript = MonoScript.FromMonoBehaviour(script);
                int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
                if (currentExecutionOrder != 15000) MonoImporter.SetExecutionOrder(monoScript, 15000);
            }
        }

        // TODO Move this to BakerInspector.cs
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawKeyframeSettings(script as Baker);
            DrawHumanoidKeyframeSettings(script);
            DrawModeSettings(script as Baker);
            DrawButtons(script as Baker);

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(script);
            }
        }

        protected void DrawHumanoidKeyframeSettings(HumanoidBaker script)
        {
            if (script.isBaking) return;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("IKKeyReductionError"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("muscleFrameRateDiv"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeHandIK"));
        }
    }
}

