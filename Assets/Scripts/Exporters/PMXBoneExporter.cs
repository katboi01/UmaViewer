using LibMMD.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// Builds the PMX bone list from the Uma runtime skeleton as a standard MMD skeleton:
/// weight-driven bone selection, the 全ての親/センター/グルーブ/腰/下半身 root chain, foot and toe
/// IK, 両目, hand-attach dummies, rotate-only real bones, and parents-first ordering.
///
/// Positions are relative to coordinateRoot; the PMX writer applies the PMX frame and scale.
/// CySpring bones take their short name from springShortNames so a baked VMD can address them
/// (15-byte track names); NameEn always keeps the runtime name, which the physics exporter and
/// PMXEditor use.
public static class PMXBoneExporter
{
    public sealed class Result
    {
        public Bone[] Bones;
        /// Runtime transform -> PMX bone index, for skinning. Not every kept transform is a
        /// bone of its own (Hip is 腰, the skeleton root is センター); use LookUp for weights.
        public Dictionary<Transform, int> BoneIndexes;

        /// Nearest bone for a transform, walking up the hierarchy; 0 if none.
        public int LookUp(Transform t)
        {
            for (Transform cur = t; cur != null; cur = cur.parent)
                if (BoneIndexes.TryGetValue(cur, out int i)) return i;
            return 0;
        }
    }

    static readonly Dictionary<string, string> BoneNameMapping = new Dictionary<string, string>()
    {
        { "Spine", "上半身" }, { "Chest", "上半身2" }, { "Neck", "首" }, { "Head", "頭" },
        { "Shoulder_L", "左肩" }, { "Arm_L", "左腕" }, { "Elbow_L", "左ひじ" },
        { "ArmRoll_L", "左手捩" }, { "Wrist_L", "左手首" },
        { "Shoulder_R", "右肩" }, { "Arm_R", "右腕" }, { "Elbow_R", "右ひじ" },
        { "ArmRoll_R", "右手捩" }, { "Wrist_R", "右手首" },
        { "Thumb_01_L", "左親指０" }, { "Thumb_02_L", "左親指１" }, { "Thumb_03_L", "左親指２" },
        { "Index_01_L", "左人指１" }, { "Index_02_L", "左人指２" }, { "Index_03_L", "左人指３" },
        { "Middle_01_L", "左中指１" }, { "Middle_02_L", "左中指２" }, { "Middle_03_L", "左中指３" },
        { "Ring_01_L", "左薬指１" }, { "Ring_02_L", "左薬指２" }, { "Ring_03_L", "左薬指３" },
        { "Pinky_01_L", "左小指１" }, { "Pinky_02_L", "左小指２" }, { "Pinky_03_L", "左小指３" },
        { "Thumb_01_R", "右親指０" }, { "Thumb_02_R", "右親指１" }, { "Thumb_03_R", "右親指２" },
        { "Index_01_R", "右人指１" }, { "Index_02_R", "右人指２" }, { "Index_03_R", "右人指３" },
        { "Middle_01_R", "右中指１" }, { "Middle_02_R", "右中指２" }, { "Middle_03_R", "右中指３" },
        { "Ring_01_R", "右薬指１" }, { "Ring_02_R", "右薬指２" }, { "Ring_03_R", "右薬指３" },
        { "Pinky_01_R", "右小指１" }, { "Pinky_02_R", "右小指２" }, { "Pinky_03_R", "右小指３" },
        { "Thigh_L", "左足" }, { "Knee_L", "左ひざ" }, { "Ankle_L", "左足首" }, { "Toe_L", "左足先EX" },
        { "Thigh_R", "右足" }, { "Knee_R", "右ひざ" }, { "Ankle_R", "右足首" }, { "Toe_R", "右足先EX" },
        { "Eye_L", "左目" }, { "Eye_R", "右目" },
        { "Ear_01_L", "左耳" }, { "Ear_02_L", "左耳1" }, { "Ear_03_L", "左耳2" },
        { "Ear_01_R", "右耳" }, { "Ear_02_R", "右耳1" }, { "Ear_03_R", "右耳2" },
        { "Mouth", "口" }, { "Jaw", "顎" }
    };

