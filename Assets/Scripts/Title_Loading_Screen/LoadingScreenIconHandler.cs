using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenIconHandler : MonoBehaviour
{

    public Transform loadingImage;

    public float bounceSpeed;
    public float bounceGravity;
    public float bounceThreshold;

    public float velocity;

    Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = loadingImage.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(loadingImage.position, initialPosition) < bounceThreshold){
            velocity = bounceSpeed;
        }

        velocity += bounceGravity * Time.deltaTime;

        if(loadingImage.position.y + velocity < initialPosition.y){
            Debug.Log(initialPosition.y);
            velocity = initialPosition.y - loadingImage.position.y;
        }

        loadingImage.position = new Vector3(loadingImage.position.x,
                                            loadingImage.position.y + velocity,
                                            loadingImage.position.z);
    }
}
