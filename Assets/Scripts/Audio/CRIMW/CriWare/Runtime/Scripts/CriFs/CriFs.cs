/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#pragma warning disable 0414

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;

/*==========================================================================
 *      CRI File System Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIFS_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>A module for loading files into memory.</summary>
 * <remarks>
 * <para header='Description'>A module for loading files into memory.<br/>
 * By combining with CriWare.CriFsBinder , the content in the CPK file can also be read.<br/></para>
 * </remarks>
 * <seealso cref='CriFsBinder'/>
 */
public class CriFsLoader : CriDisposable
{
	/**
	 * <summary>A value indicating the loader status.</summary>
	 * <seealso cref='CriFsLoader::GetStatus'/>
	 */
	public enum Status
	{
		Stop,       /**< Stopped */
		Loading,    /**< Loading */
		Complete,   /**< Loading complete */
		Error       /**< Error occurred */
	}

	public CriFsLoader()
	{
		if (!CriFsPlugin.IsLibraryInitialized()) {
			throw new Exception("CriFsPlugin is not initialized.");
		}

		/* ハンドルの作成 */
		this.handle = IntPtr.Zero;
		criFsLoader_Create(out this.handle);
		if (this.handle == IntPtr.Zero) {
			throw new Exception("criFsLoader_Create() failed.");
		}

		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Fs);
	}

	/**
	 * <summary>Discards the loader.</summary>
	 * <remarks>
	 * <para header='Note'>If you discard the loader during the loading process,
	 * the process may be blocked in this function for a long time.<br/></para>
	 * </remarks>
	 */
	public override void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		/* ハンドルの有無をチェック */
		if (this.handle != IntPtr.Zero) {
			/* ハンドルの破棄 */
			criFsLoader_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}

		if (disposing) {
			if (this.dstGch.IsAllocated) {
				this.dstGch.Free();
			}
			if (this.srcGch.IsAllocated) {
				this.srcGch.Free();
			}
		}
	}

	/**
	 * <summary>Starts loading data.</summary>
	 * <param name='binder'>Binder</param>
	 * <param name='path'>File path name</param>
	 * <param name='fileOffset'>Offset from the beginning of the file (in bytes)</param>
	 * <param name='loadSize'>Load request size (in bytes)</param>
	 * <param name='buffer'>Load destination buffer</param>
	 * <remarks>
	 * <para header='Description'>Starts reading data with the specified binder and file name.<br/>
	 * Read loadSize bytes from offset fileOffset in the file.<br/>
	 * This function returns immediately.<br/>
	 * To get the load completion status, use the CriWare.CriFsLoader::GetStatus function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsLoader::GetStatus'/>
	 */
	public void Load(CriFsBinder binder, string path, long fileOffset, long loadSize, byte[] buffer)
	{
		this.dstGch = GCHandle.Alloc(buffer, GCHandleType.Pinned);

	#if !UNITY_EDITOR && UNITY_PSP2
		criFsLoader_LoadWorkaroundForVITA(this.handle,
							(binder != null) ? binder.nativeHandle : IntPtr.Zero, fileOffset,
							path, loadSize, this.dstGch.AddrOfPinnedObject(), buffer.Length);
	#else
		criFsLoader_Load(this.handle,
							(binder != null) ? binder.nativeHandle : IntPtr.Zero, path,
							fileOffset, loadSize, this.dstGch.AddrOfPinnedObject(), buffer.Length);
	#endif
	}

	/**
	 * <summary>Starts loading data.</summary>
	 * <param name='binder'>Binder</param>
	 * <param name='id'>File ID</param>
	 * <param name='fileOffset'>Offset from the beginning of the file (in bytes)</param>
	 * <param name='loadSize'>Load request size (in bytes)</param>
	 * <param name='buffer'>Load destination buffer</param>
	 * <remarks>
	 * <para header='Description'>Starts reading data with the specified binder and file ID.<br/>
	 * Read loadSize bytes from offset fileOffset in the file.<br/>
	 * This function returns immediately.<br/>
	 * To get the load completion status, use the CriWare.CriFsLoader::GetStatus function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsLoader::GetStatus'/>
	 */
	public void LoadById(CriFsBinder binder, int id, long fileOffset, long loadSize, byte[] buffer)
	{
		this.dstGch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		criFsLoader_LoadById(this.handle,
			(binder != null) ? binder.nativeHandle : IntPtr.Zero, id,
			fileOffset, loadSize, this.dstGch.AddrOfPinnedObject(), buffer.Length);
	}

	/**
	 * <summary>Loads the compressed data into memory without expanding it.</summary>
	 * <param name='binder'>Binder</param>
	 * <param name='path'>File path name</param>
	 * <param name='fileOffset'>Offset from the beginning of the file (in bytes)</param>
	 * <param name='loadSize'>Load request size (in bytes)</param>
	 * <param name='buffer'>Load destination buffer</param>
	 * <remarks>
	 * <para header='Description'>Starts reading data with the specified binder and file name.<br/>
	 * Read loadSize bytes from offset fileOffset in the file.<br/>
	 * Unlike the CriWare.CriFsLoader::Load function, even if the data is compressed, it will load the data in memory without expanding it.<br/>
	 * This function returns immediately.<br/>
	 * To get the load completion status, use the CriWare.CriFsLoader::GetStatus function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsLoader::GetStatus'/>
	 * <seealso cref='CriFsLoader::LoadWithoutDecompressionById'/>
	 * <seealso cref='CriFsLoader::DecompressData'/>
	 */
	public void LoadWithoutDecompression(CriFsBinder binder, string path, long fileOffset, long loadSize, byte[] buffer)
	{
		this.dstGch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		criFsLoader_LoadWithoutDecompression(this.handle,
			(binder != null) ? binder.nativeHandle : IntPtr.Zero, path,
			fileOffset, loadSize, this.dstGch.AddrOfPinnedObject(), buffer.Length);
	}

	/**
	 * <summary>Loads the compressed data into memory without expanding it.</summary>
	 * <param name='binder'>Binder</param>
	 * <param name='id'>File ID</param>
	 * <param name='fileOffset'>Offset from the beginning of the file (in bytes)</param>
	 * <param name='loadSize'>Load request size (in bytes)</param>
	 * <param name='buffer'>Load destination buffer</param>
	 * <remarks>
	 * <para header='Description'>Starts reading data with the specified binder and file ID.<br/>
	 * Read loadSize bytes from offset fileOffset in the file.<br/>
	 * Unlike the CriWare.CriFsLoader::Load function, even if the data is compressed, it will load the data in memory without expanding it.<br/>
	 * This function returns immediately.<br/>
	 * To get the load completion status, use the CriWare.CriFsLoader::GetStatus function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsLoader::GetStatus'/>
	 * <seealso cref='CriFsLoader::LoadWithoutDecompression'/>
	 * <seealso cref='CriFsLoader::DecompressData'/>
	 */
	public void LoadWithoutDecompressionById(CriFsBinder binder, int id, long fileOffset, long loadSize, byte[] buffer)
	{
		this.dstGch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		criFsLoader_LoadWithoutDecompressionById(this.handle,
			(binder != null) ? binder.nativeHandle : IntPtr.Zero, id,
			fileOffset, loadSize, this.dstGch.AddrOfPinnedObject(), buffer.Length);
	}

	/**
	 * <summary>Starts decompressing the compressed data place on the memory.</summary>
	 * <param name='srcSize'>Compressed data size (in bytes)</param>
	 * <param name='srcBuffer'>The buffer containing the compressed data</param>
	 * <param name='dstSize'>Size of the expansion destination buffer (in bytes)</param>
	 * <param name='dstBuffer'>Expansion destination buffer</param>
	 * <remarks>
	 * <para header='Description'>Expands the compressed data placed on the memory to another memory area.<br/>
	 * This function returns immediately.<br/>
	 * To get the load completion status, use the criFsLoader_GetStatus function.<br/></para>
	 * <para header='Note'>If the input data is not compressed, this function copies the input data to the output address as is.<br/></para>
	 * <para header='Note'>This function supports only CRI proprietary software compression codec.<br/>
	 * This function cannot be used to expand the data when using a hardware decoder or
	 * when using a platform-specific codec.</para>
	 * </remarks>
	 * <seealso cref='CriFsLoader::GetStatus'/>
	 * <seealso cref='CriFsLoader::LoadWithoutDecompression'/>
	 * <seealso cref='CriFsLoader::LoadWithoutDecompressionById'/>
	 */
	public void DecompressData(long srcSize, byte[] srcBuffer, long dstSize, byte[] dstBuffer)
	{
		this.srcGch = GCHandle.Alloc(srcBuffer, GCHandleType.Pinned);
		this.dstGch = GCHandle.Alloc(dstBuffer, GCHandleType.Pinned);
		criFsLoader_DecompressData(this.handle, this.srcGch.AddrOfPinnedObject(), srcSize, this.dstGch.AddrOfPinnedObject(), dstSize);
	}

	/**
	 * <summary>Stops the loading process.</summary>
	 * <remarks>
	 * <para header='Description'>Stops the loading process.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * To get the stopping status, use the CriWare.CriFsLoader::GetStatus function.<br/></para>
	 * <para header='Note'>Even if this function is called, data transfer to the buffer may continue
	 * until the loader status CriWare.CriFsLoader::Status changes to Stop.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsLoader::GetStatus'/>
	 */
	public void Stop()
	{
		if (this.handle != IntPtr.Zero) {
			criFsLoader_Stop(this.handle);
		}
	}

	/**
	 * <summary>Gets the loader status.</summary>
	 * <returns>Status</returns>
	 * <remarks>
	 * <para header='Description'>Gets the loader status.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsLoader::Status'/>
	 */
	public Status GetStatus()
	{
		Status status = Status.Stop;
		if (this.handle != IntPtr.Zero) {
			criFsLoader_GetStatus(this.handle, out status);
		}
		if (status != Status.Loading) {
			if (this.dstGch.IsAllocated) {
				this.dstGch.Free();
			}
			if (this.srcGch.IsAllocated) {
				this.srcGch.Free();
			}
		}
		return status;
	}

	/**
	 * <summary>Sets the unit read size</summary>
	 * <param name='unit_size'>Unit read size</param>
	 * <remarks>
	 * <para header='Description'>Sets the read unit size.
	 * When processing a read request with large size, CriFsLoader divides it into multiple reads with smaller size and repeats the reads.<br/>
	 * It is possible to change the read unit size by using this function.<br/>
	 * Cancellation of read request or interrupt of high priority read processing, etc. are processed only at the unit read size boundary.<br/>
	 * Therefore, setting a smaller unit size improves the response of I/O processing.<br/>
	 * Conversely, if you set a large unit size, the speed of reading files improves.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsLoader::Status'/>
	 */
	public void SetReadUnitSize(int unit_size)
	{
		if (this.handle != IntPtr.Zero) {
			criFsLoader_SetReadUnitSize(this.handle, unit_size);
		}
	}

	#region Internal Member
	private IntPtr handle;
	private GCHandle dstGch;
	private GCHandle srcGch;

	~CriFsLoader()
	{
		this.Dispose(false);
	}
	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_Create(out IntPtr loader);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_Destroy(IntPtr loader);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_Load(IntPtr loader, IntPtr binder, string path, long offset, long load_size, IntPtr buffer, long buffer_size);

