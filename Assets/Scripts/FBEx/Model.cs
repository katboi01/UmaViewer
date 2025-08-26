using System.Collections.Generic;
using UnityEngine;

namespace FBEx
{
	public class Model : FbxObject
	{
		public List<string> Properties70 = new List<string>();
		public string Shading = "T";
		public string Culling = "CullingOff";
		public string ModelType = "Null";

		public Model(Transform tsf, string modelType)
		{
			Version = 232;
			ModelType = modelType;
			PropertyTemplate = @"
		PropertyTemplate: ""FbxNode"" {
			Properties70:  {
				P: ""QuaternionInterpolate"", ""enum"", """", """",0
				P: ""RotationOffset"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""RotationPivot"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""ScalingOffset"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""ScalingPivot"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""TranslationActive"", ""bool"", """", """",0
				P: ""TranslationMin"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""TranslationMax"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""TranslationMinX"", ""bool"", """", """",0
				P: ""TranslationMinY"", ""bool"", """", """",0
				P: ""TranslationMinZ"", ""bool"", """", """",0
				P: ""TranslationMaxX"", ""bool"", """", """",0
				P: ""TranslationMaxY"", ""bool"", """", """",0
				P: ""TranslationMaxZ"", ""bool"", """", """",0
				P: ""RotationOrder"", ""enum"", """", """",0
				P: ""RotationSpaceForLimitOnly"", ""bool"", """", """",0
				P: ""RotationStiffnessX"", ""double"", ""Number"", """",0
				P: ""RotationStiffnessY"", ""double"", ""Number"", """",0
				P: ""RotationStiffnessZ"", ""double"", ""Number"", """",0
				P: ""AxisLen"", ""double"", ""Number"", """",10
				P: ""PreRotation"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""PostRotation"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""RotationActive"", ""bool"", """", """",0
				P: ""RotationMin"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""RotationMax"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""RotationMinX"", ""bool"", """", """",0
				P: ""RotationMinY"", ""bool"", """", """",0
				P: ""RotationMinZ"", ""bool"", """", """",0
				P: ""RotationMaxX"", ""bool"", """", """",0
				P: ""RotationMaxY"", ""bool"", """", """",0
				P: ""RotationMaxZ"", ""bool"", """", """",0
				P: ""InheritType"", ""enum"", """", """",0
				P: ""ScalingActive"", ""bool"", """", """",0
				P: ""ScalingMin"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""ScalingMax"", ""Vector3D"", ""Vector"", """",1,1,1
				P: ""ScalingMinX"", ""bool"", """", """",0
				P: ""ScalingMinY"", ""bool"", """", """",0
				P: ""ScalingMinZ"", ""bool"", """", """",0
				P: ""ScalingMaxX"", ""bool"", """", """",0
				P: ""ScalingMaxY"", ""bool"", """", """",0
				P: ""ScalingMaxZ"", ""bool"", """", """",0
				P: ""GeometricTranslation"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""GeometricRotation"", ""Vector3D"", ""Vector"", """",0,0,0
				P: ""GeometricScaling"", ""Vector3D"", ""Vector"", """",1,1,1
				P: ""MinDampRangeX"", ""double"", ""Number"", """",0
				P: ""MinDampRangeY"", ""double"", ""Number"", """",0
				P: ""MinDampRangeZ"", ""double"", ""Number"", """",0
				P: ""MaxDampRangeX"", ""double"", ""Number"", """",0
				P: ""MaxDampRangeY"", ""double"", ""Number"", """",0
				P: ""MaxDampRangeZ"", ""double"", ""Number"", """",0
				P: ""MinDampStrengthX"", ""double"", ""Number"", """",0
				P: ""MinDampStrengthY"", ""double"", ""Number"", """",0
				P: ""MinDampStrengthZ"", ""double"", ""Number"", """",0
				P: ""MaxDampStrengthX"", ""double"", ""Number"", """",0
				P: ""MaxDampStrengthY"", ""double"", ""Number"", """",0
				P: ""MaxDampStrengthZ"", ""double"", ""Number"", """",0
				P: ""PreferedAngleX"", ""double"", ""Number"", """",0
				P: ""PreferedAngleY"", ""double"", ""Number"", """",0
				P: ""PreferedAngleZ"", ""double"", ""Number"", """",0
				P: ""LookAtProperty"", ""object"", """", """"
				P: ""UpVectorProperty"", ""object"", """", """"
				P: ""Show"", ""bool"", """", """",1
				P: ""NegativePercentShapeSupport"", ""bool"", """", """",1
				P: ""DefaultAttributeIndex"", ""int"", ""Integer"", """",-1
				P: ""Freeze"", ""bool"", """", """",0
				P: ""LODBox"", ""bool"", """", """",0
				P: ""Lcl Translation"", ""Lcl Translation"", """", ""A"",0,0,0
				P: ""Lcl Rotation"", ""Lcl Rotation"", """", ""A"",0,0,0
				P: ""Lcl Scaling"", ""Lcl Scaling"", """", ""A"",1,1,1
				P: ""Visibility"", ""Visibility"", """", ""A"",1
				P: ""Visibility Inheritance"", ""Visibility Inheritance"", """", """",1
			}
		}";
			Properties70 = new List<string>()
		{
			"\"InheritType\", \"enum\", \"\", \"\",1",
			"\"RotationOrder\", \"enum\", \"\", \"\",4",
			"\"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0",
			$"\"Lcl Translation\", \"Lcl Translation\", \"\", \"A\",{-tsf.localPosition.x},{tsf.localPosition.y},{tsf.localPosition.z}",
			$"\"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\",{tsf.localEulerAngles.x},{-tsf.localEulerAngles.y},{-tsf.localEulerAngles.z}",
			$"\"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",{tsf.localScale.x},{tsf.localScale.y},{tsf.localScale.z}",
			"\"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0",
		};
		}

		public override string Get()
		{
			List<string> strings = new List<string>();
			strings.Add($"{Indent(1)}Model: {ID}, \"Model::{Name}\", \"{ModelType}\" {{");
			strings.Add($"{Indent(2)}Version: {Version}");
			strings.Add($"{Indent(2)}Properties70: {{");
			foreach (var prop in Properties70)
			{
				strings.Add($"{Indent(3)}P: {prop}");
			}
			strings.Add($"{Indent(2)}}}");
			strings.Add($"{Indent(2)}Shading: {Shading}");
			strings.Add($"{Indent(2)}Culling: \"{Culling}\"");
			strings.Add($"{Indent(1)}}}");
			return string.Join("\n", strings);
		}
	}
}