using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class LightCone : MonoBehaviour {
    private float maxDistance;

    // Use this for initialization
    void Start () {
        maxDistance = 30;
	}

    void OnTriggerEnter(Collider col) {
        if (col.tag.Equals("Mirror")) {
            float distance = (transform.position - col.transform.position).magnitude;
            if (distance < maxDistance && distance > 0) {
                float ratio = ((maxDistance - distance) / maxDistance);
                col.GetComponent<Mirror>().Reflect(ratio);
            }
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.tag.Equals("Mirror")) {
            col.GetComponent<Mirror>().ReflectOff();
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.tag.Equals("Mirror")) {
            float distance = (transform.position - col.transform.position).magnitude;
            if (distance < maxDistance && distance > 0) {
                float ratio = ((maxDistance - distance) / maxDistance);
                col.GetComponent<Mirror>().Reflect(ratio);
            }
        }
        if ((col.tag.Equals("Ghost") && col.GetComponentInParent<GhostHealth>())) {
            float distance = (transform.position - col.transform.position).magnitude;
            if (distance < maxDistance && distance > 0) {
                float ratio = ((maxDistance - distance) / maxDistance);
                //Debug.Log("In Light. Distance: "+distance+"/MaxDistance: "+maxDistance+"/Magnitude: "+ ratio);
                col.GetComponentInParent<GhostHealth>().Hit(ratio);
            }
        }
    }
}
