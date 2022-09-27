/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace CriWare {

[Serializable]
public class CriAtomWindowPrefs : ScriptableObject
{
	public string outputAssetsRoot = String.Empty;

	/* @cond excludele */
	static readonly private string commonCuesheetGuid = "000000000";

	public bool useCommonKey = true;
	[SerializeField] private List<CueSheetKeyPair> cuesheetKeyList = new List<CueSheetKeyPair>();

	private Dictionary<string, ulong> searchableKeyList = new Dictionary<string, ulong>();
	/* @endcond */

	static string FindInstance() {
		var guids = AssetDatabase.FindAssets("t:" + typeof(CriAtomWindowPrefs).Name);
		if (guids.Length <= 0)
			return null;
		if (guids.Length > 1) {
			Debug.LogWarning("[CRIWARE] There are multiple preferences file of CriAtomWindow.");
		}
		return guids[0];
	}

	/* @cond excludele */
	[Serializable]
	public class CueSheetKeyPair {
		public string cuesheetGuid;
		public ulong key;

		public CueSheetKeyPair(string cuesheetGuid, ulong key) {
			this.cuesheetGuid = cuesheetGuid;
			this.key = key;
		}
	}

	/* Slow. Don't run it every frame */
	public void SetOrRenewKeyThenSave(string cuesheetGuid, ulong key) {
		if (useCommonKey) {
			cuesheetGuid = commonCuesheetGuid;
		}
		if (searchableKeyList.ContainsKey(cuesheetGuid)) {
			if (key == 0) {
				searchableKeyList.Remove(cuesheetGuid);
			} else {
				searchableKeyList[cuesheetGuid] = key;
			}
		} else {
			searchableKeyList.Add(cuesheetGuid, key);
		}
		this.Save();
	}

	/* Slow. Don't run it every frame */
	public ulong GetKey(string cuesheetGuid) {
		if (useCommonKey) {
			cuesheetGuid = commonCuesheetGuid;
		}
		return searchableKeyList.ContainsKey(cuesheetGuid) ? searchableKeyList[cuesheetGuid] : 0;
	}

	public void ClearKeysAndSave() {
		searchableKeyList.Clear();
		useCommonKey = true;
		this.Save();
	}

	private void SerializeKeyDict() {
		this.cuesheetKeyList.Clear();
		foreach (var elem in searchableKeyList) {
			cuesheetKeyList.Add(new CueSheetKeyPair(elem.Key, elem.Value));
		}
	}

	private void DeserializeKeyDict() {
		this.searchableKeyList.Clear();
		foreach(var elem in cuesheetKeyList) {
			searchableKeyList.Add(elem.cuesheetGuid, elem.key);
		}
	}
	/* @endcond */

	public void Save ()
	{
	/* @cond excludele */
		this.SerializeKeyDict();
	/* @endcond */

		if (string.IsNullOrEmpty(FindInstance())) {
			var script = MonoScript.FromScriptableObject(this);
			var prefsFilePath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(script)), "CriAtomWindowPrefs.asset");
			AssetDatabase.CreateAsset(this, prefsFilePath);
		} else {
			EditorUtility.SetDirty(this);
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}

	public static CriAtomWindowPrefs Load ()
	{
		CriAtomWindowPrefs preference;
		var guid = FindInstance();
		if (string.IsNullOrEmpty(guid)) {
			preference = CreateInstance<CriAtomWindowPrefs>();
			preference.Save();
		} else {
			var path = AssetDatabase.GUIDToAssetPath(guid);
			preference = AssetDatabase.LoadAssetAtPath<CriAtomWindowPrefs>(path);
		}

	/* @cond excludele */
		preference.DeserializeKeyDict();
	/* @endcond */

		return preference;
	}
}

[Serializable]
public class CriAtomWindowInfo {
	#region Serializable Classes
	[Serializable]
	public class InfoBase {
		public string name = "";
		public int id = 0;
		public string assetGuid = null;
		public string comment = "";
	}

	[Serializable]
	public class AcfInfo : InfoBase {
		public string filePath = "";
		public List<string> dspBusSettingsList = new List<string>();

		public AcfInfo(string name, int id, string comment, string filePath) {
			this.name = name;
			this.id = id;
			this.comment = comment;

			this.filePath = filePath;
		}
	}

	[Serializable]
	public class AcbInfo : InfoBase {
		public string acbPath = "";
		public string awbPath = "";
		public List<CueInfo> cueInfoList;
		public List<CueInfo> publicCueInfoList;

		private bool sortOrder = true; /* true = incremental */

