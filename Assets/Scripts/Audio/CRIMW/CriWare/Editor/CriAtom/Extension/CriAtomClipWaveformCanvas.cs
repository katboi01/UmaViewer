/****************************************************************************
 *
 * Copyright (c) CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2019_3_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using UnityEditor;
using UnityEngine;
using UnityEditor.Timeline;
using UnityEngine.Events;
using System;

namespace CriWare {

namespace CriTimeline.Atom {

	public partial class CriAtomClipWaveformPreviewer {

		private class CriAtomClipWaveformCanvas : IDisposable {
			private const string SHADER = "CRIWARE/CriAtomClipWaveformRender";
			private const int DEFAULT_TEXTURE_WIDTH = 4096;
			private static readonly Color backgroundColor = Color.clear;
			private static readonly Color waveColorForAll = new Color(1.0f, 0.635f, 0.0f, 1.0f); /* yellow */
			private static readonly Color waveColorForMono = new Color(0.332f, 0.661f, 0.991f, 1f); /* blue */

			private Texture2D texture;
			private Material material;
			private int scanPosition = 0;
			private ClipBackgroundRegion lastRegion;
			private int lastWidth = 0;
			private bool lastLoop = false;
			private bool isDirty = true;
			private bool isDisposed = false;

			public UnityAction updated = null;

			public bool IsDirty {
				get { return isDirty; }
				set {
					if (isDirty = value) {
						scanPosition = 0;
					}
				}
			}

			public CriAtomClipWaveformCanvas() { 
				this.texture = null;
				this.material = new Material(Shader.Find(SHADER));
			}

			~CriAtomClipWaveformCanvas() {
				dispose();
			}

			public void Dispose() {
				dispose();
				GC.SuppressFinalize(this);
			}

			public void UpdateTexture(ref CriAtomClipWaveformInfo info, bool loop, ClipBackgroundRegion? region = null, bool renderOnce = false) {
				const int BATCH_NUM_PIXEL = 50;

				int scanWidth = region.HasValue ? (int)region.Value.position.width : DEFAULT_TEXTURE_WIDTH;

				if (texture == null) {
					texture = new Texture2D(scanWidth, info.waveformInfo.numChannels, TextureFormat.RG16, false);
					texture.wrapMode = TextureWrapMode.Clamp;
					texture.filterMode = FilterMode.Bilinear;
				} else if (renderOnce == false) {
					if (region.HasValue && lastWidth != scanWidth) {
#if UNITY_2021_2_OR_NEWER
						texture.Reinitialize(scanWidth, texture.height);
#else
						texture.Resize(scanWidth, texture.height);
#endif
						this.IsDirty = true;
						lastWidth = scanWidth;
					}
					if (region.HasValue && region.Value != lastRegion) {
						this.IsDirty = true;
						lastRegion = region.Value;
					}
					if (loop != lastLoop) {
						this.IsDirty = true;
						lastLoop = loop;
					}
				}

				if (isDirty) {
					if (this.scanPosition >= scanWidth) {
						this.IsDirty = false;
						return;
					}
				} else {
					return;
				}

				long startSmpIdx = region.HasValue
						? (long)((region.Value.startTime / info.waveDurationSecond) * info.waveformInfo.numSamples)
						: 0;
				long endSmpIdx = region.HasValue 
						? (long)((region.Value.endTime / info.waveDurationSecond) * info.waveformInfo.numSamples) 
						: (info.waveformInfo.numSamples - 1);
				long smpLength = endSmpIdx - startSmpIdx + 1;

				float samplePerPixel = smpLength / (float)scanWidth;
				if (samplePerPixel <= 0) {
					samplePerPixel = float.Epsilon;
				}

				for (int y = 0; y < texture.height; ++y) {
					for (int x = scanPosition; x < scanPosition + BATCH_NUM_PIXEL; ++x) {
						Int16 maxValue, minValue;
						long startPnt = (long)(startSmpIdx + x * samplePerPixel);
						if (loop) { 
							startPnt %= info.waveformInfo.numSamples;
						}
						long range = (long)Mathf.Ceil(samplePerPixel);
						if (startPnt + range > info.waveformInfo.numSamples) {
							range = (long)Mathf.Floor(info.waveformInfo.numSamples - startPnt);
						}
						GetMinMaxPcmArray(info.lpcmBufferByInterleave,
							startPnt,
							range,
							y,
							info.waveformInfo.numChannels,
							out minValue, out maxValue);
						texture.SetPixel(x, y, new Color(
							maxValue / (float)Int16.MaxValue,
							minValue / (float)Int16.MinValue,
							0));
					}
				}

				texture.Apply();
			
				scanPosition += BATCH_NUM_PIXEL;

				if (this.updated != null) {
					this.updated();
				}
			}

			private static void GetMinMaxPcmArray(Int16[] array, long index, long length, int channel, int numChannels, out Int16 min, out Int16 max) {
				Int16 maxValue = 0;
				Int16 minValue = 0;
				for (long i = 0; i < length; i++) {
					var currentIndex = (index + i) * numChannels  + channel;
					var amplitude = array[currentIndex];
					if (maxValue < amplitude) {
						maxValue = amplitude;
					}
					if (minValue > amplitude) {
						minValue = amplitude;
					}
				}
				max = maxValue;
				min = minValue;
			}

			public void Draw(Rect rect) {
				if (this.texture == null || this.material == null) { return; }
				Graphics.DrawTexture(rect, this.texture, this.material);
			}

			public void SetMatParams(
				RenderChannelMode channelMode,
				int numChannels,
				bool isLooping,
				bool isMuted,
				float scale,
				float offset)
			{
				this.material.SetTexture("_MainTex", this.texture);
				this.material.SetColor("_BacCol", backgroundColor);
				this.material.SetColor("_ForCol", channelMode == RenderChannelMode.All ? waveColorForAll : waveColorForMono);
				this.material.SetFloat("_Channel", channelMode == RenderChannelMode.All ? numChannels : 1);

				this.material.SetFloat("_Scale", scale);
				this.material.SetFloat("_Offset", offset);

				this.material.SetInt("_IsLoop", isLooping ? 1 : 0);
				this.material.SetInt("_IsMute", isMuted ? 1 : 0);
				this.material.SetInt("_ForceMono", channelMode == RenderChannelMode.Mono ? 1 : 0);
			}

			private void dispose() { 
				if (this.isDisposed) { return; }
				
				if (Application.isPlaying) {
					UnityEngine.Object.Destroy(this.texture);
					UnityEngine.Object.Destroy(this.material);
				} else {
					UnityEngine.Object.DestroyImmediate(this.texture);
					UnityEngine.Object.DestroyImmediate(this.material);
				}
				this.texture = null;
				this.material = null;

				this.isDisposed = true;
			}

		} //class CriAtomClipWaveformCanvas

	} //partial class CriAtomClipWaveformPreviewer

} //namespace CriTimeline.Atom

} //namespace CriWare

#endif