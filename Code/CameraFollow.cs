using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform followTransform;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(followTransform.position.x, followTransform.position.y, transform.position.z);
    }
}
