using System.Collections.Generic;

namespace FBEx
{
    public class LayerElementUV : LayerElement
    {
        public string MappingInformationType = "ByVertice";
        public string ReferenceInformationType = "IndexToDirect";
        public List<float> UV = new List<float>();
        public List<int> UVIndex = new List<int>();

        public LayerElementUV()
        {
            Version = 101;
        }

        public override string Get()
        {
            List<string> strings = new List<string>();
            strings.Add($"{Indent(2)}LayerElementUV: 0 {{");
            strings.Add($"{Indent(3)}Version: {Version}");
            strings.Add($"{Indent(3)}Name: \"{Name}\"");
            strings.Add($"{Indent(3)}MappingInformationType: \"{MappingInformationType}\"");
            strings.Add($"{Indent(3)}ReferenceInformationType: \"{ReferenceInformationType}\"");
            strings.Add($"{Indent(3)}UV: *{UV.Count} {{");
            strings.Add($"{Indent(4)}a: " + string.Join(",", UV));
            strings.Add($"{Indent(3)}}}");
            strings.Add($"{Indent(3)}UVIndex: *{UVIndex.Count} {{");
            if (UVIndex.Count > 0)
            {
                strings.Add($"{Indent(4)}a: " + string.Join(",", UVIndex));
            }
            strings.Add($"{Indent(3)}}}");
            strings.Add($"{Indent(2)}}}");
            return string.Join("\n", strings);
        }
    }
}