using System.Collections.Generic;

namespace FBEx
{
    public class NodeAttribute : FbxObject
    {
		public string Type = "LimbNode";
		public string TypeFlags = "Null";
		public List<string> Properties70 = new List<string>();

        public NodeAttribute()
        {
			PropertyTemplate = @"
		PropertyTemplate: ""FbxNull"" {
			Properties70:  {
				P: ""Color"", ""ColorRGB"", ""Color"", """",0.8,0.8,0.8
				P: ""Size"", ""double"", ""Number"", """",100
				P: ""Look"", ""enum"", """", """",1
			}
		}";
        }

        public override string Get()
        {
			var strings = new List<string>();
			strings.Add($"{Indent(1)}NodeAttribute: {ID}, \"NodeAttribute::{Name}\", \"{Type}\" {{");
			if (Properties70.Count > 0)
			{
				strings.Add($"{Indent(2)}Properties70: {{");
				foreach (var prop in Properties70)
				{
					strings.Add($"{Indent(3)}P: {prop}");
				}
				strings.Add($"{Indent(2)}}}");
			}
			strings.Add($"{Indent(2)}TypeFlags: \"{TypeFlags}\"");
			strings.Add($"{Indent(1)}}}");
			return string.Join("\n", strings);
        }
    }
}
