using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

public class Skeleton : MonoBehaviour {

	public Animator animator;
	public PuppetMaster puppetMaster;

	public ConfigurableJoint[] leftLeg;
	public ConfigurableJoint[] rightLeg;

	bool leftLegRemoved, rightLegRemoved;

	void Start() {
		// Register to get a call from PM when a muscle is removed or disconnected
		puppetMaster.OnMuscleRemoved += OnMuscleDisconnected;
        puppetMaster.OnMuscleDisconnected += OnMuscleDisconnected;
	}

	public void OnRebuild() {
		//puppetMaster.state = PuppetMaster.State.Alive;
		animator.SetFloat("Legs", 2);
        animator.Play("Move", 0, 0f);
        leftLegRemoved = false;
        rightLegRemoved = false;
	}

	// Called by PM when a muscle is removed (once for each removed muscle)
	void OnMuscleDisconnected(Muscle m) {
		bool isLeft = false;

		// If one of the legs is missing, play the "jump on one leg" animation. If both, set PM state to Dead.
		if (IsLegMuscle(m, out isLeft)) {
			if (isLeft) leftLegRemoved = true;
			else rightLegRemoved = true;

            if (leftLegRemoved && rightLegRemoved)
            {
               puppetMaster.state = PuppetMaster.State.Dead;
            }
            else
            {
                animator.SetFloat("Legs", 1);
            }
		}
	}
    
	// Is the muscle a leg and if so, is it left or right?
	private bool IsLegMuscle(Muscle m, out bool isLeft) {
		isLeft = false;

		foreach (ConfigurableJoint j in leftLeg) {
			if (j == m.joint) {
				isLeft = true;
				return true;
			}
		}

		foreach (ConfigurableJoint j in rightLeg) {
			if (j == m.joint) {
				isLeft = false;
				return true;
			}
		}

		return false;
	}
}
