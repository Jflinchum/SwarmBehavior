using UnityEngine;
using System.Collections;

public class ShieldController : MonoBehaviour {
	
	public bool shieldUp = false;
	public GameObject shield;
	
	public float shieldRadius;
	
	// Update is called once per frame
	void FixedUpdate () {
		if (shieldUp) {
			Collider[] flock = Physics.OverlapSphere(transform.position, shieldRadius+shieldRadius/2);
			if(flock.Length > 0){
				for(int i = 0; i < flock.Length; i++){
					if(flock[i].tag == "Flock" && !flock[i].GetComponent<FlockBehavior>().shielded){
						flock[i].GetComponent<FlockBehavior>().shielded = true;
						GameObject newShield = (GameObject)Instantiate(shield, transform.position + (flock[i].transform.position - transform.position).normalized * shieldRadius,Quaternion.identity);
						newShield.GetComponent<ShieldBehavior>().target = flock[i];
						newShield.GetComponent<ShieldBehavior>().radius = shieldRadius;
						newShield.GetComponent<ShieldBehavior>().shielder = gameObject;
					}
				}
			}
		}
	}
}
