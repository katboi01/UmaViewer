using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Runtime binary FBX 7.4 writer (the Unity FBX Exporter package only works in the editor).
/// Writes the hierarchy, meshes, skinning, blend shapes and materials with embedded textures.
/// Textures are also written as PNG files next to the model.
/// </summary>
public class FbxExporter
{
    #region Binary node writer

    public class FbxNode
    {
        public string Name;
        public List<object> Properties = new List<object>();
        public List<FbxNode> Children = new List<FbxNode>();

        public FbxNode(string name, params object[] properties)
        {
            Name = name;
            Properties.AddRange(properties);
        }

        public FbxNode Add(string name, params object[] properties)
        {
            var node = new FbxNode(name, properties);
            Children.Add(node);
            return node;
        }

        public FbxNode Add(FbxNode node)
        {
            Children.Add(node);
            return node;
        }
    }

    /// <summary> Raw binary property ('R'). </summary>
    public class FbxRaw
    {
        public byte[] Data;
        public FbxRaw(byte[] data) { Data = data; }
    }

    const int Version = 7400;
    static readonly byte[] HeadMagic = Encoding.ASCII.GetBytes("Kaydara FBX Binary  \0\x1a\0");
    static readonly byte[] FootId = { 0xfa, 0xbc, 0xab, 0x09, 0xd0, 0xc8, 0xd4, 0x66, 0xb1, 0x76, 0xfb, 0x83, 0x1c, 0xf7, 0x26, 0x7e };
    static readonly byte[] FootMagic = { 0xf8, 0x5a, 0x8c, 0x6a, 0xde, 0xf5, 0xd9, 0x7e, 0xec, 0xe9, 0x0c, 0xe3, 0x75, 0x8f, 0x29, 0x0b };
    // FileId / CreationTime pair that Autodesk tools accept together with FootId.
    static readonly byte[] FileId = { 0x28, 0xb3, 0x2a, 0xeb, 0xb6, 0x24, 0xcc, 0xc2, 0xbf, 0xc8, 0xb0, 0x2a, 0xa9, 0x2b, 0xfc, 0xf1 };
    const string CreationTime = "1970-01-01 10:00:00:000";

    static void WriteFile(string path, List<FbxNode> nodes)
    {
        using (var writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write(HeadMagic);
            writer.Write((uint)Version);
            foreach (var node in nodes) WriteNode(writer, node);
            writer.Write(new byte[13]);

            writer.Write(FootId);
            writer.Write(new byte[4]);
            long ofs = writer.BaseStream.Position;
            int pad = (int)(((ofs + 15) & ~15) - ofs);
            if (pad == 0) pad = 16;
            writer.Write(new byte[pad]);
            writer.Write((uint)Version);
            writer.Write(new byte[120]);
            writer.Write(FootMagic);
        }
    }

    static void WriteNode(BinaryWriter writer, FbxNode node)
    {
        var stream = writer.BaseStream;
        long start = stream.Position;
        writer.Write(0u); // end offset, patched below
        writer.Write((uint)node.Properties.Count);
        writer.Write(0u); // property list length, patched below
        var name = Encoding.ASCII.GetBytes(node.Name);
        writer.Write((byte)name.Length);
        writer.Write(name);

        long propStart = stream.Position;
        foreach (var p in node.Properties) WriteProperty(writer, p);
        long propEnd = stream.Position;

        if (node.Children.Count > 0 || node.Properties.Count == 0)
        {
            foreach (var child in node.Children) WriteNode(writer, child);
            writer.Write(new byte[13]);
        }

        long end = stream.Position;
        stream.Position = start;
        writer.Write((uint)end);
        writer.Write((uint)node.Properties.Count);
        writer.Write((uint)(propEnd - propStart));
        stream.Position = end;
    }

