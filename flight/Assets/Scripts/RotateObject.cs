using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour {
    public float rotationSpeed = 1;
	// Use this for initialization
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.Rotate(new Vector3(0, 0, rotationSpeed*Time.fixedDeltaTime));
	}
}
