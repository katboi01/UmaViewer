using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Runtime glTF 2.0 writer. Supports .gltf (JSON + .bin + PNG textures in a folder)
/// and .glb (single binary file with embedded textures).
/// </summary>
public class GltfExporter
{
    const int FLOAT = 5126;
    const int UNSIGNED_INT = 5125;
    const int UNSIGNED_SHORT = 5123;
    const int ARRAY_BUFFER = 34962;
    const int ELEMENT_ARRAY_BUFFER = 34963;

    readonly ExportScene scene;
    readonly MemoryStream buffer = new MemoryStream();
    readonly BinaryWriter bin;
    readonly JArray bufferViews = new JArray();
    readonly JArray accessors = new JArray();

    GltfExporter(ExportScene scene)
    {
        this.scene = scene;
        bin = new BinaryWriter(buffer);
    }

    public static void Export(ExportScene scene, string path, string generator)
    {
        bool binary = Path.GetExtension(path).Equals(".glb", StringComparison.OrdinalIgnoreCase);
        new GltfExporter(scene).Write(path, binary, generator);
    }

    void Write(string path, bool binary, string generator)
    {
        var directory = Path.GetDirectoryName(path);
        var baseName = Path.GetFileNameWithoutExtension(path);
        var textureFolder = baseName + "_textures";

        var root = new JObject
        {
            ["asset"] = new JObject { ["version"] = "2.0", ["generator"] = generator },
            ["scene"] = 0,
            ["scenes"] = new JArray { new JObject { ["nodes"] = new JArray { 0 } } },
        };

        var nodes = new JArray();
        var meshes = new JArray();
        var skins = new JArray();

        var meshNodes = new Dictionary<int, JObject>();
        for (int i = 0; i < scene.Nodes.Count; i++)
        {
            var node = scene.Nodes[i];
            var jnode = new JObject { ["name"] = node.Name };
            if (node.Children.Count > 0) jnode["children"] = new JArray(node.Children);
            if (node.Translation != Vector3.zero) jnode["translation"] = Vec(node.Translation);
            if (node.Rotation != Quaternion.identity) jnode["rotation"] = new JArray(node.Rotation.x, node.Rotation.y, node.Rotation.z, node.Rotation.w);
            if (node.Scale != Vector3.one) jnode["scale"] = Vec(node.Scale);
            nodes.Add(jnode);
        }

        foreach (var mesh in scene.Meshes)
        {
            var jnode = (JObject)nodes[mesh.Node];
            jnode["mesh"] = meshes.Count;
            meshes.Add(WriteMesh(mesh));

            if (mesh.IsSkinned)
            {
                jnode["skin"] = skins.Count;
                skins.Add(new JObject
                {
                    ["joints"] = new JArray(mesh.Joints),
                    ["inverseBindMatrices"] = WriteMatrices(mesh.InverseBindMatrices),
                });
            }
        }

        var materials = new JArray();
        foreach (var material in scene.Materials)
        {
            var pbr = new JObject
            {
                ["baseColorFactor"] = new JArray(material.BaseColor.r, material.BaseColor.g, material.BaseColor.b, material.BaseColor.a),
                ["metallicFactor"] = 0,
                ["roughnessFactor"] = 1,
            };
            if (material.Texture >= 0) pbr["baseColorTexture"] = new JObject { ["index"] = material.Texture };

            var jmat = new JObject { ["name"] = material.Name, ["pbrMetallicRoughness"] = pbr };
            switch (material.Alpha)
            {
                case ExportScene.AlphaMode.Blend:
                    jmat["alphaMode"] = "BLEND";
                    break;
                case ExportScene.AlphaMode.Mask:
                    jmat["alphaMode"] = "MASK";
                    jmat["alphaCutoff"] = material.AlphaCutoff;
                    break;
            }
            if (material.DoubleSided) jmat["doubleSided"] = true;
            materials.Add(jmat);
        }

        var images = new JArray();
        var textures = new JArray();
        for (int i = 0; i < scene.Textures.Count; i++)
        {
            var texture = scene.Textures[i];
            var image = new JObject { ["name"] = texture.Name };
            if (binary)
            {
                image["bufferView"] = AddBufferView(texture.Png, null);
                image["mimeType"] = "image/png";
            }
            else
            {
                image["uri"] = Uri.EscapeDataString(textureFolder) + "/" + Uri.EscapeDataString(texture.FileName);
            }
            images.Add(image);
            textures.Add(new JObject { ["sampler"] = 0, ["source"] = i });
        }

        root["nodes"] = nodes;
        if (meshes.Count > 0) root["meshes"] = meshes;
        if (skins.Count > 0) root["skins"] = skins;
        if (materials.Count > 0) root["materials"] = materials;
        if (textures.Count > 0)
        {
            root["samplers"] = new JArray { new JObject { ["magFilter"] = 9729, ["minFilter"] = 9987, ["wrapS"] = 10497, ["wrapT"] = 10497 } };
            root["images"] = images;
            root["textures"] = textures;
        }
        if (accessors.Count > 0) root["accessors"] = accessors;
        if (bufferViews.Count > 0) root["bufferViews"] = bufferViews;

        Align(4);
        var bufferBytes = buffer.ToArray();
        var jbuffer = new JObject { ["byteLength"] = bufferBytes.Length };
        if (!binary) jbuffer["uri"] = Uri.EscapeDataString(baseName + ".bin");
        root["buffers"] = new JArray { jbuffer };

        if (binary)
        {
            WriteGlb(path, root.ToString(Formatting.None), bufferBytes);
        }
        else
        {
            File.WriteAllText(path, root.ToString(Formatting.Indented), new UTF8Encoding(false));
            File.WriteAllBytes(Path.Combine(directory, baseName + ".bin"), bufferBytes);
            scene.WriteTextures(Path.Combine(directory, textureFolder));
        }
    }

