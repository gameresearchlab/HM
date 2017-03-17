using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using UnityEngine.SceneManagement;
/* Name: Mathew Tomberlin
 * CST306
 */
public class PlayerHealth : MonoBehaviour {
    private float currentHealth;
    public float maxHealth;
    public float lastHitTime = 0.0f;
    private bool regenerating = false;
    public VignetteAndChromaticAberration vignette_chromatic;
    public AudioClip hitSound;

    // Use this for initialization
    void Start () {
        vignette_chromatic = GetComponentInChildren<VignetteAndChromaticAberration>();
        currentHealth = maxHealth;
	}

    void Update() {
        if(!regenerating && currentHealth != maxHealth && Time.time - lastHitTime > 10.0f) {
            Debug.Log("Regenerating");
            StartCoroutine(Regen());
        }
    }

    public void Hit(float damage) {
        lastHitTime = Time.time;
        StopCoroutine(Regen());
        regenerating = false;
        GetComponent<AudioSource>().clip = hitSound;
        GetComponent<AudioSource>().Play();
        if (currentHealth - damage > 0) {
            currentHealth -= damage;
            vignette_chromatic.intensity = Mathf.Min(0.4f, (1 - (currentHealth / maxHealth))/2);
            vignette_chromatic.blur = 1 - (currentHealth / maxHealth);
            vignette_chromatic.blurDistance = 1 - (currentHealth / maxHealth);
        } else {
            StartCoroutine(Die());
        }
    }

    public IEnumerator Regen() {
        regenerating = true;
        float duration = 3.0f;
        while (duration > 0) {
            duration -= Time.deltaTime;
            currentHealth = Mathf.Lerp(currentHealth, maxHealth, Time.deltaTime);
            vignette_chromatic.blur = Mathf.Lerp(vignette_chromatic.blur, 0, Time.deltaTime);
            vignette_chromatic.intensity = Mathf.Lerp(vignette_chromatic.intensity, 0.2f, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        vignette_chromatic.intensity = 0.2f;
        vignette_chromatic.blur = 0.0f;
        currentHealth = 100.0f;
        regenerating = false;
    }

    public IEnumerator Die() {
        float duration = 3.0f;
        foreach(Transform child in transform) {
             if(child.GetComponent<BoxCollider>() != null) {
                child.GetComponent<BoxCollider>().enabled = false;
            }
        }
        GetComponent<CharacterController>().enabled = false;
        GetComponent<PlayerMovement>().enabled = false;

        while (duration > 0) {
            duration -= Time.deltaTime;
            vignette_chromatic.blur = Mathf.Lerp(vignette_chromatic.blur, 1, Time.deltaTime);
            vignette_chromatic.intensity = Mathf.Lerp(vignette_chromatic.intensity, 1, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        vignette_chromatic.intensity = 1.0f;
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
