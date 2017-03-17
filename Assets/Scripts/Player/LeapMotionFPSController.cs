using UnityEngine;
using System.Collections;
using Leap.Unity;
using UnityEngine.VR;
/* Name: Mathew Tomberlin
 * CST306
 */
public class LeapMotionFPSController : MonoBehaviour {
    private Camera cam;
    //When the joystick is active, this is the neutral position
    private Vector3 joystickNeutralPos;
    //Used to tell if the player is grabbing the throttle
    private bool joystickActive = false;

    public GameObject virtualThrottle;
    //Forward speed (divided by 2 in VR)
    public float forwardSpeed;
    //Backward speed (divided by 2 in VR)
    public float backwardSpeed;
    //Horizontal speed (divided by 2 in VR)
    public float strafeSpeed;

    private float currentForwardSpeed = 0.0f;

    public float rotationSpeed;
    public float sensitivity=0.1f;

    //The gameObject for the left palm
    public GameObject leftPalm = null;
    //The gameObject for the right palm
    public GameObject rightPalm = null;
    //The gameObject for the hands root
    public GameObject hands = null;
    //The player's body
    public GameObject playerBody;

    //VR Settings
    //If on, use a VR rig
    public bool VR = false;
    //The Oculus Leap Motion rig
    public GameObject LMH;
    //The Desktop Leap Motion rig
    public GameObject desktopDisplay;
    //The last forward vector of the camera (Used to determine if the head is moving)
    public Vector3 lastForward;

    // Use this for initialization
    void Start () {
        cam = Camera.main;
		lastForward = Camera.main.transform.forward;
        
		VRSettings.enabled = VR;
        if (VR) {
			forwardSpeed /= 2;
			backwardSpeed /= 2;
			strafeSpeed /= 2;

            cam.enabled = false;
            LMH.SetActive(true);

			hands.transform.localScale = new Vector3(3,3,3);
			hands.transform.position = new Vector3(0,2,0);
			hands.SetActive(true);
        } else {
            cam.enabled = true;
            desktopDisplay.SetActive(true);

            hands.transform.parent = desktopDisplay.transform;
            hands.transform.localScale = new Vector3(.25f,.25f,.25f);
            hands.SetActive(true);
        }
    }

    public void GrabThrottle()
    {
        if (!joystickActive) {
            joystickActive = true;
            joystickNeutralPos = virtualThrottle.transform.localPosition;
        }
    }

    public void ReleaseThrottle()
    {
        if (joystickActive) {
            joystickActive = false;
            joystickNeutralPos = Vector3.zero;
        }
    }

    public void UseObject()
    {
        ProximityDetector leftPalmProximity = leftPalm.GetComponent<ProximityDetector>();
		
        if (leftPalmProximity != null && leftPalmProximity.CurrentObject.GetComponentInParent<Grabable>() != null) {
            leftPalmProximity.CurrentObject.GetComponentInParent<Grabable>().Use(leftPalm);
        }
    }

