using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Battery : Grabable {
    public float amount;
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	public void Load () {
        Destroy(gameObject);
	}

	public override void Use(GameObject palm) {
        PlayerMovement pM = palm.GetComponentInParent<PlayerMovement>();

        pM.flashlight.SetActive(true);
        pM.flashlight.GetComponent<Flashlight>().LoadBattery(amount);
		Destroy (gameObject);
	}
}