    static void WriteProperty(BinaryWriter writer, object p)
    {
        switch (p)
        {
            case bool b: writer.Write((byte)'C'); writer.Write((byte)(b ? 1 : 0)); break;
            case short s: writer.Write((byte)'Y'); writer.Write(s); break;
            case int i: writer.Write((byte)'I'); writer.Write(i); break;
            case long l: writer.Write((byte)'L'); writer.Write(l); break;
            case float f: writer.Write((byte)'F'); writer.Write(f); break;
            case double d: writer.Write((byte)'D'); writer.Write(d); break;
            case string str:
                {
                    var bytes = Encoding.UTF8.GetBytes(str);
                    writer.Write((byte)'S'); writer.Write((uint)bytes.Length); writer.Write(bytes);
                    break;
                }
            case FbxRaw raw: writer.Write((byte)'R'); writer.Write((uint)raw.Data.Length); writer.Write(raw.Data); break;
            case int[] ia:
                writer.Write((byte)'i'); WriteArrayHeader(writer, ia.Length, 4);
                foreach (var v in ia) writer.Write(v);
                break;
            case long[] la:
                writer.Write((byte)'l'); WriteArrayHeader(writer, la.Length, 8);
                foreach (var v in la) writer.Write(v);
                break;
            case float[] fa:
                writer.Write((byte)'f'); WriteArrayHeader(writer, fa.Length, 4);
                foreach (var v in fa) writer.Write(v);
                break;
            case double[] da:
                writer.Write((byte)'d'); WriteArrayHeader(writer, da.Length, 8);
                foreach (var v in da) writer.Write(v);
                break;
            default:
                throw new ArgumentException($"Unsupported FBX property type {p?.GetType()}");
        }
    }

    static void WriteArrayHeader(BinaryWriter writer, int length, int elementSize)
    {
        writer.Write((uint)length);
        writer.Write(0u); // no compression
        writer.Write((uint)(length * elementSize));
    }

    #endregion

    readonly ExportScene scene;
    readonly FbxNode objects = new FbxNode("Objects");
    readonly FbxNode connections = new FbxNode("Connections");
    readonly Dictionary<string, int> objectCounts = new Dictionary<string, int>();
    long nextId = 1000000;

    FbxExporter(ExportScene scene)
    {
        this.scene = scene;
    }

    public static void Export(ExportScene scene, string path, string creator)
    {
        new FbxExporter(scene).Write(path, creator);
    }

    long NewId() => nextId++;

    static string ObjName(string name, string cls) => name + "\0\x01" + cls;

    void Count(string type) => objectCounts[type] = objectCounts.TryGetValue(type, out int c) ? c + 1 : 1;

    void ConnectOO(long child, long parent) => connections.Add("C", "OO", child, parent);

    void ConnectOP(long child, long parent, string property) => connections.Add("C", "OP", child, parent, property);

    static FbxNode P(string name, string type, string label, string flags, params object[] values)
    {
        var node = new FbxNode("P", name, type, label, flags);
        node.Properties.AddRange(values);
        return node;
    }

