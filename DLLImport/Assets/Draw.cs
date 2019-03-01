using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VENTUS.Interaction.Sketching;

public class Draw : MonoBehaviour {

    private SketchingController sc;
    private bool drawing = false;
    private bool sentStop = true;

	// Use this for initialization
	void Start () {
        sc = transform.root.gameObject.GetComponent<SketchingController>();
	}
	
	// Update is called once per frame
	void Update () {
        sc.SetVirtualBrushPosition(transform.position);

        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            drawing = !drawing;
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
}
