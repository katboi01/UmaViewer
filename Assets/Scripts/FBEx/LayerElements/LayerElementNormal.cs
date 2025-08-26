using System;
using System.Collections.Generic;

namespace FBEx
{
    public class LayerElementNormal : LayerElement
    {
        public string MappingInformationType = "ByPolygonVertex";
        public string ReferenceInformationType = "Direct";
        public List<float> Normals = new List<float>();
        public List<float> NormalsW = new List<float>();

        public LayerElementNormal()
        {
            Version = 102;
        }

        public override string Get()
        {
            List<string> strings = new List<string>();
            strings.Add($"{Indent(2)}LayerElementNormal: 0 {{");
            strings.Add($"{Indent(3)}Version: {Version}");
            strings.Add($"{Indent(3)}Name: \"{Name}\"");
            strings.Add($"{Indent(3)}MappingInformationType: \"{MappingInformationType}\"");
            strings.Add($"{Indent(3)}ReferenceInformationType: \"{ReferenceInformationType}\"");
            strings.Add($"{Indent(3)}Normals: *{Normals.Count} {{");
            strings.Add($"{Indent(4)}a: " + string.Join(",", Normals));
            strings.Add($"{Indent(3)}}}");
            if (NormalsW.Count > 0)
            {
                strings.Add($"{Indent(3)}NormalsW: *{NormalsW.Count} {{");
                strings.Add($"{Indent(4)}a: " + string.Join(",", NormalsW));
                strings.Add($"{Indent(3)}}}");
            }
            strings.Add($"{Indent(2)}}}");
            return string.Join("\n", strings);
        }
    }
}