    void Write(string path, string creator)
    {
        var directory = Path.GetDirectoryName(path);
        var baseName = Path.GetFileNameWithoutExtension(path);
        var textureFolder = baseName + "_textures";

        var roots = new List<FbxNode>();
        roots.Add(BuildHeader(creator));
        roots.Add(new FbxNode("FileId", new FbxRaw(FileId)));
        roots.Add(new FbxNode("CreationTime", CreationTime));
        roots.Add(new FbxNode("Creator", creator));
        roots.Add(BuildGlobalSettings());

        var documents = new FbxNode("Documents");
        documents.Add("Count", 1);
        var document = documents.Add("Document", NewId(), "Scene", "Scene");
        var docProps = document.Add("Properties70");
        docProps.Add(P("SourceObject", "object", "", ""));
        docProps.Add(P("ActiveAnimStackName", "KString", "", "", ""));
        document.Add("RootNode", 0L);
        roots.Add(documents);
        roots.Add(new FbxNode("References"));

        // Textures and materials
        var textureIds = new long[scene.Textures.Count];
        for (int i = 0; i < scene.Textures.Count; i++)
        {
            textureIds[i] = AddTexture(scene.Textures[i], textureFolder);
        }
        var materialIds = new long[scene.Materials.Count];
        for (int i = 0; i < scene.Materials.Count; i++)
        {
            var material = scene.Materials[i];
            materialIds[i] = AddMaterial(material);
            if (material.Texture >= 0)
            {
                ConnectOP(textureIds[material.Texture], materialIds[i], "DiffuseColor");
            }
        }

        // Bind pose matrices for joints come from the first skin that references them
        var bindMatrices = new Dictionary<int, Matrix4x4>();
        foreach (var mesh in scene.Meshes)
        {
            if (!mesh.IsSkinned) continue;
            var meshWorld = scene.Nodes[mesh.Node].World;
            bindMatrices[mesh.Node] = meshWorld;
            for (int j = 0; j < mesh.Joints.Length; j++)
            {
                if (!bindMatrices.ContainsKey(mesh.Joints[j]))
                {
                    bindMatrices[mesh.Joints[j]] = meshWorld * mesh.InverseBindMatrices[j].inverse;
                }
            }
        }

        // Models
        var modelIds = new long[scene.Nodes.Count];
        for (int i = 0; i < scene.Nodes.Count; i++)
        {
            modelIds[i] = AddModel(scene.Nodes[i]);
            ConnectOO(modelIds[i], scene.Nodes[i].Parent >= 0 ? modelIds[scene.Nodes[i].Parent] : 0L);
        }

        // Geometry
        foreach (var mesh in scene.Meshes)
        {
            long geometryId = AddGeometry(mesh);
            long modelId = modelIds[mesh.Node];
            ConnectOO(geometryId, modelId);

            var used = new List<int>();
            foreach (var m in mesh.Materials)
            {
                if (!used.Contains(m)) used.Add(m);
            }
            foreach (var m in used) ConnectOO(materialIds[m], modelId);

            if (mesh.IsSkinned) AddSkin(mesh, geometryId, modelIds);
            if (mesh.BlendShapes.Count > 0) AddBlendShapes(mesh, geometryId);
        }

        AddBindPose(bindMatrices, modelIds);

        roots.Add(BuildDefinitions());
        roots.Add(objects);
        roots.Add(connections);
        var takes = new FbxNode("Takes");
        takes.Add("Current", "");
        roots.Add(takes);

        WriteFile(path, roots);
        scene.WriteTextures(Path.Combine(directory, textureFolder));
    }

    FbxNode BuildHeader(string creator)
    {
        var header = new FbxNode("FBXHeaderExtension");
        header.Add("FBXHeaderVersion", 1003);
        header.Add("FBXVersion", Version);
        header.Add("EncryptionType", 0);
        var now = DateTime.Now;
        var stamp = header.Add("CreationTimeStamp");
        stamp.Add("Version", 1000);
        stamp.Add("Year", now.Year);
        stamp.Add("Month", now.Month);
        stamp.Add("Day", now.Day);
        stamp.Add("Hour", now.Hour);
        stamp.Add("Minute", now.Minute);
        stamp.Add("Second", now.Second);
        stamp.Add("Millisecond", now.Millisecond);
        header.Add("Creator", creator);
        return header;
    }

    static FbxNode BuildGlobalSettings()
    {
        var settings = new FbxNode("GlobalSettings");
        settings.Add("Version", 1000);
        var props = settings.Add("Properties70");
        // Right-handed, Y-up, +Z front
        props.Add(P("UpAxis", "int", "Integer", "", 1));
        props.Add(P("UpAxisSign", "int", "Integer", "", 1));
        props.Add(P("FrontAxis", "int", "Integer", "", 2));
        props.Add(P("FrontAxisSign", "int", "Integer", "", 1));
        props.Add(P("CoordAxis", "int", "Integer", "", 0));
        props.Add(P("CoordAxisSign", "int", "Integer", "", 1));
        props.Add(P("OriginalUpAxis", "int", "Integer", "", 1));
        props.Add(P("OriginalUpAxisSign", "int", "Integer", "", 1));
        // Positions are in meters
        props.Add(P("UnitScaleFactor", "double", "Number", "", 100.0));
        props.Add(P("OriginalUnitScaleFactor", "double", "Number", "", 100.0));
        props.Add(P("AmbientColor", "ColorRGB", "Color", "", 0.0, 0.0, 0.0));
        props.Add(P("DefaultCamera", "KString", "", "", "Producer Perspective"));
        props.Add(P("TimeMode", "enum", "", "", 11));
        props.Add(P("TimeSpanStart", "KTime", "Time", "", 0L));
        props.Add(P("TimeSpanStop", "KTime", "Time", "", 46186158000L));
        props.Add(P("CustomFrameRate", "double", "Number", "", -1.0));
        return settings;
    }

