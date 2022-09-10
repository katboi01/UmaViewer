/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CriWare {

namespace CriTimeline.Atom
{
	[Serializable]
	public class CriAtomMixerBehaviour : PlayableBehaviour {
		internal PlayableDirector m_Director;
		internal IEnumerable<TimelineClip> m_Clips;
		internal CriAtomSourceBase m_Bind;
		internal string m_AisacControls;
		internal bool m_StopOnWrapping;
		internal bool m_StopAtGraphEnd;

		public Guid m_Guid { get; private set; }

		private const int cScratchTimeIntervalMs = 200;
		private DateTime m_lastScrubTime;
		private double m_lastDirectorTime = 0;

		public override void OnPlayableCreate(Playable playable) {
			base.OnPlayableCreate(playable);
			m_Guid = Guid.NewGuid();
			if (IsEditor) {
				if (CriAtomPlugin.IsLibraryInitialized() == false) {
					CriWareInitializer criInitializer = GameObject.FindObjectOfType<CriWareInitializer>();
					if (criInitializer != null) {
						CriWareInitializer.InitializeAtom(criInitializer.atomConfig);
					} else {
						CriWareInitializer.InitializeAtom(new CriAtomConfig());
						Debug.Log("[CRIWARE] Timeline / Atom: Can't find CriWareInitializer component; Using default parameters in edit mode.");
					}
				}
			}
			m_lastDirectorTime = 0;
		}

		public override void OnGraphStop(Playable playable) {
			base.OnGraphStop(playable);

			if (IsEditor) {
				if (CriAtomTimelinePreviewer.IsInitialized) {
					CriAtomTimelinePreviewer.Instance.StopAllTracks();
				}
			} else {
				if (m_Bind != null && this.m_StopAtGraphEnd) {
					m_Bind.Stop();
				}
			}
			m_lastDirectorTime = 0;
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
			if (playerData == null || m_Director.time == m_lastDirectorTime) { return; }

			bool wrapped = false;
			if (m_Director.state == PlayState.Playing && m_Director.time < m_lastDirectorTime) { wrapped = true; }
			m_lastDirectorTime = m_Director.time;

			int inputPort = 0;
			foreach (var clip in m_Clips) {
				ScriptPlayable<CriAtomBehaviour> inputPlayable = (ScriptPlayable<CriAtomBehaviour>)playable.GetInput(inputPort);
				CriAtomBehaviour clipBehaviour = inputPlayable.GetBehaviour();
				CriAtomClipBase criAtomClip = clip.asset as CriAtomClipBase;
				float inputWeight = criAtomClip.ignoreBlend ? 1f : playable.GetInputWeight(inputPort);

				if (clipBehaviour != null) {
					if (m_StopOnWrapping && wrapped) {
						if (clipBehaviour.IsClipPlaying) {
							clipBehaviour.Stop(criAtomClip.stopWithoutRelease);
						}
					}

					if (m_Director.time >= clip.end ||
						m_Director.time <= clip.start) {
						if (clipBehaviour.IsClipPlaying && criAtomClip.stopAtClipEnd) {
							clipBehaviour.Stop(criAtomClip.stopWithoutRelease);
						}
					} else if (criAtomClip.muted == false) {
						long seekTimeMs = (long)((m_Director.time - clip.start) * 1000.0);
						bool isDirectorPaused = (m_Director.state == PlayState.Paused);

						var playConfig = new CriAtomClipPlayConfig(
							criAtomClip,
							seekTimeMs,
							clip.timeScale,
							criAtomClip.loopWithinClip
						);

						if (clipBehaviour.IsClipPlaying == false) { /* Entering clip for the first time */
							if (IsEditor == false) {
								clipBehaviour.Play(m_Bind, playConfig);
							} else {
								clipBehaviour.PreviewPlay(m_Guid, isDirectorPaused, playConfig);
								m_lastScrubTime = DateTime.Now;
							}
							criAtomClip.SetClipDuration(clipBehaviour.CueLength);
						} else {
							var now = DateTime.Now;
							if (IsEditor == true && isDirectorPaused &&
								now - m_lastScrubTime > new TimeSpan(0, 0, 0, 0, cScratchTimeIntervalMs)) { /* Scrubing the track */
								clipBehaviour.Stop(true);
								clipBehaviour.PreviewPlay(m_Guid, isDirectorPaused, playConfig);
								m_lastScrubTime = now;
							}
						}

						if (IsEditor == true) {
							CriAtomTimelinePreviewer.Instance.SetVolume(m_Guid, clipBehaviour.volume * inputWeight);
							CriAtomTimelinePreviewer.Instance.SetPitch(m_Guid, clipBehaviour.pitch);
							if (string.IsNullOrEmpty(m_AisacControls) == false) {
								CriAtomTimelinePreviewer.Instance.SetAISAC(m_Guid, m_AisacControls, clipBehaviour.AISACValue);
							}
							CriAtomTimelinePreviewer.Instance.PlayerUpdateParameter(m_Guid, clipBehaviour.playback);
						} else {
							m_Bind.player.SetVolume(clipBehaviour.volume * inputWeight);
							m_Bind.player.SetPitch(clipBehaviour.pitch);
							if (string.IsNullOrEmpty(m_AisacControls) == false) {
								m_Bind.player.SetAisacControl(m_AisacControls, clipBehaviour.AISACValue);
							}
							m_Bind.player.Update(clipBehaviour.playback);
						}
					}
				}

				inputPort++;
			}
		}

		static private bool IsEditor {
			get {
#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying == false) {
					return true;
				}
#endif
				return false;
			}
		}
	}

	public class CriAtomTimelinePreviewer : IDisposable {
		static private CriAtomTimelinePreviewer instance = null;
		static public CriAtomTimelinePreviewer Instance {
			get {
				if (instance == null) {
					instance = new CriAtomTimelinePreviewer();
#if UNITY_EDITOR
					UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
				}
				return instance;
			}
		}
		static public void InstanceDispose() {
			if (instance != null) {
				instance.Dispose();
				instance = null;
#if UNITY_EDITOR
				UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
			}
		}
		static public bool IsInitialized {
			get {
				if (instance == null) {
					return false;
				}
				return true;
			}
		}

#if UNITY_EDITOR
		static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange change)
		{
			if (change == UnityEditor.PlayModeStateChange.EnteredEditMode)
				InstanceDispose();
		}
#endif

		struct PlayerSource {
			public readonly CriAtomExPlayer player;
			public readonly CriAtomEx3dSource source3d;

			public PlayerSource(CriAtomEx3dListener listener) {
				this.player = new CriAtomExPlayer();
				this.source3d = new CriAtomEx3dSource();
				this.source3d.SetPosition(0, 0, 0);
				this.source3d.Update();
				this.player.Set3dSource(this.source3d);
				this.player.Set3dListener(listener);
				this.player.UpdateAll();
			}
		}

		private CriAtom atom;
		private string lastAcfFile = "";
		private Dictionary<string, CriAtomExAcb> acbTable;
		private Dictionary<Guid, PlayerSource> playerTable;  /* preview player for each track */
		private CriAtomEx3dListener listener3d;

		public CriAtomTimelinePreviewer() {
			this.acbTable = new Dictionary<string, CriAtomExAcb>();
			this.playerTable = new Dictionary<Guid, PlayerSource>();

			if (listener3d == null) {
				listener3d = new CriAtomEx3dListener();
				listener3d.SetPosition(0, 0, 0);
				listener3d.Update();
			}
		}

		public CriAtomExPlayer GetPlayer(Guid trackId) {
			if (this.playerTable.ContainsKey(trackId)) {
				return this.playerTable[trackId].player;
			} else {
				PlayerSource playerSource = new PlayerSource(this.listener3d);
				try {
					this.playerTable.Add(trackId, playerSource);
				} catch (Exception e) { /* impossible */
					Debug.LogError("[CRIWARE] Timeline Previewer: Failed adding preview player (" + e.Message + ")");
				}
				return playerSource.player; /* return the created player anyway */
			}
		}

		public void SetCue(Guid trackId, CriAtomExAcb acb, string cueName) {
			if (acb != null && string.IsNullOrEmpty(cueName) == false) {
				this.GetPlayer(trackId).SetCue(acb, cueName);
			} else {
				Debug.LogWarning("[CRIWARE] Timeline Previewer: insufficient ACB or cue name");
			}
		}

		public CriAtomExAcb GetAcb(string acbPath, string awbPath) {
			if (string.IsNullOrEmpty(acbPath)) {
				Debug.LogWarning("[CRIWARE] Timeline Previewer: cuesheet path is vacant");
				return null;
			}

			this.atom = (CriAtom)UnityEngine.Object.FindObjectOfType(typeof(CriAtom));
			if(this.atom != null)
			{
				if (lastAcfFile != this.atom.acfFile) {
					CriAtomEx.UnregisterAcf();
					CriAtomEx.RegisterAcf(null, Path.Combine(CriWare.Common.streamingAssetsPath, atom.acfFile));
					lastAcfFile = this.atom.acfFile;
				}
			}

			if (this.acbTable.ContainsKey(acbPath)) {
				return acbTable[acbPath];
			} else {
				CriAtomExAcb acb = CriAtomExAcb.LoadAcbFile(null, acbPath, awbPath);
				if (acb != null) {
					try {
						acbTable.Add(acbPath, acb);
					} catch (Exception e) {
						if (e is ArgumentException) {
							/* impossible */
							Debug.LogWarning("[CRIWARE] Timeline Previewer: ACB already existing.");
						} else {
							Debug.LogWarning("[CRIWARE] Timeline Previewer: ACB Dictionary exception: " + e.Message);
						}
					}
				} else {
					Debug.LogWarning("[CRIWARE] Timeline Previewer: Failed loading ACB/AWB file.");
				}
				return acb;
			}
		}

		public CriAtomExPlayback Play(Guid trackId) {
			return this.GetPlayer(trackId).Start();
		}

		public void StopTrack(Guid trackId, bool stopWithoutRelease = true) {
			if (stopWithoutRelease) {
				this.GetPlayer(trackId).StopWithoutReleaseTime();
			} else {
				this.GetPlayer(trackId).Stop();
			}
		}

		public void StopAllTracks(bool stopWithoutRelease = true) {
			foreach (var elem in playerTable) {
				if (stopWithoutRelease) {
					elem.Value.player.StopWithoutReleaseTime();
				} else {
					elem.Value.player.Stop();
				}
			}
		}

		public void SetStartTime(Guid trackId, long startTimeMs) {
			this.GetPlayer(trackId).SetStartTime(startTimeMs);
		}

		public void SetLoop(Guid trackId, bool sw) {
			this.GetPlayer(trackId).Loop(sw);
		}

		public void SetVolume(Guid trackId, float volume) {
			this.GetPlayer(trackId).SetVolume(volume);
		}

		public void SetPitch(Guid trackId, float pitch) {
			this.GetPlayer(trackId).SetPitch(pitch);
		}

		public void SetAISAC(Guid trackId, string controlName, float value) {
			this.GetPlayer(trackId).SetAisacControl(controlName, value);
		}

		public void PlayerUpdateParameter(Guid trackId, CriAtomExPlayback atomExPlayback) {
			this.GetPlayer(trackId).Update(atomExPlayback);
		}

		~CriAtomTimelinePreviewer() {
			this.Dispose(false);
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			foreach (var elem in playerTable.Values) {
				if (elem.player != null) {
					elem.player.Stop();
					elem.player.Set3dListener(null);
					elem.player.Set3dSource(null);
					elem.player.Dispose();
				}
				if (elem.source3d != null) {
					elem.source3d.Dispose();
				}
			}
			playerTable.Clear();

			foreach (var elem in acbTable.Values) {
				elem.Dispose();
			}
			acbTable.Clear();

			if (listener3d != null) {
				listener3d.Dispose();
				listener3d = null;
			}
		}
	}
}

} //namespace CriWare

#endif