using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FBEx
{
	public class Video : FbxObject
	{
		public string Type = "Clip";
		public List<string> Properties70 = new List<string>();
		public int UseMipMap = 0;
		public string Filename = "";
		public string RelativeFilename = "";

        public Video(string path, string relPath)
		{
            path = path.Replace("/", "\\");
            relPath = relPath.Replace("/", "\\");
            PropertyTemplate = $@"
			PropertyTemplate: ""FbxVideo"" {{
				Properties70:
				{{
					P: ""Path"", ""KString"", ""XRefUrl"", """", """"
					P: ""RelPath"", ""KString"", ""XRefUrl"", """", """"
					P: ""Color"", ""ColorRGB"", ""Color"", """",0.8,0.8,0.8
					P: ""ClipIn"", ""KTime"", ""Time"", """",0
					P: ""ClipOut"", ""KTime"", ""Time"", """",0
					P: ""Offset"", ""KTime"", ""Time"", """",0
					P: ""PlaySpeed"", ""double"", ""Number"", """",0
					P: ""FreeRunning"", ""bool"", """", """",0
					P: ""Loop"", ""bool"", """", """",0
					P: ""Mute"", ""bool"", """", """",0
					P: ""AccessMode"", ""enum"", """", """",0
					P: ""ImageSequence"", ""bool"", """", """",0
					P: ""ImageSequenceOffset"", ""int"", ""Integer"", """",0
					P: ""FrameRate"", ""double"", ""Number"", """",0
					P: ""LastFrame"", ""int"", ""Integer"", """",0
					P: ""Width"", ""int"", ""Integer"", """",0
					P: ""Height"", ""int"", ""Integer"", """",0
					P: ""StartFrame"", ""int"", ""Integer"", """",0
					P: ""StopFrame"", ""int"", ""Integer"", """",0
					P: ""InterlaceMode"", ""enum"", """", """",0
				}}
			}}";
			Filename = path;
            RelativeFilename = relPath;
			Properties70 = new List<string>()
			{
				$"\"Path\", \"KString\", \"XRefUrl\", \"\", \"{path}\"",
				$"\"RelPath\", \"KString\", \"XRefUrl\", \"\", \"{relPath}\""
			};
		}

		public override string Get()
		{
			List<string> strings = new List<string>();

			strings.Add($"{Indent(1)}Video: {ID}, \"{GetType().Name}::{Name}\", \"Clip\" {{");

			strings.Add($"{Indent(2)}Type: \"{Type}\"");
			strings.Add($"{Indent(2)}Properties70: {{");
			foreach (var prop in Properties70)
			{
				strings.Add($"{Indent(3)}P: {prop}");
			}
			strings.Add($"{Indent(2)}}}");

			strings.Add($"{Indent(2)}UseMipMap: {UseMipMap}");
			strings.Add($"{Indent(2)}Filename: \"{Filename}\"");
			strings.Add($"{Indent(2)}RelativeFilename: \"{RelativeFilename}\"");

			strings.Add($"{Indent(1)}}}");
			return string.Join("\n", strings);
		}
	}
}