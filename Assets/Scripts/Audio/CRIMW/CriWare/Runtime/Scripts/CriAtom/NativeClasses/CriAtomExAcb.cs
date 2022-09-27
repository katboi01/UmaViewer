/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>ACB and AWB data</summary>
 * <remarks>
 * <para header='Description'>A class that manages the Cue information.<br/>
 * Loads or unloads ACB or AWB, or gets the Cue information etc.<br/></para>
 * </remarks>
 */
public class CriAtomExAcb : CriDisposable
{
	public IntPtr nativeHandle {get {return this.handle;} }

	public bool isAvailable {get {return this.handle != IntPtr.Zero;} }

	/**
	 * <summary>Loads the ACB file</summary>
	 * <param name='binder'>Binder object</param>
	 * <param name='acbPath'>Path of the ACB file</param>
	 * <param name='awbPath'>Path of the AWB file</param>
	 * <returns>CriAtomExAcb object</returns>
	 * <remarks>
	 * <para header='Description'>Loads the ACB file and import the information required for Cue playback.<br/>
	 * <br/>
	 * Specify the path of the ACB file for on-memory playback in the second argument acbPath,
	 * and the path of the AWB file for stream playback in the third argument awbPath.<br/>
	 * (When loading ACB data for on-memory playback only,
	 * the value set in awbPath is ignored.)<br/>
	 * <br/>
	 * When the ACB file and the AWB file are packed in one CPK file,
	 * you need to specify the CriFsBinder object that binds the CPK file as the first argument ( binder ).<br/>
	 * <br/>
	 * Loading the ACB file returns an CriAtomExAcb object ( CriWare.CriAtomExAcb )
	 * for accessing the ACB data.<br/>
	 * You can play the Cue in the ACB file by
	 * specifying the ACB handle and the Cue name to be played
	 * using the CriWare.CriAtomExPlayer::SetCue function for the AtomExPlayer.<br/>
	 * <br/>
	 * This function will return null as a return value
	 * if the ACB file cannot be loaded due to a read error.<br/></para>
	 * <para header='Note'>The library must be initialized before calling this function.<br/>
	 * <br/>
	 * The ACB handle internally allocates a binder ( CriFsBinder ).<br/>
	 * When loading an ACB file,
	 * it is necessary to initialize the library using the settings which allow binder for the number of ACB files can be allocated.<br/>
	 * <br/>
	 * This function is a return-on-complete function.<br/>
	 * The time it takes to load an ACB file varies depending on the platform.<br/>
	 * If this function is called at a timing when the screen needs to be updated such as in a game loop,
	 * the process is blocked in the unit of milliseconds, resulting in dropped frames.<br/>
	 * Load the ACB file at a timing when load fluctuations is accepted
	 * such as when switching scenes.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetCue'/>
	 */
	public static CriAtomExAcb LoadAcbFile(CriFsBinder binder, string acbPath, string awbPath)
	{
		/* バージョン番号が不正なライブラリではキューシートをロードしない */
		/* 備考）不正に差し替えられたsoファイルを使用している可能性あり。 */
		bool isCorrectVersion = CriWare.Common.CheckBinaryVersionCompatibility();
		if (isCorrectVersion == false) {
			return null;
		}

		IntPtr binderHandle = (binder != null) ? binder.nativeHandle : IntPtr.Zero;
		IntPtr handle = criAtomExAcb_LoadAcbFile(
			binderHandle, acbPath, binderHandle, awbPath, IntPtr.Zero, 0);
		if (handle == IntPtr.Zero) {
			return null;
		}
		return new CriAtomExAcb(handle, null);
	}

