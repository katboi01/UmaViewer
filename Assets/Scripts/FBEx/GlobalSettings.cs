
namespace FBEx
{
	public class GlobalSettings : FbxObject
	{
		public GlobalSettings()
		{
			Version = 1000;
			PropertyTemplate = "";
		}

		public string Get()
		{
			return $@"
GlobalSettings:  {{
	Version: {Version}
	Properties70:  {{
		P: ""UpAxis"", ""int"", ""Integer"", """",1
		P: ""UpAxisSign"", ""int"", ""Integer"", """",1
		P: ""FrontAxis"", ""int"", ""Integer"", """",2
		P: ""FrontAxisSign"", ""int"", ""Integer"", """",1
		P: ""CoordAxis"", ""int"", ""Integer"", """",0
		P: ""CoordAxisSign"", ""int"", ""Integer"", """",1
		P: ""OriginalUpAxis"", ""int"", ""Integer"", """",2
		P: ""OriginalUpAxisSign"", ""int"", ""Integer"", """",1
		P: ""UnitScaleFactor"", ""double"", ""Number"", """",100
		P: ""OriginalUnitScaleFactor"", ""double"", ""Number"", """",1
		P: ""AmbientColor"", ""ColorRGB"", ""Color"", """",0,0,0
		P: ""DefaultCamera"", ""KString"", """", """", ""Producer Perspective""
		P: ""TimeMode"", ""enum"", """", """",6
		P: ""TimeProtocol"", ""enum"", """", """",2
		P: ""SnapOnFrameMode"", ""enum"", """", """",0
		P: ""TimeSpanStart"", ""KTime"", ""Time"", """",0
		P: ""TimeSpanStop"", ""KTime"", ""Time"", """",153953860000
		P: ""CustomFrameRate"", ""double"", ""Number"", """",-1
		P: ""TimeMarker"", ""Compound"", """", """"
		P: ""CurrentTimeMarker"", ""int"", ""Integer"", """",-1
	}}
}}";
		}
	}
}