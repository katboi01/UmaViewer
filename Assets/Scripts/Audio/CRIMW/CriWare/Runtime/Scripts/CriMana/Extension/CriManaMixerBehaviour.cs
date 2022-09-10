/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

//#define CRI_TIMELINE_MANA_VERBOSE_DEBUG

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CriWare {

namespace CriTimeline.Mana
{
	[Serializable]
	public class CriManaMixerBehaviour : PlayableBehaviour {
		internal PlayableDirector m_PlayableDirector;
		internal IEnumerable<TimelineClip> m_clips;
		internal CriManaMovieMaterialBase m_boundMovieMaterial;
		internal Dictionary<int, GCHandle> m_gcHandleList;
		internal bool m_frameSync;

		static private double cPreloadTimeSec = 1.0;

		private Guid? m_lastClipId = null;
		private double startTime = -1.0;

		private double m_currentSeekingFrameTime = 0f;

		private float m_originalAudioVolume = float.NaN;
		private float m_originalSubAudioVolume = float.NaN;
		private float m_originalExtraAudioVolume = float.NaN;

		static private bool IsEditMode {
			get {
#if UNITY_EDITOR
				if (EditorApplication.isPlaying == false) {
					return true;
				}
#endif
				return false;
			}
		}

		private enum MovieMixerState {
			Preloading,
			Ready,
			Playing,
			Stopping,
			Stopped
		}
		private MovieMixerState m_movieMixerState = MovieMixerState.Stopped;

		private void KeepAudioVolume(bool fadeAudio) {
			m_originalAudioVolume = fadeAudio ? m_boundMovieMaterial.player.GetVolume() : float.NaN;
#if !UNITY_WEBGL
			m_originalSubAudioVolume = fadeAudio ? m_boundMovieMaterial.player.GetSubAudioVolume() : float.NaN;
			m_originalExtraAudioVolume = fadeAudio ? m_boundMovieMaterial.player.GetExtraAudioVolume() : float.NaN;
#endif
		}

		private bool PlayMovie(CriManaClipBase clipAsset, int startFrame, double startTime) {
			if (m_boundMovieMaterial == null ||
				m_boundMovieMaterial.player == null ||
				m_boundMovieMaterial.player.status == CriMana.Player.Status.StopProcessing ||
				m_boundMovieMaterial.player.status == CriMana.Player.Status.Error
				) {
				return false;
			}

			/**
			 * Set render target explicitly when in edit mode
			 * (since the Start method in the movie controller component will not run)
			 */
			if (IsEditMode) {
				if (m_boundMovieMaterial is CriManaMovieController) {
					var casted = m_boundMovieMaterial as CriManaMovieController;
					if (casted.target == null) {
						casted.target = casted.gameObject.GetComponent<Renderer>();
					}
					if (casted.target == null) {
						Debug.LogWarning("[CRIWARE] Missing render target on CriManaMovieController: Please add a renderer to the GameObject or specify the target manually.");
						return false;
					}
				} else if (m_boundMovieMaterial is CriManaMovieControllerForUI) {
					var casted = m_boundMovieMaterial as CriManaMovieControllerForUI;
					if (casted.target == null) {
						casted.target = casted.gameObject.GetComponent<UnityEngine.UI.Graphic>();
					}
					if (casted.target == null) {
						Debug.LogWarning("[CRIWARE] Missing render target on CriManaMovieControllerForUI: Please add a renderer to the GameObject or specify the target manually.");
						return false;
					}
				}
			}

			/**
			 * if (clipAsset == null) then play the prepared movie
			 * otherwise set new movie to play
			 */
			if (clipAsset != null) {
				if (clipAsset.guid != m_lastClipId) {
#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
					Debug.Log("Set <color=white>new</color> movie");
#endif
					if (clipAsset.MovieData != null) {
						if (!clipAsset.gcHandle.IsAllocated) {
							Debug.LogError("[CRIWARE] GCHandle is not Allocated!");
							return false;
						}
						m_boundMovieMaterial.player.SetData(clipAsset.gcHandle.AddrOfPinnedObject(), clipAsset.MovieData.LongLength);
					} else if (string.IsNullOrEmpty(clipAsset.MoviePath) == false) {
						m_boundMovieMaterial.player.SetFile(null, clipAsset.MoviePath);
					} else {
						/* empty clip */
						return false;
					}

					m_boundMovieMaterial.player.Loop(clipAsset.m_loopWithinClip);
					m_lastClipId = clipAsset.guid;
				}
				if (startFrame > 0) {
#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
					Debug.Log("Set start Frame <color=white>at</color> " + startFrame);
#endif
					m_boundMovieMaterial.player.SetSeekPosition(startFrame);
				} else {
#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
					Debug.Log("<color=white>Reset</color> Frame");
#endif
					m_boundMovieMaterial.player.SetSeekPosition(0);

					KeepAudioVolume(clipAsset.m_fadeAudio);
				}
			}

#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
			Debug.Log("<color=yellow>PlayMovie</color>");
#endif
			m_boundMovieMaterial.timerType = m_frameSync ? CriMana.Player.TimerType.User : CriMana.Player.TimerType.Audio;
			m_boundMovieMaterial.player.Start();
			this.startTime = startTime;

			return true;
		}

		private bool PrepareMovie(CriManaClipBase clipAsset) {
#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
			Debug.Log("<color=green>PrepareMovie</color>");
#endif
			if (m_boundMovieMaterial == null ||
				m_boundMovieMaterial.player == null ||
				m_boundMovieMaterial.player.status != CriMana.Player.Status.Stop
				) {
				return false;
			}

			if (clipAsset.MovieData != null) {
				if (!clipAsset.gcHandle.IsAllocated) {
					Debug.LogError("[CRIWARE] GCHandle is not Allocated!");
					return false;
				}
				m_boundMovieMaterial.player.SetData(clipAsset.gcHandle.AddrOfPinnedObject(), clipAsset.MovieData.LongLength);
			} else if (string.IsNullOrEmpty(clipAsset.MoviePath) == false) {
				m_boundMovieMaterial.player.SetFile(null, clipAsset.MoviePath);
			} else {
				/* empty clip */
				return false;
			}

			m_boundMovieMaterial.player.Loop(clipAsset.m_loopWithinClip);
			m_boundMovieMaterial.player.SetSeekPosition(0);
			m_lastClipId = clipAsset.guid;

			m_boundMovieMaterial.player.Prepare();

			KeepAudioVolume(clipAsset.m_fadeAudio);

			return true;
		}

		private bool StopMovie() {
#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
			Debug.Log("<color=red>StopMovie</color>");
#endif
			if (m_boundMovieMaterial == null || m_boundMovieMaterial.player == null) { return false; }

			m_boundMovieMaterial.player.Stop();
			m_lastClipId = null;

			if (!float.IsNaN(m_originalAudioVolume)) {
				m_boundMovieMaterial.player.SetVolume(m_originalAudioVolume);
#if !UNITY_WEBGL
				m_boundMovieMaterial.player.SetSubAudioVolume(m_originalSubAudioVolume);
				m_boundMovieMaterial.player.SetExtraAudioVolume(m_originalExtraAudioVolume);
#endif
			}

			return true;
		}

		private bool StopForSeekMovie() {
#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
			Debug.Log("<color=#f26fe7>StopForSeekMovie</color>");
#endif
			if (m_boundMovieMaterial == null || m_boundMovieMaterial.player == null) { return false; }

			m_boundMovieMaterial.player.StopForSeek();
			return true;
		}

		private static bool IsPlayerPreparing(CriMana.Player player) {
			var status = player.status;
			return status == CriMana.Player.Status.Dechead ||
					status == CriMana.Player.Status.Prep ||
					status == CriMana.Player.Status.WaitPrep;
		}

		private static bool IsPlayerStopped(CriMana.Player player) {
			var status = player.status;
			return status == CriMana.Player.Status.Stop;
		}

		private static bool IsPlayerError(CriMana.Player player) {
			return player.status == CriMana.Player.Status.Error;
		}

		private static bool IsPlayerReadyOrPlaying(CriMana.Player player) {
			return (player.status == CriMana.Player.Status.Ready ||
				player.status == CriMana.Player.Status.ReadyForRendering ||
				player.status == CriMana.Player.Status.Playing);
		}

		private enum ClipState {
			Idle,
			Prepare,
			Play,
			Seek
		}

		private async void ProcessFrameOnSeeking(TimelineClip activeClip, CriManaClipBase clip, double frameTime) {
			if (m_boundMovieMaterial == null ||
				m_boundMovieMaterial.player == null ||
				activeClip == null ||
				clip == null) {
				return;
			}

			m_currentSeekingFrameTime = frameTime;
			await Task.Delay(1);
			if (m_currentSeekingFrameTime != frameTime /* seeking frame changed */
				|| m_boundMovieMaterial.player == null /* clip been moved out of track */
				) {
				return;
			}

			this.ForceSyncedStop();

			if (clip.MovieData != null) {
				if (!clip.gcHandle.IsAllocated) {
					Debug.LogError("[CRIWARE] GCHandle is not Allocated!");
					return;
				}
				if (clip.gcHandle.Target != null) {
					m_boundMovieMaterial.player.SetData(clip.gcHandle.AddrOfPinnedObject(), clip.MovieData.LongLength);
				} else {
					/* empty clip */
					return;
				}
			} else if (string.IsNullOrEmpty(clip.MoviePath) == false) {
				m_boundMovieMaterial.player.SetFile(null, clip.MoviePath);
			} else {
				return;
			}

			int startFrame = clip.GetSeekFrame(frameTime - activeClip.start, clip.m_loopWithinClip);
			m_boundMovieMaterial.player.SetSeekPosition(startFrame);
			PausePlayer(true);
			m_boundMovieMaterial.player.Start();
			m_lastClipId = clip.guid;

			bool movieInfoReplaced = false;
			while (IsPlayerPreparing(m_boundMovieMaterial.player) ||
					!m_boundMovieMaterial.player.HasRenderedNewFrame()) {
				await Task.Delay(1);
				if (m_boundMovieMaterial.player == null) { return; } /* clip been moved out of track */
				if (m_currentSeekingFrameTime != frameTime) { break; } /* seeking frame changed */

				m_boundMovieMaterial.PlayerManualUpdate();
				m_boundMovieMaterial.player.OnWillRenderObject(m_boundMovieMaterial);
				if (movieInfoReplaced == false && IsPlayerReadyOrPlaying(m_boundMovieMaterial.player)) {
					clip.ReplaceMovieInfo(m_boundMovieMaterial.player.movieInfo);
					movieInfoReplaced = true;
				}

				if (IsPlayerError(m_boundMovieMaterial.player)) {
					Debug.LogError("[CRIWARE] An error has occured while preparing a frame.");
					break;
				}
			}

			this.ForceSyncedStop(true);
		}

		private void ForceSyncedStop(bool keepLastFrame = false) {
			if (m_boundMovieMaterial.player == null) { return; }

			if (keepLastFrame) {
				m_boundMovieMaterial.player.StopForSeek();
			} else {
				m_boundMovieMaterial.player.Stop();
			}

			while (!IsPlayerStopped(m_boundMovieMaterial.player)) {
				m_boundMovieMaterial.PlayerManualUpdate();
			}
			/* refresh last frame */
			m_boundMovieMaterial.PlayerManualUpdate();
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
			if (playable.GetInputCount() <= 0 || playerData == null) {
				return;
			}

			if (m_PlayableDirector.state == UnityEngine.Playables.PlayState.Paused) {
				PausePlayer(true);
			} else {
				PausePlayer(false);
			}

			double frameTime = m_PlayableDirector.time;

			ClipState clipState = ClipState.Idle;
			TimelineClip activeClip = null;
			CriManaClipBase clipAsset = null;
			foreach (var clip in m_clips) {
				if (frameTime >= clip.start && frameTime < clip.end) {
					if (IsEditMode && m_PlayableDirector.state == PlayState.Paused) {
						clipState = ClipState.Seek;
					} else {
						clipState = ClipState.Play;
					}
					activeClip = clip;
					break;
				} else if (frameTime >= clip.start - cPreloadTimeSec && frameTime <= clip.start) {
					if (m_PlayableDirector.state == UnityEngine.Playables.PlayState.Playing) {
						clipState = ClipState.Prepare;
					}
					activeClip = clip;
				}
			}

			if (clipState != ClipState.Idle) {
				if (activeClip != null) {
					clipAsset = activeClip.asset as CriManaClipBase;
				}
				if (activeClip == null || clipAsset == null) {
					/* impossible if clip system works */
					Debug.LogWarning("[CRIWARE] Timeline / Mana: null clip");
					return;
				}
			}

			if (clipState == ClipState.Seek) {
				ProcessFrameOnSeeking(activeClip, clipAsset, frameTime);
				return;
			} else if (clipState == ClipState.Idle) {
				this.StopMovie();
				m_boundMovieMaterial.PlayerManualUpdate();
				return;
			}

			var playerStatus = m_boundMovieMaterial.player.status;

			/**
			 * Cautions:
			 * 1. activeClip / clipAsset should never be used when clipState == ClipState.Idle
			 * 2. Each case should cover all ClipState variations.
			 */
			switch (m_movieMixerState) {
				case MovieMixerState.Stopped:
					if (clipState == ClipState.Prepare) {
						if (this.PrepareMovie(clipAsset)) {
							m_movieMixerState = MovieMixerState.Preloading;
						}
					} else if (clipState == ClipState.Play) {
						if (this.PlayMovie(clipAsset, clipAsset.GetSeekFrame(frameTime - activeClip.start, clipAsset.m_loopWithinClip), frameTime)) {
							m_movieMixerState = MovieMixerState.Playing;
						}
					}
					break;
				case MovieMixerState.Preloading:
					if (clipState != ClipState.Prepare && clipState != ClipState.Play) {
						if (this.StopMovie()) {
							m_movieMixerState = MovieMixerState.Stopping;
						}
					} else {
						if (playerStatus == CriMana.Player.Status.Ready ||
							playerStatus == CriMana.Player.Status.ReadyForRendering) {
							if (m_boundMovieMaterial.player.movieInfo != null) {
								m_movieMixerState = MovieMixerState.Ready;
							} else {
								if (this.StopMovie()) {
									m_movieMixerState = MovieMixerState.Stopping;
								}
							}
						} else {
							m_boundMovieMaterial.PlayerManualUpdate();
						}
					}
					break;
				case MovieMixerState.Ready:
					if ((clipState == ClipState.Play || clipState == ClipState.Seek) && clipAsset.guid == m_lastClipId) {
						if (this.PlayMovie(null, clipAsset.GetSeekFrame(frameTime - activeClip.start, clipAsset.m_loopWithinClip), frameTime)) {
							m_movieMixerState = MovieMixerState.Playing;
						}
					}
					/* Do nothing when clipState == ClipState.Prepare */
					if (clipState == ClipState.Idle || clipAsset.guid != m_lastClipId) {
						if (this.StopMovie()) {
							m_movieMixerState = MovieMixerState.Stopping;
						}
					}
					break;
				case MovieMixerState.Playing:
					if (clipState != ClipState.Play || clipAsset.guid != m_lastClipId) {
						if (this.StopMovie()) {
							m_movieMixerState = MovieMixerState.Stopping;
						}
						break;
					}
					if (clipState != ClipState.Idle && clipAsset.IsMovieInfoReady == false) {
						if (playerStatus == CriMana.Player.Status.Ready ||
							playerStatus == CriMana.Player.Status.ReadyForRendering ||
							playerStatus == CriMana.Player.Status.Playing
						) {
							clipAsset.ReplaceMovieInfo(m_boundMovieMaterial.player.movieInfo);
						}
					}
					if (m_boundMovieMaterial.timerType == CriMana.Player.TimerType.User) {
						uint userTime = (uint)((frameTime - this.startTime) * 1000.0);
						m_boundMovieMaterial.player.UpdateWithUserTime(userTime, 1000ul);
					}
					break;
				case MovieMixerState.Stopping:
					if (m_boundMovieMaterial.timerType == CriMana.Player.TimerType.User) {
						uint userTime = (uint)((frameTime - this.startTime) * 1000.0);
						m_boundMovieMaterial.player.UpdateWithUserTime(userTime, 1000ul);
					}
					if (playerStatus == CriMana.Player.Status.Stop) {
						m_movieMixerState = MovieMixerState.Stopped;
					}
					break;
				default:
					break;
			}

			/* Update movie material in Editor Mode */
			if (m_boundMovieMaterial != null && m_boundMovieMaterial.player != null) {
				var _seekPlaybackIEnumrator = seekPlaybackEnumerator();
				do {    /* Forcing update on intermediate states */
					if (startTime >= 0) {
						m_boundMovieMaterial.PlayerManualUpdate();
					}
				} while (_seekPlaybackIEnumrator.MoveNext());

				if (m_boundMovieMaterial.renderMode == CriManaMovieMaterialBase.RenderMode.Always) {
					m_boundMovieMaterial.player.OnWillRenderObject(m_boundMovieMaterial);
				}
			}

			/* Fade Movie Material and Audio Volume */
			var weight = Mathf.Min(
				clipAsset.m_fadeinCurve.Evaluate(Mathf.Approximately(clipAsset.m_fadeinDuration, 0f) ? 1f : Mathf.InverseLerp((float)activeClip.start, (float)activeClip.start + clipAsset.m_fadeinDuration, (float)frameTime)),
				clipAsset.m_fadeoutCurve.Evaluate(Mathf.Approximately(clipAsset.m_fadeoutDuration, 0f) ? 0f : Mathf.InverseLerp((float)activeClip.end - clipAsset.m_fadeoutDuration, (float)activeClip.end, (float)frameTime))
			);
			if (!float.IsNaN(m_originalAudioVolume) && m_movieMixerState == MovieMixerState.Playing)
			{
				m_boundMovieMaterial.player.SetVolume(weight * m_originalAudioVolume);
#if !UNITY_WEBGL
				m_boundMovieMaterial.player.SetSubAudioVolume(weight * m_originalSubAudioVolume);
				m_boundMovieMaterial.player.SetExtraAudioVolume(weight * m_originalExtraAudioVolume);
#endif
			}
			m_boundMovieMaterial.material.SetFloat("_Transparency", 1f - weight);
		}

		IEnumerator seekPlaybackEnumerator() {
			if (m_PlayableDirector.state != PlayState.Playing) {
				switch (m_boundMovieMaterial.player.status) {
					case CriMana.Player.Status.Dechead:
					case CriMana.Player.Status.WaitPrep:
					case CriMana.Player.Status.Prep:
					case CriMana.Player.Status.StopProcessing:
						yield return 0;
						break;
					default:
						break;
				}
			}
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info) {
			base.OnBehaviourPlay(playable, info);

			PausePlayer(false);
		}

		public override void OnBehaviourPause(Playable playable, FrameData info) {
			base.OnBehaviourPause(playable, info);

			PausePlayer(true);
		}

		private void PausePlayer(bool pause) {
			if (m_boundMovieMaterial == null || m_boundMovieMaterial.player == null) { return; }
			m_boundMovieMaterial.player.Pause(pause);
		}

		public override void OnGraphStart(Playable playable) {
#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
			Debug.Log("<color=white>OnGraphStart" + (IsEditMode ? " (Editor)</color>" : "</color>"));
#endif
			base.OnGraphStart(playable);

			if (IsEditMode) {
				if (m_boundMovieMaterial != null) {
					m_boundMovieMaterial.PlayerManualInitialize();
					m_boundMovieMaterial.PlayerManualSetup();
					m_boundMovieMaterial.RenderTargetManualSetup();
				}
			}

			m_gcHandleList = new Dictionary<int, GCHandle>();
			foreach (var clip in m_clips) {
				var manaClip = clip.asset as CriManaClipBase;
				if (manaClip.MovieData == null) {
					continue;
				}
				if (m_gcHandleList.ContainsKey(manaClip.DataId) == false) {
					manaClip.gcHandle = GCHandle.Alloc(manaClip.MovieData, GCHandleType.Pinned);
					m_gcHandleList.Add(manaClip.DataId, manaClip.gcHandle);
				} else {
					manaClip.gcHandle = m_gcHandleList[manaClip.DataId];
				}
			}
		}

		public override void OnGraphStop(Playable playable) {
#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
			Debug.Log("<color=white>OnGraphStop" + (IsEditMode ? " (Editor)</color>" : "</color>"));
#endif
			base.OnGraphStop(playable);

			if (IsEditMode) {
				if (this.StopMovie()) {
					m_movieMixerState = MovieMixerState.Stopping;
				}
				if (m_boundMovieMaterial != null) {
					m_boundMovieMaterial.PlayerManualFinalize();
					m_boundMovieMaterial.RenderTargetManualFinalize();
				}
			}
			foreach (var item in m_gcHandleList) {
				item.Value.Free();
			}
		}

		public override void OnPlayableCreate(Playable playable) {
#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
			Debug.Log("<color=white>OnPlayableCreate" + (IsEditMode ? " (Editor)</color>" : "</color>"));
#endif
			base.OnPlayableCreate(playable);
			if (IsEditMode) {
				if (CriManaPlugin.IsLibraryInitialized() == false) {
					CriWareInitializer cfg = GameObject.FindObjectOfType<CriWareInitializer>();
					if (cfg != null) {
						CriWareInitializer.InitializeMana(cfg.manaConfig);
					} else {
						CriWareInitializer.InitializeMana(new CriManaConfig());
						Debug.Log("[CRIWARE] Timeline / Mana: Can't find CriWareInitializer component; Using default parameters in edit mode.");
					}
				}
			}

			m_lastClipId = null;
			m_movieMixerState = MovieMixerState.Stopped;
		}

		public override void OnPlayableDestroy(Playable playable) {
#if CRI_TIMELINE_MANA_VERBOSE_DEBUG
			Debug.Log("<color=white>OnPlayableDestroy" + (IsEditMode ? " (Editor)</color>" : "</color>"));
#endif
			base.OnPlayableDestroy(playable);

			if (IsEditMode == false) {
				this.StopMovie();
			}
		}
	}
}

} //namespace CriWare

#endif