	/**
	 * <summary>Loads the ACB data</summary>
	 * <param name='acbData'>Byte array of the ACB data</param>
	 * <param name='awbBinder'>Binder object for AWB</param>
	 * <param name='awbPath'>Path of the AWB file</param>
	 * <returns>CriAtomExAcb object</returns>
	 * <remarks>
	 * <para header='Description'>Loads the ACB data placed on the memory and captures the information required for Cue playback.<br/>
	 * <br/>
	 * Specify the path of the AWB file for stream playback in the second argument awbPath.<br/>
	 * (When loading ACB data for on-memory playback only,
	 * the value set in awbPath is ignored.)<br/>
	 * <br/>
	 * When the AWB file is packed in a CPK file,
	 * you need to specify the CriFsBinder object that binds the CPK file as the second argument ( binder ).<br/>
	 * <br/>
	 * Loading the ACB data returns an CriAtomExAcb object ( CriWare.CriAtomExAcb )
	 * for accessing the ACB data.<br/>
	 * You can play the Cue in the ACB file by specifying the ACB handle
	 * and the Cue name to be played using the
	 * CriWare.CriAtomExPlayer::SetCue function for the AtomExPlayer.<br/>
	 * <br/>
	 * This function will return null as a return value
	 * if the ACB file cannot be loaded due to a read error.<br/></para>
	 * <para header='Note'>The library must be initialized before calling this function.<br/>
	 * <br/>
	 * The ACB handle internally allocates a binder ( CriFsBinder ).<br/>
	 * When loading an ACB file,
	 * it is necessary to initialize the library using the settings which allow binder for the number of ACB files can be allocated.<br/>
	 * <br/>
	 * This function is a return-on-complete function.<br/>
	 * The time it takes to load an ACB file varies depending on the platform.<br/>
	 * If this function is called at a timing when the screen needs to be updated such as in a game loop,
	 * the process is blocked in the unit of milliseconds, resulting in dropped frames.<br/>
	 * Load the ACB file at a timing when load fluctuations is accepted
	 * such as when switching scenes.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetCue'/>
	 */
	public static CriAtomExAcb LoadAcbData(byte[] acbData, CriFsBinder awbBinder, string awbPath)
	{
		/* バージョン番号が不正なライブラリではキューシートをロードしない */
		/* 備考）不正に差し替えられたsoファイルを使用している可能性あり。 */
		bool isCorrectVersion = CriWare.Common.CheckBinaryVersionCompatibility();
		if (isCorrectVersion == false) {
			return null;
		}

		IntPtr binderHandle = (awbBinder != null) ? awbBinder.nativeHandle : IntPtr.Zero;
		GCHandle gch = GCHandle.Alloc(acbData, GCHandleType.Pinned);
		IntPtr handle = criAtomExAcb_LoadAcbData(
			gch.AddrOfPinnedObject(), acbData.Length, binderHandle, awbPath, IntPtr.Zero, 0);
		if (handle == IntPtr.Zero) {
			return null;
		}
		return new CriAtomExAcb(handle, gch);
	}

	/**
	 * <summary>Unloads the ACB file</summary>
	 * <remarks>
	 * <para header='Description'>Unloads the loaded ACB file.<br/></para>
	 * <para header='Note'>Calling this function will
	 * stop all Cues that reference the ACB file to be unloaded.<br/>
	 * (After this function is called, the work area used to create the
	 * ACB handle or the area where the ACB data was placed is not referenced.)<br/></para>
	 * <para header='Note'>When this function is called, the library performs a search to check
	 * if there are any Atom players referencing the ACB data to be discarded.<br/>
	 * Therefore, if you create/discard an Atom player in the different thread while this function is running,
	 * serious problems such as access violation or deadlock may be triggered.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::LoadAcbData'/>
	 * <seealso cref='CriAtomExAcb::LoadAcbFile'/>
	 */
	public override void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		if (this.isAvailable) {
			criAtomExAcb_Release(this.handle);
			this.handle = IntPtr.Zero;
		}

