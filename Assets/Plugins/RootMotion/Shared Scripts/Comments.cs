using UnityEngine;
using System.Collections;

namespace RootMotion {

	/// <summary>
	/// Adding comments to GameObjects in the Inspector.
	/// </summary>
	public class Comments : MonoBehaviour {
	
		/// <summary>
		/// The comment.
		/// </summary>
		[Multiline]
		public string text;
	}
}
