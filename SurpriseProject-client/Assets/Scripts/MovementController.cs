using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    Vector3 Magnitude;
    Vector3 PhysicalVelocity;
    Vector3 Direction;
    Animator animator;
    Transform CharacterTR;
    Vector2 rotation = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        CharacterTR = GetComponent<Transform>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //FrameVelocity.Set(0,0,0);
        Direction.Set(0,0,0);
         
        if (Input.GetKey(KeyCode.W))
        {
            Magnitude.z = Mathf.Clamp(Magnitude.z + 0.01f, -0.1f,0.1f);
        }

        if (Input.GetKey(KeyCode.S))
        {
            Magnitude.z = Mathf.Clamp(Magnitude.z - 0.01f, -0.1f, 0.1f);
        }
        Magnitude.z = Magnitude.z > 0 ? Magnitude.z - Time.deltaTime*0.4f : Magnitude.z+ Time.deltaTime*0.4f;
        Magnitude.z = Mathf.Abs(Magnitude.z) < 0.0001 ? 0 :  Magnitude.z;
        PhysicalVelocity.z = Mathf.Min(PhysicalVelocity.z + Magnitude.z, 0.1f);
        PhysicalVelocity.z = Mathf.Clamp(PhysicalVelocity.z -= Time.deltaTime, 0f, 0.1f);
        CharacterTR.Translate(Magnitude);

        animator.SetFloat("velocity", Mathf.Abs(Magnitude.z));

        
            rotation.y += Input.GetAxis("Mouse X") * 5f;
            rotation.x += Input.GetAxis("Mouse Y") * 5f;
            rotation.x = Mathf.Clamp(rotation.x, -60, 60);
            transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
            transform.eulerAngles = new Vector2(0, rotation.y);
    }
}
