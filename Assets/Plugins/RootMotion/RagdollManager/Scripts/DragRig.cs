using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RootMotion.Dynamics
{
    public class DragRig : MonoBehaviour
    {

        private Rigidbody rig;
        private void Start()
        {
            rig = transform.GetComponent<Rigidbody>();
        }
        private void OnMouseDrag()
        {
            Vector3 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition+new Vector3(0,0,1.5f));
            rig.velocity = ((cursorPos - transform.position)*8);
            rig.angularVelocity = Vector3.zero;
        }
    }

}