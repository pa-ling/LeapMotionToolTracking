using UnityEngine;
using UnityEngine.Networking;
using Leap.Unity;
using VENTUS.Interaction.Sketching;

public class PlayerController : NetworkBehaviour
{
    public Vector3 centralPoint = Vector3.zero;
    public float rotatingSpeed = 50;
    public float movingSpeed = 20;
    public float minDistanceToZero = 3;
    public float maxDistanceToZero = 20;

    private const float MAX_VERTICAL_ROTATION = 90;
    private float verticalRotation = 0;

    private SketchingController sc;
    private GameObject paint;

    private bool drawing = false;
    private bool sentStop = true;

    void Start()
    {
        if (!isLocalPlayer)
        {
            GameObject cam = transform.Find("Head/Visor/Camera").gameObject;
            cam.GetComponent<Camera>().enabled = false;
            cam.GetComponent<LeapToolTracking>().enabled = false;
            cam.GetComponent<LeapServiceProvider>().enabled = false;

            GameObject bristles = transform.Find("Body/Tool Tracking/Brush/Bristles").gameObject;
            bristles.GetComponent<Pointer>().enabled = false;
            //bristles.GetComponent<Draw>().enabled = false;
            return;
        }

        sc = GetComponent<SketchingController>();
        paint = transform.Find("Body/Tool Tracking/Brush/Bristles/Paint").gameObject;
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        HandleDrawing(paint.transform.position);
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

    private void HandleDrawing(Vector3 position)
    {
        sc.SetVirtualBrushPosition(position);

        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            drawing = !drawing;
            SwitchParticles(paint.GetComponent<ParticleSystem>());
        }

        if (drawing)
        {
            sc.Draw();
            sentStop = false;
        }
        else if (!sentStop)
        {
            sc.StopDraw();
            sentStop = true;
        }
    }

    private void SwitchParticles(ParticleSystem ps)
    {
        if (ps.isPlaying)
        {
            ps.Stop();
        }
        else
        {
            ps.Play();
        }
    }
}
