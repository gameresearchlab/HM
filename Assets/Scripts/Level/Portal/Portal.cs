using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Portal : MonoBehaviour {
    private Room thisRoom;
    public Room thatRoom;
    public int ghosts = 0;
    public float delay = 1.0f;
    public GameObject route;
    private bool open = false;
    private Light light;
    // Use this for initialization
    void Start () {
        transform.localScale = new Vector3(0,0,0);
        this.light = GetComponentInChildren<Light>();
        this.light.intensity = 0.0f;
        thisRoom = GetComponentInParent<Room>();
	}

    //Create a ghost at the given point
    public IEnumerator SpawnGhosts() {
        while(ghosts > 0) {
            ghosts--;
            GameObject spawnedGhost = thisRoom.Spawn(transform);
            spawnedGhost.GetComponent<GhostSenses>().Alert(thisRoom.transform.position);
            yield return new WaitForSeconds(delay);
        }
    }

    public GameObject SpawnGhost() {
        GameObject ghost = thisRoom.Spawn(transform);

        if (ghost.GetComponent<GhostSenses>().boundToSpawnRoute) {
            int waypointCount = this.route.transform.childCount;

            for (int i = 0; i < waypointCount; i++) {
                ghost.GetComponent<GhostSenses>().route.Add(this.route.transform.GetChild(i).gameObject);
            }

            ghost.GetComponent<GhostSenses>().lastWaypoint = waypointCount-1;
            ghost.GetComponent<GhostSenses>().nextWaypoint = 0;
        }
        return ghost;
    }

    public IEnumerator OpenClose()
    {
        GetComponent<AudioSource>().Play();
        StartCoroutine(Open());
        while (!open)
        {
            yield return new WaitForEndOfFrame();
        }
        float duration = 0.25f;

        while (duration > 0.0f)
        {
            duration -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(Close());
    }

    public IEnumerator Open()
    {
        float length = 0.5f;
        float duration = 0.5f;

        while(duration > 0.0f) {
            duration -= Time.deltaTime;

            transform.localScale = new Vector3(1 - (duration / length), 1 - (duration / length), 1 - (duration / length));
            this.light.intensity = Mathf.Lerp(this.light.intensity, 1.0f, 1 - (duration / length));
            yield return new WaitForEndOfFrame();
        }
        open = true;
    }

    public IEnumerator Close()
    {
        float length = 1.0f;
        float duration = 1.0f;

        while (duration > 0.0f)
        {
            duration -= Time.deltaTime;
            duration = (duration<0)?0:duration;

            transform.localScale = new Vector3((duration / length), (duration / length), (duration / length));
            this.light.intensity = Mathf.Lerp(this.light.intensity, 0.0f, (duration / length));
            yield return new WaitForEndOfFrame();
        }
        this.light.enabled = false;
        open = false;
    }

    // Update is called once per frame
    /*void OnTriggerStay (Collider col) {
	    if(col.tag == "Ghost") {
            thisRoom.Phase(col.gameObject);
        }
	}*/
}
