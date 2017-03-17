using UnityEngine;
using System.Collections;
using Leap.Unity;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Flashlight : MonoBehaviour {
    //The LightCone object
    public GameObject lightCone;
    //The Flashlight's current status
    public string status;
    //The Flashlight's current power
    public float power = 100;
    //The force required when striking palms together to repair the flashlight
    public float hitStrengthThreshold = 0.0075f;

    //The last time the flashlight blinked on or off
    private float lastBlink;
    //The starting blink interval
    public float blinkInterval;
    //The current blink interval.
    //Each time the light blinks out, this equals blinkInterval * %ofremainingPower, or 50%, whichever is greater
    private float currentBlinkInterval;


    //The power light of the flashlight
    public GameObject flashlightLight;
    //The palm gripping the flashlight
    public GameObject flashlightHand;

    public LayerMask flashlightLayer;

    public bool lightFlickering;

    float onIntensity;

    void Start() {
        lastBlink = Time.time;
        currentBlinkInterval = blinkInterval;
        this.power = 100.0f;
        onIntensity = lightCone.GetComponent<Light>().intensity;
        TurnOn();
    }

    void Update() {

        //While the flashlight is on (also not malfunctioned), decrease its power
        if (this.status.Equals("on")) {
            this.power -= (Time.deltaTime * 0.5f);
            lightCone.GetComponent<Light>().intensity = onIntensity; ;
            //If the power runs out, turn off the flashlight
            if (this.power <= 0) {
                ChangeStatus("off");
                GameObject.Find("Player").GetComponent<PlayerStatus>().currentRoom.SpawnBattery();
                lightCone.SetActive(false);
                flashlightLight.GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", Color.black);
                StopCoroutine(BatteryLightFlicker());
                StopCoroutine(BatteryLightFlickerMalfunction());
            } else {
                lightCone.SetActive(true);
                //If the power is still on & it's time to blink, try to blink
                //Also, reset the lastBlink time to now
                if (Time.time - lastBlink > currentBlinkInterval)
                {
                    lastBlink = Time.time;
                    TryBlink();
                }
            }
        } else if (this.status.Equals("off")) {
            if (this.power > 0 && !Physics.Raycast(lightCone.transform.position, lightCone.transform.forward, 1f, flashlightLayer)) {
                TurnOn();
            }
        }
    }

    public void ChangeStatus(string status) {
        if(!this.status.Equals(status)) {
            this.status=status;
        }
    }

    public void TurnOn() {
        if (!isActiveAndEnabled) {
            ChangeStatus("on");
            if (!lightCone.activeSelf) {
                lightCone.SetActive(true);
            }
        } else if (!status.Equals("malfunction") && !status.Equals("on") && power > 0) {
            ChangeStatus("on");
            StartCoroutine(BatteryLightFlicker());

            if (!lightCone.activeSelf) {
                lightCone.SetActive(true);
            }
        }
    }

    public void TurnOff() {
        if (status.Equals("on"))
        {
            ChangeStatus("off");

            if (lightCone.activeSelf) {
                lightCone.SetActive(false);
            }
        }
    }

    public void LoadBattery(float amount) {
        PlayerMovement playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        power = 100.0f;
        TurnOn();
        /*if (playerMovement.pickedupBattery != null) {
            if (playerMovement.currentRoom != null) {
                playerMovement.currentRoom.Noise(transform.position, 10);
            }

            if (power + amount < 100.0f)
            {
                power += amount;
            } else {
                power = 100.0f;
            }
        }*/
    }

    public void TryBlink() {
        //50% of the time the light will blink out and the blinkInterval will decrease by 1/8
        if (Random.Range(0.0f, 1.0f) < 0.5f)
        {
            currentBlinkInterval = blinkInterval * Mathf.Max(0.5f,(power/100));
            //Debug.Log(currentBlinkInterval);
            BlinkOut();
        }
    }

    //The light flickers and goes out
    public void BlinkOut() {
        if (status.Equals("on")) {
            ChangeStatus("malfunction");
            StartCoroutine(BatteryLightFlickerMalfunction());

            lastBlink = Time.time;
            StartCoroutine(Flicker());
        }
    }

    //The light flickers and turns back on
    public void BlinkOn() {
        if (status.Equals("malfunction")) {
            //GameObject.Find("Player").GetComponent<PlayerMovement>().currentRoom.Noise(transform.position,10);
            ChangeStatus("on");
            StartCoroutine(BatteryLightFlicker());

            lastBlink = Time.time;
            StartCoroutine(Flicker());
        }
    }

    //Start the hitting coroutine
    public void Hit(GameObject handA) {
        StartCoroutine(Hitting(handA));
    }

    //Flash the light randomly
    IEnumerator Flicker() {
        lightCone.SetActive(false);
        yield return new WaitForSeconds(0.01f);
        int i = 0;
        int x = Random.Range(5, 10);
        while(i < x) {
            lightCone.SetActive(true);
            yield return new WaitForSeconds(Random.Range(0.025f, 0.1f));
            lightCone.SetActive(false);
            yield return new WaitForSeconds(Random.Range(0.025f, 0.1f));
            i++;
        }
    }

    //Check the average movement distance of the hands over 5/100ths of a second
    IEnumerator Hitting(GameObject handA) {
        int i = 0;
        float totalDist = 0.0f;
        int granularity = 5;
        while (i <= granularity) {
            Vector3 posA = handA.transform.localPosition;
            Vector3 posB = flashlightHand.transform.localPosition;
            //Debug.Log(posA + "," + posB);
            float dist = Vector3.Distance(posA, posB);
            totalDist += dist;
            i++;
            yield return new WaitForSeconds(0.01f);
        }
        Debug.Log(totalDist / granularity);
        if(totalDist/granularity < hitStrengthThreshold) {
            BlinkOn();
        }
    }

    //This pattern of power light flickering is bright more often than dark 
    //when the power is high and dark more often when the power is lower.
    //This pattern indicates the flashlight is working normally
    IEnumerator BatteryLightFlicker() {
        //Debug.Log("Light flickering");
        lightFlickering = true;
        StopCoroutine(BatteryLightFlicker());
        StopCoroutine(BatteryLightFlickerMalfunction());

        while (this.status.Equals("on")) {
            //Debug.Log("Light flickering");
            flashlightLight.GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", Color.black);
            yield return new WaitForSeconds(Random.Range((100-power)*0.002f, (100 - power) * 0.003f + Random.Range(0.001f,0.1f)));
            flashlightLight.GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", new Color(1.0f,0.5f,0,1));
            yield return new WaitForSeconds(Random.Range(power * 0.002f, power * 0.03f + Random.Range(0.00001f, 1)));
        }

        lightFlickering = false;
    }

    //When a Leap Motion hand moves off screen in Unity, any of its child objects are disabled
    //When this happens, any coroutines on those objects are stopped.
    //They must be restarted when the object is re-enabled
    void OnEnable() {
        if (this.status.Equals("on"))
        {
            StartCoroutine(BatteryLightFlicker());
        } else if (this.status.Equals("malfunction"))
        {
            lightCone.SetActive(false);
            StartCoroutine(BatteryLightFlickerMalfunction());
        }
    }

    //This pattern of power light flickering is dark more often then it is light and indicates
    //that the flashlight is malfunctioning
    IEnumerator BatteryLightFlickerMalfunction() {
        StopCoroutine(BatteryLightFlicker());

        while (this.status.Equals("malfunction")) {
            //Debug.Log("Light flickering");
            flashlightLight.GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", new Color(1.0f, 0.5f, 0, 1));
            yield return new WaitForSeconds(Random.Range(0.002f,  0.003f + Random.Range(0.001f, 0.1f)));
            flashlightLight.GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", Color.black);
            yield return new WaitForSeconds(Random.Range(0.002f, 0.03f + Random.Range(0.00001f, 1)));
        }
    }
}
