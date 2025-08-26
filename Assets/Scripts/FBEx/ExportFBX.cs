using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace FBEx
{
    public class ExportFBX : MonoBehaviour
    {
        public bool Skinned = true;
        string ExportPath;
        List<Transform> ImportantList = new List<Transform>();
        Dictionary<Transform, Model> ModelDict = new Dictionary<Transform, Model>();
        Dictionary<Transform, NodeAttribute> NodeDict = new Dictionary<Transform, NodeAttribute>();
        Dictionary<UnityEngine.Texture, Texture> TextureDict = new Dictionary<UnityEngine.Texture, Texture>();
        Dictionary<UnityEngine.Material, Material> MaterialDict = new Dictionary<UnityEngine.Material, Material>();

        // Start is called before the first frame update
        void Start()
        {
            var file = new FbxFile();
            ExportPath = Application.dataPath + $"/../Export/{transform.name}";
            Directory.CreateDirectory(ExportPath);
            Directory.CreateDirectory(ExportPath + "/Textures");

            int objCount = 0;

            var root = this.transform;

            BuildHierarchyOnly(file, null, root, ref objCount);
            AddMaterials(file, root, ref objCount);
            if (Skinned)
            {
                BuildSkinnedMeshRenderers(file, root, ref objCount);
            }
            BuildMeshFilters(file, root, ref objCount);
            RemoveUnimportant(file, root);

            file.Write(ExportPath + $"/{transform.name}.fbx");

            ImportantList.Clear();
            ModelDict.Clear();
            NodeDict.Clear();
            TextureDict.Clear();
            MaterialDict.Clear();
        }

        public Mesh MakeReadableMeshCopy(Mesh nonReadableMesh)
        {
            return nonReadableMesh;
            ////ONLY FOR UNITY 2021+ !
            //Mesh meshCopy = new Mesh();
            //meshCopy.indexFormat = nonReadableMesh.indexFormat;

            //var desc_orig = nonReadableMesh.GetVertexAttributes();
            //var desc_copy = desc_orig.ToArray();
            //for(int i = 0; i < desc_copy.Length; i++)
            //{
            //    desc_copy[i].stream = 0;
            //}
            //nonReadableMesh.SetVertexBufferParams(nonReadableMesh.vertexCount, desc_copy);

            //// Handle vertices
            //GraphicsBuffer verticesBuffer = nonReadableMesh.GetVertexBuffer(0);
            //int totalSize = verticesBuffer.stride * verticesBuffer.count;
            //byte[] data = new byte[totalSize];
            //verticesBuffer.GetData(data);
            //meshCopy.SetVertexBufferParams(nonReadableMesh.vertexCount, nonReadableMesh.GetVertexAttributes());
            //meshCopy.SetVertexBufferData(data, 0, 0, totalSize);
            //verticesBuffer.Release();

            //// Handle triangles
            //meshCopy.subMeshCount = nonReadableMesh.subMeshCount;
            //GraphicsBuffer indexesBuffer = nonReadableMesh.GetIndexBuffer();
            //int tot = indexesBuffer.stride * indexesBuffer.count;
            //byte[] indexesData = new byte[tot];
            //indexesBuffer.GetData(indexesData);
            //meshCopy.SetIndexBufferParams(indexesBuffer.count, nonReadableMesh.indexFormat);
            //meshCopy.SetIndexBufferData(indexesData, 0, 0, tot);
            //indexesBuffer.Release();

            //// Restore submesh structure
            //uint currentIndexOffset = 0;
            //for (int i = 0; i < meshCopy.subMeshCount; i++)
            //{
            //    uint subMeshIndexCount = nonReadableMesh.GetIndexCount(i);
            //    meshCopy.SetSubMesh(i, new SubMeshDescriptor((int)currentIndexOffset, (int)subMeshIndexCount));
            //    currentIndexOffset += subMeshIndexCount;
            //}

            //meshCopy.boneWeights = nonReadableMesh.boneWeights;
            //meshCopy.bindposes = nonReadableMesh.bindposes;

            //// Recalculate normals and bounds
            //RecalculateNormalsSeamless(meshCopy);
            //meshCopy.RecalculateTangents();
            //meshCopy.RecalculateBounds();

            //nonReadableMesh.SetVertexBufferParams(nonReadableMesh.vertexCount, desc_orig);

            //return meshCopy;
        }

        public void BuildHierarchyOnly(FbxFile file, FbxObject parent, Transform current, ref int objCount)
        {
            //Debug.Log(current.name);
            Model model = new Model(current, "Null") { ID = ++objCount, Name = current.name };
            NodeAttribute node = new NodeAttribute() { ID = ++objCount, Name = current.name, Type = "Null", TypeFlags = "Null" };
            ModelDict.Add(current, model);
            NodeDict.Add(current, node);

            file.Objects.Add(model);
            file.Objects.Add(node);
            file.Connections.Add(new Connection(model, parent));
            file.Connections.Add(new Connection(node, model));

            for (int i = 0; i < current.childCount; i++)
            {
                BuildHierarchyOnly(file, model, current.GetChild(i), ref objCount);
            }
        }

        public void AddMaterials(FbxFile file, Transform root, ref int objCount)
        {
            var materials = root.GetComponentsInChildren<Renderer>().SelectMany(r => r.sharedMaterials).Distinct().ToList();
            foreach (var material in materials)
            {
                Material mat = new Material() { ID = ++objCount, Name = material.name };
                MaterialDict.Add(material, mat);
                file.Objects.Add(mat);

                var tex = AddTextures(file, material, ref objCount);
                if (tex != null)
                {
                    file.Connections.Add(new Connection(tex, mat, "OP", "DiffuseColor"));
                }
            }
        }

        public Texture AddTextures(FbxFile file, UnityEngine.Material material, ref int objCount)
        {
            var texture = material.mainTexture;
            if (texture == null)
            {
                return null;
            }
            else if (TextureDict.ContainsKey(texture))
            {
                return TextureDict[texture];
            }
            else
            {
                string filePath = $"Textures/{material.mainTexture.name}.png";
                var vid = new Video(ExportPath + "/" + filePath, filePath) { ID = ++objCount, Name = texture.name };
                var tex = new Texture(vid, material) { ID = ++objCount };
                file.Objects.Add(vid);
                file.Objects.Add(tex);
                file.Connections.Add(new Connection(vid, tex));
                TextureDict.Add(texture, tex);
                return tex;
            }
        }

        public void BuildMeshFilters(FbxFile file, Transform root, ref int objCount)
        {
            var meshFilters = GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                Geometry geo = new Geometry(MakeReadableMeshCopy(meshFilter.sharedMesh)) { ID = ++objCount };
                Model model = ModelDict[meshFilter.transform];
                model.ModelType = "Mesh";
                file.Objects.Add(geo);
                file.Connections.Add(new Connection(geo, model));
                file.Objects.Remove(NodeDict[meshFilter.transform]);

                geo.Layers.Add(new LayerElementMaterial(meshFilter));

                foreach (var material in meshFilter.GetComponent<MeshRenderer>().sharedMaterials)
                {
                    file.Connections.Add(new Connection(MaterialDict[material], model));
                }
                MarkParentsImportant(meshFilter.transform, root);
            }
        }

        public void BuildSkinnedMeshRenderers(FbxFile file, Transform root, ref int objCount)
        {
            var skinRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedMesh in skinRenderers)
            {
                var meshCopy = MakeReadableMeshCopy(skinnedMesh.sharedMesh);
                Geometry geo = new Geometry(meshCopy) { ID = ++objCount };
                Model model = ModelDict[skinnedMesh.transform];
                model.ModelType = "Mesh";
                file.Objects.Add(geo);
                file.Connections.Add(new Connection(geo, model));
                file.Objects.Remove(NodeDict[skinnedMesh.transform]);

                var skin = new Deformer(meshCopy) { ID = ++objCount };
                file.Objects.Add(skin);
                file.Connections.Add(new Connection(skin, geo));

                geo.Layers.Add(new LayerElementMaterial(skinnedMesh));

                foreach (var material in skinnedMesh.sharedMaterials)
                {
                    if (MaterialDict.ContainsKey(material))
                    {
                        file.Connections.Add(new Connection(MaterialDict[material], model));
                    }
                    else
                    {
                        Debug.LogWarning(material + " not found in dict.");
                    }
                }


                //var pose = new Pose();
                //file.Objects.Add(pose);
                //pose.PoseNodes.Add(new PoseNode() { ID = model.ID, Matrix = Matrix4x4.identity });

                Dictionary<Transform, SubDeformer> deformDict = new Dictionary<Transform, SubDeformer>();

                for (int i = 0; i < skinnedMesh.bones.Length; i++)
                {
                    deformDict.Add(skinnedMesh.bones[i], new SubDeformer(skinnedMesh.bones[i], skinnedMesh.sharedMesh.bindposes[i]) { ID = ++objCount });
                    //pose.PoseNodes.Add(new PoseNode() { ID = deformDict[skinnedMesh.bones[i]].ID, Matrix = skinnedMesh.sharedMesh.bindposes[i] });
                }

                for (int i = 0; i < skinnedMesh.sharedMesh.boneWeights.Length; i++)
                {
                    var wght = skinnedMesh.sharedMesh.boneWeights[i];

                    if (!deformDict[skinnedMesh.bones[wght.boneIndex0]].Indexes.Contains(i))
                    {
                        deformDict[skinnedMesh.bones[wght.boneIndex0]].Indexes.Add(i);
                        deformDict[skinnedMesh.bones[wght.boneIndex0]].Weights.Add(wght.weight0);
                    }
                    if (!deformDict[skinnedMesh.bones[wght.boneIndex1]].Indexes.Contains(i))
                    {
                        deformDict[skinnedMesh.bones[wght.boneIndex1]].Indexes.Add(i);
                        deformDict[skinnedMesh.bones[wght.boneIndex1]].Weights.Add(wght.weight1);
                    }
                    if (!deformDict[skinnedMesh.bones[wght.boneIndex2]].Indexes.Contains(i))
                    {
                        deformDict[skinnedMesh.bones[wght.boneIndex2]].Indexes.Add(i);
                        deformDict[skinnedMesh.bones[wght.boneIndex2]].Weights.Add(wght.weight2);
                    }
                    if (!deformDict[skinnedMesh.bones[wght.boneIndex3]].Indexes.Contains(i))
                    {
                        deformDict[skinnedMesh.bones[wght.boneIndex3]].Indexes.Add(i);
                        deformDict[skinnedMesh.bones[wght.boneIndex3]].Weights.Add(wght.weight3);
                    }
                }

                foreach (var bone in skinnedMesh.bones)
                {
                    var subDef = deformDict[bone];
                    ModelDict[bone].ModelType = "LimbNode";
                    NodeDict[bone].Type = "LimbNode";
                    NodeDict[bone].TypeFlags = "Skeleton";
                    file.Objects.Add(subDef);
                    file.Connections.Add(new Connection(subDef, skin));
                    file.Connections.Add(new Connection(ModelDict[bone], subDef));
                    MarkParentsImportant(bone, root);
                }
                MarkParentsImportant(skinnedMesh.transform, root);
            }
        }

        /// <summary> Prevent transforms from being deleted </summary>
        public void MarkParentsImportant(Transform current, Transform root)
        {
            while(current != null)
            {
                if (ImportantList.Contains(current))
                    return;

                ImportantList.Add(current);

                if (current == root)
                    return;

                current = current.parent;
            }
        }

        public void RemoveUnimportant(FbxFile file, Transform root)
        {
            var transforms = root.GetComponentsInChildren<Transform>();
            foreach (var transform in transforms)
            {
                if (!ImportantList.Contains(transform))
                {
                    var model = ModelDict.ContainsKey(transform) ? ModelDict[transform] : null;
                    var node = NodeDict.ContainsKey(transform) ? NodeDict[transform] : null;
                    if(model != null)
                    {
                        file.Objects.Remove(model);
                        file.Connections.RemoveAll(c => c.Object1 == ModelDict[transform] || c.Object2 == ModelDict[transform]);
                    }
                    if(node != null)
                    {
                        file.Objects.Remove(NodeDict[transform]);
                        file.Connections.RemoveAll(c => c.Object1 == NodeDict[transform] || c.Object2 == NodeDict[transform]);
                    }
                }
            }
        }

        static void RecalculateNormalsSeamless(Mesh mesh)
        {
            var trianglesOriginal = mesh.triangles;
            var triangles = trianglesOriginal.ToArray();

            var vertices = mesh.vertices;

            var mergeIndices = new Dictionary<int, int>();

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertexHash = vertices[i].GetHashCode();

                if (mergeIndices.TryGetValue(vertexHash, out var index))
                {
                    for (int j = 0; j < triangles.Length; j++)
                        if (triangles[j] == i)
                            triangles[j] = index;
                }
                else
                    mergeIndices.Add(vertexHash, i);
            }

            mesh.triangles = triangles;

            var normals = new Vector3[vertices.Length];

            mesh.RecalculateNormals();
            var newNormals = mesh.normals;

            for (int i = 0; i < vertices.Length; i++)
                if (mergeIndices.TryGetValue(vertices[i].GetHashCode(), out var index))
                    normals[i] = newNormals[index];

            mesh.triangles = trianglesOriginal;
            mesh.normals = normals;
        }
    }
}