using UnityEngine;
using System.Collections;

public class ShieldBehavior : MonoBehaviour {
	
	public Collider target;
	public float radius;
	public GameObject shielder;
	
	// Use this for initialization
	void Start () {
		transform.rotation = Quaternion.FromToRotation(transform.position, target.transform.position);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (Vector3.Distance (transform.position, target.transform.position) > radius) {
			target.GetComponent<FlockBehavior>().shielded = false;
			Destroy (gameObject);
		}
		transform.LookAt (target.transform.position);
		transform.position = shielder.transform.position + (target.transform.position - shielder.transform.position).normalized * radius;
	}
}
