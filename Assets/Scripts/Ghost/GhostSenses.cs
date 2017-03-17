using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/* Name: Mathew Tomberlin
 * CST306
 */
public class GhostSenses : MonoBehaviour {
    //Determines intelligence of Ghost.
    //Ghost spawns may be triggered by noises, random interval, fixed interval, Ghost alert, Player event (Floor Change, Door Open), scripted
    /* Using the below variables, one can create Ghosts with the following attributes:
     * Ghost spawns at spawn point and remains there until it sees or hears the player. 
     *     It cannot move or leave the room, but can attack if in range.
     *     
     * Ghost spawns at spawn point and remains there until it sees or hears the player. 
     *     It can move and attack, but cannot leave the room.
     *     
     * Ghost spawns at spawn point and remains there until it sees or hears the player. It can move and attack. 
     *     It can pursue the player to other rooms using portals, but returns to its spawn point if it loses track of the player. It may unspawn if this happens.
     *     
     * Ghost spawns at spawn point and wanders aimlessly around room. 
     *     It can move and attack, but cannot leave the room.
     *     
     * Ghost spawns at spawn point and patrols a route, stopping to look around at each waypoint. 
     *     It can move and attack, but cannot leave the room.
     *     
     * Ghost spawns at spawn point and patrols a route, stopping to look around at each waypoint. It can move and attack. 
     *     It can pursue the player to other rooms using portals, but returns to its last waypoint if it loses track of the player.
     * 
     */
     //Triggers
     //Should the ghost spawn on a noise?
    //public bool noiseActivated;
    //public Vector3 noisePosition;
    //Should the ghosts spawn at a random interval?
    //public bool randomInterval;
    //public float fixedInterval; //If fixedInterval > 0
    //public bool ghostAlert; //Ghost is spawned if any other ghost in the room is alerted
    //public string playerEventActivated = "NULL"; //If not NULL, ghost spawned by named event

    //Shy - Ghost only moves when player's back is turned or the flashlight is out, runs and hides behind nearest cover otherwise. Ghost blinks laterally+backward when exposed to light.
    //Shy ghost spawn at most distant portal available from player OR closest portal the player cannot see
    //Bold - Ghost moves toward player at all times. Ghost blinks laterally+forward, but slowly, when exposed to light.
    //Bold ghosts spawn as close to the player as possible
    //Agile - Ghost moves toward player when back is turned or the flashlight is out, tries to move side to side otherwise, moves forward if it cannot. Ghost blinks laterally when expost to light.
    //Agile ghosts spawn at the most distant portal available from the player OR closest portal the player cannot see
    public string personality;

    //If the ghost loses track of player, go in wall. Otherwise, ghost will either walk randomly in room or go back to spawn point
    public bool hideOnLost;
    //Ghost boundaries
    //If true, ghost cannot move from spawnPoint. The spawn point cannot be a portal, unless hideOnLost is true.
    public bool boundToSpawnPoint;
    //If true, ghost cannot leave spawn room. Instead, if player leaves spawn room, ghost wanders randomly or goes in wall.
    public bool boundToSpawnRoom;
    //If true, ghost returns to patrolling from its last waypoint if the player is lost
    public bool boundToSpawnRoute;

    public Transform spawnPoint;
    public int lastWaypoint;
    public int nextWaypoint;
    public List<GameObject> route = new List<GameObject>();

    //How far the ghost can move from its spawn point, spawn room or last patrol point
    public float leashDistance;
    //How far in rooms the ghost can move from its spawn point, spawn room or last patrol point
    public int roomDistance;

    //Maximum health before the Ghost is destroyed and must reform at its spawnPoint (if invincible)
    public float maxHealth = 100;
    //Current health
    public float health;
    public bool invincible; //TODO: if true, Ghost respawns at spawn point after respawn cooldown with full health. If false, Ghost is destroyed.

    public Transform target; //Should be set when the Ghost spawns and then whenever the focus of their attention changes

    //Greater values are more processor consuming (unless you change the interval checked in OnAttackEnd()
    public int maxWithdrawlDistance = 3;

    public string status; //Represents what state the Ghost is in

    public float moveSpeed = 2;

    public GhostSight sight;

    private GameObject player;

