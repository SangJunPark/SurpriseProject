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
    Rigidbody CharacterRigidbody;
    Vector2 rotation = Vector2.zero;
    Vector2 MousePos;

    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        CharacterTR = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        CharacterRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //Magnitude.Set(0,0,0);
        Magnitude.x = Input.GetAxisRaw("Horizontal");
        Magnitude.z = Input.GetAxisRaw("Vertical");
        
        MousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log(Input.mousePosition);
        Debug.Log(MousePos);

        
        //FrameVelocity.Set(0,0,0);
        // Direction.Set(0,0,0);
         
        // if (Input.GetKey(KeyCode.W))
        // {
        //     Magnitude.z = Mathf.Clamp(Magnitude.z + 0.01f, -0.1f,0.1f);
        //     Direction.z = 100;
        // }

        // if (Input.GetKey(KeyCode.S))
        // {
        //     Magnitude.z = Mathf.Clamp(Magnitude.z - 0.01f, -0.1f, 0.1f);
        //     Direction.z = -100;
        // }

        // if (Input.GetKey(KeyCode.D))
        // {
        //     Magnitude.x = Mathf.Clamp(Magnitude.z + 0.01f, -0.1f,0.1f);
        //     Direction.x = 100;
        // }

        // if (Input.GetKey(KeyCode.A))
        // {
        //     Magnitude.x= Mathf.Clamp(Magnitude.z - 0.01f, -0.1f, 0.1f);
        //     Direction.x = -100;
        // }
        // // Magnitude.z = Magnitude.z > 0 ? Magnitude.z - Time.deltaTime*0.4f : Magnitude.z+ Time.deltaTime*0.4f;
        // // Magnitude.z = Mathf.Abs(Magnitude.z) < 0.0001 ? 0 :  Magnitude.z;
        // // PhysicalVelocity.z = Mathf.Min(PhysicalVelocity.z + Magnitude.z, 0.1f);
        // // PhysicalVelocity.z = Mathf.Clamp(PhysicalVelocity.z -= Time.deltaTime, 0f, 0.1f);
        // // CharacterTR.Translate(Magnitude);

        //     // rotation.y += Input.GetAxis("Mouse X") * 5f;
        //     // rotation.x += Input.GetAxis("Mouse Y") * 5f;
        //     // rotation.x = Mathf.Clamp(rotation.x, -60, 60);
        //     // transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        //     // transform.eulerAngles = new Vector2(0, rotation.y);

        // animator.SetFloat("velocity", Mathf.Abs(Direction.magnitude));
        // CharacterRigidbody.AddRelativeForce(Direction);
        //Debug.Log(CharacterRigidbody.velocity);

        //Debug.Log(Input.GetAxis("Horizontal"));
        //Debug.Log(Input.GetAxis("Mouse Y"));

    }

    void FixedUpdate() {
        CharacterRigidbody.MovePosition(CharacterRigidbody.position + Magnitude * 10 * Time.fixedDeltaTime);
        Vector2 lookDir = MousePos - new Vector2(CharacterRigidbody.position.x, CharacterRigidbody.position.z);
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        CharacterRigidbody.rotation = Quaternion.LookRotation(lookDir);
    }

        public static Vector3 GetMouseWorldPosition() {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }
        public static Vector3 GetMouseWorldPositionWithZ() {
            return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        }
        public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera) {
            return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
        }
        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera) {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }
}
