using UnityEngine;
using System.Collections;

public class GravityObject : MonoBehaviour
{

    private double G;
    public float mass = 100f;
    Rigidbody2D playerRb;
    void Start()
    {
        G = 6.674f * (10 ^ 11);
        playerRb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Gravity(mass, playerRb);
    }

    void Gravity(float m, Rigidbody2D b)
    {
        Vector3 direction = b.transform.position - transform.position;
        float r = direction.magnitude;
        if (r == 0) //Normalize
            return;
        direction /= r;
        float force = ((float)G * m * b.mass) / (r * r);
        b.AddForce(-direction * force);
    }
}