    /*  //If a ghost inherits a status from another ghost, the time and duration are also inherited.
        //noise alert - ghost heard noise, not already on alert [I.E. seen player recently]
        //              ghost moves toward noise, looks around and then goes off noise alert. May unspawn.
        //suspicion - ghost believes someone is in the room, but does not know where. Triggered if player makes noise when ghost is in noise alert or if nearby ghost is suspicious.
        //            ghost checks closets, behind curtains, etc., all hiding spots, and then moves through room waypoints until suspicion timer runs out.
        //alert - ghost knows the player's exact last position within the room. Triggered if ghost recently saw player or was alerted by nearby ghost who saw player.
        //        ghost moves to last position and attacks player, goes to suspicion status if player is no longer there.
        //engaged - ghost is currently confronting player according to personality. Shy ghosts use cover, bold ghosts simply advance and agile ghosts dodge forward.
        //          ghost continues to attempt to attack player until the player either flee them or they lose track of the player
        //attacking - ghost is currently in the middle of attacking the player
        //            if the ghost sustains enough damage from the flashlight while attacking, they are stunned
        //fleeing - ghost has sustained maxHealth/fleeThreshold damage and is fleeing to the walls. The ghost cannot be harmed and moves quickly. May simply vanish in to the floor.
        //         ghost moves directly toward the portal and enters it. If the ghost is (invincible and) shy, it remains in another room until its health regenerates, before returning to the last known
        //         player position (A position seen by a ghost, not heard). If the ghost is bold or agile and its health is > 50, it reappears from a different portal near the player and attacks again.
        //         ghost cannot inherit status effects while broken.
        //stunned - ghost temporarily cannot move or attack. It cannot be damaged, but cannot see or hear the player.
     * Idle --[OnDestinationReached]--> (hideOnIdle)? Hide : (boundToSpawnPoint)? Idle : (bountToSpawnRoom)? Wander : (boundToSpawnRoute)? Patrol : WanderFloor
     * 
     * From: Idle
     * XNoiseAlert --[OnDestinationReached]--> Idle[ToPortal/Spawn]
     * XNoiseAlert --[OnTargetSpotted]--> Engaged
     * XNoiseAlert --[OnNoiseHeard]--> Suspicion[10.0f]
     * XNoiseAlert --[OnAlerted]--> Alert[10.0f]
     * XNoiseAlert --[OnStunned]--> Stunned
     * 
     * From: NoiseAlert, Alert
     * XSuspicion --[OnDestinationReached]--> FindNewRandomDestination[Hiding/Random]
     * XSuspicion --[OnSuspicionExpired]--> Idle --> Hide
     * XSuspicion --[OnTargetSpotted]--> Engaged
     * XSuspicion --[OnNoiseHeard]--> Suspicion[10.0f][ToNoise]
     * XSuspicion --[OnAlerted]--> Alert[10.0f][ToNoise]
     * XSuspicion --[OnStunned]--> Stunned
     * 
     * From: Suspicion, Engaged, NoiseAlert
     * XAlert --[OnDestinationReached]--> Suspicion[10.0f]
     * XAlert --[OnAlertExpired]--> Suspicion[10.0f]
     * XAlert --[OnTargetSpotted]--> Engaged
     * XAlert --[OnNoiseHeard]--> Alert[ToNoise]
     * XAlert --[OnAlerted]--> Alert[ToNoise]
     * XAlert --[OnStunned]--> Stunned
     * 
     * From: Alert, Suspicion, NoiseAlert
     * Engaged --[OnAttackRange]--> Attack[AtTarget]
     * XEngaged --[OnStunned]--> Stunned
     * XEngaged --[OnTargetLost]--> Alert[10.0f]
     * 
     * Attacking --[OnAttackMiss]--> (personality=="shy")? MoveToCover : (personality==bold)? StepBack : DodgeToSide
     * Attacking ==[OnAttackHit]--> Player.RestartAtCheckpoint
     * XAttacking --[OnStunned]--> Stunned
     * 
     * XFleeing --[OnDestinationReached]--> Idle
     * 
     * XStunned --[OnStunnedExpired]--> Suspicion[10.0f]
     */

    public void Start() {
        player = GameObject.Find("Player");
    }

    public void Awaken(string status, float duration) {
        ChangeStatus(status, duration);
    }

    //When a target has been physically seen
    public void OnTargetSpotted(GameObject target) {
        //Get the status values
        string statusType;
        float statusTime, statusDuration;
        ParseStatus(out statusType, out statusTime, out statusDuration);

        switch (statusType) {
            case "noisealert":
                Engaged(target);
                break;
            case "suspicion":
                Engaged(target);
                break;
            case "alert":
                Engaged(target);
                break;
        }
    }

