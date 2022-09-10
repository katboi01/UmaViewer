/****************************************************************************
 *
 * Copyright (c) 2015-2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CriWare {

namespace CriMana.Detail
{
	public abstract class RendererResource : System.IDisposable
	{
		private bool disposed = false;
		protected Shader shader;
		protected Material currentMaterial;
		protected bool hasAlpha;
		protected bool additive;
		protected bool applyTargetAlpha;
		protected bool ui;

		~RendererResource()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposed) {
				return;
			}
			if (disposing) {
				OnDisposeManaged();
			}
			OnDisposeUnmanaged();
			disposed = true;
		}

		public int GetNumberOfFrameBeforeDestroy(int playerId)
		{
			return CRIWAREB970989D(playerId);
		}

		protected void SetupStaticMaterialProperties()
		{
			if (currentMaterial == null) {
				return;
			}

			int srcBlendMode, dstBlendMode;
			GetBlendModes(out srcBlendMode, out dstBlendMode);

			if (currentMaterial.shader != shader) {
				currentMaterial.shader = shader;
			}
			currentMaterial.SetInt("_SrcBlendMode", srcBlendMode);
			currentMaterial.SetInt("_DstBlendMode", dstBlendMode);
			currentMaterial.SetInt("_CullMode", ui ? 0 : 2);
			currentMaterial.SetInt("_ZWriteMode", ui ? 0 : 1);
			SetKeyword(currentMaterial, "CRI_ALPHA_MOVIE", hasAlpha);
			SetKeyword(currentMaterial, "CRI_APPLY_TARGET_ALPHA", applyTargetAlpha);
			SetKeyword(currentMaterial, "CRI_LINEAR_COLORSPACE", (QualitySettings.activeColorSpace == ColorSpace.Linear));
		}

		private void GetBlendModes(out int srcBlendMode, out int dstBlendMode)
		{
			srcBlendMode = additive ? (int)UnityEngine.Rendering.BlendMode.One : (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
			dstBlendMode = (additive && !hasAlpha) ? (int)UnityEngine.Rendering.BlendMode.One : (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
		}

		public virtual void SetApplyTargetAlpha(bool flag)
		{
			applyTargetAlpha = flag;
			SetupStaticMaterialProperties();
		}

		public virtual void SetUiRenderMode(bool flag)
		{
			ui = flag;
			SetupStaticMaterialProperties();
		}

		protected abstract void OnDisposeManaged();

		protected abstract void OnDisposeUnmanaged();

		public abstract bool IsPrepared();

		public abstract bool ContinuePreparing();

		public abstract void AttachToPlayer(int playerId);

		public abstract bool UpdateFrame(int playerId, FrameInfo frameInfo, ref bool frameDrop);

		public abstract bool UpdateMaterial(UnityEngine.Material material);

		public abstract void UpdateTextures();

		public abstract bool IsSuitable(int playerId, MovieInfo movieInfo, bool additive, UnityEngine.Shader userShader);

		public virtual void OnPlayerPause(bool pauseStatus, bool triggredFromApplciationPause) {}

		public virtual void OnPlayerStop() { }

		public virtual bool OnPlayerStopForSeek() { return false; }

		public virtual void OnPlayerStart() { }

		public virtual bool ShouldSkipDestroyOnStopForSeek() { return false; }

		public virtual bool HasRenderedNewFrame() { return true; }

		public static uint NextPowerOfTwo(uint x)
		{
			x = x - 1;
			x = x | (x >> 1);
			x = x | (x >> 2);
			x = x | (x >> 4);
			x = x | (x >> 8);
			x = x | (x >>16);
			return x + 1;
		}

		public static int NextPowerOfTwo(int x)
		{
			return (int)NextPowerOfTwo((uint)x);
		}

		public static int CeilingWith(int x, int ceilingValue)
		{
			return (x+ceilingValue-1) & -ceilingValue;
		}

		public static int Ceiling16(int x)
		{
			return (x+15)& -16;
		}

		public static int Ceiling32(int x)
		{
			return (x+31)& -32;
		}

		public static int Ceiling64(int x)
		{
			return (x+63)& -64;
		}

		public static int Ceiling256(int x)
		{
			return (x+255)& -256;
		}

		protected static void DisposeTextures(Texture[] textures)
		{
			if (textures == null) {
				return;
			}

			for (int i = 0; i < textures.Length; i++) {
				if (textures[i] != null) {
#if UNITY_EDITOR
					if (UnityEditor.EditorApplication.isPlaying == false) {
						Texture2D.DestroyImmediate(textures[i]);
					} else {
						Texture2D.Destroy(textures[i]);
					}
#else
					Texture2D.Destroy(textures[i]);
#endif
					textures[i] = null;
				}
			}
		}

		protected static void SetKeyword(Material material, string keyword, bool flag)
		{
			if (flag) {
				material.EnableKeyword(keyword);
			} else {
				material.DisableKeyword(keyword);
			}
		}

		#region DLL Import
		#if !CRIWARE_ENABLE_HEADLESS_MODE
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		protected static extern bool CRIWARE9061FF01(
			int player_id,
			int num_textures,
			System.IntPtr[] tex_ptrs,
			[In, Out] FrameInfo frame_info,
            ref bool frame_drop // in -> Can drop a frame?, out -> Is Frame dropped?
		);

		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		protected static extern bool CRIWAREDA63C3D7(
			int player_id,
			int num_textures,
			[In, Out] System.IntPtr[] tex_ptrs
		);

        [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
        protected static extern bool CRIWARE71A23DE1(
            int player_id,
            int num_textures,
            [In, Out] System.IntPtr[] tex_ptrs
        );

        [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
        protected static extern bool CRIWARE2676F2A5(
            int player_id,
            int num_textures,
            [In, Out] System.IntPtr[] tex_ptrs
        );

        [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
        protected static extern sbyte CRIWAREB970989D(
            int player_id
        );
		#else
		protected static bool CRIWARE9061FF01(
			int player_id,
			int num_textures,
			System.IntPtr[] tex_ptrs,
			[In, Out] FrameInfo frame_info,
			ref bool frame_drop
		) { return true; }

		protected static bool CRIWAREDA63C3D7(
			int player_id,
			int num_textures,
			[In, Out] System.IntPtr[] tex_ptrs
		) { return true; }

		protected static bool CRIWARE71A23DE1(
			int player_id,
			int num_textures,
			[In, Out] System.IntPtr[] tex_ptrs
		) { return true; }

		protected static bool CRIWARE2676F2A5(
			int player_id,
			int num_textures,
			[In, Out] System.IntPtr[] tex_ptrs
		) { return true; }

        protected static sbyte CRIWAREB970989D(
            int player_id
        ) { return 0; }
		#endif
		#endregion
    }




	public abstract class RendererResourceFactory : System.IDisposable
	{
		#region Static
		static private SortedList<int, RendererResourceFactory> factoryList = new SortedList<int, RendererResourceFactory>();

		static public void RegisterFactory(RendererResourceFactory factory, int priority)
		{
			factoryList.Add(priority, factory);
		}

		static public void DisposeAllFactories()
		{
			foreach (var factoryWithPriority in factoryList) {
				factoryWithPriority.Value.Dispose();
			}
			factoryList.Clear();
		}

		static public RendererResource DispatchAndCreate(int playerId, MovieInfo movieInfo, bool additive, UnityEngine.Shader userShader)
		{
			RendererResource    rendererResource    = null;

			foreach (var factoryWithPriority in factoryList) {
				rendererResource = factoryWithPriority.Value.CreateRendererResource(playerId, movieInfo, additive, userShader);
				if (rendererResource != null) {
					return rendererResource;
				}
			}

			UnityEngine.Debug.LogError("[CRIWARE] unsupported movie.");
			return null;
		}
		#endregion


		#region Instance
		private bool disposed = false;

		~RendererResourceFactory()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposed) {
				return;
			}
			if (disposing) {
				OnDisposeManaged();
			}
			OnDisposeUnmanaged();
			disposed = true;
		}

		protected abstract void OnDisposeManaged();

		protected abstract void OnDisposeUnmanaged();

		public abstract RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, UnityEngine.Shader userShader);
		#endregion
	}




	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class RendererResourceFactoryPriorityAttribute : System.Attribute
	{
		public readonly int priority;
		public RendererResourceFactoryPriorityAttribute(int priority)
		{
			this.priority = priority;
		}
	}




	public static partial class AutoResisterRendererResourceFactories
	{
		public static void InvokeAutoRegister()
		{
			var factoryTypes = typeof(AutoResisterRendererResourceFactories).GetNestedTypes(BindingFlags.Public);
			foreach (var factoryType in factoryTypes) {
#if !UNITY_EDITOR && (ENABLE_DOTNET || (UNITY_WINRT && !ENABLE_IL2CPP))
				if (!factoryType.GetTypeInfo().IsSubclassOf(typeof(RendererResourceFactory))) {
#else
				if (!factoryType.IsSubclassOf(typeof(RendererResourceFactory))) {
#endif
					UnityEngine.Debug.LogError("[CRIWARE] internal logic error. " + factoryType.Name + " is required to be a subclass of RendererResourceFactory.");
					continue;
				}
#if !UNITY_EDITOR && (ENABLE_DOTNET || (UNITY_WINRT && !ENABLE_IL2CPP))
				var priorityAttribute = (RendererResourceFactoryPriorityAttribute)CustomAttributeExtensions.GetCustomAttribute(
					factoryType.GetTypeInfo(),
					typeof(RendererResourceFactoryPriorityAttribute)
					);
#else
				var priorityAttribute = (RendererResourceFactoryPriorityAttribute)System.Attribute.GetCustomAttribute(
					factoryType,
					typeof(RendererResourceFactoryPriorityAttribute)
					);
#endif
				if (priorityAttribute == null) {
					UnityEngine.Debug.LogError("[CRIWARE] internal logic error. need priority attribute. (" + factoryType.Name + ")");
					continue;
				}
				RendererResourceFactory.RegisterFactory(
					(RendererResourceFactory)System.Activator.CreateInstance(factoryType),
					priorityAttribute.priority
					);
			}
		}
	}

#if CRIWARE_ENABLE_HEADLESS_MODE
	public static partial class AutoResisterRendererResourceFactories
	{
		[RendererResourceFactoryPriority(4000)]
		public class RendererResourceFactoryDummy : RendererResourceFactory
		{
			public override RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, UnityEngine.Shader userShader)
			{
				return new RendererResourceDummy(playerId, movieInfo, additive, userShader);
			}
			protected override void OnDisposeManaged() { }
			protected override void OnDisposeUnmanaged() { }
		}
	}

	public class RendererResourceDummy : RendererResource
	{
		public RendererResourceDummy(int playerId, MovieInfo movieInfo, bool additive, UnityEngine.Shader userShader) { }
		protected override void OnDisposeManaged() { }
		protected override void OnDisposeUnmanaged() { }
		public override bool IsPrepared() { return true; }
		public override bool ContinuePreparing() { return true; }
		public override bool IsSuitable(int playerId, MovieInfo movieInfo, bool additive, UnityEngine.Shader userShader) { return true; }
		public override void SetApplyTargetAlpha(bool flag) { }
		public override void AttachToPlayer(int playerId) { }
		public override bool UpdateFrame(int playerId, FrameInfo frameInfo, ref bool frameDrop) { return true; }
		public override bool UpdateMaterial(UnityEngine.Material material) { return true; }
		private void UpdateMovieTextureST(System.UInt32 dispWidth, System.UInt32 dispHeight) { }
		public override void UpdateTextures(){ }
	}
#endif
}

} //namespace CriWare