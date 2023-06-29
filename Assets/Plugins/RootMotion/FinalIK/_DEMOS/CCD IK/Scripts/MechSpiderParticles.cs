using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {
	
	/// <summary>
	/// Emitting smoke for the mech spider
	/// </summary>
	public class MechSpiderParticles: MonoBehaviour {
		
		public MechSpiderController mechSpiderController;
		
		private ParticleSystem particles;
		
		void Start() {
			particles = (ParticleSystem)GetComponent(typeof(ParticleSystem));
		}
		
		void Update() {
			// Smoke
			float inputMag = mechSpiderController.inputVector.magnitude;
			
			float emissionRate = Mathf.Clamp(inputMag * 50, 30, 50);
			
			#if (UNITY_5_3 || UNITY_5_4)
			var emission = particles.emission;
			emission.rate = new ParticleSystem.MinMaxCurve(emissionRate);
			particles.startColor = new Color (particles.startColor.r, particles.startColor.g, particles.startColor.b, Mathf.Clamp(inputMag, 0.4f, 1f));
			#else
			var emission = particles.emission;
			emission.rateOverTime = new ParticleSystem.MinMaxCurve(emissionRate);

			var main = particles.main;
			main.startColor = new Color (particles.main.startColor.color.r, particles.main.startColor.color.g, particles.main.startColor.color.b, Mathf.Clamp(inputMag, 0.4f, 1f));
			#endif
		}
	}
}
