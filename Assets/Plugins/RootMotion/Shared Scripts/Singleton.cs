using UnityEngine;
using System.Collections;

namespace RootMotion {

	/// <summary>
	/// The base abstract Singleton class.
	/// </summary>
	public abstract class Singleton<T>: MonoBehaviour where T: Singleton<T> {
		
		private static T sInstance = null;
		
		public static T instance {
			get {
				return sInstance;
			}
		}
		
		protected virtual void Awake() {
			if (sInstance != null) Debug.LogError(name + "error: already initialized", this);
			
			sInstance = (T)this;
		}
	}
}
