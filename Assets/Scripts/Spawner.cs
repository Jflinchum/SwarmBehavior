using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	public GameObject spawn;
	public float rad;
	public int amount = 0;

	public GameObject[] objects;

	// Use this for initialization
	void Awake () {
		objects = new GameObject[amount];
		for (int i = 0; i < amount; i++) {
			Vector3 pos = new Vector3(transform.position.x + Random.Range (-rad, rad), 
			                          transform.position.y + Random.Range (-rad, rad), 
			                          transform.position.z + Random.Range (-rad, rad));

			for(int j = 0; j < 50; j++){
				if(!Physics.CheckSphere(pos, spawn.GetComponent<SphereCollider>().radius))
					break;
				pos = new Vector3(transform.position.x + Random.Range (-rad, rad), 
				                  transform.position.y + Random.Range (-rad, rad),
				                  transform.position.z + Random.Range (-rad, rad));
			}
			objects[i] = (GameObject)Instantiate(spawn, pos, Quaternion.identity);
		}
	}

}
