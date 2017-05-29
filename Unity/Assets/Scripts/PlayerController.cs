using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	GameController gc;
	// Use this for initialization
	void Start () 
	{
		gc = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController> ();
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
			gc.ReloadLevel();
		}
	}
}