#if !UNITY_EDITOR && UNITY_PSP2
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_LoadWorkaroundForVITA(IntPtr loader, IntPtr binder, long offset, string path, long load_size, IntPtr buffer, long buffer_size);
#endif

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_LoadById(IntPtr loader, IntPtr binder, int id, long offset, long load_size, IntPtr buffer, long buffer_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_Stop(IntPtr loader);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_GetStatus(IntPtr loader, out Status status);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_SetReadUnitSize(IntPtr loader, long unit_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_LoadWithoutDecompression(IntPtr loader, IntPtr binder, string path, long offset, long load_size, IntPtr buffer, long buffer_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_LoadWithoutDecompressionById(IntPtr loader, IntPtr binder, int id, long offset, long load_size, IntPtr buffer, long buffer_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsLoader_DecompressData(IntPtr loader, IntPtr src, long src_size, IntPtr dst, long dst_size);

	#else
	private static int criFsLoader_Create(out IntPtr loader) { loader = new IntPtr(1); return 0; }
	private static int criFsLoader_Destroy(IntPtr loader) { return 0; }
	private static int criFsLoader_Load(IntPtr loader, IntPtr binder, string path, long offset, long load_size, IntPtr buffer, long buffer_size)
		{ return 0; }
#if !UNITY_EDITOR && UNITY_PSP2
	private static int criFsLoader_LoadWorkaroundForVITA(IntPtr loader, IntPtr binder, long offset, string path, long load_size, IntPtr buffer, long buffer_size)
		{ return 0; }
#endif
	private static int criFsLoader_LoadById(IntPtr loader, IntPtr binder, int id, long offset, long load_size, IntPtr buffer, long buffer_size)
		{ return 0; }
	private static int criFsLoader_Stop(IntPtr loader) { return 0; }
	private static int criFsLoader_GetStatus(IntPtr loader, out Status status) { status = Status.Complete; return 0; }
	private static int criFsLoader_SetReadUnitSize(IntPtr loader, long unit_size) { return 0; }
	private static int criFsLoader_LoadWithoutDecompression(IntPtr loader, IntPtr binder, string path, long offset, long load_size, IntPtr buffer, long buffer_size)
		{ return 0; }
	private static int criFsLoader_LoadWithoutDecompressionById(IntPtr loader, IntPtr binder, int id, long offset, long load_size, IntPtr buffer, long buffer_size)
		{ return 0; }
	private static int criFsLoader_DecompressData(IntPtr loader, IntPtr src, long src_size, IntPtr dst, long dst_size)
		{ return 0; }
	#endif

	#endregion
}

/**
 * <summary>A module for installing files.</summary>
 * <remarks>
 * <para header='Description'>A module for installing files.<br/>
 * Used to install content on the  server to the local storage.<br/></para>
 * <para header='Note'>If the network connection times out, CriFsInstaller retries infinitely.
 * However, an error occurs and it doesn't retry in the following cases.
 * - Network connection timed out while checking the existence of the installation source file
 * - The installation source file did not exist
 * This class does not determine when to abort the infinite retry.<br/></para>
 * <para header='Note'>It is up to the application implementation to decide when to stop infinite retry.<br/>
 * For example, interruption by the following steps is possible.
 * -# Get the installation progress using the CriWare.CriFsInstaller::GetProgress function.
 * -# After a certain period of time, get the installation progress again.
 * -# If the values obtained in step 1. and step 2. are the same, stop the installation using the CriWare.CriFsInstaller::Stop function.</para>
 * </remarks>
 */
public class CriFsInstaller : CriDisposable
{
	/**
	 * <summary>A value indicating the installer status.</summary>
	 * <seealso cref='CriFsInstaller::GetStatus'/>
	 */
	public enum Status
	{
		Stop,       /**< Stopped */
		Busy,       /**< Installation in progress */
		Complete,   /**< Installation complete */
		Error       /**< Error occurred */
	}

	private byte[] installBuffer = null;
	private GCHandle installBufferGch;

	public CriFsInstaller()
	{
		if (!CriFsPlugin.IsLibraryInitialized()) {
			throw new Exception("CriFsPlugin is not initialized.");
		}

		/* ハンドルの作成 */
		this.handle = IntPtr.Zero;
		#pragma warning disable 162
		if (CriWare.Common.supportsCriFsInstaller == true) {
			criFsInstaller_Create(out this.handle, CopyPolicy.Always);
			if (this.handle == IntPtr.Zero) {
				throw new Exception("criFsInstaller_Create() failed.");
			} else {
				CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Fs);
			}
		} else {
			throw new Exception("CriFsInstaller is not supported on this platform");
		}
		#pragma warning restore 162
	}

	/**
	 * <summary>Discards the installer.</summary>
	 * <remarks>
	 * <para header='Note'>If you discard the installer during the installation process,
	 * the process may be blocked in this function for a long time.<br/></para>
	 * </remarks>
	 */
	public override void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		/* ハンドルの破棄 */
		if (this.handle != IntPtr.Zero) {
			criFsInstaller_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}

		if (disposing) {
			/* コピーバッファを解放 */
			if (this.installBuffer != null) {
				this.installBufferGch.Free();
				this.installBuffer = null;
			}
		}
	}


	/**
	 * <summary>Copies the file.</summary>
	 * <param name='binder'>Binder</param>
	 * <param name='srcPath'>Copy source file path name</param>
	 * <param name='dstPath'>Copy destination file path name</param>
	 * <param name='installBufferSize'>Install buffer size</param>
	 * <remarks>
	 * <para header='Description'>Starts copying files.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * To get the copy completion status, use the CriWare.CriFsInstaller::GetStatus function.</para>
	 * </remarks>
	 * <seealso cref='CriFsInstaller::GetStatus'/>
	 */
	public void Copy(CriFsBinder binder, string srcPath, string dstPath, int installBufferSize)
	{
		string copySrcPath = srcPath;
		if (copySrcPath.StartsWith("http:") || copySrcPath.StartsWith("https:")) {
			/* HTTP I/Oを使用したインストールはセカンダリ HTTP I/O デバイスを使用する */
			copySrcPath = "net2:" + copySrcPath;
		}
		if (installBufferSize > 0) {
			this.installBuffer    = new byte[installBufferSize];
			this.installBufferGch = GCHandle.Alloc(this.installBuffer, GCHandleType.Pinned);
			criFsInstaller_Copy(this.handle,
				(binder != null) ? binder.nativeHandle : IntPtr.Zero,
				copySrcPath, dstPath, this.installBufferGch.AddrOfPinnedObject(), this.installBuffer.Length);
		}
		else {
			criFsInstaller_Copy(this.handle,
				(binder != null) ? binder.nativeHandle : IntPtr.Zero,
				copySrcPath, dstPath, IntPtr.Zero, 0);
		}
	}

	/**
	 * <summary>Stops the installation process.</summary>
	 * <remarks>
	 * <para header='Description'>Stops the processing.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * Use the CriWare.CriFsInstaller::GetStatus function to get the stop completion status.</para>
	 * </remarks>
	 * <seealso cref='CriFsInstaller::GetStatus'/>
	 */
	public void Stop()
	{
		if (this.handle != IntPtr.Zero) {
			criFsInstaller_Stop(this.handle);
		}
	}

	/**
	 * <summary>Gets the status of the installer.</summary>
	 * <returns>Status</returns>
	 * <seealso cref='CriFsInstaller::Status'/>
	 */
	public Status GetStatus()
	{
		Status status = Status.Stop;
		if (this.handle != IntPtr.Zero) {
			criFsInstaller_GetStatus(this.handle, out status);
		}
		return status;
	}

	/**
	 * <summary>Gets the progress of the installation process.</summary>
	 * <returns>Progress status</returns>
	 * <remarks>
	 * <para header='Description'>Gets the progress of the process. <br/>
	 * It is a 32-bit floating point number in the range from 0.0 to 1.0.<br/></para>
	 * </remarks>
	 */
	public float GetProgress()
	{
		float progress = 0.0f;
		if(this.handle != IntPtr.Zero) {
			criFsInstaller_GetProgress(this.handle, out progress);
		}
		return progress;
	}

	/**
	 * <summary>Periodic execution function</summary>
	 * <remarks>
	 * <para header='Description'>Proceed with the installation process. It should be run regularly.<br/></para>
	 * </remarks>
	 */
	public static void ExecuteMain()
	{
		criFsInstaller_ExecuteMain();
	}

	#region Internal Member
	IntPtr handle;
	enum CopyPolicy
	{
		Always
	}

	~CriFsInstaller()
	{
		this.Dispose(false);
	}
	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsInstaller_ExecuteMain();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsInstaller_Create(out IntPtr installer, CopyPolicy option);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsInstaller_Destroy(IntPtr installer);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsInstaller_Copy(IntPtr installer, IntPtr binder,
		string src_path, string dst_path, IntPtr buffer, long buffer_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsInstaller_Stop(IntPtr installer);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsInstaller_GetStatus(IntPtr installer, out Status status);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsInstaller_GetProgress(IntPtr installer, out float progress);

	#else
	private static void criFsInstaller_ExecuteMain() { }
	private static int criFsInstaller_Create(out IntPtr installer, CopyPolicy option) { installer = new IntPtr(1);  return 0; }
	private static int criFsInstaller_Destroy(IntPtr installer) { return 0; }
	private static int criFsInstaller_Copy(IntPtr installer, IntPtr binder,
		string src_path, string dst_path, IntPtr buffer, long buffer_size) { return 0; }
	private static int criFsInstaller_Stop(IntPtr installer) { return 0; }
	private static int criFsInstaller_GetStatus(IntPtr installer, out Status status) { status = Status.Complete; return 0; }
	private static int criFsInstaller_GetProgress(IntPtr installer, out float progress) { progress = 1.0f; return 0; }
	#endif

	#endregion
}

/**
 * <summary>A module for accessing the content in a CPK file.</summary>
 * <remarks>
 * <para header='Description'>CriFsBinder (binder) is a database module to handle files efficiently.<br/>
 * By binding the CPK file or directory to the binder,
 * the content information in the CPK file or directory can be acquired.<br/></para>
 * </remarks>
 */
public class CriFsBinder : CriDisposable
{
	/**
	 * <summary>A value that indicates the status of the binder.</summary>
	 * <seealso cref='CriFsBinder::GetStatus'/>
	 */
	public enum Status
	{
		None,       /**< Stopped */
		Analyze,    /**< Binding in progress */
		Complete,   /**< Binding completed */
		Unbind,     /**< Unbinding in progress */
		Removed,    /**< Unbinding complete */
		Invalid,    /**< Binding disabled */
		Error       /**< Binding failed */
	}

	/**
	 * <summary>Content file information structure</summary>
	 * <remarks>
	 * <para header='Description'>Output information of the CriWare.CriFsBinder::GetContentsFileInfo function. <br/>
	 * Information relative to the access of the content of the retrieved CPK file is stored.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetContentsFileInfo(int, out ContentsFileInfo)'/>
	 * <seealso cref='CriFsBinder::GetContentsFileInfo(string, out ContentsFileInfo)'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct ContentsFileInfo {
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string directory;   /**< Directory name */
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string fileName;    /**< File name */
		public uint readSize;               /**< Read size (file size) */
		public uint extractSize;            /**< Expanded size (same as readSize for uncompressed data) */
		public ulong offset;                /**< Offset from the beginning of the CPK file */
		public int fileId;                  /**< File ID */
		[MarshalAs(UnmanagedType.LPStr)]
		string userStr;                     /**< User string (not supported) */

		public ContentsFileInfo(byte[] data, int startIndex)
		{
			if (IntPtr.Size == 4) {
				this.directory =  Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
				this.fileName =  Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 4)));
				this.readSize = BitConverter.ToUInt32(data, startIndex + 8);
				this.extractSize = BitConverter.ToUInt32(data, startIndex + 12);
				this.offset = BitConverter.ToUInt64(data, startIndex + 16);
				this.fileId = BitConverter.ToInt32(data, startIndex + 24);
				this.userStr = null;
			} else {
				this.directory =  Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
				this.fileName =  Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 8)));
				this.readSize = BitConverter.ToUInt32(data, startIndex + 16);
				this.extractSize = BitConverter.ToUInt32(data, startIndex + 20);
				this.offset = BitConverter.ToUInt64(data, startIndex + 24);
				this.fileId = BitConverter.ToInt32(data, startIndex + 32);
				this.userStr = null;
			}
		}
	}

	public CriFsBinder()
	{
		if (!CriFsPlugin.IsLibraryInitialized()) {
			throw new Exception("CriFsPlugin is not initialized.");
		}

		/* ハンドルの作成 */
		this.handle = IntPtr.Zero;
		criFsBinder_Create(out this.handle);
		if (this.handle == IntPtr.Zero) {
			throw new Exception("criFsBinder_Create() failed.");
		}
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Fs);
	}

	/**
	 * <summary>Discard the binder.</summary>
	 * <remarks>
	 * <para header='Note'>If you discard the binder during the binding process or discard the binder without performing the Unbind process,
	 * the process may be blocked in this function for a long time.<br/></para>
	 * </remarks>
	 */
	public override void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		/* ハンドルの破棄 */
		if (this.handle != IntPtr.Zero) {
			criFsBinder_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}
	}

	/**
	 * <summary>Binds the CPK file.</summary>
	 * <param name='srcBinder'>A binder to access the CPK file to bind</param>
	 * <param name='path'>Path name of the CPK file to bind</param>
	 * <returns>Bind ID</returns>
	 * <remarks>
	 * <para header='Description'>To use the CPK file, you need to bind the CPK file.<br/>
	 * This function binds the CPK file to the binder and returns the bind ID.<br/>
	 * <br/>
	 * For srcBinder, specify the binder to search for the CPK file.<br/>
	 * If srcBinder is set to null, the default device is used.<br/>
	 * <br/>
	 * If the bind cannot be started, 0 is returned as the bind ID.<br/>
	 * If a value other than 0 is returned as the bind ID, internal resources are allocated.<br/>
	 * So be sure to unbind the bind ID that is no longer required.<br/>
	 * (The CPK file being bound is kept open.)<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * Immediately after returning from this function, CPK binding is not completed yet,
	 * so you cannot access the CPK content file.<br/>
	 * CPK will be available after the bound state becomes
	 * Complete.<br/>
	 * You can get the binding status using the CriWare.CriFsBinder::GetStatus function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetStatus'/>
	 * <seealso cref='CriFsBinder::Unbind'/>
	 */
	public uint BindCpk(CriFsBinder srcBinder, string path)
	{
		uint bindId = 0u;
		if (this.handle != IntPtr.Zero) {
			criFsBinder_BindCpk(this.handle,
				(srcBinder != null) ? srcBinder.nativeHandle : IntPtr.Zero,
				path, IntPtr.Zero, 0, out bindId);
		}
		return bindId;
	}

	/**
	 * <summary>Binds the directory path.</summary>
	 * <param name='srcBinder'>The binder to access the directory to be bound</param>
	 * <param name='path'>Path name of the directory to be bound</param>
	 * <remarks>
	 * <para header='Description'>Binds the directory pathname.<br/>
	 * Specify the directory name to bind with an absolute path.<br/>
	 * <br/>
	 * The system does not check whether the directory exists at the time of binding.<br/>
	 * Only the directory path is retained in the binder;
	 * it does not open the files in the specified directory.<br/>
	 * Therefore, unless the bind fails, the bind status of the bind ID is Complete
	 * when this function returns.<br/>
	 * <br/>
	 * If the bind cannot be started, 0 is returned as the bind ID.<br/>
	 * If a value other than 0 is returned as the bind ID, internal resources are allocated.<br/>
	 * So be sure to unbind the bind ID that is no longer required.<br/></para>
	 * <para header='Note'>This function is a debug function for development support.<br/>
	 * If you use this function, the following problems may occur.<br/>
	 * - Functions CriWare.CriFsLoader::Load or CriWare.CriFsBinder::GetFileSize may block for a long time.<br/>
	 * - When accessing files in the bound directory, stream playback of sound or movie is interrupted.<br/></para>
	 * <para header='Note'>Be careful not to use this function in the application when final release.<br/>
	 * (Convert the data in the directory to a CPK file and bind it using the CriWare.CriFsBinder::BindCpk function,
	 * or bind all the files in the directory using the CriWare.CriFsBinder::BindFile function.)<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetStatus'/>
	 * <seealso cref='CriFsBinder::Unbind'/>
	 * <seealso cref='CriFsBinder::BindCpk'/>
	 * <seealso cref='CriFsBinder::BindFile'/>
	 */
	public uint BindDirectory(CriFsBinder srcBinder, string path)
	{
		uint bindId = 0u;
		if (this.handle != IntPtr.Zero) {
			criFsBinder_BindDirectory(this.handle,
				(srcBinder != null) ? srcBinder.nativeHandle : IntPtr.Zero,
				path, IntPtr.Zero, 0, out bindId);
		}
		return bindId;
	}

	/**
	 * <summary>Binds the file.</summary>
	 * <param name='srcBinder'>The binder to access the file to be bound</param>
	 * <param name='path'>The path name of the file to be bound</param>
	 * <returns>Bind ID</returns>
	 * <remarks>
	 * <para header='Description'>Binds a file and returns the bind ID.<br/>
	 * (Searches and finds the file specified by path from the binder srcBinder.)<br/>
	 * If srcBinder is set to null, the default device is used.<br/>
	 * <br/>
	 * If the bind cannot be started, 0 is returned as the bind ID.<br/>
	 * If a value other than 0 is returned as the bind ID, internal resources are allocated.<br/>
	 * So be sure to unbind the bind ID that is no longer required.<br/>
	 * (The CPK file being bound is kept open.)<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * Immediately after returning from this function, file binding is not completed yet,
	 * so you cannot access the files using the bind ID.<br/>
	 * The files will be available after the bound status of
	 * the bind ID becomes Complete.<br/>
	 * You can get the binding status using the CriWare.CriFsBinder::GetStatus function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetStatus'/>
	 * <seealso cref='CriFsBinder::Unbind'/>
	 */
	public uint BindFile(CriFsBinder srcBinder, string path)
	{
		uint bindId = 0u;

		criFsBinder_BindFile(this.handle,
			(srcBinder != null) ? srcBinder.nativeHandle : IntPtr.Zero,
			path, IntPtr.Zero, 0, out bindId);
		return bindId;
	}

	/**
	 * <summary>Bind a part of the file so that the part can be treated as a virtual file.</summary>
	 * <param name='srcBinder'>The binder to access the file to be bound</param>
	 * <param name='path'>The path name of the file to be bound</param>
	 * <param name='offset'>Data start position (bytes)</param>
	 * <param name='size'>Data size (bytes)</param>
	 * <param name='sectionName'>Section name</param>
	 * <returns>Bind ID</returns>
	 * <remarks>
	 * <para header='Description'>Binds a part of the file and returns the bind ID.<br/>
	 * (Searches and finds the file specified by path from the binder srcBinder.)<br/>
	 * If srcBinder is set to null, the default device is used.<br/>
	 * <br/>
	 * If the bind cannot be started, 0 is returned as the bind ID.<br/>
	 * If a value other than 0 is returned as the bind ID, internal resources are allocated.<br/>
	 * So be sure to unbind the bind ID that is no longer required.<br/>
	 * (The CPK file being bound is kept open.)<br/>
	 * (Specify the section name in the path when loading.)<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * Immediately after returning from this function, file binding is not completed yet,
	 * so you cannot access the files using the bind ID.<br/>
	 * The files will be available after the bound status of
	 * the bind ID becomes Complete.<br/>
	 * You can get the binding status using the CriWare.CriFsBinder::GetStatus function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetStatus'/>
	 * <seealso cref='CriFsBinder::Unbind'/>
	 */
	public uint BindFileSection(CriFsBinder srcBinder, string path, ulong offset, int size, string sectionName)
	{
		uint bindId = 0u;
		if (this.handle != IntPtr.Zero) {
			criFsBinder_BindFileSection(this.handle,
				(srcBinder != null) ? srcBinder.nativeHandle : IntPtr.Zero, path,
				offset, size, sectionName,
				IntPtr.Zero, 0, out bindId);
		}
		return bindId;
	}

	/**
	 * <summary>Unbinds the bound content.</summary>
	 * <param name='bindId'>Bind ID</param>
	 * <remarks>
	 * <para header='Description'>Unbind the bound content.<br/>
	 * Specify which content to unbind in the bind ID.<br/></para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * It closes files as necessary, so it may take several milliseconds
	 * depending on the execution environment.<br/>
	 * <br/>
	 * Other bind IDs that depend on the bind ID to be unbound are
	 * also unbound at the same time (implicit unbinding).<br/>
	 * For example, the bind IDs that are binding the content files of the CPK bind ID
	 * are unbound implicitly when the referencing CPK bind ID is unbound.<br/></para>
	 * </remarks>
	 * <seealso cref='CriWare.CriFsBinder::BindCpk'/>
	 * <seealso cref='CriFsBinder::BindFile'/>
	 */
	public static void Unbind(uint bindId)
	{
		if (CriFsPlugin.IsLibraryInitialized()) {
			criFsBinder_Unbind(bindId);
		}
	}

	/**
	 * <summary>Gets the binder status.</summary>
	 * <returns>Status</returns>
	 * <seealso cref='CriFsBinder::Status'/>
	 */
	public static Status GetStatus(uint bindId)
	{
		Status status = Status.Removed;
		if (CriFsPlugin.IsLibraryInitialized()) {
			criFsBinder_GetStatus(bindId, out status);
		}
		return status;
	}

	/**
	 * <summary>Gets the file size.</summary>
	 * <param name='path'>Full path of the file</param>
	 * <returns>File size</returns>
	 * <remarks>
	 * <para header='Description'>Gets the file size of the specified file.<br/>
	 * Bind IDs whose binding status is Complete are searched.<br/>
	 * <br/>
	 * If the specified file does not exist, a negative value is returned.</para>
	 * <para header='Note'>If directories are bound by the CriWare.CriFsBinder::BindDirectory function,
	 * processing may be blocked for a long time within this function.</para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetFileSize(int)'/>
	 */
	public long GetFileSize(string path)
	{
		long size = -1;
		if (this.handle != IntPtr.Zero) {
			int err;
			err = criFsBinder_GetFileSize(this.handle, path, out size);
			if (err != 0) {
				return (-1);
			}
		}
		return size;
	}

	/**
	 * <summary>Gets the file size.</summary>
	 * <param name='id'>File ID</param>
	 * <returns>File size</returns>
	 * <remarks>
	 * <para header='Description'>Gets the file size.<br/>
	 * The CPK file with ID information needs to be bound.<br/>
	 * Bind IDs whose binding status is Complete are searched.<br/>
	 * <br/>
	 * If the specified file does not exist, a negative value is returned.</para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetFileSize(string)'/>
	 */
	public long GetFileSize(int id)
	{
		long size = -1;
		if (this.handle != IntPtr.Zero) {
			int err;
			err = criFsBinder_GetFileSizeById(this.handle, id, out size);
			if (err != 0) {
				return (-1);
			}
		}
		return size;
	}

	/**
	 * <summary>Gets the CPK content file information.</summary>
	 * <param name='path'>Full path of the file</param>
	 * <param name='info'>Content file information</param>
	 * <returns>Whether the acquisition succeeded</returns>
	 * <remarks>
	 * <para header='Description'>Gets the information on the file with the specified file name from the CPK file with ID+ file name information.<br/>
	 * The CPK that contains the specified file must be the CPK with ID+ file name information.<br/>
	 * If there are multiple files with the same name in the specified binder handle,
	 * the CPK containing the first file found will be selected.</para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetContentsFileInfo(int, out ContentsFileInfo)'/>
	 */
	public bool GetContentsFileInfo(string path, out ContentsFileInfo info)
	{
		using (var mem = new CriStructMemory<CriFsBinder.ContentsFileInfo>()) {
			int err = criFsBinder_GetContentsFileInfo(this.handle, path, mem.ptr);
			info = new CriFsBinder.ContentsFileInfo(mem.bytes, 0);
			return (err == 0);
		}
	}

	/**
	 * <summary>Gets the CPK content file information.</summary>
	 * <param name='id'>File ID</param>
	 * <param name='info'>Content file information</param>
	 * <returns>Whether the acquisition succeeded</returns>
	 * <remarks>
	 * <para header='Description'>Gets the information on the file with the specified file ID from the CPK file with ID+ file name information.<br/>
	 * The CPK that contains the specified file must be the CPK with ID+ file name information.<br/>
	 * If there are multiple files with the same ID in the specified binder handle,
	 * the CPK containing the first file found will be selected.</para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetContentsFileInfo(string, out ContentsFileInfo)'/>
	 */
	public bool GetContentsFileInfo(int id, out ContentsFileInfo info)
	{
		using (var mem = new CriStructMemory<CriFsBinder.ContentsFileInfo>()) {
			int err = criFsBinder_GetContentsFileInfoById(this.handle, id, mem.ptr);
			info = new CriFsBinder.ContentsFileInfo(mem.bytes, 0);
			return (err == 0);
		}
	}

	/**
	 * <summary>Gets the CPK content file information.</summary>
	 * <param name='bindId'>Bind ID</param>
	 * <param name='index'>Start index of the content file from which information is obtained</param>
	 * <param name='numFiles'>The number of content files from which information is obtained</param>
	 * <param name='info'>Content file information array</param>
	 * <returns>Whether the acquisition succeeded</returns>
	 * <remarks>
	 * <para header='Description'>Gets the information for the specified number of files from the specified index from the CPK file with ID+ file name information.<br/>
	 * The index is assigned starting from 0 to the content file when creating the CPK.<br/>
	 * The maximum value that can be specified for index and numFiles is the total number of files that can be acquired using CriWare.CriFsBinder::GetNumContentsFiles .</para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetContentsFileInfo(string, out ContentsFileInfo)'/>
	 * <seealso cref='CriFsBinder::GetNumContentsFiles'/>
	 */
	public static bool GetContentsFileInfoByIndex(uint bindId, int index, int numFiles, out ContentsFileInfo[] info)
	{
		if (index < 0 || numFiles <= 0) {
			throw new Exception("Invalid parameters.");
		}

		info = new ContentsFileInfo[numFiles];
		using (var mem = new CriStructMemory<CriFsBinder.ContentsFileInfo>(numFiles)) {
			int err = criFsBinder_GetContentsFileInfoByIndex(bindId, index, mem.ptr, numFiles);
			for (int i = 0; i < numFiles; i++) {
				info[i] = new CriFsBinder.ContentsFileInfo(mem.bytes, i * Marshal.SizeOf(typeof(CriFsBinder.ContentsFileInfo)));
			}
			return (err == 0);
		}
	}

	/**
	 * <summary>Gets the total number of CPK content files.</summary>
	 * <param name='bindId'>Bind ID</param>
	 * <returns>The number of content files</returns>
	 * <remarks>
	 * <para header='Description'>Gets the total number of included files from the CPK file with ID+ file name information.<br/>
	 * The return value of this function is the maximum number of content files for obtaining information
	 * that can be specified in the CriWare.CriFsBinder::GetContentsFileInfoByIndex function for the same bind ID.</para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::GetContentsFileInfoByIndex'/>
	 */
	public static int GetNumContentsFiles(uint bindId)
	{
		return CRIWAREB3558960(bindId);
	}

	/**
	 * <summary>Sets the search priority for a bind ID.</summary>
	 * <param name='bindId'>Bind ID</param>
	 * <param name='priority'>Priority</param>
	 * <remarks>
	 * <para header='Description'>Sets the search priority in the binder for the bind ID.<br/>
	 * The lowest priority is 0, and the higher the value, the higher the search priority.<br/>
	 * For bind IDs with the same priority, the one bound first is found first.<br/>
	 * If no priority is set, the default priority is 0.<br/>
	 * Bind IDs whose binding status is Complete are searched.<br/></para>
	 * </remarks>
	 */
	public static void SetPriority(uint bindId, int priority)
	{
		if (CriFsPlugin.IsLibraryInitialized()) {
			criFsBinder_SetPriority(bindId, priority);
		}
	}

	public IntPtr nativeHandle {
		get { return this.handle; }
	}

	#region Internal Member
	private IntPtr handle;

	~CriFsBinder()
	{
		this.Dispose(false);
	}
	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern uint criFsBinder_Create(out IntPtr binder);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern uint criFsBinder_Destroy(IntPtr binder);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern uint criFsBinder_BindCpk(IntPtr binder,
		IntPtr srcBinder, string path, IntPtr work, int worksize, out uint bindId);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern uint criFsBinder_BindDirectory(IntPtr binder,
		IntPtr srcBinder, string path, IntPtr work, int worksize, out uint bindId);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern uint criFsBinder_BindFile(IntPtr binder,
		IntPtr srcBinder, string path, IntPtr work, int worksize, out uint bindId);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern uint criFsBinder_BindFileSection(IntPtr binder,
		IntPtr srcBinder, string path, ulong offset, int size, string sectionName,
		IntPtr work, int worksize, out uint bindId);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsBinder_Unbind(uint bindId);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsBinder_GetStatus(uint bindId, out Status status);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsBinder_GetFileSize(IntPtr binder, string path, out long size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsBinder_GetFileSizeById(IntPtr binder, int id, out long size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsBinder_SetPriority(uint bindId, int priority);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsBinder_GetContentsFileInfo(IntPtr binder, string path, IntPtr info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsBinder_GetContentsFileInfoById(IntPtr binder, int id, IntPtr info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsBinder_GetContentsFileInfoByIndex(uint id, int index, IntPtr info, int num);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int CRIWAREB3558960(uint id);

	#else
	private static uint criFsBinder_Create(out IntPtr binder) { binder = new IntPtr(1); return 0u; }
	private static uint criFsBinder_Destroy(IntPtr binder) { return 0u; }
	private static uint criFsBinder_BindCpk(IntPtr binder,
		IntPtr srcBinder, string path, IntPtr work, int worksize, out uint bindId) { bindId = 0u; return 0u; }
	private static uint criFsBinder_BindDirectory(IntPtr binder,
		IntPtr srcBinder, string path, IntPtr work, int worksize, out uint bindId) { bindId = 0u; return 0u; }
	private static uint criFsBinder_BindFile(IntPtr binder,
		IntPtr srcBinder, string path, IntPtr work, int worksize, out uint bindId) { bindId = 0u; return 0u; }
	private static uint criFsBinder_BindFileSection(IntPtr binder,
		IntPtr srcBinder, string path, ulong offset, int size, string sectionName,
		IntPtr work, int worksize, out uint bindId) { bindId = 0u; return 0u; }
	private static int criFsBinder_Unbind(uint bindId) { return 0; }
	private static int criFsBinder_GetStatus(uint bindId, out Status status) { status = Status.Complete; return 0; }
	private static int criFsBinder_GetFileSize(IntPtr binder, string path, out long size) { size = 0; return 0; }
	private static int criFsBinder_GetFileSizeById(IntPtr binder, int id, out long size) { size = 0; return 0; }
	private static int criFsBinder_SetPriority(uint bindId, int priority) { return 0; }
	private static int criFsBinder_GetContentsFileInfo(IntPtr binder, string path, IntPtr info) { return 0; }
	private static int criFsBinder_GetContentsFileInfoById(IntPtr binder, int id, IntPtr info) { return 0; }
	private static int criFsBinder_GetContentsFileInfoByIndex(uint id, int index, IntPtr info, int num) { return 0; }
	private static int CRIWAREB3558960(uint id) { return 0; }
	#endif

	#endregion
}

} //namespace CriWare
/**
 * @}
 */


/*==========================================================================
 *     CRI File System Unity Component
 *=========================================================================*/
/**
 * \addtogroup CRIFS_UNITY_COMPONENT
 * @{
 */

namespace CriWare {

/**
 * <summary>A module for checking the progress or getting the result of the asynchronous process.</summary>
 * <remarks>
 * <para header='Description'>This module is used to check the progress from - or obtain the results of - the asynchronous processing.<br/>
 * When the asynchronous processing is completed, the value of "isDone" will be set to true.<br/>
 * If an error occured during the asynchronous processing, information about the error will be stored in "error".<br/></para>
 * </remarks>
 */
public class CriFsRequest : CriDisposable
{
	public delegate void DoneDelegate(CriFsRequest request);

	/**
	 * <summary>Delegate when processing is complete.</summary>
	 * <remarks>
	 * <para header='Description'>This parameter is used to determine whether asynchronous processing has completed. <br/>
	 * During asynchronous processing, the value of isDone is false. <br/>
	 * Once asynchronous processing is complete, the value of isDone is true.<br/></para>
	 * </remarks>
	 */
	public DoneDelegate doneDelegate { get; protected set; }

	/**
	 * <summary>Whether the processing is complete.</summary>
	 * <remarks>
	 * <para header='Description'>This parameter is used to determine whether asynchronous processing has completed. <br/>
	 * During asynchronous processing, the value of isDone is false. <br/>
	 * Once asynchronous processing is complete, the value of isDone is true.<br/></para>
	 * </remarks>
	 */
	public bool isDone  { get; private set; }

	/**
	 * <summary>Error information.</summary>
	 * <remarks>
	 * <para header='Description'>A parameter to check whether an error occurred during asynchronous process.<br/>
	 * If the asynchronous process completes successfully, the value of error is null.<br/>
	 * If an error occurs during asynchronous process, the error information is stored.<br/></para>
	 * </remarks>
	 */
	public string error { get; protected set; }

	/**
	 * <summary>Discard information.</summary>
	 * <remarks>
	 * <para header='Description'>A parameter to check if the request was discarded.<br/></para>
	 * </remarks>
	 */
	public bool isDisposed { get; protected set; }


	public override void Dispose()
	{
		if (!this.isDisposed) {
			this.Dispose(true);
			this.isDisposed = true;
			System.GC.SuppressFinalize(this);
		}
	}

	/**
	 * <summary>Stops the asynchronous process.</summary>
	 * <remarks>
	 * <para header='Description'>Stops the asynchronous process.<br/></para>
	 * </remarks>
	 */
	virtual public void Stop()
	{
	}

	/**
	 * <summary>Waits for the completion of asynchronous process.</summary>
	 * <remarks>
	 * <para header='Description'>Suspend the execution of coroutine until asynchronous process is complete.<br/>
	 * <br/>
	 * This function can be used only in the yield statement in the coroutine.<br/>
	 * Specifically, it should be used in the following format.<br/><code>
	 *      ：
	 *  // 非同期処理の開始
	 *  CriFsRequest request = CriFsUtility.?
	 *
	 *  // 非同期処理の完了まで待機
	 *  yield return request.WaitForDone(this);
	 *      ：
	 * </code>
	 * </para>
	 * </remarks>
	 */
	public YieldInstruction WaitForDone(MonoBehaviour mb)
	{
		return mb.StartCoroutine(CheckDone());
	}

	#region Internal Methods
	virtual protected void Dispose(bool disposing)
	{
	}

	virtual public void Update()
	{
	}

	protected void Done()
	{
		this.isDone = true;
		if (this.doneDelegate != null) {
			this.doneDelegate(this);
		}
	}

	IEnumerator CheckDone()
	{
		while (!this.isDone) {
			yield return null;
		}
	}

	~CriFsRequest()
	{
		if (!this.isDisposed) {
			this.Dispose(false);
			this.isDisposed = true;
		}
	}
	#endregion
}

/**
 * <summary>A module for checking the progress or getting the result of the loading process.</summary>
 * <remarks>
 * <para header='Description'>A module for checking the progress or getting the result of the loading process.<br/>
 * It is returned as the return value of the CriWare.CriFsUtility::LoadFile function.<br/>
 * <br/>
 * The value of isDone becomes true when the loading process completes.<br/>
 * Load result is stored in bytes.<br/>
 * If an error occurs during the loading process, the error information is stored in error.<br/></para>
 * </remarks>
 * <seealso cref='CriFsUtility::LoadFile'/>
 */
public class CriFsLoadFileRequest : CriFsRequest
{
	/**
	 * <summary>The path to the file to be loaded.</summary>
	 * <remarks>
	 * <para header='Description'>The path to the file to be loaded.<br/>
	 * The path specified when calling the CriWare.CriFsUtility::LoadFile function is stored.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsUtility::LoadFile'/>
	 */
	public string path { get; private set; }

	/**
	 * <summary>A buffer that stores load result.</summary>
	 * <remarks>
	 * <para header='Description'>A buffer that stores the load result.<br/>
	 * If the loading process is stopped using the CriWare.CriFsLoadFileRequest::Stop function
	 * or if an error occurs during loading, the value is null.<br/></para>
	 * </remarks>
	 */
	public byte[] bytes { get; private set; }

	#region Internal Methods

	public CriFsLoadFileRequest(CriFsBinder srcBinder, string path, CriFsRequest.DoneDelegate doneDelegate, int readUnitSize)
	{
		/* パスの保存 */
		this.path = path;

		/* 完了コールバック指定 */
		this.doneDelegate = doneDelegate;

		/* readUnitSizeの保存 */
		this.readUnitSize = readUnitSize;

		/* ファイルのバインド要求 */
		if (srcBinder == null) {
			this.newBinder = new CriFsBinder();
			this.refBinder = this.newBinder;
			this.bindId = this.newBinder.BindFile(srcBinder, path);
			this.phase = Phase.Bind;
		} else {
			this.newBinder = null;
			this.refBinder = srcBinder;
			this.fileSize = srcBinder.GetFileSize(path);
			if (this.fileSize < 0) {
				this.phase = Phase.Error;
			} else {
				this.phase = Phase.Load;
			}
		}

		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Fs);
	}

	override protected void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		/* ローダの破棄 */
		if (this.loader != null) {
			this.loader.Dispose();
			this.loader = null;
		}

		/* バインダの破棄 */
		if (this.newBinder != null) {
			this.newBinder.Dispose();
			this.newBinder = null;
		}

		/* メモリの解放 */
		this.bytes = null;
	}

	public override void Stop()
	{
		/* ローダに停止要求を発行 */
		if (this.phase == Phase.Load) {
			if (this.loader != null) {
				this.loader.Stop();
			}
		}
	}

	public override void Update()
	{
		if (this.phase == Phase.Bind) {
			this.UpdateBinder();
		}
		if (this.phase == Phase.Load) {
			this.UpdateLoader();
		}
		if (this.phase == Phase.Error) {
			this.OnError();
		}
	}

	private void UpdateBinder()
	{
		/* バインダのステータスをチェック */
		CriFsBinder.Status binderStatus = CriFsBinder.GetStatus(this.bindId);
		if (binderStatus == CriFsBinder.Status.Analyze) {
			/* バインド中は何もしない */
			return;
		}

		switch (binderStatus) {
			/* バインド完了 */
			case CriFsBinder.Status.Complete:
			this.fileSize = this.refBinder.GetFileSize(this.path);
			break;

			/* 上記以外はエラー扱い */
			default:
			this.fileSize = -1;
			break;
		}

		/* エラーチェック */
		if (this.fileSize < 0) {
			/* エラー状態に遷移してそっちで後始末 */
			this.phase = Phase.Error;
			return;
		}

		/* フェーズの更新 */
		this.phase = Phase.Load;
	}

	private void UpdateLoader()
	{
		/* ローダが確保済みかどうかチェック */
		if (this.loader == null) {
			/* ロード要求の発行 */
			this.loader = new CriFsLoader();
			this.loader.SetReadUnitSize(readUnitSize);
			this.bytes = new byte[this.fileSize];
			this.loader.Load(this.refBinder, this.path, 0, this.fileSize, this.bytes);
		}

		/* ローダのステータスをチェック */
		CriFsLoader.Status loaderStatus = this.loader.GetStatus();
		if (loaderStatus == CriFsLoader.Status.Loading) {
			/* ロード中は何もしない */
			return;
		}

		/* エラーチェック */
		switch (loaderStatus) {
			case CriFsLoader.Status.Stop:
			this.bytes = null;
			break;

			case CriFsLoader.Status.Error:
			/* エラーに遷移 */
			this.phase = Phase.Error;
			return;

			default:
			break;
		}

		/* フェーズの更新 */
		this.phase = Phase.Done;

		/* ローダの破棄 */
		this.loader.Dispose();
		this.loader = null;

		/* バインダの破棄 */
		if (this.newBinder != null) {
			this.newBinder.Dispose();
			this.newBinder = null;
		}

		/* 処理の完了を通知 */
		this.Done();
	}

	private void OnError()
	{
		this.bytes = null;
		this.error = "Error occurred.";
		this.refBinder = null;
		/* バインダの破棄 */
		if (this.newBinder != null) {
			this.newBinder.Dispose();
			this.newBinder = null;
		}
		/* ローダの破棄 */
		if (this.loader != null) {
			this.loader.Dispose();
			this.loader = null;
		}
		this.phase = Phase.Done;
		this.Done();

		return;
	}

	#endregion

	#region Internal Fields

	private enum Phase
	{
		Stop,
		Bind,
		Load,
		Done,
		Error
	};

	private Phase phase = Phase.Stop;
	private CriFsBinder refBinder = null;
	private CriFsBinder newBinder = null;
	private uint bindId = 0;
	private CriFsLoader loader = null;
	private int readUnitSize = 0;
	private long fileSize = 0;

	#endregion
}

/**
 * <summary>A module for checking the progress of Asset Bundle processing and acquiring the result of Asset Bundle processing.</summary>
 * <remarks>
 * <para header='Description'>A module for checking the progress of Asset Bundle processing and acquiring the result of Asset Bundle processing.<br/>
 * It is returned as the return value of the CriWare.CriFsUtility::LoadAssetBundle function.<br/>
 * <br/>
 * The value of isDone becomes true when the Asset Bundle process completes.<br/>
 * Asset Bundle result is stored in assetBundle.<br/>
 * If an error occurs during the Asset Bundle process, the error information is stored in error.<br/></para>
 * </remarks>
 * <seealso cref='CriFsUtility::LoadAssetBundle'/>
 */
public class CriFsLoadAssetBundleRequest : CriFsRequest
{
	/**
	 * <summary>The path to the Asset Bundle file to be loaded.</summary>
	 * <remarks>
	 * <para header='Description'>The path to the Asset Bundle file to be loaded.<br/>
	 * The path specified when calling the CriWare.CriFsUtility::LoadAssetBundle function is stored.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsUtility::LoadAssetBundle'/>
	 */
	public string path { get; private set; }

	/**
	 * <summary>The Asset Bundle that is the result of loading.</summary>
	 * <remarks>
	 * <para header='Description'>An instance of Asset Bundle that stores the loading result.<br/>
	 * If the loading process is stopped using the CriWare.CriFsLoadFileRequest::Stop function
	 * or if an error occurs during loading, the value is null.<br/></para>
	 * </remarks>
	 */
	public AssetBundle assetBundle { get; private set; }

	#region Internal Methods

	public CriFsLoadAssetBundleRequest(CriFsBinder binder, string path, int readUnitSize)
	{
		this.path = path;
		this.loadFileReq = CriFsUtility.LoadFile(binder, path, readUnitSize);
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Fs);
	}

	override public void Update()
	{
		if (this.loadFileReq != null) {
			if (this.loadFileReq.isDone) {
				if (this.loadFileReq.error != null) {
					this.error = "Error occurred.";
					this.Done();
				} else {
#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
					this.assetBundleReq = AssetBundle.CreateFromMemory(this.loadFileReq.bytes);
#else
					this.assetBundleReq = AssetBundle.LoadFromMemoryAsync(this.loadFileReq.bytes);
#endif
				}
				this.loadFileReq.Dispose();
				this.loadFileReq = null;
			}
		} else if (this.assetBundleReq != null) {
			if (this.assetBundleReq.isDone) {
				this.assetBundle = this.assetBundleReq.assetBundle;
				this.Done();
			}
		} else {
			this.Done();
		}
	}

	override protected void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		/* ローダの破棄 */
		if (this.loadFileReq != null) {
			this.loadFileReq.Dispose();
			this.loadFileReq = null;
		}
	}

	#endregion

	#region Internal Fields

	private CriFsLoadFileRequest loadFileReq;
	private AssetBundleCreateRequest assetBundleReq;

	#endregion
}

