using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class DrawString : Grabable {
    public GameObject curtain;
    bool closed = true;

    public override void Use(GameObject palm) {
        //Door doorScript = door.GetComponent<Door>();
        //Vector3 doorPos = door.transform.position;
        //PlayerMovement playerMovement = palm.GetComponentInParent<PlayerMovement>();

        if (closed) {
            Transform p = transform.parent;
            p.GetComponentInChildren<Curtain>().GetComponent<Animator>().SetTrigger("Activate");
            closed = false;
            p.GetComponentInChildren<Curtain>().windowLight.SetActive(true);
            p.GetComponentInChildren<Moonlight>().Activate();
            Debug.Log("Test");
        }
    }
}
