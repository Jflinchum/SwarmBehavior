using UnityEngine;
using System.Collections;

public class SwarmCamera : MonoBehaviour {

	public GameObject target;
	public GameObject spawner;

	public float xSensitivity;
	public float ySensitivity;
	public float distance;

	public float minDist;
	public float maxDist;

	private float xRotation;
	private float yRotation;

	public GameObject[] targets;
	private int index = 0;

	// Use this for initialization
	void Start () {
		if (spawner != null) {
			targets = spawner.GetComponent<Spawner>().objects;
		}
		target = targets[0];
		if(target != null)
			transform.SetParent (target.transform);

		xRotation = transform.eulerAngles.x;
		yRotation = transform.eulerAngles.y;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			index += 1;
			if(index >= targets.Length)
				index = 0;
			target = targets[index];
			transform.SetParent (target.transform);
		}
		if (target != null) {
			xRotation += Input.GetAxis ("Mouse X") * xSensitivity * distance * 0.02f;
			yRotation -= Input.GetAxis ("Mouse Y") * ySensitivity * 0.02f;

			Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0);
			distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel")*5, minDist, maxDist);

			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
			Vector3 position = rotation * negDistance + target.transform.position;

			transform.rotation = rotation;
			transform.position = position;
		}
	}
}
