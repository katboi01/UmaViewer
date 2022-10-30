/****************************************************************************
 *
 * Copyright (c) 2017 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>AtomEx sound object</summary>
 * <remarks>
 * <para header='Description'>A sound object class.<br/>
 * You can control sounds in a batch for the registered players by associating with
 * "object", "space", "situation" etc. in the application.<br/></para>
 * </remarks>
 */
public class CriAtomExSoundObject : CriDisposable
{
	public IntPtr nativeHandle {get {return this.handle;} }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct Config {
		public bool enableVoiceLimitScope;
		public bool enableCategoryCueLimitScope;
	}

	/**
	 * <summary>Creates a sound object</summary>
	 * <param name='enableVoiceLimitScope'>Whether to activate the Voice limit scope</param>
	 * <param name='enableCategoryCueLimitScope'>Whether to enable the Category Cue limit scope</param>
	 * <returns>Sound object</returns>
	 * <remarks>
	 * <para header='Description'>Creates a sound object.<br/>
	 * If enableVoiceLimitScope is set to True, the Voice count is controlled by
	 * the Voice limit group by counting the number of Voices for the sound played back
	 * from the Ex player associated with this sound object only inside this
	 * sound object.<br/>
	 * If enableCategoryCueLimit is set to True, the playback count is controlled
	 * by counting the number of Category playbacks for the Cue played back from the
	 * Ex player associated with this sound object only inside this sound object.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExSoundObject::Dispose'/>
	 */
	public CriAtomExSoundObject(bool enableVoiceLimitScope, bool enableCategoryCueLimitScope)
	{
		if (!CriAtomPlugin.IsLibraryInitialized()) {
			throw new Exception("CriAtomPlugin is not initialized.");
		}

		Config config;
		config.enableVoiceLimitScope = enableVoiceLimitScope;
		config. enableCategoryCueLimitScope = enableCategoryCueLimitScope;

		this.handle = criAtomExSoundObject_Create(ref config, IntPtr.Zero, 0);

		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	/**
	 * <summary>Discards a sound object</summary>
	 * <remarks>
	 * <para header='Description'>Discards a sound object.<br/>
	 * When this function is called, all the resources allocated in the DLL
	 * when creating the sound object are released.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExSoundObject::CriAtomExSoundObject'/>
	 */
	public override void Dispose()
	{
		CriDisposableObjectManager.Unregister(this);
		if (this.handle != IntPtr.Zero) {
			criAtomExSoundObject_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}
		GC.SuppressFinalize(this);
	}

	/**
	 * <summary>Adds an AtomExPlayer</summary>
	 * <param name='player'>AtomExPlayer</param>
	 * <remarks>
	 * <para header='Description'>Adds an AtomExPlayer to the sound object.<br/>
	 * The added AtomExPlayer is associated with the sound object and will be
	 * affected by the sound object as follows:<br/>
	 * - Limitation on the number of Voices and the range affected by the event function (scope)<br/>
	 * - Playback control (Stop, Pause etc.)<br/>
	 * - Parameter control<br/>
	 * If you want to delete the added AtomExPlayer from the sound object,
	 * call the CriAtomExSoundObject::DeletePlayer function.<br/></para>
	 * <para header='Note'>Call this function when the AtomExPlayer you are trying to add is not playing sound.<br/>
	 * If an AtomExPlayer that is playing sound is specified, playback is stopped when adding.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExSoundObject::DeletePlayer'/>
	 * <seealso cref='CriAtomExSoundObject::DeleteAllPlayers'/>
	 * <seealso cref='CriAtomExPlayer::nativeHandle'/>
	 */
	public void AddPlayer(CriAtomExPlayer player)
	{
		criAtomExSoundObject_AddPlayer(this.handle, player.nativeHandle);
	}

	/**
	 * <summary>Removes an AtomExPlayer</summary>
	 * <param name='player'>Native handle of the AtomExPlayer</param>
	 * <remarks>
	 * <para header='Description'>Removes the AtomExPlayer from the sound object.<br/>
	 * The removed AtomExPlayer is no longer associated with the sound object
	 * and is no longer affected by the sound object.<br/></para>
	 * <para header='Note'>Call this function when the AtomExPlayer you are trying to delete is not playing sound.<br/>
	 * If an AtomExPlayer that is playing sound is specified, playback is stopped when deleting.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExSoundObject::AddPlayer'/>
	 * <seealso cref='CriAtomExSoundObject::DeleteAllPlayers'/>
	 * <seealso cref='CriAtomExPlayer::nativeHandle'/>
	 */
	public void DeletePlayer(CriAtomExPlayer player)
	{
		criAtomExSoundObject_DeletePlayer(this.handle, player.nativeHandle);
	}

	/**
	 * <summary>Removes all AtomExPlayers</summary>
	 * <remarks>
	 * <para header='Description'>Removes all the AtomExPlayers associated with the sound object.<br/>
	 * The removed AtomExPlayer is no longer associated with the sound object and is no longer
	 * affected by the sound object.<br/></para>
	 * <para header='Note'>Call this function when the AtomExPlayer you are trying to delete is not playing sound.<br/>
	 * If an AtomExPlayer that is playing sound is specified, playback is stopped when deleting.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExSoundObject::AddPlayer'/>
	 * <seealso cref='CriAtomExSoundObject::DeletePlayer'/>
	 */
	public void DeleteAllPlayers()
	{
		criAtomExSoundObject_DeleteAllPlayers(this.handle);
	}

	#region Internal Members

	~CriAtomExSoundObject()
	{
		this.Dispose();
	}

	private IntPtr handle = IntPtr.Zero;

	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomExSoundObject_Create(ref Config config, IntPtr work, int work_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExSoundObject_Destroy(IntPtr soundObject);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExSoundObject_AddPlayer(IntPtr soundObject, IntPtr player);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExSoundObject_DeletePlayer(IntPtr soundObject, IntPtr player);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExSoundObject_DeleteAllPlayers(IntPtr soundObject);
	#else
	private static IntPtr criAtomExSoundObject_Create(ref Config config, IntPtr work, int work_size) { return IntPtr.Zero; }
	private static void criAtomExSoundObject_Destroy(IntPtr soundObject) { }
	private static void criAtomExSoundObject_AddPlayer(IntPtr soundObject, IntPtr player) { }
	private static void criAtomExSoundObject_DeletePlayer(IntPtr soundObject, IntPtr player) { }
	private static void criAtomExSoundObject_DeleteAllPlayers(IntPtr soundObject) { }
	#endif
	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
