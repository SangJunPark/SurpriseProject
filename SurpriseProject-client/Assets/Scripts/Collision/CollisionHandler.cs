using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CollisionHandler : MonoBehaviour
{
    IList<CollisionSensor> SensorList;
    public delegate void CollisionEvent(CollisionSensor sensor, Collider collision);

    public CollisionEvent OnEnter;
    public CollisionEvent OnExit;

    void Start()
    {
        SensorList = transform.GetComponentsInChildren<CollisionSensor>().ToList();
        foreach (var sensor in SensorList)
            sensor.Handler = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnterCollision(CollisionSensor sensor, Collider collision)
    {
        //Debug.Log("Hit");
        //Debug.Log(collision.attachedRigidbody);
        if (!collision.attachedRigidbody)
            return;

        //var mover = collision.attachedRigidbody.GetComponent<MovementController>();
        //if (!mover)
        //    return;
        var rigidbody = collision.attachedRigidbody;

        Vector3 dir = (rigidbody.transform.position - sensor.transform.position).normalized;
        //mover.ApplyKnockback(dir * 10);
        dir.y = 0;
        rigidbody.AddForce(dir* 1000);
        OnEnter?.Invoke(sensor, collision);

        //Debug.Log("Collision");
    }

    public void ExitCollision(CollisionSensor sensor, Collider collision)
    {

    }

    private void OnCollisionEnter(Collision other) {
        //Debug.Log(other);
    }

    private void OnTriggerEnter(Collider other) {
        //Debug.Log(other);
    }
}
