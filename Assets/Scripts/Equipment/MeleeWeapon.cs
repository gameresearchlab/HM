using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class MeleeWeapon : MonoBehaviour {
    public float damage = 5.0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnTriggerEnter(Collider col) {
        if(col.tag == "Player" && GetComponentInParent<GhostAttack>().attacking) {
            GameObject.Find("Player").GetComponent<PlayerHealth>().Hit(damage);
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
