using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTrigger : MonoBehaviour {

	void OnTriggerEnter(Collider c) {
		Destroy(this.gameObject);
	}
}
