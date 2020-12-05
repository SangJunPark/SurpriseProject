using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Suproject.Utils;

public class MovementController : MonoBehaviour
{
    [TextArea(1, 1)]
    public string Text;
    public Vector3 Magnitude;
    public float GroundFriction = 1;

    public float Stirring = 1;
    public float Mass = 1;
    public float Accelation = 1;
    public float DashAccelation = 1;

    public float MaxForwardVelocity = 5f;
    public float MaxStirVelocity = 2f;


    Vector3 Velocity;
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
        //Magnitude.x = Input.GetAxisRaw("Horizontal");
        //Magnitude.z = Input.GetAxisRaw("Vertical");
        
        MousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(Input.mousePosition);
        //Debug.Log(MousePos);

        
        //FrameVelocity.Set(0,0,0);
        Velocity.Set(0,0,0);
         
        if (Input.GetKey(KeyCode.W))
        {
            //Magnitude.z = Mathf.Clamp(Magnitude.z + 0.01f, -0.1f,0.1f);
            Velocity.z = 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
//            Magnitude.z = Mathf.Clamp(Magnitude.z - 0.01f, -0.1f, 0.1f);
            Velocity.z = -1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            //Magnitude.x = Mathf.Clamp(Magnitude.z + 0.01f, -0.1f,0.1f);
           // Velocity.x = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            //Magnitude.x= Mathf.Clamp(Magnitude.z - 0.01f, -0.1f, 0.1f);
            //Velocity.x = 1;
        }

        if (Input.GetKey(KeyCode.LeftShift) && DashAccelation == 1)
        {
            //Magnitude.x= Mathf.Clamp(Magnitude.z - 0.01f, -0.1f, 0.1f);
            DashAccelation = 30;
        }


        // // Magnitude.z = Magnitude.z > 0 ? Magnitude.z - Time.deltaTime*0.4f : Magnitude.z+ Time.deltaTime*0.4f;
        // // Magnitude.z = Mathf.Abs(Magnitude.z) < 0.0001 ? 0 :  Magnitude.z;
        // // PhysicalVelocity.z = Mathf.Min(PhysicalVelocity.z + Magnitude.z, 0.1f);
        // // PhysicalVelocity.z = Mathf.Clamp(PhysicalVelocity.z -= Time.deltaTime, 0f, 0.1f);
        Vector3 F = Velocity * Accelation * Mass * Time.fixedDeltaTime;

        if(DashAccelation > 1)
        {
            DashAccelation -= (Time.fixedDeltaTime * 60f);
            DashAccelation = Mathf.Max(1, DashAccelation);
            F = Velocity * DashAccelation * Mass * Time.fixedDeltaTime;
        }

        if(Magnitude.z < 0.01f * MaxForwardVelocity * DashAccelation)
            Magnitude += (F * Time.fixedDeltaTime * 0.1F * 8F);
        
        //Adjust Friction
        float MagnitudeForward = Mathf.Abs(Magnitude.z);
        float MagnitudeStir = Mathf.Abs(Magnitude.x);


        if(MagnitudeForward > 0f)
        {
            int Direction = Magnitude.z > 0 ? 1 : -1;
            float Friction = (Time.fixedDeltaTime * 0.01f * GroundFriction);
            MagnitudeForward -= Friction;
            Magnitude.z = Mathf.Max(0, MagnitudeForward) * Direction;
            Debug.Log(MagnitudeForward);
        }else
            MagnitudeForward = 0;

        if(MagnitudeStir > 0.01f)
        {
            int Direction = Magnitude.z > 0 ? 1 : -1;
            //Magnitude.x += Friction.x * Stirring;
        }


        if(DashAccelation == 1)
        {
            Vector3 point = new Vector3();
            Event currentEvent = Event.current;
            Vector2 mousePos = new Vector2();

            // Get the mouse position from Event.
            // Note that the y position from Event is inverted.

            mousePos.x = Input.mousePosition.x;
            mousePos.y = cam.pixelHeight - Input.mousePosition.y;

            point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            point = cam.WorldToScreenPoint(transform.position);

            Vector3 aimDir = -(new Vector3(mousePos.x - point.x, 0, point.y - mousePos.y)).normalized;
            //Debug.Log(aimDir);
            Quaternion q = Quaternion.LookRotation(aimDir, Vector3.up);

            //q = Quaternion.Lerp(transform.rotation, q, 0.1f);
            //q.x = 0; q.z = 0;
            transform.rotation = q;
        }
       
        //transform.LookAt(transform.position + aimDir * 10);

        transform.Translate(Magnitude);
        //CharacterRigidbody.velocity = Magnitude;
        animator.SetFloat("velocity", Mathf.Lerp(0, 1, MagnitudeForward / (MaxForwardVelocity * 0.01f)));

        //Vector3 point = new Vector3();
        //Event currentEvent = Event.current;
        //Vector2 mousePos = new Vector2();

        //// Get the mouse position from Event.
        //// Note that the y position from Event is inverted.
        //mousePos.x = currentEvent.mousePosition.x;
        //mousePos.y = cam.pixelHeight - currentEvent.mousePosition.y;

        //point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
        //point = cam.WorldToScreenPoint(transform.position);

        //Vector3 aimDir = -(new Vector3(mousePos.x - point.x, 0, mousePos.y - point.y)).normalized;
        //Debug.Log(aimDir);
        //transform.LookAt(transform.position + aimDir * 10);

        //transform.LookAt(transform.position + aimDir * 10);
        //transform.eulerAngles = new Vector3(0, UtilsClass.GetAngleFromVectorFloat(aimDir), 0);


        //     // rotation.y += Input.GetAxis("Mouse X") * 5f;
        //     // rotation.x += Input.GetAxis("Mouse Y") * 5f;
        //     // rotation.x = Mathf.Clamp(rotation.x, -60, 60);
        //     // transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        //     // transform.eulerAngles = new Vector2(0, rotation.y);

        // CharacterRigidbody.AddRelativeForce(Direction);
        //Debug.Log(CharacterRigidbody.velocity);

        //Debug.Log(Input.GetAxis("Horizontal"));
        //Debug.Log(Input.GetAxis("Mouse Y"));

    }

    void FixedUpdate() {
        //CharacterRigidbody.MovePosition(CharacterRigidbody.position + Magnitude * 10 * Time.fixedDeltaTime);
        //Vector2 lookDir = MousePos - new Vector2(CharacterRigidbody.position.x, CharacterRigidbody.position.z);
        //float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        //CharacterRigidbody.rotation = Quaternion.LookRotation(lookDir);
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
    void OnGUI()
    {


        //GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        //GUILayout.Label("Screen pixels: " + cam.pixelWidth + ":" + cam.pixelHeight);
        //GUILayout.Label("Mouse position: " + mousePos);
        //GUILayout.Label("World position: " + point.ToString("F3"));
        //GUILayout.EndArea();
    }
}
