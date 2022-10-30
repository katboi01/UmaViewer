/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEditor;
using UnityEngine;

namespace CriWare {

public class CriWareEditor : UnityEditor.Editor
{
	[MenuItem("GameObject/CRIWARE/Create CRIWARE Library Initializer", false, 150)]
	public static void CreateCriwareLibraryInitalizer()
	{
		CriWareInitializer[] criWareInitializerList = FindObjectsOfType(typeof(CriWareInitializer)) as CriWareInitializer[];
		if (criWareInitializerList.Length > 0) {
			Debug.LogError("\"CriWareLibraryInitializer\" already exists.");

			Selection.activeGameObject = criWareInitializerList[0].gameObject;

		} else {
			GameObject go = null;
			go = new GameObject("CriWareLibraryInitializer");

			go.AddComponent<CriWareInitializer>();

			Selection.activeGameObject = go;
		}
	}

	[MenuItem("GameObject/CRIWARE/Create CRIWARE Error Handler", false, 150)]
	public static void CreateCriwareErrorHandler()
	{
		CriWareErrorHandler[] criWareErrorHandlerList = FindObjectsOfType(typeof(CriWareErrorHandler)) as CriWareErrorHandler[];
		if (criWareErrorHandlerList.Length > 0) {
			Debug.LogError("\"CriWareErrorHandler\" already exists.");

			Selection.activeGameObject = criWareErrorHandlerList[0].gameObject;

		} else {
			GameObject go = null;
			go = new GameObject("CriWareErrorHandler");

			go.AddComponent<CriWareErrorHandler>();

			Selection.activeGameObject = go;
		}
	}

} // end of class

} //namespace CriWare
/* end of file */
