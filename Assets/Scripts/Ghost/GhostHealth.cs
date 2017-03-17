﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/* Name: Mathew Tomberlin
* CST306
*/
public class GhostHealth : MonoBehaviour {
    //The magnitude of light that the Ghost must be exposed to in order to stun it. Must be between 0 and 1
    public float lightThreshold;
    //The amount of time that the Ghost must be held within the flashlight beam to be stunned
    public float maxHealth;
    private float health;
    private Material instanceMaterial;
    bool corporeal = true;
    bool visible = true;
    public AudioClip hitSound;
    public AudioClip dieSound;

	// Use this for initialization
	void Start () {
        instanceMaterial = new Material(GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial);
        GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = instanceMaterial;
        health = maxHealth;
	}

    public IEnumerator Blink() {
        corporeal = false;
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        //This is used for the duration of a given color transition effect
        float duration = 0.0f;
        //Get a reference to the particle emitter
        //GameObject particleEmitter = GetComponentInChildren<ParticleSystem>().gameObject;
        //Get the default emissive color
        //Color emissiveC = GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.GetColor("_EmissionColor");

        List<Color> originalColors = new List<Color>();

        foreach (SkinnedMeshRenderer render in renderers) {
            foreach (Material material in render.sharedMaterials) {
                originalColors.Add(material.GetColor("_Color"));
            }
        }

        //Transition the material color to white over 0.2 seconds
        duration = 0.2f;
        while (duration > 0.0f) {
            duration -= Time.deltaTime;

            foreach (SkinnedMeshRenderer render in renderers) {
                foreach (Material material in render.sharedMaterials) {
                    Material newMaterial = new Material(material);
                    Color currentEmissionColor = material.GetColor("_Color");
                    float currentIntensity = material.GetFloat("_EmissiveIntensity");
                    //Lerp the color toward white
                    newMaterial.SetFloat("_EmissiveIntensity", Mathf.Lerp(currentIntensity,1,Mathf.Pow(1-(duration/0.2f),4)));
                    newMaterial.SetColor("_Color", Color.Lerp(currentEmissionColor, Color.white * 1, Mathf.Pow(1 - (duration / 0.2f),4)));
                    render.sharedMaterial = newMaterial;
                }
            }
            yield return new WaitForEndOfFrame();
        }
        //Stop the senses
        GetComponent<GhostSenses>().enabled = false;
        //Stop the navigation
        GetComponent<NavMeshAgent>().Stop();
        //Disable the weapon
        GetComponent<GhostAttack>().meleeWeapon.GetComponent<BoxCollider>().enabled = false;
        //Disable rigidbody collisions
        GetComponent<Rigidbody>().detectCollisions = false;
        //Disable the capsule collider
        GetComponent<CapsuleCollider>().enabled = false;
        int counter = 0;
        foreach(SkinnedMeshRenderer render in renderers) {
            foreach (Material material in render.materials) {
                Material newMaterial = new Material(material);
                //Disable the mesh renderer
                //render.enabled = false;
                float currentIntensity = material.GetFloat("_EmissiveIntensity");
                newMaterial.SetFloat("_EmissiveIntensity", Mathf.Lerp(currentIntensity, (health/maxHealth)*0.04f, Mathf.Pow(1 - (duration / 0.2f), 4)));
                newMaterial.SetColor("_Color", originalColors[counter]);
                render.sharedMaterial = newMaterial;
                counter++;
            }
        }
        //Disable the particle system
        //particleEmitter.SetActive(false);
        //Set wait duration to 0.05 seconds
        visible = false;

        duration = 0.05f;
        while (duration > 0.0f) {
            duration -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        //We're going to do a NavMesh.Raycast (which tells if we can move somewhere), starting 0.75 units to the right and trying again at increments of 0.25 units until we're raycast out to 0.75 units to the left
        //When we find an open spot, we're going to suddenly move there (Blink there)

        //Either transform.left or transform.right, based on the value generated by the ternary statement involving Random.Range(0,100)
        Vector3 randDir = transform.right * ((Random.Range(0, 100) < 50) ? -1 : 1);
        //Starting 0.75 units in randDir and trying again at increments of 0.25 units in -randDir...
        for (float i = 2f; i >= -2f; i -= 0.5f) {
            //Raycast to a point i units in randDir from this Ghost and if it's a clear path...
            NavMeshHit navHit;
            if (i != 0 && !NavMesh.Raycast(transform.position, transform.position - (randDir) * i, out navHit, NavMesh.AllAreas)) {
                //Move the Ghost there (Blink!)
                transform.position = transform.position - randDir * i;
                break;
            }
        }

        //Set the wait duration to 0.05 seconds
        duration = 0.05f;
        while (duration > 0.0f) {
            duration -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        //Stop the senses
        GetComponent<GhostSenses>().enabled = true;
        //Enable navigation
        GetComponent<NavMeshAgent>().Resume();
        //Enable the melee weapon collider
        GetComponent<GhostAttack>().meleeWeapon.GetComponent<BoxCollider>().enabled = true;
        //Enable rigidbody collisions
        GetComponent<Rigidbody>().detectCollisions = true;
        //Enable the capsule collider
        GetComponent<CapsuleCollider>().enabled = true;
        foreach (SkinnedMeshRenderer render in renderers) {
            //Disable the mesh renderer
            render.enabled = true;
        }
        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>()) {
            foreach (Material material in smr.materials) {
                Material newMaterial = new Material(material);
                newMaterial.SetFloat("_EmissiveIntensity", (health / maxHealth)*0.04f);
                smr.sharedMaterial = newMaterial;
                //material.color = new Color(material.color.r, material.color.g, material.color.b, Mathf.Max(0.2f, health / maxHealth));
            }
        }
        //Enable the particle system
        //particleEmitter.SetActive(true);
        visible = true;
        //Set the wait duration to 0.5 seconds to give it a second before being affected by the light
        duration = 0.5f;
        while (duration > 0.0f)
        {
            duration -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //It's corporeal again
        corporeal = true;
    }

    public IEnumerator Die()
    {
        GameObject.Find("Player").GetComponent<PlayerStatus>().currentRoom.spawnedGhosts.Remove(gameObject);
        if (GameObject.Find("Player").GetComponent<PlayerStatus>().currentRoom.spawnedGhosts.Count <= 0) {
            Debug.Log("Last Ghost Killed");
            if (GameObject.Find("Player").GetComponent<PlayerStatus>().currentRoom.phasedGhosts <= 0) {
                //GameObject.Find("Player").GetComponent<PlayerStatus>().currentRoom.phasedGhosts = GameObject.Find("Player").GetComponent<PlayerStatus>().currentRoom.phased;
            }
        }

        GetComponent<AudioSource>().PlayOneShot(dieSound);
        GetComponent<GhostSenses>().enabled = false;
        StartCoroutine(Blink());
        while (visible) {
            yield return new WaitForEndOfFrame();
        }
        while (!visible)
        {
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

    //Magnitude is a number between 0 and 1
    public void Hit(float magnitude) {
        if (health > 0 && magnitude > lightThreshold && corporeal) {
            health -= 25;
            GetComponent<GhostSenses>().Alert(GameObject.Find("Player").transform.position);
            StartCoroutine(Blink());
            GetComponent<AudioSource>().PlayOneShot(hitSound);
            
            if (health <= 0) {
                StartCoroutine(Die());
            }
        }
    }

    //Magnitude is a number between 0 and 1
    public void MoonlightHit(float magnitude) {
        if (health > 0 && magnitude > lightThreshold && corporeal) {
            health -= 25;
            GetComponent<GhostSenses>().Alert(GameObject.Find("Player").transform.position);
            StartCoroutine(Blink());
            GetComponent<AudioSource>().PlayOneShot(hitSound);

            if (health <= 0) {
                StartCoroutine(Die());
            }
        }
    }

    //Reset current health to maxHealth
    public void Recover() {
        health = maxHealth;
    }
}