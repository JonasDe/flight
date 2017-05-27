using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SoundOnTrigger : MonoBehaviour {
    public AudioClip clip;
    void OnTriggerEnter(Collider c) {
        GameObject.FindWithTag("AudioCenter").GetComponent<AudioManager>().PlayBackgroundSound(clip, 0.11f);
    }
}
