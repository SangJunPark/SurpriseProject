using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPWeapon : MonoBehaviour
{
    CollisionSensor hitSensor;
    // Start is called before the first frame update
    void Start()
    {
        hitSensor = GetComponent<CollisionSensor>();
        hitSensor.enabled = false;
    }

    IEnumerator DoAttack()
    {
        yield return null;
        //hitSensor.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
