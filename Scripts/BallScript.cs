using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour {

    public float startSpeed;
    [HideInInspector]
    public float speed;
    [HideInInspector]
    public Vector3 direction;
    [HideInInspector]
    public int lastHitter = -1;
    [HideInInspector]
    public Vector3 position;

    public Collider player1Collider; 
    public Collider player2Collider; 

    public bool playerTurn; 

    public static float Intensity = 0.1f;
    public static float Shakes = 5;
    public static float Speed = 10f;

    //increase in screen shake 
    public float IntensityIncrease = 1.2f;
    public float ShakesIncrease = 1.1f;
    public float SpeedIncrease = 1.2f;

    public static float hitCounter = 0;
    float Versnelspd = 1.3f;

    bool realityCheck = true;
    bool player1Hit = false; 

    public GameObject hitfx_1;
    public GameObject hitfx_2; 

   

    public GameObject[] p1_healthvalue;
    public int p1_lifeCount = 1;

    //more behavoir types of the ball can be added here
    public enum BallState
    {
        SERVE,
        NORMAL,
        HITSTUN,
        STANDBY
    }
    [HideInInspector]
    public BallState ballState = BallState.SERVE;
    private Vector3 startPosition;

    public static BallScript ball;

    void Start () {
        ball = this;
        speed = startSpeed;
        position = startPosition = transform.position;
	}

    //updating the state of the ball
    void Update() {

        //NORMAL is the state where the ball moves
        if (ballState == BallState.NORMAL)
        {
            position += direction * speed;
        }

        //if the ball is not dead, we have to make sure it stays in the stage
        if (ballState != BallState.STANDBY)
        {
            if (position.x < -9)
            {
                HitWall(true);
                position.x = -9;
            }
            if (position.x > 9)
            {
                HitWall(true);
                position.x = 9;
            }
            if (position.y < -5)
            {
                HitWall(false);
                position.y = -5;
            }
            if (position.y > 5)
            {
                HitWall(false);
                position.y = 5;
            }
        }
        transform.position = position;
    }

    //the ball gets hit
    public void GetHit(PlayerScript byPlayer, float hitPause)
    {
        //you can set the starting speed in the editor
        //we can play around with this of course
        //try speed *= 1.1f instead
        
        lastHitter = byPlayer.playerNum;
        hitCounter += 1; //checks how many times the ball has been hit 

        if (speed <= 3)
        {
            speed *= Versnelspd; //invcreases the speed each hit 
        }

        if (realityCheck == true && hitCounter >=3)
        {
            Intensity *= IntensityIncrease;
            Shakes *= ShakesIncrease;
            Speed *= SpeedIncrease;
            
        }

        if (hitCounter >= 3)
        {
            Camera.main.GetComponent<CameraControl>().Shake(Intensity, (int)Shakes, Speed);
        }

        if (Intensity >= .5f)
        {
            realityCheck = false; 
        } 

        GetComponent<PlayerScript>();
        Explode(); 

        //default direction set to which player it is
        if (byPlayer.playerNum == 0) direction = Vector2.right;
        else direction = Vector2.left;
        StartCoroutine(GetHitCoroutine(hitPause));
    }

    public void HitWall(bool vertical)
    {
        if (vertical) direction.x *= -1;
        else direction.y *= -1;
        lastHitter = -1;
    }

    public IEnumerator GetHitCoroutine(float hitPause)
    {
        ballState = BallState.HITSTUN;

        yield return new WaitForSeconds(hitPause);

        ballState = BallState.NORMAL;
    }

    public IEnumerator HitPlayerCoroutine(PlayerScript player)
    {
        ballState = BallState.STANDBY;
        position = 1000 * Vector3.down;
        speed = startSpeed;

        yield return new WaitForSeconds(2.4f);

        lastHitter = -1;
        ballState = BallState.SERVE;
        position = player.position + Vector3.right*(player.playerNum == 0 ? 1.5f : -1.5f);
    }

    //Keeps track of the players lives
    //removes 1 from player 1 if hit. 
    public void OnTriggerEnter(Collider player1Collider)
    {
        player1Hit = true; 

        if (player1Hit == true && p1_lifeCount >= 0)
        {
            player1Hit = false;
            //Destroy((p1_healthvalue[1].gameObject));
            p1_lifeCount -= 1;
            print(p1_lifeCount);
        }
        else if (p1_lifeCount < 0)
        {
            //player dies reset 
        }


    }

    void Explode()
    {
        GameObject effect_1 = Instantiate(hitfx_1, position, Quaternion.identity);
        GameObject effect_2 = Instantiate(hitfx_2, position, Quaternion.identity);

        if (hitCounter > 2 && hitCounter <= 5)
        {
            effect_1.GetComponent<ParticleSystem>().Play();
            effect_2.GetComponent<ParticleSystem>().Stop();
        }
        else if (hitCounter > 5)
        {
            effect_2.GetComponent<ParticleSystem>().Play();
            effect_1.GetComponent<ParticleSystem>().Stop();
        }
        else if (hitCounter > 10)
        {
            effect_1.GetComponent<ParticleSystem>().Play();
            effect_2.GetComponent<ParticleSystem>().Play();
        }
        else
        {
            effect_1.GetComponent<ParticleSystem>().Stop();
            effect_2.GetComponent<ParticleSystem>().Stop();
        }
        Destroy(effect_1, 1f);
        Destroy(effect_2, 1f); 
    }
}
