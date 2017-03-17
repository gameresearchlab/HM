using UnityEngine;
using System.Collections;

public class Win : MonoBehaviour {

	void OnTriggerEnter(Collider col) {
        if (col.transform.tag.Equals("Player")){
            Debug.Log("Test");
            GameObject.Find("Player").GetComponent<PlayerMovement>().StartCoroutine(GameObject.Find("Player").GetComponent<PlayerMovement>().Win());
        }
    }
}
