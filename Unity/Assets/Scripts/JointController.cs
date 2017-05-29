using UnityEngine;
using System.Collections;

public class JointController : MonoBehaviour
{
    public SpringJoint2D firstJoint;
    public SpringJoint2D finalJoint;
	public float retractRate = 0.1f;

    public void SetLastJointAnchor(Rigidbody2D rb)
    {
        finalJoint.connectedBody = rb;
        finalJoint.GetComponent<GrappleGraphics>().target = rb.transform;
    }

    public void LaunchFirstJoint(float force, Vector3 direction, float lockDelay)
    {
        Rigidbody2D rb = firstJoint.GetComponent<Rigidbody2D>();
        //firstJoint.transform.LookAt(direction);
        rb.AddForce(direction * force);
        rb.AddTorque(100f);
        Invoke("LockFirstJoint", lockDelay);
    }

    public void ReleaseLastJointAnchor()
    {
        GameObject dummyRB = new GameObject();
        dummyRB.transform.position = finalJoint.transform.position;
		dummyRB.transform.SetParent(transform);
        dummyRB.AddComponent<Rigidbody2D>();
        finalJoint.connectedBody = dummyRB.GetComponent<Rigidbody2D>();
        finalJoint.GetComponent<GrappleGraphics>().target = null;
        finalJoint.GetComponent<LineRenderer>().enabled = false;
        Destroy(gameObject, 5f);
    }

    void LockFirstJoint()
    {
        firstJoint.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
    }
}
