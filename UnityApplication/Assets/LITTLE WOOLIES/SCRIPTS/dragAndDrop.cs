using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class dragAndDrop : MonoBehaviour
{
    public float dragHeight = 1.0f;
    public Transform hang;
    public float speed=1f;
    //public Transform hangTarget;
    public Transform playerTrans;
    public woolies_controller controller;
    

    //private float mZcoord;
    private Vector3 mOffset;
    private Vector3 origin;
    private RaycastHit hit;
    private Rigidbody rb;
    private Animator anim;
    private bool fall=false;
    [HideInInspector] public NavMeshAgent agent;
    

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        //Cursor.lockState = CursorLockMode.Locked;
        anim = GetComponentInParent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();
    }

    public void Update()
    {
        //rb.velocity = hang.forward* Input.GetAxis("vertical")* speed;
        //hang.Rotate(0, Input.GetAxis("Mouse X"), 0);
    }

    //get mouse position on world position
    private Vector3 GetMouseWldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        //mousePoint.z = mZcoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    //get intersection point between active zone and mouse click - horizontal plane
    Vector3 GetActiveZoneIntersection(float activeZone,Camera camera)
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        float delta = ray.origin.y - activeZone;
        Vector3 dirNorm = ray.direction / ray.direction.y;
        Vector3 intersectionPos = ray.origin - dirNorm * delta;
        return intersectionPos;
    }

    // get z coordinate of mouse position when clicking
    public void OnMouseDown()
    {
        mOffset = (hang.position+new Vector3(0,dragHeight,0)) - GetActiveZoneIntersection(dragHeight, Camera.main);
        origin = hang.position;
    }
 
    // drag and drop action
    public void OnMouseDrag()
    {
        hang.GetComponent<Rigidbody>().useGravity = false;
        hang.GetComponent<Rigidbody>().velocity = (GetActiveZoneIntersection(dragHeight, Camera.main) + mOffset - hang.transform.position) * 10;
        hang.GetComponent<Rigidbody>().isKinematic = false;
        rb.AddForce(Vector3.down * 50);
        rb.angularDrag = 5f;
        rb.mass = 0.7f;
        anim.SetBool("isHanging", true);
        controller.enabled = false;
        rb.isKinematic = false;
        fall = false;
    }

    public void OnMouseUp()
    {
        hang.GetComponent<Rigidbody>().velocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        rb.angularDrag = 0.05f;
        rb.mass = 1f;
        anim.SetBool("isHanging", false);
        //hang.GetComponent<Rigidbody>().velocity = (hangTarget.position - hang.transform.position)*10;
        rb.isKinematic = true;
        hang.GetComponent<Rigidbody>().isKinematic = true;
        //hang.GetComponent<Rigidbody>().useGravity = true;
        //fall = true;
        controller.enabled = true;
        transform.localRotation = Quaternion.Euler(0, 0, 0);

        //layer 9 ignored
        int layerMask = 1 << 9;
        layerMask |= 1 << 10;
        layerMask = ~layerMask;
        RaycastHit floor;
        //raycast from hanging position to floor to get new floor contact position
        if (Physics.Raycast(new Vector3(transform.position.x,3.0f,transform.position.z), Vector3.down, out floor, Mathf.Infinity, layerMask))
        {
            //Debug.Log(floor.point.y + "<-holder");
            Vector3 pos = transform.position;
            float y = floor.point.y;
            pos.y = y;
            //player, agent and hanging position setup
            playerTrans.position = pos;
            agent.Warp(pos);
            hang.localPosition = new Vector3(-0.004851501f, 1.093219f, -0.115744f);
        }
        agent.Warp(agent.gameObject.transform.position);
    }

    public void FixedUpdate()
    {
        if (fall)
        {
            //Debug.Log("entro");
            //Quaternion qTo = Quaternion.LookRotation(hangTarget.position - hang.position);
            Quaternion qTo = Quaternion.LookRotation(Vector3.forward);
            qTo = Quaternion.Slerp(transform.rotation, qTo, speed * Time.fixedDeltaTime);
            rb.MoveRotation(qTo);
            //hang.GetComponent<Rigidbody>().MovePosition(hangTarget.position);
        }
    }
}
