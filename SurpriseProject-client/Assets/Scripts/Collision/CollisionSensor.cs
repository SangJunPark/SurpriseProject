using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSensor : MonoBehaviour
{
    CollisionHandler handler;
    Collider sensorCollider;

    public CollisionHandler Handler
    {
        set => handler = value;
    }
    // Start is called before the first frame update
    void Start()
    {
        sensorCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDisable() => sensorCollider.enabled = false;
    private void OnEnable() => sensorCollider.enabled = true;
        

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
