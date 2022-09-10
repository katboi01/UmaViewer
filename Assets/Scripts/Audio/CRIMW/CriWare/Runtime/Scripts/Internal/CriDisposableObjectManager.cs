/****************************************************************************
 *
 * Copyright (c) 2017 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !(UNITY_WEBGL || UNITY_SWITCH || UNITY_STADIA || ENABLE_IL2CPP)
#define CRIWARE_DISPOSABLEOBJECTMANAGER_USE_WEAKREF
#endif

using System;
using System.Collections.Generic;

namespace CriWare {

public abstract class CriDisposable : IDisposable
{
	public Guid guid;

	public CriDisposable()
	{
		guid = Guid.NewGuid();
	}

	public abstract void Dispose();
}

public static class CriDisposableObjectManager
{
	public enum ModuleType
	{
		Atom,
		AtomMic,
		Fs,
		FsWeb,
		Mana,
		Lips,
		Vip,
		Rtc,
	}

	public struct ObjectRef
	{
		public Guid guid;
		public ModuleType type;
	#if CRIWARE_DISPOSABLEOBJECTMANAGER_USE_WEAKREF
		public WeakReference weakRef;
	#else
		/* WeakReference with trackResurrection is not supported currently. */
		public CriDisposable disposable;
	#endif

	#if CRIWARE_DISPOSABLEOBJECTMANAGER_USE_WEAKREF
		public ObjectRef(Guid _guid, WeakReference _weakRef, ModuleType _type)
		{
			guid = _guid;
			type = _type;
			weakRef = _weakRef;
		}
	#else
		public ObjectRef(Guid _guid, CriDisposable _disposable, ModuleType _type)
		{
			guid = _guid;
			type = _type;
			disposable = _disposable;
		}
	#endif
	}

	private static List<ObjectRef> refList = new List<ObjectRef>();

	private static int SearchForDisposable(CriDisposable disposable){
		lock (refList) {
			int listCount = CriDisposableObjectManager.refList.Count;
			for (int i = listCount - 1; i >= 0; --i) {
				if (CriDisposableObjectManager.refList[i].guid == disposable.guid) {
					return i;
				}
			}
		}
		return -1;
	}

	public static void Register(CriDisposable disposable, ModuleType type)
	{
		if (CriDisposableObjectManager.SearchForDisposable(disposable) >= 0) {
			UnityEngine.Debug.LogWarning("[CRIWARE] Internal: Duplicated object GUID");
			return;
		}

	#if CRIWARE_DISPOSABLEOBJECTMANAGER_USE_WEAKREF
		/* Keep weak reference until object finalized */
		var weakRef = new WeakReference(disposable, true);
		lock (refList) {
			CriDisposableObjectManager.refList.Add(new ObjectRef(disposable.guid, weakRef, type));
		}
	#else
		/* Keep reference directly */
		lock (refList) {
			CriDisposableObjectManager.refList.Add(new ObjectRef(disposable.guid, disposable, type));
		}
	#endif
	}

	public static bool Unregister(CriDisposable disposable)
	{
		lock (refList) {
			int index = CriDisposableObjectManager.SearchForDisposable(disposable);
			if (index >= 0) {
				CriDisposableObjectManager.refList.RemoveAt(index);
				return true;
			}
		}
		return false;
	}

	public static bool IsDisposed(CriDisposable disposable)
	{
		return (CriDisposableObjectManager.SearchForDisposable(disposable) < 0) ;
	}

	public static void CallOnModuleFinalization(ModuleType type)
	{
		CriDisposableObjectManager.DisposeAll(type);
	}

	private static int GetNextWithType(ModuleType type)
	{
		int listCount = CriDisposableObjectManager.refList.Count;
		for (int i = listCount - 1; i >= 0; --i) {
			if (CriDisposableObjectManager.refList[i].type == type) {
				return i;
			}
		}
		return -1;
	}

	public static void DisposeAll(ModuleType type)
	{
		lock (refList) {
			while (true) {
				int index = GetNextWithType(type);
				if (index < 0) {
					break;
				}
	#if CRIWARE_DISPOSABLEOBJECTMANAGER_USE_WEAKREF
				var target = (CriDisposable)CriDisposableObjectManager.refList[index].weakRef.Target;
	#else
				var target = CriDisposableObjectManager.refList[index].disposable;
	#endif
				if (target != null) {
					target.Dispose();
				} else {
					/* Unsafe fallback */
					UnityEngine.Debug.LogWarning("[CRIWARE] Internal: Object disposal(Type:" +
												 CriDisposableObjectManager.refList[index].type.ToString() +
												 ") not handled by CriDisposableObjectManager; " +
												 "memory leak may have occured.");
					CriDisposableObjectManager.refList.RemoveAt(index);
				}
			}
		}
	}
}

} //namespace CriWare