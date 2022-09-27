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
 * <summary>3D sound source object</summary>
 * <remarks>
 * <para header='Description'>An object for handling the 3D sound source.<br/>
 * Used for 3D Positioning.<br/>
 * <br/>
 * You set the parameters and position information of the 3D sound source through the 3D sound source object.</para>
 * </remarks>
 */
public class CriAtomEx3dSource : CriDisposable
{
	/**
	 * <summary>Structure for 3D sound source configuration</summary>
	 * <remarks>
	 * <para header='Description'>This is the structure that contains the settings related to the initialization of the 3D sound source. <br/></para>
	 * </remarks>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Config {
		public bool enableVoicePriorityDecay;      /**< Whether to enable voice priority attenuation according to distance */
		public uint randomPositionListMaxLength;   /**< Gets the maximum number of elements that can be in the coordinates list used for position randomization of a 3D sound source */

		public Config(bool enableVoicePriorityDecay, uint randomPositionListMaxLength) {
			this.enableVoicePriorityDecay = enableVoicePriorityDecay;
			this.randomPositionListMaxLength = randomPositionListMaxLength;
		}
	}

	private uint currentRandomPositionListMaxLength = 0;

	/**
	 * <summary>Creates a 3D sound source object</summary>
	 * <param name='enableVoicePriorityDecay'>Enables voice priority attenuation according to the distance</param>
	 * <param name='randomPositionListMaxLength'>Gets the maximum number of elements that can be in the coordinates list used for position randomization of a 3D sound source</param>
	 * <remarks>
	 * <para header='Description'>Create a 3D sound source object based on the config for creating a 3D sound source object.<br/></para>
	 * <para header='Note'>The library must be initialized before calling this function.<br/></para>
	 * </remarks>
	 */
	public CriAtomEx3dSource(bool enableVoicePriorityDecay = false, uint randomPositionListMaxLength = 0)
	{
		Config config = new Config(enableVoicePriorityDecay, randomPositionListMaxLength);
		this.handle = criAtomEx3dSource_Create(ref config, IntPtr.Zero, 0);
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
		this.currentRandomPositionListMaxLength = randomPositionListMaxLength;
	}

	/**
	 * <summary>Discards a 3D sound source object</summary>
	 * <remarks>
	 * <para header='Description'>Discards a 3D source object.<br/>
	 * When this function is called, all the resources allocated in the DLL when creating the 3D sound source object are released.<br/>
	 * If there are any sounds being played by an AtomExPlayer with the 3D sound source object set,
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
			criAtomEx3dSource_Destroy(this.handle);
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
	 * <summary>Updates the 3D sound source</summary>
	 * <remarks>
	 * <para header='Description'>Updates the 3D sound source using the parameters set to the 3D sound source.<br/>
	 * This function updates all the parameters set to the 3D sound source.<br/>
	 * It is more efficient to change multiple parameters before performing update
	 * than to update using this function each time a parameter is changed.<br/></para>
	 * <para header='Note'>This function runs independently of the parameter update in the AtomExPlayer
	 * ( CriWare.CriAtomExPlayer::UpdateAll , CriAtomExPlayer::Update ).<br/>
	 * When you change the parameter of the 3D sound source, perform the update process using this function.</para>
	 * </remarks>
	 * <example><code>
	 *  ：
	 * // 音源の作成
	 * CriAtomEx3dSource source = new CriAtomEx3dSource();
	 *  ：
	 * // 音源の位置を設定
	 * source.SetPosition(0.0f, 0.0f, 1.0f);
	 *
	 * // 音源の速度を設定
	 * source.SetVelocity(1.0f, 0.0f, 0.0f);
	 *
	 * // 注意）この時点では音源の位置や速度はまだ変更されていません。
	 *
	 * // 変更の適用
	 * source.Update();
	 *  ：
	 * </code></example>
	 */
	public void Update()
	{
		criAtomEx3dSource_Update(this.handle);
	}

	/**
	 * <summary>Initializes 3D sound source parameters</summary>
	 * <remarks>
	 * <para header='Description'>Clears the parameters set to the 3D sound source and restores the initial values.<br/></para>
	 * <para header='Note'>To actually apply the cleared parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void ResetParameters()
	{
		criAtomEx3dSource_ResetParameters(this.handle);
	}

	/**
	 * <summary>Sets the position of the 3D sound source</summary>
	 * <param name='x'>X coordinate</param>
	 * <param name='y'>Y coordinate</param>
	 * <param name='z'>Z coordinate</param>
	 * <remarks>
	 * <para header='Description'>Sets the position of the 3D sound source.<br/>
	 * The position is used for calculating the distance attenuation and localization.<br/>
	 * The position is specified as a 3D vector.<br/>
	 * The unit of the position is determined by the distance factor
	 * of the 3D listener (set using the CriWare.CriAtomEx3dListener::SetDistanceFactor function).<br/>
	 * The default is (0.0f, 0.0f, 0.0f).<br/>
	 * Since the position cannot be set on the data, the value set using this function is always used.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetPosition(float x, float y, float z)
	{
		CriAtomEx.NativeVector position;
		position.x = x;
		position.y = y;
		position.z = z;
		criAtomEx3dSource_SetPosition(this.handle, ref position);
	}

	/**
	 * <summary>Sets the velocity of the 3D sound source</summary>
	 * <param name='x'>Velocity along X axis</param>
	 * <param name='y'>Velocity along Y axis</param>
	 * <param name='z'>Velocity along Z axis</param>
	 * <remarks>
	 * <para header='Description'>Sets the velocity of the 3D sound source.<br/>
	 * The velocity is used to calculate the Doppler effect.<br/>
	 * The velocity is specified as a 3D vector.<br/>
	 * The unit of velocity is the distance traveled per second.<br/>
	 * The unit of the distance is determined by the distance factor
	 * of the 3D listener (set using the CriWare.CriAtomEx3dListener::SetDistanceFactor function).
	 * The default is (0.0f, 0.0f, 0.0f).<br/>
	 * Since the velocity cannot be set on the data, the value set using this function is always used.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetVelocity(float x, float y, float z)
	{
		CriAtomEx.NativeVector velocity;
		velocity.x = x;
		velocity.y = y;
		velocity.z = z;
		criAtomEx3dSource_SetVelocity(this.handle, ref velocity);
	}

	/**
	 * <summary>Sets the orientation of the 3D sound source</summary>
	 * <param name='front'>Forward vector</param>
	 * <param name='top'>Upper vector</param>
	 * <remarks>
	 * <para header='Description'>Sets the orientation of the 3D sound source. <br/>
	 * The orientation is determined by a sound cone. <br/>
	 * The sound cone represents the direction in which the sound is emitted from the sound source. It is used to express the directivity of the sound. <br/>
	 * The orientation of the sound cone is specified by a 3D vector. The orientation vector is then normalized and used by the library internally.<br/>
	 * Since the direction of the sound cone cannot be set on the data side, calling this function is necessary to set the orientation. <br/>
	 * The default values are as follows. <br/>
	 * 	- Forward vector: (0.0f, 0.0f, 1.0f) <br/>
	 * 	- Upper vector: (0.0f, 1.0f, 0.0f) <br/></para>
	 * <para header='Note'>If you set the orientation of the sound cone, the upper vector is ignored and only the forward vector is used. <br/>
	 * Also, if you are using Ambisonics playback, Ambisonics will rotate according to the orientation specified by this function and the orientation of the listener. <br/></para>
	 * <para header='Note'>You must call the CriWare.CriAtomEx3dSource::Update function to actually apply the parameters you have set.<br/>
	 * Please note that sound cones cannot be applied to Ambisonics.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::SetConeParameter'/>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetOrientation(Vector3 front, Vector3 top)
	{
		CriAtomEx.NativeVector src_front;
		src_front.x = front.x;
		src_front.y = front.y;
		src_front.z = front.z;
		CriAtomEx.NativeVector src_top;
		src_top.x = top.x;
		src_top.y = top.y;
		src_top.z = top.z;
		criAtomEx3dSource_SetOrientation(this.handle, ref src_front, ref src_top);
	}


	/**
	 * \deprecated
	 * 削除予定の非推奨APIです。
	 * CriAtomEx3dSource.SetOrientationの使用を検討してください。
	 * <summary>Sets the orientation of the sound cone of the 3D sound source</summary>
	 * <param name='x'>Value in X direction</param>
	 * <param name='y'>Value in Y direction</param>
	 * <param name='z'>Value in Z direction</param>
	 * <remarks>
	 * <para header='Description'>Sets the orientation of the sound cone of the 3D sound source.<br/>
	 * The sound cone represents the direction in which sound is emitted from a sound source and is used to express the directionality of the sound.<br/>
	 * The orientation of the sound cone is specified using a 3D vector.<br/>
	 * The orientation vector that is set is normalized inside the library before being used.<br/>
	 * The default is (0.0f, 0.0f, -1.0f).<br/>
	 * As you cannot set the orientation of the sound cone on the data, the setting in this function is always used.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::SetConeParameter'/>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	[Obsolete("Use CriAtomEx3dSource.SetOrientation instead")]
	public void SetConeOrientation(float x, float y, float z)
	{
		CriAtomEx.NativeVector coneOrientation;
		coneOrientation.x = x;
		coneOrientation.y = y;
		coneOrientation.z = z;
		criAtomEx3dSource_SetConeOrientation(this.handle, ref coneOrientation);
	}

	/**
	 * <summary>Sets the sound cone parameters of the 3D sound source</summary>
	 * <param name='insideAngle'>Sound cone inside angle</param>
	 * <param name='outsideAngle'>Sound cone outside angle</param>
	 * <param name='outsideVolume'>Sound cone outside volume</param>
	 * <remarks>
	 * <para header='Description'>Sets the sound cone parameters of the 3D sound source.<br/>
	 * The sound cone represents the direction in which sound is emitted from a sound source and is used to express the directionality of the sound.<br/>
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
	 *  .
	 * If this function is called when the sound cone parameters are set on the data, the following settings are applied:<br/>
	 *  - Inside angle: Addition
	 *  - Outside angle: Addition
	 *  - Outside volume: Multiplication
	 *  .</para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetConeParameter(float insideAngle, float outsideAngle, float outsideVolume)
	{
		criAtomEx3dSource_SetConeParameter(this.handle, insideAngle, outsideAngle, outsideVolume);
	}

	/**
	 * <summary>Sets the minimum/maximum distances of the 3D sound source</summary>
	 * <param name='minDistance'>Minimum distance</param>
	 * <param name='maxDistance'>Maximum distance</param>
	 * <remarks>
	 * <para header='Description'>Sets the minimum/maximum distances of the 3D sound source.<br/>
	 * The minimum distance represents the distance at which the volume cannot be increased any further.<br/>
	 * The maximum distance is the distance with the lowest volume.<br/>
	 * The unit of the distance is determined by the distance factor
	 * of the 3D listener (set using the CriWare.CriAtomEx3dListener::SetDistanceFactor function).<br/>
	 * The default values when the library is initialized are as follows:<br/>
	 *  - Minimum distance: 0.0f
	 *  - Maximum distance: 0.0f
	 *  .
	 * When this function is called when the minimum/maximum distances are set on the data,
	 * the values on the data are overridden (ignored).<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetMinMaxDistance(float minDistance, float maxDistance)
	{
		criAtomEx3dSource_SetMinMaxAttenuationDistance(this.handle, minDistance, maxDistance);
	}

	/**
	 * <summary>Sets the interior Panning boundary distance of the 3D sound source</summary>
	 * <param name='sourceRadius'>3D sound source radius</param>
	 * <param name='interiorDistance'>Interior distance</param>
	 * <remarks>
	 * <para header='Description'>Sets the interior Panning boundary distance of the 3D sound source.<br/>
	 * The radius of the 3D sound source is the radius when the 3D sound source is considered as a sphere.<br/>
	 * Interior distance is the distance from the radius of the 3D sound source to which the interior Panning is applied.<br/>
	 * Within the radius of the 3D sound source, interior Panning is applied, but since the interior distance is treated as 0.0,
	 * the sound is played back from all speakers at the same volume.<br/>
	 * Within the interior distance, interior Panning is applied.<br/>
	 * Outside the interior distance, interior Panning is not applied and the sound is played from 1
	 * or 2 speakers closest to the sound source position.<br/>
	 * By default, the 3D sound source radius is set to 0.0f, and interior distance is set to 0.0f (depending on the minimum distance of the 3D sound source).</para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 * <seealso cref='CriAtomEx3dSource::SetMinMaxDistance'/>
	 */
	public void SetInteriorPanField(float sourceRadius, float interiorDistance)
	{
		criAtomEx3dSource_SetInteriorPanField(this.handle, sourceRadius, interiorDistance);
	}

	/**
	 * <summary>Sets the Doppler coefficient of the 3D sound source</summary>
	 * <param name='dopplerFactor'>Doppler coefficient</param>
	 * <remarks>
	 * <para header='Description'>Set the Doppler coefficient of the 3D sound source.<br/>
	 * The Doppler coefficient specifies the scale factor for exaggerating the Doppler effect
	 * calculated when the sound velocity of 340m/s.<br/>
	 * For example, if you specify 2.0f, the pitch is multiplied by 2 if the velocity of the sound is 340m/s. <br/>
	 * If you specify 0.0f, the Doppler effect is disabled.<br/>
	 * The default value when the library is initialized is 0.0f.<br/>
	 * When this function is called when the Doppler coefficient is set on the data,
	 * the value on the data is overridden (ignored).<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetDopplerFactor(float dopplerFactor)
	{
		criAtomEx3dSource_SetDopplerFactor(this.handle, dopplerFactor);
	}

	/**
	 * <summary>Sets the volume of the 3D sound source</summary>
	 * <param name='volume'>Volume</param>
	 * <remarks>
	 * <para header='Description'>Sets the volume of the 3D sound source.<br/>
	 * The volume of the 3D sound source affects only the volume related to localization (L,R,SL,SR),
	 * and does not affect the output level of LFE or the center.<br/>
	 * For the volume value, specify a real value in the range of 0.0f to 1.0f.<br/>
	 * The volume value is a scale factor for the amplitude of the sound data (the unit is not decibel).<br/>
	 * For example, if you specify 1.0f, the original sound is played at its unmodified volume.<br/>
	 * If you specify 0.5f, the sound is played at the volume by halving the amplitude (-6dB)
	 * of the original waveform.<br/>
	 * If you specify 0.0f, the sound is muted (silent).<br/>
	 * The default value when the library is initialized is 1.0f.<br/>
	 * When this function is called when the volume of the 3D sound source is set on the data,
	 * the value on the data is multiplied by this value.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetVolume(float volume)
	{
		criAtomEx3dSource_SetVolume(this.handle, volume);
	}

	/**
	 * <summary>Sets the maximum amount of change in angle AISAC control value</summary>
	 * <param name='maxDelta'>Maximum amount of change in angle AISAC control value</param>
	 * <remarks>
	 * <para header='Description'>Sets the maximum amount of change when the AISAC control value is changed by the angle AISAC.<br/>
	 * By changing the maximum amount of change to a lower value, the change of the AISAC control value by
	 * angle AISAC can be made smooth even if the relative angle between the sound source and the listener changes abruptly.<br/>
	 * For example, if (0.5f / 30.0f) is set, the change takes place over 30 frames
	 * when the angle changes from 0° to 180°.<br/>
	 * The default value is 1.0f (no limit).<br/>
	 * Since this parameter cannot be set on the data, the value set using this function is always used.<br/></para>
	 * <para header='Note'>To actually apply the set parameters,
	 * you need to call the CriWare.CriAtomEx3dSource::Update function.<br/>
	 * The maximum amount of change set by this function is applied
	 * only to the change of the angle AISAC control value calculated based on the localization angle. It does not affect the localization angle itself.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetMaxAngleAisacDelta(float maxDelta)
	{
		criAtomEx3dSource_SetMaxAngleAisacDelta(this.handle, maxDelta);
	}

	/**
	 * <summary>Sets the distance attenuation</summary>
	 * <param name='flag'>Whether to enable the distance attenuation (True: enable, False: disable)</param>
	 * <remarks>
	 * <para header='Description'>Sets whether to enable or disable the volume variation by distance attenuation.<br/>
	 * The default is True (enabled).<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 * <seealso cref='CriAtomEx3dSource::GetAttenuationDistanceSetting'/>
	 */
	public void SetAttenuationDistanceSetting(bool flag)
	{
		criAtomEx3dSource_SetAttenuationDistanceSetting(this.handle, flag);
	}

	/**
	 * <summary>Gets distance attenuation settings</summary>
	 * <returns>Distance attenuation setting (True: enable, False: disable)</returns>
	 * <remarks>
	 * <para header='Description'>Gets whether the volume variation by distance attenuation is enabled or disabled.<br/>
	 * The default is True (enabled).<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::SetAttenuationDistanceSetting'/>
	 */
	public bool GetAttenuationDistanceSetting()
	{
		return criAtomEx3dSource_GetAttenuationDistanceSetting(this.handle);
	}
	
	/**
	 * <summary>Settings related to position randomization of 3D sound sources</summary>
	 * <param name='config'>Config structure (nullable) used to randomize the positions of the 3D sound sources.</param>
	 * <remarks>
	 * <para header='Description'>Assigns the randomized position to the 3D sound source handle.
	 * Once this function is executed, the position of the voice will change randomly according to the original position information and settings. <br/></para>
	 * <para header='Note'>You can clear the settings by specifying null for the argument config.<br/></para>
	 * <para header='Note'>You must call the CriWare.CriAtomEx3dSource::Update function to actually apply the parameters you have set.<br/>
	 * This function does not affect the audio currently being played.<br/>
	 * The new parameters will be applied from the next audio playback.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::Randomize3dConfig'/>
	 */
	public void SetRandomPositionConfig(CriAtomEx.Randomize3dConfig? config)
	{
		if (config.HasValue) {
			if (config.Value.CalculationType == CriAtomEx.Randomize3dCalcType.None) {
				criAtomEx3dSource_SetRandomPositionConfig(this.handle, IntPtr.Zero);
			} else {
				var tempConfig = config.Value;
				criAtomEx3dSource_SetRandomPositionConfig(this.handle, ref tempConfig);
			}
		} else {
			criAtomEx3dSource_SetRandomPositionConfig(this.handle, IntPtr.Zero);
		}
	}

	/**
	 * <summary>Sets the list of coordinates to be used to randomize the position of a 3D sound source</summary>
	 * <param name='positionList'>Position coordinates list</param>
	 * <remarks>
	 * <para header='Description'>Specifies a list of possible coordinates.<br/>
	 * When the randomization of the 3D sound source's position is enabled, the coordinates will be randomly picked from that list.<br/></para>
	 * <para header='Note'>This parameter is referenced only when CriWare.CriAtomEx::Randomize3dCalcType::List <br/>
	 * is set as the calculation method for the coordinates.<br/>
	 * If any other calculation method is set, this parameter will be ignored.<br/></para>
	 * <para header='Note'>You must call the CriWare.CriAtomEx3dSource::Update function to actually apply the parameters you have set.<br/>
	 * The area passed to this function will no longer be referenced after the execution of the<br/>
	 * CriWare.CriAtomEx3dSource::SetRandomPositionList function is completed.<br/>
	 * Instead, the list of the coordinates will be saved internally.<br/>
	 * Therefore, setting a list whose length exceeds<br/>
	 * CriWare.CriAtomEx3dSource::Config::randomPositionListMaxLength will result in an error.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::Randomize3dConfig'/>
	 * <seealso cref='CriAtomEx3dSource::Config'/>
	 */
	public void SetRandomPositionList(Vector3[] positionList)
	{
		if (this.currentRandomPositionListMaxLength == 0) {
			Debug.LogError("[CRIWARE] The maxmium amount of random positions is set to 0. List will not be set.");
			return;
		}
		if (positionList.Length > this.currentRandomPositionListMaxLength) {
			Debug.LogError("[CRIWARE] Input list of positions is longer than maxmium length setting. List will not be set.");
			return;
		}
		
		CriAtomEx.NativeVector[] nativeList = new CriAtomEx.NativeVector[positionList.Length];
		for(int i = 0; i < positionList.Length; ++i) {
			nativeList[i] = new CriAtomEx.NativeVector(positionList[i]);
		}
		criAtomEx3dSource_SetRandomPositionList(this.handle, nativeList, (uint)positionList.Length);
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
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void Set3dRegion(CriAtomEx3dRegion region3d)
	{
		IntPtr region3dHandle = (region3d == null) ? IntPtr.Zero : region3d.nativeHandle;
		criAtomEx3dSource_Set3dRegionHn(this.handle, region3dHandle);
	}

	/**
	 * <summary>Sets the listener reference elevation AISAC control setting ID</summary>
	 * <param name='aisacControlId'>Listener reference elevation AISAC control ID</param>
	 * <remarks>
	 * <para header='Description'>Specifies the AISAC control ID linked to the elevation of the sound source seen from the listener.<br/>
	 * The listener reference elevation AISAC control ID set on the data is overridden by this function.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetListenerBasedElevationAngleAisacControlId(ushort aisacControlId)
	{
		criAtomEx3dSource_SetListenerBasedElevationAngleAisacControlId(this.handle, aisacControlId);
	}

	/**
	 * <summary>Sets the sound source reference azimuth AISAC control ID</summary>
	 * <param name='aisacControlId'>Sound source reference azimuth AISAC control ID</param>
	 * <remarks>
	 * <para header='Description'>Specifies the AISAC control ID linked to the azimuth of the listener as seen from the sound source.<br/>
	 * The sound source reference azimuth AISAC control ID set on the data is overridden by this function.<br/></para>
	 * <para header='Note'>To actually apply the set parameters, you need to call the CriWare.CriAtomEx3dSource::Update function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetSourceBasedElevationAngleAisacControlId(ushort aisacControlId)
	{
		criAtomEx3dSource_SetSourceBasedElevationAngleAisacControlId(this.handle, aisacControlId);
	}

	/**
	 * <summary>Sets the Distance AISAC Control ID</summary>
	 * <param name='aisacControlId'>Sound source reference azimuth AISAC control ID</param>
	 * <remarks>
	 * <para header='Description'>Specifies the AISAC control ID to be linked to the distance attenuation between the minimum and maximum distances.<br/>
	 * If an AISAC control ID is set with this function, the default distance attenuation is disabled. <br/>
	 * The distance AISAC control ID set on the data side is overridden by this function.<br/></para>
	 * <para header='Note'>To actually apply parameter changes, it is necessary to call the CriAtomEx3dSource.Update function.<br/>
	 * When this function is executed, any previous changes due to the AISAC control ID set to the 3D sound source will be invalidated,<br/>
	 * while the parameters changes previously applied to the audio being played will stay unchanged.<br/>
	 * Therefore, it is recommended to execute this function when no audio associated with the specified 3D sound source is being played.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx3dSource::Update'/>
	 */
	public void SetDistanceAisacControlId(ushort aisacControlId)
	{
		criAtomEx3dSource_SetDistanceAisacControlId(this.handle, aisacControlId);
	}

	#region Internal Members

	~CriAtomEx3dSource()
	{
		this.Dispose(false);
	}

	private IntPtr handle = IntPtr.Zero;

	#endregion

	#region DLL Import

#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomEx3dSource_Create(ref Config config, IntPtr work, int work_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_Destroy(IntPtr ex_3d_source);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_Update(IntPtr ex_3d_source);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_ResetParameters(IntPtr ex_3d_source);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetPosition(IntPtr ex_3d_source, ref CriAtomEx.NativeVector position);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetVelocity(IntPtr ex_3d_source, ref CriAtomEx.NativeVector velocity);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetOrientation(IntPtr ex_3d_source, ref CriAtomEx.NativeVector front, ref CriAtomEx.NativeVector top);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetConeOrientation(IntPtr ex_3d_source, ref CriAtomEx.NativeVector cone_orient);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetConeParameter(IntPtr ex_3d_source, float inside_angle, float outside_angle, float outside_volume);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetMinMaxAttenuationDistance(IntPtr ex_3d_source, float min_distance, float max_distance);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetInteriorPanField(IntPtr ex_3d_source, float source_radius, float interior_distance);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetDopplerFactor(IntPtr ex_3d_source, float doppler_factor);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetVolume(IntPtr ex_3d_source, float volume);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetMaxAngleAisacDelta(IntPtr ex_3d_source, float max_delta);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetAttenuationDistanceSetting(IntPtr ex_3d_source, bool flag);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomEx3dSource_GetAttenuationDistanceSetting(IntPtr ex_3d_source);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetRandomPositionConfig(IntPtr ex_3d_source, ref CriAtomEx.Randomize3dConfig config);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetRandomPositionConfig(IntPtr ex_3d_source, IntPtr config);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetRandomPositionList(IntPtr ex_3d_source, CriAtomEx.NativeVector[] position_list, uint length);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetDistanceAisacControlId(IntPtr ex_3d_source, ushort aisac_control_id);
#else
	private static IntPtr criAtomEx3dSource_Create(ref Config config, IntPtr work, int work_size) { return IntPtr.Zero; }
	private static void criAtomEx3dSource_Destroy(IntPtr ex_3d_source) { }
	private static void criAtomEx3dSource_Update(IntPtr ex_3d_source) { }
	private static void criAtomEx3dSource_ResetParameters(IntPtr ex_3d_source) { }
	private static void criAtomEx3dSource_SetPosition(IntPtr ex_3d_source, ref CriAtomEx.NativeVector position) { }
	private static void criAtomEx3dSource_SetVelocity(IntPtr ex_3d_source, ref CriAtomEx.NativeVector velocity) { }
	private static void criAtomEx3dSource_SetOrientation(IntPtr ex_3d_source, ref CriAtomEx.NativeVector front, ref CriAtomEx.NativeVector top) { }
	private static void criAtomEx3dSource_SetConeOrientation(IntPtr ex_3d_source, ref CriAtomEx.NativeVector cone_orient) { }
	private static void criAtomEx3dSource_SetConeParameter(IntPtr ex_3d_source, float inside_angle, float outside_angle, float outside_volume) { }
	private static void criAtomEx3dSource_SetMinMaxAttenuationDistance(IntPtr ex_3d_source, float min_distance, float max_distance) { }
	private static void criAtomEx3dSource_SetInteriorPanField(IntPtr ex_3d_source, float source_radius, float interior_distance) { }
	private static void criAtomEx3dSource_SetDopplerFactor(IntPtr ex_3d_source, float doppler_factor) { }
	private static void criAtomEx3dSource_SetVolume(IntPtr ex_3d_source, float volume) { }
	private static void criAtomEx3dSource_SetMaxAngleAisacDelta(IntPtr ex_3d_source, float max_delta) { }
	private static void criAtomEx3dSource_SetAttenuationDistanceSetting(IntPtr ex_3d_source, bool flag) { }
	private static bool criAtomEx3dSource_GetAttenuationDistanceSetting(IntPtr ex_3d_source) { return false; }
	private static void criAtomEx3dSource_SetRandomPositionConfig(IntPtr ex_3d_source, ref CriAtomEx.Randomize3dConfig config) { }
	private static void criAtomEx3dSource_SetRandomPositionConfig(IntPtr ex_3d_source, IntPtr config) { }
	private static void criAtomEx3dSource_SetRandomPositionList(IntPtr ex_3d_source, CriAtomEx.NativeVector[] position_list, uint length) { }
	private static void criAtomEx3dSource_SetDistanceAisacControlId(IntPtr ex_3d_source, ushort aisac_control_id) { }
#endif

#if !CRIWARE_ENABLE_HEADLESS_MODE && CRIWARE_TRANSCEIVER_N_ELEVATIONANGLEAISAC_SUPPORT
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_Set3dRegionHn(IntPtr ex_3d_source, IntPtr ex_3d_region);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetListenerBasedElevationAngleAisacControlId(IntPtr ex_3d_source, ushort aisac_control_id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx3dSource_SetSourceBasedElevationAngleAisacControlId(IntPtr ex_3d_source, ushort aisac_control_id);
#else
	private static void criAtomEx3dSource_Set3dRegionHn(IntPtr ex_3d_source, IntPtr ex_3d_region) { }
	private static void criAtomEx3dSource_SetListenerBasedElevationAngleAisacControlId(IntPtr ex_3d_source, ushort aisac_control_id) { }
	private static void criAtomEx3dSource_SetSourceBasedElevationAngleAisacControlId(IntPtr ex_3d_source, ushort aisac_control_id) { }
#endif

	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
