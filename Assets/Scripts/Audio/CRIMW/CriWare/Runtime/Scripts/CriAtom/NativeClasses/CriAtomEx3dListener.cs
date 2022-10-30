/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !UNITY_WEBGL
	#define CRIWARE_TRANSCEIVER_N_ELEVATIONANGLEAISAC_SUPPORT
#endif

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>3D listener</summary>
 * <remarks>
 * <para header='Description'>An object for handling the 3D listener.<br/>
 * Used for 3D Positioning.<br/>
 * <br/>
 * You set the parameters and position information of the 3D listener through the 3D listener object.</para>
 * </remarks>
 */
public class CriAtomEx3dListener : CriDisposable
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Config {
		public int reserved;
	}

	/**
	 * <summary>Creates a 3D listener</summary>
	 * <remarks>
	 * <para header='Description'>Creates a 3D listener object.<br/></para>
	 * <para header='Note'>The library must be initialized before calling this function.<br/></para>
	 * </remarks>
	 */
	public CriAtomEx3dListener()
	{
		Config config = new Config();
		this.handle = criAtomEx3dListener_Create(ref config, IntPtr.Zero, 0);
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	/**
	 * <summary>Discards the 3D listener object</summary>
	 * <remarks>
	 * <para header='Description'>Discards a 3D listener object.<br/>
	 * When this function is called, all the resources allocated in the DLL when creating the 3D listener are released.<br/>
	 * If there are any sounds being played by an AtomExPlayer with the 3D listener object set,
	 * either stop those sounds or discard the AtomExPlayer before calling this function.</para>
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
			criAtomEx3dListener_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}

		if (disposing) {
			GC.SuppressFinalize(this);
		}
	}

	public IntPtr nativeHandle
	{
		get {return this.handle;}
	}

	/**
	 * <summary>Updates the 3D listener</summary>
	 * <remarks>
	 * <para header='Description'>Updates the 3D listener using the parameters set in the 3D listener.<br/>
	 * This function updates all the parameters set in the 3D listener.
	 * It is more efficient to change multiple parameters before performing update
	 * than to update using this function each time a parameter is changed.</para>
	 * <para header='Note'>This function runs independently of the parameter update in the AtomExPlayer
	 * ( CriWare.CriAtomExPlayer::UpdateAll , CriWare.CriAtomExPlayer::Update ).<br/>
	 * When you change the parameter of the 3D listener, perform the update process using this function.</para>
	 * </remarks>
	 * <example><code>
	 *  ：
	 * // リスナーの作成
	 * CriAtomExListener listener = new CriAtomEx3dListener();
	 *  ：
	 * // リスナー位置の設定
	 * listener.SetPosition(0.0f, 0.0f, 1.0f);
	 *
	 * // リスナー速度の設定
	 * listener.SetVelocity(1.0f, 0.0f, 0.0f);
	 *
	 * // 注意）この時点ではリスナーの位置や速度はまだ変更されていません。
	 *
	 * // 変更の適用
	 * listener.Update();
	 *  ：
	 * </code></example>
	 */
	public void Update()
	{
		if (this.handle != IntPtr.Zero)
			criAtomEx3dListener_Update(this.handle);
	}

	/**
	 * <summary>Initializes 3D sound source parameters</summary>
	 * <remarks>
	 * <para header='Description'>Clears the parameters set in the 3D listener and restores the initial values.<br/></para>
	 * <para header='Note'>To actually apply the cleared parameters, you need to call the CriWare.CriAtomEx3dListener::Update function.</para>
	 * </remarks>
	 */
	public void ResetParameters()
	{
		if (this.handle != IntPtr.Zero)
			criAtomEx3dListener_ResetParameters(this.handle);
	}

	/**
	 * <summary>Sets the position of the 3D listener</summary>
	 * <param name='x'>X coordinate</param>
	 * <param name='y'>Y coordinate</param>
	 * <param name='z'>Z coordinate</param>
	 * <remarks>
	 * <para header='Description'>Sets the position of the 3D listener.<br/>
	 * The position is used for calculating the distance attenuation and localization.<br/>
	 * The position is specified as a 3D vector.<br/>
	 * The unit of the position is determined by the distance factor
	 * of the 3D listener(set using the CriWare.CriAtomEx3dListener::SetDistanceFactor function).<br/>
	 * The default is (0.0f, 0.0f, 0.0f).<br/>
	 * Since the position cannot be set on the data, the value set using this function is always used.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dListener::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dListener::Update'/>
	 */
	public void SetPosition(float x, float y, float z)
	{
		if (this.handle == IntPtr.Zero)
			return;

		CriAtomEx.NativeVector position;
		position.x = x;
		position.y = y;
		position.z = z;
		criAtomEx3dListener_SetPosition(this.handle, ref position);
	}

	/**
	 * <summary>Sets the velocity of the 3D listener</summary>
	 * <param name='x'>Velocity along X axis</param>
	 * <param name='y'>Velocity along Y axis</param>
	 * <param name='z'>Velocity along Z axis</param>
	 * <remarks>
	 * <para header='Description'>Sets the velocity of the 3D listener.<br/>
	 * The velocity is used to calculate the Doppler effect.<br/>
	 * The velocity is specified as a 3D vector.<br/>
	 * The unit of velocity is the distance traveled per second.<br/>
	 * The unit of the distance is determined by the distance factor
	 * of the 3D listener (set using the CriWare.CriAtomEx3dListener::SetDistanceFactor function).<br/>
	 * The default is (0.0f, 0.0f, 0.0f).<br/>
	 * Since the velocity cannot be set on the data, the value set using this function is always used.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dListener::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dListener::Update'/>
	 */
	public void SetVelocity(float x, float y, float z)
	{
		if (this.handle == IntPtr.Zero)
			return;

		CriAtomEx.NativeVector velocity;
		velocity.x = x;
		velocity.y = y;
		velocity.z = z;
		criAtomEx3dListener_SetVelocity(this.handle, ref velocity);
	}

	/**
	 * <summary>Sets the orientation of the 3D listener</summary>
	 * <param name='fx'>Value of the forward vector in the X direction</param>
	 * <param name='fy'>Value of the forward vector in the Y direction</param>
	 * <param name='fz'>Value of the forward vector in the Z direction</param>
	 * <param name='ux'>Value of the upper vector in the X direction</param>
	 * <param name='uy'>Value of the upper vector in the Y direction</param>
	 * <param name='uz'>Value of the upper vector in the Z direction</param>
	 * <remarks>
	 * <para header='Description'>Sets the orientation of the 3D listener using the forward and upward vectors.<br/>
	 * The orientation is specified using a 3D vector.<br/>
	 * The orientation vector that is set is normalized inside the library before being used.<br/>
	 * The default values are as follows:<br/>
	 *  - Forward vector: (0.0f, 0.0f, 1.0f)
	 *  - Upward vector: (0.0f, 1.0f, 0.0f)
	 *  .
	 * As you cannot set the orientation of the listener on the data, the setting in this function is always used.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dListener::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dListener::Update'/>
	 */
	public void SetOrientation(float fx, float fy, float fz, float ux, float uy, float uz)
	{
		if (this.handle == IntPtr.Zero)
			return;

		CriAtomEx.NativeVector front, top;
		front.x = fx;
		front.y = fy;
		front.z = fz;
		top.x = ux;
		top.y = uy;
		top.z = uz;
		criAtomEx3dListener_SetOrientation(this.handle, ref front, ref top);
	}

	/**
	 * \deprecated
	 * 削除予定の非推奨APIです。
	 * CriWareErrorHandler.OnCallback event の使用を検討してください。
	 * <summary>Sets the distance factor of the 3D listener</summary>
	 * <param name='distanceFactor'>Distance factor</param>
	 * <remarks>
	 * <para header='Description'>Sets the distance factor of the 3D listener.<br/>
	 * This factor is used to calculate the Doppler effect.<br/>
	 * For example, if you set distance_factor to 0.1f, the speed 1.0f is treated as 10 meters.<br/>
	 * Possible value for distanceFactor is 0 or a value greater than 0.0f.
	 * The default is 1.0f.<br/>
	 * As you cannot set the distance factor of the listener on the data, the setting in this function is always used.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dListener::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dListener::Update'/>
	 */
	[Obsolete("Use SetDopplerMultiplier instead")]
	public void SetDistanceFactor(float distanceFactor)
	{
		if (this.handle == IntPtr.Zero)
			return;

		if (distanceFactor < 0f) {
			UnityEngine.Debug.LogError("[CRIWARE] Invalid value for distanceFactor. Value >= 0f required.");
		} else if (distanceFactor == 0f) {
			criAtomEx3dListener_SetDopplerMultiplier(this.handle, 0f);
		} else {
			criAtomEx3dListener_SetDopplerMultiplier(this.handle, 1.0f / distanceFactor);
		}
	}

	/**
	 * <summary>Sets the Doppler scale factor of the 3D listener</summary>
	 * <param name='dopplerMultiplier'>Doppler scale factor</param>
	 * <remarks>
	 * <para header='Description'>Sets the Doppler scale factor of the 3D listener. This scale factor is used to calculate the Doppler effect.<br/>
	 * For example, if you set dopplerMultiplier to 10.0f, the Doppler effect will be 10 times more intense than usual.<br/>
	 * Possible value for dopplerMultiplier is 0 or a value greater than 0.0f.
	 * The default is 1.0f.<br/>
	 * As you cannot set the Doppler scale value to the listener on the data, the setting in this function is always used.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dListener::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dListener::Update'/>
	 */
	public void SetDopplerMultiplier(float dopplerMultiplier)
	{
		if (this.handle == IntPtr.Zero)
			return;

		if (dopplerMultiplier < 0f) {
			UnityEngine.Debug.LogError("[CRIWARE] Invalid value for dopplerMultiplier. Value >= 0f required.");
			return;
		}

		criAtomEx3dListener_SetDopplerMultiplier(this.handle, dopplerMultiplier);
	}

	/**
	 * <summary>Sets the focus point of the 3D listener</summary>
	 * <remarks>
	 * <para header='Description'>Set the focus point of the 3D listener.<br/>
	 * When performing the 3D Positioning,
	 * by setting the focus point, the listener's position and the focus point are connected by a straight line,
	 * and the microphone can be moved on that line.<br/>
	 * For example, by keeping the listener synchronized with the camera and setting the focus point on the position of the main character,
	 * you can flexibly express/adjust objective and subjective representation depending on the situation.<br/>
	 * Unlike the real-world microphone, the microphone that can be moved between the listener's position and the focus point
	 * has separate distance sensor (for distance attenuation calculation) and orientation sensor (for localization calculation).<br/>
	 * By operating these independently, a representation is possible such as
	 * "I want to focus on the main character, so the distance attenuation should be based on the character position."
	 * or "I want to match the localization with the screen, so the localization calculation should be based on the camera position"<br/>
	 * The default is (0.0f, 0.0f, 0.0f). In situations where the focus level of the distance sensor or orientation sensor is not set,
	 * it is not necessary to set the focus point.
	 * In that case, all the 3D Positioning calculations are done based on the listener position as usual.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dListener::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dListener::Update'/>
	 * <seealso cref='CriAtomEx3dListener::SetDistanceFocusLevel'/>
	 * <seealso cref='CriAtomEx3dListener::SetDirectionFocusLevel'/>
	 */
	public void SetFocusPoint(float x, float y, float z)
	{
		if (this.handle == IntPtr.Zero)
			return;

		CriAtomEx.NativeVector focus;
		focus.x = x;
		focus.y = y;
		focus.z = z;
		criAtomEx3dListener_SetFocusPoint(this.handle, ref focus);
	}

	/**
	 * <summary>Sets the distance sensor focus level</summary>
	 * <param name='distanceFocusLevel'>Distance sensor focus level</param>
	 * <remarks>
	 * <para header='Description'>Sets the focus level of the distance sensor.<br/>
	 * The distance sensor represents the position that is the basis of the distance attenuation calculation among the 3D Positioning calculations.
	 * It is treated as a microphone that ignores localization and senses only the degree of distance attenuation.<br/>
	 * The focus level indicates how close the sensor (microphone) can be to the focus point.
	 * The sensor (microphone) can be moved on a straight line connecting the listener position and the focus point,
	 * with 0.0f being the listener position and 1.0f being the same position as the focus point.<br/>
	 * For example, by setting the focus level of the distance sensor to 1.0f and the focus level of the direction sensor to 0.0f,
	 * distance attenuation is applied based on the focus point, and localization is determined based on the listener position.<br/>
	 * The default is 0.0f. In the situation where the focus level of the distance sensor or direction sensor is not set,
	 * all 3D Positioning calculations are done based on the listener position as before.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dListener::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dListener::Update'/>
	 * <seealso cref='CriAtomEx3dListener::SetFocusPoint'/>
	 * <seealso cref='CriAtomEx3dListener::SetDirectionFocusLevel'/>
	 */
	public void SetDistanceFocusLevel(float distanceFocusLevel)
	{
		if (this.handle != IntPtr.Zero)
			criAtomEx3dListener_SetDistanceFocusLevel(this.handle, distanceFocusLevel);
	}

	/**
	 * <summary>Sets the focus level the direction sensor</summary>
	 * <param name='directionFocusLevel'>The focus level the direction sensor</param>
	 * <remarks>
	 * <para header='Description'>Sets the focus level of the direction sensor.<br/>
	 * The direction sensor represents the position that serves as the reference for localization calculation in the 3D Positioning calculation.
	 * It is treated as a microphone that ignores distance attenuation and senses only localization.<br/>
	 * For the orientation of the direction sensor, the orientation of the listener (set by the SetOrientation function) is used as is.<br/>
	 * The focus level indicates how close the sensor (microphone) can be to the focus point.
	 * The sensor (microphone) can be moved on a straight line connecting the listener position and the focus point,
	 * with 0.0f being the listener position and 1.0f being the same position as the focus point.<br/>
	 * For example, by setting the focus level of the distance sensor to 1.0f and the focus level of the direction sensor to 0.0f,
	 * distance attenuation is applied based on the focus point, and localization is determined based on the listener position.<br/>
	 * The default is 0.0f. In the situation where the focus level of the distance sensor or direction sensor is not set,
	 * all 3D Positioning calculations are done based on the listener position as before.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dListener::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dListener::Update'/>
	 * <seealso cref='CriAtomEx3dListener::SetFocusPoint'/>
	 * <seealso cref='CriAtomEx3dListener::SetDistanceFocusLevel'/>
	 */
	public void SetDirectionFocusLevel(float directionFocusLevel)
	{
		if (this.handle != IntPtr.Zero)
			criAtomEx3dListener_SetDirectionFocusLevel(this.handle, directionFocusLevel);
	}

	/**
	 * <summary>Sets the 3D region</summary>
	 * <remarks>
	 * <para header='Description'>Sets the 3D region to the 3D listener.</para>
	 * <para header='Note'>If the regions set to the 3D sound source and 3D listener set to the same ExPlayer are different,<br/>
	 * and if there is no 3D Transceiver that has the same region as the 3D sound source, the sound is muted.<br/>
	 * To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dListener::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dRegion::Create'/>
	 * <seealso cref='CriAtomEx3dListener::Update'/>
	 */
	public void Set3dRegion(CriAtomEx3dRegion region3d)
	{
		if (this.handle == IntPtr.Zero)
			return;

		IntPtr region3dHandle = (region3d == null) ? IntPtr.Zero : region3d.nativeHandle;
		criAtomEx3dListener_Set3dRegionHn(this.handle, region3dHandle);
	}

	#region Internal Members

	~CriAtomEx3dListener()
	{
		this.Dispose(false);
	}

	private IntPtr handle = IntPtr.Zero;

	#endregion

	#region DLL Import

#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomEx3dListener_Create(ref Config config, IntPtr work, int work_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_Destroy(IntPtr ex_3d_listener);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_Update(IntPtr ex_3d_listener);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_ResetParameters(IntPtr ex_3d_listener);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_SetPosition(IntPtr ex_3d_listener, ref CriAtomEx.NativeVector position);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_SetVelocity(IntPtr ex_3d_listener, ref CriAtomEx.NativeVector velocity);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_SetOrientation(IntPtr ex_3d_listener, ref CriAtomEx.NativeVector front, ref CriAtomEx.NativeVector top);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_SetDistanceFactor(IntPtr ex_3d_listener, float distance_factor);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_SetDopplerMultiplier(IntPtr ex_3d_listener, float doppler_multiplier);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_SetFocusPoint(IntPtr ex_3d_listener, ref CriAtomEx.NativeVector focus_point);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_SetDistanceFocusLevel(IntPtr ex_3d_listener, float distance_focus_level);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_SetDirectionFocusLevel(IntPtr ex_3d_listener, float direction_focus_level);
#else
	private static IntPtr criAtomEx3dListener_Create(ref Config config, IntPtr work, int work_size) { return IntPtr.Zero; }
	private static void criAtomEx3dListener_Destroy(IntPtr ex_3d_listener) { }
	private static void criAtomEx3dListener_Update(IntPtr ex_3d_listener) { }
	private static void criAtomEx3dListener_ResetParameters(IntPtr ex_3d_listener) { }
	private static void criAtomEx3dListener_SetPosition(IntPtr ex_3d_listener, ref CriAtomEx.NativeVector position) { }
	private static void criAtomEx3dListener_SetVelocity(IntPtr ex_3d_listener, ref CriAtomEx.NativeVector velocity) { }
	private static void criAtomEx3dListener_SetOrientation(IntPtr ex_3d_listener, ref CriAtomEx.NativeVector front, ref CriAtomEx.NativeVector top) { }
	private static void criAtomEx3dListener_SetDistanceFactor(IntPtr ex_3d_listener, float distance_factor) { }
	private static void criAtomEx3dListener_SetDopplerMultiplier(IntPtr ex_3d_listener, float doppler_multiplier) { }
	private static void criAtomEx3dListener_SetFocusPoint(IntPtr ex_3d_listener, ref CriAtomEx.NativeVector focus_point) { }
	private static void criAtomEx3dListener_SetDistanceFocusLevel(IntPtr ex_3d_listener, float distance_focus_level) { }
	private static void criAtomEx3dListener_SetDirectionFocusLevel(IntPtr ex_3d_listener, float direction_focus_level) { }
#endif

#if !CRIWARE_ENABLE_HEADLESS_MODE && CRIWARE_TRANSCEIVER_N_ELEVATIONANGLEAISAC_SUPPORT
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dListener_Set3dRegionHn(IntPtr ex_3d_listener, IntPtr ex_3d_region);
#else
	private static void criAtomEx3dListener_Set3dRegionHn(IntPtr ex_3d_listener, IntPtr ex_3d_region) { }
#endif

	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
