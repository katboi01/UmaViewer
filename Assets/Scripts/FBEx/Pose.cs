using System.Collections.Generic;
using UnityEngine;

namespace FBEx
{
	public class Pose : FbxObject
	{
		public string Type = "BindPose";
		public string TypeFlags = "Null";
		public List<PoseNode> PoseNodes = new List<PoseNode>();

		public Pose()
		{
			Version = 100;
			PropertyTemplate = "";
		}

		public override string Get()
		{
			var strings = new List<string>();
			strings.Add($"{Indent(1)}Pose: {ID}, \"Pose::Bind Pose List\", \"{Type}\" {{");
			strings.Add($"{Indent(2)}Type: \"{Type}\"");
			strings.Add($"{Indent(2)}Version: \"{Version}\""); 
			strings.Add($"{Indent(2)}NbPoseNodes: \"{PoseNodes.Count}\"");
			foreach (var node in PoseNodes)
			{
				strings.Add($"{Indent(2)}PoseNode: {{");

				List<float> matrix = new List<float>();
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						matrix.Add(node.Matrix[x, y]);
					}
				}

				strings.Add($"{Indent(3)}Node: {node.ID}");
				strings.Add($"{Indent(3)}Matrix: *16 {{");
				strings.Add($"{Indent(4)}a: {string.Join(",", matrix)}");
				strings.Add($"{Indent(3)}}}");
				strings.Add($"{Indent(2)}}}");
			}
			strings.Add($"{Indent(1)}}}");
			return string.Join("\n", strings);
		}
	}

	public class PoseNode
	{
		public int ID;
		public Matrix4x4 Matrix;
	}

}