    /// Kept whether or not any vertex weights them: the animated chain must be complete.
    static readonly string[] RequiredBoneNames =
    {
        "Hip", "Spine", "Chest", "Neck", "Head",
        "Shoulder_L", "Arm_L", "Elbow_L", "ArmRoll_L", "Wrist_L",
        "Shoulder_R", "Arm_R", "Elbow_R", "ArmRoll_R", "Wrist_R",
        "Hand_Attach_L", "Hand_Attach_R",
        "Thigh_L", "Knee_L", "Ankle_L", "Toe_L",
        "Thigh_R", "Knee_R", "Ankle_R", "Toe_R", "Eye_L", "Eye_R"
    };

    const float TailLength = 0.1f;

    public static Result Build(Transform skeletonRoot, Transform coordinateRoot, IEnumerable<Renderer> renderers,
        IEnumerable<Transform> additionalBones, IReadOnlyDictionary<string, string> springShortNames)
    {
        Transform[] hierarchy = skeletonRoot.GetComponentsInChildren<Transform>(true);
        HashSet<Transform> selected = CollectReferencedBones(renderers, skeletonRoot);

        if (additionalBones != null)
            foreach (Transform bone in additionalBones)
                if (bone != null && bone != skeletonRoot && !IsExcluded(bone.name)) selected.Add(bone);

        foreach (string boneName in RequiredBoneNames)
        {
            Transform bone = hierarchy.FirstOrDefault(t => t.name.Equals(boneName, StringComparison.OrdinalIgnoreCase));
            if (bone != null) selected.Add(bone);
        }

        Transform hip = Find(selected, "Hip");
        var bones = new List<Bone>();
        var indexes = new Dictionary<Transform, int>();
        Vector3 origin = coordinateRoot.InverseTransformPoint(skeletonRoot.position);
        Vector3 hipPosition = hip != null ? coordinateRoot.InverseTransformPoint(hip.position) : origin;

        int parentOfAll = AddVirtualBone(bones, "全ての親", "ParentOfAll", origin, -1, true);
        int center = AddVirtualBone(bones, "センター", "Center", origin, parentOfAll, true);
        int groove = AddVirtualBone(bones, "グルーブ", "Groove", hipPosition, center, true);
        // 腰 IS the Hip transform (indexes[hip] = waist below), so its English name stays "Hip":
        // the physics exporter and anything else keyed by runtime name keep resolving it.
        int waist = AddVirtualBone(bones, "腰", "Hip", hipPosition, groove, false);
        int lowerBody = AddVirtualBone(bones, "下半身", "LowerBody", hipPosition, waist, false);

        indexes[skeletonRoot] = center;
        if (hip != null) indexes[hip] = waist;

        foreach (Transform transform in hierarchy)
        {
            if (transform == skeletonRoot || transform == hip || !selected.Contains(transform) || IsExcluded(transform.name))
                continue;
            int parentIndex = ResolveParentIndex(transform, skeletonRoot, hip, indexes, center, waist, lowerBody);
            indexes[transform] = bones.Count;
            bones.Add(CreateTransformBone(transform, coordinateRoot, parentIndex, springShortNames));
        }

        int bothEyes = AddBothEyesControlBone(bones, indexes, coordinateRoot);
        ReparentTongueChain(bones, indexes);
        RebuildChildLinks(bones);
        AddFootIk(bones, indexes, coordinateRoot, parentOfAll, "L");
        AddFootIk(bones, indexes, coordinateRoot, parentOfAll, "R");
        RebuildChildLinks(bones);
        AlignElbowArmRollAndWrist(bones, indexes, coordinateRoot, "L");
        AlignElbowArmRollAndWrist(bones, indexes, coordinateRoot, "R");
        AddHandAttachmentCompatibilityBone(bones, indexes, coordinateRoot, "L");
        AddHandAttachmentCompatibilityBone(bones, indexes, coordinateRoot, "R");
        SetBothEyesTailToNoseBridge(bones, bothEyes, indexes, coordinateRoot);
        EnsureUniqueNames(bones);
        EnsureParentsFirst(bones, indexes);
        Validate(bones);

        return new Result { Bones = bones.ToArray(), BoneIndexes = indexes };
    }

    // ------------------------------------------------------------------ selection

