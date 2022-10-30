using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion.Dynamics
{
    [CustomEditor(typeof(PropMuscle))]
    public class PropMuscleInspector : Editor
    {

        private PropMuscle script { get { return target as PropMuscle; } }

        private GUIStyle style = new GUIStyle();
        private GUIStyle miniLabelStyle = new GUIStyle();
        private static Color pro = new Color(0.5f, 0.7f, 0.3f, 1f);
        private static Color free = new Color(0.2f, 0.3f, 0.1f, 1f);
        private static Color sceneColor = new Color(0.2f, 0.7f, 1f);

        public override void OnInspectorGUI()
        {
            if (script == null) return;
            serializedObject.Update();
            
            style.wordWrap = true;
            style.normal.textColor = EditorGUIUtility.isProSkin ? pro : free;

            miniLabelStyle.wordWrap = true;
            miniLabelStyle.fontSize = 10;
            miniLabelStyle.normal.textColor = EditorStyles.miniLabel.normal.textColor;

            if (!Application.isPlaying)
            {
                if (GUILayout.Button("Remove This Prop Muscle"))
                {
                    //int index = script.muscle.index;
                    //Debug.Log(index);
                    var puppetMasterS = new SerializedObject(script.puppetMaster);
                    SerializedProperty muscles = puppetMasterS.FindProperty("muscles");

                    int index = 0;
                    ConfigurableJoint joint = script.GetComponent<ConfigurableJoint>();
                    for (int i = 0; i < puppetMasterS.FindProperty("muscles").arraySize; i++)
                    {
                        muscles.GetArrayElementAtIndex(i).FindPropertyRelative("index").intValue = i;
                        if (muscles.GetArrayElementAtIndex(i).FindPropertyRelative("joint").objectReferenceValue == joint) index = i;
                    }

                    /*
                    if (script.muscle.additionalPin != null)
                    {
                        script.muscle.additionalPin.gameObject.hideFlags = HideFlags.None;
                        script.muscle.additionalPinTarget.gameObject.hideFlags = HideFlags.None;
                    }

                    script.muscle.target.gameObject.hideFlags = HideFlags.None;
                    */

                    var target = script.muscle.target;

                    
                    muscles.DeleteArrayElementAtIndex(index);

                    for (int i = 0; i < puppetMasterS.FindProperty("muscles").arraySize; i++)
                    {
                        muscles.GetArrayElementAtIndex(i).FindPropertyRelative("index").intValue = i;
                    }
                    
                    puppetMasterS.ApplyModifiedProperties();

                    PuppetMasterInspector.DestroyImmediateSafe(target.gameObject);
                    PuppetMasterInspector.DestroyImmediateSafe(script.gameObject);
                    return;
                }
            }

            DrawDefaultInspector();

            //GUILayout.Space(10);

            if (!Application.isPlaying)
            {
                if (script.muscle.additionalPin == null)
                {
                    if (GUILayout.Button("Add Additional Pin"))
                    {
                        AddAdditionalPin();
                    }
                }
                else
                {
                    if (GUILayout.Button("Remove Additional Pin"))
                    {
                        RemoveAdditionalPin();
                    }
                }

                EditorGUILayout.LabelField("Additional Pins help to pin this prop muscle to animation from another point. Without it, the prop muscle would be actuated by Muscle Spring only, which is likely not enough to follow fast swinging animations. Move the additional pin by 'Additional Pin Offset' towards the end of the prop (tip of the sword).", miniLabelStyle);
            }

            serializedObject.ApplyModifiedProperties();
            
        }

        private bool AddAdditionalPin()
        {
            if (script.muscle.additionalPin != null) return false;

            ConfigurableJoint additionalJoint = null;
            Transform additionalTarget = null;
            AddAdditionalPinUnserialized(script, out additionalJoint, out additionalTarget);

            var puppetMasterS = new SerializedObject(script.puppetMaster);
            puppetMasterS.FindProperty("muscles").GetArrayElementAtIndex(script.muscle.index).FindPropertyRelative("additionalPin").objectReferenceValue = additionalJoint;
            puppetMasterS.FindProperty("muscles").GetArrayElementAtIndex(script.muscle.index).FindPropertyRelative("additionalPinTarget").objectReferenceValue = additionalTarget;
            puppetMasterS.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();

            //muscle.additionalPin.gameObject.hideFlags = HideFlags.HideInHierarchy;
            //muscle.additionalPinTarget.gameObject.hideFlags = HideFlags.HideInHierarchy;

            return true;
        }

        public static void AddAdditionalPinUnserialized(PropMuscle script, out ConfigurableJoint joint, out Transform target)
        {
            var addGO = new GameObject("Additional Pin");
            addGO.gameObject.layer = script.gameObject.layer;
            addGO.transform.parent = script.transform;
            addGO.transform.localPosition = script.additionalPinOffset;
            addGO.transform.localRotation = Quaternion.identity;
            //script.lastAdditionalPinOffset = additionalPinOffset;

            addGO.AddComponent<Rigidbody>();

            joint = addGO.AddComponent<ConfigurableJoint>();
            joint.connectedBody = script.muscle.joint.GetComponent<Rigidbody>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = script.additionalPinOffset;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            var addTargetGO = new GameObject("Additional Pin Target");
            addTargetGO.layer = script.muscle.target.gameObject.layer;
            addTargetGO.transform.parent = script.muscle.target;
            addTargetGO.transform.position = addGO.transform.position;
            addTargetGO.transform.rotation = addGO.transform.rotation;
            target = addTargetGO.transform;
        }

        private bool RemoveAdditionalPin()
        {
            if (script.muscle.additionalPin == null) return false;

            //script.muscle.additionalPin.gameObject.hideFlags = HideFlags.None;
            //script.muscle.additionalPinTarget.gameObject.hideFlags = HideFlags.None;

            PuppetMasterInspector.DestroyImmediateSafe(script.muscle.additionalPin.gameObject);
            PuppetMasterInspector.DestroyImmediateSafe(script.muscle.additionalPinTarget.gameObject);

            var puppetMasterS = new SerializedObject(script.puppetMaster);
            puppetMasterS.FindProperty("muscles").GetArrayElementAtIndex(script.muscle.index).FindPropertyRelative("additionalPin").objectReferenceValue = null;
            puppetMasterS.FindProperty("muscles").GetArrayElementAtIndex(script.muscle.index).FindPropertyRelative("additionalPinTarget").objectReferenceValue = null;
            puppetMasterS.ApplyModifiedProperties();

            return true;
        }

        private static float GetHandleSize(Vector3 position)
        {
            float s = HandleUtility.GetHandleSize(position) * 0.1f;
            return Mathf.Lerp(s, 0.025f, 0.2f);
        }

        void OnSceneGUI() {
            DrawScene(script);
		}

        private static void CubeCapSafe(Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_6_OR_NEWER
                Handles.CubeHandleCap(0, position, rotation, size, EventType.Repaint);
#else
            Handles.CubeCap(0, position, rotation, size);
#endif
        }

        private static void SphereCapSafe(Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_6_OR_NEWER
                Handles.SphereHandleCap(0, position, rotation, size, EventType.Repaint);
#else
            Handles.SphereCap(0, position, rotation, size);
#endif
        }

        public static void DrawScene(PropMuscle script)
        {
            if (script == null) return;
            if (Application.isPlaying) return;

            GUIStyle sceneLabelStyle = new GUIStyle();
            sceneLabelStyle.wordWrap = false;
            sceneLabelStyle.normal.textColor = sceneColor;

            Handles.color = sceneColor;
            float size = GetHandleSize(script.transform.position);

            if (Selection.activeGameObject == script.gameObject)
            {
                Handles.Label(script.transform.position + script.transform.forward * size * 2f, new GUIContent(script.transform.name), sceneLabelStyle);
            } else
            {
                Handles.Label(script.transform.position + script.transform.forward * size * 2f, new GUIContent("Click to select Prop Muscle"), sceneLabelStyle);
            }

            if (Selection.activeGameObject == script.gameObject)
            {
                CubeCapSafe(script.transform.position, script.transform.rotation, size * 2f);
            } else
            {
                if (DotButton(script.transform.position, script.transform.rotation, size, size))
                {
                    Selection.activeGameObject = script.gameObject;
                }
            }

            if (script.muscle.additionalPin != null)
            {
                Handles.DrawLine(script.transform.position, script.muscle.additionalPin.transform.position);
                SphereCapSafe(script.muscle.additionalPin.transform.position, script.muscle.additionalPin.transform.rotation, size);

                if (Selection.activeGameObject == script.gameObject)
                {
                    Handles.Label(script.muscle.additionalPin.transform.position + script.transform.forward * size * 2f, new GUIContent("Additional Pin"), sceneLabelStyle);
                }
            }

            Handles.color = Color.white;
        }

        public static bool DotButton(Vector3 position, Quaternion direction, float size, float pickSize)
        {
#if UNITY_5_6_OR_NEWER
			return Handles.Button(position, direction, size, pickSize, Handles.DotHandleCap);
#else
            return Handles.Button(position, direction, size, pickSize, Handles.DotCap);
#endif
        }
    }
}
