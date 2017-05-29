using UnityEngine;
using System.Collections;

public class GrappleGraphics : MonoBehaviour
{
    public Transform target;
    LineRenderer lr;
	// Use this for initialization
	void Start ()
    {
        lr = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(target != null)
        {
            lr.SetPosition(0, gameObject.transform.position);
            lr.SetPosition(1, target.transform.position);
        }
	}
}
