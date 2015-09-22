using UnityEngine;
using System.Collections;

public class FlockBehavior : MonoBehaviour {

	//The sight range and maximum speed
	public float sightRad;
	public float maxSpeed;

	//The various distances to take into account
	public float avoidDist;
	public float cohDist;
	public float alignDist;
	public float maxSepDist;

	//The various force strengths
	public float avoidForce;
	public float cohForce;
	public float alignForce;
	public float sepForce;
	public float wayPointForce;

	//The delay between updating cohesion and alignment
	public float maxDelay = 0.5f;
	private float currDelay = 0.0f;
	private float sepDelay = Random.Range (0, 5);

	//Whether or not this flock object is shielded
	public bool shielded = false;

	//The rigidbody of this object
	private Rigidbody rbody;

	//The several states the object can be in
	private bool drop = false;
	private bool avoiding = false;
	private bool inPlace = false;

	//The queue of waypoints
	public Queue wayPoints = new Queue();
	private GameObject currWayPoint = null;

	//The array of neighbors
	public Collider[] neighbors;
	//Used for Debugging
	private Vector3 splitPointDebug;

	//Various vectors for the object
	private Vector3 center = new Vector3(0, 0, 0);
	private Vector3 alignment = new Vector3(0, 0, 0);
	private Vector3 cohesion = new Vector3(0, 0, 0);

	// Use this for initialization
	void Start () {
		rbody = transform.GetComponent<Rigidbody> ();
		rbody.AddForce(new Vector3(Random.Range (-3, 3), Random.Range (-3,3), maxSpeed));

		maxDelay = Random.Range (0, maxDelay);
		currDelay = maxDelay;//Instantly update vectors
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!drop) {
			//Reset avoiding check
			avoiding = false;

			//Setting a new seperation distance to add variation and entropy to flock
			float sepDist = maxSepDist*(1+Mathf.Sin ((Time.realtimeSinceStartup+sepDelay)*2*Mathf.PI));

			//Toggling inPlace
			if(Input.GetKeyDown("space")){
				inPlace = !inPlace;
				rbody.velocity = rbody.velocity.normalized * maxSpeed/2;
				if(inPlace)
					cohForce*=3;
				else
					cohForce/=3;
			}

			//Incrementing the delay
			currDelay += 1f * Time.deltaTime;

			//Speed limiter
			if (rbody.velocity.magnitude >= maxSpeed) 
				rbody.velocity = rbody.velocity.normalized * maxSpeed;

			//Look forward
			if (rbody.velocity.magnitude != 0)
				transform.rotation = Quaternion.LookRotation (rbody.velocity);

			//All of the objects found within the sight range
			neighbors = Physics.OverlapSphere (transform.position, sightRad);

			//For alignment
			int alignNeighbors = 0;
			Vector3 alignmentVect = new Vector3(0, 0, 0);

			//For cohesion
			int cohNeighbors = 0;

			//For seperation
			Vector3 sepVect = new Vector3(0,0,0);

			for (int i = 0; i < neighbors.Length; i++) {
				//For objects tagged as avoid, apply a strong normal force away from that object
				if (neighbors [i].tag == "Avoid") {
					Vector3 avoidDirect = neighbors[i].ClosestPointOnBounds(transform.position) - transform.position;
					float objDist = avoidDirect.magnitude;
					if (objDist <= avoidDist)
						rbody.AddForce (avoidDirect.normalized * avoidForce);
					avoiding = true;
					
				}

				//All factors for reactions to flock
				else if (neighbors [i].tag == "Flock" && !avoiding) {
					//The vector and distance to the object
					Vector3 flockVector = neighbors [i].transform.position - transform.position;
					float flockDist = Vector3.Distance (neighbors [i].transform.position, transform.position);

					//Seperation update
					if (flockDist <= sepDist) 
						sepVect -= flockVector;

					//Alignment update
					if (flockDist <= alignDist && flockDist > sepDist) {
						alignNeighbors += 1;
						alignmentVect += neighbors [i].transform.forward;
					}
					//Cohesion update
					if (flockDist <= cohDist && !inPlace) {
						cohNeighbors += 1;
						cohesion += neighbors[i].transform.position;
						center = cohesion/cohNeighbors;
					}
				}

				//Going around untagged objects
				else {
					if(Physics.CheckSphere(transform.position, 2*GetComponent<SphereCollider>().radius + avoidDist)){
						Vector3 closestPoint = neighbors[i].ClosestPointOnBounds(transform.position);
						//If it has a waypoint
						if(currWayPoint!=null){
							Vector3 splitPoint = 2*(neighbors[i].transform.position - neighbors[i].ClosestPointOnBounds(currWayPoint.transform.position)) + neighbors[i].transform.position;
							splitPointDebug = splitPoint;
							Vector3 direction = (closestPoint-splitPoint).normalized;
							if(!Physics.Raycast(splitPoint, closestPoint, (splitPoint-closestPoint).magnitude, 2)){
								rbody.AddForce(direction*avoidForce);
							}
						}
						//Applying a force away from object
						rbody.AddForce((transform.position-closestPoint).normalized * avoidForce*20/(0.7f*(transform.position-closestPoint).magnitude+1));
					}
				}
			}

			//Delay for updating the vector
			if(currDelay >= maxDelay && !inPlace){
				cohesion = center;
				alignment = alignmentVect/alignNeighbors;
				//Entropy Force
				Vector3 entropy = Vector3.Cross(alignment, center - transform.position);
				rbody.AddForce(entropy.normalized * (Random.Range (0, alignForce)));
			}

			//Alignment Force
			if(alignNeighbors != 0 && !inPlace)
				rbody.AddForce (alignment.normalized * alignForce);

			//Cohesion Force
			if(cohNeighbors != 0 || inPlace){
				Vector3 direction = cohesion-transform.position;
				if(direction.magnitude >= cohForce)
					direction = direction.normalized*cohForce;
				rbody.AddForce (direction);
			}
			//Seperation Force
			if(alignNeighbors != 0)
				rbody.AddForce (sepVect.normalized*sepForce);

			//Setting the current waypoint
			if(wayPoints.Count > 0 && currWayPoint == null)
				currWayPoint = (GameObject)wayPoints.Dequeue();

			//Going towards waypoint
			if(currWayPoint != null && !inPlace){
				rbody.AddForce((currWayPoint.transform.position - transform.position).normalized * wayPointForce);
				if(Vector3.Distance(currWayPoint.transform.position, transform.position) < 20)
					Destroy(currWayPoint);
			}
			//Resetting currDelay
			if(currDelay >= maxDelay)
				currDelay = 0;
		}
	}
	//If it collides with something not with the flock, have it drop and deactivate it's trail
	void OnCollisionEnter(Collision collision){
		if (collision.transform.tag != "Flock") {
			drop = true;
			GetComponent<TrailRenderer>().enabled = false;
			GetComponent<Rigidbody>().useGravity = true;
		}
	}
	//Debugging for showing the cohesion vector, alignment vector, the sight radius, and the splitpoint position
	//Going around objects
	void OnDrawGizmos(){
		Gizmos.color = Color.blue;
		Gizmos.DrawRay (transform.position, cohesion.normalized*cohForce);
		Gizmos.color = Color.red;
		Gizmos.DrawRay (transform.position, alignment.normalized*alignForce);
		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere (transform.position, sightRad);
		Gizmos.color = Color.green;
		Gizmos.DrawCube (splitPointDebug, new Vector3(20,20,20));
	}
	//When the object is selected, it shows their own sight radius in red to easily show it
	void OnDrawGizmosSelected(){
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (transform.position, sightRad);
	}
}
