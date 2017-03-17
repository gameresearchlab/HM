using UnityEngine;
using System.Collections;

public class EndInstructions : MonoBehaviour {
    public GameObject instructionText;
	void OnTriggerEnter(Collider col) {
        if (col.transform.tag.Equals("Player")) {
            instructionText.SetActive(false);
        }
    }
}
