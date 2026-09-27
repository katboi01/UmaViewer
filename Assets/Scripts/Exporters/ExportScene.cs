using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Format-neutral snapshot of a model hierarchy used by the glTF and FBX exporters.
/// Everything is converted to a right-handed, Y-up, +Z-forward space by mirroring the X axis,
/// and expressed relative to the exported root transform.
/// UVs keep Unity's bottom-left origin; writers flip them if their format requires it.
/// </summary>
public class ExportScene
{
    public class Node
    {
        public string Name;
        public int Parent = -1;
        public List<int> Children = new List<int>();
        public Vector3 Translation;
        public Quaternion Rotation;
        public Vector3 Scale;
        /// <summary> Root-relative transform in export space. </summary>
        public Matrix4x4 World;
        public int Mesh = -1;
        public bool IsJoint;
    }

    public class BlendShape
    {
        public string Name;
        public Vector3[] DeltaPositions;
        public Vector3[] DeltaNormals;
    }

    public class MeshData
    {
        public string Name;
        public int Node;
        public Vector3[] Positions;
        public Vector3[] Normals;
        public Vector2[] UV;
        /// <summary> Triangle index lists per submesh, winding already corrected for export space. </summary>
        public List<int[]> Submeshes = new List<int[]>();
        public List<int> Materials = new List<int>();
        public List<BlendShape> BlendShapes = new List<BlendShape>();

        // Skinning (null when the mesh is not skinned)
        public int[] Joints;
        public Matrix4x4[] InverseBindMatrices;
        public Vector4Int[] JointIndices;
        public Vector4[] JointWeights;
        public bool IsSkinned => Joints != null;
    }

    public struct Vector4Int
    {
        public int x, y, z, w;
        public int this[int i] => i == 0 ? x : i == 1 ? y : i == 2 ? z : w;
    }

    public enum AlphaMode { Opaque, Mask, Blend }

    public class MaterialData
    {
        public string Name;
        public Color BaseColor = Color.white;
        public int Texture = -1;
        public AlphaMode Alpha = AlphaMode.Opaque;
        public float AlphaCutoff = 0.5f;
        public bool DoubleSided;
    }

    public class TextureData
    {
        public string Name;
        public string FileName;
        public byte[] Png;
    }

    public List<Node> Nodes = new List<Node>();
    public List<MeshData> Meshes = new List<MeshData>();
    public List<MaterialData> Materials = new List<MaterialData>();
    public List<TextureData> Textures = new List<TextureData>();

    readonly Dictionary<Transform, int> nodeLookup = new Dictionary<Transform, int>();
    readonly Dictionary<Material, int> materialLookup = new Dictionary<Material, int>();
    readonly Dictionary<Texture, int> textureLookup = new Dictionary<Texture, int>();
    readonly HashSet<string> textureFileNames = new HashSet<string>();
    int defaultMaterial = -1;

    static readonly Matrix4x4 Mirror = Matrix4x4.Scale(new Vector3(-1, 1, 1));

    public static Vector3 ConvertVector(Vector3 v) => new Vector3(-v.x, v.y, v.z);
    public static Quaternion ConvertRotation(Quaternion q) => new Quaternion(q.x, -q.y, -q.z, q.w);
    public static Matrix4x4 ConvertMatrix(Matrix4x4 m) => Mirror * m * Mirror;

    public static ExportScene Build(Transform root)
    {
        var scene = new ExportScene();
        scene.AddNode(root, -1, root);
        foreach (var renderer in root.GetComponentsInChildren<Renderer>())
        {
            scene.AddRenderer(renderer, root);
        }
        return scene;
    }

    void AddNode(Transform t, int parent, Transform root)
    {
        var node = new Node { Name = t.name, Parent = parent };
        if (t == root)
        {
            node.Translation = Vector3.zero;
            node.Rotation = Quaternion.identity;
            node.Scale = Vector3.one;
            node.World = Matrix4x4.identity;
        }
        else
        {
            node.Translation = ConvertVector(t.localPosition);
            node.Rotation = ConvertRotation(t.localRotation);
            node.Scale = t.localScale;
            node.World = ConvertMatrix(root.worldToLocalMatrix * t.localToWorldMatrix);
        }

        int index = Nodes.Count;
        Nodes.Add(node);
        nodeLookup[t] = index;
        if (parent >= 0) Nodes[parent].Children.Add(index);

        foreach (Transform child in t)
        {
            AddNode(child, index, root);
        }
    }

