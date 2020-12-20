using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Suproject.Utils;
using Mirror;
public class MovementController : NetworkBehaviour
{
    [TextArea(1, 1)]
    public string Text;
    public Vector3 Velocity;
    public float GroundFriction = 1;
    public float Stirring = 1;
    public float Mass = 1;
    public float DashAccelation = 0;

    public float MaxForwardVelocity = 5f;
    public float MaxStirVelocity = 2f;

    
    Vector3 Accel;
    Vector3 Magnitude;
    Vector3 Direction;
    Animator animator;
    Transform CharacterTR;
    Transform CharacterRotatorTR;

    Rigidbody CharacterRigidbody;
    Vector2 rotation = Vector2.zero;
    Vector2 MousePos;
    Vector3 PlayerInput;

    [SerializeField, Range(0.1f, 3f)]
    float MaxSpeed = 5f;

    [SerializeField, Range(1f, 50f)]
    float MaxDashSpeed = 10f;

    [SerializeField, Range(0.1f, 50f)]
    float ForwardAccelation = 5f;

    [SerializeField, Range(0.1f, 50f)]
    float StirAccelation = 5f;

    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        if (!base.hasAuthority) return;
        cam = Camera.main;
        cam.GetComponent<FollowCamera>().focus = transform;
        CharacterTR = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        CharacterRigidbody = GetComponent<Rigidbody>();
        CharacterRotatorTR = transform.Find("Rotator");
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.hasAuthority) return;

        MousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        Magnitude *= 0;
        PlayerInput *= 0;

        PlayerInput.x = -Input.GetAxis("Horizontal");
        PlayerInput.z = -Input.GetAxis("Vertical");

        bool isStartDash = false;
        if (Input.GetKey(KeyCode.LeftShift) && DashAccelation == 0)
        {
            isStartDash = true;
        }
        if (DashAccelation == 0)
            PlayerInput = Vector3.ClampMagnitude(PlayerInput, 1f);


        ////이동 타입 1
        //    Vector3 desiredVelocity = new Vector3(PlayerInput.x, 0f, PlayerInput.z) * MaxSpeed;
        //float maxSpeedChange = ForwardAccelation * Time.fixedDeltaTime;

        //Velocity.x = Mathf.MoveTowards(Velocity.x, desiredVelocity.x, maxSpeedChange);
        //Velocity.z = Mathf.MoveTowards(Velocity.z, desiredVelocity.z, maxSpeedChange);

        //ApplyForce(PlayerInput * Time.fixedDeltaTime * 0.1f);
        //Velocity += Magnitude;
        //transform.Translate(Velocity * Time.fixedDeltaTime);
        //return;



        //if (Input.GetKey(KeyCode.W))
        //{
        //    this.ApplyForce(new Vector3(0,0,1));
        //}

        //if (Input.GetKey(KeyCode.S))
        //{
        //    this.ApplyForce(new Vector3(0,0,-1));
        //}

        //if (Input.GetKey(KeyCode.D))
        //{
        //    this.ApplyForce(new Vector3(1,0,0));
        //}

        //if (Input.GetKey(KeyCode.A))
        //{
        //    this.ApplyForce(new Vector3(-1,0,0));
        //}

        if (isStartDash)
        {
            DashAccelation = MaxDashSpeed;
            Velocity *= 0;
            ApplyForce(PlayerInput * MaxDashSpeed);
        }
        else
        {
            ApplyForce(PlayerInput);
        }


        float N = 1 * Mass * 5f; //수직 항력 ( m * g)
        float U = 0.01f * GroundFriction; // 마찰 계수
        Vector3 RevVel = -Velocity.normalized;
        Vector3 Friction = RevVel * N * U * (DashAccelation > 0 ? 1 : 1);
        DashAccelation = Mathf.Max(0, DashAccelation - Time.fixedDeltaTime * 100f);
        //Debug.Log(DashAccelation);

        ApplyForce(Friction);

        Magnitude.x *= StirAccelation;
        Magnitude.z *= ForwardAccelation;
        Vector3 F = Magnitude * Mass * Time.fixedDeltaTime;

        Velocity += (F * Time.fixedDeltaTime * 0.1F * 8F);
        Velocity = Vector3.ClampMagnitude(Velocity, 0.1f * MaxSpeed * (DashAccelation > 0 ? MaxDashSpeed* 10 : 1));
        if (PlayerInput.sqrMagnitude == 0 && Velocity.sqrMagnitude < 0.0001f)
            Velocity *= 0f;

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
            if(Velocity.sqrMagnitude > 0f)
            {
                //Debug.Log(Velocity.normalized);
                Quaternion q = Quaternion.LookRotation(Velocity.normalized, Vector3.up);

                CharacterRotatorTR.rotation = q;
            }
            
        }

        //transform.Translate(Velocity);
        //CharacterRigidbody.velocity = Velocity * 100;
        CharacterRigidbody.AddForce(Velocity * 100);
        animator.SetFloat("velocity", Mathf.Lerp(0, 1, (Velocity.sqrMagnitude * 100) / (MaxForwardVelocity * 0.01f)));
        if(DashAccelation > 0)
            animator.SetFloat("velocity", 0f);

        Accel *= 0;
    }

    void FixedUpdate()
    {
        if (!base.hasAuthority) return;

        //CharacterRigidbody.MovePosition(CharacterRigidbody.position + Magnitude * 10 * Time.fixedDeltaTime);
        //Vector2 lookDir = MousePos - new Vector2(CharacterRigidbody.position.x, CharacterRigidbody.position.z);
        //float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        //CharacterRigidbody.rotation = Quaternion.LookRotation(lookDir);
    }

    public static Vector3 GetMouseWorldPosition()
    {

        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }
    public static Vector3 GetMouseWorldPositionWithZ()
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
    }
    public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
    }
    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
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

    public void ApplyForce(Vector3 force){
        if (!base.hasAuthority) return;

        Vector3 f = force / this.Mass;
        Magnitude += f;
    }

    public void ApplyKnockback(Vector3 knockbackMagnitude)
    {
        if (!base.hasAuthority) return;

        ApplyForce(knockbackMagnitude);
    }

    Vector3 CalculateFriction(){
        Vector3 friction = Vector3.zero;
        return friction;
    }
}
