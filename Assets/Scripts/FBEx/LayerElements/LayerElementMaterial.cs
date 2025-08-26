using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FBEx
{
    class TriangleHelper
    {
        public UnityEngine.Vector3 Vertices;
        public int TriangleIndex;
    }

    public class LayerElementMaterial : LayerElement
    {
        public string
            MappingInformationType = "AllSame",
            ReferenceInformationType = "IndexToDirect";
        public int[] Materials;

        public LayerElementMaterial()
        {
            Version = 101;
        }

        public LayerElementMaterial(SkinnedMeshRenderer smr) : this()
        {
            if (smr == null) return;
            if (smr.sharedMesh.subMeshCount > 1)
            {
                MappingInformationType = "ByPolygon";

                var triangles_hlp = new List<TriangleHelper>();
                var triangles_tmp = smr.sharedMesh.triangles;
                Materials = new int[triangles_tmp.Length / 3];

                for (int i = 0; i < triangles_tmp.Length; i += 3)
                {
                    triangles_hlp.Add(new TriangleHelper() { TriangleIndex = i / 3, Vertices = new Vector3(triangles_tmp[i], triangles_tmp[i + 1], triangles_tmp[i + 2]) });
                }
                for (int i = 0; i < smr.sharedMesh.subMeshCount; i++)
                {
                    triangles_tmp = smr.sharedMesh.GetTriangles(i);
                    for (int j = 0; j < triangles_tmp.Length; j += 3)
                    {
                        var triangleIndex = triangles_hlp.First(t => t.Vertices == new Vector3(triangles_tmp[j], triangles_tmp[j + 1], triangles_tmp[j + 2])).TriangleIndex;
                        Materials[triangleIndex] = i;
                    }
                }
            }
            else
            {
                Materials = new int[] { 0 };
            }
        }

        public LayerElementMaterial(MeshFilter filter) : this()
        {
            if (filter == null) return;
            if (filter.sharedMesh.subMeshCount > 1)
            {
                MappingInformationType = "ByPolygon";

                var triangles_hlp = new List<TriangleHelper>();
                var triangles_tmp = filter.sharedMesh.triangles;
                Materials = new int[triangles_tmp.Length / 3];

                for (int i = 0; i < triangles_tmp.Length; i += 3)
                {
                    triangles_hlp.Add(new TriangleHelper() { TriangleIndex = i / 3, Vertices = new Vector3(triangles_tmp[i], triangles_tmp[i + 1], triangles_tmp[i + 2]) });
                }
                for (int i = 0; i < filter.sharedMesh.subMeshCount; i++)
                {
                    triangles_tmp = filter.sharedMesh.GetTriangles(i);
                    for (int j = 0; j < triangles_tmp.Length; j += 3)
                    {
                        var triangleIndex = triangles_hlp.First(t => t.Vertices == new Vector3(triangles_tmp[j], triangles_tmp[j + 1], triangles_tmp[j + 2])).TriangleIndex;
                        Materials[triangleIndex] = i;
                    }
                }
            }
            else
            {
                Materials = new int[] { 0 };
            }
        }

        public override string Get()
        {
            List<string> strings = new List<string>();
            strings.Add($"{Indent(2)}LayerElementMaterial: 0 {{");
            strings.Add($"{Indent(3)}Version: {Version}");
            strings.Add($"{Indent(3)}Name: \"{Name}\"");
            strings.Add($"{Indent(3)}MappingInformationType: \"{MappingInformationType}\"");
            strings.Add($"{Indent(3)}ReferenceInformationType: \"{ReferenceInformationType}\"");
            strings.Add($"{Indent(3)}Materials: *{Materials.Length} {{");
            strings.Add($"{Indent(4)}a: " + string.Join(",", Materials));
            strings.Add($"{Indent(3)}}}");
            strings.Add($"{Indent(2)}}}");
            return string.Join("\n", strings);
        }
    }
}