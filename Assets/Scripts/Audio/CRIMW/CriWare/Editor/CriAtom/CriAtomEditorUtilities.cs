/****************************************************************************
 *
 * Copyright (c) 2021 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

/**
 * \addtogroup CRIWARE_EDITOR_UTILITY
 * @{
 */

/**
 * <summary>CriWare.Editor namespace</summary>
 * <remarks>
 * <para header='Description'>This is a namespace which includes a collection of classes that can be used in the Unity Editor environment.</para>
 * </remarks>
 */
namespace CriWare.Editor {

/**
 * <summary>Class for the editor-only features of CRI Atom</summary>
 * <remarks>
 * <para header='Description'>Class that contains the editor-specific functions of CRI Atom.</para>
 * <para header='Note'>When using this class with the Assembly Definition version plug-in,<br/>
 * it is necessary to reference the following assembly definitions:<br/>
 * - CriMw.CriWare.Editor<br/>
 * - CriMw.CriWare.Runtime<br/></para>
 * </remarks>
 */
public class CriAtomEditorUtilities
{
	/**
	 * <summary>Initialize the CRI Atom library in the editor</summary>
	 * <returns>Whether the initialization process was executed</returns>
	 * <remarks>
	 * <para header='Description'>This method is used to initialize the CRI Atom library dedicated to the Edit mode. <br/>
	 * The Atom library will be initialized according to the Atom editor settings in the project settings. <br/>
	 * Depending on the project settings, it is also possible to initialize
	 * using the settings of the CriWareInitializer component in the currently loaded scene.</para>
	 * </remarks>
	 */
	public static bool InitializeLibrary() {
		bool settingChanged = CriAtomEditorSettings.Instance.GetChangeStatusOnce();
		if (CriAtomPlugin.IsLibraryInitialized() && settingChanged == false) {
			return false;
		}
		if (settingChanged) { 
			CriAtomPlugin.FinalizeLibrary();
		}

		CriAtomConfig atomConfigEditor;
		if (CriAtomEditorSettings.Instance.TrySceneSettings == false) {
			atomConfigEditor = CriAtomEditorSettings.Instance.AtomConfig;
		} else {
			CriWareInitializer criInitializer = GameObject.FindObjectOfType<CriWareInitializer>();
			if (criInitializer != null) {
				atomConfigEditor = criInitializer.atomConfig;
			} else {
				atomConfigEditor = CriAtomEditorSettings.Instance.AtomConfig;
				Debug.Log("[CRIWARE] Atom Preview: No CriWareInitializer component found in current scene. " +
					"Using project settings instead.");
			}
		}
		return CriWareInitializer.InitializeAtom(atomConfigEditor);
	}

	/**
	 * <summary>Audio player dedicated to previewing in the editor</summary>
	 * <remarks>
	 * <para header='Description'>This class is used to preview the playback of ADX2 audio in the Edit mode. <br/>
	 * The loading of the relevant ACB data must be managed externally.</para>
	 * </remarks>
	 */
	public class PreviewPlayer : CriDisposable {
		public CriAtomExPlayer player { get; private set; }
		private bool finalizeSuppressed = false;
		private bool isPlayerReady = false;

		private void Initialize() {
			CriAtomEditorUtilities.InitializeLibrary();
			if (CriAtomPlugin.IsLibraryInitialized() == false) { return; }

			player = new CriAtomExPlayer();
			if (player == null) { return; }

			player.SetPanType(CriAtomEx.PanType.Pan3d);
			player.UpdateAll();

			isPlayerReady = true;

			CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);

			if (finalizeSuppressed) {
				GC.ReRegisterForFinalize(this);
			}
		}

		/**
		 * <summary>Initialization of the player</summary>
		 */
		public PreviewPlayer() {
			Initialize();
		}

		/**
		 * <summary>Destroying the player</summary>
		 */
		public override void Dispose() {
			this.dispose();
			GC.SuppressFinalize(this);
			finalizeSuppressed = true;
		}

		private void dispose() {
			CriDisposableObjectManager.Unregister(this);
			if (player != null) {
				player.Dispose();
				player = null;
			}
			this.isPlayerReady = false;
		}

		~PreviewPlayer() {
			this.dispose();
		}
		
		/**
		 * <summary>Set the audio data and play</summary>
		 * <param name='acb'>ACB data</param>
		 * <param name='cueName'>Cue name</param>
		 * <remarks>
		 * <para header='Description'>Specify the ACB data and Cue name and then play.</para>
		 * </remarks>
		 */
		public void Play(CriAtomExAcb acb, string cueName) {
			if (isPlayerReady == false) {
				this.Initialize();
			}

			if (acb != null) {
				if (player != null) {
					player.SetCue(acb, cueName);
					player.Start();
				} else {
					Debug.LogWarning("[CRIWARE] Player is not ready. Please try reloading the inspector / editor window");
				}
			} else {
				Debug.LogWarning("[CRIWARE] ACB data is not set for previewing playback");
			}
		}

		/**
		 * <summary>Stops the playback</summary>
		 * <param name='withoutRelease'>Whether to stop without release time</param>
		 * <remarks>
		 * <para header='Description'>Stops all audio playback.</para>
		 * </remarks>
		 */
		public void Stop(bool withoutRelease = false) {
			if (player != null) {
				if (withoutRelease) {
					player.StopWithoutReleaseTime();
				} else {
					player.Stop();
				}
			}
		}

		/**
		 * <summary>Resets the parameters of the player</summary>
		 */
		public void ResetPlayer() {
			player.SetVolume(1f);
			player.SetPitch(0);
			player.Loop(false);
		}
	}

} // end of class

} //namespace CriWare.Editor

/**
 * @}
 */

/* end of file */