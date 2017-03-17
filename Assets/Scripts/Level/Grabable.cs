using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Grabable : MonoBehaviour {
    public Light noticeLight;

    public virtual void Use(GameObject palm) {
        Debug.Log("Not Implemented");
    }

    public virtual IEnumerator Hover() {
        if(noticeLight != null) {
            noticeLight.enabled = true;
            yield return new WaitForEndOfFrame();

            if(noticeLight!=null)
                noticeLight.enabled = false;
        }
    }
}
