using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveringObject : MonoBehaviour
{

    // Use this for initialization
    public float floatForce = 0.01f;
    private Rigidbody rb;
    private float irregularity;
    void Start()
    {
        irregularity = Random.Range(0.0f, 5.0f);
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddForce(new Vector3(0, Mathf.Sin(Time.time+irregularity) * floatForce, 0), ForceMode.Acceleration);
    }
}
