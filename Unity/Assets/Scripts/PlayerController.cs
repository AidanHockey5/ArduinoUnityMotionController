using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	GameController gc;
    Interface arduino;
    AudioSource audioSource;
    public AudioClip hitTargetSound;
    public AudioClip hitObsticleSound;
    // Use this for initialization
    void Start () 
	{
		gc = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController> ();
        arduino = GameObject.FindGameObjectWithTag("interface").GetComponent<Interface>();
        audioSource = GetComponent<AudioSource>();
    }
	
	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "target") 
		{
			Destroy (other.gameObject);
            audioSource.PlayOneShot(hitTargetSound);
			gc.HitTarget(other.gameObject);
		}
		if (other.gameObject.tag == "killzone") 
		{
            //arduino.RumbleMotor(2f);
            GameObject.FindGameObjectWithTag("music").GetComponent<AudioSource>().PlayOneShot(hitObsticleSound);
            gc.ReloadLevel();
        }
	}
}
