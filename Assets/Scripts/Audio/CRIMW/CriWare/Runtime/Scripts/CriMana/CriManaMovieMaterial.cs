/****************************************************************************
 *
 * Copyright (c) 2015 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System.Collections;
using UnityEngine;


/**
 * \addtogroup CRIMANA_UNITY_COMPONENT
 * @{
 */

namespace CriWare {

/**
 * <summary>A component for feeding a movie to a Material.</summary>
 * <remarks>
 * <para header='Description'>A component for feeding a movie to a Material.<br/>
 * A base class for movie playback and rendering components.<br/>
 * By inheriting this class, it is possible to render a movie on any drawing target.<br/>
 * This component only feeds a movie to a Material, so nothing is displayed when it is attached and used.<br/>
 * Usually, use the CriManaMovieController or CriManaMovieControllerForUI component
 * according to the object you want to display the movie.</para>
 * <para header='Note'>In this class, you can only perform basic operations such as play, stop, or pause.<br/>
 * If you want to perform complicated playback control, operate the core player using the player property.</para>
 * </remarks>
 */
[AddComponentMenu("CRIWARE/CriManaMovieMaterial")]
public class CriManaMovieMaterial : CriManaMovieMaterialBase
{
	#region Properties
	/**
	 * <summary>The file path for streaming playback at Start.</summary>
	 * <param name='filePath'>File path</param>
	 * <remarks>
	 * <para header='Description'>The file path for streaming playback at Start.<br/>
	 * - If a relative path is specified, the file is loaded relative to StreamingAssets folder.
	 * - The file is loaded using the absolute path, or the specified path if a URL is specified.</para>
	 * <para header='Note'>To set the file path from the script, use the CriMana::Player::SetFile method etc. of the player
	 * property.<br/>
	 * This property is provided to set the file path for streaming playback at Start on the Inspector.
	 * Changing this property after Start does not apply to the next playback.</para>
	 * </remarks>
	 */
	public string moviePath
	{
		get { return _moviePath; }
		set {
			if (isMonoBehaviourStartCalled) {
				Debug.LogError("[CRIWARE] moviePath can not be changed. Use CriMana::Player::SetFile method.");
			} else {
				_moviePath = value;
			}
		}
	}

	/**
	 * <summary>Loop setting for movie playback at Start.</summary>
	 * <remarks>
	 * <para header='Description'>Loop setting applied on Start of a movie playback. The default value is false.</para>
	 * <para header='Note'>To set the loop from the script, use the CriMana::Player::Loop method of the player property.<br/>
	 * This property is provided to set the loop at Start on the Inspector.<br/>
	 * Changing this property after Start does not apply to the next playback.</para>
	 * </remarks>
	 */
	public bool loop
	{
		get { return _loop; }
		set {
			if (isMonoBehaviourStartCalled) {
				Debug.LogError("[CRIWARE] loop property can not be changed. Use CriMana::Player::Loop method.");
			} else {
				_loop = value;
			}
		}
	}

	/**
	 * <summary>Switches to the Advanced Audio mode.</summary>
	 * <remarks>
	 * <para header='Description'>Advanced audio playback features are available when the Advanced Audio mode is enabled. The default value is false.<br/>
	 * For example, it is necessary to enable this mode to play movies with Ambisonic audio.</para>
	 * </remarks>
	 */
	public bool advancedAudio
	{
		get { return _advancedAudio; }
		set {
			if (isMonoBehaviourStartCalled) {
				Debug.LogError("[CRIWARE] advancedAudio property can not be changed in running.");
			} else {
				if (value == false) {
					ambisonics = false;
				}
				_advancedAudio = value;
			}
		}
	}


