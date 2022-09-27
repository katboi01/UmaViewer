using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CriWare {
	public abstract class CriAtomSourceBase : CriMonoBehaviour
	{
		#region Enumlators
		/**
		 * <summary>A value indicating the playback status of CriAtomSource.</summary>
		 * <remarks>
		 * <para header='Description'>Information can be obtained using the CriAtomSource::status property.</para>
		 * </remarks>
		 */
		public enum Status
		{
			Stop,       /**< Stopped */
			Prep,       /**< Preparing for playback */
			Playing,    /**< Playing */
			PlayEnd,    /**< Playback completed */
			Error       /**< Error occurred */
		}
		#endregion

		#region Variables
		/**
		 * <summary>The CriAtomExPlayer used internally.</summary>
		 * <remarks>
		 * <para header='Description'>If you want to control CriAtomExPlayer directly, get CriAtomExPlayer from this property.</para>
		 * </remarks>
		 */
		public CriAtomExPlayer player { protected set; get; }

		/**
		 * <summary>This is the CriAtomEx3dSource used internally.</summary>
		 * <remarks>
		 * <para header='Description'>If you want to control CriAtomEx3dSource directly, please get the CriAtomEx3dSource handle from this property.</para>
		 * </remarks>
		 */
		public CriAtomEx3dSource source { protected set; get; }

		protected bool initialized = false;
		protected Vector3 lastPosition;
		protected bool hasValidPosition = false;
		private CriAtomRegion currentRegion = null;
		private CriAtomListener currentListener = null;

		[SerializeField]
		private bool _playOnStart = false;
		[SerializeField]
		private CriAtomRegion _regionOnStart = null;
		[SerializeField]
		private CriAtomListener _listenerOnStart = null;

		// Parameters
		[SerializeField]
		private bool _use3dPositioning = true;
		[SerializeField]
		private bool _freezeOrientation = false;
		[SerializeField]
		private bool _loop = false;
		[SerializeField]
		private float _volume = 1.0f;
		[SerializeField]
		private float _pitch = 0.0f;
		[SerializeField]
		private bool _androidUseLowLatencyVoicePool = false;
		[SerializeField]
		private bool need_to_player_update_all = true;
		[SerializeField]
		private bool _use3dRandomization = false;
		[SerializeField]
		private uint _randomPositionListMaxLength = 0;
		[SerializeField]
		private CriAtomEx.Randomize3dConfig randomize3dConfig = new CriAtomEx.Randomize3dConfig(0);
		#endregion

		#region Properties

		/**
		 * <summary>Sets/gets whether to play at the start of execution.</summary>
		 * <remarks>
		 * <para header='Description'>If set to True, playback starts when starting the execution.</para>
		 * <para header='Note'>The timing when playback starts is when the MonoBehaviour::Start function is called.</para>
		 * <para header='Note'>Be sure to specify the Cue Sheet name when playing a Cue at the start of execution
		 * using this flag on a platform where asynchronous ACB loading is enabled such as WebGL.<br/>
		 * If not specified, the playback of Cue fails since the system cannot identify
		 * the Cue Sheet for which it should wait for loading.</para>
		 * </remarks>
		 */
		public bool playOnStart
		{
			get { return this._playOnStart; }
			set { this._playOnStart = value; }
		}

		/**
		 * <summary>Sets whether to use the 3D Positioning.</summary>
		 * <remarks>
		 * <para header='Description'><br/>
		 * By default, the use of 3D Positioning is enabled.<br/>
		 * This parameter can be switched at any time.<br/></para>
		 * </remarks>
		 */
		public bool use3dPositioning
		{
			set
			{
				this._use3dPositioning = value;
				if (this.player != null)
				{
					this.player.Set3dSource(this.use3dPositioning ? this.source : null);
					this.SetNeedToPlayerUpdateAll();
				}
			}
			get { return this._use3dPositioning; }
		}

		/**
		 * <summary>Sets whether to fix the orientation of the 3D sound source.</summary>
		 * <remarks>
		 * <para header='Description'><br/>
		 * By default, the orientation of the 3D sound source corresponds to the orientation of the GameObject. <br/>
		 * If this parameter is set to true, the direction of the sound source will be fixed to the current value. <br/></para>
		 * </remarks>
		 */
		public bool freezeOrientation
		{
			get { return this._freezeOrientation; }
			set { this._freezeOrientation = value; }
		}

		/**
		 * <summary>Sets whether to use randomization of the position of the 3D sound source.</summary>
		 * <remarks>
		 * <para header='Description'><br/>
		 * By default, the randomization of a sound source position is disabled.<br/>
		 * This parameter can be switched at any time. <br/></para>
		 * </remarks>
		 */
		public bool use3dRandomization
		{
			set
			{
				this._use3dRandomization = value;
				if (this.source != null)
				{
					if (this._use3dRandomization)
					{
						this.source.SetRandomPositionConfig(this.randomize3dConfig);
					}
					else
					{
						this.source.SetRandomPositionConfig(null);
					}
				}
			}
			get { return this._use3dRandomization; }
		}

		/**
		 * <summary>Sets the maximum number of elements that can be in the coordinates list used for position randomization of a 3D sound source</summary>
		 * <remarks>
		 * <para header='Description'><br/>
		 * By default, the maximum number of elements in the list is 0. <br/>
		 * This parameter can only be changed before the Awake method of the CriAtomSource is executed. <br/></para>
		 * </remarks>
		 */
		public uint randomPositionListMaxLength
		{
			set
			{
				if (initialized)
				{
					Debug.LogError("[CRIWARE] Max length of random position list cannot be changed after initialization of the CriAtomSource.", this);
					return;
				}
				this._randomPositionListMaxLength = value;
			}
			get
			{
				return this._randomPositionListMaxLength;
			}
		}

		/**
		 * <summary>Sets/gets 3D region of the sound source</summary>
		 * <remarks>
		 * <para header='Note'>If the 3D Positioning is disabled, you cannot set the region.</para>
		 * </remarks>
		 */
		public CriAtomRegion region3d
		{
			get { return this.currentRegion; }
			set
			{
				if (this.currentRegion == value)
				{
					return;
				}
				if (this._use3dPositioning == false)
				{
					Debug.LogWarning("[CRIWARE] Cannot set 3D Region on audio source with 3d positioning disabled.");
					return;
				}
				/* Remove the reference frome the current region  */
				if (this.currentRegion != null)
				{
					this.currentRegion.referringSources.Remove(this);
				}
				CriAtomEx3dRegion regionHandle = (value == null) ? null : value.region3dHn;
				if (this.source != null)
				{
					this.source.Set3dRegion(regionHandle);
					this.source.Update();
					this.currentRegion = value;
					/* Seup a reference from a new region */
					if (this.currentRegion != null)
					{
						this.currentRegion.referringSources.Add(this);
					}
				}
				else
				{
					Debug.LogError("[CRIWARE] Internal: 3D Positioning is not initialized correctly.");
					this.currentRegion = null;
				}
			}
		}

		/**
		 * <summary>Gets and sets the listener of the sound source</summary>
		 * <remarks>
		 * <para header='Note'>The listener closest to the sound source will be used if no listener is specified.</para>
		 * <para header='Note'>If the 3D Positioning is disabled, you cannot set the listener.</para>
		 * </remarks>
		 */
		public CriAtomListener listener
		{
			get { return currentListener; }
			set
			{
				if (this._use3dPositioning == false)
				{
					Debug.LogWarning("[CRIWARE] Cannot set 3D Listener on audio source with 3d positioning disabled.");
					return;
				}
				currentListener = value;
				player.Set3dListener(value == null ? null : value.nativeListener);
			}
		}

		/**
		 * <summary>Sets the initial region.</summary>
		 * <remarks>
		 * <para header='Description'>Set the region to be applied when Start is invoked.<br/>
		 * Applied only when 3D Positioning is enabled.<br/>
		 * It is not applied if empty (null).<br/></para>
		 * </remarks>
		 */
		public CriAtomRegion regionOnStart
		{
			get { return this._regionOnStart; }
			set { this._regionOnStart = value; }
		}

		/**
		 * <summary>Sets the initial listener.</summary>
		 * <remarks>
		 * <para header='Description'>Sets the listener to be applied when Start is executed. <br/>
		 * Applies only when 3D positioning is enabled. <br/>
		 * Does not apply if empty (null). <br/></para>
		 * <para header='Note'>The listener closest to the sound source will be used if no listener is specified.</para>
		 * </remarks>
		 */
		public CriAtomListener listenerOnStart
		{
			get { return _listenerOnStart; }
			set { _listenerOnStart = value; }
		}

		/**
		 * <summary>Switches on/off the loop playback</summary>
		 * <param name='loop'>Loop switch (True: loop mode, False: cancel loop mode)</param>
		 * <remarks>
		 * <para header='Description'>Switches the loop playback ON/OFF for the waveform data that does not have a loop point.<br/>
		 * By default, loop is OFF.<br/>
		 * When loop playback is turned ON, the playback does not end at the end of the sound, and the playback is repeated from the beginning.<br/></para>
		 * <para header='Note'>The setting in this function is applied to the waveform data.<br/>
		 * When this function is called for Sequence data,
		 * the individual waveform data in the Sequence data is played back in a loop.<br/>
		 * <br/>
		 * The specification by this function is valid only for waveform data that does not have loop points.<br/>
		 * When playing back waveform data with loop points, loop playback is performed
		 * according to the loop position of the waveform data regardless of the specification of this function.<br/>
		 * <br/>
		 * This function internally uses the seamless link playback feature.<br/>
		 * Therefore, if you use a format that does not support seamless linked playback (such as HCA-MX),
		 * some amount of silence is inserted at the loop position.<br/>
		 * <br/>
		 * This parameter is evaluated when the status of the CriAtomSource component is stopped
		 * and the CriAtomSource::Play function is called.<br/></para>
		 * </remarks>
		 */
		public bool loop
		{
			set { this._loop = value; }
			get { return this._loop; }
		}

		/**
		 * <summary>Sets/gets the volume.</summary>
		 * <remarks>
		 * <para header='Description'>Sets/gets the volume of the output sound.<br/>
		 * The volume value is a scale factor for the amplitude of the sound data (the unit is not decibel).<br/>
		 * For example, if you specify 1.0f, the original sound is played at its unmodified volume.<br/>
		 * If you specify 0.5f, the sound is played at the volume by halving the amplitude (-6dB)
		 * of the original waveform.<br/>
		 * If you specify 0.0f, the sound is muted (silent).<br/>
		 * The default value for volume is 1.0f.</para>
		 * <para header='Note'>When playing a Cue, if this function is called when the volume is set on the data,
		 * the value set on the data and the setting in this function is <b>multiplied</b> and the result is applied.<br/>
		 * For example, if the volume on the data is 0.8f and the volume of CriAtomSource is 0.5f,
		 * the actual volume applied is 0.4f.<br/></para>
		 * </remarks>
		 */
		public float volume
		{
			set
			{
				this._volume = value;
				if (this.player != null)
				{
					this.player.SetVolume(this._volume);
					this.SetNeedToPlayerUpdateAll();
				}
			}
			get { return this._volume; }
		}

		/**
		 * <summary>Sets/gets the pitch.</summary>
		 * <remarks>
		 * <para header='Description'>Sets/gets the pitch of the output sound.<br/>
		 * The pitch is specified in cents.<br/>
		 * One cent is 1/1200 of one octave. A semitone is 100 cents.<br/>
		 * For example, if you specify 100.0f, the pitch increases by a semitone. If you specify -100.0f, the pitch decreases by a semitone.<br/>
		 * The default pitch is 0.0f.</para>
		 * <para header='Note'>When playing a Cue, if this function is called when the pitch is set on the data,
		 * the value set on the data and the setting in this function is <b>added</b> and the result is applied.<br/>
		 * For example, if the pitch on the data is -100.0f and the pitch of CriAtomSource is 200.0f,
		 * the actual pitch applied is 100.0f.</para>
		 * </remarks>
		 */
		public float pitch
		{
			set
			{
				this._pitch = value;
				if (this.player != null)
				{
					this.player.SetPitch(this._pitch);
					this.SetNeedToPlayerUpdateAll();
				}
			}
			get { return this._pitch; }
		}

		/**
		 * <summary>Sets/gets the Panning 3D angle.</summary>
		 * <remarks>
		 * <para header='Description'>Sets/gets the Panning 3D angle.<br/>
		 * The angle is specified in degrees.<br/>
		 * With front being 0 degree, you can set up to 180.0f in the right direction (clockwise) and -180.0f in the left direction (counterclockwise).<br/>
		 * For example, if you specify 45.0f, the localization will be 45 degrees to the front right. If you specify -45.0f, the localization will be 45 degree to the front left.<br/></para>
		 * <para header='Note'>When playing a Cue, if this function is called when the Panning 3D angle is set on the data,
		 * the value set on the data and the setting in this function is <b>added</b> and the result is applied.<br/>
		 * For example, if the Panning 3D angle on the data is 15.0f and the Panning 3D angle on CriAtomSource is 30.0f,
		 * the actual Panning 3D angle applied will be 45.0f.
		 * <br/>
		 * If the actual applied Panning 3D angle exceeds 180.0f, -360.0f is added to the value so that it is within the range.<br/>
		 * Similarly, if the actual applied volume value is less than -180.0f, +360.0f is add so that it is within the range.<br/>
		 * (Since the localization does not change even when +360.0f, or -360.0f is added, you can effectively set a value outside the range of -180.0f to 180.0f.)</para>
		 * </remarks>
		 */
		public float pan3dAngle
		{
			set
			{
				if (this.player != null)
				{
					this.player.SetPan3dAngle(value);
					this.SetNeedToPlayerUpdateAll();
				}
			}
			get
			{
				return (this.player != null) ?
					this.player.GetParameterFloat32(CriAtomEx.Parameter.Pan3dAngle) : 0.0f;
			}
		}

		/**
		 * <summary>Sets/gets the Panning 3D distance.</summary>
		 * <remarks>
		 * <para header='Description'>Sets/gets the distance when doing the interior Panning with Panning 3D.<br/>
		 * The distance is specified in the range of -1.0f to 1.0f, with the listener position being 0.0f and the circumference of the speaker position being 1.0f.<br/>
		 * If you specify a negative value, the Panning 3D angle inverts 180 degrees and the localization is reversed.</para>
		 * <para header='Note'>When playing a Cue, if this function is called when the Panning 3D distance is set on the data,
		 * the value set on the data and the setting in this function is <b>multiplied</b> and the result is applied.<br/>
		 * For example, if the Panning 3D distance on the data is 0.8f and the Panning 3D distance on CriAtomSource is 0.5f,
		 * the actual Panning 3D distance applied will be 0.4f.
		 * <br/>
		 * If the actual applied Panning 3D distance exceeds 1.0f, the value is clipped to 1.0f.<br/>
		 * Similarly, if the actual applied Panning 3D distance is smaller than -1.0f, the value is clipped to -1.0f.<br/></para>
		 * </remarks>
		 */
		public float pan3dDistance
		{
			set
			{
				if (this.player != null)
				{
					this.player.SetPan3dInteriorDistance(value);
					this.SetNeedToPlayerUpdateAll();
				}
			}
			get
			{
				return (this.player != null) ?
					this.player.GetParameterFloat32(CriAtomEx.Parameter.Pan3dDistance) : 0.0f;
			}
		}

		/**
		 * <summary>Sets/gets the playback start position.</summary>
		 * <remarks>
		 * <para header='Description'>Sets/gets the position to start playback.
		 * By setting the playback start position, you can play the sound data halfway.<br/>
		 * The playback start position is specified in milliseconds. For example, if you set 10000,
		 * the sound data to be played next is played from the position at 10 seconds.</para>
		 * <para header='Note'>When playing a sound from the middle of the data, the play timing delays
		 * compared to when playing it from the beginning.<br/>
		 * This is because the system must analyze the header of the sound data,
		 * jump to the specified position, and rereads the data to start playback.</para>
		 * <para header='Note'>Encrypted ADX data must be decrypted sequentially from the beginning of the data.<br/>
		 * Therefore, when playing encrypted ADX data from the middle,
		 * decryption of the data up to the seeking position will occur,
		 * causing the processing load to increase significantly.<br/>
		 * <br/>
		 * If a start position is specified before playing the sequence, 
		 * waveform data placed before that position will not be played.<br/>
		 * (Individual waveforms within the sequence will not be played from the middle.)<br/></para>
		 * </remarks>
		 */
		public int startTime
		{
			set
			{
				if (this.player != null)
				{
					this.player.SetStartTime(value);
					this.SetNeedToPlayerUpdateAll();
				}
			}
			get
			{
				return (this.player != null) ?
					this.player.GetParameterSint32(CriAtomEx.Parameter.StartTime) : 0;
			}
		}


		/**
		 * <summary>Gets the playback time (in milliseconds).</summary>
		 * <remarks>
		 * <para header='Description'>If the playback time can be acquired, this property indicates a value of 0 or bigger.<br/>
		 * This function indicates a negative value when the playback time cannot be get (when the Voice cannot be get).<br/></para>
		 * <para header='Note'>Indicates the time of the "last" sound played when multiple
		 * sounds are played by the same CriAtomSource component.<br/>
		 * If you need to check the playback time for multiple sounds,
		 * create as many CriAtomSource components as the number of sounds to play.<br/>
		 * <br/>
		 * The playback time indicated by this property is "the elapsed time from the start of playback".<br/>
		 * The time does not rewind depending on the playback position,
		 * even during loop playback or seamless linked playback.<br/>
		 * <br/>
		 * When the playback is paused using the CriAtomSource::Pause function,
		 * the playback time count-up also stops.<br/>
		 * (If you unpause the playback, the count-up resumes.)
		 * <br/></para>
		 * <para header='Note'>The return type is long, but currently there is no precision over 32bit.<br/>
		 * When performing control based on the playback time, it should be noted that the playback time becomes incorrect in about 24 days.<br/>
		 * (The playback time overflows and becomes a negative value when it exceeds 2147483647 milliseconds.)<br/>
		 * <br/>
		 * If the Voice being played is erased by the Voice control,
		 * the playback time count-up also stops at that point.<br/>
		 * In addition, if no Voice is allocated by the Voice control at the start of playback,
		 * this function does not return the correct time.<br/>
		 * (Negative value is returned.)<br/>
		 * <br/>
		 * Even if the sound data supply is temporarily interrupted due to a delay in reading the file etc.,
		 * the playback time count-up is not interrupted.<br/>
		 * (The time progresses even if the playback is stopped due to the stop of data supply.)<br/>
		 * Therefore, when synchronizing sound with the source video based on the time acquired by this function,
		 * the synchronization may be greatly deviated each time a data supply shortage occurs.<br/></para>
		 * </remarks>
		 */
		public long time
		{
			get
			{
				return (this.player != null) ?
					this.player.GetTime() : 0;
			}
		}

		/**
		 * <summary>Gets the status.</summary>
		 * <remarks>
		 * <para header='Description'>Gets the status of the CriAtomSource component.<br/>
		 * The status is one of following 5 values indicating the playback status of the CriAtomSource component.<br/>
		 * -# Stop
		 * -# Prep
		 * -# Playing
		 * -# Playend
		 * -# Error
		 * .
		 *  <br/>
		 *  <br/>
		 *  When the CriAtomSource component is created, the status of the CriAtomSource component
		 * is Stop.<br/>
		 *  When you start playback using the CriAtomSource.Play function etc., the status of the CriAtomSource component
		 * changes to Prep.<br/>
		 * (Prep is the status waiting for the data to be supplied or start of decoding.)<br/>
		 * When enough data is supplied for starting playback, the CriAtomSource component
		 * changes the status to Playing.<br/>
		 * If an error occurs during playback, CriAtomSource component
		 * changes the status to Error.<br/>
		 * <br/>
		 * By checking the status of CriAtomSource component and switching the processing depending on the status,
		 * it is possible to create a program linked to the sound playback status.</para>
		 * </remarks>
		 */
		public Status status
		{
			get
			{
				return (this.player != null) ?
					(Status)this.player.GetStatus() : Status.Error;
			}
		}

		/**
		 * <summary>Sets/gets if the distance attenuation is enabled.</summary>
		 * <remarks>
		 * <para header='Description'>Sets/gets whether to enable or disable the volume variation by distance attenuation.<br/>
		 * The default is enabled.</para>
		 * </remarks>
		 */
		public bool attenuationDistanceSetting
		{
			set
			{
				if (this.source != null)
				{
					source.SetAttenuationDistanceSetting(value);
					source.Update();
				}
			}
			get
			{
				return (this.source != null) ?
					source.GetAttenuationDistanceSetting() : false;
			}
		}

		/**
		 * <summary>Sets/gets whether to play sound from the low delay playback Voice Pool.</summary>
		 * <remarks>
		 * <para header='Description'>If set to True, playback starts using the low latency playback Voice Pool.</para>
		 * <para header='Note'>To enable this flag, it is necessary to set the number of low delay playback Voice Pools of CriWareInitializer.</para>
		 * </remarks>
		 */
		public bool androidUseLowLatencyVoicePool
		{
			get { return this._androidUseLowLatencyVoicePool; }
			set { this._androidUseLowLatencyVoicePool = value; }
		}

		#endregion

		#region Functions

		protected void SetNeedToPlayerUpdateAll()
		{
			this.need_to_player_update_all = true;
		}

		protected virtual void InternalInitialize()
		{
			CriAtomPlugin.InitializeLibrary();
			this.player = new CriAtomExPlayer();
			this.source = new CriAtomEx3dSource(randomPositionListMaxLength: this.randomPositionListMaxLength);
			this.initialized = true;
		}

		protected virtual void InternalFinalize()
		{
			this.initialized = false;
			this.region3d = null;
			this.player.Dispose();
			this.player = null;
			this.source.Dispose();
			this.source = null;
			CriAtomPlugin.FinalizeLibrary();
		}

		void Awake()
		{
			this.InternalInitialize();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.hasValidPosition = false;
			this.SetInitialParameters();
			this.SetNeedToPlayerUpdateAll();
		}

		void OnDestroy()
		{
			this.InternalFinalize();
		}

		protected bool SetInitialSourcePosition()
		{
			Vector3 position = this.transform.position;
			this.lastPosition = position;
			if (this.source != null)
			{
				this.source.SetPosition(position.x, position.y, position.z);
				this.source.Update();
				return true;
			}
			else
			{
				return false;
			}
		}

		protected virtual void SetInitialParameters()
		{
			this.use3dPositioning = this.use3dPositioning; /* ここで必要に応じて3Dソースが設定される */
			this.use3dRandomization = this.use3dRandomization;
			if (this.SetInitialSourcePosition() == false)
			{
				Debug.LogError("[ADX2][SetInitialParameters] source is null.", this);
			}

			this.player.SetVolume(this._volume);
			this.player.SetPitch(this._pitch);
		}

		protected virtual void UpdatePosition()
		{
			Vector3 position = this.transform.position;
			this.source.SetPosition(position.x, position.y, position.z);
			if (this.hasValidPosition == true)
			{
				Vector3 velocity = (position - this.lastPosition) / Time.deltaTime;
				this.source.SetVelocity(velocity.x, velocity.y, velocity.z);
			}
			if (this.freezeOrientation == false)
			{
				this.source.SetOrientation(this.transform.forward, this.transform.up);
			}
			this.source.Update();
			this.lastPosition = position;
			this.hasValidPosition = true;
		}

		void Start()
		{
			if (this.use3dPositioning && this.regionOnStart != null)
			{
				this.region3d = this.regionOnStart;
			}
			if (use3dPositioning && listenerOnStart != null)
				listener = listenerOnStart;
			this.PlayOnStart();
		}

		public override void CriInternalUpdate() { }

		public override void CriInternalLateUpdate()
		{
			if (this.use3dPositioning == true)
			{
				UpdatePosition();
			}

			if (this.need_to_player_update_all == true)
			{
				this.player.UpdateAll();
				this.need_to_player_update_all = false;
			}
		}

#if UNITY_EDITOR
		static private readonly Color c_CriWareLightBlue = new Color(0.332f, 0.661f, 0.991f);
		static private readonly Color c_RandomizeGizmoColor = new Color(0.332f, 0.661f, 0.991f, 0.2f);
		private Mesh randomize3dGizmoMesh = null;
		private CriAtomEx.Randomize3dCalcType lastRandomizeTypeGizmo = CriAtomEx.Randomize3dCalcType.None;
		private Quaternion currentGizmoRotation = Quaternion.identity;
		private Quaternion lastGizmoRotation = Quaternion.identity;

		private void SetGizmoMesh(PrimitiveType meshType)
		{
			GameObject tempPrimitive = GameObject.CreatePrimitive(meshType);
			this.randomize3dGizmoMesh = tempPrimitive.GetComponent<MeshFilter>().sharedMesh;
			DestroyImmediate(tempPrimitive);
		}

		public void OnDrawGizmos()
		{
			if (this.enabled == false) { return; }
			var gizmoColor = (!Application.isPlaying || this.status == Status.Playing) ? c_CriWareLightBlue : Color.gray;
			if (this.freezeOrientation == false)
			{
				currentGizmoRotation = transform.rotation;
				lastGizmoRotation = currentGizmoRotation;
			}
			else if (Application.isPlaying)
			{
				currentGizmoRotation = lastGizmoRotation;
			}
			else
			{
				currentGizmoRotation = Quaternion.identity;
			}
			UnityEditor.Handles.color = gizmoColor;
			UnityEditor.Handles.DrawLine(this.transform.position, this.transform.position + this.currentGizmoRotation * Vector3.forward);
			UnityEditor.Handles.DrawLine(this.transform.position, this.transform.position + this.currentGizmoRotation * Vector3.up);
			UnityEditor.Handles.ArrowHandleCap(1, this.transform.position + this.currentGizmoRotation * Vector3.forward, this.currentGizmoRotation, 1f, EventType.Repaint);
			UnityEditor.Handles.CircleHandleCap(1, this.transform.position, this.currentGizmoRotation * Quaternion.LookRotation(Vector3.up), 1f, EventType.Repaint);

			if (UnityEditor.Selection.activeGameObject == this.gameObject)
			{
				bool needRefreshMesh = false;
				if (lastRandomizeTypeGizmo != randomize3dConfig.CalculationType)
				{
					needRefreshMesh = true;
					lastRandomizeTypeGizmo = randomize3dConfig.CalculationType;
				}
				if (use3dRandomization)
				{
					Gizmos.color = c_RandomizeGizmoColor;
					Vector3 scale;
					switch (randomize3dConfig.CalculationType)
					{
						case CriAtomEx.Randomize3dCalcType.Rectangle:
							if (needRefreshMesh) { this.SetGizmoMesh(PrimitiveType.Quad); }
							scale = new Vector3(randomize3dConfig.CalculationParameter1, randomize3dConfig.CalculationParameter2, 1f);
							Gizmos.DrawWireMesh(randomize3dGizmoMesh, this.transform.position, this.currentGizmoRotation * Quaternion.LookRotation(Vector3.up), scale);
							break;
						case CriAtomEx.Randomize3dCalcType.Cuboid:
							if (needRefreshMesh) { this.SetGizmoMesh(PrimitiveType.Cube); }
							scale = new Vector3(randomize3dConfig.CalculationParameter1, randomize3dConfig.CalculationParameter3, randomize3dConfig.CalculationParameter2);
							Gizmos.DrawWireMesh(randomize3dGizmoMesh, this.transform.position, this.currentGizmoRotation, scale);
							break;
						case CriAtomEx.Randomize3dCalcType.Circle:
							if (needRefreshMesh) { this.SetGizmoMesh(PrimitiveType.Cylinder); }
							scale = new Vector3(randomize3dConfig.CalculationParameter1 * 2f, 0, randomize3dConfig.CalculationParameter1 * 2f);
							Gizmos.DrawWireMesh(randomize3dGizmoMesh, this.transform.position, this.currentGizmoRotation, scale);
							break;
						case CriAtomEx.Randomize3dCalcType.Cylinder:
							if (needRefreshMesh) { this.SetGizmoMesh(PrimitiveType.Cylinder); }
							scale = new Vector3(randomize3dConfig.CalculationParameter1 * 2f, randomize3dConfig.CalculationParameter2, randomize3dConfig.CalculationParameter1 * 2f);
							Gizmos.DrawWireMesh(randomize3dGizmoMesh, this.transform.position, this.currentGizmoRotation, scale);
							break;
						case CriAtomEx.Randomize3dCalcType.Sphere:
							if (needRefreshMesh) { this.SetGizmoMesh(PrimitiveType.Sphere); }
							scale = new Vector3(randomize3dConfig.CalculationParameter1 * 2f, randomize3dConfig.CalculationParameter1 * 2f, randomize3dConfig.CalculationParameter1 * 2f);
							Gizmos.DrawWireMesh(randomize3dGizmoMesh, this.transform.position, this.currentGizmoRotation, scale);
							break;
						default:
							randomize3dGizmoMesh = null;
							break;
					}
				}
			}
		}
#endif

		#region Abstract Functions
		public abstract CriAtomExPlayback Play();
		protected abstract CriAtomExAcb GetAcb();
		protected abstract void PlayOnStart();
		#endregion

		#region PlaybackAndController

		/**
		 * <summary>Starts playing the Cue with the specified Cue name.</summary>
		 * <param name='cueName'>Cue name</param>
		 * <returns>Playback ID</returns>
		 * <remarks>
		 * <para header='Description'>Plays the Cue with the name specified in this function, regardless of how the Cue is specified in the corresponding property.</para>
		 * </remarks>
		 */
		public CriAtomExPlayback Play(string cueName)
		{
			if (this.player == null)
				return new CriAtomExPlayback(CriAtomExPlayback.invalidId);

			CriAtomExAcb acb = GetAcb();
			this.player.SetCue(acb, cueName);
			return InternalPlayCue();
		}

		/**
		 * <summary>Starts playing the Cue with the specified Cue ID.</summary>
		 * <param name='cueId'>Cue ID</param>
		 * <returns>Playback ID</returns>
		 * <remarks>
		 * <para header='Description'>Plays the Cue with the ID specified in this function, regardless of how the Cue is specified in the corresponding property.</para>
		 * </remarks>
		 */
		public CriAtomExPlayback Play(int cueId)
		{
			if (this.player == null)
				return new CriAtomExPlayback(CriAtomExPlayback.invalidId);


			CriAtomExAcb acb = GetAcb();
			this.player.SetCue(acb, cueId);
			return InternalPlayCue();
		}

		protected CriAtomExPlayback InternalPlayCue()
		{
#if !UNITY_EDITOR && UNITY_ANDROID
		if (androidUseLowLatencyVoicePool) {
			this.player.SetSoundRendererType(CriAtomEx.SoundRendererType.Native);
		} else {
			this.player.SetSoundRendererType(CriAtomEx.androidDefaultSoundRendererType);
		}
#endif
			if (this.hasValidPosition == false)
			{
				this.SetInitialSourcePosition();
				this.hasValidPosition = true;
			}
			if (this.status == Status.Stop)
			{
				this.player.Loop(this._loop);
			}
			return this.player.Start();
		}

		/**
		 * <summary>Stops playback.</summary>
		 * <remarks>
		 * <para header='Description'>If this function is called for the CriAtomSource component that is playing sound,
		 * the CriAtomSource component stops playing (stops reading files or playback),
		 * and transitions to the Stop state.<br/>
		 * If this function is called for an CriAtomSource component that has already stopped
		 * (CriAtomSource component whose status is Playend or Error), the status
		 * of the CriAtomSource component is changed to Stop.</para>
		 * <para header='Note'>If this function is called for the CriAtomSource component that is playing sound, the status
		 * may not change to Stop immediately.<br/>
		 * (It may take some time for it to change to Stop.)</para>
		 * </remarks>
		 */
		public void Stop()
		{
			if (this.player != null)
			{
				this.player.Stop();
			}
		}

		/**
		 * <summary>Pauses/resumes.</summary>
		 * <param name='sw'>True: Pause, False: Resume</param>
		 * <remarks>
		 * <para header='Description'>Pauses/unpauses playback.<br/>
		 * When this function is called by specifying True in sw,
		 * the CriAtomSource component pauses the sound being played.<br/>
		 * When this function is called by specifying False in sw,
		 * the CriAtomSource component resumes the playback of paused sound.<br/></para>
		 * </remarks>
		 */
		public void Pause(bool sw)
		{
			if (this.player == null)
				return;

			if (sw == false)
			{
				this.player.Resume(CriAtomEx.ResumeMode.PausedPlayback);
			}
			else
			{
				this.player.Pause();
			}
		}

		/**
		 * <summary>Gets the posing status.</summary>
		 * <returns>Pausing status</returns>
		 * <remarks>
		 * <para header='Description'>Gets the ON/OFF status of the pause.<br/></para>
		 * </remarks>
		 * <seealso cref='CriAtomSource::Pause'/>
		 */
		public bool IsPaused()
		{
			return (this.player != null) ? this.player.IsPaused() : false;
		}

		/**
		 * <summary>Set the Bus Send Level by specifying the bus name.</summary>
		 * <remarks>
		 * <para header='Note'>When playing a Cue, if this function is called when the Bus Send Level is set on the data,
		 * the value set on the data and the setting in this function is <b>multiplied</b> and the result is applied.<br/>
		 * For example, if the Bus Send Level on the data is 0.8f and the Bus Send Level of the CriAtomSource is 0.5f,
		 * the Bus Send Level actually applied is 0.4f.<br/></para>
		 * </remarks>
		 */
		public void SetBusSendLevel(string busName, float level)
		{
			if (this.player != null)
			{
				this.player.SetBusSendLevel(busName, level);
				this.SetNeedToPlayerUpdateAll();
			}
		}

		/**
		 * \deprecated
		 * This is a deprecated API that will be removed.
		 * Please consider using CriAtomSource.SetBusSendLevel(string, float) instead.
		*/
		[System.Obsolete("Use CriAtomSource.SetBusSendLevel(string, float)")]
		public void SetBusSendLevel(int busId, float level)
		{
			if (this.player != null)
			{
				this.player.SetBusSendLevel(busId, level);
				this.SetNeedToPlayerUpdateAll();
			}
		}

		/**
		 * <summary>Set the Bus Send Level by specifying the bus name and offset.</summary>
		 * <remarks>
		 * <para header='Description'>When playing a Cue, if this function is called when the Bus Send Level is set on the data,
		 * the value set on the data and the setting in this function is <b>added</b> and the result is applied.<br/>
		 * For example, if the Bus Send Level on the data is 0.2f and the Bus Send Level of the CriAtomSource is 0.5f,
		 * the Bus Send Level actually applied is 0.7f.<br/></para>
		 * </remarks>
		 */
		public void SetBusSendLevelOffset(string busName, float levelOffset)
		{
			if (this.player != null)
			{
				this.player.SetBusSendLevelOffset(busName, levelOffset);
				this.SetNeedToPlayerUpdateAll();
			}
		}

		/**
		 * \deprecated
		 * This is a deprecated API that will be removed.
		 * Please consider using CriAtomSource.SetBusSendLevelOffset(string, float) instead.
		*/
		[System.Obsolete("Use CriAtomSource.SetBusSendLevelOffset(string, float)")]
		public void SetBusSendLevelOffset(int busId, float levelOffset)
		{
			if (this.player != null)
			{
				this.player.SetBusSendLevelOffset(busId, levelOffset);
				this.SetNeedToPlayerUpdateAll();
			}
		}


		/**
		 * <summary>Sets the AISAC control value by specifying the AISAC control name.</summary>
		 */
		public void SetAisacControl(string controlName, float value)
		{
			if (this.player != null)
			{
				this.player.SetAisacControl(controlName, value);
				this.SetNeedToPlayerUpdateAll();
			}
		}

		/**
		 * \deprecated
		 * This is a deprecated API that will be removed.
		 * Please consider using CriAtomSource.SetAisacControl instead.
		*/
		[System.Obsolete("Use CriAtomSource.SetAisacControl")]
		public void SetAisac(string controlName, float value)
		{
			SetAisacControl(controlName, value);
		}

		/**
		 * <summary>Sets the AISAC control value by specifying the AISAC control name.</summary>
		 */
		public void SetAisacControl(uint controlId, float value)
		{
			if (this.player != null)
			{
				this.player.SetAisacControl(controlId, value);
				this.SetNeedToPlayerUpdateAll();
			}
		}

		/**
		 * \deprecated
		 * This is a deprecated API that will be removed.
		 * Please consider using CriAtomSource.SetAisacControl instead.
		*/
		[System.Obsolete("Use CriAtomSource.SetAisacControl")]
		public void SetAisac(uint controlId, float value)
		{
			SetAisacControl(controlId, value);
		}

		/**
		 * <summary>Attach to the output data analysis module.</summary>
		 */
		public void AttachToAnalyzer(CriAtomExOutputAnalyzer analyzer)
		{
			if (this.player != null)
			{
				analyzer.AttachExPlayer(this.player);
			}
		}

		/**
		 * <summary>Detaches from the output data analysis module.</summary>
		 */
		public void DetachFromAnalyzer(CriAtomExOutputAnalyzer analyzer)
		{
			analyzer.DetachExPlayer();
		}
		#endregion

		#endregion
	} // end of class
} // namespace CriWare
