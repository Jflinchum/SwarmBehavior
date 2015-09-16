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
	public float wayPointForce;

	public float maxDelay = 0.5f;
	private float currDelay = 0.0f;

	public float altitude;

	private Rigidbody rbody;
	private bool drop = false;
	private bool avoiding = false;
	private bool inPlace = false;

	public Queue wayPoints = new Queue();
	private GameObject currWayPoint = null;

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

			//Toggling inPlace
			if(Input.GetKeyDown("space")){
				inPlace = !inPlace;
				if(inPlace){
					//rbody.velocity = new Vector3(0,0,0);
					cohForce *= 2;
				}
				else
					cohForce /= 2;
			}

			//Incrementing the delay
			currDelay += 1f * Time.deltaTime;

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
			Vector3 alignmentVect = new Vector3(0, 0, 0);

			//For cohesion
			int cohNeighbors = 0;

			for (int i = 0; i < neighbors.Length; i++) {
				if (neighbors [i].tag == "Flock" && !avoiding) {
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
						alignmentVect += neighbors [i].transform.forward;
					}
					//Cohesion update
					if (flockDist <= cohDist && !inPlace) {
						cohNeighbors += 1;
						cohesion += neighbors[i].transform.position;
						center = cohesion/cohNeighbors;
					}
				} 
				else if (neighbors [i].tag == "Avoid") {
					Vector3 avoidDirect = transform.position - neighbors [i].transform.position;
					float objDist = Vector3.Distance (neighbors [i].transform.position, transform.position);
					if (objDist <= avoidDist)
						rbody.AddForce (avoidDirect.normalized * avoidForce);
					avoiding = true;
				}
				//Going around untagged objects
				else {
					if(Physics.CheckCapsule(transform.position, transform.position + transform.forward*avoidDist, 2*GetComponent<SphereCollider>().radius + avoidDist)){
						Vector3 closestPos = neighbors[i].bounds.ClosestPoint(transform.position);
						Vector3 toCenter = neighbors[i].transform.position - transform.position;
						Vector3 direction = toCenter - closestPos;

						if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
							direction = new Vector3(-direction.x - (Mathf.Sign (direction.x) * (neighbors[i].bounds.size.x + avoidDist)), direction.y, direction.z).normalized * avoidForce;
						else
							direction = new Vector3(direction.x, -direction.y - ( Mathf.Sign (direction.y) * (neighbors[i].bounds.size.y + avoidDist)), direction.z).normalized * avoidForce;
						avoiding = true;
						rbody.AddForce(direction);
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
				rbody.AddForce (direction.normalized * cohForce);
			}
			//Setting the current waypoint
			if(wayPoints.Count > 0 && currWayPoint == null){
				currWayPoint = (GameObject)wayPoints.Dequeue();
			}
			//Going towards waypoint
			if(currWayPoint != null && !inPlace){
				rbody.AddForce((currWayPoint.transform.position - transform.position).normalized * wayPointForce);
				if(Vector3.Distance(currWayPoint.transform.position, transform.position) < 20)
					Destroy(currWayPoint);
			}
			//Resetting currDelay
			if(currDelay >= maxDelay){
				currDelay = 0;
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

	void OnDrawGizmosSelected(){
		Gizmos.color = Color.blue;
		Gizmos.DrawRay (transform.position, cohesion.normalized*cohForce);
		Gizmos.color = Color.red;
		Gizmos.DrawRay (transform.position, alignment.normalized*alignForce);
	}
}
