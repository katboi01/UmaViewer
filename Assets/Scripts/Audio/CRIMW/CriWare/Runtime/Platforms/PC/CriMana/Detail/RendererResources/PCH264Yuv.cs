/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)

using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace CriWare {

namespace CriMana.Detail
{
	public static partial class AutoResisterRendererResourceFactories
	{
		[RendererResourceFactoryPriority(7050)]
		public class RendererResourceFactoryH264Yuv : RendererResourceFactory
		{
			public override RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
			{
				bool isCodecSuitable = movieInfo.codecType == CodecType.H264;
				bool isSuitable      = isCodecSuitable;
				return isSuitable
					? new RendererResourceH264Yuv(playerId, movieInfo, additive, userShader)
					: null;
			}

			protected override void OnDisposeManaged()
			{
			}

			protected override void OnDisposeUnmanaged()
			{
			}
		}
	}




	public class RendererResourceH264Yuv : RendererResourceSofdecPrimeYuv
	{
		public RendererResourceH264Yuv(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
			: base(playerId, movieInfo, additive, userShader)
		{
		}
	}
}

} //namespace CriWare

#endif
