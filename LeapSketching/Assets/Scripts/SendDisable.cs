using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendDisable : MonoBehaviour {

    private void OnDisable()
    {
        transform.root.gameObject.GetComponent<PlayerController>().BrushDisabled();
    }

}
