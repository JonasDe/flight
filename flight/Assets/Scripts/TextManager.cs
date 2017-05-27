using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{

    public FlightHandlerPhys flightHandler;
    public static int score;
    public Text scoreText;
    public Text time;
    private bool reachedMax = false;
    void Start()
    {
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "Score: " + score;

        if (!reachedMax && score < 50)
        {
            time.text = "Total Time: " + Time.timeSinceLevelLoad.ToString("0.00");
        }
        else
        {
            reachedMax = true;
            time.color = Color.green;
        }

    }

}

