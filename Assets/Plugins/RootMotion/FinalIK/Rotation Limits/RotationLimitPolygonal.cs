using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Using a spherical polygon to limit the range of rotation on universal and ball-and-socket joints. A reach cone is specified as a spherical polygon 
	/// on the surface of a a reach sphere that defines all positions the longitudinal segment axis beyond the joint can take.
	/// 
	/// This class is based on the "Fast and Easy Reach-Cone Joint Limits" paper by Jane Wilhelms and Allen Van Gelder. 
	/// Computer Science Dept., University of California, Santa Cruz, CA 95064. August 2, 2001
	/// http://users.soe.ucsc.edu/~avg/Papers/jtl.pdf
	/// 
	/// </summary>
	[HelpURL("http://www.root-motion.com/finalikdox/html/page14.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Rotation Limits/Rotation Limit Polygonal")]
	public class RotationLimitPolygonal : RotationLimit {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		private void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page14.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_rotation_limit_polygonal.html");
		}
		
		// Link to the Final IK Google Group
		[ContextMenu("Support Group")]
		void SupportGroup() {
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}
		
		// Link to the Final IK Asset Store thread in the Unity Community
		[ContextMenu("Asset Store Thread")]
		void ASThread() {
			Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		#region Main Interface
		
		/// <summary>
		/// Limit of twist rotation around the main axis.
		/// </summary>
		[Range(0f, 180f)] public float twistLimit = 180;
		/// <summary>
		/// The number of smoothing iterations applied to the polygon.
		/// </summary>
		[Range(0, 3)] public int smoothIterations = 0;
		
		/// <summary>
		/// Sets the limit points and recalculates the reach cones.
		/// </summary>
		/// <param name='_points'>
		/// _points.
		/// </param>
		public void SetLimitPoints(LimitPoint[] points) {
			if (points.Length < 3) {
				LogWarning("The polygon must have at least 3 Limit Points.");
				return;
			}
			this.points = points;
			BuildReachCones();
		}
		
		#endregion Main Interface
		
		/*
		 * Limits the rotation in the local space of this instance's Transform.
		 * */
		protected override Quaternion LimitRotation(Quaternion rotation) {
			if (reachCones.Length == 0) Start();

			// Subtracting off-limits swing
			Quaternion swing = LimitSwing(rotation);

			// Apply twist limits
			return LimitTwist(swing, axis, secondaryAxis, twistLimit);
		}
		
		/*
		 * Tetrahedron composed of 2 Limit points, the origin and an axis point.
		 * */
		[System.Serializable]
		public class ReachCone {
			public Vector3[] tetrahedron;
			public float volume;
			public Vector3 S, B;
			
			public Vector3 o { get { return tetrahedron[0]; }}
			public Vector3 a { get { return tetrahedron[1]; }}
			public Vector3 b { get { return tetrahedron[2]; }}
			public Vector3 c { get { return tetrahedron[3]; }}
			
			public ReachCone(Vector3 _o, Vector3 _a, Vector3 _b, Vector3 _c) {
				this.tetrahedron = new Vector3[4];
				this.tetrahedron[0] = _o; // Origin
				this.tetrahedron[1] = _a; // Axis
				this.tetrahedron[2] = _b; // Limit Point 1
				this.tetrahedron[3] = _c; // Limit Point 2
				
				this.volume = 0;
				this.S = Vector3.zero;
				this.B = Vector3.zero;
			}
			
			public bool isValid { get { return volume > 0; }}
			
			public void Calculate() {
				Vector3 crossAB = Vector3.Cross(a, b);
				volume = Vector3.Dot(crossAB, c) / 6.0f;
				
				S = Vector3.Cross(a, b).normalized;
				B = Vector3.Cross(b, c).normalized;
			}
		}
		
		/*
		 * The points defining the polygon
		 * */
		[System.Serializable]
		public class LimitPoint {
			public Vector3 point;
			public float tangentWeight;
			
			public LimitPoint() {
				this.point = Vector3.forward;
				this.tangentWeight = 1;
			}
		}
		
		[SerializeField][HideInInspector] public LimitPoint[] points;
		[SerializeField][HideInInspector] public Vector3[] P;
		[SerializeField][HideInInspector] public ReachCone[] reachCones = new ReachCone[0];
		
		void Start() {
			if (points.Length < 3) ResetToDefault();
			
			// Check if Limit Points are valid
			for (int i = 0; i < reachCones.Length; i++) {
				if (!reachCones[i].isValid) {
					if (smoothIterations <= 0) {
						int nextPoint = 0;
						if (i < reachCones.Length - 1) nextPoint = i + 1;
						else nextPoint = 0;
						LogWarning("Reach Cone {point " + i + ", point " + nextPoint + ", Origin} has negative volume. Make sure Axis vector is in the reachable area and the polygon is convex.");
					} else LogWarning("One of the Reach Cones in the polygon has negative volume. Make sure Axis vector is in the reachable area and the polygon is convex.");
				}
			}
			
			axis = axis.normalized;
		}
		
		#region Precalculations
		
		/*
		 * Apply the default initial setup of 4 Limit Points
		 * */
		public void ResetToDefault() {
			points = new LimitPoint[4];
			for (int i = 0; i < points.Length; i++) points[i] = new LimitPoint();
			
			Quaternion swing1Rotation = Quaternion.AngleAxis(45, Vector3.right);
			Quaternion swing2Rotation = Quaternion.AngleAxis(45, Vector3.up);
			
			points[0].point = (swing1Rotation * swing2Rotation) * axis;
			points[1].point = (Quaternion.Inverse(swing1Rotation) * swing2Rotation) * axis;
			points[2].point = (Quaternion.Inverse(swing1Rotation) * Quaternion.Inverse(swing2Rotation)) * axis;
			points[3].point = (swing1Rotation * Quaternion.Inverse(swing2Rotation)) * axis;
			
			BuildReachCones();
		}
		
		/*
		 * Recalculate reach cones if the Limit Points have changed
		 * */
		public void BuildReachCones() {
			smoothIterations = Mathf.Clamp(smoothIterations, 0, 3);
			
			// Make another array for the points so that they could be smoothed without changing the initial points
			P = new Vector3[points.Length];
			for (int i = 0; i < points.Length; i++) P[i] = points[i].point.normalized;
			
			for (int i = 0; i < smoothIterations; i++) P = SmoothPoints();
			
			// Calculating the reach cones
			reachCones = new ReachCone[P.Length]; 
			for (int i = 0; i < reachCones.Length - 1; i++) {
				reachCones[i] = new ReachCone(Vector3.zero, axis.normalized, P[i], P[i + 1]);
			}
			
			reachCones[P.Length - 1] = new ReachCone(Vector3.zero, axis.normalized, P[P.Length - 1], P[0]);
			
			for (int i = 0; i < reachCones.Length; i++) reachCones[i].Calculate();
		}
		
		/*
		 * Automatically adds virtual limit points to smooth the polygon
		 * */
		private Vector3[] SmoothPoints() {
			// Create the new point array with double length
			Vector3[] Q = new Vector3[P.Length * 2];
			
			float scalar = GetScalar(P.Length); // Get the constant used for interpolation
			
			// Project all the existing points on a plane that is tangent to the unit sphere at the Axis point
			for (int i = 0; i < Q.Length; i+= 2) Q[i] = PointToTangentPlane(P[i / 2], 1);
			
			// Interpolate the new points
			for (int i = 1; i < Q.Length; i+= 2) {
				Vector3 minus2 = Vector3.zero;
				Vector3 plus1 = Vector3.zero;
				Vector3 plus2 = Vector3.zero;
				
				if (i > 1 && i < Q.Length - 2) {
					minus2 = Q[i - 2];
					plus2 = Q[i + 1];
				} else if (i == 1) {
					minus2 = Q[Q.Length - 2];
					plus2 = Q[i + 1];
				} else if (i == Q.Length - 1) {
					minus2 = Q[i - 2];
					plus2 = Q[0];
				}
				
				if (i < Q.Length - 1) plus1 = Q[i + 1];
				else plus1 = Q[0];
				
				int t = Q.Length / points.Length;
				
				// Interpolation
				Q[i] = (0.5f * (Q[i - 1] + plus1)) + (scalar * points[i / t].tangentWeight * (plus1 - minus2)) + (scalar * points[i / t].tangentWeight * (Q[i - 1] - plus2));
			}

			// Project the points from tangent plane to the sphere
			for (int i = 0; i < Q.Length; i++) Q[i] = TangentPointToSphere(Q[i], 1);
			
			return Q;
		}
		
		/*
		 * Returns scalar values used for interpolating smooth positions between limit points
		 * */
		private float GetScalar(int k) {
			// Values k (number of points) == 3, 4 and 6 are calculated by analytical geometry, values 5 and 7 were estimated by interpolation
			if (k <= 3) return .1667f;
			if (k == 4) return .1036f;
			if (k == 5) return .0850f;
			if (k == 6) return .0773f;
			if (k == 7) return .0700f;
			return .0625f; // Cubic spline fit
		}
		
		/*
		 * Project a point on the sphere to a plane that is tangent to the unit sphere at the Axis point
		 * */
		private Vector3 PointToTangentPlane(Vector3 p, float r) {
			float d = Vector3.Dot(axis, p);
			float u = (2 * r * r) / ((r * r) + d);
			return (u * p) + ((1 - u) * -axis);
		}
		
		/*
		 * Project a point on the tangent plane to the sphere
		 * */
		private Vector3 TangentPointToSphere(Vector3 q, float r) {
			float d = Vector3.Dot(q - axis, q - axis);
			float u = (4 * r * r) / ((4 * r * r) + d);
			return (u * q) + ((1 - u) * -axis);
		}
		
		#endregion Precalculations
		
		#region Runtime calculations
		
		/*
		 * Applies Swing limit to the rotation
		 * */
		private Quaternion LimitSwing(Quaternion rotation) {		
			if (rotation == Quaternion.identity) return rotation; // Assuming initial rotation is in the reachable area
			
			Vector3 L = rotation * axis; // Test this vector against the reach cones
			
			int r = GetReachCone(L); // Get the reach cone to test against (can be only 1)
			
			// Just in case we are running our application with invalid reach cones
			if (r == -1) {
				if (!Warning.logged) LogWarning("RotationLimitPolygonal reach cones are invalid.");
				return rotation; 
			}
			
			// Dot product of cone normal and rotated axis
			float v = Vector3.Dot(reachCones[r].B, L);
			if (v > 0) return rotation; // Rotation is reachable
			
			// Find normal for a plane defined by origin, axis, and rotated axis
			Vector3 rotationNormal = Vector3.Cross(axis, L);
			
			// Find the line where this plane intersects with the reach cone plane
			L = Vector3.Cross(-reachCones[r].B, rotationNormal);
			
			// Rotation from current(illegal) swing rotation to the limited(legal) swing rotation
			Quaternion toLimits = Quaternion.FromToRotation(rotation * axis, L);
			
			// Subtract the illegal rotation
			return toLimits * rotation;
		}
		
		/*
		 * Finding the reach cone to test against
		 * */
		private int GetReachCone(Vector3 L) {
			float p = 0;
			float p1 = Vector3.Dot(reachCones[0].S, L);
			
			for (int i = 0; i < reachCones.Length; i++) {
				p = p1;
				
				if (i < reachCones.Length - 1) p1 = Vector3.Dot(reachCones[i + 1].S, L);
				else p1 = Vector3.Dot(reachCones[0].S, L);
				
				if (p >= 0 && p1 < 0) return i;
			}
			
			return -1;
		}
		
		#endregion Runtime calculations
	}
}
