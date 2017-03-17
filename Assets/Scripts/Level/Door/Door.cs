using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Door : MonoBehaviour {
    public bool open = false;
    public int number;
    public bool locked = false;
    public Keyhole keyhole;

    public virtual void Open(Vector3 pushDir) {
        GetComponent<AudioSource>().Play();
        StartCoroutine(PerformOpen(pushDir));
    }

    public virtual IEnumerator PerformOpen(Vector3 pushDir) {
        Quaternion openRotation = Quaternion.LookRotation(pushDir, transform.up);
        while (Vector3.Angle(transform.forward, pushDir) > 0)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, openRotation, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        open = true;
    }
}
