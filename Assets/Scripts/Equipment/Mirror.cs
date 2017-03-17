using UnityEngine;
using System.Collections;

public class Mirror : MonoBehaviour {

    public MirrorCamera mirrorCamera;
    public float lastOnTime;

    public void Reflect(float ratio) {
        lastOnTime = Time.time;
        mirrorCamera.Reflect(ratio);
    }

    public void ReflectOff() {
        mirrorCamera.ReflectOff();
    }

    public void Update() {
        if (Time.time - lastOnTime > 0.5f) {
            mirrorCamera.ReflectOff();
        }
    }
}
