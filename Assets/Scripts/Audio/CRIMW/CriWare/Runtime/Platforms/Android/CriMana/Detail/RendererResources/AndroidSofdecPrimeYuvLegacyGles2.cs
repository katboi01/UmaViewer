/****************************************************************************
 *
 * Copyright (c) 2015 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !UNITY_EDITOR && UNITY_ANDROID

using UnityEngine;
using System;

namespace CriWare {

namespace CriMana.Detail
{
	public static partial class AutoResisterRendererResourceFactories
	{
		[RendererResourceFactoryPriority(5060)]
		public class RendererResourceFactoryAndroidSofdecPrimeYuvLegacyGles2 : RendererResourceFactory
		{
			public override RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
			{
				bool isCodecSuitable = movieInfo.codecType == CodecType.SofdecPrime || movieInfo.codecType == CodecType.VP9 ||
										(CriManaPlugin.criManaUnity_IsBufferOutputForH264Enabled_ANDROID() && movieInfo.codecType == CodecType.H264);
				bool isGraphicsApiSuitable = (UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2 ||
											  UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Vulkan);
				bool isSuitable = isCodecSuitable && isGraphicsApiSuitable;
				return isSuitable
					? new RendererResourceAndroidSofdecPrimeYuvLegacyGles2(playerId, movieInfo, additive, userShader)
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


	public class RendererResourceAndroidSofdecPrimeYuvLegacyGles2 : RendererResource
	{
		private int width, height;
		private int chromaWidth, chromaHeight;
		private int alphaWidth, alphaHeight;
		private bool    useUserShader;
		CodecType       codecType;

		private int movieWidth, movieHeight;

		private const TextureFormat   format = TextureFormat.Alpha8;
		private const TextureFormat formatUV = TextureFormat.RG16;

		private Vector4     movieTextureST = Vector4.zero;
		private Vector4     movieChromaTextureST = Vector4.zero;
		private Vector4     movieAlphaTextureST = Vector4.zero;

		private Texture2D[][] textures;
		private RenderTexture[] renderTextures;
		private IntPtr[][]  nativeTextures;
		private int numImages = -1;
		private int numImagesForYUV = -1;

		private Int32   numTextureSets = 2;
		private Int32   currentTextureSet = 0;
		private Int32   drawTextureSet = -1;
		private Int32   playerID;
		private bool    isStoppingForSeek = false;

		public RendererResourceAndroidSofdecPrimeYuvLegacyGles2(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			hasAlpha        = movieInfo.hasAlpha;
			this.additive   = additive;
			useUserShader   = userShader != null;

			movieWidth = (int)movieInfo.width;
			movieHeight = (int)movieInfo.height;

			if (userShader != null) {
				shader = userShader;
			} else {
				string shaderName = "CriMana/AndroidSofdecPrimeYuvLegacy";
				shader = Shader.Find(shaderName);
			}

			textures = new Texture2D[numTextureSets][];
			for (int i = 0; i < numTextureSets; i++) {
				textures[i] = new Texture2D[4];
				for (int j = 0; j < 4; j++) {
					textures[i][j] = null;
				}
			}
			nativeTextures = new IntPtr[numTextureSets][];

			for (int i = 0; i < numTextureSets; i++) {
				nativeTextures[i] = new IntPtr[4];
				for (int j = 0; j < 4; j++) {
					nativeTextures[i][j] = IntPtr.Zero;
				}
			}
			codecType = movieInfo.codecType;
			playerID = playerId;
		}


		protected override void OnDisposeManaged()
		{}


		protected override void OnDisposeUnmanaged()
		{
			CRIWARE2676F2A5(playerID, 0 ,null);

			for (int i = 0; i < numTextureSets; i++) {
				DisposeTextures(textures[i]);
			}
			textures = null;
			nativeTextures = null;
		}

		public override bool IsPrepared()
		{ return true; }


		public override bool ContinuePreparing()
		{ return true; }


		public override bool IsSuitable(int playerId, MovieInfo movieInfo, bool additive, Shader userShader) {
			bool isCodecSuitable = movieInfo.codecType == codecType;
			bool isSizeSuitable = (movieWidth == (int)movieInfo.width) && (movieHeight == (int)movieInfo.height);
			bool isAlphaSuitable = hasAlpha == movieInfo.hasAlpha;
			bool isAdditiveSuitable = this.additive == additive;
			bool isShaderSuitable = this.useUserShader ? (userShader == shader) : true;
			return isCodecSuitable && isSizeSuitable && isAlphaSuitable && isAdditiveSuitable && isShaderSuitable;
		}


		public override bool OnPlayerStopForSeek() {
			isStoppingForSeek = (drawTextureSet >= 0);
			return true;
		}

		public override void AttachToPlayer(int playerId) {
			CRIWARE71A23DE1(playerID, 0, new IntPtr[1]);
			drawTextureSet = -1;
		}

		public override bool UpdateFrame(int playerId, FrameInfo frameInfo, ref bool frameDrop) {
			bool isFrameUpdated = CRIWARE9061FF01(playerId, 0, null, frameInfo, ref frameDrop);
			if (isFrameUpdated && !frameDrop) {
				drawTextureSet = currentTextureSet;
				currentTextureSet = (currentTextureSet + 1) % numTextureSets;
				isStoppingForSeek = false;
			}

			if (isFrameUpdated && textures[0][0] == null) {
				numImages = (int)frameInfo.numImages;
				numImagesForYUV = hasAlpha ? numImages - 1 : numImages;

				CalculateTextureSize(ref width, ref height, (int)frameInfo.width, (int)frameInfo.height, codecType, false);
				CalculateTextureSize(ref chromaWidth, ref chromaHeight, (int)frameInfo.width, (int)frameInfo.height, codecType, true);
				if (hasAlpha) {
					CalculateTextureSize(ref alphaWidth, ref alphaHeight, (int)frameInfo.width, (int)frameInfo.height, CodecType.SofdecPrime, false);
				}
				UpdateMovieTextureST(frameInfo.dispWidth, frameInfo.dispHeight);

				for (int i = 0; i < numTextureSets; i++) {
					textures[i][0] = new Texture2D(width, height, format, false, true);
					textures[i][0].wrapMode = TextureWrapMode.Clamp;
					if (numImagesForYUV == 3) {
						textures[i][1] = new Texture2D(chromaWidth, chromaHeight, format, false, true);
						textures[i][1].wrapMode = TextureWrapMode.Clamp;
						textures[i][2] = new Texture2D(chromaWidth, chromaHeight, format, false, true);
						textures[i][2].wrapMode = TextureWrapMode.Clamp;
					} else {
						textures[i][1] = new Texture2D(chromaWidth, chromaHeight, formatUV, false, true);
						textures[i][1].wrapMode = TextureWrapMode.Clamp;
					}
					for (int j = 0; j < numImagesForYUV; j++) {
						nativeTextures[i][j] = textures[i][j].GetNativeTexturePtr();
					}
					if (hasAlpha) {
						textures[i][numImages - 1] = new Texture2D(alphaWidth, alphaHeight, format, false, true);
						textures[i][numImages - 1].wrapMode = TextureWrapMode.Clamp;
						nativeTextures[i][numImages - 1] = textures[i][numImages - 1].GetNativeTexturePtr();
					}
				}
			}

			return isFrameUpdated;
		}

		public override bool UpdateMaterial(Material material) {
			if (isStoppingForSeek)
				return true;

			if (drawTextureSet < 0)
				return false;

			if (currentMaterial != material) {
				currentMaterial = material;
				SetupStaticMaterialProperties();

				if (numImagesForYUV == 3) {
					material.DisableKeyword("CRI_UV_FORMAT");
				} else {
					material.EnableKeyword("CRI_UV_FORMAT");
				}
			}
			material.SetVector("_MovieTexture_ST", movieTextureST);
			material.SetVector("_MovieChromaTexture_ST", movieChromaTextureST);

			material.SetTexture("_TextureY", textures[drawTextureSet][0]);
			if (numImagesForYUV == 3) {
				material.SetTexture("_TextureU", textures[drawTextureSet][1]);
				material.SetTexture("_TextureV", textures[drawTextureSet][2]);
			} else {
				material.SetTexture("_TextureUV", textures[drawTextureSet][1]);
			}
			if (hasAlpha) {
				material.SetVector("_MovieAlphaTexture_ST", movieAlphaTextureST);
				material.SetTexture("_TextureA", textures[drawTextureSet][numImages - 1]);
			}

			return true;
		}


		private void UpdateMovieTextureST(System.UInt32 dispWidth, System.UInt32 dispHeight) {
			float uScale = (dispWidth != width) ? (float)(dispWidth - 1) / width : 1.0f;
			float vScale = (dispHeight != height) ? (float)(dispHeight - 1) / height : 1.0f;
			movieTextureST.x = uScale;
			movieTextureST.y = -vScale;
			movieTextureST.z = 0.0f;
			movieTextureST.w = vScale;

			uScale = (dispWidth != chromaWidth * 2) ?
					 (float)(dispWidth / 2 - 1) / (chromaWidth * 2) * 2 :
					 1.0f;
			vScale = (dispHeight != chromaHeight * 2) ?
					 (float)(dispHeight / 2 - 1) / (chromaHeight * 2) * 2 :
					 1.0f;
			movieChromaTextureST.x = uScale;
			movieChromaTextureST.y = -vScale;
			movieChromaTextureST.z = 0.0f;
			movieChromaTextureST.w = vScale;

			if (hasAlpha) {
				uScale = (dispWidth != alphaWidth) ? (float)(dispWidth - 1) / alphaWidth : 1.0f;
				vScale = (dispHeight != alphaHeight) ? (float)(dispHeight - 1) / alphaHeight : 1.0f;
				movieAlphaTextureST.x = uScale;
				movieAlphaTextureST.y = -vScale;
				movieAlphaTextureST.z = 0.0f;
				movieAlphaTextureST.w = vScale;
			}
		}

		public override void UpdateTextures()
		{
			if (drawTextureSet < 0)
				return;

			CRIWAREDA63C3D7(playerID, numImages, nativeTextures[drawTextureSet]);
		}

		private static void CalculateTextureSize(ref int w, ref int h, int videoWidth, int videoHeight, CodecType type, bool isChroma) {
			if (type == CodecType.SofdecPrime) {
				if (!isChroma) {
					w = Ceiling32(Ceiling16(CeilingWith(videoWidth, 8)));
					h = CeilingWith(videoHeight, 8);
				} else {
					w = Ceiling32(Ceiling16(CeilingWith(videoWidth, 8)) / 2);
					h = CeilingWith(videoHeight, 8) / 2;
				}

			} else if (type == CodecType.VP9) {
				if (!isChroma) {
					w = CeilingWith(CeilingWith(videoWidth, 2), 8);
					h = CeilingWith(CeilingWith(videoHeight, 2), 8);
				} else {
					w = CeilingWith(CeilingWith(videoWidth, 2) / 2, 8);
					h = CeilingWith(CeilingWith(videoHeight, 2) / 2, 8);
				}
			} else if (type == CodecType.H264) {
				if (!isChroma) {
					w = videoWidth;
					h = videoHeight;
				} else {
					w = videoWidth / 2;
					h = videoHeight / 2;
				}
			}
		}
	}
}

} //namespace CriWare

#endif
