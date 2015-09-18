using UnityEngine;
using System.Collections;

public class WayPointManager : MonoBehaviour {

	public float distance;
	public GameObject wayPoint;

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(1)) {
			float spawnDistance = distance;
			GameObject newWayPoint = (GameObject)Instantiate(wayPoint, transform.position + transform.forward*spawnDistance, Quaternion.identity);
			GameObject[] flockMembers = GetComponent<SwarmCamera>().targets;

			for(int i = 0; i < flockMembers.Length; i++){
				GameObject flock = (GameObject)flockMembers[i];
				flock.GetComponent<FlockBehavior>().wayPoints.Enqueue(newWayPoint);
			}
		}
	}
}
