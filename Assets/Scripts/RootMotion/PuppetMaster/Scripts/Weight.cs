using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	/// <summary>
	/// A floating point value that can be used as a simple float or a weight curve evaluated by another floating point parameter.
	/// </summary>
	[System.Serializable]
	public class Weight {

		/// <summary>
		/// Simple float value or a curve evaluated by another floating point parameter.
		/// </summary>
		[System.Serializable]
		public enum Mode {
			Float,
			Curve
		}

		/// <summary>
		/// Simple float value or a curve evaluated by another floating point parameter.
		/// </summary>
		public Mode mode;

		/// <summary>
		/// The float value.
		/// </summary>
		public float floatValue;

		/// <summary>
		/// The AnimationCurve.
		/// </summary>
		public AnimationCurve curve;

		// A workaround for adding tooltips to custom property drawers.
		public string tooltip = "";

		/// <summary>
		/// Initializes a new instance of the <see cref="RootMotion.Dynamics.Weight"/> class.
		/// </summary>
		public Weight(float floatValue) {
			this.floatValue = floatValue;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RootMotion.Dynamics.Weight"/> class.
		/// </summary>
		/// <param name="floatValue">Float value.</param>
		/// <param name="tooltip">Editor tooltip for this Weight.</param>
		public Weight(float floatValue, string tooltip) {
			this.floatValue = floatValue;
			this.tooltip = tooltip;
		}

		/// <summary>
		/// Gets the value. If in 'Float' mode, will return floatValue, if 'Curve' mode, will return the curve's value evaluated at 'param'.
		/// </summary>
		public float GetValue(float param) {
			switch(mode) {
			case Mode.Curve: return curve.Evaluate(param);
			default: return floatValue;
			}
		}
	}
}
