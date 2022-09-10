/****************************************************************************
 *
 * Copyright (c) 2015 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;

/**
 * \addtogroup CRIMANA_UNITY_COMPONENT
 * @{
 */

namespace CriWare {

/**
 * <summary>A component for playing movie on Unity UI.</summary>
 * <remarks>
 * <para header='Description'>A component for playing movie on Unity UI.<br/>
 * The movie is displayed by setting a Material to UnityEngine.UI.Graphic.<br/>
 * It inherits CriManaMovieMaterial.<br/></para>
 * <para header='Note'>In this class, you can only perform basic operations such as play, stop, or pause.<br/>
 * If you want to perform complicated playback control, operate the core player using the player property.<br/></para>
 * </remarks>
 */
[AddComponentMenu("CRIWARE/CriManaMovieControllerForUI")]
public class CriManaMovieControllerForUI : CriManaMovieMaterial
{
	#region Properties
	/**
	 * <summary>A UnityEngine.UI.Graphic to which the movie Material is set.</summary>
	 * <remarks>
	 * <para header='Description'>A UnityEngine.UI.Graphic to which the movie Material is set.<br/>
	 * If not specified, UnityEngine.UI.Graphic of the attached game object is used.</para>
	 * </remarks>
	 */
	public UnityEngine.UI.Graphic   target;


	/**
	 * <summary>Whether to display the original Material when movie frames are not available.</summary>
	 * <remarks>
	 * <para header='Description'>Whether to display the original material when a movie frame is not available.<br/>
	 * true : display the original material when a movie frame is not available.<br/>
	 * false : disable the rendering of the target when a movie frame is not available.<br/></para>
	 * </remarks>
	 */
	public bool                     useOriginalMaterial;
	#endregion


	#region Internal Variables
	private Material    originalMaterial;
	#endregion


	protected override void Awake()
	{
		uiRenderMode = true;
		base.Awake();
	}

	public override void CriInternalUpdate()
	{
		base.CriInternalUpdate();

		// If there is a target connected but current GameObject is not a Renderer,
		// we check target activation an then update movie material if active.
		if (renderMode == RenderMode.OnVisibility) {
			if (HaveRendererOwner == false && target != null && target.IsActive()) {
				player.OnWillRenderObject(this);
			}
		}
	}


	public override bool RenderTargetManualSetup()
	{
		if (target == null) {
			target = gameObject.GetComponent<UnityEngine.UI.Graphic>();
		}
		if (target == null) {
			Debug.LogError("[CRIWARE] Missing render target for the Mana Controller component: Please add a renderer to the GameObject or specify the target manually.");
			return false;
		}
		originalMaterial = target.materialForRendering;
		if (!useOriginalMaterial) {
			target.enabled = false;
		}
		return true;
	}


	public override void RenderTargetManualFinalize()
	{
		if (target != null) {
			target.material = originalMaterial;
			if (!useOriginalMaterial) {
				target.enabled = false;
			}
		}
		originalMaterial = null;
	}

	protected override void OnMaterialAvailableChanged()
	{
		if (target == null) {
			return;
		}

		if (isMaterialAvailable) {
			target.material = material;
			target.enabled  = true;
		} else {
			target.material = originalMaterial;
			if (!useOriginalMaterial) {
				target.enabled = false;
			}
		}
	}
}

} //namespace CriWare
/**
 * @}
 */


/* end of file */
