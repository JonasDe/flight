using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Profile : MonoBehaviour {
	public FlightHandlerPhys fh;


	public void Start() {
		SelectSlow ();
	}

	public void SelectFast(){

		fh.airDrag = 0.15f;
		fh.verticalAcceleration = 2;
		fh.maxVerticalSpeed = 8;
		fh.maxHorizontalSpeed = 25;
		fh.horizontalAcceleration = 3.5f;
		fh.brakingSpeed = 0.1f;
		fh.gravityReduction = 80;
		fh.increaseFallSpeedWithRotation = true;
		fh.diveIntensity = 50.0f;
		fh.tiltIntensity = 5;
		fh.canGlideBackwards = true;
		fh.rotationMode = FlightHandlerPhys.RotationMode.Linear;
		fh.glideControllerMinDistance = 0.8f;



	}
	public void SelectSlow(){


	

		fh.airDrag = 1;
		fh.verticalAcceleration = 1.5f;
		fh.maxVerticalSpeed = 8;
		fh.maxHorizontalSpeed = 16;
		fh.horizontalAcceleration = 3.5f;
		fh.brakingSpeed = 0.3f;
		fh.gravityReduction = 90;
		fh.increaseFallSpeedWithRotation = true;
		fh.diveIntensity = 50.0f;
		fh.tiltIntensity = 5.0f;
		fh.canGlideBackwards = false;
		fh.rotationMode = FlightHandlerPhys.RotationMode.Linear;
		fh.glideControllerMinDistance = 0.8f;
	}
}
