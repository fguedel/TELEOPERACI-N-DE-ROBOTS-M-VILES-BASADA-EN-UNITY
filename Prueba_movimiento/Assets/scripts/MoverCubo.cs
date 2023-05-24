using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverCubo : MonoBehaviour
{
    public float radius = 2f; // radius of the circular path
    public float speed = 1f; // speed of the object around the circle

    private Vector3 center; // center of the circle
    private float angle; // current angle of the object around the circle

    private void Start()
    {
        center = transform.position; // initialize center to current position of the object
    }

    private void Update()
    {
        angle += speed * Time.deltaTime; // update angle based on speed

        // calculate new position based on angle and radius
        float x = center.x + radius * Mathf.Cos(angle);
        float z = center.z + radius * Mathf.Sin(angle);
        transform.position = new Vector3(x, transform.position.y, z);

        // rotate the object to face towards the center of the circle
        transform.LookAt(center);
    }
}