	/**
	 * <summary>Enables the playback of movies with Ambisonic sound.</summary>
	 * <remarks>
	 * <para header='Description'>A property to enable the playback of movies with Ambisonic sound.<br/>
	 * Only available when Advanced Audio mode is enabled.</para>
	 * <para header='Note'>When enabling this mode, a GameObject called Ambisonic Source is created as a child object.<br/>
	 * The CriManaAmbisonicSource component is attached to this Ambisonic Source object.</para>
	 * </remarks>
	 */
	public bool ambisonics
	{
		get { return _ambisonics; }
		set {
			if (isMonoBehaviourStartCalled) {
				Debug.LogError("[CRIWARE] ambisonics property can not be changed in running.");
			} else if (_advancedAudio != true) {
				Debug.LogError("[CRIWARE] ambisonics property needs for advancedAudio property to be true.");
			} else {
				/*  Advanced Audio モードの ON/OFF 切替時に、Ambisonic Source オブジェクトを 生成/破棄 */
				if (value == false) {
					GameObject obj = null;
					if (gameObject.transform.childCount > 0) {
						/* 自分の子オブジェクトである "Ambisonic Source" を破棄する */
						obj = (ambisonicSource != null) ? ambisonicSource : gameObject.transform.Find("Ambisonic Source").gameObject;
						if (obj != null) {
							DestroyImmediate(obj);
							obj = null;
						}
					}
				} else {
					if (ambisonicSource == null) {
						ambisonicSource = new GameObject();
						ambisonicSource.name = "Ambisonic Source";
						ambisonicSource.transform.parent = gameObject.transform;
						ambisonicSource.transform.position = gameObject.transform.position;
						ambisonicSource.transform.rotation = gameObject.transform.rotation;
						ambisonicSource.transform.localScale = gameObject.transform.localScale;
						ambisonicSource.AddComponent<CriManaAmbisonicSource>();
					}
				}
				_ambisonics = value;
			}
		}
	}


	/**
	 * <summary>Additive synthesis mode setting at Start.</summary>
	 * <remarks>
	 * <para header='Description'>Additive mode setting applied on Start of a movie playback. The default value is false.</para>
	 * <para header='Note'>To set the additive synthesis mode from the script, use the CriMana::Player::additiveMode property of
	 * the player property.<br/>
	 * This property is provided to set the additive synthesis mode at Start on the Inspector.<br/>
	 * Changing this property after Start does not apply to the next playback.</para>
	 * </remarks>
	 */
	public bool additiveMode
	{
		get { return _additiveMode; }
		set {
			if (isMonoBehaviourStartCalled) {
				Debug.LogError("[CRIWARE] additiveMode can not be changed. Use CriMana::Player::additiveMode method.");
			} else {
				_additiveMode = value;
			}
		}
	}

	/**
	 * <summary>Sets whether to apply the object transparency.</summary>
	 * <remarks>
	 * <para header='Description'>Sets whether the movie will be transparent according to the transparency of the attached object.<br/>
	 * The default value is false.</para>
	 * </remarks>
	 */
	public bool applyTargetAlpha
	{
		get { return _applyTargetAlpha; }
		set {
			if (isMonoBehaviourStartCalled) {
				Debug.LogError("[CRIWARE] applyTargetAlpha property can not be changed in running.");
			} else {
				_applyTargetAlpha = value;
			}
		}
	}

	/**
	 * <summary>Specifies whether shader settings for UI components are applied.</summary>
	 * <remarks>
	 * <para header='Description'>Sets whether to apply the rendering settings for UI components to the shader
	 * that renders the movie. <br/>
	 * The default value is false.</para>
	 * </remarks>
	 */
	public bool uiRenderMode
	{
		get { return _uiRenderMode; }
		set {
			if (isMonoBehaviourStartCalled) {
				Debug.LogError("[CRIWARE] uiRenderMode property can not be changed in running.");
			} else {
				_uiRenderMode = value;
			}
		}
	}

	#endregion

