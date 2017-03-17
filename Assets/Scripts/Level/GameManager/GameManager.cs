using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/* Name: Mathew Tomberlin
 * CST306
 */
public class GameManager : MonoBehaviour {
    public List<Floor> floors = new List<Floor>();
    public Floor currentFloor;
    public LayerMask groundLayer;

    public LayerMask getGroundLayer() {
        return groundLayer;
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
