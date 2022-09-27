/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace CriWare {

public class CriFsServer : CriMonoBehaviour
{
	#region Internal Fields
	private static CriFsServer _instance        = null;
	private List<CriFsRequest> requestList      = null;

	static public CriFsServer instance
	{
		get {
			CreateInstance();
			return _instance;
		}
	}
	#endregion

	public int installBufferSize { get; private set; }

	#region Internal Methods
	public static void CreateInstance()
	{
		if (_instance == null) {
			CriWare.Common.managerObject.AddComponent<CriFsServer>();
			_instance.installBufferSize = CriFsPlugin.installBufferSize;
		}
	}

	public static void DestroyInstance()
	{
		if (_instance != null) {
			UnityEngine.GameObject.Destroy(_instance);
		}
	}

	void Awake()
	{
		if (_instance == null) {
			_instance = this;
			this.requestList = new List<CriFsRequest>();
			/* 高速化のため、ダミーを追加してListの内部配列の自動確保を促す
			 * 追加による自動確保が目的なのでダミーはすぐに削除する */
			CriFsRequest dummy = new CriFsRequest();
			this.requestList.Add(dummy);
			this.requestList.RemoveAt(0);
		} else {
			GameObject.Destroy(this);
		}
	}

	void OnDestroy()
	{
		if (_instance == this) {
			foreach (var reqest in this.requestList) {
				reqest.Dispose();
			}
			_instance = null;
		}
	}

	public override void CriInternalUpdate()
	{
		#pragma warning disable 162
		if (CriWare.Common.supportsCriFsInstaller == true) {
			CriFsInstaller.ExecuteMain();
			if (CriFsWebInstaller.isInitialized) {
				CriFsWebInstaller.ExecuteMain();
			}
		}
		#pragma warning restore 162

		for (int i=0;i<this.requestList.Count;i++) {
			CriFsRequest request = this.requestList[i];
			request.Update();
		}
		for (int i = 0; i < requestList.Count; i++) {
			if (requestList[i].isDone || requestList[i].isDisposed) {
				requestList.Remove(requestList[i]);
			}
		}
	}

	public override void CriInternalLateUpdate() { }

	public void AddRequest(CriFsRequest request)
	{
		this.requestList.Add(request);
	}

	public CriFsLoadFileRequest LoadFile(CriFsBinder binder, string path, CriFsRequest.DoneDelegate doneDelegate, int readUnitSize)
	{
		var request = new CriFsLoadFileRequest(binder, path, doneDelegate, readUnitSize);
		this.AddRequest(request);
		return request;
	}

	public CriFsLoadAssetBundleRequest LoadAssetBundle(CriFsBinder binder, string path, int readUnitSize)
	{
		var request = new CriFsLoadAssetBundleRequest(binder, path, readUnitSize);
		this.AddRequest(request);
		return request;
	}

	public CriFsInstallRequest Install(CriFsBinder srcBinder, string srcPath, string dstPath, CriFsRequest.DoneDelegate doneDelegate)
	{
		var request = new CriFsInstallRequestLegacy(srcBinder, srcPath, dstPath, doneDelegate, this.installBufferSize);
		this.requestList.Add(request);
		return request;
	}

	public CriFsInstallRequest WebInstall(string srcPath, string dstPath, CriFsRequest.DoneDelegate doneDelegate)
	{
		var request = new CriFsWebInstallRequest(srcPath, dstPath, doneDelegate);
		this.requestList.Add(request);
		return request;
	}

	public CriFsBindRequest BindCpk(CriFsBinder targetBinder, CriFsBinder srcBinder, string path)
	{
		var request = new CriFsBindRequest(
			CriFsBindRequest.BindType.Cpk, targetBinder, srcBinder, path);
		this.AddRequest(request);
		return request;
	}

	public CriFsBindRequest BindDirectory(CriFsBinder targetBinder, CriFsBinder srcBinder, string path)
	{
		var request = new CriFsBindRequest(
			CriFsBindRequest.BindType.Directory, targetBinder, srcBinder, path);
		this.AddRequest(request);
		return request;
	}

	public CriFsBindRequest BindFile(CriFsBinder targetBinder, CriFsBinder srcBinder, string path)
	{
		var request = new CriFsBindRequest(
			CriFsBindRequest.BindType.File, targetBinder, srcBinder, path);
		this.AddRequest(request);
		return request;
	}

	#endregion
}

} //namespace CriWare