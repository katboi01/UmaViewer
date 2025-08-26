using System;
using UnityEngine;

namespace FBEx
{
	public class FBXHeaderExtension
	{
		public string Get()
		{
			var date = DateTime.Now;
			return $@"; FBX 7.4.0 project file
; ----------------------------------------------------

FBXHeaderExtension:  {{
	FBXHeaderVersion: 1003
	FBXVersion: 7400
	CreationTimeStamp:  {{
		Version: 1000
		Year: {date.Year}
		Month: {date.Month}
		Day: {date.Day}
		Hour: {date.Hour}
		Minute: {date.Minute}
		Second: {date.Second}
		Millisecond: {date.Millisecond}
	}}
	Creator: ""Katworks FBEx plugin""
	SceneInfo: ""SceneInfo::GlobalInfo"", ""UserData"" {{
		Type: ""UserData""
		Version: 100
		MetaData:  {{
			Version: 100
			Title: """"
			Subject: """"
			Author: """"
			Keywords: """"
			Revision: """"
			Comment: """"
		}}
		Properties70:  {{
			P: ""DocumentUrl"", ""KString"", ""Url"", """", ""C:\UmaViewer\test.fbx""
			P: ""SrcDocumentUrl"", ""KString"", ""Url"", """", ""C:\UmaViewer\test.fbx""
			P: ""Original"", ""Compound"", """", """"
			P: ""Original|ApplicationVendor"", ""KString"", """", """", ""{Application.companyName}""
			P: ""Original|ApplicationName"", ""KString"", """", """", ""{Application.productName}""
			P: ""Original|ApplicationVersion"", ""KString"", """", """", ""{Application.version} {Application.unityVersion}""
			P: ""Original|DateTime_GMT"", ""DateTime"", """", """", ""{date.ToString("dd/MM/yyyy HH:mm:ss.fff")}""
			P: ""Original|FileName"", ""KString"", """", """", ""{"todo"}""
		}}
	}}
}}
";
		}
	}
}