    FbxNode BuildDefinitions()
    {
        var definitions = new FbxNode("Definitions");
        definitions.Add("Version", 100);
        int total = 1;
        foreach (var c in objectCounts.Values) total += c;
        definitions.Add("Count", total);
        definitions.Add("ObjectType", "GlobalSettings").Add("Count", 1);
        foreach (var pair in objectCounts)
        {
            definitions.Add("ObjectType", pair.Key).Add("Count", pair.Value);
        }
        return definitions;
    }

    long AddModel(ExportScene.Node node)
    {
        long id = NewId();
        string type = node.Mesh >= 0 ? "Mesh" : node.IsJoint ? "LimbNode" : "Null";

        if (type != "Mesh")
        {
            long attrId = NewId();
            var attr = objects.Add("NodeAttribute", attrId, ObjName(node.Name, "NodeAttribute"), type);
            if (type == "LimbNode")
            {
                attr.Add("Properties70").Add(P("Size", "double", "Number", "", 1.0));
                attr.Add("TypeFlags", "Skeleton");
            }
            else
            {
                attr.Add("TypeFlags", "Null");
            }
            Count("NodeAttribute");
            ConnectOO(attrId, id);
        }

        var model = objects.Add("Model", id, ObjName(node.Name, "Model"), type);
        model.Add("Version", 232);
        var props = model.Add("Properties70");
        props.Add(P("RotationActive", "bool", "", "", 1));
        props.Add(P("InheritType", "enum", "", "", 1));
        props.Add(P("ScalingMax", "Vector3D", "Vector", "", 0.0, 0.0, 0.0));
        props.Add(P("DefaultAttributeIndex", "int", "Integer", "", 0));
        var t = node.Translation;
        var r = ToEulerXYZ(node.Rotation);
        var s = node.Scale;
        props.Add(P("Lcl Translation", "Lcl Translation", "", "A", (double)t.x, (double)t.y, (double)t.z));
        props.Add(P("Lcl Rotation", "Lcl Rotation", "", "A", (double)r.x, (double)r.y, (double)r.z));
        props.Add(P("Lcl Scaling", "Lcl Scaling", "", "A", (double)s.x, (double)s.y, (double)s.z));
        model.Add("Shading", true);
        model.Add("Culling", "CullingOff");
        Count("Model");
        return id;
    }

    /// <summary> Convert to FBX eXYZ Euler angles in degrees (R = Rz * Ry * Rx). </summary>
    static Vector3 ToEulerXYZ(Quaternion q)
    {
        var m = Matrix4x4.Rotate(q.normalized);
        double x, y, z;
        double sy = -m.m20;
        if (Math.Abs(sy) < 0.999999)
        {
            y = Math.Asin(sy);
            x = Math.Atan2(m.m21, m.m22);
            z = Math.Atan2(m.m10, m.m00);
        }
        else
        {
            y = sy > 0 ? Math.PI / 2 : -Math.PI / 2;
            x = Math.Atan2(-m.m12, m.m11);
            z = 0;
        }
        const double rad2deg = 180.0 / Math.PI;
        return new Vector3((float)(x * rad2deg), (float)(y * rad2deg), (float)(z * rad2deg));
    }

