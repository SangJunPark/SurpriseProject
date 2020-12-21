using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class AttackComponent : NetworkBehaviour
{
    Animator animator;
    public float SpecialAttackTime = 0.1f;
    float SpecialAttackAccumTime = 0f;

    CollisionHandler collisionHandler;
    SPWeapon AnyWeapon;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        collisionHandler = GetComponent<CollisionHandler>();
        AnyWeapon = GetComponentInChildren<SPWeapon>();

        collisionHandler.OnEnter += OnOtherHit;
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.hasAuthority) return;

        if (Input.GetMouseButtonDown(0))
        {
            SpecialAttackAccumTime = 0;
        }

        if (Input.GetMouseButton(0))
        {
            SpecialAttackAccumTime += Time.fixedDeltaTime;
            DoSpecialAttack();

        }

        if (Input.GetMouseButtonUp(0))
        {
            if(SpecialAttackAccumTime < SpecialAttackTime){
                DoNormalAttack();
            }else if(SpecialAttackAccumTime >= SpecialAttackTime){
                DoSpecialAttack();
            }
            SpecialAttackAccumTime = 0;
        }
    }

    void DoSpecialAttack()
    {
        if (SpecialAttackAccumTime >= SpecialAttackTime)
        {
            animator.SetTrigger("special_attack");
            SpecialAttackAccumTime = 0;
        }
    }
    void DoNormalAttack()
    {
        animator.SetTrigger("normal_attack");
        SpecialAttackAccumTime = 0;
        //Debug.Log("SpecialAttackAccumTime");
    }

    void OnAttackStart(){
        //Debug.Log("ATTACK START");
        AnyWeapon.collisionEnabled = true;
    }

    void OnAttackEnd(){
        //Debug.Log("ATTACK END");
        AnyWeapon.collisionEnabled = false;
    }

    //적을 적중했을 때
    void OnOtherHit(CollisionSensor sensor, Collider other){
        AnyWeapon.DoHit(other.ClosestPoint(AnyWeapon.transform.position), Quaternion.identity);
    }
}