/**
 * <summary>A module for checking the progress or getting the result of the installation process.</summary>
 * <remarks>
 * <para header='Description'>A module for checking the progress or getting the result of the installation process.<br/>
 * It is returned as the return value of the CriWare.CriFsUtility::Install function.<br/>
 * <br/>
 * The value of isDone becomes true when the installation process completes.<br/>
 * If an error occurs during the installation process, the error information is stored in the error.<br/>
 * This function retries infinitely unless you explicitly call CriWare.CriFsInstallRequest::Stop .<br/>
 * When performing timeout handling, monitor the value of CriWare.CriFsInstallRequest::progress , and if the value does not change
 * even after sufficient time, stop the installation process using CriWare.CriFsInstallRequest::Stop etc.<br/></para>
 * </remarks>
 * <seealso cref='CriFsUtility::Install'/>
 */
public class CriFsInstallRequest : CriFsRequest
{
	/**
	 * <summary>Installation source file path.</summary>
	 * <remarks>
	 * <para header='Description'>The file path of the installation source.<br/>
	 * The path specified when calling the CriWare.CriFsUtility::Install function is stored.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsUtility::LoadAssetBundle'/>
	 */
	public string sourcePath { get; protected set; }

	/**
	 * <summary>Installation destination file path.</summary>
	 * <remarks>
	 * <para header='Description'>The file path of the installation destination.<br/>
	 * The path specified when calling the CriWare.CriFsUtility::Install function is stored.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsUtility::LoadAssetBundle'/>
	 */
	public string destinationPath { get; protected set; }