    long AddGeometry(ExportScene.MeshData mesh)
    {
        long id = NewId();
        var geometry = objects.Add("Geometry", id, ObjName(mesh.Name, "Geometry"), "Mesh");
        geometry.Add("Properties70");
        geometry.Add("GeometryVersion", 124);

        int count = mesh.Positions.Length;
        var vertices = new double[count * 3];
        for (int i = 0; i < count; i++)
        {
            vertices[i * 3] = mesh.Positions[i].x;
            vertices[i * 3 + 1] = mesh.Positions[i].y;
            vertices[i * 3 + 2] = mesh.Positions[i].z;
        }
        geometry.Add("Vertices", vertices);

        int indexCount = 0;
        foreach (var sub in mesh.Submeshes) indexCount += sub.Length / 3 * 3;
        var polygonIndices = new int[indexCount];
        var polygonMaterials = new int[indexCount / 3];
        var materialSlots = new List<int>();
        int k = 0;
        for (int s = 0; s < mesh.Submeshes.Count; s++)
        {
            int slot = materialSlots.IndexOf(mesh.Materials[s]);
            if (slot < 0)
            {
                slot = materialSlots.Count;
                materialSlots.Add(mesh.Materials[s]);
            }
            var sub = mesh.Submeshes[s];
            for (int i = 0; i + 2 < sub.Length; i += 3)
            {
                polygonMaterials[k / 3] = slot;
                polygonIndices[k++] = sub[i];
                polygonIndices[k++] = sub[i + 1];
                polygonIndices[k++] = ~sub[i + 2]; // negative marks end of polygon
            }
        }
        geometry.Add("PolygonVertexIndex", polygonIndices);

        // Normals, by polygon vertex
        var normals = new double[indexCount * 3];
        for (int i = 0; i < indexCount; i++)
        {
            int v = polygonIndices[i] < 0 ? ~polygonIndices[i] : polygonIndices[i];
            normals[i * 3] = mesh.Normals[v].x;
            normals[i * 3 + 1] = mesh.Normals[v].y;
            normals[i * 3 + 2] = mesh.Normals[v].z;
        }
        var normalLayer = geometry.Add("LayerElementNormal", 0);
        normalLayer.Add("Version", 101);
        normalLayer.Add("Name", "");
        normalLayer.Add("MappingInformationType", "ByPolygonVertex");
        normalLayer.Add("ReferenceInformationType", "Direct");
        normalLayer.Add("Normals", normals);

        // UVs, indexed per polygon vertex
        var uv = new double[count * 2];
        for (int i = 0; i < count; i++)
        {
            uv[i * 2] = mesh.UV[i].x;
            uv[i * 2 + 1] = mesh.UV[i].y;
        }
        var uvIndex = new int[indexCount];
        for (int i = 0; i < indexCount; i++)
        {
            uvIndex[i] = polygonIndices[i] < 0 ? ~polygonIndices[i] : polygonIndices[i];
        }
        var uvLayer = geometry.Add("LayerElementUV", 0);
        uvLayer.Add("Version", 101);
        uvLayer.Add("Name", "UVMap");
        uvLayer.Add("MappingInformationType", "ByPolygonVertex");
        uvLayer.Add("ReferenceInformationType", "IndexToDirect");
        uvLayer.Add("UV", uv);
        uvLayer.Add("UVIndex", uvIndex);

        var materialLayer = geometry.Add("LayerElementMaterial", 0);
        materialLayer.Add("Version", 101);
        materialLayer.Add("Name", "");
        materialLayer.Add("MappingInformationType", "ByPolygon");
        materialLayer.Add("ReferenceInformationType", "IndexToDirect");
        materialLayer.Add("Materials", polygonMaterials);

        var layer = geometry.Add("Layer", 0);
        layer.Add("Version", 100);
        foreach (var type in new[] { "LayerElementNormal", "LayerElementMaterial", "LayerElementUV" })
        {
            var element = layer.Add("LayerElement");
            element.Add("Type", type);
            element.Add("TypedIndex", 0);
        }

        Count("Geometry");
        return id;
    }

