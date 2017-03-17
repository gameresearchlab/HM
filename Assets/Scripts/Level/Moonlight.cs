using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Moonlight : MonoBehaviour {
    List<GameObject> illuminated = new List<GameObject>();
    public bool active = false;
	public void Activate() {
        active = true;
        GetComponent<MeshCollider>().enabled = true;
    }

    public void Update() {
        if (active) {
            foreach(GameObject ghost in illuminated) {
                ghost.GetComponentInParent<GhostHealth>().Hit(1.0f);
            }
        }
    }

    void OnTriggerEnter(Collider col) {
        if(col.GetComponentInParent<GhostHealth>() != null) {
            if (!illuminated.Contains(col.GetComponentInParent<GhostHealth>().gameObject)) {
                illuminated.Add(col.GetComponentInParent<GhostHealth>().gameObject);
            }
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.GetComponentInParent<GhostHealth>() != null) {
            if (illuminated.Contains(col.GetComponentInParent<GhostHealth>().gameObject)) {
                illuminated.Remove(col.GetComponentInParent<GhostHealth>().gameObject);
            }
        }
    }
}
