using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class GhostMovement : MonoBehaviour {
    //Move target
    public Vector3 target;
    //How close the AI needs to be to target to be considered reached
    public float destinationThreshold = 0.25f;
    //A short delay so that a spawned ghost doesn't simply re-enter a portal
    public float spawnTimer;

    public AudioClip leftFootstep;
    public AudioClip rightFootstep;

    public void Start() {
        spawnTimer = 1.0f;
    }

    public void Update() {
        spawnTimer -= Time.deltaTime;

        Vector3 footPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetFootPos = new Vector3(target.x, 0, target.z);
        
        if(Vector3.Distance(footPos,targetFootPos) <= destinationThreshold) {
            GetComponent<GhostSenses>().OnDestinationReached(target);
        }
    }

    //Stop AI movement when stunned
    public void Stun() {
        GetComponent<NavMeshAgent>().Stop();
    }

    //Set the move target to a nearby portal
    public void Hide() {
        Debug.Log("Hiding");

        float closest = Mathf.Infinity;
        Transform closestPortal = null;
        Collider[] objects = Physics.OverlapSphere(transform.position, 30.0f);
        foreach(Collider obj in objects) {
            if (obj.GetComponent<Portal>()) {
                if(Vector3.Distance(transform.position,obj.transform.position) < closest)
                {
                    closestPortal = obj.transform;
                    closest = Vector3.Distance(transform.position, obj.transform.position);
                }
            }
        }
        GetComponent<GhostSenses>().target = closestPortal;
        SetMoveTarget(closestPortal.transform.position);
    }

    public void LeftFootstep() {
        GetComponent<AudioSource>().PlayOneShot(leftFootstep);
    }

    public void RightFootstep() {
        GetComponent<AudioSource>().PlayOneShot(rightFootstep);
    }

    //Whenever the AI recovers from being stunned
    public void Recover()
    {
        GetComponent<NavMeshAgent>().Resume();
    }

    //Set the NavAgent destination to the new move target
    public void SetMoveTarget(Vector3 target) {
        this.target = target;
        if (GetComponent<NavMeshAgent>() != null &&  this.target != Vector3.zero) {
            GetComponent<NavMeshAgent>().SetDestination(target);
        }
    }
}
