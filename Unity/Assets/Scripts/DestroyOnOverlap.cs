using UnityEngine;
using System.Collections;

public class DestroyOnOverlap : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "target")
            Destroy(gameObject);
        Debug.Log("Removed killzone");
    }
}
