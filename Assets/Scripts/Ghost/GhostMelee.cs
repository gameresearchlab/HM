using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/* Name: Mathew Tomberlin
 * CST306
 */
public class GhostMelee : MonoBehaviour {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            col.gameObject.GetComponentInParent<PlayerHealth>().Hit(10);
            //StartCoroutine(AttackHitting());
            //GetComponentInParent<Animator>().speed = 0;
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public IEnumerator AttackHitting()
    {
        float duration = 4.0f;
        GetComponentInParent<Animator>().speed = 0.1f;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        GetComponentInParent<Animator>().speed = 1f;
    }
}
