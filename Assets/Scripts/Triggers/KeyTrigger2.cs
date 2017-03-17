using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class KeyTrigger2 : MonoBehaviour {

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
                GameObject.Find("Text").GetComponent<Instructions>().ChangeText("Avoid accidentally moving objects.");
            } else {
                GameObject.Find("Text").GetComponent<Instructions>().ChangeText("Shine your flashlight at the ghosts to damage and slow them!");
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
        GameObject.Find("Text").GetComponent<Instructions>().ChangeText("If you disturb objects, you may summon evil spirits!");
    }
}
