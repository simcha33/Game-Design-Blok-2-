using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class PlayerController : MonoBehaviour {

    [Header("References")]
    public GameObject camera;

    [Header("CollisionSettings")]
    public LayerMask worldMask;
    public float rayCastToGround = 0.05f;
    [Header("Movement Settings")]
    public AnimationCurve speedCurve;
    public float timeToTopSpeed = 10f;
    private float height = 1.4f;
    private float radius = 0.25f;
    private float offSet = 0.1f;

    public Vector3 jumpVelocity = new Vector3(0,6,3);
    [Tooltip("This is a percentage that the velocity loses per frame")]
    public float noMoveFriction = 0.95f;
    public float maxSpeed = 6f;
    public int numDoubleJumps = 2;
    public float gravityAmount = 10f;

    [Header("Camera Settings")]
    public float sensitivityX = 15;
    public float sensitivityY = 15;
    public float maxYAngle = 85f;

    private Rigidbody rb;
    private float camAngleX;
    private float camAngleY;
    private Vector3 velocity;
    private float speedModifier;

    private float climbSpeed = 0.5f; 

    public float jumpCharge = 1; 
    float t = 0; 

    public Image chargeMeter; 

    public Image chargeMeterBackground; 

    public float maxCharge = 3 ; 

    public bool parkouring = false; 

    public Text parkourMode;  

    public CharacterController characterController;

    private bool grounded = false;
    public bool Grounded {
        get { return grounded; }
        set {
            if(value && !grounded)
            {
                OnBecomeGrounded();
            }
            grounded = value;
        }

    }
    private int doubleJump = 0;
    private float gravity;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
    }

    // Use this for initialization
    void Start () {
         	
	}
	
	// Update is called once per frame
	private void Update () {

        ChargeMeterUpdate(); 

        float vert = Input.GetAxis("Vertical");
        float hor = Input.GetAxis("Horizontal");
        //Grounded = Physics.CheckSphere(transform.position + transform.up * (radius- rayCastToGround), radius, worldMask);
        Grounded = characterController.isGrounded;
        //Step Height
        float rayDistance = radius + 0.1f;
        float stepHeight = 0.2f;
        //if (Physics.Raycast(transform.position + transform.up * stepHeight, transform.forward, rayDistance, worldMask))
        //{
            //Debug.Log("Hit");
            //Debug.DrawLine(transform.position + transform.forward * rayDistance + transform.up * stepHeight, transform.position + transform.forward * rayDistance + transform.up * stepHeight - transform.up);
            //RaycastHit hit;
            //if (Physics.Raycast(transform.position + transform.forward * rayDistance + transform.up * stepHeight, -transform.up, out hit, stepHeight, worldMask)&&
            //    vert != 0)
            //{
            //    transform.position = Vector3.MoveTowards(transform.position, hit.point, velocity.magnitude *Time.deltaTime);
            //    Debug.Log("Step!");
            //}
            //else
            //{
            
            bool collisionWithWall = Physics.CheckCapsule(velocity.normalized * offSet + transform.position + transform.up * (radius), velocity.normalized * offSet + transform.position + transform.up * (height - radius), radius, worldMask);

            if (collisionWithWall)
            {
                Debug.Log("Colliding with wall!");
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    velocity = new Vector3(0, velocity.y + climbSpeed, 0);
                    Debug.Log ("parkour mode"); 
                }
                if (velocity.y >= 5)
                {
                    velocity.y = 5; 
                }       
                speedModifier = Mathf.Min(speedModifier, 0.1f);
            }
    

        //Apply forward velocity
        if(vert != 0 || hor != 0){
            speedModifier += (1f / Mathf.Max(0.001f,timeToTopSpeed)) * Time.deltaTime;
            speedModifier = Mathf.Clamp01(speedModifier);
            float speed = maxSpeed * speedCurve.Evaluate(speedModifier);
            Vector3 velXZ = (vert * transform.forward + hor * transform.right).normalized * speed;
            velocity = new Vector3(velXZ.x, velocity.y, velXZ.z);
        }
        else{
            velocity -= Vector3.Scale(velocity, new Vector3(1,0,1) * (1-noMoveFriction));
            speedModifier = 0;
        }

        //Apply Gravity
        if (!grounded){
            gravity = gravityAmount * Time.deltaTime;
            velocity -= new Vector3(0, 1, 0) * gravity;
        }
        else{
             gravity = 0;
        }

        //Apply Jump charge
        //if (Input.GetKey(KeyCode.Space))
        //{
        //    jumpCharge = 0; 
        //}
        if(Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift))
        {
           t += Time.deltaTime; 
           jumpCharge = 1 * t;
           Debug.Log(t);    
        }
        if (t >= maxCharge)
        {
            t = maxCharge; 
            jumpCharge = 2 * t;
        } 
        //jumpCharge = jumpCharge * t;

        //Apply Jump Force without charge boost 
        if (Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift) && doubleJump < numDoubleJumps && parkouring == false)
        { 
            velocity = new Vector3(velocity.x, jumpVelocity.y, velocity.z) + transform.forward * +jumpVelocity.z * Mathf.Max(0.5f, (velocity.magnitude / maxSpeed));
            doubleJump++;
            gravity = 0;

        }//Apply jump Force with charge boost 
        else if (Input.GetKeyUp(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift) && doubleJump < numDoubleJumps && parkouring == true){
            velocity = new Vector3(velocity.x, jumpVelocity.y + jumpCharge, velocity.z) + transform.forward * +jumpVelocity.z * Mathf.Max(0.5f, (velocity.magnitude / maxSpeed));
            doubleJump++;
            gravity = 0;
            t= 0; 
        }

        if (Input.GetKeyUp(KeyCode.LeftShift)){
            t = 0;  
            parkouring = false;  
        }

        if (Input.GetKey(KeyCode.LeftShift)){     
            parkouring = true; 
            parkourMode.text = ("Parkour mode: on"); 
            parkourMode.color = new Color(0f,255f,0f,1f);
        }else{
            parkourMode.text = ("Parkour mode: off"); 
            parkourMode.color = new Color(255f,0f,0f,1f);
        }
        


        //Apply rotation and update camera
        float camX = Input.GetAxis("Mouse X");
        float camY = Input.GetAxis("Mouse Y");

        camAngleX += camX * sensitivityX;
        camAngleY += camY * sensitivityY;
        camAngleY = Mathf.Clamp(camAngleY, -maxYAngle, maxYAngle);
        camera.transform.localRotation = Quaternion.Euler(-camAngleY, 0, 0);

        transform.rotation = Quaternion.Euler(0, camAngleX, 0);

    }
    private void OnBecomeGrounded()
    {
        if (velocity.y <= 0)
        {
            doubleJump = 0;
        }
        velocity = Vector3.Scale(velocity , new Vector3(1,0,1));
    }

    private void FixedUpdate()
    {
        characterController.Move(velocity *Time.deltaTime);
        //rb.velocity = new Vector3(velocity.x, velocity.y, velocity.z);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(velocity.normalized * offSet + transform.position + transform.up * (radius), radius);
        Gizmos.DrawWireSphere(velocity.normalized * offSet + transform.position + transform.up * (height - radius), radius);  
    }

    void ChargeMeterUpdate()
    {
        float ratio = (t / maxCharge); 
        chargeMeter.rectTransform.localScale = new Vector3(ratio,.16f,1); 
        
    }
}

//parkour systeem 
//UI 
//Charge jump op dezelfde button voelde eerst slecht maar na het op dezelfde button en zonder delay voelde het goed 
//Orb spawner wat aangepast (meer tijd)
//Muziek toegevoegd?
// 