	/**
	 * <summary>The progress of the installation process.</summary>
	 * <returns>Progress status</returns>
	 * <remarks>
	 * <para header='Description'>Gets the progress of the process.<br/>
	 * Progress is a 32 bit floating point number in the range of 0.0 to 1.0.<br/>
	 * <br/>
	 * When the process is stopped using the CriWare.CriFsInstallRequest::Stop function, the progress at the time of the stop is saved.<br/>
	 * If an error occurs during the installation process, the value is negative.<br/>
	 * The value is updated when the buffer of size CriWareInitializer.fileSystemConfig.installBufferSize
	 * is filled.<br/>
	 * If the size of the installation buffer is too large, the value is updated less frequently,
	 * even though the installation is advancing.<br/>
	 * Be careful about the size of the installation buffer when performing timeout processing etc.</para>
	 * </remarks>
	 */
	public float progress { get; protected set; }
}


public class CriFsInstallRequestLegacy : CriFsInstallRequest
{
	/**
	 * <summary>Stops the installation process.</summary>
	 * <remarks>
	 * <para header='Description'>Stops the processing.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * It may take up to 20 seconds until the installation process stops after calling this function.<br/>
	 * When the installation process stops, the CriWare.CriFsInstallRequest::WaitForDone function running as a coroutine returns.<br/></para>
	 * </remarks>
	 */
	public override void Stop()
	{
		if (this.installer != null) {
			this.installer.Stop();
		}
	}

