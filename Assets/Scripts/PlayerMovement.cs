using UnityEngine;
using System.Collections;
using Leap.Unity;
using UnityEngine.VR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/* Author: Mathew Tomberlin
* CST306
*/
public class PlayerMovement : MonoBehaviour {
    private GameObject player;
    private Camera cam;
    private bool moving = false;
    private bool immobile = false;
    private Vector3 throttlePivot;

    public bool VR = false;
    public GameObject LMH;
    public GameObject desktopDisplay;

    public float forwardSpeed;
    public float backwardSpeed;
    public float strafeSpeed;
    public float rotationSpeed;
    public float sensitivity = 0;
    public GameObject leftPalm = null;
    public GameObject rightPalm = null;
    public GameObject handsRoot = null;
    public GameObject flashlight;
    public GameObject pauseMenu;
    public GameObject introBG1;
    public GameObject introBG2;
    public GameObject introBG3;
    public GameObject introBG4;
    public GameObject introBG5;
    public GameObject introBG6;
    public GameObject introText;
    public GameObject introTitle;
    public GameObject winText;
    public GameObject winCredits;
    public GameObject canvas;
    private Vector3 canvasOffset;

    private bool start = false;
    private bool started = false;

    // Use this for initialization
    void Start () {
        //canvasOffset = new Vector3(canvas.transform.position.x - transform.position.x, canvas.transform.position.y - transform.position.y, canvas.transform.position.z - transform.position.z);
        cam = Camera.main;
        player = this.gameObject;// GameObject.Find("Player");
		//If V is pressed, toggle VRSettings.enabled
		VRSettings.enabled = VR;
        if (VR) {
            cam.enabled = false;
            desktopDisplay.SetActive(false);
            LMH.SetActive(true);

            //Camera.main.gameObject.layer = 0;
			handsRoot.transform.position = new Vector3(handsRoot.transform.position.x-1,handsRoot.transform.position.y,handsRoot.transform.position.z);
            LMH.GetComponentInChildren<Camera>().gameObject.layer = 4;
        } else {
            cam.enabled = true;
            desktopDisplay.SetActive(true);
            LMH.SetActive(false);

            //Camera.main.gameObject.layer = 4;
            //LMH.GetComponentInChildren<Camera>().gameObject.layer = 0;

            handsRoot.transform.parent = desktopDisplay.transform;
            //handsRoot.transform.localPosition = new Vector3(0,0,0);
            //handsRoot.transform.localScale = new Vector3(1, 1, 1);
            //handsRoot.SetActive(true);
        }

        Time.timeScale = 0.0f;
        introText.GetComponent<Text>().canvasRenderer.SetAlpha(0);
        winText.GetComponent<Text>().canvasRenderer.SetAlpha(0);
        winCredits.GetComponent<Text>().canvasRenderer.SetAlpha(0);
    }

    public void GrabThrottle()
    {
        if (!moving) {
            moving = true;
            throttlePivot = leftPalm.transform.localPosition;
        }
    }

    public void ReleaseThrottle()
    {
        if (moving) {
            moving = false;
            throttlePivot = Vector3.zero;
        }
    }

