using UnityEngine;
using System.Collections;
/* Name: Mathew Tomberlin
 * CST306
 */
public class ClosetDoor : Door {
    public Transform closetPoint;
    public Transform outsidePoint;
    public bool moving = false;

    public override void Open(Vector3 pushDir) {
        GameObject player = GameObject.Find("Player");
        if (!moving)
        {
            if (!player.GetComponent<PlayerStatus>().hiding)
            {
                StartCoroutine(GetIn(pushDir));
            } else
            {
                StartCoroutine(GetOut(pushDir));
            }
        }
    }
    public IEnumerator GetIn(Vector3 pushDir) {
        GameObject player = GameObject.Find("Player");
        moving = true;
        Vector3 closeDir = transform.forward;
        StartCoroutine(PerformOpen(pushDir));
        player.GetComponent<PlayerStatus>().hiding = true;
        while (!open)
        {
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(MovePlayerAndFacePoint(closetPoint));
        while (Vector3.Distance(player.transform.position, closetPoint.position) > 1)
        {
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(PerformClose(closeDir));
    }

    public IEnumerator GetOut(Vector3 pushDir)
    {
        GameObject player = GameObject.Find("Player");
        moving = true;
        Vector3 closeDir = transform.forward;
        StartCoroutine(PerformOpen(pushDir));
        while (!open)
        {
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(MovePlayer(outsidePoint.position));
        while (Vector3.Distance(player.transform.position, outsidePoint.position) > 1f)
        {
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(PerformClose(closeDir));
    }

    //This method is recursively called on each child transform that its own children,
    //Until it finds a child that doesn't have its own children, at which point it
    //checks if it has a collider and activates or deactivates it.
    public void SetCollidersActive(bool enable, Transform current) {
        Debug.Log(current.name);
        //For each child transform of the current Transform...
        foreach (Transform child in current) {
            //If this child transform has children of its own...
            if (child.childCount > 0) {
                //Recursively call this method on the child transform!
                SetCollidersActive(enable,child);
            }

            //If this child transform has a collider...
            if(child.GetComponent<Collider>()!= null) {
                //Active/Deactivate the collider depending on the enabled variable (passed from the original recursive call)
                child.GetComponent<Collider>().enabled = enable;
            }

            //If this child transform has a collider...
            if (child.GetComponent<NavMeshObstacle>() != null) {
                //Active/Deactivate the collider depending on the enabled variable (passed from the original recursive call)
                child.GetComponent<NavMeshObstacle>().enabled = enable;
            }
        }
    }

    public override IEnumerator PerformOpen(Vector3 pushDir) {
        Quaternion openRotation = Quaternion.LookRotation(pushDir, Vector3.up);
        SetCollidersActive(false, transform);
        while (Vector3.Angle(transform.forward, pushDir) > 5)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, Time.deltaTime* 2.5F);
            yield return new WaitForEndOfFrame();
        }
        open = true;
    }

    public IEnumerator PerformClose(Vector3 closeDir) {
        Quaternion closedRotation = Quaternion.LookRotation(closeDir,Vector3.up);
        while (Vector3.Angle(transform.forward, closeDir) > 5)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, closedRotation, Time.deltaTime*2.5F);
            yield return new WaitForEndOfFrame();
        }
        SetCollidersActive(true, transform);
        open = false;
        moving = false;
    }

    public IEnumerator MovePlayerAndFacePoint(Transform point) {
        GameObject player = GameObject.Find("Player");
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<NavMeshAgent>().enabled = true;
        player.GetComponent<NavMeshAgent>().Resume();
        player.GetComponent<NavMeshAgent>().SetDestination(point.position);
        while (Vector3.Distance(player.transform.position, point.position) > 0.75f)
        {
            yield return new WaitForEndOfFrame();
        }
        player.GetComponent<NavMeshAgent>().Stop();

        while (Vector3.Angle(player.transform.forward, point.forward) > 5)
        {
            Debug.Log(Vector3.Angle(player.transform.forward, closetPoint.forward));
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, Quaternion.LookRotation(closetPoint.forward, Vector3.up), Time.deltaTime*2.5f);
            yield return new WaitForEndOfFrame();
        }
        player.GetComponent<NavMeshAgent>().enabled = false;
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        moving = false;
    }

    public IEnumerator MovePlayer(Vector3 point)
    {
        GameObject player = GameObject.Find("Player");
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<NavMeshAgent>().enabled = true;
        player.GetComponent<NavMeshAgent>().Resume();
        player.GetComponent<NavMeshAgent>().SetDestination(point);
        while (Vector3.Distance(player.transform.position, point) > 0.75f)
        {
            yield return new WaitForEndOfFrame();
        }
        player.GetComponent<NavMeshAgent>().Stop();
        player.GetComponent<NavMeshAgent>().enabled = false;
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerStatus>().hiding = false;
        moving = false;
    }
}
