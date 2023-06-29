using UnityEngine;
using System.Collections;

namespace RootMotion {

	/// <summary>
	/// Large header attribute for Editor.
	/// </summary>
	public class LargeHeader : PropertyAttribute  {

		public string name;
		public string color = "white";

		public LargeHeader (string name) {
			this.name = name;
			this.color = "white";
		}

		public LargeHeader (string name, string color) {
			this.name = name;
			this.color = color;
		}
	}
}