    static HashSet<Transform> CollectReferencedBones(IEnumerable<Renderer> renderers, Transform skeletonRoot)
    {
        var weighted = new HashSet<Transform>();
        foreach (SkinnedMeshRenderer renderer in renderers.OfType<SkinnedMeshRenderer>())
        {
            Mesh mesh = renderer.sharedMesh;
            Transform[] rendererBones = renderer.bones;
            if (mesh == null || rendererBones == null) continue;
            foreach (BoneWeight w in mesh.boneWeights)
            {
                AddWeighted(weighted, rendererBones, w.boneIndex0, w.weight0);
                AddWeighted(weighted, rendererBones, w.boneIndex1, w.weight1);
                AddWeighted(weighted, rendererBones, w.boneIndex2, w.weight2);
                AddWeighted(weighted, rendererBones, w.boneIndex3, w.weight3);
            }
        }

        // Ancestors of a weighted bone carry its motion even without weights of their own.
        var result = new HashSet<Transform>();
        foreach (Transform bone in weighted)
            for (Transform cur = bone; cur != null && cur != skeletonRoot; cur = cur.parent)
                if (!IsExcluded(cur.name)) result.Add(cur);
        return result;
    }

    static void AddWeighted(HashSet<Transform> result, Transform[] bones, int index, float weight)
    {
        const float MinimumWeight = 0.000001f;
        if (weight <= MinimumWeight || index < 0 || index >= bones.Length) return;
        Transform bone = bones[index];
        if (bone != null && !IsExcluded(bone.name)) result.Add(bone);
    }

