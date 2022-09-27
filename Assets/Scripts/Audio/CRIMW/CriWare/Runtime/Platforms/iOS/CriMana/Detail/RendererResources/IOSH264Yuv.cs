/****************************************************************************
 *
 * Copyright (c) 2015 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !UNITY_TVOS
#define CRIMANAUNITY_ENABLE_PAUSE_TEXTURE
#endif

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)

using UnityEngine;
using System.Runtime.InteropServices;

namespace CriWare {

namespace CriMana.Detail
{
	public static partial class AutoResisterRendererResourceFactories
	{
		[RendererResourceFactoryPriority(7000)]
		public class RendererResourceFactoryIOSH264Yuv : RendererResourceFactory
		{
			public override RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
			{
				bool isCodecSuitable = movieInfo.codecType == CodecType.H264;
				bool isSuitable      = isCodecSuitable;
				return isSuitable
					? new RendererResourceIOSH264Yuv(playerId, movieInfo, additive, userShader)
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


	public class RendererResourceIOSH264Yuv : RendererResource
	{
		private int     width;
		private int     height;
		private int     playerId;
		private bool    useUserShader;
		private bool    useOGLTempTextures;
		private bool    isPaused;

		private Vector4         movieTextureST = Vector4.zero;

		private Texture2D[]     textures;
		private RenderTexture[] pauseTextures;
		private Texture[]       currentTextures;
		private System.IntPtr[] nativePtrs;
		private bool            isStoppingForSeek = false;
		private bool            isStartTriggered = true;

		public RendererResourceIOSH264Yuv(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			this.width      = (int)movieInfo.width;
			this.height     = (int)movieInfo.height;
			this.playerId   = playerId;
			hasAlpha        = movieInfo.hasAlpha;
			this.additive   = additive;
			useUserShader   = userShader != null;

			if (userShader != null) {
				shader = userShader;
			} else {
				string shaderName = "CriMana/IOSH264Yuv";
				shader = Shader.Find(shaderName);
			}

			int numTextures = hasAlpha ? 3 : 2;
			textures = new Texture2D[numTextures];
			nativePtrs = new System.IntPtr[numTextures];

			UpdateMovieTextureST(movieInfo.dispWidth, movieInfo.dispHeight);

#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
			useOGLTempTextures = SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL");
#else
			useOGLTempTextures = (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2);
#endif
			currentTextures = textures;
		}


		protected override void OnDisposeManaged()
		{
		}


		protected override void OnDisposeUnmanaged()
		{
			DisposeTextures(textures);
			DisposeTextures(pauseTextures);
			textures = null;
			pauseTextures = null;
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

		public override void AttachToPlayer(int playerId)
		{
			DisposeTextures(textures);
			if (!isStoppingForSeek) {
				DisposeTextures(pauseTextures);
				pauseTextures = null;
				currentTextures = textures;
			}
			isPaused = false;
		}

		// app did goes background / will goes foreground or player is paused
		public override void OnPlayerPause(bool pauseStatus, bool triggredFromApplciationPause)
		{
#if CRIMANAUNITY_ENABLE_PAUSE_TEXTURE
			if (pauseStatus && !triggredFromApplciationPause) {
				// Do nothing if the user code triggered Pause(true).
				return;
			}

			if (pauseStatus == true) {
				if (isPaused == false && !isStoppingForSeek) {
					isPaused = true;
					applyCurrentTexturesToPausedTextures();
				}
			} else {
				isPaused = false;
			}
#endif
		}

		public override bool OnPlayerStopForSeek() {
			isStoppingForSeek = true;
			isStartTriggered = false;
			if (pauseTextures != null || textures[0] == null) {
				return true;
			}
			applyCurrentTexturesToPausedTextures();
			DisposeTextures(textures);
			return true;
		}

		public override void OnPlayerStart() {
			isStartTriggered = true;
		}

		private void restorePausedTextures()
		{
			forceUpdateMaterialTextures(textures);
			DisposeTextures(pauseTextures);
			pauseTextures = null;
		}

		private void applyCurrentTexturesToPausedTextures() {
			if (textures[0] == null) {
				return;
			}
			pauseTextures = new RenderTexture[hasAlpha ? 3 : 2];
			for (int i = 0; i < pauseTextures.Length; i++) {
				Texture2D baseTexture = textures[i];
				RenderTexture texture = new RenderTexture(baseTexture.width, baseTexture.height, 0,
															i == 1 ? RenderTextureFormat.RG16 : RenderTextureFormat.R8);
				texture.Create();
				Graphics.Blit(baseTexture, texture);
				pauseTextures[i] = texture;
			}
			forceUpdateMaterialTextures(pauseTextures);
		}

		private void forceUpdateMaterialTextures(Texture[] newTextures)
		{
			currentTextures = newTextures;
			if (currentMaterial != null) {
				currentMaterial.SetTexture("_TextureY", currentTextures[0]);
				currentMaterial.SetTexture("_TextureUV", currentTextures[1]);
				if (hasAlpha) {
					currentMaterial.SetTexture("_TextureA", currentTextures[2]);
				}
			}
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
			if (currentTextures[0] != null) {
				if (currentMaterial != material) {
					currentMaterial = material;
					SetupStaticMaterialProperties();
				}

				if (!(isPaused || isStoppingForSeek) && pauseTextures != null &&
					!CRIWARE98E7CD5C(playerId)) {
					restorePausedTextures();
					isStoppingForSeek = false;
				} else {
					material.SetTexture("_TextureY", currentTextures[0]);
					material.SetTexture("_TextureUV", currentTextures[1]);
					if (hasAlpha) {
						material.SetTexture("_TextureA", currentTextures[2]);
					}
                }
				material.SetVector("_MovieTexture_ST", movieTextureST);

				return true;
			}
			return false;
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
			int numTextures = hasAlpha ? 3 : 2;
			for (int i = 0; i < numTextures; i++) {
				nativePtrs[i] = System.IntPtr.Zero;
			}
			bool isTextureUpdated = CRIWAREDA63C3D7(playerId, numTextures, nativePtrs); // out textures
			if (isTextureUpdated && nativePtrs[0] != System.IntPtr.Zero && isStartTriggered) {
				if (useOGLTempTextures) {
					for (int i = 0; i < numTextures; i++) {
						if (textures[i] == null) {
							textures[i] = Texture2D.CreateExternalTexture((i == 1) ? width / 2 : width,
																			(i == 1) ? height / 2 : height,
																			TextureFormat.Alpha8, false, false, nativePtrs[i]);
						}
						Texture2D tmptexture = Texture2D.CreateExternalTexture(textures[i].width, textures[i].height,
																				 textures[i].format, false, false, nativePtrs[i]);
						tmptexture.wrapMode = TextureWrapMode.Clamp;
						textures[i].UpdateExternalTexture(tmptexture.GetNativeTexturePtr());
						Texture2D.Destroy(tmptexture);
					}
				} else {
					if (textures[0] == null) {
						for (int i = 0; i < numTextures; i++) {
							textures[i] = Texture2D.CreateExternalTexture((i == 1) ? width / 2 : width,
																		  (i == 1) ? height / 2 : height,
																		  TextureFormat.Alpha8, false, false, nativePtrs[i]);
							textures[i].wrapMode = TextureWrapMode.Clamp;
						}
					} else {
						for (int i = 0; i < numTextures; i++) {
							textures[i].UpdateExternalTexture(nativePtrs[i]);
						}
					}
				}
				isStoppingForSeek = false;
			}
		}
#region DLL Import
#if !CRIWARE_ENABLE_HEADLESS_MODE
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		private static extern bool CRIWARE98E7CD5C(int player_id);
#else
		private static bool CRIWARE98E7CD5C(int player_id) { return false; }
#endif
#endregion
	}
}

} //namespace CriWare

#endif