    void AddSkin(ExportScene.MeshData mesh, long geometryId, long[] modelIds)
    {
        long skinId = NewId();
        var skin = objects.Add("Deformer", skinId, ObjName(mesh.Name, "Deformer"), "Skin");
        skin.Add("Version", 101);
        skin.Add("Link_DeformAcuracy", 50.0);
        Count("Deformer");
        ConnectOO(skinId, geometryId);

        var meshWorld = scene.Nodes[mesh.Node].World;

        // Gather influences per joint
        int jointCount = mesh.Joints.Length;
        var indices = new List<int>[jointCount];
        var weights = new List<double>[jointCount];
        for (int j = 0; j < jointCount; j++)
        {
            indices[j] = new List<int>();
            weights[j] = new List<double>();
        }
        for (int v = 0; v < mesh.JointIndices.Length; v++)
        {
            var ji = mesh.JointIndices[v];
            var jw = mesh.JointWeights[v];
            for (int c = 0; c < 4; c++)
            {
                if (jw[c] <= 0) continue;
                indices[ji[c]].Add(v);
                weights[ji[c]].Add(jw[c]);
            }
        }

        // FBX allows only one cluster per bone; merge duplicate joints.
        var clusterByNode = new Dictionary<int, int>();
        for (int j = 0; j < jointCount; j++)
        {
            int node = mesh.Joints[j];
            if (clusterByNode.TryGetValue(node, out int first))
            {
                indices[first].AddRange(indices[j]);
                weights[first].AddRange(weights[j]);
                indices[j] = null;
                continue;
            }
            clusterByNode[node] = j;
        }

        for (int j = 0; j < jointCount; j++)
        {
            if (indices[j] == null) continue;
            int node = mesh.Joints[j];
            long clusterId = NewId();
            var cluster = objects.Add("Deformer", clusterId, ObjName(scene.Nodes[node].Name, "SubDeformer"), "Cluster");
            cluster.Add("Version", 100);
            cluster.Add("UserData", "", "");
            cluster.Add("Indexes", indices[j].ToArray());
            cluster.Add("Weights", weights[j].ToArray());
            cluster.Add("Transform", ToArray(mesh.InverseBindMatrices[j]));
            cluster.Add("TransformLink", ToArray(meshWorld * mesh.InverseBindMatrices[j].inverse));
            Count("Deformer");
            ConnectOO(clusterId, skinId);
            ConnectOO(modelIds[node], clusterId);
        }
    }

    void AddBlendShapes(ExportScene.MeshData mesh, long geometryId)
    {
        long blendId = NewId();
        var blend = objects.Add("Deformer", blendId, ObjName(mesh.Name, "Deformer"), "BlendShape");
        blend.Add("Version", 100);
        Count("Deformer");
        ConnectOO(blendId, geometryId);

        foreach (var shape in mesh.BlendShapes)
        {
            var indexList = new List<int>();
            for (int i = 0; i < shape.DeltaPositions.Length; i++)
            {
                if (shape.DeltaPositions[i] != Vector3.zero || shape.DeltaNormals[i] != Vector3.zero) indexList.Add(i);
            }
            var vertices = new double[indexList.Count * 3];
            var normals = new double[indexList.Count * 3];
            for (int n = 0; n < indexList.Count; n++)
            {
                var dv = shape.DeltaPositions[indexList[n]];
                var dn = shape.DeltaNormals[indexList[n]];
                vertices[n * 3] = dv.x; vertices[n * 3 + 1] = dv.y; vertices[n * 3 + 2] = dv.z;
                normals[n * 3] = dn.x; normals[n * 3 + 1] = dn.y; normals[n * 3 + 2] = dn.z;
            }

            long channelId = NewId();
            var channel = objects.Add("Deformer", channelId, ObjName(shape.Name, "SubDeformer"), "BlendShapeChannel");
            channel.Add("Version", 100);
            channel.Add("DeformPercent", 0.0);
            channel.Add("FullWeights", new double[] { 100.0 });
            Count("Deformer");
            ConnectOO(channelId, blendId);

            long shapeId = NewId();
            var geometry = objects.Add("Geometry", shapeId, ObjName(shape.Name, "Geometry"), "Shape");
            geometry.Add("Version", 100);
            geometry.Add("Indexes", indexList.ToArray());
            geometry.Add("Vertices", vertices);
            geometry.Add("Normals", normals);
            Count("Geometry");
            ConnectOO(shapeId, channelId);
        }
    }

