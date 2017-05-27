using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMover : MonoBehaviour {
    public Transform[] waypoints;
    public int firstWaypoint = 0;
    public float speed;
    private float minDistance = 5.0f;
    // Use this for initialization
    private int currentWaypoint;
	void Start () {
        currentWaypoint = 0;
	}

    // Update is called once per frame
    void FixedUpdate()
    {
        if (waypoints.Length > 0)
        {
            float step = speed * Time.deltaTime;
            Vector3 target = waypoints[currentWaypoint].position;
            if (Vector3.Distance(target, transform.position) < minDistance)
            {
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            }
            transform.position = Vector3.MoveTowards(transform.position, target, step);
        }
    }
}