	#region Internal Methods

	public CriFsInstallRequestLegacy(CriFsBinder srcBinder, string srcPath, string dstPath, CriFsRequest.DoneDelegate doneDelegate, int installBufferSize)
	{
		this.sourcePath = srcPath;
		this.destinationPath = dstPath;
		this.doneDelegate = doneDelegate;
		this.progress = 0.0f;
		this.installer = new CriFsInstaller();
		this.installer.Copy(srcBinder, srcPath, dstPath, installBufferSize);
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Fs);
	}

	override public void Update()
	{
		/* インストーラの有無をチェック */
		if (this.installer == null) {
			return;
		}

		/* 進捗状況の更新 */
		this.progress = this.installer.GetProgress();

		/* ステータスのチェック */
		CriFsInstaller.Status status = this.installer.GetStatus();
		if (status == CriFsInstaller.Status.Busy) {
			return;
		}

		/* エラーチェック */
		if (status == CriFsInstaller.Status.Error) {
			this.progress = -1.0f;
			this.error = "Error occurred.";
		}

		/* インストーラの破棄 */
		this.installer.Dispose();
		this.installer = null;

		/* 処理の完了を通知 */
		this.Done();
	}

	#endregion

	#region Internal Fields

	private CriFsInstaller installer;

	override protected void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		if (this.installer != null) {
			this.installer.Dispose();
			this.installer = null;
		}
	}

	#endregion
}


/**
 * <summary>A module for checking the progress or getting the result of the installation process.</summary>
 * <remarks>
 * <para header='Description'>A module for checking the progress or getting the result of the installation process.<br/>
 * It is returned as the return value of the CriWare.CriFsUtility::WebInstall function.<br/>
 * <br/>
 * The value of isDone becomes true when the installation process completes.<br/>
 * If an error occurs during the installation process, the error information is stored in error.<br/></para>
 * </remarks>
 * <seealso cref='CriFsUtility::WebInstall'/>
 */
public class CriFsWebInstallRequest : CriFsInstallRequest
{
	/**
	 * <summary>Stops the installation process.</summary>
	 * <remarks>
	 * <para header='Description'>Stops the processing.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * It may take up to 20 seconds until the installation process stops after calling this function.<br/>
	 * When the installation process stops, the CriWare.CriFsWebInstallRequest::WaitForDone
	 * function running as a coroutine returns.<br/></para>
	 * </remarks>
	 */
	public override void Stop()
	{
		if (this.installer != null) {
			this.installer.Stop();
		}
	}

	public bool GetCRC32(out uint ret_val){
		ret_val = crc32;
		return crc32_set;
	}

	#region Internal Methods

