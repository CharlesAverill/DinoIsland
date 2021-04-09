using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenCameraOrbit : MonoBehaviour
{

    public Transform target;
    public float moveSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update(){
        transform.RotateAround(target.position, Vector3.up, moveSpeed * Time.deltaTime);
    }
}
