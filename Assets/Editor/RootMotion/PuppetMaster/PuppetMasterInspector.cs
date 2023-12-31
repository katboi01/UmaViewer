using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace RootMotion.Dynamics
{

    [CustomEditor(typeof(PuppetMaster))]
    public class PuppetMasterInspector : Editor
    {

        private PuppetMaster script { get { return target as PuppetMaster; } }
        private Transform animatedCharacter;
        private bool isValid;
        private int characterControllerLayer = 8;
        private int ragdollLayer = 9;
        private ConfigurableJoint addPropMuscleTo;
        private MonoScript monoScript;
        private Animator targetAnimator;
        private SkinnedMeshRenderer[] skinnedMeshRenderers = new SkinnedMeshRenderer[0];

        // Colors
        private GUIStyle style = new GUIStyle();
        private GUIStyle miniLabelStyle = new GUIStyle();
        private static Color pro = new Color(0.5f, 0.7f, 0.3f, 1f);
        private static Color free = new Color(0.2f, 0.3f, 0.1f, 1f);

        #region Inspector

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                monoScript = MonoScript.FromMonoBehaviour(script);
                int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
                if (currentExecutionOrder != 10100) MonoImporter.SetExecutionOrder(monoScript, 10100);

                // Fix negative collider sizes for ragdolls created before PuppetMaster 0.3
                BoxCollider[] boxColliders = script.GetComponentsInChildren<BoxCollider>();
                foreach (BoxCollider box in boxColliders)
                {
                    box.size = new Vector3(Mathf.Abs(box.size.x), Mathf.Abs(box.size.y), Mathf.Abs(box.size.z));
                }

                CapsuleCollider[] capsuleColliders = script.GetComponentsInChildren<CapsuleCollider>();
                foreach (CapsuleCollider capsule in capsuleColliders)
                {
                    capsule.height = Mathf.Abs(capsule.height);
                    capsule.radius = Mathf.Abs(capsule.radius);
                }

                if (script.transform.parent != null)
                {
                    targetAnimator = script.transform.parent.GetComponentInChildren<Animator>();
                }
            }
            else
            {
                if (script.initiated) targetAnimator = script.targetAnimator;
            }

            if (script.transform.parent != null)
            {
                skinnedMeshRenderers = script.transform.parent.GetComponentsInChildren<SkinnedMeshRenderer>();
            }

            isValid = IsValid();
        }

        private bool IsValid()
        {
            if (script.muscles.Length > 0) return true;

            ConfigurableJoint[] joints = script.gameObject.GetComponentsInChildren<ConfigurableJoint>();

            if (joints == null || joints.Length == 0)
            {
                return false;
            }

            if (animatedCharacter == null) animatedCharacter = script.transform;
            return true;
        }

        public override void OnInspectorGUI()
        {
            if (script == null) return;
            serializedObject.Update();

            if (script.muscles.Length > 0 && script.muscles[0].target != null && script.targetRoot == null) script.targetRoot = script.FindTargetRootRecursive(script.muscles[0].target);

            style.wordWrap = true;
            style.normal.textColor = EditorGUIUtility.isProSkin ? pro : free;

            miniLabelStyle.wordWrap = true;
            miniLabelStyle.fontSize = 10;
            miniLabelStyle.normal.textColor = EditorStyles.miniLabel.normal.textColor;

            if (!isValid)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "To set up PuppetMaster, add the component to a ragdoll character that is built with ConfigurableJoints. " +
                    "If your ragdoll uses other types of joints, you can easily convert them by selecting the character and clicking on GameObject/Convert To ConfigurableJoints."
                    , style);
                EditorGUILayout.Space();
                return;
            }

            if (!Application.isPlaying)
            {
                for (int i = 0; i < script.muscles.Length; i++)
                {
                    script.muscles[i].index = i;
                }
            }

            foreach (Muscle m in script.muscles)
            {
                if (m.joint == null)
                {
                    m.name = "Missing Joint Reference!";
                }
                else if (m.target == null)
                {
                    m.name = "Missing Target Reference!";
                }
                else m.name = m.joint.name;
            }

            if (script.muscles.Length == 0)
            {
                GUILayout.Space(5);

                animatedCharacter = (Transform)EditorGUILayout.ObjectField(new GUIContent("Animated Target", "If it is assigned to the same GameObject, it will be duplicated and cleaned up of all the ragdoll components. It can also be an instance of the character or even another character as long as the positions of the bones match. The rotations of the bones don't need to be identical. That makes it possible to share ragdoll structures and is also most useful when the feet bones are not aligned to the ground (like Mixamo characters). In that case you can simply rotate the ragdoll's feet to align, keeping the target's feet where they are."), animatedCharacter, typeof(Transform), true);

                GUILayout.Space(5);
                characterControllerLayer = EditorGUILayout.IntField(new GUIContent("Character Controller Layer", "The layer to assign the character controller to. Collisions between this layer and the 'Ragdoll Layer' will be ignored, or else the ragdoll would collide with the character controller."), characterControllerLayer);
                ragdollLayer = EditorGUILayout.IntField(new GUIContent("Ragdoll Layer", "The layer to assign the PuppetMaster and all it's muscles to. Collisions between this layer and the 'Character Controller Layer' will be ignored, or else the ragdoll would collide with the character controller."), ragdollLayer);

                if (characterControllerLayer == ragdollLayer)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField(
                        "The 'Character Controller Layer' must not be the same as the 'Ragdoll Layer', your ragdoll bones would collide with the character controller."
                        , style);
                    EditorGUILayout.Space();
                }

                if (animatedCharacter != null && characterControllerLayer != ragdollLayer)
                {
                    GUILayout.Space(5);

                    if (GUILayout.Button("Set Up PuppetMaster", GUILayout.MaxWidth(140)))
                    {
#if UNITY_2018_3_OR_NEWER
                        if (animatedCharacter == script.transform)
                        {
                            if (PrefabUtility.IsPartOfPrefabInstance(animatedCharacter.gameObject)) PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetNearestPrefabInstanceRoot(animatedCharacter.gameObject), PrefabUnpackMode.Completely, InteractionMode.UserAction);
                        } else
                        {
                            if (PrefabUtility.IsPartOfPrefabInstance(script.gameObject)) PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetNearestPrefabInstanceRoot(script.gameObject), PrefabUnpackMode.Completely, InteractionMode.UserAction);
                        }
