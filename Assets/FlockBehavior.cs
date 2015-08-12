using UnityEngine;
using System.Collections;

public class FlockBehavior : MonoBehaviour {

	public float sightRad;
	public float maxSpeed;

	public float avoidDist;
	public float cohDist;
	public float alignDist;
	public float sepDist;

	public float cohForce;
	public float alignForce;
	public float sepForce;


	private Rigidbody rbody;
	// Use this for initialization
	void Start () {
		rbody = transform.GetComponent<Rigidbody> ();

		rbody.AddForce(new Vector3(Random.Range (-3, 3), Random.Range (-3,3), maxSpeed/2));
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//Speed limiter
		if (rbody.velocity.magnitude >= maxSpeed) {
			rbody.velocity = rbody.velocity.normalized * maxSpeed;
		}

		Collider[] neighbors = Physics.OverlapSphere (transform.position, sightRad);

		int alignNeighbors = 0;
		Vector3 alignment = new Vector3 (0, 0, 0);

		int cohNeighbors = 0;
		Vector3 cohesionPos = new Vector3 (0, 0, 0);

		for (int i = 0; i < neighbors.Length; i++) {

			if(neighbors[i].tag == "Flock"){
				Vector3 flockVector = neighbors[i].transform.position - transform.position;
				float flockDist = flockVector.magnitude;

				//Seperation update
				if(flockDist <= sepDist){
					rbody.AddForce(-flockVector.normalized*sepForce);
				}
				//Alignment update
				if(flockDist <= alignDist && flockDist > sepDist){
					alignNeighbors += 1;
					alignment += neighbors[i].transform.forward;
				}
				//Cohesion update
				if(flockDist <= cohDist && flockDist > alignDist){
					cohNeighbors += 1;
					cohesionPos += neighbors[i].transform.position;
				}
			}
			//For other objects to go around
			else{

			}
		}

		if (alignNeighbors != 0)
			rbody.AddForce (alignment.normalized * alignForce);

		if (cohNeighbors != 0)
			rbody.AddForce (cohesionPos / cohNeighbors * cohForce);
	}
}
