using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 using UnityEngine.SceneManagement;

public class OrbSpawner : MonoBehaviour {

    public Transform ObjecToFollow;
    public float maxTimer;

    public float spawnDistanceFromPlayer;
    public LayerMask layerMask;
    public AudioClip[] spawnSounds;
    public AudioClip[] pickupSounds;
    public Text text;

    public Text lifesText; 

    public int lifes = 3; 
    int score = 0;
    private static float currentTimer = 0;

    public float critcalTimeLeft = currentTimer/3; 
    public Text countDownText; 
    private Material mat;
	// Use this for initialization
	void Start () {
        mat = GetComponentInChildren<MeshRenderer>().material;
        Respawn();
    }
	
	// Update is called once per frame
	void Update () {
        currentTimer -= Time.deltaTime;
        mat.color = Color.Lerp(Color.red, Color.green, currentTimer / maxTimer);
        UpdateCountdown(); 
        lifesText.text = "Lifes: " + lifes.ToString(); 

        if (currentTimer <= 0)
        {
            LifesCourentine(); 
            Respawn();
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Player")
        {
            AudioSource.PlayClipAtPoint(pickupSounds[Random.Range(0,pickupSounds.Length)], transform.position);
            Debug.Log("Pickedup!");
            score++;
            UpdateScore();
            Respawn();
        }    
    }
    void UpdateScore()
    {
        text.text = "Score: " + score.ToString();
    }

    void UpdateCountdown()
    {
        if (currentTimer <= critcalTimeLeft){
            countDownText.color = new Color(255f,0f,0f,1f);
        }
        else if (currentTimer > critcalTimeLeft){
            countDownText.color = new Color(0f,255f,0f,1f);
        }
         countDownText.text = "Time Left: " + Mathf.RoundToInt(currentTimer).ToString();
    }

    void Respawn()
    {
        currentTimer = maxTimer;
        Vector3 rayCastPoint=ObjecToFollow.transform.position + (new Vector3(Random.Range(-1,1),0,Random.Range(-1,1))).normalized * spawnDistanceFromPlayer + new Vector3(0,40,0);
        RaycastHit hit;
        if (Physics.Raycast(rayCastPoint, -Vector3.up,out hit,140, layerMask))
        {
            AudioSource.PlayClipAtPoint(spawnSounds[Random.Range(0, pickupSounds.Length)], transform.position);
            transform.position = hit.point;
        }
        else
        {
            Respawn();
        }
    }

    void  LifesCourentine()
    {
        lifes--; 
        if (lifes <= 0)
        {
            SceneManager.LoadScene("Scene");
        }  
        if (lifes == 1){
            lifesText.color = new Color (255f,0f,0f,1f); 
        }
    }
}
