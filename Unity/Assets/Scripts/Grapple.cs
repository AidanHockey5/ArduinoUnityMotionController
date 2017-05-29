using UnityEngine;
using System.Collections;

public class Grapple : MonoBehaviour
{
    public GameObject HingePoint;
    //public float maxGrappleDistance = 30f;
    public float grappleThrowForce = 5000f;
    public float grappleLockTime = 0.5f;
    GameObject activeJoint;
    Rigidbody2D rb;

	// Use this for initialization
	void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        //if (Input.GetMouseButtonDown(0))
            //FireGrapple();
        //if (Input.GetMouseButtonUp(0))
            //ReleaseGrapple();
	}

    public void FireGrapple()
    {
        //Vector3 mousePosition = Input.mousePosition;

        //mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        //mousePosition.z = 0;
        //if (Vector2.Distance(gameObject.transform.position, mousePosition) > maxGrappleDistance)
        //  return;
        ReleaseGrapple();
        activeJoint = Instantiate(HingePoint, gameObject.transform.position, Quaternion.identity) as GameObject;
        JointController jointController = activeJoint.GetComponent<JointController>();
        jointController.SetLastJointAnchor(rb);
        jointController.LaunchFirstJoint(grappleThrowForce, gameObject.transform.right, grappleLockTime);
    }

    public void ReleaseGrapple()
    {
        if (activeJoint != null)
        {
            JointController jointController = activeJoint.GetComponent<JointController>();
            jointController.ReleaseLastJointAnchor();
        }
    }

}
