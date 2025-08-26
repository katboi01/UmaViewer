using System.Collections.Generic;
using UnityEngine;

namespace FBEx
{
    public class Deformer : FbxObject
    {
        public Deformer(Mesh msh)
        {
            Name = msh.name;
            Version = 101;
            PropertyTemplate = "";
            for (int i = 0; i < msh.vertices.Length; i++)
            {
                Indexes.Add(i);
                BlendWeights.Add(0);
            }
        }

        public override string Get()
        {
            List<string> strings = new List<string>();
            strings.Add($"{Indent(1)}Deformer: {ID}, \"{GetType().Name}::{Name}\", \"Skin\" {{");
            strings.Add($"{Indent(2)}Version: {Version}");
            strings.Add($"{Indent(2)}Link_DeformAcuracy: {Link_DeformAcuracy}");
            strings.Add($"{Indent(2)}SkinningType: \"{SkinningType}\"");

            strings.Add($"{Indent(2)}Indexes: *{Indexes.Count} {{");
            strings.Add($"{Indent(3)}a: " + string.Join(",", Indexes));
            strings.Add($"{Indent(2)}}}");

            strings.Add($"{Indent(2)}BlendWeights: *{BlendWeights.Count} {{");
            strings.Add($"{Indent(3)}a: " + string.Join(",", BlendWeights));
            strings.Add($"{Indent(2)}}}");

            strings.Add($"{Indent(1)}}}");
            return string.Join("\n", strings);
        }

        public float Link_DeformAcuracy = 50;
        public string SkinningType = "Blend";
        public List<int> Indexes = new List<int>();
        public List<float> BlendWeights = new List<float>();
    }
}