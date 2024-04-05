using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody rb;

    public int damage;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            rb.velocity = Vector3.zero;
            Destroy(gameObject, 3);
        }else if(collision.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