	protected override uint FilePathLength
	{
		get {
			return
				string.IsNullOrEmpty(moviePath) ?
				0 : (uint)(moviePath.Length + (CriWare.Common.IsStreamingAssetsPath(moviePath) ? CriWare.Common.streamingAssetsPath.Length + 1 : 0)) + 1;
		}
	}

	protected override bool initializeWithAdvancedAudio
	{
		get {
			return advancedAudio;
		}
	}

	protected override bool initializeWithAmbisonics
	{
		get {
			return ambisonics;
		}
	}

	protected override void SetDataToPlayer()
	{
		if (!System.String.IsNullOrEmpty(moviePath)) {
			player.SetFile(null, moviePath);
		}
		player.Loop(loop);
		player.additiveMode = additiveMode;
		player.maxFrameDrop = (int)maxFrameDrop;
		player.applyTargetAlpha = applyTargetAlpha;
		player.uiRenderMode = uiRenderMode;
	}

	[SerializeField] private string _moviePath;

	[SerializeField] private bool _loop = false;
	[SerializeField] private bool _additiveMode = false;
	[SerializeField] private bool _advancedAudio = false;
	[SerializeField] private bool _ambisonics = false;
	[SerializeField] private bool _applyTargetAlpha = false;
	[SerializeField] private bool _uiRenderMode = false;
	private GameObject ambisonicSource = null;
}

public abstract class CriManaMovieMaterialBase : CriMonoBehaviour
{

	#region Properties

	/**
	 * <summary>Sets whether to play at Start.</summary>
	 * <remarks>
	 * <para header='Description'>Specifies whether playback is to be performed on Start. The default value is false.</para>
	 * </remarks>
	 */
	public bool     playOnStart     = false;

	/**
	 * <summary>A setting whether to play from the start on OnEnable.</summary>
	 * <remarks>
	 * <para header='Description'>If true, the playback will start over from the beginning when a component is disabled during playback and then re-enabled.<br/>
	 * The default value is false.</para>
	 * </remarks>
	 */
	public bool restartOnEnable = false;

	/**
	 * <summary>Maximum frame drop number type</summary>
	 */
	public enum MaxFrameDrop
	{
		Disabled = 0,
		One = 1,
		Two = 2,
		Three = 3,
		Four = 4,
		Five = 5,
		Six = 6,
		Seven = 7,
		Eight = 8,
		Nine = 9,
		Ten = 10,
		Infinite = -1
	};

	/**
	 * <summary>Sets the maximum number of frame drops.</summary>
	 * <remarks>
	 * <para header='Description'>Sets the maximum number of frames to be dropped in one update
	 * if the update of rendered frames is not keeping up with the playback.<br/>
	 * This allows  intended playback if the frame rate of the application is low
	 * or when the playback speed of the video is raised higher than the frame rate.</para>
	 * </remarks>
	 */
	public MaxFrameDrop maxFrameDrop
	{
		get { return _maxFrameDrop; }
		set {
			_maxFrameDrop = value;
			if (player != null) {
				player.maxFrameDrop = (int)_maxFrameDrop;
			}
		}
	}

	protected abstract bool initializeWithAdvancedAudio { get; }
	protected abstract bool initializeWithAmbisonics { get; }

	/**
	 * <summary>Whether movie frames can be rendered with CriManaMovieMaterial::material</summary>
	 * <remarks>
	 * <para header='Description'>Indicates whether movie frames can be rendered with CriManaMovieMaterial::material.</para>
	 * </remarks>
	 */
	public bool isMaterialAvailable { get; private set; }

	/**
	 * <summary>Playback control player</summary>
	 * <remarks>
	 * <para header='Description'>A player property for fine movie playback control.<br/>
	 * If you want to perform operations other than Start/Stop/Pause, use the CriMana::Player API via this property.</para>
	 * </remarks>
	 */
	public CriMana.Player player { get; private set; }

