using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Room : MonoBehaviour {

    //Assets
    public GameObject ghostPrefab;

    //Reference Lists
    private List<Portal> portals = new List<Portal>();
    private List<GameObject> tables = new List<GameObject>();
    public List<GameObject> spawnedGhosts = new List<GameObject>();

    //Counters
    public int phasedGhosts;
    public int phased;
    private float lastSpawnTime;

    //Status
    private string status;

    public GameObject batteryPrefab;

	//Use this for initialization
	void Start () {
        phased = phasedGhosts;
        //Get all the portals in the room
        Portal[] portals = GetComponentsInChildren<Portal>();
        foreach(Transform child in transform) {
            if(child.name == "Furniture") {
                foreach (Transform c in child) {
                    if (c.name == "BatteryPoint") {
                        Debug.Log("Adding battery point to room " + gameObject.name);
                        tables.Add(c.gameObject);
                    }
                }
            }
        }
        foreach(Portal portal in portals)
        {
            this.portals.Add(portal);
        }
        
        //Set the room unaware
        Unaware();
    }

    //When the room becomes aware, allocate ghosts to portals
    public void AllocateGhosts() {
        Debug.Log("Allocate");
        while(phasedGhosts > 0) {
            for(int i = 0; i < portals.Count; i++) {
                if(phasedGhosts > 0) {
                    portals[i].ghosts++;
                    phasedGhosts--;
                }
            }
        }
    }

    //Start the portal coroutine that spawns ghosts after a certain amount of time
    public void SpawnAtPortals() {
        for(int i = 0; i < portals.Count; i++) {
            portals[i].StartCoroutine(portals[i].SpawnGhosts());
        }
    }

    //Spawn a phased ghost at a random portal or spawn an adjacent phased ghost at a portal
    public void SpawnPhasedGhosts(Transform point) {
        Portal adjacent = FindAdjacentPhasedGhost();

        //If there's phased ghosts in this room, spawn one
        if (phasedGhosts > 0) {
            phasedGhosts--;

            Portal portal = GetRandomPortal();
            Spawn(portal.transform);

            lastSpawnTime = Time.time;
        //If there's phased ghosts in an adjacent room, spawn one
        } else if (adjacent != null) {
            adjacent.thatRoom.phasedGhosts--;
            adjacent.StartCoroutine(adjacent.OpenClose());
            adjacent.SpawnGhost();

            lastSpawnTime = Time.time;
        }
    }

    //Create a ghost at the given point
    public GameObject Spawn(Transform point) {
        GameObject ghostInstance = Instantiate(ghostPrefab, point.position, point.rotation) as GameObject;
        ghostInstance.GetComponent<GhostSenses>().spawnPoint = point;
        spawnedGhosts.Add(ghostInstance);
        phasedGhosts--;
        return ghostInstance;
    }

    public Portal GetRandomPortal() {
        Portal portal = portals[Random.Range(0, portals.Count)];
        return portal;
    }

    //Change the room status to aware, start spawning enemies
    public void Aware() {
        string statusType;
        float statusTime, statusDuration;
        ParseStatus(out statusType, out statusTime, out statusDuration);

        if(statusType != "aware" || phasedGhosts > 0) {
            AllocateGhosts();
            SpawnAtPortals();
            ChangeStatus("aware", 2.0f);
        }
    }

    //Change the room status to unaware, stop spawning enemies
    public void Unaware() {
        ChangeStatus("unaware", 0.0f);
    }

    public GameObject Alert() {
        ChangeStatus("alert", 0);

        if (spawnedGhosts.Count <= 0 && portals.Count > 0) {
            Portal portal = portals[Random.Range(0, portals.Count)];
            portal.StartCoroutine(portal.OpenClose());
            return portal.SpawnGhost();
        }

        //There is already a ghost spawned
        return null;
    }

    public void Noise(Vector3 point, float range) {
        //Get the status values
        string statusType;
        float statusTime, statusDuration;
        ParseStatus(out statusType, out statusTime, out statusDuration);

        switch (statusType)
        {
            case "unaware":
                //Debug.Log("Unaware Noise Heard Room");
                GameObject ghostInstance = Alert();
                if (ghostInstance != null) {
                    ghostInstance.GetComponent<GhostSenses>().NoiseHeard(point);
                }
                break;
            case "alert":
                //Debug.Log("Alert Noise Heard Room");
                if (phasedGhosts > 0 && spawnedGhosts.Count > 0) {
                    spawnedGhosts[0].GetComponent<GhostSenses>().NoiseHeard(point);
                    Alert();
                }
                break;
        }

        Collider[] objects = Physics.OverlapSphere(point, range);

        for(int i = 0; i < objects.Length; i++) {
            if (objects[i].tag.Equals("Ghost")){
                objects[i].GetComponent<GhostSenses>().NoiseHeard(point);
            }
        }
    }

    public void Phase(GameObject ghost) {
        if (ghost.GetComponentInParent<GhostMovement>().spawnTimer < 0) {
            phasedGhosts++;
            this.spawnedGhosts.Remove(ghost);
            Destroy(ghost);

            Debug.Log(ghost + " is phasing");
            if (this.spawnedGhosts.Count <= 0) {
                ChangeStatus("unaware", 0.0f);
            }
        }
    }

    public void SpawnBattery() {
        if(tables.Count > 0) {
            GameObject batteryInstance = Instantiate(batteryPrefab, tables[Random.Range(0, tables.Count)].transform.position, Quaternion.identity) as GameObject;
        }
    }

    //Check if any adjacent rooms have phased ghosts
    private Portal FindAdjacentPhasedGhost() {
        foreach(Portal portal in this.portals) {
            if(portal.thatRoom.phasedGhosts > 0) {
                return portal;
            }
        }

        return null;
    }

    //When the player enters the room, the player's current room is updated
    public void OnTriggerEnter(Collider col) {
        if(col.tag == "Player") {
            Debug.Log(col.name);
            col.GetComponentInParent<PlayerStatus>().currentRoom = this;
        }
    }

    //When the player enters the room, the player's current room is updated
    public void OnTriggerExit(Collider col) {
        if (col.tag == "Player") {
            if(spawnedGhosts.Count > 0) {
                Unaware();
                /*foreach(GameObject ghost in spawnedGhosts) {
                    spawnedGhosts.Remove(ghost);
                    Destroy(ghost);
                }
                phasedGhosts = phased;*/
            }
        }
    }

    //-----------Status Methods-------------------
    //Parse the status to our variables
    public void ParseStatus(out string statusType, out float statusTime, out float statusDuration)
    {
        statusType = status.Split(',')[0];
        statusTime = float.Parse(status.Split(',')[1]);
        statusDuration = float.Parse(status.Split(',')[2]);
    }

    //Change the room status
    private void ChangeStatus(string statusType, float statusDuration) {
        status = statusType.ToLower() + "," + Time.time + "," + statusDuration;
    }
}