    //After a small delay, if the target is not in sight, it is lost and the AI goes to alerted
    public void OnTargetLost(Vector3 lastPosition) {
        //Get the status values
        string statusType;
        float statusTime, statusDuration;
        ParseStatus(out statusType, out statusTime, out statusDuration);
        
        switch (statusType)
        {
            case "engaged":
                Alert(lastPosition);
                break;
            case "attacking":
                Alert(lastPosition);
                break;
        }
    }

    //When the duration of a status expired (Suspicion, Alert, Stunned)
    public void OnStatusExpired(string statusType) {
        switch (statusType) {
            case "suspicion":
                Idle();
                break;
            case "alert":
                Suspicion(transform.position);
                break;
            case "stunned":
                Alert(transform.position);
                break;
            case "engaged":
                PathfindToPoint(target.transform.position - (transform.position - target.transform.position).normalized * 0.5f);
                break;
        }
    }

    //When the AI has reached its destination (NoiseAlert, Suspicion, Alert)
    public void OnDestinationReached(Vector3 point) {
        //Get the status values
        string statusType;
        float statusTime, statusDuration;
        ParseStatus(out statusType, out statusTime, out statusDuration);

        //Debug.Log(statusType);
        switch (statusType) {
            case "idle":
                Debug.Log("Idle");
                if (route.Count > 2) {
                    lastWaypoint = nextWaypoint;
                    
                    nextWaypoint = (nextWaypoint < route.Count-1)? nextWaypoint+1 : 0;

                    GetComponent<GhostMovement>().SetMoveTarget(route[nextWaypoint].transform.position);
                } else if (boundToSpawnRoom) {
                    FindRandomMoveTarget(spawnPoint.position);
                }
                break;
            case "noisealert":
                StartCoroutine(PerformSpotCheck(Random.Range(5.0f,6.0f)));
                break;
            case "suspicion":
                StartCoroutine(PerformSpotCheck(Random.Range(5.0f, 6.0f)));
                break;
            case "alert":
                StartCoroutine(PerformSpotCheck(Random.Range(9.0f, 12.0f)));
                break;
            case "fleeing":
                if (this.target != null && target.GetComponent<Portal>().thatRoom != null) {
                    target.GetComponent<Portal>().thatRoom.Phase(gameObject);
                } else {
                    Idle();
                }
                break;
            case "engaged":
                PathfindToPoint(target.transform.position - (transform.position - target.transform.position).normalized * 0.5f);
                break;
        }
    }

    public void OnSpotCheckEnded() {
        // Get the status values
        string statusType;
        float statusTime, statusDuration;
        ParseStatus(out statusType, out statusTime, out statusDuration);
        GetComponent<NavMeshAgent>().Resume();
        switch (statusType) {
            case "noisealert":
                Idle();
                break;
            case "suspicion":
                FindRandomMoveTarget(transform.position);
                break;
            case "alert":
                Suspicion(transform.position);
                break;
        }
    }

