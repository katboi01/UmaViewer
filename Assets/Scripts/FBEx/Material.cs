using System.Collections.Generic;

namespace FBEx
{
	public class Material : FbxObject
	{
		public string ShadingModel = "phong";
		public int MultiLayer = 0;
		public List<string> Properties70 = new List<string>();

		public Material()
		{
			Name = "";
			PropertyTemplate = @"
			PropertyTemplate: ""FbxSurfacePhong"" {
				Properties70:  {
					P: ""ShadingModel"", ""KString"", """", """", ""Phong""
					P: ""MultiLayer"", ""bool"", """", """",0
					P: ""EmissiveColor"", ""Color"", """", ""A"",0,0,0
					P: ""EmissiveFactor"", ""Number"", """", ""A"",1
					P: ""AmbientColor"", ""Color"", """", ""A"",0.2,0.2,0.2
					P: ""AmbientFactor"", ""Number"", """", ""A"",1
					P: ""DiffuseColor"", ""Color"", """", ""A"",0.8,0.8,0.8
					P: ""DiffuseFactor"", ""Number"", """", ""A"",1
					P: ""Bump"", ""Vector3D"", ""Vector"", """",0,0,0
					P: ""NormalMap"", ""Vector3D"", ""Vector"", """",0,0,0
					P: ""BumpFactor"", ""double"", ""Number"", """",1
					P: ""TransparentColor"", ""Color"", """", ""A"",0,0,0
					P: ""TransparencyFactor"", ""Number"", """", ""A"",0
					P: ""DisplacementColor"", ""ColorRGB"", ""Color"", """",0,0,0
					P: ""DisplacementFactor"", ""double"", ""Number"", """",1
					P: ""VectorDisplacementColor"", ""ColorRGB"", ""Color"", """",0,0,0
					P: ""VectorDisplacementFactor"", ""double"", ""Number"", """",1
					P: ""SpecularColor"", ""Color"", """", ""A"",0.2,0.2,0.2
					P: ""SpecularFactor"", ""Number"", """", ""A"",1
					P: ""ShininessExponent"", ""Number"", """", ""A"",20
					P: ""ReflectionColor"", ""Color"", """", ""A"",0,0,0
					P: ""ReflectionFactor"", ""Number"", """", ""A"",1
				}
			}";
			Properties70 = new List<string>()
		{
			"\"ShadingModel\", \"KString\", \"\", \"\", \"phong\""
			,"\"EmissiveFactor\", \"Number\", \"\", \"A\",0"
			,"\"AmbientColor\", \"Color\", \"\", \"A\",0.587999999523163,0.587999999523163,0.587999999523163"
			,"\"DiffuseColor\", \"Color\", \"\", \"A\",0.587999999523163,0.587999999523163,0.587999999523163"
			,"\"TransparentColor\", \"Color\", \"\", \"A\",1,1,1"
			,"\"SpecularColor\", \"Color\", \"\", \"A\",0.899999976158142,0.899999976158142,0.899999976158142"
			,"\"SpecularFactor\", \"Number\", \"\", \"A\",0"
			,"\"ShininessExponent\", \"Number\", \"\", \"A\",2"
			,"\"Emissive\", \"Vector3D\", \"Vector\", \"\",0,0,0"
			,"\"Ambient\", \"Vector3D\", \"Vector\", \"\",0.587999999523163,0.587999999523163,0.587999999523163"
			,"\"Diffuse\", \"Vector3D\", \"Vector\", \"\",0.587999999523163,0.587999999523163,0.587999999523163"
			,"\"Specular\", \"Vector3D\", \"Vector\", \"\",0,0,0"
			,"\"Shininess\", \"double\", \"Number\", \"\",2"
			,"\"Opacity\", \"double\", \"Number\", \"\",1"
			,"\"Reflectivity\", \"double\", \"Number\", \"\",0"
		};
		}

		public override string Get()
		{
			List<string> strings = new List<string>();

			strings.Add($"{Indent(1)}Material: {ID}, \"{GetType().Name}::{Name}\", \"\" {{");
			strings.Add($"{Indent(2)}Version: {Version}");
			strings.Add($"{Indent(2)}ShadingModel: \"{ShadingModel}\"");
			strings.Add($"{Indent(2)}MultiLayer: {MultiLayer}");

			strings.Add($"{Indent(2)}Properties70: {{");
			foreach (var prop in Properties70)
			{
				strings.Add($"{Indent(3)}P: {prop}");
			}
			strings.Add($"{Indent(2)}}}");

			strings.Add($"{Indent(1)}}}");
			return string.Join("\n", strings);
		}
	}
}