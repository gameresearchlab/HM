using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Breakable : MonoBehaviour {
    bool falling = false;
    bool playerActivated = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(GetComponent<Rigidbody>().velocity.magnitude > 1f || GetComponent<Rigidbody>().angularVelocity.magnitude > 1f) {
            //Debug.Log(gameObject.name+"/"+transform.parent.gameObject.name+"Sound: Velocity:"+ GetComponent<Rigidbody>().velocity.magnitude+", Angular:"+GetComponent<Rigidbody>().angularVelocity.magnitude);
            falling = true;
            MakeNoise();
        }
	}

    void OnCollisionEnter(Collision col)
    {
        
        if (col.gameObject.tag == "Player") {
            //Debug.Log("pLAYER COLLISION");
            playerActivated = true;

        }
    }

    public void MakeNoise() {
        //Get the status values
        string statusType;
        float statusTime, statusDuration;
        Room room = GameObject.Find("Player").GetComponent<PlayerStatus>().currentRoom;
        if(room != null) {
            room.ParseStatus(out statusType, out statusTime, out statusDuration);

            if (playerActivated && statusType.ToLower() == "unaware") {
                GetComponent<AudioSource>().Play();
                room.Noise(transform.position, GetComponent<Rigidbody>().mass * GetComponent<Rigidbody>().velocity.magnitude);
                falling = false;
                playerActivated = false;
            }
        }
    }
}
