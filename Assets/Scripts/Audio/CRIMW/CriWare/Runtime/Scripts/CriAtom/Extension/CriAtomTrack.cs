/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CriWare {

namespace CriTimeline.Atom
{
	[TrackColor(0.3317462f, 0.6611561f, 0.990566f)]
	[TrackClipType(typeof(CriAtomClipBase))]
	[TrackBindingType(typeof(CriAtomSourceBase))]
	public class CriAtomTrack : TrackAsset {
		public string m_AisacControls;
		public bool m_StopOnWrapping = true;
		public bool m_StopAtGraphEnd = true;
#if UNITY_EDITOR
		public bool m_IsRenderMono = true;
#endif

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject owner, int inputCount) {
			var mixerPlayable = ScriptPlayable<CriAtomMixerBehaviour>.Create(graph, inputCount);
			var mixerBehaviour = mixerPlayable.GetBehaviour();
			if (mixerBehaviour != null) {
				mixerBehaviour.m_Director = owner.GetComponent<PlayableDirector>();
				mixerBehaviour.m_Clips = this.GetClips();
				mixerBehaviour.m_Bind = mixerBehaviour.m_Director.GetGenericBinding(this) as CriAtomSourceBase;

				if(mixerBehaviour.m_Bind != null) {
					CriAtomClipBase criAtomClip;
					foreach (var clip in mixerBehaviour.m_Clips) {
						criAtomClip = clip.asset as CriAtomClipBase;

						criAtomClip.SetCueFromAtomSource(mixerBehaviour.m_Bind);

						clip.displayName = criAtomClip.CueName;
					}
				}
				mixerBehaviour.m_AisacControls = this.m_AisacControls;
				mixerBehaviour.m_StopOnWrapping = this.m_StopOnWrapping;
				mixerBehaviour.m_StopAtGraphEnd = this.m_StopAtGraphEnd;
			}
			return mixerPlayable;
		}
	}
}

} //namespace CriWare

#endif