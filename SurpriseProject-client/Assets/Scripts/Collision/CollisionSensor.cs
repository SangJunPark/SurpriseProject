using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSensor : MonoBehaviour
{
    CollisionHandler handler;
    public CollisionHandler Handler
    {
        set => handler = value;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        //if (handler)
        //    handler.EnterCollision(this, collision);
    }

    void OnCollisionExit(Collision collision)
    {
        //if (handler)
        //    handler.ExitCollision(this, collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (handler)
            handler.EnterCollision(this, other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (handler)
            handler.ExitCollision(this, other);
    }

}
