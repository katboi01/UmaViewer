using System;
using UnityEngine;

namespace RuntimeGizmos
{
	public static class ExtTransform
	{
		//This acts as if you are using a parent transform as your new pivot and transforming that parent instead of the child.
		//So instead of creating a gameobject and parenting "target" to it and translating only the parent gameobject, we can use this method.
		public static void SetScaleFrom(this Transform target, Vector3 worldPivot, Vector3 newScale)
		{
			Vector3 localOffset = target.InverseTransformPoint(worldPivot);

			Vector3 localScale = target.localScale;
			Vector3 scaleRatio = new Vector3(ExtMathf.SafeDivide(newScale.x, localScale.x), ExtMathf.SafeDivide(newScale.y, localScale.y), ExtMathf.SafeDivide(newScale.z, localScale.z));
			Vector3 scaledLocalOffset = Vector3.Scale(localOffset, scaleRatio);

			Vector3 newPosition = target.TransformPoint(localOffset - scaledLocalOffset);

			target.localScale = newScale;
			target.position = newPosition;
		}

		//This acts as if you are scaling based on a point that is offset from the actual pivot.
		//It gives results similar to when you scale an object in the unity editor when in Center mode instead of Pivot mode.
		//The Center was an offset from the actual Pivot.
		public static void SetScaleFromOffset(this Transform target, Vector3 worldPivot, Vector3 newScale)
		{
			//Seemed to work, except when under a parent that has a non uniform scale and rotation it was a bit off.
			//This might be due to transform.lossyScale not being accurate under those conditions, or possibly something else is wrong...
			//Maybe things can work if we can find a way to convert the "newPosition = ..." line to use Matrix4x4 for possibly more scale accuracy.
			//However, I have tried and tried and have no idea how to do that kind of math =/
			//Seems like unity editor also has some inaccuracies with skewed scales, such as scaling little by little compared to scaling one large scale.
			//
			//Will mess up or give undesired results if the target.localScale or target.lossyScale has any set to 0.
			//Unity editor doesnt even allow you to scale an axis when it is set to 0.

			Vector3 localOffset = target.InverseTransformPoint(worldPivot);

			Vector3 localScale = target.localScale;
			Vector3 scaleRatio = new Vector3(ExtMathf.SafeDivide(newScale.x, localScale.x), ExtMathf.SafeDivide(newScale.y, localScale.y), ExtMathf.SafeDivide(newScale.z, localScale.z));
			Vector3 scaledLocalOffset = Vector3.Scale(localOffset, scaleRatio);

			Vector3 newPosition = target.rotation * Vector3.Scale(localOffset - scaledLocalOffset, target.lossyScale) + target.position;

			target.localScale = newScale;
			target.position = newPosition;
		}

		public static Vector3 GetCenter(this Transform transform, CenterType centerType)
		{
			if(centerType == CenterType.Solo)
			{
				Renderer renderer = transform.GetComponent<Renderer>();
				if(renderer != null)
				{
					return renderer.bounds.center;
				}else{
					return transform.position;
				}
			}
			else if(centerType == CenterType.All)
			{
				Bounds totalBounds = new Bounds(transform.position, Vector3.zero);
				GetCenterAll(transform, ref totalBounds);
				return totalBounds.center;
			}

			return transform.position;
		}
		static void GetCenterAll(this Transform transform, ref Bounds currentTotalBounds)
		{
			Renderer renderer = transform.GetComponent<Renderer>();
			if(renderer != null)
			{
				currentTotalBounds.Encapsulate(renderer.bounds);
			}else{
				currentTotalBounds.Encapsulate(transform.position);
			}

			for(int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).GetCenterAll(ref currentTotalBounds);
			}
		}
	}
}
