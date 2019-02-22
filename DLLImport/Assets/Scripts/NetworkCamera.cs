using UnityEngine;
using UnityEngine.Networking;

public class NetworkCamera : NetworkBehaviour {

	void Start ()
    {
        if (!isLocalPlayer)
        {
            transform.Find("Head/Visor/Camera").gameObject.GetComponent<Camera>().enabled = false;
            return;
        }
    }
}
