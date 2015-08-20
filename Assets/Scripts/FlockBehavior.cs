using UnityEngine;
using System.Collections;

public class FlockBehavior : MonoBehaviour {

	public float sightRad;
	public float maxSpeed;

	public float avoidDist;
	public float cohDist;
	public float alignDist;
	public float sepDist;

	public float avoidForce;
	public float cohForce;
	public float alignForce;
	public float sepForce;

	public float altitude;

	private Rigidbody rbody;
	private bool drop = false;
	private bool avoiding = false;
	// Use this for initialization
	void Start () {
		rbody = transform.GetComponent<Rigidbody> ();

		rbody.AddForce(new Vector3(Random.Range (-100, 100), Random.Range (-100,100), maxSpeed*10));
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!drop) {
			//Reset avoiding check
			avoiding = false;

			//Speed limiter
			if (rbody.velocity.magnitude >= maxSpeed) {
				rbody.velocity = rbody.velocity.normalized * maxSpeed;
			}

			//Look forward
			if (rbody.velocity.magnitude != 0)
				transform.rotation = Quaternion.LookRotation (rbody.velocity);

			//All of the objects found within the sight range
			Collider[] neighbors = Physics.OverlapSphere (transform.position, sightRad);

			//For alignment
			int alignNeighbors = 0;
			Vector3 alignment = new Vector3 (0, 0, 0);

			//For cohesion
			int cohNeighbors = 0;
			Vector3 cohesionPos = new Vector3( 0, 0, 0);

			for (int i = 0; i < neighbors.Length; i++) {
				if (neighbors [i].tag == "Flock" && avoiding == false) {
					//The vector and distance to the object
					Vector3 flockVector = neighbors [i].transform.position - transform.position;
					float flockDist = Vector3.Distance (neighbors [i].transform.position, transform.position);

					//Seperation update
					if (flockDist <= sepDist) {
						rbody.AddForce (-flockVector.normalized * sepForce * 1 / ((flockDist + 1) / sepDist));
					}
					//Alignment update
					if (flockDist <= alignDist && flockDist > sepDist) {
						alignNeighbors += 1;
						alignment += neighbors [i].transform.forward;
					}
					//Cohesion update
					if (flockDist <= cohDist) {
						cohNeighbors += 1;
						cohesionPos += neighbors[i].transform.position;
					}
				} 
				else if (neighbors [i].tag == "Avoid") {
					Vector3 avoidDirect = transform.position - neighbors [i].transform.position;
					float objDist = Vector3.Distance (neighbors [i].transform.position, transform.position);
					if (objDist <= avoidDist)
						rbody.AddForce (avoidDirect.normalized * avoidForce);
					avoiding = true;
				}
				//For other objects to go around
				else {
					if(Physics.CapsuleCast(transform.position, transform.position + (transform.forward * avoidDist), GetComponent<SphereCollider>().radius + (GetComponent<SphereCollider>().radius/2), transform.forward)){
						Vector3 closestPos = neighbors[i].bounds.ClosestPoint(transform.position);
						Vector3 toCenter = neighbors[i].transform.position - transform.position;
						Vector3 direction = toCenter - closestPos;
						if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
							direction = new Vector3(-direction.x - (Mathf.Sign (direction.x) * neighbors[i].bounds.size.x) , direction.y, direction.z).normalized * avoidForce;
						else
							direction = new Vector3(direction.x, -direction.y - ( Mathf.Sign (direction.y) * neighbors[i].bounds.size.y), direction.z).normalized * avoidForce;
						avoiding = true;
						rbody.AddForce(direction);
					}
				}
			}
			//Alignment force
			if (alignNeighbors != 0)
				rbody.AddForce (alignment.normalized * alignForce);
			if(cohNeighbors != 0){
				Vector3 direction = (cohesionPos/cohNeighbors)-transform.position;
				rbody.AddForce (direction.normalized * cohForce);
			}
		}
	}

	void OnCollisionEnter(Collision collision){
		if (collision.transform.tag != "Flock") {
			drop = true;
			GetComponent<TrailRenderer>().enabled = false;
			GetComponent<Rigidbody>().useGravity = true;
		}
	}
}
