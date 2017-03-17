using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class GhostSight : MonoBehaviour {
    public Transform eyeLine;
    public bool targetInSight = false;
    public LayerMask sightMask;

    public void OnTriggerStay(Collider col) {
        if (col.tag == "Player") {
            RaycastHit hit;
            Vector3 collisionPoint = new Vector3(col.transform.position.x, col.transform.position.y, col.transform.position.z);
            //Debug.DrawRay(eyeLine.transform.position, collisionPoint - eyeLine.transform.position, Color.red, 1.0f);
            float distance = Vector3.Distance(eyeLine.position, collisionPoint);
            if (Physics.Raycast(eyeLine.transform.position, collisionPoint - eyeLine.transform.position, out hit, distance+2, sightMask)){
                if (hit.transform.tag == "Player") {
                    //Debug.Log("Player seen");
                    StopCoroutine("TargetLost");
                    targetInSight = true;
                    GetComponentInParent<GhostSenses>().OnTargetSpotted(col.gameObject);
                } else {
                    //Debug.Log("Player lost");
                    if (targetInSight) {
                        targetInSight = false;
                        StartCoroutine(TargetLost(col.gameObject, col.transform.position));
                    }
                }
            } else {
                //Debug.Log("Player lost");
                if (targetInSight) {
                    targetInSight = false;
                    StartCoroutine(TargetLost(col.gameObject, col.transform.position));
                }
            }
        }
    }

    public void OnTriggerExit(Collider col) {
        if (col.tag == "Player")
        {
            RaycastHit hit;
            Vector3 collisionPoint = new Vector3(col.transform.position.x, col.transform.position.y, col.transform.position.z);
            Debug.DrawRay(eyeLine.transform.position, collisionPoint - eyeLine.transform.position, Color.red, 1.0f);
            float distance = Vector3.Distance(eyeLine.position, collisionPoint);
            if (Physics.Raycast(eyeLine.transform.position, collisionPoint - eyeLine.transform.position, out hit, distance+2, sightMask))
            {
                if (hit.transform.tag == "Player")
                {
                    //Debug.Log("Player lost");
                    if (targetInSight) {
                        targetInSight = false;
                        StartCoroutine(TargetLost(col.gameObject, col.transform.position));
                    }
                }
            }
        }
    }

    IEnumerator TargetLost(GameObject target, Vector3 lastPosition)
    {
        float startTime = Time.time;
        while(Time.time-startTime < 10.0f) {
            yield return new WaitForEndOfFrame();
        }
        GetComponentInParent<GhostSenses>().OnTargetLost(lastPosition);
    }
}
