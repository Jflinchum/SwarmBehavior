using UnityEngine;
using System.Collections;

public class WayPointManager : MonoBehaviour {

	public float distance;
	public GameObject wayPoint;

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(1)) {
			float spawnDistance = distance;
			RaycastHit[] allCollisions = Physics.RaycastAll(transform.position, transform.forward, distance);

//			if(allCollisions.Length != 0){
//				for(int i = 0; i < allCollisions.Length; i++){
//					if(allCollisions[i].collider.tag != "Flock" && allCollisions[i].distance < spawnDistance){
//						spawnDistance = allCollisions[i].distance;
//					}
//				}
//			}
			GameObject newWayPoint = (GameObject)Instantiate(wayPoint, transform.position + transform.forward*spawnDistance, Quaternion.identity);
			GameObject[] flockMembers = GetComponent<SwarmCamera>().targets;

			for(int i = 0; i < flockMembers.Length; i++)
				flockMembers[i].GetComponent<FlockBehavior>().wayPoints.Enqueue(newWayPoint);

		}
	}
}
