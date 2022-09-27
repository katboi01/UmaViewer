/****************************************************************************
 *
 * Copyright (c) 2015 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX)
using UnityEngine;
using System.Runtime.InteropServices;

namespace CriWare {

namespace CriMana.Detail
{
	public static partial class AutoResisterRendererResourceFactories
	{
		[RendererResourceFactoryPriority(7000)]
		public class RendererResourceFactoryOSXH264Yuv : RendererResourceFactory
		{
			public override RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
			{
				bool isCodecSuitable = movieInfo.codecType == CodecType.H264;
				bool isSuitable      = isCodecSuitable;
				return isSuitable
					? new RendererResourceOSXH264Yuv(playerId, movieInfo, additive, userShader)
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

	public class RendererResourceOSXH264Yuv : RendererResource
	{
		private int     width;
		private int     height;
		private int     playerId;
		private bool    useUserShader;
		private Vector4         movieTextureST = Vector4.zero;
		private Texture2D[]     textures;
		private RenderTexture[] renderTextures;
		System.IntPtr[]         nativePtrs;
		private bool isStoppingForSeek = false;
		private bool isStartTriggered = true;
		private bool hasRenderedNewFrame = false;

		public RendererResourceOSXH264Yuv(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			this.width      = (int)movieInfo.width;
			this.height     = (int)movieInfo.height;
			this.playerId   = playerId;
			this.hasAlpha   = movieInfo.hasAlpha;
			this.additive   = additive;
			this.useUserShader  = userShader != null;

			if (userShader != null) {
				shader = userShader;
			} else {
				string shaderName = "CriMana/OSXH264Yuv";
				shader = Shader.Find(shaderName);
			}

			if (hasAlpha) {
				nativePtrs = new System.IntPtr[2];
			} else {
				nativePtrs = new System.IntPtr[1];
			}
		}

		protected override void OnDisposeManaged()
		{
		}

		protected override void OnDisposeUnmanaged()
		{
			DisposeTextures(textures);
			DisposeTextures(renderTextures);
			textures = null;
			renderTextures = null;
			currentMaterial = null;
		}

		public override bool IsPrepared()
		{ return true; }

		public override bool ContinuePreparing()
		{ return true; }

		public override bool IsSuitable(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			bool isCodecSuitable    = movieInfo.codecType == CodecType.H264;
			bool isAlphaSuitable    = hasAlpha == movieInfo.hasAlpha;
			bool isAdditiveSuitable = this.additive == additive;
			bool isShaderSuitable   = this.useUserShader ? (userShader == shader) : true;
			return isCodecSuitable && isAlphaSuitable && isAdditiveSuitable && isShaderSuitable;
		}

		public override bool OnPlayerStopForSeek()
		{
			isStoppingForSeek = true;
			isStartTriggered = false;
			if (renderTextures != null || textures == null) {
				return true;
			}
			renderTextures = new RenderTexture[hasAlpha ? 2 : 1];
			for (int i = 0; i < renderTextures.Length; i++) {
				Texture2D baseTexture = textures[i];
				RenderTexture texture = new RenderTexture(baseTexture.width, baseTexture.height, 0,
															i == 0 ? RenderTextureFormat.ARGB32 : RenderTextureFormat.R8);
				texture.Create();
				Graphics.Blit(baseTexture, texture);
				renderTextures[i] = texture;
			}
			forceUpdateMaterialTextures(renderTextures);
			DisposeTextures(textures);
			textures = null;
			return true;
		}

		public override void OnPlayerStart()
		{
			isStartTriggered = true;
		}

		public override bool HasRenderedNewFrame()
		{
			return hasRenderedNewFrame;
		}

		private void forceUpdateMaterialTextures(Texture[] newTextures)
		{
			if (currentMaterial != null) {
				currentMaterial.mainTexture = newTextures[0];
				currentMaterial.SetTexture("_TextureRGB", newTextures[0]);
				if (hasAlpha) {
					currentMaterial.SetTexture("_TextureA", newTextures[1]);
				}
			}
		}

		public override void AttachToPlayer(int playerId)
		{
			// reset texture if exist
			DisposeTextures(textures);
			textures = null;
			hasRenderedNewFrame = false;
		}

		public override bool UpdateFrame(int playerId, FrameInfo frameInfo, ref bool frameDrop)
		{
			bool isFrameUpdated = CRIWARE9061FF01(playerId, 0, null, frameInfo, ref frameDrop);
			if (isFrameUpdated && !frameDrop) {
				UpdateMovieTextureST(frameInfo.dispWidth, frameInfo.dispHeight);
			}

			return isFrameUpdated;
		}

		public override bool UpdateMaterial(Material material)
		{
			if (currentMaterial != material) {
				if (textures != null || isStoppingForSeek) {
					currentMaterial = material;
					SetupStaticMaterialProperties();
				} else {
					return false;
				}
			}
			material.SetVector("_MovieTexture_ST", movieTextureST);
			if (textures != null && !isStoppingForSeek) {
				material.SetTexture("_TextureRGB", textures[0]);
				if (hasAlpha) {
					material.SetTexture("_TextureA", textures[1]);
				}
				DisposeTextures(renderTextures);
				renderTextures = null;
				hasRenderedNewFrame = true;
			}
			return true;
		}

		private void UpdateMovieTextureST(System.UInt32 dispWidth, System.UInt32 dispHeight)
		{
			float uScale = (dispWidth != width) ? (float)(dispWidth - 0.5f) / width : 1.0f;
			float vScale = (dispHeight != height) ? (float)(dispHeight - 0.5f) / height : 1.0f;
			movieTextureST.x = uScale;
			movieTextureST.y = -vScale;
			movieTextureST.z = 0.0f;
			movieTextureST.w = vScale;
		}

		public override void UpdateTextures()
		{
			for (int i = 0; i < nativePtrs.Length; i++) {
				nativePtrs[i] = System.IntPtr.Zero;
			}
			bool isTextureUpdated = CRIWAREDA63C3D7(playerId, nativePtrs.Length, nativePtrs);
			if (isTextureUpdated && nativePtrs[0] != System.IntPtr.Zero && isStartTriggered) {
				if (textures == null) {
					textures = new Texture2D[2];
					textures[0] = Texture2D.CreateExternalTexture(width, height, TextureFormat.BGRA32, false, false, nativePtrs[0]);
					textures[0].wrapMode = TextureWrapMode.Clamp;
					if (hasAlpha) {
						textures[1] = Texture2D.CreateExternalTexture(width, height, TextureFormat.R8, false, false, nativePtrs[1]);
						textures[1].wrapMode = TextureWrapMode.Clamp;
					}
				} else {
					textures[0].UpdateExternalTexture(nativePtrs[0]);
					if (hasAlpha) {
						textures[1].UpdateExternalTexture(nativePtrs[1]);
					}
				}
				isStoppingForSeek = false;
			}
		}
	}
}

} //namespace CriWare

#endif