    public void ConfirmSelection() {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, rightPalm.transform.position - cam.transform.position, out hit)) {
            if(hit.transform.GetComponentInParent<Grabable>() != null) {
                hit.transform.GetComponentInParent<Grabable>().Use(rightPalm);
            }
        }
    }

    public void UseObject() {
        ProximityDetector leftPalmProximity = leftPalm.GetComponent<ProximityDetector>();
		Debug.Log ("Use "+leftPalmProximity.CurrentObject);
		if (leftPalmProximity != null && leftPalmProximity.CurrentObject != null && (leftPalmProximity.CurrentObject.GetComponentInParent<Grabable>() != null || leftPalmProximity.CurrentObject.GetComponent<Grabable>() != null)) {
            leftPalmProximity.CurrentObject.GetComponentInParent<Grabable>().Use(leftPalm);
            //Debug.Log ("Use "+leftPalmProximity.CurrentObject);
        }
    }

    void LateUpdate() {
        //If we're not using an HMD, then we get the screen point of the hand and use that to determine which direction rotate and by how much
        if (!VR) {
            if (rightPalm.activeInHierarchy && !immobile) {
                //The right palm's position as a percentage of the screen
                float screenPercentX = Mathf.Clamp(cam.WorldToScreenPoint(rightPalm.transform.position).x / Screen.width, 0, 1);
                float screenPercentY = Mathf.Clamp(cam.WorldToScreenPoint(rightPalm.transform.position).y / Screen.height, 0, 1);

                float deltaX = ((screenPercentX > 0.725f) ? Mathf.Min(0.825f, screenPercentX) - 0.725f : ((screenPercentX < 0.675f) ? 0.675f - Mathf.Max(0.575f, screenPercentX) : 0)) * 10f;
                deltaX = (deltaX <= 0.0001f) ? deltaX = 0.0001f : deltaX;
                float deltaY = ((screenPercentY > 0.625f) ? Mathf.Min(0.725f, screenPercentY) - 0.625f : ((screenPercentY < 0.575f) ? 0.575f - Mathf.Max(0.475f, screenPercentY) : 0)) * 10f;

                player.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(rightPalm.transform.position.x, transform.position.y, rightPalm.transform.position.z) - (transform.position + transform.right * 0.15f), Vector3.up), Time.deltaTime * rotationSpeed * Mathf.Pow(deltaX, 1.5f));
            }
            //If are using an HMD, then we get the angle of the arm and if it's 15 degrees greater than the body's forward angle or 10 degrees less than the body's forward angle, rotate that direction
        } else {
            if (rightPalm.activeInHierarchy && !immobile) {
				Vector3 palmPos2D = new Vector3(rightPalm.transform.position.x, transform.position.y, rightPalm.transform.position.z);

				Vector3 armAngle = Vector3.Cross(transform.forward, palmPos2D - new Vector3(transform.position.x, 0, transform.position.z));
				if (armAngle.y > 0.25f + (sensitivity)) {
					player.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.right, Vector3.up), Time.deltaTime * rotationSpeed*Mathf.Abs((armAngle.y-0.4f)));
				} else if (armAngle.y < 0.25f + (sensitivity)) {
					player.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-transform.right, Vector3.up), Time.deltaTime * rotationSpeed*Mathf.Abs((armAngle.y-0.4f)));
				}
            }
        }
    }

    public void Quit() {
        Application.Quit();
    }

    public void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public IEnumerator PlayIntro() {
        float duration1 = 1.0f;
        float duration2 = 10.0f;
        introTitle.GetComponent<Text>().CrossFadeAlpha(0, 2.0f, true);
        introText.GetComponent<Text>().CrossFadeAlpha(1, 2.0f, true);
        while (duration2 > 0) {
            duration2 -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        introText.GetComponent<Text>().CrossFadeAlpha(0, 1.0f, true);
        introBG1.GetComponent<Image>().CrossFadeAlpha(0, 1.0f, true);
        introBG2.GetComponent<Image>().CrossFadeAlpha(0, 1.0f, true);
        introBG3.GetComponent<Image>().CrossFadeAlpha(0, 1.0f, true);
        introBG4.GetComponent<Image>().CrossFadeAlpha(0, 1.0f, true);
        introBG5.GetComponent<Image>().CrossFadeAlpha(0, 1.0f, true);
        introBG6.GetComponent<Image>().CrossFadeAlpha(0, 1.0f, true);
        while (duration1 > 0) {
            duration1 -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        introBG1.SetActive(false);
        introBG2.SetActive(false);
        introBG3.SetActive(false);
        introBG4.SetActive(false);
        introBG5.SetActive(false);
        introBG6.SetActive(false);
        Debug.Log("started");
        started = true;
    }

    public IEnumerator Win() {
        canvas.transform.position = transform.position;
        this.enabled = false;
        float duration1 = 30.0f;
        float duration2 = 120.0f;
        introBG1.SetActive(true);
        introBG2.SetActive(true);
        introBG3.SetActive(true);
        introBG4.SetActive(true);
        introBG5.SetActive(true);
        introBG6.SetActive(true);
        winText.GetComponent<Text>().CrossFadeAlpha(1, 1.0f, true);
        introBG1.GetComponent<Image>().CrossFadeAlpha(1, 2.0f, true);
        introBG2.GetComponent<Image>().CrossFadeAlpha(1, 2.0f, true);
        introBG3.GetComponent<Image>().CrossFadeAlpha(1, 2.0f, true);
        introBG4.GetComponent<Image>().CrossFadeAlpha(1, 2.0f, true);
        introBG5.GetComponent<Image>().CrossFadeAlpha(1, 2.0f, true);
        introBG6.GetComponent<Image>().CrossFadeAlpha(1, 2.0f, true);
        while (duration1 > 0) {
            duration1 -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        winText.GetComponent<Text>().CrossFadeAlpha(0, 1.0f, true);
        yield return new WaitForSeconds(1.0f);
        winCredits.GetComponent<Text>().CrossFadeAlpha(1, 2.0f, true);
        while (duration2 > 0) {
            duration2 -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public void Pause() {
        Debug.Log("Test1");
        if(Time.timeScale > 0.0f) {
            Debug.Log("Test2");
            if (started) {
                Debug.Log("Test3");
                Time.timeScale = 0.0f;
                pauseMenu.SetActive(true);
            }
        } else {
            Debug.Log("Test4");
            if (!start) {
                Debug.Log("Test5");
                start = true;
                StartCoroutine(PlayIntro());
            }
            Debug.Log("Test6");
            Time.timeScale = 1.0f;
            pauseMenu.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp(KeyCode.Space)) {
            Pause();
        }
        //If the player is grabbing the virtual joystick and we're not immobile, get the horizontal and vertical axis clamped between -1 and 1, convert it to an absolute value to determine
        //the rate of change since last frame and then compare each axis to see if its greater than or less than +-0.01. If it is, move in the indicated direction (Can be forward/backward 
        //and strafe simultaneously)
        if (moving && !immobile) {
            float verticalAxis = Mathf.Clamp(throttlePivot.z - leftPalm.transform.localPosition.z, -0.1f, 0.1f) * 10;
            float horizontalAxis = Mathf.Clamp(throttlePivot.x - leftPalm.transform.localPosition.x, -0.1f, 0.1f) * 10;
            float deltaZ = Mathf.Abs(verticalAxis);
            float deltaX = Mathf.Abs(horizontalAxis);

            Vector3 speed = transform.TransformDirection(new Vector3((horizontalAxis > 0.01f) ? -strafeSpeed * deltaX : ((horizontalAxis < -0.01f) ? strafeSpeed * deltaX : 0), 0, (verticalAxis > 0.01f) ? -backwardSpeed * deltaZ : ((verticalAxis < -0.01f) ? forwardSpeed * deltaZ : 0)));

            player.GetComponent<CharacterController>().Move(Vector3.down + speed * Time.deltaTime);
        }

        RaycastHit hit;
        if(Physics.SphereCast(rightPalm.transform.position,1,rightPalm.transform.forward,out hit)) {
            //Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.transform.GetComponentInParent<Grabable>() != null) {
                hit.collider.transform.GetComponentInParent<Grabable>().StartCoroutine(hit.collider.transform.GetComponentInParent<Grabable>().Hover());
            }
        }

        //Always move the player down. So far we have not developed a jumping function as we have not perfected the use of gravity with this setup
        //player.transform.position = new Vector3(player.transform.position.x, 0.1f, player.transform.position.z);
        //player.GetComponent<CharacterController>().Move(Vector3.down);
    }
}
