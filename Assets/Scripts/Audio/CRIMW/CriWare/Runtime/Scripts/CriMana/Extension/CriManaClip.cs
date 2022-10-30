/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CriWare {

namespace CriTimeline.Mana
{
	public class CriManaClip : CriManaClipBase
	{
		public string m_moviePath = "";
		public TextAsset m_movieData = null;

		public override string MoviePath {
			get {
				return m_moviePath;
			}
		}

		public override byte[] MovieData
		{
			get
			{
				return (m_movieData == null) ? null : m_movieData.bytes;
			}
		}

		public override string MovieName
		{
			get {
				return (m_movieData != null) ? m_movieData.name : System.IO.Path.GetFileNameWithoutExtension(m_moviePath);
			}
		}

		public override int DataId {
			get {
				if(m_movieData == null)
				{
					Debug.LogError("[CRIWARE] This property is undefined when the movie is referenced by its file path.");
					return 0;
				}
				return m_movieData.GetInstanceID();
			}
		}
	}

	public abstract class CriManaClipBase : PlayableAsset , ITimelineClipAsset
	{
		private struct MovieInfoStruct {
			public UInt32 width;                /**< ムービ最大幅（８の倍数） */
			public UInt32 height;               /**< ムービ最大高さ（８の倍数） */
			public UInt32 dispWidth;            /**< 表示したい映像の横ピクセル数（左端から） */
			public UInt32 dispHeight;           /**< 表示したい映像の縦ピクセル数（上端から） */
			public UInt32 framerateN;           /**< 有理数形式フレームレート(分子) framerate [x1000] = framerateN / framerateD */
			public UInt32 framerateD;           /**< 有理数形式フレームレート(分母) framerate [x1000] = framerateN / framerateD */
			public UInt32 totalFrames;      /**< 総フレーム数 */
			public CriMana.CodecType _codecType;
			public CriMana.CodecType _alphaCodecType;
		}

		public abstract string MoviePath { get; }
		public abstract byte[] MovieData { get; }
		public abstract string MovieName { get; }
		public abstract int DataId { get; }

		public readonly Guid guid = Guid.NewGuid();
		public bool m_loopWithinClip = false;
		public bool m_useOnMemoryPlayback = false;
		public GCHandle gcHandle = default(GCHandle);
		[SerializeField] private double m_movieFrameRate = 0.0;
		[SerializeField] private double m_clipDuration = 0.0;

		public float m_fadeinDuration = 0f;
		public AnimationCurve m_fadeinCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		public float m_fadeoutDuration = 0f;
		public AnimationCurve m_fadeoutCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
		public bool m_fadeAudio = true;
		[NonSerialized]
		public TimelineClip m_clip;

		public CriManaBehaviour m_manaBehaviour = new CriManaBehaviour();
		private MovieInfoStruct? m_movieInfoStruct = null;

		private MovieInfoStruct? StructToMovieInfo(CriMana.MovieInfo movieInfo){
			if (movieInfo == null) {
				return null;
			}

			MovieInfoStruct infoStruct = new MovieInfoStruct {
				width = movieInfo.width,
				height = movieInfo.height,
				dispWidth = movieInfo.dispWidth,
				dispHeight = movieInfo.dispHeight,
				framerateN = movieInfo.framerateN,
				framerateD = movieInfo.framerateD,
				totalFrames = movieInfo.totalFrames,
				_codecType = movieInfo.codecType,
				_alphaCodecType = movieInfo.alphaCodecType,
			};

			return infoStruct;
		}

		public ClipCaps clipCaps {
			get { return ClipCaps.Looping; }
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
			return ScriptPlayable<CriManaBehaviour>.Create(graph, m_manaBehaviour);
		}

		public void ReplaceMovieInfo(CriMana.MovieInfo movieInfo) {
			MovieInfoStruct? movieInfoStruct = StructToMovieInfo(movieInfo);
			if (movieInfo == null || movieInfoStruct.Equals(m_movieInfoStruct)) {
				return;
			}

			m_movieInfoStruct = movieInfoStruct;
			m_movieFrameRate = (m_movieInfoStruct.Value.framerateN / (double)m_movieInfoStruct.Value.framerateD);
			m_clipDuration = (m_movieInfoStruct.Value.totalFrames * 1000.0 / (double)m_movieInfoStruct.Value.framerateN);
		}

		public bool IsSameMovie(CriMana.MovieInfo movieInfo) {
			if (!IsMovieInfoReady || movieInfo == null) {
				return false;
			}

			MovieInfoStruct? movieInfoStruct = StructToMovieInfo(movieInfo);
			return movieInfoStruct.Equals(m_movieInfoStruct);
		}

		public bool IsMovieInfoReady {
			get { return (m_movieInfoStruct != null); }
		}

		public int GetSeekFrame(double seekTimeSec, bool loop) {
			if (m_movieInfoStruct != null) {
				double seekFrame = seekTimeSec * m_movieFrameRate;
				if (loop == false) {
					seekFrame = Math.Min(seekFrame, m_movieInfoStruct.Value.totalFrames - 1);
				} else {
					seekFrame %= m_movieInfoStruct.Value.totalFrames;
				}
				return (int)seekFrame;
			} else {
				return 0;
			}
		}

		public override double duration {
			get {
				return m_clipDuration > 0.0 ? m_clipDuration : 10.0;
			}
		}
	}

}

} //namespace CriWare

#endif