	public CriFsWebInstallRequest(string srcPath, string dstPath, CriFsRequest.DoneDelegate doneDelegate)
	{
		this.sourcePath = srcPath;
		this.destinationPath = dstPath;
		this.doneDelegate = doneDelegate;
		this.progress = 0.0f;
		this.installer = new CriFsWebInstaller();
		System.IO.File.Delete(dstPath);
		this.installer.Copy(srcPath, dstPath);
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Fs);
	}

	override public void Update()
	{
		/* インストーラの有無をチェック */
		if (this.installer == null) {
			return;
		}

		var statusInfo = this.installer.GetStatusInfo();

		/* 進捗状況の更新 */
		this.progress = (float)statusInfo.receivedSize / ((statusInfo.contentsSize > 0) ? statusInfo.contentsSize : 1);

		/* ステータスのチェック */
		if (statusInfo.status != CriFsWebInstaller.Status.Busy) {
			/* エラーチェック */
			if (statusInfo.status == CriFsWebInstaller.Status.Error) {
				this.progress = -1.0f;
				this.error = "[CriFsWebInstallerError]" + statusInfo.error.ToString();
			}

			/* CRCを取得する */
			if (CriFsWebInstaller.isCrcEnabled && statusInfo.status == CriFsWebInstaller.Status.Complete) {
				this.crc32_set = this.installer.GetCRC32(out this.crc32);
			}

			/* インストーラの破棄 */
			this.installer.Dispose();
			this.installer = null;

			/* 処理の完了を通知 */
			this.Done();
		}
	}

	#endregion

	#region Internal Fields

	private CriFsWebInstaller installer;
	private uint crc32 = 0;
	private bool crc32_set = false;

	override protected void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		if (this.installer != null) {
			this.installer.Dispose();
			this.installer = null;
		}
	}

	#endregion
}

/**
 * <summary>A module for checking the progress or getting the result of the binding process.</summary>
 * <remarks>
 * <para header='Description'>This module is used to check the progress and obtain the result of the binding process, <br/>
 * which is returned by the CriFsUtility::BindCpk function. <br/>
 * <br/>
 * Once the binding process is completed, the value of isDone becomes true.<br/>
 * If an error occurs during the binding process, the error information will be stored in the "error" argument.<br/></para>
 * </remarks>
 * <seealso cref='CriFsUtility::BindCpk'/>
 */
public class CriFsBindRequest : CriFsRequest
{
	public enum BindType
	{
		Cpk,
		Directory,
		File
	}

	/**
	 * <summary>The path of the file to be bound.</summary>
	 * <remarks>
	 * <para header='Description'>The path of the file to bind.<br/>
	 * The path specified when calling the CriWare.CriFsUtility::BindCpk function etc. is stored.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsUtility::BindCp'/>
	 * <seealso cref='CriFsUtility::BindFile'/>
	 * <seealso cref='CriFsUtility::BindDirectory'/>
	 */
	public string path { get; private set; }

	/**
	 * <summary>The ID which identifies the binding process.</summary>
	 * <remarks>
	 * <para header='Description'>The ID which identifies the binding process.<br/>
	 * It is used to release only a specific binding after multi-binding.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsBinder::Unbind'/>
	 */
	public uint bindId { get; private set; }

	public CriFsBindRequest(BindType type, CriFsBinder targetBinder, CriFsBinder srcBinder, string path)
	{
		/* パスの保存 */
		this.path = path;

		/* バインド種別のチェック */
		switch (type) {
			case BindType.Cpk:
			this.bindId = targetBinder.BindCpk(srcBinder, path);
			break;

			case BindType.Directory:
			this.bindId = targetBinder.BindDirectory(srcBinder, path);
			break;

			case BindType.File:
			this.bindId = targetBinder.BindFile(srcBinder, path);
			break;

			default:
			throw new Exception("Invalid bind type.");
		}

		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Fs);
	}

	public override void Stop()
	{
	}

	override public void Update()
	{
		/* バインド処理中かどうかチェック */
		if (this.isDone) {
			/* バインド処理中以外は何もしない */
			return;
		}

		/* ステータスのチェック */
		CriFsBinder.Status status = CriFsBinder.GetStatus(this.bindId);
		if (status == CriFsBinder.Status.Analyze) {
			return;
		}

		/* エラーチェック */
		if (status == CriFsBinder.Status.Error) {
			this.error = "Error occurred.";
		}

		/* 完了の通知 */
		this.Done();
	}

	override protected void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);
	}
}

/**
 * <summary>A utility class for easily binding CPK files or loading files.</summary>
 * <remarks>
 * <para header='Description'>A utility class for easily binding CPK files or loading files.<br/></para>
 * </remarks>
 */
public static class CriFsUtility
{
	/**
	 * <summary>The default read unit size for each loading request.</summary>
	 * <remarks>
	 * <para header='Description'>The default for the last argument of each load request.<br/></para>
	 * </remarks>
	 * <seealso cref='CriWare.CriFsUtility::LoadFile'/>
	 * <seealso cref='CriWare.CriFsUtility::LoadAssetBundle'/>
	 * <seealso cref='CriWare.CriFsUtility::LoadAssetBundle'/>
	 */
	public const int DefaultReadUnitSize = (1024 * 1024);   // 1.0 Mb

	/**
	 * <summary>Starts loading the file.</summary>
	 * <param name='path'>File path</param>
	 * <param name='readUnitSize'>Read unit size of the CriFsLoader used internal</param>
	 * <returns>CriFsLoadFileRequest</returns>
	 * <remarks>
	 * <para header='Description'>Starts reading the specified file.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsLoadFileRequest::isDone to check the completion of the loading.<br/>
	 * If an error occurs during the loading process, the error information is stored in CriWare.CriFsLoadFileRequest::error .<br/></para>
	 * <para header='Note'>When loading the data in the CPK file,
	 * it is necessary to use CriWare.CriFsUtility::LoadFile instead of this function
	 * to specify the binder from which to load the data.<br/>
	 * <br/>
	 * You can also load the file from network by specifying a URL in the path.<br/><code>
	 * // crimot.comからFMPRO_Intro_e.txtをダウンロード
	 * CriFsLoadFileRequest request = CriFsUtility.LoadFile(
	 *  "http://crimot.com/sdk/sampledata/crifilesystem/FMPRO_Intro_e.txt");
	 * </code>
	 * </para>
	 * </remarks>
	 * <example>The code to load a file using the CriWare.CriFsUtility::LoadFile function is as follows.<br/><code>
	 * IEnumerator UserLoadFile(string path)
	 * {
	 *  // ファイルの読み込みを開始
	 *  CriFsLoadFileRequest request = CriFsUtility.LoadFile(path);
	 *
	 *  // 読み込み完了を待つ
	 *  yield return request.WaitForDone(this);
	 *
	 *  // エラーチェック
	 *  if (request.error != null) {
	 *      // エラー発生時の処理
	 *      …
	 *      yield break;
	 *  }
	 *
	 *  // 備考）ロードされたファイルの内容は request.bytes 内に格納されています。
	 * }
	 * </code>
	 * <br/>To load data under the StreamingAssets folder, 
	 * concatenate CriWare::Common::streamingAssetsPath with the file path.<code>
	 *  string path = Path.Combine(CriWare.Common.streamingAssetsPath, "sample_text.txt");
	 *  CriFsLoadFileRequest request = CriFsUtility.LoadFile(path);
	 * </code></example>
	 * <seealso cref='CriFsLoadFileRequest'/>
	 * <seealso cref='CriFsUtility::LoadFile(CriFsBinder, string)'/>
	 */
	public static CriFsLoadFileRequest LoadFile(string path, int readUnitSize = DefaultReadUnitSize)
	{
		return CriFsServer.instance.LoadFile(null, path, null, readUnitSize);
	}
	public static CriFsLoadFileRequest LoadFile(string path, CriFsRequest.DoneDelegate doneDelegate, int readUnitSize = DefaultReadUnitSize)
	{
		return CriFsServer.instance.LoadFile(null, path, doneDelegate, readUnitSize);
	}

	/**
	 * <summary>Starts loading the file.</summary>
	 * <param name='binder'>Binder</param>
	 * <param name='path'>File path</param>
	 * <param name='readUnitSize'>Read unit size of the CriFsLoader used internal</param>
	 * <returns>CriFsLoadFileRequest</returns>
	 * <remarks>
	 * <para header='Description'>Begins loading the bound file.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsLoadFileRequest::isDone to check the completion of the loading.<br/>
	 * If an error occurs during the loading process, the error information is stored in CriWare.CriFsLoadFileRequest::error .<br/></para>
	 * <para header='Note'>Same as the CriWare.CriFsUtility::LoadFile function, except that the binder is specified as the first argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsLoadFileRequest'/>
	 * <seealso cref='CriFsUtility::LoadFile(string)'/>
	 */
	public static CriFsLoadFileRequest LoadFile(CriFsBinder binder, string path, int readUnitSize = DefaultReadUnitSize)
	{
		return CriFsServer.instance.LoadFile(binder, path, null, readUnitSize);
	}

	/**
	 * <summary>Starts loading the Asset Bundle file.</summary>
	 * <param name='path'>File path</param>
	 * <param name='readUnitSize'>Read unit size of the CriFsLoader used internal</param>
	 * <returns>CriFsLoadAssetBundleRequest</returns>
	 * <remarks>
	 * <para header='Description'>Starts loading the specified Asset Bundle file.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsLoadAssetBundleRequest::isDone to check the completion of the loading.<br/>
	 * If an error occurs during the loading process, the error information is stored in CriWare.CriFsLoadFileRequest::error .<br/></para>
	 * <para header='Note'>When loading the asset data in the CPK file,
	 * it is necessary to use CriWare.CriFsUtility::LoadAssetBundle instead of this function
	 * to specify the binder from which to load the data.<br/>
	 * <br/>
	 * You can also load the file from network by specifying a URL in the path.<br/><code>
	 * // crimot.comからCharaMomo.unity3dをダウンロード
	 * CriFsLoadFileRequest request = CriFsUtility.LoadAssetBundle(
	 *  "http://crimot.com/sdk/sampledata/crifilesystem/CharaMomo.unity3d");
	 * </code>
	 * </para>
	 * </remarks>
	 * <example>The code to load an asset bundle using the CriWare.CriFsUtility::LoadAssetBundle function is as follows.<br/><code>
	 * IEnumerator UserLoadAssetBundle(string path)
	 * {
	 *  // アセットバンドルの読み込みを開始
	 *  CriFsLoadAssetBundleRequest request = CriFsUtility.LoadAssetBundle(path);
	 *
	 *  // 読み込み完了を待つ
	 *  yield return request.WaitForDone(this);
	 *
	 *  // エラーチェック
	 *  if (request.error != null) {
	 *      // エラー発生時の処理
	 *      …
	 *      yield break;
	 *  }
	 *
	 *  // インスタンスの作成
	 *  var obj = GameObject.Instantiate(request.assetBundle.mainAsset,
	 *          new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
	 *      ：
	 * }
	 * </code>
	 * <br/>To load data under the StreamingAssets folder, 
	 * concatenate CriWare::Common::streamingAssetsPath with the file path.<code>
	 *  string path = Path.Combine(CriWare.Common.streamingAssetsPath, "sample.assetbundle");
	 *  CriFsLoadAssetBundleRequest request = CriFsUtility.LoadAssetBundle(path);
	 * </code></example>
	 * <seealso cref='CriFsLoadAssetBundleRequest'/>
	 * <seealso cref='CriFsUtility::LoadAssetBundle(CriFsBinder, string)'/>
	 */
	public static CriFsLoadAssetBundleRequest LoadAssetBundle(string path, int readUnitSize = DefaultReadUnitSize)
	{
		return CriFsUtility.LoadAssetBundle(null, path, readUnitSize);
	}

	/**
	 * <summary>Starts loading the Asset Bundle file.</summary>
	 * <param name='binder'>Binder</param>
	 * <param name='path'>File path</param>
	 * <param name='readUnitSize'>Read unit size of the CriFsLoader used internal</param>
	 * <returns>CriFsLoadAssetBundleRequest</returns>
	 * <remarks>
	 * <para header='Description'>Begins loading the bound Asset Bundle file.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsLoadAssetBundleRequest::isDone to check the completion of the loading.<br/>
	 * If an error occurs during the loading process, the error information is stored in CriWare.CriFsLoadFileRequest::error .<br/></para>
	 * <para header='Note'>Same as the CriWare.CriFsUtility::LoadAssetBundle function, except that the binder is specified as the first argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsLoadAssetBundleRequest'/>
	 * <seealso cref='CriFsUtility::LoadAssetBundle(string)'/>
	 */
	public static CriFsLoadAssetBundleRequest LoadAssetBundle(CriFsBinder binder, string path, int readUnitSize = DefaultReadUnitSize)
	{
		return CriFsServer.instance.LoadAssetBundle(binder, path, readUnitSize);
	}

