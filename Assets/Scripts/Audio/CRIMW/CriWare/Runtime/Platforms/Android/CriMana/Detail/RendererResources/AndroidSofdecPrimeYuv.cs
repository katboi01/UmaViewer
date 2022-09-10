/****************************************************************************
 *
 * Copyright (c) 2015-2018 CRI Middleware Co., Ltd.
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
		[RendererResourceFactoryPriority(5050)]
		public class RendererResourceFactoryAndroidSofdecPrimeYuv : RendererResourceFactory
		{
			public override RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
			{
				bool isCodecSuitable = movieInfo.codecType == CodecType.SofdecPrime ||
										movieInfo.codecType == CodecType.VP9 ||
										(CriManaPlugin.criManaUnity_IsBufferOutputForH264Enabled_ANDROID() && movieInfo.codecType == CodecType.H264);
				bool isGraphicsApiSuitable = (UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3);
				bool isSuitable = isCodecSuitable && isGraphicsApiSuitable;
				return isSuitable
					? new RendererResourceAndroidSofdecPrimeYuv(playerId, movieInfo, additive, userShader)
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




	public class RendererResourceAndroidSofdecPrimeYuv : RendererResource
	{
		private int     width, height;
		private int     chromaWidth, chromaHeight;
		private int     alphaWidth, alphaHeight;
		private bool    useUserShader;
		private CodecType   codecType;

		private const TextureFormat   format = TextureFormat.R8;
		private const TextureFormat formatUV = TextureFormat.RG16;

		private Vector4     movieTextureST = Vector4.zero;
		private Vector4     movieChromaTextureST = Vector4.zero;
		private Vector4     movieAlphaTextureST = Vector4.zero;

		private Texture2D[] textures;
		private int numImages = -1;
		private int numImagesForYUV = -1;
		System.IntPtr[] nativePtrs = new System.IntPtr[4];
		private RenderTexture[] renderTextures;

		private Int32       playerID;
		private bool        areTexturesUpdated = false;
		private bool        isFrameUpdated = false;
		private bool        isStoppingForSeek = false;
		private bool        isStartTriggered = true;


		public RendererResourceAndroidSofdecPrimeYuv(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			CalculateTextureSize(ref width, ref height, (int)movieInfo.width, (int)movieInfo.height, movieInfo.codecType, false);
			CalculateTextureSize(ref chromaWidth, ref chromaHeight, (int)movieInfo.width, (int)movieInfo.height, movieInfo.codecType, true);

			this.additive   = additive;
			hasAlpha    = movieInfo.hasAlpha;
			useUserShader   = userShader != null;
			codecType       = movieInfo.codecType;

			if (userShader != null) {
				shader = userShader;
			} else {
				string shaderName = "CriMana/AndroidSofdecPrimeYuv";
				shader = Shader.Find(shaderName);
			}

			if (hasAlpha) {
				CalculateTextureSize(ref alphaWidth, ref alphaHeight, (int)movieInfo.width, (int)movieInfo.height, movieInfo.alphaCodecType, false);
			}

			UpdateMovieTextureST(movieInfo.dispWidth, movieInfo.dispHeight);

			nativePtrs = new IntPtr[4];
			playerID = playerId;
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
		}


		public override bool IsPrepared()
		{
			return areTexturesUpdated && isFrameUpdated;
		}


		public override bool ContinuePreparing()
		{ return true; }


		public override bool IsSuitable(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			int w = 0, h = 0;
			CalculateTextureSize(ref w, ref h, (int)movieInfo.width, (int)movieInfo.height, movieInfo.codecType, false);
			bool isCodecSuitable    = movieInfo.codecType == codecType;
			bool isSizeSuitable     = (width == w) && (height == h);
			bool isAlphaSuitable    = hasAlpha == movieInfo.hasAlpha;
			bool isAdditiveSuitable = this.additive == additive;
			bool isShaderSuitable   = this.useUserShader ? (userShader == shader) : true;
			return isCodecSuitable && isSizeSuitable && isAlphaSuitable && isAdditiveSuitable && isShaderSuitable;
		}


		public override bool OnPlayerStopForSeek()
		{
			isStoppingForSeek = true;
			isStartTriggered = false;

			if (renderTextures != null || textures == null || textures[0] == null) {
				return true;
			}

			renderTextures = new RenderTexture[hasAlpha ? 4 : 3];

			for (int i = 0; i < renderTextures.Length; i++) {
				Texture2D baseTexture = textures[i];
				RenderTexture texture = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.R8);
				texture.Create();

				Graphics.Blit(baseTexture, texture);
				renderTextures[i] = texture;
			}

			forceUpdateMaterialTextures(renderTextures);

			DisposeTextures(textures);

			return true;
		}


		public override void OnPlayerStart() {
			isStartTriggered = true;
		}


		private void forceUpdateMaterialTextures(Texture[] newTextures) {
			if (currentMaterial != null) {
				currentMaterial.SetTexture("_TextureY", newTextures[0]);
				currentMaterial.SetTexture("_TextureU", newTextures[1]);
				currentMaterial.SetTexture("_TextureV", newTextures[2]);
				if (hasAlpha) {
					currentMaterial.SetTexture("_TextureA", newTextures[3]);
				}
			}
		}


		public override void AttachToPlayer(int playerId)
		{
			DisposeTextures(textures);
			areTexturesUpdated = false;
			isFrameUpdated = false;
		}


		public override bool UpdateFrame(int playerId, FrameInfo frameInfo, ref bool frameDrop)
		{
			bool updated = CRIWARE9061FF01(playerId, 4, null, frameInfo, ref frameDrop);
			if (updated && !frameDrop) {
				UpdateMovieTextureST(frameInfo.dispWidth, frameInfo.dispHeight);
			}
			isFrameUpdated |= updated;
			if (isFrameUpdated) {
				numImages = (int)frameInfo.numImages;
				numImagesForYUV = hasAlpha ? numImages - 1 : numImages;
			}
			return updated;
		}


		public override bool UpdateMaterial(Material material)
		{
			if (areTexturesUpdated) {
				if (textures != null && textures[0] != null) {
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

					if (!isStoppingForSeek) {
						material.SetTexture("_TextureY", textures[0]);
						if (numImagesForYUV == 3) {
							material.SetTexture("_TextureU", textures[1]);
							material.SetTexture("_TextureV", textures[2]);
						} else {
							material.SetTexture("_TextureUV", textures[1]);
						}
						if (hasAlpha) {
							material.SetVector("_MovieAlphaTexture_ST", movieAlphaTextureST);
							material.SetTexture("_TextureA", textures[numImages - 1]);
						}
						DisposeTextures(renderTextures);
						renderTextures = null;
					}
				}
				return true;
			} else {
				return renderTextures != null;
			}
		}

		private void UpdateMovieTextureST(System.UInt32 dispWidth, System.UInt32 dispHeight)
		{
			float uScale = (dispWidth != width) ? (float)(dispWidth - 1) / width : 1.0f;
			float vScale = (dispHeight != height) ? (float)(dispHeight - 1) / height : 1.0f;
			movieTextureST.x = uScale;
			movieTextureST.y = -vScale;
			movieTextureST.z = 0.0f;
			movieTextureST.w = vScale;

			uScale = (dispWidth != chromaWidth * 2) ? (float)(dispWidth / 2 - 1) / (chromaWidth * 2) * 2 : 1.0f;
			vScale = (dispHeight != chromaHeight * 2) ? (float)(dispHeight / 2 - 1) / (chromaHeight * 2) * 2 : 1.0f;
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
			nativePtrs[0] = System.IntPtr.Zero;
			bool updated = CRIWAREDA63C3D7(playerID, numImages, nativePtrs);
			areTexturesUpdated |= updated;

			if (updated && nativePtrs[0] != System.IntPtr.Zero && isStartTriggered) {
				if (textures == null || textures[0] == null) {
					textures = new Texture2D[numImages];
					textures[0] = Texture2D.CreateExternalTexture(width, height, format, false, false, nativePtrs[0]);
					textures[0].wrapMode = TextureWrapMode.Clamp;
					if (numImagesForYUV == 3) { /* Y/U/V */
						textures[1] = Texture2D.CreateExternalTexture(chromaWidth, chromaHeight, format, false, false, nativePtrs[1]);
						textures[1].wrapMode = TextureWrapMode.Clamp;
						textures[2] = Texture2D.CreateExternalTexture(chromaWidth, chromaHeight, format, false, false, nativePtrs[2]);
						textures[2].wrapMode = TextureWrapMode.Clamp;
					} else { /* Y/UV */
						textures[1] = Texture2D.CreateExternalTexture(chromaWidth, chromaHeight, formatUV, false, false, nativePtrs[1]);
						textures[1].wrapMode = TextureWrapMode.Clamp;
					}
					if (hasAlpha) {
						textures[numImages - 1] = Texture2D.CreateExternalTexture(alphaWidth, alphaHeight, format, false, false, nativePtrs[numImages - 1]);
						textures[numImages - 1].wrapMode = TextureWrapMode.Clamp;
					}
				} else {
					for (int i = 0; i < numImages; i++) {
						textures[i].UpdateExternalTexture(nativePtrs[i]);
					}
				}
				isStoppingForSeek = false;
			}
		}

		private static void CalculateTextureSize(ref int w, ref int h, int videoWidth, int videoHeight, CodecType type, bool isChroma)
		{
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
					h = CeilingWith(videoHeight, 2);
				} else {
					w = CeilingWith(CeilingWith(videoWidth, 2) / 2, 8);
					h = CeilingWith(videoHeight, 2) / 2;
				}
			} else if (type == CodecType.H264) {
				if (!isChroma) {
					w = Ceiling16(Ceiling16(CeilingWith(videoWidth, 8)));
					h = CeilingWith(videoHeight, 8);
				} else {
					w = Ceiling16(Ceiling16(CeilingWith(videoWidth, 8)) / 2);
					h = CeilingWith(videoHeight, 8) / 2;
				}
			}
		}

	} // class
} // namespace

} //namespace CriWare

#endif
