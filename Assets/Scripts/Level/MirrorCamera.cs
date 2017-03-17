using UnityEngine;
using System.Collections;

public class MirrorCamera : MonoBehaviour {
    GameObject player;
    public GameObject flashlight;
	// Use this for initialization
	void Start () {
        player = GameObject.Find("Player");
	}

    public void Reflect(float ratio) {
        GetComponent<Light>().enabled = true;
        GetComponent<Light>().intensity = ratio;
        GetComponent<Light>().spotAngle = ((1 - ratio) * 90) + 60 ;
    }

    public void ReflectOff() {
        GetComponent<Light>().enabled = false;
    }

    // Update is called once per frame
    void LateUpdate () {
        transform.rotation = Quaternion.Inverse(Quaternion.LookRotation(flashlight.transform.forward, Vector3.up));
	}
}
