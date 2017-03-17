using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class DoorTrigger : MonoBehaviour {
    bool hadKey = false;
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
            if (col.GetComponentInParent<KeyInventory>().GetKeyCount() <= 0 && !hadKey)
            {
                GameObject.Find("Text").GetComponent<Instructions>().ChangeText("This door is locked, go pickup the key so you can unlock it.");
            } else if (col.GetComponentInParent<KeyInventory>().GetKeyCount() <= 0 && hadKey) {
                GameObject.Find("Text").GetComponent<Instructions>().ChangeText("The door is unlocked. Touch the doorknob to open it.");
            } else {
                hadKey = true;
                GameObject.Find("Text").GetComponent<Instructions>().ChangeText("Move your hand near the color coded door lock to unlock it.");
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