		if (disposing && this.dataHandle.IsAllocated) {
			this.dataHandle.Free();
		}
	}

	/**
	 * <summary>Checks the existence of the Cue (Cue name specified)</summary>
	 * <param name='cueName'>Cue name</param>
	 * <returns>Whether the Cue exists (exists: True, does not exist: False)</returns>
	 * <remarks>
	 * <para header='Description'>Gets whether the Cue with the specified name exists.<br/>
	 * Returns True if it exists.<br/></para>
	 * </remarks>
	 */
	public bool Exists(string cueName)
	{
		return criAtomExAcb_ExistsName(this.handle, cueName);
	}

	/**
	 * <summary>Checking the existence of the Cue (Cue ID specified)</summary>
	 * <param name='cueId'>Cue ID</param>
	 * <returns>Whether the Cue exists (exists: True, does not exist: False)</returns>
	 * <remarks>
	 * <para header='Description'>Gets whether the Cue with the specified ID exists.<br/>
	 * Returns True if it exists.<br/></para>
	 * </remarks>
	 */
	public bool Exists(int cueId)
	{
		return criAtomExAcb_ExistsId(this.handle, cueId);
	}

	/**
	 * <summary>Gets Cue information (Cue name specified)</summary>
	 * <param name='cueName'>Cue name</param>
	 * <param name='info'>Cue information</param>
	 * <returns>Whether the acquisition was successful (success: True, failure: False)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the Cue information by specifying the Cue name.<br/>
	 * Returns False if there is no Cue with the specified name.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetCueInfoByIndex'/>
	 */
	public bool GetCueInfo(string cueName, out CriAtomEx.CueInfo info)
	{
		using (var mem = new CriStructMemory<CriAtomEx.CueInfo>()) {
			bool result = criAtomExAcb_GetCueInfoByName(this.handle, cueName, mem.ptr);
			info = new CriAtomEx.CueInfo(mem.bytes, 0);
			return result;
		}
	}

	/**
	 * <summary>Gets Cue information (Cue ID specified)</summary>
	 * <param name='cueId'>Cue ID</param>
	 * <param name='info'>Cue information</param>
	 * <returns>Whether the acquisition was successful (success: True, failure: False)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the Cue information by specifying the Cue ID.<br/>
	 * Returns False if there is no Cue with the specified ID.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetCueInfoByIndex'/>
	 */
	public bool GetCueInfo(int cueId, out CriAtomEx.CueInfo info)
	{
		using (var mem = new CriStructMemory<CriAtomEx.CueInfo>()) {
			bool result = criAtomExAcb_GetCueInfoById(this.handle, cueId, mem.ptr);
			info = new CriAtomEx.CueInfo(mem.bytes, 0);
			return result;
		}
	}

	/**
	 * <summary>Gets Cue information (Cue index specified)</summary>
	 * <param name='index'>Cue index</param>
	 * <param name='info'>Cue information</param>
	 * <returns>Whether the acquisition was successful (success: True, failure: False)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the Cue information by specifying the Cue index.<br/>
	 * Returns False if there is no Cue with the specified index.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetCueInfo'/>
	 */
	public bool GetCueInfoByIndex(int index, out CriAtomEx.CueInfo info)
	{
		using (var mem = new CriStructMemory<CriAtomEx.CueInfo>()) {
			bool result = criAtomExAcb_GetCueInfoByIndex(this.handle, index, mem.ptr);
			info = new CriAtomEx.CueInfo(mem.bytes, 0);
			return result;
		}
	}


	/**
	 * <summary>Gets all Cue information</summary>
	 * <returns>Cues information array</returns>
	 * <remarks>
	 * <para header='Description'>Gets all Cue information contained in the Acb file.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetCueInfo'/>
	 */
	public CriAtomEx.CueInfo[] GetCueInfoList()
	{
		int numCues = criAtomExAcb_GetNumCues(this.handle);
		var infoList = new CriAtomEx.CueInfo[numCues];
		for (int i = 0; i < numCues; i++) {
			this.GetCueInfoByIndex(i, out infoList[i]);
		}
		return infoList;
	}

	/**
	 * <summary>Gets sound waveform information (Cue name specified)</summary>
	 * <param name='cueName'>Cue name</param>
	 * <param name='info'>Sound waveform information</param>
	 * <returns>Whether the acquisition was successful (success: True, failure: False)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the information on the sound waveform played by a Cue by specifying the Cue name.<br/>
	 * If there are multiple sound waveforms played by that Cue,
	 * the information of the sound waveform played first in the first Track is obtained.<br/>
	 * Returns false if there is no Cue with the specified name.<br/></para>
	 * </remarks>
	 */
	public bool GetWaveFormInfo(string cueName, out CriAtomEx.WaveformInfo info)
	{
		using (var mem = new CriStructMemory<CriAtomEx.WaveformInfo>()) {
			bool result = criAtomExAcb_GetWaveformInfoByName(this.handle, cueName, mem.ptr);
			info = new CriAtomEx.WaveformInfo(mem.bytes, 0);
			return result;
		}
	}

	/**
	 * <summary>Gets the sound waveform information (Cue ID specified)</summary>
	 * <param name='cueId'>Cue ID</param>
	 * <param name='info'>Sound waveform information</param>
	 * <returns>Whether the acquisition was successful (success: True, failure: False)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the information on the sound waveform played by a Cue by specify the Cue ID.<br/>
	 * If there are multiple sound waveforms played by that Cue,
	 * the information of the sound waveform played first in the first Track is obtained.
	 * Returns False if there is no Cue with the specified ID.<br/></para>
	 * </remarks>
	 */
	public bool GetWaveFormInfo(int cueId, out CriAtomEx.WaveformInfo info)
	{
		using (var mem = new CriStructMemory<CriAtomEx.WaveformInfo>()) {
			bool result = criAtomExAcb_GetWaveformInfoById(this.handle, cueId, mem.ptr);
			info = new CriAtomEx.WaveformInfo(mem.bytes, 0);
			return result;
		}
	}

	/**
	 * <summary>Gets the number of Voices of the Cue for which Cue limit is set (Cue name specified)</summary>
	 * <param name='name'>Cue name</param>
	 * <returns>The number of Voices (-1 is returned when a Cue with no Cue limit is specified)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of Voices of the Cue with a Cue limit by specifying the Cue name.<br/>
	 * Returns -1 if there is no Cue with the specified name, or if the specified Cue has no Cue limit.<br/></para>
	 * </remarks>
	 */
	public int GetNumCuePlaying(string name)
	{
		return criAtomExAcb_GetNumCuePlayingCountByName(this.handle, name);
	}

	/**
	 * <summary>Gets the number of Voices of the Cue for which Cue limit is set (Cue ID specified)</summary>
	 * <param name='id'>Cue ID</param>
	 * <returns>The number of Voices (-1 is returned when a Cue with no Cue limit is specified)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of Voices of the Cue with a Cue limit by specifying the Cue ID.<br/>
	 * Returns -1 if there is no Cue with the specified ID, or if the specified Cue has no Cue limit.<br/></para>
	 * </remarks>
	 */
	public int GetNumCuePlaying(int id)
	{
		return criAtomExAcb_GetNumCuePlayingCountById(this.handle, id);
	}

	/**
	 * <summary>Gets the Block index (Cue name specified)</summary>
	 * <param name='cueName'>Cue name</param>
	 * <param name='blockName'>Block name</param>
	 * <returns>Block index</returns>
	 * <remarks>
	 * <para header='Description'>Get the Block index from the Cue name and the Block name.<br/>
	 * Returns 0xFFFFFFFF if there is no Cue with the specified name
	 * or the Block name.</para>
	 * </remarks>
	 */
	public int GetBlockIndex(string cueName, string blockName)
	{
		return criAtomExAcb_GetBlockIndexByName(this.handle, cueName, blockName);
	}

	/**
	 * <summary>Gets Block index (Cue ID specified)</summary>
	 * <param name='cueId'>Cue ID</param>
	 * <param name='blockName'>Block name</param>
	 * <returns>Block index</returns>
	 * <remarks>
	 * <para header='Description'>Gets the Block index from the Cue ID and the Block name.<br/>
	 * Returns 0xFFFFFFFF if there is no Cue with the specified ID
	 * or the Block name.</para>
	 * </remarks>
	 */
	public int GetBlockIndex(int cueId, string blockName)
	{
		return criAtomExAcb_GetBlockIndexById(this.handle, cueId, blockName);
	}

	/**
	 * <summary>Gets the number of AISAC Controls that can be controlled by a Cue (Cue name specified)</summary>
	 * <param name='cueName'>Cue name</param>
	 * <returns>The number of AISAC Controls</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of AISAC Controls that can be controlled by the Cue by specifying the Cue name.<br/>
	 * Returns -1 if there is no Cue with the specified name.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetUsableAisacControl'/>
	 */
	public int GetNumUsableAisacControls(string cueName)
	{
		return criAtomExAcb_GetNumUsableAisacControlsByName(this.handle, cueName);
	}

	/**
	 * <summary>Gets the number of AISAC Controls that can be controlled by a Cue (Cue ID specified)</summary>
	 * <param name='cueId'>Cue ID</param>
	 * <returns>The number of AISAC Controls</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of AISAC Controls that can be controlled by the Cue by specifying the Cue ID.<br/>
	 * Returns -1 if there is no Cue with the specified ID.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetUsableAisacControl'/>
	 */
	public int GetNumUsableAisacControls(int cueId)
	{
		return criAtomExAcb_GetNumUsableAisacControlsById(this.handle, cueId);
	}

	/**
	 * <summary>Gets the AISAC Controls that can be controlled by a Cue (Cue name specified)</summary>
	 * <param name='cueName'>Cue name</param>
	 * <param name='index'>AISAC Control index</param>
	 * <param name='info'>AISAC Control information</param>
	 * <returns>Whether the acquisition was successful (success: True, failure: False)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the AISAC Control information by specifying the Cue name and the AISAC Control index.<br/>
	 * Returns False if there is no Cue with the specified name.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetNumUsableAisacControls'/>
	 */
	public bool GetUsableAisacControl(string cueName, int index, out CriAtomEx.AisacControlInfo info)
	{
		using (var mem = new CriStructMemory<CriAtomEx.AisacControlInfo>()) {
			bool result = criAtomExAcb_GetUsableAisacControlByName(this.handle, cueName, (ushort)index, mem.ptr);
			info = new CriAtomEx.AisacControlInfo(mem.bytes, 0);
			return result;
		}
	}

	/**
	 * <summary>Gets the AISAC Controls that can be controlled by a Cue (Cue ID specified)</summary>
	 * <param name='cueId'>Cue ID</param>
	 * <param name='index'>AISAC Control index</param>
	 * <param name='info'>AISAC Control information</param>
	 * <returns>Whether the acquisition was successful (success: True, failure: False)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the AISAC Control information by specifying the Cue ID and the AISAC Control index.<br/>
	 * Returns False if there is no Cue with the specified ID.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetNumUsableAisacControls'/>
	 */
	public bool GetUsableAisacControl(int cueId, int index, out CriAtomEx.AisacControlInfo info)
	{
		using (var mem = new CriStructMemory<CriAtomEx.AisacControlInfo>()) {
			bool result = criAtomExAcb_GetUsableAisacControlById(this.handle, cueId, (ushort)index, mem.ptr);
			info = new CriAtomEx.AisacControlInfo(mem.bytes, 0);
			return result;
		}
	}

	public CriAtomEx.AisacControlInfo[] GetUsableAisacControlList(string cueName)
	{
		int numControls = GetNumUsableAisacControls(cueName);
		var infoList = new CriAtomEx.AisacControlInfo[numControls];
		for (int i = 0; i < numControls; i++) {
			this.GetUsableAisacControl(cueName, i, out infoList[i]);
		}
		return infoList;
	}

	public CriAtomEx.AisacControlInfo[] GetUsableAisacControlList(int cueId)
	{
		int numControls = GetNumUsableAisacControls(cueId);
		var infoList = new CriAtomEx.AisacControlInfo[numControls];
		for (int i = 0; i < numControls; i++) {
			this.GetUsableAisacControl(cueId, i, out infoList[i]);
		}
		return infoList;
	}

	/**
	 * <summary>Resets the Cue type state (Cue name specified)</summary>
	 * <param name='cueName'>Cue name</param>
	 * <remarks>
	 * <para header='Description'>Resets the Cue type state by specifying the Cue name.<br/></para>
	 * <para header='Note'>Only the state of the specified Cue is reset. The status of the sub synth contained in the Cue or Cue link destinations
	 * is not reset.</para>
	 * <para header='Note'>The Cue type state is a mechanism that manages the previously played Track as a state
	 * when playing a Cue other than the Polyphonic type Cue<br/>
	 * This function resets the state management area to the state immediately after ACB loading.</para>
	 * </remarks>
	 */
	public void ResetCueTypeState(string cueName)
	{
		criAtomExAcb_ResetCueTypeStateByName(this.handle, cueName);
	}

	/**
	 * <summary>Resets the Cue type state (Cue ID specified)</summary>
	 * <param name='cueId'>Cue name</param>
	 * <remarks>
	 * <para header='Description'>Resets the Cue type state by specifying the Cue name.<br/></para>
	 * <para header='Note'>Only the state of the specified Cue is reset. The status of the sub synth contained in the Cue or Cue link destinations
	 * is not reset.</para>
	 * <para header='Note'>The Cue type state is a mechanism that manages the previously played Track as a state
	 * when playing a Cue other than the Polyphonic type Cue<br/>
	 * This function resets the state management area to the state immediately after ACB loading.</para>
	 * </remarks>
	 */
	public void ResetCueTypeState(int cueId)
	{
		criAtomExAcb_ResetCueTypeStateById(this.handle, cueId);
	}

	/**
	 * <summary>Attaches the AWB file for streaming</summary>
	 * <param name='awb_binder'>Handle of the binder containing the AWB files</param>
	 * <param name='awb_path'>Path of the AWB file</param>
	 * <param name='awb_name'>AWB name</param>
	 * <remarks>
	 * <para header='Description'>Attaches an AWB file for the stream to the ACB handle.
	 * For the first argument awb_binder and the second argument awb_path,
	 * specify the AWB file for stream playback.<br/>
	 * The third argument awb_name is used to specify the slot to which the AWB is attached.
	 * Therefore, if you changed the AWB name output by AtomCraft (the base file name without extension),
	 * specify the original AWB name.<br/>
	 * An error callback is called when the attachment of the AWB file fails.<br/>
	 * The reason for the failure can be determined from the error callback message.<br/></para>
	 * <para header='Note'>When the AWB is attached, the binder ( CriFsBinderHn ) and loader ( CriFsLoaderHn ) are
	 * allocated inside the library.<br/>
	 * If you want to attach additional AWB files, you need to initialize the Atom library (or the CRI File System library)
	 * using the settings which allow the allocation of additional binders and loaders.<br/></para>
	 * </remarks>
	 */
	public void AttachAwbFile(CriFsBinder awb_binder, string awb_path, string awb_name)
	{
		if (this.isAvailable) {
			IntPtr binderHandle = (awb_binder != null) ? awb_binder.nativeHandle : IntPtr.Zero;
			criAtomExAcb_AttachAwbFile(this.handle, binderHandle, awb_path, awb_name, IntPtr.Zero, 0);
		}
	}

	/**
	 * <summary>Detaches the AWB file for streaming</summary>
	 * <param name='awb_name'>AWB name</param>
	 * <remarks>
	 * <para header='Description'>Detaches the AWB file for the stream attached to the ACB handle.
	 * For the first argument awb_name, specify the same AWB name as that specified when attaching the AWB.<br/></para>
	 * </remarks>
	 */
	public void DetachAwbFile(string awb_name)
	{
		if (this.isAvailable) {
			criAtomExAcb_DetachAwbFile(this.handle, awb_name);
		}
	}

	/**
	 * <summary>Gets the immediate release state of the ACB handle</summary>
	 * <returns>Status of ACB (True = instant release possible, False = there are players playing)</returns>
	 * <remarks>
	 * <para header='Description'>Checks if the ACB handle can be released immediately.<br/>
	 * Calling the CriWare.CriAtomExAcb::Dispose at the timing when this function returns false
	 * and will stop the player referencing the ACB handle.<br/>
	 * (For the ACB handle for stream playback, the process may block for a long time
	 * in the CriWare.CriAtomExAcb::Dispose function because it waits for the completion of file read.)<br/></para>
	 * </remarks>
	 */
	public bool IsReadyToRelease()
	{
		if (this.isAvailable) {
			bool result = criAtomExAcb_IsReadyToRelease(this.handle);
			return result;
		} else {
			return false;
		}
	}

	/* @cond excludele */
	/**
	 * <summary>Gets the loading progress</summary>
	 * <remarks>
	 * <para header='Description'>Gets the load status of the AAC data included in ACB on WebGLplatform.
	 * A value between 0.0f and 1.0f is returned. Loading is finished if 1.0f is returned.</para>
	 * </remarks>
	 */
	public float GetLoadProgress()
	{
#if !UNITY_EDITOR && UNITY_WEBGL
		return criAtomExAcb_GetLoadProgress(this.handle);
#else
		return 1.0f;
#endif
	}
	/* @endcond */

	/* @cond notpublic */
	/**
	 * <summary>Sets the decryption key</summary>
	 * <param name='key'>Decryption key</param>
	 * <param name='nonce'>Nonce (be sure to specify 0)</param>
	 * <remarks>
	 * <para header='Description'>Specifies the decryption key to decrypt the ACB data.<br/>
	 * It is possible to use different decryption keys for each ACB handle
	 * by calling this function after loading the ACB file.<br/>
	 * (The decryption key specified using this function is used for the relevant ACB file
	 * ignoring the setting by the decryptor.)</para>
	 * </remarks>
	 */
	public void Decrypt(ulong key, ulong nonce)
	{
		if (this.isAvailable) {
			CriAtomPlugin.DecryptAcb(this.handle, key, nonce);
		}
	}
	/* @endcond */

	#region Internal Members

	internal CriAtomExAcb(IntPtr handle, GCHandle? dataHandle)
	{
		this.handle = handle;
		if (dataHandle.HasValue) {
			this.dataHandle = dataHandle.Value;
		}
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	~CriAtomExAcb()
	{
		this.Dispose(false);
	}

	private IntPtr handle = IntPtr.Zero;
	private GCHandle dataHandle;
	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomExAcb_LoadAcbFile(IntPtr acb_binder, string acb_path,
		IntPtr awb_binder, string awb_path, IntPtr work, int work_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomExAcb_LoadAcbData(IntPtr acb_data, int acb_data_size,
		IntPtr awb_binder, string awb_path, IntPtr work, int work_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAcb_Release(IntPtr acb_hn);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcb_GetNumCues(IntPtr acb_hn);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcb_ExistsId(IntPtr acb_hn, int id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcb_ExistsName(IntPtr acb_hn, string name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcb_GetNumUsableAisacControlsById(IntPtr acb_hn, int id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcb_GetNumUsableAisacControlsByName(IntPtr acb_hn, string name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcb_GetUsableAisacControlById(
		IntPtr acb_hn, int id, ushort index, IntPtr info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcb_GetUsableAisacControlByName(
		IntPtr acb_hn, string name, ushort index, IntPtr info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcb_GetWaveformInfoById(
		IntPtr acb_hn, int id, IntPtr waveform_info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcb_GetWaveformInfoByName(
		IntPtr acb_hn, string name, IntPtr waveform_info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcb_GetCueInfoByName(IntPtr acb_hn, string name, IntPtr info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcb_GetCueInfoById(IntPtr acb_hn, int id, IntPtr info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcb_GetCueInfoByIndex(IntPtr acb_hn, int index, IntPtr info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcb_GetNumCuePlayingCountByName(IntPtr acb_hn, string name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcb_GetNumCuePlayingCountById(IntPtr acb_hn, int id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcb_GetBlockIndexById(IntPtr acb_hn, int id, string block_name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcb_GetBlockIndexByName(IntPtr acb_hn, string name, string block_name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAcb_ResetCueTypeStateByName(IntPtr acb_hn, string name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAcb_ResetCueTypeStateById(IntPtr acb_hn, int id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAcb_AttachAwbFile(IntPtr acb_hn, IntPtr awb_binder,
									string awb_path, string awb_name, IntPtr work, int work_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAcb_DetachAwbFile(IntPtr acb_hn, string awb_name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcb_IsReadyToRelease(IntPtr acb_hn);

	/* @cond excludele */
	#if !UNITY_EDITOR && UNITY_WEBGL
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern float criAtomExAcb_GetLoadProgress(IntPtr acb_hn);
	#endif
	/* @endcond */
	#else
	private static IntPtr criAtomExAcb_LoadAcbFile(IntPtr acb_binder, string acb_path,
		IntPtr awb_binder, string awb_path, IntPtr work, int work_size) { return new IntPtr(1); }
	private static IntPtr criAtomExAcb_LoadAcbData(IntPtr acb_data, int acb_data_size,
		IntPtr awb_binder, string awb_path, IntPtr work, int work_size) { return new IntPtr(1); }
	private static void criAtomExAcb_Release(IntPtr acb_hn) { }
	private static int criAtomExAcb_GetNumCues(IntPtr acb_hn) { return 0; }
	private static bool criAtomExAcb_ExistsId(IntPtr acb_hn, int id) { return false; }
	private static bool criAtomExAcb_ExistsName(IntPtr acb_hn, string name) { return false; }
	private static int criAtomExAcb_GetNumUsableAisacControlsById(IntPtr acb_hn, int id) { return 0; }
	private static int criAtomExAcb_GetNumUsableAisacControlsByName(IntPtr acb_hn, string name) { return 0; }
	private static bool criAtomExAcb_GetUsableAisacControlById(
		IntPtr acb_hn, int id, ushort index, IntPtr info) { return false; }
	private static bool criAtomExAcb_GetUsableAisacControlByName(
		IntPtr acb_hn, string name, ushort index, IntPtr info) { return false; }
	private static bool criAtomExAcb_GetWaveformInfoById(
		IntPtr acb_hn, int id, IntPtr waveform_info) { return false; }
	private static bool criAtomExAcb_GetWaveformInfoByName(
		IntPtr acb_hn, string name, IntPtr waveform_info) { return false; }
	private static bool criAtomExAcb_GetCueInfoByName(IntPtr acb_hn, string name, IntPtr info) { return false; }
	private static bool criAtomExAcb_GetCueInfoById(IntPtr acb_hn, int id, IntPtr info) { return false; }
	private static bool criAtomExAcb_GetCueInfoByIndex(IntPtr acb_hn, int index, IntPtr info) { return false; }
	private static int criAtomExAcb_GetNumCuePlayingCountByName(IntPtr acb_hn, string name) { return 0; }
	private static int criAtomExAcb_GetNumCuePlayingCountById(IntPtr acb_hn, int id) { return 0; }
	private static int criAtomExAcb_GetBlockIndexById(IntPtr acb_hn, int id, string block_name) { return -1; }
	private static int criAtomExAcb_GetBlockIndexByName(IntPtr acb_hn, string name, string block_name) { return -1; }
	private static void criAtomExAcb_ResetCueTypeStateByName(IntPtr acb_hn, string name) { }
	private static void criAtomExAcb_ResetCueTypeStateById(IntPtr acb_hn, int id) { }
	private static void criAtomExAcb_AttachAwbFile(IntPtr acb_hn, IntPtr awb_binder,
									string awb_path, string awb_name, IntPtr work, int work_size) { }
	private static void criAtomExAcb_DetachAwbFile(IntPtr acb_hn, string awb_name) { }
	private static bool criAtomExAcb_IsReadyToRelease(IntPtr acb_hn) { return false; }
	/* @cond excludele */
	#if !UNITY_EDITOR && UNITY_WEBGL
	private static float criAtomExAcb_GetLoadProgress(IntPtr acb_hn) { return 1.0f; }
	#endif
	/* @endcond */
	#endif
	#endregion
}

/**
 * <summary>Asynchronous loader of ACB and AWB data</summary>
 * <remarks>
 * <para header='Description'>A class for loading the ACB or AWB files asynchronously.<br/></para>
 * </remarks>
 */
public class CriAtomExAcbLoader : CriDisposable
{
	/**
	 * <summary>Status</summary>
	 * <remarks>
	 * <para header='Description'>The status of the asynchronous loader.<br/></para>
	 * </remarks>
	 */
	public enum Status
	{
		Stop,
		Loading,
		Complete,
		Error
	}

	/**
	 * <summary>Asynchronous loading of the ACB file</summary>
	 * <param name='binder'>Binder object</param>
	 * <param name='acbPath'>Path of the ACB file</param>
	 * <param name='awbPath'>Path of the AWB file</param>
	 * <param name='loadAwbOnMemory'>Whether to load the AWB file on the memory (optional)</param>
	 * <returns>CriAtomExAcbLoader object</returns>
	 * <remarks>
	 * <para header='Description'>Starts the asynchronous loading of the ACB file.<br/>
	 * You should call CriWare.CriAtomExAcbLoader::GetStatus for the return value to check the loading status.
	 * If the status changes to Complete, you can get the CriWare.CriAtomExAcb object using CriWare.CriAtomExAcbLoader::MoveAcb .<br/></para>
	 * </remarks>
	 */
	public static CriAtomExAcbLoader LoadAcbFileAsync(CriFsBinder binder, string acbPath, string awbPath, bool loadAwbOnMemory = false)
	{
		/* バージョン番号が不正なライブラリではキューシートをロードしない */
		/* 備考）不正に差し替えられたsoファイルを使用している可能性あり。 */
		bool isCorrectVersion = CriWare.Common.CheckBinaryVersionCompatibility();
		if (isCorrectVersion == false) {
			return null;
		}

		IntPtr binderHandle = (binder != null) ? binder.nativeHandle : IntPtr.Zero;
		LoaderConfig config = new LoaderConfig();
		config.shouldLoadAwbOnMemory = loadAwbOnMemory;
		IntPtr handle = criAtomExAcbLoader_Create(ref config);
		if (handle == IntPtr.Zero) { return null; }
		bool result = criAtomExAcbLoader_LoadAcbFileAsync(handle, binderHandle, acbPath, binderHandle, awbPath);
		if (result == false) { return null; }
		return new CriAtomExAcbLoader(handle, null);
	}

	/**
	 * <summary>Asynchronous loading of the ACB data</summary>
	 * <param name='acbData'>Byte array of the ACB data</param>
	 * <param name='awbBinder'>Binder object for AWB</param>
	 * <param name='awbPath'>Path of the AWB file</param>
	 * <param name='loadAwbOnMemory'>Whether to load the AWB file on the memory (optional)</param>
	 * <returns>CriAtomExAcbLoader object</returns>
	 * <remarks>
	 * <para header='Description'>Starts asynchronous loading of the ACB data.<br/>
	 * You should call CriWare.CriAtomExAcbLoader::GetStatus for the return value to check the loading status.
	 * If the status changes to Complete, you can get CriWare.CriAtomExAcb using CriWare.CriAtomExAcbLoader::MoveAcb .<br/></para>
	 * </remarks>
	 */
	public static CriAtomExAcbLoader LoadAcbDataAsync(byte[] acbData, CriFsBinder awbBinder, string awbPath, bool loadAwbOnMemory = false)
	{
		/* バージョン番号が不正なライブラリではキューシートをロードしない */
		/* 備考）不正に差し替えられたsoファイルを使用している可能性あり。 */
		bool isCorrectVersion = CriWare.Common.CheckBinaryVersionCompatibility();
		if (isCorrectVersion == false) {
			return null;
		}

		IntPtr binderHandle = (awbBinder != null) ? awbBinder.nativeHandle : IntPtr.Zero;
		LoaderConfig config = new LoaderConfig();
		config.shouldLoadAwbOnMemory = loadAwbOnMemory;
		IntPtr handle = criAtomExAcbLoader_Create(ref config);
		if (handle == IntPtr.Zero) { return null; }
		GCHandle dataHandle = GCHandle.Alloc(acbData, GCHandleType.Pinned);
		bool result = criAtomExAcbLoader_LoadAcbDataAsync(handle, dataHandle.AddrOfPinnedObject(), acbData.Length, binderHandle, awbPath);
		if (result == false) { return null; }
		return new CriAtomExAcbLoader(handle, dataHandle);
	}

	/**
	 * <summary>Gets the status</summary>
	 * <returns>Asynchronous loader loading status</returns>
	 * <remarks>
	 * <para header='Description'>Gets the status of the asynchronous loader.<br/>
	 * When the return value of this function changes to Complete, you can get CriWare.CriAtomExAcb using CriWare.CriAtomExAcbLoader::MoveAcb .<br/></para>
	 * </remarks>
	 */
	public Status GetStatus()
	{
		return criAtomExAcbLoader_GetStatus(this.handle);
	}

	/**
	 * <summary>Gets the ACB data</summary>
	 * <returns>CriAtomExAcb object</returns>
	 * <remarks>
	 * <para header='Description'>Gets the ACB data loaded asynchronously.<br/>
	 * Call this function after the return value of CriWare.CriAtomExAcbLoader::GetStatus changes to Complete.<br/></para>
	 * </remarks>
	 */
	public CriAtomExAcb MoveAcb()
	{
		IntPtr movedAcbHandle = criAtomExAcbLoader_MoveAcbHandle(this.handle);
		if (movedAcbHandle != IntPtr.Zero) {
			CriAtomExAcb movedAcb = new CriAtomExAcb(movedAcbHandle, this.gch);
			this.gch = null;
			return movedAcb;
		}
		return null;
	}

	/**
	 * <summary>Discards an asynchronous loader</summary>
	 * <remarks>
	 * <para header='Description'>Discard an asynchronous loader.<br/></para>
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

		if (this.handle != IntPtr.Zero) {
			criAtomExAcbLoader_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}

		if (disposing && this.gch.HasValue && this.gch.Value.IsAllocated) {
			this.gch.Value.Free();
		}
	}

	#region Internal Members
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct LoaderConfig
	{
		public bool shouldLoadAwbOnMemory;
	}

	private IntPtr handle = IntPtr.Zero;
	private GCHandle? gch;

	private CriAtomExAcbLoader(IntPtr handle, GCHandle? dataHandle)
	{
		this.handle = handle;
		if (dataHandle.HasValue) {
			this.gch = dataHandle.Value;
		}
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	~CriAtomExAcbLoader()
	{
		this.Dispose(false);
	}
	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomExAcbLoader_Create([In] ref LoaderConfig config);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAcbLoader_Destroy(IntPtr acb_loader);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcbLoader_LoadAcbFileAsync(IntPtr acb_loader, IntPtr acb_binder, string acb_path, IntPtr awb_binder, string awb_path);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcbLoader_LoadAcbDataAsync(IntPtr acb_loader, IntPtr acb_data, int acb_size, IntPtr awb_binder, string awb_path);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern Status criAtomExAcbLoader_GetStatus(IntPtr acb_loader);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAcbLoader_WaitForCompletion(IntPtr acb_loader);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomExAcbLoader_MoveAcbHandle(IntPtr acb_loader);
	#else
	private static IntPtr criAtomExAcbLoader_Create([In] ref LoaderConfig config) { return new IntPtr(1); }
	private static void criAtomExAcbLoader_Destroy(IntPtr acb_loader) { }
	private static bool criAtomExAcbLoader_LoadAcbFileAsync(IntPtr acb_loader, IntPtr acb_binder, string acb_path, IntPtr awb_binder, string awb_path)
	{ return false; }
	private static bool criAtomExAcbLoader_LoadAcbDataAsync(IntPtr acb_loader, IntPtr acb_data, int acb_size, IntPtr awb_binder, string awb_path)
	{ return false; }
	private static Status criAtomExAcbLoader_GetStatus(IntPtr acb_loader) { return Status.Complete; }
	private static bool criAtomExAcbLoader_WaitForCompletion(IntPtr acb_loader) { return true; }
	private static IntPtr criAtomExAcbLoader_MoveAcbHandle(IntPtr acb_loader) { return IntPtr.Zero; }
	#endif
	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
