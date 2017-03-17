using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Key : Grabable {
    public int door;
    

    public override void Use(GameObject palm) {
        KeyInventory kI = palm.GetComponentInParent<KeyInventory>();
        GetComponent<AudioSource>().Play();
        kI.PutKey(door);
		transform.parent = kI.keyPoint;
		transform.localScale = new Vector3(.33f,.33f,.33f);
		transform.localPosition = new Vector3(0,0,0+ (kI.GetKeyCount()-1)*0.005f);
		transform.localRotation = Quaternion.Euler(15*(kI.GetKeyCount() - 1), 15 * (kI.GetKeyCount() - 1), 0);
		gameObject.layer = 0;
		foreach (Transform child in transform) {
            //child.gameObject.layer = 0;
            if(child.GetComponent<BoxCollider>()!=null)
                child.GetComponent<BoxCollider>().enabled = false;
		}
    }
}
