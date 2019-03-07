using UnityEngine;
using UnityEngine.Networking;
using Leap.Unity;
using VENTUS.Interaction.Sketching;

public class PlayerController : NetworkBehaviour
{
    // Movement
    public Vector3 centralPoint = Vector3.zero;
    public float rotatingSpeed = 50;
    public float movingSpeed = 20;
    public float minDistanceToZero = 3;
    public float maxDistanceToZero = 20;

    private const float MAX_VERTICAL_ROTATION = 90;
    private float verticalRotation = 0;

    // Pointing
    public GameObject laserPrefab;
    public GameObject markerPrefab;
    public int markerOffset = 2;

    private GameObject laser;
    private bool isLaserOn = false;
    private GameObject marker;

    // Color
    [SyncVar(hook = "OnPlayerColorChange")]
    private Color playerColor = Color.black;

    // Sketching
    [SyncVar]
    private bool drawing = false;
    private bool sentStop = true;
    private SketchingController sc;
    private GameObject paint;

    private void Start()
    {
        paint = transform.Find("Body/Tool Tracking/Brush/Bristles/Paint").gameObject;
        sc = GetComponent<SketchingController>();

        if (!isLocalPlayer)
        {
            GameObject cam = transform.Find("Head/Visor/Camera").gameObject;
            cam.GetComponent<Camera>().enabled = false;
            cam.GetComponent<ToolTrackingController>().enabled = false;
            cam.GetComponent<LeapServiceProvider>().enabled = false;

            OnPlayerColorChange(playerColor);
            return;
        }

        laser = Instantiate(laserPrefab);
        laser.SetActive(false);

        CmdAssignColor();
    }

    private void Update()
    {
        // All Clients and the Server execute this
        HandleParticles();

        if (!isLocalPlayer)
        {
            return;
        }
        
        // Only the local client executes this.
        HandleMovement();
        HandleDrawing(paint.transform.position);
        HandlePointer(paint.transform.position, paint.transform.up);
    }

    [Command]
    private void CmdAssignColor()
    {
        playerColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }

    [Command]
    private void CmdSwitchDrawing()
    {
        drawing = !drawing;
    }

    [Command]
    private void CmdPlaceMarker(Vector3 point, Vector3 normal)
    {
        marker = Instantiate(markerPrefab);
        marker.transform.position = point + normal * markerOffset;
        marker.transform.LookAt(point, normal);
        marker.transform.Rotate(-90, 0, 0);
        marker.GetComponentInChildren<MeshRenderer>().material.color = playerColor; // marker color

        NetworkServer.Spawn(marker);
    }

    /// <summary>
    /// Hook for playerColor member. Assigns the given Color to all colored parts and actions of the player.
    /// </summary>
    /// <param name="color">The color of the player</param>
    private void OnPlayerColorChange(Color color)
    {
        transform.Find("Head").GetComponent<MeshRenderer>().material.color = color; // player color
        transform.Find("Body/Tool Tracking/Brush/Bristles").GetComponent<MeshRenderer>().material.color = color; // bristles color
        transform.Find("Body/Tool Tracking/Brush/Trail").GetComponent<TrailRenderer>().material.color = color; // trail color
        sc.strokeColor = color; // drawing color
        ParticleSystem.MainModule settings = paint.GetComponent<ParticleSystem>().main;
        settings.startColor = new ParticleSystem.MinMaxGradient(color); // particle color
    }

    private void HandleParticles()
    {
        ParticleSystem ps = paint.GetComponent<ParticleSystem>();

        if (drawing && ps.isStopped)
        {
            ps.Play();
        }

        if (!drawing && ps.isPlaying)
        {
            ps.Stop();
        }

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

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            CmdSwitchDrawing();
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

    private void HandlePointer(Vector3 position, Vector3 direction)
    {
        RaycastHit hit;
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            isLaserOn = !isLaserOn;
            laser.SetActive(isLaserOn);
        }

        if (isLaserOn && Physics.Raycast(position, direction, out hit))
        {
            ShowLaser(hit, position);
            if (Input.GetKeyDown(KeyCode.B))
            {
                CmdPlaceMarker(hit.point, hit.normal);
            }
        }
    }

    private void ShowLaser(RaycastHit hit, Vector3 origin)
    {
        laser.SetActive(true); //Show the laser
        laser.GetComponent<MeshRenderer>().material.color = playerColor; // laser color
        laser.transform.position = Vector3.Lerp(origin, hit.point, .5f); // Move laser to the middle between the controller and the position the raycast hit
        laser.transform.LookAt(hit.point); // Rotate laser facing the hit point
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, hit.distance); // Scale laser so it fits exactly between the controller & the hit point
    }

}
