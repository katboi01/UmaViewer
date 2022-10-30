/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/
//#define CRI_TIMELINE_ATOM_VERBOSE_DEBUG

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using UnityEditor;
using UnityEngine;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CriWare {

namespace CriTimeline.Atom {

	[CustomPropertyDrawer(typeof(CriAtomBehaviour))]
	public class CriAtomTimelineDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SerializedProperty volumeProp = property.FindPropertyRelative("volume");
			SerializedProperty pitchProp = property.FindPropertyRelative("pitch");
			SerializedProperty aisacProp = property.FindPropertyRelative("AISACValue");

			EditorGUILayout.PropertyField(volumeProp);
			EditorGUILayout.PropertyField(pitchProp);
			EditorGUILayout.PropertyField(aisacProp);
		}
	}

	[CustomEditor(typeof(CriAtomClipBase)), CanEditMultipleObjects]
	public class CriAtomClipEditor : UnityEditor.Editor { }

	[CustomEditor(typeof(CriAtomTrack))]
	public class CriAtomTrackEditor : UnityEditor.Editor {
		private CriAtomTrack m_object;

		public void OnEnable() {
			if (target != null) {
				m_object = target as CriAtomTrack;
			}
		}

		public override void OnInspectorGUI() {
			if (m_object == null) {
				return;
			}

			serializedObject.Update();
			EditorGUI.BeginChangeCheck();

			
			EditorGUILayout.LabelField("Track Settings", EditorStyles.boldLabel);
			DrawLine(Color.black);
			m_object.m_AisacControls
				= EditorGUILayout.TextField("Aisac Control", m_object.m_AisacControls);
			m_object.m_StopOnWrapping
				= EditorGUILayout.Toggle("Stop On Wrapping", m_object.m_StopOnWrapping);
			m_object.m_StopAtGraphEnd
				= EditorGUILayout.Toggle("Stop At Graph End", m_object.m_StopAtGraphEnd);

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Unity Editor", EditorStyles.boldLabel);
			DrawLine(Color.black);

			m_object.m_IsRenderMono
				= EditorGUILayout.Toggle("Show Mono Waveform", m_object.m_IsRenderMono);

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck()) {
				EditorUtility.SetDirty(m_object);
			}
		}

		private static void DrawLine(Color color, int thickness = 1, int padding = 10) {
			Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			rect.height = thickness;
			rect.y += padding / 2;
			rect.x -= 2;
			rect.width += 6;
			EditorGUI.DrawRect(rect, color);
		}
	}

