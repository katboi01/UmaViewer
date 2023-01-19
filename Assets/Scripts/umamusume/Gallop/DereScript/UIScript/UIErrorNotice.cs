using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIErrorNotice : MonoBehaviour {

    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
        
    }

    public void OnClickClose()
    {
        Destroy(base.gameObject);
    }
}
