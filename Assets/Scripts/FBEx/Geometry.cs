using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FBEx
{
    public class Geometry : FbxObject
    {
        public List<string> Properties70 = new List<string>();
        public List<float> Vertices = new List<float>();
        public List<int> PolygonVertexIndex = new List<int>();
        public List<int> Edges = new List<int>(); //Can be skipped
        public List<LayerElement> Layers = new List<LayerElement>();

        public Geometry(Mesh mr)
        {
            Name = mr.name;
            Version = 124;
            Properties70 = new List<string>()
        {
            "\"Color\", \"ColorRGB\", \"Color\", \"\",0.341176470588235,0.776470588235294,0.882352941176471"
        };
            PropertyTemplate = @"
        PropertyTemplate: ""FbxMesh"" {
			Properties70:  {
				P: ""BBoxMin"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""BBoxMax"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""Primary Visibility"", ""bool"", """", """",1
				P: ""Casts Shadows"", ""bool"", """", """",1
				P: ""Receive Shadows"", ""bool"", """", """",1
			}
		}";
            foreach (var v in mr.vertices)
            {
                Vertices.Add(-v.x);
                Vertices.Add(v.y);
                Vertices.Add(v.z);
            }


            for (int i = 0; i < mr.triangles.Count(); i += 3)
            {
                PolygonVertexIndex.Add(mr.triangles[i + 1]);
                PolygonVertexIndex.Add(mr.triangles[i]);
                PolygonVertexIndex.Add(~mr.triangles[i + 2]);
            }

            LayerElementNormal nrm = new LayerElementNormal();
            Layers.Add(nrm);

            int triangleCount = mr.triangles.Length;
            int[] triangles = mr.triangles;

            Vector3[] normals = mr.normals;

            for (int i = 0; i < triangleCount; i += 3)
            {
                // To get the correct normals, must rewind the normal triangles like the triangles above since x was flipped
                Vector3 newNormal = normals[triangles[i + 1]];
                nrm.Normals.Add(newNormal.x * -1);
                nrm.Normals.Add(newNormal.y);
                nrm.Normals.Add(newNormal.z);

                newNormal = normals[triangles[i]];
                nrm.Normals.Add(newNormal.x * -1);
                nrm.Normals.Add(newNormal.y);
                nrm.Normals.Add(newNormal.z);

                newNormal = normals[triangles[i + 2]];
                nrm.Normals.Add(newNormal.x * -1);
                nrm.Normals.Add(newNormal.y);
                nrm.Normals.Add(newNormal.z);
            }

            //foreach (var n in mr.normals)
            //{
            //    nrm.Normals.Add(-n.x);
            //    nrm.Normals.Add(n.y);
            //    nrm.Normals.Add(n.z);
            //}

            List<Vector2> uvs = new List<Vector2>();
            mr.GetUVs(0, uvs);
            if (uvs.Count > 0)
            {
                LayerElementUV uv = new LayerElementUV() { Name = "UVChannel_1" };
                Layers.Add(uv);
                for (int i = 0; i < mr.uv.Count(); i++)
                {
                    uv.UV.Add(mr.uv[i].x);
                    uv.UV.Add(mr.uv[i].y);
                    uv.UVIndex.Add(i);
                }
            }
        }

        public override string Get()
        {
            List<string> strings = new List<string>();
            strings.Add($"{Indent(1)}Geometry: {ID}, \"{GetType().Name}::{Name}\", \"Mesh\" {{");
            strings.Add($"{Indent(2)}Properties70: {{");
            foreach (var prop in Properties70)
            {
                strings.Add($"{Indent(3)}P: {prop}");
            }
            strings.Add($"{Indent(2)}}}");

            strings.Add($"{Indent(2)}Vertices: *{Vertices.Count} {{");
            strings.Add($"{Indent(3)}a: " + string.Join(",", Vertices));
            strings.Add($"{Indent(2)}}}");

            strings.Add($"{Indent(2)}PolygonVertexIndex: *{PolygonVertexIndex.Count} {{");
            strings.Add($"{Indent(3)}a: " + string.Join(",", PolygonVertexIndex));
            strings.Add($"{Indent(2)}}}");

            strings.Add($"{Indent(2)}GeometryVersion: {Version}");
            foreach (var layer in Layers)
            {
                strings.Add(layer.Get());
            }
            strings.Add($"{Indent(2)}Layer: 0 {{");
            strings.Add($"{Indent(3)}Version: 100");
            foreach (var layer in Layers)
            {
                strings.Add($"{Indent(3)}LayerElement: {{");
                strings.Add($"{Indent(4)}Type: \"{layer.GetType().Name}\"");
                strings.Add($"{Indent(4)}TypedIndex: 0");
                strings.Add($"{Indent(3)}}}");
            }
            strings.Add($"{Indent(2)}}}");
            strings.Add($"{Indent(1)}}}");
            return string.Join("\n", strings);
        }
    }
}