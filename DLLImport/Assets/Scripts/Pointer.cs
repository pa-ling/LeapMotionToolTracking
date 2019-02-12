
using UnityEngine;

public class Pointer : MonoBehaviour
{
	public GameObject laserPrefab; // The laser prefab
	public GameObject markerPrefab; // Stores a reference to the marker prefab.
	public int markerOffset;

	private GameObject laser; // A reference to the spawned laser
	private bool isLaserOn = false;
	private GameObject marker; // A reference to an instance of the marker

	void Start()
	{
		laser = Instantiate(laserPrefab);
		laser.SetActive (false);
	}

	void Update()
	{
		RaycastHit hit;
		if (Input.GetKeyDown(KeyCode.PageUp))
		{
			isLaserOn = true;
		}

		if (Physics.Raycast(transform.position, transform.up, out hit) && isLaserOn)
		{
			ShowLaser(hit, transform.position);
			if (Input.GetKeyUp(KeyCode.PageUp))
			{
				PlaceMarker (hit);
				isLaserOn = false;
				laser.SetActive (false);
			}
		}
	}

	void OnDestroy() 
	{
		Destroy (laser);
	}

	private void ShowLaser(RaycastHit hit, Vector3 origin)
	{
		laser.SetActive(true); //Show the laser
		laser.transform.position = Vector3.Lerp(origin, hit.point, .5f); // Move laser to the middle between the controller and the position the raycast hit
		laser.transform.LookAt(hit.point); // Rotate laser facing the hit point
		laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, hit.distance); // Scale laser so it fits exactly between the controller & the hit point
	}

	private void PlaceMarker(RaycastHit hit)
	{
		marker = Instantiate(markerPrefab);
		marker.transform.position = hit.point + hit.normal * markerOffset;
		marker.transform.LookAt(hit.point, hit.normal);
		marker.transform.Rotate (-90, 0, 0);
	}

}
