using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class AttackComponent : NetworkBehaviour
{
    Animator animator;
    public float SpecialAttackTime = 0.1f;
    float SpecialAttackAccumTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
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
        }

        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("SpecialAttackAccumTime");
            if(SpecialAttackAccumTime >= SpecialAttackTime)
            {
                animator.SetTrigger("special_attack");
            }
            else
            {
                animator.SetTrigger("normal_attack");
            }
            SpecialAttackAccumTime = 0;
        }
    }

    void DoSpecialAttack(){
        
    }
    void DoNormalAttack(){

    }
}
