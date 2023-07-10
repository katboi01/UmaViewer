using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	// Custom scene view helpers for the InteractionTrigger
	[CustomEditor(typeof(InteractionTrigger))]
	public class InteractionTriggerInspector : Editor {

		private InteractionTrigger script { get { return target as InteractionTrigger; }}
		
		void OnSceneGUI() {
			if (!script.enabled) return;

			var collider = script.GetComponent<Collider>();
			if (collider != null)
				collider.isTrigger = true;
			else {
				Warning.Log ("InteractionTrigger requires a Collider component.", script.transform, true);
				return;
			}
			
			if (script.ranges.Length == 0) return;
			
			for (int i = 0; i < script.ranges.Length; i++) {
				DrawRange (script.ranges[i], i);
			}
			
			Handles.BeginGUI();	
			int h = script.ranges.Length * 18;
			GUILayout.BeginArea(new Rect(10, 10, 200, h + 25), "InteractionTrigger Visualization", "Window");
			
			// Rotating display
			for (int i = 0; i < script.ranges.Length; i++) {
				script.ranges[i].show = GUILayout.Toggle(script.ranges[i].show, new GUIContent(" Show Range " + i.ToString() + ": " + script.ranges[i].name, string.Empty));
			}
			
			GUILayout.EndArea();
			Handles.EndGUI();
		}
		
		private void DrawRange(InteractionTrigger.Range range, int index) {
			range.name = string.Empty;
			for (int i = 0; i < range.interactions.Length; i++) {
				if (range.name.Length > 50) {
					range.name += "...";
					break;
				}
				
				if (i > 0) range.name += "; ";
			
				for (int e = 0; e < range.interactions[i].effectors.Length; e++) {
					if (e > 0) range.name += ", ";
					range.name += range.interactions[i].effectors[e].ToString();
				}
				
				if (range.interactions[i].interactionObject != null) {
					range.name += ": " + range.interactions[i].interactionObject.name;
				}
				
			}
			
			if (!range.show) return;
			
			Color color = GetColor(index);
			Handles.color = color;
			GUI.color = color;
			
			// Character Position
			DrawCharacterPosition(range, index);
			
			// Camera Position
			DrawCameraPosition(range, index);
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}
		
		private void DrawCharacterPosition(InteractionTrigger.Range range, int index) {
			Vector3 labelPosition = script.transform.position - Vector3.up * index * 0.05f;
			
			if (!range.characterPosition.use) {
				Handles.Label(labelPosition, "Character Position is not used for Range " + index.ToString() + ": " + range.name);
				return;
			}
			range.characterPosition.radius = Mathf.Max(range.characterPosition.radius, 0f);
			if (range.characterPosition.radius <= 0f) {
				Handles.Label(labelPosition, "Character Position radius is zero for Range " + index.ToString() + ": " + range.name);
				return;
			}
			if (range.characterPosition.maxAngle <= 0f) {
				Handles.Label(labelPosition, "Character Position max angle is zero for Range " + index.ToString() + ": " + range.name);
				return;
			}
			
			Vector3 f = script.transform.forward;
			if (range.characterPosition.fixYAxis) f.y = 0f;
			if (f == Vector3.zero) {
				Handles.Label(script.transform.position - Vector3.up * index * 0.05f, "Invalid rotation of InteractionTrigger for Range " + index.ToString() + ": " + range.name);
				return; // Singularity
			}
			
			Quaternion triggerRotation = Quaternion.LookRotation(f, (range.characterPosition.fixYAxis? Vector3.up: script.transform.up));
			
			Vector3 position = script.transform.position + triggerRotation * range.characterPosition.offset3D;
			
			Vector3 direction = triggerRotation * range.characterPosition.direction3D;
			
			Quaternion rotation = direction == Vector3.zero? triggerRotation: Quaternion.LookRotation(direction, (range.characterPosition.fixYAxis? Vector3.up: script.transform.up));
			Vector3 up = rotation * Vector3.up;
			Vector3 forward = rotation * Vector3.forward;
				
			Handles.DrawWireDisc(position, up, range.characterPosition.radius);
				
			if (range.characterPosition.orbit) {
				float mag = range.characterPosition.offset.magnitude;
					
				if (mag - range.characterPosition.radius > 0f) {
					Handles.DrawWireDisc(script.transform.position, up, mag - range.characterPosition.radius);
				}
					
				Handles.DrawWireDisc(script.transform.position, up, mag + range.characterPosition.radius);
			}
				
			Vector3 x = forward * range.characterPosition.radius;
			Quaternion q = Quaternion.AngleAxis(-range.characterPosition.maxAngle, up);
				
			Vector3 dir = q * x;
				
			if (direction != Vector3.zero && range.characterPosition.maxAngle < 180f) {
				Handles.DrawLine(position, position + x);
				Inspector.DotCap(0, position + x, Quaternion.identity, range.characterPosition.radius * 0.01f);
			}
				
			Handles.Label(position - Vector3.up * index * 0.05f, "Character Position for Range " + index.ToString() + ": " + range.name);
				
			Color color = Handles.color;
			Color transparent = new Color(color.r, color.g, color.b, 0.3f);
			Handles.color = transparent;
			
			Handles.DrawSolidArc(position, up, dir, range.characterPosition.maxAngle * 2f, range.characterPosition.radius);
			
			Handles.color = color;
		}
		
		private void DrawCameraPosition(InteractionTrigger.Range range, int index) {
			if (range.cameraPosition.lookAtTarget == null) return;
			
			Vector3 labelPosition = range.cameraPosition.lookAtTarget.transform.position - Vector3.up * index * 0.05f;
			
			if (range.cameraPosition.direction == Vector3.zero) {
				Handles.Label(labelPosition, "Camera Position direction is Vector3.zero for Range" + index.ToString() + ": " + range.name);
				return;
			}
			if (range.cameraPosition.maxAngle <= 0f) {
				Handles.Label(labelPosition, "Camera Position max angle is zero for Range" + index.ToString() + ": " + range.name);
				return;
			}
			range.cameraPosition.maxDistance = Mathf.Max(range.cameraPosition.maxDistance, 0f);
			if (range.cameraPosition.maxDistance <= 0f) {
				Handles.Label(labelPosition, "Camera Position Max Distance is zero for Range" + index.ToString() + ": " + range.name);
				return;
			}
			
			Quaternion targetRotation = range.cameraPosition.GetRotation();
			Vector3 position = range.cameraPosition.lookAtTarget.transform.position;
			
			Vector3 direction = targetRotation * range.cameraPosition.direction;
			direction = direction.normalized * range.cameraPosition.maxDistance;
			
			Handles.DrawLine(position, position + direction);
			Inspector.DotCap(0, position + direction, Quaternion.identity, 0.005f);
			
			Handles.Label(position + direction * 1.1f, "Camera Position for Range " + index.ToString() + ": " + range.name);
			
			if (range.cameraPosition.maxAngle >= 180f) return;
			
			float r = Mathf.Sin(range.cameraPosition.maxAngle * Mathf.Deg2Rad) * range.cameraPosition.maxDistance;
			float d = Mathf.Cos(range.cameraPosition.maxAngle * Mathf.Deg2Rad) * range.cameraPosition.maxDistance;
			
			Quaternion rotation = targetRotation * Quaternion.LookRotation(range.cameraPosition.direction);
			
			Inspector.CircleCap(0, position + direction.normalized * d, rotation, r);
			
			if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null) {
				//Vector3 c = Vector3.Cross(direction, SceneView.lastActiveSceneView.camera.transform.forward);
				Vector3 c = Vector3.Cross(direction, (range.cameraPosition.lookAtTarget.transform.position - SceneView.lastActiveSceneView.camera.transform.position).normalized);
				c = Vector3.Cross(direction, c);
				Quaternion dirRotation = Quaternion.AngleAxis(range.cameraPosition.maxAngle, c);
				Vector3 dir3 = dirRotation * direction;
				Handles.DrawLine(position, position + dir3);
				
				Vector3 dir4 = Quaternion.Inverse(dirRotation) * direction;
				Handles.DrawLine(position, position + dir4);
				
				Handles.DrawWireArc(position, -c, dir3, range.cameraPosition.maxAngle * 2, range.cameraPosition.maxDistance);
			}
		}
		
		private static Color GetColor(int index) {
			float i = (float)index + 1f;
			return new Color(1f / i, i * 0.1f, (i * i) + 0.1f);
		}
	}
}