	/**
	 * <summary>Sets the Material for feeding the movie.</summary>
	 * <remarks>
	 * <para header='Description'>If you set a Material, the movie is fed to that Material.<br/>
	 * If you do not set a Material, a Material to which the movie is fed is created.</para>
	 * <para header='Note'>If you set a Material, it must be set before the Start method is called.</para>
	 * </remarks>
	 */
	public Material material
	{
		get { return _material; }
		set {
			if (value != _material) {
				if (materialOwn) {
#if UNITY_EDITOR
					if (UnityEditor.EditorApplication.isPlaying == false) {
						DestroyImmediate(_material);
					} else {
						Destroy(_material);
					}
#else
					Destroy(_material);
#endif
					materialOwn = false;
				}
				_material = value;
				isMaterialAvailable = false;
			}
		}
	}

	/**
	 * <summary>Material rendering mode type</summary>
	 * <remarks>
	 * <para header='Description'> A type which indicates how the video should be rendered on the Material.<br/>
	 * - Always: Draw every frame.<br/>
	 * - OnVisibility: Renders if the GameObject is the rendering target (if active for UI.Graphic).<br/>
	 * - Never: Do not render. You must explicitly call CriManaMovieMaterial::RenderMovie to do the rendering.</para>
	 * </remarks>
	 */
	public enum RenderMode
	{
		Always = 0,
		OnVisibility = 1,
		Never = 2
	};

	/**
	 * <summary>Material rendering mode</summary>
	 * <remarks>
	 * <para header='Description'>Sets how the video is rendered for the Material.</para>
	 * <para header='Note'>On some platforms (PC/iOS, etc.), rendering is always done regardless of this setting.</para>
	 * </remarks>
	 */
	public RenderMode renderMode = RenderMode.Always;

	/**
	 * <summary>Callback delegate on application pause</summary>
	 * <remarks>
	 * <para header='Description'>A callback delegate called at the time of suspend/resume or when the
	 * application pauses such as by pressing the pause button while running the Editor.</para>
	 * </remarks>
	 */
	public delegate void OnApplicationPauseCallback(CriManaMovieMaterialBase manaMovieMaterial, bool appPause);

	/**
	 * <summary>Callback on application pause</summary>
	 * <remarks>
	 * <para header='Description'>Sets the processing when the application is paused/resumed.<br/>
	 * The behavior if not set (when null is specified) is "paused at start, resumed at end
	 * (remains paused if it was already paused at the)".<br/>
	 * If you want to change the above behavior in the application side,
	 * set your own callback process in this property.</para>
	 * </remarks>
	 */
	public OnApplicationPauseCallback onApplicationPauseCallback = null;

	private CriMana.Player.TimerType _timerType = CriMana.Player.TimerType.Audio;
	public CriMana.Player.TimerType timerType
	{
		get { return _timerType; }
		set {
			_timerType = value;
			if (player != null)
				player.SetMasterTimerType(timerType);
		}
	}

	#endregion

	#region Abstracts

	protected abstract uint FilePathLength { get; }
	protected abstract void SetDataToPlayer();

	#endregion

	#region Internal Variables
	[SerializeField] private Material _material;
	[SerializeField] private MaxFrameDrop _maxFrameDrop = MaxFrameDrop.Disabled;
	private bool materialOwn = false;
	protected bool isMonoBehaviourStartCalled = false;
	private bool wasDisabled = false;
	private bool wasPausedOnDisable = false;
	private WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
#if UNITY_EDITOR
	private bool isApplicationPaused = false;
	private bool isEditorPaused = false;
#endif
	private bool unpauseOnApplicationUnpause = false;
	#endregion
	protected bool HaveRendererOwner { get; private set; }


