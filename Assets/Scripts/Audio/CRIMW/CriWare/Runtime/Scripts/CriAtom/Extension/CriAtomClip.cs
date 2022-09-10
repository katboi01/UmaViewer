/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CriWare {

namespace CriTimeline.Atom
{
	public class CriAtomClip : CriAtomClipBase
	{
		public string cueSheet;
		public string cueName;

		public override string CueName {
			get {
				return cueName;
			}
		}

		public override CriAtomExAcb GetAcb() => CriAtom.GetAcb(cueSheet);

		CriAtomCueSheet GetCueSheet() {
#if UNITY_EDITOR
			var atom = (CriAtom)UnityEngine.Object.FindObjectOfType(typeof(CriAtom));
			if (atom == null)
			{
				Debug.LogWarning("[CRIWARE] Timeline Previewer: CriAtom not set in the scene");
				return null;
			}
			return atom.GetCueSheetInternal(cueSheet);
#else
			return CriAtom.GetCueSheet(cueSheet);
#endif
		}

		public override string AcbPath {
			get {
				var sheet = GetCueSheet();
				if (string.IsNullOrEmpty(sheet.acbFile))
					return null;
				return System.IO.Path.Combine(CriWare.Common.streamingAssetsPath, sheet.acbFile);
			}
		}

		public override string AwbPath {
			get
			{
				var sheet = GetCueSheet();
				if (string.IsNullOrEmpty(sheet.awbFile))
					return null;
				return System.IO.Path.Combine(CriWare.Common.streamingAssetsPath, sheet.awbFile);
			}
		}

		public override void SetCueFromAtomSource(CriAtomSourceBase atomSource) {
			if (!(atomSource is CriAtomSource)) return;
			if (string.IsNullOrEmpty(cueSheet)) {
				cueSheet = (atomSource as CriAtomSource).cueSheet;
			}
			if (string.IsNullOrEmpty(cueName)) {
				cueName = (atomSource as CriAtomSource).cueName;
			}
		}
	}

	public abstract class CriAtomClipBase : PlayableAsset, ITimelineClipAsset {
		public bool stopWithoutRelease = false;
		public bool muted = false;
		public bool ignoreBlend = false;
		public bool loopWithinClip = false;
		public bool stopAtClipEnd = true;

		public CriAtomBehaviour templateBehaviour = new CriAtomBehaviour();

		[SerializeField, HideInInspector] private double clipDuration = 0.0;

		public ClipCaps clipCaps {
			get { return ClipCaps.Looping | ClipCaps.SpeedMultiplier | ClipCaps.Blending; }
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
			return ScriptPlayable<CriAtomBehaviour>.Create(graph, templateBehaviour);
		}

		public void SetClipDuration(double clipDuration) {
			this.clipDuration = clipDuration;
		}

		public override double duration {
			get {
				return clipDuration > 0.0 ? clipDuration : 2.0;
			}
		}

#region Abstracts
		
		public abstract string CueName { get; }

		public abstract CriAtomExAcb GetAcb();
		public abstract string AcbPath { get; }
		public abstract string AwbPath { get; }

		public abstract void SetCueFromAtomSource(CriAtomSourceBase atomSource);
		
#endregion
	}
}

} //namespace CriWare
#endif