    void AddBindPose(Dictionary<int, Matrix4x4> bindMatrices, long[] modelIds)
    {
        if (bindMatrices.Count == 0) return;
        var pose = objects.Add("Pose", NewId(), ObjName("BindPose", "Pose"), "BindPose");
        pose.Add("Type", "BindPose");
        pose.Add("Version", 100);
        pose.Add("NbPoseNodes", bindMatrices.Count);
        foreach (var pair in bindMatrices)
        {
            var poseNode = pose.Add("PoseNode");
            poseNode.Add("Node", modelIds[pair.Key]);
            poseNode.Add("Matrix", ToArray(pair.Value));
        }
        Count("Pose");
    }

    long AddMaterial(ExportScene.MaterialData material)
    {
        long id = NewId();
        var node = objects.Add("Material", id, ObjName(material.Name, "Material"), "");
        node.Add("Version", 102);
        node.Add("ShadingModel", "lambert");
        node.Add("MultiLayer", 0);
        var props = node.Add("Properties70");
        var c = material.BaseColor;
        props.Add(P("DiffuseColor", "Color", "", "A", (double)c.r, (double)c.g, (double)c.b));
        props.Add(P("DiffuseFactor", "Number", "", "A", 1.0));
        props.Add(P("AmbientColor", "Color", "", "A", 0.0, 0.0, 0.0));
        props.Add(P("Emissive", "Vector3D", "Vector", "", 0.0, 0.0, 0.0));
        props.Add(P("Ambient", "Vector3D", "Vector", "", 0.0, 0.0, 0.0));
        props.Add(P("Diffuse", "Vector3D", "Vector", "", (double)c.r, (double)c.g, (double)c.b));
        props.Add(P("Opacity", "double", "Number", "", (double)c.a));
        Count("Material");
        return id;
    }

    long AddTexture(ExportScene.TextureData texture, string textureFolder)
    {
        string relative = textureFolder + "/" + texture.FileName;

        long videoId = NewId();
        var video = objects.Add("Video", videoId, ObjName(texture.Name, "Video"), "Clip");
        video.Add("Type", "Clip");
        video.Add("Properties70").Add(P("Path", "KString", "XRefUrl", "", relative));
        video.Add("UseMipMap", 0);
        video.Add("Filename", relative);
        video.Add("RelativeFilename", relative);
        video.Add("Content", new FbxRaw(texture.Png));
        Count("Video");

        long textureId = NewId();
        var node = objects.Add("Texture", textureId, ObjName(texture.Name, "Texture"), "");
        node.Add("Type", "TextureVideoClip");
        node.Add("Version", 202);
        node.Add("TextureName", ObjName(texture.Name, "Texture"));
        var props = node.Add("Properties70");
        props.Add(P("UVSet", "KString", "", "", "UVMap"));
        props.Add(P("UseMaterial", "bool", "", "", 1));
        node.Add("Media", ObjName(texture.Name, "Video"));
        node.Add("FileName", relative);
        node.Add("RelativeFilename", relative);
        node.Add("ModelUVTranslation", 0.0, 0.0);
        node.Add("ModelUVScaling", 1.0, 1.0);
        node.Add("Texture_Alpha_Source", "None");
        node.Add("Cropping", 0, 0, 0, 0);
        Count("Texture");

        ConnectOO(videoId, textureId);
        return textureId;
    }

    static double[] ToArray(Matrix4x4 m)
    {
        var result = new double[16];
        for (int i = 0; i < 16; i++) result[i] = m[i]; // column-major
        return result;
    }
}
