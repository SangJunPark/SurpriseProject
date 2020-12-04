using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePointerLook : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //LookerTR = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        // Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        // float midPoint = (transform.position - Camera.main.transform.position).magnitude * 0.5f;
        // Vector3 vec = mouseRay.origin + mouseRay.direction * midPoint;
        // vec.y = transform.position.y;
        // transform.LookAt(vec);

         Vector3 mouse = Input.mousePosition;
    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(
                                                        mouse.x, 
                                                        mouse.y,
                                                        transform.position.y));
    Vector3 forward = mouseWorld - transform.position;
    forward.y = 0;
    transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}
