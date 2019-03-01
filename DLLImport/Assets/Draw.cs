using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VENTUS.Interaction.Sketching;

public class Draw : MonoBehaviour {

    private SketchingController sc;
    private ParticleSystem ps;

    private bool drawing = false;
    private bool sentStop = true;
    private bool particleActive = false;

	// Use this for initialization
	void Start () {
        sc = transform.root.gameObject.GetComponent<SketchingController>();
        ps = GetComponent<ParticleSystem>();
    }
	
	// Update is called once per frame
	void Update () {
        sc.SetVirtualBrushPosition(transform.position);

        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            drawing = !drawing;
            SwitchParticles();
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

    private void SwitchParticles()
    {
        if (ps.isPlaying)
        {
            ps.Stop();
        } else
        {
            ps.Play();
        }
    }
}
