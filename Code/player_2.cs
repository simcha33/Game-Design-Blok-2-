using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class player_2 : MonoBehaviour
{
    public float horizontalSpeed = 50.0f;
    public float verticalSpeed = 40.0f;

    public float dashForceMultiplier = 5.0f;

    public ForceMode2D dashForceMode;

    public float maxFuel = 100.0f;

	private float minFuel = 0f; 
    public float fuel = 100.0f;
    public float fuelDecreasePerSecond = 25.0f;
    public float fuelIncreasePerSecond = 50.0f;

	public float fuelCountdownTimer;

    public static float hitCooldownTimer = 3.0f; 

	public float discounter; 

	public Image fuelMeter; 
    public Rigidbody2D myRigidbody;

	// Use this for initialization
	void Start()
    {
		
	}

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 200), fuel.ToString());
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        PlayerGetsHit(); 
        Vector2 movement = Vector2.zero;

		if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            movement.x -= horizontalSpeed;
        }
        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            movement.x += horizontalSpeed;
        }

		//jetpack 
        if(Input.GetKey(KeyCode.Space) && fuel > .0f)
        {
            movement.y = verticalSpeed;
            fuel -= fuelDecreasePerSecond * Time.deltaTime;
            if(fuel < .0f) fuel = .0f;
			fuelCountdownTimer = 0; 
        }
		 
        else if(!Input.GetKey(KeyCode.Space))
        {
			fuelCountdownTimer++; 
			if (fuelCountdownTimer> 20){
 				fuel += fuelIncreasePerSecond * Time.deltaTime;
            	if(fuel > maxFuel) fuel = maxFuel;
			}  
		}
		 

		if (fuel > 30){
			//discounter = .3f; 
		}
		else if (fuel < 30){
			//discounter = 0f;
		}

        myRigidbody.AddForce(movement * Time.deltaTime);

        if(Input.GetKeyDown(KeyCode.LeftShift)){
            myRigidbody.AddForce(movement * dashForceMultiplier * Time.deltaTime, dashForceMode);
        }

		FuelCounterUpdater();
	}

	void FuelCounterUpdater()
    {
        float ratio = (fuel / maxFuel); 
        fuelMeter.rectTransform.localScale = new Vector3(0.7f,ratio,0.7f); 
	}

    void PlayerGetsHit()
    {
        if (Traps.playerHit == true && hitCooldownTimer <= 90){
            hitCooldownTimer++; 
        }
        else{
             hitCooldownTimer = 0; 
        }
    }
}
