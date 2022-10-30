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
            Vector3 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rig.AddForce(0.01f * Time.deltaTime * (cursorPos-transform.position).normalized,ForceMode.Force);
        }
    }

}