    //If the status has expired, 
    void Update () {
        //Get the status values
        string statusType;
        float statusTime, statusDuration;
        ParseStatus(out statusType, out statusTime, out statusDuration);

        if (Time.time - statusTime > statusDuration) {
            OnStatusExpired(statusType);
        }

        if (target != null)
        {
            //transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position, Vector3.up);
            if (Vector3.Distance(transform.position, target.transform.position) < 3.0f)
            {
                GetComponent<GhostMovement>().SetMoveTarget(transform.position-(target.transform.position-transform.position).normalized);
            }
        }

        if (statusType.Equals("engaged") ) {
            Vector3 targetPos = new Vector3(target.transform.position.x,0, target.transform.position.z);
            Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);
            transform.rotation = Quaternion.LookRotation(targetPos - pos, Vector3.up);
            //Debug.Log("Engaged: "+Vector3.Distance(transform.position, target.transform.position));
            if (Vector3.Distance(transform.position, target.transform.position) < GetComponent<GhostAttack>().range && sight.targetInSight)
            {
                Debug.Log("Attack!");
                Attack();
            }
        }
    }

    //---------------------State Methods-------------------------------
    //Depending on the bind flags, either hide, move to spawnPoint, wander inside room or resume the route
    public void Idle() {
        ChangeStatus("idle", 0.0f);

        if (hideOnLost) {
            Flee();
        } else if (boundToSpawnPoint) {
            GetComponent<GhostMovement>().SetMoveTarget(spawnPoint.position);
        } else if (boundToSpawnRoom) {
            FindRandomMoveTarget(spawnPoint.position);
        } else if(boundToSpawnRoute) {
            ResumeRoute();
        } else {
            FindRandomMoveRoom();
        }
    }

    //Change the status to noiseHeard and move the AI to the point to check it out
    public void NoiseHeard(Vector3 point)
    {
        if(status != "") {
            //Get the status values
            string statusType;
            float statusTime, statusDuration;
            ParseStatus(out statusType, out statusTime, out statusDuration);

            switch (statusType)
            {
                case "noisealert":
                    ChangeStatus("noisealert", 0.0f);
                    GetComponent<GhostMovement>().SetMoveTarget(point);
                    break;
                case "suspicion":
                    ChangeStatus("noisealert", 0.0f);
                    GetComponent<GhostMovement>().SetMoveTarget(point);
                    break;
                case "alert":
                    ChangeStatus("noisealert", 0.0f);
                    GetComponent<GhostMovement>().SetMoveTarget(point);
                    break;
            }
        } else {
            ChangeStatus("noisealert", 0.0f);
            GetComponent<GhostMovement>().SetMoveTarget(point);
        }
    }

    //Move to and open any curtains/closets in the room, or wander around the room randomly
    public void Suspicion(Vector3 point) {
        ChangeStatus("suspicion", 10.0f);
        target = null;
        FindRandomMoveTarget(point);
    }

    //The AI know's the player's last spotted position and moves toward it until
    //the alert expires or it reaches the position and the player is either spotted or not
    public void Alert(Vector3 pos) {
        ChangeStatus("alert", 10.0f);
        GetComponent<GhostMovement>().SetMoveTarget(pos);
        if (player != null) {
            //Debug.Log("Ghost alerted!");
            player.GetComponent<PlayerStatus>().currentRoom.Aware();
        }
    }

    //Depending on the personality, move toward the target to attack
    public void Engaged(GameObject target) {
        //Get the status values
        string statusType;
        float statusTime, statusDuration;
        ParseStatus(out statusType, out statusTime, out statusDuration);
        GhostAttack ghostAttack = GetComponent<GhostAttack>();
        if (sight.targetInSight && !ghostAttack.maneuvering && !ghostAttack.attacking) {
            ChangeStatus("engaged", 1.0f);
            //GetComponent<GhostSenses>().Alert(GameObject.Find("Player").transform.position);
            this.target = target.transform;
            Vector3 dir = (transform.position - target.transform.position);
            Vector3 pos = target.transform.position - dir.normalized*0.5f;
            //NavMesh.Raycast checks if the given point is on the navmesh (Giving the same point twice checks it against itself)
            NavMeshHit navHit;
            if (NavMesh.Raycast(transform.position, pos, out navHit, NavMesh.AllAreas)) {
                pos = target.transform.position;
            }

            GetComponent<GhostMovement>().SetMoveTarget(pos);
        }
    }

    //The AI faces the target and then attacks
    public void Attack() {
        //Get the status values
        string statusType;
        float statusTime, statusDuration;
        ParseStatus(out statusType, out statusTime, out statusDuration);
        GhostAttack ghostAttack = GetComponent<GhostAttack>();

        if (!statusType.Equals("attacking") && Time.time - ghostAttack.lastAttackTime > ghostAttack.cooldown && !ghostAttack.maneuvering && !ghostAttack.attacking) {
            Debug.Log("Attacking");
            ChangeStatus("attacking", 0.0f);
            GetComponent<GhostAttack>().target = this.target.gameObject;
            ghostAttack.StartCoroutine(ghostAttack.TryAttack());
        }
    }

    //Back up as far as possible on a missed hit
    public void OnAttackEnd()
    {
        ChangeStatus("engaged", 1.0f);
        NavMeshHit navHit;
        for (int i = maxWithdrawlDistance; i >= 0; i--) {
            Vector3 point = transform.position - (transform.forward * i);
            //NavMesh.Raycast checks if the given point is on the navmesh (Giving the same point twice checks it against itself)
            if (!NavMesh.Raycast(transform.position, point, out navHit, NavMesh.AllAreas)) {
                //Debug.Log("On navmesh");
                GetComponent<GhostMovement>().SetMoveTarget(point);
                return;
            } else
            {
                GetComponent<GhostMovement>().SetMoveTarget(transform.position);
            }
        }
    }

    //The AI has been weakened during an attack and cannot move for the status duration
    public void Stun() {
        ChangeStatus("stunned", 3.0f);
        GetComponent<GhostMovement>().Stun();
    }

    //The AI moves toward a portal
    public void Flee() {
        ChangeStatus("fleeing", 0);
        GetComponent<GhostMovement>().Hide();
    }

    //-----------------Movement Methods--------------------------
    //Go back to the last waypoint
    public void ResumeRoute() {
        GetComponent<GhostMovement>().SetMoveTarget(route[lastWaypoint].transform.position);
    }

    //Find a random move target within the room
    public void FindRandomMoveTarget(Vector3 point) {
        bool aboveFloor = false;
        while (!aboveFloor) {
            Vector3 randomPoint = point + Random.insideUnitSphere*5;
            randomPoint.y = Mathf.Max(0.0001f, randomPoint.y);

            RaycastHit hit;
            if (Physics.Raycast(randomPoint, Vector3.down, out hit)) {
                randomPoint = hit.point;
                target = null;
                GetComponent<GhostMovement>().SetMoveTarget(randomPoint);
                aboveFloor = true;
            }
        }
    }

    //Get a random room, then find a random point within it to move to
    public void FindRandomMoveRoom() {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        Vector3 point = gm.currentFloor.rooms[Random.Range(0, gm.currentFloor.rooms.Count)].transform.position;

        Vector3 randomPoint = point + Random.insideUnitSphere*5;
        randomPoint.y = Mathf.Max(0.0001f, randomPoint.y);

        RaycastHit hit;
        if (Physics.Raycast(randomPoint, Vector3.down, out hit, 10, gm.getGroundLayer())) {
            randomPoint = hit.point;
            target = null;
            PathfindToPoint(randomPoint);
        }
    }

    //Move along a path toward the target, facing along the path
    public void PathfindToPoint(Vector3 point) {
        if(GetComponent<GhostMovement>().target != point) {
            GetComponent<GhostMovement>().SetMoveTarget(point);
        }
    }

    public IEnumerator PerformSpotCheck(float duration)
    {
        //Stop moving
        GetComponent<NavMeshAgent>().Stop();
        GetComponent<GhostMovement>().SetMoveTarget(Vector3.zero);

        //Get the Vector3 pointing to this ghost's right and the Quaternion that rotates us in that heading
        Vector3 lookDir = (Random.Range(0, 100) > 50) ? transform.right : -transform.right;
        Quaternion lookRotation = Quaternion.LookRotation(lookDir, Vector3.up);
        //The ghost will continue to look around until this timer runs out or the coroutine is cancelled
        float timer = duration;

        //While timer is still running...
        while (timer > 0) {
            //Until we're facing in the look direction...
            while (Vector3.Angle(transform.forward, lookDir) > 5) {
                //Turn toward the current look rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime);
                //Decrease timer
                timer -= Time.deltaTime;
                //Yield until the next frame
                yield return new WaitForEndOfFrame();
            }
            //Once we've looked in one direction, look in the opposite
            lookDir *= -1;
            lookRotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }

        OnSpotCheckEnded();
    }

    public IEnumerator Perform360SpotCheck(float duration) {
        //Stop moving
        GetComponent<NavMeshAgent>().Stop();
        GetComponent<GhostMovement>().SetMoveTarget(Vector3.zero);

        //Get the Vector3 pointing to this ghost's right and the Quaternion that rotates us in that heading
        Vector3 lookDir = -transform.forward;
        Quaternion lookRotation = Quaternion.LookRotation(lookDir, Vector3.up);
        //The ghost will continue to look around until this timer runs out or the coroutine is cancelled
        float timer = duration;

        //While timer is still running...
        while (timer > 0)
        {
            //Until we're facing in the look direction...
            while (Vector3.Angle(transform.forward, lookDir) > 5)
            {
                //Turn toward the current look rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime);
                //Decrease timer
                timer -= Time.deltaTime;
                //Yield until the next frame
                yield return new WaitForEndOfFrame();
            }
            //Once we've looked in one direction, look in the opposite
            lookDir *= -1;
            lookRotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }

        OnSpotCheckEnded();
    }

    //------------------Getter and Setter for the status------------------
    //Change the AI's status to statusType, with the currentTime and duration of the status
    private void ChangeStatus(string statusType, float statusDuration) {
        if (GetComponent<NavMeshAgent>() != null) {
            GetComponent<NavMeshAgent>().Resume();
        }
        StopAllCoroutines();
        status = statusType.ToLower() + "," + Time.time + "," + statusDuration;
    }

    //Parse the status to our variables
    private void ParseStatus(out string statusType, out float statusTime, out float statusDuration) {
        statusType =  status.Split(',')[0];
        statusTime = float.Parse(status.Split(',')[1]);
        statusDuration = float.Parse(status.Split(',')[2]);
    }
}
