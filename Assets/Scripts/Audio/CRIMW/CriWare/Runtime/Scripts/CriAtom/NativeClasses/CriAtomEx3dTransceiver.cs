/****************************************************************************
 *
 * Copyright (c) 2020 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !UNITY_WEBGL
	#define CRIWARE_TRANSCEIVER_N_ELEVATIONANGLEAISAC_SUPPORT
#endif

using System;
using System.Runtime.InteropServices;
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
 * <summary>3D Transceiver object</summary>
 * <remarks>
 * <para header='Description'>An object for handling the 3D Transceivers.<br/>
 * <br/>
 * You set the parameters or location information of the 3D Transceiver through this object.</para>
 * </remarks>
 */
public class CriAtomEx3dTransceiver : CriDisposable
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Config
	{
		public int reserved;
	}

	/**
	 * <summary>Creates a 3D Transceiver object</summary>
	 * <remarks>
	 * <para header='Description'>Creates a 3D Transceiver.<br/></para>
	 * <para header='Note'>The library must be initialized before calling this function.<br/></para>
	 * </remarks>
	 */
	public CriAtomEx3dTransceiver()
	{
		Config config = new Config();
		this.handle = UnsafeNativeMethods.criAtomEx3dTransceiver_Create(ref config, IntPtr.Zero, 0);
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	/**
	 * <summary>Discards a 3D Transceiver object</summary>
	 * <remarks>
	 * <para header='Description'>Discards the 3D Transceiver object.<br/>
	 * When this function is called, all the resources allocated in the DLL when creating the 3D Transceiver object are released,<br/>
	 * and the 3D Transceiver is disabled.</para>
	 * </remarks>
	 */
	public override void Dispose()
	{
		this.Dispose(true);
	}

	private void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		if (this.handle != IntPtr.Zero) {
			UnsafeNativeMethods.criAtomEx3dTransceiver_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}

		if (disposing) {
			GC.SuppressFinalize(this);
		}
	}

	public IntPtr nativeHandle
	{
		get { return this.handle; }
	}

	/**
	 * <summary>Updates the 3D Transceiver</summary>
	 * <remarks>
	 * <para header='Description'>Updates the 3D Transceiver using the parameters set for the 3D Transceiver.<br/>
	 * This function updates all the parameters set in the 3D Transceiver.<br/>
	 * It is more efficient to change multiple parameters before performing update
	 * than to update using this function each time a parameter is changed.<br/></para>
	 * <para header='Note'>This function runs independently of the parameter update in the AtomExPlayer
	 * ( CriWare.CriAtomExPlayer::UpdateAll , CriAtomExPlayer::Update ).<br/>
	 * If you change the parameters of the 3D transceiver, perform the update process using this function.</para>
	 * </remarks>
	 * <example><code>
	 *  ：
	 * // トランシーバーの作成
	 * CriAtomEx3dTransceiver transceiver = new CriAtomEx3dTransceiver();
	 *  ：
	 * // トランシーバーの入力位置を設定
	 * transceiver.SetInputPosition(Vector3.zero);
	 *
	 * // トランシーバーの出力位置を設定
	 * transceiver.SetOutputPosition(Vector3.zero);
	 *
	 * // 注意）この時点ではトランシーバーの位置はまだ変更されていません。
	 *
	 * // 変更の適用
	 * transceiver.Update();
	 *  ：
	 * </code></example>
	 */
	public void Update()
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_Update(this.handle);
	}

	/**
	 * <summary>Sets the position of the 3D Transceiver input</summary>
	 * <param name='position'>Input position vector</param>
	 * <remarks>
	 * <para header='Description'>Sets the position of the 3D Transceiver input.<br/>
	 * The position is used for calculating the distance attenuation and localization.<br/>
	 * The position is specified as a 3D vector.<br/>
	 * The default is (0.0f, 0.0f, 0.0f).<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetInputPosition(Vector3 position)
	{
		CriAtomEx.NativeVector pos = new CriAtomEx.NativeVector(position);
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetInputPosition(this.handle, ref pos);
	}

	/**
	 * <summary>Sets the position of the 3D Transceiver output</summary>
	 * <param name='position'>Output position vector</param>
	 * <remarks>
	 * <para header='Description'>Sets the position of the 3D Transceiver output.<br/>
	 * The position is used for calculating the distance attenuation and localization.<br/>
	 * The position is specified as a 3D vector.<br/>
	 * The default is (0.0f, 0.0f, 0.0f).<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetOutputPosition(Vector3 position)
	{
		CriAtomEx.NativeVector pos = new CriAtomEx.NativeVector(position);
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetOutputPosition(this.handle, ref pos);
	}

	/**
	 * <summary>Sets the orientation of the 3D Transceiver input</summary>
	 * <param name='front'>Forward vector</param>
	 * <param name='top'>Upper vector</param>
	 * <remarks>
	 * <para header='Description'>Sets the orientation of the 3D Transceiver using the forward and upward vectors.<br/>
	 * The orientation is specified using a 3D vector. The orientation vector that is set is normalized inside the library before being used.<br/>
	 * The default values are as follows:<br/>
	 *  - Forward vector: (0.0f, 0.0f, 1.0f)
	 *  - Upward vector: (0.0f, 1.0f, 0.0f)</para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetInputOrientation(Vector3 front, Vector3 top)
	{
		CriAtomEx.NativeVector orient_front = new CriAtomEx.NativeVector(front);
		CriAtomEx.NativeVector orient_top = new CriAtomEx.NativeVector(top);
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetInputOrientation(this.handle, ref orient_front, ref orient_top);
	}

	/**
	 * <summary>Sets the orientation of the 3D Transceiver output</summary>
	 * <param name='front'>Forward vector</param>
	 * <param name='top'>Upper vector</param>
	 * <remarks>
	 * <para header='Description'>Sets the orientation of the 3D Transceiver using the forward and upward vectors.<br/>
	 * The orientation is specified using a 3D vector. The orientation vector that is set is normalized inside the library before being used.<br/>
	 * The default values are as follows:<br/>
	 *  - Forward vector: (0.0f, 0.0f, 1.0f)
	 *  - Upward vector: (0.0f, 1.0f, 0.0f)</para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetOutputOrientation(Vector3 front, Vector3 top)
	{
		CriAtomEx.NativeVector orient_front = new CriAtomEx.NativeVector(front);
		CriAtomEx.NativeVector orient_top = new CriAtomEx.NativeVector(top);
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetOutputOrientation(this.handle, ref orient_front, ref orient_top);
	}

	/**
	 * <summary>Sets the sound cone parameters for the 3D Transceiver output</summary>
	 * <param name='insideAngle'>Sound cone inside angle</param>
	 * <param name='outsideAngle'>Sound cone outside angle</param>
	 * <param name='outsideVolume'>Sound cone outside volume</param>
	 * <remarks>
	 * <para header='Description'>Sets the sound cone parameters for the 3D Transceiver output.<br/>
	 * The sound cone represents the direction in which the sound is emitted from the transceiver and is used to express the directionality of the sound.<br/>
	 * The sound cone consists of inner cone and outer cone. The inside angle represents the angle of the inner cone,
	 * the outside angle represents the angle of the outer cone, and the outside volume represents the volume
	 * beyond the outer cone angle.<br/>
	 * At angles smaller than the inner cone angle, no attenuation is applied by the cone.
	 * In the direction between the inner and outer cones, the sound is gradually attenuated to the outside volume.<br/>
	 * The inside and outside angles are specified within the range of 0.0f to 360.0f.<br/>
	 * The outside volume is specifies as a scale factor for the amplitude within the range of 0.0f to 1.0f (the unit is not decibel).<br/>
	 * The default values when initializing the library are as follows, and no attenuation is applied by the cone.<br/>
	 *  - Inside angle: 360.0f
	 *  - Outside angle: 360.0f
	 *  - Outside volume: 0.0f
	 *  .</para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetOutputConeParameter(float insideAngle, float outsideAngle, float outsideVolume)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetOutputConeParameter(this.handle, insideAngle, outsideAngle, outsideVolume);
	}

	/**
	 * <summary>Sets the minimum/maximum distance of the 3D Transceiver output</summary>
	 * <param name='minDistance'>Minimum distance</param>
	 * <param name='maxDistance'>Maximum distance</param>
	 * <remarks>
	 * <para header='Description'>Sets the minimum/maximum distance of the 3D Transceiver output.<br/>
	 * The minimum distance represents the distance at which the volume cannot be increased any further.<br/>
	 * The maximum distance is the distance with the lowest volume.<br/>
	 * The default values when the library is initialized are as follows:<br/>
	 *  - Minimum distance: 0.0f
	 *  - Maximum distance: 0.0f
	 *  .
	 * When this function is called when the minimum/maximum distances are set on the data,
	 * the values on the data are overridden (ignored).<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetOutputMinMaxDistance(float minDistance, float maxDistance)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetOutputMinMaxAttenuationDistance(this.handle, minDistance, maxDistance);
	}

	/**
	 * <summary>Sets the interior Panning boundary distance for the 3D Transceiver output</summary>
	 * <param name='radius'>Radius of the 3D Transceiver output</param>
	 * <param name='interiorDistance'>Interior distance</param>
	 * <remarks>
	 * <para header='Description'>Sets the interior Panning boundary distance for the 3D Transceiver output.<br/>
	 * The radius of the 3D Transceiver output is the radius when the 3D Transceiver output is considered as a sphere.<br/>
	 * Interior distance is the distance from the radius of the 3D sound source to which the interior Panning is applied.<br/>
	 * Within the radius of the 3D Transceiver output, interior Panning is applied, but since the interior distance is treated as 0.0,
	 * the sound is played back from all speakers at the same volume.<br/>
	 * Within the interior distance, interior Panning is applied.<br/>
	 * Outside the interior distance, interior Panning is not applied and the sound is played from 1 or 2 speakers
	 * closest to the 3D Transceiver output position.<br/>
	 * The default values when the library is initialized are as follows:<br/>
	 *  - 3D Transceiver radius: 0.0f
	 *  - Interior distance: 0.0f (depends on the minimum distance of the 3D Transceiver)
	 *  .</para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 * <seealso cref='CriAtomEx3dTransceiver::SetMinMaxDistance'/>
	 */
	public void SetOutputInteriorPanField(float radius, float interiorDistance)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetOutputInteriorPanField(this.handle, radius, interiorDistance);
	}

	/**
	 * <summary>Sets the cross-fade boundary distance of the 3D Transceiver input</summary>
	 * <param name='directAudioRadius'>Radius of direct sound area</param>
	 * <param name='crossfadeDistance'>Cross-fade distance</param>
	 * <remarks>
	 * <para header='Description'>Sets the cross-fade boundary distance of the 3D Transceiver input.<br/>
	 * Within the radius of the direct sound area, the sound from the 3D Transceiver output is not played, and only the sound from the 3D sound source is played.<br/>
	 * The cross-fade distance is the distance from the direct sound area where the cross-fade of the 3D Transceiver output and the sound from the 3D source is applied.<br/>
	 * In the direct sound area, the sound from the sound source =1 and the sound from the 3D Transceiver is =0,
	 * so the sound from the 3D Transceiver output is inaudible and only the sound from the sound source is played.<br/>
	 * Within the cross-fade distance, the cross0fade is applied depending on the position of the listener.<br/>
	 * Outside the cross-fade distance, the sound from the 3D sound source is not heard, only the sound from the 3D Transceiver output is heard.
	 * The default values when the library is initialized are as follows:<br/>
	 *  - Radius of direct sound area: 0.0f
	 *  - Cross-fade distance: 0.0f
	 *  .</para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 * <seealso cref='CriAtomEx3dTransceiver::SetMinMaxDistance'/>
	 */
	public void SetInputCrossFadeField(float directAudioRadius, float crossfadeDistance)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetInputCrossFadeField(this.handle, directAudioRadius, crossfadeDistance);
	}

	/**
	 * <summary>Sets the 3D Transceiver output volume</summary>
	 * <param name='volume'>Volume</param>
	 * <remarks>
	 * <para header='Description'>Sets the volume of the 3D Transceiver output.<br/>
	 * The volume of the 3D Transceiver output affects only the volume related to localization (L,R,SL,SR),
	 * and does not affect the output level of LFE or the center.<br/>
	 * For the volume value, specify a real value in the range of 0.0f to 1.0f.<br/>
	 * The volume value is a scale factor for the amplitude of the sound data (the unit is not decibel).<br/>
	 * For example, if you specify 1.0f, the original sound is played at its unmodified volume.<br/>
	 * If you specify 0.5f, the sound is played at the volume by halving the amplitude (-6dB)
	 * of the original waveform.<br/>
	 * If you specify 0.0f, the sound is muted (silent).<br/>
	 * The default value when the library is initialized is 1.0f.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetOutputVolume(float volume)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetOutputVolume(this.handle, volume);
	}

	/**
	 * <summary>Attach AISAC to the 3D Transceiver</summary>
	 * <param name='globalAisacName'>Global AISAC name to attach</param>
	 * <remarks>
	 * <para header='Description'>Attach AISAC to the 3D Transceiver.
	 * By attaching AISAC, you can get AISAC effect even if you didn't set AISAC
	 * for the Cue or the Track.<br/>
	 * <br/>
	 * If attaching AISAC failed, CriWareErrorHandler calls the error callback.<br/>
	 * Check the error message for the reason why the attaching AISAC failed.<br/></para>
	 * <para header='Note'>Only the global ACF included in the global settings (AISAC file) can be attached.<br/>
	 * To get the effect of AISAC , it is necessary to set the appropriate AISAC control value
	 * in the same way as the AISAC set for Cues or Tracks.<br/></para>
	 * <para header='Note'>Even if "an AISAC to change AISAC control value" is set for the Cue or Track,
	 * the applied AISAC control value does not affect the AISAC attached to the 3Dtransceiver.<br/>
	 * Currently, it does not support the AISAC attachment of control type
	 * "Automodulation" or "Random".<br/>
	 * Currently, the maximum number of AISACs that can be attached to a 3D Transceiver is fixed to 8.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::DetachAisac'/>
	 */
	public void AttachAisac(string globalAisacName)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_AttachAisac(this.handle, globalAisacName);
	}

	/**
	 * <summary>Detach AISAC from the 3D Transceiver</summary>
	 * <param name='globalAisacName'>Global AISAC name to detach</param>
	 * <remarks>
	 * <para header='Description'>Detach (remove) AISAC from the 3D Transceiver.<br/>
	 * <br/>
	 * If detaching AISAC fails, CriWareErrorHandler calls the error callback.<br/>
	 * Check the error message for the reason why detaching AISAC failed.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::AttachAisac'/>
	 */
	public void DetachAisac(string globalAisacName)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_DetachAisac(this.handle, globalAisacName);
	}

	/**
	 * <summary>Sets the maximum amount of change in angle AISAC control value</summary>
	 * <param name='maxDelta'>Maximum amount of change in angle AISAC control value</param>
	 * <remarks>
	 * <para header='Description'>Sets the maximum amount of change when the AISAC control value is changed by the angle AISAC.<br/>
	 * By changing the maximum amount of change to a lower value, the change of the AISAC control value by
	 * angle AISAC can be made smooth even if the relative angle between 3D the transceiver and the listener changes abruptly.<br/>
	 * For example, if (0.5f / 30.0f) is set, the change takes place over 30 frames
	 * when the angle changes from 0° to 180°.<br/>
	 * The default value is 1.0f (no limit).
	 * Since this parameter cannot be set on the data, the value set using this function is always used.<br/></para>
	 * <para header='Note'>To actually apply the set parameters,
	 * you need to call the CriWare.CriAtomEx3dTransceiver::Update function.<br/>
	 * The maximum amount of change set by this function is applied
	 * only to the change of the angle AISAC control value calculated based on the localization angle.
	 * It does not affect the localization angle itself.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetMaxAngleAisacDelta(float maxDelta)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetMaxAngleAisacDelta(this.handle, maxDelta);
	}

	/**
	 * <summary>Sets the Distance AISAC Control ID</summary>
	 * <param name='aisacControlId'>Distance AISAC Control ID</param>
	 * <remarks>
	 * <para header='Description'>Specifies the AISAC control ID that is linked to the distance attenuation between the minimum distance and the maximum distance.<br/>
	 * If the AISAC control ID is set by this function, the default distance attenuation becomes invalid.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetDistanceAisacControlId(ushort aisacControlId)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetDistanceAisacControlId(this.handle, aisacControlId);
	}

	/**
	 * <summary>Sets the listener reference azimuth AISAC control setting ID</summary>
	 * <param name='aisacControlId'>Listener reference azimuth AISAC control ID</param>
	 * <remarks>
	 * <para header='Description'>Specifies the AISAC control ID linked to the azimuth of the 3D Transceiver seen from the listener.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetListenerBasedAzimuthAngleAisacControlId(ushort aisacControlId)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetListenerBasedAzimuthAngleAisacControlId(this.handle, aisacControlId);
	}

	/**
	 * <summary>Sets the listener reference elevation AISAC control setting ID</summary>
	 * <param name='aisacControlId'>Listener reference elevation AISAC control ID</param>
	 * <remarks>
	 * <para header='Description'>Specifies the AISAC control ID linked to the elevation of the 3D Transceiver seen from the listener.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetListenerBasedElevationAngleAisacControlId(ushort aisacControlId)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetListenerBasedElevationAngleAisacControlId(this.handle, aisacControlId);
	}

	/**
	 * <summary>Sets the 3D Transceiver output reference azimuth AISAC control ID</summary>
	 * <param name='aisacControlId'>3D Transceiver reference azimuth AISAC control ID</param>
	 * <remarks>
	 * <para header='Description'>Specify the AISAC control ID linked to the azimuth of listener as seen from the position of the 3D Transceiver output.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetTransceiverOutputBasedAzimuthAngleAisacControlId(ushort aisacControlId)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetTransceiverOutputBasedAzimuthAngleAisacControlId(this.handle, aisacControlId);
	}

	/**
	 * <summary>Sets the 3D Transceiver output reference elevation AISAC control ID</summary>
	 * <param name='aisacControlId'>3D Transceiver reference elevation AISAC control ID</param>
	 * <remarks>
	 * <para header='Description'>Specify the AISAC control ID linked to the elevation of listener as seen from the position of the 3D Transceiver output.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dTransceiver::Update function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void SetTransceiverOutputBasedElevationAngleAisacControlId(ushort aisacControlId)
	{
		UnsafeNativeMethods.criAtomEx3dTransceiver_SetTransceiverOutputBasedElevationAngleAisacControlId(this.handle, aisacControlId);
	}

	/**
	 * <summary>Sets the 3D region</summary>
	 * <remarks>
	 * <para header='Description'>Set the 3D regions to the 3D sound source.</para>
	 * <para header='Note'>If the regions set to the 3D sound source and 3D listener set to the same ExPlayer are different,<br/>
	 * and if there is no 3D Transceiver that has the same region as the 3D sound source, the sound is muted.<br/>
	 * To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dRegion::Create'/>
	 * <seealso cref='CriAtomEx3dTransceiver::Update'/>
	 */
	public void Set3dRegion(CriAtomEx3dRegion region3d)
	{
		IntPtr region3dHandle = (region3d == null) ? IntPtr.Zero : region3d.nativeHandle;
		UnsafeNativeMethods.criAtomEx3dTransceiver_Set3dRegionHn(this.handle, region3dHandle);
	}

	#region Internal Members
	~CriAtomEx3dTransceiver()
	{
		this.Dispose(false);
	}

	private IntPtr handle = IntPtr.Zero;
	#endregion

	#region Dll Import
	private static class UnsafeNativeMethods
	{
#if !CRIWARE_ENABLE_HEADLESS_MODE && CRIWARE_TRANSCEIVER_N_ELEVATIONANGLEAISAC_SUPPORT
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern IntPtr criAtomEx3dTransceiver_Create(ref Config config, IntPtr work, int work_size);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_Destroy(IntPtr ex_3d_transceiver);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_Update(IntPtr ex_3d_transceiver);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetInputPosition(IntPtr ex_3d_transceiver, ref CriAtomEx.NativeVector position);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetOutputPosition(IntPtr ex_3d_transceiver, ref CriAtomEx.NativeVector position);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetInputOrientation(IntPtr ex_3d_transceiver, ref CriAtomEx.NativeVector front, ref CriAtomEx.NativeVector top);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetOutputOrientation(IntPtr ex_3d_transceiver, ref CriAtomEx.NativeVector front, ref CriAtomEx.NativeVector top);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetOutputConeParameter(IntPtr ex_3d_transceiver, float inside_angle, float outside_angle, float outside_volume);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetOutputMinMaxAttenuationDistance(IntPtr ex_3d_transceiver, float min_attenuation_distance, float max_attenuation_distance);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetOutputInteriorPanField(IntPtr ex_3d_transceiver, float transceiver_radius, float interior_distance);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetInputCrossFadeField(IntPtr ex_3d_transceiver, float direct_audio_radius, float crossfade_distance);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetOutputVolume(IntPtr ex_3d_transceiver, float volume);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_AttachAisac(IntPtr ex_3d_transceiver, string global_aisac_name);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_DetachAisac(IntPtr ex_3d_transceiver, string global_aisac_name);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetMaxAngleAisacDelta(IntPtr ex_3d_transceiver, float max_delta);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetDistanceAisacControlId(IntPtr ex_3d_transceiver, ushort aisac_control_id);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetListenerBasedAzimuthAngleAisacControlId(IntPtr ex_3d_transceiver, ushort aisac_control_id);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetListenerBasedElevationAngleAisacControlId(IntPtr ex_3d_transceiver, ushort aisac_control_id);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetTransceiverOutputBasedAzimuthAngleAisacControlId(IntPtr ex_3d_transceiver, ushort aisac_control_id);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_SetTransceiverOutputBasedElevationAngleAisacControlId(IntPtr ex_3d_transceiver, ushort aisac_control_id);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dTransceiver_Set3dRegionHn(IntPtr ex_3d_transceiver, IntPtr ex_3d_region);
#else
		internal static IntPtr criAtomEx3dTransceiver_Create(ref Config config, IntPtr work, int work_size) { return IntPtr.Zero; }
		internal static void criAtomEx3dTransceiver_Destroy(IntPtr ex_3d_transceiver) { }
		internal static void criAtomEx3dTransceiver_Update(IntPtr ex_3d_transceiver) { }
		internal static void criAtomEx3dTransceiver_SetInputPosition(IntPtr ex_3d_transceiver, ref CriAtomEx.NativeVector position) { }
		internal static void criAtomEx3dTransceiver_SetOutputPosition(IntPtr ex_3d_transceiver, ref CriAtomEx.NativeVector position) { }
		internal static void criAtomEx3dTransceiver_SetInputOrientation(IntPtr ex_3d_transceiver, ref CriAtomEx.NativeVector front, ref CriAtomEx.NativeVector top) { }
		internal static void criAtomEx3dTransceiver_SetOutputOrientation(IntPtr ex_3d_transceiver, ref CriAtomEx.NativeVector front, ref CriAtomEx.NativeVector top) { }
		internal static void criAtomEx3dTransceiver_SetOutputConeParameter(IntPtr ex_3d_transceiver, float inside_angle, float outside_angle, float outside_volume) { }
		internal static void criAtomEx3dTransceiver_SetOutputMinMaxAttenuationDistance(IntPtr ex_3d_transceiver, float min_attenuation_distance, float max_attenuation_distance) { }
		internal static void criAtomEx3dTransceiver_SetOutputInteriorPanField(IntPtr ex_3d_transceiver, float transceiver_radius, float interior_distance) { }
		internal static void criAtomEx3dTransceiver_SetInputCrossFadeField(IntPtr ex_3d_transceiver, float direct_audio_radius, float crossfade_distance) { }
		internal static void criAtomEx3dTransceiver_SetOutputVolume(IntPtr ex_3d_transceiver, float volume) { }
		internal static void criAtomEx3dTransceiver_AttachAisac(IntPtr ex_3d_transceiver, string global_aisac_name) { }
		internal static void criAtomEx3dTransceiver_DetachAisac(IntPtr ex_3d_transceiver, string global_aisac_name) { }
		internal static void criAtomEx3dTransceiver_SetMaxAngleAisacDelta(IntPtr ex_3d_transceiver, float max_delta) { }
		internal static void criAtomEx3dTransceiver_SetDistanceAisacControlId(IntPtr ex_3d_transceiver, ushort aisac_control_id) { }
		internal static void criAtomEx3dTransceiver_SetListenerBasedAzimuthAngleAisacControlId(IntPtr ex_3d_transceiver, ushort aisac_control_id) { }
		internal static void criAtomEx3dTransceiver_SetListenerBasedElevationAngleAisacControlId(IntPtr ex_3d_transceiver, ushort aisac_control_id) { }
		internal static void criAtomEx3dTransceiver_SetTransceiverOutputBasedAzimuthAngleAisacControlId(IntPtr ex_3d_transceiver, ushort aisac_control_id) { }
		internal static void criAtomEx3dTransceiver_SetTransceiverOutputBasedElevationAngleAisacControlId(IntPtr ex_3d_transceiver, ushort aisac_control_id) { }
		internal static void criAtomEx3dTransceiver_Set3dRegionHn(IntPtr ex_3d_transceiver, IntPtr ex_3d_region) { }
#endif
	}
	#endregion
}


