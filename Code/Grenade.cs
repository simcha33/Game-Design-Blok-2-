using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    public float lifetime = 3.0f;

    public GameObject explosionPrefab;

    void Start()
    {
        StartCoroutine(Explode(lifetime));
    }

    private IEnumerator Explode(float time)
    {
        yield return new WaitForSeconds(time);

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
    
    void Update()
    {

    }
}