    /// Rig helpers that are not bones, plus the secondary eye bones (Eye_High_* etc.), which
    /// double-transform the eye mesh when exported alongside Eye_L/Eye_R.
    static bool IsExcluded(string name)
    {
        return name.StartsWith("Col_", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith("_Handle", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith("_Pole", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith("_Target", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith("_Ctrl", StringComparison.OrdinalIgnoreCase) ||
               name.IndexOf("_offset", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("locator", StringComparison.OrdinalIgnoreCase) >= 0 ||
               (name.StartsWith("Eye_", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals("Eye_L", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals("Eye_R", StringComparison.OrdinalIgnoreCase));
    }

    static Transform Find(IEnumerable<Transform> bones, string name)
    {
        return bones.FirstOrDefault(t => t.name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    static int ResolveParentIndex(Transform transform, Transform skeletonRoot, Transform hip,
        Dictionary<Transform, int> indexes, int center, int waist, int lowerBody)
    {
        Transform parent = transform.parent;
        while (parent != null && parent != hip && parent != skeletonRoot)
        {
            if (indexes.TryGetValue(parent, out int parentIndex)) return parentIndex;
            parent = parent.parent;
        }
        if (parent == hip) return IsUpperBodyBranch(transform, hip) ? waist : lowerBody;
        return center;
    }

    /// Spine and its descendants hang off 腰; everything else at hip level (legs, skirt, tail)
    /// off 下半身.
    static bool IsUpperBodyBranch(Transform transform, Transform hip)
    {
        for (Transform cur = transform; cur != null && cur != hip; cur = cur.parent)
        {
            if (cur.name.Equals("Spine", StringComparison.OrdinalIgnoreCase) ||
                cur.name.Equals("Waist", StringComparison.OrdinalIgnoreCase) ||
                cur.name.Equals("UpBody_Ctrl", StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    // ------------------------------------------------------------------ bones

    static Bone CreateTransformBone(Transform transform, Transform coordinateRoot, int parentIndex,
        IReadOnlyDictionary<string, string> springShortNames)
    {
        string name = transform.name;
        if (BoneNameMapping.TryGetValue(transform.name, out string mapped)) name = mapped;
        else if (springShortNames != null && springShortNames.TryGetValue(transform.name, out string shortName)) name = shortName;

        return new Bone
        {
            Name = name,
            NameEn = transform.name,
            Position = coordinateRoot.InverseTransformPoint(transform.position),
            ParentIndex = parentIndex,
            TransformLevel = 0,
            Rotatable = true,
            Movable = false,
            Visible = true,
            Controllable = true,
            ChildBoneVal = new Bone.ChildBone { ChildUseId = false, Offset = Vector3.up * TailLength }
        };
    }

    static int AddVirtualBone(List<Bone> bones, string name, string nameEn, Vector3 position, int parent, bool movable)
    {
        bones.Add(new Bone
        {
            Name = name,
            NameEn = nameEn,
            Position = position,
            ParentIndex = parent,
            TransformLevel = 0,
            Rotatable = true,
            Movable = movable,
            Visible = true,
            Controllable = true,
            ChildBoneVal = new Bone.ChildBone { ChildUseId = false, Offset = Vector3.up * TailLength }
        });
        return bones.Count - 1;
    }

    /// Tail = first child by index; leaves get a short upward offset.
    static void RebuildChildLinks(List<Bone> bones)
    {
        for (int i = 0; i < bones.Count; i++)
        {
            int child = bones.FindIndex(b => b.ParentIndex == i);
            if (child >= 0) { bones[i].ChildBoneVal.ChildUseId = true; bones[i].ChildBoneVal.Index = child; }
            else { bones[i].ChildBoneVal.ChildUseId = false; bones[i].ChildBoneVal.Offset = Vector3.up * TailLength; }
        }
    }

    static int AddBothEyesControlBone(List<Bone> bones, Dictionary<Transform, int> indexes, Transform coordinateRoot)
    {
        Transform head = Find(indexes.Keys, "Head");
        Transform leftEye = Find(indexes.Keys, "Eye_L");
        Transform rightEye = Find(indexes.Keys, "Eye_R");
        if (head == null || (leftEye == null && rightEye == null)) return -1;

        int bothEyes = AddVirtualBone(bones, "両目", "BothEyes",
            coordinateRoot.InverseTransformPoint(head.position), indexes[head], false);
        if (leftEye != null) bones[indexes[leftEye]].ParentIndex = bothEyes;
        if (rightEye != null) bones[indexes[rightEye]].ParentIndex = bothEyes;
        return bothEyes;
    }

    static void SetBothEyesTailToNoseBridge(List<Bone> bones, int bothEyes, Dictionary<Transform, int> indexes, Transform coordinateRoot)
    {
        if (bothEyes < 0) return;
        Transform head = Find(indexes.Keys, "Head");
        Transform leftEye = Find(indexes.Keys, "Eye_L");
        Transform rightEye = Find(indexes.Keys, "Eye_R");
        if (head == null) return;

        Transform nose = FindNoseBone(head);
        Vector3 target;
        if (nose != null) target = coordinateRoot.InverseTransformPoint(nose.position);
        else if (leftEye != null && rightEye != null) target = coordinateRoot.InverseTransformPoint((leftEye.position + rightEye.position) * 0.5f);
        else return;

        Bone control = bones[bothEyes];
        control.ChildBoneVal.ChildUseId = false;
        control.ChildBoneVal.Offset = target - control.Position;
    }

    static Transform FindNoseBone(Transform head)
    {
        Transform[] descendants = head.GetComponentsInChildren<Transform>(true);
        return descendants.FirstOrDefault(t => t.name.Equals("Nose", StringComparison.OrdinalIgnoreCase))
            ?? descendants.FirstOrDefault(t => t.name.StartsWith("Nose_00", StringComparison.OrdinalIgnoreCase))
            ?? descendants.FirstOrDefault(t => t.name.StartsWith("Nose_01", StringComparison.OrdinalIgnoreCase))
            ?? descendants.FirstOrDefault(t => t.name.StartsWith("Nose", StringComparison.OrdinalIgnoreCase));
    }

    static void ReparentTongueChain(List<Bone> bones, Dictionary<Transform, int> indexes)
    {
        Transform chin = Find(indexes.Keys, "Chin") ?? indexes.Keys.FirstOrDefault(t => t.name.StartsWith("Jaw"));
        if (chin == null) return;
        Transform tongue = Find(indexes.Keys, "Tongue");
        Transform out01 = Find(indexes.Keys, "Tongue_Out_01");
        Transform out02 = Find(indexes.Keys, "Tongue_Out_02");

        if (tongue != null)
        {
            bones[indexes[tongue]].ParentIndex = indexes[chin];
            if (out01 != null) bones[indexes[out01]].ParentIndex = indexes[tongue];
            if (out01 != null && out02 != null) bones[indexes[out02]].ParentIndex = indexes[out01];
            return;
        }
        foreach (Transform candidate in indexes.Keys.Where(t => t.name.StartsWith("Tongue") && !t.name.Contains("Out")))
            bones[indexes[candidate]].ParentIndex = indexes[chin];
    }

    /// Elbow tail, ArmRoll and Wrist meet at one point in a standard arm. Only applied when
    /// the two runtime bones are already close, since moving a skinned bone's rest position
    /// otherwise distorts the mesh around it.
    static void AlignElbowArmRollAndWrist(List<Bone> bones, Dictionary<Transform, int> indexes, Transform coordinateRoot, string side)
    {
        Transform elbow = Find(indexes.Keys, "Elbow_" + side);
        Transform armRoll = Find(indexes.Keys, "ArmRoll_" + side);
        Transform wrist = Find(indexes.Keys, "Wrist_" + side);
        if (elbow == null || armRoll == null || wrist == null ||
            !indexes.TryGetValue(elbow, out int elbowIndex) ||
            !indexes.TryGetValue(armRoll, out int armRollIndex) ||
            !indexes.TryGetValue(wrist, out int wristIndex)) return;

        const float MaxAlignDistance = 0.01f;
        Vector3 connection = coordinateRoot.InverseTransformPoint(armRoll.position);
        if ((bones[wristIndex].Position - connection).magnitude <= MaxAlignDistance)
        {
            bones[armRollIndex].Position = connection;
            bones[wristIndex].Position = connection;
        }
        bones[elbowIndex].ChildBoneVal.ChildUseId = true;
        bones[elbowIndex].ChildBoneVal.Index = wristIndex;
    }

    /// Hand_Attach keeps its runtime name; ダミー.L/R are added at the same point for tools
    /// that look the attachment up by the MMD name. Both tails point perpendicular to the
    /// forearm, away from the fingers.
    static void AddHandAttachmentCompatibilityBone(List<Bone> bones, Dictionary<Transform, int> indexes, Transform coordinateRoot, string side)
    {
        Transform handAttach = Find(indexes.Keys, "Hand_Attach_" + side);
        if (handAttach == null || !indexes.TryGetValue(handAttach, out int handAttachIndex)) return;

        Vector3 attachmentPosition = coordinateRoot.InverseTransformPoint(handAttach.position);
        Vector3 tailOffset = HandAttachmentTailOffset(indexes.Keys, handAttach, coordinateRoot, side);

        Bone attachment = bones[handAttachIndex];
        attachment.Position = attachmentPosition;
        attachment.ChildBoneVal.ChildUseId = false;
        attachment.ChildBoneVal.Offset = tailOffset;

        string dummyName = side == "L" ? "ダミー.L" : "ダミー.R";
        if (bones.Any(b => b.Name.Equals(dummyName, StringComparison.OrdinalIgnoreCase))) return;
        int dummy = AddVirtualBone(bones, dummyName, "Dummy." + side, attachmentPosition, handAttachIndex, false);
        bones[dummy].ChildBoneVal.ChildUseId = false;
        bones[dummy].ChildBoneVal.Offset = tailOffset;
    }

    static Vector3 HandAttachmentTailOffset(IEnumerable<Transform> transforms, Transform handAttach, Transform coordinateRoot, string side)
    {
        Transform wrist = Find(transforms, "Wrist_" + side);
        Transform elbow = Find(transforms, "Elbow_" + side);
        if (wrist == null || elbow == null) return Vector3.up * TailLength;

        Transform armRoll = Find(transforms, "ArmRoll_" + side);
        Vector3 wristPosition = coordinateRoot.InverseTransformPoint(armRoll != null ? armRoll.position : wrist.position);
        Vector3 elbowPosition = coordinateRoot.InverseTransformPoint(elbow.position);
        Vector3 attachmentPosition = coordinateRoot.InverseTransformPoint(handAttach.position);
        Vector3 wristToAttachment = attachmentPosition - wristPosition;
        if (wristToAttachment.sqrMagnitude < 0.000001f) return Vector3.up * TailLength;

        Vector3 forearm = wristPosition - elbowPosition;
        if (forearm.sqrMagnitude < 0.000001f) return Vector3.up * wristToAttachment.magnitude;

        float length = wristToAttachment.magnitude;
        Vector3 positive = (Quaternion.AngleAxis(90f, Vector3.forward) * forearm).normalized * length;
        Vector3 negative = (Quaternion.AngleAxis(-90f, Vector3.forward) * forearm).normalized * length;

        var fingerRoots = new[] { "Index_01_", "Middle_01_", "Ring_01_", "Pinky_01_" }
            .Select(n => Find(transforms, n + side)).Where(t => t != null).ToList();
        if (fingerRoots.Count == 0)
        {
            float p = Vector3.Dot(positive.normalized, wristToAttachment.normalized);
            float n = Vector3.Dot(negative.normalized, wristToAttachment.normalized);
            return p <= n ? positive : negative;
        }

        Vector3 knuckles = Vector3.zero;
        foreach (Transform root in fingerRoots) knuckles += coordinateRoot.InverseTransformPoint(root.position);
        knuckles /= fingerRoots.Count;
        Vector3 toKnuckles = (knuckles - attachmentPosition).normalized;
        return Vector3.Dot(positive.normalized, toKnuckles) <= Vector3.Dot(negative.normalized, toKnuckles) ? positive : negative;
    }

    /// 足IK親 -> 足ＩＫ -> つま先ＩＫ per side. Limits are in the runtime frame; the writer maps
    /// them to the PMX frame (X and Z negate and swap), so the knee's [0.5°, 180°] here is the
    /// standard [-180°, -0.5°] in the file.
    static void AddFootIk(List<Bone> bones, Dictionary<Transform, int> indexes, Transform coordinateRoot, int parentOfAll, string side)
    {
        Transform thigh = Find(indexes.Keys, "Thigh_" + side);
        Transform knee = Find(indexes.Keys, "Knee_" + side);
        Transform ankle = Find(indexes.Keys, "Ankle_" + side);
        Transform toe = Find(indexes.Keys, "Toe_" + side);
        if (thigh == null || knee == null || ankle == null) return;

        string prefix = side == "L" ? "左" : "右";
        Vector3 anklePosition = coordinateRoot.InverseTransformPoint(ankle.position);
        Vector3 ikParentPosition = new Vector3(anklePosition.x, 0, anklePosition.z);
        int ikParent = AddVirtualBone(bones, prefix + "足IK親", "FootIKParent_" + side, ikParentPosition, parentOfAll, true);
        int footIk = AddVirtualBone(bones, prefix + "足ＩＫ", "FootIK_" + side, anklePosition, ikParent, true);
        bones[footIk].HasIk = true;
        bones[footIk].TransformLevel = 1;
        bones[footIk].IkInfoVal = new Bone.IkInfo
        {
            IkTargetIndex = indexes[ankle],
            CcdIterateLimit = 40,
            CcdAngleLimit = 2.0f,
            IkLinks = new[]
            {
                new Bone.IkLink { LinkIndex = indexes[knee], HasLimit = true, LoLimit = new Vector3(0.0087f, 0, 0), HiLimit = new Vector3(Mathf.PI, 0, 0) },
                new Bone.IkLink { LinkIndex = indexes[thigh], HasLimit = false }
            }
        };

        if (toe == null) return;
        int toeIk = AddVirtualBone(bones, prefix + "つま先ＩＫ", "ToeIK_" + side,
            coordinateRoot.InverseTransformPoint(toe.position), footIk, true);
        bones[toeIk].HasIk = true;
        bones[toeIk].TransformLevel = 1;
        bones[toeIk].IkInfoVal = new Bone.IkInfo
        {
            IkTargetIndex = indexes[toe],
            CcdIterateLimit = 3,
            CcdAngleLimit = 1.0f,
            IkLinks = new[] { new Bone.IkLink { LinkIndex = indexes[ankle], HasLimit = false } }
        };
    }

    // ------------------------------------------------------------------ finishing

    /// Duplicate primary names get a numeric suffix; MMD tools key bones by name.
    static void EnsureUniqueNames(List<Bone> bones)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (Bone bone in bones)
        {
            if (seen.Add(bone.Name)) continue;
            for (int n = 2; ; n++)
            {
                string candidate = bone.Name + "_" + n;
                if (seen.Add(candidate)) { bone.Name = candidate; break; }
            }
        }
    }

    /// Stable reorder so every bone follows its parent, remapping every index that refers to a
    /// bone. Required by MMD and by babylon-mmd; the 両目 and dummy bones are added after the
    /// bones they parent.
    static void EnsureParentsFirst(List<Bone> bones, Dictionary<Transform, int> indexes)
    {
        int n = bones.Count;
        var order = new List<int>(n);
        var placed = new bool[n];
        var visiting = new bool[n];
        void Place(int i)
        {
            if (placed[i]) return;
            if (visiting[i]) throw new InvalidOperationException("PMX bone parent cycle at " + bones[i].Name);
            visiting[i] = true;
            int p = bones[i].ParentIndex;
            if (p >= 0 && p < n) Place(p);
            placed[i] = true;
            order.Add(i);
        }
        for (int i = 0; i < n; i++) Place(i);

        bool identity = true;
        for (int i = 0; i < n && identity; i++) identity = order[i] == i;
        if (identity) return;

        var map = new int[n];
        for (int newIdx = 0; newIdx < n; newIdx++) map[order[newIdx]] = newIdx;

        var reordered = order.Select(i => bones[i]).ToList();
        foreach (Bone b in reordered)
        {
            if (b.ParentIndex >= 0) b.ParentIndex = map[b.ParentIndex];
            if (b.ChildBoneVal.ChildUseId && b.ChildBoneVal.Index >= 0) b.ChildBoneVal.Index = map[b.ChildBoneVal.Index];
            if (b.IkInfoVal != null)
            {
                if (b.IkInfoVal.IkTargetIndex >= 0) b.IkInfoVal.IkTargetIndex = map[b.IkInfoVal.IkTargetIndex];
                foreach (Bone.IkLink link in b.IkInfoVal.IkLinks)
                    if (link.LinkIndex >= 0) link.LinkIndex = map[link.LinkIndex];
            }
        }
        bones.Clear();
        bones.AddRange(reordered);
        foreach (Transform key in indexes.Keys.ToList()) indexes[key] = map[indexes[key]];
    }

    static void Validate(List<Bone> bones)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < bones.Count; i++)
        {
            Bone bone = bones[i];
            if (!names.Add(bone.Name))
                throw new InvalidOperationException("PMX duplicate bone name: " + bone.Name);
            if (bone.ParentIndex < -1 || bone.ParentIndex >= bones.Count || bone.ParentIndex == i)
                throw new InvalidOperationException("PMX bone parent index invalid: " + bone.Name);
            if (bone.ParentIndex > i)
                throw new InvalidOperationException("PMX bone precedes its parent: " + bone.Name);
            if (bone.ChildBoneVal.ChildUseId && (bone.ChildBoneVal.Index < 0 || bone.ChildBoneVal.Index >= bones.Count))
                throw new InvalidOperationException("PMX bone tail index invalid: " + bone.Name);
            if (!bone.HasIk) continue;
            if (bone.IkInfoVal == null || bone.IkInfoVal.IkTargetIndex < 0 || bone.IkInfoVal.IkTargetIndex >= bones.Count)
                throw new InvalidOperationException("PMX IK target index invalid: " + bone.Name);
            foreach (Bone.IkLink link in bone.IkInfoVal.IkLinks)
            {
                if (link.LinkIndex < 0 || link.LinkIndex >= bones.Count)
                    throw new InvalidOperationException("PMX IK link index invalid: " + bone.Name);
                if (!link.HasLimit) continue;
                if (!IsFinite(link.LoLimit) || !IsFinite(link.HiLimit))
                    throw new InvalidOperationException("PMX IK limit not finite: " + bone.Name);
                if (link.LoLimit.x > link.HiLimit.x || link.LoLimit.y > link.HiLimit.y || link.LoLimit.z > link.HiLimit.z)
                    throw new InvalidOperationException("PMX IK limit inverted: " + bone.Name);
            }
        }
    }

    static bool IsFinite(Vector3 v)
    {
        return !float.IsNaN(v.x) && !float.IsInfinity(v.x) && !float.IsNaN(v.y) && !float.IsInfinity(v.y) &&
               !float.IsNaN(v.z) && !float.IsInfinity(v.z);
    }
}
