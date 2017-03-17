using UnityEngine;
using System.Collections;

public class SqueakyFloor : MonoBehaviour {
    private bool activated = false;
    private float lastActivated = 0.0f;
    void Update() {
        if (activated && Time.time - lastActivated > 1.0f) {
            GameObject.Find("Player").GetComponent<PlayerStatus>().currentRoom.Noise(transform.position, 10.0f);
        } else if (activated && Time.time - lastActivated > 30.0f) {
            activated = false;
        }
    }
	void OnTriggerEnter(Collider col) {
        if (!activated && col.transform.tag.Equals("Player")) {
            activated = true;
            lastActivated = Time.time;
            GetComponent<AudioSource>().Play();
        }
    }
}
