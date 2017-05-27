using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMultipleSounds : MonoBehaviour {

    public AudioClip clip;
    public AudioClip clip2;
    private AudioSource source;
	void OnTriggerEnter(Collider c) {
	}
	// Use this for initialization
    void Awake () {
        source = GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>();

        float vol = 0.3f;

        source.PlayOneShot(clip,vol);

        source.PlayOneShot(clip2,vol);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
