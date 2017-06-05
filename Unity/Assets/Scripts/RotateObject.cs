using UnityEngine;
using System.Collections;

public class RotateObject : MonoBehaviour
{
    public float rotaionRate = 3f;
	// Update is called once per frame
	void Update ()
    {
        transform.Rotate(Vector3.forward * rotaionRate * Time.deltaTime);
	}
}
