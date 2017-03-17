using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
/* Name: Mathew Tomberlin
 * CST306
 */
public class GhostAttack : MonoBehaviour {
    public float range; //The range of the melee attack
    public GameObject meleeWeapon; //An object with a collider to check collision during ghost's attack animation
    public bool maneuvering; //If the ghost is in range and checking for LOS, this is true
    public bool attacking; //If the ghost is playing the attack animation, this is true
    public GameObject target; //Set by GhostSenses when the Ghost begins trying to attack and is set to null when they finish trying to attack
    public LayerMask targettingLayer;
    public Transform eyeLine;
    public float cooldown;
    public float lastAttackTime;
    public AudioClip attackSound;

    public void Start()
    {
        lastAttackTime = Time.time;
    }

    //Conditions for attacking:
    //  Ghost must still be targetting player
    //  Ghost must be within LOS of the player
    //  Ghost must be facing the player
    public IEnumerator TryAttack() {
        float tryAttackTime = 5.0f;
        //Debug.Log("TryAttackOutside");
        while(target != null && tryAttackTime > 0) {
            //Debug.Log("TryAttackInside");
            tryAttackTime -= Time.deltaTime;
            maneuvering = true;

            RaycastHit hit;
            //Vector3 targetBody = new Vector3(target.transform.position.x, eyeLine.transform.position.y, target.transform.position.z);
            //Debug.DrawRay(eyeLine.transform.position, targetBody - eyeLine.transform.position, Color.red, 1.0f);
            //Debug.DrawRay(eyeLine.transform.position, eyeLine.transform.up, Color.green, 1.0f);
            if (Physics.Raycast(eyeLine.transform.position, new Vector3(target.transform.position.x,eyeLine.transform.position.y,target.transform.position.z) - eyeLine.transform.position, out hit, range, targettingLayer)) {
                
                //Debug.Log("Target in LOS, facing: "+ Vector3.Angle(eyeLine.transform.forward, new Vector3(target.transform.position.x, eyeLine.transform.position.y, target.transform.position.z) - eyeLine.transform.position));
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(target.transform.position.x, eyeLine.transform.position.y, target.transform.position.z) - eyeLine.transform.position, Vector3.up), 10*Time.deltaTime);
                //Debug.Log("Angle: " + Vector3.Angle(transform.transform.forward, new Vector3(target.transform.position.x, eyeLine.transform.position.y, target.transform.position.z) - eyeLine.transform.position));
                if(Vector3.Angle(transform.transform.forward, new Vector3(target.transform.position.x, eyeLine.transform.position.y, target.transform.position.z) - eyeLine.transform.position) <= 15) {
                    GetComponentInParent<NavMeshAgent>().Stop();
                    lastAttackTime = Time.time;

                    //The attacking bool is set to true here and will be set to false by the same event in the animation that enabled/disables the weapon collider
                    attacking = true;
                    maneuvering = false;
                    GetComponent<AudioSource>().PlayOneShot(attackSound);
                    GetComponentInChildren<Animator>().SetTrigger("attack");
                    GetComponent<GhostSenses>().Engaged(target);
                    meleeWeapon.GetComponent<BoxCollider>().enabled = true;
                    yield break;
                }
            }
            maneuvering = false;
            yield return new WaitForEndOfFrame();
        }
        attacking = false;
        maneuvering = false;
        GetComponentInParent<GhostSenses>().Engaged(target);
    }

    //Called by an animation event
    public void AttackEnd() {
        attacking = false;
        maneuvering = false;
        meleeWeapon.GetComponent<BoxCollider>().enabled = false;
        GetComponent<NavMeshAgent>().Resume();
        GetComponent<GhostSenses>().OnAttackEnd();
    }
}