	/**
	 * <summary>Start playback.</summary>
	 * <remarks>
	 * <para header='Description'>Starts movie playback.<br/>
	 * When playback starts, the status ( CriWare.CriMana::Player::Status ) changes to Playing.</para>
	 * <para header='Note'>After calling this function, it takes several frames before the rendering of movie actually starts.<br/>
	 * If you want to synchronize the movie playback, instead of using this function, access the player property,
	 * and prepare for playback in advance using the CriMana::Player::Prepare function.</para>
	 * </remarks>
	 * <seealso cref='CriManaMovieMaterial::Stop'/>
	 * <seealso cref='CriMana::Player::Status'/>
	 */
	public void Play()
	{
		player.Start();
		this.CriInternalUpdate();
	}


	/**
	 * <summary>Requests to stop movie playback.</summary>
	 * <remarks>
	 * <para header='Description'>Issues a request to stop movie playback. This function returns immediately.<br/>
	 * The rendering ends immediately when you call this function, but the playback does not stop immediately.</para>
	 * </remarks>
	 * <seealso cref='CriMana::Player::Status'/>
	 */
	public void Stop()
	{
		player.Stop();
		if (isMaterialAvailable) {
			isMaterialAvailable = false;
			OnMaterialAvailableChanged();
		}
	}



	/**
	 * <summary>Pauses/unpauses the movie playback.</summary>
	 * <param name='sw'>Pause switch (true: pause, false: resume)</param>
	 * <remarks>
	 * <para header='Description'>Turns the pause on and off.<br/>
	 * Pauses the playback if the argument is true, resumes the playback if it is false. <br/>
	 * The pause state will be cleared when CriManaMovieMaterial::Stop is called.</para>
	 * </remarks>
	 */
	public void Pause(bool sw)
	{
		if (this.wasDisabled) {
			this.wasPausedOnDisable = sw;
		} else {
			player.Pause(sw);
		}
	}


	/**
	 * <summary>A method called when the isMaterialAvailable property changes.</summary>
	 * <remarks>
	 * <para header='Description'>A method called when the isMaterialAvailable property changes.<br/>
	 * It is supposed to be overridden in the inherited class.</para>
	 * </remarks>
	 */
	protected virtual void OnMaterialAvailableChanged()
	{
	}


	/**
	 * <summary>A method called when a new frame is fed to the Material.</summary>
	 * <remarks>
	 * <para header='Description'>A method called when a new frame is fed to the Material.<br/>
	 * It is supposed to be overridden in the inherited class.</para>
	 * </remarks>
	 */
	protected virtual void OnMaterialUpdated()
	{
	}

	private CriManaMoviePlayerHolder playerHolder;

	/**
	 * <summary>Initializes the player (for manual)</summary>
	 * <remarks>
	 * <para header='Description'>Manually initializes the player.<br/></para>
	 * <para header='Note'>Since it is usually called from the Awake function, it is not required to call this function separately.<br/>
	 * Use this function only if you want to perform initialization manually for editor extension etc.</para>
	 * </remarks>
	 */
	public void PlayerManualInitialize()
	{
		if (player != null) {
			Debug.LogError("[CRIWARE][Error] CriManaMovieMaterial is already initialized. There is no need to call this function multiple times.");
			return;
		}

		player = new CriMana.Player(initializeWithAdvancedAudio, initializeWithAmbisonics, FilePathLength);
		player.SetMasterTimerType(timerType);
		isMaterialAvailable = false;
		if (Application.isPlaying) {
			// object that can keep alive player for defering graphics cleaning
			GameObject go = new GameObject("CriManaMovieResources");
			playerHolder = go.AddComponent<CriManaMoviePlayerHolder>();
			playerHolder.player = player;
			player.playerHolder = playerHolder;
		}
	}

	/**
	 * <summary>Terminates the player (for manual)</summary>
	 * <remarks>
	 * <para header='Description'>Manually terminates the player.<br/></para>
	 * <para header='Note'>This function is called from OnDestroy(), therefore it is not necessary to call it manually. <br/>
	 * Call this function only when you want to perform initialization manually (e.g., for editor customization).</para>
	 * </remarks>
	 */
	public void PlayerManualFinalize()
	{
		if (player != null) {
			player.Dispose();
			player = null;
			material = null;
		}
	}

