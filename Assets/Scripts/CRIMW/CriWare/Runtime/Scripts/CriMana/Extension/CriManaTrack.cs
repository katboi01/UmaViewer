/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CriWare {

namespace CriTimeline.Mana
{
	[TrackColor(0.15f, 0.5f, 1f)]
	[TrackBindingType(typeof(CriManaMovieMaterialBase))]
	[TrackClipType(typeof(CriManaClipBase))]
	public class CriManaTrack : TrackAsset {
		public bool frameSync = false;
		public readonly Guid guid = Guid.NewGuid();

		static private Dictionary<int, Guid> bindDict = new Dictionary<int, Guid>();

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
			var mixer = ScriptPlayable<CriManaMixerBehaviour>.Create(graph, inputCount);
			var director = go.GetComponent<PlayableDirector>();
			if (director != null) {
				CriManaMovieMaterialBase boundMovieMaterial = director.GetGenericBinding(this) as CriManaMovieMaterialBase;
				CriManaMixerBehaviour bh = mixer.GetBehaviour();
				bh.m_clips = GetClips();
				bh.m_PlayableDirector = director;
				bh.m_frameSync = this.frameSync;

				foreach (var clip in bh.m_clips) {
					var clipAsset = (clip.asset as CriManaClipBase);
					clipAsset.m_clip = clip;
					clip.displayName = clipAsset.MovieName;
				}

				/* make sure binding is unique across all tracks */
				if (boundMovieMaterial != null) {
					var movieInstanceID = boundMovieMaterial.GetInstanceID();

					if (bindDict.ContainsKey(movieInstanceID) && bindDict[movieInstanceID] != guid) {
						director.SetGenericBinding(this, null);
						boundMovieMaterial = null;
						Debug.LogWarning("[CRIWARE] Binding the same movie controller to multiple tracks is not allowed. Operation has been cancelled.");
					} else {
						if (bindDict.ContainsKey(movieInstanceID)) {
							try {
								bindDict.Remove(movieInstanceID);
							} catch {
								Debug.LogError("[CRIWARE] Timeline / Mana: (Internal) Binding dictionary logic error");
							}
						}
						RemoveTrackFromBindDict(this);
						try {
							bindDict.Add(movieInstanceID, guid);
						} catch {
							Debug.LogError("[CRIWARE] Timeline / Mana: (Internal) Binding dictionary logic error");
						}
					}
				} else {
					RemoveTrackFromBindDict(this);
				}

				bh.m_boundMovieMaterial = boundMovieMaterial;
			}
			return mixer;
		}

		private void OnDestroy() {
			/* remove binding record when track is destroyed */
			RemoveTrackFromBindDict(this);
		}

		static private void RemoveTrackFromBindDict(CriManaTrack trackAsset) {
			List<int> deleteKeyList = new List<int>();
			foreach (var pair in bindDict) {
				if (pair.Value == trackAsset.guid) {
					deleteKeyList.Add(pair.Key);
				}
			}
			foreach (var key in deleteKeyList) {
				try {
					bindDict.Remove(key);
				} catch {
					Debug.LogError("[CRIWARE] Timeline / Mana: (Internal) Binding dictionary logic error");
				}
			}
			deleteKeyList.Clear();
		}
	}
}

} //namespace CriWare

#endif