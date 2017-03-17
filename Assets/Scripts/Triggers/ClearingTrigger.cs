﻿using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class ClearingTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player" || col.tag == "PlayerBody") {
            GameObject.Find("Text").GetComponent<Instructions>().StopAllCoroutines();
            GameObject.Find("Text").GetComponent<Instructions>().alternating = false;
            GameObject.Find("Text").GetComponent<Instructions>().ChangeText("");
        }
    }
}
