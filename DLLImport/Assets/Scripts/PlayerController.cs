using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public Vector3 centralPoint = Vector3.zero;
    public float rotatingSpeed = 50;
    public float movingSpeed = 20;
    public float minDistanceToZero = 3;
    public float maxDistanceToZero = 20;

    private const float MAX_VERTICAL_ROTATION = 90;
    private float verticalRotation = 10;

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        float rotatingRange = rotatingSpeed * Time.deltaTime;
        float movingRange = movingSpeed * Time.deltaTime;

        // Move forward
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && Vector3.Distance(transform.position, centralPoint) > minDistanceToZero)
        {
            transform.position += transform.forward * movingRange;
        }

        // Rotate left
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.RotateAround(centralPoint, Vector3.up, rotatingRange);
        }
        
        // Move backward
        if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && Vector3.Distance(transform.position, centralPoint) < maxDistanceToZero)
        {
            transform.position -= transform.forward * movingRange;
        }

        // Rotate Right
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(centralPoint, -Vector3.up, rotatingRange);
        }

        //Rotate Top
        if (Input.GetKey(KeyCode.Space) && MAX_VERTICAL_ROTATION > verticalRotation + rotatingRange)
        {
            transform.RotateAround(centralPoint, transform.right, rotatingRange);
            verticalRotation += rotatingRange;
        }

        //Rotate Down
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && -MAX_VERTICAL_ROTATION < verticalRotation - rotatingRange)
        {
            transform.RotateAround(centralPoint, -transform.right, rotatingRange);
            verticalRotation -= rotatingRange;
        }
    }

}
