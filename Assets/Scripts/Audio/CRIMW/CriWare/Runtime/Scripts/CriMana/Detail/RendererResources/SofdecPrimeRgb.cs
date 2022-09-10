/****************************************************************************
 *
 * Copyright (c) 2015 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !UNITY_EDITOR && (UNITY_PS3 || UNITY_WINRT)

using UnityEngine;
using System.Runtime.InteropServices;

namespace CriWare {

namespace CriMana.Detail
{
	public static partial class AutoResisterRendererResourceFactories
	{
		[RendererResourceFactoryPriority(5050)]
		public class RendererResourceFactorySofdecPrimeRgb : RendererResourceFactory
		{
			public override RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
			{
				bool isCodecSuitable = movieInfo.codecType == CodecType.SofdecPrime;
				bool isSuitable      = isCodecSuitable;
				return isSuitable
					? new RendererResourceSofdecPrimeRgb(playerId, movieInfo, additive, userShader)
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




	public class RendererResourceSofdecPrimeRgb : RendererResource
	{
		private int     width;
		private int     height;
		private bool    useUserShader;

		private Vector4     movieTextureST = Vector4.zero;

		private Texture2D   texture;
		private Color32[]   pixels;
		private GCHandle    pixelsHandle;
		private bool        isTextureReady;


		public RendererResourceSofdecPrimeRgb(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_PSP2 || UNITY_PS5 || UNITY_PS4 || UNITY_PS3 || UNITY_WINRT || UNITY_SWITCH
			width  = Ceiling64((int)movieInfo.width);
			height = Ceiling64((int)movieInfo.height);
#elif UNITY_ANDROID || UNITY_IOS || UNITY_TVOS
			width  = NextPowerOfTwo((int)movieInfo.width);
			height = NextPowerOfTwo((int)movieInfo.height);
#else
	#error unsupported platform
#endif
			hasAlpha        = movieInfo.hasAlpha;
			this.additive   = additive;
			useUserShader   = userShader != null;

			if (userShader != null) {
				shader = userShader;
			} else {
				string shaderName = "CriMana/SofdecPrimeRgb";
				shader = Shader.Find(shaderName);
			}

			UpdateMovieTextureST(movieInfo.dispWidth, movieInfo.dispHeight);

			texture             = new Texture2D(width, height, TextureFormat.RGBA32, false);
			texture.wrapMode    = TextureWrapMode.Clamp;
			pixels              = texture.GetPixels32(0);
			pixelsHandle        = GCHandle.Alloc(pixels, GCHandleType.Pinned);
		}


		protected override void OnDisposeManaged()
		{
		}


		protected override void OnDisposeUnmanaged()
		{
			if (texture != null) {
				if (pixelsHandle.IsAllocated == true) {
					pixelsHandle.Free();
				}
			}
			if (texture != null) {
				Texture2D.Destroy(texture);
			}
			texture = null;
			pixels  = null;
			currentMaterial = null;
		}


		public override bool IsPrepared()
		{ return isTextureReady; }


		public override bool ContinuePreparing()
		{ return true; }


		public override bool IsSuitable(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			bool isCodecSuitable    = movieInfo.codecType == CodecType.SofdecPrime;
			bool isSizeSuitable     = (width >= (int)movieInfo.width) && (height >= (int)movieInfo.height);
			bool isAlphaSuitable    = hasAlpha == movieInfo.hasAlpha;
			bool isAdditiveSuitable = this.additive == additive;
			bool isShaderSuitable   = this.useUserShader ? (userShader == shader) : true;
			return isCodecSuitable && isSizeSuitable && isAlphaSuitable && isAdditiveSuitable && isShaderSuitable;
		}


		public override void AttachToPlayer(int playerId)
		{
			isTextureReady = false;
		}


		public override bool UpdateFrame(int playerId, FrameInfo frameInfo, ref bool frameDrop)
		{
			// Note: This renderer use a deprecated API: CRIWARE1DE4FE72.
			// Todo: use CRIWAREDA63C3D7 with a c++ rendereDecoder class if needed.
			bool isFrameUpdated = CRIWARE1DE4FE72(
				playerId,
				pixelsHandle.AddrOfPinnedObject(),
				frameInfo,
				width
				);
			if (isFrameUpdated) {
				texture.SetPixels32(pixels, 0);
				texture.Apply();
				UpdateMovieTextureST(frameInfo.dispWidth, frameInfo.dispHeight);
				isTextureReady = true;
			}
			// Frame dropping cannot be supported with deprecated API.
			frameDrop = false;
			return isFrameUpdated;
		}


		public override bool UpdateMaterial(Material material)
		{
			if (currentMaterial != material) {
				currentMaterial = material;
				SetupStaticMaterialProperties();
				//Temporary fix for Switch
#if !UNITY_EDITOR && UNITY_SWITCH
				material.EnableKeyword("CRI_SWITCH");
#endif
				material.mainTexture = texture;
				material.SetTexture("_TextureRGBA", texture);
			}
			material.SetVector("_MovieTexture_ST", movieTextureST);
			return true;
		}


		private void UpdateMovieTextureST(System.UInt32 dispWidth, System.UInt32 dispHeight)
		{
			float uScale = (float)(dispWidth) / width;
			float vScale = (float)(dispHeight) / height;
			movieTextureST.x = uScale;
			movieTextureST.y = -vScale;
			movieTextureST.z = 0.0f;
			movieTextureST.w = vScale;
		}


		public override void UpdateTextures()
		{
		}


		#region Native API Definitions
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		private static extern bool CRIWARE1DE4FE72(
			int player_id,
			System.IntPtr texbuf,
			[In, Out] FrameInfo frame_info,
			int texbuf_width
			);
		#endregion
	}
}

} //namespace CriWare

#endif