	/**
	 * <summary>Starts installing files.</summary>
	 * <param name='srcPath'>Installation source file path</param>
	 * <param name='dstPath'>Installation destination file path</param>
	 * <returns>CriFsInstallRequest</returns>
	 * <remarks>
	 * <para header='Description'>Starts the installation of the specified file.<br/>
	 * Specify the installation source file path in srcPath and the installation destination file path in dstPath.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should use CriWare.CriFsInstallRequest::isDone to check the completion of the installation.<br/>
	 * If an error occurs during the installation process, the error information is stored in CriWare.CriFsInstallRequest::error .<br/>
	 * <br/>
	 * When specifying data under the StreamingAssets folder as the installation source,
	 * prepend CriWare::Common::streamingAssetsPath to the file path.</para>
	 * <para header='Note'>You can also load the file from network by specifying a URL in the path.<br/></para>
	 * <para header='Note'>Create a folder in advance and call this API since an error occurs if the installation destination does not exist.<br/></para>
	 * </remarks>
	 * <example>The code to install files using the CriWare.CriFsUtility::Install function is as follows.<br/><code>
	 * private IEnumerator UserInstallFile(string url, string path)
	 * {
	 *  // インストールの開始
	 *  CriFsInstallRequest request = CriFsUtility.Install(url, path);
	 *
	 *  // インストール完了待ち
	 *  yield return request.WaitForDone(this);
	 *
	 *  // エラーチェック
	 *  if (request.error != null) {
	 *      // エラー発生時の処理
	 *      …
	 *      yield break;
	 *  }
	 *      ：
	 * }
	 * </code>
	 * </example>
	 * <seealso cref='CriFsInstallRequest'/>
	 */
	public static CriFsInstallRequest Install(string srcPath, string dstPath)
	{
		return CriFsUtility.Install(null, srcPath, dstPath, null);
	}
	public static CriFsInstallRequest Install(string srcPath, string dstPath, CriFsRequest.DoneDelegate doneDeleagate)
	{
		return CriFsUtility.Install(null, srcPath, dstPath, doneDeleagate);
	}

	/**
	 * <summary>Starts installing files.</summary>
	 * <param name='srcBinder'>Installation source binder</param>
	 * <param name='srcPath'>Installation source file path</param>
	 * <param name='dstPath'>Installation destination file path</param>
	 * <returns>CriFsInstallRequest</returns>
	 * <remarks>
	 * <para header='Description'>Start the installation of bound files.<br/>
	 * Specify the installation source file path in srcPath and the installation destination file path in dstPath.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsInstallRequest::isDone to check the completion of the installation.<br/>
	 * If an error occurs during the installation process, the error information is stored in CriWare.CriFsInstallRequest::error .<br/></para>
	 * <para header='Note'>Same as the CriWare.CriFsUtility::Install(string, string) function, except that the binder is specified as the first argument.<br/>
	 * (This function is used only when writing the contents in the CPK file as a file.)<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsInstallRequest'/>
	 * <seealso cref='CriFsUtility::Install(string, string)'/>
	 */
	public static CriFsInstallRequest Install(CriFsBinder srcBinder, string srcPath, string dstPath)
	{
		return CriFsServer.instance.Install(srcBinder, srcPath, dstPath, null);
	}
	public static CriFsInstallRequest Install(CriFsBinder srcBinder, string srcPath, string dstPath, CriFsRequest.DoneDelegate doneDeleagate)
	{
		return CriFsServer.instance.Install(srcBinder, srcPath, dstPath, doneDeleagate);
	}


	/**
	 * <summary>Starts installing files.</summary>
	 * <param name='srcPath'>Installation source file path (URL)</param>
	 * <param name='dstPath'>Installation destination file path</param>
	 * <param name='doneDeleagate'>Asynchronous process completion callback</param>
	 * <returns>CriFsWebInstallRequest</returns>
	 * <remarks>
	 * <para header='Description'>Starts installing files.<br/>
	 * Specify the installation source file path (URL) in srcPath and the installation destination file path in dstPath.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsWebInstallRequest::isDone to check the completion of the installation.<br/>
	 * If an error occurs during the installation process, the error information is stored in CriWare.CriFsWebInstallRequest::error .<br/></para>
	 * <para header='Note'>On iOS, this function works on iOS7 or later.</para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstallRequest'/>
	 */
	public static CriFsInstallRequest WebInstall(string srcPath, string dstPath, CriFsRequest.DoneDelegate doneDeleagate)
	{
		return CriFsServer.instance.WebInstall(srcPath, dstPath, doneDeleagate);
	}


	/**
	 * <summary>Start binding the CPK file.</summary>
	 * <param name='targetBinder'>Binder to be bound</param>
	 * <param name='srcPath'>CPK file path</param>
	 * <returns>CriFsBindRequest</returns>
	 * <remarks>
	 * <para header='Description'>Start binding the CPK file.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsBindRequest::isDone to check the completion of the binding.<br/>
	 * If an error occurs during the binding process, the error information is stored in CriWare.CriFsBindRequest::error .<br/>
	 * <br/>
	 * When binding the data under the StreamingAssets folder, prepend CriWare::Common::streamingAssetsPath
	 * to the file path.</para>
	 * <para header='Note'>If you want to use the multi-bind function or bind the CPK data in the CPK file,
	 * you must call CriWare.CriFsUtility::BindCpk(CriFsBinder, CriFsBinder, string)
	 * instead of this function.<br/>
	 * <br/>
	 * It is also possible to bind the CPK file on the network by specifying a URL in the path.<br/><code>
	 * // crimot.comのsample.cpkをバインド
	 * CriFsLoadFileRequest request = CriFsUtility.LoadFile(
	 *  "http://crimot.com/sdk/sampledata/crifilesystem/sample.cpk");
	 * </code>
	 * </para>
	 * <para header='Note'>CPK content information is acquired only at the time of binding.<br/>
	 * Therefore, when you bind a CPK file on the network,
	 * even if the file is updated on the server side, the update cannot be detected on the client side.<br/>
	 * (There may be an unintended access to the updated CPK file.)<br/></para>
	 * </remarks>
	 * <example>This sample code shows how to load the content in the CPK file using the CriWare.CriFsUtility::BindCpk and CriWare.CriFsUtility::LoadFile functions. <br/><code>
	 * private CriFsBinder binder = null;   // バインダ
	 * private uint bindId = 0;             // バインドID
	 *
	 * void OnEnable()
	 * {
	 *  // バインダの作成
	 *  this.binder = new CriFsBinder();
	 * }
	 *
	 * void OnDisable()
	 * {
	 *  // アンバインド処理の実行
	 *  if (this.bindId > 0) {
	 *      CriFsBinder.Unbind(this.bindId);
	 *      this.bindId = 0;
	 *  }
	 *
	 *  // バインダの破棄
	 *  this.binder.Dispose();
	 *  this.binder = null;
	 * }
	 *
	 * IEnumerator UserLoadFileFromCpk(string cpk_path, string content_path)
	 * {
	 *  // CPKファイルのバインドを開始
	 *  CriFsBindRequest bind_request = CriFsUtility.BindCpk(this.binder, cpk_path);
	 *
	 *  // バインドの完了を待つ
	 *  yield return bind_request.WaitForDone(this);
	 *
	 *  // エラーチェック
	 *  if (bind_request.error != null) {
	 *      // エラー発生時の処理
	 *      …
	 *      yield break;
	 *  }
	 *
	 *  // CPK内のコンテンツの読み込みを開始
	 *  CriFsLoadFileRequest load_request = CriFsUtility.LoadFile(this.binder, content_path);
	 *
	 *  // 読み込み完了を待つ
	 *  yield return load_request.WaitForDone(this);
	 *
	 *  // エラーチェック
	 *  if (load_request.error != null) {
	 *      // エラー発生時の処理
	 *      …
	 *      yield break;
	 *  }
	 *
	 *  // 備考）ロードされたファイルの内容は request.bytes 内に格納されています。
	 * }
	 * </code>
	 * </example>
	 * <seealso cref='CriFsBindRequest'/>
	 * <seealso cref='CriFsUtility::BindCpk(CriFsBinder, CriFsBinder, string)'/>
	 */
	public static CriFsBindRequest BindCpk(CriFsBinder targetBinder, string srcPath)
	{
		return CriFsUtility.BindCpk(targetBinder, null, srcPath);
	}

	/**
	 * <summary>Start binding the CPK file.</summary>
	 * <param name='targetBinder'>Binder to be bound</param>
	 * <param name='srcBinder'>A binder to access the CPK file to bind</param>
	 * <param name='srcPath'>CPK file path</param>
	 * <returns>CriFsBindRequest</returns>
	 * <remarks>
	 * <para header='Description'>Start binding the CPK file.<br/>
	 * In addition to CriWare.CriFsUtility::BindCpk(CriFsBinder, string) ,
	 * you can specify the binder to access to the sub CPK in the CPK file.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsBindRequest::isDone to check the completion of the binding.<br/>
	 * If an error occurs during the binding process, the error information is stored in CriWare.CriFsBindRequest::error .<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsBindRequest'/>
	 * <seealso cref='CriFsUtility::BindCpk(CriFsBinder, string)'/>
	 */
	public static CriFsBindRequest BindCpk(CriFsBinder targetBinder, CriFsBinder srcBinder, string srcPath)
	{
		return CriFsServer.instance.BindCpk(targetBinder, srcBinder, srcPath);
	}

	/**
	 * <summary>Starts binding the directory path.</summary>
	 * <param name='targetBinder'>Binder to be bound</param>
	 * <param name='srcPath'>Path name of the directory to be bound</param>
	 * <remarks>
	 * <para header='Description'>Binds the directory pathname.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsBindRequest::isDone to check the completion of the binding.<br/>
	 * If an error occurs during the binding process, the error information is stored in CriWare.CriFsBindRequest::error .<br/>
	 * <br/>
	 * When binding the directory under the StreamingAssets folder, prepend CriWare::Common::streamingAssetsPath
	 * to the file path.</para>
	 * <para header='Note'>The system does not check whether the directory exists at the time of binding.<br/>
	 * Only the directory path is retained in the binder;
	 * it does not open the files in the specified directory.<br/></para>
	 * <para header='Note'>This function is a debug function for development support.<br/>
	 * If you use this function, the following problems may occur.<br/>
	 * - Functions CriWare.CriFsLoader::Load or CriWare.CriFsBinder::GetFileSize may block for a long time.<br/>
	 * - When accessing files in the bound directory, stream playback of sound or movie is interrupted.<br/></para>
	 * <para header='Note'>Be careful not to use this function in the application when final release.<br/>
	 * (Convert the data in the directory to a CPK file and bind it using the CriWare.CriFsUtility::BindCpk function,
	 * or bind all the files in the directory using the CriWare.CriFsUtility::BindFile function.)<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsBindRequest'/>
	 * <seealso cref='CriFsUtility::BindCpk'/>
	 * <seealso cref='CriFsUtility::BindFile'/>
	 */
	public static CriFsBindRequest BindDirectory(CriFsBinder targetBinder, string srcPath)
	{
		return CriFsServer.instance.BindDirectory(targetBinder, null, srcPath);
	}

	/**
	 * <summary>Starts binding the directory path.</summary>
	 * <param name='targetBinder'>Binder to be bound</param>
	 * <param name='srcBinder'>The binder to access the directory to be bound</param>
	 * <param name='srcPath'>CPK file path</param>
	 * <returns>CriFsBindRequest</returns>
	 * <remarks>
	 * <para header='Description'>Begins binding the directory path.<br/>
	 * In addition to CriWare.CriFsUtility::BindDirectory(CriFsBinder, string),
	 * you can specify the binder to access the directory in the CPK file.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsBindRequest::isDone to check the completion of the binding.<br/>
	 * If an error occurs during the binding process, the error information is stored in CriWare.CriFsBindRequest::error .<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsBindRequest'/>
	 * <seealso cref='CriFsUtility::BindDirectory(CriFsBinder, string)'/>
	 */
	public static CriFsBindRequest BindDirectory(CriFsBinder targetBinder, CriFsBinder srcBinder, string srcPath)
	{
		return CriFsServer.instance.BindDirectory(targetBinder, srcBinder, srcPath);
	}

