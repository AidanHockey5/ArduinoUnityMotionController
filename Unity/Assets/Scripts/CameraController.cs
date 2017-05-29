using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
	public Transform target;
	public float followSpeed = 10f;
	public float zOffset = -75f;
    public float orthoMin = 15f;
    public float orthoMax = 60f;

	void Update () 
	{
		transform.position = Vector3.MoveTowards (transform.position, target.transform.position, Time.deltaTime * followSpeed);
		transform.position = new Vector3(transform.position.x, transform.position.y, zOffset);
        if (Input.GetAxis("Mouse ScrollWheel") < 0) 
 {
            Camera.main.orthographicSize++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
 {
            Camera.main.orthographicSize--;
        }
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, orthoMin, orthoMax);
    }
}
