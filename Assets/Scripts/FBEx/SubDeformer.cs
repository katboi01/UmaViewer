using System.Collections.Generic;
using UnityEngine;

namespace FBEx
{
    public class SubDeformer : FbxObject
    {
        public SubDeformer(Transform tsf, Matrix4x4 bindPose)
        {
            Name = tsf.name;
            Version = 100;
            PropertyTemplate = "";

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Transform.Add(bindPose[x, y]);
                }
            }
            Transform[1] *= -1;
            Transform[2] *= -1;
            Transform[4] *= -1;
            Transform[8] *= -1;
            Transform[12] *= -1;

            Matrix4x4 matrix = tsf.localToWorldMatrix;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    TransformLink.Add(matrix[x, y]);
                }
            }
            TransformLink[1] *= -1;
            TransformLink[2] *= -1;
            TransformLink[4] *= -1;
            TransformLink[8] *= -1;
            TransformLink[12] *= -1;
        }

        public override string Get()
        {
            List<string> strings = new List<string>();
            strings.Add($"{Indent(1)}Deformer: {ID}, \"{GetType().Name}::{Name}\", \"Cluster\" {{");
            strings.Add($"{Indent(2)}Version: {Version}");
            strings.Add($"{Indent(2)}UserData: \"\", \"\"");

            strings.Add($"{Indent(2)}Indexes: *{Indexes.Count} {{");
            strings.Add($"{Indent(3)}a: " + string.Join(",", Indexes));
            strings.Add($"{Indent(2)}}}");

            strings.Add($"{Indent(2)}Weights: *{Weights.Count} {{");
            strings.Add($"{Indent(3)}a: " + string.Join(",", Weights));
            strings.Add($"{Indent(2)}}}");

            strings.Add($"{Indent(2)}Transform: *{Transform.Count} {{");
            strings.Add($"{Indent(3)}a: " + string.Join(",", Transform));
            strings.Add($"{Indent(2)}}}");

            strings.Add($"{Indent(2)}TransformLink: *{TransformLink.Count} {{");
            strings.Add($"{Indent(3)}a: " + string.Join(",", TransformLink));
            strings.Add($"{Indent(2)}}}");

            strings.Add($"{Indent(1)}}}");
            return string.Join("\n", strings);
        }

        public List<int> Indexes = new List<int>();
        public List<float> Weights = new List<float>();
        public List<float> Transform = new List<float>();
        public List<float> TransformLink = new List<float>();
    }
}