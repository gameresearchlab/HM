using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Doorknob : Grabable {
    public GameObject door;

    public override void Use(GameObject palm) {
        Door doorScript = door.GetComponent<Door>();
        Vector3 doorPos = door.transform.position;
        PlayerMovement playerMovement = palm.GetComponentInParent<PlayerMovement>();

        if (!doorScript.locked && !doorScript.open) {
            Vector3 pushDir = transform.forward;
            doorScript.Open(pushDir.normalized);
        }
    }
}