		public AcbInfo(string name, int id, string comment, string acbPath, string awbPath) {
			this.name = name;
			this.id = id;
			this.comment = comment;

			this.acbPath = acbPath;
			this.awbPath = awbPath;
			this.cueInfoList = new List<CueInfo>();
			this.publicCueInfoList = new List<CueInfo>();
		}

		public void SortCueInfo(CueSortType type) {
			if (cueInfoList.Count <= 0) {
				return;
			}

			switch (type) {
				case CueSortType.Id:
					this.cueInfoList.Sort((CueInfo x, CueInfo y) => {
						return sortOrder ? (x.id - y.id) : (y.id - x.id);
					});
					this.publicCueInfoList.Sort((CueInfo x, CueInfo y) => {
						return sortOrder ? (x.id - y.id) : (y.id - x.id);
					});
					break;
				case CueSortType.Name:
					this.cueInfoList.Sort((CueInfo x, CueInfo y) => {
						return sortOrder ? string.Compare(x.name, y.name) : string.Compare(y.name, x.name);
					});
					this.publicCueInfoList.Sort((CueInfo x, CueInfo y) => {
						return sortOrder ? string.Compare(x.name, y.name) : string.Compare(y.name, x.name);
					});
					break;
				default:
					break;
			}

			sortOrder = !sortOrder;
		}
	}

	public void SortCueSheet() {
		switch (cuesheetSortType) {
			case CueSheetSortType.NameInc:
				this.acbInfoList.Sort((AcbInfo x, AcbInfo y) => (
					string.Compare(x.name, y.name)
				));
				cuesheetSortType = CueSheetSortType.NameDec;
				break;
			case CueSheetSortType.NameDec:
				this.acbInfoList.Sort((AcbInfo x, AcbInfo y) => (
					string.Compare(y.name, x.name)
				));
				cuesheetSortType = CueSheetSortType.Id;
				break;
			case CueSheetSortType.Id:
				this.acbInfoList.Sort((AcbInfo x, AcbInfo y) => (
					x.id - y.id
				));
				cuesheetSortType = CueSheetSortType.NameInc;
				break;
			default:
				break;
		}
	}

	private enum CueSheetSortType {
		NameInc,
		NameDec,
		Id
	}
	private CueSheetSortType cuesheetSortType = CueSheetSortType.NameInc;

	public enum CueSortType {
		Id,
		Name
	}

	[Serializable]
	public class CueInfo : InfoBase {
		public long length;
		public bool isPublic;
		public CueInfo(string name, int id, string comment, long length, bool isPublic) {
			this.name = name;
			this.id = id;
			this.comment = comment;
			this.length = length;
			this.isPublic = isPublic;
		}
	} /* end of class */
	#endregion

	#region Fields
	[SerializeField] private List<AcfInfo> acfInfoList = new List<AcfInfo>();
	[SerializeField] private List<AcbInfo> acbInfoList = new List<AcbInfo>();
	#endregion


	public List<AcbInfo> GetAcbInfoList(bool forceReload, string searchPath) {
		if (acbInfoList == null || forceReload) {
			if (acbInfoList != null) { acbInfoList.Clear(); }
			acbInfoList = new List<AcbInfo>();

			string[] files = null;
			try {
				files = Directory.GetFiles(searchPath, "*.acb", SearchOption.AllDirectories);
			} catch (Exception ex) {
				if (ex is ArgumentException || ex is ArgumentNullException) {
					Debug.LogWarning("[CRIWARE] Insufficient search path. Please check the path for file searching.");
				} else if (ex is DirectoryNotFoundException) {
					Debug.LogWarning("[CRIWARE] Search path not found: " + searchPath);
				} else {
					Debug.LogError("[CRIWARE] Error getting ACB files. Message: " + ex.Message);
				}
			}
			if (files != null && CriAtomPlugin.IsLibraryInitialized()) {
				int acbIndex = 0;
				foreach (string file in files) {
					string acbFilePath = file.Replace("\\", "/");
					AcbInfo acbInfo = new AcbInfo(
						Path.GetFileNameWithoutExtension(acbFilePath),
						acbIndex++,
						"",
						TryGetRelFilePath(acbFilePath),
						TryGetAwbFile(acbFilePath));
					/* 指定したACBファイル名(キューシート名)を指定してキュー情報を取得 */
					CriAtomExAcb acb = CriAtomExAcb.LoadAcbFile(null, acbFilePath, "");
					if (acb != null) {
						acbInfo.assetGuid = AssetDatabase.AssetPathToGUID("Assets" + acbFilePath.Substring(Application.dataPath.Length));
						/* キュー名リストの作成 */
						CriAtomEx.CueInfo[] cueInfoList = acb.GetCueInfoList();
						foreach (CriAtomEx.CueInfo cueInfo in cueInfoList) {
							bool found = false;
							foreach (var key in acbInfo.cueInfoList) {
								if (key.id == cueInfo.id) {
									found = true;
									break;
								}
							}
							if (found == false) {
								var newCueInfo = new CueInfo(cueInfo.name, cueInfo.id, cueInfo.userData, cueInfo.length, Convert.ToBoolean(cueInfo.headerVisibility));
								acbInfo.cueInfoList.Add(newCueInfo);
								if (newCueInfo.isPublic) {
									acbInfo.publicCueInfoList.Add(newCueInfo);
								}
							} else {
								/* inGame時のサブシーケンスの場合あり */
								Debug.Log("[CRIWARE] Duplicate cue ID " + cueInfo.id.ToString() + " in cue sheet " + acbInfo.name + ". Last cue name:" + cueInfo.name);
							}
						}
						acb.Dispose();
					} else {
						Debug.Log("[CRIWARE] Failed to load ACB file: " + file);
					}
					acbInfoList.Add(acbInfo);
				}
			}
		}
		return acbInfoList;
	}