#endif
                        script.SetUpTo(animatedCharacter, characterControllerLayer, ragdollLayer);

                        animatedCharacter = null;
                    }

                    EditorGUILayout.LabelField("Setting up PuppetMaster is not undoable. Make sure to make a backup duplicate of the character before you do that.", miniLabelStyle);

                    if (animatedCharacter == script.transform)
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.BeginVertical("Box");
                        GUILayout.Label("NB! 'Animated Target' is set to the same GameObject as this. PuppetMaster will make a duplicate of this GameObject and break the prefab to remove ragdoll components. It is recommended to set up the ragdoll on a new model instance, add PuppetMater to that and assign the original character as it's 'Animated Target'.", miniLabelStyle);
                        EditorGUILayout.EndVertical();
                    }
                }

                GUILayout.Space(5);
            }
            else
            {
                // Fix muscle positions and rotations to their targets
                if (!Application.isPlaying && script.targetRoot != null)
                {
                    if (script.transform.parent == script.targetRoot.parent)
                    {
                        script.transform.localPosition = Vector3.zero;
                        script.transform.localRotation = Quaternion.identity;
                        script.targetRoot.localPosition = Vector3.zero;
                        script.targetRoot.localRotation = Quaternion.identity;
                    }

                    foreach (Muscle m in script.muscles)
                    {
                        if (m.joint != null && m.target != null && !m.isPropMuscle)
                        {
                            m.joint.transform.position = m.target.position;

                            var r = m.joint.GetComponent<Rigidbody>();

                            bool hasChild = false;
                            foreach (Muscle otherMuscle in script.muscles)
                            {
                                if (otherMuscle != m && otherMuscle.joint != null && otherMuscle.joint.connectedBody != null)
                                {
                                    if (otherMuscle.joint.connectedBody == r)
                                    {
                                        hasChild = true;
                                        break;
                                    }
                                }
                            }
                            if (hasChild)
                            {
                                m.joint.transform.rotation = m.target.rotation;
                            }
                        }
                    }
                }

                if (!Application.isPlaying && script.humanoidConfig != null && script.targetRoot != null && script.targetAnimator != null && script.targetAnimator.isHuman)
                {
                    script.humanoidConfig.ApplyTo(script);

                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical("Box");
                    GUILayout.Label("Properties of this PuppetMaster are locked to the Humanoid Config file.", miniLabelStyle);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }

                script.muscleSpring = Mathf.Clamp(script.muscleSpring, 0f, script.muscleSpring);
                script.muscleDamper = Mathf.Clamp(script.muscleDamper, 0f, script.muscleDamper);

                DrawDefaultInspector();

                GUILayout.Space(10);

                // Prop Muscles
                GUIStyle largeHeaderStyle = new GUIStyle(GUI.skin.label);
                largeHeaderStyle.fontSize = 16;
                largeHeaderStyle.fontStyle = FontStyle.Normal;
                //largeHeaderStyle.alignment = TextAnchor.LowerLeft;
                largeHeaderStyle.alignment = TextAnchor.MiddleLeft;

                GUI.color = Color.grey * 0.7f;

                EditorGUILayout.LabelField("Prop Muscles", largeHeaderStyle);

                GUI.color = Color.white;

                EditorGUILayout.BeginHorizontal("Box");

                addPropMuscleTo = (ConfigurableJoint)EditorGUILayout.ObjectField(GUIContent.none, addPropMuscleTo, typeof(ConfigurableJoint), true);

                if (GUILayout.Button("Add Prop Muscle"))
                {
                    serializedObject.ApplyModifiedProperties();
                    AddPropMuscle(script, addPropMuscleTo, addPropMuscleTo.transform.position, addPropMuscleTo.transform.rotation, Vector3.forward * 0.5f);
                    serializedObject.Update();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Prop Muscles are muscles that props (PuppetMasterProp) can be attached to. For example, to put a sword in the right hand, assign the right hand muscle above, click on 'Add Prop Muscle', then move the added muscle to where you would want the sword to be held. To pick up the sword, just assign it as 'Current Prop' in the PropMuscle component on the prop muscle.", miniLabelStyle);
                GUILayout.Space(5);
            }

            bool animatorWarning = targetAnimator != null && targetAnimator.cullingMode != AnimatorCullingMode.AlwaysAnimate;
            if (animatorWarning)
            {
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("If Animator 'Culling Mode' is not set to 'Always Animate', animation might pause and cause glitches when the ragdoll is out of SkinnedMeshRenderer.bounds.", miniLabelStyle);
                EditorGUILayout.EndVertical();
            }

            bool rendererWarning = false;
            foreach (SkinnedMeshRenderer s in skinnedMeshRenderers)
            {
                if (!s.updateWhenOffscreen)
                {
                    rendererWarning = true;
                    break;
                }
            }
            if (rendererWarning)
            {
                if (!animatorWarning) GUILayout.Space(5);
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("One of the SkinnedMeshRenderers in the animated character hierarchy has 'Update When Offscreen' set to false. This might cause it to disappear from view when the ragdoll is out of the renderer's bounds.", miniLabelStyle);
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        public static void DestroyImmediateSafe(GameObject go)
        {
#if UNITY_2018_3_OR_NEWER
            if (PrefabUtility.IsPartOfPrefabInstance(go)) PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetNearestPrefabInstanceRoot(go), PrefabUnpackMode.Completely, InteractionMode.UserAction);
#endif
            DestroyImmediate(go);
        }

        public static bool AddPropMuscle(PuppetMaster script, ConfigurableJoint addPropMuscleTo, Vector3 position, Quaternion rotation, Vector3 additionalPinOffset, Transform targetParent = null)
        {
            var serializedObject = new SerializedObject(script);

            if (addPropMuscleTo != null)
            {
                bool isFlat = script.HierarchyIsFlat();

                var addToMuscle = script.GetMuscle(addPropMuscleTo);
                if (addToMuscle != null)
                {

                    GameObject go = new GameObject("Prop Muscle " + addPropMuscleTo.name);
                    go.layer = addPropMuscleTo.gameObject.layer;
                    go.transform.parent = isFlat ? script.transform : addPropMuscleTo.transform;
                    go.transform.position = position;
                    go.transform.rotation = rotation;

                    var r = go.AddComponent<Rigidbody>();

                    GameObject target = new GameObject("Prop Muscle Target " + addPropMuscleTo.name);
                    target.gameObject.layer = addToMuscle.target.gameObject.layer;
                    target.transform.parent = targetParent == null ? addToMuscle.target : targetParent;
                    target.transform.position = go.transform.position;
                    target.transform.rotation = go.transform.rotation;

                    var joint = go.AddComponent<ConfigurableJoint>();
                    joint.connectedBody = addToMuscle.joint.gameObject.GetComponent<Rigidbody>();
                    joint.xMotion = ConfigurableJointMotion.Locked;
                    joint.yMotion = ConfigurableJointMotion.Locked;
                    joint.zMotion = ConfigurableJointMotion.Locked;
                    joint.angularXMotion = ConfigurableJointMotion.Locked;
                    joint.angularYMotion = ConfigurableJointMotion.Locked;
                    joint.angularZMotion = ConfigurableJointMotion.Locked;

                    r.interpolation = joint.connectedBody.interpolation;

                    int newLength = script.muscles.Length + 1;
                    int newMuscleIndex = newLength - 1;

                    SerializedProperty muscles = serializedObject.FindProperty("muscles");
                    muscles.InsertArrayElementAtIndex(newMuscleIndex);

                    muscles.GetArrayElementAtIndex(newMuscleIndex).FindPropertyRelative("index").intValue = newMuscleIndex;
                    muscles.GetArrayElementAtIndex(newMuscleIndex).FindPropertyRelative("joint").objectReferenceValue = joint;
                    muscles.GetArrayElementAtIndex(newMuscleIndex).FindPropertyRelative("target").objectReferenceValue = target.transform;
                    muscles.GetArrayElementAtIndex(newMuscleIndex).FindPropertyRelative("props").FindPropertyRelative("group").intValue = 8;
                    muscles.GetArrayElementAtIndex(newMuscleIndex).FindPropertyRelative("isPropMuscle").boolValue = true;
                    serializedObject.ApplyModifiedProperties();

                    var propMuscle = joint.gameObject.AddComponent<PropMuscle>();
                    var propMuscleS = new SerializedObject(propMuscle);
                    propMuscleS.FindProperty("puppetMaster").objectReferenceValue = script;
                    propMuscleS.FindProperty("additionalPinOffset").vector3Value = additionalPinOffset;

                    propMuscleS.ApplyModifiedProperties();

                    if (additionalPinOffset != Vector3.zero)
                    {
                        propMuscleS.Update();
                        ConfigurableJoint additionalJoint = null;
                        Transform additionalTarget = null;
                        PropMuscleInspector.AddAdditionalPinUnserialized(propMuscle, out additionalJoint, out additionalTarget);

                        muscles.GetArrayElementAtIndex(newMuscleIndex).FindPropertyRelative("additionalPin").objectReferenceValue = additionalJoint;
                        muscles.GetArrayElementAtIndex(newMuscleIndex).FindPropertyRelative("additionalPinTarget").objectReferenceValue = additionalTarget;

                        propMuscleS.ApplyModifiedProperties();
                    }

                    serializedObject.ApplyModifiedProperties();
                    // TODO Do not add if addPropMuscleTo already has a PropMuscle

                    return true;
                }
                else
                {
                    Debug.LogError("Can't add Prop Muscle to a ConfigurableJoint that is not in the list of PuppetMaster.muscles.", script.transform);
                    return false;
                }

            }
            else
            {
                Debug.LogError("Please assign the ConfigurableJoint of the muscle you wish to add the Prop Muscle to.", script.transform);
                return false;
            }
        }

        private bool IsRagdoll()
        {
            Rigidbody[] rigidbodies = (Rigidbody[])script.gameObject.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody r in rigidbodies)
            {
                if (r != script.GetComponent<Rigidbody>()) return true;
            }
            return false;
        }

        #endregion Inspector

        #region Scene


        void OnSceneGUI()
        {
            if (script == null) return;
            if (Application.isPlaying) return;

            foreach (Muscle m in script.muscles)
            {
                if (m.joint != null)
                {
                    var propMuscle = m.joint.GetComponent<PropMuscle>();
                    if (propMuscle != null)
                    {
                        PropMuscleInspector.DrawScene(propMuscle);
                    }
                }
            }

            Handles.color = Color.white;
        }


        #endregion Scene

    }
}
