using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RootMotion.Dynamics {

	/// <summary>
	/// The global master settings and optimizations for all PuppetMasters in the scene. Will only work if an instance of this singleton is added to the scene. If not, the PuppetMasters will use default values.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/PuppetMaster Settings")]
	public class PuppetMasterSettings : Singleton <PuppetMasterSettings> {

		[System.Serializable]
		public class PuppetUpdateLimit {
			[Range(1, 100)] public int puppetsPerFrame;

			private int index;

			public PuppetUpdateLimit() {
				this.puppetsPerFrame = 100;
			}

			public void Step(int puppetCount) {
				index += puppetsPerFrame;
				if (index >= puppetCount) index -= puppetCount;
			}

			public bool Update(List<PuppetMaster> puppets, PuppetMaster puppetMaster) {
				if (puppetsPerFrame >= puppets.Count) return true;
				
				if (index >= puppets.Count) return false;
				
				for (int i = 0; i < puppetsPerFrame; i++) {
					int o = index + i;
					if (o >= puppets.Count) o -= puppets.Count;
					
					if (puppets[o] == puppetMaster) {
						return true;
					}
				}
				
				return false;
			}
		}

		[Header("Optimizations")]

		public PuppetUpdateLimit kinematicCollidersUpdateLimit = new PuppetUpdateLimit();
		public PuppetUpdateLimit freeUpdateLimit = new PuppetUpdateLimit();
		public PuppetUpdateLimit fixedUpdateLimit = new PuppetUpdateLimit();
		public bool collisionStayMessages = true;
		public bool collisionExitMessages = true;
		public float activePuppetCollisionThresholdMlp = 0f;

		public int currentlyActivePuppets { get; private set; }
		public int currentlyKinematicPuppets { get; private set; }
		public int currentlyDisabledPuppets { get; private set; }

		public List<PuppetMaster> puppets {
			get {
				return _puppets;
			}
		}

		public void Register(PuppetMaster puppetMaster) {
			if (_puppets.Contains(puppetMaster)) return;
			_puppets.Add(puppetMaster);
		}
		
		public void Unregister(PuppetMaster puppetMaster) {
			_puppets.Remove(puppetMaster);
		}

		public bool UpdateMoveToTarget(PuppetMaster puppetMaster) {
			return kinematicCollidersUpdateLimit.Update(_puppets, puppetMaster);
		}

		public bool UpdateFree(PuppetMaster puppetMaster) {
			return freeUpdateLimit.Update(_puppets, puppetMaster);
		}

		public bool UpdateFixed(PuppetMaster puppetMaster) {
			return fixedUpdateLimit.Update(_puppets, puppetMaster);
		}

		private List<PuppetMaster> _puppets = new List<PuppetMaster>();
		//private int optIndexMoveToTarget;
		//private int optIndexFixed;

		void Update() {
			currentlyActivePuppets = 0;
			currentlyKinematicPuppets = 0;
			currentlyDisabledPuppets = 0;

			foreach (PuppetMaster puppet in _puppets) {
				if (puppet.isActive && puppet.isActiveAndEnabled) currentlyActivePuppets ++;
				if (puppet.mode == PuppetMaster.Mode.Kinematic) currentlyKinematicPuppets ++;
				if ((puppet.mode == PuppetMaster.Mode.Disabled && !puppet.isActive) || !puppet.isActiveAndEnabled) currentlyDisabledPuppets ++;
			}

			freeUpdateLimit.Step(_puppets.Count);
			kinematicCollidersUpdateLimit.Step(_puppets.Count);
		}

		void FixedUpdate() {
			fixedUpdateLimit.Step(_puppets.Count);

			/*
			optIndexMoveToTarget += updateKinematicCollidersLimit;
			if (optIndexMoveToTarget >= puppets.Count) optIndexMoveToTarget -= _puppets.Count;

			optIndexFixed += updatePuppetsFixedFrameLimit;
			if (optIndexFixed >= puppets.Count) optIndexFixed -= _puppets.Count;
			*/
		}

		/*
		void OnGUI() {
			GUILayout.Label("Active puppets: " + currentlyActivePuppets.ToString());
		}
		*/

	}
}
