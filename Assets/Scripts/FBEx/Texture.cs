using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FBEx
{
	public class Texture : FbxObject
	{
		public string Type = "TextureVideoClip";
		public List<string> Properties70 = new List<string>();
		public Video Media;
		public Vector2 ModelUVTranslation = Vector2.zero;
		public Vector2 ModelUVScaling = Vector2.one;
		public Vector4 Cropping = Vector4.zero;
		public string Texture_Alpha_Source = "None";

        /// <summary> enable read/write on texture by cloning it </summary>
        Texture2D duplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public Texture(Video media, UnityEngine.Material mat)
		{
			Name = "";
			Version = 202;
			PropertyTemplate = @"
			PropertyTemplate: ""FbxFileTexture"" {
				Properties70:  {
					P: ""TextureTypeUse"", ""enum"", """", """",0
					P: ""Texture alpha"", ""Number"", """", ""A"",1
					P: ""CurrentMappingType"", ""enum"", """", """",0
					P: ""WrapModeU"", ""enum"", """", """",0
					P: ""WrapModeV"", ""enum"", """", """",0
					P: ""UVSwap"", ""bool"", """", """",0
					P: ""PremultiplyAlpha"", ""bool"", """", """",1
					P: ""Translation"", ""Vector"", """", ""A"",0,0,0
					P: ""Rotation"", ""Vector"", """", ""A"",0,0,0
					P: ""Scaling"", ""Vector"", """", ""A"",1,1,1
					P: ""TextureRotationPivot"", ""Vector3D"", ""Vector"", """",0,0,0
					P: ""TextureScalingPivot"", ""Vector3D"", ""Vector"", """",0,0,0
					P: ""CurrentTextureBlendMode"", ""enum"", """", """",1
					P: ""UVSet"", ""KString"", """", """", ""default""
					P: ""UseMaterial"", ""bool"", """", """",0
					P: ""UseMipMap"", ""bool"", """", """",0
				}
			}";

			Properties70 = new List<string>()
			{
				"\"UVSet\", \"KString\", \"\", \"\", \"UVChannel_1\"",
				"\"UseMaterial\", \"bool\", \"\", \"\",1\""
            };

			if (mat.name.EndsWith("_eye"))
			{
				Properties70.Insert(0, "\"Translation\", \"Vector\", \"\", \"A\",0.5,0,0");
                Properties70.Insert(1, "\"Scaling\", \"Vector\", \"\", \"A\",0.25,1,1");
            }

			Media = media;

			System.IO.File.WriteAllBytes(media.Filename, duplicateTexture((Texture2D)mat.mainTexture).EncodeToPNG());
        }

		public override string Get()
		{
			List<string> strings = new List<string>();

			strings.Add($"{Indent(1)}Texture: {ID}, \"{GetType().Name}::{Name}\", \"\" {{");
			strings.Add($"{Indent(2)}Type: \"{Type}\"");
			strings.Add($"{Indent(2)}Version: {Version}");
			strings.Add($"{Indent(2)}TextureName: \"{GetType().Name}::{Name}\"");
			strings.Add($"{Indent(2)}Properties70: {{");
			foreach (var prop in Properties70)
			{
				strings.Add($"{Indent(3)}P: {prop}");
			}
			strings.Add($"{Indent(2)}}}");
			strings.Add($"{Indent(2)}Media: \"{Media.GetType().Name}::{Media.Name}\"");
			strings.Add($"{Indent(2)}Filename: \"{Media.Filename}\"");
			strings.Add($"{Indent(2)}RelativeFilename: \"{Media.RelativeFilename}\"");
			//strings.Add($"{Indent(2)}ModelUVTranslation: {ModelUVTranslation.x},{ModelUVTranslation.y}");
			//strings.Add($"{Indent(2)}ModelUVScaling: {ModelUVScaling.x},{ModelUVScaling.y}");
			strings.Add($"{Indent(2)}Texture_Alpha_Source: \"{Texture_Alpha_Source}\"");
			strings.Add($"{Indent(2)}Cropping: {Cropping.x},{Cropping.y},{Cropping.z},{Cropping.w}");

			strings.Add($"{Indent(1)}}}");
			return string.Join("\n", strings);
		}
	}
}