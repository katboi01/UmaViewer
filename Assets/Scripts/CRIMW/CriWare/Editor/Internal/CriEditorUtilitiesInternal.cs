/****************************************************************************
 *
 * Copyright (c) 2021 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/
#if UNITY_2019_3_OR_NEWER
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CriMw.CriWare.Adxlipsync.Editor")]
[assembly: InternalsVisibleTo("CriMw.CriWare.Assets.Editor")]

namespace CriWare {

	internal static class CriEditorUtilitiesInternal
	{
		public static float SingleReturnHeight
		{
			get
			{
				return EditorGUIUtility.singleLineHeight + 2f;
			}
		}

		public static string BackingFieldNameOf(string propertyName)
		{
			return "<" + propertyName + ">k__BackingField";
		}

		static Dictionary<System.Type, List<System.Type>> typesCache = new Dictionary<System.Type, List<System.Type>>();
		public static List<System.Type> GetSubclassesOf(System.Type baseType)
		{
			if (typesCache.ContainsKey(baseType))
				return typesCache[baseType];

			var types = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(assem => assem.GetTypes()).
						Where(t => baseType.IsAssignableFrom(t) && !(t == baseType)).ToList();
			typesCache.Add(baseType, types);
			return types;
		}
	}
} //namespace CriWare

#endif