    JObject WriteMesh(ExportScene.MeshData mesh)
    {
        int count = mesh.Positions.Length;
        var attributes = new JObject
        {
            ["POSITION"] = WriteVec3(mesh.Positions, true),
            ["NORMAL"] = WriteVec3(mesh.Normals, false),
        };

        // glTF UV origin is top-left
        var uv = new Vector2[count];
        for (int i = 0; i < count; i++) uv[i] = new Vector2(mesh.UV[i].x, 1 - mesh.UV[i].y);
        attributes["TEXCOORD_0"] = WriteVec2(uv);

        if (mesh.IsSkinned)
        {
            attributes["JOINTS_0"] = WriteJoints(mesh.JointIndices);
            attributes["WEIGHTS_0"] = WriteVec4(mesh.JointWeights);
        }

        JArray targets = null;
        if (mesh.BlendShapes.Count > 0)
        {
            targets = new JArray();
            foreach (var shape in mesh.BlendShapes)
            {
                targets.Add(WriteMorphTarget(shape));
            }
        }

        var primitives = new JArray();
        for (int s = 0; s < mesh.Submeshes.Count; s++)
        {
            var primitive = new JObject
            {
                ["attributes"] = attributes,
                ["indices"] = WriteIndices(mesh.Submeshes[s]),
                ["material"] = mesh.Materials[s],
                ["mode"] = 4,
            };
            if (targets != null) primitive["targets"] = targets;
            primitives.Add(primitive);
        }

        var jmesh = new JObject { ["name"] = mesh.Name, ["primitives"] = primitives };
        if (targets != null)
        {
            var names = new JArray();
            var weights = new JArray();
            foreach (var shape in mesh.BlendShapes)
            {
                names.Add(shape.Name);
                weights.Add(0);
            }
            jmesh["weights"] = weights;
            jmesh["extras"] = new JObject { ["targetNames"] = names };
        }
        return jmesh;
    }

    JObject WriteMorphTarget(ExportScene.BlendShape shape)
    {
        int count = shape.DeltaPositions.Length;
        var changed = new List<int>();
        for (int i = 0; i < count; i++)
        {
            if (shape.DeltaPositions[i] != Vector3.zero || shape.DeltaNormals[i] != Vector3.zero) changed.Add(i);
        }

        var min = Vector3.zero;
        var max = Vector3.zero;
        foreach (var i in changed)
        {
            min = Vector3.Min(min, shape.DeltaPositions[i]);
            max = Vector3.Max(max, shape.DeltaPositions[i]);
        }

        if (changed.Count == 0)
        {
            return new JObject
            {
                ["POSITION"] = AddAccessor(null, FLOAT, count, "VEC3", Vec(min), Vec(max)),
                ["NORMAL"] = AddAccessor(null, FLOAT, count, "VEC3"),
            };
        }

        Align(4);
        int indicesStart = (int)buffer.Position;
        foreach (var i in changed) bin.Write((uint)i);
        int indicesView = AddBufferViewRange(indicesStart, (int)buffer.Position - indicesStart, null);

        Func<Vector3[], int> writeValues = values =>
        {
            Align(4);
            int start = (int)buffer.Position;
            foreach (var i in changed)
            {
                bin.Write(values[i].x); bin.Write(values[i].y); bin.Write(values[i].z);
            }
            return AddBufferViewRange(start, (int)buffer.Position - start, null);
        };

        Func<int, JObject> sparse = valuesView => new JObject
        {
            ["count"] = changed.Count,
            ["indices"] = new JObject { ["bufferView"] = indicesView, ["componentType"] = UNSIGNED_INT },
            ["values"] = new JObject { ["bufferView"] = valuesView },
        };

        int position = AddAccessor(null, FLOAT, count, "VEC3", Vec(min), Vec(max));
        ((JObject)accessors[position])["sparse"] = sparse(writeValues(shape.DeltaPositions));
        int normal = AddAccessor(null, FLOAT, count, "VEC3");
        ((JObject)accessors[normal])["sparse"] = sparse(writeValues(shape.DeltaNormals));

        return new JObject { ["POSITION"] = position, ["NORMAL"] = normal };
    }