    // Update is called once per frame
    void Update () {
		float delta = Vector3.Angle (lastForward, Camera.main.transform.forward);
		lastForward = Camera.main.transform.forward;

		if (!VR) {
			if (rightPalm.activeInHierarchy && !GetComponent<PlayerStatus>().hiding) {
                //Get the player's rightPalm's position (on the x) as a percentage of the screen width (left side = 0.0, right side = 1.0)
				float screenPercent = Mathf.Clamp(cam.WorldToScreenPoint (rightPalm.transform.position).x / Screen.width,0,1);
                float minLeft = 0.65f;
                float minRight = 0.75f;
                //Minimum armAngle to start rotating left
                float leftAngle = 0.7f - Mathf.Min(0.1f, sensitivity);
                //Minimum armAngle to start rotating right
                float rightAngle = 0.7f + Mathf.Min(0.1f, sensitivity);

                //If armAngle (on the y axis) is less than leftAngle, rotate left
                if (screenPercent <= leftAngle) {
                    float range = minLeft - Mathf.Max(0.4f, screenPercent);
                    float amount = (range) / 0.25f;
                    Debug.Log(amount);
					transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.LookRotation (-transform.right, Vector3.up), Time.deltaTime * amount * rotationSpeed);
				} else if (screenPercent >= rightAngle) {
                    //Amount = ((0.75 - 1.0) - 0.85) = [0.0 - 0.15]
                    //[0.0 - 0.15] / 0.75 = [0.0 - 0.2]
                    //[0.0 - 0.2] / 0.2 = [0.0 - 1.0]
                    float range = Mathf.Min(1.0f, screenPercent) - minRight;
                    float amount = (range)/0.25f;
                    Debug.Log(amount);
                    transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.LookRotation (transform.right, Vector3.up), Time.deltaTime * amount * rotationSpeed);
				}
			}
		} else {
			if (rightPalm.activeInHierarchy && !GetComponent<PlayerStatus>().hiding && (Mathf.Abs(delta)<0.25f && Vector3.Angle (Camera.main.transform.forward, playerBody.transform.forward)<25)) {
                //Get the player's palm position's x and z, y = playerBody position
				Vector3 rightPalmDir = new Vector3(rightPalm.transform.position.x, playerBody.transform.position.y, rightPalm.transform.position.z);

                //This is the angle from 0 - 180 (+/- depending on direction) from the player's body's forward and the vector from the player's body to the player's right palm
				Vector3 armAngle = Vector3.Cross(playerBody.transform.forward,rightPalmDir - new Vector3(playerBody.transform.position.x,0,playerBody.transform.position.z));
                //Minimum armAngle to start rotating the body right
                float rightAngle = 0.5f - Mathf.Min(0.05f, sensitivity);
                //Minimum armAngle to start rotating the body left
                float leftAngle = 0.4f + Mathf.Min(0.05f, sensitivity);

                //If the armAngle (on the y axis) exceeds the rightAngle amount, rotate right
                if (armAngle.y > rightAngle) {
					transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (transform.right, Vector3.up), Time.deltaTime * rotationSpeed *Mathf.Abs(armAngle.y-rightAngle));
                //If the armAngle (on the y axis) is less than the leftAngle amount, rotate left
                } else if(armAngle.y < leftAngle) {
					transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (-transform.right, Vector3.up), Time.deltaTime * rotationSpeed *Mathf.Abs(0.4f-leftAngle));
				}
			}
		}

		if (joystickActive && !GetComponent<PlayerStatus>().hiding && (!VR || Mathf.Abs(delta)<0.5f && Vector3.Angle (Camera.main.transform.forward, playerBody.transform.forward)<25)) {
            //The difference between the joystick's current and neutral positions on the z axis (forward/backward)
            float verticalAxis = joystickNeutralPos.z - virtualThrottle.transform.localPosition.z;
            //The difference between the joystick's current and neutral positions on the x axis (left/right)
            float horizontalAxis = joystickNeutralPos.x - virtualThrottle.transform.localPosition.x;
            //The joystick's readable area is from 0 - 0.25 on the z axis, so convert it to a percentage
            float deltaZ = Mathf.Min(0.25f,Mathf.Abs(verticalAxis))/0.25f;
            float deltaX = Mathf.Min(0.25f, Mathf.Abs(horizontalAxis))/0.25f;
            Debug.Log(deltaX);

            if (verticalAxis > 0.05f) {
                currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, forwardSpeed * deltaZ, Time.deltaTime*4);
                GetComponent<CharacterController>().Move(currentForwardSpeed * -transform.forward*Time.deltaTime);
            } else if (verticalAxis < -0.05f) {
                currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, -backwardSpeed * deltaZ, Time.deltaTime*4);
                GetComponent<CharacterController>().Move(currentForwardSpeed * -transform.forward*Time.deltaTime);
            } else {
                currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, 0, Time.deltaTime*4);
                GetComponent<CharacterController>().Move(currentForwardSpeed * -transform.forward * Time.deltaTime);
            }

            if (horizontalAxis < -0.05f) {
                GetComponent<CharacterController>().Move((strafeSpeed * transform.right) * deltaX * Time.deltaTime);
            } else if (horizontalAxis > 0.05f) {
                GetComponent<CharacterController>().Move((strafeSpeed * -transform.right) * deltaX * Time.deltaTime);
            }
        }

        GetComponent<CharacterController>().Move(Vector3.down);
    }
}
