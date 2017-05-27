using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddScoreOnTrigger : MonoBehaviour
{

    // Use this for initialization
    private bool alreadyTriggered = false;
	public int score = 1;
    void OnTriggerEnter(Collider c)
    {
        if (!alreadyTriggered)
        {
            TextManager.score += score;
            alreadyTriggered = true;
        }
    }
}