	public List<AcfInfo> GetAcfInfoList(bool forceReload, string searchPath) {
		if (acfInfoList == null || forceReload) {
			if (acfInfoList != null) { acfInfoList.Clear(); }
			acfInfoList = new List<AcfInfo>();

			string[] files = null;
			try {
				files = System.IO.Directory.GetFiles(searchPath, "*.acf", System.IO.SearchOption.AllDirectories);
			} catch (Exception ex) {
				if (ex is ArgumentException || ex is ArgumentNullException) {
					Debug.LogWarning("[CRIWARE] Insufficient search path. Please check the path for file searching.");
				} else if (ex is DirectoryNotFoundException) {
					Debug.LogWarning("[CRIWARE] Search path not found: " + searchPath);
				} else {
					Debug.LogError("[CRIWARE] Error getting ACF files. Message: " + ex.Message);
				}
			}
			if (files != null && CriAtomPlugin.IsLibraryInitialized()) {
				int index = 0;
				foreach (string file in files) {
					string acfFilePath = file.Replace("\\", "/");
					var acfInfo = new AcfInfo(
						System.IO.Path.GetFileNameWithoutExtension(acfFilePath),
						index++,
						"",
						TryGetRelFilePath(acfFilePath));
					acfInfo.assetGuid = AssetDatabase.AssetPathToGUID("Assets" + acfFilePath.Substring(Application.dataPath.Length));
					CriAtomEx.RegisterAcf(null, acfFilePath);
					CriAtomExAcf.AcfInfo nativeAcfInfo;
					CriAtomExAcf.GetAcfInfo(out nativeAcfInfo);
					for (ushort i = 0; i < nativeAcfInfo.numDspSettings; ++i) {
						acfInfo.dspBusSettingsList.Add(CriAtomExAcf.GetDspSettingNameByIndex(i));
					}
					CriAtomEx.UnregisterAcf();
					acfInfoList.Add(acfInfo);
				}
			}
		}
		return acfInfoList;
	}

	public void ResetInfo() {
		this.acfInfoList.Clear();
		this.acfInfoList = null;
		this.acbInfoList.Clear();
		this.acbInfoList = null;
	}

	private string TryGetRelFilePath(string path) {
		Uri streamingAssetsUri = new Uri(CriWare.Common.streamingAssetsPath);
		Uri fullPathUri = new Uri(path);
		if (fullPathUri.ToString().Contains(streamingAssetsUri.ToString())) {
			string[] splitter = { streamingAssetsUri.ToString() + "/" };
			var pathChunks = fullPathUri.ToString().Split(splitter, StringSplitOptions.RemoveEmptyEntries);
			return pathChunks[pathChunks.Length - 1];
		} else {
			return path;
		}
	}

	private string TryGetAwbFile(string acbPath) {
		string dir = Path.GetDirectoryName(acbPath);
		string filenameNoExt = Path.GetFileNameWithoutExtension(acbPath);
		string presumedAwbPath = Path.Combine(dir, filenameNoExt + ".awb");
		if (File.Exists(presumedAwbPath)) {
			return TryGetRelFilePath(presumedAwbPath);
		} else {
			return "";
		}
	}
}

} //namespace CriWare