using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class Player : MonoBehaviour
{
    public float horizontalSpeed = 50.0f;
    public float verticalSpeed = 40.0f;

    public float fuelCounter = 3.0f;

    public float fuelUP = .5f;
    public float fuelDown = 1.0f;

    public float fuelMax = 6.0f; 

    private bool flying = false;

    private bool fuelCooldown = false; 

    public float dashSpeed = 10.0f;


    public Rigidbody2D myRigidbody;

    private int fuelTimer = 0;

    private int emptyFuelTimer = 0; 

    public Image fuelMeter;

    public float discounter = 0.3f;  


    void FixedUpdate()
    {
        fuelTimer++; 
        Debug.Log(emptyFuelTimer); 
    }

    void Update()
    {
        FuelCounterUpdater(); 
        //Debug.Log(fuelCounter); 
        Vector2 movement = Vector2.zero;
        if (fuelCounter > fuelMax)
        {
            fuelCounter = fuelCounter - (fuelCounter - fuelMax); 
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            movement.x -= horizontalSpeed;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            movement.x += horizontalSpeed;
        }

        //Controls the jetpack
        if (Input.GetKey(KeyCode.Space) && fuelCounter > 0f)
        {
            movement.y = verticalSpeed;
            if (fuelTimer > 10)
            {
                fuelCounter -= fuelDown;
                fuelTimer = 0;
                emptyFuelTimer = 0; 
                flying = true; 
            }
        }
        else
        {
            if ((!Input.GetKey(KeyCode.Space)) && fuelTimer > 14 && fuelCounter < fuelMax) //Checks if there is enough fuel left 
            {
                emptyFuelTimer++; //delay for the fuel to recharge
                flying = false; 

                if (emptyFuelTimer >= 90)
                { 
                    //flying = false;
                    fuelCounter += fuelUP;
                    fuelTimer = 0;            
                }
            }
        }
            
        if(fuelCounter <= 0f)
        {
            discounter =-0f; 
        }
        else if (fuelCounter > 0) //Makes it so the fuel sprite doesnt glitch out 
        {
            discounter = 0.3f;  
        }
        
        

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            myRigidbody.AddForce(movement * dashSpeed * Time.deltaTime, ForceMode2D.Impulse);
            fuelCounter -= (fuelDown * 4); 
        }

        myRigidbody.AddForce(movement * Time.deltaTime);
   
    }

    void FuelCounterUpdater(){
        float ratio = (fuelCounter / fuelMax) - discounter; 
        fuelMeter.rectTransform.localScale = new Vector3(0.7f,ratio,0.7f); 
         // fuelMeter.fillAmount = (float)fuelMax / (float) fuelCounter
    }
}

//fuelmeter visueel gemaakt 
//karakter vloog te veel alle kanten op voor gezorgd dat de speler meer controle had over het karakter
//Maar daarbij er ook voor gezorgd dat het wel voelt alsof de speler in de lucht zweeft ook wanneer die valt dat die nog een klein
// naar beneden zakt voordat die weer opstijgt 
// Jetpack refuel een kleine pauze gegeven omdat zodat spatie niet ingedrukt gehouden kan worden voor onneindige flight 
// Heel veel gekloot met hoe snel de fuel counter leeg moet lopen 
