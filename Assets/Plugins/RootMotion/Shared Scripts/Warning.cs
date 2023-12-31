using UnityEngine;
using System.Collections;

namespace RootMotion {

	/// <summary>
	/// Manages warning messages.
	/// </summary>
	public static class Warning {
		
		public static bool logged;
		
		public delegate void Logger(string message);
		
		public static void Log(string message, Logger logger, bool logInEditMode = false) {
			if (!logInEditMode && !Application.isPlaying) return;
			if (logged) return;
			if (logger != null) logger(message);
			logged = true;
		}
		
		public static void Log(string message, Transform context, bool logInEditMode = false) {
			if (!logInEditMode && !Application.isPlaying) return;
			if (logged) return;
			Debug.LogWarning(message, context);
			logged = true;
		}
	}
}
