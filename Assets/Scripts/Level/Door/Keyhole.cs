using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Keyhole : Grabable {
    public Door door;

    public override void Use(GameObject palm) {
        if (door != null) {
            Vector3 doorPos = door.transform.position;
            KeyInventory kI = palm.GetComponentInParent<KeyInventory>();

            if (door.locked)
            {
                if (kI.HasKey(door.number))
                {
                    kI.RemoveKey(door.number);
                    for(int i = 0; i < kI.keyPoint.childCount; i++) {
                        if (kI.keyPoint.GetChild(i).GetComponent<Key>() != null) {
                            GameObject key = kI.keyPoint.GetChild(i).gameObject;
                            if(key.GetComponent<Key>().door == door.number) {
                                key.transform.position = transform.position;
                                key.transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
                                key.transform.parent = null;
								key.gameObject.layer = 0;
								foreach (Transform child in key.transform) {
									child.gameObject.layer = 0;
								}
                                StartCoroutine(PerformTurnKey(key));
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    IEnumerator PerformTurnKey(GameObject key)
    {
        GetComponent<AudioSource>().Play();
        Vector3 turnAxis = transform.forward;
        Vector3 turnDir = transform.forward;
        float duration = 3.0f;
        while (Vector3.Angle(key.transform.forward, Vector3.down) > 10 && duration >= 0.0f)
        {
            duration -= Time.deltaTime;
            key.transform.Rotate(turnAxis, Time.deltaTime*100);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(1.0f);
        Destroy(key);
        door.locked = false;
        noticeLight = null;
        door.Open(transform.forward);
    }
}
