using UnityEngine;

public class SelfDestroy : MonoBehaviour {

	public float markerTime;
	private float targetTime = 0;
	private Vector3 currentPosition = Vector3.zero;

	// Use this for initialization
	void OnEnable () {
		targetTime = markerTime;
	}
	
	// Update is called once per frame
	void Update () {

		if (transform.position != currentPosition) {
			targetTime = markerTime;
			currentPosition = transform.position;
		}

		if (targetTime != 0)
		{
			targetTime -= Time.deltaTime;
			if (targetTime <= 0.0f)
			{
				Destroy (this.gameObject);

				targetTime = 0;
			}
		}
	}
}
