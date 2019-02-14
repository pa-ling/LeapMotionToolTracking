using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Vector3 centralPoint = Vector3.zero;
    public float rotatingSpeed = 50;
    public float movingSpeed = 0.5f;
    public float minDistanceToZero = 0.1f;
    public float maxDistanceToZero = 100;

    private const float MAX_VERTICAL_ROTATION = 90;
    private float verticalRotation = 10;

    void Start()
    {
        transform.LookAt(centralPoint);
        //transform.RotateAround(centralPoint, transform.right, 10);
    }

    void Update()
    {
        float rotatingRange = rotatingSpeed * Time.deltaTime;
        float movingRange = movingSpeed * Time.deltaTime;

        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && Vector3.Distance(transform.position, centralPoint) > minDistanceToZero)
        {
            transform.position += transform.forward * movingRange;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.RotateAround(centralPoint, Vector3.up, rotatingRange);
        }

        if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && Vector3.Distance(transform.position, centralPoint) < maxDistanceToZero)
        {
            transform.position -= transform.forward * movingRange;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(centralPoint, -Vector3.up, rotatingRange);
        }

        if (Input.GetKey(KeyCode.Space) && MAX_VERTICAL_ROTATION > verticalRotation + rotatingRange)
        {
            transform.RotateAround(centralPoint, transform.right, rotatingRange);
            verticalRotation += rotatingRange;
        }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && -MAX_VERTICAL_ROTATION < verticalRotation - rotatingRange)
        {
            transform.RotateAround(centralPoint, -transform.right, rotatingRange);
            verticalRotation -= rotatingRange;
        }
    }
}
