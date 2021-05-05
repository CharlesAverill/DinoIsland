using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Projector))]
public class GroundShadow : MonoBehaviour
{

    public Vector3 upVector = new Vector3(90f, 0f, 0f);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = upVector;
    }
}