	/**
	 * <summary>Prepares the player (for manual)</summary>
	 * <remarks>
	 * <para header='Description'>Manually prepares the player.<br/></para>
	 * <para header='Note'>Since it is usually called from the Start function, it is not required to call this function separately.<br/>
	 * Use this function if you want perform preparation manually for editor extension etc.</para>
	 * </remarks>
	 */
	public void PlayerManualSetup()
	{
		HaveRendererOwner = (this.GetComponent<Renderer>() != null);

		if (_material == null) {
			CreateMaterial();
		}
		SetDataToPlayer();
		if (playOnStart) {
			player.Start();
		}
	}

	/**
	 * <summary>Initialization process of the rendering target (for manual use)</summary>
	 * <remarks>
	 * <para header='Description'>This method should be called to set the rendering target at initialization. <br/>
	 * It is supposed to be overridden in the inherited class.</para>
	 * <para header='Note'>Since it is usually called from the Start function, it is not required to call this function separately.<br/>
	 * Use this function if you want perform preparation manually for editor extension etc.</para>
	 * </remarks>
	 */
	public virtual bool RenderTargetManualSetup()
	{
		return true;
	}

	/**
	 * <summary>Finalization process of the rendering target (for manual use)</summary>
	 * <remarks>
	 * <para header='Description'>This method is called to configure the rendering target when finalizing. <br/>
	 * It is supposed to be overridden in the inherited class.</para>
	 * <para header='Note'>Since it is usually called from the OnDestroy function, it is not required to call this function separately.<br/>
	 * Use this function if you want perform finalization manually for editor extension etc.</para>
	 * </remarks>
	 */
	public virtual void RenderTargetManualFinalize()
	{
	}


	/**
	 * <summary>Updates the player frame (for manual)</summary>
	 * <remarks>
	 * <para header='Description'>Manually updates the player frame.<br/></para>
	 * <para header='Note'>Since it is usually called from the Update function, it is not required to call this function separately.<br/>
	 * Use this function if you want update frames manually for editor extension etc.</para>
	 * </remarks>
	 */
	public void PlayerManualUpdate()
	{
		if (player != null) {
			if (player.timerType == CriMana.Player.TimerType.User) {
				player.UpdateWithUserTime(0ul, 1000ul);
			} else {
				player.Update();
			}
			bool isMaterialAvailableCurrent;
			if (player.isFrameAvailable) {
				isMaterialAvailableCurrent = player.UpdateMaterial(material);
				if (isMaterialAvailableCurrent) {
					OnMaterialUpdated();
				}
			} else {
				isMaterialAvailableCurrent = false;
			}
			if (isMaterialAvailable != isMaterialAvailableCurrent) {
				isMaterialAvailable = isMaterialAvailableCurrent;
				OnMaterialAvailableChanged();
			}
		}
	}

	#region MonoBehavior Inherited Methods
	protected virtual void Awake()
	{
		PlayerManualInitialize();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.wasDisabled && this.player != null && this.player.isAlive) {
			this.player.Pause(this.wasPausedOnDisable);
			if (this.restartOnEnable) {
				StartCoroutine(RestartPlayerRoutine());
			}
		}
		this.wasDisabled = false;
#if UNITY_EDITOR
#if UNITY_2017_2_OR_NEWER
		UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		UnityEditor.EditorApplication.pauseStateChanged += OnPauseStateChanged;
#else
		UnityEditor.EditorApplication.playmodeStateChanged += OnPlaymodeStateChange;
#endif
#endif
	}

