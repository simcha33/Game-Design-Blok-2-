using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour {

void OnCollisionEnter(Collision other)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Do something here");
        }
	}
}