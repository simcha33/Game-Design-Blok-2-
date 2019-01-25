using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

public class Traps : MonoBehaviour {

	public static bool died = false; 
    public static bool playerHit = false; 

	  void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("yeet"); 
            playerHit = true; 
        }
    }
	
}

