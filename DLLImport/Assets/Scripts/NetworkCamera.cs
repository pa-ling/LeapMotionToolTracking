using UnityEngine;
using UnityEngine.Networking;
using Leap.Unity;

public class NetworkCamera : NetworkBehaviour {

	void Start ()
    {
        if (!isLocalPlayer)
        {
            GameObject cam = transform.Find("Head/Visor/Camera").gameObject;
            cam.GetComponent<Camera>().enabled = false;
            cam.GetComponent<LeapToolTracking>().enabled = false;
            cam.GetComponent<LeapServiceProvider>().enabled = false;
            return;
        }
    }
}
