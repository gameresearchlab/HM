using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class KeyTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player" || col.tag == "PlayerBody" && GameObject.Find("Text").GetComponent<Instructions>().text != "") {
            GameObject.Find("Text").GetComponent<Instructions>().StopAllCoroutines();
            GameObject.Find("Text").GetComponent<Instructions>().alternating = false;
            if (col.GetComponentInParent<KeyInventory>().GetKeyCount() <= 0) {
                GameObject.Find("Text").GetComponent<Instructions>().ChangeText("Pinch the key between your thumb and index finger to pick up.");
            } else {
                GameObject.Find("Text").GetComponent<Instructions>().ChangeText("You can also do this to pickup batteries when yours runs out.");
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if ((col.tag == "PlayerBody") && !GameObject.Find("Text").GetComponent<Instructions>().alternating && GameObject.Find("Text").GetComponent<Instructions>().text != "") {
            StartCoroutine(ClearText());
        }
    }

    IEnumerator ClearText()
    {
        float duration = 3.0f;
        while(duration > 0.0f)
        {
            duration -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        GameObject.Find("Text").GetComponent<Instructions>().ChangeText("Aim the flashlight at the edges of the screen to rotate.");
        GameObject.Find("Text").GetComponent<Instructions>().StartCoroutine(GameObject.Find("Text").GetComponent<Instructions>().Alternate());
    }
}