    void AddRenderer(Renderer renderer, Transform root)
    {
        Mesh mesh;
        SkinnedMeshRenderer smr = renderer as SkinnedMeshRenderer;
        if (smr)
        {
            mesh = smr.sharedMesh;
        }
        else if (renderer is MeshRenderer)
        {
            var filter = renderer.GetComponent<MeshFilter>();
            mesh = filter ? filter.sharedMesh : null;
        }
        else
        {
            return;
        }

        if (!mesh || !nodeLookup.TryGetValue(renderer.transform, out int nodeIndex)) return;

        var source = mesh;
        if (!mesh.isReadable)
        {
            UmaViewerUI.Instance.ShowMessage($"Mesh {mesh.name} was not readable. Attempting to export", UIMessageType.Warning);
            try
            {
                mesh = ModelExporter.MakeReadableMeshCopy(mesh);
            }
            catch
            {
                UmaViewerUI.Instance.ShowMessage($"Export {mesh.name} Failed : Mesh is not readable", UIMessageType.Error);
                return;
            }
        }

        var data = new MeshData { Name = mesh.name, Node = nodeIndex };

        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var uv = mesh.uv;
        int count = vertices.Length;
        if (count == 0) return;

        data.Positions = new Vector3[count];
        data.Normals = new Vector3[count];
        data.UV = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            data.Positions[i] = ConvertVector(vertices[i]);
            data.Normals[i] = normals.Length == count ? ConvertVector(normals[i]).normalized : Vector3.up;
            data.UV[i] = uv.Length == count ? uv[i] : Vector2.zero;
        }

        var materials = renderer.sharedMaterials;
        for (int s = 0; s < mesh.subMeshCount; s++)
        {
            if (mesh.GetTopology(s) != MeshTopology.Triangles) continue;
            var indices = mesh.GetTriangles(s);
            // Mirroring flips handedness, so reverse the winding order.
            for (int i = 0; i + 2 < indices.Length; i += 3)
            {
                (indices[i + 1], indices[i + 2]) = (indices[i + 2], indices[i + 1]);
            }
            if (indices.Length == 0) continue;
            data.Submeshes.Add(indices);

            Material material = materials.Length == 0 ? null : materials[Mathf.Min(s, materials.Length - 1)];
            data.Materials.Add(AddMaterial(material));
        }
        if (data.Submeshes.Count == 0) return;

        ReadBlendShapes(mesh, data);

        if (smr && smr.bones.Length > 0 && source.bindposes.Length > 0)
        {
            ReadSkin(smr, mesh, source.bindposes, data, nodeIndex);
        }