	/**
	 * <summary>Binds the file.</summary>
	 * <param name='targetBinder'>Binder to be bound</param>
	 * <param name='srcPath'>The path name of the file to be bound</param>
	 * <returns>Bind ID</returns>
	 * <remarks>
	 * <para header='Description'>Starts binding the file.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsBindRequest::isDone to check the completion of the binding.<br/>
	 * If an error occurs during the binding process, the error information is stored in CriWare.CriFsBindRequest::error .<br/>
	 * <br/>
	 * When binding the files under the StreamingAssets folder, prepend CriWare::Common::streamingAssetsPath
	 * to the file path.</para>
	 * <para header='Note'>This function is used when you want to get only the file size without loading the file.<br/>
	 * It is possible to get the file size asynchronously by calling the
	 * CriWare.CriFsBinder::GetFileSize function after binding the file.<br/></para>
	 * </remarks>
	 * <example>This sample code shows how to get the file size using the CriWare.CriFsUtility::BindFile and CriWare.CriFsBinder::GetFileSize functions. <br/><code>
	 * IEnumerator UserGetFileSize(string path, out long fileSize)
	 * {
	 *  // ファイルのバインドを開始
	 *  CriFsBindRequest bind_request = CriFsUtility.BindFile(path);
	 *
	 *  // バインドの完了を待つ
	 *  yield return bind_request.WaitForDone(this);
	 *
	 *  // エラーチェック
	 *  if (bind_request.error != null) {
	 *      // エラー発生時の処理
	 *      …
	 *      yield break;
	 *  }
	 *
	 *  // ファイルサイズの取得
	 *  fileSize = bind_request.binder.GetFileSize();
	 * }
	 * </code>
	 * </example>
	 * <seealso cref='CriFsBindRequest'/>
	 * <seealso cref='CriFsBinder::GetFileSize'/>
	 */
	public static CriFsBindRequest BindFile(CriFsBinder targetBinder, string srcPath)
	{
		return CriFsServer.instance.BindFile(targetBinder, null, srcPath);
	}

	/**
	 * <summary>Starts binding files.</summary>
	 * <param name='targetBinder'>Binder to be bound</param>
	 * <param name='srcBinder'>The binder to access the file to be bound</param>
	 * <param name='srcPath'>File path</param>
	 * <returns>CriFsBindRequest</returns>
	 * <remarks>
	 * <para header='Description'>Starts binding files.<br/>
	 * In addition to CriWare.CriFsUtility::BindFile(CriFsBinder, string),
	 * you can specify the binder to access to the files in the CPK file.<br/>
	 * <br/>
	 * This function returns immediately.<br/>
	 * You should check CriWare.CriFsBindRequest::isDone to check the completion of the binding.<br/>
	 * If an error occurs during the binding process, the error information is stored in CriWare.CriFsBindRequest::error .<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsBindRequest'/>
	 * <seealso cref='CriFsUtility::BindFile(CriFsBinder, string)'/>
	 */
	public static CriFsBindRequest BindFile(CriFsBinder targetBinder, CriFsBinder srcBinder, string srcPath)
	{
		return CriFsServer.instance.BindFile(targetBinder, srcBinder, srcPath);
	}

	/**
	 * <summary>Specifies the User-Agent string used in HTTP requests.</summary>
	 * <param name='userAgentString'>User-Agent character</param>
	 * <remarks>
	 * <para header='Description'>Specifies the User-Agent string used in HTTP requests.<br/>
	 * If not specified, the version character of the lower file system module is specified.<br/>
	 * Specify the User-Agent string using up to 255 characters ((255 bytes).<br/></para>
	 * </remarks>
	 */
	public static void SetUserAgentString(string userAgentString)
	{
		/* User-Agent文字列を指定 */
		CRIWARE7BCABE2D(userAgentString);
	}

	/**
	 * <summary>Specifies the proxy server used for HTTP requests.</summary>
	 * <param name='proxyPath'>Proxy server address</param>
	 * <param name='proxyPort'>Proxy server port number</param>
	 * <remarks>
	 * <para header='Description'>Specifies the proxy server address to be used for HTTP request of HTTP I/O.<br/>
	 * Specify proxyPath using up to 256 characters.<br/></para>
	 * </remarks>
	 */
	public static void SetProxyServer(string proxyPath, UInt16 proxyPort)
	{
		/* プロキシ設定 */
		CRIWARE0E8F8E33(proxyPath, proxyPort);
	}

	/**
	 * <summary>Specifies the path separator</summary>
	 * <param name='filter'>A list of characters used as a separator</param>
	 * <remarks>
	 * <para header='Description'>Changes the character that the CRI File System library interprets as a separator.<br/>
	 * By default, the three characters ",", "\\t", and "\\n" are treated as a separator.<br/>
	 * For filter, specify a string that contains a list of characters to be used as a separator.<br/>
	 * For example, if you specify "@+-*", four types of characters "@", "+", "-", and "*" are
	 * treated as a separator.<br/>
	 * If you specify an empty string ("") for filter, the separator will be disabled.
	 * If you specify null, the setting returns to the default.<br/></para>
	 * <para header='Note'>If you pass the path including the separator specified in this function to the plug-in function,
	 * it is treated as an invalid path internally and the path after the separator is invalid.<br/>
	 * When handling the path including ",", "\\t" or "\\n" in the application, it is necessary to
	 * change it to a separator in advance using this function.<br/></para>
	 * </remarks>
	 */
	public static void SetPathSeparator(string filter)
	{
		CRIWARE2F1D5BBC(filter);
	}

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool CRIWARE7BCABE2D(string userAgentString);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool CRIWARE0E8F8E33(string proxyPath, UInt16 proxyPort);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool CRIWARE2F1D5BBC(string filter);
	#else
	private static bool CRIWARE7BCABE2D(string userAgentString) { return true; }
	private static bool CRIWARE0E8F8E33(string proxyPath, UInt16 proxyPort) { return true; }
	private static bool CRIWARE2F1D5BBC(string filter) {return true; }
	#endif
	#endregion
}

/**
 * @}
 */

/*==========================================================================
 *      CRI Fs Unity Integration
 *=========================================================================*/

/**
 * \addtogroup CRIFS_UNITY_INTEGRATION
 * @{
 */

/**
 * <summary>Global class in the CRIFs library.</summary>
 * <remarks>
 * <para header='Description'>Class that contains the initialization function of the CRI File System library and the definitions of the types used within the library. <br/></para>
 * </remarks>
 */
public static class CriFsPlugin
{
	/* 初期化カウンタ */
	private static int initializationCount = 0;

	private static bool isConfigured = false;

	public static int defaultInstallBufferSize   = (4 * 1024 * 1024); // 4.0 Mb
	public static int installBufferSize         = defaultInstallBufferSize;

	public static bool isInitialized { get { return initializationCount > 0; } }

	public static void SetConfigParameters(
		int num_loaders, int num_binders, int num_installers, int argInstallBufferSize, int max_path, bool minimize_file_descriptor_usage, bool enable_crc_check)
	{
		CriFsPlugin.CRIWARE57ED305B(
			num_loaders, num_binders, num_installers, max_path, minimize_file_descriptor_usage, enable_crc_check);
		installBufferSize = argInstallBufferSize;

		CriFsPlugin.isConfigured = true;
	}

	public static void SetReadDeviceEnabled(int deviceId, bool enabled)
	{
		if (deviceId == 0 && enabled == false) {
			Debug.LogError("[CRIWARE] Read Device 0 should never be disabled.");
			return;
		}

		criFs_SetReadDeviceEnabled(deviceId, enabled);
	}

	public static void SetConfigAdditionalParameters_ANDROID(
		int device_read_bps)
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		CriFsPlugin.criFsUnity_SetConfigAdditionalParameters_ANDROID(device_read_bps);
#endif
	}

	public static void SetMemoryFileSystemThreadPriorityExperimentalAndroid(int prio)
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		CriFsPlugin.criFsUnity_SetMemoryFileSystemThreadPriority_ANDROID(prio);
#endif
	}

	public static void SetDataDecompressionThreadPriorityExperimentalAndroid(int prio)
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		CriFsPlugin.criFsUnity_SetDataDecompressionThreadPriority_ANDROID(prio);
#endif
	}

	public static void InitializeLibrary()
	{
		/* 初期化カウンタの更新 */
		CriFsPlugin.initializationCount++;
		if (CriFsPlugin.initializationCount != 1) {
			return;
		}

		/* シーン実行前に初期化済みの場合は終了させる */
		if (CriFsPlugin.IsLibraryInitialized() == true) {
			CriFsPlugin.FinalizeLibrary();
			CriFsPlugin.initializationCount = 1;
		}

		/* 初期化パラメータが設定済みかどうかを確認 */
		if (CriFsPlugin.isConfigured == false) {
			Debug.Log("[CRIWARE] FileSystem initialization parameters are not configured. "
				+ "Initializes FileSystem by default parameters.");
		}

		/* ライブラリの初期化 */
		CriFsPlugin.CRIWARE51EC119C();
	}

	public static bool IsLibraryInitialized()
	{
		/* ライブラリが初期化済みかチェック */
		return CRIWARE2F4B4F68();
	}

	public static void FinalizeLibrary()
	{
		/* 初期化カウンタの更新 */
		CriFsPlugin.initializationCount--;
		if (CriFsPlugin.initializationCount < 0) {
			CriFsPlugin.initializationCount = 0;
			if (CriFsPlugin.IsLibraryInitialized() == false) {
				return;
			}
		}
		if (CriFsPlugin.initializationCount != 0) {
			return;
		}

		/* CriFsServerのインスタンスが存在すれば破棄 */
		CriFsServer.DestroyInstance();

		/* パラメータを初期値に戻す */
		installBufferSize = defaultInstallBufferSize;

		/* 未破棄のDisposableを破棄 */
		CriDisposableObjectManager.CallOnModuleFinalization(CriDisposableObjectManager.ModuleType.Fs);

		/* ライブラリの終了 */
		CriFsPlugin.CRIWARE74CAAF39();
	}

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE57ED305B(
		int num_loaders, int num_binders, int num_installers, int max_path, bool minimize_file_descriptor_usage, bool enable_crc_check);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE51EC119C();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern bool CRIWARE2F4B4F68();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE74CAAF39();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern uint CRIWARE4A9B2613();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern UInt32 criFsLoader_GetRetryCount();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern Int32 criFs_GetNumBinds(ref int cur, ref int max, ref int limit);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern Int32 criFs_GetNumUsedLoaders(ref int cur, ref int max, ref int limit);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern Int32 criFs_GetNumUsedInstallers(ref int cur, ref int max, ref int limit);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern Int32 criFs_SetReadDeviceEnabled(int device_id, bool enabled);

	#if !UNITY_EDITOR && UNITY_ANDROID
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criFsUnity_SetConfigAdditionalParameters_ANDROID(int device_read_bps);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void criFsUnity_SetMemoryFileSystemThreadPriority_ANDROID(int prio);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void criFsUnity_SetDataDecompressionThreadPriority_ANDROID(int prio);
	#endif
	#else
	private static bool _dummyInitialized = false;
	private static void CRIWARE57ED305B(
		int num_loaders, int num_binders, int num_installers, int max_path, bool minimize_file_descriptor_usage, bool enable_crc_check) { }
	private static void CRIWARE51EC119C() { _dummyInitialized = true; }
	public static bool CRIWARE2F4B4F68() { return _dummyInitialized; }
	private static void CRIWARE74CAAF39() { _dummyInitialized = false; }
	public static uint CRIWARE4A9B2613() { return 0u; }
	public static UInt32 criFsLoader_GetRetryCount() { return 0u; }
	public static Int32 criFs_GetNumBinds(ref int cur, ref int max, ref int limit) { return 0; }
	public static Int32 criFs_GetNumUsedLoaders(ref int cur, ref int max, ref int limit) { return 0; }
	public static Int32 criFs_GetNumUsedInstallers(ref int cur, ref int max, ref int limit) { return 0; }
	private static Int32 criFs_SetReadDeviceEnabled(int device_id, bool enabled) { return 0; }
	#if !UNITY_EDITOR && UNITY_ANDROID
	private static void criFsUnity_SetConfigAdditionalParameters_ANDROID(int device_read_bps) { }
	public static void criFsUnity_SetMemoryFileSystemThreadPriority_ANDROID(int prio) { }
	public static void criFsUnity_SetDataDecompressionThreadPriority_ANDROID(int prio) { }
	#endif
	#endif
	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
