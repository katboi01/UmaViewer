using System;
using UnityEngine;

namespace RuntimeGizmos
{
//Currently doesnt really handle TransformType.All
	public class TransformGizmoCustomGizmo : MonoBehaviour
	{
		public bool autoFindTransformGizmo = true;
		public TransformGizmo transformGizmo;

		public CustomTransformGizmos customTranslationGizmos = new CustomTransformGizmos();
		public CustomTransformGizmos customRotationGizmos = new CustomTransformGizmos();
		public CustomTransformGizmos customScaleGizmos = new CustomTransformGizmos();

		public bool scaleBasedOnDistance = true;
		public float scaleMultiplier = .4f;

		public int gizmoLayer = 2; //2 is the ignoreRaycast layer. Set to whatever you want.

		LayerMask mask;

		void Awake()
		{
			if(transformGizmo == null && autoFindTransformGizmo)
			{
				transformGizmo = GameObject.FindObjectOfType<TransformGizmo>();
			}

			transformGizmo.manuallyHandleGizmo = true;

			//Since we are using a mesh, rotating can get weird due to how the rotation method works,
			//so we use a different rotation method that will let us rotate by acting like our custom rotation gizmo is a wheel.
			//Can still give weird results depending on camera angle, but I think its more understanding for the user as to why its messing up.
			transformGizmo.circularRotationMethod = true;

			mask = LayerMask.GetMask(LayerMask.LayerToName(gizmoLayer));

			customTranslationGizmos.Init(gizmoLayer);
			customRotationGizmos.Init(gizmoLayer);
			customScaleGizmos.Init(gizmoLayer);
		}

		void OnEnable()
		{
			transformGizmo.onCheckForSelectedAxis += CheckForSelectedAxis;
			transformGizmo.onDrawCustomGizmo += OnDrawCustomGizmos;
		}
		void OnDisable()
		{
			transformGizmo.onCheckForSelectedAxis -= CheckForSelectedAxis;
			transformGizmo.onDrawCustomGizmo -= OnDrawCustomGizmos;
		}

		void CheckForSelectedAxis()
		{
			ShowProperGizmoType();

			if(Input.GetMouseButtonDown(0))
			{
				RaycastHit hitInfo;
				if(Physics.Raycast(transformGizmo.myCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, mask))
				{
					Axis selectedAxis = Axis.None;
					TransformType type = transformGizmo.transformType;

					if(selectedAxis == Axis.None && transformGizmo.TransformTypeContains(TransformType.Move))
					{
						selectedAxis = customTranslationGizmos.GetSelectedAxis(hitInfo.collider);
						type = TransformType.Move;
					}
					if(selectedAxis == Axis.None && transformGizmo.TransformTypeContains(TransformType.Rotate))
					{
						selectedAxis = customRotationGizmos.GetSelectedAxis(hitInfo.collider);
						type = TransformType.Rotate;
					}
					if(selectedAxis == Axis.None && transformGizmo.TransformTypeContains(TransformType.Scale))
					{
						selectedAxis = customScaleGizmos.GetSelectedAxis(hitInfo.collider);
						type = TransformType.Scale;
					}

					transformGizmo.SetTranslatingAxis(type, selectedAxis);
				}
			}
		}

		void OnDrawCustomGizmos()
		{
			if(transformGizmo.TranslatingTypeContains(TransformType.Move)) DrawCustomGizmo(customTranslationGizmos);
			if(transformGizmo.TranslatingTypeContains(TransformType.Rotate)) DrawCustomGizmo(customRotationGizmos);
			if(transformGizmo.TranslatingTypeContains(TransformType.Scale)) DrawCustomGizmo(customScaleGizmos);
		}

		void DrawCustomGizmo(CustomTransformGizmos customGizmo)
		{
			AxisInfo axisInfo = transformGizmo.GetAxisInfo();
			customGizmo.SetAxis(axisInfo);
			customGizmo.SetPosition(transformGizmo.pivotPoint);

			Vector4 totalScaleMultiplier = Vector4.one;
			if(scaleBasedOnDistance)
			{
				totalScaleMultiplier.w *= (scaleMultiplier * transformGizmo.GetDistanceMultiplier());
			}

			if(transformGizmo.transformingType == TransformType.Scale)
			{
				float totalScaleAmount = 1f + transformGizmo.totalScaleAmount;
				if(transformGizmo.translatingAxis == Axis.Any) totalScaleMultiplier += (Vector4.one * totalScaleAmount);
				else if(transformGizmo.translatingAxis == Axis.X) totalScaleMultiplier.x *= totalScaleAmount;
				else if(transformGizmo.translatingAxis == Axis.Y) totalScaleMultiplier.y *= totalScaleAmount;
				else if(transformGizmo.translatingAxis == Axis.Z) totalScaleMultiplier.z *= totalScaleAmount;
			}

			customGizmo.ScaleMultiply(totalScaleMultiplier);
		}

		void ShowProperGizmoType()
		{
			bool hasSelection = transformGizmo.mainTargetRoot != null;
			customTranslationGizmos.SetEnable(hasSelection && transformGizmo.TranslatingTypeContains(TransformType.Move));
			customRotationGizmos.SetEnable(hasSelection && transformGizmo.TranslatingTypeContains(TransformType.Rotate));
			customScaleGizmos.SetEnable(hasSelection && transformGizmo.TranslatingTypeContains(TransformType.Scale));
		}
	}

