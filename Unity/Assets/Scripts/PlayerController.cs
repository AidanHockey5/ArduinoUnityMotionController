using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	GameController gc;
    Interface arduino;
	// Use this for initialization
	void Start () 
	{
		gc = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController> ();
        arduino = GameObject.FindGameObjectWithTag("interface").GetComponent<Interface>();
    }
	
	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "target") 
		{
			Destroy (other.gameObject);
			gc.HitTarget(other.gameObject);
		}
		if (other.gameObject.tag == "killzone") 
		{
            //arduino.RumbleMotor(2f);
            gc.ReloadLevel();
        }
	}
}
