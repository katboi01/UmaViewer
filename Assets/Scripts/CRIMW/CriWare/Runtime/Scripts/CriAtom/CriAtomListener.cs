/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

/**
 * \addtogroup CRIATOM_UNITY_COMPONENT
 * @{
 */

namespace CriWare {

/**
 * <summary>A component that represents a 3D listener.</summary>
 * <remarks>
 * <para header='Description'>Usually, it is used by attaching it to the GameObject object of the camera or main character.
 * The current position is updated automatically, so no special operation or setting is required.</para>
 * </remarks>
 */
[AddComponentMenu("CRIWARE/CRI Atom Listener")]
public class CriAtomListener : CriMonoBehaviour
{
	#region CRIWARE internals
	public static void CreateDummyNativeListener()
	{
		if (dummyNativeListener == null) {
			dummyNativeListener = new CriAtomEx3dListener();
		}
	}

	public static void DestroyDummyNativeListener()
	{
		if (dummyNativeListener != null) {
			dummyNativeListener.Dispose();
			dummyNativeListener = null;
		}
	}
	#endregion

	#region Fields & Properties
	/**
	 * <summary>This is the CriAtomEx3dListener used internally.</summary>
	 * <remarks>
	 * <para header='Description'>If you want to control CriAtom3dListener directly, get CriAtom3dListener from this property.</para>
	 * </remarks>
	 */
	public CriAtomEx3dListener nativeListener { get; protected set; }

	[SerializeField] CriAtomRegion regionOnStart = null;

	/**
	 * <summary>Whether to make it an exclusive active listener in OnEnable</summary>
	 * <remarks>
	 * <para header='Description'>If true, when OnEnable is called, this listener will be activated
	 * and all the other listeners will be deactivated.
	 * If false, it will become active without affecting other listeners.</para>
	 * </remarks>
	 */
	public bool activateListenerOnEnable = false;

	/**
	 * <summary>Whether the CriAtomListener is active</summary>
	 * <remarks>
	 * <para header='Description'>An active CriAtomListener acts as a listener for CriAtomSource audio. <br/>
	 * If there are multiple CriAtomListeners, The 3D sound will be calculated <br/>
	 * using the active listener that is closest to the CriAtomSource.</para>
	 * </remarks>
	 */
	public bool isActive {
		get { return _isActive; }
		set {
			if (_isActive == value) return;
			_isActive = value;
			if (value)
				UpdatePosition();
			else {
				/* Make the listener unactive by setting far position. */
				nativeListener.SetPosition(float.MaxValue, float.MaxValue, float.MaxValue);
				nativeListener.Update();
			}
		}
	}

	/**
	 * <summary>Sets/gets 3D region of the sound source</summary>
	 */
	public CriAtomRegion region3d
	{
		get { return this.currentRegion; }
		set {
			if (this.currentRegion == value) {
				return;
			}
			/* Remove the reference frome the current region  */
			if (this.currentRegion != null) {
				this.currentRegion.referringListeners.Remove(this);
			}
			CriAtomEx3dRegion regionHandle = (value == null) ? null : value.region3dHn;
			if (this.nativeListener != null) {
				this.nativeListener.Set3dRegion(regionHandle);
				this.nativeListener.Update();
				this.currentRegion = value;
				/* Seup a reference from a new region */
				if (this.currentRegion != null) {
					this.currentRegion.referringListeners.Add(this);
				}
			} else {
				Debug.LogError("[CRIWARE] Internal: CriAtomListener is not initialized correctly.");
				this.currentRegion = null;
			}
		}
	}
	#endregion

	#region Internal Variables
	static List<CriAtomListener> listenersList = new List<CriAtomListener>();

	/* Dummy listenr used when CriAtomListenr is not exists. */
	static CriAtomEx3dListener dummyNativeListener;

	private Vector3 lastPosition;
	private CriAtomRegion currentRegion = null;
	private bool _isActive = true;
	#endregion

	#region Functions
	private void Awake()
	{
		if (!listenersList.Contains(this))
			listenersList.Add(this);
		DestroyDummyNativeListener();
		nativeListener = new CriAtomEx3dListener();
		isActive = enabled;
	}

	private void Start()
	{
		if (regionOnStart != null) {
			region3d = this.regionOnStart;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ActivateListener(activateListenerOnEnable);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		isActive = false;
	}

	private void OnDestroy()
	{
		if (listenersList.Contains(this)) {
			listenersList.Remove(this);
		}
		region3d = null;
		nativeListener.Dispose();
		nativeListener = null;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		if (this.enabled == false) { return; }
		var criWareLightBlue = new Color(0.332f, 0.661f, 0.991f);
		Gizmos.color = isActive || !Application.isPlaying ? criWareLightBlue : Color.gray;
		Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.forward);
		Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.up);
		UnityEditor.Handles.color = isActive || !Application.isPlaying ? criWareLightBlue : Color.gray;
		UnityEditor.Handles.ArrowHandleCap(1, this.transform.position + this.transform.forward, this.transform.rotation, 1f, EventType.Repaint);
		UnityEditor.Handles.RectangleHandleCap(1, this.transform.position, this.transform.rotation * Quaternion.LookRotation(Vector3.up), 1f, EventType.Repaint);
	}
#endif

	public override void CriInternalUpdate() { }

	public override void CriInternalLateUpdate()
	{
		if (isActive)
			UpdatePosition();
	}

	void UpdatePosition()
	{
		Vector3 position = this.transform.position;
		Vector3 velocity = (position - this.lastPosition) / Time.deltaTime;
		Vector3 front = this.transform.forward;
		Vector3 up = this.transform.up;
		this.lastPosition = position;
		if (nativeListener != null) {
			nativeListener.SetPosition(position.x, position.y, position.z);
			nativeListener.SetVelocity(velocity.x, velocity.y, velocity.z);
			nativeListener.SetOrientation(front.x, front.y, front.z, up.x, up.y, up.z);
			nativeListener.Update();
		}
	}
	#endregion

	/**
	 * <summary>Make it an active listener</summary>
	 * <param name='exclusive'>Whether to make this AtomListener the only active listener</param>
	 * <remarks>
	 * <para header='Description'>When it becomes an active listener, it acts as a 3D listener for CriWare.CriAtomSource .</para>
	 * <para header='Note'>For compatibility with old plugins, when called with no arguments, <br/>
	 * only the CriAtomListener calling this method will be active.</para>
	 * </remarks>
	 */
	public void ActivateListener(bool exclusive = true)
	{
		if (exclusive) {
			foreach (var listener in listenersList) {
				if (listener == this) continue;
				listener.isActive = false;
			}
		}
		isActive = true;
	}
} // end of class

} //namespace CriWare
/** @} */
/* end of file */
