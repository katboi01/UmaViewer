using UnityEngine;
using System.Collections.Generic;

namespace RootMotion {

	/// <summary>
	/// This class contains tools for working with LayerMasks. 
	/// Most of this was copied from Unity Wiki: http://wiki.unity3d.com/index.php?title=LayerMaskExtensions.
	/// </summary>
	public static class LayerMaskExtensions
	{
		/// <summary>
		/// Does the LayerMask contain a specific layer index?
		/// </summary>
		public static bool Contains(LayerMask mask, int layer) {
			return mask == (mask | (1 << layer));
		}

		/// <summary>
		/// Creates a LayerMask from an array of layer names.
		/// </summary>
		public static LayerMask Create(params string[] layerNames)
		{
			return NamesToMask(layerNames);
		}

		/// <summary>
		/// Creates a LayerMask from an array of layer indexes.
		/// </summary>
		public static LayerMask Create(params int[] layerNumbers)
		{
			return LayerNumbersToMask(layerNumbers);
		}

		/// <summary>
		/// Creates a LayerMask from a number of layer names.
		/// </summary>
		public static LayerMask NamesToMask(params string[] layerNames)
		{
			LayerMask ret = (LayerMask)0;
			foreach(var name in layerNames)
			{
				ret |= (1 << LayerMask.NameToLayer(name));
			}
			return ret;
		}

		/// <summary>
		/// Creates a LayerMask from a number of layer indexes.
		/// </summary>
		public static LayerMask LayerNumbersToMask(params int[] layerNumbers)
		{
			LayerMask ret = (LayerMask)0;
			foreach(var layer in layerNumbers)
			{
				ret |= (1 << layer);
			}
			return ret;
		}

		/// <summary>
		/// Inverts a LayerMask.
		/// </summary>
		public static LayerMask Inverse(this LayerMask original)
		{
			return ~original;
		}

		/// <summary>
		/// Adds a number of layer names to an existing LayerMask.
		/// </summary>
		public static LayerMask AddToMask(this LayerMask original, params string[] layerNames)
		{
			return original | NamesToMask(layerNames);
		}

		/// <summary>
		/// Removes a number of layer names from an existing LayerMask.
		/// </summary>
		public static LayerMask RemoveFromMask(this LayerMask original, params string[] layerNames)
		{
			LayerMask invertedOriginal = ~original;
			return ~(invertedOriginal | NamesToMask(layerNames));
		}

		/// <summary>
		/// Returns a string array of layer names from a LayerMask.
		/// </summary>
		public static string[] MaskToNames(this LayerMask original)
		{
			var output = new List<string>();
			
			for (int i = 0; i < 32; ++i)
			{
				int shifted = 1 << i;
				if ((original & shifted) == shifted)
				{
					string layerName = LayerMask.LayerToName(i);
					if (!string.IsNullOrEmpty(layerName))
					{
						output.Add(layerName);
					}
				}
			}
			return output.ToArray();
		}

		/// <summary>
		/// Returns an array of layer indexes from a LayerMask.
		/// </summary>
		public static int[] MaskToNumbers(this LayerMask original)
		{
			var output = new List<int>();
			
			for (int i = 0; i < 32; ++i)
			{
				int shifted = 1 << i;
				if ((original & shifted) == shifted)
				{
					output.Add(i);
				}
			}
			return output.ToArray();
		}

		/// <summary>
		/// Parses a LayerMask to a string.
		/// </summary>
		public static string MaskToString(this LayerMask original)
		{
			return MaskToString(original, ", ");
		}

		/// <summary>
		/// Parses a LayerMask to a string using the specified delimiter.
		/// </summary>
		public static string MaskToString(this LayerMask original, string delimiter)
		{
			return string.Join(delimiter, MaskToNames(original));
		}
	}
}