        Nodes[nodeIndex].Mesh = Meshes.Count;
        Meshes.Add(data);
    }

    void ReadSkin(SkinnedMeshRenderer smr, Mesh mesh, Matrix4x4[] bindposes, MeshData data, int meshNode)
    {
        var bones = smr.bones;
        int jointCount = Mathf.Min(bones.Length, bindposes.Length);
        data.Joints = new int[jointCount];
        data.InverseBindMatrices = new Matrix4x4[jointCount];
        for (int j = 0; j < jointCount; j++)
        {
            int nodeIndex = meshNode;
            if (bones[j] && nodeLookup.TryGetValue(bones[j], out int boneNode))
            {
                nodeIndex = boneNode;
            }
            data.Joints[j] = nodeIndex;
            data.InverseBindMatrices[j] = ConvertMatrix(bindposes[j]);
            Nodes[nodeIndex].IsJoint = true;
        }

        var weights = mesh.boneWeights;
        int count = data.Positions.Length;
        data.JointIndices = new Vector4Int[count];
        data.JointWeights = new Vector4[count];
        for (int i = 0; i < count; i++)
        {
            if (weights.Length != count)
            {
                data.JointWeights[i] = new Vector4(1, 0, 0, 0);
                continue;
            }
            var w = weights[i];
            var idx = new Vector4Int
            {
                x = ClampJoint(w.boneIndex0, jointCount),
                y = ClampJoint(w.boneIndex1, jointCount),
                z = ClampJoint(w.boneIndex2, jointCount),
                w = ClampJoint(w.boneIndex3, jointCount)
            };
            var wt = new Vector4(w.weight0, w.weight1, w.weight2, w.weight3);
            float sum = wt.x + wt.y + wt.z + wt.w;
            wt = sum > 0 ? wt / sum : new Vector4(1, 0, 0, 0);
            data.JointIndices[i] = idx;
            data.JointWeights[i] = wt;
        }
    }

    static int ClampJoint(int index, int jointCount) => index >= 0 && index < jointCount ? index : 0;

    static void ReadBlendShapes(Mesh mesh, MeshData data)
    {
        int count = mesh.vertexCount;
        var dv = new Vector3[count];
        var dn = new Vector3[count];
        var dt = new Vector3[count];
        for (int b = 0; b < mesh.blendShapeCount; b++)
        {
            int frame = mesh.GetBlendShapeFrameCount(b) - 1;
            if (frame < 0) continue;
            float frameWeight = mesh.GetBlendShapeFrameWeight(b, frame);
            float scale = frameWeight > 0 ? 100f / frameWeight : 1f;
            mesh.GetBlendShapeFrameVertices(b, frame, dv, dn, dt);

            var shape = new BlendShape
            {
                Name = mesh.GetBlendShapeName(b),
                DeltaPositions = new Vector3[count],
                DeltaNormals = new Vector3[count]
            };
            for (int i = 0; i < count; i++)
            {
                shape.DeltaPositions[i] = ConvertVector(dv[i]) * scale;
                shape.DeltaNormals[i] = ConvertVector(dn[i]) * scale;
            }
            data.BlendShapes.Add(shape);
        }
    }

    int AddMaterial(Material material)
    {
        if (material == null)
        {
            if (defaultMaterial < 0)
            {
                defaultMaterial = Materials.Count;
                Materials.Add(new MaterialData { Name = "Default" });
            }
            return defaultMaterial;
        }

        if (materialLookup.TryGetValue(material, out int existing)) return existing;

        var data = new MaterialData { Name = material.name.Replace(" (Instance)", "") };

        Texture texture = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
        if (texture is Texture2D tex2d)
        {
            data.Texture = AddTexture(tex2d);
        }

        int queue = material.renderQueue;
        if (queue >= (int)RenderQueue.Transparent)
        {
            data.Alpha = AlphaMode.Blend;
        }
        else if (queue >= (int)RenderQueue.AlphaTest)
        {
            data.Alpha = AlphaMode.Mask;
            if (material.HasProperty("_Cutoff")) data.AlphaCutoff = material.GetFloat("_Cutoff");
        }

        if (material.HasProperty("_Cull"))
        {
            data.DoubleSided = Mathf.RoundToInt(material.GetFloat("_Cull")) == (int)CullMode.Off;
        }

        int index = Materials.Count;
        Materials.Add(data);
        materialLookup[material] = index;
        return index;
    }

    int AddTexture(Texture2D texture)
    {
        if (textureLookup.TryGetValue(texture, out int existing)) return existing;

        byte[] png;
        try
        {
            png = TextureExporter.EncodeTextureToPNG(texture);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to encode texture {texture.name}: {e.Message}");
            return -1;
        }

        string baseName = SanitizeFileName(texture.name);
        string fileName = baseName + ".png";
        for (int i = 1; textureFileNames.Contains(fileName); i++)
        {
            fileName = $"{baseName}_{i}.png";
        }
        textureFileNames.Add(fileName);

        int index = Textures.Count;
        Textures.Add(new TextureData { Name = texture.name, FileName = fileName, Png = png });
        textureLookup[texture] = index;
        return index;
    }

    public static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name)) name = "texture";
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name;
    }

    /// <summary> Write all textures as PNG files into the given directory. </summary>
    public void WriteTextures(string directory)
    {
        if (Textures.Count == 0) return;
        Directory.CreateDirectory(directory);
        foreach (var texture in Textures)
        {
            File.WriteAllBytes(Path.Combine(directory, texture.FileName), texture.Png);
        }
    }
}
