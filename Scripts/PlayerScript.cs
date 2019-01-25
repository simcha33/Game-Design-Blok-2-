using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour

{
    public float Intensity = 0.1f;
    private int Shakes = 5;
    public float Speed = 10f;

    public float IntensityIncrease = 1.3f;
    public int ShakesIncrease = 2;
    public float SpeedIncrease = 2f;

    public float stateChange_1 = 3; 

    public float pauseUp = 1.5f;

    public int life = 3; 

    public GameObject[] p1_healthvalue;
    public int p1_lifeCount = 1; 
  
    //public GameObject p1_lifePoint_1;
    //public GameObject p1_lifePoint_2;
    //public GameObject p1_lifePoint_3;

    public bool player1Hit = false; 

    float hitPause = 0.3f;

    //which player it is. 0 is left, 1 is right
    public int playerNum;

    //these can be tweaked in the editor
    public float moveSpeed;
    public float swingDuration;
    public float recoveryDuration;

    //animator 
    

    //you can add stuff here and just drag a sprite on it in the editor
    //same for the ball, if you want a different visual there
    //spriteRenderer.sprite = whateverSprite; is how you change picture
    private SpriteRenderer spriteRenderer;
    public Collider swingCollider;
    public Sprite idleSprite;
    public Sprite swingSprite;
    public Sprite hitSprite;
    public Sprite recoverySprite;
    public Sprite dieSprite;

    //input mapping (just set these in the editor)
    public KeyCode hitKey = KeyCode.C;
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode leftKey = KeyCode.A;

    [HideInInspector]
    public Vector3 position;

    //if you want to add more abilities or states of the player, add them in here
    //and then put them in Update() so the game knows what to do during them
    public enum PlayerState
    {
        NORMAL,
        SWINGING,
        HITTING,
        SWING_RECOVERY,
        GET_HIT,
        DEAD,
    }
    PlayerState playerState = PlayerState.NORMAL;
    Vector3 startPosition;

    void Start()
    {
        position = startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        swingCollider.enabled = false;
        //p1_healthvalue = new GameObject[2];

    }

    //checking if we hit the ball
    public void OnTriggerStay(Collider otherCollider)
    {
        BallScript ball = otherCollider.GetComponent<BallScript>();
        // Intensity *= IntensityIncrease;
        //Shakes *= ShakesIncrease;
        //Speed *= SpeedIncrease;


        if (ball.lastHitter != playerNum) //don't hit the ball twice 
        {
            //you can do exemptions or calculations with this depending on the ball or the player

            //Intensity *= IntensityIncrease;
            //Shakes *= ShakesIncrease;
            //Speed *= SpeedIncrease;
            //Camera.main.GetComponent<CameraControl>().Shake(Intensity, Shakes, Speed);

            if (hitPause >= 1.0f)
            {
                hitPause = .8f;
            }

            else
            {
                hitPause *= 1.2f;
            }

            ball.GetHit(this, hitPause);//send info to the ball

            if (BallScript.hitCounter > stateChange_1)
            {
                StopAllCoroutines(); //stop the swinging 
                StartCoroutine(HitCoroutine(hitPause)); //start the hitting with the right hitpause duration
            }  
        }
    }

    //checking if we get hit
    public void OnCollisionEnter(Collision collision)
    {
        player1Hit = true; 

        /*
        if (player1Hit == true && p1_lifeCount >= 0)
        {
            player1Hit = false;
            Destroy((p1_healthvalue[p1_lifeCount].gameObject));
            p1_lifeCount -= 1;
            print(p1_lifeCount); 
        }
        */
         
        
        BallScript ball = collision.rigidbody.GetComponent<BallScript>();
       // p1_lifeCount -= 1;
       // Destroy(p1_lifePoint_1); 


        //only if the ball is flying around, you could put other exemptions here
        if (ball != null && ball.ballState == BallScript.BallState.NORMAL) 
        {
            StopAllCoroutines(); //stop whatever you're doing
            StartCoroutine(DieCoroutine()); //start dying
            ball.StartCoroutine(ball.HitPlayerCoroutine(this)); //remove the ball but let it know who it killed
        }
    }

    //we update the state of the player
    //what sprite to show and what hitbox to enable
    //you can add more abilities here as PlayerStates, more or other hitboxes etc
    void Update()
    {
        switch (playerState)
        {
            case PlayerState.NORMAL:
                swingCollider.enabled = false;
                spriteRenderer.sprite = idleSprite;

                //this is where we can check to start actions
                //since the player is doing nothing right now
                if (Input.GetKeyDown(hitKey))
                {
                    StartCoroutine(SwingCoroutine());
                }

                //move around
                if (Input.GetKey(rightKey))
                    position += Vector3.right * moveSpeed;

                //insert jump
                if (Input.GetKey(leftKey))
                    position += Vector3.left * moveSpeed;

                position.x = Mathf.Clamp(position.x, -8f, 8f);
                break;

            case PlayerState.SWINGING:
                swingCollider.enabled = true;
                spriteRenderer.sprite = swingSprite;
                break;

            case PlayerState.HITTING:
                swingCollider.enabled = true;
                spriteRenderer.sprite = hitSprite;
                //as we're hitting, we can aim the ball
                //feel free to remove or alter this completely

                //for setting specific directions you can use: 
                //new Vector2(0.5f, 0.5f); is diagonally up for example
                //see it as distance from zero, the direction is the line between zero and your x y numbers
                //Vector3.up or down etc also works

                if (playerNum == 0)//left side player
                {
                    if (Input.GetKey(rightKey))
                        BallScript.ball.direction = Vector3.right;
                    if (Input.GetKey(upKey))
                        BallScript.ball.direction = new Vector2(0.5f, 0.5f);
                    if (Input.GetKey(downKey))
                        BallScript.ball.direction = new Vector2(0.5f, -0.5f);
                }
                else
                {
                    if (Input.GetKey(leftKey))
                        BallScript.ball.direction = Vector3.left;
                    if (Input.GetKey(upKey))
                        BallScript.ball.direction = new Vector2(-0.5f, 0.5f);
                    if (Input.GetKey(downKey))
                        BallScript.ball.direction = new Vector2(-0.5f, -0.5f);
                }
                //tip: you can use BallScript.ball to access the ball from anywhere
                break;
            case PlayerState.SWING_RECOVERY:
                swingCollider.enabled = false;
                spriteRenderer.sprite = recoverySprite;
                break;

            case PlayerState.GET_HIT:
                
                swingCollider.enabled = false;
                spriteRenderer.sprite = dieSprite;
                break;


            case PlayerState.DEAD:
                swingCollider.enabled = false;
                spriteRenderer.sprite = dieSprite;
                position += Vector3.down * 0.25f;
                break;
        }

        transform.position = position;
    }

    //these are for doing the timings
    //after WaitForSeconds it goes on where it left off
    //if you want to add different abilities you can copy one over and change it

    public IEnumerator SwingCoroutine()
    {
        playerState = PlayerState.SWINGING;
        yield return new WaitForSeconds(swingDuration);

        playerState = PlayerState.SWING_RECOVERY;
        yield return new WaitForSeconds(recoveryDuration);

        playerState = PlayerState.NORMAL;
    }

    public IEnumerator HitCoroutine(float hitPause)
    {
        if (BallScript.hitCounter > stateChange_1)
        {
            playerState = PlayerState.HITTING;
            yield return new WaitForSeconds(hitPause);
        }

        playerState = PlayerState.SWING_RECOVERY;
        yield return new WaitForSeconds(recoveryDuration);

        playerState = PlayerState.NORMAL;
    }

    public IEnumerator DieCoroutine()
    {
        playerState = PlayerState.GET_HIT;
        yield return new WaitForSeconds(0.2f);

        playerState = PlayerState.DEAD;
        yield return new WaitForSeconds(2.0f);

        playerState = PlayerState.NORMAL;
        position = startPosition; 

        BallScript.Intensity = 0.1f;
        BallScript.Shakes = 5;
        BallScript.Speed = 10f;
        BallScript.hitCounter = 0f; 
        hitPause = 0.3f; 
    }
}