#if UNITY_2019_3_OR_NEWER

	[CustomTimelineEditor(typeof(CriAtomClipBase))]
	public class CriAtomClipWaveformEditor : UnityEditor.Timeline.ClipEditor {
		private Dictionary<CriAtomClipBase, CriAtomClipWaveformPreviewer> atomClipPreviewDirectory
			= new Dictionary<CriAtomClipBase, CriAtomClipWaveformPreviewer>();

		public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom) {
			base.OnCreate(clip, track, clonedFrom);
#if CRI_TIMELINE_ATOM_VERBOSE_DEBUG
			Debug.Log("[CRIWARE][Timeline] OnCreate");
#endif

			atomClipPreviewDirectory.Clear();

			CriAtomClipBase atomClip = clip.asset as CriAtomClipBase;
			atomClipPreviewDirectory.Add(atomClip, new CriAtomClipWaveformPreviewer(atomClip));
		}

		public override void OnClipChanged(TimelineClip clip) {
			base.OnClipChanged(clip);
#if CRI_TIMELINE_ATOM_VERBOSE_DEBUG
			Debug.Log("[CRIWARE][Timeline] OnClipChanged");
#endif

			CriAtomClipBase atomClip = clip.asset as CriAtomClipBase;
			if (atomClipPreviewDirectory.ContainsKey(atomClip)) {
				return;
			}
			atomClipPreviewDirectory.Add(atomClip, new CriAtomClipWaveformPreviewer(atomClip));
		}

		public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region) {
			base.DrawBackground(clip, region);
#if CRI_TIMELINE_ATOM_VERBOSE_DEBUG
			Debug.Log("[CRIWARE][Timeline] DrawBackground");

			Debug.Log("region.startTime : " + region.startTime.ToString() + "region.endTime : " + region.endTime.ToString());
#endif

			CriAtomClipBase atomClip = clip.asset as CriAtomClipBase;
			CriAtomClipWaveformPreviewer atomClipPreviewer;
			if (!atomClipPreviewDirectory.TryGetValue(atomClip, out atomClipPreviewer)) {
#if CRI_TIMELINE_ATOM_VERBOSE_DEBUG
				Debug.LogError("[CRIWARE][Timeline] not contains key : " + clip.displayName);
#endif
				return;
			}
			if (!atomClipPreviewer.HasDecodeTask()) {
				atomClipPreviewDirectory.Remove(atomClip);
				atomClipPreviewDirectory.Add(atomClip, new CriAtomClipWaveformPreviewer(atomClip));
			}

			if (!atomClipPreviewer.HasPreviewData()) {
				/* While decoding or decode error. Skip renderring. */
				return;
			}

			atomClipPreviewer.IsLooping = atomClip.loopWithinClip;
			atomClipPreviewer.IsMuted = atomClip.muted;
			CriAtomTrack atomTrack = clip.parentTrack as CriAtomTrack;
			if (atomTrack.m_IsRenderMono) {
				atomClipPreviewer.ChannelMode = CriAtomClipWaveformPreviewer.RenderChannelMode.Mono;
			} else {
				atomClipPreviewer.ChannelMode = CriAtomClipWaveformPreviewer.RenderChannelMode.All;
			}

			if (Event.current.type == EventType.Repaint) {
				atomClipPreviewer.RenderMaterial(region);
			}
		}
	}

	public partial class CriAtomClipWaveformPreviewer {
		/* Number of drawing channels */
		public enum RenderChannelMode {
			Mono = 0, /* Mono(Left Channel) */
			All       /* All Channels */
		}

		public bool IsLooping = false;
		public bool IsMuted = true;
		public RenderChannelMode ChannelMode = RenderChannelMode.Mono;

		private struct CriAtomClipWaveformInfo {
			public CriAtomEx.WaveformInfo waveformInfo;
			public double waveDurationSecond;
			public Int16[] lpcmBufferByInterleave;
		}

		private Task decodeTask = null;
		private CriAtomClipWaveformInfo atomClipWaveformInfo;
		private CriAtomClipWaveformCanvas simpleCanvas;
		private CriAtomClipWaveformCanvas regionalCanvas;

		public CriAtomClipWaveformPreviewer(CriAtomClipBase criAtomClip) {
			if (!CriAtomTimelinePreviewer.IsInitialized || string.IsNullOrEmpty(criAtomClip.AcbPath)) {
				return;
			}
			var acb = CriAtomTimelinePreviewer.Instance.GetAcb(criAtomClip.AcbPath, criAtomClip.AwbPath);
			if (acb == null) {
				Debug.LogError("[CRIWARE][Timeline] faild to load acb object.");
				return;
			}

			decodeTask = Task.Run(() =>
			{
				this.atomClipWaveformInfo = new CriAtomClipWaveformInfo();
				if (!acb.GetWaveFormInfo(criAtomClip.CueName, out this.atomClipWaveformInfo.waveformInfo)) {
					/* without waveform */
					return;
				}
				this.atomClipWaveformInfo.lpcmBufferByInterleave = new Int16[this.atomClipWaveformInfo.waveformInfo.numSamples * this.atomClipWaveformInfo.waveformInfo.numChannels];
				if (!LoadWaveform(acb.nativeHandle, criAtomClip.CueName, ref this.atomClipWaveformInfo.lpcmBufferByInterleave)) {
					this.atomClipWaveformInfo.lpcmBufferByInterleave = null;
					return;
				}
				this.atomClipWaveformInfo.waveDurationSecond = this.atomClipWaveformInfo.waveformInfo.numSamples / (double)this.atomClipWaveformInfo.waveformInfo.samplingRate;
			});

			simpleCanvas = new CriAtomClipWaveformCanvas();
			simpleCanvas.updated += () => {
				TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
			};
			regionalCanvas = new CriAtomClipWaveformCanvas();
			regionalCanvas.updated += () => {
				TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
			};
		}

		~CriAtomClipWaveformPreviewer() {
			if (decodeTask != null) {
				if (!decodeTask.IsCompleted) {
					decodeTask.Wait();
				}
				decodeTask.Dispose();
				decodeTask = null;
			}
			simpleCanvas.Dispose();
			simpleCanvas = null;
			regionalCanvas.Dispose();
			regionalCanvas = null;
		}

		public bool HasDecodeTask() {
			return decodeTask == null ? false : true;
		}

		public bool HasPreviewData() {
			if (decodeTask == null || decodeTask.IsFaulted || decodeTask.IsCanceled || !decodeTask.IsCompleted) {
				return false;
			}
			/* decodeTask.IsCompleted == true and buffer == null is decode error. */
			return this.atomClipWaveformInfo.lpcmBufferByInterleave != null;
		}

		public void RenderMaterial(ClipBackgroundRegion region) {
			if (region.position.height <= 0 | region.position.width <= 0) {
				/* region.position is invalid. */
				return;
			}

			simpleCanvas.UpdateTexture(ref this.atomClipWaveformInfo, this.IsLooping, renderOnce:true);
			regionalCanvas.UpdateTexture(ref this.atomClipWaveformInfo, this.IsLooping, region);
				
			simpleCanvas.SetMatParams(
				channelMode: ChannelMode,
				numChannels: this.atomClipWaveformInfo.waveformInfo.numChannels,
				isLooping: this.IsLooping,
				isMuted: this.IsMuted,
				scale: (float)((region.endTime - region.startTime) / this.atomClipWaveformInfo.waveDurationSecond),
				offset: (float)(region.startTime / this.atomClipWaveformInfo.waveDurationSecond));

			regionalCanvas.SetMatParams(
				channelMode: ChannelMode,
				numChannels: this.atomClipWaveformInfo.waveformInfo.numChannels,
				isLooping: this.IsLooping,
				isMuted: this.IsMuted,
				scale: 1f,
				offset: 0);

			if (regionalCanvas.IsDirty) {
				simpleCanvas.Draw(region.position);
			} else {
				regionalCanvas.Draw(region.position);
			}
		}

		private static bool LoadWaveform(IntPtr acbHn, string cueName, ref System.Int16[] decodeLpcmBuffer) {
			var gcHandle = GCHandle.Alloc(decodeLpcmBuffer, GCHandleType.Pinned);
			var result = CRIWAREF2B79675(acbHn, cueName, gcHandle.AddrOfPinnedObject(), decodeLpcmBuffer.LongLength);
			gcHandle.Free();
			return result;
		}

		#region DLL Import
#if !CRIWARE_ENABLE_HEADLESS_MODE
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		private static extern bool CRIWAREF2B79675(IntPtr acbHn, string cue_name, IntPtr decode_lpcm_buffer, System.Int64 decodeLpcmBufferLength);
#else
		private static bool CRIWAREF2B79675(IntPtr acbHn, string cue_name, IntPtr decode_lpcm_buffer, System.Int64 decodeLpcmBufferLength) {return false;}
#endif
		#endregion
	}

#endif //UNITY_2019_3_OR_NEWER

}

} //namespace CriWare

#endif //UNITY_2018_1_OR_NEWER