/**
 * <summary>3D region object</summary>
 * <remarks>
 * <para header='Description'>An object for handling the 3D region.</para>
 * </remarks>
 */
public class CriAtomEx3dRegion : CriDisposable
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Config
	{
		public int reserved;
	}

	/**
	 * <summary>Creates a 3D region object</summary>
	 * <remarks>
	 * <para header='Description'>Creates a 3D region object.<br/></para>
	 * <para header='Note'>The library must be initialized before calling this function.<br/></para>
	 * </remarks>
	 */
	public CriAtomEx3dRegion()
	{
		Config config = new Config();
		this.handle = UnsafeNativeMethods.criAtomEx3dRegion_Create(ref config, IntPtr.Zero, 0);
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	/**
	 * <summary>Discards a 3D region object</summary>
	 * <remarks>
	 * <para header='Description'>Discards a 3D region object.<br/>
	 * When this function is called, all the resources allocated in the DLL when creating the 3D region object are released,<br/>
	 * and the 3D region object is disabled.</para>
	 * </remarks>
	 */
	public override void Dispose()
	{
		this.Dispose(true);
	}

	private void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		if (this.handle != IntPtr.Zero) {
			UnsafeNativeMethods.criAtomEx3dRegion_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}

		if (disposing) {
			GC.SuppressFinalize(this);
		}
	}

	~CriAtomEx3dRegion()
	{
		this.Dispose(false);
	}

	public IntPtr nativeHandle
	{
		get { return this.handle; }
	}

	private IntPtr handle = IntPtr.Zero;

	private static class UnsafeNativeMethods
	{
#if !CRIWARE_ENABLE_HEADLESS_MODE && CRIWARE_TRANSCEIVER_N_ELEVATIONANGLEAISAC_SUPPORT
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern IntPtr criAtomEx3dRegion_Create(ref Config config, IntPtr work, int work_size);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomEx3dRegion_Destroy(IntPtr ex_3d_region);
#else
		internal static IntPtr criAtomEx3dRegion_Create(ref Config config, IntPtr work, int work_size) { return IntPtr.Zero; }
		internal static void criAtomEx3dRegion_Destroy(IntPtr ex_3d_region) { }
#endif
	}
} // end of class

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
