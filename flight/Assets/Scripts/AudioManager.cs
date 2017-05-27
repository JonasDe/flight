using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public FlightHandlerPhys fh;
    private AudioSource [] audioSources;
    private float lastWindCall;
	private bool coroutineStarted = false;
   

	public void Start () {
        audioSources = GetComponents<AudioSource>();

	}
    public void PlayWingflap()
    {
        audioSources[0].Play();
    }

    IEnumerator FadeIn()
    {
		AudioSource audio = audioSources [1];
        float t = 0.0f;
        float maxVol = audio.volume;
        float fadeExtender = 10.0f;
        while (t < maxVol)
        {
			
            t += Time.deltaTime/fadeExtender;
            audio.volume = t;
            yield return null;
        }
    }

	IEnumerator PitchAdjuster()
	{

		Debug.Log("Coroutine");
		while (true)
		{
			float speed = Vector3.Magnitude(new Vector3 (fh.GetCurrentSpeed ().x, 0, fh.GetCurrentSpeed ().z));
			float maxSpeed = fh.maxHorizontalSpeed;
			audioSources [1].pitch = 1 + Mathf.Min (1.0f, (speed * 2.0f / maxSpeed));

			yield return null;
		}

	}
    public void PlayWind()
    {
		if (!coroutineStarted) { 
			StartCoroutine("PitchAdjuster");
			coroutineStarted = true;
		}
        //audioSources[1].volume = vol;
        //audioSources[1].pitch = (vol*3) > 1 ?  vol*3 : 1;
        if (!audioSources[1].isPlaying)
        {
			
			StartCoroutine("FadeIn");
         
            audioSources[1].Play();
            audioSources[1].loop = true;
        }
    }
    public void StopWind()
    {

            audioSources[1].loop = false;
    }
    public void PlayThrust()
    {
        if (!audioSources[2].isPlaying)
        {
            StopPlayingThrust();
            audioSources[2].Play();
        }
    }
    public void StopPlayingThrust()
    {
        
        audioSources[2].Stop();

    }
    public void PlayBackgroundSound(AudioClip c, float vol)
    {
        audioSources[0].PlayOneShot(c, vol);
    }

	
}