    int WriteVec3(Vector3[] values, bool bounds)
    {
        Align(4);
        int start = (int)buffer.Position;
        var min = Vector3.positiveInfinity;
        var max = Vector3.negativeInfinity;
        foreach (var v in values)
        {
            bin.Write(v.x); bin.Write(v.y); bin.Write(v.z);
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }
        int view = AddBufferViewRange(start, (int)buffer.Position - start, ARRAY_BUFFER);
        return bounds
            ? AddAccessor(view, FLOAT, values.Length, "VEC3", Vec(min), Vec(max))
            : AddAccessor(view, FLOAT, values.Length, "VEC3");
    }

    int WriteVec2(Vector2[] values)
    {
        Align(4);
        int start = (int)buffer.Position;
        foreach (var v in values)
        {
            bin.Write(v.x); bin.Write(v.y);
        }
        int view = AddBufferViewRange(start, (int)buffer.Position - start, ARRAY_BUFFER);
        return AddAccessor(view, FLOAT, values.Length, "VEC2");
    }

    int WriteVec4(Vector4[] values)
    {
        Align(4);
        int start = (int)buffer.Position;
        foreach (var v in values)
        {
            bin.Write(v.x); bin.Write(v.y); bin.Write(v.z); bin.Write(v.w);
        }
        int view = AddBufferViewRange(start, (int)buffer.Position - start, ARRAY_BUFFER);
        return AddAccessor(view, FLOAT, values.Length, "VEC4");
    }

    int WriteJoints(ExportScene.Vector4Int[] values)
    {
        Align(4);
        int start = (int)buffer.Position;
        foreach (var v in values)
        {
            bin.Write((ushort)v.x); bin.Write((ushort)v.y); bin.Write((ushort)v.z); bin.Write((ushort)v.w);
        }
        int view = AddBufferViewRange(start, (int)buffer.Position - start, ARRAY_BUFFER);
        return AddAccessor(view, UNSIGNED_SHORT, values.Length, "VEC4");
    }

    int WriteIndices(int[] indices)
    {
        Align(4);
        int start = (int)buffer.Position;
        foreach (var i in indices) bin.Write((uint)i);
        int view = AddBufferViewRange(start, (int)buffer.Position - start, ELEMENT_ARRAY_BUFFER);
        return AddAccessor(view, UNSIGNED_INT, indices.Length, "SCALAR");
    }

    int WriteMatrices(Matrix4x4[] matrices)
    {
        Align(4);
        int start = (int)buffer.Position;
        foreach (var m in matrices)
        {
            // Matrix4x4 indexer is column-major, matching glTF
            for (int i = 0; i < 16; i++) bin.Write(m[i]);
        }
        int view = AddBufferViewRange(start, (int)buffer.Position - start, null);
        return AddAccessor(view, FLOAT, matrices.Length, "MAT4");
    }

    int AddAccessor(int? bufferView, int componentType, int count, string type, JArray min = null, JArray max = null)
    {
        var accessor = new JObject { ["componentType"] = componentType, ["count"] = count, ["type"] = type };
        if (bufferView.HasValue) accessor["bufferView"] = bufferView.Value;
        if (min != null) accessor["min"] = min;
        if (max != null) accessor["max"] = max;
        accessors.Add(accessor);
        return accessors.Count - 1;
    }

    int AddBufferView(byte[] data, int? target)
    {
        Align(4);
        int start = (int)buffer.Position;
        bin.Write(data);
        return AddBufferViewRange(start, data.Length, target);
    }

    int AddBufferViewRange(int offset, int length, int? target)
    {
        var view = new JObject { ["buffer"] = 0, ["byteOffset"] = offset, ["byteLength"] = length };
        if (target.HasValue) view["target"] = target.Value;
        bufferViews.Add(view);
        return bufferViews.Count - 1;
    }

    void Align(int alignment)
    {
        while (buffer.Position % alignment != 0) bin.Write((byte)0);
    }

    static JArray Vec(Vector3 v) => new JArray(v.x, v.y, v.z);

    static void WriteGlb(string path, string json, byte[] binChunk)
    {
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        int jsonPadded = (jsonBytes.Length + 3) & ~3;
        int binPadded = (binChunk.Length + 3) & ~3;
        int total = 12 + 8 + jsonPadded + (binChunk.Length > 0 ? 8 + binPadded : 0);

        using (var writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write(0x46546C67u); // "glTF"
            writer.Write(2u);
            writer.Write((uint)total);

            writer.Write((uint)jsonPadded);
            writer.Write(0x4E4F534Au); // "JSON"
            writer.Write(jsonBytes);
            for (int i = jsonBytes.Length; i < jsonPadded; i++) writer.Write((byte)0x20);

            if (binChunk.Length > 0)
            {
                writer.Write((uint)binPadded);
                writer.Write(0x004E4942u); // "BIN\0"
                writer.Write(binChunk);
                for (int i = binChunk.Length; i < binPadded; i++) writer.Write((byte)0);
            }
        }
    }
}