	[Serializable]
	public class CustomTransformGizmos
	{
		public Transform xAxisGizmo;
		public Transform yAxisGizmo;
		public Transform zAxisGizmo;
		public Transform anyAxisGizmo;

		Collider xAxisGizmoCollider;
		Collider yAxisGizmoCollider;
		Collider zAxisGizmoCollider;
		Collider anyAxisGizmoCollider;

		Vector3 originalXAxisScale;
		Vector3 originalYAxisScale;
		Vector3 originalZAxisScale;
		Vector3 originalAnyAxisScale;

		public void Init(int layer)
		{
			if(xAxisGizmo != null)
			{
				SetLayerRecursively(xAxisGizmo.gameObject, layer);
				xAxisGizmoCollider = xAxisGizmo.GetComponentInChildren<Collider>();
				originalXAxisScale = xAxisGizmo.localScale;
			}
			if(yAxisGizmo != null)
			{
				SetLayerRecursively(yAxisGizmo.gameObject, layer);
				yAxisGizmoCollider = yAxisGizmo.GetComponentInChildren<Collider>();
				originalYAxisScale = yAxisGizmo.localScale;
			}
			if(zAxisGizmo != null)
			{
				SetLayerRecursively(zAxisGizmo.gameObject, layer);
				zAxisGizmoCollider = zAxisGizmo.GetComponentInChildren<Collider>();
				originalZAxisScale = zAxisGizmo.localScale;
			}
			if(anyAxisGizmo != null)
			{
				SetLayerRecursively(anyAxisGizmo.gameObject, layer);
				anyAxisGizmoCollider = anyAxisGizmo.GetComponentInChildren<Collider>();
				originalAnyAxisScale = anyAxisGizmo.localScale;
			}
		}

		public void SetEnable(bool enable)
		{
			if(xAxisGizmo != null && xAxisGizmo.gameObject.activeSelf != enable) xAxisGizmo.gameObject.SetActive(enable);
			if(yAxisGizmo != null && yAxisGizmo.gameObject.activeSelf != enable) yAxisGizmo.gameObject.SetActive(enable);
			if(zAxisGizmo != null && zAxisGizmo.gameObject.activeSelf != enable) zAxisGizmo.gameObject.SetActive(enable);
			if(anyAxisGizmo != null && anyAxisGizmo.gameObject.activeSelf != enable) anyAxisGizmo.gameObject.SetActive(enable);
		}

		public void SetAxis(AxisInfo axisInfo)
		{
			Quaternion lookRotation = Quaternion.LookRotation(axisInfo.zDirection, axisInfo.yDirection);

			if(xAxisGizmo != null) xAxisGizmo.rotation = lookRotation;
			if(yAxisGizmo != null) yAxisGizmo.rotation = lookRotation;
			if(zAxisGizmo != null) zAxisGizmo.rotation = lookRotation;
			if(anyAxisGizmo != null) anyAxisGizmo.rotation = lookRotation;
		}

		public void SetPosition(Vector3 position)
		{
			if(xAxisGizmo != null) xAxisGizmo.position = position;
			if(yAxisGizmo != null) yAxisGizmo.position = position;
			if(zAxisGizmo != null) zAxisGizmo.position = position;
			if(anyAxisGizmo != null) anyAxisGizmo.position = position;
		}

		public void ScaleMultiply(Vector4 scaleMultiplier)
		{
			if(xAxisGizmo != null) xAxisGizmo.localScale = Vector3.Scale(originalXAxisScale, new Vector3(scaleMultiplier.w + scaleMultiplier.x, scaleMultiplier.w, scaleMultiplier.w));
			if(yAxisGizmo != null) yAxisGizmo.localScale = Vector3.Scale(originalYAxisScale, new Vector3(scaleMultiplier.w, scaleMultiplier.w + scaleMultiplier.y, scaleMultiplier.w));
			if(zAxisGizmo != null) zAxisGizmo.localScale = Vector3.Scale(originalZAxisScale, new Vector3(scaleMultiplier.w, scaleMultiplier.w, scaleMultiplier.w + scaleMultiplier.z));
			if(anyAxisGizmo != null) anyAxisGizmo.localScale = originalAnyAxisScale * scaleMultiplier.w;
		}

		public Axis GetSelectedAxis(Collider selectedCollider)
		{
			if(xAxisGizmoCollider != null && xAxisGizmoCollider == selectedCollider) return Axis.X;
			if(yAxisGizmoCollider != null && yAxisGizmoCollider == selectedCollider) return Axis.Y;
			if(zAxisGizmoCollider != null && zAxisGizmoCollider == selectedCollider) return Axis.Z;
			if(anyAxisGizmoCollider != null && anyAxisGizmoCollider == selectedCollider) return Axis.Any;

			return Axis.None;
		}

		void SetLayerRecursively(GameObject gameObject, int layer)
		{
			Transform[] selfAndChildren = gameObject.GetComponentsInChildren<Transform>(true);

			for(int i = 0; i < selfAndChildren.Length; i++)
			{
				selfAndChildren[i].gameObject.layer = layer;
			}
		}
	}
}
