using System;
using CommandUndoRedo;
using UnityEngine;

namespace RuntimeGizmos
{
	public class TransformCommand : ICommand
	{
		TransformValues newValues;
		TransformValues oldValues;

		Transform transform;
		TransformGizmo transformGizmo;

		public TransformCommand(TransformGizmo transformGizmo, Transform transform)
		{
			this.transformGizmo = transformGizmo;
			this.transform = transform;

			oldValues = new TransformValues() {position=transform.position, rotation=transform.rotation, scale=transform.localScale};
		}

		public void StoreNewTransformValues()
		{
			newValues = new TransformValues() {position=transform.position, rotation=transform.rotation, scale=transform.localScale};
		}
		
		public void Execute()
		{
			transform.position = newValues.position;
			transform.rotation = newValues.rotation;
			transform.localScale = newValues.scale;

			transformGizmo.SetPivotPoint();
		}

		public void UnExecute()
		{
			transform.position = oldValues.position;
			transform.rotation = oldValues.rotation;
			transform.localScale = oldValues.scale;

			transformGizmo.SetPivotPoint();
		}

		struct TransformValues
		{
			public Vector3 position;
			public Quaternion rotation;
			public Vector3 scale;
		}
	}
}