	private IEnumerator RestartPlayerRoutine()
	{
		if (player.status == CriMana.Player.Status.Playing || player.status == CriMana.Player.Status.PlayEnd) {
			this.Stop();
			while (this.player.status != CriMana.Player.Status.Stop) {
				if (this.player.status != CriMana.Player.Status.StopProcessing) {
					yield break;
				}
				yield return frameEnd;
			}
			this.Play();
			/* Re-applying pause */
			this.player.Pause(this.wasPausedOnDisable);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.player != null && this.player.isAlive) {
			this.wasPausedOnDisable = this.player.IsPaused();
			this.player.Pause(true);
		}
#if UNITY_EDITOR
#if UNITY_2017_2_OR_NEWER
		UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		UnityEditor.EditorApplication.pauseStateChanged -= OnPauseStateChanged;
#else
		UnityEditor.EditorApplication.playmodeStateChanged -= OnPlaymodeStateChange;
#endif
#endif
		this.wasDisabled = true;
	}

	protected virtual void OnDestroy()
	{
		RenderTargetManualFinalize();
		PlayerManualFinalize();
	}

	protected virtual void Start()
	{
		PlayerManualSetup();
		isMonoBehaviourStartCalled = true;

		if (!RenderTargetManualSetup()) {
			Destroy(this);
		}
	}

	public override void CriInternalUpdate()
	{
		PlayerManualUpdate();
	}

	// Render the movie picture to target material.
	public virtual void RenderMovie()
	{
		player.OnWillRenderObject(this);
	}

	// The movie picture is always rendered to target material whenever owner object is visible or not.
	public override void CriInternalLateUpdate()
	{
		if (renderMode == RenderMode.Always) {
			player.OnWillRenderObject(this);
		}
	}

	// If owner object is visible the movie picture is rendered to target material.
	protected virtual void OnWillRenderObject()
	{
		if (renderMode == RenderMode.OnVisibility) {
			player.OnWillRenderObject(this);
		}
	}

#if UNITY_EDITOR
	private void OnPlaymodeStateChange()
	{
		bool paused = UnityEditor.EditorApplication.isPaused;
		if (!isApplicationPaused && isEditorPaused != paused) {
			ProcessApplicationPause(paused);
			isEditorPaused = paused;
		}
	}

#if UNITY_2017_2_OR_NEWER
	private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
	{
		OnPlaymodeStateChange();
	}
	private void OnPauseStateChanged(UnityEditor.PauseState state)
	{
		OnPlaymodeStateChange();
	}
#endif

#endif

	void OnApplicationPause(bool appPause)
	{
#if UNITY_EDITOR
		if (!isEditorPaused && isApplicationPaused != appPause) {
			ProcessApplicationPause(appPause);
			isApplicationPaused = appPause;
		}
#else
		ProcessApplicationPause(appPause);
#endif
	}

	void ProcessApplicationPause(bool appPause)
	{
		if (onApplicationPauseCallback != null) {
			onApplicationPauseCallback(this, appPause);
			return;
		}


		if (appPause) {
			unpauseOnApplicationUnpause = !player.IsPaused();
			if (unpauseOnApplicationUnpause) {
				player.PauseOnApplicationPause(true);
			}
		} else {
			if (unpauseOnApplicationUnpause) {
				player.PauseOnApplicationPause(false);
			}
			unpauseOnApplicationUnpause = false;
		}
	}

#if UNITY_EDITOR
	protected virtual void OnDrawGizmos()
	{
		if ((player != null) && (player.status == CriMana.Player.Status.Playing)) {
			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.8f);
		} else {
			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
		}

		Gizmos.DrawLine(this.transform.position, new Vector3(0, 0, 0));
	}
#endif
	#endregion


	#region Private Methods
	private void CreateMaterial()
	{
		_material = new Material(Shader.Find("VertexLit"));
		_material.name = "CriMana-MovieMaterial";
		materialOwn = true;
	}

	/*
		private System.Collections.IEnumerator CallPluginAtEndOfFrames()
		{
			while (true) {
				yield return new WaitForEndOfFrame();
				player.IssuePluginEvent();
			}
		}
	*/
	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* end of file */
