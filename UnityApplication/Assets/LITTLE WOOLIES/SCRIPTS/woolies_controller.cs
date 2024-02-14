using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class woolies_controller : MonoBehaviour
{
    public bool player = false;
    [Header("Path finder options")]
    public bool showPath = false;
    public Transform reference;
    [Header("Motion options")]
    public float acceleration = 5.0f;
    public float rotationSpeed = 25.0f;
    [Header("Animator options")]
    public float runPlayer = 2.0f;
    public float walkPlayer = 1.0f;
    [Header("Agent options")]
    public float agentSpeed = 1f;
    public float runAgent = 0.55f;
    public float walkAgent = 0.4f;
    

    private float num = 0;
    private NavMeshAgent agente;
    private Transform chara;
    private Animator anim;
    private Vector3 holder;
    private bool fcontact = true;
    private float target = 0.0f;
    [HideInInspector] public float aSpeed = 1.1f;

    //switch para prender y apagar el path de navmeshagent
    void switchPath(bool show)
    {
        if (!show)
        {
            showPath = true;
        }
        else
        {
            showPath = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.updatePosition = false;
        agente.updateRotation = false;
        chara = GetComponent<Transform>();
        anim = GetComponent<Animator>();

        if (!player)
        { 
            NavMeshObstacle obs = gameObject.AddComponent(typeof(NavMeshObstacle)) as NavMeshObstacle;
            obs.shape = NavMeshObstacleShape.Capsule;
            obs.carveOnlyStationary = false;
            obs.carving = true;
        }
    }

    public float getYpos(float pos)
    {
        return Mathf.Clamp(pos,0.0f,1.0f);
    }
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            switchPath(showPath);
        }
        
        holder = new Vector3(chara.transform.position.x,3.0f, chara.transform.position.z);
        //mouse para point y clic
        if (Input.GetMouseButtonDown(0) )
        {
            if (player)
            {
                anim.SetBool("walk", true);
                anim.SetInteger("idleType", 0);
                RaycastHit Hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out Hit))
                {
                    agente.Warp(reference.position);
                    agente.ResetPath();
                    agente.destination = Hit.point;
                    fcontact = true;
                }
            }
        }
        else
        {
            anim.SetBool("walk", false);
        }

        // input para celebrar
        if (Input.GetKeyDown(KeyCode.V))
        {
            anim.SetInteger("idleType", 5);
        }
        // input para caida
        //if (Input.GetKeyDown(KeyCode.X))
        //{
        //    anim.SetTrigger("acting");
        //}
        // input para saltar
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            anim.SetInteger("idleType", 0);
            anim.SetBool("isFalling", false);
            anim.SetBool("walk", true);
            anim.SetTrigger("jump");
            target = 1.0f;
        }

        //control de rotacion de personaje sobre navmeshagent
        var curRot = chara.rotation;
        float dist = Vector3.Distance(agente.destination, reference.position);


        //Deteccion de distancia para activar caminata, giro y reposición en Y con raycast
        if (dist > Mathf.Epsilon+1 && anim.GetBool("isFalling") == false)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                target = runPlayer;
                aSpeed = runAgent;
            }
            else
            {
                target = walkPlayer;
                aSpeed = walkAgent;
            }
            
            Vector3 orient = agente.steeringTarget - chara.position;
            //Vector3 rot = Quaternion.Euler();
            // quita la capa 9 de la seleccion de capas para tomar colisiones
            int layerMask = 1 << 9;
            layerMask |= 1 << 10;
            layerMask = ~layerMask;
            RaycastHit floor;
            if (Physics.Raycast(holder, Vector3.down, out floor, Mathf.Infinity, layerMask) && fcontact)
            {
                //Debug.Log(floor.point.y + "<-holder");
                Vector3 pos = chara.transform.position;
                float y = floor.point.y;
                pos.y = y;
                reference.transform.position = pos;
            }
            if (player)
            {
                anim.SetBool("walk", true);
                Quaternion res = Quaternion.Slerp(curRot, Quaternion.LookRotation(orient, Vector3.up), rotationSpeed * Time.deltaTime);
                chara.rotation = Quaternion.Euler(0, res.eulerAngles.y, 0);
            }
        }
        else
        {
            target = 0.0f;
            anim.SetBool("walk", false);
            // actualiza el next position de navmeshagent para que este empatado con la velocidad del gameobject
            agente.Warp(this.gameObject.transform.position);
        }
        float interpolY = Mathf.Lerp(anim.GetFloat("Y"), target, acceleration * Time.deltaTime);
        if (player)
        {
            anim.SetFloat("Y", interpolY);
            agente.speed = interpolY * agentSpeed * aSpeed;
            OnDrawGizmosSelected();
        }
    }

    void OnDrawGizmosSelected()
    {
        var nav = GetComponent<NavMeshAgent>();
        if (nav == null || nav.path == null)
            return;


        var line = this.GetComponent<LineRenderer>();

        if (!showPath)
        {
            line.enabled = false;
            return;
        }
        else
        {
            line.enabled = true;
        }


        if (line == null)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 0.0f);
            curve.AddKey(0.1f, 0.1f);

            line = this.gameObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default")) { color = Color.yellow };
            line.widthCurve = curve;
            line.widthMultiplier = 0.01f;
            line.SetColors(Color.yellow, Color.yellow);
        }

        var path = nav.path;

        line.SetVertexCount(path.corners.Length);

        for (int i = 0; i < path.corners.Length; i++)
        {
            //Debug.Log(i + "<- numero de punto");
            line.SetPosition(i, path.corners[i]);
        }

    }
}
