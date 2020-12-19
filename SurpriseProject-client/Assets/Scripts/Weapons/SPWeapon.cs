using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPWeapon : MonoBehaviour
{
    CollisionSensor hitSensor;
    [SerializeField] GameObject HitEffectTemplate;

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

    public bool collisionEnabled {
        set {
            hitSensor.enabled = value;
        }
    }

    public void DoHit(Vector3 hitPosition, Quaternion hitRotation){
        Instantiate(HitEffectTemplate, hitPosition, Quaternion.identity);
    }
}
