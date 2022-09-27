/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !UNITY_EDITOR && UNITY_ANDROID

using UnityEngine;
using System.Runtime.InteropServices;

namespace CriWare {

namespace CriMana.Detail
{
	public static partial class AutoResisterRendererResourceFactories
	{
		[RendererResourceFactoryPriority(7000)]
		public class RendererResourceFactoryAndroidH264Rgb : RendererResourceFactory
		{
			public override RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
			{
				bool isCodecSuitable = movieInfo.codecType == CodecType.H264 && !CriManaPlugin.criManaUnity_IsBufferOutputForH264Enabled_ANDROID();
				bool isGraphicsApiSuitable = (UnityEngine.SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Vulkan);
				bool isSuitable      = isCodecSuitable && isGraphicsApiSuitable;
				return isSuitable
					? new RendererResourceAndroidH264Rgb(playerId, movieInfo, additive, userShader)
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




	public class RendererResourceAndroidH264Rgb : RendererResource
	{
		private const int RenderEventAction_ATTACH = (int)Player.CriManaUnityPlayer_RenderEventAction.DESTROY + 1;

		private int     playerId;

		private int     width;
		private int     height;
		private int     dispWidth;
		private int     dispHeight;
		private int     alphaWidth;
		private int     alphaHeight;
		private bool    useUserShader;

		private Vector4         movieTextureST = new Vector4(1, -1, 0, 1);
		private Vector4         alphaTextureST = Vector4.zero;
		private Texture2D[]     textures;
		private System.IntPtr[] nativePtrs = new System.IntPtr[1];

		private bool            needsUpdateTexture = false;
		private bool            needsToDetachInitTexture = false;
		private bool            areTexturesUpdated = false;

		private bool            isStoppingForSeek = false;
		private bool            isStartTriggered = true;
		private System.UInt32   nativeTextureId;

		public RendererResourceAndroidH264Rgb(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			this.playerId   = playerId;
			width       = (int)movieInfo.width;
			height      = (int)movieInfo.height;
			dispWidth   = (int)movieInfo.dispWidth;
			dispHeight  = (int)movieInfo.dispHeight;

			hasAlpha = movieInfo.hasAlpha;
			if (hasAlpha) {
				alphaWidth = Ceiling32(Ceiling16(CeilingWith(width, 8)));
				alphaHeight = CeilingWith(height, 16);
			}
			this.additive   = additive;
			useUserShader   = userShader != null;

			if (userShader != null) {
				shader = userShader;
			} else {
				string shaderName = "CriMana/AndroidH264Rgb";
				shader = Shader.Find(shaderName);
			}
		}

		protected override void OnDisposeManaged()
		{
		}

		protected override void OnDisposeUnmanaged()
		{
			DisposeTextures(textures);
			textures = null;
		}

		public override bool IsPrepared()
		{ return true; }

		public override bool ContinuePreparing()
		{ return true; }

		public override bool IsSuitable(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			bool isPlayerSuitable   = playerId == this.playerId;
			bool isCodecSuitable    = movieInfo.codecType == CodecType.H264;
			bool isSizeSuitable     = (width == (int)movieInfo.width) && (height == (int)movieInfo.height);
			bool isAlphaSuitable    = hasAlpha == movieInfo.hasAlpha;
			bool isAdditiveSuitable = this.additive == additive;
			bool isShaderSuitable   = this.useUserShader ? (userShader == shader) : true;
			return isPlayerSuitable && isCodecSuitable && isSizeSuitable && isAlphaSuitable && isAdditiveSuitable && isShaderSuitable;
		}

		public override bool OnPlayerStopForSeek()
		{
			if (textures == null) {
				return true;
			}
			isStoppingForSeek = true;
			isStartTriggered = false;
			return true;
		}

		public override void OnPlayerStart()
		{
			isStartTriggered = true;
		}

		public override bool ShouldSkipDestroyOnStopForSeek()
		{ return true; }

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
			if (!isStoppingForSeek) {
				nativeTextureId = 0xffffffff;
			}
			criManaUnityPlayer_MediaCodecAttachTexture_ANDROID(playerId, nativeTextureId);
			needsToDetachInitTexture = true;
		}


		public override bool UpdateFrame(int playerId, FrameInfo frameInfo, ref bool frameDrop)
		{
			if (needsToDetachInitTexture) { // only once
				needsToDetachInitTexture = false;
				criManaUnityPlayer_MediaCodecDetachTexture_ANDROID(playerId, 0xffffffff);
			}

			bool isFrameUpdated = CRIWARE9061FF01(playerId, 0, null, frameInfo, ref frameDrop);
			if (isFrameUpdated && !frameDrop) {
				needsUpdateTexture = true;
			}

			return isFrameUpdated;
		}


		public override bool UpdateMaterial(Material material)
		{
			if (!areTexturesUpdated) {
				return false;
			}

			if (currentMaterial != material) {
				currentMaterial = material;
				SetupStaticMaterialProperties();
			}
			if (textures != null) {
				material.mainTexture = textures[0];
				material.SetTexture("_TextureRGB", textures[0]);
				if (hasAlpha && textures[1] != null) {
					material.SetTexture("_TextureA", textures[1]);
				}
			}
			material.SetVector("_MovieTexture_ST", movieTextureST);
			if (hasAlpha) {
				material.SetVector("_AlphaTexture_ST", alphaTextureST);
			}

			return true;
		}

		private void UpdateMovieTextureST(float[] texCoords, float dispWidth, float dispHeight)
		{
			movieTextureST.x = texCoords[0];
			movieTextureST.y = texCoords[1];
			movieTextureST.z = texCoords[2];
			movieTextureST.w = texCoords[3];

			float uScale = (dispWidth != alphaWidth) ? ((float)dispWidth - 0.5f) / (float)alphaWidth : 1.0f;
			float vScale = (dispHeight != alphaHeight) ? ((float)dispHeight - 0.5f) / (float)alphaHeight : 1.0f;
			alphaTextureST.x = uScale;
			alphaTextureST.y = -vScale;
			alphaTextureST.z = 0.0f;
			alphaTextureST.w = vScale;
		}

		public override void UpdateTextures()
		{
			if (needsUpdateTexture) { // only once by frame (updates texture may be called multiple time because it is by object an camera
				int numTextures = hasAlpha ? 2 : 1;
				nativePtrs[0] = System.IntPtr.Zero;
				bool isTextureUpdated = CRIWAREDA63C3D7(playerId, numTextures, nativePtrs);
				areTexturesUpdated |= isTextureUpdated;

				if (isTextureUpdated && nativePtrs[0] != System.IntPtr.Zero && isStartTriggered) {
					needsUpdateTexture = false;
					var nativeTexture = (NativeTexture)Marshal.PtrToStructure(nativePtrs[0], typeof(NativeTexture));

					if (textures == null) {
						textures = new Texture2D[2];
						textures[0] = Texture2D.CreateExternalTexture(width, height, TextureFormat.ARGB32, false, false, new System.IntPtr(nativeTexture.nativePtrRGB));
						textures[0].wrapMode = TextureWrapMode.Clamp;
						if (hasAlpha) {
#if !UNITY_5_6_OR_NEWER || UNITY_5_6_0 || UNITY_5_6_1
							TextureFormat format = TextureFormat.Alpha8;
#else
							TextureFormat format = TextureFormat.R8;
#endif
							textures[1] = Texture2D.CreateExternalTexture(alphaWidth, alphaHeight, format, false, false, new System.IntPtr(nativeTexture.nativePtrA));
							textures[1].wrapMode = TextureWrapMode.Clamp;
						}
						if (currentMaterial != null) {
							forceUpdateMaterialTextures(textures);
						}
						nativeTextureId = nativeTexture.nativePtrRGB;
					} else {
						textures[0].UpdateExternalTexture(new System.IntPtr(nativeTexture.nativePtrRGB));
						if (hasAlpha) {
							textures[1].UpdateExternalTexture(new System.IntPtr(nativeTexture.nativePtrA));
						}
					}

					UpdateMovieTextureST(nativeTexture.texCoords, dispWidth, dispHeight);
					isStoppingForSeek = false;
				}
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		struct NativeTexture
		{
			public System.UInt32 nativePtrRGB; // opengl GLuint texture name
			public System.UInt32 nativePtrA; // opengl GLuint texture name

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public float[] texCoords; // texture coordinates
		}

		public static bool IsSupported()
		{
			if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Vulkan) {
				Debug.Log("[CRIWARE] Another RendererResource will be used for Vulkan. Please use CriManaPlugin.IsCodecSupported instead.");
				return true;
			}

			/* check if native code for Android MediaCodec is supported by device */
			if (!criManaUnity_IsMediaCodecSupported_ANDROID((int)SystemInfo.graphicsDeviceType)) {
				Debug.Log("[CRIWARE] criManaUnity_IsMediaCodecSupported_ANDROID returned false.");
				return false;
			}

			/* If OpenGLES2, all devices can render h.264 videos */
			if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2) {
				return true;
			}

			/* Load a shader for rendering test */
			Shader shader = Shader.Find("CriMana/AndroidH264DummySupportCheck");

			/* Check the compiled shader is supported(only works on old Unity) */
			if (!shader.isSupported) {
				return false;
			}

			/* Save current active RenderTexture */
			var current = RenderTexture.active;

			/* Create textures for shader rendering test */
			Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			RenderTexture renderTexture = new RenderTexture(1, 1, 0);
			renderTexture.Create();
			/* Create material with the shader */
			Material material = new Material(shader);
			/* Render on RenderTexture */
			Graphics.Blit(texture, renderTexture, material, 0);

			/* Copy pixels of RenderTexture to Texture */
			RenderTexture.active = renderTexture;
			texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			texture.Apply();
			/* Check pixel color */
			var color = texture.GetPixel(0, 0);
			bool ret = (color.r == 1.0f) && (color.g == 1.0f) && (color.b == 1.0f);

			/* Reset current active RenderTexture */
			RenderTexture.active = current;

			/* Cleaning */
			Material.Destroy(material);
			Texture2D.Destroy(texture);
			Texture2D.Destroy(renderTexture);

			return ret;
		}

		#region DLL Import
		#if !CRIWARE_ENABLE_HEADLESS_MODE
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		private static extern bool criManaUnity_IsMediaCodecSupported_ANDROID(int device_type);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		private static extern System.UInt32 criManaUnity_MediaCodecCreateTexture_ANDROID();

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		private static extern void criManaUnity_MediaCodecDeleteTexture_ANDROID(System.UInt32 oes_texture);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		private static extern bool criManaUnityPlayer_MediaCodecAttachTexture_ANDROID(int player_id, System.UInt32 oes_texture);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		private static extern void criManaUnityPlayer_MediaCodecDetachTexture_ANDROID(int player_id, System.UInt32 oes_texture);
		#else
		private static bool criManaUnity_IsMediaCodecSupported_ANDROID(int device_type) { return true; }
		private static System.UInt32 criManaUnity_MediaCodecCreateTexture_ANDROID() { return 0x0; }
		private static void criManaUnity_MediaCodecDeleteTexture_ANDROID(System.UInt32 oes_texture) { }
		private static bool criManaUnityPlayer_MediaCodecAttachTexture_ANDROID(int player_id, System.UInt32 oes_texture) { return true; }
		private static void criManaUnityPlayer_MediaCodecDetachTexture_ANDROID(int player_id, System.UInt32 oes_texture) { }
		#endif
		#endregion

	}
}

} //namespace CriWare

#endif
